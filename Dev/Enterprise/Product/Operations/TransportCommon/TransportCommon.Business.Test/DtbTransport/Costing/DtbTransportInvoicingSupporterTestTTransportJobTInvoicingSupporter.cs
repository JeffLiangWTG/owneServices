using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInvoicingSupporterTest<TTransportJob, TInvoicingSupporter> : JobInvoicingSupporterTest
			where TTransportJob : DtbTransport
			where TInvoicingSupporter : DtbTransportInvoicingSupporter
	{
		#region Packages

		public void TestContainerMode()
		{
			AssertEquals(Constants.ContainerModes.LCL, InvoicingSupporter.ContainerMode);
			var container = Helper.CreatePackageContainer("CNT-1");
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(Constants.ContainerModes.FCL, InvoicingSupporter.ContainerMode);

			var package = Helper.CreatePackage("PKG-1");
			Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(Constants.ContainerModes.LCL, InvoicingSupporter.ContainerMode);
		}

		public void TestActualVolume()
		{
			AssertEquals(0m, InvoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, InvoicingSupporter.ActualVolumeUnit);

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package20.KP_VolumeUQ = Constants.Volume.CubicFeet;

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(12.622971m, InvoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, InvoicingSupporter.ActualVolumeUnit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40, 26);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(26m, InvoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, InvoicingSupporter.ActualVolumeUnit);
		}

		public void TestActualWeight()
		{
			AssertEquals(0m, InvoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, InvoicingSupporter.ActualWeightUnit);

			var frmInstruction = Helper.CreateInstruction(TransportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(TransportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_WeightUQ = Constants.Weight.Kilograms;
			package20.KP_WeightUQ = Constants.Weight.Pounds;

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(20.52544m, InvoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, InvoicingSupporter.ActualWeightUnit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 400);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(400m, InvoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, InvoicingSupporter.ActualWeightUnit);
		}

		public void TestContainerCount()
		{
			AssertEquals(0, InvoicingSupporter.ContainerCount);

			var instruction = Helper.CreateInstruction(TransportJob);
			var container20GP = Helper.CreatePackageContainer("CNT-1");
			Helper.CreatePackageDivot(instruction, container20GP, 1);
			AssertEquals(1, InvoicingSupporter.ContainerCount);

			var container40GP = Helper.CreatePackageContainer("CNT-2", containerType: "40GP");
			Helper.CreatePackageDivot(instruction, container40GP, 1);
			AssertEquals(2, InvoicingSupporter.ContainerCount);
		}

		public override void TestOuterPackTotal()
		{
			AssertEquals(0, InvoicingSupporter.OuterPackTotal);

			var instruction = Helper.CreateInstruction(TransportJob);
			var package1 = Helper.CreatePackage(Constants.PkgUnit.Pallet, 1);
			Helper.CreatePackageDivot(instruction, package1, 1);
			AssertEquals(1, InvoicingSupporter.OuterPackTotal);

			var package2 = Helper.CreatePackage(Constants.PkgUnit.Box, 2);
			Helper.CreatePackageDivot(instruction, package2, 2);
			AssertEquals(3, InvoicingSupporter.OuterPackTotal);

			var instruction2 = Helper.CreateInstruction(TransportJob, InstructionTypes.Codes.Delivery);
			var package3 = Helper.CreatePackage(Constants.PkgUnit.Pallet, 4);
			Helper.CreatePackageDivot(instruction2, package3, 4);
			AssertEquals(7, InvoicingSupporter.OuterPackTotal);
		}

		public void TestTEUCount()
		{
			AssertEquals(0m, InvoicingSupporter.TEUCount);

			var instruction = Helper.CreateInstruction(TransportJob);
			var container20GP = Helper.CreatePackageContainer("CNT-1");
			Helper.CreatePackageDivot(instruction, container20GP, 1);
			AssertEquals(1m, InvoicingSupporter.TEUCount);

			var container40GP = Helper.CreatePackageContainer("CNT-2", containerType: "40GP");
			Helper.CreatePackageDivot(instruction, container40GP, 1);
			AssertEquals(3m, InvoicingSupporter.TEUCount);
		}

		#endregion

		#region Other

		public void TestConsolType()
		{
			AssertEquals(Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, InvoicingSupporter.ConsolType);
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			AssertEquals(true, InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);

			Factory.Save();
			AssertEquals(false, InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		public void TestTransportMode()
		{
			AssertEquals(Constants.TransportModes.Road, InvoicingSupporter.TransportMode);
		}

		public void TestConsignor()
		{
			AssertNull(InvoicingSupporter.Consignor);
		}

		public void TestOverriddenDepartmentPK()
		{
			var defaultDepartment = Guid.NewGuid();
			AccountingConfigurationRegistry.Instance.TransportBookingJobsDefaultDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultDepartment);

			AssertEquals(defaultDepartment, InvoicingSupporter.OverriddenDepartmentPK);
		}

		public void TestOverriddenDefaultLocalClient()
		{
			TransportJob.ConsolidationSingleJob.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BookingPartyDocumentaryAddress).OrganisationPK = ZGuid.Empty;
			AssertNull(InvoicingSupporter.OverriddenDefaultLocalClient);

			var org = Factory.New<OrgHeader>();
			TransportJob.ConsolidationSingleJob.BookedByAddress.OrganisationPK = org.PK;
			AssertEquals(org, InvoicingSupporter.OverriddenDefaultLocalClient);
		}

		#endregion

		#region TestOperationsBranch

		public void TestTransport_OperationsBranch_UseRegistryFallback()
		{
			// pick up instruction
			var pickupInstruction = Helper.CreateInstruction(TransportJob, InstructionTypes.Codes.PickUp);
			var zoneA = Helper.CreateZone("Zone A");
			pickupInstruction.KN_TZ_DomesticZone = zoneA.PK;

			var pickupOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = pickupOrgHeader.Addresses.AddNew();
			SetupAddress(pickupAddress, "Main Office Address", OrgConstants.AddressType.Office, true);
			var pickupControllingBranch = Factory.NewWithValidTestData<GlbBranch>();
			pickupOrgHeader.CompanyData.OB_GB_ControllingBranch = pickupControllingBranch.PK;

			// deliver instruction
			var deliverInstruction = Helper.CreateInstruction(TransportJob, InstructionTypes.Codes.Delivery);
			var zoneB = Helper.CreateZone("Zone B");
			deliverInstruction.KN_TZ_DomesticZone = zoneB.PK;

			var deliverOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var deliverAddress = deliverOrgHeader.Addresses.AddNew();
			SetupAddress(deliverAddress, "Main Office Address", OrgConstants.AddressType.Office, true);
			var deliverControllingBranch = Factory.NewWithValidTestData<GlbBranch>();
			deliverOrgHeader.CompanyData.OB_GB_ControllingBranch = deliverControllingBranch.PK;

			// PortHubSelection
			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = pickupAddress.PK;
			portHubSelection1.TY_Direction = "PIC";
			portHubSelection1.TY_RatingFreightMode = "ALL";

			// PortHubZonePivot
			var portHubZonePivot1 = Factory.New<PortHubZonePivot>();
			portHubZonePivot1.TX_TY_Hub = portHubSelection1.PK;
			portHubZonePivot1.TX_TZ_Zone = zoneA.PK;

			Factory.Save();

			// before accounting registry set up
			SetUpRegistry(1, 0);
			AssertNull("Invoicing branch should default to be null.", InvoicingSupporter.OperationsBranch);

			// DefaultToBranchRelatedToPortOrWarehouseBranch > 0
			SetUpRegistry(0, 1);
			AssertEquals("Invoicing branch should default to be associated pick-up depot's controlling branch.", pickupControllingBranch, invoicingSupporter.OperationsBranch);
		}

		void SetUpRegistry(ZShort defaultToBlank, ZShort defaultToBranchRelatedToPortOrWarehouseBranch)
		{
			var rule = new JobBranchDefaultOrderRule();

			rule.DefaultToBlank = defaultToBlank;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = defaultToBranchRelatedToPortOrWarehouseBranch;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToLoginUserDefault = 0;

			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		void SetupAddress(OrgAddress address, string description, string addressCapability, bool isMain)
		{
			address.OA_Address1 = description;
			address.OA_Address2 = description + " 2";
			address.OA_City = "Org City";
			address.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
			address.AddressCapability.SetCapabilityEnabled(addressCapability);

			if (isMain)
			{
				address.AddressCapability.SetIsMainAddress(addressCapability);
			}
			else
			{
				address.AddressCapability.SetIsNotMainAddress(addressCapability);
			}
		}

		#endregion

		#region Implementation

		#region TransportJob

		protected TTransportJob TransportJob
		{
			get { return transportJob ?? (transportJob = (TTransportJob)GetNewBusinessObject()); }
		}

		TTransportJob transportJob;

		#endregion

		#region InvoicingSupporter

		protected TInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = CreateNewInvoicingSupporter(TransportJob)); }
		}

		TInvoicingSupporter invoicingSupporter;

		protected abstract TInvoicingSupporter CreateNewInvoicingSupporter(TTransportJob transport);

		#endregion

		#region Implementation

		protected TransportCommonTestHelper Helper
		{
			get { return helper ?? (helper = GetNewTestHelper()); }
		}

		protected virtual TransportCommonTestHelper GetNewTestHelper()
		{
			return new TransportCommonTestHelper(Factory);
		}

		TransportCommonTestHelper helper;

		#endregion

		#endregion
	}
}
