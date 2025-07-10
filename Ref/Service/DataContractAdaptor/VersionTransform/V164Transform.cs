using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V164Transform : ITransformStrategy
	{
		static V164Transform transform;
		V164Transform() { }
		public static V164Transform Instance()
		{
			return transform ?? (transform = new V164Transform());
		}

		public Type[] ToBeTransformedTypes => new[] { typeof(RefCusTariffType) };

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 164 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusTariffType tariffType)
			{
				if (!tariffType.Deleted && tariffType.ZZI_Description?.Length > 50)
				{
					tariffType.ZZI_Description = tariffType.ZZI_Description.Substring(0, 50);
				}
			}

			return data;
		}
	}
}
