using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QueryCarrierOption))]
	sealed class QueryCarrierOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCarrierNameInvalidCharacters()
		{
			QueryCarrierOption.US_CarrierName = "A  2";
			AssertEquals(false, QueryCarrierOption.US_CarrierNameInfo.HasNotifications());

			QueryCarrierOption.US_CarrierName = "A　S　　D　W";
			AssertEquals(false, QueryCarrierOption.US_CarrierNameInfo.HasNotifications());

			QueryCarrierOption.US_CarrierName = "A@ 2";
			AssertEquals(true, QueryCarrierOption.US_CarrierNameInfo.HasNotifications());
		}

		public void TestFullWideCharactersAsInputValue()
		{
			QueryCarrierOption.US_CarrierCode = "A　　D";
			QueryCarrierOption.US_CarrierName = "CA　RRIER NAME";

			AssertEquals("US_CarrierCode", "A  D", QueryCarrierOption.US_CarrierCode);
			AssertEquals("US_CarrierName", "CA RRIER NAME", QueryCarrierOption.US_CarrierName);

			var nextNum = "EDIEDIDAT_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).PeekPreliminaryFormatted(Factory);
			QueryCarrierOption.SendQuery();
			var message = new Messaging.Business.CBPEDIMessage.Loader(Factory).LoadTop1(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.ExtractReference, nextNum);
			AssertEquals("B         FQ                                               EDIEDIDAT_1          F106CA RRIER NAME                      A  D                                     Y         FQ"
				, message.EM_MessageText);
		}

		public void TestReplaceInvalidCharacters()
		{
			QueryCarrierOption.US_CarrierCode = "A@ 4";
			QueryCarrierOption.US_CarrierName = "CARRIER#123 NAME";

			var nextNum = "EDIEDIDAT_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).PeekPreliminaryFormatted(Factory);
			QueryCarrierOption.SendQuery();
			var message = new Messaging.Business.CBPEDIMessage.Loader(Factory).LoadTop1(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.ExtractReference, nextNum);
			Assert(message.EM_MessageText.StartsWith("B  "));//ACE B block has space at position 2 and 3
			AssertEquals("B         FQ                                               EDIEDIDAT_1          F106CARRIER 123 NAME                   A  4                                     Y         FQ"
				, message.EM_MessageText);
		}

		public void TestCarrierCodeValidation()
		{
			QueryCarrierOption.ValidateUS_CarrierCode();
			AssertEquals(false, QueryCarrierOption.US_CarrierCodeInfo.HasNotifications());

			QueryCarrierOption.US_CarrierCode = "A　S　";
			AssertEquals(false, QueryCarrierOption.US_CarrierCodeInfo.HasNotifications());

			QueryCarrierOption.US_CarrierCode = "A123";
			AssertEquals(false, QueryCarrierOption.US_CarrierCodeInfo.HasNotifications());

			QueryCarrierOption.US_CarrierCode = "2A";
			AssertEquals(false, QueryCarrierOption.US_CarrierCodeInfo.HasNotifications());

			QueryCarrierOption.US_CarrierCode = "1";
			AssertEquals(true, QueryCarrierOption.US_CarrierCodeInfo.HasNotifications());

			QueryCarrierOption.US_CarrierCode = "A  2";
			AssertEquals(false, QueryCarrierOption.US_CarrierCodeInfo.HasNotifications());

			QueryCarrierOption.US_CarrierCode = "A@ 2";
			AssertEquals(true, QueryCarrierOption.US_CarrierCodeInfo.HasNotifications());
			AssertHasError("ShouldBeLettersAndNumbersSpaceOnly", QueryCarrierOption.US_CarrierCodeInfo, StringChecker.ShouldBeLettersAndNumbersSpaceOnly);
		}

		public void TestSend()
		{
			QueryCarrierOption.US_CarrierCode = "1234";
			QueryCarrierOption.US_CarrierName = "CARRIER NAME";

			var nextNum = "EDIEDIDAT_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).PeekPreliminaryFormatted(Factory);
			QueryCarrierOption.SendQuery();
			var message = new Messaging.Business.CBPEDIMessage.Loader(Factory).LoadTop1(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.ExtractReference, nextNum);
			Assert(message.EM_MessageText.StartsWith("B  "));//ACE B block has space at position 2 and 3
		}

		QueryCarrierOption queryCarrierOption;
		QueryCarrierOption QueryCarrierOption => queryCarrierOption ?? (queryCarrierOption = new QueryCarrierOption(Factory));
	}
}
