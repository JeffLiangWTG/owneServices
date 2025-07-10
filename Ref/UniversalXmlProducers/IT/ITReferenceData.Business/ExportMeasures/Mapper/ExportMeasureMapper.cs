using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public class ExportMeasureMapper : IExportMeasureMapper
	{
		public RefCusTariff GetMapping(TariffInput input)
		{
			return new RefCusTariff
			{
				ZZ1_TariffCode = input.TariffCode,
				RefCusTariffAdditionalCodes = input.AdditionalCodes.Select(x => new RefCusTariffAdditionalCode
				{
					ZY2_AdditionalCode = x.Code,
					ZY2_Description = x.Description,
					ZY2_ZY3_NKCategory = Constants.Measures.ExportStatisticalMonitoring,
					RefCusApplicabilities = x.Applicabilities.Select(x => new RefCusApplicability
					{
						ZZT_StartDate = x.StartDate,
						ZZT_ZZA_NKTradeGroup = x.TradeGroup
					}).ToArray()
				}).ToArray()
			};
		}
	}
}
