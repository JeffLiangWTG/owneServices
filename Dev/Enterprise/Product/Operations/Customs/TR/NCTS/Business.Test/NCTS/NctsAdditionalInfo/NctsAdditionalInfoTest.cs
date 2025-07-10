using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsAdditionalInfo))]
	class NctsAdditionalInfoTest : EU.NCTS.Business.Testing.NctsAdditionalInfoTest<NctsAdditionalInfo>
	{
		protected override IEnumerable<NctsAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var additionalDocuments = header.MovementHeader.GoodsItems.AddNew().AdditionalInfos.AddNew();
			yield return additionalDocuments;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			return goodsItem.AdditionalInfos.AddNew();
		}
	}
}
