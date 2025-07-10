using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class IdentityNumberAndLPCODetailsValidator
	{
		public static void ValidateAdditionalIdentityNumQualifier(ZPropertyInfo propertyInfo, NHTSAHeader header, CodeDescriptionPairList numberTypes)
		{
			ListValidation.MessageErrorIfInvalidCode(propertyInfo, numberTypes);

			var isACECargoReleaseValidationMode = header != null && header.IsPGAValidationOn;
			var isMotorVehicles = header != null && header.IsMotorVehicles;

			if (isACECargoReleaseValidationMode && isMotorVehicles)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		public static void ValidateAdditionalIdentityNumber(ZPropertyInfo propertyInfo, ZString number, ZString numQualifier, bool isVIN, ZString boxNumber)
		{
			if (number.IsEmpty && !numQualifier.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(propertyInfo);
			}
			else if (isVIN && number.Length != 17)
			{
				if (boxNumber == DepartmentOfTransportBoxNumberList.Codes._01)
				{
					propertyInfo.AddWarning(ValidationConstants.NHTSA.InvalidAdditionalNumberFormat);
				}
				else
				{
					propertyInfo.AddMessageError(ValidationConstants.NHTSA.InvalidAdditionalNumberFormat);
				}
			}
		}

		public static void ValidateLPCOType(ZPropertyInfo propertyInfo, NHTSALPCOTypeList lPCOTypes, ZString lpcoNumber, ZString lpcoDateType, ZDateTime lpcoDate, ZDecimal qTY)
		{
			ListValidation.MessageErrorIfInvalidCode(propertyInfo, lPCOTypes);

			if (!lpcoNumber.IsEmpty || !lpcoDateType.IsEmpty || !lpcoDate.IsEmpty || !qTY.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(propertyInfo);
			}
		}

		public static void ValidateLPCODate(ZPropertyInfo propertyInfo, ZString lpcoDateType)
		{
			if (!lpcoDateType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		public static void ValidateLPCODateType(ZPropertyInfo propertyInfo, ZDateTime lpcoDate)
		{
			if (!lpcoDate.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		public static void ValidateLPCONumber(ZPropertyInfo propertyInfo, bool isACECargoReleaseMode, ZString lpcoType, bool isRegImporterNo, bool isImportPermissionLetter, bool isVehicleEligibilityNumber)
		{
			var value = (ZString)propertyInfo.Value;

			if (isACECargoReleaseMode && !value.IsEmpty)
			{
				if (lpcoType == NHTSALPCOTypeList.Codes.NH0 && !isRegImporterNo)
				{
					propertyInfo.AddMessageError(ValidationConstants.NHTSA.InvalidLPCONumberFormatForRegisteredImporterNumber);
				}
				else if (lpcoType == NHTSALPCOTypeList.Codes.NH2 && !isImportPermissionLetter)
				{
					propertyInfo.AddMessageError(ValidationConstants.NHTSA.InvalidLPCONumberFormatForNHTSAImportPermissionLetterr);
				}
				else if (lpcoType == NHTSALPCOTypeList.Codes.NH3 && !isVehicleEligibilityNumber)
				{
					propertyInfo.AddMessageError(ValidationConstants.NHTSA.InvalidLPCONumberFormatForVehicleEligbilityNumber);
				}
			}

			if (!lpcoType.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(propertyInfo);
			}
		}
	}
}
