using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class AllocateNumberExtraValidationTest : TestCaseWithFactory
	{
		public void TestValidateFormalEntry()
		{
			var args = new AllocateNumberArgs();
			args.MaxLength = 8;
			args.Factory = Factory;
			args.Branch = GlbBranch.CurrentBranch;
			args.EntryFilerCode = "XJ5";
			args.ValidateNumber = (info, validator) => validator.ValidateFormalEntryNumber(info);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "XXX";
			declaration.ImportEntryNumber = "12345";
			Factory.Save();

			var number = new AllocateNumber(args);
			number.AE_Number = "12345";
			Assert("Entry Filer code is different!. It is not duplicate", !number.AE_NumberInfo.HasError(AllocateNumberExtraValidation.Constants.FormalEntryNumber.AlreadyExists));

			var errorText = string.Format(AllocateNumberExtraValidation.Constants.NumberLength, 8, AllocateNumberExtraValidation.Constants.FormalEntryNumber.CheckDigitIncluded);
			AssertHasError(number.AE_NumberInfo, errorText);

			number.AE_Number = "6001128";
			AssertHasError(number.AE_NumberInfo, string.Format(AllocateNumberExtraValidation.Constants.InvalidCheckDigit, 1));

			number.AE_Number = "60011288";
			AssertHasError(number.AE_NumberInfo, string.Format(AllocateNumberExtraValidation.Constants.InvalidCheckDigit, 1));

			number.AE_Number = "60011281";
			AssertNoError(number.AE_NumberInfo, string.Format(AllocateNumberExtraValidation.Constants.InvalidCheckDigit, 1));
		}

		public void TestValidateFormalEntry_MultipleRanges()
		{
			var args = new AllocateNumberArgs();
			args.MaxLength = 8;
			args.Factory = Factory;
			args.Branch = GlbBranch.CurrentBranch;
			args.EntryFilerCode = "XXX";
			args.ValidateNumber = (info, validator) => validator.ValidateFormalEntryNumber(info);

			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XXX");
			var stmNum1 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 200, 299);
			stmNum1.CheckDigitAddition = 1;
			var stmNum2 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 300, 399);
			stmNum2.CheckDigitAddition = 2;

			var number = new AllocateNumber(args);
			number.AE_Number = "0000200";
			AssertHasError(number.AE_NumberInfo, string.Format(AllocateNumberExtraValidation.Constants.InvalidCheckDigit, 7));

			number.AE_Number = "0000300";
			AssertHasError(number.AE_NumberInfo, string.Format(AllocateNumberExtraValidation.Constants.InvalidCheckDigit, 5));
		}

		public void TestValidateProtestCBPAssignedNumber()
		{
			var args = new AllocateNumberArgs();
			args.MaxLength = 12;
			args.Factory = Factory;
			args.ValidateNumber = (info, validator) => validator.ValidateProtestCBPAssignedNumber(info);

			var number = new AllocateNumber(args);
			number.AE_Number = "12345";

			var errorText = string.Format(AllocateNumberExtraValidation.Constants.NumberLength, 12, ".");
			AssertHasError(number.AE_NumberInfo, errorText);

			number.AE_Number = "123456783652";
			AssertNoError(number.AE_NumberInfo, errorText);

			number.AE_Number = ZString.Empty;
			AssertHasError(number.AE_NumberInfo, AllocateNumberExtraValidation.Constants.ProtestCBPNUmber.ShouldNotBeEmpty);
			number.AE_Number = "123456783652";
			AssertNoError(number.AE_NumberInfo, AllocateNumberExtraValidation.Constants.ProtestCBPNUmber.ShouldNotBeEmpty);
		}
	}
}
