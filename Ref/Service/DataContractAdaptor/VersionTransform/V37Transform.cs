using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V37Transform : ITransformStrategy
	{
		static V37Transform v37Transform;

		V37Transform() { }

		public static V37Transform Instance()
		{
			if (v37Transform == null)
			{
				v37Transform = new V37Transform();
			}
			return v37Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 37 && (dataSetType == typeof(RefSysConfigType));
		}

		public T Transform<T>(T data)
		{
			if (data is RefSysConfigType configType)
			{
				var configs = configType.RefSysConfigs;
				if (configs != null)
				{
					var transformedConfigs = configs.Where(x => x.ZRC_BinaryValue == null).ToArray();
					configType.RefSysConfigs = transformedConfigs;
				}
			}
			return data;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefSysConfigType),
					typeof(RefSysConfig)
				};
			}
		}
	}
}
