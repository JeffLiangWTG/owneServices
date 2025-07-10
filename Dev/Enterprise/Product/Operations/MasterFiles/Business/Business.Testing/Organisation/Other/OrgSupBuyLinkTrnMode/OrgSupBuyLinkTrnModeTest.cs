using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupBuyLinkTrnMode))]
	sealed class OrgSupBuyLinkTrnModeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestImporterCountry()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "USLAX";
			OrgSupplierBuyerLink link = org.BuyerLinks.AddNew(org2);
			link.OL_RN_NKImporterCountry = ZString.Empty;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			AssertEquals(Core.Constants.CountryGuids.UnitedStates, linkTrnMode.ImporterCountry.PK);
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryGuids.Australia, linkTrnMode.ImporterCountry.PK);
		}

		public void TestIsUSImporterCountry()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			link.OL_RN_NKImporterCountry = ZString.Empty;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			AssertNull(linkTrnMode.ImporterCountry);
			AssertEquals(false, linkTrnMode.IsUSImporterCountry);

			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals(false, linkTrnMode.IsUSImporterCountry);

			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(true, linkTrnMode.IsUSImporterCountry);
		}

		public void TestPF_OA_OverridePickupAddress()
		{
			var anotherOrg = Factory.New<OrgHeader>();
			var anotherAddress = anotherOrg.Addresses.AddNew();

			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();
			var contact = org.Contacts.AddNew();
			var link = org.BuyerLinks.AddNew();
			var linkTrnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			AssertEquals(ZGuid.Empty, linkTrnMode.PF_OA_OverridePickupAddress);

			linkTrnMode.PF_OC_OverrideSupplierContact = contact.PK;
			linkTrnMode.PF_OA_OverridePickupAddress = address1.PK;
			AssertEquals(contact.PK, linkTrnMode.PF_OC_OverrideSupplierContact);

			linkTrnMode.PF_OA_OverridePickupAddress = ZGuid.Empty;
			AssertEquals(contact.PK, linkTrnMode.PF_OC_OverrideSupplierContact);

			linkTrnMode.PF_OA_OverridePickupAddress = address1.PK;
			linkTrnMode.PF_OC_OverrideSupplierContact = contact.PK;
			linkTrnMode.PF_OA_OverridePickupAddress = address2.PK;
			AssertEquals(contact.PK, linkTrnMode.PF_OC_OverrideSupplierContact);

			linkTrnMode.PF_OA_OverridePickupAddress = anotherAddress.PK;
			AssertEquals(ZGuid.Empty, linkTrnMode.PF_OC_OverrideSupplierContact);
		}

		public void TestPF_OA_OverrideDeliveryAddress()
		{
			var anotherOrg = Factory.New<OrgHeader>();
			var anotherAddress = anotherOrg.Addresses.AddNew();

			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();
			var contact = org.Contacts.AddNew();
			var link = org.SupplierLinks.AddNew();
			var linkTrnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			AssertEquals(ZGuid.Empty, linkTrnMode.PF_OA_OverrideDeliveryAddress);

			linkTrnMode.PF_OC_OverrideConsigneeContact = contact.PK;
			linkTrnMode.PF_OA_OverrideDeliveryAddress = address1.PK;
			AssertEquals(contact.PK, linkTrnMode.PF_OC_OverrideConsigneeContact);

			linkTrnMode.PF_OA_OverrideDeliveryAddress = ZGuid.Empty;
			AssertEquals(contact.PK, linkTrnMode.PF_OC_OverrideConsigneeContact);

			linkTrnMode.PF_OA_OverrideDeliveryAddress = address1.PK;
			linkTrnMode.PF_OC_OverrideConsigneeContact = contact.PK;
			linkTrnMode.PF_OA_OverrideDeliveryAddress = address2.PK;
			AssertEquals(contact.PK, linkTrnMode.PF_OC_OverrideConsigneeContact);

			linkTrnMode.PF_OA_OverrideDeliveryAddress = anotherAddress.PK;
			AssertEquals(ZGuid.Empty, linkTrnMode.PF_OC_OverrideConsigneeContact);
		}

		public void TestPF_OA_OverrideNotifyPartyAddress()
		{
			var anotherOrg = Factory.New<OrgHeader>();
			var anotherAddress = anotherOrg.Addresses.AddNew();
			var anotherContact = anotherOrg.Contacts.AddNew();
			anotherContact.OC_IsActive = true;

			var anotherContactDocument = anotherContact.Documents.AddNew();
			anotherContactDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;
			anotherContactDocument.OD_DefaultContact = true;

			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			contact2.OC_IsActive = true;

			var contactDocument = contact2.Documents.AddNew();
			contactDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;
			contactDocument.OD_DefaultContact = true;

			var link = org.SupplierLinks.AddNew();
			var linkTrnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			AssertEquals(ZGuid.Empty, linkTrnMode.PF_OA_OverrideNotifyPartyAddress);

			linkTrnMode.PF_OC_OverrideNotifyParty = contact1.PK;
			linkTrnMode.PF_OA_OverrideNotifyPartyAddress = address1.PK;
			AssertEquals(contact1.PK, linkTrnMode.PF_OC_OverrideNotifyParty);

			linkTrnMode.PF_OA_OverrideNotifyPartyAddress = ZGuid.Empty;
			AssertEquals(contact1.PK, linkTrnMode.PF_OC_OverrideNotifyParty);

			linkTrnMode.PF_OA_OverrideNotifyPartyAddress = address1.PK;
			linkTrnMode.PF_OC_OverrideNotifyParty = contact1.PK;
			linkTrnMode.PF_OA_OverrideNotifyPartyAddress = address2.PK;
			AssertEquals(contact1.PK, linkTrnMode.PF_OC_OverrideNotifyParty);

			linkTrnMode.PF_OC_OverrideNotifyParty = anotherContact.PK;
			linkTrnMode.PF_OA_OverrideNotifyPartyAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, linkTrnMode.PF_OC_OverrideNotifyParty);

			linkTrnMode.PF_OA_OverrideNotifyPartyAddress = anotherAddress.PK;
			AssertEquals(anotherContact.PK, linkTrnMode.PF_OC_OverrideNotifyParty);
		}

		public void TestGetNewPF_OA_OverrideNotifyPartyAddress_ZAddress()
		{
			var linkTrnMode = Factory.NewWithValidTestData<OrgSupBuyLinkTrnMode>();
			AssertEquals(AddressType.OFC, linkTrnMode.PF_OA_OverrideNotifyPartyAddress_ZAddress.DefaultAddressType);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = factory.NewWithValidTestData<OrgHeader>();
			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew(org2);
			return link.OrgSupBuyLinkTrnModes[0];
		}

		#region TestContainerMode_ReadOnly

		public void TestContainerMode_ReadOnly()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var link = org1.BuyerLinks.AddNew(org2);

			var linkTrnMode = link.OrgSupBuyLinkTrnModes.AddNew();
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Should not be readonly", false, linkTrnMode.PF_ContainerModeInfo.ReadOnly);
			AssertEquals("Should be FCL", "FCL", linkTrnMode.PF_ContainerMode);

			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.All;
			AssertEquals("Should be readonly", true, linkTrnMode.PF_ContainerModeInfo.ReadOnly);
			AssertEquals("Should be empty", ZString.Empty, linkTrnMode.PF_ContainerMode);
		}

		#endregion

		#region CanDelete

		public void TestCanDelete()
		{
			AssertEquals("PreCondition: OrgSupplierBuyerLink should have 1 mode", 1, OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.Count);
			OrgSupBuyLinkTrnMode mode1 = OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes[0];
			AssertEquals("Shouldn't be able to delete mode as there is only 1", false, ((ICanDelete)mode1).CanDelete);

			OrgSupBuyLinkTrnMode mode2 = OrgSupplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			AssertEquals("Should be able to delete mode2 as there are 2", true, ((ICanDelete)mode2).CanDelete);
			mode2.Delete();

			AssertEquals("Shouldn't be able to delete mode as there is only 1", false, ((ICanDelete)mode1).CanDelete);
		}

		#endregion

		#region Implementation

		OrgSupplierBuyerLink OrgSupplierBuyerLink
		{
			get { return orgSupplierBuyerLink ?? (orgSupplierBuyerLink = Factory.New<OrgSupplierBuyerLink>()); }
		}
		OrgSupplierBuyerLink orgSupplierBuyerLink;

		#endregion
	}
}
