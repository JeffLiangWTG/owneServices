using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// This class could contain helpful and performance tuned macros which can be used for user rateline macro evaluations
	/// </summary>
	sealed class RateLineConditionMacroLibrary : MacroLibrary
	{
		#region SuppressResourceStringsCheckRegion

		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<ICustomFieldProvider, string, object>>(
					"GetCustomField",
					"Returns value of a custom field.",
					(customFieldProvider, customFieldName) => GetCustomFieldValue(customFieldProvider, customFieldName));

				yield return new Handler<Func<decimal, string, string, decimal>>(
					"Convert",
					"Converts a value from one unit of measure to another.",
					(value, sourceUnit, targetUnit) => Convert(value, sourceUnit, targetUnit));
			}
		}

		#endregion

		static object GetCustomFieldValue(ICustomFieldProvider customFieldProvider, string customFieldName)
		{
			if (string.IsNullOrWhiteSpace(customFieldName))
			{
				return null;
			}

			var customBizObj = customFieldProvider?.GetCustomBusinessObject();

			if (!(customBizObj is IDynamicBusinessObject dynamicBizObj)
				|| !(customBizObj is BusinessObject bizObj))
			{
				return null;
			}

			foreach (var propertyName in dynamicBizObj.PropertyNames)
			{
				var propertyInfo = bizObj.ZPropertyInfoHash[propertyName];

				if (string.Compare(propertyInfo?.HumanReadableName, customFieldName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return customBizObj[propertyName];
				}
			}

			return null;
		}

		static decimal Convert(decimal value, string sourceUnit, string targetUnit)
		{
			if (Core.Constants.Weight.ContainsCode(sourceUnit) && Core.Constants.Weight.ContainsCode(targetUnit))
			{
				return Core.Constants.Weight.Convert(value, sourceUnit, targetUnit);
			}
			if (Core.Constants.Volume.ContainsCode(sourceUnit) && Core.Constants.Volume.ContainsCode(targetUnit))
			{
				return Core.Constants.Volume.Convert(value, sourceUnit, targetUnit);
			}
			if (Core.Constants.Area.ContainsCode(sourceUnit) && Core.Constants.Area.ContainsCode(targetUnit))
			{
				return Core.Constants.Area.Convert(value, sourceUnit, targetUnit);
			}
			if (Core.Constants.Length.ContainsCode(sourceUnit) && Core.Constants.Length.ContainsCode(targetUnit))
			{
				return Core.Constants.Length.Convert(value, sourceUnit, targetUnit);
			}

			return decimal.Zero;
		}
	}
}
