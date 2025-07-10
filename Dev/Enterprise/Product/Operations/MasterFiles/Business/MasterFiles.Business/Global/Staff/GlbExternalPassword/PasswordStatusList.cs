using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class PasswordStatusList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Codes
		{
			public const string Deactivated = "INA";
			public const string PasswordOK = Core.Constants.PasswordOK;
			public const string Invalid = "INV";
			public const string Valid = "VAL";
			public const string Pending = "PEN";
			public const string Expired = "EXP";
			public const string Error = "FAL";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Descriptions
		{
			public static MultilingualString Deactivated => ResString.GetMultilingualString("PasswordStatusList|INA", "Deactivated");
			public static MultilingualString PasswordOK => ResString.GetMultilingualString("PasswordStatusList|OK", "Password Valid");
			public static MultilingualString Invalid => ResString.GetMultilingualString("PasswordStatusList|Invalid", "Invalid");
			public static MultilingualString Valid => ResString.GetMultilingualString("PasswordStatusList|Valid", "Valid");
			public static MultilingualString Pending => ResString.GetMultilingualString("PasswordStatusList|Pending", "Pending");
			public static MultilingualString Expired => ResString.GetMultilingualString("PasswordStatusList|Expired", "Expired");
			public static MultilingualString Error => ResString.GetMultilingualString("PasswordStatusList|Error", "Error");
		}

		public static CodeDescriptionPairList GetFullList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PasswordStatusList_Full", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.PasswordOK, Descriptions.PasswordOK);
				result.AddPair(Codes.Deactivated, Descriptions.Deactivated);
				result.AddPair(Codes.Invalid, Descriptions.Invalid);
				result.AddPair(Codes.Valid, Descriptions.Valid);
				result.AddPair(Codes.Pending, Descriptions.Pending);
				result.AddPair(Codes.Expired, Descriptions.Expired);
				result.AddPair(Codes.Error, Descriptions.Error);
				return result;
			});
		}
	}
}
