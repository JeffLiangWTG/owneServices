using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V44Transform : ITransformStrategy
	{
		static V44Transform v44Transform;

		V44Transform() { }

		public static V44Transform Instance()
		{
			if (v44Transform == null)
			{
				v44Transform = new V44Transform();
			}
			return v44Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 44 && (dataSetType == typeof(RefStlScript));
		}

		public T Transform<T>(T data)
		{
			if (data is RefStlScript stlScript)
			{
				if (stlScript.STL_DataGranularity == "DAY")
				{
					stlScript.Deleted = true;
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
					typeof(RefStlScript)
				};
			}
		}
	}
}
