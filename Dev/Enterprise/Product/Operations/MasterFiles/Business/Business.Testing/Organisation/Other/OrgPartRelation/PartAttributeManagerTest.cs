using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PartAttributeManagerTest : TestCaseWithFactory
	{
		#region TestIsPartAttributeReleaseCaptured

		public void TestIsPartAttributeReleaseCaptured()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var manager = orgHeader.PartAttributeManager;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123245";
			var relation = product.RelatedOrganisations.AddOwner(orgHeader);
			var miscServ = orgHeader.MiscServ;
			miscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			miscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			miscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;

			AssertEquals(false, manager.IsPartAttributeReleaseCaptured(product, 1));
			AssertEquals(false, manager.IsPartAttributeReleaseCaptured(product, 2));
			AssertEquals(false, manager.IsPartAttributeReleaseCaptured(product, 3));

			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			AssertEquals(false, manager.IsPartAttributeReleaseCaptured(product, 1));
			AssertEquals(false, manager.IsPartAttributeReleaseCaptured(product, 2));
			AssertEquals(false, manager.IsPartAttributeReleaseCaptured(product, 3));

			relation.OU_IsPartAttrib1ReleaseCaptured = true;
			relation.OU_IsPartAttrib2ReleaseCaptured = true;
			relation.OU_IsPartAttrib3ReleaseCaptured = true;
			AssertEquals(true, manager.IsPartAttributeReleaseCaptured(product, 1));
			AssertEquals(true, manager.IsPartAttributeReleaseCaptured(product, 2));
			AssertEquals(true, manager.IsPartAttributeReleaseCaptured(product, 3));
		}

		#endregion

		#region TestConstructor

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor()
		{
			AssertEquals(Org.PK, Manager.Organisation.PK);
			Manager = new PartAttributeManager(null);
		}

		#endregion

		#region TestOrganisation

		public void TestOrganisation()
		{
			AssertEquals(Org.PK, Manager.Organisation.PK);
		}

		#endregion

		#region TestVINAttribute

		public void TestVINAttribute()
		{
			AssertNull(Manager.VINPartAttribute);
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			string attribName = "VIN Noodle";
			OrgMisc.OM_IMPartAttrib2Name = attribName;
			AssertNotNull(Manager.VINPartAttribute);
			AssertEquals("Index", 2, Manager.VINPartAttribute.Index);
			AssertEquals("Name", attribName, Manager.VINPartAttribute.Name);
		}

		#endregion

		#region TestHasVINForProduct

		public void TestHasVINForProduct()
		{
			AssertEquals("Has VIN", false, Manager.HasVINForProduct(Part));
			Manager.SetProductToUseAttribute(Part, 2, true);
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.VIN;
			AssertEquals("Has VIN", true, Manager.HasVINForProduct(Part));
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			AssertEquals("Has VIN", false, Manager.HasVINForProduct(Part));
		}

		#endregion

		#region TestIsPartAttributeUsedByProduct

		public void TestIsPartAttributeUsedByProduct()
		{
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 1));
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 2));
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 3));

			// only need to test 1 attribute as generic property construct is used
			Manager.SetProductToUseAttribute(Part, 2, true);
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 2));
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 1));
			AssertEquals(true, Manager.IsPartAttributeUsedByProduct(Part, 2));
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 3));
		}

		#endregion

		#region TestIsExpiryDateUsedByProduct

		public void TestIsExpiryDateUsedByProduct()
		{
			AssertEquals(false, Manager.IsExpiryDateUsedByProduct(Part));
			Manager.SetProductToUseAttribute(Part, 4, true);
			AssertEquals(false, Manager.IsExpiryDateUsedByProduct(Part));
			OrgMisc.OM_IMUseExpiryDate = true;
			AssertEquals(true, Manager.IsExpiryDateUsedByProduct(Part));
		}

		#endregion

		#region TestIsPackingDateUsedByProduct

		public void TestIsPackingDateUsedByProduct()
		{
			AssertEquals(false, Manager.IsPackingDateUsedByProduct(Part));
			Manager.SetProductToUseAttribute(Part, 5, true);
			AssertEquals(false, Manager.IsPackingDateUsedByProduct(Part));
			OrgMisc.OM_IMUsePackingDate = true;
			AssertEquals(true, Manager.IsPackingDateUsedByProduct(Part));
		}

		#endregion

		#region TestIsSerialNumberUsedByProduct

		public void TestIsSerialNumberUsedByProduct()
		{
			AssertEquals(false, Manager.IsSerialNumberUsedByProduct(Part));
			Manager.SetProductToUseAttribute(Part, 6, true);
			AssertEquals(false, Manager.IsSerialNumberUsedByProduct(Part));
			OrgMisc.OM_IMUseSerialNumber = true;
			AssertEquals(true, Manager.IsSerialNumberUsedByProduct(Part));
		}

		#endregion

		#region TestIsSerialNumberReleaseCaptured

		public void TestIsSerialNumberReleaseCaptured()
		{
			AssertEquals(false, Manager.IsSerialNumberReleaseCaptured(Part));
			Manager.SetProductToUseAttribute(Part, 6, true);
			AssertEquals(false, Manager.IsSerialNumberReleaseCaptured(Part));
			OrgMisc.OM_IMUseSerialNumber = true;
			AssertEquals(false, Manager.IsSerialNumberReleaseCaptured(Part));
			Part.RelatedOrganisations.FindByOrganisationPKAndRelationship(Org.PK, OrgPartRelation.RelationshipTypes.Owner).OU_IsSerialNumberReleaseCaptured = true;
			AssertEquals(true, Manager.IsSerialNumberReleaseCaptured(Part));
		}

		#endregion

		#region TestExpiryDateFormatString

		public void TestExpiryDateFormatString()
		{
			AssertEquals("", Manager.ExpiryDateFormatString(Part));
			Manager.SetProductToUseAttribute(Part, 4, true);
			AssertEquals("", Manager.ExpiryDateFormatString(Part));
			OrgMisc.OM_IMUseExpiryDate = true;
			Part.RelatedOrganisations[0].OU_ExpiryDateFormatString = "YYMMDD";
			AssertEquals("YYMMDD", Manager.ExpiryDateFormatString(Part));
		}

		#endregion

		#region TestPackingDateFormatString

		public void TestPackingDateFormatString()
		{
			AssertEquals("", Manager.PackingDateFormatString(Part));
			Manager.SetProductToUseAttribute(Part, 5, true);
			AssertEquals("", Manager.PackingDateFormatString(Part));
			OrgMisc.OM_IMUsePackingDate = true;
			Part.RelatedOrganisations[0].OU_PackingDateFormatString = "DDMMYY";
			AssertEquals("DDMMYY", Manager.PackingDateFormatString(Part));
		}

		#endregion

		#region Organisation Level Queries

		#region TestPartAttributeName1

		public void TestPartAttributeName1()
		{
			AssertEquals("Part Attrib. 1", Manager.PartAttributeName1);
			OrgMisc.OM_IMPartAttrib1Name = "Batch No";
			AssertEquals("Batch No", Manager.PartAttributeName1);
		}

		#endregion

		#region TestPartAttributeName2

		public void TestPartAttributeName2()
		{
			AssertEquals((NoResString)"Part Attrib. 2", Manager.PartAttributeName2);
			OrgMisc.OM_IMPartAttrib2Name = "Batch No";
			AssertEquals("Batch No", Manager.PartAttributeName2);
		}

		#endregion

		#region TestPartAttributeName3

		public void TestPartAttributeName3()
		{
			AssertEquals((NoResString)"Part Attrib. 3", Manager.PartAttributeName3);
			OrgMisc.OM_IMPartAttrib3Name = "Batch No";
			AssertEquals("Batch No", Manager.PartAttributeName3);
		}

		#endregion

		public void TestSerialNumberName()
		{
			AssertEquals("Serial Number", Manager.SerialNumberName);
		}

		#region TestPartAttributeName

		public void TestPartAttributeName()
		{
			AssertEquals((NoResString)"Part Attrib. 1", Manager.PartAttributeName(1));
			AssertEquals((NoResString)"Part Attrib. 2", Manager.PartAttributeName(2));
			AssertEquals((NoResString)"Part Attrib. 3", Manager.PartAttributeName(3));
			OrgMisc.OM_IMPartAttrib1Name = "Batch No";
			OrgMisc.OM_IMPartAttrib2Name = "Serial No";
			OrgMisc.OM_IMPartAttrib3Name = "VIN No";
			AssertEquals("Batch No", Manager.PartAttributeName(1));
			AssertEquals("Serial No", Manager.PartAttributeName(2));
			AssertEquals("VIN No", Manager.PartAttributeName(3));
			AssertEquals("Serial Number", Manager.PartAttributeName(4));

			// test generic property error
			ZString temp = Manager.PartAttributeName(0);
			AssertEquals("PartAttributeName(int)", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			temp = Manager.PartAttributeName(PartAttributeManager.MaxAttributes + 1);
			AssertEquals("PartAttributeName(int)", ErrorReporter.LastKeyReported);
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region TestPartAttributeNames

		public void TestPartAttributeName1_Translatable()
		{
			AssertEquals("Part Attrib. 1", Manager.PartAttributeName1);
			OrgMisc.OM_IMPartAttrib1Name = "Colour";
			AssertEquals("Colour", Manager.PartAttributeName1);

			var resKey = OrgMisc.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(OrgMisc, "Colour").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "颜色"));
				AssertEquals("颜色", Manager.PartAttributeName1);
			}
		}
		public void TestPartAttributeName2_Translatable()
		{
			AssertEquals("Part Attrib. 2", Manager.PartAttributeName2);
			OrgMisc.OM_IMPartAttrib2Name = "Serial Number";
			AssertEquals("Serial Number", Manager.PartAttributeName2);

			var resKey = OrgMisc.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(OrgMisc, "Serial Number").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "序列号"));
				AssertEquals("序列号", Manager.PartAttributeName2);
			}
		}
		public void TestPartAttributeName3_Translatable()
		{
			AssertEquals("Part Attrib. 3", Manager.PartAttributeName3);
			OrgMisc.OM_IMPartAttrib3Name = "Batch No.";
			AssertEquals("Batch No.", Manager.PartAttributeName3);

			var resKey = OrgMisc.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(OrgMisc, "Batch No.").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "批号"));
				AssertEquals("批号", Manager.PartAttributeName3);
			}
		}

		#endregion

		#region TestIsPartAttributeUsedByOrganisation

		public void TestIsPartAttributeUsedByOrganisation()
		{
			AssertEquals(false, Manager.IsPartAttributeUsedByOrganisation(1));
			AssertEquals(false, Manager.IsPartAttributeUsedByOrganisation(2));
			AssertEquals(false, Manager.IsPartAttributeUsedByOrganisation(3));

			// only need to test 1 attribute as generic property construct is used
			OrgMisc.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;
			AssertEquals(false, Manager.IsPartAttributeUsedByOrganisation(1));
			AssertEquals(false, Manager.IsPartAttributeUsedByOrganisation(2));
			AssertEquals(true, Manager.IsPartAttributeUsedByOrganisation(3));

			// test generic property error
			Manager.IsPartAttributeUsedByOrganisation(0);
			AssertEquals("PartAttributeType(int)", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			Manager.IsPartAttributeUsedByOrganisation(PartAttributeManager.MaxAttributes + 1);
			AssertEquals("PartAttributeType(int)", ErrorReporter.LastKeyReported);
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region TestIsPartAttributeAJulianBatchNumber

		public void TestIsPartAttributeAJulianBatchNumber()
		{
			AssertEquals(false, Manager.IsPartAttributeAJulianBatchNumber(1));
			AssertEquals(false, Manager.IsPartAttributeAJulianBatchNumber(2));
			AssertEquals(false, Manager.IsPartAttributeAJulianBatchNumber(3));

			OrgMisc.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			OrgMisc.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals(true, Manager.IsPartAttributeAJulianBatchNumber(1));
			AssertEquals(true, Manager.IsPartAttributeAJulianBatchNumber(2));
			AssertEquals(true, Manager.IsPartAttributeAJulianBatchNumber(3));

			OrgMisc.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			OrgMisc.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals(false, Manager.IsPartAttributeAJulianBatchNumber(1));
			AssertEquals(false, Manager.IsPartAttributeAJulianBatchNumber(2));
			AssertEquals(false, Manager.IsPartAttributeAJulianBatchNumber(3));
		}

		#endregion

		#region TestIsExpiryDateUsedByOrganisation

		public void TestIsExpiryDateUsedByOrganisation()
		{
			AssertEquals(false, Manager.IsExpiryDateUsedByOrganisation);
			OrgMisc.OM_IMUseExpiryDate = true;
			AssertEquals(true, Manager.IsExpiryDateUsedByOrganisation);
		}

		#endregion

		#region TestIsPackingDateUsedByOrganisation

		public void TestIsPackingDateUsedByOrganisation()
		{
			AssertEquals(false, Manager.IsPackingDateUsedByOrganisation);
			OrgMisc.OM_IMUsePackingDate = true;
			AssertEquals(true, Manager.IsPackingDateUsedByOrganisation);
		}

		#endregion

		#region TestIsSerialnumberUsedByOrganisation

		public void TestIsSerialnumberUsedByOrganisation()
		{
			AssertEquals(false, Manager.IsSerialNumberUsedByOrganisation);
			OrgMisc.OM_IMUseSerialNumber = true;
			AssertEquals(true, Manager.IsSerialNumberUsedByOrganisation);
		}

		#endregion

		#endregion

		#region TestIsCompletePalletPickingUsedByProduct

		public void TestIsCompletePalletPickingUsedByProduct()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.RelatedOrganisations.AddOwner(org1);
			part1.RelatedOrganisations.AddOwner(org2).OU_CompletePalletPicking = true;

			AssertEquals(false, org1.PartAttributeManager.IsCompletePalletPickingUsedByProduct(part1));
			AssertEquals(true, org2.PartAttributeManager.IsCompletePalletPickingUsedByProduct(part1));

			var part2 = Factory.New<OrgSupplierPart>();
			AssertEquals("Should handle unrelated products", false, org2.PartAttributeManager.IsCompletePalletPickingUsedByProduct(part2));
			AssertEquals("Should handle null products", false, org2.PartAttributeManager.IsCompletePalletPickingUsedByProduct(null));
		}

		#endregion

		#region TestIsPartAttributeMandatory

		public void TestIsPartAttributeMandatory()
		{
			AssertEquals(false, Manager.IsPartAttributeMandatory(1));
			AssertEquals(false, Manager.IsPartAttributeMandatory(2));
			AssertEquals(false, Manager.IsPartAttributeMandatory(3));

			AssertIsPartAttribMandatory(OrgMisc.OM_IMPartAttrib1TypeInfo, 1);
			AssertIsPartAttribMandatory(OrgMisc.OM_IMPartAttrib2TypeInfo, 2);
			AssertIsPartAttribMandatory(OrgMisc.OM_IMPartAttrib3TypeInfo, 3);
		}

		#endregion

		#region TestIsAttributeNeutralUsedByProduct

		public void TestIsAttributeNeutralUsedByProduct()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.RelatedOrganisations.AddOwner(org1);
			part1.RelatedOrganisations.AddOwner(org2).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			AssertEquals(false, org1.PartAttributeManager.IsAttributeNeutralUsedByProduct(part1));
			AssertEquals(true, org2.PartAttributeManager.IsAttributeNeutralUsedByProduct(part1));

			var part2 = Factory.New<OrgSupplierPart>();
			AssertEquals("Should handle unrelated products", false, org2.PartAttributeManager.IsAttributeNeutralUsedByProduct(part2));
			AssertEquals("Should handle null products", false, org2.PartAttributeManager.IsAttributeNeutralUsedByProduct(null));
		}

		#endregion

		#region TestIsDocumentRollUp()

		public void TestIsDocumentRollUp()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var org4 = Factory.New<OrgHeader>();
			var part1 = Factory.New<OrgSupplierPart>();

			AssertEquals("Should handle unrelated products", false, org2.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part1));
			AssertEquals("Should handle null products", false, org2.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(null));

			var relation1 = part1.RelatedOrganisations.AddOwner(org1);
			var relation2 = part1.RelatedOrganisations.AddOwner(org2);
			var relation3 = part1.RelatedOrganisations.AddOwner(org3);
			var relation4 = part1.RelatedOrganisations.AddOwner(org4);

			relation1.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation2.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			relation3.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation4.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation2.OU_RollUpAttributesOnDocuments = true;
			relation4.OU_RollUpAttributesOnDocuments = true;

			AssertEquals(false, org1.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part1));
			AssertEquals(false, org2.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part1));
			AssertEquals(false, org3.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part1));
			AssertEquals(true, org4.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part1));
		}

		#endregion

		#region TestIsPartAttributeAJulianBatchNumberAndUsed

		public void TestIsPartAttributeAJulianBatchNumberAndUsed()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.RelatedOrganisations.AddOwner(org1);

			TestIsPartAttributeAJulianBatchNumberAndUsedCore(org1, part1, OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, 1);
			TestIsPartAttributeAJulianBatchNumberAndUsedCore(org1, part1, OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, 2);
			TestIsPartAttributeAJulianBatchNumberAndUsedCore(org1, part1, OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, 3);

			var part2 = Factory.New<OrgSupplierPart>();
			AssertEquals("Should handle unrelated products", false, org2.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(part2, 1));
			AssertEquals("Should handle null products", false, org2.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(null, 1));
		}

		void TestIsPartAttributeAJulianBatchNumberAndUsedCore(OrgHeader client, OrgSupplierPart part, SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn usePartAttributeColumn, int attributeNumber)
		{
			AssertEquals(false, client.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(part, attributeNumber));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals(false, client.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(part, attributeNumber));

			part.RelatedOrganisations[0][usePartAttributeColumn] = true;
			AssertEquals(false, client.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(part, attributeNumber));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals(true, client.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(part, attributeNumber));

			part.RelatedOrganisations[0][usePartAttributeColumn] = false;
			AssertEquals(false, client.PartAttributeManager.IsPartAttributeAJulianBatchNumberAndUsed(part, attributeNumber));
		}

		#endregion

		#region TestIsAJulianBatchNumberAttributeUsed

		public void TestIsAJulianBatchNumberAttributeUsed()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.RelatedOrganisations.AddOwner(org1);

			TestIsAJulianBatchNumberAttributeUsedCore(org1, part1, OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1);
			TestIsAJulianBatchNumberAttributeUsedCore(org1, part1, OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2);
			TestIsAJulianBatchNumberAttributeUsedCore(org1, part1, OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3);

			var part2 = Factory.New<OrgSupplierPart>();
			AssertEquals("Should handle unrelated products", false, org2.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(part2));
			AssertEquals("Should handle null products", false, org2.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(null));
		}

		void TestIsAJulianBatchNumberAttributeUsedCore(OrgHeader client, OrgSupplierPart part, SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn usePartAttributeColumn)
		{
			AssertEquals(false, client.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(part));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals(false, client.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(part));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals(false, client.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(part));

			part.RelatedOrganisations[0][usePartAttributeColumn] = true;
			AssertEquals(true, client.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(part));

			part.RelatedOrganisations[0][usePartAttributeColumn] = false;
			AssertEquals(false, client.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(part));
		}

		#endregion

		#region TestSetProductToUseAttribute1

		public void TestSetProductToUseAttribute1()
		{
			Manager.SetProductToUseAttribute(Part, 1, true);
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 1));
			OrgMisc.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			AssertEquals(true, Manager.IsPartAttributeUsedByProduct(Part, 1));
		}

		#endregion

		#region TestSetProductToUseAttribute2

		public void TestSetProductToUseAttribute2()
		{
			Manager.SetProductToUseAttribute(Part, 2, true);
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 2));
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			AssertEquals(true, Manager.IsPartAttributeUsedByProduct(Part, 2));
		}

		#endregion

		#region TestSetProductToUseAttribute3

		public void TestSetProductToUseAttribute3()
		{
			Manager.SetProductToUseAttribute(Part, 3, true);
			AssertEquals(false, Manager.IsPartAttributeUsedByProduct(Part, 3));
			OrgMisc.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			AssertEquals(true, Manager.IsPartAttributeUsedByProduct(Part, 3));
		}

		#endregion

		#region TestSetProductToUseExpiryDate

		public void TestSetProductToUseExpiryDate()
		{
			Manager.SetProductToUseAttribute(Part, 4, true);
			AssertEquals(false, Manager.IsExpiryDateUsedByProduct(Part));
			OrgMisc.OM_IMUseExpiryDate = true;
			AssertEquals(true, Manager.IsExpiryDateUsedByProduct(Part));
		}

		#endregion

		#region TestSetProductToUsePackingDate

		public void TestSetProductToUsePackingDate()
		{
			Manager.SetProductToUseAttribute(Part, 5, true);
			AssertEquals(false, Manager.IsPackingDateUsedByProduct(Part));
			OrgMisc.OM_IMUsePackingDate = true;
			AssertEquals(true, Manager.IsPackingDateUsedByProduct(Part));
		}

		#endregion

		#region TestPartAttributeType

		public void TestPartAttributeType()
		{
			AssertEquals(ZString.Empty, Manager.PartAttributeType(1));
			OrgMisc.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
			AssertEquals(PartAttributeTypeList.Codes.VIN, Manager.PartAttributeType(1));

			AssertEquals(ZString.Empty, Manager.PartAttributeType(2));
			OrgMisc.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals(PartAttributeTypeList.Codes.BatchNumber, Manager.PartAttributeType(2));

			AssertEquals(ZString.Empty, Manager.PartAttributeType(3));
			OrgMisc.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals(PartAttributeTypeList.Codes.JulianBatchNumber, Manager.PartAttributeType(3));
		}

		#endregion

		#region Implementation

		void AssertIsPartAttribMandatory(ZPropertyInfo info, int attributeNumber)
		{
			info.Value = (ZString)PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals(true, Manager.IsPartAttributeMandatory(attributeNumber));

			info.Value = (ZString)PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals(true, Manager.IsPartAttributeMandatory(attributeNumber));

			info.Value = (ZString)PartAttributeTypeList.Codes.VIN;
			AssertEquals(true, Manager.IsPartAttributeMandatory(attributeNumber));

			info.Value = (ZString)PartAttributeTypeList.Codes.Mandatory;
			AssertEquals(true, Manager.IsPartAttributeMandatory(attributeNumber));

			info.Value = (ZString)PartAttributeTypeList.Codes.NonMandatory;
			AssertEquals(false, Manager.IsPartAttributeMandatory(attributeNumber));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			OrgMisc = Factory.New<OrgMiscServ>();
			OrgMisc.OM_OH = Org.PK;
			Manager = Org.PartAttributeManager;
			Part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation = Part.RelatedOrganisations.AddNew();
			relation.OU_OH = Org.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
		}

		OrgHeader Org;
		OrgMiscServ OrgMisc;
		OrgSupplierPart Part;
		PartAttributeManager Manager;

		#endregion
	}
}
