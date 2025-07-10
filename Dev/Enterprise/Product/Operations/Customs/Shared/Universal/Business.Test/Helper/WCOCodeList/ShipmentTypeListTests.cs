using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class ShipmentTypeListTests : TestCase
	{
		public void TestImport23Only()
		{
			var list = ShipmentTypeList.Import23Only();
			AssertEquals(ShipmentTypeList.Codes.Import23, list.CodesAsString);
		}

		public void TestExport22Only()
		{
			var list = ShipmentTypeList.Export22Only();
			AssertEquals(ShipmentTypeList.Codes.Export22, list.CodesAsString);
		}

		public void TestExport22AndImport23()
		{
			var list = ShipmentTypeList.Export22AndImport23();
			AssertContainsExactElementsInAnyOrder(new[] { ShipmentTypeList.Codes.Export22, ShipmentTypeList.Codes.Import23 }, list.GetAllCodes());
		}
	}
}
