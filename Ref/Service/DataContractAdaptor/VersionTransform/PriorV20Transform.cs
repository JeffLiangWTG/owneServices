using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	class PriorV20Transform : ITransformStrategy
	{
		static PriorV20Transform priorV20Transform;

		PriorV20Transform() { }

		public static PriorV20Transform Instance()
		{
			if (priorV20Transform == null)
			{
				priorV20Transform = new PriorV20Transform();
			}
			return priorV20Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 20 && (dataSetType == typeof(RefCountry));
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCountry)
				{
					var item = result as RefCountry;
					if (item != null && item.RN_Code == "GB")
					{
						item.RN_EconomicGrouping = "EUN";
					}
				}
			}
			return result;
		}

		public Type[] ToBeTransformedTypes { get; } = {
				typeof(RefCountry)
			};
	}
}
