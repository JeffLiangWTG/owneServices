using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V27Transform : ITransformStrategy
	{
		static V27Transform v27Transform;

		V27Transform() { }

		public static V27Transform Instance()
		{
			if (v27Transform == null)
			{
				v27Transform = new V27Transform();
			}
			return v27Transform;
		}

		public Type[] ToBeTransformedTypes => new[] { typeof(RefCusCodeType) };

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 27 && dataSetType == typeof(RefCusCodeType);
		}

		public T Transform<T>(T data)
		{
			var result = data as RefCusCodeType;
			if (result != null)
			{
				result.ZZK_ZZZ_NKDataGrouping = string.Empty;
			}
			return data;
		}
	}
}
