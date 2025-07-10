using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Module
{
	class CusInBondBillModuleCollection : BusinessObjectCollection<CusInBondBill>
	{
		public CusInBondBillModuleCollection(BusinessObjectFactory factory)
			: base(factory, GetBillFilter())
		{
		}

		static ZQuery GetBillFilter()
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondBill));
			result.AddToFilter(CusInBondBillSchema.B0_ShipmentType, SQLComparisonOperator.NotEqual, CusInBondBill.OceanBillType);
			var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_TransitDirection, DirectionTypeList.Codes.NVOCC);
			result.AddSubQuery(CusInBondBillSchema.B0_BH, CusInBondHeaderSchema.PK, headerQuery, JoinCondition.And);
			return result;
		}
	}
}
