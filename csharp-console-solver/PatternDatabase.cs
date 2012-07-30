﻿using System;
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
        private Dictionary<UInt32, int>[] database;

        public PatternDatabase(PuzzleType type)
        {
            this.type = type;
            InitDatabase();
            if (!FileExists())
            {
                for (int partition = 0; partition < parts; partition++)
                {
                    BreadthFirstSearchGenerator(partition);
                    CleanDatabase(partition);   // optional space-optimization: mirror configurations
                                                // optional space-optimization: clean the database: remove all values <= node.Manahattan()
                                                // -> don't use that, because I want to have all precomputed (it doesn't take so much of space)
                }
                SaveToFile();
            }
            else
                LoadFromFile();
        }

        private void CleanDatabase(int partition)
        {
            UInt32 key, mask = ~(((UInt32)15) << (partitions[partition][(rows * cols) - 1] * 4));
            Dictionary<UInt32, int> db = new Dictionary<UInt32, int>();
            foreach (var pair in database[partition])
            {   // cancel the empty tile
                key = ((UInt32)pair.Key) & mask;    // remove the empty tile position from the hash
                if (!db.ContainsKey(key)) db.Add(key, pair.Value);
                else if (db[key] > pair.Value) db[key] = pair.Value;
            }
            database[partition] = db;   // overwrite db
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
            // ==================================================================================================================================================================== //
            // note: it could be done all in 1 'double-loop' (index loop inside of the partition loop), but I think it might be faster this way, because I can precompute [row,col] //
            //       if I wanted to speed it up, I could get rid of the partition loops, because I know that there must be less or equal to 4 partitions                            //
            // ==================================================================================================================================================================== //
            UInt32[] hash = new UInt32[parts];
            for (int partition = 0; partition < parts; partition++)
                hash[partition] = 0;
            //
            int pos, row, col, tile;
            for (int index = 0, im = board.Width * board.Height; index < im; index++)
            {
                row = index / cols;
                col = index % cols;
                for (int partition = 0; partition < parts; partition++)
                {
                    tile = board.Tiles[row, col] - 1;
                    if (partitioning[tile] == partition)    // correct partition (and not the empty tile)?
                    {
                        pos = partitions[partition][tile]; // board.Tiles is 1-based, but PatternDB is all 0-based
                        hash[partition] |= ((UInt32)index) << (pos * 4);   // then save the current index to the default tile position
                    }
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
                    parts = 8;
                    rows = cols = 3;
                    partitioning = new int[]
                    {
                        0, 1, 2,
                        3, 4, 5,
                        6, 7, 8
                    };
                    partitions = new int[][]
                    {
                        new int[]
                        {
                             0,-1,-1,
                            -1,-1,-1,
                            -1,-1, 1
                        },
                        new int[]
                        {
                            -1, 0,-1,
                            -1,-1,-1,
                            -1,-1, 1
                        },
                        new int[]
                        {
                            -1,-1, 0,
                            -1,-1,-1,
                            -1,-1, 1
                        },
                        new int[]
                        {
                            -1,-1,-1,
                             0,-1,-1,
                            -1,-1, 1
                        },
                        new int[]
                        {
                            -1,-1,-1,
                            -1, 0,-1,
                            -1,-1, 1
                        },
                        new int[]
                        {
                            -1,-1,-1,
                            -1,-1, 0,
                            -1,-1, 1
                        },
                        new int[]
                        {
                            -1,-1,-1,
                            -1,-1,-1,
                             0,-1, 1
                        },
                        new int[]
                        {
                            -1,-1,-1,
                            -1,-1,-1,
                            -1, 0, 1
                        }
                    };
                    break;

                case PuzzleType.PUZZLE_4x4:
                    parts = 3;  // 5-5-5
                    rows = cols = 4;
                    partitioning = new int[]
                    {
                        0, 0, 0, 0,
                        1, 1, 0, 2,
                        1, 1, 2, 2,
                        1, 2, 2, 3
                    };
                    partitions = new int[][]
                    {
                        new int[]
                        {
                             0, 1, 2, 3,
                            -1,-1, 4,-1,
                            -1,-1,-1,-1,
                            -1,-1,-1, 5
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,
                             0, 1,-1,-1,
                             2, 3,-1,-1,
                             4,-1,-1, 5
                        },
                        new int[]
                        {
                            -1,-1,-1,-1,
                            -1,-1,-1, 0,
                            -1,-1, 1, 2,
                            -1, 3, 4, 5
                        }
                    };
                    break;
                
                default:
                    throw new Exception("Invalid PuzzleType! Supported types are 3x3 and 4x4.");
            }
            //
            database = new Dictionary<UInt32, int>[parts];
            for (int p = 0; p < parts; p++)
                database[p] = new Dictionary<UInt32, int>();
        }

        private void BreadthFirstSearchGenerator(int partition)  // expand all possible states and store the move count estimates (if larger than Manhattan distance)
        {
            BFSNode node, next;
            Queue<BFSNode> queue = new Queue<BFSNode>();
            queue.Enqueue(new BFSNode(this));
            while (queue.Count > 0)
            {
                node = queue.Dequeue();
                if (node.CanGoLeft())
                {
                    next = node.MoveLeft(partition);
                    if (StoreInDatabase(next, partition))
                        queue.Enqueue(next);
                }
                if (node.CanGoRight())
                {
                    next = node.MoveRight(partition);
                    if (StoreInDatabase(next, partition))
                        queue.Enqueue(next);
                }
                if (node.CanGoUp())
                {
                    next = node.MoveUp(partition);
                    if (StoreInDatabase(next, partition))
                        queue.Enqueue(next);
                }
                if (node.CanGoDown())
                {
                    next = node.MoveDown(partition);
                    if (StoreInDatabase(next, partition))
                        queue.Enqueue(next);
                }
            }
        }

        private bool StoreInDatabase(BFSNode node, int partition)
        {
            UInt32 key = node.GetHashKey(partition);
            if (!database[partition].ContainsKey(key))
            {
                database[partition].Add(key, node.distance);
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

            public int distance { get; set; }

            public BFSNode(PatternDatabase db)  // initial state --> the empty tile is in the bottom right corner
            {
                this.db = db;
                this.empty_row = db.rows - 1;
                this.empty_col = db.cols - 1;
                this.direction = Direction.NONE;
                this.board = new MiniBoard(db.rows, db.cols);
                this.distance = 0;
            }

            private BFSNode(PatternDatabase db, MiniBoard board, int empty_row, int empty_col, int distance, Direction direction)
            {
                this.db = db;
                this.empty_row = empty_row;
                this.empty_col = empty_col;
                this.board = board;
                this.distance = distance;
                this.direction = direction;
            }

            public UInt32 GetHashKey(int partition)
            {
                // max. 4x4 board with partitions of size max. 5 + 1 for empty tile => save 6 positions (0-15) => 6x4b => UInt32
                UInt32 hash = 0;
                int pos;
                for (int index = 0, im = db.rows * db.cols; index < im; index++)
                {
                    pos = db.partitions[partition][board.Get(index)];
                    if (pos >= 0)  // correct partition? (including the empty tile)
                        hash |= ((UInt32)index) << (pos * 4);   // then save the current index to the default tile position
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

            public BFSNode MoveLeft(int partition)
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index - 1); b.Set(index - 1, b.Get(index)); b.Set(index, tile);
                int dist = distance + ((db.partitioning[tile] == partition) ? 1 : 0);
                return new BFSNode(db, b, empty_row, empty_col - 1, dist, Direction.LEFT);
            }

            public BFSNode MoveRight(int partition)
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + 1); b.Set(index + 1, b.Get(index)); b.Set(index, tile);
                int dist = distance + ((db.partitioning[tile] == partition) ? 1 : 0);
                return new BFSNode(db, b, empty_row, empty_col + 1, dist, Direction.RIGHT);
            }

            public BFSNode MoveUp(int partition)
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index - db.cols); b.Set(index - db.cols, b.Get(index)); b.Set(index, tile);
                int dist = distance + ((db.partitioning[tile] == partition) ? 1 : 0);
                return new BFSNode(db, b, empty_row - 1, empty_col, dist, Direction.UP);
            }

            public BFSNode MoveDown(int partition)
            {
                MiniBoard b = new MiniBoard(board);
                int index = (empty_row * db.cols) + empty_col;
                UInt64 tile = b.Get(index + db.cols); b.Set(index + db.cols, b.Get(index)); b.Set(index, tile);
                int dist = distance + ((db.partitioning[tile] == partition) ? 1 : 0);
                return new BFSNode(db, b, empty_row + 1, empty_col, dist, Direction.DOWN);
            }
        }
    }
}
