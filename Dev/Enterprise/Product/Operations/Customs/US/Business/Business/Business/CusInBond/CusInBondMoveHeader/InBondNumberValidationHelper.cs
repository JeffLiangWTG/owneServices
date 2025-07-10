using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class InBondNumberValidationHelper
	{
		public static void ValidateInBondNumber(BusinessObjectFactory factory, ZString inBondNumber, ZPropertyInfo inBondNumberInfo, GlbBranch branch, bool isPostDepartureMessageOnly, bool isPaperlessNumber, int conventionalInBondNumberMaxLength, int postDepartureMessageInbondNumberMaxLength, bool isBulkSendArrival = false)
		{
			if (inBondNumber.IsEmpty)
			{
				if (isPostDepartureMessageOnly)
				{
					inBondNumberInfo.AddError(Res.GetString("09B069B2-2979-472D-BE88-E8D34FC6FBB5", "In-Bond Number cannot be empty."));
				}
				else
				{
					ZString errorMessage = InBondNumberAvailabilityChecker.Check(branch);
					if (!errorMessage.IsEmpty)
					{
						inBondNumberInfo.AddError(errorMessage);
					}
				}
			}
			else if (isPaperlessNumber)
			{
				if (!Regex.IsMatch(inBondNumber, @"^V[A-Z0-9]{2}[0-9]{8}$", RegexOptions.IgnoreCase))
				{
					inBondNumberInfo.AddError(ValidationConstants.AllocateInBondNumber.InvalidPaperless);
				}
				else
				{
					int actualCheckDigit = Convert.ToInt32(inBondNumber.SubstringSafe(10, 1));
					int expectedCheckDigit = InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit(inBondNumber);

					if (actualCheckDigit != expectedCheckDigit)
					{
						inBondNumberInfo.AddError(ValidationConstants.AllocateInBondNumber.InvalidCheckDigit(expectedCheckDigit.ToString()));
					}
				}
			}
			else
			{
				if (!inBondNumber.IsNumbersOnlyOrEmpty)
				{
					inBondNumberInfo.AddError(ValidationConstants.AllocateInBondNumber.MustBeNumeric);
				}
				else if (!isPostDepartureMessageOnly && inBondNumber.Length != conventionalInBondNumberMaxLength)
				{
					inBondNumberInfo.AddError(ValidationConstants.AllocateInBondNumber.LengthShouldBeNine);
				}
				else if (isPostDepartureMessageOnly && inBondNumber.Length != postDepartureMessageInbondNumberMaxLength && inBondNumber.Length != conventionalInBondNumberMaxLength)
				{
					inBondNumberInfo.AddError(ValidationConstants.AllocateInBondNumber.InBondNumberLengthShouldBeNineOrEleven);
				}
				else
				{
					var actualCheckDigit = inBondNumber.SubstringSafe(8, 1);
					var expectedCheckDigit = InBondNumberCheckDigitCalculator.GetCheckDigit(inBondNumber.Left(8));
					var timeIn3Years = ZDateTime.UtcNow.AddYears(-InBondNumberSetting.ExpirationYear);
					if (!isPostDepartureMessageOnly && actualCheckDigit != expectedCheckDigit)
					{
						inBondNumberInfo.AddError(ValidationConstants.AllocateInBondNumber.InvalidCheckDigit(expectedCheckDigit));
					}
					else if (CusEntryNumber.Load(factory, CusEntryHeaderMessageTypeList.Codes.InBond, inBondNumber, Core.Constants.CountryCodes.UnitedStates).
						OrderByDescending(x => x.CE_SystemCreateTimeUtc).Take(2).Count(x => x.CE_SystemCreateTimeUtc > timeIn3Years) > (isBulkSendArrival ? 1 : 0))
					{
						inBondNumberInfo.AddError(ValidationConstants.AllocateInBondNumber.AlreadyExists);
					}
				}
			}
		}
	}
}
