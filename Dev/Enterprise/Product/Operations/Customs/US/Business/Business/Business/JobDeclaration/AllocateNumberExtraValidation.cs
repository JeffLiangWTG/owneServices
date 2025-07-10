using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	public class AllocateNumberExtraValidation
	{
		public AllocateNumberExtraValidation(AllocateNumberArgs args)
		{
			this.args = args;
		}

		readonly AllocateNumberArgs args;

		public static class Constants
		{
			public const string NumberLength = "Number must be {0} digits{1}";
			public const string InvalidCheckDigit = "Check digit is invalid - should be {0}";

			public static class FormalEntryNumber
			{
				public const string CheckDigitIncluded = " including the last check digit.";
				public const string AlreadyExists = "This Entry Number is already allocated to an existing job.";
			}

			public static class ProtestCBPNUmber
			{
				public const string ShouldNotBeEmpty = "CBP Number is mandatory and should not be empty.";
			}
		}

		public void ValidateFormalEntryNumber(ZPropertyInfo numberInfo)
		{
			var value = (ZString)numberInfo.Value;

			if (!value.IsEmpty)
			{
				var isDuplicate = false;
				if (value.Length < 7 || value.Length > 8)
				{
					numberInfo.AddError(string.Format(Constants.NumberLength, 8, Constants.FormalEntryNumber.CheckDigitIncluded));
				}
				else
				{
					var entryWithNoCheckDigit = value.SubstringSafe(0, 7);
					var checkDigit = EntryNumberCheckDigitCalculator.GetCheckDigit(args.EntryFilerCode, entryWithNoCheckDigit, EntryNumberValidator.GetCheckDigitAddition(args.Branch, args.EntryFilerCode, ZDecimal.ParseSafe(entryWithNoCheckDigit, 0)));
					if (value.Length == 7 || checkDigit != ZInt.ParseSafe(value.Right(1), -1))
					{
						numberInfo.AddError(string.Format(CultureInfo.CurrentCulture, Constants.InvalidCheckDigit, checkDigit));
					}
				}

				var cusEntryNums = CusEntryNumber.Load(args.Factory, CusEntryNumberTypes.UnitedStates.EntrySummary, value, Core.Constants.CountryCodes.UnitedStates);
				foreach (var entryNumber in cusEntryNums)
				{
					if (args.EntryFilerCode == entryNumber.GetEntryFilerCode())
					{
						isDuplicate = true;
						break;
					}
				}

				if (isDuplicate)
				{
					numberInfo.AddError(Constants.FormalEntryNumber.AlreadyExists);
				}
			}
		}

		public void ValidateProtestCBPAssignedNumber(ZPropertyInfo numberInfo)
		{
			var value = (ZString)numberInfo.Value;
			if (value.Length != 12)
			{
				numberInfo.AddError(string.Format(Constants.NumberLength, 12, "."));
			}

			if (value.IsEmpty)
			{
				numberInfo.AddError(Constants.ProtestCBPNUmber.ShouldNotBeEmpty);
			}
		}
	}
}
