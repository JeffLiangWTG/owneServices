using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V141Transform : ITransformStrategy
	{
		static V141Transform transform;
		V141Transform() { }
		public static V141Transform Instance()
		{
			return transform ?? (transform = new V141Transform());
		}

		public Type[] ToBeTransformedTypes => new[] { typeof(RefExchangeRateZZ) };

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 141 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefExchangeRateZZ refExchangeRateZZ)
			{
				if (!refExchangeRateZZ.Deleted)
				{
					if (refExchangeRateZZ.ZZN_ExRateType == "BUY" || refExchangeRateZZ.ZZN_ExRateType == "SEL")
					{
						refExchangeRateZZ.Deleted = true;
					}
					else if (refExchangeRateZZ.ZZN_AsPublished?.Length > 10)
					{
						refExchangeRateZZ.ZZN_AsPublished = refExchangeRateZZ.ZZN_AsPublished.Substring(0, 10);
					}
				}
			}

			return data;
		}
	}
}
