using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Customs.ZA
{
	class ShipmentMenuTest : TestCaseWithFactory
	{
		public void TestCustomsMenu()
		{
			var menuQuery = new DocumentZQuery(nameof(BusinessContext.Shipment), true);
			menuQuery.ReLoadExistingRows = true;
			menuQuery.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			menuQuery.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
			menuQuery.AddToFilter(StmMenuItemSchema.SU_MenuPath, "Customs");
			var candidateMenuItems = new DocumentEngine.Business.StmMenuItemBaseCollection(Factory, true);
			candidateMenuItems.Load(menuQuery);
			var actualNames = candidateMenuItems.Select(x => x.SU_MenuName).ToArray();

			foreach (var expectedName in ExpectedMenuItemNames)
			{
				AssertCollectionContains(expectedName, expectedName, actualNames);
			}
		}

		string[] ExpectedMenuItemNames => new string[]
		{
			"Application For Refund (DA63)",
			"Application for Special Release",
			"Container List (VOC)",
			"Customs Declaration Response",
			"Customs Worksheet",
			"DA 306 (Section 38 Release)",
			"DA 68",
			"DA 70 (application to make provisional payment)",
			"DA 73",
			"EFT Request",
			"SAD Document Pack",
			"Voucher of Correction",
		};
	}
}
