using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeTypeOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceTypeList()
		{
			AccChargeTypeOverride chargeTypeOverride = Factory.New<AccChargeTypeOverride>();

			chargeTypeOverride.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertContains(InvoiceTypesList.Codes.FinalInvoice, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);
			AssertNotContains(AgencyInvoiceTypesList.Codes.ForeignCollect, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);

			chargeTypeOverride.AN_JobType = JobInvoicingConsumerTypes.AgencyBooking.Code;
			AssertNotContains(InvoiceTypesList.Codes.FinalInvoice, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);
			AssertContains(AgencyInvoiceTypesList.Codes.ForeignCollect, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);

			chargeTypeOverride.AN_JobType = JobInvoicingConsumerTypes.AgencyBillOfLading.Code;
			AssertNotContains(InvoiceTypesList.Codes.FinalInvoice, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);
			AssertContains(AgencyInvoiceTypesList.Codes.ForeignCollect, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);

			chargeTypeOverride.AN_JobType = "XXX";
			AssertEquals(1, chargeTypeOverride.Lookups.InvoiceTypes.Count);

			chargeTypeOverride.AN_JobType = "";
			AssertEquals(1, chargeTypeOverride.Lookups.InvoiceTypes.Count);

			chargeTypeOverride.AN_JobType = "ALL";
			AssertContains(InvoiceTypesList.Codes.FinalInvoice, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);
			AssertNotContains(AgencyInvoiceTypesList.Codes.ForeignCollect, chargeTypeOverride.Lookups.InvoiceTypes.CodesAsString);
		}

		public void TestChargeTypeList()
		{
			AccChargeTypeOverride chargeTypeOverride = Factory.New<AccChargeTypeOverride>();
			AccChargeTypeOverrideLookups lookups = new AccChargeTypeOverrideLookups(chargeTypeOverride);
			Assert("Should not contain charge type NonAccrual", !lookups.AC_ChargeType_List.ContainsCode(Core.Constants.ChargeType.NonAccrual));
			Assert("Should not contain charge type Overhead", !lookups.AC_ChargeType_List.ContainsCode(Core.Constants.ChargeType.Overhead));
			Assert("Should not contain charge type Comment", !lookups.AC_ChargeType_List.ContainsCode(Core.Constants.ChargeType.Comment));
		}

		public void TestJobTypeList()
		{
			AccChargeTypeOverride chargeTypeOverride = Factory.New<AccChargeTypeOverride>();
			AccChargeTypeOverrideLookups lookups = new AccChargeTypeOverrideLookups(chargeTypeOverride);

			AssertEquals("The JobTypeList should contain 'SHP'", true, lookups.JobTypes.ContainsCode("SHP"));
			AssertEquals("The JobTypeList should contain 'CLL'", true, lookups.JobTypes.ContainsCode("CLL"));
			AssertEquals("The JobTypeList should contain 'CSH'", true, lookups.JobTypes.ContainsCode("CSH"));
			AssertEquals("The JobTypeList should contain 'BRK'", true, lookups.JobTypes.ContainsCode("BRK"));
			AssertEquals("The JobTypeList should contain 'ALL'", true, lookups.JobTypes.ContainsCode("ALL"));
			Assert("The JobType 'ALL' should be JobInvoicingConsumerType", lookups.JobTypes["ALL"] is JobInvoicingConsumerType);
		}
	}
}
