using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution.ExportToExcel
{
	public class ProfitShareRedistributionExcelExporter
	{
		public ProfitShareRedistributionExcelExporter(ForwardingProfitShareRedistribution profitShareRedistribution, IExcelExporterNotifications notifications)
		{
			Notifications = notifications;
			DataToExport = profitShareRedistribution;
		}

		readonly IExcelExporterNotifications Notifications;
		readonly ForwardingProfitShareRedistribution DataToExport;

		public void ExportToExcelAndOpen()
		{
			if
				(
					!(DataToExport?.Consols?.Any() ?? false)
					|| !(DataToExport?.Shipments?.Any() ?? false)
				)
			{
				return;
			}

			var consolsExporter = new ExcelExporter(DataToExport.Consols, DataToExport.Consols.Cast<ProfitShareForwardingConsolWrapper>().First().GetExcelExportColumns(), Notifications);

			var shipmentExporter = new ExcelExporter(DataToExport.Shipments, DataToExport.Shipments.Cast<ProfitShareForwardingShipmentWrapper>().First().GetExcelExportColumns(), Notifications);

			using (var excelInterface = ExcelInterfaceFactory.New())
			{
				excelInterface.NewExcelFile(2);
				consolsExporter.ExportIntoExcel(excelInterface, excelInterface.WorkSheets[0]);
				shipmentExporter.ExportIntoExcel(excelInterface, excelInterface.WorkSheets[1]);

				if (!consolsExporter.IsExportCancelled && !shipmentExporter.IsExportCancelled)
				{
					var fileName = consolsExporter.GetNewRandomFileName();
#if DEBUG
					#region for Testing
					if (Globals.IsTest)
					{
						LatestExportedFileNameForTest = fileName;
						excelInterface.PreviewInXlWithoutDeletingFile(fileName);
					}
					else
					#endregion
#endif
					{
						excelInterface.PreviewInXl(fileName);
					}
				}
			}
		}

		#region for Testing
#if DEBUG
		public ZString LatestExportedFileNameForTest { get; set; }
#endif
		#endregion
	}
}
