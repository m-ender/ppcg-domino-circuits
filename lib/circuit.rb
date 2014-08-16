# First index: Direction of falling domino
# Second index: Relative coordinate of affected domino
# Third index: Type of affected domino
# Value: Resulting direction of affected domino
PUSHES = {
    'Q' => {
        [-1, 0] => {'|' => 'A', '/' => 'Q'},
        [-1,-1] => {'|' => 'A', '/' => 'Q', '-' => 'W'},
        [ 0,-1] => {'/' => 'Q', '-' => 'W'}
    },
    'W' => {
        [ 0,-1] => {'-' => 'W', '/' => 'Q', '\\' => 'E'}
    },
    'E' => {
        [ 1, 0] => {'|' => 'D', '\\' => 'E'},
        [ 1,-1] => {'|' => 'D', '\\' => 'E', '-' => 'W'},
        [ 0,-1] => {'\\' => 'E', '-' => 'W'}
    },
    'A' => {
        [-1, 0] => {'|' => 'A', '/' => 'Q', '\\' => 'Z'},
    },
    'D' => {
        [ 1, 0] => {'|' => 'D', '/' => 'C', '\\' => 'E'},
    },
    'Z' => {
        [-1, 0] => {'|' => 'A', '\\' => 'Z'},
        [-1, 1] => {'|' => 'A', '\\' => 'Z', '-' => 'X'},
        [ 0, 1] => {'\\' => 'Z', '-' => 'X'}
    },
    'X' => {
        [ 0, 1] => {'-' => 'X', '\\' => 'Z', '/' => 'C'}
    },
    'C' => {
        [ 1, 0] => {'|' => 'D', '/' => 'C'},
        [ 1, 1] => {'|' => 'D', '/' => 'C', '-' => 'X'},
        [ 0, 1] => {'/' => 'C', '-' => 'X'}
    }
}


class Circuit
    attr_accessor :m, :n, :setup

    def initialize(spec)
        @m, @n, f_spec = spec.split
        @m = @m.to_i
        @n = @n.to_i

        @f = {}
        f_values = f_spec.split(',')
        f_values.each_index do |i|
            @f[i.to_s(2).rjust(@m,'0')] = f_values[i]
        end
    end

    def time_limit
        2**@m * @n
    end

    def validate(verbose)
        if verbose
            puts 'Solution:'
            puts @setup 
        end
        raw_setup = @setup

        @setup = @setup.split("\n")
        @h = @setup.length
        @w = @setup[0].length
        @setup[1..-1].map! {|line| line.ljust(@w) }

        if char = raw_setup[/[^\n \\\/|-]/]
            return false, "Invalid character in setup: #{char}"
        elsif char = @setup[0][/[^\/]/]
            return false, "Invalid character in power line: #{char}"
        end

        begin
            tr_setup = @setup.map(&:chars).transpose.map(&:join)
        rescue IndexError
            return false, "Power line is too short."
        end

        if char = tr_setup[0][1..-1][/[^ |]/]
            return false, "Invalid character in input column: #{char}"
        elsif (m = tr_setup[0].count('|')) != @m
            return false, "Wrong number of inputs. Should be #{@m}, was #{m}."
        elsif char = tr_setup[-1][1..-1][/[^ |]/]
            return false, "Invalid character in output column: #{char}"
        elsif (n = tr_setup[-1].count('|')) != @n
            return false, "Wrong number of outputs. Should be #{@n}, was #{n}."
        end

        if @w*@h > 8000000
            return false, "Solution too large. Must not exceed 8 million cells, has #{@w*@h}."
        elsif score > 1000000
            return false, "Too many dominoes. Must not use more than 1 million, has #{score}."
        end                

        @f.each do |i,o|
            result = compute(i)
            return false, "Wrong result for input #{i}. Should be #{o}, was #{result}." if result != o
        end

        return true
    end

    def compute(input)
        setup = @setup.map {|line| line.clone }

        setup[0][0] = 'C'
        falling = {[0,0]=>true}
        locked = {}

        i = j = 0
        while i < input.length
            j += 1
            if setup[j][0] == '|'
                if input[i] == '1'
                    setup[j][0] = 'D' 
                    falling[[j,0]] = true
                end
                i += 1
            end
        end

        modified = true

        k = 0
        while modified
            pushed = {}

            falling.each do |coords, bool|
                j, i = *coords
                # Iterate over all adjacent cells that can possibly be affected
                PUSHES[setup[j][i]].each {|dir, pushes|
                    x, y = i+dir[0], j+dir[1]
                    coords = [y,x]
                    # Reject out-of-bounds coordinates and cells that have already fallen
                    next if x < 0 || x >= @w || y < 0 || y >= @h || 
                            falling[coords] || locked[coords] || 
                            !(domino = setup[y][x])[/[\\\/|-]/]

                    # Make sure the the domino in the affected cell can be pushed
                    if push = pushes[domino]
                        # Check if there's a conflicting push on the tile already
                        if pushed[coords] && pushed[coords] != push
                            locked[coords] = true
                            pushed.delete(coords)
                        else
                            pushed[coords] = push
                        end
                    end
                }
            end

            falling = {}

            pushed.each do |coords, push| 
                y, x = *coords
                setup[y][x] = push
                falling[coords] = true
            end

            modified = pushed.length > 0

        end

        result = ''

        setup.each do |line|
            case line[-1]
            when '|'
                result << '0'
            when 'D'
                result << '1'
            end
        end

        return result
    end

    def score
        # Count all non-space characters including the power line 
        # and input and output column.
        @score ||= @setup.join.count('^ ')
    end
end