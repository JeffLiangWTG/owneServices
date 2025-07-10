using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class ContactNameHelper
	{
		public static ZString GetFormattedName(ZString contactName, bool isGetAllCharactersForMiddleName = false)
		{
			var result = contactName;
			if (!contactName.IsEmpty)
			{
				var names = contactName.Split(' ');
				var namesCount = names.Length;
				if (namesCount == 1)
				{
					result = names[0].Left(40);
				}
				else
				{
					result = (ZString)(names[names.Length - 1].Left(40) + ", " + names[0].Left(40));
					if (namesCount > 2)
					{
						result += ", " + (isGetAllCharactersForMiddleName ? names[1].Left(40) : names[1].Left(1));
					}
				}
			}
			return result;
		}

		public static ZString GetWarningMessageIfInvalidFormat(ZString contactName)
		{
			var result = ZString.Empty;
			if (!contactName.IsEmpty)
			{
				var regex = new Regex("^([a-zA-Z]{1,40}, [a-zA-Z]{1,40})(, [a-zA-Z]{1,18})?$");
				if (!regex.IsMatch(contactName))
				{
					result = "Format of name should be Last Name(max 40 characters), First Name(max 40 characters), Middle Initial(optional, max 18 characters)";
				}
			}
			return result;
		}

		public static (ZString, ZString, ZString) GetSplitContactName(ZString contactName)
		{
			var firstName = ZString.Empty;
			var middleInitial = ZString.Empty;
			var lastName = ZString.Empty;
			contactName = MessageBlockStringDataCorrector.KeepOnlyValidCharacters(contactName, ABICharacterTypeString.Constants.Alphabetic, OrgContact.Schema.OC_ContactNameMaxLength);

			var contactNames = contactName.Split(' ');
			if (contactNames.Length == 1)
			{
				lastName = contactNames[0];
			}
			else if (contactNames.Length > 1)
			{
				firstName = contactNames[0];
				lastName = contactNames[contactNames.Length - 1];

				if (contactNames.Length > 2)
				{
					middleInitial = contactNames[1].Left(1);
				}
			}
			return (firstName, middleInitial, lastName);
		}
	}
}
