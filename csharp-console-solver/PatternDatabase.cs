using System;
using System.IO;
using System.Collections.Generic;

// TODO: check the rest and try to generate the DB, then do file-save/load and (de)compress, if necessary

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
                    writer.Write(database[dbi].Count.ToString());
                    // Write pairs
                    foreach (var pair in database[dbi])
                    {
                        writer.Write(pair.Key.ToString());
                        writer.Write(pair.Value.ToString());
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
                        UInt32 key = reader.ReadUInt32();
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
            // TODO: sum of database[partition][miniboard.getKey] or manhattan, if db not defined for the board
            return int.MaxValue;
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
                            -1,-1,-1,-1,
                            -1,-1, 0, 1,
                            -1,-1, 2, 3,
                            -1, 4, 5,-1
                        },
                        new int[]
                        {
                            -1, 0, 1, 2,
                            -1,-1,-1,-1,
                            -1,-1,-1,-1,
                            -1,-1,-1,-1
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
            //
            database = new Dictionary<UInt32, int>[parts];
            for (int p = 0; p < parts; p++)
                database[p] = new Dictionary<UInt32, int>();
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
                    if((next = node.MoveLeft()) != null)
                        if(StoreInDatabase(next))
                            queue.Enqueue(next);
                }
                if (node.CanGoRight())
                {
                    if ((next = node.MoveRight()) != null)
                        if (StoreInDatabase(next))
                            queue.Enqueue(next);
                }
                if (node.CanGoUp())
                {
                    if ((next = node.MoveUp()) != null)
                        if (StoreInDatabase(next))
                            queue.Enqueue(next);
                }
                if (node.CanGoDown())
                {
                    if ((next = node.MoveDown()) != null)
                        if (StoreInDatabase(next))
                            queue.Enqueue(next);
                }
            }
        }

        private bool StoreInDatabase(BFSNode node)
        {
            UInt32 key = node.GetHashKey();
            if (!database[node.Partition].ContainsKey(key))
            {
                database[node.Partition].Add(key, node.Distance);
                return true;
            }
            return false;
        }

        private class BFSNode
        {
            private PatternDatabase db;
            private MiniBoard board;
            private int empty_row, empty_col;
            private Direction direction;
            private int[] distance;

            public int Partition { get; set; }
            public int Distance { get { return distance[Partition]; } set { distance[Partition] = value; } }

            public BFSNode(PatternDatabase db)  // initial state --> the empty tile is in the bottom right corner
            {
                this.Partition = -1;
                this.db = db;
                this.empty_row = db.rows - 1;
                this.empty_col = db.cols - 1;
                this.direction = Direction.NONE;
                this.board = new MiniBoard(db.rows, db.cols);
                this.distance = new int[db.parts];
                for (int i = 0; i < db.parts; i++)
                    this.distance[i] = 0;
            }

            private BFSNode(PatternDatabase db, int partition, MiniBoard board, int empty_row, int empty_col, int[] distance, Direction direction)
            {
                this.db = db;
                this.empty_row = empty_row;
                this.empty_col = empty_col;
                this.board = board;
                this.distance = distance;
                this.direction = direction;
                this.Partition = partition;
            }

            public UInt32 GetHashKey()
            {
                // max. 5x5 board with partitions of size max. 6 => save 6 positions (0-25) => 6x5b => UInt32
                UInt32 hash = 0;
                int pos;
                for (int index = 0, im = db.partitions[Partition].Length; index < im; index++)
                {
                    pos = db.partitions[Partition][board.Get(index)];
                    if (pos >= 0)  // corrent partition?
                        hash |= ((UInt32)index) << (pos * 5);   // then save the current index to the default tile position
                }
                return hash;
            }

            public int Manhattan(int partition) // sum of Manahattan distances of all tiles in the partition (could be generalizes to the whole board)
            {
                int h = 0, val;
                for (int i = 0, im = db.partitions[Partition].Length; i < im; i++)
                {
                    val = (int)board.Get(i);
                    if (db.partitions[Partition][val] >= 0)
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
                int[] dist = (int[])distance.Clone();
                for (int partition = 0; partition < db.parts; partition++)
                {
                    if (db.partitions[partition][tile] >= 0) // only 1 tile moves each step => only 1 distance[partition] will change
                    {
                        dist[partition] = distance[partition] + 1;
                        return new BFSNode(db, partition, b, empty_row, empty_col - 1, dist, Direction.LEFT);
                    }
                }
                return null;
            }

            public BFSNode MoveRight()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + 1); b.Set(index + 1, b.Get(index)); b.Set(index, tile);
                int[] dist = (int[])distance.Clone();
                for (int partition = 0; partition < db.parts; partition++)
                {
                    if (db.partitions[partition][tile] >= 0) // only 1 tile moves each step => only 1 distance[partition] will change
                    {
                        dist[partition] = distance[partition] + 1;
                        return new BFSNode(db, partition, b, empty_row, empty_col + 1, dist, Direction.RIGHT);
                    }
                }
                return null;
            }

            public BFSNode MoveUp()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index - db.cols); b.Set(index - db.cols, b.Get(index)); b.Set(index, tile);
                int[] dist = (int[])distance.Clone();
                for (int partition = 0; partition < db.parts; partition++)
                {
                    if (db.partitions[partition][tile] >= 0) // only 1 tile moves each step => only 1 distance[partition] will change
                    {
                        dist[partition] = distance[partition] + 1;
                        return new BFSNode(db, partition, b, empty_row - 1, empty_col, dist, Direction.UP);
                    }
                }
                return null;
            }

            public BFSNode MoveDown()
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + db.cols); b.Set(index + db.cols, b.Get(index)); b.Set(index, tile);
                int[] dist = (int[])distance.Clone();
                for (int partition = 0; partition < db.parts; partition++)
                {
                    if (db.partitions[partition][tile] >= 0) // only 1 tile moves each step => only 1 distance[partition] will change
                    {
                        dist[partition] = distance[partition] + 1;
                        return new BFSNode(db, partition, b, empty_row + 1, empty_col, dist, Direction.DOWN);
                    }
                }
                return null;
            }
        }
    }
}
