using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business.Testing
{
	class GenericMessageDeliveryInterchangeCreatorTest : TestCaseWithFactory
	{
		public void TestSupportedInterchangeTypes()
		{
			var interchangeCreator = new GenericMessageDeliveryInterchangeCreator();
			AssertContainsExactElementsInExactOrder(new[] { GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse }, interchangeCreator.SupportedInterchangeTypes);
		}

		public void TestCreateSupportedInterchange()
		{
			var branch = Factory.New<GlbBranch>();
			var interchangeCreator = new GenericMessageDeliveryInterchangeCreator();
			var result = interchangeCreator.Create(branch.PK, "EASYLOG2TEST_EAD", "HYEDNACMT", GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse, "<Message>TEST</Message>");

			AssertEquals(ApplicationCodeList.Codes.GenericMessageDelivery, result.EI_ApplicationCode);
			AssertEquals(branch.PK, result.EI_GB);
			AssertEquals(GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse, result.EI_InterchangeType);
			AssertEquals(EDIInterchange.Direction.Receive, result.EI_ReceiveTransmit);
			AssertEquals("EASYLOG2TEST_EAD", result.EI_From);
			AssertEquals("HYEDNACMT", result.EI_To);
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, result.EI_Status);
			AssertEquals("<Message>TEST</Message>", result.EI_BodyText);
		}

		public void TestCreateUnsupportedInterchange()
		{
			var branch = Factory.New<GlbBranch>();
			var interchangeCreator = new GenericMessageDeliveryInterchangeCreator();
			interchangeCreator.Create(branch.PK, "EASYLOG2TEST_EAD", "HYEDNACMT", EDIInterchangeTypeList.Codes.TST, "<Message>TEST</Message>");

			var expectedError = $"Unknown Interchange Type '{EDIInterchangeTypeList.Codes.TST}'; " +
				"receive GMD Interchange must be in the supported list 'Enterprise.Messaging.Integration.GenericMessageDeliveryInterchangeTypeList'";
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
