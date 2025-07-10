using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentSystemFormMenuItemsTest : TestCaseWithFactory
	{
		public void TestConstantsCoverage()
		{
			var definedForms = typeof(ShipmentSystemFormMenuItems)
				.GetProperties()
				.Select(f => f.GetValue(null))
				.ToArray();

			var query = new ZQuery(StmMenuItemSchema.PK, definedForms);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Enterprise.Core.Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);

			var formsInDatabase = Factory
				.Load<StmMenuItem>(query)
				.Select(mi => mi.PK);

			AssertContainsExactElementsInAnyOrder("constants correspond with system db data", definedForms, formsInDatabase);
		}

		public void TestBillOfLadingMenuItemsPivots()
		{
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.TANHBL);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.FIATAHBL);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.ITClubNewZealand);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.DataHawkBill);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTerms);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.CargowiseBill);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStates);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.ITClubAustralia);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZ);
			CheckBillOfLadingMenuItemsPivots(Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTermsNoLaw);
		}

		public void CheckBillOfLadingMenuItemsPivots(string houseBillOfLadingType)
		{
			var query = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, ShipmentSystemFormMenuItems.BillOfLadingPK);
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_MenuTemplateFilter, $"JS_HouseBillOfLadingType == \"{houseBillOfLadingType}\"");
			var pivotsTemplates = Factory.Load<StmMenuTemplatePivot>(query).ToArray();

			AssertEquals($"{houseBillOfLadingType} Template Filter - Original", 1,
				pivotsTemplates.Count(pivot => pivot.DocumentTitle == "Original")); // Document Title of Menu Item Pivot
			AssertEquals($"{houseBillOfLadingType} Template Filter - Copy PRN", 1,
				pivotsTemplates.Count(pivot => pivot.DocumentTitle == "Copy" // Document Title of Menu Item Pivot
												 && pivot.SI_PrintCopyType == nameof(PrintType.PRN)));
			AssertEquals($"{houseBillOfLadingType} Template Filter - Copy EML", 1,
				pivotsTemplates.Count(pivot => pivot.DocumentTitle == "Copy" // Document Title of Menu Item Pivot
												 && pivot.SI_PrintCopyType == nameof(PrintType.EML)));
			AssertEquals($"{houseBillOfLadingType} Template Filter - Copy FAX", 1,
				pivotsTemplates.Count(pivot => pivot.DocumentTitle == "Copy" // Document Title of Menu Item Pivot
												 && pivot.SI_PrintCopyType == nameof(PrintType.FAX)));
		}

		public void TestAllShipmentSystemFormMenuItemsExists()
		{
			var menuItems = typeof(ShipmentSystemFormMenuItems)
				.GetProperties()
				.Select(f => f.GetValue(null))
				.ToArray();

			var query = new ZQuery(StmMenuItemSchema.PK, menuItems);

			var menuItemsInDatabase = Factory
				.Load<StmMenuItem>(query)
				.Select(mi => mi.PK).ToArray();

			AssertEquals("Number of menu items are same", menuItems.Length, menuItemsInDatabase.Length);
			AssertContainsExactElementsInAnyOrder("All menu items exists", menuItems, menuItemsInDatabase);
		}
	}
}
