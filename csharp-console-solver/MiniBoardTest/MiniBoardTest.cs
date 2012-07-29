using csharp_console_solver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MiniBoardTest
{
    [TestClass()]
    public class MiniBoardTest
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        /// <summary>
        ///A test for Get and Set
        ///</summary>
        [TestMethod()]
        public void GetSetTest()
        {
            // init + get
            MiniBoard board = new MiniBoard(5, 5);
            for (int i = 0; i < 25; i++)
                Assert.AreEqual(i, (int)board.Get(i), "init + get");
            //
            // set + get from i = [0,12]
            board.Set(6, 10);
            Assert.AreEqual(10, (int)board.Get(6), "set + get from i = [0,11]");
            //
            // set + get from i = [13,24]
            board.Set(18, 10);
            Assert.AreEqual(10, (int)board.Get(18), "set + get from i = [12,23]");
            //
            // set + get from i = [25]
            board.Set(24, 10);
            Assert.AreEqual(10, (int)board.Get(24), "set + get from i = [24]");
            //
            // get: out of range exception
            bool thrown = false;
            try { board.Get(25); }
            catch (IndexOutOfRangeException ex) { thrown = true; }
            Assert.IsTrue(thrown, "get: out of range exception");
            //
            // set: out of range exception
            thrown = false;
            try { board.Set(25, 25); }
            catch (IndexOutOfRangeException ex) { thrown = true; }
            Assert.IsTrue(thrown, "set: out of range exception");
        }
    }
}
