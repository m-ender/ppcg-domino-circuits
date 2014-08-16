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
    puts "Testing solver by #{author}" unless author.empty?
    puts "Running '#{solver_command.join ' '}' against #{benchmark_file}"
    puts "Selected circuits: #{selected_circuits}"
    puts

    results[author] = {}

    selected_circuits.each do |idx|
        name, spec = circuits[idx - 1].split("\n", 2)

        puts if verbose

        print "Solving circuit no. #{idx}: \"#{name}\"... "

        if verbose
            puts
            puts 'Spec:'
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
        else
            valid = false
        end

        score = -1 unless valid

        results[author][idx] = score

        if verbose
            puts
        end

        if valid
            puts "Done."
        else
            puts "Error: #{error}"
        end
    end

end

cols = selected_circuits.map {|idx| (results.map {|k,v| v[idx].to_s.length} + [idx.to_s.length]).max + 2}

c = -1
headline = ' User           Track:' + selected_circuits.map {|idx| idx.to_s.rjust(cols[c+=1])}.join

puts
puts '  ' + '='*headline.length
puts '   Score Board'
puts '  ' + '='*headline.length
puts
puts '  ' + headline
puts '  ' + '-'*headline.length
results.each do |k,v| 
    print '  %-22s' % k
    c = -1
    v.each do |idx,score|
        print score.to_s.rjust(cols[c+=1])
    end
    puts
end
puts '  ' + '-'*headline.length
puts