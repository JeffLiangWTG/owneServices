using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V139Transform : ITransformStrategy
	{
		static V139Transform transform;

		V139Transform() { }

		public static V139Transform Instance()
		{
			return transform ?? (transform = new V139Transform());
		}

		public Type[] ToBeTransformedTypes
			=> new[] { typeof(RefUNLOCO) };

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 139 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefUNLOCO unloco)
			{
				if (!unloco.Deleted && unloco.RefUNLOCORelatedPorts?.Length > 1)
				{
					unloco.RefUNLOCORelatedPorts = new RefUNLOCORelatedPort[] { unloco.RefUNLOCORelatedPorts.First(x => x.RLR_GroupNumber == unloco.RefUNLOCORelatedPorts.Max(g => g.RLR_GroupNumber)) };
				}
			}

			return data;
		}
	}
}
