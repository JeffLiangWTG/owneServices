using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvoiceRollupOrGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIsShipmentBrokerageOrBoth()
		{
			Assert("Should be true, JobType is Brokerage type", OrgInvoiceRollupOrGroupLookups.IsJobTypeShipmentBrokerageOrBoth(JobInvoicingConsumerTypes.Brokerage.Code));
			Assert("Should be true, JobType is Shipment type", OrgInvoiceRollupOrGroupLookups.IsJobTypeShipmentBrokerageOrBoth(JobInvoicingConsumerTypes.Shipment.Code));
			Assert("Should be true, JobType is ShipmentAndBrokerage type", OrgInvoiceRollupOrGroupLookups.IsJobTypeShipmentBrokerageOrBoth(OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code));
			Assert("Should be false, JobType is CTOCusMawb type", !OrgInvoiceRollupOrGroupLookups.IsJobTypeShipmentBrokerageOrBoth(JobInvoicingConsumerTypes.CTOCusMAWB.Code));
		}

		public void TestJobTypeFullList()
		{
			int originalCount = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes().Count;
			AssertEquals("OrgInvoiceRollupOrGroupLookups.JobTypeFullList.Count should be 3 more than standard list", originalCount + 3, OrgInvoiceRollupOrGroupLookups.JobTypeFullList.Count);
		}

		public void TestGroupOrSubTotalList()
		{
			AssertEquals("GroupOrSubTotalList.Count should be 8", 8, Lookups.GroupOrSubTotalList.Count);

			OrgInvoice.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("GroupOrSubTotalList.Count should be 8", 8, Lookups.GroupOrSubTotalList.Count);

			OrgInvoice.PG_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			AssertEquals("GroupOrSubTotalList.Count should be 9", 9, Lookups.GroupOrSubTotalList.Count);
		}

		public void TestInvoicePostingOptionsList()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			Assert("Default code added", org.CompanyData.InvoiceRollupOrGroups[0].Lookups.InvoicePostingOptionsList.ContainsCode("DEF"));

			InvoiceRollupOrGroup defaults = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			defaults.InvoicePostingStyle = "DFI";
			InvoiceRollupOrGroupCollection value = new InvoiceRollupOrGroupCollection();
			value.Add(defaults);
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
			AssertEquals("Description correct", "Use Defaults from Registry - refer to registry setting for details", org.CompanyData.InvoiceRollupOrGroups[0].Lookups.InvoicePostingOptionsList.GetDescriptionFromCode("DEF"));

			defaults.InvoicePostingStyle = "ITC";
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
			AssertEquals("Description correct", "Use Defaults from Registry - refer to registry setting for details", org.CompanyData.InvoiceRollupOrGroups[0].Lookups.InvoicePostingOptionsList.GetDescriptionFromCode("DEF"));
		}

		public void TestInvoiceLineDisplayOptionsList()
		{
			InvoiceRollupOrGroup defaults = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			defaults.InvoiceLineDisplayOption = "FRX";
			InvoiceRollupOrGroupCollection value = new InvoiceRollupOrGroupCollection();
			value.Add(defaults);
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);

			OrgHeader org = Factory.New<OrgHeader>();
			Assert("Default code added", org.CompanyData.InvoiceRollupOrGroups[0].Lookups.InvoiceLineDisplayOptionsList.ContainsCode("DEF"));
			AssertEquals("Description correct", "Use Defaults from Registry - refer to registry setting for details", org.CompanyData.InvoiceRollupOrGroups[0].Lookups.InvoiceLineDisplayOptionsList.GetDescriptionFromCode("DEF"));

			defaults.InvoiceLineDisplayOption = "ALL";
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
			AssertEquals("Description correct", "Use Defaults from Registry - refer to registry setting for details", org.CompanyData.InvoiceRollupOrGroups[0].Lookups.InvoiceLineDisplayOptionsList.GetDescriptionFromCode("DEF"));
		}

		public void TestDescriptionOfCLCGroupOrSubTotalStyle()
		{
			var groupOrSubTotalStyleList = Lookups.GroupOrSubTotalStyleList;
			AssertEquals("GroupOrSubTotalStyleList.Count should be 6", 6, groupOrSubTotalStyleList.Count);
			Assert(groupOrSubTotalStyleList.ContainsCode("CLC"));
			AssertEquals("6 Columns Display, Subtotal by Currency, Roll Up by Charge Code and Currency", groupOrSubTotalStyleList["CLC"].Description);
		}

		public void TestGroupOrSubTotalStyleList()
		{
			AssertEquals("GroupOrSubTotalStyleList.Count should be 6", 6, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList.Count should be 5", 5, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList("").Count);

			OrgInvoice.PG_JobType = JobInvoicingConsumerTypes.CusMAWB.Code;
			AssertEquals("When GroupOrRollup is not a shipment or brokerage", 6, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is not a shipment or brokerage", 5, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);

			OrgInvoice.PG_JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			AssertEquals("When GroupOrRollup is brokerage type", 15, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is brokerage type", 14, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);

			OrgInvoice.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("When GroupOrRollup is Shipment type", 15, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is Shipment type", 14, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);

			OrgInvoice.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			AssertEquals("When GroupOrRollup is ShipmentAndBrokerage type", 15, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is ShipmentAndBrokerage type", 14, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);

			OrgInvoice.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			AssertEquals("When GroupOrRollup is ALL type", 15, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is ALL type", 14, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);

			OrgInvoice.PG_JobType = JobInvoicingConsumerTypes.LocalCartage.Code;
			AssertEquals("When GroupOrRollup is LocalTransport", 6, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is LocalTransport", 5, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);

			OrgInvoice.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code;
			AssertEquals("When GroupOrRollup is NonJobRelated", 5, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is NonJobRelated", 4, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);

			OrgInvoice.PG_JobType = JobInvoicingConsumerTypes.AgencyBillOfLading.Code;
			AssertEquals("When GroupOrRollup is Agency Bill of Lading", 15, Lookups.GroupOrSubTotalStyleList.Count);
			AssertEquals("When GroupOrRollup is Agency Bill of Lading", 14, OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(OrgInvoice.PG_JobType).Count);
		}

		public void TestGroupOrSubTotalStyleCodeWhenRegistryValueInvalid()
		{
			AssertEquals(OrgConstants.InvoiceLineGroupings.Code.None, OrgInvoiceRollupOrGroupLookups.GroupOrSubTotalStyleCodeWhenRegistryValueInvalid);
		}

		OrgInvoiceRollupOrGroupLookups Lookups;
		OrgInvoiceRollupOrGroup OrgInvoice;
		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader org = Factory.New<OrgHeader>();
			OrgInvoice = org.CompanyData.InvoiceRollupOrGroups.AddNew();
			Lookups = OrgInvoice.Lookups;
		}
	}
}
