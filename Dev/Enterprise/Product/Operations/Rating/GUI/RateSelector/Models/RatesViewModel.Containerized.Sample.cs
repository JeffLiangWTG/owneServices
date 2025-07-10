using System;
using System.Collections.Generic;
using System.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	[CodeAlive("Used in XAML designer")]
	public class ContainerizedRatesViewModelSample : ContainerizedRatesViewModel
	{
		public ContainerizedRatesViewModelSample()
		{
			#region SuppressResourceStringsCheckRegion

			ContainerGroups.Add(
				new ContainerGroupViewModel("LD-7", "CARS", 2, new RateViewModel[]
				{
					new CargoguideRateViewModelSample
					{
						CarrierCode = "WW_QNTAS",
						CarrierName = "Qantas",
						CarrierServiceLevel = "STD",
						ContractNumber = "ABCD123456",
						Commodities = "General",
						Deck = "Upper",
						PaymentTerms = "Prepaid",
						Origin = "AUSYD",
						Destination = "UAIEV",
						StartDate = new DateTime(2020, 01, 01),
						ExpiryDate = new DateTime(2020, 06, 01),
						IssueDate = new DateTime(2019, 12, 01),
						Remarks = "McLaren Looooooooooooooooooooonggggggggggggg Remarks",
						Ratio = "1:6",
						Currency = "USD",
						DisplayPrice = true,
						TotalPriceString = "AUD 1,555.55",
						IsExpanded = false,
						CommodityGroups = new List<string>() { "Personal effects" },
						ContainerPayloadWeight = "600 KG",
						ContainerPayloadVolume = "4.3 M3"
					},
					new CW1RateViewModelSample()
					{
						ServiceProviderCode = "WW_EMIRS",
						ServiceProviderName = "Emirates",
						ServiceLevel = "EXP",
						Consignee = "KKKLLL",
						Consignor = "BBBCCC",
						TransitTime = "SMD",
						CarrierCode = "WW_QNTAS",
						CarrierName = "Qantas",
						CarrierServiceLevel = "STD",
						ContractNumber = "ABCD123456",
						Commodities = "General",
						PaymentTerms = "Prepaid",
						Origin = "AUSYD",
						Via = "SGSIN",
						Destination = "UAIEV",
						StartDate = new DateTime(2020, 01, 01),
						ExpiryDate = new DateTime(2020, 06, 01),
						DisplayPrice = true,
						IsExpanded = false
					}
				})
				{
					GoodsWeight = 666,
					GoodsVolume = 4.65m
				});

			ContainerGroups.Add(new ContainerGroupViewModel("LD-8", "BODIES", 5, new RateViewModel[]
			{
				new CargoguideRateViewModelSample
				{
					CarrierCode = "EMIR",
					CarrierName = "Emirates",
					CarrierServiceLevel = "STD",
					ContractNumber = "ABCD123456",
					Commodities = "General",
					Deck = "Upper",
					PaymentTerms = "Prepaid",
					Origin = "AUSYD",
					Destination = "UAIEV",
					StartDate = new DateTime(2020, 01, 01),
					ExpiryDate = new DateTime(2020, 06, 01),
					IssueDate = new DateTime(2019, 12, 01),
					Remarks = "McLaren Looooooooooooooooooooonggggggggggggg Remarks",
					Ratio = "1:6",
					Currency = "USD",
					DisplayPrice = true,
					TotalPriceString = "AUD 1,666.55",
					IsExpanded = false,
					CommodityGroups = new List<string>() { "Personal effects" },
				},
				new CW1RateViewModelSample()
				{
					ServiceProviderCode = "LFTHNSA",
					ServiceProviderName = "Luftansa",
					ServiceLevel = "EXP",
					Consignee = "KKKLLL",
					Consignor = "BBBCCC",
					TransitTime = "SMD",
					CarrierCode = "WW_QNTAS",
					CarrierName = "Qantas",
					CarrierServiceLevel = "STD",
					ContractNumber = "ABCD123456",
					Commodities = "General",
					PaymentTerms = "Prepaid",
					Origin = "AUSYD",
					Via = "SGSIN",
					Destination = "UAIEV",
					StartDate = new DateTime(2020, 01, 01),
					ExpiryDate = new DateTime(2020, 06, 01),
					DisplayPrice = true,
					IsExpanded = false
				}
			})
			{
				GoodsWeight = 430.50m,
				GoodsVolume = 3.2m
			});

			ContainerGroups.Add(new ContainerGroupViewModel("LD-8", "BODIES", 5, Array.Empty<RateViewModel>()));
			ContainerGroups[1].SelectedRate = ContainerGroups[1].Rates.First();

			#endregion
		}
	}
}
