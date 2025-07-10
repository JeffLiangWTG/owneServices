using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupBuyLinkTrnModeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUSPortsOfLading()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_RL_NKClosestPort = "USLAX";
			OrgSupplierBuyerLink supplierBuyerLink = Factory.New<OrgSupplierBuyerLink>();
			supplierBuyerLink.OL_RN_NKImporterCountry = ZString.Empty;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollection>(), LinkMode.Lookups.USPortsOfLading.GetType());

			LinkMode.PF_OL = supplierBuyerLink.PK;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollection>(), LinkMode.Lookups.USPortsOfLading.GetType());

			supplierBuyerLink.OL_OH_Buyer = buyer.PK;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCForeignPortCollection>(), LinkMode.Lookups.USPortsOfLading.GetType());

			supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollection>(), LinkMode.Lookups.USPortsOfLading.GetType());

			supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCForeignPortCollection>(), LinkMode.Lookups.USPortsOfLading.GetType());
		}

		public void TestUSPortsOfUnLading()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_RL_NKClosestPort = "USLAX";
			OrgSupplierBuyerLink supplierBuyerLink = Factory.New<OrgSupplierBuyerLink>();
			supplierBuyerLink.OL_RN_NKImporterCountry = ZString.Empty;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCForeignPortCollection>(), LinkMode.Lookups.USPortsOfUnLading.GetType());

			LinkMode.PF_OL = supplierBuyerLink.PK;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCForeignPortCollection>(), LinkMode.Lookups.USPortsOfUnLading.GetType());

			supplierBuyerLink.OL_OH_Buyer = buyer.PK;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollection>(), LinkMode.Lookups.USPortsOfUnLading.GetType());

			supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCForeignPortCollection>(), LinkMode.Lookups.USPortsOfUnLading.GetType());

			supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollection>(), LinkMode.Lookups.USPortsOfUnLading.GetType());
		}

		#region Addresses

		public void TestAddresses()
		{
			AssertEquals(LinkMode.Lookups.CustomsExamSites, LinkMode.Lookups.CustomsControlledArrivalLocations);
		}

		#endregion

		#region Contacts

		public void TestOverrideConsigneeContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			LinkMode.PF_OL = Factory.NewWithValidTestData<OrgSupplierBuyerLink>().PK;
			LinkMode.SupplierBuyerLink.OL_OH_Buyer = org.PK;
			AssertSequencesEqual(org.Contacts, LinkMode.Lookups.OverrideConsigneeContacts);

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = deliveryOrg.Addresses.AddNew();
			LinkMode.PF_OA_OverrideDeliveryAddress = deliveryAddress.PK;
			AssertSequencesEqual(deliveryOrg.Contacts, LinkMode.Lookups.OverrideConsigneeContacts);
		}

		public void TestOverrideSupplierContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			LinkMode.PF_OL = Factory.NewWithValidTestData<OrgSupplierBuyerLink>().PK;
			LinkMode.SupplierBuyerLink.OL_OH_Supplier = org.PK;
			AssertSequencesEqual(org.Contacts, LinkMode.Lookups.OverrideSupplierContacts);

			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = pickupOrg.Addresses.AddNew();
			LinkMode.PF_OA_OverridePickupAddress = pickupAddress.PK;
			AssertSequencesEqual(pickupOrg.Contacts, LinkMode.Lookups.OverrideSupplierContacts);
		}

		public void TestOverrideNotifyParties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_IsActive = false;
			var contact2 = org.Contacts.AddNew();
			contact2.OC_IsActive = true;
			LinkMode.PF_OL = Factory.NewWithValidTestData<OrgSupplierBuyerLink>().PK;
			LinkMode.SupplierBuyerLink.OL_OH_Buyer = org.PK;

			AssertEquals("There should only be one contact", 1, LinkMode.Lookups.OverrideNotifyParties.Count);
			AssertEquals("There should only be active contacts", contact2, LinkMode.Lookups.OverrideNotifyParties.FirstOrDefault());

			contact2.OC_IsActive = false;

			AssertEquals("There should no active contacts", 0, LinkMode.Lookups.OverrideNotifyParties.Count);

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			var notifyPartyAddress = notifyParty.Addresses.AddNew();
			LinkMode.PF_OA_OverrideNotifyPartyAddress = notifyPartyAddress.PK;
			AssertSequencesEqual(notifyParty.Contacts, LinkMode.Lookups.OverrideNotifyParties);
		}

		#endregion

		#region ContainerModeList

		public void TestContainerModeList()
		{
			LinkMode.PF_TransportMode = "AIR";
			Assert(!LinkMode.Lookups.ContainerModeList.ContainsCode("FCL"));
			LinkMode.PF_TransportMode = "SEA";
			Assert(LinkMode.Lookups.ContainerModeList.ContainsCode("FCL"));
			LinkMode.PF_TransportMode = "ROA";
			Assert(LinkMode.Lookups.ContainerModeList.ContainsCode("BLK"));
		}

		[ExpectNoExceptions]
		public void TestContainerModeListCache()
		{
			var mock = new Mock<IFreightCodePairListProvider>();
			mock.Setup(x => x.GetContainerModeList(It.IsAny<string>())).Returns(() => new CodeDescriptionPairList());
			ObjectFactory.Substitute(mock.Object);

			LinkMode.PF_TransportMode = "AIR";
			var containerModeList = LinkMode.Lookups.ContainerModeList;
			mock.Verify(x => x.GetContainerModeList(It.IsAny<string>()), Times.Once, "First call.");
			containerModeList = LinkMode.Lookups.ContainerModeList;
			containerModeList = LinkMode.Lookups.ContainerModeList;
			mock.Verify(x => x.GetContainerModeList(It.IsAny<string>()), Times.Once, "The method should not be invoked no matter how many times we call it.");

			LinkMode.PF_TransportMode = "SEA";
			containerModeList = LinkMode.Lookups.ContainerModeList;
			mock.Verify(x => x.GetContainerModeList(It.IsAny<string>()), Times.Exactly(2), "The cache should be updated as the transport mode changed.");
		}

		#endregion

		#region TransportModeList

		public void TestTransportModeList()
		{
			Assert("TransportModeList.Count > 0", LinkMode.Lookups.TransportModeList.Count > 0);
		}

		#endregion

		#region IncoTermList

		[TestDate(2010, 12, 1)]
		public void TestIncoTermList()
		{
			string[] actualCodes = (from code in LinkMode.Lookups.IncoTermList.Cast<CodeDescriptionPair>() select code.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Core.Constants.IncoTerms.Incoterms2000.Concat(domesticPaymentTermsCodes), actualCodes);
		}

		[TestDate(2011, 1, 11)]
		public void TestIncoTermList2()
		{
			string[] actualCodes = (from code in LinkMode.Lookups.IncoTermList.Cast<CodeDescriptionPair>() select code.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Core.Constants.IncoTerms.Incoterms2010.Concat(domesticPaymentTermsCodes), actualCodes);
		}

		readonly string[] domesticPaymentTermsCodes = new string[]
			{
				Core.Constants.DomesticPaymentTerms.Collect, Core.Constants.DomesticPaymentTerms.CollectThirdParty,
				Core.Constants.DomesticPaymentTerms.CollectCOD, Core.Constants.DomesticPaymentTerms.Prepaid
			};

		#endregion
		#region IncoTermModeList
		public void TestIncoTermModeList()
		{
			Assert(LinkMode.Lookups.IncoTermModeList.ContainsCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH));
			Assert(LinkMode.Lookups.IncoTermModeList.ContainsCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT));
			Assert(LinkMode.Lookups.IncoTermModeList.ContainsCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS));
		}
		#endregion

		#region Implementation

		OrgSupBuyLinkTrnMode LinkMode
		{
			get { return orgSupBuyLinkTrnMode ?? (orgSupBuyLinkTrnMode = Factory.New<OrgSupBuyLinkTrnMode>()); }
		}
		OrgSupBuyLinkTrnMode orgSupBuyLinkTrnMode;

		#endregion
	}
}
