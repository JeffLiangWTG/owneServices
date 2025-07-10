using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class ConversionExtensions
	{
		public static ZString RemoveNonNumericCharacters(this ZString stringNumericCharacters) =>
			Regex.Replace(stringNumericCharacters, @"[^0-9]", string.Empty);
	}
}
