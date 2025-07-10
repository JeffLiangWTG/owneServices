using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestOrgPartRelationValidation : BusinessObjectValidationTestCase
	{
		#region TestCheckOU_RFAttributeConfirm

		#region TestCheckOU_RFAttributeConfirm

		public void TestCheckOU_RFAttributeConfirm()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			OrgPartRelation relation = part.RelatedOrganisations.AddOwner(org);
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = false;

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute has not been selected for use");
		}

		#endregion

		public void TestCheckOU_RFAttributeConfirm_AttributeNeutral()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddOwner(org);

			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = true;

			AssertNoErrors("Precondition - product should have no errors.", relation.OU_RFAttributeConfirmInfo);

			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "The Pick Mode is Attribute Neutral. A Serial Number Attribute must be used as Confirmation.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "The Pick Mode is Attribute Neutral. A Serial Number Attribute must be used as Confirmation.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "The Pick Mode is Attribute Neutral. A Serial Number Attribute must be used as Confirmation.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "The Pick Mode is Attribute Neutral. A Serial Number Attribute must be used as Confirmation.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertNoErrors("When Pick Mode is Attribute Neutral, then RF Confirm Attribute should be Serial Number Attribute.", relation.OU_RFAttributeConfirmInfo);
		}

		#endregion

		#region TestCheckOU_RFAttributeConfirmDoesNotAllowReleaseCapturedAttributesToBeSelected

		public void TestCheckOU_RFAttributeConfirmDoesNotAllowReleaseCapturedAttributesToBeSelected()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddOwner(org);
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			relation.OU_UseSerialNumber = true;

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_IsPartAttrib1ReleaseCaptured = true;
			relation.OU_IsPartAttrib2ReleaseCaptured = true;
			relation.OU_IsPartAttrib3ReleaseCaptured = true;
			relation.OU_IsSerialNumberReleaseCaptured = true;

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			relation.OU_IsPartAttrib1ReleaseCaptured = false;
			relation.OU_IsPartAttrib2ReleaseCaptured = false;
			relation.OU_IsPartAttrib3ReleaseCaptured = false;
			relation.OU_IsSerialNumberReleaseCaptured = false;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_IsPartAttrib1ReleaseCaptured = true;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_IsPartAttrib2ReleaseCaptured = true;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_IsPartAttrib3ReleaseCaptured = true;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_IsSerialNumberReleaseCaptured = true;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute is Release Captured.");
		}

		#endregion

		#region TestCheckOU_RFAttributeConfirm_AttributeNeutralWithPartAttributeUsage

		public void TestCheckOU_RFAttributeConfirm_AttributeNeutralWithPartAttributeUsage()
		{
			AssertRFAttributeConfirm(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, RFAttributeConfirmCode.Codes.PartAttribute1);
			AssertRFAttributeConfirm(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, RFAttributeConfirmCode.Codes.PartAttribute2);
			AssertRFAttributeConfirm(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, RFAttributeConfirmCode.Codes.PartAttribute3);
		}

		void AssertRFAttributeConfirm(SchemaStringColumn orgMiscServAttributeType, SchemaBoolColumn usePartAttributeType, string confirmAttribute)
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddOwner(org);
			AssertNoErrors("Precondition", relation.OU_PickModeInfo);

			// Not using any attributes
			org.MiscServ[orgMiscServAttributeType] = PartAttributeTypeList.Codes.VIN;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = confirmAttribute;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors("Since attribute is not used there should be no errors.", relation.OU_RFAttributeConfirmInfo);

			org.MiscServ[orgMiscServAttributeType] = PartAttributeTypeList.Codes.BatchNumber;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = confirmAttribute;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors("Since attribute is not used there should be no errors.", relation.OU_RFAttributeConfirmInfo);

			// Using attributes
			org.MiscServ[orgMiscServAttributeType] = PartAttributeTypeList.Codes.VIN;
			relation[usePartAttributeType] = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = confirmAttribute;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, OrgPartRelation.RFAttributeConfirm_AttributeNeutralError);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			org.MiscServ[orgMiscServAttributeType] = PartAttributeTypeList.Codes.VIN;
			relation[usePartAttributeType] = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation.OU_RFAttributeConfirm = confirmAttribute;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			org.MiscServ[orgMiscServAttributeType] = PartAttributeTypeList.Codes.BatchNumber;
			relation[usePartAttributeType] = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = confirmAttribute;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, OrgPartRelation.RFAttributeConfirm_AttributeNeutralError);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			org.MiscServ[orgMiscServAttributeType] = PartAttributeTypeList.Codes.BatchNumber;
			relation[usePartAttributeType] = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation.OU_RFAttributeConfirm = confirmAttribute;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
		}

		public void TestCheckOU_RFAttributeConfirm_AttributeNeutralWithSerialNumber()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddOwner(org);
			AssertNoErrors("Precondition", relation.OU_PickModeInfo);

			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = false;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute has not been selected for use");

			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "The Pick Mode is Attribute Neutral. A Serial Number Attribute must be used as Confirmation.");
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			org.MiscServ.OM_IMUseSerialNumber = false;
			relation.OU_UseSerialNumber = false;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute has not been selected for use");

			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = false;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			org.MiscServ.OM_IMUseSerialNumber = false;
			relation.OU_UseSerialNumber = false;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None; // trigger validation
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
		}

		#endregion

		#region TestCheckOU_RFAttributeConfirmNotSetForNonBarcodedProduct

		public void TestCheckOU_RFAttributeConfirmNotSetForNonBarcodedProduct()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddOwner(org);
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;

			part.OP_IsBarcoded = false;

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "Non barcoded products should not have RF Confirm attributes.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "Non barcoded products should not have RF Confirm attributes.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "Non barcoded products should not have RF Confirm attributes.");

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			part.OP_IsBarcoded = true;

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
		}

		#endregion

		#region TestValidateOU_UsePartAttrib1

		public void TestValidateOU_UsePartAttrib1()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_UsePartAttrib1 = false;
			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute has not been selected for use");

			relation.OU_UsePartAttrib1 = true;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
		}

		#endregion

		#region TestValidateOU_UsePartAttrib2

		public void TestValidateOU_UsePartAttrib2()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_UsePartAttrib2 = false;
			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute has not been selected for use");

			relation.OU_UsePartAttrib2 = true;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
		}

		#endregion

		#region TestValidateOU_UsePartAttrib3

		public void TestValidateOU_UsePartAttrib3()
		{
			OrgPartRelation relation = Factory.New<OrgPartRelation>();
			relation.OU_UsePartAttrib3 = false;
			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute has not been selected for use");

			relation.OU_UsePartAttrib3 = true;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
		}

		#endregion

		#region TestCheckOU_UseSerialNumber

		public void TestCheckOU_UseSerialNumber_ValidatesRFAttributeConfirm()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_UseSerialNumber = false;
			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.None;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);

			relation.OU_RFAttributeConfirm = CodeLists.RFAttributeConfirmCode.Codes.SerialNumber;
			AssertHasError(relation.OU_RFAttributeConfirmInfo, "This attribute has not been selected for use");

			relation.OU_UseSerialNumber = true;
			AssertNoErrors(relation.OU_RFAttributeConfirmInfo);
		}

		public void TestCheckOU_UseSerialNumber_ValidatesPickMode()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_UsePartAttrib1 = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation.OU_UseSerialNumber = false;
			AssertNoErrors(relation.OU_PickModeInfo);

			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertHasError(relation.OU_PickModeInfo, "Attribute Neutral Pick Mode can only be specified for products that use the Serial Number Attribute.");

			relation.OU_UseSerialNumber = true;
			AssertNoErrors(relation.OU_PickModeInfo);
		}

		#endregion

		#region TestValidateOU_UseExpiryDate

		public void TestValidateOU_UseExpiryDate()
		{
			var organisation = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = organisation.PK;
			AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(relation, OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1);
			AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(relation, OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2);
			AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(relation, OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		void AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(OrgPartRelation relation, SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn partAttributeUseColumn)
		{
			AssertNoError(relation.OU_UseExpiryDateInfo, "Use Expiry Date must be checked on in conjunction with Julian Batch Numbers.");

			relation.Organisation.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			relation[partAttributeUseColumn] = true;
			relation.OU_UseExpiryDate = true;
			AssertNoError(relation.OU_UseExpiryDateInfo, "Use Expiry Date must be checked on in conjunction with Julian Batch Numbers.");

			relation.OU_UseExpiryDate = false;
			AssertHasError(relation.OU_UseExpiryDateInfo, "Use Expiry Date must be checked on in conjunction with Julian Batch Numbers.");

			relation.Organisation.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			relation.OU_UseExpiryDate = false;
			AssertNoError(relation.OU_UseExpiryDateInfo, "Use Expiry Date must be checked on in conjunction with Julian Batch Numbers.");
		}

		#endregion

		#region Test Release Captured

		#region TestValidateOU_IsPartAttrib1ReleaseCaptured

		public void TestValidateOU_IsPartAttrib1ReleaseCaptured()
		{
			TestValidationOfIsPartAttribReleaseCapturedCore(OrgPartRelationSchema.OU_UsePartAttrib1, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured,
				1, relation => relation.OU_IsPartAttrib1ReleaseCapturedInfo);
			TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithJulianBatchNumbers(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured,
				relation => relation.OU_IsPartAttrib1ReleaseCapturedInfo);
		}

		public void TestValidateOU_IsPartAttrib1ReleaseCaptured_CompletePalletPicking()
		{
			TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithCompletePalletPicking(OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured);
		}

		#endregion

		#region TestValidateOU_IsPartAttrib2ReleaseCaptured

		public void TestValidateOU_IsPartAttrib2ReleaseCaptured()
		{
			TestValidationOfIsPartAttribReleaseCapturedCore(OrgPartRelationSchema.OU_UsePartAttrib2, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured,
				2, relation => relation.OU_IsPartAttrib2ReleaseCapturedInfo);
			TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithJulianBatchNumbers(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured,
				relation => relation.OU_IsPartAttrib2ReleaseCapturedInfo);
		}

		public void TestValidateOU_IsPartAttrib2ReleaseCaptured_CompletePalletPicking()
		{
			TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithCompletePalletPicking(OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured);
		}

		#endregion

		#region TestValidateOU_IsPartAttrib3ReleaseCaptured

		public void TestValidateOU_IsPartAttrib3ReleaseCaptured()
		{
			TestValidationOfIsPartAttribReleaseCapturedCore(OrgPartRelationSchema.OU_UsePartAttrib3, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured,
				3, relation => relation.OU_IsPartAttrib3ReleaseCapturedInfo);
			TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithJulianBatchNumbers(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured,
				relation => relation.OU_IsPartAttrib3ReleaseCapturedInfo);
		}

		public void TestValidateOU_IsPartAttrib3ReleaseCaptured_CompletePalletPicking()
		{
			TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithCompletePalletPicking(OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured);
		}

		#endregion

		#region TestValidateOU_IsSerialNumberReleaseCaptured

		public void TestValidateOU_IsSerialNumberReleaseCaptured_CompletePalletPicking()
		{
			TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithCompletePalletPicking(OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured);
		}

		#endregion

		#region TestValidationOfIsPartAttribReleaseCapturedCore

		void TestValidationOfIsPartAttribReleaseCapturedCore(SchemaBoolColumn useAttributeColumn, SchemaBoolColumn isReleaseCapturedColumn,
			int attributeNumber, Func<OrgPartRelation, ZPropertyInfo> getPropertyInfo)
		{
			var unlinkedRelation = Factory.New<OrgPartRelation>();
			unlinkedRelation[useAttributeColumn] = true;
			unlinkedRelation[isReleaseCapturedColumn] = true;
			var unlinkedRelationInfo = getPropertyInfo(unlinkedRelation);
			AssertNoErrors(unlinkedRelationInfo);

			unlinkedRelation[isReleaseCapturedColumn] = false;
			AssertNoErrors(unlinkedRelationInfo);
			unlinkedRelation.Delete(); // clean-up

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var client2 = Factory.Load<OrgHeader>(helper.CreateClient("Other"));
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "PartA");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "PartB");

			var relation = part1.RelatedOrganisations.FindFirstByOrganisationPK(client1.PK);
			part1.RelatedOrganisations.AddOwner(client2);
			client1.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			client1.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			client1.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.BatchNumber;
			client1.PartAttributeManager.SetProductToUseAttribute(part1, attributeNumber, true);

			BusinessObject inventoryForPart1 = null;

			switch (attributeNumber)
			{
				case 1:
					inventoryForPart1 = helper.CreateStock(warehouse.PK, client1.PK, "R1", part1.PK, 10m, "PA1");
					break;
				case 2:
					inventoryForPart1 = helper.CreateStock(warehouse.PK, client1.PK, "R1", part1.PK, 10m, "", "PA2");
					break;
				case 3:
					inventoryForPart1 = helper.CreateStock(warehouse.PK, client1.PK, "R1", part1.PK, 10m, "", "", "PA3");
					break;
			}

			helper.CreateStock(warehouse.PK, client2.PK, "R2", part1.PK, 10m); // stock for same product, different client
			helper.CreateStock(warehouse.PK, client1.PK, "R3", part2.PK, 10m); // stock for same client, different product

			Factory.Save();

			string errorMessage = @"
Release Capture cannot be changed because inventory already exists that uses this attribute.
You must first remove this inventory from the warehouse.".Trim();

			relation[isReleaseCapturedColumn] = true;
			var propertyInfo = getPropertyInfo(relation);
			AssertHasError(propertyInfo, errorMessage);

			relation[isReleaseCapturedColumn] = false;
			AssertNoErrors(propertyInfo);

			// Make In-Transit
			var orderPK = helper.CreateWhsOrder(client1.PK, warehouse.PK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part1.PK, 10m);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			Factory.Save();

			relation[isReleaseCapturedColumn] = true;
			AssertHasError(propertyInfo, errorMessage);

			relation[isReleaseCapturedColumn] = false;
			AssertNoErrors(propertyInfo);

			helper.FinaliseDocket(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();

			relation[isReleaseCapturedColumn] = true;
			AssertNoErrors(propertyInfo);
		}

		#endregion

		#region TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithJulianBatchNumbers

		void TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithJulianBatchNumbers(SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn usePartAttributeColumn, SchemaBoolColumn isPartAttributeReleaseCapturedColumn, Func<OrgPartRelation, ZPropertyInfo> getPropertyInfo)
		{
			var organisation = Factory.New<OrgHeader>();
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = organisation.PK;
			var info = getPropertyInfo(relation);

			relation[isPartAttributeReleaseCapturedColumn] = true;
			AssertNoError(info, "Julian Batch Numbers cannot be flagged as Release Capture.");

			relation[usePartAttributeColumn] = true;
			relation[isPartAttributeReleaseCapturedColumn] = true;
			AssertNoError(info, "Julian Batch Numbers cannot be flagged as Release Capture.");

			organisation.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			relation[isPartAttributeReleaseCapturedColumn] = true;
			AssertNoError(info, "Julian Batch Numbers cannot be flagged as Release Capture.");

			organisation.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			relation[isPartAttributeReleaseCapturedColumn] = true;
			AssertHasError(info, "Julian Batch Numbers cannot be flagged as Release Capture.");

			relation[isPartAttributeReleaseCapturedColumn] = false;
			AssertNoError(info, "Julian Batch Numbers cannot be flagged as Release Capture.");
		}

		#endregion

		#region TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithCompletePalletPicking

		void TestValidateOU_IsPartAttribReleaseCaptured_IsNotUsedWithCompletePalletPicking(SchemaBoolColumn isPartAttributeReleaseCapturedColumn)
		{
			var organisation = Factory.New<OrgHeader>();
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = organisation.PK;

			relation[isPartAttributeReleaseCapturedColumn] = true;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation[isPartAttributeReleaseCapturedColumn] = false;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = true;

			relation[isPartAttributeReleaseCapturedColumn] = true;
			AssertHasError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation[isPartAttributeReleaseCapturedColumn] = false;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");
		}

		#endregion

		#endregion

		#region TestCheckOU_CompletePalletPicking

		public void TestCheckOU_CompletePalletPicking()
		{
			var organisation = Factory.New<OrgHeader>();
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = organisation.PK;

			relation.OU_CompletePalletPicking = true;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = false;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_IsPartAttrib1ReleaseCaptured = true;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = true;
			AssertHasError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = false;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_IsPartAttrib1ReleaseCaptured = false;
			relation.OU_IsPartAttrib2ReleaseCaptured = true;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = true;
			AssertHasError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = false;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_IsPartAttrib2ReleaseCaptured = false;
			relation.OU_IsPartAttrib3ReleaseCaptured = true;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = true;
			AssertHasError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = false;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_IsPartAttrib3ReleaseCaptured = false;
			relation.OU_IsSerialNumberReleaseCaptured = true;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = true;
			AssertHasError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");

			relation.OU_CompletePalletPicking = false;
			AssertNoError(relation.OU_CompletePalletPickingInfo, "Complete Pallet Picking is not supported for products with Release Captured Attributes.");
		}

		#endregion

		#region TestCheckOU_PickMode

		#region TestCheckOU_PickMode

		public void TestCheckOU_PickMode()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertNoErrors(relation.OU_PickModeInfo);

			relation.OU_PickMode = "";
			AssertHasErrors(relation.OU_PickModeInfo);

			relation.OU_PickMode = "XXX";
			AssertHasErrors(relation.OU_PickModeInfo);
		}

		#endregion

		#region TestCheckOU_PickMode_AttributeNeutralMode

		public void TestCheckOU_PickMode_AttributeNeutralMode_SerialNumberColumn()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddOwner(org);
			AssertNoErrors("Precondition - product should have no errors.", relation.OU_PickModeInfo);

			// Org doesn't have attributes
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertNoErrors("Product should have no errors since no attribute is used.", relation.OU_PickModeInfo);

			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertNoErrors("Product should have no errors since no attribute is used.", relation.OU_PickModeInfo);

			// Org has attribute but product doesn't use it
			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = false;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertNoErrors("Product has no serial number attribute used, therefore should have no errors.", relation.OU_PickModeInfo);
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertNoErrors("Product has no serial number attribute used, therefore should have no errors.", relation.OU_PickModeInfo);

			org.MiscServ.OM_IMUseSerialNumber = false;
			relation.OU_UseSerialNumber = false;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertNoErrors("The product has no serial number attribute used, therefore should have no errors.", relation.OU_PickModeInfo);
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertNoErrors("The product has no serial number attribute used, therefore should have no errors.", relation.OU_PickModeInfo);

			// Org has serial number attribute and product use it
			org.MiscServ.OM_IMUseSerialNumber = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertNoErrors("The product has serial number attribute used, therefore should have no errors.", relation.OU_PickModeInfo);
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertNoErrors("The product has serial number attribute used, therefore should have no errors.", relation.OU_PickModeInfo);
		}

		#endregion

		#endregion

		#region TestCheckOU_JulianBatchNoFormat

		#region TestCheckOU_JulianBatchNoFormat

		public void TestCheckOU_JulianBatchNoFormat()
		{
			var relation = Factory.New<OrgPartRelation>();
			var expectedErrorMessage = "Enter a valid Julian Batch No Format.";
			AssertNoError(relation.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			relation.OU_JulianBatchNoFormat = "ABC";
			AssertHasError(relation.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			relation.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			AssertNoError(relation.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckOU_JulianBatchNoFormat_MandatoryIfJulianBatchNumberIsUsed

		public void TestCheckOU_JulianBatchNoFormat_MandatoryIfJulianBatchNumberIsUsed()
		{
			var organisation = Factory.New<OrgHeader>();
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = organisation.PK;
			TestCheckOU_JulianBatchNoFormat_MandatoryIfJulianBatchNumberIsUsedCore(relation, OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1);
			TestCheckOU_JulianBatchNoFormat_MandatoryIfJulianBatchNumberIsUsedCore(relation, OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2);
			TestCheckOU_JulianBatchNoFormat_MandatoryIfJulianBatchNumberIsUsedCore(relation, OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3);
		}

		void TestCheckOU_JulianBatchNoFormat_MandatoryIfJulianBatchNumberIsUsedCore(OrgPartRelation relation, SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn usePartAttributeColumn)
		{
			var expectedErrorMessage = "Please enter a Julian Batch No Format.";
			AssertNoError(relation.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			relation.Organisation.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			relation[usePartAttributeColumn] = true;
			relation.OU_JulianBatchNoFormat = "";
			AssertNoError(relation.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			relation.Organisation.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			relation.OU_JulianBatchNoFormat = "";
			AssertHasError(relation.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			relation[usePartAttributeColumn] = false;
			relation.OU_JulianBatchNoFormat = "";
			AssertNoError(relation.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock

		#region TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock

		[TestDate(2017, 1, 1)]
		public void TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock()
		{
			TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock_Core(isInTransit: false);
		}

		#endregion

		#region TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock_InTransit

		[TestDate(2017, 1, 1)]
		public void TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock_InTransit()
		{
			TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock_Core(isInTransit: true);
		}

		#endregion

		#region TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock_Core

		void TestCheckOU_JulianBatchNoFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock_Core(bool isInTransit)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "CLIENT";
			organisation.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			var part_WithoutStock = Factory.New<OrgSupplierPart>();
			var part_WithStock = Factory.New<OrgSupplierPart>();
			var part_WithStock_EmptyJulianFormat = Factory.New<OrgSupplierPart>();
			part_WithoutStock.OP_PartNum = "P1";
			part_WithStock.OP_PartNum = "P2";
			part_WithStock_EmptyJulianFormat.OP_PartNum = "P3";

			var relation_WithoutStock = part_WithoutStock.RelatedOrganisations.AddOwner(organisation);
			var relation_WithStock = part_WithStock.RelatedOrganisations.AddOwner(organisation);
			var relation_WithStock_EmptyJulianFormat = part_WithStock_EmptyJulianFormat.RelatedOrganisations.AddOwner(organisation);
			relation_WithoutStock.OU_UsePartAttrib1 = true;
			relation_WithStock.OU_UsePartAttrib1 = true;
			relation_WithStock_EmptyJulianFormat.OU_UsePartAttrib1 = true;

			relation_WithoutStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			relation_WithStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;

			var receivePK = helper.CreateWhsReceive(organisation.PK, warehouse.PK, "R1", null);
			var inventory = helper.CreateWhsReceiveInventoryLine(receivePK, part_WithStock.PK, 10m, "");
			Factory.Save();

			var pickPK = ZGuid.Empty;
			var orderPK = ZGuid.Empty;
			if (isInTransit)
			{
				var whsParams = helper.CreateProductParamsByWhsAndClient(part_WithStock.PK, organisation.PK, warehouse.PK, (ZShort)365);
				var inventoryLine = Factory.Load<IWhsDocketLine>(inventory);
				inventoryLine.WE_PartAttrib1 = "a17345";
				helper.WhsReceiveAllocateLocationsMock(receivePK);
				helper.FinaliseDocketWithoutUserConfirmation(receivePK);
				Factory.Save();

				orderPK = helper.CreateWhsOrder(organisation.PK, warehouse.PK, "O1", null);
				var orderLinePK = helper.CreateWhsOrderLine(orderPK, part_WithStock.PK, 10m);
				pickPK = helper.CreateWhsPick(new[] { orderPK });

				var pickLine = helper.GetPickLines(pickPK).Single();
				helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
				Factory.Save();
			}

			var expectedErrorMessage = PartAttributeValidation.GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage("Julian Batch No Format");

			AssertNoError("Precondition", relation_WithoutStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
			AssertNoError("Precondition", relation_WithStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
			AssertNoError("Precondition", relation_WithStock_EmptyJulianFormat.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			// Changing Julian Batch Number Format
			relation_WithoutStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
			relation_WithStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
			relation_WithStock_EmptyJulianFormat.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
			AssertNoError(relation_WithoutStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
			AssertHasError(relation_WithStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
			AssertNoError(relation_WithStock_EmptyJulianFormat.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			// Changing back Julian Batch Number Format
			relation_WithoutStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			relation_WithStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			relation_WithStock_EmptyJulianFormat.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			AssertNoError(relation_WithoutStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
			AssertNoError(relation_WithStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
			AssertNoError(relation_WithStock_EmptyJulianFormat.OU_JulianBatchNoFormatInfo, expectedErrorMessage);

			if (isInTransit)
			{
				helper.FinaliseDocketWithoutUserConfirmation(orderPK);
				helper.FinalisePick(pickPK);
				Factory.Save();

				// Changing Julian Batch Number Format
				relation_WithoutStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
				relation_WithStock.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
				relation_WithStock_EmptyJulianFormat.OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
				AssertNoError(relation_WithoutStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
				AssertNoError(relation_WithStock.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
				AssertNoError(relation_WithStock_EmptyJulianFormat.OU_JulianBatchNoFormatInfo, expectedErrorMessage);
			}
		}

		#endregion

		#endregion

		#endregion

		#region TestCheckOU_Relationship

		public void TestCheckOU_Relationship()
		{
			var relation = Factory.New<OrgPartRelation>();
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, relation.OU_Relationship);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Org130320";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TEST13032020";
			part.OP_Desc = "TEst 13032020";

			var msg = "You have not entered a Relationship.";
			var relOrg1 = part.RelatedOrganisations.AddNew();
			relOrg1.OU_Relationship = ZString.Empty;
			AssertHasErrorContaining(relOrg1.OU_RelationshipInfo, msg);

			relOrg1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relOrg1.Validation.ValidateOU_Relationship();
			AssertNoErrorContaining(relOrg1.OU_RelationshipInfo, msg);
		}

		#endregion

		#region TestCheckOU_UnitPrice

		public void TestCheckOU_UnitPrice()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertNoErrors(relation.OU_UnitPriceInfo);

			relation.OU_UnitPrice = -5;
			AssertHasError(relation.OU_UnitPriceInfo, "Unit Price cannot be negative.");

			relation.OU_UnitPrice = 5;
			AssertNoErrors(relation.OU_UnitPriceInfo);
		}

		#endregion

		#region TestCheckOU_RX_NKUnitPriceCurrency

		public void TestCheckOU_RX_NKUnitPriceCurrency()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertNoErrors(relation.OU_RX_NKUnitPriceCurrencyInfo);

			relation.OU_UnitPrice = 5;
			AssertHasError(relation.OU_RX_NKUnitPriceCurrencyInfo, "Please enter a Unit Price Currency.");

			relation.OU_RX_NKUnitPriceCurrency = "AED";
			AssertNoErrors(relation.OU_RX_NKUnitPriceCurrencyInfo);

			relation.OU_RX_NKUnitPriceCurrency = "TTT";
			relation.OU_UnitPrice = 0;
			AssertHasError(relation.OU_RX_NKUnitPriceCurrencyInfo, "Enter a valid Unit Price Currency.");
		}

		#endregion

		#region TestCheckOU_WCG_CartonGroup

		public void TestCheckOU_WCG_CartonGroup()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertNoErrors(relation.OU_WCG_CartonGroupInfo);

			relation.OU_WCG_CartonGroup = ZGuid.NewZGuid();
			AssertHasError(relation.OU_WCG_CartonGroupInfo, "Enter a valid Carton Group.");

			var group = Factory.New<IWhsCartonGroup>();
			group.WCG_Code = "G1";
			group.WCG_Description = "Group";

			relation.OU_WCG_CartonGroup = group.PK;
			AssertNoErrors(relation.OU_WCG_CartonGroupInfo);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertNoErrors(relation.OU_WCG_CartonGroupInfo);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation.OU_WCG_CartonGroupInfo, "Carton Groups can only be entered on 'Owner' or 'Both' Part Relationships.");

			relation.OU_WCG_CartonGroup = ZGuid.Empty;
			AssertNoErrors(relation.OU_WCG_CartonGroupInfo);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation.OU_WCG_CartonGroup = group.PK;
			AssertNoErrors(relation.OU_WCG_CartonGroupInfo);

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertHasError(relation.OU_WCG_CartonGroupInfo, "Carton Groups can only be entered on 'Owner' or 'Both' Part Relationships.");

			relation.OU_WCG_CartonGroup = ZGuid.Empty;
			AssertNoErrors(relation.OU_WCG_CartonGroupInfo);

			relation.OU_WCG_CartonGroup = group.PK;
			AssertHasError(relation.OU_WCG_CartonGroupInfo, "Carton Groups can only be entered on 'Owner' or 'Both' Part Relationships.");

			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertNoErrors(relation.OU_WCG_CartonGroupInfo);
		}

		#endregion

		#region TestCheckOU_OH

		public void TestCheckOU_OH_Owner()
		{
			TestCheckOU_OH_Core(OrgPartRelation.RelationshipTypes.Owner, false);
		}

		public void TestCheckOU_OH_Both()
		{
			TestCheckOU_OH_Core(OrgPartRelation.RelationshipTypes.Both, false);
		}

		public void TestCheckOU_OH_Owner_RelChange()
		{
			TestCheckOU_OH_Core(OrgPartRelation.RelationshipTypes.Owner, true);
		}

		public void TestCheckOU_OH_Both_RelChange()
		{
			TestCheckOU_OH_Core(OrgPartRelation.RelationshipTypes.Both, true);
		}

		void TestCheckOU_OH_Core(string relationshipType, bool testRelationChange)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_Relationship = relationshipType;
			var org1PK = relation1.Organisation.PK;
			var org2PK = helper.CreateClient("ORG2");
			var warehouse1 = helper.CreateWarehouse("1", "A");
			helper.CreateStock(warehouse1.PK, org1PK, product1.PK, 10m);
			Factory.Save();

			if (testRelationChange)
			{
				// change relation to non-owner type, the original value remains Owner
				relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			}

			// change Owner - fails
			relation1.OU_OH = org2PK;
			AssertHasError(relation1.OU_OHInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			// delete relation - fails
			relation1.OU_OH = org1PK;
			AssertEquals(false, relation1.CanDelete);

			if (testRelationChange)
			{
				relation1.OU_Relationship = relationshipType;
			}

			// Try deleting with In-Transit Qty
			var orderPK = helper.CreateWhsOrder(org1PK, warehouse1.PK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, product1.PK, 10m);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			Factory.Save();

			if (testRelationChange)
			{
				relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			}

			// change Owner - fails
			relation1.OU_OH = org2PK;
			AssertHasError(relation1.OU_OHInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			if (testRelationChange)
			{
				relation1.OU_Relationship = relationshipType;
			}

			// Still should fail as there are transactions
			relation1.OU_OH = org1PK;
			helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();

			if (testRelationChange)
			{
				relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			}

			relation1.OU_OH = org2PK;
			AssertHasError(relation1.OU_OHInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);
		}

		public void TestCheckOU_OH_Supplier()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation1 = product1.RelatedOrganisations[0];
			var relation2 = product1.RelatedOrganisations.AddNew();
			var org1PK = relation1.Organisation.PK;
			var org2PK = helper.CreateClient("ORG2");
			var warehouse1 = helper.CreateWarehouse("1", "A");

			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = org1PK;

			helper.CreateStock(warehouse1.PK, org1PK, product1.PK, 0m);     // transaction without any SOH
			Factory.Save();

			relation2.OU_OH = org2PK;
			AssertNoErrors("Should be able to change Supplier when no SOH", relation2.OU_OHInfo);

			AssertEquals("Should be able to delete Supplier when no SOH", true, relation2.CanDelete);
			product1.RelatedOrganisations.RemoveAndDelete(relation2);
			Factory.Save();

			relation2 = product1.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = org1PK;
			helper.CreateStock(warehouse1.PK, org1PK, product1.PK, 10m);     // transaction with SOH
			Factory.Save();

			relation2.OU_OH = org2PK;
			AssertNoErrors("Should be able to change Supplier if there is SOH, as supplier is irrelevant to Whs.", relation2.OU_OHInfo);

			relation2.OU_OH = org1PK;
			AssertEquals("Should be able to delete Supplier if there is SOH, as supplier is irrelevant to Whs.", true, relation2.CanDelete);
			product1.RelatedOrganisations.RemoveAndDelete(relation2);
		}

		public void TestCheckOU_OH_ValidateForSameBarcode_BetweenTwoProducts()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");
			var client3 = helper.CreateClient("ABC3");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var relatedOrg1 = part1.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = client2;
			relatedOrg1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var barcode11 = part1.PartBarcodes.AddNew();
			barcode11.PH_Barcode = "TestCode";
			barcode11.PH_F3_NKPackType = "PKG";
			var barcode12 = part1.PartBarcodes.AddNew();
			barcode12.PH_Barcode = "TestCode1";
			barcode12.PH_F3_NKPackType = "PKG";
			Factory.Save();

			// Conflict with the client which relationshipType is Owner
			var part2 = (OrgSupplierPart)helper.CreateProduct(client3, "PRODUCT2");
			var relatedOrg2 = part2.RelatedOrganisations.Cast<OrgPartRelation>().First();
			var barcode21 = part2.PartBarcodes.AddNew();
			barcode21.PH_Barcode = "TestCode";
			barcode21.PH_F3_NKPackType = "PKG";
			var barcode22 = part2.PartBarcodes.AddNew();
			barcode22.PH_Barcode = "TestCode2";
			barcode22.PH_F3_NKPackType = "PKG";
			AssertNoErrors(relatedOrg2.OU_OHInfo);

			relatedOrg2.OU_OH = client1;
			AssertHasError(relatedOrg2.OU_OHInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			relatedOrg2.OU_OH = client2;
			AssertHasError(relatedOrg2.OU_OHInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			// Confilict with the client which relationshipType is Both
			barcode22.PH_Barcode = "TestCode1";
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relatedOrg2.OU_OH = client3;
			AssertNoErrors(relatedOrg2.OU_OHInfo);

			relatedOrg2.OU_OH = client1;
			AssertHasError(relatedOrg2.OU_OHInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			relatedOrg2.OU_OH = client2;
			AssertHasError(relatedOrg2.OU_OHInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			part1.OP_IsActive = false;
			relatedOrg2.RunPreSaveValidation();
			AssertNoErrors(relatedOrg2.OU_OHInfo);
		}

		public void TestDuplicatedCombineOU_OH_And_OU_Relationship() => CombineAssertions(() =>
		{
			const string expectedError = "Duplicate organization relationship";
			var org1 = ZGuid.NewZGuid();
			var org2 = ZGuid.NewZGuid();
			var part = Factory.New<OrgSupplierPart>();
			var relation1 = part.RelatedOrganisations.AddNew();
			var relation2 = part.RelatedOrganisations.AddNew();

			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Owner);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Supplier);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Both);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Both);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Supplier);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.WarehouseConsignee, OrgPartRelation.RelationshipTypes.WarehouseConsignee);
			CheckForDifferentCombine(OrgPartRelation.RelationshipTypes.ClassificationOrganization, OrgPartRelation.RelationshipTypes.ClassificationOrganization);

			void CheckForDifferentCombine(ZString relationType1, ZString relationType2)
			{
				relation1.OU_OH = org1;
				relation1.OU_Relationship = relationType1;
				relation2.OU_OH = org1;
				relation2.OU_Relationship = relationType2;
				relation2.Validation.ValidateOU_OH();
				relation2.Validation.ValidateOU_Relationship();
				AssertHasErrorContaining("OU_OH has error for type both", relation2.OU_OHInfo, expectedError);
				AssertHasErrorContaining("OU_Relationship has error for type both", relation2.OU_RelationshipInfo, expectedError);

				relation2.OU_OH = org2;
				relation2.Validation.ValidateOU_OH();
				relation2.Validation.ValidateOU_Relationship();
				AssertNoErrors(relation2.OU_OHInfo);
				AssertNoErrors(relation2.OU_RelationshipInfo);
			}
		});

		#region TestCheckOU_OH_DuplicatedBarcode

		public void TestCheckOU_OH_DuplicatedBarcode_Product1IsActiveAndProduct2IsActive()
		{
			TestCheckOU_OH_DuplicatedBarcode(true, true, expectedHasError: true);
		}

		public void TestCheckOU_OH_DuplicatedBarcode_Product1IsInactiveAndProduct2IsActive()
		{
			TestCheckOU_OH_DuplicatedBarcode(false, true);
		}

		public void TestCheckOU_OH_DuplicatedBarcode_Product1IsActiveAndProduct2IsInactive()
		{
			TestCheckOU_OH_DuplicatedBarcode(false, true);
		}

		public void TestCheckOU_OH_DuplicatedBarcode_Product1IsInactiveAndProduct2IsInactive()
		{
			TestCheckOU_OH_DuplicatedBarcode(false, false);
		}

		void TestCheckOU_OH_DuplicatedBarcode(bool active1, bool active2, bool expectedHasError = false)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			part1.OP_IsActive = active1;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var client2 = helper.CreateClient("ABC");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			part2.OP_IsActive = active2;
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";

			AssertNoErrors(relatedOrg2.OU_OHInfo);

			relatedOrg2.OU_OH = client1;
			if (expectedHasError)
			{
				AssertHasError(relatedOrg2.OU_OHInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
			}
			else
			{
				AssertNoErrors("One of Products is in-active, should not have error", relatedOrg2.OU_OHInfo);
			}
		}

		#endregion

		public void TestCheckOU_OH_ProductCodeEqualToBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "TestCode");
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";

			AssertNoErrors(relatedOrg2.OU_OHInfo);

			relatedOrg2.OU_OH = client1;
			AssertHasError(relatedOrg2.OU_OHInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
		}

		public void TestCheckOU_OH_ProductCodeEqualToBarcode_RelationshipIsSupplier()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "TestCode");
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";

			AssertNoErrors(relatedOrg2.OU_OHInfo);

			relatedOrg2.OU_OH = client1;
			AssertNoErrors(relatedOrg2.OU_OHInfo);
		}

		public void TestCheckOU_OH_ProductCodeEqualToBarcode_HasNoBarcodes()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "TestCode");
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();

			AssertNoErrors(relatedOrg2.OU_OHInfo);

			relatedOrg2.OU_OH = client1;
			AssertHasError(relatedOrg2.OU_OHInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
		}

		public void TestCheckOU_OH_ValidateForSameBarcode_OwnersOfOneProductExchanged()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");
			var part = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT");
			var owner1 = part.RelatedOrganisations.Cast<OrgPartRelation>().First();
			var owner2 = part.RelatedOrganisations.AddNew();
			owner2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			owner2.OU_OH = client2;
			var barcode = part.PartBarcodes.AddNew();
			barcode.PH_Barcode = "BarCode";
			barcode.PH_F3_NKPackType = "PKG";
			Factory.Save();

			owner1.OU_OH = client2;
			owner2.OU_OH = client1;
			owner1.RunPreSaveValidation();
			owner2.RunPreSaveValidation();

			AssertNoErrors(owner1.OU_OHInfo);
			AssertNoErrors(owner2.OU_OHInfo);

			AssertNoExceptionThrown("Owners exchanged and saved successfully.", Factory.Save);
		}

		public void TestCheckOU_OH_ValidateForTwoEmptyBarcodes_TwoProducts()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");

			var barcode1 = part1.PartBarcodes.AddNew();
			var barcode2 = part2.PartBarcodes.AddNew();
			Assert("Precondition: Barcode1 is empty.", barcode1.PH_Barcode.IsEmpty);
			Assert("Precondition: Barcode2 is empty.", barcode2.PH_Barcode.IsEmpty);

			var owner1 = part1.RelatedOrganisations.Cast<OrgPartRelation>().First();
			var owner2 = part2.RelatedOrganisations.Cast<OrgPartRelation>().First();
			owner2.RunPreSaveValidation();

			AssertNoErrors("No duplicate barcode errors for empty the barcode.", owner2.OU_OHInfo);
		}

		#endregion

		#region TestCheckOU_Relationship

		public void TestCheckOU_Relationship_Owner()
		{
			TestCheckOU_Relationship(OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestCheckOU_Relationship_Both()
		{
			TestCheckOU_Relationship(OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner);
		}

		void TestCheckOU_Relationship(string relationshipType, string otherRelationshipType)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_Relationship = relationshipType;
			var org1PK = relation1.Organisation.PK;
			var org2PK = helper.CreateClient("ORG2");
			var warehouse1 = helper.CreateWarehouse("1", "A");
			helper.CreateStock(warehouse1.PK, org1PK, product1.PK, 0m);
			Factory.Save();

			// change Relationship to Supplier - fails
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation1.OU_RelationshipInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			// change to the other valid type - ok
			relation1.OU_Relationship = otherRelationshipType;
			AssertNoErrors(relation1.OU_RelationshipInfo);
		}

		public void TestCheckOU_Relationship_Supplier()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse1 = helper.CreateWarehouse("1", "A");
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation1 = product1.RelatedOrganisations[0];
			var relation2 = product1.RelatedOrganisations.AddNew();
			var org1PK = relation1.Organisation.PK;

			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = org1PK;

			helper.CreateStock(warehouse1.PK, relation1.Organisation.PK, product1.PK, 10m);
			Factory.Save();

			// change Relationship from Supplier to WarehouseConsignee - no problem
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			AssertNoErrors(relation1.OU_RelationshipInfo);
		}

		public void TestCheckOU_Relationship_WithInTransitStock()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var org1PK = helper.CreateClient("ORG1");
			var org2PK = helper.CreateClient("ORG2");
			var warehouse1 = helper.CreateWarehouse("1", "A");

			var product1 = (OrgSupplierPart)helper.CreateProduct(org1PK, "P1");
			var relation1 = (OrgPartRelation)product1.RelatedOrganisations.Single();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			helper.CreateStock(warehouse1.PK, org1PK, product1.PK, 10m);
			Factory.Save();

			var orderPK = helper.CreateWhsOrder(org1PK, warehouse1.PK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, product1.PK, 10m);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			Factory.Save();

			// change Relationship to Supplier - fails
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertHasError(relation1.OU_RelationshipInfo, OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);

			// change to the other valid type - ok
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertNoErrors(relation1.OU_RelationshipInfo);
		}

		public void TestCheckOU_Relationship_Owner_ValidateForSameBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var relatedOrg1 = part1.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = client2;
			relatedOrg1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var barcode11 = part1.PartBarcodes.AddNew();
			barcode11.PH_Barcode = "TestCode";
			barcode11.PH_F3_NKPackType = "PKG";
			var barcode12 = part1.PartBarcodes.AddNew();
			barcode12.PH_Barcode = "TestCode1";
			barcode12.PH_F3_NKPackType = "PKG";
			Factory.Save();

			// client1
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			part2.RelatedOrganisations.RemoveAndDeleteAll();
			var relatedOrg2 = part2.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = client1;
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var barcode21 = part2.PartBarcodes.AddNew();
			barcode21.PH_Barcode = "TestCode";
			barcode21.PH_F3_NKPackType = "PKG";
			var barcode22 = part2.PartBarcodes.AddNew();
			barcode22.PH_Barcode = "TestCode2";
			barcode22.PH_F3_NKPackType = "PKG";
			AssertNoErrors(relatedOrg2.OU_RelationshipInfo);

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertNoErrors(relatedOrg2.OU_RelationshipInfo);

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			// client2
			barcode22.PH_Barcode = "TestCode1";
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relatedOrg2.OU_OH = client2;
			AssertNoErrors(relatedOrg2.OU_RelationshipInfo);

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertNoErrors(relatedOrg2.OU_RelationshipInfo);

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
		}

		#region TestCheckOU_Relationship_DuplicatedBarcode

		public void TestCheckOU_Relationship_DuplicatedBarcode_Product1IsActiveAndProduct2IsActive()
		{
			TestCheckOU_Relationship_DuplicatedBarcodeCore(true, true, expectedHasError: true);
		}

		public void TestCheckOU_Relationship_DuplicatedBarcode_Product1IsInactiveAndProduct2IsActive()
		{
			TestCheckOU_Relationship_DuplicatedBarcodeCore(false, true);
		}

		public void TestCheckOU_Relationship_DuplicatedBarcode_Product1IsActiveAndProduct2IsInactive()
		{
			TestCheckOU_Relationship_DuplicatedBarcodeCore(true, false);
		}

		public void TestCheckOU_Relationship_DuplicatedBarcode_Product1IsInactiveAndProduct2IsInactive()
		{
			TestCheckOU_Relationship_DuplicatedBarcodeCore(false, false);
		}

		void TestCheckOU_Relationship_DuplicatedBarcodeCore(bool active1, bool active2, bool expectedHasError = false)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			part1.OP_IsActive = active1;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "TestCode");
			part2.OP_IsActive = active2;
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";

			AssertNoErrors(relatedOrg2.OU_RelationshipInfo);

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			if (expectedHasError)
			{
				AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
			}
			else
			{
				AssertNoErrors("One of Products is in-active, should not have error", relatedOrg2.OU_RelationshipInfo);
			}

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			if (expectedHasError)
			{
				AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
			}
			else
			{
				AssertNoErrors("One of Products is in-active, should not have error", relatedOrg2.OU_RelationshipInfo);
			}
		}

		#endregion

		public void TestCheckOU_Relationship_ProductEqualToBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "TestCode");
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";

			AssertNoErrors(relatedOrg2.OU_RelationshipInfo);
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
		}

		public void TestCheckOU_Relationship_ProductEqualToBarcode_ProductHasNoBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC1");

			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "TestCode");
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			AssertNoErrors(relatedOrg2.OU_RelationshipInfo);

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");

			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertHasError(relatedOrg2.OU_RelationshipInfo, "The same organization has a non-unique barcode or a barcode same to the Product Code on Product 'PRODUCT1'.");
		}

		#endregion

		#region EnablingAnUnusedPartAttribute

		#region TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute

		public void TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute_Receive()
		{
			EnablingAnUnusedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute_Held()
		{
			EnablingAnUnusedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute_AdjustmentIn()
		{
			EnablingAnUnusedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute_Transfer()
		{
			EnablingAnUnusedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute_TransferLineFinalised()
		{
			EnablingAnUnusedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute_InTransit()
		{
			EnablingAnUnusedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_EnablingAnUnusedPartAttribute_Staged()
		{
			EnablingAnUnusedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		#endregion

		#region TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute

		public void TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute_Receive()
		{
			EnablingAnUnusedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute_Held()
		{
			EnablingAnUnusedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute_AdjustmentIn()
		{
			EnablingAnUnusedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute_Transfer()
		{
			EnablingAnUnusedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute_TransferLineFinalised()
		{
			EnablingAnUnusedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute_InTransit()
		{
			EnablingAnUnusedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_EnablingAnUnusedPartAttribute_Staged()
		{
			EnablingAnUnusedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		#endregion

		#region TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute

		public void TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute_Receive()
		{
			EnablingAnUnusedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute_Held()
		{
			EnablingAnUnusedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute_AdjustmentIn()
		{
			EnablingAnUnusedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute_Transfer()
		{
			EnablingAnUnusedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute_TransferLineFinalised()
		{
			EnablingAnUnusedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute_InTransit()
		{
			EnablingAnUnusedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_EnablingAnUnusedPartAttribute_Staged()
		{
			EnablingAnUnusedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		#endregion

		#region TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute

		public void TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute_Receive()
		{
			EnablingAnUnusedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute_Held()
		{
			EnablingAnUnusedPartAttributeCore_Held(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute_AdjustmentIn()
		{
			EnablingAnUnusedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute_Transfer()
		{
			EnablingAnUnusedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute_TransferLineFinalised()
		{
			EnablingAnUnusedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute_InTransit()
		{
			EnablingAnUnusedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UseSerialNumber_EnablingAnUnusedPartAttribute_Staged()
		{
			EnablingAnUnusedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_PartAttrib1);
		}

		#endregion

		#region TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute

		public void TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute_Receive()
		{
			EnablingAnUnusedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute_Held()
		{
			EnablingAnUnusedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute_AdjustmentIn()
		{
			EnablingAnUnusedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute_Transfer()
		{
			EnablingAnUnusedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute_TransferLineFinalised()
		{
			EnablingAnUnusedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute_InTransit()
		{
			EnablingAnUnusedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_EnablingAnUnusedPartAttribute_Staged()
		{
			EnablingAnUnusedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		#endregion

		#region TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute

		public void TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute_Receive()
		{
			EnablingAnUnusedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute_Held()
		{
			EnablingAnUnusedPartAttributeCore_Held(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute_AdjustmentIn()
		{
			EnablingAnUnusedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute_Transfer()
		{
			EnablingAnUnusedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute_TransferLineFinalised()
		{
			EnablingAnUnusedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute_InTransit()
		{
			EnablingAnUnusedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_EnablingAnUnusedPartAttribute_Staged()
		{
			EnablingAnUnusedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		#endregion

		#region EnablingAnUnusedPartAttributeCore

		void EnablingAnUnusedPartAttributeCore_Receive(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			EnablingAnUnusedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Receive);
		}

		void EnablingAnUnusedPartAttributeCore_Held(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			EnablingAnUnusedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Held);
		}

		void EnablingAnUnusedPartAttributeCore_AdjustmentIn(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			EnablingAnUnusedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_AdjustmentIn);
		}

		void EnablingAnUnusedPartAttributeCore_Transfer(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			EnablingAnUnusedPartAttributeCore(relationColumn, docketLineColumn,
				(helper, whs, client, product, partAttribute) => InventoryTestHelperForPartAttributeValidation.CreateInventory_Transfer(helper, whs, client, product, partAttribute, false));
		}

		void EnablingAnUnusedPartAttributeCore_TransferLineFinalised(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			EnablingAnUnusedPartAttributeCore(relationColumn, docketLineColumn,
				(helper, whs, client, product, partAttribute) => InventoryTestHelperForPartAttributeValidation.CreateInventory_Transfer(helper, whs, client, product, partAttribute, true));
		}

		void EnablingAnUnusedPartAttributeCore_InTransit(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			EnablingAnUnusedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_InTransit);
		}

		void EnablingAnUnusedPartAttributeCore_Staged(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			EnablingAnUnusedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Staged);
		}

		delegate void CreateInventoryDelegate(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader org, OrgSupplierPart part, SchemaColumn partAttribColumn);

		void EnablingAnUnusedPartAttributeCore(SchemaColumn relationColumn, SchemaColumn docketLineAttributeColumn, CreateInventoryDelegate createInventory)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("WH1", "A", 2, 2);
			Factory.Save();

			var client = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));

			InventoryTestHelperForPartAttributeValidation.SetOrgMiscServAttribs(client, PartAttributeTypeList.Codes.Mandatory);
			InventoryTestHelperForPartAttributeValidation.SetOrgMiscServDates(client, true);

			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "P1");
			var relation = part.RelatedOrganisations[0];

			var propertyInfo = relation.ZPropertyInfoHash[relationColumn.Name];
			AssertNoErrors("Precondition: Should not return error.", propertyInfo);

			createInventory(helper, whs, client, part, null);
			Factory.Save();

			// Set bad state
			relation[relationColumn] = true;
			AssertHasError("Should return error as there is inventory.", propertyInfo, "There is current inventory which is NOT using this attribute. This inventory must be removed from the warehouse before this attribute can be enabled.");

			// Revert, assert no errors
			relation[relationColumn] = false;
			AssertNoErrors("Precondition: Should not return error.", propertyInfo);
		}

		#endregion

		#endregion

		#region DisablingAUsedPartAttribute

		#region TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute

		public void TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute_Receive()
		{
			DisablingAUsedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute_Held()
		{
			DisablingAUsedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute_AdjustmentIn()
		{
			DisablingAUsedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute_Transfer()
		{
			DisablingAUsedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute_TransferLineFinalised()
		{
			DisablingAUsedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute_InTransit()
		{
			DisablingAUsedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOU_UsePartAttrib1_DisablingAUsedPartAttribute_Staged()
		{
			DisablingAUsedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		#endregion

		#region TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute

		public void TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute_Receive()
		{
			DisablingAUsedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute_Held()
		{
			DisablingAUsedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute_AdjustmentIn()
		{
			DisablingAUsedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute_Transfer()
		{
			DisablingAUsedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute_TransferLineFinalised()
		{
			DisablingAUsedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute_InTransit()
		{
			DisablingAUsedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOU_UsePartAttrib2_DisablingAUsedPartAttribute_Staged()
		{
			DisablingAUsedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		#endregion

		#region TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute

		public void TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute_Receive()
		{
			DisablingAUsedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute_Held()
		{
			DisablingAUsedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute_AdjustmentIn()
		{
			DisablingAUsedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute_Transfer()
		{
			DisablingAUsedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute_TransferLineFinalised()
		{
			DisablingAUsedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute_InTransit()
		{
			DisablingAUsedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOU_UsePartAttrib3_DisablingAUsedPartAttribute_Staged()
		{
			DisablingAUsedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		#endregion

		#region TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute

		public void TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute_Receive()
		{
			DisablingAUsedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_SerialNumber);
		}

		public void TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute_Held()
		{
			DisablingAUsedPartAttributeCore_Held(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_SerialNumber);
		}

		public void TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute_AdjustmentIn()
		{
			DisablingAUsedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_SerialNumber);
		}

		public void TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute_Transfer()
		{
			DisablingAUsedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_SerialNumber);
		}

		public void TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute_TransferLineFinalised()
		{
			DisablingAUsedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_SerialNumber);
		}

		public void TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute_InTransit()
		{
			DisablingAUsedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_SerialNumber);
		}

		public void TestCheckOU_UseSerialNumber_DisablingAUsedPartAttribute_Staged()
		{
			DisablingAUsedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UseSerialNumber, WhsDocketLineSchema.WE_SerialNumber);
		}

		#endregion

		#region TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute

		public void TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute_Receive()
		{
			DisablingAUsedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute_Held()
		{
			DisablingAUsedPartAttributeCore_Held(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute_AdjustmentIn()
		{
			DisablingAUsedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute_Transfer()
		{
			DisablingAUsedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute_TransferLineFinalised()
		{
			DisablingAUsedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute_InTransit()
		{
			DisablingAUsedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		public void TestCheckOU_UsePackingDate_DisablingAUsedPartAttribute_Staged()
		{
			DisablingAUsedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UsePackingDate, WhsDocketLineSchema.WE_PackingDate);
		}

		#endregion

		#region TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute

		public void TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute_Receive()
		{
			DisablingAUsedPartAttributeCore_Receive(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute_Held()
		{
			DisablingAUsedPartAttributeCore_Held(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute_AdjustmentIn()
		{
			DisablingAUsedPartAttributeCore_AdjustmentIn(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute_Transfer()
		{
			DisablingAUsedPartAttributeCore_Transfer(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute_TransferLineFinalised()
		{
			DisablingAUsedPartAttributeCore_TransferLineFinalised(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute_InTransit()
		{
			DisablingAUsedPartAttributeCore_InTransit(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		public void TestCheckOU_UseExpiryDate_DisablingAUsedPartAttribute_Staged()
		{
			DisablingAUsedPartAttributeCore_Staged(OrgPartRelationSchema.OU_UseExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
		}

		#endregion

		#region DisablingAUsedPartAttributeCore

		void DisablingAUsedPartAttributeCore_Receive(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			DisablingAUsedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Receive);
		}

		void DisablingAUsedPartAttributeCore_Held(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			DisablingAUsedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Held);
		}

		void DisablingAUsedPartAttributeCore_AdjustmentIn(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			DisablingAUsedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_AdjustmentIn);
		}

		void DisablingAUsedPartAttributeCore_Transfer(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			DisablingAUsedPartAttributeCore(relationColumn, docketLineColumn,
				(helper, whs, client, product, partAttrib) => InventoryTestHelperForPartAttributeValidation.CreateInventory_Transfer(helper, whs, client, product, partAttrib, false));
		}

		void DisablingAUsedPartAttributeCore_TransferLineFinalised(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			DisablingAUsedPartAttributeCore(relationColumn, docketLineColumn,
				(helper, whs, client, product, partAttrib) => InventoryTestHelperForPartAttributeValidation.CreateInventory_Transfer(helper, whs, client, product, partAttrib, true));
		}

		void DisablingAUsedPartAttributeCore_InTransit(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			DisablingAUsedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_InTransit);
		}

		void DisablingAUsedPartAttributeCore_Staged(SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			DisablingAUsedPartAttributeCore(relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Staged);
		}

		void DisablingAUsedPartAttributeCore(SchemaColumn relationColumn, SchemaColumn docketLineAttributeColumn, CreateInventoryDelegate createInventory)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("WH1", "A", 2, 2);
			Factory.Save();
			var client = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));

			InventoryTestHelperForPartAttributeValidation.SetOrgMiscServAttribs(client, PartAttributeTypeList.Codes.Mandatory);
			InventoryTestHelperForPartAttributeValidation.SetOrgMiscServDates(client, true);
			client.MiscServ.OM_IMUseSerialNumber = true;

			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "P1");
			var relation = part.RelatedOrganisations[0];
			relation[relationColumn] = true;

			var propertyInfo = relation.ZPropertyInfoHash[relationColumn.Name];
			AssertNoErrors("Precondition: Should not return error.", propertyInfo);

			createInventory(helper, whs, client, part, docketLineAttributeColumn);
			Factory.Save();

			// Set bad state
			relation[relationColumn] = false;
			AssertHasError("Should return error as there is inventory.", propertyInfo, "There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");

			// Revert, assert no errors
			relation[relationColumn] = true;
			AssertNoErrors("Precondition: Should not return error.", propertyInfo);
		}

		#endregion

		#endregion

		#region TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePartAttrib1_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePartAttrib1, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePartAttrib1_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePartAttrib1, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePartAttrib2_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePartAttrib2, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePartAttrib2_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePartAttrib2, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePartAttrib3_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePartAttrib3, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePartAttrib3_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePartAttrib3, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UseSerialNumber_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UseSerialNumber, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UseSerialNumber_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UseSerialNumber, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UseExpiryDate_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UseExpiryDate, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UseExpiryDate_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UseExpiryDate, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePackingDate_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePackingDate, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_UsePackingDate_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_UsePackingDate, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsPartAttrib1ReleaseCaptured_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsPartAttrib1ReleaseCaptured_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsPartAttrib2ReleaseCaptured_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsPartAttrib2ReleaseCaptured_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsPartAttrib3ReleaseCaptured_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsPartAttrib3ReleaseCaptured_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured, enabled: true);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsSerialNumberReleaseCaptured_FromDisableToAble()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured, enabled: false);
		}

		public void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChanged_IsSerialNumberReleaseCaptured_FromAbleToDisable()
		{
			TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured, enabled: true);
		}

		void TestCheckChangingAttributes_WhenHasASNLines_ThenCouldNotBeChangedCore(SchemaBoolColumn column, bool enabled)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var relation = part.RelatedOrganisations[0];
			var whs = helper.CreateWarehouse("1", "A");
			SetRelationshipValues(relation, defaultValueToSet: enabled, column, enabled);
			Factory.Save();

			var receivePk = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePk, part.PK, 0m, "A");
			helper.CreateAsnLine(receivePk, part.PK, 0m);
			Factory.Save();
			AssertEquals(false, relation.HasCurrentStockIncludingInTransit());
			AssertEquals(true, relation.HasAsnLineOnUnfinalisedReceive);

			AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_ThatAttributesCanBeChangedFromDisableToAble(part, helper, column, enabled);
			AssertRelationshipValues(relation, defaultExpectedValue: enabled, column, expectedColumnValue: enabled);
			AssertRelationshipInfosErrors(relation, columnThatShouldHaveError: null, errorMessage: null);

			var errorMessage = "Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.";
			SetRelationshipValues(relation, defaultValueToSet: enabled, column, !enabled);
			AssertRelationshipInfosErrors(relation, column, errorMessage);

			helper.FinaliseDocketWithoutUserConfirmation(receivePk);
			Factory.Save();
			AssertEquals(false, relation.HasAsnLineOnUnfinalisedReceive);

			SetRelationshipValues(relation, defaultValueToSet: enabled, column, !enabled);
			AssertRelationshipInfosErrors(relation, columnThatShouldHaveError: null, errorMessage: null);
		}

		void AssertAnotherRelationShouldNotBeAffectedByRelationWithUnfinalisedASNLines_ThatAttributesCanBeChangedFromDisableToAble(OrgSupplierPart part, IWhsTransactionTestHelper helper, SchemaBoolColumn column, bool enabled)
		{
			var anotherRelation = part.RelatedOrganisations.AddNew();
			var anotherClient = helper.CreateClient("Org6");
			anotherRelation.OU_OH = anotherClient;
			SetRelationshipValues(anotherRelation, defaultValueToSet: enabled, column, enabled);
			Factory.Save();
			AssertEquals(false, anotherRelation.HasAsnLineOnUnfinalisedReceive);
			AssertRelationshipValues(anotherRelation, defaultExpectedValue: enabled, column, expectedColumnValue: enabled);
			AssertRelationshipInfosErrors(anotherRelation, columnThatShouldHaveError: null, errorMessage: null);

			SetRelationshipValues(anotherRelation, defaultValueToSet: enabled, column, !enabled);

			AssertRelationshipInfosErrors(anotherRelation, columnThatShouldHaveError: null, errorMessage: null);
		}

		void SetRelationshipValues(OrgPartRelation relation, bool defaultValueToSet, SchemaBoolColumn column, bool columnValueToSet)
		{
			relation.OU_UsePartAttrib1 = column == OrgPartRelationSchema.OU_UsePartAttrib1 ? columnValueToSet : defaultValueToSet;
			relation.OU_UsePartAttrib2 = column == OrgPartRelationSchema.OU_UsePartAttrib2 ? columnValueToSet : defaultValueToSet;
			relation.OU_UsePartAttrib3 = column == OrgPartRelationSchema.OU_UsePartAttrib3 ? columnValueToSet : defaultValueToSet;
			relation.OU_UseSerialNumber = column == OrgPartRelationSchema.OU_UseSerialNumber ? columnValueToSet : defaultValueToSet;
			relation.OU_UseExpiryDate = column == OrgPartRelationSchema.OU_UseExpiryDate ? columnValueToSet : defaultValueToSet;
			relation.OU_UsePackingDate = column == OrgPartRelationSchema.OU_UsePackingDate ? columnValueToSet : defaultValueToSet;
			relation.OU_IsPartAttrib1ReleaseCaptured = column == OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured ? columnValueToSet : defaultValueToSet;
			relation.OU_IsPartAttrib2ReleaseCaptured = column == OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured ? columnValueToSet : defaultValueToSet;
			relation.OU_IsPartAttrib3ReleaseCaptured = column == OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured ? columnValueToSet : defaultValueToSet;
			relation.OU_IsSerialNumberReleaseCaptured = column == OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured ? columnValueToSet : defaultValueToSet;
		}

		void AssertRelationshipValues(OrgPartRelation relation, bool defaultExpectedValue, SchemaBoolColumn column, bool expectedColumnValue)
		{
			AssertEquals(nameof(relation.OU_UsePartAttrib1), column == OrgPartRelationSchema.OU_UsePartAttrib1 ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_UsePartAttrib2), column == OrgPartRelationSchema.OU_UsePartAttrib2 ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_UsePartAttrib3), column == OrgPartRelationSchema.OU_UsePartAttrib3 ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_UseSerialNumber), column == OrgPartRelationSchema.OU_UseSerialNumber ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_UseExpiryDate), column == OrgPartRelationSchema.OU_UseExpiryDate ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_UsePackingDate), column == OrgPartRelationSchema.OU_UsePackingDate ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_IsPartAttrib1ReleaseCaptured), column == OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_IsPartAttrib2ReleaseCaptured), column == OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_IsPartAttrib3ReleaseCaptured), column == OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
			AssertEquals(nameof(relation.OU_IsSerialNumberReleaseCaptured), column == OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured ? expectedColumnValue : defaultExpectedValue, relation.OU_UsePartAttrib1);
		}

		void AssertRelationshipInfosErrors(OrgPartRelation relation, SchemaBoolColumn columnThatShouldHaveError, string errorMessage)
		{
			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_UsePartAttrib1)
			{
				AssertHasError(relation.OU_UsePartAttrib1Info, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_UsePartAttrib1Info);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_UsePartAttrib2)
			{
				AssertHasError(relation.OU_UsePartAttrib2Info, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_UsePartAttrib2Info);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_UsePartAttrib3)
			{
				AssertHasError(relation.OU_UsePartAttrib3Info, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_UsePartAttrib3Info);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_UseSerialNumber)
			{
				AssertHasError(relation.OU_UseSerialNumberInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_UseSerialNumberInfo);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_UseExpiryDate)
			{
				AssertHasError(relation.OU_UseExpiryDateInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_UseExpiryDateInfo);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_UsePackingDate)
			{
				AssertHasError(relation.OU_UsePackingDateInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_UsePackingDateInfo);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured)
			{
				AssertHasError(relation.OU_IsPartAttrib1ReleaseCapturedInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_IsPartAttrib1ReleaseCapturedInfo);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured)
			{
				AssertHasError(relation.OU_IsPartAttrib2ReleaseCapturedInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_IsPartAttrib2ReleaseCapturedInfo);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured)
			{
				AssertHasError(relation.OU_IsPartAttrib3ReleaseCapturedInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_IsPartAttrib3ReleaseCapturedInfo);
			}

			if (columnThatShouldHaveError == OrgPartRelationSchema.OU_IsSerialNumberReleaseCaptured)
			{
				AssertHasError(relation.OU_IsSerialNumberReleaseCapturedInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(relation.OU_IsSerialNumberReleaseCapturedInfo);
			}
		}

		#endregion

		#region TestCheckOU_ConsigneeMinShelfLifeAccepted_NotNegative

		public void TestCheckOU_ConsigneeMinShelfLifeAccepted_NotNegative()
		{
			var part = Factory.New<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_ConsigneeMinShelfLifeAccepted = 0;
			AssertNoErrors("Precondition", relation.OU_ConsigneeMinShelfLifeAcceptedInfo);

			relation.OU_ConsigneeMinShelfLifeAccepted = -1;
			AssertHasError(relation.OU_ConsigneeMinShelfLifeAcceptedInfo, "Consignee Minimum Shelf Life Accepted Days cannot be negative.");

			relation.OU_ConsigneeMinShelfLifeAccepted = 2;
			AssertNoErrors("Posetive value is valid.", relation.OU_ConsigneeMinShelfLifeAcceptedInfo);
		}

		#endregion

		#region TestCheckOU_ConsigneeMinShelfLifeAccepted_MinShelfLessThanMaxShelf

		public void TestCheckOU_ConsigneeMinShelfLifeAccepted_MinShelfLessThanMaxShelf()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse1 = helper.CreateWarehouse("MEL", "A");
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "PartA");
			helper.CreateProductParamsByWhsAndClient(part1.PK, client1.PK, warehouse1.PK, 45);

			var relation1 = part1.RelatedOrganisations.FindFirstByOrganisationPK(client1.PK);

			AssertNoErrors("Precondition", relation1.OU_ConsigneeMinShelfLifeAcceptedInfo);

			relation1.OU_ConsigneeMinShelfLifeAccepted = 70;
			AssertHasError(relation1.OU_ConsigneeMinShelfLifeAcceptedInfo, "Minimum shelf life 70 cannot be greater than Maximum Shelf Life 45.");

			relation1.OU_ConsigneeMinShelfLifeAccepted = 40;
			AssertNoErrors("Minimum shelf life is less than Maximum should not have error.", relation1.OU_ConsigneeMinShelfLifeAcceptedInfo);
		}

		#endregion

		#region TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate

		public void TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate_Owner()
		{
			TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate_Core(OrgPartRelation.RelationshipTypes.Owner);
		}

		public void TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate_Both()
		{
			TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate_Core(OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate_WarehouseConsignee()
		{
			TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate_Core(OrgPartRelation.RelationshipTypes.WarehouseConsignee);
		}

		void TestCheckOU_ConsigneeMinShelfLifeAccepted_CannotBeModifiedIfPickInProgressHasProductWithExpiryDate_Core(string relationship)
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			helper.CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, 60);
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			client.MiscServ.OM_IMUseExpiryDate = true;

			var ownerRelation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			ownerRelation.OU_UseExpiryDate = true;

			var relation = ownerRelation;
			if (relationship != OrgPartRelation.RelationshipTypes.Owner && relationship != OrgPartRelation.RelationshipTypes.Both)
			{
				relation = (OrgPartRelation)helper.CreateProductClientRelationShip(consignee.PK, part.PK);
			}
			relation.OU_Relationship = relationship;
			Factory.Save();

			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			Factory.Load<IWhsDocketLine>(receiveLinePK).WE_ExpiryDate = today.AddDays(10);
			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var expectedErrorMessage = "This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.";

			// If there no Orders or Picks for the consignee.
			relation.OU_ConsigneeMinShelfLifeAccepted = 0;
			AssertNoError("Precondition", relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			AssertNoError(relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			relation.OU_ConsigneeMinShelfLifeAccepted = 8;
			AssertNoError(relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			// With Orders
			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			var orderLinePK = helper.CreateWhsOrderLine(order.PK, part.PK, 10m);
			Factory.Load<IWhsDocketLine>(orderLinePK).WE_ExpiryDate = today.AddDays(10);
			Factory.Save();
			AssertNoError(relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			AssertNoError(relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			relation.OU_ConsigneeMinShelfLifeAccepted = 7;
			AssertNoError(relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			// Pick
			helper.CreateWhsPick(new ZGuid[] { order.PK });
			Factory.Save();
			AssertNoError(relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			AssertHasError("User should NOT be allowed to increase Minimum Shelf Life.", relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			relation.OU_ConsigneeMinShelfLifeAccepted = 5;
			AssertNoError("User should be allowed to decrease Minimum Shelf Life.", relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			relation.OU_ConsigneeMinShelfLifeAccepted = 20;
			AssertHasError("User should NOT be allowed to increase Minimum Shelf Life.", relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 5;
			Factory.Save();

			relation.OU_ConsigneeMinShelfLifeAccepted = 20;
			AssertNoError("If consignee override value it should not get error here.", relation.OU_ConsigneeMinShelfLifeAcceptedInfo, expectedErrorMessage);
		}

		#endregion

		#region TestOU_WHC_DefaultInventoryHoldCode

		public void TestCheckOU_WHC_DefaultInventoryHoldCode()
		{
			var relation = Factory.New<OrgPartRelation>();

			AssertEquals("Precondition: No default inventory hold code.", ZGuid.Empty, relation.OU_WHC_DefaultInventoryHoldCode);
			AssertNoErrors(relation.OU_WHC_DefaultInventoryHoldCodeInfo);

			relation.OU_WHC_DefaultInventoryHoldCode = ZGuid.Invalid;
			AssertEquals("Invalid guid inventory hold code.", ZGuid.Invalid, relation.OU_WHC_DefaultInventoryHoldCode);
			AssertHasError("Invalid guid inventory hold code.", relation.OU_WHC_DefaultInventoryHoldCodeInfo, "Enter a valid Default Hold Code.");

			var validHoldCode = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Single();
			relation.OU_WHC_DefaultInventoryHoldCode = validHoldCode.PK;
			AssertEquals("Valid guid inventory hold code.", validHoldCode.PK, relation.OU_WHC_DefaultInventoryHoldCode);
			AssertNoErrors("Valid guid inventory hold code.", relation.OU_WHC_DefaultInventoryHoldCodeInfo);

			relation.OU_WHC_DefaultInventoryHoldCode = ZGuid.BrettsGuid;
			AssertEquals("Wrong inventory hold code.", ZGuid.BrettsGuid, relation.OU_WHC_DefaultInventoryHoldCode);
			AssertHasError("Wrong inventory hold code.", relation.OU_WHC_DefaultInventoryHoldCodeInfo, "Enter a valid Default Hold Code.");
		}

		#endregion

		#region TestCheckOU_ReceiveOverageTolerancePercent

		#region TestCheckOU_ReceiveOverageTolerancePercent_IsNegative()

		public void TestCheckOU_ReceiveOverageTolerancePercent_IsNegative()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_ReceiveOverageTolerancePercent = -1;
			AssertEquals((ZShort)(-1), relation.OU_ReceiveOverageTolerancePercent);
			AssertHasError(relation.OU_ReceiveOverageTolerancePercentInfo, "Please enter a 'Receive Overage Tolerance Percent' within the range 0 to 500.");
		}

		#endregion

		#region TestCheckOU_ReceiveOverageTolerancePercent_LargerThanFiveHundreds()

		public void TestCheckOU_ReceiveOverageTolerancePercent_LargerThanFiveHundreds()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_ReceiveOverageTolerancePercent = 501;
			AssertEquals((ZShort)501, relation.OU_ReceiveOverageTolerancePercent);
			AssertHasError(relation.OU_ReceiveOverageTolerancePercentInfo, "Please enter a 'Receive Overage Tolerance Percent' within the range 0 to 500.");
		}

		#endregion

		#region TestCheckOU_ReceiveOverageTolerancePercent_BetweenZeroAndFiveHundreds()

		public void TestCheckOU_ReceiveOverageTolerancePercent_BetweenZeroAndFiveHundreds()
		{
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_ReceiveOverageTolerancePercent = 0;
			AssertEquals(ZShort.Zero, relation.OU_ReceiveOverageTolerancePercent);
			AssertNoErrors(relation.OU_ReceiveOverageTolerancePercentInfo);

			relation.OU_ReceiveOverageTolerancePercent = 50;
			AssertEquals((ZShort)50, relation.OU_ReceiveOverageTolerancePercent);
			AssertNoErrors(relation.OU_ReceiveOverageTolerancePercentInfo);

			relation.OU_ReceiveOverageTolerancePercent = 500;
			AssertEquals((ZShort)500, relation.OU_ReceiveOverageTolerancePercent);
			AssertNoErrors(relation.OU_ReceiveOverageTolerancePercentInfo);
		}

		#endregion

		#endregion

		#region TestCheckDateFormats

		#region TestCheckOU_ExpiryDateFormatString

		public void TestCheckOU_ExpiryDateFormatString_ValidEmpty()
		{
			TestCheckDateFormatStringCore_Valid_Empty(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo);
		}

		public void TestCheckOU_ExpiryDateFormatString_ValidStandard()
		{
			TestCheckDateFormatStringCore_Valid_Standard(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo);
		}

		public void TestCheckOU_ExpiryDateFormatString_ValidCustom()
		{
			TestCheckDateFormatStringCore_Valid_Custom(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo);
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidStandard_ContainsTimeWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_StandardContainsTime(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format contains time (time data will be ignored)");
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidStandard_NoDayWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_StandardNoDay(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format has no day (will default to first of month)");
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidStandard_UnknownError()
		{
			TestCheckDateFormatStringCore_InvalidError_StandardUnknown(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format uses a format that is invalid");
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidStandard_NotEnoughInfoError()
		{
			TestCheckDateFormatStringCore_InvalidError_StandardNotEnoughInfo(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format does not contain enough information (must have a minimum of month and year, e.g. MM/yyyy)");
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidCustom_ContainsTimeWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_CustomContainsTime(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format contains time (time data will be ignored)");
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidCustom_NoDayWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_CustomNoDay(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format has no day (will default to first of month)");
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidCustom_Y2KWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_CustomY2K(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format is subject to Y2K-style errors as only 2 digits for year are used");
		}

		public void TestCheckOU_ExpiryDateFormatString_InvalidCustom_NotEnoughInfoError()
		{
			TestCheckDateFormatStringCore_InvalidError_CustomNotEnoughInfo(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				"RF Expiry Date Format does not contain enough information (must have a minimum of month and year, e.g. MM/yyyy)");
		}

		public void TestCheckOU_ExpiryDateFormatString_AllWarnings()
		{
			TestCheckDateFormatStringCore_InvalidWarning_All(
				(relation, dateFormat) => relation.OU_ExpiryDateFormatString = dateFormat,
				relation => relation.OU_ExpiryDateFormatStringInfo,
				new string[] {
					"RF Expiry Date Format contains time (time data will be ignored)",
					"RF Expiry Date Format has no day (will default to first of month)",
					"RF Expiry Date Format is subject to Y2K-style errors as only 2 digits for year are used" });
		}

		#endregion

		#region TestCheckOU_PackingDateFormatString

		public void TestCheckOU_PackingDateFormatString_ValidEmpty()
		{
			TestCheckDateFormatStringCore_Valid_Empty(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo);
		}

		public void TestCheckOU_PackingDateFormatString_ValidStandard()
		{
			TestCheckDateFormatStringCore_Valid_Standard(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo);
		}

		public void TestCheckOU_PackingDateFormatString_ValidCustom()
		{
			TestCheckDateFormatStringCore_Valid_Custom(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo);
		}

		public void TestCheckOU_PackingDateFormatString_InvalidStandard_ContainsTimeWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_StandardContainsTime(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format contains time (time data will be ignored)");
		}

		public void TestCheckOU_PackingDateFormatString_InvalidStandard_NoDayWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_StandardNoDay(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format has no day (will default to first of month)");
		}

		public void TestCheckOU_PackingDateFormatString_InvalidStandard_UnknownError()
		{
			TestCheckDateFormatStringCore_InvalidError_StandardUnknown(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format uses a format that is invalid");
		}

		public void TestCheckOU_PackingDateFormatString_InvalidStandard_NotEnoughInfoError()
		{
			TestCheckDateFormatStringCore_InvalidError_StandardNotEnoughInfo(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format does not contain enough information (must have a minimum of month and year, e.g. MM/yyyy)");
		}

		public void TestCheckOU_PackingDateFormatString_InvalidCustom_ContainsTimeWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_CustomContainsTime(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format contains time (time data will be ignored)");
		}

		public void TestCheckOU_PackingDateFormatString_InvalidCustom_NoDayWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_CustomNoDay(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format has no day (will default to first of month)");
		}

		public void TestCheckOU_PackingDateFormatString_InvalidCustom_Y2KWarning()
		{
			TestCheckDateFormatStringCore_InvalidWarning_CustomY2K(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format is subject to Y2K-style errors as only 2 digits for year are used");
		}

		public void TestCheckOU_PackingDateFormatString_InvalidCustom_NotEnoughInfoError()
		{
			TestCheckDateFormatStringCore_InvalidError_CustomNotEnoughInfo(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				"RF Packing Date Format does not contain enough information (must have a minimum of month and year, e.g. MM/yyyy)");
		}

		public void TestCheckOU_PackingDateFormatString_AllWarnings()
		{
			TestCheckDateFormatStringCore_InvalidWarning_All(
				(relation, dateFormat) => relation.OU_PackingDateFormatString = dateFormat,
				relation => relation.OU_PackingDateFormatStringInfo,
				new string[] {
					"RF Packing Date Format contains time (time data will be ignored)",
					"RF Packing Date Format has no day (will default to first of month)",
					"RF Packing Date Format is subject to Y2K-style errors as only 2 digits for year are used" });
		}

		#endregion

		#region TestCheckDateFormatStringCore_Valid

		void TestCheckDateFormatStringCore_Valid_Empty(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo)
		{
			TestCheckDateFormatStringCore_Valid_Core(setDateFormatString, getPropInfo, new string[] { "", " " });
		}

		void TestCheckDateFormatStringCore_Valid_Standard(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo)
		{
			TestCheckDateFormatStringCore_Valid_Core(setDateFormatString, getPropInfo, new string[] { "d", "D" });
		}

		void TestCheckDateFormatStringCore_Valid_Custom(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo)
		{
			TestCheckDateFormatStringCore_Valid_Core(
				setDateFormatString,
				getPropInfo,
				new string[] { "ddMMyyyy", "yyyy-MM-dd", "dd/MM/yyyy" });
		}

		void TestCheckDateFormatStringCore_Valid_Core(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string[] formats)
		{
			var relation = Factory.New<OrgPartRelation>();

			foreach (var format in formats)
			{
				setDateFormatString(relation, format);
				AssertNoErrors(getPropInfo(relation));
				AssertNoWarnings(getPropInfo(relation));
			}
		}

		#endregion

		#region TestCheckDateFormatStringCore_InvalidWarning

		void TestCheckDateFormatStringCore_InvalidWarning_StandardContainsTime(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedWarningMessage)
		{
			TestCheckDateFormatStringCore_InvalidWarning_Core(
				setDateFormatString,
				getPropInfo,
				expectedWarningMessage,
				new string[] { "u", "s", "O" });
		}

		void TestCheckDateFormatStringCore_InvalidWarning_StandardNoDay(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedWarningMessage)
		{
			TestCheckDateFormatStringCore_InvalidWarning_Core(
				setDateFormatString,
				getPropInfo,
				expectedWarningMessage,
				new string[] { "y", "Y" });
		}

		void TestCheckDateFormatStringCore_InvalidWarning_CustomContainsTime(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedWarningMessage)
		{
			TestCheckDateFormatStringCore_InvalidWarning_Core(
				setDateFormatString,
				getPropInfo,
				expectedWarningMessage,
				new string[] { "ddMMyyyy fffffff", "yyyy-MM-dd hh:mm:ss", "HH dd/MM/yyyy" });
		}

		void TestCheckDateFormatStringCore_InvalidWarning_CustomNoDay(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedWarningMessage)
		{
			TestCheckDateFormatStringCore_InvalidWarning_Core(
				setDateFormatString,
				getPropInfo,
				expectedWarningMessage,
				new string[] { "MMyyyy", "yyyy-MM", "MM/yyyy" });
		}

		void TestCheckDateFormatStringCore_InvalidWarning_CustomY2K(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedWarningMessage)
		{
			TestCheckDateFormatStringCore_InvalidWarning_Core(
				setDateFormatString,
				getPropInfo,
				expectedWarningMessage,
				new string[] { "ddMMyy", "yy-MM-dd", "dd/MM/y" });
		}

		void TestCheckDateFormatStringCore_InvalidWarning_Core(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedWarningMessage,
			string[] formats)
		{
			var relation = Factory.New<OrgPartRelation>();

			foreach (var format in formats)
			{
				setDateFormatString(relation, format);
				AssertNoErrors(getPropInfo(relation));
				AssertHasWarning(getPropInfo(relation), expectedWarningMessage);
			}
		}

		void TestCheckDateFormatStringCore_InvalidWarning_All(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string[] expectedWarningMessages)
		{
			var relation = Factory.New<OrgPartRelation>();
			var format = "yy/MM ss";

			setDateFormatString(relation, format);
			AssertNoErrors(getPropInfo(relation));

			foreach (var expectedWarningMessage in expectedWarningMessages)
			{
				AssertHasWarning(getPropInfo(relation), expectedWarningMessage);
			}
		}

		#endregion

		#region TestCheckDateFormatStringCore_InvalidError

		void TestCheckDateFormatStringCore_InvalidError_StandardUnknown(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedErrorMessage)
		{
			TestCheckDateFormatStringCore_InvalidError_Core(
				setDateFormatString,
				getPropInfo,
				expectedErrorMessage,
				new string[] { "~", "0", "\\" });
		}

		void TestCheckDateFormatStringCore_InvalidError_StandardNotEnoughInfo(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedErrorMessage)
		{
			TestCheckDateFormatStringCore_InvalidError_Core(
				setDateFormatString,
				getPropInfo,
				expectedErrorMessage,
				new string[] { "M", "m" });
		}

		void TestCheckDateFormatStringCore_InvalidError_CustomNotEnoughInfo(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedErrorMessage)
		{
			TestCheckDateFormatStringCore_InvalidError_Core(
				setDateFormatString,
				getPropInfo,
				expectedErrorMessage,
				new string[] { "yyyy", "dd/yyyy", "dd/MM" });
		}

		void TestCheckDateFormatStringCore_InvalidError_Core(
			Action<OrgPartRelation, string> setDateFormatString,
			Func<OrgPartRelation, ZPropertyInfo> getPropInfo,
			string expectedErrorMessage,
			string[] formats)
		{
			var relation = Factory.New<OrgPartRelation>();

			foreach (var format in formats)
			{
				setDateFormatString(relation, format);
				AssertNoWarnings(getPropInfo(relation));
				AssertHasError(getPropInfo(relation), expectedErrorMessage);
			}
		}

		#endregion

		#endregion
	}
}
