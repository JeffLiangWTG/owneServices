using System;
using System.Linq;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V89Transform : ITransformStrategy
	{
		static V89Transform v89Transform;

		V89Transform() { }

		public static V89Transform Instance()
		{
			if (v89Transform == null)
			{
				v89Transform = new V89Transform();
			}
			return v89Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 89 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefStlScript script && script.STL_DataGranularity == "SPS")
			{
				script.Deleted = true;
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
