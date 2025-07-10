using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPreviousDocument))]
	class NctsPreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<NctsPreviousDocument>
	{
		protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var previousDocument = header.MovementHeader.GoodsItems.AddNew().PreviousDocuments.AddNew();
			yield return previousDocument;
		}

		protected override BusinessObject GetNewBusinessObject() => nctsPreviousDocument;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			nctsPreviousDocument = header.MovementHeader.GoodsItems.AddNew().PreviousDocuments.AddNew();
		}
		NctsPreviousDocument nctsPreviousDocument;
	}
}
