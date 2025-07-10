using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRelatedPartyValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckPR_PartyTypeWorksIfHasChanges

		public void TestCheckPR_PartyTypeWorksIfHasChanges()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			ZGuid orgPK = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_OH_Parent = orgPK;
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			partyRecord.PR_OH_RelatedParty = orgPK;
			AssertEquals("No notifications", false, partyRecord.PR_PartyTypeInfo.HasNotifications());

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = false;
			partyRecord.Validation.ValidateAll();
			AssertHasError(partyRecord.PR_PartyTypeInfo, "New Related Party of this type is not allowed based on your current security rights.");

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = true;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = true;

			Factory.Save();

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = false;

			partyRecord.Validation.ValidateAll();
			AssertEquals("No notifications because we didn't touch it after loading.", false, partyRecord.PR_PartyTypeInfo.HasNotifications());

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			AssertHasError("Should have error because it has changes.", partyRecord.PR_PartyTypeInfo, "New Related Party of this type is not allowed based on your current security rights.");
		}

		#endregion

		#region TestCheckPR_PartyType

		public void TestCheckPR_PartyType()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_PartyType = "ABC";
			AssertEquals("Should have errors", true, partyRecord.PR_PartyTypeInfo.HasErrors());

			var servicePartyRecord = Factory.New<OrgRelatedParty>();
			servicePartyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			servicePartyRecord.PR_OH_RelatedParty = servicePartyRecord.PR_OH_Parent;
			servicePartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
			AssertNoNotifications("No notifications", servicePartyRecord.PR_PartyTypeInfo);

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			AssertEquals("No notifications", false, partyRecord.PR_PartyTypeInfo.HasNotifications());

			partyRecord.PR_PartyType = ZString.Empty;
			AssertEquals("Should have errors", true, partyRecord.PR_PartyTypeInfo.HasErrors());

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = true;

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			AssertHasError(partyRecord.PR_PartyTypeInfo, "New Related Party of this type is not allowed based on your current security rights.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			AssertNoError(partyRecord.PR_PartyTypeInfo, "New Related Party of this type is not allowed based on your current security rights.");

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = true;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = false;

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			AssertNoError(partyRecord.PR_PartyTypeInfo, "New Related Party of this type is not allowed based on your current security rights.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			AssertHasError(partyRecord.PR_PartyTypeInfo, "New Related Party of this type is not allowed based on your current security rights.");

			OrgHeader parent = Factory.Load<OrgHeader>(partyRecord.PR_OH_Parent);
			parent.OH_IsBroker = false;
			partyRecord.PR_OH_RelatedParty = partyRecord.PR_OH_Parent;
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertHasError("CAB type cannot be chosen for non broker organizations", partyRecord.PR_PartyTypeInfo, "CAB type cannot be chosen for non broker organizations");

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedVendor;
			AssertHasErrorContaining(partyRecord.PR_PartyTypeInfo, "CAU or CAV type cannot be chosen for non CSA Approved Importer organizations.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertNoErrorContaining(partyRecord.PR_PartyTypeInfo, "CAU or CAV type cannot be chosen for non CSA Approved Importer organizations.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			AssertHasErrorContaining(partyRecord.PR_PartyTypeInfo, "CAU or CAV type cannot be chosen for non CSA Approved Importer organizations.");
			var impAddInfo = parent.GetCountryData(Core.Constants.CountryCodes.Canada).ImpAddInfo;
			impAddInfo[CAOrgImpAddInfoSchema.Constants.ZO_IsCSAApprovedImporter] = true;
			Factory.Save();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedVendor;
			AssertNoErrorContaining(partyRecord.PR_PartyTypeInfo, "CAU or CAV type cannot be chosen for non CSA Approved Importer organizations.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			AssertNoErrorContaining(partyRecord.PR_PartyTypeInfo, "CAU or CAV type cannot be chosen for non CSA Approved Importer organizations.");
		}

		public void TestCheckPR_PartyTypeOnConsignorConsignee()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var partyRecord = org.ConsignorRelatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertHasError(partyRecord.PR_PartyTypeInfo, "The selected Party Type cannot be added as a Consignor Related Party.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryTo;
			AssertHasError(partyRecord.PR_PartyTypeInfo, "The selected Party Type cannot be added as a Consignor Related Party.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupAgent;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupFrom;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.WarehouseForwarder;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.Delete();

			partyRecord = org.ConsigneeRelatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryTo;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupAgent;
			AssertHasError(partyRecord.PR_PartyTypeInfo, "The selected Party Type cannot be added as a Consignee Related Party.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupFrom;
			AssertHasError(partyRecord.PR_PartyTypeInfo, "The selected Party Type cannot be added as a Consignee Related Party.");
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.WarehouseForwarder;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.Delete();

			partyRecord = org.AllRelatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryTo;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupAgent;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupFrom;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.WarehouseForwarder;
			AssertNoErrors(partyRecord.PR_PartyTypeInfo);
		}

		public void TestCheckPR_PartyTypeForPPT() => CombineAssertions(() =>
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "org3";

			var existingRelationship = Factory.New<OrgRelatedParty>();
			existingRelationship.PR_OH_Parent = org1.PK;
			existingRelationship.PR_OH_RelatedParty = org2.PK;
			existingRelationship.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;

			var newRelationship = Factory.New<OrgRelatedParty>();
			newRelationship.PR_OH_Parent = org2.PK;
			newRelationship.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertHasError(newRelationship.PR_PartyTypeInfo, "org2 has a Product Parent, it cannot have Product children");

			newRelationship.PR_OH_Parent = org3.PK;
			newRelationship.Validation.ValidatePR_PartyType();
			AssertNoErrors(newRelationship.PR_PartyTypeInfo);
		});

		#endregion

		#region TestCheckPR_PartyType_DoesNotAllowMoreThanOneWarehouseRelatedParty

		public void TestCheckPR_PartyType_DoesNotAllowMoreThanOneWarehouseRelatedParty()
		{
			var parentOrg = Factory.New<OrgHeader>();
			var relatedPartyRecord1 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);

			var relatedPartyRecord2 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			relatedPartyRecord2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);

			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			AssertHasError(relatedPartyRecord2.PR_PartyTypeInfo, "There can only be one Warehouse Related Party.");

			var relatedPartyRecord3 = Factory.New<OrgRelatedParty>();
			relatedPartyRecord3.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			AssertNoErrors(relatedPartyRecord3.PR_PartyTypeInfo);
		}

		#endregion

		#region TestMultipleSRVRelatedParties

		public void TestCanHaveMultipleSRVRelatedPartiesWithDifferentOrganisations()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedPartyRecord1 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord1.PR_OH_RelatedParty = relatedPartyOrg1.PK;

			var relatedPartyRecord2 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord2.PR_OH_RelatedParty = relatedPartyOrg2.PK;

			relatedPartyRecord1.RunPreSaveValidation();
			relatedPartyRecord2.RunPreSaveValidation();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);
				AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);
			});
		}

		public void TestCanNotHaveMultipleSRVRelatedPartiesWithSameOrganisation()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg = Factory.NewWithValidTestData<OrgHeader>();

			var relatedPartyRecord1 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord1.PR_OH_RelatedParty = relatedPartyOrg.PK;

			var relatedPartyRecord2 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord2.PR_OH_RelatedParty = relatedPartyOrg.PK;

			relatedPartyRecord1.RunPreSaveValidation();
			relatedPartyRecord2.RunPreSaveValidation();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord1.PR_OH_RelatedPartyInfo, "There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");

				AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord2.PR_OH_RelatedPartyInfo, "There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");
			});
		}

		public void TestCanNotHaveMultipleSRVRelatedPartiesWithAtLeastOneSameOrganisation()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg3 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedPartyRecord1 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord1.PR_OH_RelatedParty = relatedPartyOrg1.PK;

			//relatedPartyRecord 2 & 3 are related to the same organisation
			var relatedPartyRecord2 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord2.PR_OH_RelatedParty = relatedPartyOrg2.PK;

			var relatedPartyRecord3 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord3.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord3.PR_OH_RelatedParty = relatedPartyOrg2.PK;

			var relatedPartyRecord4 = parentOrg.ConsigneeRelatedParties.AddNew();
			relatedPartyRecord4.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			relatedPartyRecord4.PR_OH_RelatedParty = relatedPartyOrg3.PK;

			relatedPartyRecord1.RunPreSaveValidation();
			relatedPartyRecord2.RunPreSaveValidation();
			relatedPartyRecord3.RunPreSaveValidation();
			relatedPartyRecord4.RunPreSaveValidation();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);
				AssertNoErrors(relatedPartyRecord1.PR_OH_RelatedPartyInfo);

				AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord2.PR_OH_RelatedPartyInfo, "There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");

				AssertNoErrors(relatedPartyRecord3.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord3.PR_OH_RelatedPartyInfo, "There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");

				AssertNoErrors(relatedPartyRecord4.PR_PartyTypeInfo);
				AssertNoErrors(relatedPartyRecord4.PR_OH_RelatedPartyInfo);
			});
		}

		#endregion

		#region TestMultipleMANRelatedParties

		public void TestCanHaveMultipleMANRelatedPartiesWithDifferentOrganizations()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedPartyRecord1 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord1.PR_OH_RelatedParty = relatedPartyOrg1.PK;

			var relatedPartyRecord2 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord2.PR_OH_RelatedParty = relatedPartyOrg2.PK;

			relatedPartyRecord1.RunPreSaveValidation();
			relatedPartyRecord2.RunPreSaveValidation();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);
				AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);
			});
		}

		public void TestCanNotHaveMultipleMANRelatedPartiesWithSameOrganization()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg = Factory.NewWithValidTestData<OrgHeader>();

			var relatedPartyRecord1 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord1.PR_OH_RelatedParty = relatedPartyOrg.PK;

			var relatedPartyRecord2 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord2.PR_OH_RelatedParty = relatedPartyOrg.PK;

			relatedPartyRecord1.RunPreSaveValidation();
			relatedPartyRecord2.RunPreSaveValidation();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord1.PR_OH_RelatedPartyInfo,
					"There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");

				AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord2.PR_OH_RelatedPartyInfo,
					"There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");
			});
		}

		public void TestCanNotHaveMultipleMANRelatedPartiesWithAtLeastOneSameOrganization()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg3 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedPartyRecord1 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord1.PR_OH_RelatedParty = relatedPartyOrg1.PK;

			var relatedPartyRecord2 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord2.PR_OH_RelatedParty = relatedPartyOrg2.PK;

			var relatedPartyRecord3 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord3.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord3.PR_OH_RelatedParty = relatedPartyOrg2.PK;

			var relatedPartyRecord4 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord4.PR_PartyType = RelatedPartyTypeList.Codes.Manufacturer;
			relatedPartyRecord4.PR_OH_RelatedParty = relatedPartyOrg3.PK;

			relatedPartyRecord1.RunPreSaveValidation();
			relatedPartyRecord2.RunPreSaveValidation();
			relatedPartyRecord3.RunPreSaveValidation();
			relatedPartyRecord4.RunPreSaveValidation();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);
				AssertNoErrors(relatedPartyRecord1.PR_OH_RelatedPartyInfo);

				AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord2.PR_OH_RelatedPartyInfo,
					"There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");

				AssertNoErrors(relatedPartyRecord3.PR_PartyTypeInfo);
				AssertHasError(relatedPartyRecord3.PR_OH_RelatedPartyInfo,
					"There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");

				AssertNoErrors(relatedPartyRecord4.PR_PartyTypeInfo);
				AssertNoErrors(relatedPartyRecord4.PR_OH_RelatedPartyInfo);
			});
		}

		#endregion

		#region TestMultipleICSRelatedParties

		public void TestCanHaveMultipleICSRelatedPartiesWithDifferentOrganizationsTransportModeAndLocation()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedPartyOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedPartyRecord1 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			relatedPartyRecord1.PR_OH_RelatedParty = relatedPartyOrg1.PK;
			relatedPartyRecord1.PR_RN_NKImporterCountry = "AU";
			relatedPartyRecord1.PR_Location = "AUSYD";

			var relatedPartyRecord2 = parentOrg.AllRelatedParties.AddNew();
			relatedPartyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			relatedPartyRecord2.PR_OH_RelatedParty = relatedPartyOrg2.PK;
			relatedPartyRecord2.PR_RN_NKImporterCountry = "NZ";
			relatedPartyRecord2.PR_Location = "NZAKL";

			relatedPartyRecord1.RunPreSaveValidation();
			relatedPartyRecord2.RunPreSaveValidation();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoErrors(relatedPartyRecord1.PR_PartyTypeInfo);
				AssertNoErrors(relatedPartyRecord2.PR_PartyTypeInfo);
			});
		}

		public void TestCanNotHaveMultipleICSRelatedPartiesWithSameTransportModeAndLocation()
		{
			var errorMessage = "Only one Self-Filer is allowed for the same Transport Mode, Location and Import Country/Region fields combination.";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "org";
			var relationShipOrg1 = Factory.New<OrgHeader>();
			relationShipOrg1.OH_Code = "org2";
			var relationShipOrg2 = Factory.New<OrgHeader>();
			relationShipOrg2.OH_Code = "org3";

			var existingRelationShip = Factory.New<OrgRelatedParty>();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_Parent = org.PK;
			existingRelationShip.PR_OH_RelatedParty = relationShipOrg1.PK;
			existingRelationShip.PR_RN_NKImporterCountry = "AU";
			existingRelationShip.PR_Location = "AUSYD";
			existingRelationShip.PR_FreightTransportMode = Core.Constants.TransportModes.All;
			AssertNoError(existingRelationShip.PR_PartyTypeInfo, errorMessage);

			var newRelationship = Factory.New<OrgRelatedParty>();
			newRelationship.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			newRelationship.PR_OH_Parent = org.PK;
			newRelationship.PR_OH_RelatedParty = relationShipOrg1.PK;
			newRelationship.PR_RN_NKImporterCountry = "AU";
			newRelationship.PR_Location = "AUSYD";
			newRelationship.PR_FreightTransportMode = Core.Constants.TransportModes.All;

			newRelationship.RunPreSaveValidation();
			AssertHasError(newRelationship.PR_PartyTypeInfo, errorMessage);
			AssertHasError(newRelationship.PR_FreightTransportModeInfo, errorMessage);
			AssertHasError(newRelationship.PR_RN_NKImporterCountryInfo, errorMessage);
			AssertNoError(newRelationship.PR_FreightContainerModeInfo, errorMessage);
			AssertNoError(newRelationship.PR_FreightDirectionInfo, errorMessage);

			newRelationship.PR_OH_RelatedParty = relationShipOrg2.PK;
			newRelationship.RunPreSaveValidation();
			AssertHasError(newRelationship.PR_PartyTypeInfo, errorMessage);
			AssertHasError(newRelationship.PR_FreightTransportModeInfo, errorMessage);
			AssertHasError(newRelationship.PR_RN_NKImporterCountryInfo, errorMessage);
			AssertNoError(newRelationship.PR_FreightContainerModeInfo, errorMessage);
			AssertNoError(newRelationship.PR_FreightDirectionInfo, errorMessage);

			newRelationship.PR_Location = "NZAKL";
			newRelationship.RunPreSaveValidation();
			AssertNoError(newRelationship.PR_PartyTypeInfo, errorMessage);
			AssertNoError(newRelationship.PR_FreightTransportModeInfo, errorMessage);
			AssertNoError(newRelationship.PR_RN_NKImporterCountryInfo, errorMessage);
			AssertNoError(newRelationship.PR_FreightContainerModeInfo, errorMessage);
			AssertNoError(newRelationship.PR_FreightDirectionInfo, errorMessage);
		}

		#endregion

		#region TestCheckPR_OH_RelatedParty

		#region TestCheckPR_OH_RelatedParty

		public void TestCheckPR_OH_RelatedParty()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;

			OrgHeader relOrg = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "HENAUS");
			relOrg.OH_IsBroker = false;

			partyRecord.PR_OH_RelatedParty = relOrg.PK;
			AssertHasError("Party Type: NON Broker Organizations cannot be selected for CAB Type", partyRecord.PR_OH_RelatedPartyInfo, "Party Type: NON Broker Organizations cannot be selected for CAB Type");
		}

		#endregion

		#region TestCheckPR_OH_RelatedParty_NationalDistributionCenter

		public void TestCheckPR_OH_RelatedParty_NationalDistributionCenter()
		{
			var distributionCenter = SetupDistributionCenter(true);
			var nonDistributionCenter = SetupDistributionCenter(false);
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.NationalDistributionCentre;
			relatedParty.PR_OH_RelatedParty = distributionCenter.PK;
			AssertNoErrors("Since it's a distribution center there shouldn't be any errors.", relatedParty.PR_OH_RelatedPartyInfo);

			relatedParty.PR_OH_RelatedParty = nonDistributionCenter.PK;
			AssertHasError("Since it's not a distribution center there should be errors.", relatedParty.PR_OH_RelatedPartyInfo, "Enter a Distribution Center.");

			relatedParty.PR_OH_RelatedParty = organisation.PK;
			AssertHasError("Since it's not a distribution center there should be errors.", relatedParty.PR_OH_RelatedPartyInfo, "Enter a Distribution Center.");
		}

		OrgHeader SetupDistributionCenter(bool isDistributionCenter)
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsMiscFreightServices = true;
			organisation.OH_IsDistributionCentre = isDistributionCenter;
			return organisation;
		}

		#endregion

		public void TestCheckPR_OH_RelatedParty_IsNotSelfReferringManagementRelation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var relation1 = Factory.New<OrgRelatedParty>();
			relation1.PR_OH_Parent = org1.PK;
			relation1.PR_OH_RelatedParty = org2.PK;
			relation1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			AssertNoErrors(relation1.PR_OH_RelatedPartyInfo);

			relation1.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation1.PR_OH_RelatedPartyInfo, "You cannot have this organization be a party of itself for this type of party.");

			relation1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relation1.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation1.PR_OH_RelatedPartyInfo, "You cannot have this organization be a party of itself for this type of party.");

			relation1.PR_OH_RelatedParty = org2.PK;
			AssertNoErrors(relation1.PR_OH_RelatedPartyInfo);

			relation1.PR_GC = GlbCompany.CurrentCompany.PK;
			relation1.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation1.PR_OH_RelatedPartyInfo, "You cannot have this organization be a party of itself for this type of party.");

			relation1.PR_OH_RelatedParty = org2.PK;
			AssertNoErrors(relation1.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_NoCycles()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TEST111";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TEST222";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "TEST333";

			Factory.Save();

			var relation12 = Factory.New<OrgRelatedParty>();
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation12.PR_OH_Parent = org1.PK;
			relation12.PR_OH_RelatedParty = org2.PK;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			Factory.Save();

			var relation21 = Factory.New<OrgRelatedParty>();
			relation21.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation21.PR_OH_Parent = org2.PK;
			relation21.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation21.PR_OH_RelatedPartyInfo, @"Management Grouping Error: Organization (TEST111) is already a related descendant of Organization (TEST222).
Making Organization (TEST111) the parent of Organization (TEST222) would create an illegal cycle.

Conflicting relationship sequence: Organization (TEST222) > Organization (TEST111)");

			relation21.PR_OH_RelatedParty = org3.PK;
			AssertNoErrors(relation21.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_CAG_ToOrgProxy_EnableControllingAgentFunctionalityAndValidationsIsTrue()
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsControllingAgent = true;
			org2.OH_IsForwarder = true;

			var companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			companyOrgProxy.OH_IsControllingAgent = true;
			companyOrgProxy.OH_IsForwarder = true;

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_IsControllingAgent = true;
			branchOrgProxy.OH_IsForwarder = true;
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			Factory.Save();

			var relation12 = Factory.New<OrgRelatedParty>();
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relation12.PR_OH_Parent = org1.PK;
			relation12.PR_OH_RelatedParty = companyOrgProxy.PK;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			Factory.Save();

			relation12.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relation12.PR_OH_Parent = org1.PK;
			relation12.PR_OH_RelatedParty = branchOrgProxy.PK;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			Factory.Save();

			relation12.PR_OH_RelatedParty = org2.PK;
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, "You can only set an Organization Proxy of the current Login Company as a Controlling Agent, based on your current security rights.");

			relation12.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_CAG_ToOrgProxy_EnableControllingAgentFunctionalityAndValidationsIsFalse()
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed = false;
			Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;
			Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed = true;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg2";

			var companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_IsControllingAgent = true;
			branchOrgProxy.OH_IsForwarder = true;
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_OH_OrgProxy = branchOrgProxy.PK;

			Factory.Save();

			var relation12 = Factory.New<OrgRelatedParty>();
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relation12.PR_OH_Parent = org1.PK;
			relation12.PR_OH_RelatedParty = companyOrgProxy.PK;

			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			relation12.PR_OH_RelatedParty = branchOrgProxy.PK;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			relation12.PR_OH_RelatedParty = org2.PK;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			relation12.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_CAG_MustRelateToControllingAgentAndForwarderOrganization_EnableControllingAgentFunctionalityAndValidationsIsTrue()
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsControllingAgent = false;
			org2.OH_IsForwarder = false;

			Factory.Save();

			var relation12 = GetRelation(RelatedPartyTypeList.Codes.ControllingAgent, org1.PK, org2.PK);

			AssertHasError(relation12.PR_OH_RelatedPartyInfo, "Only an organization flagged as Controlling Agent and Forwarder/Agent can be used as a Controlling Agent.");

			org2.OH_IsControllingAgent = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, "Only an organization flagged as Controlling Agent and Forwarder/Agent can be used as a Controlling Agent.");

			org2.OH_IsForwarder = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			org2.OH_IsControllingAgent = false;
			relation12 = GetRelation(RelatedPartyTypeList.Codes.ControllingAgent, org1.PK, org2.PK); // need to recreate the relation for testing as it is dropped when set OH_IsControllingAgent to false
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, "Only an organization flagged as Controlling Agent and Forwarder/Agent can be used as a Controlling Agent.");

			relation12.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_CAG_MustRelateToControllingAgentAndForwarderOrganization_EnableControllingAgentFunctionalityAndValidationsIsFalse()
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsControllingAgent = false;
			org2.OH_IsForwarder = false;

			Factory.Save();

			var relation12 = Factory.New<OrgRelatedParty>();
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relation12.PR_OH_Parent = org1.PK;
			relation12.PR_OH_RelatedParty = org2.PK;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			org2.OH_IsControllingAgent = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			org2.OH_IsForwarder = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			org2.OH_IsControllingAgent = false;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			relation12.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_CCB_MustRelateToControllingCustomerOrganization_EnableControllingCustomerFunctionalityAndValidationsIsTrue()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsControllingCustomer = false;

			Factory.Save();

			var relation12 = GetRelation(RelatedPartyTypeList.Codes.ControllingCustomer, org1.PK, org2.PK);
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, "Only an organization flagged as Controlling Customer can be used as a Controlling Customer.");

			org2.OH_IsControllingCustomer = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			org2.OH_IsControllingCustomer = false;
			relation12 = GetRelation(RelatedPartyTypeList.Codes.ControllingCustomer, org1.PK, org2.PK); // need to recreate the relation for testing as it is dropped when set OH_IsControllingCustomer to false
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_CCB_MustRelateToControllingCustomerOrganization_EnableControllingCustomerFunctionalityAndValidationsIsFalse()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsControllingCustomer = false;

			Factory.Save();

			var relation12 = Factory.New<OrgRelatedParty>();
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			relation12.PR_OH_Parent = org1.PK;
			relation12.PR_OH_RelatedParty = org2.PK;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			org2.OH_IsControllingCustomer = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);

			org2.OH_IsControllingCustomer = false;
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_PartyType_ACG()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTA";

			RefCountry country1 = Factory.NewWithValidTestData<RefCountry>();
			country1.RN_Code = "Z1";
			RefUNLOCO unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco1.RL_RN_NKCountryCode = country1.Code;
			org1.OH_RL_NKClosestPort = unloco1.RL_Code;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_RL_NKClosestPort = "TESTB";
			RefCountry country2 = Factory.NewWithValidTestData<RefCountry>();
			country2.RN_Code = "Z2";
			RefUNLOCO unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco2.RL_RN_NKCountryCode = country2.Code;
			org2.OH_RL_NKClosestPort = unloco2.RL_Code;

			Factory.Save();

			var relation12 = Factory.New<OrgRelatedParty>();
			relation12.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			relation12.PR_OH_Parent = org1.PK;
			relation12.PR_OH_RelatedParty = org2.PK;
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, "The AR/AP Organization must have a UNLOCO from the SAME country/region as the Related Party");

			var relation13 = Factory.New<OrgRelatedParty>();
			relation13.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			relation13.PR_OH_Parent = org1.PK;
			relation13.PR_OH_RelatedParty = org1.PK;
			AssertNoError(relation13.PR_OH_RelatedPartyInfo, "The AR/AP Organization must have a UNLOCO from the SAME country/region as the Related Party");

			var relation14 = Factory.New<OrgRelatedParty>();
			relation14.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			AssertNoError(relation14.PR_OH_RelatedPartyInfo, "The AR/AP Organization must have a UNLOCO from the SAME country/region as the Related Party");

			var relation15 = Factory.New<OrgRelatedParty>();
			relation15.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			relation15.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation15.PR_OH_RelatedPartyInfo, "The Related Party must be an Organization Proxy of the current Login Company");

			var relation16 = Factory.New<OrgRelatedParty>();
			relation16.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			relation16.PR_OH_RelatedParty = GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy;
			AssertNoError(relation16.PR_OH_RelatedPartyInfo, "The Related Party must be an Organization Proxy of the current Login Company");
		}

		public void TestCheckPR_OH_RelatedParty_SPC_MustRelateToPayablesOrganization()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsDebtor = false;

			Factory.Save();

			var relation12 = GetRelation(RelatedPartyTypeList.Codes.ServiceProviderCreditor, org1.PK, org2.PK);
			relation12.CompanyLevel = CompanyLevelList.Codes.ENT;
			AssertNoErrors("Being payable should NOT be checked at the system level", relation12.PR_OH_RelatedPartyInfo);

			relation12.CompanyLevel = CompanyLevelList.Codes.COM;
			AssertHasError("Being payable should be checked at the company level", relation12.PR_OH_RelatedPartyInfo, $"Only an organization flagged as Payable can be used as an {RelatedPartyTypeList.Descriptions.ServiceProviderCreditor}.");

			org2.OH_IsCreditor = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors("Being payable should be checked at the company level", relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_IWT_MustRelateToReceivablesOrganization()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsDebtor = false;

			Factory.Save();

			var relation12 = GetRelation(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, org1.PK, org2.PK);
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, $"Only an organization flagged as Receivable can be used as an {RelatedPartyTypeList.Descriptions.InvoiceWarehouseJobsTo}.");

			org2.OH_IsDebtor = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_ICT_MustRelateToReceivablesOrganization()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsDebtor = false;

			Factory.Save();

			var relation12 = GetRelation(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, org1.PK, org2.PK);
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, $"Only an organization flagged as Receivable can be used as an {RelatedPartyTypeList.Descriptions.InvoiceCustomsJobsTo}.");

			org2.OH_IsDebtor = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_IFT_MustRelateToReceivablesOrganization()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsDebtor = false;

			Factory.Save();

			var relation12 = GetRelation(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, org1.PK, org2.PK);
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, $"Only an organization flagged as Receivable can be used as an {RelatedPartyTypeList.Descriptions.InvoiceFreightJobsTo}.");

			org2.OH_IsDebtor = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_LocalTransportProvider()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TestOrg1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TestOrg2";
			org2.OH_IsShippingProvider = false;
			org2.OH_IsLocalTransport = false;

			Factory.Save();
			var relation12 = GetRelation(RelatedPartyTypeList.Codes.LocalTransport, org1.PK, org2.PK);
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, $"Only an organization set up as a Carrier and flagged as Road Transport can be used as a {RelatedPartyTypeList.Descriptions.LocalTransport}.");

			org2.OH_IsShippingProvider = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertHasError(relation12.PR_OH_RelatedPartyInfo, $"Only an organization set up as a Carrier and flagged as Road Transport can be used as a {RelatedPartyTypeList.Descriptions.LocalTransport}.");

			org2.OH_IsLocalTransport = true;
			relation12.Validation.ValidatePR_OH_RelatedParty();
			AssertNoErrors(relation12.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_WhenRelatedPartyIsProductParent() => CombineAssertions(() => 
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "org3";

			var existingRelationship = Factory.New<OrgRelatedParty>();
			existingRelationship.PR_OH_Parent = org1.PK;
			existingRelationship.PR_OH_RelatedParty = org2.PK;
			existingRelationship.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertNoErrors(existingRelationship.PR_OH_RelatedPartyInfo);

			var newRelationship = Factory.New<OrgRelatedParty>();
			newRelationship.PR_OH_Parent = org2.PK;
			newRelationship.PR_OH_RelatedParty = org1.PK;
			newRelationship.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertHasError("Can't choose PPT which is a Product Parent", newRelationship.PR_OH_RelatedPartyInfo, "Related Part is a Product Parent, please select an organization that is not already a Parent");
			newRelationship.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			AssertNoErrorContaining(newRelationship.PR_OH_RelatedPartyInfo, "Related Part is a Product Parent, please select an organization that is not already a Parent");

			var newRelationship1 = Factory.New<OrgRelatedParty>();
			newRelationship1.PR_OH_Parent = org3.PK;
			newRelationship1.PR_OH_RelatedParty = org1.PK;
			newRelationship1.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertHasError("Can't choose PPT which is a Product Parent", newRelationship1.PR_OH_RelatedPartyInfo, "Related Part is a Product Parent, please select an organization that is not already a Parent");
			newRelationship1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			AssertNoErrorContaining(newRelationship1.PR_OH_RelatedPartyInfo, "Related Part is a Product Parent, please select an organization that is not already a Parent");
		});

		public void TestCheckPR_OH_RelatedParty_WhenRelatedPartyChooseItselfAsProductParent() => CombineAssertions(() =>
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "org";
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = org.PK;
			relatedParty.PR_OH_RelatedParty = org.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertHasError("Can't choose itself as PPT", relatedParty.PR_OH_RelatedPartyInfo, "Related Part is a Product Parent, please select an organization that is not already a Parent");

			relatedParty.PR_OH_RelatedParty = org1.PK;
			AssertNoErrors(relatedParty.PR_OH_RelatedPartyInfo);
		});

		public void TestCheckPR_OH_RelatedParty_WhenHasMultiplePPT() => CombineAssertions(() =>
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "org3";
			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = org1.PK;
			relatedParty.PR_OH_RelatedParty = org2.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertNoErrors(relatedParty.PR_OH_RelatedPartyInfo);

			var relatedParty1 = Factory.New<OrgRelatedParty>();
			relatedParty1.PR_OH_Parent = org1.PK;
			relatedParty1.PR_OH_RelatedParty = org3.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertNoErrors(relatedParty1.PR_OH_RelatedPartyInfo);
		});

		public void TestCheckPR_OH_RelatedParty_ShouldHaveOnlyOneParent() => CombineAssertions(() =>
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "org3";
			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = org1.PK;
			relatedParty.PR_OH_RelatedParty = org3.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;

			var relatedParty1 = Factory.New<OrgRelatedParty>();
			relatedParty1.PR_OH_Parent = org2.PK;
			relatedParty1.PR_OH_RelatedParty = org3.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			AssertHasError("Can't have multi parent", relatedParty1.PR_OH_RelatedPartyInfo, "Organization already has a Product Parent, please select an organization that does not have a product parent");
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			AssertNoErrors(relatedParty1.PR_OH_RelatedPartyInfo);
		});

		#endregion

		#region TestCheckPR_OA

		public void TestCheckPR_OA()
		{
			var partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			AssertEquals("Should have no warning", false, partyRecord.PR_OAInfo.HasWarnings());

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			AssertEquals("For Address should be defaulted from org main address", partyRecord.PR_OA, partyRecord.Parent.MainAddress.PK);

			partyRecord.PR_OA = ZGuid.Empty;
			AssertEquals("Should have warning", true, partyRecord.PR_OAInfo.HasWarnings());

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupFrom;
			partyRecord.PR_OA = ZGuid.Empty;
			AssertEquals("Should have no warning", false, partyRecord.PR_OAInfo.HasWarnings());

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryTo;
			partyRecord.PR_OA = ZGuid.Empty;
			AssertEquals("Should have no warning", false, partyRecord.PR_OAInfo.HasWarnings());

			partyRecord.PR_OA = partyRecord.Parent.MainAddress.PK;
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			AssertNoErrors(partyRecord.PR_OAInfo);

			partyRecord.PR_OA = ZGuid.Empty;
			AssertHasError(partyRecord.PR_OAInfo, "Enter a Warehouse Address.");

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NationalDistributionCentre;
			partyRecord.PR_OA = ZGuid.Empty;
			AssertHasError(partyRecord.PR_OAInfo, "Enter a 'For Address' which National Distribution Center is related to.");

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NationalDistributionCentre;
			partyRecord.PR_OA = ZGuid.Invalid;
			AssertHasError(partyRecord.PR_OAInfo, "Enter a valid For Address.");

			partyRecord.PR_OA = partyRecord.Parent.MainAddress.PK;
			AssertNoErrors(partyRecord.PR_OAInfo);
		}

		#endregion

		#region TestValidateIsInCollectionAlready

		public void TestValidateIsInCollectionAlready()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ParentOrg";
			OrgHeader relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "relatedParty";

			org.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Pickup);

			OrgRelatedPartyCompanySpecificCollection parties = org.AllRelatedParties;

			OrgRelatedParty partyRecord1 = parties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord1.PR_OH_RelatedParty = relatedParty.PK;

			partyRecord1.Validation.ValidatePR_PartyType();
			AssertHasError(partyRecord1.PR_PartyTypeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");

			partyRecord1.PR_Location = "AUBNE";
			partyRecord1.Validation.ValidatePR_PartyType();
			AssertNoErrors(partyRecord1.PR_PartyTypeInfo);
		}

		public void TestValidateIsRelatedPartyInCollectionAlready_CAV()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			{
				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_Code = "ParentOrg";
				OrgHeader relatedParty1 = Factory.New<OrgHeader>();
				relatedParty1.OH_Code = "Party1";
				var impAddInfo = relatedParty1.GetCountryData(Core.Constants.CountryCodes.Canada).ImpAddInfo;

				org.SetRelatedParty(relatedParty1, RelatedPartyTypeList.Codes.CSAApprovedVendor, ZString.Empty);

				OrgRelatedPartyCompanySpecificCollection parties = org.AllRelatedParties;

				OrgRelatedParty partyRecord1 = parties.AddNew();
				partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedVendor;
				partyRecord1.PR_FreightDirection = ZString.Empty;
				partyRecord1.PR_OH_RelatedParty = relatedParty1.PK;

				partyRecord1.Validation.ValidatePR_PartyType();
				AssertHasError(partyRecord1.PR_OH_RelatedPartyInfo, "There is already a party with the same type, address, direction, mode, company level, UNLOCO and related party. You can only specify a single related organization for this combination.");

				OrgHeader relatedParty2 = Factory.New<OrgHeader>();
				relatedParty2.OH_Code = "Party2";
				var impAddInfo2 = relatedParty2.GetCountryData(Core.Constants.CountryCodes.Canada).ImpAddInfo;
				partyRecord1.PR_OH_RelatedParty = relatedParty2.PK;
				AssertNoErrors(partyRecord1.PR_OH_RelatedPartyInfo);
			}
		}

		public void TestValidateIsRelatedPartyInCollectionAlready_SPC()
		{
			OrgHeader parentOrg = Factory.New<OrgHeader>();
			parentOrg.OH_Code = "ParentOrg";

			OrgHeader relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "Party1";
			relatedParty.OH_IsCreditor = true;

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Name = Constants.CountryCodes.UnitedStates + " Company";
			company1.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;

			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = company1.GC_Code;
			branch1.GB_BranchName = "USLAX Branch";
			branch1.GB_RL_NKHomePort = "USLAX";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var org = Factory.Load<OrgHeader>(parentOrg.PK);
				OrgRelatedPartyCompanySpecificCollection parties = org.AllRelatedParties;

				OrgRelatedParty partyRecord1 = parties.AddNew();
				partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
				partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
				partyRecord1.PR_FreightTransportMode = Constants.TransportModes.Sea;
				partyRecord1.PR_FreightContainerMode = Constants.ContainerModes.FCL;
				partyRecord1.PR_Location = "USLAX";
				partyRecord1.PR_OH_RelatedParty = relatedParty.PK;
				partyRecord1.CompanyLevel = CompanyLevelList.Codes.ENT;

				OrgRelatedParty partyRecord2 = parties.AddNew();
				partyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
				partyRecord2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
				partyRecord2.PR_FreightTransportMode = Constants.TransportModes.Sea;
				partyRecord2.PR_FreightContainerMode = Constants.ContainerModes.FCL;
				partyRecord2.PR_Location = "USLAX";
				partyRecord2.PR_OH_RelatedParty = relatedParty.PK;
				partyRecord2.CompanyLevel = CompanyLevelList.Codes.ENT;

				partyRecord2.Validation.ValidatePR_PartyType();
				AssertHasError(partyRecord2.PR_PartyTypeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");

				partyRecord2.CompanyLevel = CompanyLevelList.Codes.COM;
				partyRecord2.Validation.ValidatePR_PartyType();
				AssertNoErrors(partyRecord2.PR_PartyTypeInfo);

				OrgRelatedParty partyRecord3 = parties.AddNew();
				partyRecord3.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
				partyRecord3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
				partyRecord3.PR_FreightTransportMode = Constants.TransportModes.Sea;
				partyRecord3.PR_FreightContainerMode = Constants.ContainerModes.FCL;
				partyRecord3.PR_Location = "USLAX";
				partyRecord3.PR_OH_RelatedParty = relatedParty.PK;
				partyRecord3.CompanyLevel = CompanyLevelList.Codes.COM;

				partyRecord3.Validation.ValidatePR_PartyType();
				AssertHasError(partyRecord3.PR_PartyTypeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");

				parties.RemoveAndDelete(partyRecord3);

				Factory.Save();
			}

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Name = Constants.CountryCodes.Canada + " Company";
			company2.GC_RN_NKCountryCode = Constants.CountryCodes.Canada;

			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = company2.GC_Code;
			branch2.GB_BranchName = "CATOR Branch";
			branch2.GB_RL_NKHomePort = "CATOR";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var org = new BusinessObjectFactory().Load<OrgHeader>(parentOrg.PK);
				OrgRelatedPartyCompanySpecificCollection parties = org.AllRelatedParties;
				AssertEquals(1, parties.Count);

				OrgRelatedParty partyRecord3 = parties.AddNew();
				partyRecord3.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
				partyRecord3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
				partyRecord3.PR_FreightTransportMode = Constants.TransportModes.Sea;
				partyRecord3.PR_FreightContainerMode = Constants.ContainerModes.FCL;
				partyRecord3.PR_Location = "USLAX";
				partyRecord3.PR_OH_RelatedParty = relatedParty.PK;
				partyRecord3.CompanyLevel = CompanyLevelList.Codes.ENT;

				partyRecord3.Validation.ValidatePR_PartyType();
				AssertHasError(partyRecord3.PR_PartyTypeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");

				partyRecord3.CompanyLevel = CompanyLevelList.Codes.COM;
				partyRecord3.Validation.ValidatePR_PartyType();
				AssertNoErrors(partyRecord3.PR_PartyTypeInfo);
			}
		}

		public void TestValidateIsRelatedPartyInCollectionAlready_NFP()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ParentOrg";
			OrgHeader relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "relatedParty";

			org.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.NotifyParty, RelatedPartyDirectionList.Codes.Pickup);

			OrgRelatedPartyCompanySpecificCollection parties = org.AllRelatedParties;

			OrgRelatedParty partyRecord = parties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NotifyParty;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord.PR_OH_RelatedParty = relatedParty.PK;

			partyRecord.Validation.ValidatePR_PartyType();
			AssertHasError(partyRecord.PR_PartyTypeInfo, "When Party Type is NFP, 'direction, transport mode and container mode' must be unique. You can only specify a single related organization for this combination.");
		}

		#endregion

		#region TestValidateIsInCollectionAlreadyWhenInDifferentCollections

		public void TestValidateIsInCollectionAlreadyWhenInDifferentCollections()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ParentOrg";
			var relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "relatedParty";
			org.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Pickup);

			AssertEquals(1, org.AllRelatedPartiesView.Count);
			var partyRecord1 = org.AllRelatedPartiesView[0];

			AssertEquals("Precondition", 0, org.ConsignorRelatedParties.Count);
			var partyRecord2 = org.ConsignorRelatedParties.AddNew();
			partyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			partyRecord2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord2.PR_OH_RelatedParty = relatedParty.PK;
			AssertEquals(1, org.ConsignorRelatedParties.Count);

			AssertEquals("Other collection not refreshed automatically", 1, org.AllRelatedPartiesView.Count);

			partyRecord1.Validation.ValidatePR_PartyType();
			AssertHasError(partyRecord1.PR_PartyTypeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");
			partyRecord2.Validation.ValidatePR_PartyType();
			AssertHasError(partyRecord2.PR_PartyTypeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");

			partyRecord2.PR_Location = "AUBNE";

			partyRecord1.Validation.ValidatePR_PartyType();
			AssertNoErrors(partyRecord1.PR_PartyTypeInfo);
			partyRecord2.Validation.ValidatePR_PartyType();
			AssertNoErrors(partyRecord2.PR_PartyTypeInfo);
		}

		public void TestValidateIsInCollectionAlreadyWhenInDifferentCollections_ForwarderCoLoadWithType()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ParentOrg";
			var relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "relatedParty";
			org.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.ForwarderCoLoadWith, RelatedPartyDirectionList.Codes.Pickup);

			AssertEquals(1, org.AllRelatedPartiesView.Count);
			var partyRecord1 = org.AllRelatedPartiesView[0];

			AssertEquals("Precondition", 0, org.ConsignorRelatedParties.Count);
			var partyRecord2 = org.ConsignorRelatedParties.AddNew();
			partyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
			partyRecord2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord2.PR_OH_RelatedParty = relatedParty.PK;
			AssertEquals(1, org.ConsignorRelatedParties.Count);

			AssertEquals("Other collection not refreshed automatically", 1, org.AllRelatedPartiesView.Count);

			partyRecord1.Validation.ValidatePR_PartyType();
			var errorMessage = "There is already a party with the same type, address, direction, mode, company level, UNLOCO and import country. You can only specify a single related organization for this combination.";
			AssertHasError(partyRecord1.PR_PartyTypeInfo, errorMessage);
			partyRecord2.Validation.ValidatePR_PartyType();
			AssertHasError(partyRecord2.PR_PartyTypeInfo, errorMessage);
			partyRecord2.Validation.ValidatePR_RN_NKImporterCountry();
			AssertHasError(partyRecord2.PR_RN_NKImporterCountryInfo, errorMessage);

			partyRecord2.PR_RN_NKImporterCountry = "SG";

			partyRecord1.Validation.ValidatePR_PartyType();
			AssertNoErrors(partyRecord1.PR_PartyTypeInfo);
			partyRecord2.Validation.ValidatePR_PartyType();
			AssertNoErrors(partyRecord2.PR_PartyTypeInfo);
			partyRecord2.Validation.ValidatePR_RN_NKImporterCountry();
			AssertNoErrors(partyRecord2.PR_RN_NKImporterCountryInfo);
		}

		#endregion

		#region TestCheckPR_FreightTransportMode

		public void TestCheckPR_FreightTransportMode()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_FreightTransportMode = "ABC";
			AssertEquals("Should have errors", true, partyRecord.PR_FreightTransportModeInfo.HasErrors());
			partyRecord.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Should not have errors", false, partyRecord.PR_FreightTransportModeInfo.HasNotifications());
		}

		public void TestCheckPR_FreightTransportMode_IsInCollectionAlready()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			var relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "relatedParty";

			org.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, "");

			var orgRelatedParty1 = org.AllRelatedParties.AddNew();
			orgRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			orgRelatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			orgRelatedParty1.PR_OH_RelatedParty = relatedParty.PK;
			orgRelatedParty1.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			orgRelatedParty1.Validation.ValidatePR_FreightTransportMode();
			AssertHasError(orgRelatedParty1.PR_FreightTransportModeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");

			orgRelatedParty1.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
			orgRelatedParty1.Validation.ValidatePR_FreightTransportMode();
			AssertNoErrors(orgRelatedParty1.PR_FreightTransportModeInfo);
		}

		#endregion

		#region TestCheckPR_FreightContainerMode

		public void TestCheckPR_FreightContainerMode()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_FreightContainerMode = "XXX";
			AssertEquals("Should have errors", true, partyRecord.PR_FreightContainerModeInfo.HasErrors());
			partyRecord.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Should not have errors", false, partyRecord.PR_FreightContainerModeInfo.HasNotifications());

			partyRecord.Validation.ValidatePR_FreightContainerMode();
		}

		public void TestCheckPR_FreightContainerMode_IsInCollectionAlready()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			var relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "relatedParty";

			org.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			var orgRelatedParty1 = org.AllRelatedParties.AddNew();
			orgRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			orgRelatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			orgRelatedParty1.PR_OH_RelatedParty = relatedParty.PK;
			orgRelatedParty1.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			orgRelatedParty1.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;
			orgRelatedParty1.Validation.ValidatePR_FreightContainerMode();
			AssertHasError(orgRelatedParty1.PR_FreightContainerModeInfo, "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.");

			orgRelatedParty1.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;
			orgRelatedParty1.Validation.ValidatePR_FreightContainerMode();
			AssertNoErrors(orgRelatedParty1.PR_FreightContainerModeInfo);
		}

		#endregion

		#region TestCheckCompanyLevel

		public void TestCheckCompanyLevel()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			AssertEquals("Precondition: partyRecord.IsCompanySpecific", true, partyRecord.IsCompanySpecific);
			AssertEquals("Precondition: partyRecord.IsEnterpriseLevelOnly", false, partyRecord.IsEnterpriseLevelOnly);

			partyRecord.CompanyLevel = "ENT";
			partyRecord.Validation.ValidateCompanyLevel();
			AssertHasError(partyRecord.CompanyLevelInfo, "The Company Level must be 'COM' for this type of party.");
			AssertNoError(partyRecord.CompanyLevelInfo, "The Company Level must be 'ENT' for this type of party.");

			partyRecord.CompanyLevel = "COM";
			partyRecord.Validation.ValidateCompanyLevel();
			AssertNoError(partyRecord.CompanyLevelInfo, "The Company Level must be 'COM' for this type of party.");
			AssertNoError(partyRecord.CompanyLevelInfo, "The Company Level must be 'ENT' for this type of party.");

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ExportConsolidationDepot;
			AssertEquals("Precondition: partyRecord.IsCompanySpecific", false, partyRecord.IsCompanySpecific);
			AssertEquals("Precondition: partyRecord.IsEnterpriseLevelOnly", true, partyRecord.IsEnterpriseLevelOnly);

			partyRecord.CompanyLevel = "ENT";
			partyRecord.Validation.ValidateCompanyLevel();
			AssertNoError(partyRecord.CompanyLevelInfo, "The Company Level must be 'COM' for this type of party.");
			AssertNoError(partyRecord.CompanyLevelInfo, "The Company Level must be 'ENT' for this type of party.");

			partyRecord.CompanyLevel = "COM";
			partyRecord.Validation.ValidateCompanyLevel();
			AssertNoError(partyRecord.CompanyLevelInfo, "The Company Level must be 'COM' for this type of party.");
			AssertHasError(partyRecord.CompanyLevelInfo, "The Company Level must be 'ENT' for this type of party.");
		}

		#endregion

		#region TestCheckPR_FreightDirection

		public void TestCheckPR_FreightDirection()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_FreightDirection = ZString.Empty;
			AssertEquals(true, partyRecord.PR_FreightDirectionInfo.HasErrors());

			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			AssertEquals(false, partyRecord.PR_FreightDirectionInfo.HasErrors());
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;

			partyRecord.PR_FreightDirection = "ABC";
			AssertEquals(true, partyRecord.PR_FreightDirectionInfo.HasErrors());

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProvider;
			partyRecord.PR_FreightDirection = "";
			AssertEquals("Should not require direction for Service Provider party type", false, partyRecord.PR_FreightDirectionInfo.HasErrors());

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo;
			partyRecord.PR_FreightDirection = "";
			AssertEquals("Should not require direction for Invoice Warehouse Job To party type", false, partyRecord.PR_FreightDirectionInfo.HasErrors());

			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			partyRecord.PR_FreightDirection = "";
			AssertEquals("Should not require direction for Product Parent party type", false, partyRecord.PR_FreightDirectionInfo.HasErrors());

			var servicePartyRecord = Factory.New<OrgRelatedParty>();
			servicePartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
			servicePartyRecord.PR_FreightDirection = "";
			AssertEquals("Should not require direction for Authorized Cargo Reporter party type", false, servicePartyRecord.PR_FreightDirectionInfo.HasErrors());
		}

		#endregion

		#region TestCheckPartyTypeDescription

		public void TestCheckPartyTypeDescription()
		{
			OrgRelatedParty partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PartyTypeDescription = "ABC";
			partyRecord.Validation.ValidatePartyTypeDescription();
			AssertEquals(true, partyRecord.PartyTypeDescriptionInfo.HasErrors());

			partyRecord.PartyTypeDescription = RelatedPartyTypeList.Descriptions.LocalTransport;
			partyRecord.Validation.ValidatePartyTypeDescription();
			AssertEquals(false, partyRecord.PartyTypeDescriptionInfo.HasErrors());
		}

		#endregion

		#region TestCheckPR_RN_NKImporterCountry

		public void TestCheckPR_RN_NKImporterCountry()
		{
			var partyRecord = Factory.New<OrgRelatedParty>();
			partyRecord.PR_OH_Parent = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG").PK;
			partyRecord.PR_RN_NKImporterCountry = ZString.Empty;
			partyRecord.Validation.ValidatePR_RN_NKImporterCountry();
			AssertNoErrors(partyRecord.PR_RN_NKImporterCountryInfo);

			partyRecord.PR_RN_NKImporterCountry = "XX";
			partyRecord.Validation.ValidatePR_RN_NKImporterCountry();
			AssertHasError(partyRecord.PR_RN_NKImporterCountryInfo, "Enter a valid Import Country/Region.");

			partyRecord.PR_RN_NKImporterCountry = "SG";
			partyRecord.Validation.ValidatePR_RN_NKImporterCountry();
			AssertNoErrors(partyRecord.PR_RN_NKImporterCountryInfo);
		}

		#endregion

		#region TestHelpers
		OrgRelatedParty GetRelation(ZString partyTypeCode, ZGuid org1PK, ZGuid org2PK)
		{
			var relation = Factory.New<OrgRelatedParty>();
			relation.PR_PartyType = partyTypeCode;
			relation.PR_OH_Parent = org1PK;
			relation.PR_OH_RelatedParty = org2PK;
			return relation;
		}
		#endregion
	}
}
