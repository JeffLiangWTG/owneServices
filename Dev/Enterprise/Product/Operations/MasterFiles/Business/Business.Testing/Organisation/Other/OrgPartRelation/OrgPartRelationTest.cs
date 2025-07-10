using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartRelation))]
	sealed class OrgPartRelationTest : EnterpriseBusinessObjectTestCase
	{
		#region IsPartAttribReleaseCaptured

		#region TestOU_IsSerialNumberReleaseCaptured_ReadOnly

		public void TestOU_IsSerialNumberReleaseCaptured_ReadOnly()
		{
			var orgPartRelation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition:", false, orgPartRelation.OU_IsSerialNumberReleaseCaptured);
			AssertEquals(true, orgPartRelation.OU_IsSerialNumberReleaseCapturedInfo.ReadOnly);

			orgPartRelation.OU_UseSerialNumber = true;
			AssertEquals(false, orgPartRelation.OU_IsSerialNumberReleaseCapturedInfo.ReadOnly);
		}

		#endregion

		#region TestOU_IsSerialNumberReleaseCaptured_CallsValidation

		public void TestOU_IsSerialNumberReleaseCaptured_CallsValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_IMUseSerialNumber = true;

			var orgPartRelation = Factory.New<OrgPartRelation>();
			orgPartRelation.OU_OH = org.PK;

			orgPartRelation.OU_UseSerialNumber = true;
			orgPartRelation.OU_RFAttributeConfirm = "SER";

			orgPartRelation.OU_IsSerialNumberReleaseCaptured = false;
			AssertNoErrors(orgPartRelation.OU_RFAttributeConfirmInfo);

			orgPartRelation.OU_IsSerialNumberReleaseCaptured = true;
			AssertHasError(orgPartRelation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			orgPartRelation.OU_IsSerialNumberReleaseCaptured = false;
			AssertNoErrors(orgPartRelation.OU_RFAttributeConfirmInfo);
		}

		#endregion

		#region TestOU_IsPartAttrib1ReleaseCaptured_ReadOnly

		public void TestOU_IsPartAttrib1ReleaseCaptured_ReadOnly()
		{
			AssertIsPartAttribReleaseCaptured_ReadOnly(OrgPartRelationSchema.OU_UsePartAttrib1, relation => relation.OU_IsPartAttrib1ReleaseCapturedInfo);
		}

		#endregion

		#region TestOU_IsPartAttrib2ReleaseCaptured_ReadOnly

		public void TestOU_IsPartAttrib2ReleaseCaptured_ReadOnly()
		{
			AssertIsPartAttribReleaseCaptured_ReadOnly(OrgPartRelationSchema.OU_UsePartAttrib2, relation => relation.OU_IsPartAttrib2ReleaseCapturedInfo);
		}

		#endregion

		#region TestOU_IsPartAttrib3ReleaseCaptured_ReadOnly

		public void TestOU_IsPartAttrib3ReleaseCaptured_ReadOnly()
		{
			AssertIsPartAttribReleaseCaptured_ReadOnly(OrgPartRelationSchema.OU_UsePartAttrib3, relation => relation.OU_IsPartAttrib3ReleaseCapturedInfo);
		}

		#endregion

		#region TestOU_UseSerialNumberSetsIsSerialNumberReleaseCaptured

		public void TestOU_UseSerialNumberSetsIsSerialNumberReleaseCaptured()
		{
			var orgPartRelation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition:", false, orgPartRelation.OU_UseSerialNumber);
			AssertEquals(false, orgPartRelation.OU_IsSerialNumberReleaseCaptured);

			orgPartRelation.OU_UseSerialNumber = true;
			AssertEquals(false, orgPartRelation.OU_IsSerialNumberReleaseCaptured);

			orgPartRelation.OU_UseSerialNumber = false;
			AssertEquals(false, orgPartRelation.OU_IsSerialNumberReleaseCaptured);

			orgPartRelation.OU_UseSerialNumber = true;
			orgPartRelation.OU_IsSerialNumberReleaseCaptured = true;
			AssertEquals(true, orgPartRelation.OU_IsSerialNumberReleaseCaptured);

			orgPartRelation.OU_UseSerialNumber = false;
			AssertEquals(false, orgPartRelation.OU_IsSerialNumberReleaseCaptured);

			orgPartRelation.OU_IsSerialNumberReleaseCaptured = true;
			orgPartRelation.OU_UseSerialNumber = true;
			AssertEquals(true, orgPartRelation.OU_IsSerialNumberReleaseCaptured);
		}

		#endregion

		#region TestOU_UsePartAttrib1SetsIsPartAttrib1ReleaseCaptured

		public void TestOU_UsePartAttrib1SetsIsPartAttrib1ReleaseCaptured()
		{
			AssertUsePartAttribSetsIsPartAttribReleaseCapturedToFalseWhenSetToFalse(OrgPartRelationSchema.OU_UsePartAttrib1, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured);
		}

		#endregion

		#region TestOU_UsePartAttrib2SetsIsPartAttrib2ReleaseCaptured

		public void TestOU_UsePartAttrib2SetsIsPartAttrib2ReleaseCaptured()
		{
			AssertUsePartAttribSetsIsPartAttribReleaseCapturedToFalseWhenSetToFalse(OrgPartRelationSchema.OU_UsePartAttrib2, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured);
		}

		#endregion

		#region TestOU_UsePartAttrib3SetsIsPartAttrib3ReleaseCaptured

		public void TestOU_UsePartAttrib3SetsIsPartAttrib3ReleaseCaptured()
		{
			AssertUsePartAttribSetsIsPartAttribReleaseCapturedToFalseWhenSetToFalse(OrgPartRelationSchema.OU_UsePartAttrib3, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured);
		}

		#endregion

		void AssertIsPartAttribReleaseCaptured_ReadOnly(SchemaColumn useAttributeColumn, Func<OrgPartRelation, ZPropertyInfo> getPropertyInfo)
		{
			var orgPartRelation = Factory.New<OrgPartRelation>();
			var propertyInfo = getPropertyInfo(orgPartRelation);
			AssertEquals("Precondition:", false, orgPartRelation[useAttributeColumn]);
			AssertEquals(true, propertyInfo.ReadOnly);

			orgPartRelation[useAttributeColumn] = true;
			AssertEquals(false, propertyInfo.ReadOnly);
		}

		void AssertUsePartAttribSetsIsPartAttribReleaseCapturedToFalseWhenSetToFalse(SchemaBoolColumn usePartAttributeColumn, SchemaBoolColumn isPartAttribReleaseCapturedColumn)
		{
			var orgPartRelation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition:", false, orgPartRelation[usePartAttributeColumn]);
			AssertEquals(false, orgPartRelation[isPartAttribReleaseCapturedColumn]);

			orgPartRelation[usePartAttributeColumn] = true;
			AssertEquals(false, orgPartRelation[isPartAttribReleaseCapturedColumn]);

			orgPartRelation[usePartAttributeColumn] = false;
			AssertEquals(false, orgPartRelation[isPartAttribReleaseCapturedColumn]);

			orgPartRelation[usePartAttributeColumn] = true;
			orgPartRelation[isPartAttribReleaseCapturedColumn] = true;
			AssertEquals(true, orgPartRelation[isPartAttribReleaseCapturedColumn]);

			orgPartRelation[usePartAttributeColumn] = false;
			AssertEquals(false, orgPartRelation[isPartAttribReleaseCapturedColumn]);
		}

		#endregion

		#region TestOU_UseSerialNumber_ReadOnly

		public void TestOU_UseSerialNumber_ReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgPartRelation = Factory.New<OrgPartRelation>();
			orgPartRelation.OU_OH = org.PK;

			org.MiscServ.OM_IMUseSerialNumber = false;
			AssertEquals(true, orgPartRelation.OU_UseSerialNumberInfo.ReadOnly);

			org.MiscServ.OM_IMUseSerialNumber = true;
			AssertEquals(false, orgPartRelation.OU_UseSerialNumberInfo.ReadOnly);
		}

		#endregion

		#region TestLoggingRelationshipEditsAgainstParent

		public void TestLoggingRelationshipEditsAgainstParent()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Daniel";
			org1.OH_Code = "DANSEXY";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "James";
			org2.OH_Code = "POOPY";
			var part = OrgSupplierPart.New(Factory);
			var pivot1 = part.RelatedOrganisations.AddNew();
			pivot1.OU_OH = org1.PK;
			AssertContainsLog(part, "Party OWN became 'DANSEXY'");
			System.Threading.Thread.Sleep(100);  // to ensure that "Logs.MostRecentLog" does not get confused
			pivot1.OU_Relationship = "BTH";
			AssertContainsLog(part, "'DANSEXY' became party BTH");
			System.Threading.Thread.Sleep(100);
			pivot1.OU_OH = org2.PK;
			AssertContainsLog(part, "Party BTH became 'POOPY'");
			System.Threading.Thread.Sleep(100);
			AssertEquals("OrgPartRelation should be deletable.", true, pivot1.CanDelete);
			part.RelatedOrganisations.RemoveAndDelete(pivot1);
			AssertContainsLog(part, "Relationship BTH to party 'POOPY' removed");
		}

		void AssertContainsLog(OrgSupplierPart part, string expected)
		{
			var log = part.Logs.MostRecentLog;
			AssertEquals(expected, log.SL_Reference);
		}

		#endregion

		#region TestOperationalActionsFieldToShowVisibility

		public void TestOperationalActionsFieldToShowVisibility()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(OrgPartRelation).GetProperty(OrgPartRelationSchema.OU_OH.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(OrgPartRelation).GetProperty(OrgPartRelationSchema.OU_FormLayoutController.Name)).ReadOnly);
		}

		#endregion

		#region TestGetsHighestRoyaltyFromMatchingOwnersOrSuppliers

		public void TestGetsHighestRoyaltyFromMatchingOwnersOrSuppliers()
		{
			OrgHeader orgSupplier1 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgOwner1 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgOwner2 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();

			OrgSupplierBuyerLink supplier1ToOwner1 = GetNewSupplierLinkAndAddToOrganisation(orgOwner1, orgSupplier1, 10m);
			OrgSupplierBuyerLink supplier1ToOwner2 = GetNewSupplierLinkAndAddToOrganisation(orgOwner2, orgSupplier1, 20m);

			OrgPartRelation relationOwner1 = GetNewRelationToPart(orgOwner1, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationOwner2 = GetNewRelationToPart(orgOwner2, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationSupplier1 = GetNewRelationToPart(orgSupplier1, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals("relationSupplier1.OU_RoyaltyPercent", 20m, relationSupplier1.OU_RoyaltyPercent);
		}

		#endregion

		#region TestOU_OHGetsRoyaltyWhenOrganisationChanges

		public void TestOU_OHGetsRoyaltyWhenOrganisationChanges()
		{
			OrgHeader orgSupplier1 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgSupplier2 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgOwner1 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgOwner2 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();

			OrgSupplierBuyerLink supplier1ToOwner1 = GetNewSupplierLinkAndAddToOrganisation(orgOwner1, orgSupplier1, 10m);
			OrgSupplierBuyerLink supplier1ToOwner2 = GetNewSupplierLinkAndAddToOrganisation(orgOwner2, orgSupplier1, 20m);
			OrgSupplierBuyerLink supplier2ToOwner1 = GetNewSupplierLinkAndAddToOrganisation(orgOwner1, orgSupplier2, 7m);

			OrgPartRelation relationOwner1 = GetNewRelationToPart(orgOwner1, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationOwner2 = GetNewRelationToPart(orgOwner2, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationSupplier1 = GetNewRelationToPart(orgSupplier1, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals("relationSupplier1.OU_RoyaltyPercent", 20m, relationSupplier1.OU_RoyaltyPercent);

			relationSupplier1.OU_OH = orgSupplier2.PK;

			AssertEquals("relationSupplier1.OU_RoyaltyPercent", 7m, relationSupplier1.OU_RoyaltyPercent);
		}

		#endregion

		#region TestOU_OHGetsNoDefaultRoyaltyFromInvalidOrganisation

		public void TestOU_OHGetsNoDefaultRoyaltyFromInvalidOrganisation()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = ZGuid.Invalid;

			AssertEquals("relation.OU_RoyaltyPercent", 0m, relation.OU_RoyaltyPercent);
		}

		#endregion

		#region TestOU_OHGetsNoDefaultRoyaltyFromOrganisationWithNoRoyaltyDefined

		public void TestOU_OHGetsNoDefaultRoyaltyFromOrganisationWithNoRoyaltyDefined()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;

			OrgHeader orgSupplier = Factory.New<OrgHeader>();
			orgSupplier.OH_IsConsignee = true;
			orgSupplier.OH_IsConsignor = true;

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;

			AssertEquals("relation.OU_RoyaltyPercent", 0m, relation.OU_RoyaltyPercent);
		}

		#endregion

		#region TestOU_OH_ForcesBOMPartsValidationRun

		public void TestOU_OH_ForcesBOMPartsValidationRun()
		{
			// Two parts, Two owners
			var owner1 = OrgHeader.New(Factory);
			var owner1Part = OrgSupplierPart.New(Factory);
			owner1Part.OP_PartNum = "Part1";
			var owner1PartRel = owner1Part.RelatedOrganisations.AddNew();
			owner1PartRel.OU_OH = owner1.PK;
			owner1PartRel.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var owner2 = OrgHeader.New(Factory);
			var owner2Part = OrgSupplierPart.New(Factory);
			owner2Part.OP_PartNum = "Part2";
			var owner2PartRel = owner2Part.RelatedOrganisations.AddNew();
			owner2PartRel.OU_OH = owner2.PK;
			owner2PartRel.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			// Create a new part owned by owner 1 with a bom part also owned by owner1
			var bomMainProduct = OrgSupplierPart.New(Factory);
			var bomMainProductOwnerRel = bomMainProduct.RelatedOrganisations.AddNew();
			bomMainProductOwnerRel.OU_OH = owner1.PK;
			bomMainProductOwnerRel.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			// Check that there are no validation errors on bom parts
			var bomChildPart = bomMainProduct.BillOfMaterials.AddNew();
			bomChildPart.OE_OP_Component = owner1Part.PK;
			bomMainProduct.RunPreSaveValidation(); // If marked correctly this should force the children to validate
			AssertEquals("Precondition - Correct BillOfMaterials children.", 1, bomMainProduct.BillOfMaterials.Count);
			AssertEquals("Precondition - No Validation Errors on BOM Child Part.", false, bomChildPart.OE_OP_ComponentInfo.HasErrors());

			// Change the OU_OH, check that bom parts are now marked invalid
			bomMainProductOwnerRel.OU_OH = owner2.PK;
			bomMainProduct.RunPreSaveValidation(); // If marked correctly this should force the children to validate
			AssertEquals("Expected Validation Errors on BOM Child Part.", true, bomChildPart.OE_OP_ComponentInfo.HasErrors());
		}

		#endregion

		#region TestMatches

		public void TestMatches()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertEquals(true, relation.MatchesImporter(org.PK));
			AssertEquals(false, relation.MatchesSupplier(org.PK));
			AssertEquals(false, relation.MatchesClassificationOrganisation(org.PK));

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertEquals(true, relation.MatchesImporter(org.PK));
			AssertEquals(true, relation.MatchesSupplier(org.PK));
			AssertEquals(false, relation.MatchesClassificationOrganisation(org.PK));

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.ClassificationOrganization;
			AssertEquals(false, relation.MatchesImporter(org.PK));
			AssertEquals(false, relation.MatchesSupplier(org.PK));
			AssertEquals(true, relation.MatchesClassificationOrganisation(org.PK));
		}

		#endregion

		#region TestOU_OH

		#region TestOnOU_OHChanging

		public void TestOnOU_OHChanging()
		{
			OrgPartRelation relation = SetUpRelation();
			relation.OnOU_OHChanging += new System.ComponentModel.CancelEventHandler(relation_OnOU_OHChanging);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			relation.OU_OH = org2.PK;
			AssertEquals("Not cancelled and the change stays", org2.PK, relation.OU_OH);

			shouldCancelOU_OH = true;

			relation.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Cancelled and the change should be reverted", org2.PK, relation.OU_OH);
		}

		bool shouldCancelOU_OH;
		void relation_OnOU_OHChanging(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (shouldCancelOU_OH)
			{
				e.Cancel = true;
			}
		}

		#endregion

		#region TestOU_OH_SetDefaultPickMode

		public void TestOU_OH_SetDefaultPickMode()
		{
			var part = Factory.New<OrgSupplierPart>();

			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			AssertEquals("Should be default Pick Mode", WhsPickMode.Codes.AttributeSpecified, relation.OU_PickMode);

			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_WhsDefaultWarehousePickMode = WhsPickMode.Codes.AttributeNeutral;

			relation.OU_OH = org.PK;
			AssertEquals("Should be Pick Mode from Organisation", WhsPickMode.Codes.AttributeNeutral, relation.OU_PickMode);
		}

		#endregion

		#endregion

		#region TestOU_OH_SetDefaultRollUp()

		public void TestOU_OH_SetDefaultRollUp()
		{
			var part = Factory.New<OrgSupplierPart>();

			var relation = part.RelatedOrganisations.AddNew();
			AssertEquals("Precondition: By default Roll Up is turned off.", false, relation.OU_RollUpAttributesOnDocuments);

			var org = Factory.New<OrgHeader>();
			relation.OU_OH = org.PK;
			AssertEquals("Should be Roll Up from Organisation", false, relation.OU_RollUpAttributesOnDocuments);

			org.MiscServ.OM_WhsDefaultWarehouseRollUp = true;
			relation.OU_OH = ZGuid.Empty;
			relation.OU_OH = org.PK;
			AssertEquals("Should be Roll Up from Organisation", true, relation.OU_RollUpAttributesOnDocuments);
		}

		#endregion

		#region TestHasAsnLineOnUnfinalisedReceive

		public void TestHasAsnLineOnUnfinalisedReceive()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var relation = part.RelatedOrganisations[0];
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = helper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient;
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part.PK, 0m);
			Factory.Save();

			var receive = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receivePk);

			AssertNotEquals("Current status of the receive is not finalised.", "FIN", receive.WD_DocketStatus);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be true when unfinalised Receive is referencing the OrgPartRelation.", true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false because no unfinalised Receive is referencing the another OrgPartRelation.", false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();

			AssertEquals("Current status of the receive is finalised.", "FIN", receive.WD_DocketStatus);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false when finalised these Receive which is referencing the OrgPartRelation.", false, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false because no unfinalised Receive is referencing the another OrgPartRelation.", false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
		}

		public void TestHasAsnLineOnUnfinalisedReceive_Cancelled()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var relation = part.RelatedOrganisations[0];
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = helper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient;
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part.PK, 0m);
			Factory.Save();

			var receive = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receivePk);

			AssertNotEquals("Current status of the receive is not finalised.", "FIN", receive.WD_DocketStatus);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be true when unfinalised Receive is referencing the OrgPartRelation.", true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false because no unfinalised Receive is referencing the another OrgPartRelation.", false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			receive.WD_DocketStatus = "ERR";
			receive.WD_DocketStatus = "CAN";
			Factory.Save();
			AssertEquals("Current status of the receive is cancelled.", "CAN", receive.WD_DocketStatus);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false when cancelled these Receive which is referencing the OrgPartRelation.", false, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals("HasAsnLineOnUnfinalisedReceive should be false because no unfinalised Receive is referencing the another OrgPartRelation.", false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
		}

		#endregion

		#region CusPart Pivots

		#region TestHasRelatedPartPivots

		public void TestHasRelatedPartPivots()
		{
			OrgPartRelation relation = SetUpRelation();

			AssertEquals("HasRelatedPartPivots", false, relation.HasRelatedPartPivots);

			BusinessObject cusClassPartPivotNZ = SetUpCusClassPartPivot(relation.OU_OP, "NZ");
			cusClassPartPivotNZ[CusClassPartPivotSchema.Constants.CI_OH] = relation.OU_OH;

			AssertEquals("HasRelatedPartPivots", true, relation.HasRelatedPartPivots);

			BusinessObject cusClassPartPivotAU = SetUpCusClassPartPivot(relation.OU_OP, "AU");
			cusClassPartPivotAU[CusClassPartPivotSchema.Constants.CI_OH] = relation.OU_OH;

			AssertEquals("HasRelatedPartPivots", true, relation.HasRelatedPartPivots);
		}

		#endregion

		#region TestGetCountryCodesWithSupplierImporterClassificationOverrides

		public void TestGetCountryCodesWithSupplierImporterClassificationOverrides()
		{
			OrgPartRelation relation = SetUpRelation();
			OrgPartRelation relation2 = relation.SupplierPart.RelatedOrganisations.AddNew();
			relation2.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			string countries = relation.GetCountryCodesWithRelatedCusClassPartPivot();
			AssertEquals("no countries yet", 0, countries.Length);

			BusinessObject cusClassPartPivotNZ = SetUpCusClassPartPivot(relation.OU_OP, "NZ");
			cusClassPartPivotNZ[CusClassPartPivotSchema.Constants.CI_OH] = relation.OU_OH;

			BusinessObject cusClassPartPivotNZ2 = SetUpCusClassPartPivot(relation.OU_OP, "NZ");
			cusClassPartPivotNZ2[CusClassPartPivotSchema.Constants.CI_OH] = relation2.OU_OH;

			countries = relation.GetCountryCodesWithRelatedCusClassPartPivot();
			AssertEquals("countries", "NZ", countries);

			BusinessObject cusClassPartPivotAU = SetUpCusClassPartPivot(relation.OU_OP, "AU");
			cusClassPartPivotAU[CusClassPartPivotSchema.Constants.CI_OH] = relation.OU_OH;
			countries = relation.GetCountryCodesWithRelatedCusClassPartPivot();
			AssertEquals("countries", "NZ,AU", countries);
		}

		#endregion

		#region TestDeleteRelatedCusClassPartPivots

		public void TestDeleteRelatedCusClassPartPivots()
		{
			OrgPartRelation relation = SetUpRelation();
			relation.Delete();
			AssertNoExceptionThrown(() => Factory.Save());

			relation = SetUpRelation();
			BusinessObject cusClassPartPivotNZ = SetUpCusClassPartPivot(relation.OU_OP, "NZ");
			cusClassPartPivotNZ[CusClassPartPivotSchema.Constants.CI_OH] = relation.OU_OH;

			relation.Delete();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		OrgPartRelation SetUpRelation()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.FillWithValidTestData();

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			return relation;
		}

		BusinessObject SetUpCusClassPartPivot(ZGuid productPK, ZString country)
		{
			GlbCompany.CurrentCompany.SetCountry(country);
			BusinessObject cusClassification = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();
			cusClassification[CusClassificationSchema.Constants.CC_LookupCode] = Guid.NewGuid().ToString().Replace("-", "");

			BusinessObject result = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
			result[CusClassPartPivotSchema.Constants.CI_OP] = productPK;
			result[CusClassPartPivotSchema.Constants.CI_CC] = cusClassification.PK;

			return result;
		}

		#endregion

		#region Change Reference

		#region TestCanChangeReferenceIfNoStockOnHand

		public void TestCanChangeReferenceIfNoStockOnHand()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = org.PK;

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Assert(!relation.OU_RelationshipInfo.HasError(OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError));

			relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			Assert(!relation.OU_RelationshipInfo.HasError(OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError));

			relation.OU_OH = Factory.New<OrgHeader>().PK;
			Assert(!relation.OU_RelationshipInfo.HasError(OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError));
		}

		#endregion

		#region TestCanChangeReferenceIfNoStockOnHandBoth

		public void TestCanChangeReferenceIfNoStockOnHandBoth()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = org.PK;

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertNoErrors(relation.OU_RelationshipInfo);

			relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertNoErrors(relation.OU_RelationshipInfo);

			relation.OU_OH = Factory.New<OrgHeader>().PK;
			AssertNoErrors(relation.OU_OHInfo);
		}

		#endregion

		#region TestCannotChangeReferenceIfUnfinalisedASNLinesExist

		public void TestCannotChangeReferenceIfUnfinalisedASNLinesExist_Relationship()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG2");
			var whs = helper.CreateWarehouse("1", "A");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var relation = part1.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var anotherRelation = part1.RelatedOrganisations.AddNew();
			var anotherClient = helper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient;
			anotherRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part1.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part1.PK, 0m);
			Factory.Save();

			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			AssertNoErrors(relation.OU_RelationshipInfo);
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation.OU_RelationshipInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			AssertNoErrors(anotherRelation.OU_RelationshipInfo);
			anotherRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertNoErrors(anotherRelation.OU_RelationshipInfo);
		}

		public void TestCannotChangeReferenceIfUnfinalisedASNLinesExist_Client()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG2");
			var secondClient = helper.CreateClient("ORG9");
			var whs = helper.CreateWarehouse("1", "A");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var relation = part1.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var anotherRelation = part1.RelatedOrganisations.AddNew();
			var anotherClient = helper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient;
			anotherRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part1.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part1.PK, 0m);
			Factory.Save();

			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);

			AssertNoErrors(relation.OU_OHInfo);
			relation.OU_OH = secondClient;
			AssertHasError(relation.OU_OHInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			AssertNoErrors(anotherRelation.OU_OHInfo);
			anotherRelation.OU_OH = client;
			AssertNoErrors(anotherRelation.OU_OHInfo);

			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			relation.OU_OH = secondClient;
			relation.Validation.ValidateOU_OH();
			AssertNoErrors(relation.OU_OHInfo);
		}

		#endregion

		#region TestCannotChangeReferenceIfTransactionsExist

		public void TestCannotChangeReferenceIfTransactionsExist()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = org.PK;

			// add some stock on hand
			var whsPK = Helper.CreateWarehouse("1", "A").PK;
			Helper.CreateStock(whsPK, org.PK, part.PK, 10m);

			AssertNoErrors(relation.OU_RelationshipInfo);
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertNoErrors("Not yet InDatabase so no error", relation.OU_RelationshipInfo);

			AssertNoErrors(relation.OU_OHInfo);
			relation.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNoErrors("Not yet InDatabase so no error", relation.OU_OHInfo);

			relation.OU_OH = org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation.OU_RelationshipInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertNoErrors(relation.OU_OHInfo);
			relation.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertHasError(relation.OU_OHInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);
		}

		public void TestCannotChangeReferenceIfTransactionsExist_Both()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = org.PK;

			// add some stock on hand
			var whsPK = Helper.CreateWarehouse("W1", "A").PK;
			Helper.CreateStock(whsPK, org.PK, part.PK, 10m);

			AssertNoErrors(relation.OU_RelationshipInfo);
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertNoErrors("Can change from OWN to BTH", relation.OU_RelationshipInfo);

			relation.OU_OP = relation.OU_OH = ZGuid.Empty;
			part.RelatedOrganisations.RemoveAndDeleteAll();
			relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertNoErrors("Not yet InDatabase so no error", relation.OU_RelationshipInfo);

			AssertNoErrors(relation.OU_OHInfo);
			relation.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNoErrors("Not yet InDatabase so no error", relation.OU_OHInfo);

			relation.OU_OH = org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation.OU_RelationshipInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertNoErrors(relation.OU_OHInfo);
			relation.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertHasError(relation.OU_OHInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);
		}

		public void TestCannotChangeReferenceIfTransactionsExist_Other()
		{
			var whs = Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = org.PK;

			Helper.CreateStock(whs.PK, org.PK, part.PK, 0m);
			Factory.Save();
			AssertNoErrors("Precondition", relation.OU_RelationshipInfo);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertHasError(relation.OU_RelationshipInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertNoErrors(relation.OU_RelationshipInfo);
		}

		#endregion

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			OrgPartRelation relation = (OrgPartRelation)GetNewBusinessObject();
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, relation.OU_Relationship);
		}

		#endregion

		#region TestCloneInternal

		public void TestCloneInternal()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			Factory.Save();

			OrgPartRelation partRelation = Factory.New<OrgPartRelation>();

			partRelation.OU_Hi = (ZShort)5;
			partRelation.OU_LandedCostMarginPercent1 = 22.2m;
			partRelation.OU_LandedCostMarginPercent2 = 55.5m;
			partRelation.OU_LandedCostMarginPercent3 = 662.6m;
			partRelation.OU_LocalPartNumber = "partNum";
			partRelation.OU_OH = org.PK;
			partRelation.OU_Relationship = "ORG";
			partRelation.OU_Ti = (ZShort)6;
			partRelation.OU_UseExpiryDate = true;
			partRelation.OU_UsePackingDate = true;
			partRelation.OU_UsePartAttrib1 = true;
			partRelation.OU_UsePartAttrib2 = true;
			partRelation.OU_UsePartAttrib3 = true;
			partRelation.OU_UseSerialNumber = true;
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			OrgPartRelation partRelationClone = (OrgPartRelation)partRelation.Clone();

			AssertEquals(partRelation.OU_Hi, partRelationClone.OU_Hi);
			AssertEquals(partRelation.OU_LandedCostMarginPercent1, partRelationClone.OU_LandedCostMarginPercent1);
			AssertEquals(partRelation.OU_LandedCostMarginPercent2, partRelationClone.OU_LandedCostMarginPercent2);

			AssertEquals(partRelation.OU_LandedCostMarginPercent3, partRelationClone.OU_LandedCostMarginPercent3);
			AssertEquals(partRelation.OU_LocalPartNumber, partRelationClone.OU_LocalPartNumber);
			AssertEquals(partRelation.OU_OH, partRelationClone.OU_OH);
			AssertEquals(partRelation.OU_Organisation, partRelationClone.OU_Organisation);
			AssertEquals(partRelation.OU_Relationship, partRelationClone.OU_Relationship);
			AssertEquals(partRelation.OU_Ti, partRelationClone.OU_Ti);

			AssertEquals(partRelation.OU_UseExpiryDate, partRelationClone.OU_UseExpiryDate);
			AssertEquals(partRelation.OU_UsePackingDate, partRelationClone.OU_UsePackingDate);
			AssertEquals(partRelation.OU_UsePartAttrib1, partRelationClone.OU_UsePartAttrib1);
			AssertEquals(partRelation.OU_UsePartAttrib2, partRelationClone.OU_UsePartAttrib2);
			AssertEquals(partRelation.OU_UsePartAttrib3, partRelationClone.OU_UsePartAttrib3);
			AssertEquals(partRelation.OU_UseSerialNumber, partRelationClone.OU_UseSerialNumber);
			AssertEquals(partRelation.OU_PickMode, partRelationClone.OU_PickMode);
		}

		#endregion

		#region TestReadOnlyProperties

		public void TestReadOnlyProperties()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertEquals("Supplier -> Margin percentages should be readonly", true, relation.OU_LandedCostMarginPercent1Info.ReadOnly);
			AssertEquals("Supplier -> Margin percentages should be readonly", true, relation.OU_LandedCostMarginPercent2Info.ReadOnly);
			AssertEquals("Supplier -> Margin percentages should be readonly", true, relation.OU_LandedCostMarginPercent3Info.ReadOnly);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertEquals("Owner -> Margin percentages should be readonly", false, relation.OU_LandedCostMarginPercent1Info.ReadOnly);
			AssertEquals("Owner -> Margin percentages should be readonly", false, relation.OU_LandedCostMarginPercent2Info.ReadOnly);
			AssertEquals("Owner -> Margin percentages should be readonly", false, relation.OU_LandedCostMarginPercent3Info.ReadOnly);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertEquals("Both -> Margin percentages should be readonly", false, relation.OU_LandedCostMarginPercent1Info.ReadOnly);
			AssertEquals("Both -> Margin percentages should be readonly", false, relation.OU_LandedCostMarginPercent2Info.ReadOnly);
			AssertEquals("Both -> Margin percentages should be readonly", false, relation.OU_LandedCostMarginPercent3Info.ReadOnly);
		}

		public void TestReadOnlyPropertiesForPartAttributes()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;

			AssertEquals("UsePartAttrib1 should be readonly", true, relation.OU_UsePartAttrib1Info.ReadOnly);
			AssertEquals("UsePartAttrib2 should be readonly", true, relation.OU_UsePartAttrib2Info.ReadOnly);
			AssertEquals("UsePartAttrib3 should be readonly", true, relation.OU_UsePartAttrib3Info.ReadOnly);
			AssertEquals("UseExpiryDate should be readonly", true, relation.OU_UseExpiryDateInfo.ReadOnly);
			AssertEquals("UsePackingDate should be readonly", true, relation.OU_UsePackingDateInfo.ReadOnly);

			org.MiscServ.OM_IMPartAttrib1Type = "~~";
			AssertEquals("UsePartAttrib1 should not be readonly", false, relation.OU_UsePartAttrib1Info.ReadOnly);

			org.MiscServ.OM_IMPartAttrib2Type = "~~";
			AssertEquals("UsePartAttrib2 should not be readonly", false, relation.OU_UsePartAttrib2Info.ReadOnly);

			org.MiscServ.OM_IMPartAttrib3Type = "~~";
			AssertEquals("UsePartAttrib3 should not be readonly", false, relation.OU_UsePartAttrib3Info.ReadOnly);
		}

		#region TestReadOnly_OU_RollUpAttributesOnDocuments

		public void TestOU_RollUpAttributesOnDocuments_ReadOnly_NoOrg()
		{
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();

			AssertEquals("OU_RollUpAttributesOnDocuments Should Be ReadOnly", true, relation.OU_RollUpAttributesOnDocumentsInfo.ReadOnly);
		}

		public void TestOU_RollUpAttributesOnDocuments_ReadOnly_AttribNeutral()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;

			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("OU_RollUpAttributesOnDocuments Shouldn't Be ReadOnly", false, relation.OU_RollUpAttributesOnDocumentsInfo.ReadOnly);
		}

		public void TestOU_RollUpAttributesOnDocuments_ReadOnly_AttributeSpecified()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;

			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("OU_RollUpAttributesOnDocuments Shouldn't Be ReadOnly", true, relation.OU_RollUpAttributesOnDocumentsInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestSetSupplierRelationClearMarginPercentage

		public void TestSetSupplierRelationClearMarginPercentage()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_LandedCostMarginPercent1 = 0.2m;
			relation.OU_LandedCostMarginPercent2 = 0.3m;
			relation.OU_LandedCostMarginPercent3 = 0.4m;

			AssertEquals("Margin Percentage should remain the same", 0.2m, relation.OU_LandedCostMarginPercent1);
			AssertEquals("Margin Percentage should remain the same", 0.3m, relation.OU_LandedCostMarginPercent2);
			AssertEquals("Margin Percentage should remain the same", 0.4m, relation.OU_LandedCostMarginPercent3);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertEquals("Margin Percentage should be cleared", 0m, relation.OU_LandedCostMarginPercent1);
			AssertEquals("Margin Percentage should be cleared", 0m, relation.OU_LandedCostMarginPercent2);
			AssertEquals("Margin Percentage should be cleared", 0m, relation.OU_LandedCostMarginPercent3);
		}

		#endregion

		#region TestSetSupplierRelationClearMarginPercentageFromBoth

		public void TestSetSupplierRelationClearMarginPercentageFromBoth()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_LandedCostMarginPercent1 = 0.2m;
			relation.OU_LandedCostMarginPercent2 = 0.3m;
			relation.OU_LandedCostMarginPercent3 = 0.4m;

			AssertEquals("Margin Percentage should remain the same", 0.2m, relation.OU_LandedCostMarginPercent1);
			AssertEquals("Margin Percentage should remain the same", 0.3m, relation.OU_LandedCostMarginPercent2);
			AssertEquals("Margin Percentage should remain the same", 0.4m, relation.OU_LandedCostMarginPercent3);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertEquals("Margin Percentage should be cleared", 0m, relation.OU_LandedCostMarginPercent1);
			AssertEquals("Margin Percentage should be cleared", 0m, relation.OU_LandedCostMarginPercent2);
			AssertEquals("Margin Percentage should be cleared", 0m, relation.OU_LandedCostMarginPercent3);
		}

		#endregion

		#region TestSetSupplierRelationClearMarginPercentageToBoth

		public void TestSetSupplierRelationClearMarginPercentageToBoth()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_LandedCostMarginPercent1 = 0.2m;
			relation.OU_LandedCostMarginPercent2 = 0.3m;
			relation.OU_LandedCostMarginPercent3 = 0.4m;

			AssertEquals("Margin Percentage should remain the same", 0.2m, relation.OU_LandedCostMarginPercent1);
			AssertEquals("Margin Percentage should remain the same", 0.3m, relation.OU_LandedCostMarginPercent2);
			AssertEquals("Margin Percentage should remain the same", 0.4m, relation.OU_LandedCostMarginPercent3);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertEquals("Margin Percentage should remain the same", 0.2m, relation.OU_LandedCostMarginPercent1);
			AssertEquals("Margin Percentage should remain the same", 0.3m, relation.OU_LandedCostMarginPercent2);
			AssertEquals("Margin Percentage should remain the same", 0.4m, relation.OU_LandedCostMarginPercent3);
		}

		#endregion

		#region TestOrgHeaderCollection

		[ExpectNoExceptions()]
		public void TestOrgHeaderCollection()
		{
			OrgPartRelation relation1 = Factory.New<OrgPartRelation>();
			AssertEquals(typeof(OrgHeaderCollection), relation1.OrgHeaderCollection.GetType());
			AssertNotNull(relation1.OrgHeaderCollection);
		}

		#endregion

		#region TestInvalidCodeHasError

		public void TestInvalidCodeHasError()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = ZGuid.Invalid;
			Assert(relation.OU_OHInfo.HasErrors());
		}

		#endregion

		#region TestEmptyCodeHasError

		public void TestEmptyCodeHasError()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = ZGuid.Empty;
			relation.RunPreSaveValidation();
			Assert(relation.OU_OHInfo.HasErrors());
		}

		#endregion

		#region TestInvalidCodeHasErrorAfterRunningDuplicateCheck

		public void TestInvalidCodeHasErrorAfterRunningDuplicateCheck()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgHeader orgHeader2 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();

			relation1.OU_OH = ZGuid.Invalid;
			Assert("Has errors after setting to invalid", relation1.OU_OHInfo.HasErrors());
			relation2.OU_OH = orgHeader2.PK;
			Assert("Has errors after setting another object's OU_OH", relation1.OU_OHInfo.HasErrors());
		}

		#endregion

		#region TestRelationshipTypeList

		public void TestRelationshipTypeList()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			AssertEquals("RelationshipType count", 5, relation1.RelationshipTypeList.Count);
		}

		#endregion

		#region TestCanHaveTwoImportersOnPart

		public void TestCanHaveTwoImportersOnPart()
		{
			OrgHeader owner1 = Factory.New<OrgHeader>();
			owner1.OH_IsConsignee = true;

			OrgHeader owner2 = Factory.New<OrgHeader>();
			owner2.OH_IsConsignee = true;

			OrgSupplierPart part = OrgSupplierPart.New(Factory);

			OrgPartRelation ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			ownerRelation1.OU_OH = owner1.PK;

			OrgPartRelation ownerRelation2 = part.RelatedOrganisations.AddNew();
			ownerRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			ownerRelation2.OU_OH = owner2.PK;

			AssertEquals("OwnerRelation1.HasNotifications : ", false, ownerRelation1.OU_OHInfo.HasNotifications());
			AssertEquals("OwnerRelation2.HasNotifications : ", false, ownerRelation2.OU_OHInfo.HasNotifications());
		}

		#endregion

		#region TestCanHaveTwoSuppliersOnPart

		public void TestCanHaveTwoSuppliersOnPart()
		{
			OrgHeader owner1 = Factory.New<OrgHeader>();
			owner1.OH_IsConsignor = true;

			OrgHeader owner2 = Factory.New<OrgHeader>();
			owner2.OH_IsConsignor = true;

			OrgSupplierPart part = OrgSupplierPart.New(Factory);

			OrgPartRelation ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			ownerRelation1.OU_OH = owner1.PK;

			OrgPartRelation ownerRelation2 = part.RelatedOrganisations.AddNew();
			ownerRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			ownerRelation2.OU_OH = owner2.PK;

			Assert("No errors on Relation1", !ownerRelation1.HasNotifications());
			Assert("No errors on Relation2", !ownerRelation2.HasNotifications());
		}

		#endregion

		#region TestCanHaveTwoBothOnPart

		public void TestCanHaveTwoBothOnPart()
		{
			OrgHeader owner1 = Factory.New<OrgHeader>();
			owner1.OH_IsConsignor = true;

			OrgHeader owner2 = Factory.New<OrgHeader>();
			owner2.OH_IsConsignor = true;

			OrgSupplierPart part = OrgSupplierPart.New(Factory);

			OrgPartRelation ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			ownerRelation1.OU_OH = owner1.PK;

			OrgPartRelation ownerRelation2 = part.RelatedOrganisations.AddNew();
			ownerRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			ownerRelation2.OU_OH = owner2.PK;

			Assert("No errors on Relation1", !ownerRelation1.HasNotifications());
			Assert("No errors on Relation2", !ownerRelation2.HasNotifications());
		}

		#endregion

		#region TestWarehouseConsigneeRelationshipWarning

		public void TestWarehouseConsigneeRelationshipWarning()
		{
			OrgPartRelation ownerRelation1 = Factory.New<OrgPartRelation>();

			ownerRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertHasWarning(ownerRelation1.OU_RelationshipInfo, OrgPartRelation.WarehouseConsigneeRelationshipWarning);

			ownerRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertNoWarning(ownerRelation1.OU_RelationshipInfo, OrgPartRelation.WarehouseConsigneeRelationshipWarning);
		}

		#endregion

		#region Properties

		#region TestOU_FormLayoutController

		public void TestOU_FormLayoutController()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			OrgPartRelation relation3 = part.RelatedOrganisations.AddNew();

			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			relation1.OU_FormLayoutController = true;
			AssertEquals(true, relation1.OU_FormLayoutController);
			AssertEquals(false, relation2.OU_FormLayoutController);
			AssertEquals(false, relation3.OU_FormLayoutController);
			relation2.OU_FormLayoutController = true;
			AssertEquals(false, relation1.OU_FormLayoutController);
			AssertEquals(true, relation2.OU_FormLayoutController);
			AssertEquals(false, relation3.OU_FormLayoutController);
			relation2.OU_FormLayoutController = false;
			AssertEquals(false, relation1.OU_FormLayoutController);
			AssertEquals(false, relation2.OU_FormLayoutController);
			AssertEquals(false, relation3.OU_FormLayoutController);
			relation1.OU_FormLayoutController = false;
			AssertEquals(false, relation1.OU_FormLayoutController);
			AssertEquals(false, relation2.OU_FormLayoutController);
			AssertEquals(false, relation3.OU_FormLayoutController);
			relation1.OU_FormLayoutController = true;
			AssertEquals(true, relation1.OU_FormLayoutController);
			AssertEquals(false, relation2.OU_FormLayoutController);
			AssertEquals(false, relation3.OU_FormLayoutController);
			relation3.OU_FormLayoutController = true;
			AssertEquals(false, relation1.OU_FormLayoutController);
			AssertEquals(false, relation2.OU_FormLayoutController);
			AssertEquals(true, relation3.OU_FormLayoutController);
		}

		#endregion

		#region TestOU_PickMode

		public void TestOU_PickMode()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = part.RelatedOrganisations.AddOwner(org);

			AssertEquals(WhsPickMode.Codes.AttributeSpecified, relation.OU_PickMode);
			AssertEquals(RFAttributeConfirmCode.Codes.None, relation.OU_RFAttributeConfirm);

			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(WhsPickMode.Codes.AttributeNeutral, relation.OU_PickMode);
			AssertEquals(RFAttributeConfirmCode.Codes.None, relation.OU_RFAttributeConfirm);
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified; // clean up

			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(WhsPickMode.Codes.AttributeNeutral, relation.OU_PickMode);
			AssertEquals(RFAttributeConfirmCode.Codes.SerialNumber, relation.OU_RFAttributeConfirm);
		}

		#endregion

		#region TestSetDefaulRFAttributeConfirm

		public void TestSetDefaulRFAttributeConfirm_SerialNumber()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_IMUseSerialNumber = true;

			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddOwner(org);
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			relation.OU_UseSerialNumber = false;
			AssertEquals("When no attributes enabled, default to none.", RFAttributeConfirmCode.Codes.None, relation.OU_RFAttributeConfirm);

			relation.OU_UseSerialNumber = true;
			AssertEquals("Default to Serial Number when enabled.", RFAttributeConfirmCode.Codes.SerialNumber, relation.OU_RFAttributeConfirm);
		}

		#endregion

		#region TestOU_UsePartAttrib_JulianBatchNumber

		public void TestOU_UsePartAttrib1_JulianBatchNumber()
		{
			TestOU_UsePartAttrib_JulianBatchNumberCore(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1);
		}

		public void TestOU_UsePartAttrib2_JulianBatchNumber()
		{
			TestOU_UsePartAttrib_JulianBatchNumberCore(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2);
		}

		public void TestOU_UsePartAttrib3_JulianBatchNumber()
		{
			TestOU_UsePartAttrib_JulianBatchNumberCore(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		void TestOU_UsePartAttrib_JulianBatchNumberCore(SchemaStringColumn clientsPartAttribTypeColumn, SchemaBoolColumn relationUsePartAttribColumn)
		{
			var org = Factory.New<OrgHeader>();
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition", false, relation.OU_UsePackingDate);
			AssertEquals("Precondition", false, relation.OU_UseExpiryDate);
			AssertEquals("Precondition", "", relation.OU_JulianBatchNoFormat);

			relation[relationUsePartAttribColumn] = true;
			AssertEquals(false, relation.OU_UsePackingDate);
			AssertEquals(false, relation.OU_UseExpiryDate);
			AssertEquals("", relation.OU_JulianBatchNoFormat);

			relation.OU_OH = org.PK;
			relation[relationUsePartAttribColumn] = true;
			AssertEquals(false, relation.OU_UsePackingDate);
			AssertEquals(false, relation.OU_UseExpiryDate);
			AssertEquals("", relation.OU_JulianBatchNoFormat);

			org.MiscServ[clientsPartAttribTypeColumn] = PartAttributeTypeList.Codes.Mandatory;
			relation[relationUsePartAttribColumn] = true;
			AssertEquals(false, relation.OU_UsePackingDate);
			AssertEquals(false, relation.OU_UseExpiryDate);
			AssertEquals("", relation.OU_JulianBatchNoFormat);

			org.MiscServ[clientsPartAttribTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			relation[relationUsePartAttribColumn] = true;
			AssertEquals(true, relation.OU_UsePackingDate);
			AssertEquals(true, relation.OU_UseExpiryDate);
			AssertEquals(JulianBatchNumberFormatList.Codes.YDDD_BatchNumber, relation.OU_JulianBatchNoFormat);
		}

		#endregion

		#region TestOU_WCG_CartonGroup

		public void TestOU_WCG_CartonGroup()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(OrgPartRelation), OrgPartRelationSchema.Constants.OU_WCG_CartonGroup, false,
				la => la.ListDataSourceMember == "Lookups.CartonGroups");
		}

		#endregion

		#region TestCategoryCode

		public void TestCategoryCode()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition", "", relation.CategoryCode);

			var category1 = Factory.New<OrgPartCategory>();
			category1.OPC_CategoryCode = "ABC";

			relation.OU_OPC_Category = category1.PK;
			AssertEquals(nameof(OrgPartRelation.CategoryCode), "ABC", relation.CategoryCode);

			var category2 = Factory.New<OrgPartCategory>();
			category2.OPC_CategoryCode = "DEF";

			relation.OU_OPC_Category = category2.PK;
			AssertEquals(nameof(OrgPartRelation.CategoryCode), "DEF", relation.CategoryCode);
		}

		#endregion

		#endregion

		#region TestDefaultingBoth

		public void TestDefaultingBoth()
		{
			OrgHeader owner1 = Factory.New<OrgHeader>();
			owner1.OH_IsConsignee = true;
			OrgHeader owner2 = Factory.New<OrgHeader>();
			owner2.OH_IsConsignee = true;
			owner2.OH_IsConsignor = true;
			owner2.CountryData.OV_MakePartsBothImportAndExport = true;

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation ownerRelation1 = part.RelatedOrganisations.AddNew();
			AssertEquals("Should default to OWN", ownerRelation1.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
			ownerRelation1.OU_OH = owner1.PK;
			AssertEquals("Should default to OWN", ownerRelation1.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
			ownerRelation1.OU_OH = owner2.PK;
			AssertEquals("Should default to BTH", ownerRelation1.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
			OrgPartRelation ownerRelation2 = part.RelatedOrganisations.AddNew();
			AssertEquals("Should default to OWN", ownerRelation2.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
			ownerRelation2.OU_OH = owner2.PK;
			AssertEquals("Should default to BTH", ownerRelation2.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
		}

		#endregion

		#region TestDuplicateRelationshipError

		public void TestDuplicateRelationshipError()
		{
			OrgHeader party1 = Factory.New<OrgHeader>();

			OrgSupplierPart part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "A1";
			OrgPartRelation relation = part1.RelatedOrganisations.AddNew();
			relation.OU_OH = party1.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgPartRelation relation2 = part1.RelatedOrganisations.AddNew();
			relation2.OU_OH = party1.PK;

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.DuplicateRelationshipError);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.DuplicateRelationshipError);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.RunPreSaveValidation();    // Validation is on other object...
			AssertHasError(relation.OU_RelationshipInfo, OrgPartRelation.SameOrgAsOwnerAndSupplier);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertNoErrors(relation2.OU_RelationshipInfo);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.DuplicateRelationshipError);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.DuplicateRelationshipError);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.DuplicateRelationshipError);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertNoErrors(relation2.OU_RelationshipInfo);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.DuplicateRelationshipError);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.DuplicateRelationshipError);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertHasError(relation2.OU_RelationshipInfo, OrgPartRelation.SameOrgAsOwnerAndSupplier);

			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertNoErrors(relation2.OU_RelationshipInfo);
		}

		#endregion

		#region TestHasTransactionsIncludingInTransit

		public void TestHasTransactionsIncludingInTransit()
		{
			var whs = Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			Factory.Save();

			AssertEquals("Precondition - No transactions, no SOH", false, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("Precondition - No SOH", false, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));

			var receive1 = Helper.CreateWhsReceive(org.PK, whs.PK, "RRR1", new NotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive1, part.PK, 0m, ZGuid.Empty);             // just a transaction, no SOH
			Factory.Save();

			AssertEquals("A transaction exists; no SOH", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("No SOH", false, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));

			var receive2 = Helper.CreateWhsReceive(org.PK, whs.PK, "RRR2", new NotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive2, part.PK, 10m, ZGuid.Empty);             // some SOH added
			Helper.WhsReceiveAllocateLocationsMock(receive2);
			Helper.FinaliseDocketWithoutUserConfirmation(receive2);
			Factory.Save();

			AssertEquals("Both a transaction and SOH exist", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("SOH exists", true, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));

			var orderPK = Helper.CreateWhsOrder(org.PK, whs.PK, "O1", null);
			var orderLinePK = Helper.CreateWhsOrderLine(orderPK, part.PK, 10m);
			var pickPK = Helper.CreateWhsPick(new[] { orderPK });
			var pickLine = Helper.GetPickLines(pickPK).Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			Factory.Save();

			AssertEquals("Both a transaction and In-Transit stock exist", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("In-Transit stock exists", true, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));

			Helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			Helper.FinalisePick(pickPK);
			Factory.Save();

			AssertEquals("Transactions exist.", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("No In-Transit or stock on hand exists", false, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));
		}

		#endregion

		#region TestHasTransactionsIncludingInTransit_Staged

		public void TestHasTransactionsIncludingInTransit_Staged()
		{
			var whs = Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org.PK, whs.PK, "R1", new NotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, part.PK, 10m, ZGuid.Empty);
			Helper.WhsReceiveAllocateLocationsMock(receive);
			Helper.FinaliseDocketWithoutUserConfirmation(receive);
			Factory.Save();

			var orderPK = Helper.CreateWhsOrder(org.PK, whs.PK, "O1", null);
			var orderLinePK = Helper.CreateWhsOrderLine(orderPK, part.PK, 10m);
			var pickPK = Helper.CreateWhsPick(new[] { orderPK });
			var transferLine = Helper.PickAndMakeInTransitTransfer(Helper.GetPickLines(pickPK).Single(), ZDateTime.Today);
			Helper.FinaliseDocketLine(transferLine.PK);
			AssertEquals("Should be Staged.", "STA", transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			AssertEquals("Should return true as transactions exist.", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("Should return true as stock exists.", true, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));
			AssertEquals("Should return true as stock exists.", true, relation.HasCurrentStockIncludingInTransit());

			Helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			Helper.FinalisePick(pickPK);
			Factory.Save();

			AssertEquals("Transactions exist.", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("No stock on hand exists.", false, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));
			AssertEquals("No stock on hand exists.", false, relation.HasCurrentStockIncludingInTransit());
		}

		#endregion

		#region TestHasTransactionsIncludingInTransit_FiltersIfNotOwner

		public void TestHasTransactionsIncludingInTransit_FiltersIfNotOwner()
		{
			var whs = Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = org.PK;
			Factory.Save();

			AssertEquals("Precondition - No transactions, no SOH", false, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("Precondition - No SOH", false, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));

			// This shouldn't be valid
			var receive1 = Helper.CreateWhsReceive(org.PK, whs.PK, "RRR1", new NotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive1, part.PK, 10m, ZGuid.Empty);
			Factory.Save();

			AssertEquals("HasTransactionsIncludingInTransit should only check for owner relationships.", false, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("HasTransactionsIncludingInTransit should only check for owner relationships.", false, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));
			AssertEquals("HasCurrentStockIncludingInTransit should only check for owner relationships.", false, relation.HasCurrentStockIncludingInTransit());

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			Factory.Save();
			AssertEquals("HasTransactionsIncludingInTransit should only check for owner relationships.", false, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("HasTransactionsIncludingInTransit should only check for owner relationships.", false, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Factory.Save();
			AssertEquals("HasTransactionsIncludingInTransit should return true as the relationship is an owner relationship and there is stock on hand.", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("HasTransactionsIncludingInTransit should return true as the relationship is an owner relationship and there is stock on hand.", true, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));

			// Change type, don't save (i.e. check uses OriginalValue)
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertEquals("HasTransactionsIncludingInTransit should return true as the relationship is an owner relationship and there is stock on hand.", true, relation.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals("HasTransactionsIncludingInTransit should return true as the relationship is an owner relationship and there is stock on hand.", true, relation.HasTransactionsIncludingInTransit(org.PK, currentStockOrInTransitOnly: true));
		}

		#endregion

		#region TestIsOwnerType

		public void TestIsOwnerType()
		{
			AssertEquals(true, OrgPartRelation.IsOwnerType(OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals(true, OrgPartRelation.IsOwnerType(OrgPartRelation.RelationshipTypes.Both));
			AssertEquals(false, OrgPartRelation.IsOwnerType(OrgPartRelation.RelationshipTypes.Supplier));
			AssertEquals(false, OrgPartRelation.IsOwnerType(OrgPartRelation.RelationshipTypes.WarehouseConsignee));
			AssertEquals(false, OrgPartRelation.IsOwnerType(ZString.Empty));
			AssertEquals(false, OrgPartRelation.IsOwnerType("ZZZ"));
		}

		#endregion

		#region TestCanDelete

		#region TestCanDelete_NoTransactions

		public void TestCanDelete_NoTransactions()
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;
			Factory.Save();

			AssertEquals(true, relation.IsInDatabase);
			AssertEquals(false, relation.IsDeleted);
			AssertNotNull(relation.SupplierPart);
			AssertEquals(false, relation.SupplierPart.IsDeleted);
			AssertEquals(false, relation.HasTransactionsIncludingInTransit(org.PK));

			AssertEquals(true, relation.CanDelete);
		}

		#endregion

		#region TestCanDelete_HasUnfinalisedASNLines

		public void TestCanDelete_HasUnfinalisedASNLines()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var whs = helper.CreateWarehouse("1", "A");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var relation = part1.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var anotherRelation = part1.RelatedOrganisations.AddNew();
			var anotherClient = helper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient;

			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part1.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part1.PK, 0m);
			Factory.Save();

			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(false, relation.CanDelete);

			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(true, anotherRelation.CanDelete);

			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();

			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(true, relation.CanDelete);

			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(true, anotherRelation.CanDelete);
		}

		#endregion

		#region TestCanDelete_HasTransactions

		public void TestCanDelete_HasTransactions()
		{
			var whs = Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;
			var receive = Helper.CreateWhsReceive(org.PK, whs.PK, "RRR1", new NotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, part.PK, 10m, ZGuid.Empty);
			Factory.Save();

			AssertEquals(true, relation.IsInDatabase);
			AssertEquals(false, relation.IsDeleted);
			AssertNotNull(relation.SupplierPart);
			AssertEquals(false, relation.SupplierPart.IsDeleted);
			AssertEquals(true, relation.HasTransactionsIncludingInTransit(org.PK));

			AssertEquals(false, relation.CanDelete);
		}

		#endregion

		#region TestCanDelete_DuplicateOwner

		public void TestCanDelete_DuplicateOwner()
		{
			var whs = Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation1.OU_OH = org.PK;

			var relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation2.OU_OH = org.PK;

			var receive = Helper.CreateWhsReceive(org.PK, whs.PK, "RRR1", new NotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, part.PK, 10m, ZGuid.Empty);
			helper.CreateAsnLine(receive, part.PK, 0m);
			Factory.Save();

			AssertEquals(true, relation1.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals(true, relation2.HasTransactionsIncludingInTransit(org.PK));
			AssertEquals(true, relation1.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(true, relation2.HasAsnLineOnUnfinalisedReceive);
			AssertEquals(true, relation1.CanDelete);
			AssertEquals(true, relation2.CanDelete);
		}

		#endregion

		#region TestCanDelete_AllowIfNotOwner

		public void TestCanDelete_AllowIfNotOwner()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relationOwner = part.RelatedOrganisations[0];
			relationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var org0PK = relationOwner.Organisation.PK;
			Helper.CreateStock(whs.PK, org0PK, part.PK, 10m);
			Factory.Save();

			var org1PK = Helper.CreateClient("ORG2");
			var relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relationSupplier.OU_OH = org1PK;
			Factory.Save();

			AssertEquals(2, part.RelatedOrganisations.Count);
			var docketLines = (BusinessObject[])Factory.Load<IWhsDocketLine>(new ZQuery());
			AssertEquals(1, docketLines.Length);

			AssertEquals(true, relationSupplier.CanDelete);
		}

		#endregion

		#region TestCanDelete_ChangedOrgValue

		public void TestCanDelete_ChangedOrgValue()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = product.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var org1PK = relation.Organisation.PK;
			var org2PK = Helper.CreateClient("ORG2");
			var warehouse1 = Helper.CreateWarehouse("1", "A");
			Helper.CreateStock(warehouse1.PK, org1PK, product.PK, 10m);
			Factory.Save();

			// remove Owner relationship - fails
			AssertEquals(false, relation.CanDelete);

			// change Owner and remove Owner relationship - should also fail
			relation.OU_OH = org2PK;
			AssertEquals(false, relation.CanDelete);
		}

		#endregion

		#region TestCanDelete_ChangedRelationValue

		public void TestCanDelete_ChangedRelationValue()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = product.RelatedOrganisations[0];
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var orgPK = relation.Organisation.PK;
			var warehouse = Helper.CreateWarehouse("1", "A");
			Helper.CreateStock(warehouse.PK, orgPK, product.PK, 10m);
			Factory.Save();

			AssertEquals(false, relation.CanDelete);
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			// remove relationship - fails
			AssertEquals(false, relation.CanDelete);
		}

		#endregion

		#region TestCanDelete_ForLicencedWarehouse

		public void TestCanDelete_ForLicencedWarehouse()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation1 = part.RelatedOrganisations[0];
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			// add some stock on hand
			var whsPk = Helper.CreateWarehouse("1", "A").PK;
			Helper.CreateStock(whsPk, relation1.OU_OH, part.PK, 10m);
			Factory.Save();

			AssertEquals(false, relation1.CanDelete);

			var orderPK = Helper.CreateWhsOrder(relation1.OU_OH, whsPk, "O1", null);
			Helper.CreateWhsOrderLine(orderPK, part.PK, 10);
			var pickPK = Helper.CreateWhsPick(new[] { orderPK });
			Helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			Helper.FinalisePick(pickPK);
			Factory.Save();
			var docketLines = Factory.Load<IWhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.NotEqual, 0));
			AssertEquals("Precondition. No SOH should remain", 0, docketLines.Length);

			var newFactory = new BusinessObjectFactory();
			part = newFactory.New<OrgSupplierPart>();
			var relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_OH = relation1.OU_OH;

			AssertEquals(true, relation2.CanDelete);
		}

		#endregion

		#region TestCanDelete_WithInTransitQty_Owner

		public void TestCanDelete_WithInTransitQty_Owner()
		{
			var client = Helper.CreateClient("Client1");
			var part1 = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
			var part2 = (OrgSupplierPart)Helper.CreateProduct(client, "P2");
			var relation1 = (OrgPartRelation)part1.RelatedOrganisations.Single();
			var relation2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part2.HasStockOnHandOrInTransit);

			// Add some stock on hand
			var whsPK = Helper.CreateWarehouse("1", "A").PK;
			Factory.Save();

			var receiveLine = (IWhsDocketLine)Helper.CreateStock(whsPK, client, part1.PK, 10m);
			Factory.Save();
			AssertEquals("Should have Stock On Hand.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Should *not* have Stock On Hand.", false, part2.HasStockOnHandOrInTransit);

			var orderPK = Helper.CreateWhsOrder(client, whsPK, "O1", null);
			Helper.CreateWhsOrderLine(orderPK, part1.PK, 10);
			var pickPK = Helper.CreateWhsPick(new[] { orderPK });

			var pickLine = Helper.GetPickLines(pickPK).Single();
			AssertEquals("Precondition: Stock On Hand exists.", 10m, receiveLine.WE_StockOnHand);
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			AssertEquals("Precondition: Stock On Hand reduced.", 0m, receiveLine.WE_StockOnHand);
			Factory.Save();

			AssertEquals("Precondition: Should have Stock In-Transit.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock In-Transit.", false, part2.HasStockOnHandOrInTransit);
			AssertEquals("Should not allow to delete relation as there are transactions", false, relation1.CanDelete);

			AssertEquals("Should allow to delete relation as there are no transactions", true, relation2.CanDelete);
			part2.RelatedOrganisations.RemoveAndDelete(relation2);
			AssertEquals("Should remove and delete the relation on part2 as there is no stock on hand or in transit.", 0, part2.RelatedOrganisations.Count);
			AssertEquals("Should remove and delete the relation on part2 as there is no stock on hand or in transit.", true, relation2.IsDeleted);

			Helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			Helper.FinalisePick(pickPK);
			Factory.Save();
			AssertEquals("Precondition: Should *not* have Stock On Hand or In-Transit.", false, part1.HasStockOnHandOrInTransit);

			AssertEquals("Should not allow to delete relation as there are transactions", false, relation1.CanDelete);
		}

		#endregion

		#region TestCanDelete_ReasonForNotAbleToDelete

		public void TestCanDelete_ReasonForNotAbleToDelete()
		{
			var relation = Factory.NewWithValidTestData<OrgPartRelation>();
			AssertEquals("There are existing transactions for this Product in the Warehouse module. This relationship cannot be deleted.", relation.ReasonForNotAbleToDelete);
		}

		#endregion

		#endregion

		#region TestIOrgPartRelation

		public void TestIOrgPartRelation_PK()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals("PK is correct", relation.PK, ((IOrgPartRelation)relation).PK);
		}

		public void TestIOrgPartRelation_CategoryCode()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition: CategoryCode", "", ((IOrgPartRelation)relation).CategoryCode);

			var category1 = Factory.New<OrgPartCategory>();
			category1.OPC_CategoryCode = "ABC";

			relation.OU_OPC_Category = category1.PK;
			AssertEquals(nameof(OrgPartRelation.CategoryCode), "ABC", ((IOrgPartRelation)relation).CategoryCode);

			var category2 = Factory.New<OrgPartCategory>();
			category2.OPC_CategoryCode = "DEF";

			relation.OU_OPC_Category = category2.PK;
			AssertEquals(nameof(OrgPartRelation.CategoryCode), "DEF", ((IOrgPartRelation)relation).CategoryCode);
		}

		public void TestIOrgPartRelation_UnitPrice()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition: UnitPrice", 0m, ((IOrgPartRelation)relation).OU_UnitPrice);

			relation.OU_UnitPrice = 1548.4154m;
			AssertEquals("Unit Price is correct", 1548.4154m, ((IOrgPartRelation)relation).OU_UnitPrice);

			relation.OU_UnitPrice = 77845.986m;
			AssertEquals("Unit Price is correct", 77845.986m, ((IOrgPartRelation)relation).OU_UnitPrice);
		}

		public void TestIOrgPartRelation_UnitPriceCurrency()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals("Precondition: UnitPriceCurrencyNK", string.Empty, ((IOrgPartRelation)relation).OU_RX_NKUnitPriceCurrency);

			relation.OU_RX_NKUnitPriceCurrency = "AUD";
			AssertEquals("Unit Price Currency NK is correct AUD", "AUD", ((IOrgPartRelation)relation).OU_RX_NKUnitPriceCurrency);

			relation.OU_RX_NKUnitPriceCurrency = "USD";
			AssertEquals("Unit Price Currency NK is correct USD", "USD", ((IOrgPartRelation)relation).OU_RX_NKUnitPriceCurrency);
		}

		#endregion

		#region TestTriggers

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "PKG";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			part2.RelatedOrganisations.RemoveAndDeleteAll();
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "PKG";
			Factory.Save();

			var relatedOrg2 = part2.RelatedOrganisations.AddNew();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relatedOrg2.OU_OH = client;

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same barcode have the same owner.");
		}

		#endregion

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_AddNewRelations

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_AddNewRelations()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "PKG";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "PKG";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same barcode have the same owner.");
		}

		#endregion

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_AddNewDuplicatedRelationsWithAnotherProductChanged

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_AddNewDuplicatedRelationsWithAnotherProductChanged()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "PKG";

			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "PKG";

			var client3 = helper.CreateClient("ABC3");
			var part3 = (OrgSupplierPart)helper.CreateProduct(client3, "PRODUCT3");
			var barcode3 = part3.PartBarcodes.AddNew();
			barcode3.PH_Barcode = "TestCode";
			barcode3.PH_F3_NKPackType = "PKG";
			Factory.Save();

			var newRelation1 = part1.RelatedOrganisations.AddNew();
			newRelation1.OU_OH = client3;
			newRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part2.RelatedOrganisations.Cast<OrgPartRelation>().Single().OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same barcode have the same owner.");
		}

		#endregion

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ExchangeTwoRelations

		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ExchangeTwoRelations()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var owner1 = part.RelatedOrganisations[0];
			var barcode = part.PartBarcodes.AddNew();
			barcode.PH_Barcode = "TestCode";
			barcode.PH_F3_NKPackType = "PKG";

			var client2 = helper.CreateClient("ABC2");
			var owner2 = part.RelatedOrganisations.AddNew();
			owner2.OU_OH = client2;
			owner2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			Factory.Save();

			owner1.OU_OH = client2;
			owner2.OU_OH = client1;
			AssertNoExceptionThrown("Throw no exception when exchanging relations.", Factory.Save);
		}

		#endregion

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation_Owner()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation(OrgPartRelation.RelationshipTypes.Owner);
		}

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation_Both()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation(OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation_Supplier()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation(OrgPartRelation.RelationshipTypes.Supplier);
		}

		void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_AddNewRelation(string relationShip)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "TestCode2");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "PKG";

			var client2 = helper.CreateClient("DEF");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_F3_NKPackType = "PKG";

			Factory.Save();

			var owner2 = part1.RelatedOrganisations.AddNew();
			owner2.OU_OH = client2;
			owner2.OU_Relationship = relationShip;

			if (relationShip == OrgPartRelation.RelationshipTypes.Supplier)
			{
				AssertNoExceptionThrown("Throw no exception when exchanging relations.", Factory.Save);
			}
			else
			{
				NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product code same with barcode of another product has same owner.");
			}
		}

		#endregion

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationship_ProductHasNoProduct

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationship_ProductHasNoProduct_Both()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationship_ProductHasNoProduct(OrgPartRelation.RelationshipTypes.Both);
		}

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationship_ProductHasNoProduct_Owner()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationship_ProductHasNoProduct(OrgPartRelation.RelationshipTypes.Owner);
		}

		void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationship_ProductHasNoProduct(string relationShip)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "Product1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode2";
			barcode1.PH_F3_NKPackType = "PKG";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "TestCode2");
			var relation2 = part2.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			relation2.OU_Relationship = relationShip;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same barcode have the same owner.");
		}

		#endregion

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient_Owner()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient(OrgPartRelation.RelationshipTypes.Owner, true);
		}

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient_Both()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient(OrgPartRelation.RelationshipTypes.Both, true);
		}

		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient_Supplier()
		{
			TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient(OrgPartRelation.RelationshipTypes.Supplier, false);
		}

		void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient(string relationShip, bool expectHasException)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "TestCode2");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "PKG";

			var client2 = helper.CreateClient("DEF");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var relation2 = part2.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation2.OU_Relationship = relationShip;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_F3_NKPackType = "PKG";

			Factory.Save();

			relation2.OU_OH = client1;

			if (expectHasException)
			{
				NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same barcode have the same owner.");
			}
			else
			{
				AssertNoExceptionThrown("Throw no exception when exchanging relations.", Factory.Save);
			}
		}

		#endregion

		#region TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient_ProductHasNoBarcode

		[ExpectNoExceptions]
		public void TestTG_OrgPartRelation_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductEqualToBarcode_ChangeRelationClient_ProductHasNoBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "Product1");
			var barcode = part1.PartBarcodes.AddNew();
			barcode.PH_Barcode = "TestCode2";
			barcode.PH_F3_NKPackType = "PKG";

			var client2 = helper.CreateClient("DEF");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "TestCode2");
			var relation2 = part2.RelatedOrganisations.Cast<OrgPartRelation>().Single();

			Factory.Save();

			relation2.OU_OH = client1;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same barcode have the same owner.");
		}

		#endregion

		#endregion

		#region TestOU_PreventReceivingOvers

		public void TestOU_PreventReceivingOvers()
		{
			var orgPartRelation = Factory.New<OrgPartRelation>();
			AssertEquals(false, orgPartRelation.OU_PreventReceivingOvers);

			orgPartRelation.OU_PreventReceivingOvers = true;
			AssertEquals(true, orgPartRelation.OU_PreventReceivingOvers);
		}

		#endregion

		#region TestOU_ReceiveOverageTolerancePercent_ReadOnly

		public void TestOU_ReceiveOverageTolerancePercent_ReadOnly()
		{
			var orgPartRelation = Factory.New<OrgPartRelation>();
			AssertEquals(ZShort.Zero, orgPartRelation.OU_ReceiveOverageTolerancePercent);
			AssertEquals(true, orgPartRelation.OU_ReceiveOverageTolerancePercentInfo.ReadOnly);

			orgPartRelation.OU_PreventReceivingOvers = true;
			AssertEquals(false, orgPartRelation.OU_ReceiveOverageTolerancePercentInfo.ReadOnly);
		}

		#endregion

		#region TestOU_ReceiveOverageTolerancePercent_WhenOU_PreventReceivingOversResetToFalse

		public void TestOU_ReceiveOverageTolerancePercent_WhenOU_PreventReceivingOversResetToFalse()
		{
			var orgPartRelation = Factory.New<OrgPartRelation>();

			orgPartRelation.OU_PreventReceivingOvers = true;
			AssertEquals(true, orgPartRelation.OU_PreventReceivingOvers);
			AssertEquals(ZShort.Zero, orgPartRelation.OU_ReceiveOverageTolerancePercent);

			orgPartRelation.OU_ReceiveOverageTolerancePercent = 500;
			AssertEquals((ZShort)500, orgPartRelation.OU_ReceiveOverageTolerancePercent);

			orgPartRelation.OU_PreventReceivingOvers = false;
			AssertEquals(false, orgPartRelation.OU_PreventReceivingOvers);
			AssertEquals(ZShort.Zero, orgPartRelation.OU_ReceiveOverageTolerancePercent);
		}

		#endregion

		#region TestOU_LocalPartDescription

		public void TestOU_CanSetLocalPartDescriptioToMaxLengthAndReadback()
		{
			var relation = SetUpRelation();
			var maxLength = OrgPartRelation.Schema.OU_LocalPartDescriptionMaxLength;

			AssertEquals("Max length of description should be 128", 128, maxLength);
			AssertNoExceptionThrown(() => relation.OU_LocalPartDescription = new string('a', maxLength));

			var description = relation.OU_LocalPartDescription;

			AssertEquals("Description can be set to max length and read back as max number of chars", maxLength, description.Length);
		}

		#endregion

		#region TestOnSaving

		public void TestOnSaving_ClearingOU_ClientUQResetsOU_UnitsPerClientUQ()
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;
			relation.OU_ClientUQ = "PLT";

			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = part.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 10m;
			Factory.Save();
			AssertEquals("OU_UnitsPerClientUQ should be set in memory.", 0.1m, relation.OU_UnitsPerClientUQ);

			relation.OU_ClientUQ = string.Empty;
			AssertEquals("Should not yet clear OU_UnitsPerClientUQ.", 0.1m, relation.OU_UnitsPerClientUQ);

			Factory.Save();
			AssertEquals("Should have cleared OU_UnitsPerClientUQ.", 0m, relation.OU_UnitsPerClientUQ);
		}

		public void TestOnSaving_ClearingAndResettingOU_ClientUQDoesNotResetOU_UnitsPerClientUQ()
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;
			relation.OU_ClientUQ = "PLT";

			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = part.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 10m;
			Factory.Save();
			AssertEquals("OU_UnitsPerClientUQ should be set in memory.", 0.1m, relation.OU_UnitsPerClientUQ);

			relation.OU_ClientUQ = string.Empty;
			AssertEquals("Should not yet clear OU_UnitsPerClientUQ.", 0.1m, relation.OU_UnitsPerClientUQ);

			relation.OU_ClientUQ = "PLT";
			Factory.Save();
			AssertEquals("Should not have cleared OU_UnitsPerClientUQ.", 0.1m, relation.OU_UnitsPerClientUQ);
		}

		public void TestOnSaving_ClearingOU_ClientUQResetsOU_UnitsPerClientUQ_InOriginalFactory()
			=> TestOnSaving_ClearingOU_ClientUQResetsOU_UnitsPerClientUQ_InOriginalFactory(isInDatabase: false);

		public void TestOnSaving_ClearingOU_ClientUQResetsOU_UnitsPerClientUQ_InOriginalFactory_IsInDatabase()
			=> TestOnSaving_ClearingOU_ClientUQResetsOU_UnitsPerClientUQ_InOriginalFactory(isInDatabase: true);

		void TestOnSaving_ClearingOU_ClientUQResetsOU_UnitsPerClientUQ_InOriginalFactory(bool isInDatabase)
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = part.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 10m;

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;

			if (isInDatabase)
			{
				Factory.Save();
			}

			relation.OU_ClientUQ = "PLT";
			Factory.Save();
			AssertEquals("OU_UnitsPerClientUQ should be set in memory.", 0.1m, relation.OU_UnitsPerClientUQ);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var relationInDB = newFactory.Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_UnitsPerClientUQ should be set.", 0.1m, relationInDB.OU_UnitsPerClientUQ);

			relation.OU_ClientUQ = string.Empty;
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("Should have cleared OU_UnitsPerClientUQ.", 0m, relation.OU_UnitsPerClientUQ);
		}

		public void TestOnSaving_ClearingOU_ClientUQResetsOU_UnitsPerClientUQ_FractionalUQ()
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "CTN";

			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = "UNT";
			partUnit.OF_ParentPackType = "CTN";
			partUnit.OF_QuantityInParent = 0.7m;

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;

			relation.OU_ClientUQ = "UNT";
			Factory.Save();
			AssertEquals("OU_UnitsPerClientUQ should be set in memory.", 0.7m, relation.OU_UnitsPerClientUQ);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var relationInDB = newFactory.Load<OrgPartRelation>(relation.PK);
			AssertEquals("OU_UnitsPerClientUQ should be set.", 0.7m, relationInDB.OU_UnitsPerClientUQ);

			relation.OU_ClientUQ = string.Empty;
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("Should have cleared OU_UnitsPerClientUQ.", 0m, relation.OU_UnitsPerClientUQ);
		}

		public void TestOnSaving_InvalidOU_ClientUQ()
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;
			relation.OU_ClientUQ = "XXX";
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("Should not have set OU_UnitsPerClientUQ.", 0m, relation.OU_UnitsPerClientUQ);
		}

		public void TestOnSaving_ChangingToInvalidOU_ClientUQResetsOU_UnitsPerClientUQ()
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;
			relation.OU_ClientUQ = "PLT";

			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = part.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 10m;
			Factory.Save();
			AssertEquals("OU_UnitsPerClientUQ should be set in memory.", 0.1m, relation.OU_UnitsPerClientUQ);

			relation.OU_ClientUQ = "XXX";
			Factory.Save();
			AssertEquals("Should have cleared OU_UnitsPerClientUQ.", 0m, relation.OU_UnitsPerClientUQ);
		}

		public void TestOnSaving_ZeroConversionDoesNotThrow()
		{
			Helper.CreateWarehouse("WHS1", "X");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = part.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 0m;

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_OH = org.PK;
			relation.OU_ClientUQ = "PLT";
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("Should not have set OU_UnitsPerClientUQ.", 0m, relation.OU_UnitsPerClientUQ);
		}

		#endregion

		#region Constructor

		public void TestConstructor_SetConcurrencyPolicy()
		{
			var partRelation = Factory.New<OrgPartRelation>();
			AssertEquals("Concurrency Policy should be ignore for OU_UnitsPerClientUQ", ConcurrencyPolicy.Ignore, partRelation.OU_UnitsPerClientUQInfo.ConcurrencyPolicy);
		}

		#endregion

		#region Implementation

		OrgHeader GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor()
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_IsConsignee = true;
			result.OH_IsConsignor = true;
			return result;
		}

		OrgSupplierPart Part => fPart ?? (fPart = Factory.New<OrgSupplierPart>());
		OrgSupplierPart fPart;

		IWhsTransactionTestHelper Helper => helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory));
		IWhsTransactionTestHelper helper;

		OrgSupplierBuyerLink GetNewSupplierLinkAndAddToOrganisation(OrgHeader organisation, OrgHeader organisationToLinkTo, ZDecimal percentage)
		{
			OrgSupplierBuyerLink result = organisation.SupplierLinks.AddNew();
			result.OL_OH_Supplier = organisationToLinkTo.PK;
			result.OL_RoyaltyPercentage = percentage;
			return result;
		}

		OrgPartRelation GetNewRelationToPart(OrgHeader organisation, ZString relationType)
		{
			OrgPartRelation result = Part.RelatedOrganisations.AddNew();
			result.OU_Relationship = relationType;
			result.OU_OH = organisation.PK;
			return result;
		}

		#endregion
	}

	#region DeferrableTriggerTest

	[TestedType(typeof(OrgPartRelation))]
	class OrgPartRelationDeferrableTriggerTest : DeferrableTriggerTestCase<OrgPartRelation>
	{
	}

	#endregion
}
