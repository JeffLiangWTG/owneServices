using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class MessageSubTypeCodesTest : TestCaseWithFactory
	{
		public void TestTranslateToMessageSubType()
		{
			AssertEquals(MessageSubTypes.Create, MessageSubTypeCodes.TranslateToMessageSubType(MessageSubTypeCodes.Codes.Original));
			AssertEquals(MessageSubTypes.Change, MessageSubTypeCodes.TranslateToMessageSubType(MessageSubTypeCodes.Codes.Change));
			AssertEquals(MessageSubTypes.Withdraw, MessageSubTypeCodes.TranslateToMessageSubType(MessageSubTypeCodes.Codes.Cancellation));
			AssertEquals(MessageSubTypes.Replace, MessageSubTypeCodes.TranslateToMessageSubType(MessageSubTypeCodes.Codes.Replace));
			AssertEquals(MessageSubTypes.Undefined, MessageSubTypeCodes.TranslateToMessageSubType("XXX"));
			AssertEquals(MessageSubTypes.Undefined, MessageSubTypeCodes.TranslateToMessageSubType(""));
			AssertEquals(MessageSubTypes.Undefined, MessageSubTypeCodes.TranslateToMessageSubType(null));
		}
	}
}
