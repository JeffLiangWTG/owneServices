using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V118Transform : ITransformStrategy
	{
		static V118Transform transform;

		V118Transform() { }

		public static V118Transform Instance()
		{
			return transform ?? (transform = new V118Transform());
		}

		public Type[] ToBeTransformedTypes
			=> new Type[]
			{
				typeof(RefExchangeRateZZ),
			};

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 118 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			var newValuesInConstrain = new string[] { "BNB", "BNS" };

			if (data is RefExchangeRateZZ dataRefExchangeRateZZ)
			{
				if (newValuesInConstrain.Contains(dataRefExchangeRateZZ.ZZN_ExRateType))
				{
					dataRefExchangeRateZZ.Deleted = true;
				}
			}

			return data;
		}
	}
}
