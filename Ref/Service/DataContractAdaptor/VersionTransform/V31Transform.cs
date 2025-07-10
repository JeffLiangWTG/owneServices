using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V31Transform : ITransformStrategy
	{
		static V31Transform v31Transform;

		V31Transform() { }

		public static V31Transform Instance()
		{
			if (v31Transform == null)
			{
				v31Transform = new V31Transform();
			}
			return v31Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 31 && (dataSetType == typeof(RefHarbourRate));
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefHarbourRate)
				{
					var item = result as RefHarbourRate;
					if (item.ZXF_Mode == "ALL")
					{
						item.Deleted = true;
					}
				}
			}
			return result;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefHarbourRate)
				};
			}
		}
	}
}
