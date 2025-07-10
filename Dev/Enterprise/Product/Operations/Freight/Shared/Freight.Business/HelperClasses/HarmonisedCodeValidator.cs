using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public static class HarmonisedCodeValidator
	{
		public static void Validate(ZPropertyInfo harmonisedCodeInfo, ZBool isInDatabase)
		{
			ZString harmonisedCode = harmonisedCodeInfo.Value.ToString();

			if (!harmonisedCode.IsEmpty && !Regex.IsMatch(harmonisedCode, FreightConstants.HarmonisedCodeRegexPattern))
			{
				var notification = Res.GetString("7e778961-30aa-4e82-97d0-167b28bd3e80", "Invalid Harmonized Code. Only numeric characters and dots are allowed.");

				if (isInDatabase && !harmonisedCodeInfo.HasChanges)
				{
					harmonisedCodeInfo.AddWarning(notification);
				}
				else
				{
					harmonisedCodeInfo.AddError(notification);
				}
			}
		}
	}
}
