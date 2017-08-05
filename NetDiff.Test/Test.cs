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
        public void Test1()
        {
            var str1 = "abcde";
            var str2 = "abcde";

            var results = NetDiff.Diff(str1, str2);

            Assert.AreEqual(str1.Count(), results.Count());
            Assert.IsTrue(results.All(r => r.Status == DiffStatus.Equal));
            Assert.IsTrue(results.Select(r => r.Obj1).SequenceEqual(str1.Select(c => c)));
            Assert.IsTrue(results.Select(r => r.Obj2).SequenceEqual(str1.Select(c => c)));

            Output(results);
        }

        [TestMethod]
        public void Test2()
        {
            var str1 = "abccdefg";
            var str2 = "abzcdefg";

            var results = NetDiff.Diff(str1, str2);

            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(2).Status);
            Assert.AreEqual('c', results.ElementAt(2).Obj1);
            Assert.AreEqual(DiffStatus.Added, results.ElementAt(3).Status);
            Assert.AreEqual('z', results.ElementAt(3).Obj2);

            Output(results);
        }

        [TestMethod]
        public void Test3()
        {
            var str1 = "abcdef";
            var str2 = "abc";

            var results = NetDiff.Diff(str1, str2);

            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(5).Status);

            Output(results);
        }

        [TestMethod]
        public void Test4()
        {
            var str1 = "abcedfg";
            var str2 = "gfdecba";

            var results = NetDiff.Diff(str1, str2);

            Output(results);
        }

        [TestMethod]
        public void Test5()
        {
            var str1 = "cccccc";
            var str2 = "abcdef";

            var results = NetDiff.Diff(str1, str2);

            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(0).Status);
            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(1).Status);
            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(2).Status);
            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(3).Status);
            Assert.AreEqual(DiffStatus.Removed, results.ElementAt(4).Status);
            Assert.AreEqual(DiffStatus.Added, results.ElementAt(5).Status);
            Assert.AreEqual(DiffStatus.Added, results.ElementAt(6).Status);
            Assert.AreEqual(DiffStatus.Equal, results.ElementAt(7).Status);
            Assert.AreEqual(DiffStatus.Added, results.ElementAt(8).Status);
            Assert.AreEqual(DiffStatus.Added, results.ElementAt(9).Status);
            Assert.AreEqual(DiffStatus.Added, results.ElementAt(10).Status);

            Output(results);
        }

        private static void Output<T>(IEnumerable<DiffResult<T>> results)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var result in results)
            {
                var obj = result.Status == DiffStatus.Removed ? result.Obj1 : result.Obj2;
                sb.AppendLine($"{result.Status.GetStatusChar()} {obj}");
            }

            Console.Write(sb.ToString());
        }
    }
}
