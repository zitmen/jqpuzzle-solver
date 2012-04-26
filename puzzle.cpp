#include <iostream>
#include <string>
#include <vector>
#include <queue>
#include <map>
using namespace std;

#define GO_PRE      cfg = orig_cfg;

#define GO_POS      /* check if i already was there */ \
					if(visited.find(cfg.first) == visited.end()) \
					{ \
						visited[cfg.first] = true; \
						/* check if it is solved */ \
						if(cfg.first == 123456789ULL) \
						{ \
							solution = cfg.second; \
							return; \
						} \
						q.push(cfg); \
					}

#define GO_LEFT		GO_PRE \
					digit = (cfg.first % multipliers[empty-1]) / multipliers[empty]; \
					tmp = cfg.first % multipliers[empty]; \
					cfg.first = (((cfg.first / multipliers[empty-1]) * 10 + 9) * 10 + digit) * multipliers[empty+1] + (tmp % multipliers[empty+1]); \
					cfg.second.push_back(empty-1); \
					GO_POS

#define GO_RIGHT	GO_PRE \
					digit = (cfg.first % multipliers[empty+1]) / multipliers[empty+2]; \
					tmp = cfg.first % multipliers[empty+1]; \
					cfg.first = (((cfg.first / multipliers[empty]) * 10 + digit) * 10 + 9) * multipliers[empty+2] + (tmp % multipliers[empty+2]); \
					cfg.second.push_back(empty+1); \
					GO_POS

#define GO_DOWN		GO_PRE \
					digit = (cfg.first % multipliers[empty+3]) / multipliers[empty+4]; \
					tmp = cfg.first % multipliers[empty+1]; \
					cfg.first = ((cfg.first / multipliers[empty]) * 10 + digit) * multipliers[empty+1] + tmp; \
					tmp = cfg.first % multipliers[empty+4]; \
					cfg.first = ((cfg.first / multipliers[empty+3]) * 10 + 9) * multipliers[empty+4] + tmp; \
					cfg.second.push_back(empty+3); \
					GO_POS

#define GO_UP		GO_PRE \
					digit = (cfg.first % multipliers[empty-3]) / multipliers[empty-2]; \
					tmp = cfg.first % multipliers[empty-2]; \
					cfg.first = ((cfg.first / multipliers[empty-3]) * 10 + 9) * multipliers[empty-2] + tmp; \
					tmp = cfg.first % multipliers[empty+1]; \
					cfg.first = ((cfg.first / multipliers[empty]) * 10 + digit) * multipliers[empty+1] + tmp; \
					cfg.second.push_back(empty-3); \
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

void findSolution(unsigned long long tiles, vector<byte> &solution);

int main()
{
	//
	// Load board configuration
	unsigned long long tiles = 0, input;
	cout << "What is the configuration of the puzzle board? \n1 2 3\n4 5 6\n7 8 9\n\n";
	for(int i = 0; i < 3; i++)
	{
		for(int j = 0; j < 3; j++)
		{
			cin >> input;
			tiles = tiles*10 + input;
		}
	}
	//
	// Find solution
	cout << "Solving the puzzle...";
	vector<byte> solution;
	findSolution(tiles, solution);
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

void findSolution(unsigned long long tiles, vector<byte> &solution)
{
	queue<pair<unsigned long long, vector<byte> > > q;
	map<unsigned long long, bool> visited;
	pair<unsigned long long, vector<byte> > cfg, orig_cfg;
	unsigned long long digit, tmp;
	byte empty;
	//
	q.push(make_pair(tiles,vector<byte>()));
	q.front().second.push_back(9);
	//
	unsigned long long multipliers[11] = { 10000000000ULL,
											1000000000ULL, 100000000ULL, 10000000ULL,
											   1000000ULL,    100000ULL,    10000ULL,
											      1000ULL,       100ULL,       10ULL,
										             1ULL };
	//
	while(!q.empty())
	{
		orig_cfg = cfg = q.front(); q.pop();
		// try a new configuration
		switch(empty = cfg.second.back())	// last command == empty field
		{
			case 1:          GO_RIGHT;        GO_DOWN; break;
			case 2: GO_LEFT; GO_RIGHT;        GO_DOWN; break;
			case 3: GO_LEFT;                  GO_DOWN; break;
			case 4:          GO_RIGHT; GO_UP; GO_DOWN; break;
			case 5: GO_LEFT; GO_RIGHT; GO_UP; GO_DOWN; break;
			case 6: GO_LEFT;           GO_UP; GO_DOWN; break;
			case 7:          GO_RIGHT; GO_UP;          break;
			case 8: GO_LEFT; GO_RIGHT; GO_UP;          break;
			case 9: GO_LEFT;           GO_UP;          break;
		}
	}
}
