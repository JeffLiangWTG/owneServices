using System;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public partial class PricingPageRateLineFactory
	{
		sealed class ContainerEquipmentKey : IEquatable<ContainerEquipmentKey>
		{
			public ContainerEquipmentKey(RateLine line)
			{
				line.SendErrorReporterIfParentIsNull("ContainerEquipmentKey.Constructor1");

				ContainerCode = line.Parent.Container != null ? line.Parent.Container.RC_Code : ZString.Empty;

				equipmentType = line.Calculator.EquipmentType;
			}

			public ContainerEquipmentKey(ZString containerCode, RateLine line)
			{
				line.SendErrorReporterIfParentIsNull("ContainerEquipmentKey.Constructor2");

				ContainerCode = containerCode;

				equipmentType = line.Parent.Container != null ? ZString.Empty : line.Calculator.EquipmentType;
			}

			public override bool Equals(object obj) => Equals(obj as ContainerEquipmentKey);

			public bool Equals(ContainerEquipmentKey other)
				=> other != null
					&& other.ContainerCode == ContainerCode
					&& other.equipmentType == equipmentType;

			public override int GetHashCode()
			{
				unchecked
				{
					var result = (uint)ContainerCode.GetHashCode();
					result = (result << 27) | (result >> 5);
					return ((int)result) ^ equipmentType.GetHashCode();
				}
			}

			public readonly ZString ContainerCode;
			readonly ZString equipmentType;
		}
	}
}
