using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	public class RefCusTariffGridTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestChinaTariffCannotBeExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Taiwan);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China);
			Factory.Save();
			var tariffTypeTW = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var tariffTypeCN = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "HSN");
			Factory.Save();
			var tariffTW = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeTW.PK, "1020304050", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariffCN = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypeCN.PK, "1020304050", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var existingFileNames = Directory.GetFiles(EnvProxy.Instance.TempPath);
			using (var module = new RefCusTariffModuleForTesting())
			using (module.ShowPopup())
			{
				AssertEquals("Precondition: Tariff Module should have two export menu items", 2, module.ExportMenuItems_Exposed.Count);
				AssertClickExportMenuItems(module, false);
				module.PerformSearch_ForTest();
				AssertEquals("Precodintion: 2 tarrifs should be shown on the grid", 2, module.DisplayGrid.List.Count);
				AssertClickExportMenuItems(module, false);
				module.DisplayGrid.SelectSingleElementByPK(tariffTW.PK);
				AssertClickExportMenuItems(module, true);
				module.DisplayGrid.SelectSingleElementByPK(tariffCN.PK);
				AssertClickExportMenuItems(module, false);
				var countryOrGroupingFilter = (ModuleNkFilter)module.FilterBusinessObject.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping];
				countryOrGroupingFilter.IsActive = true;
				countryOrGroupingFilter.Property = Core.Constants.CountryCodes.Taiwan;
				AssertClickExportMenuItems(module, true);
				module.PerformSearch_ForTest();
				AssertEquals("Precodintion: 1 tarrif should be shown on the grid", 1, module.DisplayGrid.List.Count);
				AssertClickExportMenuItems(module, true);
			}

			var exportedExcelFiles = Directory.GetFiles(EnvProxy.Instance.TempPath).Except(existingFileNames);
			Assert("excel files should be exported", exportedExcelFiles.Any());
			exportedExcelFiles.ForEach(File.Delete);
		}

		void AssertClickExportMenuItems(RefCusTariffModuleForTesting module, bool canExport)
		{
			foreach (var menu in module.ExportMenuItems_Exposed)
			{
				menu.OnClick.Invoke(this, null);
				if (canExport)
				{
					AssertContains("Can export without CN data", "", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertContains("Cannot export with CN data", "China Tariff data is not available for data export due to WTG contractual obligations", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessages();
			}
		}
	}
}
