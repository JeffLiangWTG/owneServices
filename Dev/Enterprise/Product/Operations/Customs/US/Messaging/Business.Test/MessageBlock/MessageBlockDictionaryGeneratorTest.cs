using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageBlockDictionaryGeneratorTest : TestCase
	{
		delegate void MyDelegate(Exception ex);
		class MessageBlockDictionaryGeneratorWithCaughtException : MessageBlockDictionaryGenerator
		{
			public MessageBlockDictionaryGeneratorWithCaughtException(MyDelegate d)
			{
				this.d = d;
			}

			readonly MyDelegate d;
			protected override void AddType(MessageBlockDictionary dictionary, MessageBlockTypeProvider typeProvider)
			{
				try
				{
					base.AddType(dictionary, typeProvider);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					d.Invoke(ex);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestReportAllInputDuplicates()
		{
			StringBuilder errors = new StringBuilder();
			new MessageBlockDictionaryGeneratorWithCaughtException((Exception ex) => errors.AppendLine(ex.Message)).GenerateBlockDictionaries(typeof(InputBlockAttribute));
			if (errors.Length > 0)
			{
				Fail(errors.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestReportAllOutputDuplicates()
		{
			StringBuilder errors = new StringBuilder();
			new MessageBlockDictionaryGeneratorWithCaughtException((Exception ex) => errors.AppendLine(ex.Message)).GenerateBlockDictionaries(typeof(OutputBlockAttribute));
			if (errors.Length > 0)
			{
				Fail(errors.ToString());
			}
		}

		public void TestGenerateBlockDictionariesInOrderOfSizeDescending()
		{
			var generator = new MessageBlockDictionaryGenerator();
			var messageBlockDictionaries = generator.GenerateBlockDictionaries(typeof(OutputBlockAttribute));
			int maxLength = 999;
			foreach (MessageBlockDictionary dictionary in messageBlockDictionaries)
			{
				Assert(dictionary.MandatoryCharactersLength < maxLength);
				maxLength = dictionary.MandatoryCharactersLength;
			}
		}

		public void TestMultipleDictionaries()
		{
			var generator = new MessageBlockDictionaryGeneratorOverloaded();
			var messageBlockDictionaries = generator.GenerateBlockDictionaries(null);
			AssertEquals(2, messageBlockDictionaries.Count);
			Type messageBlockType;
			AssertEquals(3, messageBlockDictionaries[0].MandatoryCharactersLength);
			AssertEquals(true, messageBlockDictionaries[0].TryGetValue("YYY", "  ", "ABC", "", out messageBlockType));
			AssertEquals(typeof(bool), messageBlockType);
			AssertEquals(true, messageBlockDictionaries[0].TryGetValue("YYY", "  ", "ABC", "01", out messageBlockType));
			AssertEquals(typeof(string), messageBlockType);
			AssertEquals(2, messageBlockDictionaries[1].MandatoryCharactersLength);
			AssertEquals(true, messageBlockDictionaries[1].TryGetValue("YYY", "XX", "AB", "", out messageBlockType));
			AssertEquals(typeof(int), messageBlockType);
			AssertEquals(true, messageBlockDictionaries[1].TryGetValue("YYY", "XX", "AB", "01", out messageBlockType));
			AssertEquals(typeof(decimal), messageBlockType);
		}

		[ExpectExceptionMessage(typeof(Exception), @"Duplicate types found
Place an ApplicationIdentifier attribute on BOTH types
Type 1 : System.Int32
Type 2 : System.Boolean")]
		public void TestDuplicateKeysRaisesError()
		{
			new MessageBlockDictionaryGeneratorError().GenerateBlockDictionaries(typeof(OutputBlockAttribute));
		}

		sealed class MessageBlockDictionaryGeneratorError : MessageBlockDictionaryGenerator
		{
			protected override IEnumerable<MessageBlockTypeProvider> GetMessageBlockTypes(Type attributeType)
			{
				yield return new MessageBlockTypeProvider(typeof(int), "YYY", "XX", "AB", "");
				yield return new MessageBlockTypeProvider(typeof(bool), "YYY", "XX", "AB", "");
				yield return new MessageBlockTypeProvider(typeof(decimal), "YYY", "XX", "AB", "01");
				yield return new MessageBlockTypeProvider(typeof(string), "YYY", "XX", "AB", "01");
			}
		}

		sealed class MessageBlockDictionaryGeneratorOverloaded : MessageBlockDictionaryGenerator
		{
			protected override IEnumerable<MessageBlockTypeProvider> GetMessageBlockTypes(Type attributeType)
			{
				yield return new MessageBlockTypeProvider(typeof(int), "YYY", "XX", "AB", "");
				yield return new MessageBlockTypeProvider(typeof(bool), "YYY", "  ", "ABC", "");
				yield return new MessageBlockTypeProvider(typeof(decimal), "YYY", "XX", "AB", "01");
				yield return new MessageBlockTypeProvider(typeof(string), "YYY", "  ", "ABC", "01");
			}
		}
	}
}
