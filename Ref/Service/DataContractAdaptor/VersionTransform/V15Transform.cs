using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V15Transform : ITransformStrategy
	{
		static V15Transform v15Transform;

		V15Transform() { }

		public static V15Transform Instance()
		{
			if (v15Transform == null)
			{
				v15Transform = new V15Transform();
			}
			return v15Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 15 && dataSetType == typeof(RefCusRateType);
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCusRateCodeLanguage)
				{
					var item = result as RefCusRateCodeLanguage;
					item.ZXB_ZX6_NKLanguage = item.ZXC_ZX6_NKLanguage;
					item.ZXB_Description = item.ZXC_Description;
				}
			}
			return result;
		}

		readonly Type[] typesToBeTransformed = {
				typeof(RefCusRateCodeLanguage)
			};

		public Type[] ToBeTransformedTypes
		{
			get { return typesToBeTransformed; }
		}
	}
}
