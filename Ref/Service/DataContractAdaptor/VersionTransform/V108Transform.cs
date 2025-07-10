using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V108Transform : ITransformStrategy
	{
		static V108Transform transform;

		V108Transform() { }

		public static V108Transform Instance()
		{
			return transform ?? (transform = new V108Transform());
		}

		public Type[] ToBeTransformedTypes
			=> new Type[]
			{
				typeof(RefShippingLine),
			};

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 108 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefShippingLine refShippingLine)
			{
				if (!refShippingLine.Deleted)
				{
					if (refShippingLine.RefShippingLineEBLProviders != null && refShippingLine.RefShippingLineEBLProviders.Length > 0)
					{
						refShippingLine.RefShippingLineEBLProviders = null;
					}
				}
			}

			return data;
		}
	}
}
