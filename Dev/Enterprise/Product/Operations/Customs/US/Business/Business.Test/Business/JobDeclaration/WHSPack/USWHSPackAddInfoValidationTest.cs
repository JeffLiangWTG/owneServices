using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USWHSPackAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_PackageQty()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var org2 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var pack = declaration.WHSPacks.AddNew();
			pack.US_PackageQty = ZShort.Zero;
			AssertHasMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			pack.US_PackageQty = (ZShort)10;
			AssertNoMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			pack.US_PackageQty = (ZShort)(-10);
			AssertHasMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			declaration.JE_OH_Importer = org2.PK;
			pack.AddInfoValidation.ValidateUS_PackageQty();
			AssertNoMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			declaration.JE_OH_Importer = org.PK;
			pack.AddInfoValidation.ValidateUS_PackageQty();
			AssertHasMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			pack.AddInfoValidation.ValidateUS_PackageQty();
			AssertNoMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			pack.AddInfoValidation.ValidateUS_PackageQty();
			AssertHasMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			pack.AddInfoValidation.ValidateUS_PackageQty();
			AssertNoMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-2);
			pack = declaration.WHSPacks.AddNew();
			pack.US_PackageQty = ZShort.Zero;
			AssertHasMessageError(pack.US_PackageQtyInfo, ValidationConstants.WHSPack.PackageQtyIsRequired);
		}
	}
}
