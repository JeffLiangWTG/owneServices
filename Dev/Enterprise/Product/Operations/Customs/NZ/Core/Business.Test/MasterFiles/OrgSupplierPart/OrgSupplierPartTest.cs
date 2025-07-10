using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	using NUnit.Framework;

	[TestedType(typeof(OrgSupplierPart))]
	public class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.NewZealand, pivot.CI_RN_NKCountry);
			}
		}

		protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);

		[TestDate(2008, 6, 6)]
		public void TestDutyRateForCurrentCountry()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			AssertEquals("part.DutyRateForCurrentCountry", "", part.DutyRateForCurrentCountry);

			CusClassification classification = Factory.New<CusClassification>();
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			AssertEquals("part.DutyRateForCurrentCountry", "", part.DutyRateForCurrentCountry);

			classification.CC_TariffNum = "0101.10.00.90E";
			AssertEquals("part.DutyRateForCurrentCountry", "", part.DutyRateForCurrentCountry);

			classification.CC_TariffNum = "4201.00.00.01B";
			AssertEquals("part.DutyRateForCurrentCountry", "", part.DutyRateForCurrentCountry);
		}

		public void TestClassificationCount()
		{
			AssertEquals("Precondition: No classification yet", 0, Product.ClassificationsForBinding.Count);
			var pivot1 = Product.PivotsForBinding.AddNew();
			var classification1PK = Factory.New(typeof(CusClassification)).PK;
			var classification2PK = Factory.New(typeof(CusClassification)).PK;
			pivot1.CI_CC = classification1PK;
			AssertEquals("Classification added", 1, Product.ClassificationsForBinding.Count);

			pivot1.CI_CC = classification2PK;
			AssertEquals("Classification changed", 1, Product.ClassificationsForBinding.Count);

			pivot1.CI_CC = CargoWise.Types.ZGuid.Empty;
			AssertEquals("Classification removed", 0, Product.ClassificationsForBinding.Count);

			pivot1.CI_CC = classification1PK;
			var pivot2 = Product.PivotsForBinding.AddNew();
			pivot2.CI_CC = CargoWise.Types.ZGuid.Empty;
			AssertEquals("Classification removed", 1, Product.ClassificationsForBinding.Count);

			pivot2.CI_CC = classification2PK;
			AssertEquals("Classification removed", 2, Product.ClassificationsForBinding.Count);

			pivot2.CI_CC = classification1PK;
			AssertEquals("Classification removed", 1, Product.ClassificationsForBinding.Count);
		}

		public void TestImportLookupClassification()
		{
			var importClass = Factory.New<CusClassification>();
			importClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			importClass.CC_LookupCode = "LookupCode1";
			importClass.CC_Description = "Lookup 1 DESCRIPTION";
			importClass.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var importPivot = Factory.New<CusClassPartPivot>();
			importPivot.CI_CC = importClass.PK;
			importPivot.CI_OP = Product.PK;
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Product.OP_PartNum = "Import Part";

			AssertEquals("ImportClassificationLookup", "LookupCode1", Product.ImportClassificationLookup);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Product.RelatedOrganisations.AddOrganisationIfNotExist(supplier, OrgPartRelation.RelationshipTypes.Supplier);

			var anotherImportClass = Factory.New<CusClassification>();
			anotherImportClass.CC_LookupCode = "LookupCode2";
			anotherImportClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			anotherImportClass.CC_TariffNum = "0208.90.00 28";
			Factory.Save();

			var pivot2 = Factory.New<CusClassPartPivot>();
			pivot2.CI_CC = anotherImportClass.PK;
			pivot2.CI_OP = Product.PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("ImportClassificationLookup - multiple values", "MULTI", Product.ImportClassificationLookup);
		}

		public void TestExportLookupClassification()
		{
			AssertEquals("ExportClassificationLookup", "", Product.ExportClassificationLookup);

			var importClass = Factory.New<CusClassification>();
			importClass.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			importClass.CC_LookupCode = "LC_Exp1";
			importClass.CC_Description = "LC_Exp1 DESCRIPTION";
			importClass.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var importPivot = Factory.New<CusClassPartPivot>();
			importPivot.CI_CC = importClass.PK;
			importPivot.CI_OP = Product.PK;
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			Product.OP_PartNum = "Import & Export Part";

			AssertEquals("ExportClassificationLookup", "LC_Exp1", Product.ExportClassificationLookup);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Product.RelatedOrganisations.AddOrganisationIfNotExist(supplier, OrgPartRelation.RelationshipTypes.Supplier);

			var anotherImportClass = Factory.New<CusClassification>();
			anotherImportClass.CC_LookupCode = "LC_Exp2";
			anotherImportClass.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			anotherImportClass.CC_TariffNum = "0208.90.00 28";
			Factory.Save();

			var pivot2 = Factory.New<CusClassPartPivot>();
			pivot2.CI_CC = anotherImportClass.PK;
			pivot2.CI_OP = Product.PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("ExportClassificationLookup - multiple values", "MULTI", Product.ExportClassificationLookup);
		}

		public void TestExportAuditAndImportAuditWithHTBChildType()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_LastAuditedUser = "IU";
			pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 9);
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_LastAuditedUser = "BU";
			pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 8);
			AssertEquals("ImportLastAuditUser - multiple value", "MULTI", product.ImportLastAuditUser);
			AssertEquals("ImportLastAuditDate - multiple value", "MULTI", product.ImportLastAuditDate);
			AssertEquals("ExportLastAuditUser - single value", "BU", product.ExportLastAuditUser);
			AssertEquals("ExportLastAuditDate - single value", "08-Apr-18", product.ExportLastAuditDate);

			product = Factory.New<OrgSupplierPart>();
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_LastAuditedUser = "EU";
			pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 9);
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_LastAuditedUser = "BU";
			pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 8);
			AssertEquals("ImportLastAuditUser - single value", "BU", product.ImportLastAuditUser);
			AssertEquals("ImportLastAuditDate - single value", "08-Apr-18", product.ImportLastAuditDate);
			AssertEquals("ExportLastAuditUser - multiple value", "MULTI", product.ExportLastAuditUser);
			AssertEquals("ExportLastAuditDate - multiple value", "MULTI", product.ExportLastAuditDate);

			product = Factory.New<OrgSupplierPart>();
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_LastAuditedUser = "BU1";
			pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 9);
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_LastAuditedUser = "BU2";
			pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 8);
			AssertEquals("ImportLastAuditUser - multiple value", "MULTI", product.ImportLastAuditUser);
			AssertEquals("ImportLastAuditDate - multiple value", "MULTI", product.ImportLastAuditDate);
			AssertEquals("ExportLastAuditUser - multiple value", "MULTI", product.ExportLastAuditUser);
			AssertEquals("ExportLastAuditDate - multiple value", "MULTI", product.ExportLastAuditDate);
		}

		#region Implementation
		protected override string ValidTariffCode
		{
			get { return "1010.10.10.10Y"; }
		}

		OrgSupplierPart Product
		{
			get
			{
				if (fProduct == null)
				{
					fProduct = Factory.New<OrgSupplierPart>();
					fProduct.OP_PartNum = fProduct.PK.ToString().Replace("-", "");
					fProduct.HasChanges = false;
				}
				return fProduct;
			}
		}
		OrgSupplierPart fProduct;
		#endregion
	}
}
