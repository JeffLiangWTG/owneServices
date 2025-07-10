using System;
using CargoWise.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public static class WorkTaskRelatedItemHelper
	{
		public static string GetSelectionCriterionString(ICodeDescriptionPairList lookupValues, string code)
		{
			var description = lookupValues.GetDescriptionFromCode(code);

			return string.IsNullOrEmpty(description) ? code : FormattableString.Invariant($"{code} - {description}"); // There aren't any actual words here
		}
	}
}
