using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class UNDGPermissableQuantitiesHelper
	{
		public readonly struct UNDGPermissibleQuantity : IEquatable<UNDGPermissibleQuantity>
		{
			public UNDGPermissibleQuantity(ZString packingInstruction, ZString packingInstructionSection, ZDecimal paxLimit, ZString paxLimitUnits, ZDecimal caoLimit, ZString caoLimitUnits)
			{
				PackingInstruction = packingInstruction;
				PackingInstructionSection = packingInstructionSection;
				PaxLimit = paxLimit;
				PaxLimitUnits = paxLimitUnits;
				CaoLimit = caoLimit;
				CaoLimitUnits = caoLimitUnits;
			}
			public ZString PackingInstruction { get; }
			public ZString PackingInstructionSection { get; }
			public ZDecimal PaxLimit { get; }
			public ZString PaxLimitUnits { get; }
			public ZDecimal CaoLimit { get; }
			public ZString CaoLimitUnits { get; }

			public static bool operator ==(UNDGPermissibleQuantity a, UNDGPermissibleQuantity b)
			{
				return a.PackingInstruction == b.PackingInstruction &&
					a.PackingInstructionSection == b.PackingInstructionSection &&
					a.PaxLimit == b.PaxLimit &&
					a.PaxLimitUnits == b.PaxLimitUnits &&
					a.CaoLimit == b.CaoLimit &&
					a.CaoLimitUnits == b.CaoLimitUnits;
			}
			public static bool operator !=(UNDGPermissibleQuantity a, UNDGPermissibleQuantity b)
			{
				return !(a == b);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = 364954169;
					hashCode = hashCode * 318717613 + PackingInstruction.GetHashCode();
					hashCode = hashCode * 318717613 + PackingInstructionSection.GetHashCode();
					hashCode = hashCode * 318717613 + PaxLimit.GetHashCode();
					hashCode = hashCode * 318717613 + PaxLimitUnits.GetHashCode();
					hashCode = hashCode * 318717613 + CaoLimit.GetHashCode();
					hashCode = hashCode * 318717613 + CaoLimitUnits.GetHashCode();
					return hashCode;
				}
			}

			bool IEquatable<UNDGPermissibleQuantity>.Equals(UNDGPermissibleQuantity other)
			{
				return other is UNDGPermissibleQuantity element && this == element;
			}

			public override bool Equals(object obj)
			{
				return obj is UNDGPermissibleQuantity element && this == element;
			}
		}

		public static IEnumerable<UNDGPermissibleQuantity> GetUNDGPermissibleQuantities()
		{
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIA, 0, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIB, 0, Core.Constants.Weight.Kilograms, 10, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI966, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI966, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI967, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI967, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI968, PackingInstructionSectionTypeList.Codes.SectionIA, 0, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI968, PackingInstructionSectionTypeList.Codes.SectionIB, 0, Core.Constants.Weight.Kilograms, 2.5, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI969, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI969, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI970, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI970, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.Forbidden, PackingInstructionSectionTypeList.Codes.SectionI, 0, Core.Constants.Weight.Kilograms, 0, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.Forbidden, PackingInstructionSectionTypeList.Codes.SectionII, 0, Core.Constants.Weight.Kilograms, 0, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.Forbidden, PackingInstructionSectionTypeList.Codes.SectionIA, 0, Core.Constants.Weight.Kilograms, 0, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.Forbidden, PackingInstructionSectionTypeList.Codes.SectionIB, 0, Core.Constants.Weight.Kilograms, 0, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI977, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI977, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI978, PackingInstructionSectionTypeList.Codes.SectionI, 5, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms);
			yield return new UNDGPermissibleQuantity(SodiumBatteryConstants.RefPackingInstructions.PI978, PackingInstructionSectionTypeList.Codes.SectionII, 5, Core.Constants.Weight.Kilograms, 5, Core.Constants.Weight.Kilograms);
		}

		public static IEnumerable<(ZString, ZString)> GetConsignmentRestrictedLithiumBatteryCodesAndSection()
		{
			yield return (LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries, PackingInstructionSectionTypeList.Codes.SectionII);
			yield return (LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries, PackingInstructionSectionTypeList.Codes.SectionII);
		}

		public static UNDGPermissibleQuantity GetPermissibleQuantityForUNDGDataItem(UNDGDataItem undgDataItem, bool isCargoOnly)
		{
			var substance = undgDataItem.UNDGSubstance;
			var packingInstructionSection = undgDataItem.DI_PackingInstructionSection;
			var packingInstruction = isCargoOnly
				? (substance?.DG_CargoPackIns ?? ZString.Empty)
				: (substance?.DG_PaxPackIns ?? ZString.Empty);

			var permissibleQuantities = GetUNDGPermissibleQuantities();
			var undgPermissibleQuantity = permissibleQuantities
				.FirstOrDefault(permissibleQuantity => permissibleQuantity.PackingInstruction == packingInstruction && permissibleQuantity.PackingInstructionSection == packingInstructionSection);

			return undgPermissibleQuantity;
		}

		public static IEnumerable<UNDGPermissibleQuantity> GetPermissibleQuantitiesForPackingInstruction(ZString packingInstruction)
		{
			if (packingInstruction == LithiumBatteryConstants.RefPackingInstructions.Forbidden || packingInstruction == SodiumBatteryConstants.RefPackingInstructions.Forbidden)
			{
				return new List<UNDGPermissibleQuantity>();
			}
			var permissibleQuantities = GetUNDGPermissibleQuantities();
			var undgPermissibleQuantities = permissibleQuantities
				.Where(permissibleQuantity => permissibleQuantity.PackingInstruction == packingInstruction)
				.OrderBy(x => x.PackingInstructionSection);

			return undgPermissibleQuantities;
		}
	}
}
