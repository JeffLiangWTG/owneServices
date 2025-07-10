using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocumentZQueryTest : TestCaseWithFactory
	{
		[StressTest]
		public void TestQuery()
		{
			TestCaseHelper.ClearTable(StmMenuDocumentConfigItemSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuDocumentConfigSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuTemplatePivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuMenuPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuEDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuItemSchema.Constants.TableName);

			var menu1 = Factory.New<StmMenuItem>();
			var menu2 = Factory.New<StmMenuItem>();
			var menu3 = Factory.New<StmMenuItem>();
			var menu4 = Factory.New<StmMenuItem>();
			var menu5 = Factory.New<StmMenuItem>();
			var menu6 = Factory.New<StmMenuItem>();
			var menu7 = Factory.New<StmMenuItem>();

			menu1.SU_BusinessContext = nameof(BusinessContext.AgencyBooking);
			menu2.SU_BusinessContext = nameof(BusinessContext.AgencyBooking);
			menu3.SU_BusinessContext = nameof(BusinessContext.AirCTOExport);
			menu4.SU_BusinessContext = nameof(BusinessContext.AirCTOExport);
			menu5.SU_BusinessContext = nameof(BusinessContext.Order);
			menu6.SU_BusinessContext = nameof(BusinessContext.Order);
			menu7.SU_BusinessContext = nameof(BusinessContext.Order);

			menu1.SU_MenuName = "Menu 1";
			menu2.SU_MenuName = "Menu 2";
			menu3.SU_MenuName = "Menu 3";
			menu4.SU_MenuName = "Menu 4";
			menu5.SU_MenuName = "Menu 5";
			menu6.SU_MenuName = "Menu 6";
			menu7.SU_MenuName = "Menu 7";

			menu1.SU_MenuType = Constants.StmMenuItemTypes.Documents;
			menu2.SU_MenuType = Constants.StmMenuItemTypes.Documents;
			menu3.SU_MenuType = Constants.StmMenuItemTypes.Documents;
			menu4.SU_MenuType = Constants.StmMenuItemTypes.OperationalActions;
			menu5.SU_MenuType = Constants.StmMenuItemTypes.WebReports;
			menu6.SU_MenuType = Constants.StmMenuItemTypes.Documents;
			menu7.SU_MenuType = Constants.StmMenuItemTypes.Forms;

			Factory.Save();

			var menus = Factory.Load<StmMenuItem>(new DocumentZQuery());
			AssertContainsExactElementsInAnyOrder("no filter excluding forms",
				new[]
				{
						menu1,
						menu2,
						menu3,
						menu5,
						menu6
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(true));
			AssertContainsExactElementsInAnyOrder("no filter including forms",
				new[]
				{
						menu1,
						menu2,
						menu3,
						menu5,
						menu6,
						menu7
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(BusinessContext.AgencyBooking));
			AssertContainsExactElementsInAnyOrder("BusinessContext.AgencyBooking excluding forms",
				new[]
				{
						menu1,
						menu2
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(nameof(BusinessContext.AgencyBooking)));
			AssertContainsExactElementsInAnyOrder("BusinessContext.AgencyBooking.ToString() excluding forms",
				new[]
				{
						menu1,
						menu2
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(BusinessContext.AirCTOExport, "Menu 3"));
			AssertContainsExactElementsInAnyOrder("BusinessContext.AirCTOExport Menu 3 excluding forms",
				new[]
				{
						menu3
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(nameof(BusinessContext.AirCTOExport), "Menu 3"));
			AssertContainsExactElementsInAnyOrder("BusinessContext.AirCTOExport.ToString() Menu 3 excluding forms",
				new[]
				{
						menu3
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Menu 1"));
			AssertContainsExactElementsInAnyOrder("StmMenuItemSchema.SU_MenuName Menu 1 excluding forms",
				new[]
				{
						menu1
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(nameof(BusinessContext.Order)));
			AssertContainsExactElementsInAnyOrder("BusinessContext.Order.ToString() excluding forms",
				new[]
				{
						menu5,
						menu6
				},
				menus);

			menus = Factory.Load<StmMenuItem>(new DocumentZQuery(nameof(BusinessContext.Order), true));
			AssertContainsExactElementsInAnyOrder("BusinessContext.Order.ToString() including forms",
				new[]
				{
						menu5,
						menu6,
						menu7
				},
				menus);
		}
	}
}
