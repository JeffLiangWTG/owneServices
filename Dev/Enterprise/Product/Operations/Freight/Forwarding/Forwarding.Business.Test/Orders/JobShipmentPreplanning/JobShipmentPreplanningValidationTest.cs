using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobShipmentPreplanningValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSendingReceivingAgentCompareValidation()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();

			preAdvice.EF_OH_SendingAgent = org1.PK;
			preAdvice.EF_OH_ReceivingAgent = org2.PK;
			AssertNoErrors(preAdvice.EF_OH_SendingAgentInfo);
			AssertNoErrors(preAdvice.EF_OH_ReceivingAgentInfo);

			preAdvice.EF_OH_ReceivingAgent = org1.PK;
			AssertNoErrors(preAdvice.EF_OH_SendingAgentInfo);
			AssertHasErrors(preAdvice.EF_OH_ReceivingAgentInfo);

			preAdvice.EF_OH_ReceivingAgent = org2.PK;
			preAdvice.EF_OH_SendingAgent = org2.PK;
			preAdvice.RunPreSaveValidation();
			AssertHasErrors(preAdvice.EF_OH_SendingAgentInfo);
			AssertHasErrors(preAdvice.EF_OH_ReceivingAgentInfo);

			preAdvice.EF_OH_SendingAgent = org1.PK;
			preAdvice.RunPreSaveValidation();
			AssertNoErrors(preAdvice.EF_OH_SendingAgentInfo);
			AssertNoErrors(preAdvice.EF_OH_ReceivingAgentInfo);
		}

		public void TestSendingReceivingAgentCompareValidationWithNoEntry()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.RunPreSaveValidation();
			AssertNoErrors(preAdvice.EF_OH_SendingAgentInfo);
			AssertNoErrors(preAdvice.EF_OH_ReceivingAgentInfo);
		}

		public void TestBuyerAddressMandatory()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();

			preAdvice.RunPreSaveValidation();
			AssertHasErrors(preAdvice.EF_OA_BuyerAddressInfo);

			preAdvice.EF_OA_BuyerAddress = Factory.New<OrgAddress>().PK;
			AssertNoErrors(preAdvice.EF_OA_BuyerAddressInfo);
		}

		public void TestCheckEF_MasterBillAirConsolAlreadyExists()
		{
			ForwardingConsol cosnol = Factory.New<ForwardingConsol>();
			cosnol.JK_TransportMode = Constants.TransportModes.Air;
			cosnol.JK_MasterBillNum = "mybil";

			ForwardingConsol cosnol2 = Factory.New<ForwardingConsol>();
			cosnol2.JK_TransportMode = Constants.TransportModes.Air;
			cosnol2.JK_MasterBillNum = "";

			Factory.Save();

			JobShipmentPreplanning preplan = Factory.New<JobShipmentPreplanning>();
			preplan.EF_MasterBill = "mybil";
			AssertHasErrors(preplan.EF_MasterBillInfo);

			preplan.EF_MasterBill = "mybil2";
			AssertNoErrors(preplan.EF_MasterBillInfo);

			preplan.EF_MasterBill = "";
			AssertNoErrors(preplan.EF_MasterBillInfo);
		}
	}
}
