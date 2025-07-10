using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class PECRegistrationNumberValidator : ValidationProvider
	{
		public static void ValidatePECRegistrationNumber(ZPropertyInfo propertyInfo)
		{
			if (!IsPECRegistrationNumberValid((ZString)propertyInfo.Value))
			{
				propertyInfo.AddError(Res.GetString("84DBC09A-C7AA-4E10-BC86-4E7DE02E1D98", "PEC Registration Number is not valid. It should be in a valid Email format."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		const string Pattern =
			@"^((?>[a-zA-Z\d!#$%&'*+\-/=?^_`{|}~]+\x20*" +
			@"|""((?=[\x01-\x7f])[^""\\]|\\[\x01-\x7f])*""\x20*)*" +
			@"(?<angle><))?" +
			@"((?!\.)(?>\.?[a-zA-Z\d!#$%&'*+\-/=?^_`{|}~]+)+" +
			@"|""((?=[\x01-\x7f])[^""\\]|\\[\x01-\x7f])*"")" +
			@"@" +
			@"(((?!-)[a-zA-Z\d\-_]+(?<!-)\.)+[a-zA-Z]{2,}" +
			@"|\[" +
			@"(((?(?<!\[)\.)(25[0-5]|2[0-4]\d|[01]?\d?\d)){4}" +
			@"|[a-zA-Z\d\-_]*[a-zA-Z\d]:" +
			@"((?=[\x01-\x7f])[^\\\[\]]|\\[\x01-\x7f])+)" +
			@"\])" +
			@"(?(angle)>)$";

		static bool IsPECRegistrationNumberValid(ZString pecRegistrationNumberToValidate) =>
			pecRegistrationNumberToValidate.IsEmpty || new Regex(Pattern).IsMatch(pecRegistrationNumberToValidate);
	}
}
