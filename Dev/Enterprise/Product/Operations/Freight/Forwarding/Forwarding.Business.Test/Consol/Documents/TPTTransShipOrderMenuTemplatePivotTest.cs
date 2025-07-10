using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business
{
	sealed class TPTTransShipOrderMenuTemplatePivotTest : TestCase
	{
		public void TestValues()
		{
			var factory = new BusinessObjectFactory();
			var menuItem = new TPTTransShipOrderMenuItem(factory, "ZADUR", "Transhipment Order(ZADUR)");

			var pivot = new TPTTransShipOrderMenuTemplatePivot(factory, menuItem, "Service Instruction - Transhipment Order(ZADUR)");

			AssertEquals("SI_DocumentTitle", "Service Instruction - Transhipment Order(ZADUR)", pivot.SI_DocumentTitle);
			AssertEquals("SI_SU", menuItem.PK, pivot.SI_SU);
			AssertEquals("SI_SO", new ZGuid("56bafcc0-9c3e-4bc7-92cb-89ef43f9f6f6"), pivot.SI_SO);
			AssertEquals("SI_DataStoreName", "Service Instruction - Transhipment Order(ZADUR)", pivot.SI_DataStoreName);
			AssertEquals("SI_IsSystemDefined", ZBool.True, pivot.SI_IsSystemDefined);
			AssertEquals("SI_PrintCopyType", nameof(PrintCopyType.ALL), pivot.SI_PrintCopyType);

			AssertNotNull("Template", pivot.Template);
		}
	}
}
