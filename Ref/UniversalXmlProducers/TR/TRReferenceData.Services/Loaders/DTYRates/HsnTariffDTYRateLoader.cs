using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public class HsnTariffDTYRateLoader
	{
		public HsnTariffDTYRateLoader(ILogger logger)
		{
			if (logger != null)
			{
				this.logger = logger;
				return;
			}

			using (var factory = LoggerFactory.Create(builder => { }))
			{
				logger = factory.CreateLogger<HsnTariffDTYRateLoader>();
			}

			this.logger = logger;
		}

		public HsnTariffDTYRateRule GetRule(string tariffCode)
		{
			foreach (var reader in AllReaders)
			{
				if (reader.TryGetRule(tariffCode, out var dtyRateRule))
				{
					return dtyRateRule;
				}
			}

			return null;

			// TODO: Move implementation details as much as possible
			// The goal is to make the HsnTariffDTYRatesProcessor a simple implementation
			// and leave the loading logic to the loader -M12
		}

		public string GetOtherCountriesTradingGroupCode(string tariffCode)
		{
			foreach (var reader in AllReaders)
			{
				if (reader.TryGetOtherCountryGroupCode(tariffCode, out var groupCode))
				{
					return groupCode;
				}
			}

			return null;
		}

		public static string GetTradingGroupCode(HsnTariffDTYRateFootnoteRule rule, bool isExclusion = false)
		{
			return HsnTariffDTYRatesReader.GetTradeGroupCode(rule, isExclusion);
		}

		public IEnumerable<TradeGroup> LoadAdditionalTradeGroups()
		{
			// TODO idea: Instead of direct yield, gather all and consolidate if there is any overlaps. -M12
			// As in, if there are same countries with different start and end dates, extend date of one of them and omit the rest.
			// Counter arg: not sure if it's a good idea. They have their names and desc. Combining them may confuse people and make it hard to troubleshoot.
			foreach (var reader in AllReaders)
			{
				foreach (var tradeGroup in reader.AdditionalTradeGroups)
				{
					yield return tradeGroup;
				}
			}
		}

		public IEnumerable<string> GetUnusedRules(IEnumerable<RefCusTariff> tariffs)
		{
			var inputTariffCodes = tariffs.Select(tariff => tariff.ZZ1_TariffCode).ToHashSet();
			return AllReaders.SelectMany(reader => reader.TariffCodes).Where(tariffCode => !inputTariffCodes.Contains(tariffCode));
		}

		public virtual HsnTariffDTYRatesReader[] GetAllReaders()
		{
			return new HsnTariffDTYRatesReader[]
			{
				new HsnTariffDTYRatesReader_List1(logger),
				// new HsnTariffDTYRatesReader_04To24(logger),
				// new ...
				// ...
			};
		}

		HsnTariffDTYRatesReader[] AllReaders
		{
			get
			{
				if (allReaders != null)
				{
					return allReaders;
				}

				allReaders = GetAllReaders();
				return allReaders;
			}
		}
		HsnTariffDTYRatesReader[] allReaders;

		readonly ILogger logger;
	}
}
