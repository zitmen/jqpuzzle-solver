using System;
using System.IO;
using System.Collections.Generic;

namespace csharp_console_solver
{
    class PatternDatabase
    {
        private PuzzleType type;
        private int rows, cols, parts;
        private int[][] partitions; // [partition_index][tile_index] --> negative value = another partition,  values >= 0 = positions (indices) of tiles in hash key, value == parts = empty tile
        private int[] partitioning; // indices into `partitions`
        private Dictionary<UInt64, int>[] database;

        public PatternDatabase(PuzzleType type)
        {
            this.type = type;
            InitDatabase();
            if (FileExists())
                LoadFromFile();
            else
            {
                BreadthFirstSearchGenerator();  // space-optimization: clean the database: remove all values <= node.Manahattan()
                                                // -> don't use that, because I want to have all precomputed (it doesn't take so much of space)
                SaveToFile();
            }
        }

        private void SaveToFile()
        {
            for (int dbi = 0; dbi < parts; dbi++)
            {
                string filename = string.Format("puzzle_{0:d}x{1:d}_p{2:d}.db", rows, cols, dbi);
                using (FileStream fs = File.OpenWrite(filename))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    // Put count
                    writer.Write(database[dbi].Count);
                    // Write pairs
                    foreach (var pair in database[dbi])
                    {
                        writer.Write(pair.Key);
                        writer.Write(pair.Value);
                    }
                }
            }
        }

