using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(DeliveryAgentToSelectFromForPrinting))]
	sealed class DeliveryAgentToSelectFromForPrintingTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			DeliveryAgentToSelectFromForPrinting deliveryAgent = factory.New<DeliveryAgentToSelectFromForPrinting>();
			deliveryAgent.OH_FullName = "Full Name";
			deliveryAgent.OH_RL_NKClosestPort = "AUSYD";
			deliveryAgent.MainAddress.OA_Address1 = "Address1";
			deliveryAgent.MainAddress.OA_City = "City";

			return deliveryAgent;
		}

		public void TestOH_Calc_PrintDocumentForDeliveryAgent()
		{
			DeliveryAgentToSelectFromForPrinting deliveryAgent = Factory.New<DeliveryAgentToSelectFromForPrinting>();

			AssertEquals("Default value for print document for Delivery Agent", true, deliveryAgent.OH_Calc_PrintDocumentForDeliveryAgent);
			deliveryAgent.OH_Calc_PrintDocumentForDeliveryAgent = ZBool.False;
			AssertEquals("Value for print document for Delivery Agent", false, deliveryAgent.OH_Calc_PrintDocumentForDeliveryAgent);
		}

		public void TestSetFieldsReadonly()
		{
			DeliveryAgentToSelectFromForPrinting deliveryAgent = Factory.New<DeliveryAgentToSelectFromForPrinting>();
			AssertEquals("Code is read only for Delivery Agent", true, deliveryAgent.OH_CodeInfo.ReadOnly);
			AssertEquals("Full name is read only for Delivery Agent", true, deliveryAgent.OH_FullNameInfo.ReadOnly);
		}

		public void TestSetDefaultValueForPrintDocumentFlag()
		{
			DeliveryAgentToSelectFromForPrintingTestClass deliveryAgentTest = Factory.New<DeliveryAgentToSelectFromForPrintingTestClass>();
			deliveryAgentTest.SetDefaultValueForPrintDocumentFlagMethod();
			AssertEquals("Default value for print document for Delivery Agent", true, deliveryAgentTest.OH_Calc_PrintDocumentForDeliveryAgent);
		}
	}
}
