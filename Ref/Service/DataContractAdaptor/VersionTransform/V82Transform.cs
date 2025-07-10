using System;
using System.Linq;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V82Transform : ITransformStrategy
	{
		static V82Transform v82Transform;

		V82Transform() { }

		public static V82Transform Instance()
		{
			if (v82Transform == null)
			{
				v82Transform = new V82Transform();
			}
			return v82Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 82 && ToBeTransformedTypes.Contains(dataSetType);
		}

		public T Transform<T>(T data)
		{
			if (data is RefCusTariff tariff)
			{
				var exceptedTypes = new[] { "NCMTE", "LPC", "LPCT" };
				var exceptedStyles = new[] { "COMPOSED", "DATE" };
				if(!tariff.Deleted && tariff.RefCusTariffBRCharacteristics != null)
				{
					tariff.RefCusTariffBRCharacteristics = tariff.RefCusTariffBRCharacteristics.Where(x => !exceptedTypes.Contains(x.ZB1_CharacteristicType) &&
						!exceptedStyles.Contains(x.ZB1_Style)).ToArray();
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
					typeof(RefCusTariff)
				};
			}
		}
	}
}
