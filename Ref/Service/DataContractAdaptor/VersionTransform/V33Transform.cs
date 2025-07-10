using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V33Transform : ITransformStrategy
	{
		static V33Transform v33Transform;

		V33Transform() { }

		public static V33Transform Instance()
		{
			if (v33Transform == null)
			{
				v33Transform = new V33Transform();
			}
			return v33Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 33 && (dataSetType == typeof(UNDGSubstanceADR));
		}

		public T Transform<T>(T data)
		{
			var result = data as UNDGSubstanceADR;
			if (result != null)
			{
				if (result.ADR_ADRTankSpecProv?.Length > 62)
				{
					result.ADR_ADRTankSpecProv = result.ADR_ADRTankSpecProv.Substring(0, 62);
				}
			}
			return data;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(UNDGSubstanceADR)
				};
			}
		}
	}
}
