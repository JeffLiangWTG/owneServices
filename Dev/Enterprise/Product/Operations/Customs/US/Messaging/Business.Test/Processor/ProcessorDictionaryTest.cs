using System;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class ProcessorDictionaryTest : TestCase
	{
		public void TestAddAndGetFromDictionaryWithAppCode()
		{
			ProcessorDictionary dictionary = new ProcessorDictionary();
			dictionary.Add(CBPEDIInterchange.ApplicationCodeForTesting, "AA", typeof(string));
			dictionary.Add(CBPEDIInterchange.ApplicationCodeForTesting, "BB", typeof(decimal));
			Type result;
			AssertEquals(true, dictionary.TryGetValue(CBPEDIInterchange.ApplicationCodeForTesting, "AA", out result));
			AssertEquals(typeof(string), result);
		}

		public void TestSameApplicationIdentifier()
		{
			ProcessorDictionary dictionary = new ProcessorDictionary();
			dictionary.Add(CBPEDIInterchange.ApplicationCodeForTesting, "AA", typeof(string), typeof(decimal));
			dictionary.Add(CBPEDIInterchange.ApplicationCodeForTesting, "AA", typeof(decimal), typeof(string));
			Type processorType;
			AssertEquals(true, dictionary.TryGetValue(CBPEDIInterchange.ApplicationCodeForTesting, "AA", typeof(string), out processorType));
			AssertEquals(typeof(decimal), processorType);
			AssertEquals(true, dictionary.TryGetValue(CBPEDIInterchange.ApplicationCodeForTesting, "AA", typeof(decimal), out processorType));
			AssertEquals(typeof(string), processorType);
		}

		public void TestDifferentApplicationIdentifierGivesDifferentType()
		{
			ProcessorDictionary dictionary = new ProcessorDictionary();
			dictionary.Add(CBPEDIInterchange.ApplicationCodeForTesting, "AA", typeof(string), typeof(string));
			dictionary.Add(CBPEDIInterchange.ApplicationCodeForTesting, "BB", typeof(string), typeof(decimal));
			Type processorType;
			AssertEquals(true, dictionary.TryGetValue(CBPEDIInterchange.ApplicationCodeForTesting, "AA", typeof(string), out processorType));
			AssertEquals(typeof(string), processorType);
			AssertEquals(true, dictionary.TryGetValue(CBPEDIInterchange.ApplicationCodeForTesting, "BB", typeof(string), out processorType));
			AssertEquals(typeof(decimal), processorType);
		}
	}
}
