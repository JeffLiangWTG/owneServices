using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class USFSISLineAddInfoValidationHelper
	{
		public static void CheckEstablishNumbers(ZPropertyInfo propertyInfo, ZString valueExtNo)
		{
			if (!valueExtNo.IsEmpty && !Regex.IsMatch(valueExtNo, "^[a-z0-9A-Z]*$"))
			{
				propertyInfo.AddMessageError(EstablishmentNumbersOnlyContainNumbersAndLetters);
			}
		}
		public const string EstablishmentNumbersOnlyContainNumbersAndLetters = "Establishment numbers can only contain letters and numbers, no punctuation.";
	}
}
