using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.eHub.Core.Tests.TransformHelpers
{
    [TestClass]
    public class CultureOverrideTests
    {
        CultureInfo preTestCulture;

        [TestInitialize]
        public void TestSetup()
        {
            preTestCulture = Thread.CurrentThread.CurrentCulture;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Thread.CurrentThread.CurrentCulture = preTestCulture;
        }

        [TestMethod]
        public void CurrentThreadCultureIsOverriddenThenRestored()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            Assert.AreEqual("en-GB", Thread.CurrentThread.CurrentCulture.Name);

            using (new CultureOverride())
            {
                Assert.AreEqual("en-AU", Thread.CurrentThread.CurrentCulture.Name);
            }

            Assert.AreEqual("en-GB", Thread.CurrentThread.CurrentCulture.Name);
        }

        [TestMethod]
        public void GivenFailure_CurrentThreadCultureIsOverriddenThenRestored()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            Assert.AreEqual("en-GB", Thread.CurrentThread.CurrentCulture.Name);

            try
            {
                using (new CultureOverride())
                {
                    Assert.AreEqual("en-AU", Thread.CurrentThread.CurrentCulture.Name);

                    throw new NotSupportedException();
                }
            }
            catch (NotSupportedException)
            {
                // Ignore the expected excpetion thrown by the test
            }

            Assert.AreEqual("en-GB", Thread.CurrentThread.CurrentCulture.Name);
        }
    }
}
