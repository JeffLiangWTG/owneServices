using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V32Transform : ITransformStrategy
	{
		static V32Transform v32Transform;

		V32Transform() { }

		public static V32Transform Instance()
		{
			if (v32Transform == null)
			{
				v32Transform = new V32Transform();
			}
			return v32Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 32 && (dataSetType == typeof(UNDGSubstanceADR));
		}

		public T Transform<T>(T data)
		{
			var result = data as UNDGSubstanceADR;
			if (result != null)
			{
				if (result.ADR_PSN?.Length > 200)
				{
					result.ADR_PSN = result.ADR_PSN.Substring(0, 200);
				}
				if (result.ADR_SpecialProvisions?.Length > 30)
				{
					result.ADR_SpecialProvisions = result.ADR_SpecialProvisions.Substring(0, 30);
				}
				if (result.ADR_BulkTankIns?.Length > 50)
				{
					result.ADR_BulkTankIns = result.ADR_BulkTankIns.Substring(0, 50);
				}
				if (result.ADR_TransportCategory?.Length > 12)
				{
					result.ADR_TransportCategory = result.ADR_TransportCategory.Substring(0, 12);
				}
				if (result.ADR_BulkSpecialProv?.Length > 14)
				{
					result.ADR_BulkSpecialProv = result.ADR_BulkSpecialProv.Substring(0, 14);
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
