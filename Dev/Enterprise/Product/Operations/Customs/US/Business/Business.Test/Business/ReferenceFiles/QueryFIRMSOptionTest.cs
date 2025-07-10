using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QueryFIRMSOption))]
	sealed class QueryFIRMSOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendWithFacilityName()
		{
			QueryFIRMSOption.US_FacilityName = "F@34";

			ZString nextNum = "EDIEDIDAT_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).PeekPreliminaryFormatted(Factory);
			QueryFIRMSOption.SendQuery();

			var message = new Messaging.Business.CBPEDIMessage.Loader(Factory).LoadTop1(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.ExtractReference, nextNum);

			AssertEquals("B         FQ                                               EDIEDIDAT_1          F111    F@34                                                                    Y         FQ", message.EM_MessageText);
		}

		public void TestFIRMSCodeValidation()
		{
			QueryFIRMSOption.ValidateUS_FIRMSCode();
			AssertEquals(false, QueryFIRMSOption.US_FIRMSCodeInfo.HasNotifications());

			QueryFIRMSOption.US_FIRMSCode = "A123";
			AssertEquals(false, QueryFIRMSOption.US_FIRMSCodeInfo.HasNotifications());

			QueryFIRMSOption.US_FIRMSCode = "234";
			AssertEquals(true, QueryFIRMSOption.US_FIRMSCodeInfo.HasNotifications());

			QueryFIRMSOption.US_FIRMSCode = "1234";
			AssertEquals(false, QueryFIRMSOption.US_FIRMSCodeInfo.HasNotifications());

			QueryFIRMSOption.US_FIRMSCode = "1@34";
			AssertEquals(true, QueryFIRMSOption.US_FIRMSCodeInfo.HasNotifications());
		}

		public void TestFacilityNameValidation()
		{
			QueryFIRMSOption.ValidateUS_FacilityName();
			AssertEquals(false, QueryFIRMSOption.US_FacilityNameInfo.HasNotifications());

			QueryFIRMSOption.US_FIRMSCode = "1234";
			QueryFIRMSOption.ValidateUS_FacilityName();
			AssertEquals(false, QueryFIRMSOption.US_FacilityNameInfo.HasNotifications());

			QueryFIRMSOption.US_FacilityName = "TEST";
			AssertEquals(true, QueryFIRMSOption.US_FacilityNameInfo.HasNotifications());

			QueryFIRMSOption.US_FIRMSCode = "";
			QueryFIRMSOption.ValidateUS_FacilityName();
			AssertEquals(false, QueryFIRMSOption.US_FacilityNameInfo.HasNotifications());

			QueryFIRMSOption.US_FacilityName = "T@ST";
			AssertEquals(false, QueryFIRMSOption.US_FacilityNameInfo.HasNotifications());
		}

		public void TestDistrictValidation()
		{
			QueryFIRMSOption.ValidateUS_DistrictCode();
			AssertEquals(false, QueryFIRMSOption.US_DistrictCodeInfo.HasNotifications());

			QueryFIRMSOption.US_FacilityName = "TEST";
			QueryFIRMSOption.ValidateUS_DistrictCode();
			AssertEquals(true, QueryFIRMSOption.US_DistrictCodeInfo.HasNotifications());

			QueryFIRMSOption.US_DistrictCode = "08";
			AssertEquals(false, QueryFIRMSOption.US_DistrictCodeInfo.HasNotifications());

			QueryFIRMSOption.US_DistrictCode = "A";
			AssertEquals(true, QueryFIRMSOption.US_DistrictCodeInfo.HasNotifications());

			QueryFIRMSOption.US_FacilityName = "";
			QueryFIRMSOption.US_DistrictCode = "1";
			AssertEquals(true, QueryFIRMSOption.US_DistrictCodeInfo.HasNotifications());

			QueryFIRMSOption.US_DistrictCode = "1A";
			AssertEquals(true, QueryFIRMSOption.US_DistrictCodeInfo.HasNotifications());

			QueryFIRMSOption.US_DistrictCode = "11";
			AssertEquals(false, QueryFIRMSOption.US_DistrictCodeInfo.HasNotifications());
		}

		public void TestSend()
		{
			QueryFIRMSOption.US_FIRMSCode = "1234";

			ZString nextNum = "EDIEDIDAT_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).PeekPreliminaryFormatted(Factory);
			QueryFIRMSOption.SendQuery();

			var message = new Messaging.Business.CBPEDIMessage.Loader(Factory).LoadTop1(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.ExtractReference, nextNum);

			AssertEquals("B         FQ                                               EDIEDIDAT_1          F1111234                                                                        Y         FQ", message.EM_MessageText);
		}

		QueryFIRMSOption queryFIRMSOption;
		QueryFIRMSOption QueryFIRMSOption => queryFIRMSOption ?? (queryFIRMSOption = new QueryFIRMSOption(Factory));
	}
}
