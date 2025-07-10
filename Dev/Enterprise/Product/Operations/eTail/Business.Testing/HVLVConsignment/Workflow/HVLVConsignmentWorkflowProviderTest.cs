using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignment))]
	class HVLVConsignmentWorkflowProviderTest : WorkflowProviderTest<HVLVConsignment, HVLVConsignmentProcessTaskCollection>
	{
		public void TestGetTemplateSelectionCriteria_eTailer()
		{
			Consignment.BookingHeader.BillToParty.OA_Code = "FLOOGERJOBBIN"; // because NewWithValidTestData creates duplicate values, causing unique index failure
			AssertGetTemplateFilterCriteria(Consignment.BookingHeader.BillToParty.OA_OHInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_Dispatch()
		{
			Consignment.BookingHeader.HVH_OA_DispatchAddress = Consignment.Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertGetTemplateFilterCriteria<ZString>(Consignment.BookingHeader.DispatchAddress.OA_RL_NKRelatedPortCodeInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "USLAX", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ServiceLevel()
		{
			AssertGetTemplateFilterCriteria<ZString>(Consignment.BookingHeader.HVH_RS_NKBookingServiceLevelInfo, ProcessTaskTemplate.P0_SubType1Info, "STD", "D2D", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_Branch()
		{
			var company = Consignment.Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "ABC";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "XYZ";

			ProcessTaskTemplate.P0_GC = company.PK;
			ProcessTaskTemplate.P0_GB = branch2.PK;
			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;

			Consignment.Factory.Save();

			AssertEquals("Precondition; ProcessTaskTemplate has a Task", 1, ProcessTaskTemplate.WorkflowItems.Tasks.Count);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Consignment.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals(0, Consignment.WorkflowItems.Tasks.Count);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Consignment.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals(1, Consignment.WorkflowItems.Tasks.Count);
			}
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			BusinessObject.HasChanges = true;
			Factory.Save();
			AssertEquals("HVLVConsignment doesn't support persistent Tasks & Milestones.", 0, ((IWorkflowProvider)BusinessObject).WorkflowItems.Count);
		}

		public void TestConsigneeDefaultAddress_OrganisationHasOneAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var organisation = Factory.New<OrgHeader>();
			var address = organisation.Addresses[0];

			AssertEquals("precondition", 1, organisation.AddressesActive.Count);
			AssertEquals("precondition", ZGuid.Empty, consignment.HVC_OA_ConsigneeAddress);

			consignment.HVC_OA_ConsigneeAddress_ZAddress.OrgPK = organisation.PK;

			AssertEquals("Consignee Address should be auto assigned", address.PK, consignment.HVC_OA_ConsigneeAddress);
		}

		public void TestConsigneeDefaultAddress_OrganisationHasMultipleAddresses()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var organisation = Factory.New<OrgHeader>();
			organisation.Addresses.AddNew();

			AssertEquals("precondition", 2, organisation.AddressesActive.Count);
			AssertEquals("precondition", ZGuid.Empty, consignment.HVC_OA_ConsigneeAddress);

			consignment.HVC_OA_ConsigneeAddress_ZAddress.OrgPK = organisation.PK;

			AssertEquals("Consignee Address should not be auto assigned", ZGuid.Empty, consignment.HVC_OA_ConsigneeAddress);
		}

		public void TestConsigneeDefaultAddress_OrganisationHasOneInactiveAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var organisation = Factory.New<OrgHeader>();
			var address = organisation.Addresses[0];
			address.OA_IsActive = ZBool.False;

			AssertEquals("precondition", 0, organisation.AddressesActive.Count);
			AssertEquals("precondition", ZGuid.Empty, consignment.HVC_OA_ConsigneeAddress);

			consignment.HVC_OA_ConsigneeAddress_ZAddress.OrgPK = organisation.PK;

			AssertEquals("Consignee Address should not be auto assigned", ZGuid.Empty, consignment.HVC_OA_ConsigneeAddress);
		}

		public void TestShipperDefaultAddress_OrganisationHasOneAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var organisation = Factory.New<OrgHeader>();
			var address = organisation.Addresses[0];

			AssertEquals("precondition", 1, organisation.AddressesActive.Count);
			AssertEquals("precondition", ZGuid.Empty, consignment.HVC_OA_ShipperAddress);

			consignment.HVC_OA_ShipperAddress_ZAddress.OrgPK = organisation.PK;

			AssertEquals("Shipper Address should be auto assigned", address.PK, consignment.HVC_OA_ShipperAddress);
		}

		public void TestShipperDefaultAddress_OrganisationHasMultipleAddresses()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var organisation = Factory.New<OrgHeader>();
			organisation.Addresses.AddNew();

			AssertEquals("precondition", 2, organisation.AddressesActive.Count);
			AssertEquals("precondition", ZGuid.Empty, consignment.HVC_OA_ShipperAddress);

			consignment.HVC_OA_ShipperAddress_ZAddress.OrgPK = organisation.PK;

			AssertEquals("Shipper Address should not be auto assigned", ZGuid.Empty, consignment.HVC_OA_ShipperAddress);
		}

		public void TestShipperDefaultAddress_OrganisationHasOneInactiveAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var organisation = Factory.New<OrgHeader>();
			var address = organisation.Addresses[0];
			address.OA_IsActive = ZBool.False;

			AssertEquals("precondition", 0, organisation.AddressesActive.Count);
			AssertEquals("precondition", ZGuid.Empty, consignment.HVC_OA_ShipperAddress);

			consignment.HVC_OA_ShipperAddress_ZAddress.OrgPK = organisation.PK;

			AssertEquals("Shipper Address should be auto assigned", ZGuid.Empty, consignment.HVC_OA_ShipperAddress);
		}

		#region Implementation

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;

		HVLVConsignment Consignment => BusinessObject;

		protected override HVLVConsignment GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var consignment = base.GetNewBusinessObject(factory);
			consignment.HVC_HVH_BookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>().PK;

			return consignment;
		}

		#endregion
	}
}
