The program basically consists of two parts. The first is recognition of the puzzle board and the second is the solver, which looks for an optimal solution for the given puzzle.


# Board recognition #

The algorithm that performs detection is very simple. It is the most primitive way possible - [brute-force pattern matching](http://opencv.willowgarage.com/documentation/c/object_detection.html). In essence, the individual tiles are moving across the image and looking for a place with the highest correlation between the tile and parts of the picture against which I compare tile. Then I find the maximum for all the tiles and then I determine their position on the board by finding their mutual locations (simple sorting by Y and X axes) - so it doesn't matter how accurately the user selects the area where the puzzle is.


# Finding the optimal solution #

This is the most sophisticated part of the program because a brute force solver doesn't help me. For an 8-puzzle it helps, but for the 15-puzzle not. The first version of the solver used the [A\*](http://en.wikipedia.org/wiki/A*) algorithm - it solved 8-puzzle, but after deploying on a 15-puzzle it all went to hell. The problem was that A`*` remembers all visited states, and so it always ran out of program memory and crashed (why? since 16!/2 = 10.46e+12, and I unfortunately don't have a 10TB RAM :-) - in fact it is not 15! but for simplicity of things... it doesn't matter anyway).

That was when [IDA\*](http://en.wikipedia.org/wiki/IDA*) algorithm joined the party - it is basically a hybrid emerged from the [DFS](http://en.wikipedia.org/wiki/Depth-first_search). This algorithm is also a heuristic algorithm (just like A`*`), so you need some _h_ function, such as Manhattan distance or Manhattan distance with linear conflicts (eg., if the tile _2_ is on position _3_ and tile _3_ is on position _2_, then there are not needed only 2 moves, but at least 4) - both heuristics solved 15-puzzles, but on my computer it took about 30 seconds. So I tried a heuristic called [Pattern Database](http://heuristicswiki.wikispaces.com/pattern+database). This is used for example for solving Rubik's cubes and other similar problems of the similar character. The heuristic solves only a sub-region of the puzzle and in the next step it solves another sub-region. In the database is then saved, how many moves were needed by each tile to solve a sub-region. So the solver just looks into a database that was pre-generated and uses this estimate. This procedure can be further improved by using so-called [Additive Pattern Database](http://www.ise.bgu.ac.il/faculty/felner/research/jairpdb.pdf). Detailed explanation of how both of these heuristics work is beyond the scope of this text, so it won't be further explained.

Basically the ultimate goal of any heuristic function is to return the highest value of the lower estimate, while maintaining the properties of [admissible heuristics](http://en.wikipedia.org/wiki/Admissible_heuristic). The higher estimate is, the less the main loop iterations of IDA`*` is needed. But do not be fooled - IDA`*` is really stupid in a sence, because it is pure brute force - the cleverness lies in that it seeks only paths of certain length - thus, the higher the estimate, the faster the solution is found, because it does not unsuccessfully looks for all the shorter paths.


# Implementation #

The application is written in C# as a `WinForms` application and uses the the [OpenCV](http://opencv.org/) library, which for use in managed languages ​​requires either an unsafe-code, or a wrapper. I used the [EMGU CV](http://www.emgu.com/) wrapper.