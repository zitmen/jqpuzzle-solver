using System;
using System.Collections.Generic;

// TODO: check if bit operations work properly! then check the rest and try to generate the DB, then do file-save/load and (de)compress, if necessary

namespace csharp_console_solver
{
    class PatternDatabase
    {
        private PuzzleType type;
        private int rows, cols, parts;
        private int[][] partitions; // [partition_index][tile_index] --> negative value = another partition,  values >= 0 = positions (indices) of tiles in hash key
        private Dictionary<UInt32, int>[] database;

        public PatternDatabase(PuzzleType type)
        {
            this.type = type;
            if (FileExists())
                LoadFromFile();
            else
            {
                Generate();
                SaveToFile();
            }
        }

        public void Generate()
        {
            InitGenerator();
            //
            database = new Dictionary<UInt32, int>[parts];
            for (int p = 0; p < parts; p++)
                BreadthFirstSearchGenerator(p);
        }

        public void SaveToFile()
        {
            /*
            // Example Dictionary again
            Dictionary<string, int> d = new Dictionary<string, int>()
            {
                {"cat", 2},
                {"dog", 1},
                {"llama", 0},
                {"iguana", -1}
            };
            // Loop over pairs with foreach
            foreach (KeyValuePair<string, int> pair in d)
            {
                Console.WriteLine("{0}, {1}",
                    pair.Key,
                    pair.Value);
            }
            // Use var keyword to enumerate dictionary
            foreach (var pair in d)
            {
                Console.WriteLine("{0}, {1}",
                    pair.Key,
                    pair.Value);
            }
             */
        }

        public void LoadFromFile()
        {
            //if not exists throw new Exception("File does not exist!");
        }

        public bool FileExists()
        {
            //filename=puzzle_NxN_pP.db
            return false;
        }

        public int GetEstimate(Board board)
        {
            // TODO: sum of database[partition][miniboard.getKey] or manhattan, if db not defined for the board
            return int.MaxValue;
        }

        private void InitGenerator()
        {
            switch (type)
            {
                case PuzzleType.PUZZLE_3x3:
                    parts = 1;  // 9
                    rows = cols = 3;
                    partitions = new int[][]
                    {
                        new int[]
                        {
                            0, 1, 2,
                            3, 4, 5,
                            6, 7, 8
                        }
                    };
                    break;

                case PuzzleType.PUZZLE_4x4:
                    parts = 3;  // 6-6-3
                    rows = cols = 4;
                    partitions = new int[][]
                    {
                        new int[]
                        {
                            0,-1,-1,-1,
                            1, 2,-1,-1,
                            3, 4,-1,-1,
                            5,-1,-1,-1
                        },
                        new int[]
                        {
                            -1, 0, 1, 2,
                            -1,-1,-1,-1,
                            -1,-1,-1,-1,
                            -1,-1,-1,-1
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,
                            -1,-1, 0, 1,
                            -1,-1, 2, 3,
                            -1, 4, 5,-1
                        }
                    };
                    break;

                case PuzzleType.PUZZLE_5x5:
                    parts = 4;  // 6-6-6-6
                    rows = cols = 5;
                    partitions = new int[][]
                    {
                        new int[]
                        {
                             0, 1, 2,-1,-1,
                             3, 4, 5,-1,-1,
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1
                        },
                        new int[]
                        {
                            -1,-1,-1, 0, 1,
                            -1,-1,-1, 2, 3,
                            -1,-1,-1, 4, 5,
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1,
                             0, 1,-1,-1,-1,
                             2, 3,-1,-1,-1,
                             4, 5,-1,-1,-1
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1,
                            -1,-1, 0,-1,-1,
                            -1,-1, 1, 2, 3,
                            -1,-1, 4, 5,-1
                        }
                    };
                    break;

                default:
                    throw new Exception("Invalid PuzzleType! Supported types are 3x3, 4x4 and 5x5.");
            }
        }

