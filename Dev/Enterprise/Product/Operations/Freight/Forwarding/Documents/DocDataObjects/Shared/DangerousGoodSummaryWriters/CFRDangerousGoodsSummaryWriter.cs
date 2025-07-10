using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public static class CFRDangerousGoodsSummaryWriter
	{
		public static string WriteCFRSummary(this IDangerousGood dangerousGood)
		{
			var data = new string[]
			{
				dangerousGood.CFRUNNumber(),
				dangerousGood.CFRPSNComponent(),
				dangerousGood.CFRSpecialPermitNumberComponent(),
				dangerousGood.CFRWasteCodeComponent(),
				dangerousGood.MaterialFormDescriptionComponent(),
				dangerousGood.CFRClassComponent(),
				dangerousGood.RadionuclideComponent(),
				dangerousGood.RadioactiveLabelCategoryComponent(),
				dangerousGood.RadioactiveTransportIndexComponent(),
				dangerousGood.PackingGroupDescription(),
				dangerousGood.CFRReportableQuantityComponent(),
				dangerousGood.CFRPoisonInhalationHazardComponent(),
				dangerousGood.FlashPointDescription(),
				dangerousGood.CFRMarinePollutantComponent(),
				dangerousGood.CFRPSAGroupComponent(),
				dangerousGood.CFRLimitedQuantityComponent(),
				dangerousGood.CFRResidueLastContainedComponent(),
				dangerousGood.HRCQComponent(),
				dangerousGood.FissileExceptedComponent(),
				dangerousGood.ExclusiveUseComponent()
			};

			return string.Join(", ", data.Where(s => !s.IsNullOrEmpty())); // Fixed format text for document
		}

		#region CFR UNNO

		static string CFRUNNumber(this IDangerousGood dangerousGood)
		{
			return FormattableString.Invariant($"{dangerousGood.CFRUNNumberPrefix()}{dangerousGood.Unno}"); // Fixed format text for document
		}

		static string CFRUNNumberPrefix(this IDangerousGood dangerousGood)
		{
			if (!dangerousGood.Prefix.IsEmpty)
			{
				return dangerousGood.Prefix.ToUpper();
			}

			return dangerousGood.Unno == "8000"
				? "ID"
				: "UN";
		}

		#endregion

		#region Proper Shipping Name

		static string CFRPSNComponent(this IDangerousGood dangerousGood)
		{
			var properShippingNameWithTechnicalName = GetPSNWithTechnicalNameOfSubstance(dangerousGood);

			if (dangerousGood.ProperShippingName.Contains(RawMoltenString, System.StringComparison.InvariantCultureIgnoreCase) ||
				dangerousGood.ProperShippingName.Contains(RawElevatedTemperatureString, System.StringComparison.InvariantCultureIgnoreCase))
			{
				return properShippingNameWithTechnicalName;
			}

			var stringBuilder = new StringBuilder();

			if (CheckIfSubstanceIsInElevatedTemperatures(dangerousGood))
			{
				stringBuilder.Append((NoResString)"HOT - "); // Non-translateable prefix
			}

			stringBuilder.Append(properShippingNameWithTechnicalName);

			return stringBuilder.ToString();
		}

		static string GetPSNWithTechnicalNameOfSubstance(IDangerousGood dangerousGood)
		{
			var properShippingName = dangerousGood.ProperShippingName;

			if (!properShippingName.IsEmpty
				&& !dangerousGood.TechnicalName.IsEmpty)
			{
				return properShippingName + " (" + dangerousGood.TechnicalName + ")";
			}

			return properShippingName;
		}

		static bool CheckIfSubstanceIsInElevatedTemperatures(IDangerousGood dangerousGood)
		{
			switch (dangerousGood.State)
			{
				case UNDGSubstanceLookups.StateTypes.Code.Liquid:
					return IsLiquidSubstanceInElevatedTemperature(dangerousGood);

				case UNDGSubstanceLookups.StateTypes.Code.Solid:
					return IsSolidSubstanceInElevatedTemperature(dangerousGood);

				default:
					return false;
			}
		}

		static bool IsLiquidSubstanceInElevatedTemperature(IDangerousGood dangerousGood)
		{
			if (!dangerousGood.RequiresTemperatureControl)
			{
				return false;
			}

			var maximumTemperatureInCentigrade = GetRequiredMaximumTemperatureInCentigrade(dangerousGood);
			var flashPointOfDG = dangerousGood.FlashPoint?.Value ?? 0;

			var waterBoilingTemperatureInCentigrade = 100;
			var oshaFlashPointLimitInCentigrade = 38;

			if (maximumTemperatureInCentigrade >= waterBoilingTemperatureInCentigrade ||
				(flashPointOfDG >= oshaFlashPointLimitInCentigrade &&
				maximumTemperatureInCentigrade >= flashPointOfDG))
			{
				return true;
			}

			return false;
		}

		static bool IsSolidSubstanceInElevatedTemperature(IDangerousGood dangerousGood)
		{
			if (!dangerousGood.RequiresTemperatureControl)
			{
				return false;
			}

			var solidPhaseTemperatureLimitInCentigrade = 240;
			var maximumTemperatureInCentigrade = GetRequiredMaximumTemperatureInCentigrade(dangerousGood);
			if (maximumTemperatureInCentigrade >= solidPhaseTemperatureLimitInCentigrade)
			{
				return true;
			}

			return false;
		}

		static decimal GetRequiredMaximumTemperatureInCentigrade(IDangerousGood dangerousGood)
		{
			return Core.Constants.Temperature.Convert(
				sourceValue: dangerousGood.RequiredTemperatureMaximum?.Value ?? 0,
				sourceUnitCode: dangerousGood.RequiredTemperatureMaximum?.Unit?.Code ?? string.Empty,
				targetUnitCode: Core.Constants.Temperature.Centigrade);
		}

		const string RawMoltenString = "MOLTEN"; // Non-translateable constant
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translateable constant")]
		const string RawElevatedTemperatureString = "ELEVATED TEMPERATURE";

		#endregion

		#region Special Permit Number

		static string CFRSpecialPermitNumberComponent(this IDangerousGood dangerousGood)
		{
			if (dangerousGood.SpecialPermitNumber.IsEmpty)
			{
				return string.Empty;
			}

			return string.Concat(SpecialPermitPrefixNotation, " ", dangerousGood.SpecialPermitNumber);
		}

		const string SpecialPermitPrefixNotation = "DOT-SP";

		#endregion

		#region Waste Code

		static string CFRWasteCodeComponent(this IDangerousGood dangerousGood) => dangerousGood.HazardousWasteCode;

		#endregion

		#region Class

		static string CFRClassComponent(this IDangerousGood dangerousGood)
		{
			var unnoWithVariant = dangerousGood.Code.ToUpperInvariant();

			if (unnoWithVariant == combustibleLiquidCode)
			{
				return string.Empty;
			}

			if (dangerousGood.IMOClass.IsEmpty)
			{
				return string.Empty;
			}

			return $"class {dangerousGood.IMOClass}"; // Fixed format text for document
		}

		const string combustibleLiquidCode = "1993D";

		#endregion

		#region Reportable Quantity

		static string CFRReportableQuantityComponent(this IDangerousGood dangerousGood)
		{
			if (dangerousGood.ReportableQuantity == null)
			{
				return string.Empty;
			}

			if (TryGetReportableQuantityInPounds(dangerousGood, out var reportableQuantityInPounds))
			{
				var dataItemWeightInPounds = Core.Constants.Weight
					.ConvertSafe(
						sourceValue: dangerousGood.Weight.Value,
						sourceUnitCode: dangerousGood.Weight.Unit.Code.ToUpperInvariant(),
						targetUnitCode: Core.Constants.Weight.Pounds);

				if (dataItemWeightInPounds >= reportableQuantityInPounds)
				{
					return ReportableQuantityAcronym;
				}
			}

			return string.Empty;
		}

		static bool TryGetReportableQuantityInPounds(IDangerousGood dangerousGood, out decimal reportableQuantity)
		{
			if (dangerousGood.ReportableQuantity.Value == ZDecimal.Zero)
			{
				reportableQuantity = 0;
				return false;
			}

			reportableQuantity = Core.Constants.Weight
				.ConvertSafe(
					sourceValue: dangerousGood.ReportableQuantity.Value,
					sourceUnitCode: dangerousGood.ReportableQuantity.Unit.Code.ToUpperInvariant(),
					targetUnitCode: Core.Constants.Weight.Pounds);

			return true;
		}

		const string ReportableQuantityAcronym = "RQ";

		#endregion

		#region Poison Inhalation Hazard

		static string CFRPoisonInhalationHazardComponent(this IDangerousGood dangerousGood)
		{
			if (dangerousGood.PoisonInhalationHazard.IsEmpty)
			{
				return string.Empty;
			}

			return $"Poison-Inhalation Hazard Zone {dangerousGood.PoisonInhalationHazard}"; // Fixed format text for document
		}

		#endregion

		#region PSA Group

		static string CFRPSAGroupComponent(this IDangerousGood dangerousGood) => !dangerousGood.PSAGroup.IsEmpty ? $"PSA Group: {dangerousGood.PSAGroup}" : string.Empty; // Fixed format text for document

		#endregion

		#region Marine Pollutant

		static string CFRMarinePollutantComponent(this IDangerousGood dangerousGood)
		{
			var marinePollutantCode = dangerousGood.MarinePollutant?.Code ?? string.Empty;
			if (marinePollutantCode.IsEmpty || marinePollutantCode == UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code)
			{
				return string.Empty;
			}

			return (NoResString)"MARINE POLLUTANT"; // Fixed format text for document
		}

		#endregion

		#region Limited Quantity

		static string CFRLimitedQuantityComponent(this IDangerousGood dangerousGood)
		{
			if (dangerousGood.Unno == "8000")
			{
				return (NoResString)"Limited Quantity"; // Fixed format text for document
			}

			var containsRadioactiveSubclass = !dangerousGood.IMOClass.Contains("7")
				&& (dangerousGood.SecondaryClass.Contains("7") || dangerousGood.TertiaryClass.Contains("7")); // non-translatable registration number

			if (dangerousGood.PackedInLimitedQuantity && containsRadioactiveSubclass)
			{
				return (NoResString)"Limited quantity radioactive material"; // Fixed format text for document
			}
			else if (dangerousGood.PackedInLimitedQuantity)
			{
				return (NoResString)"LTD QTY"; // Fixed format text for document
			}

			return string.Empty;
		}

		#endregion

		#region Residue Last Contained

		static string CFRResidueLastContainedComponent(this IDangerousGood dangerousGood)
		{
			return dangerousGood.IsResidueLastContained
				? (NoResString)"RESIDUE: Last Contained * * *" // Fixed format text for document
				: string.Empty;
		}

		#endregion
	}
}
