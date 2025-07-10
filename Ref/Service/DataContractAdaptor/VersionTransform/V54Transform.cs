using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V54Transform : ITransformStrategy
	{
		static V54Transform transForm;

		V54Transform() { }

		public static V54Transform Instance()
		{
			if (transForm == null)
			{
				transForm = new V54Transform();
			}
			return transForm;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 54 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusTariff tariff)
			{
				if (!tariff.Deleted && tariff.RefCusTariffNationalCodes != null)
				{
					if (tariff.RefCusTariffNationalCodes.Any(x => x.ZZW_NationalCode.Length > 3))
					{
						tariff.RefCusTariffNationalCodes = tariff.RefCusTariffNationalCodes.Where(x => x.ZZW_NationalCode.Length <= 3).ToArray();
					}
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
