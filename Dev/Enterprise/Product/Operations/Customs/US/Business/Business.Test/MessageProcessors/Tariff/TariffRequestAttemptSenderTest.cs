using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class TariffRequestAttemptSenderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			TariffRequestAttemptSender sender = new TariffRequestAttemptSender(901, Factory);
			AssertEquals(true, sender.CurrentYearNotEqualAttemptYear);
			AssertEquals(902, sender.NextAttempt);
			AssertEquals(1001, sender.NextAttemptForYearCutover);
		}

		[TestDate(2014, 9, 10, 15, 35, 45)]
		public void TestSendMessageIfAllowed()
		{
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles);
			AssertEquals("Precodition: no FI messages in EDIMessage table", 0, Factory.Load<EDIMessage>(query).Length);

			USCDataVersion lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			TariffRequestAttemptSender sender = new TariffRequestAttemptSender(lastHTSAttemptObj.UZ_Version, Factory);
			sender.SendMessageIfAllowed(true, 9806, lastHTSAttemptObj);

			AssertEquals(ExtractReferenceFilesResult.Pending + string.Format(",{0}{1}", USCDataVersion.Constant.DatabaseNameIdentifier, Db.DatabaseName), lastHTSAttemptObj.UZ_Note);
			AssertEquals(9806, lastHTSAttemptObj.UZ_Version);
			AssertEquals("FI messages should be sent", 1, Factory.Load<EDIMessage>(query).Length);

			var dateTime = ZDateTime.UtcToday;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("HAVI", "TEST HAVI", Core.Constants.CountryCodes.UnitedStates);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType.ZZK_CodeType, "9807", codeType.ZZK_Description, dateTime.AddDays(-10), dateTime.AddDays(10));
			Factory.Save();

			lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
			sender = new TariffRequestAttemptSender(lastHTSAttemptObj.UZ_Version, Factory);
			sender.SendMessageIfAllowed(true, 9807, lastHTSAttemptObj);

			AssertEquals(ExtractReferenceFilesResult.Pending + string.Format(",{0}{1}", USCDataVersion.Constant.DatabaseNameIdentifier, Db.DatabaseName), lastHTSAttemptObj.UZ_Note);
			AssertEquals(9808, lastHTSAttemptObj.UZ_Version);
			AssertEquals("FI messages should be sent", 2, Factory.Load<EDIMessage>(query).Length);
		}
	}
}
