using CargoWise.eHub.Products.GBCustoms.Core.Correlation;

using NUnit.Framework;

using System;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.CorrelationTests
{
    public static class CorrelationTestHelper
    {
        public static void AssertGBCustomsCorrelationIdentifiers(List<GBCustomsCorrelationIdentifier> expected, List<GBCustomsCorrelationIdentifier> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, $"Expected {expected.Count} items, Actual {actual.Count} items");

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Name, actual[i].Name);
                Assert.AreEqual(expected[i].Type, actual[i].Type);
                Assert.AreEqual(expected[i].Value, actual[i].Value);
            }
        }

        public static void AssertException<T>(Action action, string message = null) where T : Exception
        {
            try
            {
                action();
                Assert.Fail("Expected to throw an exception.");
            }
            catch (T e)
            {
                if (message != null) Assert.AreEqual(message, e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("Expected {0} but was {1}", typeof(T).ToString(), e.GetType().ToString()));
            }

        }

    }
}
