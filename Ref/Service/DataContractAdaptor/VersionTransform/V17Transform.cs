using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V17Transform : ITransformStrategy
	{
		static V17Transform v17Transform;

		V17Transform() { }

		public static V17Transform Instance()
		{
			if (v17Transform == null)
			{
				v17Transform = new V17Transform();
			}
			return v17Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 17 && dataSetType == typeof(RefCusCodeList);
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCusCodeListAttribute)
				{
					var item = result as RefCusCodeListAttribute;
					item.ZZE_Name = item.ZZE_ZXE_NKName;
				}
			}
			return result;
		}

		readonly Type[] typesToBeTransformed = {
				typeof(RefCusCodeListAttribute)
			};

		public Type[] ToBeTransformedTypes
		{
			get { return typesToBeTransformed; }
		}
	}
}
