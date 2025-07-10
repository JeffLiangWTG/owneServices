using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V29Transform : ITransformStrategy
	{
		static V29Transform v29Transform;

		V29Transform() { }

		public static V29Transform Instance()
		{
			if (v29Transform == null)
			{
				v29Transform = new V29Transform();
			}
			return v29Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 29 && (dataSetType == typeof(RefExchangeRateZZ)
				|| dataSetType == typeof(RefCusTaxOrFeeType)
				|| dataSetType == typeof(RefCusTariff));
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefExchangeRateZZ)
				{
					var item = result as RefExchangeRateZZ;
					if (item.ZZN_ExRateType == "CUD")
					{
						item.Deleted = true;
					}
				}
				else if (result is RefCusTaxOrFeeType)
				{
					var item = result as RefCusTaxOrFeeType;
					item.RefCusTaxOrFees = item.RefCusTaxOrFees?.Where(x => x.ZZF_Code.Length < 4).ToArray();
				}
				else if (result is RefCusTariff)
				{
					var item = result as RefCusTariff;
					item.RefCusTariffAttributes = GetRefCusTariffAttribute(item.RefCusTariffAttributes);
				}
			}
			return result;
		}
		static RefCusTariffAttribute[] GetRefCusTariffAttribute(RefCusTariffAttribute[] attributes)
		{
			if (attributes == null)
			{
				return null;
			}

			var attributeList = new List<RefCusTariffAttribute>();
			foreach (var attribute in attributes)
			{
				if (attribute.ZZ3_Value.Length > 100)
				{
					attribute.ZZ3_Value = attribute.ZZ3_Value.Substring(0, 100);
				}
				attributeList.Add(attribute);
			}
			return attributeList.ToArray();
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefExchangeRateZZ),
					typeof(RefCusTaxOrFeeType),
					typeof(RefCusTariff),
				};
			}
		}
	}
}
