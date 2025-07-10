using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V107Transform : ITransformStrategy
	{
		static V107Transform transform;

		V107Transform() { }

		public static V107Transform Instance()
		{
			return transform ?? (transform = new V107Transform());
		}

		public Type[] ToBeTransformedTypes
			=> new Type[]
			{
				typeof(UNDGSubstanceCFR),
			};

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 107 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			var newValuesInConstrain = new string[] { "NLT", "GLM" };

			if (data is UNDGSubstanceCFR dataUNDGSubstanceCFR)
			{
				if (newValuesInConstrain.Contains(dataUNDGSubstanceCFR.CFR_PAXAirRailLimitType)
					|| newValuesInConstrain.Contains(dataUNDGSubstanceCFR.CFR_CargoAirRailLimitType))
				{
					dataUNDGSubstanceCFR.Deleted = true;
				}
			}

			return data;
		}
	}
}
