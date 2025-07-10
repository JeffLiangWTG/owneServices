using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentDocumentSupporter))]
	sealed class DtbConsignmentDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.LTConsignment, DocumentSupporter.BusinessContext);
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.DtbConsignmentCustomiseDocuments, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGetContactOrganisation

		public void TestGetContactOrganisation_WhenConsignorDocGroupSelected_AndOrgHeaderExistsOnPickupAddress_OfTypeConsignor_ShouldCreateCorrectOrgContact()
		{
			var client = Factory.New<OrgHeader>();
			client.OrganisationTypes = OrganisationTypes.Consignor;
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, client.MainAddress);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY);

			AssertNotNull(documentContact);
			AssertEquals("The contact should be the Pickup (Consignor) Org.", client.PK, documentContact.OrgHeader.PK);
		}

		public void TestGetContactOrganisation_WhenConsignorDocGroupSelected_AndOrgHeaderExistsOnPickupAddress_OfMultipleTypesIncludingConsignor_ShouldCreateCorrectOrgContact()
		{
			var client = Factory.New<OrgHeader>();
			client.OrganisationTypes = OrganisationTypes.Consignor | OrganisationTypes.Consignee | OrganisationTypes.Carrier;
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, client.MainAddress);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY);

			AssertNotNull(documentContact);
			AssertEquals("The contact should be the Pickup (Consignor) Org.", client.PK, documentContact.OrgHeader.PK);
		}

		public void TestGetContactOrganisation_WhenConsignorDocGroupSelected_AndOrgHeaderExistsOnPickupAddress_OfOtherTypeThanConsignor_ShouldNotGetOrgContact()
		{
			var client = Factory.New<OrgHeader>();
			client.OrganisationTypes = OrganisationTypes.Carrier | OrganisationTypes.Broker;
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, client.MainAddress);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY);

			AssertNull(documentContact);
		}

		public void TestGetContactOrganisation_WhenConsignorDocGroupSelected_WithNoPickupAddress_ShouldNotGetOrgContact()
		{
			var consignment = Helper.CreateConsignment();

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY);

			AssertNull(documentContact);
		}

		public void TestGetContactOrganisation_WhenConsignorDocGroupSelected_AndOrgHeaderDoesNotExist_ShouldNotGetOrgContact()
		{
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY);
			AssertNull(documentContact);
		}

		public void TestGetContactOrganisation_WhenLocalClientDocGroupSelected_AndOrgHeaderAndAddressExists_ShouldCreateCorrectOrgContact()
		{
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var consignment = Helper.CreateConsignment();
			var jobHeader = Helper.CreateJobHeader(consignment);
			var mainAddress = localClient.MainAddress;
			jobHeader.LocalChargesPK = localClient.PK;

			var orgContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY);

			AssertNotNull(orgContact);
			AssertEquals(orgContact.OrgHeader, localClient);
			AssertEquals(orgContact.OrgAddress, mainAddress);
		}

		public void TestGetContactOrganisation_WhenLocalClientDocGroupSelected_AndOrgHeaderDoesNotExist_ShouldNotGetOrgContact()
		{
			var consignment = Helper.CreateConsignment();
			Helper.CreateJobHeader(consignment);

			var orgContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY);

			AssertNull(orgContact);
		}

		public void TestGetContactOrganisation_WhenLocalClientDocGroupSelected_AndNoJobHeaderIsPresent_ShouldNotGetOrgContact()
		{
			var consignment = Helper.CreateConsignment();

			var orgContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY);

			AssertNull(orgContact);
		}

		public void TestGetContactOrganisation_WhenConsigneeDocGroupSelected_AndOrgHeaderExistsOnDeliveryAddress_OfTypeConsignee_ShouldCreateCorrectOrgContact()
		{
			var client = Factory.New<OrgHeader>();
			client.OrganisationTypes = OrganisationTypes.Consignee;
			var consignment = Helper.CreateConsignment();
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, client.MainAddress);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee, DocumentDirection.ANY);

			AssertEquals("The contact should be the Delivery (Consignee) Org.", client.PK, documentContact.OrgHeader.PK);
		}

		public void TestGetContactOrganisation_WhenConsigneeDocGroupSelected_AndOrgHeaderExistsOnDeliveryAddress_OfMultipleTypesIncludingConsignee_ShouldGetOrgContact()
		{
			var client = Factory.New<OrgHeader>();
			client.OrganisationTypes = OrganisationTypes.Broker | OrganisationTypes.Carrier | OrganisationTypes.Consignee;
			var consignment = Helper.CreateConsignment();
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, client.MainAddress);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee, DocumentDirection.ANY);

			AssertNotNull(documentContact);
			AssertEquals("The contact should be the Delivery (Consignee) Org.", client.PK, documentContact.OrgHeader.PK);
		}

		public void TestGetContactOrganisation_WhenConsigneeDocGroupSelected_AndOrgHeaderExistsOnDeliveryAddress_OfOtherTypeThanConsignee_ShouldNotGetOrgContact()
		{
			var client = Factory.New<OrgHeader>();
			client.OrganisationTypes = OrganisationTypes.Carrier | OrganisationTypes.Broker;
			var consignment = Helper.CreateConsignment();
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, client.MainAddress);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee, DocumentDirection.ANY);

			AssertNull(documentContact);
		}

		public void TestGetContactOrganisation_WhenConsigneeDocGroupSelected_WithNoDeliveryAddress_ShouldNotGetOrgContact()
		{
			var consignment = Helper.CreateConsignment();

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee, DocumentDirection.ANY);

			AssertNull(documentContact);
		}

		public void TestGetContactOrganisation_WhenConsigneeDocGroupSelected_AndOrgHeaderDoesNotExist_ShouldNotGetOrgContact()
		{
			var consignment = Helper.CreateConsignment();
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee, DocumentDirection.ANY);

			AssertNull(documentContact);
		}

		#endregion

		#region DoSetupForDocument

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			var address = ((DtbConsignment)documentSupportableBO).Services.AddNew(); // for Service Documents
			base.DoSetupForDocument(command, documentSupportableBO);
		}

		#endregion

		#region TestShowReasonForNotPrinting

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals("ShowReasonForNotPrinting", false, DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		#endregion

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return ExpectedBizO;
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				var consignment = Helper.CreateConsignment("LTC001");
				var address = Helper.CreateConsignmentAddress(consignment, InstructionTypes.Codes.PickUp);
				Helper.CreateConsignmentAction(address, ActionTypes.Codes.PickUp);
				yield return consignment;
			}
		}

		DtbConsignmentDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = (DtbConsignmentDocumentSupporter)((IDocumentSupportable)ExpectedBizO).DocumentSupporter); }
		}
		DtbConsignmentDocumentSupporter documentSupporter;

		DtbConsignment ExpectedBizO
		{
			get { return expectedBizO ?? (expectedBizO = Helper.CreateConsignment()); }
		}
		DtbConsignment expectedBizO;

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper helper;

		#endregion
	}
}
