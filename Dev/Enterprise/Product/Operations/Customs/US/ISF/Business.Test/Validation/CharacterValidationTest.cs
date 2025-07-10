using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CharacterValidationTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ZPropertyInfoString info = (ZPropertyInfoString)header.BF_MasterBillInfo;
			info.Value = "Z!Z";
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaNumeric);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyAlphaNumericCharactersAreAllowed);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyAlphabeticCharactersAreAllowed);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.Mix);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.NumericOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyNumericCharactersAreAllowed);
			info.Value = "Z1Z";
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaNumeric);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyAlphabeticCharactersAreAllowed);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.Mix);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.NumericOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyNumericCharactersAreAllowed);
			info.Value = "Z Z";
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaNumeric);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyAlphaNumericCharactersAreAllowed);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyAlphabeticCharactersAreAllowed);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.Mix);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.NumericOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyNumericCharactersAreAllowed);
			info.Value = "ZAZ";
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaNumeric);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaOnly);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.Mix);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.NumericOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyNumericCharactersAreAllowed);
			info.Value = "123";
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaNumeric);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaOnly);
			AssertHasMessageError(info, ValidationConstants.Character.OnlyAlphabeticCharactersAreAllowed);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.Mix);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.NumericOnly);
			AssertNoMessageErrors(info);
			info.Value = "";
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaNumeric);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.AlphaOnly);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.Mix);
			AssertNoMessageErrors(info);
			info.ClearAllNotifications();
			CharacterValidation.Validate(info, CharacterValidation.Type.NumericOnly);
			AssertNoMessageErrors(info);
			if (ErrorReporter.LastKeyReported == "Validation:BF_MasterBill")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
