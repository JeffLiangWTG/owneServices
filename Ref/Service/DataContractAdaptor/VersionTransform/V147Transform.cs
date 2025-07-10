using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V147Transform : ITransformStrategy
	{
		static V147Transform transform;
		V147Transform() { }
		public static V147Transform Instance()
		{
			return transform ?? (transform = new V147Transform());
		}

		public Type[] ToBeTransformedTypes => new[] { typeof(RefCarrierCode) };

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 147 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCarrierCode refCarrierCode)
			{
				if (!refCarrierCode.Deleted && refCarrierCode.RefCarrierCodeAttributes.Any())
				{
					refCarrierCode.RefCarrierCodeAttributes = refCarrierCode.RefCarrierCodeAttributes?.Where(
							attribute => NotContainsUnicode(attribute.ZZG_Value))
						.ToArray();
				}
			}

			return data;
		}

		private static bool NotContainsUnicode(string input)
		{
			return input.All(c => c <= 127);
		}
	}
}
