using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V113Transform : ITransformStrategy
	{
		static V113Transform transform;

		V113Transform() { }

		public static V113Transform Instance()
		{
			return transform ?? (transform = new V113Transform());
		}

		public Type[] ToBeTransformedTypes
			=> new[]
			{
				typeof(RefCusConditionType),
				typeof(RefCusTariff),
				typeof(RefCusPreference),
				typeof(RefCusNomenclatureGroup)
			};

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 113 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusConditionType conditionType)
			{
				if (!conditionType.Deleted && conditionType.ZX2_ConditionType.Length > 5)
				{
					conditionType.Deleted = true;
				}
			}

			if (data is RefCusTariff tariff)
			{
				if (!tariff.Deleted && tariff.RefCusConditions != null)
				{
					tariff.RefCusConditions = tariff.RefCusConditions.Where(x => x.RefCusConditionType.ZX2_ConditionType.Length <= 5).ToArray();
				}
			}

			if (data is RefCusPreference preference)
			{
				if (!preference.Deleted && preference.RefCusConditions != null)
				{
					preference.RefCusConditions = preference.RefCusConditions.Where(x => x.RefCusConditionType.ZX2_ConditionType.Length <= 5).ToArray();
				}
			}

			if (data is RefCusNomenclatureGroup nomenclatureGroup)
			{
				if (!nomenclatureGroup.Deleted && nomenclatureGroup.RefCusConditions != null)
				{
					nomenclatureGroup.RefCusConditions = nomenclatureGroup.RefCusConditions.Where(x => x.RefCusConditionType.ZX2_ConditionType.Length <= 5).ToArray();
				}
			}

			return data;
		}
	}
}
