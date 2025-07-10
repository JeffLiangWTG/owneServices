using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V28Transform : ITransformStrategy
	{
		static V28Transform v28Transform;

		V28Transform() { }

		public static V28Transform Instance()
		{
			if (v28Transform == null)
			{
				v28Transform = new V28Transform();
			}
			return v28Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 28 && (dataSetType == typeof(RefCusConditionType)
				|| dataSetType == typeof(RefCusNomenclatureGroup)
				|| dataSetType == typeof(RefCusPreference)
				|| dataSetType == typeof(RefCusTariff));
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCusConditionType)
				{
					var item = result as RefCusConditionType;
					if (item.ZX2_ConditionClass == "VAT")
					{
						item.Deleted = true;
					}
				}
				else if (result is RefCusNomenclatureGroup)
				{
					var item = result as RefCusNomenclatureGroup;
					item.RefCusConditions = GetRefCusCondition(item.RefCusConditions);
				}
				else if (result is RefCusPreference)
				{
					var item = result as RefCusPreference;
					item.RefCusConditions = GetRefCusCondition(item.RefCusConditions);
				}
				else if (result is RefCusTariff)
				{
					var item = result as RefCusTariff;
					item.RefCusConditions = GetRefCusCondition(item.RefCusConditions);
				}
			}
			return result;
		}
		static RefCusCondition[] GetRefCusCondition(RefCusCondition[] conditions)
		{
			if (conditions == null)
			{
				return null;
			}

			var conditionList = new List<RefCusCondition>();
			foreach (var condition in conditions)
			{
				if (condition.RefCusConditionType != null && condition.RefCusConditionType.ZX2_ConditionClass == "VAT")
				{
					continue;
				}
				conditionList.Add(condition);
			}
			return conditionList.ToArray();
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefCusConditionType),
					typeof(RefCusNomenclatureGroup),
					typeof(RefCusPreference),
					typeof(RefCusTariff),
				};
			}
		}
	}
}
