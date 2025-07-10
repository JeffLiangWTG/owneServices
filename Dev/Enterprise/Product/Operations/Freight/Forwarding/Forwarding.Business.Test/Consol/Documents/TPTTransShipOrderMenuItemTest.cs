using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business
{
	sealed class TPTTransShipOrderMenuItemTest : TestCase
	{
		public void TestValues()
		{
			var factory = new BusinessObjectFactory();
			var menuItem = new TPTTransShipOrderMenuItem(factory, "ZADUR", "Transhipment Order (ZADUR)");

			AssertEquals("LoadPort", "ZADUR", menuItem.PortCode);
			AssertEquals("SU_MenuName", "Transhipment Order (ZADUR)", menuItem.SU_MenuName);
			AssertEquals("SU_MenuNameMultilingual", "Transhipment Order (ZADUR)", menuItem.SU_MenuNameMultilingual);
			AssertEquals("SU_MenuType", Core.Constants.StmMenuItemTypes.Forms, menuItem.SU_MenuType);
			AssertEquals("SU_IsSystemDefined", ZBool.True, menuItem.SU_IsSystemDefined);
		}
	}
}
