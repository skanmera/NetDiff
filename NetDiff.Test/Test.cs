using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetDiff.Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void TestEquals()
        {
            var str1 = "abcde";
            var str2 = "abcde";

            var option = DiffOption<char>.Default;

            option.Order = DiffOrder.GreedyDeleteFirst;
            var results1 = NetDiff.Diff(str1, str2, option);
            option.Order = DiffOrder.GreedyInsertFirst;
            var results2 = NetDiff.Diff(str1, str2, option);
            option.Order = DiffOrder.LazyDeleteFirst;
            var results3 = NetDiff.Diff(str1, str2, option);
            option.Order = DiffOrder.LazyInsertFirst;
            var results4 = NetDiff.Diff(str1, str2, option);

            Assert.AreEqual(str1.Count(), results1.Count());
            Assert.IsTrue(results1.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results1.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results1.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));

            Assert.AreEqual(str1.Count(), results2.Count());
            Assert.IsTrue(results2.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results2.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results2.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));

            Assert.AreEqual(str1.Count(), results3.Count());
            Assert.IsTrue(results3.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results3.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results3.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));

            Assert.AreEqual(str1.Count(), results4.Count());
            Assert.IsTrue(results4.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results4.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results4.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));
        }

        [TestMethod]
        public void TestRestore()
        {
            var str1 = "q4DU8sbeD4JdhFA4hWShCv3bbtD7djX5SaNnQUHJHdCEJs6X2LJipbEEr7bZZbzcUrpuKpRDKNz92x5P";
            var str2 = "3GKLWNDdCxip8kda2r2MUT45RrHUiESQhmhUZtMcpBGcSwJVS9uq4DWBAQk2zPUJCJabaeWuP5mxyPBz";

            var option = DiffOption<char>.Default;

            option.Order = DiffOrder.GreedyDeleteFirst;
            var results1 = NetDiff.Diff(str1, str2, option);
            option.Order = DiffOrder.GreedyInsertFirst;
            var results2 = NetDiff.Diff(str1, str2, option);
            option.Order = DiffOrder.LazyDeleteFirst;
            var results3 = NetDiff.Diff(str1, str2, option);
            option.Order = DiffOrder.LazyInsertFirst;
            var results4 = NetDiff.Diff(str1, str2, option);
            option.Limit = 10;
            var results5 = NetDiff.Diff(str1, str2, option);

            var src1 = new string(NetDiff.CreateSrc(results1).ToArray());
            var src2 = new string(NetDiff.CreateSrc(results2).ToArray());
            var src3 = new string(NetDiff.CreateSrc(results3).ToArray());
            var src4 = new string(NetDiff.CreateSrc(results4).ToArray());
            var src5 = new string(NetDiff.CreateSrc(results5).ToArray());

            var dst1 = new string(NetDiff.CreateDst(results1).ToArray());
            var dst2 = new string(NetDiff.CreateDst(results2).ToArray());
            var dst3 = new string(NetDiff.CreateDst(results3).ToArray());
            var dst4 = new string(NetDiff.CreateDst(results4).ToArray());
            var dst5 = new string(NetDiff.CreateDst(results5).ToArray());

            Assert.AreEqual(src1, str1);
            Assert.AreEqual(src2, str1);
            Assert.AreEqual(src3, str1);
            Assert.AreEqual(src4, str1);
            Assert.AreEqual(src5, str1);

            Assert.AreEqual(dst1, str2);
            Assert.AreEqual(dst2, str2);
            Assert.AreEqual(dst3, str2);
            Assert.AreEqual(dst4, str2);
            Assert.AreEqual(dst5, str2);
        }

        /*
             a b a b a b
             - + - + - +
        */
        [TestMethod]
        public void TestLazyInsertFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var option = new DiffOption<char>
            {
                Order = DiffOrder.LazyInsertFirst,
            };

            var results = NetDiff.Diff(str1, str2, option);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(5).Status);
        }

        /*
             b a b a b a
             + - + - + -
        */
        [TestMethod]
        public void TestLazyDeleteFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var option = new DiffOption<char>
            {
                Order = DiffOrder.LazyDeleteFirst,
            };

            var results = NetDiff.Diff(str1, str2, option);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(5).Status);
        }

        /*
             b b b a a a 
             + + + - - -
        */
        [TestMethod]
        public void TestGreedyInsertFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var option = new DiffOption<char>
            {
                Order = DiffOrder.GreedyInsertFirst,
            };

            var results = NetDiff.Diff(str1, str2, option);

            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(5).Status);
        }

        /*
             a a a b b b 
             - - - + + +
        */
        [TestMethod]
        public void TestGreedyDeleteFirst()
        {
            var str1 = "aaa";
            var str2 = "bbb";

            var option = new DiffOption<char>
            {
                Order = DiffOrder.GreedyDeleteFirst,
            };

            var results = NetDiff.Diff(str1, str2, option);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(5).Status);
        }

        [TestMethod]
        public void TestComparer()
        {
            var str1 = "abc";
            var str2 = "dBf";

            var option = DiffOption<char>.Default;
            option.EqualityComparer = new CaseInsensitiveComparer();

            var results = NetDiff.Diff(str1, str2, option);

            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Deleted, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Inserted, results.ElementAt(4).Status);
        }

        [TestMethod]
        public void TestPerformance()
        {
            var str1 = Enumerable.Repeat("Good dog", 1000).SelectMany(c => c);
            var str2 = Enumerable.Repeat("Bad dog", 1000).SelectMany(c => c);

            var option = DiffOption<char>.Default;
            option.Limit = 100;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var result1 = NetDiff.Diff(str1, str2);
            sw.Stop();
            var time1 = sw.Elapsed;

            sw.Restart();
            var result2 = NetDiff.Diff(str1, str2, option);
            sw.Stop();
            var time2 = sw.Elapsed;

            Assert.IsTrue(time2 < time1);
        }

        internal class CaseInsensitiveComparer : IEqualityComparer<char>
        {
            public bool Equals(char x, char y)
            {
                return x.ToString().ToLower().Equals(y.ToString().ToLower());
            }

            public int GetHashCode(char obj)
            {
                return obj.ToString().ToLower().GetHashCode();
            }
        }
    }
}
