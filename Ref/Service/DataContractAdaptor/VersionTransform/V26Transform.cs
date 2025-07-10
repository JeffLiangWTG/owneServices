using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V26Transform : ITransformStrategy
	{
		static V26Transform v26Transform;

		V26Transform() { }

		public static V26Transform Instance()
		{
			if (v26Transform == null)
			{
				v26Transform = new V26Transform();
			}
			return v26Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 26 && dataSetType == typeof(RefDocOrgCusCode);
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefDocOrgCusCode)
				{
					var item = result as RefDocOrgCusCode;
					item.DOC_Label = $"{item.DOC_ShortLabel}|{item.DOC_LongLabel}";
					if (item.DOC_DocumentType == "HAW")
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
					typeof(RefDocOrgCusCode),
				};
			}
		}
	}
}
