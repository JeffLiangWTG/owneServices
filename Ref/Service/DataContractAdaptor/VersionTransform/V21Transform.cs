using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V21Transform : ITransformStrategy
	{
		static V21Transform v21Transform;

		V21Transform() { }

		public static V21Transform Instance()
		{
			if (v21Transform == null)
			{
				v21Transform = new V21Transform();
			}
			return v21Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 21 && (dataSetType == typeof(RefTimeZoneSet) || dataSetType == typeof(RefCusTradeGroup));
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefTimeZone)
				{
					var item = result as RefTimeZone;
					item.RefTimeZoneRule = item.RefTimeZoneRules;
				}
				else if (result is RefCusTradeGroup)
				{
					var item = result as RefCusTradeGroup;
					item.RefCusTradeGroupLanguage = item.RefCusTradeGroupLanguages;
				}
			}
			return result;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefTimeZone),
					typeof(RefCusTradeGroup)
				};
			}
		}
	}
}
