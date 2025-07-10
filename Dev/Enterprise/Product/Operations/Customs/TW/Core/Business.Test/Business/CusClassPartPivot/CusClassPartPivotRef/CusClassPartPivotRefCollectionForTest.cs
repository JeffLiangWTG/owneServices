using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business.Testing
{
	class CusClassPartPivotRefCollectionForTest : CusClassPartPivotRefCollection<CusClassPartPivotRefForTest>
	{
		public CusClassPartPivotRefCollectionForTest(BusinessObject parent) : base(parent, "ZZZ")
		{
		}
	}
}
