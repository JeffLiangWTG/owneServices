using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V14Transform : ITransformStrategy
	{
		static V14Transform v14Transform;

		V14Transform() { }

		public static V14Transform Instance()
		{
			if (v14Transform == null)
			{
				v14Transform = new V14Transform();
			}
			return v14Transform;
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCusNomenclatureGroup)
				{
					var item = result as RefCusNomenclatureGroup;
					if (item.ZZ5_ZZ9_NKNomenclatureGroupType == "ZA")
					{
						item.ZZ5_Type = "1P1";
					}
					else
					{
						item.ZZ5_Type = item.ZZ5_ZZ9_NKNomenclatureGroupType;
					}
				}
			}
			return result;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 14 && dataSetType == typeof(RefCusNomenclatureGroup);
		}

		readonly Type[] typesToBeTransformed = {
				typeof(RefCusNomenclatureGroupNote)
			};

		public Type[] ToBeTransformedTypes
		{
			get { return typesToBeTransformed; }
		}
	}
}
