using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	public class OrgSupplierPartTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Singapore, pivot.CI_RN_NKCountry);
			}
		}

		public void TestModelMaxLength()
		{
			AssertEquals(OrgSupplierPartSchema.OP_Model.MaxLength, OrgSupplierPart.OP_ModelInfo.MaxLength);
		}

		public void TestBrandNameMaxLength()
		{
			AssertEquals(OrgSupplierPartSchema.OP_Brand.MaxLength, OrgSupplierPart.OP_BrandInfo.MaxLength);
		}

		public void TestImportLookupClassification()
		{
			var importClass = Factory.New<BaseCusClassification>();
			importClass.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			importClass.CC_LookupCode = "LookupCode1";
			importClass.CC_Description = "Lookup 1 DESCRIPTION";
			importClass.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var importPivot = Factory.New<CusClassPartPivot>();
			importPivot.CI_CC = importClass.PK;
			importPivot.CI_OP = OrgSupplierPart.PK;
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			OrgSupplierPart.OP_PartNum = "Import Part";
			AssertEquals("ImportClassificationLookup", "LookupCode1", OrgSupplierPart.ImportClassificationLookup);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			OrgSupplierPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var anotherImportClass = Factory.New<BaseCusClassification>();
			anotherImportClass.CC_LookupCode = "LookupCode2";
			anotherImportClass.CC_Description = "Lookup 2 DESCRIPTION";
			anotherImportClass.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			anotherImportClass.CC_TariffNum = "0208.90.00 28";
			Factory.Save();
			var anotherImportPivot = Factory.New<CusClassPartPivot>();
			anotherImportPivot.CI_CC = anotherImportClass.PK;
			anotherImportPivot.CI_OP = OrgSupplierPart.PK;
			anotherImportPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("ImportClassificationLookup - multiple values", "MULTI", OrgSupplierPart.ImportClassificationLookup);
		}

		public void TestExportLookupClassification()
		{
			AssertEquals("ExportClassificationLookup", "", OrgSupplierPart.ExportClassificationLookup);
			var exportClass = Factory.New<BaseCusClassification>();
			exportClass.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			exportClass.CC_LookupCode = "LC_Exp1";
			exportClass.CC_Description = "LC_Exp1 DESCRIPTION";
			exportClass.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var importExportPivot = Factory.New<CusClassPartPivot>();
			importExportPivot.CI_CC = exportClass.PK;
			importExportPivot.CI_OP = OrgSupplierPart.PK;
			importExportPivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			OrgSupplierPart.OP_PartNum = "Import & Export Part";
			AssertEquals("ExportClassificationLookup", "LC_Exp1", OrgSupplierPart.ExportClassificationLookup);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			OrgSupplierPart.RelatedOrganisations.AddOrganisationIfNotExist(supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var anotherExportClass = Factory.New<BaseCusClassification>();
			anotherExportClass.CC_LookupCode = "LookupCode2";
			anotherExportClass.CC_Description = "Lookup 2 DESCRIPTION";
			anotherExportClass.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			anotherExportClass.CC_TariffNum = "0208.90.00 28";
			Factory.Save();
			var exportPivot = Factory.New<CusClassPartPivot>();
			exportPivot.CI_CC = anotherExportClass.PK;
			exportPivot.CI_OP = OrgSupplierPart.PK;
			exportPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("ExportClassificationLookup - multiple values", "MULTI", OrgSupplierPart.ExportClassificationLookup);
		}

		#region OrgSupplierPart
		OrgSupplierPart OrgSupplierPart
		{
			get
			{
				if (orgSupplierPart == null)
				{
					orgSupplierPart = Factory.New<OrgSupplierPart>();
					orgSupplierPart.OP_PartNum = orgSupplierPart.PK.ToString().Replace("-", "");
				}

				return orgSupplierPart;
			}
		}

		OrgSupplierPart orgSupplierPart;
		#endregion
	}
}
