using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageWorkSheetDocManagerInfo))]
	class CartageWorkSheetDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CommonWorkSheet>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.AddNew();
			workSheet.CartageLegs.AddNew();
			workSheet.CartageLegs.AddNew();
			workSheet.CartageLegs.AddNew();
			workSheet.CartageLegs.AddNew();
			return workSheet;
		}
	}
}
