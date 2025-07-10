using System;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageBlockDictionaryTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals(10, new MessageBlockDictionary(10).MandatoryCharactersLength);
			AssertEquals(8, new MessageBlockDictionary(8).MandatoryCharactersLength);
		}

		public void TestHasElements()
		{
			MessageBlockDictionary dictionary = new MessageBlockDictionary(3);
			AssertEquals(false, dictionary.HasElements);
			dictionary.AddType(new MessageBlockTypeProvider(typeof(string), "YYY", "AA", "BBB", ""));
			AssertEquals(true, dictionary.HasElements);
		}

		public void TestTryGetValue()
		{
			MessageBlockDictionary dictionary = new MessageBlockDictionary(3);
			Type messageBlockType;
			AssertEquals(false, dictionary.TryGetValue("YYY", "AA", "ABC", "", out messageBlockType));
			dictionary.AddType(new MessageBlockTypeProvider(typeof(string), "YYY", "AA", "ABC", ""));
			AssertEquals(true, dictionary.TryGetValue("YYY", "AA", "ABC", "", out messageBlockType));
			AssertEquals(typeof(string), messageBlockType);
			AssertExceptionThrown(typeof(ArgumentException), () => dictionary.TryGetValue("YYY", "", "123", "", out messageBlockType));
		}
	}
}
