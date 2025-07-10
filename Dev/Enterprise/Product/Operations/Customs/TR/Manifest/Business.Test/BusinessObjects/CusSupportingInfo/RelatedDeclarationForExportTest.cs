using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(RelatedDeclarationForExport))]
	public class RelatedDeclarationForExportTest : CusSupportingInfoTest<RelatedDeclarationForExport>
	{
		public void TestValidation()
		{
			AssertEquals("Validation", typeof(RelatedDeclarationForExportValidation), relatedDeclarationForExport.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals("Lookups", typeof(RelatedDeclarationForExportLookups), relatedDeclarationForExport.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<RelatedDeclarationForExport>();
			var type = header.CSI_Type;
			AssertEquals("BIL", type);
			var subType = header.CSI_SubType;
			AssertEquals(SubTypeList.Codes.No, subType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return bill.RelatedDeclarationForExports.AddNew();
		}

		protected override IEnumerable<RelatedDeclarationForExport> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var relatedDeclarationForExports = bill.RelatedDeclarationForExports.AddNew();
			Factory.Save();
			yield return relatedDeclarationForExports;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
			bill = header.Bills.AddNew();
			relatedDeclarationForExport = bill.RelatedDeclarationForExports.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		RelatedDeclarationForExport relatedDeclarationForExport;
	}
}
