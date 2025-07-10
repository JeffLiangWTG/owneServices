using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class EntryNumberValidator
	{
		public static void ValidateFormatAndCheckDigit(ZPropertyInfo entryNumberInfo, GlbBranch branch, string errorMessage = "", bool showAsWarning = false)
		{
			var entryNumber = (ZString)entryNumberInfo.Value;
			if (!entryNumber.IsEmpty)
			{
				if (!Regex.IsMatch(entryNumber.ToString(), @"^[A-Z0-9]{3}[0-9]{8}$", RegexOptions.IgnoreCase))
				{
					entryNumberInfo.AddNotification(showAsWarning ? NotificationType.Warning : NotificationType.MessageError, EntryNumberFormat);
				}
				else
				{
					if (branch == null)
					{
						branch = GlbBranch.CurrentBranch;
					}
					var filerCode = entryNumber.Left(3);
					var entryWithNoCheckDigit = entryNumber.SubstringSafe(3, 7);
					var calculatedCheckDigit = EntryNumberCheckDigitCalculator.GetCheckDigit(filerCode, entryWithNoCheckDigit, GetCheckDigitAddition(branch, filerCode, ZDecimal.ParseSafe(entryWithNoCheckDigit, 0)));
					var enteredCheckDigit = ZInt.ParseSafe(entryNumber.Right(1), -1);

					if (calculatedCheckDigit != enteredCheckDigit)
					{
						if (!string.IsNullOrEmpty(errorMessage))
						{
							entryNumberInfo.AddWarning(errorMessage + calculatedCheckDigit.ToString(CultureInfo.CurrentCulture));
						}
						else
						{
							if (UsingCompanyFilerCode(branch, filerCode))
							{
								entryNumberInfo.AddNotification(showAsWarning ? NotificationType.Warning : NotificationType.MessageError, InvalidCheckDigit + calculatedCheckDigit.ToString(CultureInfo.CurrentCulture));
							}
							else
							{
								entryNumberInfo.AddWarning(InvalidCheckDigitOtherFiler + calculatedCheckDigit.ToString(CultureInfo.CurrentCulture));
							}
						}
					}
				}
			}
		}

		public static int GetCheckDigitAddition(GlbBranch branch, ZString filerCode, ZDecimal entryNumber)
		{
			var setting = ACEEntryStmNumsSetting.New(branch, filerCode);
			return setting?.GetMatchingSequenceWrapper(branch, entryNumber)?.CheckDigitAddition ?? 0;
		}

		static bool UsingCompanyFilerCode(GlbBranch branch, string filerCode)
		{
			var currentCompanyEntryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetValueWithoutFallback(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
			return filerCode == currentCompanyEntryFilerCode;
		}

		public const string EntryNumberFormat = "Format should be 3 alpha-numerics (Filer Code) followed by 8 numerics (entry number and check digit).";
		public const string InvalidCheckDigit = "The Filer Code / Entry No. check digit is invalid. It should be ";
		public const string InvalidCheckDigitOtherFiler = "Check digit verification for Filer Code / Entry No. is not completely possible for 'Other Filer' values.\r\nFrom the information available, the check digit should be ";
	}
}
