using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V46Transform : ITransformStrategy
	{
		static V46Transform v46Transform;

		V46Transform() { }

		public static V46Transform Instance()
		{
			if (v46Transform == null)
			{
				v46Transform = new V46Transform();
			}
			return v46Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 46 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusTariff tariff)
			{
				tariff.Deleted = tariff.Deleted || (!string.IsNullOrEmpty(tariff.ZZ1_ZZF_NKTaxOrFeeCode) && tariff.ZZ1_ZZF_NKTaxOrFeeCode.Length > 3);
				if (!tariff.Deleted)
				{
					tariff.RefCusTariffNationalCodes = tariff.RefCusTariffNationalCodes?.Where(x => x.ZZW_ZZF_NKTaxOrFeeCode.Length <= 3).ToArray();
					tariff.RefCusVATApplicabilities = tariff.RefCusVATApplicabilities?.Where(x => x.ZX5_ZZF_NKTaxOrFeeCode.Length <= 3).ToArray();
				}
			}
			return data;
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
