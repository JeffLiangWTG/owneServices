using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V112Transform : ITransformStrategy
	{
		static V112Transform v112Transform;

		V112Transform() { }

		public static V112Transform Instance()
		{
			if (v112Transform == null)
			{
				v112Transform = new V112Transform();
			}
			return v112Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 112 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is UNDGSubstanceCFR undgCFR)
			{
				if (!undgCFR.Deleted)
				{
					if (undgCFR.CFR_Variation != null && undgCFR.CFR_Variation.Length > 80)
					{
						undgCFR.Deleted = true;
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
					typeof(UNDGSubstanceCFR)
				};
			}
		}
	}
}