        private void BreadthFirstSearchGenerator(int partition)  // expand all possible states and store the move count estimates (if larger than Manhattan distance)
        {
            BFSNode node;
            Queue<BFSNode> queue = new Queue<BFSNode>();
            queue.Enqueue(new BFSNode(this, partition));
            while (queue.Count > 0)
            {
                node = queue.Dequeue();
                StoreInDatabase(partition, node);
                //
                if (node.CanGoLeft()) queue.Enqueue(node.MoveLeft());
                if (node.CanGoRight()) queue.Enqueue(node.MoveRight());
                if (node.CanGoUp()) queue.Enqueue(node.MoveUp());
                if (node.CanGoDown()) queue.Enqueue(node.MoveDown());
            }
        }

        private void StoreInDatabase(int partition, BFSNode node)
        {
            UInt32 key = node.GetHashKey();
            if (!database[partition].ContainsKey(key))
                if (node.distance > node.Manhattan())
                    database[partition].Add(key, node.distance);
        }

        private class BFSNode
        {
            private PatternDatabase db;
            private int partition;
            private MiniBoard board;
            private int empty_row, empty_col;
            private Direction direction;
            public int distance { get; set; }

            public BFSNode(PatternDatabase db, int partition)  // initial state --> the empty tile is in the bottom right corner
            {
                this.db = db;
                this.partition = partition;
                this.distance = 0;
                this.empty_row = db.rows - 1;
                this.empty_col = db.cols - 1;
                this.direction = Direction.NONE;
                this.board = new MiniBoard(db.rows, db.cols);
            }

            private BFSNode(PatternDatabase db, int partition, MiniBoard board, int empty_row, int empty_col, int distance, Direction direction)
            {
                this.db = db;
                this.partition = partition;
                this.empty_row = empty_row;
                this.empty_col = empty_col;
                this.board = board;
                this.distance = distance;
                this.direction = direction;
            }

            public UInt32 GetHashKey()
            {
                // max. 5x5 board with partitions of size max. 6 => save 6 positions (0-25) => 6x5b => UInt32
                UInt32 hash = 0;
                int pos;
                for (int index = 0, im = db.partitions[partition].Length; index < im; index++)
                {
                    pos = db.partitions[partition][board.Get(index)];
                    if (pos >= 0)  // corrent partition?
                        hash |= ((UInt32)index) << (pos * 5);   // then save the current index to the default tile position
                }
                return hash;
            }

            public int Manhattan()
            {
                int h = 0, val;
                for (int i = 0, im = db.partitions[partition].Length; i < im; i++)
                {
                    val = (int)board.Get(i);
                    if (db.partitions[partition][val] >= 0)
                        h += Math.Abs((i / db.cols) - (val / db.cols)) + Math.Abs((i % db.cols) - (val % db.cols));
                }
                return h;
            }

            public bool CanGoLeft()
            {
                return ((empty_col > 0) && (direction != Direction.RIGHT));
            }

            public bool CanGoRight()
            {
                return ((empty_col < (db.cols - 1)) && (direction != Direction.LEFT));
            }

            public bool CanGoUp()
            {
                return ((empty_row > 0) && (direction != Direction.DOWN));
            }

            public bool CanGoDown()
            {
                return ((empty_row < (db.rows - 1)) && (direction != Direction.UP));
            }

            public BFSNode MoveLeft()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index - 1); b.Set(index - 1, b.Get(index)); b.Set(index, tile);
                return new BFSNode(db, partition, b, empty_row, empty_col - 1, distance + ((db.partitions[partition][tile] < 0) ? 0 : 1), Direction.LEFT);
            }

            public BFSNode MoveRight()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + 1); b.Set(index + 1, b.Get(index)); b.Set(index, tile);
                return new BFSNode(db, partition, b, empty_row, empty_col + 1, distance + ((db.partitions[partition][tile] < 0) ? 0 : 1), Direction.RIGHT);
            }

            public BFSNode MoveUp()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index - db.cols); b.Set(index - db.cols, b.Get(index)); b.Set(index, tile);
                return new BFSNode(db, partition, b, empty_row - 1, empty_col, distance + ((db.partitions[partition][tile] < 0) ? 0 : 1), Direction.UP);
            }

            public BFSNode MoveDown()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + db.cols); b.Set(index + db.cols, b.Get(index)); b.Set(index, tile);
                return new BFSNode(db, partition, b, empty_row + 1, empty_col, distance + ((db.partitions[partition][tile] < 0) ? 0 : 1), Direction.DOWN);
            }
        }
    }
}
