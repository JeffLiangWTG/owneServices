using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondCargoDescDataObjectWriter : DataTransfer.Universal.CusInBondCargoDescDataObjectWriter
	{
		public CusInBondCargoDescDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
			: base(writeManager, helper)
		{
		}

		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondCargoDesc commodityBO, PackingLine commodityData)
		{
			base.PopulateInBondSpecificData(commodityBO, commodityData);
			var amsCommodityBO = (CusInBondCargoDesc)commodityBO;
			commodityData.Weight = amsCommodityBO.BY_GrossWeight;
			commodityData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(commodityBO.BY_GrossWeightUnit, amsCommodityBO.Lookups.WeightUnitList);
			commodityData.PackQty = new ZLong(amsCommodityBO.BY_PieceCount);
			commodityData.PackType = ListHelper.GetWithDescription<PackageType>(commodityBO.BY_ManifestUnitCode, amsCommodityBO.Lookups.ManifestUnitList);
			commodityData.CountryOfOrigin = ListHelper.GetWithName<Country>(commodityBO.BY_RN_NKCountryOfOrigin, amsCommodityBO.Lookups.CountryOfOrigins);
			commodityData.ReferenceNumber = amsCommodityBO.BY_CusC4Number;
			commodityData.DetailedDescription = amsCommodityBO.DetailedDescription;
		}
	}
}
