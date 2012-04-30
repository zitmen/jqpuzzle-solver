#include <iostream>
#include <string>
#include <vector>
#include <queue>
#include <map>
using namespace std;

#define SWAP(a,b)   { char tmp = (a); (a) = (b); (b) = tmp; }

#define STRI(index) ((index)-1)

#define GO_PRE      cfg = orig_cfg;

#define GO_POS      /* check if i already was there */ \
					if(visited.find(cfg.first) == visited.end()) \
					{ \
						visited[cfg.first] = true; \
						q.push(make_pair(h = fnH(cfg.first, rows, cols), cfg)); \
						/* check if it is solved */ \
						if(h == 0/*isSolved(cfg.first)*/) \
						{ \
							solution = cfg.second; \
							return; \
						} \
					}

#define GO_LEFT		GO_PRE \
					SWAP(cfg.first[STRI(empty)],cfg.first[STRI(empty-1)]) \
					cfg.second.push_back(empty-1); \
					GO_POS

#define GO_RIGHT	GO_PRE \
					SWAP(cfg.first[STRI(empty)],cfg.first[STRI(empty+1)]) \
					cfg.second.push_back(empty+1); \
					GO_POS

#define GO_DOWN		GO_PRE \
					SWAP(cfg.first[STRI(empty)],cfg.first[STRI(empty+cols)]) \
					cfg.second.push_back(empty+cols); \
					GO_POS

#define GO_UP		GO_PRE \
					SWAP(cfg.first[STRI(empty)],cfg.first[STRI(empty-cols)]) \
					cfg.second.push_back(empty-cols); \
					GO_POS

// ================================================================ //
// - 4px mezera mezi dlazdicema                                     //
//   - 2x 1px border + 1x 2px margin                                //
// - pokud neni obrazek patricne delitelny, urizne se vpravo a dole //
//                                                                  //
// napr.: image : 500x356 px                                        //
//        width : 500-2-4-4-2=488, 548/3 = 162,667px -> ceil: 163px //
//        height: 356-2-4-4-2=344, 344/3 = 114,667px -> ceil: 115px //
// ================================================================ //

typedef unsigned char byte;

void findSolution(const char *tiles, int rows, int cols, vector<byte> &solution);
bool isSolved(const string &board);
int fnH(const string &board, int rows, int cols);	// heuristic function

int main()
{
	//
	// Load board configuration
	int rows, cols, input;
	cout << "What are the dimensions of the puzzle board?\nRows: ";
	cin >> rows;
	cout << "Columns: ";
	cin >> cols;
	char *tiles = new char[rows*cols+1];
	cout << "What is the configuration of the puzzle board? \n1 2 3\n4 5 6\n7 8 9\n\n";
	for(int i = 0; i < rows; i++)
		for(int j = 0; j < cols; j++)
			{ cin >> input; tiles[i*cols+j] = input; }
	tiles[rows*cols] = 0;
	//
	// Find solution
	cout << "Solving the puzzle...";
	vector<byte> solution;
	findSolution(tiles, rows, cols, solution);
	cout << "done.\n";
	//
	// Print the solution
	cout << "\nSolution (" << (solution.size()-1)  << " moves):\n";
	for(size_t i = 1, im = solution.size(); i < im; ++i)
		cout << int(solution[i]) << ',';
	cout << endl;
	//
	return 0;
}

class CompareH
{
	public:
		bool operator()(pair<int, pair<string, vector<byte> > > &a, pair<int, pair<string, vector<byte> > > &b)
		{
			if(b.second.second.size() < a.second.second.size()) return true;
			if(b.second.second.size() > a.second.second.size()) return false;
			/* == */ return (b.first < a.first);
		}
};

void findSolution(const char *tiles, int rows, int cols, vector<byte> &solution)
{
	priority_queue<pair<int, pair<string, vector<byte> > >, vector<pair<int, pair<string, vector<byte> > > >, CompareH > q;		// board state, vector of moves to get there
	map<string, bool> visited;					// board state, (not)visited
	pair<string, vector<byte> > cfg, orig_cfg;	// board state, vector of moves to get there
	byte empty, row, col;						// empty field on the board
	int h;
	//
	// insert the initial state
	q.push(make_pair(fnH(string(tiles), rows, cols), make_pair(string(tiles),vector<byte>())));
	q.top().second.second.push_back(rows*cols);
	//
	// go through other states
	while(!q.empty())
	{
		orig_cfg = cfg = q.top().second; q.pop();
		// try a new configuration
		empty = cfg.second.back();	// last command == empty field
		row = ((empty-1) / cols) + 1;
		col = ((empty-1) % cols) + 1;
		//
		if(col == 1)
		{
			     if(row == 1   ) { GO_RIGHT; GO_DOWN; }
			else if(row == rows) { GO_RIGHT; GO_UP  ; }
			else                 { GO_RIGHT; GO_DOWN; GO_UP; }
		}
		else if(col == cols)
		{
			     if(row == 1   ) { GO_LEFT; GO_DOWN; }
			else if(row == rows) { GO_LEFT; GO_UP  ; }
			else                 { GO_LEFT; GO_DOWN; GO_UP; }
		}
		else if(row == 1)		// cols boundaries was already resolved in the first 2 conditions
			{ GO_DOWN; GO_LEFT; GO_RIGHT; }
		else if(row == rows)	// cols boundaries was already resolved in the first 2 conditions
			{ GO_UP  ; GO_LEFT; GO_RIGHT; }
		else	// can go enywhere :)
			{ GO_UP  ; GO_LEFT; GO_RIGHT; GO_DOWN; }
	}
}

bool isSolved(const string &board)
{
	for(size_t i = board.length()-1; i > 0; --i)
		if(board[i] != (board[i-1]+1))
			return false;
	return true;
}

int fnH(const string &board, int rows, int cols)
{
/* event this was not improving heuristic function :( */
	// Distance to between neighnours of each tile
	int h = 0;
	for(int r = 0, rm = rows-1; r < rm; r++)
	{
		for(int c = 0, cm = cols-1; c < cm; c++)
		{
			h += abs(board[r*cols+c] - board[r*cols+c+1]);		// right
			h += abs(board[r*cols+c] - board[r*cols+c+cols]);	// down
		}
	}
	return h;
//
/* this was not improving heuristic function
	// Manhattan distance between current tile positions and solution
	int h = 0, tmp;
	for(int r = 1; r <= rows; r++)
	{
		for(int c = 1; c <= cols; c++)
		{
			tmp = board[STRI(r)*cols+STRI(c)];
			h += abs((((tmp-1) / cols) + 1) - r);
			h += abs((((tmp-1) % cols) + 1) - c);
		}
	}
	return h;
*/
}