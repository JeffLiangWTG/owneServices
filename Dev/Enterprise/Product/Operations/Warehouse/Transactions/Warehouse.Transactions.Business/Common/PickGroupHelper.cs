using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PickGroupHelper
	{
		public static ZString GetPickGroupDescription(ZShort pickGroup, ICodeDescriptionPairList pickGroups)
		{
			ZString result = "";

			if (pickGroup != 0)
			{
				var pickGroupAsString = pickGroup.ToString();
				ZString description = pickGroups.GetDescriptionFromCode(pickGroupAsString);
				result = description.IsEmpty ? pickGroupAsString : string.Format(Culture.Current, "{0} - {1}", pickGroup, description);
			}

			return result;
		}

		public static ZShort GetPickGroupFromString(ZString value)
		{
			var numericValues = value.KeepNumericCharacters();
			return numericValues.IsEmpty ? ZShort.Zero : ZShort.ParseSafe(numericValues, short.MaxValue);
		}

		public const int PickGroupForBindingMaxLength = 5 + 3 + 256; // short max len + " - " + desc max len
	}
}
