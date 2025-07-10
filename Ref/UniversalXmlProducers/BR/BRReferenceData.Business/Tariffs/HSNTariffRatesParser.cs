using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class HSNTariffRatesParser(string dataSource) : BaseParser(dataSource)
	{
		public void ExportToXMLFile(IEnumerable<TariffDTO> tariffs, string outputFileName, DateTime publicationTime)
		{
			var refRateList = GetData(tariffs);
			var writerConfiguration = Helper.GetHSNRefCusRateWriterConfiguration();
			if (writerConfiguration != null)
			{
				Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refRateList);
			}
		}

		public static IEnumerable<RefCusTariff> GetData(IEnumerable<TariffDTO> tariffs)
		{
			Contract.Assume(tariffs != null);
			var maxEndDate = new DateTime(2079, 06, 06, 23, 59, 00);

			var result = new List<RefCusTariff>();
			foreach (var tariff in tariffs)
			{
				var tariffCode = tariff.Code;
				if (tariffCode.Length == 8)
				{
					var rates = new List<RefCusRate>();
					foreach (var rate in tariff.TariffRates)
					{
						if(DateTime.TryParseExact(rate.StartDate, "ddMMyyyy", CultureInfo.InvariantCulture,  DateTimeStyles.None, out var startDate)
							&& DateTime.TryParseExact(rate.EndDate, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
						{
							endDate = endDate > maxEndDate ? maxEndDate : endDate;
							rates.Add(RatesUtils.FeedRefCusTariffRate(rate.Percentual, rate.Code, Constants.PreferenceCodes.NORMAL, applicabilityStartDate: startDate, applicabilityEndDate: endDate));
						}
					}

					rates = rates.Where(x => x != null).ToList();

					result.Add(new RefCusTariff { ZZ1_TariffCode = tariffCode, RefCusRates = [.. rates] });
				}
			}

			return result;
		}
	}
}
