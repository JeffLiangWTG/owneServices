using System;
using System.Linq;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V87Transform : ITransformStrategy
	{
		static V87Transform v87Transform;

		V87Transform() { }

		public static V87Transform Instance()
		{
			if (v87Transform == null)
			{
				v87Transform = new V87Transform();
			}
			return v87Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 87 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusTariff tariff)
			{
				if (!tariff.Deleted)
				{
					if (tariff.RefCusVATApplicabilities != null && tariff.RefCusVATApplicabilities.Length > 0)
					{
						tariff.RefCusVATApplicabilities = GetDistinctRefCusVATApplicabilities(tariff.RefCusVATApplicabilities).ToArray();
					}
					if (tariff.RefCusTariffNationalCodes != null && tariff.RefCusTariffNationalCodes.Length > 0)
					{
						foreach (var nationalCode in tariff.RefCusTariffNationalCodes)
						{
							if (nationalCode.RefCusVATApplicabilities != null && nationalCode.RefCusVATApplicabilities.Length > 0)
							{
								nationalCode.RefCusVATApplicabilities = GetDistinctRefCusVATApplicabilities(nationalCode.RefCusVATApplicabilities).ToArray();
							}
						}
					}
				}
			}
			return data;
		}

		static IEnumerable<RefCusVATApplicability> GetDistinctRefCusVATApplicabilities(RefCusVATApplicability[] refCusVATApplicabilities)
		{
			return refCusVATApplicabilities
				.GroupBy(x => new { x.ZX5_ZZF_NKTaxOrFeeCode, x.ZX5_AdditionalCode, x.ZX5_StartDate, x.RefCusTradeGroup })
				.Select(x => x.First())
				.GroupBy(x => new { x.ZX5_ZZF_NKTaxOrFeeCode, x.ZX5_AdditionalCode, x.ZX5_EndDate, x.RefCusTradeGroup })
				.Select(x => x.First());
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[]
				{
					typeof(RefCusTariff)
				};
			}
		}
	}
}
