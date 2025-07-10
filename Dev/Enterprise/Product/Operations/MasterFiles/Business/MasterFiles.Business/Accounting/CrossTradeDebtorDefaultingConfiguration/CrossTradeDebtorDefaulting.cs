using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	internal static class CrossTradeDebtorDefaulting
	{
		internal static ChargedPartyForCrossTradeJob GetDefaultPartyToChargeForCrossTradeJob(ZString jobType, ZString transportMode, bool isCollect)
		{
			var allowedJobTypesToBeDefaulted = new ZString[]
												{
													JobInvoicingConsumerTypes.ShipmentCode,
													JobInvoicingConsumerTypes.QuotedBookingCode,
												};
			if (!allowedJobTypesToBeDefaulted.Contains(jobType))
			{
				return ChargedPartyForCrossTradeJob.Unknown;
			}

			var configProvider = ObjectFactory.Get<ICrossTradeDebtorDefaultingConfigurationProvider>();
			var configs = configProvider.GetConfiguration();

			//Matching Job Types
			var selectedConfigItems = GetMatchedConfigItems(configs, c => c.JobTypeCode == jobType, c => c.JobTypeCode == "ALL");

			//Matching Transport Mode
			if (selectedConfigItems.Any())
			{
				selectedConfigItems = GetMatchedConfigItems(selectedConfigItems, c => c.TransportModeCode == transportMode, c => c.TransportModeCode == "ALL");
			}

			//Matching Prepaid/Collect
			if (selectedConfigItems.Any())
			{
				selectedConfigItems = GetMatchedConfigItems(selectedConfigItems, c => (isCollect && c.IsCollect) || (!isCollect && c.IsPrepaid));
			}

			if (selectedConfigItems.Any())
			{
				var configItem = selectedConfigItems.First();
				return configItem.BillToParty;
			}

			return ChargedPartyForCrossTradeJob.Unknown;

			List<ICrossTradeDebtorDefaultingConfigurationItem> GetMatchedConfigItems(List<ICrossTradeDebtorDefaultingConfigurationItem> configItems, params Func<ICrossTradeDebtorDefaultingConfigurationItem, bool>[] selectors)
			{
				foreach (var selector in selectors)
				{
					var matchedConfigItems = configItems.Where(c => selector(c));
					if (matchedConfigItems.Any())
					{
						return matchedConfigItems.ToList();
					}
				}

				return new List<ICrossTradeDebtorDefaultingConfigurationItem>();
			}
		}
	}

	public class CrossTradeDebtorDefaultingParam
	{
		public JobInvoicingConsumerType JobType { get; set; }
		public ZString TransportMode { get; set; }
	}
}
