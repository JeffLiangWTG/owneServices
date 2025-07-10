using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class SouthAfricanOrganisationCodeValidator
	{
		public bool IsValid(string organisationCode)
		{
			bool isValid;

			ZInt organisationAsInt = 0;
			ZInt.TryParse(organisationCode, out organisationAsInt);

			if (organisationAsInt == 70707070)
			{
				isValid = true;
			}
			else
			{
				int result = 0;
				for (int i = 0; i < 8; i++)
				{
					int positionalValue = (organisationAsInt / divisors[i]) % 10;
					int positionalMultiplier = multipliers[i];
					int amountToAdd = positionalValue * positionalMultiplier;
					result += amountToAdd;
				}
				isValid = organisationAsInt > 0 && result % 11 == 0;
				isValid = isValid || (organisationAsInt > 0 && result % 10 == 0);
			}
			return isValid;
		}

		readonly int[] multipliers = { 1, 2, 3, 4, 6, 7, 8, 9 };
		readonly int[] divisors = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000 };
	}
}
