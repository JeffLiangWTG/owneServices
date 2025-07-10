using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public class RefCusTariffGrid : ZDisplayGrid
	{
		protected override void ExportIntoAndOpenExcel()
		{
			if (AllDataCanBeExport())
			{
				base.ExportIntoAndOpenExcel();
			}
		}

		protected override void ExportVisibleIntoAndOpenExcel()
		{
			if (AllDataCanBeExport())
			{
				base.ExportVisibleIntoAndOpenExcel();
			}
		}

		bool AllDataCanBeExport()
		{
			bool hasChinaTariffToExport = false;

			if (List != null && List.Count > 0)
			{
				var tariffs = SelectedRowCount > 0 ? GetSelectedRows().Cast<TariffView>() : (List as IBusinessObjectCollection).Cast<TariffView>();
				hasChinaTariffToExport = tariffs.Any(x => x.ZZ1_ZZZ_NKDataGrouping == Core.Constants.CountryCodes.China);
			}
			else
			{
				hasChinaTariffToExport = Factory.ExistsInDatabase(TariffViewSchema.Constants.TableName, ExportQuery.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.China));
			}

			if (hasChinaTariffToExport)
			{
				Globals.Message.ShowError(Res.GetString("039198fc-eb04-42f2-994c-d36015696354", "China Tariff data is not available for data export due to WTG contractual obligations.\r\nIn order to proceed with data export, you must make sure that no China Tariff data is selected."), Res.GetString("af5dfaa8-fbba-4d3f-ac2e-b4df9ef30308", "Errors..."));
			}

			return !hasChinaTariffToExport;
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory() { NameForDebugging = "FactoryForAllDataCanBeExport" });
		BusinessObjectFactory factory;
	}
}
