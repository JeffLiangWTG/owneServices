using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.TransportConsignment.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignment))]
	sealed class DtbConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		#region ICustomFieldProvider

		[TestedType(typeof(DtbConsignment))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestICustomFieldProvider()
		{
			var customFieldProvider = Factory.NewWithValidTestData<DtbConsignment>() as ICustomFieldProvider;
			AssertNotNull(customFieldProvider);
			var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			AssertNotNull(customBusinessObject);
		}

		public void TestCustomFieldsReadOnlyState_IsTheSameAsDtbConsignmentReadOnlyState_WhenBizoReadOnlyStateChanged()
		{
			var consignment = Factory.New<DtbConsignmentForTest>();

			consignment.ReadOnly = true;
			var consignmentProvider = (ICustomFieldProvider)consignment;
			var consignmentCustomBizo = consignmentProvider.GetCustomBusinessObject();
			AssertEquals(consignment.ReadOnly, consignmentCustomBizo.ReadOnly);

			consignment.ReadOnly = false;
			consignmentCustomBizo = consignmentProvider.GetCustomBusinessObject();
			AssertEquals(consignment.ReadOnly, consignmentCustomBizo.ReadOnly);
		}

		public void TestCustomFieldsDoChange_WhenParameter_shouldRefresh_IsTrue()
		{
			var consignment = Factory.New<DtbConsignmentForTest>();

			var consignmentProvider = (ICustomFieldProvider)consignment;
			var consignmentCustomBizo1 = consignmentProvider.GetCustomBusinessObject();
			var consignmentCustomBizo2 = consignmentProvider.GetCustomBusinessObject(false);
			AssertEquals("Expecting same instance of CustomBusinessObject when shouldRefresh parameter is false.", consignmentCustomBizo1, consignmentCustomBizo2);
			var consignmentCustomBizo3 = consignmentProvider.GetCustomBusinessObject(true);
			AssertNotEquals("Expecting different instance of CustomBusinessObject when shouldRefresh parameter is true.", consignmentCustomBizo1, consignmentCustomBizo3);
		}

		#endregion

		#region OnLoaded

		public void TestConsignmentOnLoaded_WhenClosedTimeIsEmpty_ShouldNotSetConsignmentReadOnly()
		{
			var consignment = Helper.CreateConsignment("CN1");
			consignment.LTC_ClosedTime = ZDateTimeOffset.Empty;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedConsignment = newFactory.Load<DtbConsignment>(consignment.PK);

			AssertEquals("Consignment should be not be read-only.", expected: false, loadedConsignment.ReadOnly);
		}

		public void TestConsignmentOnLoaded_WhenClosedTimeIsNotEmpty_ShouldSetConsignmentReadonly()
		{
			var consignment = Helper.CreateConsignment("CN1");
			consignment.LTC_ClosedTime = ZDateTimeOffset.Now;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedConsignment = newFactory.Load<DtbConsignment>(consignment.PK);

			AssertEquals("Consignment should be read only.", expected: true, loadedConsignment.ReadOnly);
		}

		public void TestConsignmentOnLoaded_WhenClosedTimeIsEmpty_ShouldNotSetWorkflowsReadOnly()
		{
			var consignment = Helper.CreateConsignment("CN1");
			consignment.LTC_ClosedTime = ZDateTimeOffset.Empty;
			consignment.WorkflowItems.AddNew();

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedConsignment = newFactory.Load<DtbConsignment>(consignment.PK);

			AssertEquals("Workflows should be not be read-only.", expected: false, loadedConsignment.Workflows.ReadOnly);
			AssertEquals("WorkflowItems should be not be read-only.", expected: false, loadedConsignment.WorkflowItems.ReadOnly);
			foreach (var workflowItem in loadedConsignment.WorkflowItems)
			{
				AssertEquals("WorkflowItem should be not be read-only.", expected: false, workflowItem.ReadOnly);
			}
		}

		public void TestConsignmentOnLoaded_WhenClosedTimeIsNotEmpty_ShouldSetWorkflowsReadonly()
		{
			var consignment = Helper.CreateConsignment("CN1");
			consignment.LTC_ClosedTime = ZDateTimeOffset.Now;
			consignment.WorkflowItems.AddNew();

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedConsignment = newFactory.Load<DtbConsignment>(consignment.PK);

			AssertEquals("Workflows should be read only.", expected: true, loadedConsignment.Workflows.ReadOnly);
			AssertEquals("WorkflowItems should be read only.", expected: true, loadedConsignment.WorkflowItems.ReadOnly);
			foreach (var workflowItem in loadedConsignment.WorkflowItems)
			{
				AssertEquals("WorkflowItem should be be read-only.", expected: true, workflowItem.ReadOnly);
			}
		}

		#endregion

		#region OnSaving

		public void TestConsignmentOnSaving_WhenIncotermFieldIsEmpty_ShouldSetFromOrganizations()
		{
			var org = Helper.CreateOrganisation("ORG1");
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMDefaultINCOTerm = Constants.IncoTerms.DeliveredDutyPaid;
			Factory.Save();

			var consignment = Helper.CreateConsignment("CN1");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, org.MainAddress);

			AssertEquals("Verifying the address is set up correctly", Constants.IncoTerms.DeliveredDutyPaid, consignment.DeliveryOrgAddress.Header.MiscServ.OM_IMDefaultINCOTerm);
			AssertEquals(string.Empty, consignment.LTC_Incoterm);

			Factory.Save();

			AssertEquals("The user did not select an incoterm, so values from the related organizations should be used automatically.", Constants.IncoTerms.DeliveredDutyPaid, consignment.LTC_Incoterm);
		}

		public void TestConsignmentOnSaving_WhenIncotermFieldIsNotEmpty_ShouldLeaveCurrentIncoterm()
		{
			var org = Helper.CreateOrganisation("ORG1");
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMDefaultINCOTerm = Constants.IncoTerms.DeliveredDutyPaid;

			var consignment = Helper.CreateConsignment("CN1");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, org.MainAddress);

			AssertEquals("Verifying the address is set up correctly", Constants.IncoTerms.DeliveredDutyPaid, consignment.DeliveryOrgAddress.Header.MiscServ.OM_IMDefaultINCOTerm);

			consignment.LTC_Incoterm = Constants.IncoTerms.DeliveredAtTerminal;

			Factory.Save();

			AssertEquals("The user already selected an Incoterm, so the values from its related organizations should be ignored.", Constants.IncoTerms.DeliveredAtTerminal, consignment.LTC_Incoterm);
		}

		public void TestConsignmentOnSaving_WhenServiceLevelFieldIsEmpty_ShouldSetFromOrganizations()
		{
			var org = Helper.CreateOrganisation("ORG1");
			org.OH_IsConsignee = true;
			org.MiscServ.OM_RS_NKIMDefaultServiceLevel = "TSP";

			var consignment = Helper.CreateConsignment("CN1");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, org.MainAddress);

			AssertEquals("Verifying the address is set up correctly", "TSP", consignment.DeliveryOrgAddress.Header.MiscServ.OM_RS_NKIMDefaultServiceLevel);
			AssertEquals(string.Empty, consignment.LTC_RS_NKServiceLevel);

			Factory.Save();

			AssertEquals("The user did not select a service level, so values from the related organizations should be used automatically.", "TSP", consignment.LTC_RS_NKServiceLevel);
		}

		public void TestConsignmentOnSaving_WhenServiceLevelFieldIsNotEmpty_ShouldLeaveCurrentServiceLevel()
		{
			var org = Helper.CreateOrganisation("ORG1");
			org.OH_IsConsignee = true;
			org.MiscServ.OM_RS_NKIMDefaultServiceLevel = "TSP";

			var consignment = Helper.CreateConsignment("CN1");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, org.MainAddress);
			AssertEquals("Verifying the address is set up correctly", "TSP", consignment.DeliveryOrgAddress.Header.MiscServ.OM_RS_NKIMDefaultServiceLevel);

			consignment.LTC_RS_NKServiceLevel = "D2D";

			Factory.Save();

			AssertEquals("The user already selected a service level, so the values from its related organizations should be ignored.", "D2D", consignment.LTC_RS_NKServiceLevel);
		}

		public void TestConsignmentOnSaving_WhenIncotermAndServiceLevelNotEmpty_ShouldNotCheckOrganizations()
		{
			Factory.RefreshEnabled = false;
			var org = Helper.CreateOrganisation("ORG1");
			var consignment = Helper.CreateConsignment("CN1");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, org.MainAddress);
			consignment.LTC_Incoterm = Constants.IncoTerms.DeliveredAtTerminal;
			consignment.LTC_RS_NKServiceLevel = "D2D";

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var loadedConsignment = newFactory.Load<DtbConsignmentForTest>(consignment.PK);
			var mockService = new Mock<IDtbConsignmentService>();
			loadedConsignment.ConsignmentServiceOverride = mockService.Object;

			loadedConsignment.LTC_ChargeableWeight = 1;
			newFactory.Save();

			mockService.Verify(
				x => x.GetDefaultValuesForConsignment(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()),
				Times.Never,
				"The Incoterm and ServiceLevel fields are already set, so we don't need to check the organizations.");
			Assert(true);
		}

		public void TestConsignmentOnSaving_WhenIncotermOrServiceLevelEmpty_AndSameCombinationHasAlreadyBeenChecked_ShouldUseCachedValue()
		{
			var org1 = Helper.CreateOrganisation("ORG1");
			var consignment = Factory.New<DtbConsignmentForTest>();
			consignment.LTC_JobID = "CN1";
			consignment.LTC_Direction = Constants.CartageDirection.Local;
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, org1.MainAddress);
			consignment.LTC_Incoterm = Constants.IncoTerms.DeliveredAtTerminal;

			var emptyResult = new Dictionary<string, string>
			{
				{ DtbConsignmentService.IncoTermKey, string.Empty },
				{ DtbConsignmentService.ServiceLevelKey, string.Empty },
			};
			var mockService = new Mock<IDtbConsignmentService>();
			mockService.Setup(x => x.GetDefaultValuesForConsignment(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(),
				It.IsAny<string>(), It.IsAny<BusinessObjectFactory>())).Returns(emptyResult);
			consignment.ConsignmentServiceOverride = mockService.Object;

			Factory.Save();

			mockService.Verify(
				x => x.GetDefaultValuesForConsignment(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()),
				Times.Once,
				"This is the first time we're checking this combination of addresses, so the service should be used.");

			consignment.LTC_ChargeableWeight = 1;
			Factory.Save();

			mockService.Verify(
				x => x.GetDefaultValuesForConsignment(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()),
				Times.Once(),
				"We've already checked this particular combination of organizations, so there's no need to check again.");

			var org2 = Helper.CreateOrganisation("ORG2");
			Factory.Save();

			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, org2.MainAddress);
			consignment.LTC_ChargeableWeight = 3;
			Factory.Save();

			mockService.Verify(
				x => x.GetDefaultValuesForConsignment(It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<BusinessObjectFactory>()),
				Times.Exactly(2),
				"The addresses have changed, so we need to use the service again.");
			Assert(true);
		}

		public void TestSetDefaultBranch_ShouldUpdateDefaultBranch_WhenBranchDefaultingManagerHasDefaultValue()
		{
			var consignment = Helper.CreateConsignment();
			var job = Helper.CreateJobHeaderForConsignment(consignment);
			var branch = GlbBranch.CurrentBranch;

			var branchManager = new Mock<IControllingBranchDefaultingManager>();
			branchManager.Setup(m => m.FetchDefaultValue(consignment, GlbCompany.CurrentCompany, branch, GlbDepartment.CurrentDepartment)).Returns(branch.PK);
			ObjectFactory.Substitute(branchManager.Object);

			AssertEquals("Branch should be empty", true, consignment.LTC_GB_Branch.IsEmpty);

			Factory.Save();
			AssertEquals("Branch should have value", branch.PK, consignment.LTC_GB_Branch);
		}

		public void TestSetDefaultBranch_ShouldNotUpdateDefaultBranch_WhenBranchDefaultingManagerDoesNotHaveDefaultValue()
		{
			var consignment = Helper.CreateConsignment();
			var branch = GlbBranch.CurrentBranch;
			var job = Helper.CreateJobHeaderForConsignment(consignment);

			var branchManager = new Mock<IControllingBranchDefaultingManager>();
			branchManager.Setup(m => m.FetchDefaultValue(consignment, GlbCompany.CurrentCompany, branch, GlbDepartment.CurrentDepartment)).Returns(new ZString(""));
			ObjectFactory.Substitute(branchManager.Object);

			AssertEquals("Branch should be empty", true, consignment.LTC_GB_Branch.IsEmpty);

			Factory.Save();
			AssertEquals("Branch should still be empty", true, consignment.LTC_GB_Branch.IsEmpty);
		}

		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			var consignment = Factory.New<DtbConsignmentForTest>();
			AssertEquals(true, consignment.IsAutoLogged);
		}

		#endregion

		#region Delivery Address

		public void TestDeliveryAddress()
		{
			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var address2 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			AssertEquals("Precondition", 2, consignment.Addresses.Count);

			var deliveryAddress = consignment.DeliveryAddress;
			AssertEquals(ConsignmentAddressTypes.Codes.Delivery, deliveryAddress.LTS_InstructionType);
			AssertCollectionContains(deliveryAddress, consignment.Addresses);

			var dodgyAddress = consignment.Addresses.AddNew(ConsignmentAddressTypes.Codes.Delivery);
			AssertExceptionThrown(typeof(InvalidOperationException), () => { var poke = consignment.DeliveryAddress; });
		}

		public void TestDeliveryOrgAddress()
		{
			var consignment = Helper.CreateConsignment();
			var orgAddress = Helper.CreateOrganisation("Org").MainAddress;
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, orgAddress);

			AssertEquals(orgAddress, consignment.DeliveryOrgAddress);
		}

		#endregion

		#region Pickup Address

		public void TestPickupAddress()
		{
			var badConsignment = Factory.New<DtbConsignment>();
			var poke = badConsignment.PickupAddress;
			AssertEquals("While we expect one pickup address, this is managed by validation. The error report will only come to us now if there are 2 or more pickup addresses.",
				string.Empty, ErrorReporter.LastMessageReported);

			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var address2 = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			AssertEquals("Precondition", 2, consignment.Addresses.Count);

			var pickupAddress = consignment.PickupAddress;
			AssertEquals(ConsignmentAddressTypes.Codes.PickUp, pickupAddress.LTS_InstructionType);
			AssertCollectionContains(pickupAddress, consignment.Addresses);

			consignment.Addresses.AddNew(ConsignmentAddressTypes.Codes.PickUp);
			poke = consignment.PickupAddress;
			AssertEquals("Consignment contains 2 Pickup Address.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPickupOrgAddress()
		{
			var consignment = Helper.CreateConsignment();
			var orgAddress = Helper.CreateOrganisation("Org").MainAddress;
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, orgAddress);

			AssertEquals(orgAddress, consignment.PickupOrgAddress);
		}

		#endregion

		#region Related Objects

		public void TestBookingPartyAddress()
		{
			var consignment = Factory.New<DtbConsignment>();
			AssertNull("The booking party should be null until one is added.", consignment.BookingPartyAddress);

			var otherAddress = consignment.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			AssertNull("A different address type was added so the Booking Party address should still be null.", consignment.BookingPartyAddress);

			var bookingPartyAddress = consignment.DocAddresses.AddNew(DocAddressType.BookingPartyDocumentaryAddress);
			AssertEquals(bookingPartyAddress, consignment.BookingPartyAddress);
		}

		public void TestClientRequestedBillingPartyAddress()
		{
			var consignment = Factory.New<DtbConsignment>();
			AssertNull("The client requested billing party address should be null until one is added.", consignment.ClientRequestedBillingPartyAddress);

			var otherAddress = consignment.DocAddresses.AddNew(DocAddressType.BookingPartyDocumentaryAddress);
			AssertNull("A different address type was added so the client requested billing party address should still be null.", consignment.ClientRequestedBillingPartyAddress);

			var clientRequestedAddress = consignment.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
			AssertEquals(clientRequestedAddress, consignment.ClientRequestedBillingPartyAddress);
		}

		#endregion

		#region AllActions

		public void TestAllActions()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			AssertEquals("Should contain 0 action.", 0, consignment.AllActions.Count);

			var picAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var dlvAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);

			var picAction = picAddress.Actions[0];
			var dlvAction = dlvAddress.Actions[0];

			AssertContainsExactElementsInAnyOrder("Should contain 2 actions.", new[] { picAction, dlvAction }, consignment.AllActions);
		}

		#endregion

		#region TestPackageJob

		public void TestPackageJob()
		{
			var consignment = Helper.CreateConsignment();
			AssertNotNull(consignment.PackageJob);
			AssertEquals(consignment.PackageJob, consignment.PackageJob);
			AssertEquals(false, consignment.PackageJob.HasChanges);
		}

		#endregion

		#region TestDeletePackage

		#region TestDeletePackage_CanDeletePackage

		public void TestDeletePackage_CanDeletePackage()
		{
			var consignment = Helper.CreateConsignment();
			var package = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package);
			AssertEquals("Can Delete Packages from Consignments", true, package.CanDelete);

			package.Delete();
			AssertEquals("Consignment Package count is 0", 0, consignment.PackageJob.Packages.Count);
		}

		#endregion

		#endregion

		#region TestContainers

		public void TestContainers()
		{
			var consignment = Helper.CreateConsignment();
			AssertEquals("Should contain 0 Container", 0, consignment.Containers.Count());
			AssertEquals("Should contain 0 LoosePackage", 0, consignment.LoosePackages.Count());

			var package = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Container);
			AssertContainsExactElementsInAnyOrder(new[] { package }, consignment.Containers);
			AssertEquals("Should contain 0 LoosePackage", 0, consignment.LoosePackages.Count());
		}

		#endregion

		#region TestLoosePackages

		public void TestLoosePackages()
		{
			var consignment = Helper.CreateConsignment();
			AssertEquals("Should contain 0 LoosePackage", 0, consignment.LoosePackages.Count());
			AssertEquals("Should contain 0 Container", 0, consignment.Containers.Count());

			var package = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package);
			AssertContainsExactElementsInAnyOrder(new[] { package }, consignment.LoosePackages);
			AssertEquals("Should contain 0 Container", 0, consignment.Containers.Count());
		}

		#endregion

		#region TestIsLooseOnly

		public void TestIsLooseOnly()
		{
			var consignment = Helper.CreateConsignment();
			AssertEquals("Should contain 0 LoosePackage", 0, consignment.LoosePackages.Count());
			AssertEquals("Should contain 0 Container", 0, consignment.Containers.Count());
			AssertEquals("Should return true", true, consignment.IsLooseOnly);

			var package1 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package);
			AssertContainsExactElementsInAnyOrder(new[] { package1 }, consignment.LoosePackages);
			AssertEquals("Should return true", true, consignment.IsLooseOnly);

			var package2 = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Container);
			AssertContainsExactElementsInAnyOrder(new[] { package2 }, consignment.Containers);
			AssertEquals("Should return true", true, consignment.IsLooseOnly);
		}

		#endregion

		#region TestGetTotalPackageWeight And Volume

		#region TestGetTotalPackageVolume

		public void TestGetTotalPackageVolume()
		{
			var consignment = Helper.CreateConsignment();

			var packageVolume = consignment.GetTotalLoosePackageVolume();
			AssertEquals(0m, packageVolume.Amount);
			AssertEquals(Constants.Volume.CubicMetres, packageVolume.Unit);
			var package1 = AddPackage(consignment, Constants.PkgUnit.Package, 10, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			var package2 = AddPackage(consignment, Constants.PkgUnit.Package, 20, 21, Constants.Weight.Kilograms, 22, Constants.Volume.CubicFeet);

			packageVolume = consignment.GetTotalLoosePackageVolume();
			AssertEquals(12.622971m, packageVolume.Amount);
			AssertEquals(Constants.Volume.CubicMetres, packageVolume.Unit);

			var package3 = AddPackage(consignment, Constants.PkgUnit.Container, 30, 31, Constants.Weight.Kilograms, 32, Constants.Volume.CubicMetres);

			packageVolume = consignment.GetTotalLoosePackageVolume();
			AssertEquals(12.622971m, packageVolume.Amount);
			AssertEquals(Constants.Volume.CubicMetres, packageVolume.Unit);

			packageVolume = consignment.GetTotalContainerisedVolume();
			AssertEquals(1155.240m, packageVolume.Amount);
			AssertEquals(Constants.Volume.CubicMetres, packageVolume.Unit);
		}

		#endregion

		#region TestGetTotalPackageWeight

		public void TestGetTotalPackageWeight()
		{
			var consignment = Helper.CreateConsignment();

			var packageWeight = consignment.GetTotalLoosePackageWeight();
			AssertEquals(0m, packageWeight.Amount);
			AssertEquals(Constants.Weight.Kilograms, packageWeight.Unit);
			var package1 = AddPackage(consignment, Constants.PkgUnit.Package, 10, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			var package2 = AddPackage(consignment, Constants.PkgUnit.Package, 20, 21, Constants.Weight.Pounds, 22, Constants.Volume.CubicMetres);

			packageWeight = consignment.GetTotalLoosePackageWeight();
			AssertEquals(20.52544m, packageWeight.Amount);
			AssertEquals(Constants.Weight.Kilograms, packageWeight.Unit);

			var package3 = AddPackage(consignment, Constants.PkgUnit.Container, 30, 31, Constants.Weight.Kilograms, 32, Constants.Volume.CubicMetres);

			packageWeight = consignment.GetTotalLoosePackageWeight();
			AssertEquals(20.52544m, packageWeight.Amount);
			AssertEquals(Constants.Weight.Kilograms, packageWeight.Unit);

			packageWeight = consignment.GetTotalContainerisedWeight();
			AssertEquals(68431m, packageWeight.Amount);
			AssertEquals(Constants.Weight.Kilograms, packageWeight.Unit);
		}

		#endregion

		PkgPackage AddPackage(DtbConsignment consignment, ZString packageType, ZInt quantity, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			var package = consignment.PackageJob.Packages.AddNew(packageType, quantity);
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;

			if (packageType == Constants.PkgUnit.Container)
			{
				package.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			}

			return package;
		}

		#endregion

		#region TestPortOfOrigin

		public void TestPortOfOrigin()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			var consignment = Helper.CreateConsignment();
			var consignorAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			var consigneeAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);

			consignorAddress.Address.E2_AddressOverride = false;
			AssertEquals("Port of origin UNLOCO should come from pickup address.", "AUBNE", consignment.PortOfOrigin);

			pickupAddress.OA_RL_NKRelatedPortCode = "";
			AssertEquals("if address doesn't have a UNLOCO then fall back to pickup org header.", "AUSYD", consignment.PortOfOrigin);

			pickupOrg.OH_RL_NKClosestPort = "";
			AssertEquals("if address and header doesn't have a UNLOCO return empty string.", "", consignment.PortOfOrigin);
		}

		#endregion

		#region TestPortOfDestination

		public void TestPortOfDestination()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var deliveryAddress = Helper.CreateOrgAddress(deliveryOrg, "NZAVD");
			var consignment = Helper.CreateConsignment();
			var consignorAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			var consigneeAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryAddress);

			consignorAddress.Address.E2_AddressOverride = false;
			AssertEquals("Port of destination UNLOCO should come from delivery address.", "NZAVD", consignment.PortOfDestination);

			deliveryAddress.OA_RL_NKRelatedPortCode = "";
			AssertEquals("if address doesn't have a UNLOCO then fall back to delivery org header.", "NZAKL", consignment.PortOfDestination);

			deliveryOrg.OH_RL_NKClosestPort = "";
			AssertEquals("if address and header doesn't have a UNLOCO return empty string.", "", consignment.PortOfDestination);
		}

		#endregion

		#region TestJob

		public void TestJob()
		{
			var consignment = Helper.CreateConsignment();
			AssertNull(consignment.Job);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			job.JH_ParentTableCode = DtbConsignmentSchema.Constants.Prefix;
			AssertNotNull(consignment.Job);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var consignment = Factory.New<DtbConsignment>();
			AssertEquals("Land Transport Consignment", consignment.HumanReadableName);

			consignment.LTC_JobID = "CN0001";
			AssertEquals("Land Transport Consignment CN0001", consignment.HumanReadableName);
		}

		#endregion

		#region TestVariations

		public void TestVariations()
		{
			var consignment1 = Helper.CreateConsignment();
			AssertNotNull(consignment1.Variations);
			AssertEquals(0, consignment1.Variations.Count);

			var consignmentAddress1 = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.PickUp);
			var action1 = Helper.CreateConsignmentAction(consignmentAddress1, ActionTypes.Codes.PickUp);
			var action2 = Helper.CreateConsignmentAction(consignmentAddress1, ActionTypes.Codes.PickUp);
			var package1 = Helper.CreatePackage(consignment1, 1, 2, 3);
			var package2 = Helper.CreatePackage(consignment1, 1, 2, 3);
			var divot1 = Helper.CreatePackageDivot(action1, package1);
			var divot2 = Helper.CreatePackageDivot(action2, package2);
			var variation1 = Helper.CreateVariation(consignment1, divot1);
			var variation2 = Helper.CreateVariation(consignment1, divot2);

			AssertEquals(2, consignment1.Variations.Count);
			AssertContainsExactElementsInAnyOrder(new[] { variation1, variation2 }, consignment1.Variations);

			var consignment2 = Helper.CreateConsignment();
			var consignmentAddress2 = Helper.CreateConsignmentAddress(consignment2, ConsignmentAddressTypes.Codes.PickUp);
			var action3 = Helper.CreateConsignmentAction(consignmentAddress2, ActionTypes.Codes.PickUp);
			var action4 = Helper.CreateConsignmentAction(consignmentAddress2, ActionTypes.Codes.PickUp);
			var package3 = Helper.CreatePackage(consignment2, 1, 2, 3);
			var package4 = Helper.CreatePackage(consignment2, 1, 2, 3);
			var divot3 = Helper.CreatePackageDivot(action3, package3);
			var divot4 = Helper.CreatePackageDivot(action4, package4);
			var variation3 = Helper.CreateVariation(consignment2, divot3);
			var variation4 = Helper.CreateVariation(consignment2, divot4);

			AssertEquals(2, consignment2.Variations.Count);
			AssertContainsExactElementsInAnyOrder(new[] { variation3, variation4 }, consignment2.Variations);

			AssertCollectionNotContains(variation3, consignment1.Variations);
			AssertCollectionNotContains(variation4, consignment1.Variations);
		}

		#endregion

		#region TestOriginalConsignor

		public void TestOriginalConsignor()
		{
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var consignorAddress = consignment.DocAddresses.AddNew(DocAddressType.ConsignorAddress);

			AssertEquals(consignorAddress, consignment.OriginalConsignor);
		}

		#endregion

		#region FinalConsignee

		public void TestFinalConsignee()
		{
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var consigneeAddress = consignment.DocAddresses.AddNew(DocAddressType.ConsigneeAddress);

			AssertEquals(consigneeAddress, consignment.FinalConsignee);
		}

		#endregion

		#region TestJobType

		public void TestJobTypeIsBlank()
		{
			var consignment = Helper.CreateConsignment();
			AssertEquals("", consignment.LTC_JobType);
		}

		public void TestJobTypeIsLTL_WhenSavedWithoutAPackage()
		{
			var consignment = Helper.CreateConsignment();
			Factory.Save();
			AssertEquals("LTL", consignment.LTC_JobType);
		}

		public void TestJobTypeIsFCL()
		{
			var consignment = Helper.CreateConsignment();
			AssertEquals("", consignment.LTC_JobType);

			AddPackage(consignment, Constants.PkgUnit.Container, 30, 31, Constants.Weight.Kilograms, 32, Constants.Volume.CubicMetres);
			AddPackage(consignment, Constants.PkgUnit.Package, 30, 31, Constants.Weight.Kilograms, 32, Constants.Volume.CubicMetres);
			Factory.Save();
			AssertEquals("FCL", consignment.LTC_JobType);
		}

		public void TestJobTypeIsLTL()
		{
			var consignment = Helper.CreateConsignment();
			AssertEquals("", consignment.LTC_JobType);
			AddPackage(consignment, Constants.PkgUnit.Package, 30, 31, Constants.Weight.Kilograms, 32, Constants.Volume.CubicMetres);
			Factory.Save();
			AssertEquals("LTL", consignment.LTC_JobType);
		}

		#endregion

		#region Deactivation

		public void TestCanCancel_WithNoJobHeader_ShouldReturnEmptyString()
		{
			var consignment = Helper.CreateConsignment();
			AssertNull(consignment.Job);

			Factory.Save();

			AssertEquals(string.Empty, consignment.CanCancel());
		}

		public void TestCanCancel_WithJobHeaderAndNoCharges_ShouldReturnEmptyString()
		{
			var consignment = Helper.CreateConsignment();
			var job = (Job)Helper.CreateJobHeaderForConsignment(consignment);
			AssertContainsExactElementsInAnyOrder(Array.Empty<Charge>(), job.Charges);

			Factory.Save();

			AssertEquals(string.Empty, consignment.CanCancel());
		}

		public void TestCanCancel_WithJobHeaderAndChargesThatAreZero_ShouldReturnEmptyString()
		{
			var consignment = Helper.CreateConsignment();
			var job = (Job)Helper.CreateJobHeaderForConsignment(consignment);
			var charge = job.Charges.AddNew();
			charge.FillWithValidTestData();
			AssertEquals(false, charge.HasCostAmount);

			Factory.Save();

			AssertEquals(string.Empty, consignment.CanCancel());
		}

		public void TestCanCancel_WithJobHeaderAndChargeGreaterThanZero_ShouldReturnReason()
		{
			var consignment = Helper.CreateConsignment();
			var job = (Job)Helper.CreateJobHeaderForConsignment(consignment);
			var charge = job.Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_OSCostAmt = 1;

			Factory.Save();

			AssertMultilineASCIIEquals($"""
			                            Land Transport Consignment LTC001 cannot be deactivated.
			                            Job Invoicing Charge(s) have been saved against this Invoicing Job Header ({job.JH_JobNum}) in the company EDI.
			                            """, consignment.CanCancel());
		}

		[TestDate(1991, 7, 3, 1, 0, 0)]
		public void TestIsCancelled_ServiceCancelledEventAdded()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			AssertEquals("Precondition: Should not have log entries", 0, consignment.Logs.DatabaseCount);
			consignment.LTC_IsActive = false;
			AssertEquals("Should have log entry for service cancelled", 1, consignment.Logs.GetAllLogs().Count);
			var logEntry = consignment.Logs.MostRecentLog;
			CombineAssertions("Should log service cancelled", () =>
			{
				AssertEquals("Should be of type ServiceCancelled", AutoEvents.ServiceCancelledCode, logEntry.SL_SE_NKEvent);
				AssertEquals("Should not be an estimate", false, logEntry.IsEstimate);
				AssertEquals("Should have event time", ZDateTimeOffset.Now, logEntry.EventTimeOffset);
				AssertEquals("Should have log reference", consignment.LTC_JobID, logEntry.ReferenceFreeText);
				AssertEquals("Should have event reference parameter type of transport", Constants.EventReferenceParameterTypes.Transport, logEntry.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			});
		}

		public void TestIsCancelledOrIsActivated_BookingStatusChanged()
		{
			var booking = Helper.CreateBookingWithConsolidation();
			var consignment = Helper.CreateConsignment();
			consignment.LTC_KM_Booking = booking.PK;
			Factory.Save();
			booking.UpdateStatus();
			AssertEquals("Precondition: Booking has status service commenced", BookingStatuses.Codes.ServiceCommenced, booking.KM_Status);
			Factory.Save();

			consignment.LTC_IsActive = false;
			Factory.Save();
			AssertEquals("Booking should have status updated to available after first cancellation", BookingStatuses.Codes.Available, booking.KM_Status);

			consignment.LTC_IsActive = true;
			Factory.Save();
			AssertEquals("Booking should have status updated to service commenced after first activation", BookingStatuses.Codes.ServiceCommenced, booking.KM_Status);

			consignment.LTC_IsActive = false;
			Factory.Save();
			AssertEquals("Booking should have status updated to available after second cancellation", BookingStatuses.Codes.Available, booking.KM_Status);

			consignment.LTC_IsActive = true;
			Factory.Save();
			AssertEquals("Booking should have status updated to service commenced after second activation", BookingStatuses.Codes.ServiceCommenced, booking.KM_Status);
		}

		#endregion

		#region Activation

		[TestDate(1991, 7, 3, 1, 0, 0)]
		public void TestIsActivated_LogServicesCommenced()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			consignment.LTC_IsActive = false;
			AssertEquals("Precondition: Should have log entry for service cancelled", 1, consignment.Logs.GetAllLogs().Count);
			consignment.LTC_IsActive = true;
			AssertEquals("Should have log entry for services commenced", 2, consignment.Logs.GetAllLogs().Count);
			var logEntry = consignment.Logs.MostRecentLogByEventTime(AutoEvents.ServiceCommenced);
			CombineAssertions("Should log services commenced", () =>
			{
				AssertEquals("Should be of type ServiceCommenced", AutoEvents.ServiceCommencedCode, logEntry.SL_SE_NKEvent);
				AssertEquals("Should not be an estimate", false, logEntry.IsEstimate);
				AssertEquals("Should have event time", ZDateTimeOffset.Now, logEntry.EventTimeOffset);
				AssertEquals("Should have log reference", consignment.LTC_JobID, logEntry.ReferenceFreeText);
				AssertEquals("Should have event reference parameter type of transport", Constants.EventReferenceParameterTypes.Transport, logEntry.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			});
		}

		public void TestIsActivated_LogServicesCommenced_DoesNotCreateRedundantEntries()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			AssertEquals("Precondition: LTC_IsActive starts off true", true, consignment.LTC_IsActive);
			consignment.LTC_IsActive = true;
			AssertEquals("Should not create a redundant entry", 0, consignment.Logs.GetAllLogs().Count);
		}

		#endregion Activation

		#region IJobInvoicingPlugIn Members

		#region TestIJobInvoicingPlugIn

		public void TestIJobInvoicingPlugIn()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			IJobInvoicingPlugIn invoicingPlugIn = consignment;
			AssertEquals(typeof(DtbConsignmentInvoicingSupporter), invoicingPlugIn.InvoicingSupporter.GetType());
		}

		#endregion

		#region TestOnJobCreating_LinkJobToParentJob

		public void TestOnJobCreating_LinkJobToParentJob()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			using (var dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider())
			using (var transportBookingTestCache = TransportBookingTestCache.Instance)
			{
				var tbHelper = new TransportBookingTestHelper(Factory);

				var dummy = Factory.New<DummyWithDtbBooking>();
				var bookingWithParent = tbHelper.CreateConsolidation(dummy).Bookings.AddNew();
				var dummyJob = new JobHeader.Loader(dummy).TryLoadOrCreate();

				var consignment = Helper.CreateConsignment("LTC001");
				consignment.LTC_KM_Booking = bookingWithParent.PK;

				Factory.Save();

				AssertNull("consignmentJob should be null before act", consignment.Job);

				var job = new JobHeader.Loader(consignment).TryLoadOrCreate();

				AssertNotNull(consignment.Job);
				AssertEquals(dummyJob.PK, job.JH_JH_ParentJob);
			}
		}

		public void TestOnJobCreating_WhenTransportBookingIsNull()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			consignment.LTC_KM_Booking = Guid.NewGuid();

			AssertNull("consignmentJob should be null before act", consignment.Job);
			new JobHeader.Loader(consignment).TryLoadOrCreate();

			AssertNotNull(consignment.Job);
		}

		public void TestOnJobCreating_WhenLTC_KM_BookingIsEmpty()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			consignment.LTC_KM_Booking = Guid.Empty;

			Factory.Save();

			AssertNull("consignmentJob should be null before act", consignment.Job);
			new JobHeader.Loader(consignment).TryLoadOrCreate();

			AssertNotNull(consignment.Job);
		}

		public void TestOnJobCreating_WhenTransportBookingJobIsNull()
		{
			var consignment = Helper.CreateConsignment("LTC001");

			var transportBooking = Factory.NewWithValidTestData<DtbBooking>();
			consignment.LTC_KM_Booking = transportBooking.PK;

			var transportConsol = Factory.New<DtbBookingConsolidation>();
			transportBooking.KM_KB_Booking = transportConsol.PK;

			Factory.Save();

			AssertNull("consignmentJob should be null before act", consignment.Job);
			new JobHeader.Loader(consignment).TryLoadOrCreate();

			AssertNotNull(consignment.Job);
		}

		#endregion

		#region IJobHeaderParent Members

		#region TestIJobHeaderParent

		public void TestIJobHeaderParent()
		{
			IJobHeaderParent parent = Helper.CreateConsignment("LT001");
			AssertEquals(true, parent.AllowInvoiceDeletion);
			AssertEquals("LT001", parent.JobNumber);
			AssertNoExceptionThrown(() => parent.OnJobCreating(null));
			AssertNoExceptionThrown(() => parent.OnJobDeleting(null));

			parent.SetJobNumberFieldOnSaving();
			AssertNotEquals("", parent.JobNumber);
		}

		#endregion

		#region JobCreatedEventHandler

		public void JobCreatedEventHandler()
		{
			var consignment = Helper.CreateConsignment("LT001");
			bool isJobCreatedEventCalled = false;
			consignment.JobCreated += delegate
			{ isJobCreatedEventCalled = true; };
			AssertEquals("Precondition", false, isJobCreatedEventCalled);

			((IJobHeaderParent)consignment).OnJobCreated(null);
			AssertEquals(true, isJobCreatedEventCalled);
		}

		#endregion

		#endregion

		#region IJobHeaderParentCore Members

		public void TestIJobHeaderParentCore()
		{
			var consignment = Helper.CreateConsignment("LT001");
			var parentCore = consignment as IJobHeaderParentCore;
			AssertEquals(consignment.PK, parentCore.PK);
			AssertEquals(consignment.TableName, parentCore.TableName);
			AssertEquals(consignment.Factory, parentCore.Factory);
		}

		#endregion

		#endregion

		#region IDtbConsignment Members

		#region JobHeaderPK

		public void TestJobHeaderPK()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			AssertEquals(ZGuid.Empty, ((IDtbConsignment)consignment).JobHeaderPK);

			var job = new JobHeader.Loader(consignment).TryLoadOrCreate();
			AssertNotNull(consignment.Job);
			AssertEquals(consignment.Job.PK, ((IDtbConsignment)consignment).JobHeaderPK);
		}

		#endregion

		#endregion

		#region IEDocsProvider Members

		public void TestIEDocsProvider_GetEDocsProviderSupporter()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iConsignmentEDocsProvider = (IEDocsProvider)consignment;
			AssertNotNull(iConsignmentEDocsProvider.GetEDocsProviderSupporter());
			AssertEquals(typeof(JobInvoicingEDocsProviderSupporter), iConsignmentEDocsProvider.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region IDocManagerSupport Members

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			var consignment = Helper.CreateConsignment();
			var iConsignmentDocManager = (IDocManagerSupport)consignment;
			AssertNotNull(iConsignmentDocManager.DocManagerInfo);
			AssertEquals(typeof(DtbConsignmentDocManagerInfo), iConsignmentDocManager.DocManagerInfo.GetType());
			AssertEquals(Constants.DocManagerCodes.LandTransportConsignment, iConsignmentDocManager.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region IDocumentSupportable Members

		public void TestIDocumentSupportable_DocumentSupporter()
		{
			var consignment = Helper.CreateConsignment();
			var iConsignmentDocumentSupportable = (IDocumentSupportable)consignment;
			AssertNotNull(iConsignmentDocumentSupportable.DocumentSupporter);
			AssertEquals(typeof(DtbConsignmentDocumentSupporter), iConsignmentDocumentSupportable.DocumentSupporter.GetType());
		}

		#endregion

		#region IHaveServices Members

		#region TestServices

		public void TestServices()
		{
			var service = Factory.New<ConsignmentJobService>();
			var dtbConsignment = Factory.New<DtbConsignment>();
			service.ES_ParentID = dtbConsignment.PK;
			service.ES_ParentTableCode = dtbConsignment.TablePrefix;

			AssertCollectionContains("Collection was not loaded and/or the relationship filter is incorrect.", service, dtbConsignment.Services);
			AssertEquals(typeof(ConsignmentJobService), dtbConsignment.Services.TypeOfElements);
			AssertEquals(false, dtbConsignment.IsRegisteredEditableChildObject(dtbConsignment.Services));
		}

		public void TestServiceBranch()
		{
			var consignment = Factory.New<DtbConsignment>();
			var iHaveServices = (IHaveServices)consignment;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			consignment.LTC_GB_Branch = branch.PK;
			AssertEquals("Service branch", branch.PK, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#endregion

		#region IPackingParent Members

		#region TestIPackingParent

		public void TestIPackingParent()
		{
			var consignment = Helper.CreateConsignment();
			IPackingParent packingParent = consignment;
			Factory.Save();

			AssertEquals(consignment.Factory, packingParent.Factory);
			AssertEquals(consignment.PK, packingParent.PK);
			AssertEquals(consignment.TablePrefix, packingParent.TablePrefix);
			AssertEquals(ControllerIDs.DtbConsignment, packingParent.ControllerID);
			AssertEquals("Consignment", packingParent.JobDescription);
			AssertEquals(consignment.LTC_JobID, packingParent.JobNo);
			AssertEquals(consignment.LTC_ConnoteNumber, packingParent.ConnoteNo);
			AssertEquals(DocumentOptions.None, packingParent.DocumentOptions);
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
			AssertEquals(false, packingParent.IsParentJobFinalised);
			AssertEquals(false, packingParent.IsReadOnly);
			AssertEquals(false, packingParent.IsPackingJobReadOnly);
			AssertEquals(true, packingParent.IsScanEventsVisible);
			AssertEquals(PackageSequenceType.Outer, packingParent.PackageSequenceType);
			AssertNull(packingParent.CarrierBookingAgent);
		}

		#endregion

		#region TestIPackingParentSupportsImportingBookedDimensions

		public void TestIPackingParentSupportsImportingBookedDimensions()
		{
			var consignment = Factory.New<DtbConsignment>();
			AssertNotNull("Dtb Consignment support importing Booked Dimensions.", consignment);
		}

		#endregion

		#region TestIPackingParent_ShouldPackTrackedPackagesViaDivot

		public void TestIPackingParent_ShouldPackTrackedPackagesViaDivot()
		{
			var consignment = Factory.New<DtbConsignment>();
			IPackingParent packingParent = consignment;
			AssertEquals(false, packingParent.ShouldPackTrackedPackagesViaDivot);
		}

		#endregion

		#endregion

		#region IJobNumber Members

		public void TestIJobNumberMembers()
		{
			IJobNumber consignment = Helper.CreateConsignment("LT001");
			AssertEquals("LT001", consignment.JobNumber);
		}

		#endregion

		#region IWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			IWorkflowProvider consignment = Helper.CreateConsignment("LT001");
			var workflowItem = consignment.WorkflowItems.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { workflowItem }, consignment.WorkflowItems);
			AssertEquals(WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode, consignment.WorkflowType);
			AssertNull(consignment.GetWorkflowInformationProvider());
			AssertNotNull(consignment.GetTemplateSelectionCriteria());
		}

		#endregion

		#region IRatingSupporter Members

		public void TestIRatingSupporter()
		{
			IRatingSupporter ratingSupporter = Factory.New<DtbConsignment>();
			AssertEquals(typeof(DtbConsignmentRatingAdaptersProvider), ratingSupporter.AdaptersProvider.GetType());
		}

		#endregion

		#region Notes

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var consignment = Helper.CreateConsignment();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation,
				PredefinedNoteTypes.Instance.HandlingInstructions,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes
			}, consignment.NoteTypes);
		}

		#endregion

		#region TestNoteContextsForRelatedNotes

		public void TestNoteContextsForRelatedNotes_Module()
		{
			var consignment = Helper.CreateConsignment();
			var noteContexts = consignment.GetNoteContextsForRelatedNotesForTest();
			AssertEquals("Cartage should load notes for 'Transport' module only.", StmNoteContextModule.A | StmNoteContextModule.T, noteContexts.Module);
		}

		public void TestNoteContextsForRelatedNotes_Direction()
		{
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Export, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Origin, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Import, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Destination, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.LineHaul, false, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Local, false, false);
			CreateAndAssertNoteContextDirection("", false, false);
		}

		void CreateAndAssertNoteContextDirection(ZString direction, bool hasExport, bool hasImport)
		{
			var consignment = Helper.CreateConsignment();
			consignment.LTC_Direction = direction;
			var noteContexts = consignment.GetNoteContextsForRelatedNotesForTest();
			AssertEquals("Direction: All", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.A));
			AssertEquals("Direction: Import", hasImport, noteContexts.Direction.HasFlag(StmNoteContextDirection.I));
			AssertEquals("Direction: Export", hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.E));
			AssertEquals("Direction: Import and Export", hasImport || hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.B));
			AssertEquals("Direction: Domestic", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.D));
			AssertEquals("Direction: Cross Trade", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.X));
			AssertEquals("Direction: All Forwarding", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.F));
			AssertEquals("Direction: Other / Warehouse Out", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.O));
			AssertEquals("Direction: Warehouse In", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.R));
		}

		#endregion

		#endregion

		#region TestIsPickUpDirection

		public void TestIsPickUpDirection()
		{
			AssertFlag("IsPickUpDirection", DtbConsignmentSchema.LTC_Direction, Constants.CartageDirection.Origin, Constants.CartageDirection.Destination);
			AssertFlag("IsPickUpDirection", DtbConsignmentSchema.LTC_Direction, Constants.CartageDirection.Export, Constants.CartageDirection.Import);
		}

		#endregion

		#region TestIsDeliveryDirection

		public void TestIsDeliveryDirection()
		{
			AssertFlag("IsDeliveryDirection", DtbConsignmentSchema.LTC_Direction, Constants.CartageDirection.Destination, Constants.CartageDirection.Origin);
			AssertFlag("IsDeliveryDirection", DtbConsignmentSchema.LTC_Direction, Constants.CartageDirection.Import, Constants.CartageDirection.Export);
		}

		#endregion

		#region TestSystemHeld
		public void TestDtbConsignment_WhenNoPackage_ShouldSystemHeldBeTrue()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidPICAddress_WithAddressOverrideAndNoAddress1_ShouldSystemHeldBeTrue()
		{
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var consignment = Helper.CreateConsignment();
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			picAddress.Address.E2_AddressOverride = true;
			picAddress.Address.E2_RN_NKCountryCode = "AU";
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidPICAddress_WithAddressOverrideAndNoCountryCode_ShouldSystemHeldBeTrue()
		{
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var consignment = Helper.CreateConsignment();
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			picAddress.Address.E2_AddressOverride = true;
			picAddress.Address.Address1 = "123 Test ave";
			picAddress.Address.E2_RN_NKCountryCode = "";
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidPICAddress_WithAddressOverrideAndNoAddress1AndCountryCode_ShouldSystemHeldBeTrue()
		{
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var consignment = Helper.CreateConsignment();
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			picAddress.Address.E2_AddressOverride = true;
			picAddress.Address.Address1 = "";
			picAddress.Address.E2_RN_NKCountryCode = "";
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidPICAddress_WithAddressOverrideFalseAndNoOrgAddress_ShouldSystemHeldBeTrue()
		{
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var consignment = Helper.CreateConsignment();
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			picAddress.Address.E2_AddressOverride = false;
			picAddress.Address.E2_OA_Address = ZGuid.Empty;
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidDLVAddress_WithAddressOverrideAndNoAddress1_ShouldSystemHeldBeTrue()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			dlvAddress.Address.E2_AddressOverride = true;
			dlvAddress.Address.E2_RN_NKCountryCode = "AU";
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidDLVAddress_WithAddressOverrideAndNoCountryCode_ShouldSystemHeldBeTrue()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			dlvAddress.Address.E2_AddressOverride = true;
			dlvAddress.Address.Address1 = "234 test ave";
			dlvAddress.Address.E2_RN_NKCountryCode = "";
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidDLVAddress_WithAddressOverrideAndNoAddress1AndCountryCode_ShouldSystemHeldBeTrue()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			dlvAddress.Address.E2_AddressOverride = true;
			dlvAddress.Address.Address1 = "";
			dlvAddress.Address.E2_RN_NKCountryCode = "";
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenNoValidDLVAddress_WithAddressOverrideFalseAndNoOrgAddress_ShouldSystemHeldBeTrue()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			dlvAddress.Address.E2_AddressOverride = false;
			dlvAddress.Address.E2_OA_Address = ZGuid.Empty;
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(true, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenPackageAndOrgAddressesExists_ShouldSystemHeldBeFalse()
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			var consignment = Helper.CreateConsignment();
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(false, consignment.SystemHeld);
		}
		public void TestDtbConsignment_WhenPackageAndAddressesExists_ShouldSystemHeldBeFalse()
		{
			var consignment = Helper.CreateConsignment();
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			picAddress.Address.E2_AddressOverride = true;
			picAddress.Address.Address1 = "1 test street";
			picAddress.Address.E2_RN_NKCountryCode = "NZ";
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			dlvAddress.Address.E2_AddressOverride = true;
			dlvAddress.Address.Address1 = "123 demo ave";
			dlvAddress.Address.E2_RN_NKCountryCode = "AU";
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
			AssertEquals(false, consignment.SystemHeld);
		}

		public void TestDtbConsignment_WhenMandatoryFieldsBillToPartyIsEmpty_ShouldSystemHeldBeTrue()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.BillToParty, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				var billToPartyAddressesToDelete = consignment.DocAddresses.FindDocAddressesByType(DocAddressType.ClientRequestedBillingParty);
				billToPartyAddressesToDelete.ForEach(address => consignment.DocAddresses.Remove(address));
				consignment.Factory.Save();

				AssertEquals(true, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsBillToPartyIsInvalid_ShouldSystemHeldBeTrue()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.BillToParty, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				var billToPartyAddress = consignment.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
				billToPartyAddress.E2_OA_Address = Guid.Empty;
				billToPartyAddress.E2_AddressOverride = false;
				consignment.Factory.Save();

				AssertEquals(true, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsBillToPartyExist_ShouldSystemHeldBeFalse()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.BillToParty, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				var billToPartyAddress = consignment.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
				billToPartyAddress.E2_AddressOverride = false;
				billToPartyAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consignment.Factory.Save();

				AssertEquals(billToPartyAddress, consignment.ClientRequestedBillingPartyAddress);
				AssertEquals(false, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsBookingPartyIsEmpty_ShouldSystemHeldBeTrue()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.BookingParty, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				var bookingPartyAddressesToDelete = consignment.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress);
				bookingPartyAddressesToDelete.ForEach(address => consignment.DocAddresses.Remove(address));
				consignment.Factory.Save();

				AssertEquals(true, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsBookingPartyIsInvalid_ShouldSystemHeldBeTrue()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.BookingParty, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				var bookingPartyAddress = consignment.DocAddresses.AddNew(DocAddressType.BookingPartyDocumentaryAddress);
				bookingPartyAddress.E2_OA_Address = Guid.Empty;
				bookingPartyAddress.E2_AddressOverride = false;
				consignment.Factory.Save();

				AssertEquals(true, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsBookingPartyExist_ShouldSystemHeldBeFalse()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.BookingParty, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				var bookingPartyAddress = consignment.DocAddresses.AddNew(DocAddressType.BookingPartyDocumentaryAddress);
				bookingPartyAddress.E2_AddressOverride = false;
				bookingPartyAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consignment.Factory.Save();

				AssertEquals(bookingPartyAddress, consignment.BookingPartyAddress);
				AssertEquals(false, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsControllingBranchIsEmpty_ShouldSystemHeldBeTrue()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.ControllingBranch, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				consignment.LTC_GB_Branch = Guid.Empty;
				consignment.Factory.Save();

				AssertEquals(true, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsControllingBranchExist_ShouldSystemHeldBeFalse()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.ControllingBranch, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				consignment.LTC_GB_Branch = GlbBranch.CurrentBranch.PK;
				consignment.Factory.Save();

				AssertEquals(false, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsServiceLevelIsEmpty_ShouldSystemHeldBeTrue()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.ServiceLevel, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				consignment.LTC_RS_NKServiceLevel = "";
				consignment.Factory.Save();

				AssertEquals(true, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsServiceLevelExist_ShouldSystemHeldBeFalse()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.ServiceLevel, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				consignment.LTC_RS_NKServiceLevel = "STD";
				consignment.Factory.Save();

				AssertEquals(false, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsConsignmentNoteIsEmpty_ShouldSystemHeldBeTrue()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.ConsignmentNote, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				consignment.LTC_ConnoteNumber = "";
				consignment.Factory.Save();

				AssertEquals(true, consignment.SystemHeld);
			}
		}

		public void TestDtbConsignment_WhenMandatoryFieldsConsignmentNoteExist_ShouldSystemHeldBeFalse()
		{
			var mandatoryCollection = LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.Value;
			mandatoryCollection.Set(LandTransportRegistry.ConsignmentMandatoryFieldOptions.ConsignmentNote, true);

			using (LandTransportRegistry.Instance.LandTransportConsignmentMandatoryFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryCollection))
			{
				var consignment = Helper.CreateConsignment();
				AddPackagesAndOrgAddressesToConsignment(consignment);
				consignment.LTC_ConnoteNumber = "Some consignment note";
				consignment.Factory.Save();

				AssertEquals(false, consignment.SystemHeld);
			}
		}

		#endregion

		#region AssertFlag

		void AssertFlag(ZString flagPropertyName, SchemaColumn codeColumn, ZString validCode, ZString invalidCode)
		{
			var consignment = Helper.CreateConsignment();

			consignment[codeColumn.Name] = "";
			AssertEquals(false, consignment[flagPropertyName]);

			consignment[codeColumn.Name] = validCode;
			AssertEquals(true, consignment[flagPropertyName]);

			consignment[codeColumn.Name] = invalidCode;
			AssertEquals(false, consignment[flagPropertyName]);
		}

		#endregion

		#region IDocAddresses Members

		#region TestIDocAddresses_CanDeleteAddress

		public void TestIDocAddresses_CanDeleteAddress()
		{
			IDocAddresses consignment = Helper.CreateConsignment("LT001");
			AssertEquals(false, consignment.CanDeleteAddress(null));
		}

		#endregion

		#region TestIDocAddresses_DocAddresses

		public void TestIDocAddresses_DocAddresses()
		{
			var consignment = Helper.CreateConsignment("LT001");
			AssertEquals(typeof(JobDocAddressDependentCollection), consignment.DocAddresses.GetType());
			AssertEquals(true, consignment.IsRegisteredEditableChildObject(consignment.DocAddresses));
		}

		#endregion

		#region TestIDocAddresses_GetCanOverrideCheckpoint

		public void TestIDocAddresses_GetCanOverrideCheckpoint()
		{
			IDocAddresses consignment = Helper.CreateConsignment("LT001");
			AssertEquals(Env.Security.None, consignment.GetCanOverrideCheckpoint(null));
		}

		#endregion

		#region TestIDocAddresses_GetDocAddressRequirement

		public void TestIDocAddresses_GetDocAddressRequirement()
		{
			IDocAddresses consignment = Helper.CreateConsignment("LT001");
			var requirement = consignment.GetDocAddressRequirement(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull(requirement);
			AssertEquals(false, requirement.IsMandatory);
		}

		#endregion

		#region TestIDocAddresses_GetOrgHeaderList

		public void TestIDocAddresses_GetOrgHeaderList()
		{
			IDocAddresses consignment = Helper.CreateConsignment("LT001");
			AssertEquals(typeof(DebtorCollection), consignment.GetOrgHeaderList(DocAddressType.ClientRequestedBillingParty).GetType());
		}

		#endregion

		#region TestIDocAddresses_SupportedAddressTypes

		public void TestIDocAddresses_SupportedAddressTypes()
		{
			IDocAddresses consignment = Helper.CreateConsignment("LT001");
			AssertContainsExactElementsInAnyOrder(ExpectedSupportedDocAddressTypes, consignment.SupportedAddressTypes);
		}

		DocAddressType[] ExpectedSupportedDocAddressTypes
		{
			get
			{
				return new[]
				{
					DocAddressType.BookingPartyDocumentaryAddress,
					DocAddressType.NotifyParty,
					DocAddressType.ClientRequestedBillingParty,
					DocAddressType.FinalConsigneeAddress,
					DocAddressType.OriginatingConsignorAddress
				};
			}
		}

		#endregion

		#endregion

		#region IAdditionalReferenceNumberTypeProvider

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var consignment = Helper.CreateConsignment("LT001");

			var additionalReferenceNumberLookups = consignment.AdditionalReferenceNumbers.AddNew().Lookups;
			var additionalReferenceNumberTypes = additionalReferenceNumberLookups.GetType().GetProperty("AdditionalReferenceNumberTypes").GetValue(additionalReferenceNumberLookups, null);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"ETB|External Transport Booking Number|Y",
					"TRF|Transport Reference Number|Y",
					"CIN|Commercial Invoice Number|Y",
					"CLR|Customer Reference Number|Y",
					"HSB|House Bill|Y",
					"MAB|Master Bill|N",
					"ORD|Order Number|N",
					"BPR|Booking Party Reference|Y",
					"UCR|External (3rd Party) Unique Consignment Reference|N",
				},
				((CodeDescriptionPairList)additionalReferenceNumberTypes)
				.Cast<TransportReferenceNumberType>()
				.Select((number) => string.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		#endregion

		#region IRelatedJob Memers

		public void TestIRelatedJob()
		{
			var consignment = Helper.CreateConsignment();
			var relatedJob = (IRelatedJob)consignment;
			Factory.Save();
			AssertEquals("Precondition", false, consignment.LTC_JobID.IsEmpty);
			AssertEquals("Precondition", false, consignment.LTC_Status.IsEmpty);

			AssertEquals(ControllerIDs.DtbConsignment, relatedJob.ControllerID);
			AssertEquals(consignment.PK.ToGuid(), relatedJob.BusinessObjectPK);
			AssertEquals(string.Format("Land Transport Consignment {0}", consignment.LTC_JobID), relatedJob.JobDescription);
			AssertEquals(consignment.LTC_JobID, relatedJob.JobNumber);
		}

		public void TestIRelatedJobStatus()
		{
			var consignment1 = Helper.CreateConsignment("LTC001", ConsignmentStatuses.Codes.Booked);
			var relatedJob1 = (IRelatedJob)consignment1;
			consignment1.LTC_IsActive = false;

			AssertEquals("Deactivated", relatedJob1.JobStatus);

			var consignment2 = Helper.CreateConsignment("LTC002", ConsignmentStatuses.Codes.Booked);
			var relatedJob2 = (IRelatedJob)consignment2;

			AssertEquals("Booked", relatedJob2.JobStatus);

			var consignment3 = Helper.CreateConsignment("LTC003", ConsignmentStatuses.Codes.Commenced);
			var relatedJob3 = (IRelatedJob)consignment3;

			AssertEquals("Commenced", relatedJob3.JobStatus);

			var consignment4 = Helper.CreateConsignment("LTC004", ConsignmentStatuses.Codes.Completed);
			var relatedJob4 = (IRelatedJob)consignment4;

			AssertEquals("Completed", relatedJob4.JobStatus);

			var consignment5 = Helper.CreateConsignment("LTC005", ConsignmentStatuses.Codes.New);
			var relatedJob5 = (IRelatedJob)consignment5;

			AssertEquals("New", relatedJob5.JobStatus);
		}

		#endregion

		#region Implementations

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Helper.CreateConsignment();
		}

		#endregion

		#region TestSave_PopulateUnqiueIDIfNeeded

		public void TestSave_PopulateUnqiueIDIfNeeded()
		{
			var consignment = Helper.CreateConsignment(Constants.CartageDirection.Local, TransportStatuses.Codes.Booked, 1, "KM", "");
			AssertEquals("Precondition", "", consignment.LTC_JobID);

			Factory.Save();
			AssertEquals("CN00000001", consignment.LTC_JobID);
		}

		#endregion

		#region Test_BusinessObjectsWithRelatedEventsCore

		public void TestBusinessObjectsWithRelatedEventsCore_ShouldFindConsignmentAddresses()
		{
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var depotAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			Factory.Save();

			var relatedObjects = consignment.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder(new[] { pickupAddress, depotAddress, deliveryAddress }, relatedObjects);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_ShouldFindConsignmentActions()
		{
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			Factory.Save();

			var relatedObjects = consignment.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { pickupAddress, deliveryAddress, pickupAction, deliveryAction }, relatedObjects);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_ShouldFindPackages()
		{
			var consignment = Helper.CreateConsignment();
			var package1 = Helper.CreatePackage(consignment, 1, 2, 3);
			var package2 = Helper.CreatePackage(consignment, 4, 5, 6);
			Factory.Save();

			var relatedObjects = consignment.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, relatedObjects);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenConsignmentIsAllocatedToRunSheet_ShouldFindRunSheetInstructions()
		{
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);
			var leg = Helper.CreateConsignmentLeg(consignment, pickupAction, deliveryAction);
			leg.LTG_Sequence = 1;

			var runSheet = Helper.CreateRunSheet();
			var pickupInstruction = Helper.CreateRunSheetInstruction(pickupAction, runSheet.PK);
			var deliveryInstruction = Helper.CreateRunSheetInstruction(deliveryAction, runSheet.PK);

			Factory.Save();

			var relatedObjects = consignment.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { pickupAddress, deliveryAddress, pickupAction, deliveryAction, pickupInstruction, deliveryInstruction }, relatedObjects);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenConsignmentHasJobHeader_ShouldFindJobHeader()
		{
			var consignment = Helper.CreateConsignment();
			var jobHeader = Helper.CreateJobHeader(consignment);
			Factory.Save();

			AssertEquals("Expected JobHeader in business objects.", jobHeader.PK, consignment.BusinessObjectsWithRelatedEvents.Single().PK);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenConsignmentHasBooking_ShouldFindBooking()
		{
			var consignment = Helper.CreateConsignment();
			var booking = Helper.CreateBookingWithConsolidation();
			consignment.LTC_KM_Booking = booking.PK;
			Factory.Save();

			AssertEquals("Expected Booking in business objects.", booking.PK, consignment.BusinessObjectsWithRelatedEvents.Single().PK);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenConsignmentHasBookingShipment_ShouldFindBookingAndShipment()
		{
			var consignment = Helper.CreateConsignment();
			var shipment = Helper.CreateForwardingShipment();
			var consolidation = Helper.CreateConsolidation(shipment);
			var booking = Helper.CreateBooking(consolidation);
			consignment.LTC_KM_Booking = booking.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Expected exact booking and shipment records in business objects.", new[] { booking.PK.ToString(), shipment.PK.ToString() }, consignment.BusinessObjectsWithRelatedEvents.Select(o => o.PK.ToString()));
		}

		public void TestBusinessObjectsWithRelatedEventsCore_ShouldFindVariations()
		{
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			var package = consignment.PackageJob.Packages.AddNew();
			package.KP_PackageQty = 1;
			var divot = Factory.New<DtbConsignmentActionPackageDivot>();
			divot.LTP_LTA_ConsignmentAction = pickupAction.PK;
			divot.LTP_KP_Package = package.PK;
			divot.LTP_PackageQuantity = 1;

			var variation = helper.CreateVariation(consignment, divot);
			Factory.Save();

			var relatedObjects = consignment.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { pickupAddress, deliveryAddress, pickupAction, deliveryAction, package, variation }, relatedObjects);
		}

		#endregion

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.DtbConsignment);
			}
		}

		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		void AddPackagesAndOrgAddressesToConsignment(DtbConsignment consignment)
		{
			var pickupOrg = Helper.CreateOrganisation("O1", "AUSYD");
			var deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			var pickupAddress = Helper.CreateOrgAddress(pickupOrg, "AUBNE");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupAddress);
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			helper.CreatePackage(consignment, "PackageID", "Box");
			consignment.Factory.Save();
		}
		#endregion

		#region IJobInvoicingAdditionalData Members

		public void TestGetAdditionalProperties()
		{
			var consignment = Helper.CreateConsignment();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = consignment.PK;
			job.JH_ParentTableCode = DtbConsignmentSchema.Constants.Prefix;

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;

			var additionalData = consignment as IJobInvoicingAdditionalData;
			var additionalProperties = additionalData.GetAdditionalProperties();

			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketID].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketReference].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeAddress1].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeAddress2].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeCity].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneePostCode].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeState].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeUNLOCO].GetValue(charge));
			AssertEquals("", additionalProperties[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeCode].GetValue(charge));
		}

		#endregion
	}
}
