using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V59Transform : ITransformStrategy
	{
		static V59Transform transForm;

		V59Transform() { }

		public static V59Transform Instance()
		{
			if (transForm == null)
			{
				transForm = new V59Transform();
			}
			return transForm;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 59 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusConditionType conditionType)
			{
				if (!conditionType.Deleted && conditionType.ZX2_ConditionClass == "RISK")
				{
					conditionType.Deleted = true;
				}
			}

			if (data is RefCusTariff tariff)
			{
				if (!tariff.Deleted && tariff.RefCusConditions != null)
				{
					tariff.RefCusConditions = tariff.RefCusConditions.Where(x => x.RefCusConditionType.ZX2_ConditionClass != "RISK").ToArray();
				}
			}

			if (data is RefCusPreference preference)
			{
				if (!preference.Deleted && preference.RefCusConditions != null)
				{
					preference.RefCusConditions = preference.RefCusConditions.Where(x => x.RefCusConditionType.ZX2_ConditionClass != "RISK").ToArray();
				}
			}

			if (data is RefCusNomenclatureGroup nomenclatureGroup)
			{
				if (!nomenclatureGroup.Deleted && nomenclatureGroup.RefCusConditions != null)
				{
					nomenclatureGroup.RefCusConditions = nomenclatureGroup.RefCusConditions.Where(x => x.RefCusConditionType.ZX2_ConditionClass != "RISK").ToArray();
				}
			}
			return data;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[]
				{
					typeof(RefCusConditionType),
					typeof(RefCusTariff),
					typeof(RefCusPreference),
					typeof(RefCusNomenclatureGroup)
				};
			}
		}
	}
}
