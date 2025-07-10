using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocument))]
	public class NctsSupportingDocumentTest : CusSupportingInfoTest<NctsSupportingDocument>
	{
		public void TestLookups_Phase4()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsSupportingDocumentLookups>(supportingDocument.Lookups);
		}

		public void TestLookups_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsSupportingDocumentLookups>(supportingDocument.Lookups);
		}

		protected override IEnumerable<NctsSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var supportingDoc = goodsItem.SupportingDocuments.AddNew();
			yield return supportingDoc;
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			supportingDocument = goodsItem.SupportingDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsSupportingDocument supportingDocument;
	}
}
