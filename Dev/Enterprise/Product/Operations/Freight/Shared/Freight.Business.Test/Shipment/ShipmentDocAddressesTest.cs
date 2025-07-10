using System;
using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentDocAddressesTest : TestCaseWithFactory
	{
		#region Test Shipment Startup

		public void TestCanDeleteAddress()
		{
			var address = Shipment.DocAddresses.AddNew(DocAddressType.ConsigneeDocumentaryAddress);
			address.OrganisationPK = Factory.New<OrgHeader>().PK;

			Assert("Can delete address for a Common Shipment", ((IDocAddresses)Shipment).CanDeleteAddress(address));

			var deleteEventTriggered = false;

			address.OnDeleting += delegate
			{
				deleteEventTriggered = true;
				AssertEquals("OrganisationPK has been cleared before deleting", ZGuid.Empty, address.OrganisationPK);
			};

			address.Delete();
			Assert(deleteEventTriggered);
		}

		public void TestShipmentStartsWithDefaultDocAddressTypes()
		{
			Assert("Precondition", !Globals.IsWeb);
			AssertShipmentStartsWithDefaultDocAddressTypes();
			Globals.IsWeb = true;
			try
			{
				SetUp();
				AssertShipmentStartsWithDefaultDocAddressTypes();
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		void AssertShipmentStartsWithDefaultDocAddressTypes()
		{
			AssertNotNull("DocAddress Manager should be set.", Shipment.DocAddressManager);
			AssertEquals("DocAddress Manager should have 5 requirements.", 5, Shipment.DocAddressManager.Requirements.Length);
			JobDocAddressRequirement[] requirements = Shipment.DocAddressManager.Requirements;
			bool hasConsigneePD = false;
			bool hasConsignorPD = false;
			bool hasConsignee = false;
			bool hasConsignor = false;
			bool hasNotifyParty = false;
			int consignee = 0;
			int consignor = 0;
			int consigneePD = 0;
			int consignorPD = 0;
			int notifyParty = 0;
			int count = 0;
			foreach (JobDocAddressRequirement requirement in requirements)
			{
				if (requirement.DefaultDocAddressType == DocAddressType.ConsigneePickupDeliveryAddress)
				{
					hasConsigneePD = true;
					consigneePD = count;
				}
				if (requirement.DefaultDocAddressType == DocAddressType.ConsignorPickupDeliveryAddress)
				{
					hasConsignorPD = true;
					consignorPD = count;
				}
				if (requirement.DefaultDocAddressType == DocAddressType.ConsigneeDocumentaryAddress)
				{
					hasConsignee = true;
					consignee = count;
				}
				if (requirement.DefaultDocAddressType == DocAddressType.ConsignorDocumentaryAddress)
				{
					hasConsignor = true;
					consignor = count;
				}
				if (requirement.DefaultDocAddressType == DocAddressType.NotifyParty)
				{
					hasNotifyParty = true;
					notifyParty = count;
				}
				count++;
			}
			AssertEquals("ConsigneePD, ConsignorPD, Consignee, Consignor and NotifyParty Requirements should exist.", true, hasConsigneePD && hasConsignorPD && hasConsignee && hasConsignor && hasNotifyParty);

			AssertEquals(requirements[consigneePD], Shipment.ConsigneePickupDeliveryAddressRequirement);
			AssertEquals(requirements[consignorPD], Shipment.ConsignorPickupDeliveryAddressRequirement);
			AssertEquals(requirements[consignee], Shipment.ConsigneeDocAddressRequirement);
			AssertEquals(requirements[consignor], Shipment.ConsignorDocAddressRequirement);
			AssertEquals(requirements[notifyParty], Shipment.NotifyPartyDocAddressRequirement);

			hasConsigneePD = false;
			hasConsignorPD = false;
			hasConsignee = false;
			hasConsignor = false;
			hasNotifyParty = false;
			foreach (JobDocAddressRequirement requirement in requirements)
			{
				hasConsigneePD = hasConsigneePD || (requirement.DefaultDocAddressType == Shipment.ConsigneePickupDeliveryAddressRequirement.DefaultDocAddressType);
				hasConsignorPD = hasConsignorPD || (requirement.DefaultDocAddressType == Shipment.ConsignorPickupDeliveryAddressRequirement.DefaultDocAddressType);
				hasConsignee = hasConsignee || (requirement.DefaultDocAddressType == Shipment.ConsigneeDocAddressRequirement.DefaultDocAddressType);
				hasConsignor = hasConsignor || (requirement.DefaultDocAddressType == Shipment.ConsignorDocAddressRequirement.DefaultDocAddressType);
				hasNotifyParty = hasNotifyParty || (requirement.DefaultDocAddressType == Shipment.NotifyPartyDocAddressRequirement.DefaultDocAddressType);
			}
			AssertEquals("ConsigneePD, ConsignorPD, Consignee, Consignor and NotifyParty Requirements should exist.", true, hasConsignee && hasConsignor && hasNotifyParty);

			AssertCodeInArray(requirements[consignee].SupportedDocAddressTypes, DocAddressType.ConsigneePickupDeliveryAddress, !Globals.IsWeb);
			AssertCodeInArray(requirements[consignor].SupportedDocAddressTypes, DocAddressType.ConsignorPickupDeliveryAddress, !Globals.IsWeb);

			AssertCodeInArray(requirements[consigneePD].SupportedDocAddressTypes, DocAddressType.ConsigneeDocumentaryAddress, Globals.IsWeb);
			AssertCodeInArray(requirements[consignorPD].SupportedDocAddressTypes, DocAddressType.ConsignorDocumentaryAddress, Globals.IsWeb);
		}

		void AssertCodeInArray(DocAddressType[] array, DocAddressType code, ZBool contains)
		{
			AssertEquals("Code should " + (contains ? "" : "not") + " have been found: " + DocAddressTypes.GetCode(Factory, code), true, contains ? ((IList)array).Contains(code) : !((IList)array).Contains(code));
		}

		public void TestAddressesAreProvidedCorrectly()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			JobDocAddress cED = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			AssertNull("Consignee Documentary address set by default.", cED);
			JobDocAddress cE = shipment.ConsigneeDocumentaryAddress;
			AssertEquals("Consignee Documentary address not set.", cE, shipment.ConsigneeDocumentaryAddress);
			cED = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			AssertEquals("Consignee Documentary address not set.", cED, shipment.ConsigneeDocumentaryAddress);

			JobDocAddress cRD = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			AssertNull("Consignor Documentary address set by default.", cRD);
			cRD = shipment.ConsignorDocumentaryAddress;
			AssertNotNull("Consignor Documentary address not set.", cRD);
			AssertEquals("Consignor Documentary address not set.", cRD, shipment.ConsignorDocumentaryAddress);

			JobDocAddress nPP = shipment.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
			AssertNull("NotifyParty Documentary address set.", nPP);
			nPP = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			AssertNotNull("NotifyParty Documentary address not set.", nPP);
			AssertEquals("NotifyParty Documentary address not set.", nPP, shipment.NotifyPartyDocumentaryAddress);

			JobDocAddress cEPAD = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
			AssertNotNull("Consignee Deliver address not set.", cEPAD);
			AssertEquals("Consignee Deliver address not set.", cEPAD, shipment.ConsigneeDeliveryAddress);

			JobDocAddress cRPAD = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			AssertNotNull("Consignor Pickup address not set.", cRPAD);
			AssertEquals("Consignor Pickup address not set.", cRPAD, shipment.ConsignorPickupAddress);
		}

		public void TestAddressesSetHasChangesInParent()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertEquals("Shipment should not have changes.", false, shipment.HasChanges);

			shipment.ConsigneeDocumentaryAddress.E2_ParentTableCode = "";
			AssertEquals("Shipment should have changes.", true, shipment.HasChanges);

			shipment = CommonShipment.New(Factory);
			JobDocAddress cED = shipment.ConsigneeDocumentaryAddress;
			AssertEquals("Shipment should not have changes.", false, shipment.HasChanges);

			shipment.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			AssertEquals("Shipment should not have changes.", false, shipment.HasChanges);
			shipment.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageYard).E2_AddressOverride = true;
			AssertEquals("Shipment should have changes.", true, shipment.HasChanges);
		}

		#endregion

		#region Test OrgHeaders (Consignee/or, Notify Party)

		public void TestChangeOHDirectly()
		{
			AssertNotNull("Consignee's Documentary Address should be set.", Shipment.ConsigneeDocumentaryAddress);
			OrgHeader anyOrg = Factory.New<OrgHeader>();
			Shipment.ConsigneePK = anyOrg.PK;
			AssertEquals("Documentary DocAddress should indirectly point to new Org.", anyOrg.PK, Shipment.ConsigneeDocumentaryAddress.OrganisationPK);
			Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Shipment's Consignee should be null.", true, Shipment.Consignee == null);

			Shipment.ConsignorPK = anyOrg.PK;
			AssertNotNull("Consignor's Documentary Address should be set.", Shipment.ConsignorDocumentaryAddress);
			AssertEquals("Documentary DocAddress should indirectly point to new Org.", anyOrg.PK, Shipment.ConsignorDocumentaryAddress.OrganisationPK);
			Shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Shipment's Consignor should be null.", true, Shipment.Consignor == null);

			Shipment.NotifyPartyContactPK = anyOrg.Contacts.AddNew().PK;
			AssertNotNull("NotifyOrganisation's Documentary Address should be set.", Shipment.NotifyPartyDocumentaryAddress);
			AssertEquals("DocAddress should indirectly point to new Org.", anyOrg.PK, Shipment.NotifyPartyDocumentaryAddress.OrganisationPK);
			Shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Shipment's NotifyOrganisation should be null.", true, Shipment.NotifyParty == null);
		}

		public void TestCalcCodesAndNames()
		{
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_FullName = "Consignor Full Name";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_FullName = "Consignee Full Name";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;
			Factory.Save();

			CommonShipment shipment = CommonShipment.New(Factory);

			AssertEquals("Blank Consignor.", "", shipment.JS_Calc_ConsignorCompanyCode);
			AssertEquals("Blank Consignor company name.", "", shipment.JS_Calc_ConsignorCompanyName);
			AssertEquals("Blank Consignee.", "", shipment.JS_Calc_ConsigneeCompanyCode);
			AssertEquals("Blank Consignee company name.", "", shipment.JS_Calc_ConsigneeCompanyName);

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			AssertEquals("Consignor.", consignor.OH_Code, shipment.JS_Calc_ConsignorCompanyCode);
			AssertEquals("Consignor company name.", consignor.OH_FullName, shipment.JS_Calc_ConsignorCompanyName);
			AssertEquals("Consignee.", consignee.OH_Code, shipment.JS_Calc_ConsigneeCompanyCode);
			AssertEquals("Consignee company name.", consignee.OH_FullName, shipment.JS_Calc_ConsigneeCompanyName);

			AssertEquals("Consignor Code is R/O.", true, shipment.JS_Calc_ConsignorCompanyCodeInfo.ReadOnly);
			AssertEquals("Consignee Code is R/O.", true, shipment.JS_Calc_ConsigneeCompanyCodeInfo.ReadOnly);
			AssertEquals("Consignor CO.Name is R/O.", true, shipment.JS_Calc_ConsignorCompanyNameInfo.ReadOnly);
			AssertEquals("Consignee CO.Name is R/O.", true, shipment.JS_Calc_ConsigneeCompanyNameInfo.ReadOnly);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;

			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "Harry Was Here";
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "JUNK IS GOOD";

			AssertEquals("Consignor.", "(JUNKISGOOD)", shipment.JS_Calc_ConsignorCompanyCode);
			AssertEquals("Consignor company name.", shipment.ConsignorDocumentaryAddress.E2_CompanyName, shipment.JS_Calc_ConsignorCompanyName);
			AssertEquals("Consignee.", "(HarryWasHere)", shipment.JS_Calc_ConsigneeCompanyCode);
			AssertEquals("Consignee company name.", shipment.ConsigneeDocumentaryAddress.E2_CompanyName, shipment.JS_Calc_ConsigneeCompanyName);

			AssertEquals("Consignor Code is R/O.", true, shipment.JS_Calc_ConsignorCompanyCodeInfo.ReadOnly);
			AssertEquals("Consignee Code is R/O.", true, shipment.JS_Calc_ConsigneeCompanyCodeInfo.ReadOnly);
		}

		public void TestPickupAddressOrgSet()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			AssertEquals("Consignor PK should be set", consignor.PK, shipment.ConsignorPickupAddress.OrganisationPK);
		}

		public void TestPickupAddressOrgSetFromDocumentaryAddress()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			AssertEquals("Pickup Address OrgPK should be set", consignor.PK, shipment.ConsignorPickupAddress.OrganisationPK);

			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor2.PK;
			AssertEquals("Pickup Address OrgPK should be set", consignor2.PK, shipment.ConsignorPickupAddress.OrganisationPK);

			var consignor3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPickupAddress.OrganisationPK = consignor3.PK;
			shipment.ConsignorPK = consignor.PK;
			AssertEquals("Pickup Address OrgPK should not be set", consignor3.PK, shipment.ConsignorPickupAddress.OrganisationPK);

			shipment.ConsignorPK = consignor3.PK;
			AssertEquals("Pickup Address OrgPK should be set", consignor3.PK, shipment.ConsignorPickupAddress.OrganisationPK);

			shipment.ConsignorPK = ZGuid.Empty;
			AssertEquals("Pickup Address OrgPK should not be set", ZGuid.Empty, shipment.ConsignorPickupAddress.OrganisationPK);

			shipment.ConsignorPK = consignor3.PK;
			AssertEquals("Pickup Address OrgPK should be set", consignor3.PK, shipment.ConsignorPickupAddress.OrganisationPK);

			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPK = consignor.PK;
			AssertEquals("Pickup Address OrgPK should not be set", ZGuid.Empty, shipment.ConsignorPickupAddress.OrganisationPK);

			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			AssertEquals("Pickup Address OrgPK should be set back", consignor3.PK, shipment.ConsignorPickupAddress.OrganisationPK);

			shipment.ConsignorPK = consignor3.PK;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Pickup Address should be synch'd", true, shipment.ConsignorPickupAddress.E2_AddressOverride);

			shipment.ConsignorDocumentaryAddress.E2_City = "hello";
			AssertEquals("Pickup Address City should be synch'd", "hello", shipment.ConsignorPickupAddress.E2_City);

			shipment.ConsignorPickupAddress.E2_City = "bye";
			AssertEquals("Documentary Address city should be 'hello'", "hello", shipment.ConsignorDocumentaryAddress.E2_City);
			AssertEquals("Pickup Address city should be 'bye' (now unsych'd)", "bye", shipment.ConsignorPickupAddress.E2_City);

			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "A Company";
			AssertEquals("Pickup Address company name should be A Company - only company in synch", "A Company", shipment.ConsignorPickupAddress.E2_CompanyName);

			shipment.ConsignorDocumentaryAddress.E2_Address2 = "Address2";
			AssertEquals("Pickup Address2 should be blank still", "", shipment.ConsignorPickupAddress.E2_Address2);

			shipment.ConsignorPickupAddress.E2_City = "hello";
			shipment.ConsignorPickupAddress.E2_Address2 = "Address2";

			shipment.ConsignorDocumentaryAddress.E2_Postcode = "2000";
			AssertEquals("Pickup Address postcode should be '2000' (sych'd again because perfect match)", "2000", shipment.ConsignorPickupAddress.E2_Postcode);
		}

		public void TestDeliveryAddressOrgSetFromDocumentaryAddress()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			AssertEquals("Delivery Address OrgPK should be set", consignee.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee2.PK;
			AssertEquals("Delivery Address OrgPK should be set", consignee2.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);

			var consignee3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneeDeliveryAddress.OrganisationPK = consignee3.PK;
			shipment.ConsigneePK = consignee.PK;
			AssertEquals("Delivery Address OrgPK should not be set", consignee3.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);

			shipment.ConsigneePK = consignee3.PK;
			AssertEquals("Delivery Address OrgPK should be set", consignee3.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);

			shipment.ConsigneePK = ZGuid.Empty;
			AssertEquals("Back in sync, Delivery Address OrgPK should be set", ZGuid.Empty, shipment.ConsigneeDeliveryAddress.OrganisationPK);

			shipment.ConsigneePK = consignee3.PK;
			AssertEquals("Delivery Address OrgPK should be set", consignee3.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneePK = consignee.PK;
			AssertEquals("Delivery Address OrgPK should not be set", ZGuid.Empty, shipment.ConsigneeDeliveryAddress.OrganisationPK);

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			shipment.ConsigneePK = consignee.PK;
			AssertEquals("Delivery Address OrgPK should be set back", consignee3.PK, shipment.ConsigneeDeliveryAddress.OrganisationPK);
		}

		#endregion

		#region TestNotifyPartyReadOnly

		public void TestNotifyPartyReadOnly()
		{
			AssertEquals("Shipment.NotifyPartyDocumentary.ReadOnly when no consignee.", true, Shipment.NotifyPartyDocumentaryAddress.ReadOnly);

			Shipment.ConsigneePK = (Factory.NewWithValidTestData(typeof(OrgHeader))).PK;
			AssertEquals("Shipment.NotifyParty.ReadOnly but consignee set.", false, Shipment.NotifyPartyDocumentaryAddress.ReadOnly);
		}

		#endregion

		#region TestSupportedAddressTypes + TestGetDocAddress

		public void TestSupportedAddressTypes()
		{
			var addressTypes = ((IDocAddresses)Shipment).SupportedAddressTypes;

			AssertEquals("New address types might have been added, ensure they are tested.", 15, addressTypes.Count);
			AssertCollectionContains(DocAddressType.ConsignorDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ConsignorPickupDeliveryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ConsigneeDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ConsigneePickupDeliveryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.NotifyParty, addressTypes);
			AssertCollectionContains(DocAddressType.NotifyParty2, addressTypes);
			AssertCollectionContains(DocAddressType.NotifyParty3, addressTypes);
			AssertCollectionContains(DocAddressType.BuyerDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.InsuredByDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.AssuredPartyDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ClaimsPayableByDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.SurveyReportPartyDocumentaryAddress, addressTypes);
			AssertCollectionContains(DocAddressType.ControllingCustomer, addressTypes);
			AssertCollectionContains(DocAddressType.PickupAgent, addressTypes);
			AssertCollectionContains(DocAddressType.Manufacturer, addressTypes);
		}

		public void TestGetDocAddress()
		{
			IDocAddresses iDocAddresses = Shipment;

			DocAddressType addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.ConsignorDocumentaryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.ConsignorPickupDeliveryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.ConsigneeDocumentaryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.ConsigneePickupDeliveryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.ConsigneePickupDeliveryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.NotifyParty).DefaultDocAddressType;
			AssertEquals(DocAddressType.NotifyParty, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.NotifyParty2).DefaultDocAddressType;
			AssertEquals(DocAddressType.NotifyParty2, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.NotifyParty3).DefaultDocAddressType;
			AssertEquals(DocAddressType.NotifyParty3, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.BuyerDocumentaryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.BuyerDocumentaryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.InsuredByDocumentaryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.InsuredByDocumentaryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.AssuredPartyDocumentaryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.AssuredPartyDocumentaryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.ClaimsPayableByDocumentaryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.ClaimsPayableByDocumentaryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.SurveyReportPartyDocumentaryAddress).DefaultDocAddressType;
			AssertEquals(DocAddressType.SurveyReportPartyDocumentaryAddress, addressType);

			addressType = iDocAddresses.GetDocAddressRequirement(DocAddressType.PickupAgent).DefaultDocAddressType;
			AssertEquals(DocAddressType.PickupAgent, addressType);
		}

		#endregion

		#region GetOrgHeaderList

		public void TestGetOrgHeaderList()
		{
			var docAddress = Shipment;
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(docAddress, DocAddressType.ConsignorDocumentaryAddress, OrganisationTypes.Consignor, ZString.Empty, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(docAddress, DocAddressType.ConsigneeDocumentaryAddress, OrganisationTypes.Consignee, ZString.Empty, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(docAddress, DocAddressType.PickupAgent, OrganisationTypes.Forwarder, ZString.Empty, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(docAddress, DocAddressType.ConsignorPickupDeliveryAddress, OrganisationTypes.None, ZString.Empty, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(docAddress, DocAddressType.ConsigneePickupDeliveryAddress, OrganisationTypes.None, ZString.Empty, ZString.Empty);
			AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(docAddress, DocAddressType.NotifyParty, OrganisationTypes.None, OrganisationsSubTypeList.Codes.NotifyParty, ZString.Empty);
		}

		void AssertCollectionWithValueFromOrganisationDefaultProviderAttribute(
			IDocAddresses docAddress,
			DocAddressType docAddressTypeForTest,
			OrganisationTypes expectedOrganisationTypes,
			ZString expectedOrganisationSubType,
			ZString expectedDocAddressType)
		{
			var collection = docAddress.GetOrgHeaderList(docAddressTypeForTest);
			AssertEquals(typeof(OrganisationsFindBoxCollection), collection.GetType());
			var findBoxCollection = collection as OrganisationsFindBoxCollection;
			AssertEquals(expectedOrganisationTypes, findBoxCollection.OrganisationType);
			AssertEquals(expectedOrganisationSubType, findBoxCollection.OrganisationSubType);
			AssertEquals(expectedDocAddressType, findBoxCollection.DocAddressType);
		}

		public void TestGetOrganisationsFindBoxCollectionWithOriginalProviderType()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipmentForTest>();
			OrganisationsFindBoxCollection result = null;
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				result = shipment.GetOrganisationsFindBoxCollectionWithOriginalProviderType_Exposed(null);
			});

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				result = shipment.GetOrganisationsFindBoxCollectionWithOriginalProviderType_Exposed(string.Empty);
			});

			AssertExceptionThrown(typeof(InvalidOperationException), "This property does not exist in this collection. Error on Property name: PropertyNotExist", () =>
			{
				result = shipment.GetOrganisationsFindBoxCollectionWithOriginalProviderType_Exposed("PropertyNotExist");
			});

			result = shipment.GetOrganisationsFindBoxCollectionWithOriginalProviderType_Exposed(nameof(shipment.Lookups.Broker_List));
			var findBoxCollection = result;
			AssertEquals(OrganisationTypes.Broker, findBoxCollection.OrganisationType);
			AssertEquals(OrganisationsSubTypeList.Codes.ImportBroker, findBoxCollection.OrganisationSubType);
			AssertEquals(ZString.Empty, findBoxCollection.DocAddressType);
		}

		#endregion

		#region Factory Hits

		public void TestCommonShipmentConsigneeFactoryHits_WhenOrganizationIsReal()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneePK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedShipment = factory2.Load<CommonShipment>(shipment.PK);

			try
			{
				factory2.ResetDatabaseLoadCount();
				BusinessObjectFactory.StopLogging();
				BusinessObjectFactory.StartLogging();

				var reloadedConsignee = reloadedShipment.Consignee;
				var loadCounter = JobDocAddressTest.GetLoadCountForEachTable(BusinessObjectFactory.DebugLog);

				AssertEquals(2, loadCounter["Enterprise.MasterFiles.Business.JobDocAddress"]);
				AssertEquals(13, loadCounter["Enterprise.MasterFiles.Business.OrgAddress"]);
				AssertEquals(5, loadCounter["Enterprise.MasterFiles.Business.OrgHeader"]);
				AssertEquals(26, BusinessObjectFactory.DebugLogCount);
			}
			finally
			{
				BusinessObjectFactory.StopLogging();
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<CommonShipment>();
		}

		CommonShipment Shipment;

		class CommonShipmentForTest : CommonShipment
		{
			public CommonShipmentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
			public OrganisationsFindBoxCollection GetOrganisationsFindBoxCollectionWithOriginalProviderType_Exposed(string baseJobShipmentLookupsPropertyName)
			{
				return GetOrganisationsFindBoxCollectionWithOriginalProviderType(baseJobShipmentLookupsPropertyName);
			}
		}

		#endregion
	}
}
