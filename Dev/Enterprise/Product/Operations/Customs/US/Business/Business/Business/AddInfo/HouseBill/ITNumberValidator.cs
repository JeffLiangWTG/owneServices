using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using INotificationType = CargoWise.ComponentModel.INotificationType;

namespace Enterprise.Customs.US.Business
{
	public static class ITNumberValidator
	{
		#region Constants

		public static class Constants
		{
			public static class ITNumber
			{
				public const string Invalid = "Invalid IT Number. IT Number should be in the following format: 9 numerics including a valid check digit.";
				public const string InvalidForAir = "Invalid IT Number. IT Number should be in the following format: 9 numerics including a valid check digit or 11 numerics (when transport mode is Air).";
				public const string ITnumberIsRequiredWhenITDateIsEntered = "An IT number must be entered when an IT date is supplied.";
				public const string ITNumberIsRequiredWhenEntryAndDischargeDistrictsDiffer = "An IT Number must be entered when Port of Discharge is not in the same district as Port of Entry.";
				public const string ITNumberIsRequiredWhenEntryAndDischargePortDifferBCR = "An IT Number must be entered when Port of Discharge is different to Port of Entry for Border Crossing.";
				public const string ITNoShouldBeEnteredatLowestBill = "An IT Number should be entered at the lowest Bill level only.";
				public const string ITNoShouldBeEnteredatMasterLevel = "An IT Number should be entered at the Master level.";
				public const string ITNoShouldBeEnteredForAllLowestBills = "You should enter an IT number here because you have entered IT numbers against another lowest bill.";
				public const string DuplicateNumber = "You have entered duplicate IT Number for this bill.";
			}

			public static class PreviousEntryNumber
			{
				public const string Invalid = "Invalid Number: Previous In-Bond Number should be in the following format: 9 digits including a valid check digit (IT Number) or 11 numeric (Master AWB – when transport mode is Air).";
			}
		}

		#endregion

		public static void ValidateITNumberFormat(ZPropertyInfo itNumberInfo, ZBool isAIR, bool showAsWarning = false)
		{
			var result = ValidateITFormat(itNumberInfo, isAIR, false);

			if (!string.IsNullOrEmpty(result.Item1))
			{
				itNumberInfo.AddNotification(showAsWarning ? NotificationType.Warning : result.Item2, result.Item1);
			}
		}

		public static void ValidateITNumber(ZPropertyInfo propertyInfo, Bill bill)
		{
			if (bill != null && bill.Declaration != null)
			{
				JobDeclaration declaration = bill.Declaration;
				ZString itNumber = (ZString)propertyInfo.Value;
				if (!itNumber.IsEmpty)
				{
					if (!bill.IsLowestBill)
					{
						if (declaration.IsAir || declaration.IsSea)
						{
							propertyInfo.AddError(Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
						}
						else
						{
							propertyInfo.AddError(Constants.ITNumber.ITNoShouldBeEnteredatMasterLevel);
						}
					}
					else if (itNumber != JobDeclaration.Constants.Multiple)
					{
						ValidateITNumberFormat(propertyInfo, declaration.IsAir);
					}
					CheckDuplicateNumbersForThisBill(propertyInfo, bill, itNumber);
				}
				else
				{
					bool shouldValidateITNoAgainstAllBills = bill.IsLowestBill && declaration.ITNumbersFromBills.Count > 0;
					if (shouldValidateITNoAgainstAllBills)
					{
						propertyInfo.AddMessageError(Constants.ITNumber.ITNoShouldBeEnteredForAllLowestBills);
					}
				}

				ValidateITNumbersOnPrimaryBill(bill);
			}
		}

		static void CheckDuplicateNumbersForThisBill(ZPropertyInfo propertyInfo, Bill bill, ZString itNumber)
		{
			if (bill.ITAndSplitDetails.FindByItNumber(itNumber).Length > 1)
			{
				propertyInfo.AddMessageError(Constants.ITNumber.DuplicateNumber);
			}
		}

		public static void ValidateITNumbersOnPrimaryBill(Bill bill)
		{
			if (bill != null && bill.ParentBill != null && !bill.ParentBill.NoITNumbersExist)
			{
				foreach (ITAndSplitDetails itNo in bill.ParentBill.ITAndSplitDetails)
				{
					itNo.AddInfoValidation.ValidateUS_ITNumber();
				}
			}
		}

		public static void ValidatePreviousITNumberFormat(ZPropertyInfo itNumberInfo)
		{
			var result = ValidateITFormat(itNumberInfo, true, true);

			if (!string.IsNullOrEmpty(result.Item1))
			{
				itNumberInfo.AddNotification(result.Item2, result.Item1);
			}
		}

		static Tuple<string, INotificationType> ValidateITFormat(ZPropertyInfo itNumberInfo, ZBool isAIR, ZBool isPreviousEntryNo)
		{
			var result = new Tuple<string, INotificationType>(string.Empty, NotificationType.Warning);
			ZString itNumber = (ZString)itNumberInfo.Value;
			ZString firstChar = ZString.Empty;

			if (!itNumber.IsEmpty)
			{
				firstChar = itNumber.SubstringSafe(0, 1).ToUpper();

				if (firstChar == "V")
				{
					if (!Regex.IsMatch(itNumber, @"^V[A-Z0-9]{2}[0-9]{8}$", RegexOptions.IgnoreCase))
					{
						result = new Tuple<string, INotificationType>(ValidationConstants.AllocateInBondNumber.InvalidPaperless, NotificationType.MessageError);
					}
					else
					{
						int actualCheckDigit = Convert.ToInt32(itNumber.SubstringSafe(10, 1));
						int expectedCheckDigit = InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit(itNumber);

						if (actualCheckDigit != expectedCheckDigit)
						{
							result = new Tuple<string, INotificationType>(ValidationConstants.AllocateInBondNumber.InvalidCheckDigit(expectedCheckDigit.ToString()), NotificationType.MessageError);
						}
					}
				}
				else
				{
					if (isPreviousEntryNo && itNumber.KeepAlphanumericCharacters().Length == 11)
					{
						// valid entry number;
					}
					else if (!isAIR || itNumber.KeepNumericCharacters().Length != 11)
					{
						if (itNumber.Length == 9 && itNumber.IsNumbersOnlyOrEmpty)
						{
							ZString actualCheckDigit = itNumber.Right(1);
							ZString expectedCheckDigit = InBondNumberCheckDigitCalculator.GetCheckDigit(itNumber.Left(8));
							if (actualCheckDigit != expectedCheckDigit)
							{
								result = new Tuple<string, INotificationType>(ValidationConstants.AllocateInBondNumber.InvalidCheckDigit(expectedCheckDigit), NotificationType.Warning);
							}
						}
						else
						{
							result = new Tuple<string, INotificationType>(isAIR ? Constants.ITNumber.InvalidForAir : Constants.ITNumber.Invalid, NotificationType.MessageError);
						}
					}
				}
			}

			if (isPreviousEntryNo && !string.IsNullOrEmpty(result.Item1) && firstChar != "V" && !result.Item1.StartsWith("Invalid check digit", StringComparison.OrdinalIgnoreCase))
			{
				result = new Tuple<string, INotificationType>(Constants.PreviousEntryNumber.Invalid, NotificationType.MessageError);
			}

			return result;
		}
	}
}
