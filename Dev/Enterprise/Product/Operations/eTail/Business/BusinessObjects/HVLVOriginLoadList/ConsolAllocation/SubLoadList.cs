using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail
{
	public class SubLoadList
	{
		public SubLoadList(HVLVOriginLoadList actualLoadList, IEnumerable<HVLVItem> allocatedItems, ZGuid billToPartyPK, ZString serviceLevel)
		{
			ActualLoadList = actualLoadList;
			loadListFactory = ActualLoadList.Factory;
			ActiveItems = allocatedItems;
			BillToPartyPK = billToPartyPK;
			ServiceLevel = serviceLevel;
			CalculateGoodsValueFromActiveItems();
		}

		readonly BusinessObjectFactory loadListFactory;

		public HVLVOriginLoadList ActualLoadList { get; }

		public ZGuid BillToPartyPK { get; }
		public ZString ServiceLevel { get; }
		public IEnumerable<HVLVItem> ActiveItems { get; }

		public ZString? DestinationCountry { get; set; }
		public ZString DestinationPort { get; set; }

		public decimal ActiveItemsCalculatedGoodsValue { get; private set; }
		public string ActiveItemsCalculatedGoodsValueCurrencyCode { get; private set; }

		public void CalculateGoodsValueFromActiveItems()
		{
			if (ActiveItems.Any())
			{
				var consignmentsByCurrency = ActiveItems.GroupBy(x => x.HVI_HVC_Consignment).Select(x => loadListFactory.Load<HVLVConsignment>(x.Key)).GroupBy(x => x.HVC_RX_NKGoodsValueCurrency).ToArray();

				if (consignmentsByCurrency.Length == 1)
				{
					ActiveItemsCalculatedGoodsValue = consignmentsByCurrency[0].Sum(x => x.HVC_GoodsValue);
					ActiveItemsCalculatedGoodsValueCurrencyCode = consignmentsByCurrency[0].Key;
				}
				else
				{
					var currency = GetCurrencyFromDestinationCountry();
					ActiveItemsCalculatedGoodsValueCurrencyCode = currency.Code;

					foreach (var consignmentGroup in consignmentsByCurrency)
					{
						var consignmentCurrency = RefCurrency.LoadFromCurrencyCode(loadListFactory, consignmentGroup.Key);

						if (consignmentCurrency != null)
						{
							var consignmentValue = consignmentGroup.Sum(x => x.HVC_GoodsValue);
							ActiveItemsCalculatedGoodsValue += consignmentCurrency.ConvertUsingSellRate(ZDateTime.Now, consignmentValue, currency);
						}
					}
				}
			}
		}

		public RefCurrency GetCurrencyFromDestinationCountry()
		{
			if (DestinationCountry.HasValue)
			{
				var destinationCountry = RefCountry.LoadFromCountryCode(loadListFactory, DestinationCountry.Value);
				return destinationCountry.LocalCurrency;
			}

			return RefCurrency.LoadFromCurrencyCode(loadListFactory, CurrencyCodes.UnitedStates);
		}
	}
}
