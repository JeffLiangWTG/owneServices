using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class RadioCallSignCodeFindBoxTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			using (var form1 = new JobDeclarationForm(declaration))
			{
				form1.Show();
				form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.DeclarationTabPage;
				var userControl = form1.CustomsBrokerageUserControl.DeclarationUserControl as ZADeclarationUserControl;
				userControl.UZ_RadioCallSignTextBox.SelectFromPopupForm();
				var form = ZFormModaliser.LastFormShownForTest;
				var grid = ((ZArchitecture.GUI.Internal.EmbeddedModulePopup)form).Module_ForTest.DisplayGrid;
				var columns = grid.Columns;
				CombineAssertions(() =>
				{
					AssertEquals("Only four columns available", 4, columns.Count(x => !x.IsUnavailable));
					Assert("ZZO_Code", !columns.First(x => x.ColumnName == RefVesselZZSchema.Constants.ZZO_Code).IsUnavailable);
					Assert("ZZO_RadioCallSign", !columns.First(x => x.ColumnName == RefVesselZZSchema.Constants.ZZO_RadioCallSign).IsUnavailable);
					Assert("CarrierCodes", !columns.First(x => x.ColumnName == "CarrierCodes").IsUnavailable);
					Assert("CarrierNames", !columns.First(x => x.ColumnName == "CarrierNames").IsUnavailable);
				});
			}
		}

		public void TestFilters()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			using (var form1 = new JobDeclarationForm(declaration))
			{
				form1.Show();
				form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.DeclarationTabPage;
				var userControl = form1.CustomsBrokerageUserControl.DeclarationUserControl as ZADeclarationUserControl;
				userControl.UZ_RadioCallSignTextBox.SelectFromPopupForm();
				var form = ZFormModaliser.LastFormShownForTest;
				var module = ((ZArchitecture.GUI.Internal.EmbeddedModulePopup)form).Module_ForTest;
				var filters = module.FilterBusinessObject.ModuleFilters;
				CombineAssertions(() =>
				{
					AssertEquals("Only 4 filters available", 5, filters.Count());
					Assert("Vessel Name", filters.Any(x => x.Code == "Vessel Name"));
					Assert("Radio Call Sign", filters.Any(x => x.Code == "Radio Call Sign"));
					Assert("Carrier Code(s)", filters.Any(x => x.Code == "Carrier Code(s)"));
					Assert("Carrier Names(s)", filters.Any(x => x.Code == "Carrier Name(s)"));
				});
			}
		}

		public void TestFiltersUsingVesselName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			var systemVessel = Factory.NewWithValidTestData<RefVesselZZ>();
			systemVessel.ZZO_RadioCallSign = "AAAA";
			var systemVessel2nd = Factory.NewWithValidTestData<RefVesselZZ>();
			systemVessel2nd.ZZO_Code = systemVessel.ZZO_Code;
			systemVessel2nd.ZZO_RadioCallSign = "DDDD";
			var overrideVessel = Factory.NewWithValidTestData<RefVessel>();
			overrideVessel.RV_Code = systemVessel.ZZO_Code;
			overrideVessel.RV_RadioCallSign = "BBBB";
			Factory.Save();
			declaration.JE_VesselName = overrideVessel.RV_Code;
			Factory.Save();
			using (var form1 = new JobDeclarationForm(declaration))
			{
				form1.Show();
				form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.DeclarationTabPage;
				var userControl = form1.CustomsBrokerageUserControl.DeclarationUserControl as ZADeclarationUserControl;
				userControl.UZ_RadioCallSignTextBox.SelectFromPopupForm();
				var form = ZFormModaliser.LastFormShownForTest;
				var module = ((ZArchitecture.GUI.Internal.EmbeddedModulePopup)form).Module_ForTest;
				var filters = module.FilterBusinessObject.ModuleFilters;
				CombineAssertions(() =>
				{
					AssertEquals("Only 4 filters available", 5, filters.Count());
					Assert("Vessel Name", filters.Any(x => x.Code == "Vessel Name"));
					Assert("Radio Call Sign", filters.Any(x => x.Code == "Radio Call Sign"));
					Assert("Carrier Code(s)", filters.Any(x => x.Code == "Carrier Code(s)"));
					Assert("Carrier Name(s)", filters.Any(x => x.Code == "Carrier Name(s)"));
				});
			}
		}
	}
}
