using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbConsignmentFilterBusinessObject))]
	class DtbConsignmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestBookingPartyCompanyName()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = helper.CreateConsignment("CN1");
			var consignment2 = helper.CreateConsignment("CN2");

			var org1 = helper.CreateOrganisation("ORG1");
			var org2 = helper.CreateOrganisation("ORG2");
			org1.OH_FullName = "Vik's Variety LLC";
			org2.OH_FullName = "Adam's Accessories Inc.";

			var orgAddress1 = helper.CreateOrgAddress(org1, "2000");
			var orgAddress2 = helper.CreateOrgAddress(org2, "2000");

			var jobDocAddress1 = Factory.New<JobDocAddress>();
			jobDocAddress1.E2_ParentID = consignment1.PK;
			jobDocAddress1.E2_ParentTableCode = "LTC";
			jobDocAddress1.E2_OA_Address = orgAddress1.PK;
			jobDocAddress1.E2_AddressType = DocAddressTypes.Codes.LocalCartageExporter;

			var jobDocAddress2 = Factory.New<JobDocAddress>();
			jobDocAddress2.E2_ParentID = consignment2.PK;
			jobDocAddress2.E2_ParentTableCode = "LTC";
			jobDocAddress2.E2_OA_Address = orgAddress2.PK;
			jobDocAddress2.E2_AddressType = DocAddressTypes.Codes.BookingPartyDocumentaryAddress;

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("BookingPartyCompanyName", "Adam's Accessories Inc.");
			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query).Select(x => x.LTC_JobID);

			AssertContainsExactElementsInAnyOrder("Only the consignment with the matching Booking party name should be returned. " + query.LiteralTextADOFormatted,
				new[] { "CN2" }, results);
		}

		public void TestBillingPartyCompanyName()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = helper.CreateConsignment("CN1");
			var consignment2 = helper.CreateConsignment("CN2");

			var org1 = helper.CreateOrganisation("ORG1");
			var org2 = helper.CreateOrganisation("ORG2");
			org1.OH_FullName = "Vik's Variety LLC";
			org2.OH_FullName = "Adam's Accessories Inc.";

			var orgAddress1 = helper.CreateOrgAddress(org1, "2000");
			var orgAddress2 = helper.CreateOrgAddress(org2, "2000");

			var jobDocAddress1 = Factory.New<JobDocAddress>();
			jobDocAddress1.E2_ParentID = consignment1.PK;
			jobDocAddress1.E2_ParentTableCode = "LTC";
			jobDocAddress1.E2_OA_Address = orgAddress1.PK;
			jobDocAddress1.E2_AddressType = DocAddressTypes.Codes.LocalCartageExporter;

			var jobDocAddress2 = Factory.New<JobDocAddress>();
			jobDocAddress2.E2_ParentID = consignment2.PK;
			jobDocAddress2.E2_ParentTableCode = "LTC";
			jobDocAddress2.E2_OA_Address = orgAddress2.PK;
			jobDocAddress2.E2_AddressType = DocAddressTypes.Codes.ClientRequestedBillingParty;

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("BillingPartyCompanyName", "Adam's Accessories Inc.");
			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query).Select(x => x.LTC_JobID);

			AssertContainsExactElementsInAnyOrder("Only the consignment with the matching Billing party name should be returned. " + query.LiteralTextADOFormatted,
				new[] { "CN2" }, results);
		}

		public void TestBillingPartyOrgCode()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = helper.CreateConsignment("CN1");
			var consignment2 = helper.CreateConsignment("CN2");

			var org1 = helper.CreateOrganisation("ORG1");
			var org2 = helper.CreateOrganisation("ORG2");
			org1.OH_Code = "DNLFGB";
			org2.OH_Code = "JRARNL";

			var orgAddress1 = helper.CreateOrgAddress(org1, "2000");
			var orgAddress2 = helper.CreateOrgAddress(org2, "2000");

			var jobDocAddress1 = Factory.New<JobDocAddress>();
			jobDocAddress1.E2_ParentID = consignment1.PK;
			jobDocAddress1.E2_ParentTableCode = "LTC";
			jobDocAddress1.E2_OA_Address = orgAddress1.PK;
			jobDocAddress1.E2_AddressType = DocAddressTypes.Codes.LocalCartageExporter;

			var jobDocAddress2 = Factory.New<JobDocAddress>();
			jobDocAddress2.E2_ParentID = consignment2.PK;
			jobDocAddress2.E2_ParentTableCode = "LTC";
			jobDocAddress2.E2_OA_Address = orgAddress2.PK;
			jobDocAddress2.E2_AddressType = DocAddressTypes.Codes.ClientRequestedBillingParty;

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("BillingPartyOrgCode", "JRARNL");
			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query).Select(x => x.LTC_JobID);

			AssertContainsExactElementsInAnyOrder("Only the consignment with the matching Billing party organisation code should be returned." + query.LiteralTextADOFormatted,
				new[] { "CN2" }, results);
		}

		public void TestDeliveryCompanyName()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = helper.CreateConsignment("CN1");
			var consignment2 = helper.CreateConsignment("CN2");

			var org1 = helper.CreateOrganisation("ORG1");
			var org2 = helper.CreateOrganisation("ORG2");
			org1.OH_FullName = "Vik's Variety LLC";
			org2.OH_FullName = "Adam's Accessories Inc.";

			var orgAddress1 = helper.CreateOrgAddress(org1, "2000");
			var orgAddress2 = helper.CreateOrgAddress(org2, "2000");

			helper.CreateConsignmentAddressWithAutoSequence(consignment1, ConsignmentAddressTypes.Codes.PickUp, orgAddress1, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageExporter);
			helper.CreateConsignmentAddressWithAutoSequence(consignment1, ConsignmentAddressTypes.Codes.Delivery, orgAddress2, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageImporter);
			helper.CreateConsignmentAddressWithAutoSequence(consignment2, ConsignmentAddressTypes.Codes.PickUp, orgAddress2, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageExporter);
			helper.CreateConsignmentAddressWithAutoSequence(consignment2, ConsignmentAddressTypes.Codes.Delivery, orgAddress1, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageImporter);

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("DeliveryCompanyName", "Adam's Accessories Inc.");
			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query).Select(x => x.LTC_JobID);

			AssertContainsExactElementsInAnyOrder("Only the consignment with the matching delivery company name should be returned. The other consignment has this company name as well, but only for its pickup. " + query.LiteralTextADOFormatted,
				new[] { "CN1" }, results);
		}

		public void TestPickupCompanyName()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = helper.CreateConsignment("CN1");
			var consignment2 = helper.CreateConsignment("CN2");

			var org1 = helper.CreateOrganisation("ORG1");
			var org2 = helper.CreateOrganisation("ORG2");
			org1.OH_FullName = "Vik's Variety LLC";
			org2.OH_FullName = "Adam's Accessories Inc.";

			var orgAddress1 = helper.CreateOrgAddress(org1, "2000");
			var orgAddress2 = helper.CreateOrgAddress(org2, "2000");

			helper.CreateConsignmentAddressWithAutoSequence(consignment1, ConsignmentAddressTypes.Codes.PickUp, orgAddress1, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageExporter);
			helper.CreateConsignmentAddressWithAutoSequence(consignment1, ConsignmentAddressTypes.Codes.Delivery, orgAddress2, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageImporter);
			helper.CreateConsignmentAddressWithAutoSequence(consignment2, ConsignmentAddressTypes.Codes.PickUp, orgAddress2, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageExporter);
			helper.CreateConsignmentAddressWithAutoSequence(consignment2, ConsignmentAddressTypes.Codes.Delivery, orgAddress1, ConsignmentAddressStatus.Codes.Allocated, DocAddressType.LocalCartageImporter);

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("PickupCompanyName", "Adam's Accessories Inc.");
			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query).Select(x => x.LTC_JobID);

			AssertContainsExactElementsInAnyOrder("Only the consignment with the matching pickup address should be returned. The other consignment has this address as well, but only for its delivery. " + query.LiteralTextADOFormatted,
				new[] { "CN2" }, results);
		}

		public void TestConsignmentNumber()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = helper.CreateConsignment("CN1");
			var consignment2 = helper.CreateConsignment("CN21");

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("ConsignmentNumber", "CN2");
			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query).Select(x => x.LTC_JobID);

			AssertContainsExactElementsInAnyOrder("Only the consignment with the matching consignment number should be returned." + query.LiteralTextADOFormatted,
				new[] { "CN21" }, results);
		}

		int HelperForMakingButtonAndGettingCount<T>(string description)
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.DtbConsignment))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = typeof(T) == typeof(ZDropButton) ? filterBizo.AddFilterStrip<ModuleFilter>(description) : filterBizo.AddFilterStrip<ModuleFlagsFilter>(description);

				var layout = FilterStripsTestHelper.SaveFilterLayout(filterBizo, "my layout", true, true, false);
				module.PerformSearch_ForTest();
				Application.DoEvents();

				using (var popup = (ZForm)module.ShowPopup())
				{
					Application.DoEvents();

					var buttons = popup.FindAll<T>();
					return buttons.Count();
				}
			}
		}

		public void TestJobAccrualAmountFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job Accrual Amount"));
		}

		public void TestJobCostAmountFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job Cost Amount"));
		}

		public void TestJobMarginPercentageFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job Margin %"));
		}

		public void TestJobProfitAmountFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job Profit Amount"));
		}

		public void TestJobRevenueAmountFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job Revenue Amount"));
		}

		public void TestJobWIPAmountFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job WIP Amount")	);
		}

		public void TestJobWIPAmountDeferredChargesOnlyFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job WIP Amount (Deferred Charges Only)"));
		}

		public void TestJobWIPAmountExcludingDeferredChargesFilter()
		{
			AssertEquals(2, HelperForMakingButtonAndGettingCount<ZDropButton>("Job WIP Amount (Excluding Deferred Charges)"));
		}

		public void TestJobswithoutstandingAccrualsFilter()
		{
			AssertEquals(1, HelperForMakingButtonAndGettingCount<ZCheckBox>("Jobs with outstanding Accruals"));
		}

		public void TestJobswithoutstandingWIPsFilter()
		{
			AssertEquals(1, HelperForMakingButtonAndGettingCount<ZCheckBox>("Jobs with outstanding WIPs"));
		}

		public void TestJobswithoutanyoutstandingAccrualFilter()
		{
			AssertEquals(1, HelperForMakingButtonAndGettingCount<ZCheckBox>("Jobs without any outstanding Accrual"));
		}

		public void TestJobswithoutanyoutstandingWIPFilter()
		{
			AssertEquals(1, HelperForMakingButtonAndGettingCount<ZCheckBox>("Jobs without any outstanding WIP"));
		}

		#region Accounting

		public void TestInvoiceStatusFilter()
		{
			var consignment1 = Factory.NewWithValidTestData<DtbConsignment>();
			var consignment2 = Factory.NewWithValidTestData<DtbConsignment>();
			var consignment3 = Factory.NewWithValidTestData<DtbConsignment>();

			consignment1.LTC_JobID = "FIRST";
			consignment2.LTC_JobID = "SECOND";
			consignment3.LTC_JobID = "THIRD";

			var job1 = new JobHeader.Loader(consignment1).TryCreate();
			var job2 = new JobHeader.Loader(consignment2).TryCreate();
			job1.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			job2.JH_Status = JobHeaderStatus.JobReadyForDelivery.Code;

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			filterBizo.AddTextFilterStrip("Invoice Status", JobHeaderStatus.WorkOnHold.Code);

			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query);

			AssertContainsExactElementsInAnyOrder(new[] { "FIRST" }, results.Select(x => x.LTC_JobID));
		}

		public void TestInvoicedChargesBillingFilter()
		{
			var consignment1 = Factory.NewWithValidTestData<DtbConsignment>();
			var consignment2 = Factory.NewWithValidTestData<DtbConsignment>();

			consignment1.LTC_JobID = "FIRST";
			consignment2.LTC_JobID = "SECOND";

			var job1 = new JobHeader.Loader(consignment1).TryCreate();
			var job2 = new JobHeader.Loader(consignment2).TryCreate();

			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job1.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 10m;

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<ModuleFlagsFilter>("Invoiced / Charges / Billing");
			filter["No Charges"] = true;

			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query);

			AssertContainsExactElementsInAnyOrder("Only the first job has charges, so the other one should be the only one included.",
				new[] { "SECOND" }, results.Select(x => x.LTC_JobID));

			AssertEquals("The 'Local billing not paid' options should be included.", true,
				filter.FlagNames.Contains("Local Billing Not Paid", StringComparer.InvariantCultureIgnoreCase));
		}

		#endregion

		public void TestIncotermFilter()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = helper.CreateConsignment("CN1");
			var consignment2 = helper.CreateConsignment("CN2");

			consignment1.LTC_Incoterm = "FOB";
			consignment2.LTC_Incoterm = "EXW";

			Factory.Save();

			var filterBizo = new DtbConsignmentFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("Incoterm", "FOB");

			var query = filterBizo.Filter;
			var results = Factory.Load<DtbConsignment>(query).Select(x => x.LTC_JobID);

			AssertContainsExactElementsInAnyOrder("Only CN1 should be returned as it alone has the incoterm FOB",
				new[] { "CN1" }, results);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbConsignmentFilterBusinessObject();
		}

		#endregion
	}
}
