using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V125Transform : ITransformStrategy
	{
		static V125Transform transform;

		V125Transform() {}

		public static V125Transform Instance()
		{
			return transform ?? (transform = new V125Transform());
		}

		public Type[] ToBeTransformedTypes
			=> new[] { typeof(UNDGSubstanceJTT) };

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 125 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is UNDGSubstanceJTT undgSubstanceJtt)
			{
				if (!undgSubstanceJtt.Deleted && undgSubstanceJtt.JTT_PSN.Length > 260)
				{
					undgSubstanceJtt.Deleted = true;
				}
			}

			return data;
		}
	}
}
