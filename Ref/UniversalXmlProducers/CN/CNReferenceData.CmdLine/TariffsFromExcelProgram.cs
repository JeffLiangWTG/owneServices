using System;
using CargoWise.RefDbRepo.CNReferenceData.Business;

namespace CargoWise.RefDbRepo.CNReferenceData.CmdLine
{
	class TariffsFromExcelProgram
	{
		public static void Run()
		{
			using (GlobalOption.Instance.CreateDisposableLog("CN Tariff From Excel Program"))
			{
				try
				{
					var effectiveDate = GlobalOption.Instance.FirstDayOfThisMonth;

					GlobalOption.Instance.Log.Info($"==========> Run TariffProducer for {GlobalOption.Instance.Setting.ExcelSourceFileFolder}");
					var excelReader = new TariffDataExcelReader(effectiveDate);
					excelReader.ReadFromExcels();
					var refCusTariffs = excelReader.OutputRefCusTariffs;

					new CNRefCusTariffUniversalXMLWriter(effectiveDate).Write(refCusTariffs);
					new CNRefCusCodeListUniversalXMLWriter(effectiveDate, excelReader.AdditionalElementHelper).Write();

					GlobalOption.Instance.Log.Info("==========> finished.");
				}
				catch (Exception ex)
				{
					GlobalOption.Instance.Log.Error("==========> error:", ex);
					throw;
				}
			}
		}
	}
}
