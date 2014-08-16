# Run like
# ruby scorer.rb [file_name]

file_name = ARGV[0] || 'raw_scores.txt'

abort("File #{file_name} not found.") unless File::exists? file_name

puts
puts "Reading scores from #{file_name}"
puts

results = {}
existing_columns = []

File.open(file_name, 'r') do |file|
    existing_columns = file.gets.split[1..-1].map(&:to_i)
    
    file.each_line do |line|
        user, *scores = line.split
        scores.map!(&:to_i)

        user_scores = {}

        scores.each_index do |i|
            score = scores[i]
            if score == -2
                puts "Warning: Score for #{user} on circuit no. #{existing_columns[i]} missing. Assuming failure."
                score = -1
            end
            
            user_scores[existing_columns[i]] = score 
        end

        results[user] = user_scores
    end
end

max_scores = {}
existing_columns.each do |idx|
    max = results.map {|author, scores| scores[idx]}.max
    if max < 0
        puts "Warning: Circuit no. #{idx} unsolved and will be omitted from results."
    else
        max_scores[idx] = max
    end
end

results.each_key do |user|
    scores = results[user]
    scores.delete_if {|idx, score| !max_scores.has_key? idx}
    scores.each_key do |idx| 
        score = scores[idx]
        scores[idx] = score < 0 ? 0 : 10000 * max_scores[idx] / score
    end
end

headline = ' Author         Track:' + max_scores.keys.map {|idx| idx.to_s.rjust(6)}.join + '  Total'
puts
puts '  ' + '='*headline.length
puts '   Score Board'
puts '  ' + '='*headline.length
puts
puts '  ' + headline
puts '  ' + '-'*headline.length
results.each do |author, scores| 
    print '  %-22s' % author
    total_score = 0
    scores.each do |idx, score|
        total_score += score
        print score.to_s.rjust(6)
    end
    print total_score.to_s.rjust(7)
    puts
end
puts '  ' + '-'*headline.length
puts