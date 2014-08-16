# Run like
# ruby controller.rb benchmarkfilename command to run solver
# e.g.
# ruby controller.rb benchmark.txt ruby mysolver.rb
# For more detailed output, you can add "-v" before the benchmark
# file name.

require 'optparse'
require 'timeout'

require_relative 'lib/circuit'

verbose = false
silent = false
selected_circuits = nil
OptionParser.new do |opts|
    opts.banner = "Usage: #$0 [options] benchmark_file [solver]"

    opts.on("-v", "Verbose") { verbose = true }
    opts.on("-c CIRCUITS", "Select circuits") { |c| selected_circuits = eval("[#{c}]").map{|el|el.respond_to?(:to_a) ? el.to_a : el}.flatten }
end.parse!

benchmark_file = ARGV.shift

if ARGV.length > 0
    # Pretend we've read this from commands.txt as the only command
    solvers = [ARGV]
    solvers[0].unshift ''
else
    solvers = File.open('submissions/commands.txt').read.split("\n").map(&:split)
end

circuits = File.open(benchmark_file).read.split("\n\n")
selected_circuits ||= (1..circuits.size)

results = {}

solvers.each do |solver|

    author = solver.shift
    next if author[0] == '#'
    solver_command = solver

    puts
    puts "Solver by #{author}" unless author.empty?
    puts "Running '#{solver_command.join ' '}' against #{benchmark_file}"
    puts

    total_score = 0

    puts ' No.       Size     Target   Score     Details'
    puts '-'*85

    selected_circuits.each do |idx|
        name, spec = circuits[idx - 1].split("\n", 2)

        if verbose
            puts
            puts "Starting circuit for #{name}. Circuit data:"
            puts
            puts spec
            puts
        end

        circuit = Circuit.new(spec)

        error = ''

        solver = IO.popen(solver_command + spec.split, 'r+')

        begin
            Timeout::timeout(circuit.time_limit) do
                circuit.setup = solver.read
            end
        rescue Timeout::Error => e
            error = 'Solver exceeded time limit.'

            # Kill the process manually, otherwise we might have to
            # wait for it to finish before closing.
            Process.kill('KILL', solver.pid)
        rescue Exception => e
            $stderr.puts e.message
            $stderr.puts e.backtrace.inspect
            error = 'Controller raised exception.'
        end

        solver.close

        if error.empty?
            valid, error = circuit.validate(verbose)
            score = circuit.score if valid
            total_score += score if valid
        else
            valid = false
        end


        if verbose
            puts
            puts 'Result:'
        end

        if valid
            puts "Circuit was solved with #{score} dominoes."
        else
            puts "Error on circuit #{name}: #{error}"
        end
    end

    puts '-'*85
    puts 'TOTAL SCORE: % 23.5f' % total_score
    puts

    results[author] = total_score
end

if solvers.length > 1
    puts '          Score Board'
    puts '  ============================'
    puts
    puts '   User                 Score'
    puts '  ----------------------------'
    results.each { |k,v| puts '  %-18s %9.5f' % [k, v] }
    puts
end
