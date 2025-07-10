using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	sealed class EDIMessagePackerUtilsTest : TestCaseWithFactory
	{
		public void TestPack_Basic()
		{
			var branchPK = ZGuid.NewZGuid();
			var externalPassword = ZGuid.NewZGuid();
			var sessionGuid = ZGuid.NewZGuid();
			var appCode = "APP";
			var interchangeType = "TYP";
			var from = "From";
			var to = "To";

			var interchange = Factory.New<EDIInterchange>();

			EDIMessagePackerUtils.PopulateInterchange(interchange, appCode, interchangeType, from, to, branchPK, externalPassword, sessionGuid);

			CombineAssertions(() =>
			{
				AssertEquals("EI_ApplicationCode", appCode, interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", interchangeType, interchange.EI_InterchangeType);
				AssertEquals("EI_From", from, interchange.EI_From);
				AssertEquals("EI_To", to, interchange.EI_To);
				AssertEquals("EI_GB", branchPK, interchange.EI_GB);
				AssertEquals("EI_GP", externalPassword, interchange.EI_GP);
				AssertEquals("EI_SessionGUID", sessionGuid, interchange.EI_SessionGUID);
				AssertEquals("EI_Status", EDIMessageStatusList.Codes.Queued, interchange.EI_Status);
				AssertEquals("EI_IsActive", true, interchange.EI_IsActive);
				AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
			});
		}

		public void TestPack_WithOptional()
		{
			var branchPK = ZGuid.NewZGuid();
			var externalPassword = ZGuid.NewZGuid();
			var sessionGuid = ZGuid.NewZGuid();
			var appCode = "APP";
			var interchangeType = "TYP";
			var from = "From";
			var to = "To";

			var interchange = Factory.New<EDIInterchange>();

			EDIMessagePackerUtils.PopulateInterchange(interchange, appCode, interchangeType, from, to, branchPK, externalPassword, sessionGuid,  EDIInterchangeStatusList.Codes.Cancelled, false, ReceiveTransmitList.Codes.Receive, EDIInterchangeTransportTypeList.Codes.eHub);

			CombineAssertions(() =>
			{
				AssertEquals("EI_ApplicationCode", appCode, interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", interchangeType, interchange.EI_InterchangeType);
				AssertEquals("EI_From", from, interchange.EI_From);
				AssertEquals("EI_To", to, interchange.EI_To);
				AssertEquals("EI_GB", branchPK, interchange.EI_GB);
				AssertEquals("EI_GP", externalPassword, interchange.EI_GP);
				AssertEquals("EI_SessionGUID", sessionGuid, interchange.EI_SessionGUID);
				AssertEquals("EI_Status", EDIMessageStatusList.Codes.Cancelled, interchange.EI_Status);
				AssertEquals("EI_IsActive", false, interchange.EI_IsActive);
				AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			});
		}
	}
}
