using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentDataObjectWriterTest : DtbConsignmentUniversalTestCase
	{
		public void TestPopulateDataObject_DtbConsignment()
		{
			var consignment = Helper.CreateConsignment("STD", "LOC", "Test CN", 100.0, "AUD", 40.0D, "USD", "LTL", "INC", "AdditionalTerms");

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			AssertEquals("STD", consignmentDataObject.ServiceLevel.Code);
			AssertEquals("Test CN", consignmentDataObject.ConsignmentNote);
			AssertEquals("LOC", consignmentDataObject.Direction);
			AssertEquals(100m, consignmentDataObject.GoodsValue);
			AssertEquals("AUD", consignmentDataObject.GoodsValueCurrency.Code);
			AssertEquals(40m, consignmentDataObject.InsuranceValue);
			AssertEquals("USD", consignmentDataObject.InsuranceValueCurrency.Code);
			AssertEquals("LTL", consignmentDataObject.LocalTransportJobType.Code);
			AssertEquals("INC", consignmentDataObject.ShipmentIncoTerm.Code);
			AssertEquals("AdditionalTerms", consignmentDataObject.AdditionalTerms);
		}

		public void TestPopulateDataObject_BookingJob()
		{
			var consignment = Helper.CreateConsignment();
			var booking = Helper.CreateBookingWithConsolidation();
			consignment.LTC_KM_Booking = booking.PK;

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var bookingDataSource = consignmentDataObject.DataContext.DataSourceCollection.Single(d => d.Type.GetValueOrDefault() == nameof(DataContextType.TransportBooking));
			AssertEquals(booking.KM_JobID, bookingDataSource.Key);
		}

		public void TestPopulateDataObject_WhenNoBookingAttached_ShouldNotAddTransportBookingSource()
		{
			var consignment = Helper.CreateConsignment();

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var bookingDataSource = consignmentDataObject.DataContext.DataSourceCollection.FirstOrDefault(d => d.Type.GetValueOrDefault() == nameof(DataContextType.TransportBooking));
			AssertNull(bookingDataSource);
		}

		public void TestPopulateDataObject_LocalClient()
		{
			var consignment = Helper.CreateConsignment();
			var job = Helper.CreateJobHeaderForConsignment(consignment);
			var org = Helper.CreateOrganisation("CFS");
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var orgAddress = consignmentDataObject.OrganizationAddressCollection.Single();
			AssertEquals("SendersLocalClient", orgAddress.AddressType);
		}

		public void TestPopulateDataObject_WhenNoJobAttached_ShouldNoLocalClientOrgAddress()
		{
			var consignment = Helper.CreateConsignment();
			
			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var orgAddress = consignmentDataObject.OrganizationAddressCollection.FirstOrDefault();
			AssertNull(orgAddress);
		}

		public void TestPopulateDataObject_WhenNoJobLocalChargesAddr_ShouldNoLocalClientOrgAddress()
		{
			var consignment = Helper.CreateConsignment();
			var job = Helper.CreateJobHeaderForConsignment(consignment);

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var orgAddress = consignmentDataObject.OrganizationAddressCollection.FirstOrDefault();
			AssertNull(orgAddress);
		}

		public void TestPopulateDataObject_OrgAddress()
		{
			var org = Helper.CreateOrganisation("CFS");
			var org2 = Helper.CreateOrganisation("CSF");
			var consignment = Helper.CreateConsignment();
			consignment.DocAddresses.AddNew(org.MainAddress);
			consignment.DocAddresses.AddNew(org2.MainAddress);

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var docAddressCount = consignmentDataObject.OrganizationAddressCollection.Count;
			AssertEquals(2, docAddressCount);
		}

		public void TestPopulateDataObject_WhenNoDocAddresses_ShouoldNoOrgAddress()
		{
			var consignment = Helper.CreateConsignment();

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			AssertNull(consignmentDataObject.OrganizationAddressCollection);
		}

		public void TestPopulateDataObject_WhenLocalClientAndDocAddressExists_ShouldMergeBoth()
		{
			var consignment = Helper.CreateConsignment();
			var job = Helper.CreateJobHeaderForConsignment(consignment);
			var org = Helper.CreateOrganisation("CFS");
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
			consignment.DocAddresses.AddNew(org.MainAddress);

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var docAddressCount = consignmentDataObject.OrganizationAddressCollection.Count;
			AssertEquals(2, docAddressCount);
			consignmentDataObject.OrganizationAddressCollection.Single(orgAddr => orgAddr.AddressType.Value == "SendersLocalClient");
			consignmentDataObject.OrganizationAddressCollection.Single(orgAddr => orgAddr.AddressType.Value == "None");
		}

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var consignment = Helper.CreateConsignment("STD", "LOC", "Test CN", 100.0, "AUD", 40.0D, "USD", "LTL", "INC", "Additional Terms");
			var issueDate = DateTime.UtcNow;
			Helper.CreateAdditionalReference(consignment, issueDate, TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber, "N110", "LineRef1");
			Helper.CreateAdditionalReference(consignment, issueDate, TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "N111", "LineRef2");

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			AssertEquals(2, consignmentDataObject.AdditionalReferenceCollection.Count);
		}

		public void TestPopulateDataObject_CustomFields_Consignment()
		{
			var consignment = Helper.CreateConsignment();

			consignment.SetUserDefinedValue("Squanch", new ZString("Schwifty"));

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			var customFields = consignmentDataObject.CustomizedFieldCollection.Select(x => $"{x.DataType}: {x.Key} - {x.Value}");
			AssertContainsExactElementsInAnyOrder(new[] { "String: Squanch - Schwifty" }, customFields);
		}

		public void TestPopulateDataObject_Notes()
		{
			var consignment = Helper.CreateConsignment("STD", "LOC", "Test CN", 100.0, "AUD", 40.0D, "USD", "LTL", "INC", "Additional Terms");
			consignment.Notes.AddNew(true, "Note 1 description", "note 1 Text");
			consignment.Notes.AddNew(true, "Note 2 description", "note 2 Text");

			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);

			AssertEquals(2, consignmentDataObject.NoteCollection.Count);
		}

		#region TestPopulateDataObject_Packages_LoosePackages
		public void TestPopulateDataObject_Packages_LoosePackages()
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var package1 = Helper.CreatePackage(consignment, "P1", Constants.PkgUnit.Pallet);
			var package2 = Helper.CreatePackage(consignment, "P2", Constants.PkgUnit.Box);
			var innerPackage = Helper.CreateChildPackage(package2, "P3", Constants.PkgUnit.Roll);
			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), new DataObjectList<Container>());
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);
			AssertEquals("Two packlines must be created for each outer package.", 2, consignmentDataObject.PackingLineCollection.Count);
			var packingLineForPackage1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "P1");
			var packingLineForPackage2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "P2");
			var packingLineForPackage3 = packingLineForPackage2.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "P3");
			AssertEquals(Constants.PkgUnit.Pallet, packingLineForPackage1.PackType.Code);
			AssertEquals(Constants.PkgUnit.Box, packingLineForPackage2.PackType.Code);
			AssertEquals(Constants.PkgUnit.Roll, packingLineForPackage3.PackType.Code);
		}

		#endregion
		#region TestPopulateDataObject_Packages_ContainerisedPackages
		public void TestPopulateDataObject_Packages_ContainerisedPackages()
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var container1 = Helper.CreatePackage(consignment, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment, "CN2", Constants.PkgUnit.Container);
			Helper.CreateChildPackage(container1, "P1", Constants.PkgUnit.Pallet);
			Helper.CreateChildPackage(container2, "P2", Constants.PkgUnit.Roll);
			var packageWithChild = Helper.CreateChildPackage(container1, "P3", Constants.PkgUnit.Box);
			Helper.CreateChildPackage(packageWithChild, "P4", Constants.PkgUnit.Pallet);
			var containerList = new DataObjectList<Container>();
			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), containerList);
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);
			AssertEquals("Three packlines must be created for each inner package.", 3, consignmentDataObject.PackingLineCollection.Count);
			AssertEquals("Two containers must be created.", 2, containerList.Count);
			var containerLinkForCN1 = containerList.Single(c => c.ContainerNumber.Value == "CN1");
			var containerLinkForCN2 = containerList.Single(c => c.ContainerNumber.Value == "CN2");
			AssertPackingLine(consignmentDataObject.PackingLineCollection.ToList(), "P1", Constants.PkgUnit.Pallet, containerLinkForCN1.Link);
			AssertPackingLine(consignmentDataObject.PackingLineCollection.ToList(), "P2", Constants.PkgUnit.Roll, containerLinkForCN2.Link);
			AssertPackingLine(consignmentDataObject.PackingLineCollection.ToList(), "P3", Constants.PkgUnit.Box, containerLinkForCN1.Link);
			AssertPackingLine(consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "P3").PackingLineCollection, "P4", Constants.PkgUnit.Pallet);
		}

		void AssertPackingLine(List<PackingLine> packLines, string referencNumber, string packageUnit, ZInt? containerLink = null)
		{
			var packLine = packLines.Single(p => p.ReferenceNumber.Value == referencNumber);
			AssertEquals(packageUnit, packLine.PackType.Code);
			AssertEquals(packLine.ContainerLink, containerLink);
		}

		#endregion
		#region TestPopulateDataObject_ContainerisedPackages_StandaloneConsignments
		public void TestPopulateDataObject_ContainerisedPackages_StandaloneConsignments()
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var container1 = Helper.CreatePackage(consignment, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment, "CN2", Constants.PkgUnit.Container);
			Helper.CreateChildPackage(container1, "P1", Constants.PkgUnit.Pallet);
			Helper.CreateChildPackage(container2, "P2", Constants.PkgUnit.Roll);
			var packageWithChild = Helper.CreateChildPackage(container1, "P3", Constants.PkgUnit.Box);
			Helper.CreateChildPackage(packageWithChild, "P4", Constants.PkgUnit.Pallet);
			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);
			AssertEquals("Three packlines must be created for each inner package.", 3, consignmentDataObject.PackingLineCollection.Count);
			AssertEquals("Two containers must be created.", 2, consignmentDataObject.ContainerCollection.Count);
			var containerLinkForCN1 = consignmentDataObject.ContainerCollection.Single(c => c.ContainerNumber.Value == "CN1");
			var containerLinkForCN2 = consignmentDataObject.ContainerCollection.Single(c => c.ContainerNumber.Value == "CN2");
			AssertPackingLine(consignmentDataObject.PackingLineCollection.ToList(), "P1", Constants.PkgUnit.Pallet, containerLinkForCN1.Link);
			AssertPackingLine(consignmentDataObject.PackingLineCollection.ToList(), "P2", Constants.PkgUnit.Roll, containerLinkForCN2.Link);
			AssertPackingLine(consignmentDataObject.PackingLineCollection.ToList(), "P3", Constants.PkgUnit.Box, containerLinkForCN1.Link);
			AssertPackingLine(consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "P3").PackingLineCollection, "P4", Constants.PkgUnit.Pallet);
		}

		#endregion
		#region TestPopulateDataObject_Addresses
		public void TestPopulateDataObject_Addresses()
		{
			var pickupOrg = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var departureOrg = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, departureOrg.MainAddress);
			var containerList = new DataObjectList<Container>();
			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), containerList);
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);
			AssertEquals("Pick up and delivery addresses must be exported as instructions.", 2, consignmentDataObject.InstructionCollection.Count);
		}

		#endregion
		#region TestPopulateDataObject_DataSource
		public void TestPopulateDataObject_DataSource()
		{
			var consignment = Helper.CreateConsignment("LTC1");
			var containerList = new DataObjectList<Container>();
			var consignmentDataObjectWriter = new DtbConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consignment)), containerList);
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(consignment);
			var consignmentDataSource = consignmentDataObject.DataContext.DataSourceCollection.Single();
			AssertEquals(nameof(DataContextType.LandTransportConsignment), consignmentDataSource.Type);
			AssertEquals("LTC1", consignmentDataSource.Key);
		}
		#endregion
	}
}
