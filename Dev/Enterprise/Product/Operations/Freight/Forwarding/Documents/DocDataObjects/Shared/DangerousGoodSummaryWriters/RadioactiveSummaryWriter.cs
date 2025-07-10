using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class RadioactiveSummaryWriter
	{
		public static IEnumerable<string> GetRadioactiveSummary(this IDangerousGood dangerousGood)
		{
			yield return dangerousGood.RadionuclideComponent();
			yield return dangerousGood.MaterialFormDescriptionComponent();
			yield return dangerousGood.RadioactiveLabelCategoryComponent();
			yield return dangerousGood.RadioactiveTransportIndexComponent();
			yield return dangerousGood.HRCQComponent();
			yield return dangerousGood.FissileExceptedComponent();
			yield return dangerousGood.ExclusiveUseComponent();
		}

		#region Material Form Description

		public static string MaterialFormDescriptionComponent(this IDangerousGood dangerousGood) => dangerousGood.MaterialFormDescription;

		#endregion

		#region Radionuclide

		public static string RadionuclideComponent(this IDangerousGood dangerousGood)
		{
			var radionuclideElement = dangerousGood.RadionuclideElement?.Code ?? string.Empty;
			var radionuclideElementSuffix = dangerousGood.RadionuclideElementSuffix;
			var maximumActivity = dangerousGood.RadioactiveMaximumActivity?.Value ?? 0;
			var activityUnit = dangerousGood.RadioactiveMaximumActivity?.Unit?.Code ?? string.Empty;

			var radionuclideComponentLines = new List<string>();
			if (!radionuclideElement.IsEmpty && !radionuclideElementSuffix.IsEmpty)
			{
				if (char.IsDigit(radionuclideElementSuffix[0]))
				{
					radionuclideComponentLines.Add(string.Format("{0}-{1}", radionuclideElement, radionuclideElementSuffix));
				}
				else
				{
					radionuclideComponentLines.Add(string.Format("{0} {1}", radionuclideElement, radionuclideElementSuffix));
				}
			}

			if (!activityUnit.IsEmpty)
			{
				(var becquerelQuantity, var becquerelUnits) = ConvertToAppropriateBecquerelUnits(maximumActivity, activityUnit);
				(var curieQuantity, var curieUnits) = ConvertToAppropriateCurieUnits(maximumActivity, activityUnit);

				var becquerelUnitsSymbol = Core.Constants.RadioactiveUnits.GetSymbol(becquerelUnits);
				var curieUnitsSymbol = Core.Constants.RadioactiveUnits.GetSymbol(curieUnits);
				if (!string.IsNullOrEmpty(becquerelUnitsSymbol) && !string.IsNullOrEmpty(curieUnitsSymbol))
				{
					radionuclideComponentLines.Add(string.Format("{0} {1} ({2} {3})", becquerelQuantity, becquerelUnitsSymbol, curieQuantity, curieUnitsSymbol));
				}
			}

			if (radionuclideComponentLines.Any())
			{
				var radionuclideComponentResult = string.Join(", ", radionuclideComponentLines);
				return radionuclideComponentResult;
			}

			return string.Empty;
		}

		public static (decimal quantity, string unit) ConvertToAppropriateCurieUnits(decimal maximumActivity, string activityUnit)
		{
			var curie = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Curie);
			if (curie < 0.001m)
			{
				var microCurie = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Microcurie);
				microCurie = Utilities.Round(microCurie, 2);
				return (microCurie, Core.Constants.RadioactiveUnits.Microcurie);
			}
			else if (curie < 0.1m)
			{
				var milliCurie = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Millicurie);
				milliCurie = Utilities.Round(milliCurie, 2);
				return (milliCurie, Core.Constants.RadioactiveUnits.Millicurie);
			}

			curie = Utilities.Round(curie, 2);
			return (curie, Core.Constants.RadioactiveUnits.Curie);
		}

		static (decimal quantity, string unit) ConvertToAppropriateBecquerelUnits(decimal maximumActivity, string activityUnit)
		{
			var megabecquerel = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Megabecquerel);
			if (megabecquerel >= 100000m)
			{
				var terabecquerel = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Terabecquerel);
				terabecquerel = Utilities.Round(terabecquerel, 2);
				return (terabecquerel, Core.Constants.RadioactiveUnits.Terabecquerel);
			}
			else if (megabecquerel >= 100m)
			{
				var gigabecquerel = Core.Constants.RadioactiveUnits.ConvertSafe(maximumActivity, activityUnit, Core.Constants.RadioactiveUnits.Gigabecquerel);
				gigabecquerel = Utilities.Round(gigabecquerel, 2);
				return (gigabecquerel, Core.Constants.RadioactiveUnits.Gigabecquerel);
			}

			megabecquerel = Utilities.Round(megabecquerel, 2);
			return (megabecquerel, Core.Constants.RadioactiveUnits.Megabecquerel);
		}

		#endregion

		#region Radioactive Label Category

		public static string RadioactiveLabelCategoryComponent(this IDangerousGood dangerousGood)
		{
			var radioactiveLabelCategory = dangerousGood.RadioactiveLabelCategory;
			if (radioactiveLabelCategory == null)
			{
				return string.Empty;
			}

			if (!radioactiveLabelCategory.Code.IsEmpty)
			{
				var description = new RadioactiveLabelCategoryList()
					.GetDescriptionFromCode(radioactiveLabelCategory.Code);

				return $"RADIOACTIVE {description?.ToUpper()} LABEL"; // Fixed format text for document
			}

			return string.Empty;
		}

		#endregion

		#region Radioactive Transport Index

		public static string RadioactiveTransportIndexComponent(this IDangerousGood dangerousGood)
		{
			var transportIndex = dangerousGood.RadioactiveTransportIndex;
			if (!transportIndex.IsEmpty)
			{
				return $"TI = {transportIndex}"; // Fixed format text for document
			}

			return string.Empty;
		}

		#endregion

		#region Highway Route Controlled Quantity

		public static string HRCQComponent(this IDangerousGood dangerousGood)
		{
			return dangerousGood.IsHighwayRouteControlledQuantity
				? "HRCQ"
				: string.Empty;
		}

		#endregion

		#region Fissile Excepted

		public static string FissileExceptedComponent(this IDangerousGood dangerousGood)
		{
			return dangerousGood.IsFissileExcepted
				? (NoResString)"Fissile Excepted"  // Fixed format text for document
				: string.Empty;
		}

		#endregion

		#region Exclusive Use

		public static string ExclusiveUseComponent(this IDangerousGood dangerousGood)
		{
			return dangerousGood.IsExclusiveUse
				? (NoResString)"Exclusive Use" // not translatable
				: string.Empty;
		}

		#endregion
	}
}
