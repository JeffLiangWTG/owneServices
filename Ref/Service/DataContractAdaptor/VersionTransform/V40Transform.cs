using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V40Transform : ITransformStrategy
	{
		static V40Transform v40Transform;

		V40Transform() { }

		public static V40Transform Instance()
		{
			if (v40Transform == null)
			{
				v40Transform = new V40Transform();
			}
			return v40Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 40 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefExchangeRateZZ exchangeRateZZ)
			{
				if (exchangeRateZZ.ZZN_ExRateType == "IAT")
				{
					exchangeRateZZ.Deleted = true;
				}
			}
			if (data is RefCusTariff tariff)
			{
				RemoveApplicabilityWithSecondTradeGroup(tariff.RefCusRates);
				RemoveApplicabilityWithSecondTradeGroup(tariff.RefCusConditions);
				RemoveApplicabilityWithSecondTradeGroup(tariff.RefCusTariffAdditionalCodes);
			}
			if (data is RefCusPreference preference)
			{
				RemoveApplicabilityWithSecondTradeGroup(preference.RefCusRates);
				RemoveApplicabilityWithSecondTradeGroup(preference.RefCusConditions);
			}
			if (data is RefCusNomenclatureGroup nomenclatureGroup)
			{
				RemoveApplicabilityWithSecondTradeGroup(nomenclatureGroup.RefCusConditions);
			}

			return data;
		}

		static void RemoveApplicabilityWithSecondTradeGroup<T>(T[] array)
		{
			if (array != null && array.Length > 0)
			{
				if (array is RefCusRate[] rates)
				{
					Array.ForEach(rates, x =>
					{
						if (x.RefCusApplicabilities != null && x.RefCusApplicabilities.Length > 0)
						{
							x.RefCusApplicabilities = x.RefCusApplicabilities.Where(a => a.RefCusTradeGroup1 == null).ToArray();
						}
					});
				}
				else if (array is RefCusCondition[] conditions)
				{
					Array.ForEach(conditions, x =>
					{
						if (x.RefCusApplicabilities != null && x.RefCusApplicabilities.Length > 0)
						{
							x.RefCusApplicabilities = x.RefCusApplicabilities.Where(a => a.RefCusTradeGroup1 == null).ToArray();
						}
					});
				}
				else if (array is RefCusTariffAdditionalCode[] additionalCodes)
				{
					Array.ForEach(additionalCodes, x =>
					{
						if (x.RefCusApplicabilities != null && x.RefCusApplicabilities.Length > 0)
						{
							x.RefCusApplicabilities = x.RefCusApplicabilities.Where(a => a.RefCusTradeGroup1 == null).ToArray();
						}
					});
				}
			}
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[]
				{
					typeof(RefExchangeRateZZ),
					typeof(RefCusTariff),
					typeof(RefCusPreference),
					typeof(RefCusNomenclatureGroup)
				};
			}
		}
	}
}
