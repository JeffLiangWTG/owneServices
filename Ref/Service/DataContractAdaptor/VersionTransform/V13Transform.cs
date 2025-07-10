using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V13Transform : ITransformStrategy
	{
		static V13Transform v13Transform;

		V13Transform() { }

		public static V13Transform Instance()
		{
			if (v13Transform == null)
			{
				v13Transform = new V13Transform();
			}
			return v13Transform;
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCusNomenclatureGroupNote)
				{
					var note = result as RefCusNomenclatureGroupNote;
					note.ZZL_Language = note.ZZL_ZX6_NKLanguage;
				}
			}
			return result;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 13 && dataSetType == typeof(RefCusNomenclatureGroup);
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