        private void LoadFromFile()
        {
            for (int dbi = 0; dbi < parts; dbi++)
            {
                string filename = string.Format("puzzle_{0:d}x{1:d}_p{2:d}.db", rows, cols, dbi);
                using (FileStream fs = File.OpenRead(filename))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // Get count
                    int count = reader.ReadInt32();
                    // Read in all pairs
                    for (int i = 0; i < count; i++)
                    {
                        UInt64 key = reader.ReadUInt64();
                        int value = reader.ReadInt32();
                        database[dbi].Add(key, value);
                    }
                }
            }
        }

        private bool FileExists()
        {
            for (int dbi = 0; dbi < parts; dbi++)
            {
                string filename = string.Format("puzzle_{0:d}x{1:d}_p{2:d}.db", rows, cols, dbi);
                if (!File.Exists(filename))
                    return false;
            }
            return true;
        }

        public int GetEstimate(Board board)
        {
            // ==================================================================================================================================================================== //
            // note: it could be done all in 1 'double-loop' (index loop inside of the partition loop), but I think it might be faster this way, because I can precompute [row,col] //
            //       if I wanted to speed it up, I could get rid of the partition loops, because I know that there must be less or equal to 4 partitions                            //
            // ==================================================================================================================================================================== //
            UInt64[] hash = new UInt64[parts];
            for (int partition = 0; partition < parts; partition++)
                hash[partition] = 0;
            //
            int pos, row, col;
            for (int index = 0, im = board.Width * board.Height; index < im; index++)
            {
                row = index / cols;
                col = index % cols;
                for (int partition = 0; partition < parts; partition++)
                {
                    pos = partitions[partition][board.Tiles[row, col] - 1]; // board.Tiles is 1-based, but PatternDB is all 0-based
                    if (pos >= 0)  // correct partition?
                        hash[partition] |= ((UInt64)index) << (pos * 5);   // then save the current index to the default tile position
                }
            }
            //
            int estimate = 0;
            for (int partition = 0; partition < parts; partition++)
                if (database[partition].ContainsKey(hash[partition]))
                    estimate += database[partition][hash[partition]];
            //
            return estimate;
        }

        private void InitDatabase()
        {
            switch (type)
            {
                case PuzzleType.PUZZLE_3x3:
                    parts = 1;  // 9
                    rows = cols = 3;
                    partitions = new int[][]
                    {
// CHYBA!! tady by melo bejt 9 parts!! kazda tile je v jednom
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
                    partitioning = new int[]
                    {
                        0, 2, 2, 2,
                        0, 0, 1, 1,
                        0, 0, 1, 1,
                        0, 1, 1, 3
                    };
                    partitions = new int[][]
                    {
                        new int[]
                        {
                             0,-1,-1,-1,
                             1, 2,-1,-1,
                             3, 4,-1,-1,
                             5,-1,-1, 6
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,
                            -1,-1, 0, 1,
                            -1,-1, 2, 3,
                            -1, 4, 5, 6
                        },
                        new int[]
                        {
                            -1, 0, 1, 2,
                            -1,-1,-1,-1,
                            -1,-1,-1,-1,
                            -1,-1,-1, 6
                        }
                    };
                    break;

                case PuzzleType.PUZZLE_5x5:
                    parts = 4;  // 6-6-6-6
                    rows = cols = 5;
                    partitioning = new int[]
                    {
                        0, 0, 0, 1, 1,
                        0, 0, 0, 1, 1,
                        2, 2, 3, 1, 1,
                        2, 2, 3, 3, 3,
                        2, 2, 3, 3, 4
                    };
                    partitions = new int[][]
                    {
                        new int[]
                        {
                             0, 1, 2,-1,-1,
                             3, 4, 5,-1,-1,
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1, 6
                        },
                        new int[]
                        {
                            -1,-1,-1, 0, 1,
                            -1,-1,-1, 2, 3,
                            -1,-1,-1, 4, 5,
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1, 6
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1,
                             0, 1,-1,-1,-1,
                             2, 3,-1,-1,-1,
                             4, 5,-1,-1, 6
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,-1,
                            -1,-1,-1,-1,-1,
                            -1,-1, 0,-1,-1,
                            -1,-1, 1, 2, 3,
                            -1,-1, 4, 5, 6
                        }
                    };
                    break;

                default:
                    throw new Exception("Invalid PuzzleType! Supported types are 3x3, 4x4 and 5x5.");
            }
            //
            database = new Dictionary<UInt64, int>[parts];
            for (int p = 0; p < parts; p++)
                database[p] = new Dictionary<UInt64, int>();
        }

        private void BreadthFirstSearchGenerator()  // expand all possible states and store the move count estimates (if larger than Manhattan distance)
        {
            BFSNode node, next;
            Queue<BFSNode> queue = new Queue<BFSNode>();
            queue.Enqueue(new BFSNode(this));
            while (queue.Count > 0)
            {
                node = queue.Dequeue();
                if (node.CanGoLeft())
                {
                    next = node.MoveLeft();
                    if(StoreInDatabase(next))
                        queue.Enqueue(next);
                }
                if (node.CanGoRight())
                {
                    next = node.MoveRight();
                    if (StoreInDatabase(next))
                        queue.Enqueue(next);
                }
                if (node.CanGoUp())
                {
                    next = node.MoveUp();
                    if (StoreInDatabase(next))
                        queue.Enqueue(next);
                }
                if (node.CanGoDown())
                {
                    next = node.MoveDown();
                    if (StoreInDatabase(next))
                        queue.Enqueue(next);
                }
            }
        }

        private bool StoreInDatabase(BFSNode node)
        {
            UInt64 key;
            bool stored = false;
            for (int partition = 0; partition < parts; partition++)
            {
                key = node.GetHashKey(partition);
                if (!database[partition].ContainsKey(key))
                {
                    database[partition].Add(key, node.distance[partition]);
                    stored = true;
                }
            }
            return stored;
        }

        private class BFSNode
        {
            private PatternDatabase db;
            private MiniBoard board;
            private int empty_row, empty_col;
            private Direction direction;

            public int[] distance { get; set; }

            public BFSNode(PatternDatabase db)  // initial state --> the empty tile is in the bottom right corner
            {
                this.db = db;
                this.empty_row = db.rows - 1;
                this.empty_col = db.cols - 1;
                this.direction = Direction.NONE;
                this.board = new MiniBoard(db.rows, db.cols);
                this.distance = new int[db.parts];
                for (int i = 0; i < db.parts; i++)
                    this.distance[i] = 0;
            }

            private BFSNode(PatternDatabase db, MiniBoard board, int empty_row, int empty_col, int[] distance, Direction direction)
            {
                this.db = db;
                this.empty_row = empty_row;
                this.empty_col = empty_col;
                this.board = board;
                this.distance = distance;
                this.direction = direction;
                //
                if (board.Get(4) == 1UL && board.Get(10) == 2UL && board.Get(12) == 3UL)
                    throw new Exception("THIS IS IT!");
            }

            public UInt64 GetHashKey(int partition)
            {
                // max. 5x5 board with partitions of size max. 6 + 1 for empty tile => save 7 positions (0-24) => 7x5b => UInt64
                UInt64 hash = 0;
                int pos;
                for (int index = 0, im = db.rows * db.cols; index < im; index++)
                {
                    pos = db.partitions[partition][board.Get(index)];
                    if (pos >= 0)  // correct partition?
                        hash |= ((UInt64)index) << (pos * 5);   // then save the current index to the default tile position
                }
                return hash;
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
                int[] dist = (int[])distance.Clone();
                int partition = db.partitioning[tile];
                dist[partition] = distance[partition] + 1;
                return new BFSNode(db, b, empty_row, empty_col - 1, dist, Direction.LEFT);
            }

            public BFSNode MoveRight()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + 1); b.Set(index + 1, b.Get(index)); b.Set(index, tile);
                int[] dist = (int[])distance.Clone();
                int partition = db.partitioning[tile];
                dist[partition] = distance[partition] + 1;
                return new BFSNode(db, b, empty_row, empty_col + 1, dist, Direction.RIGHT);
            }

            public BFSNode MoveUp()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index - db.cols); b.Set(index - db.cols, b.Get(index)); b.Set(index, tile);
                int[] dist = (int[])distance.Clone();
                int partition = db.partitioning[tile];
                dist[partition] = distance[partition] + 1;
                return new BFSNode(db, b, empty_row - 1, empty_col, dist, Direction.UP);
            }

            public BFSNode MoveDown()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + db.cols); b.Set(index + db.cols, b.Get(index)); b.Set(index, tile);
                int[] dist = (int[])distance.Clone();
                int partition = db.partitioning[tile];
                dist[partition] = distance[partition] + 1;
                return new BFSNode(db, b, empty_row + 1, empty_col, dist, Direction.DOWN);
            }
        }
    }
}
