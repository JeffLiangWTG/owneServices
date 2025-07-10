using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	class ContactValidator
	{
		internal const string INELIGIBILITY_PATTERN = @"^(.)\1+$";

		public static void CheckContactDetails(int fieldMaxLength, ZPropertyInfo contactInfo, USOrganisation org)
		{
			if (MessageBlockStringDataCorrector.KeepOnlyValidCharacters((ZString)contactInfo.Value, ABICharacterTypeString.Constants.Alphabetic, fieldMaxLength).IsEmpty)
			{
				contactInfo.AddMessageError(ContactNameRequired);
			}

			if (!org.LastName.IsEmpty)
			{
				if (!IsNameValid(org.LastName))
				{
					contactInfo.AddMessageError(NameInvalid);
				}
			}
			if (!org.FirstName.IsEmpty)
			{
				if (!IsNameValid(org.FirstName))
				{
					contactInfo.AddMessageError(NameInvalid);
				}
			}
		}

		public static bool IsNameValid(ZString name)
		{
			bool result = true;
			if (name.Trim('.').Length < 2 || Regex.IsMatch(name, INELIGIBILITY_PATTERN))
			{
				result = false;
			}

			return result;
		}

		internal const string ContactNameRequired = "A valid Contact Name is required.";
		internal const string NameInvalid = "A name (first name or last name) must contain at least two letters and cannot all be the same letter.";
	}
}
