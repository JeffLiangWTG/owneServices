using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V24Transform : ITransformStrategy
	{
		static V24Transform v24Transform;

		V24Transform() { }

		public static V24Transform Instance()
		{
			if (v24Transform == null)
			{
				v24Transform = new V24Transform();
			}
			return v24Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 24 && (dataSetType == typeof(RefCarrierCode)
				|| dataSetType == typeof(RefCusCodeList));
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefCarrierCode)
				{
					var item = result as RefCarrierCode;

					var modeOfTransportAttributes = new List<RefCarrierCodeAttribute>();
					if (item.ZZ4_IsSea)
					{
						modeOfTransportAttributes.Add(new RefCarrierCodeAttribute() { ZZG_Name = "SEA", ZZG_Value = "SEA" });
					}
					if (item.ZZ4_IsAir)
					{
						modeOfTransportAttributes.Add(new RefCarrierCodeAttribute() { ZZG_Name = "AIR", ZZG_Value = "AIR" });
					}
					if (item.ZZ4_IsRoad)
					{
						modeOfTransportAttributes.Add(new RefCarrierCodeAttribute() { ZZG_Name = "ROAD", ZZG_Value = "ROAD" });
					}
					if (item.ZZ4_IsRail)
					{
						modeOfTransportAttributes.Add(new RefCarrierCodeAttribute() { ZZG_Name = "RAIL", ZZG_Value = "RAIL" });
					}
					modeOfTransportAttributes.AddRange(item.RefCarrierCodeAttributes);
					item.RefCarrierCodeAttributes = modeOfTransportAttributes.ToArray();
				}
				else if (result is RefCusCodeListAttribute item)
				{
					if (item.ZZE_Value != null && item.ZZE_Value.Length > 100)
					{
						item.ZZE_Value = item.ZZE_Value?.Substring(0, 100);
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
					typeof(RefCusCodeListAttributeName),
					typeof(RefCusCodeListAttribute)
				};
			}
		}
	}
}
