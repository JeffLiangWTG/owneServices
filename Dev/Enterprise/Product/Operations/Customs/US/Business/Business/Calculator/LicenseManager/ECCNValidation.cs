using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.LicenseManager
{
	public static class ECCNValidation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006", Justification = "Type IEnumerable is necessary")]
		public static IEnumerable<Func<ZString, ZString, string>> GetECCNValidate(BusinessObjectFactory factory, string countryCode, ZDateTime exportDate, string destinationCountry)
		{
			yield return ValidateT12ECCN;
			yield return (licenseType, eccn) => ValidateECCNIsRequiredForSpecialCountry(factory, licenseType, eccn, countryCode);
			yield return (licenseType, eccn) => ValidateECCNIsRequiredOrNotAllowed(factory, licenseType, eccn, exportDate);
			yield return (licenseType, eccn) => ValidateECCNIsInAllowedList(factory, licenseType, eccn);
			yield return ValidateECCNIsInNotAllowedList;
			yield return ValidateECCNFormat;
			yield return Validate600SeriesECCN;
			yield return (licenseType, eccn) => Validate600SeriesDotYECCN(factory, licenseType, eccn, destinationCountry);
			yield return ValidateAdditional600SeriesECCNs;
			yield return ValidateAdditional515ECCNs;
		}

		#region ValidateECCNIsRequiredOrNotAllowed

		static string ValidateECCNIsRequiredOrNotAllowed(BusinessObjectFactory factory, ZString licenseType, ZString eccn, ZDateTime exportDate)
		{
			var result = "";
			var eCCNRequiredValue = ECCNRequiredValue(factory, licenseType, exportDate);
			if (eccn.IsEmpty && eCCNRequiredValue.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Mandatory))
			{
				result = string.Format(CultureInfo.InvariantCulture, ECCNIsRequiredMessage, licenseType);
			}
			else if (!eccn.IsEmpty && eCCNRequiredValue.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.NotAllowed))
			{
				result = string.Format(ECCNNotAllowedMessage, licenseType);
			}

			return result;
		}

		internal static ZString ECCNRequiredValue(BusinessObjectFactory factory, ZString licenseType, ZDateTime exportDate)
		{
			var result = ZString.Empty;
			if (exportDate.IsValid)
			{
				var eCCNRequired = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, exportDate,
								 Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, licenseType, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ECCNRequired).FirstOrDefault();
				if (eCCNRequired != null)
				{
					result = eCCNRequired.ZZE_Value;
				}
			}
			return result;
		}

		internal const string ECCNIsRequiredMessage = "ECCN is required when License Type is '{0}'.";
		internal const string ECCNNotAllowedMessage = "ECCN is not allowed when License Type is '{0}'.";

		#endregion

		#region ValidateECCNIsRequiredForSpecialCountry

		static string ValidateECCNIsRequiredForSpecialCountry(BusinessObjectFactory factory, ZString licenseType, ZString eccn, ZString countryCode)
		{
			var result = ZString.Empty;

			if (SpecialCountry.Contains(countryCode))
			{
				if (eccn.IsEmpty)
				{
					result = ECCNIsRequiredMessageForSpecialCountry;
				}
				else
				{
					var eccnAttributeMEU = HasECCNAttribute(factory, eccn, RefCusCodeListAttributeTypes.Codes.MEU);
					var hasBISLicenseType = HasLicenseTypeBIS(factory, licenseType, RefCusCodeListAttributeTypes.Codes.LicenseTypeCodes, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.BureauOfIndustryAndSecurity);
					if (eccnAttributeMEU && !hasBISLicenseType)
					{
						result = LicenseTypeMustGOVorBISWhenECCNHasMEUAttribute;
					}
				}
			}

			return result;
		}

		static bool HasLicenseTypeBIS(BusinessObjectFactory factory, ZString licenseType, ZString attributeName, ZString attributeValue)
		=> ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, licenseType, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, ZDateTime.Today)
				?.HasAttribute(attributeName, attributeValue) ?? false;

		static bool HasECCNAttribute(BusinessObjectFactory factory, ZString eccnNumber, ZString attributeName)
		=> ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, eccnNumber, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, ZDateTime.Today)
				?.HasAttribute(attributeName, eccnNumber) ?? false;

		static ZString[] SpecialCountry => new ZString[] { Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.Russia, Core.Constants.CountryCodes.Venezuela };

		internal const string LicenseTypeMustGOVorBISWhenECCNHasMEUAttribute = "Commodities with this ECCN are subject to licensing requirements and cannot be exported without a BIS license unless eligible for License Exception GOV";
		internal const string ECCNIsRequiredMessageForSpecialCountry = "ECCN number is required when either the Ultimate Consignee, Intermediate Consignee or Ultimate Destination Country are China, Russia or Venezuela.";

		#endregion

		#region ValidateECCNIsInAllowedList

		static string ValidateECCNIsInAllowedList(BusinessObjectFactory factory, ZString licenseType, ZString eccn)
		{
			var result = ZString.Empty;
			if (!licenseType.IsEmpty || !eccn.IsEmpty)
			{
				var availableECCNs = LicenseValidationHelper.GetECCNNumbersByLicenseType(factory, licenseType);
				if (availableECCNs.Count > 0 && !availableECCNs.ContainsCode(eccn))
				{
					result = ZString.Format(CodeIsInvalid, eccn, licenseType);
				}
			}

			return result;
		}
		internal const string CodeIsInvalid = "The ECCN number {0} is invalid for License Type {1}.";

		#endregion

		#region ValidateECCNFormat

		static string ValidateECCNFormat(ZString licenseType, ZString eccn)
		{
			return !eccn.IsEmpty && eccn != "EAR99" && !Regex.IsMatch(eccn, @"^[0-9]{1}[A-Z]{1}[0-9]{3}$", RegexOptions.IgnoreCase) ? ECCNIsInvalidFormat : string.Empty;
		}
		internal const string ECCNIsInvalidFormat = "ECCN must be formatted NANNN.";

		#endregion

		#region ValidateECCNIsInNotAllowedList

		static string ValidateECCNIsInNotAllowedList(ZString licenseType, ZString eccn)
		{
			if (!eccn.IsEmpty)
			{
				var notAllowedList = GetECCNNotAllowedList(licenseType);
				if (notAllowedList.Length > 0 && notAllowedList.Contains(eccn.ToString()))
				{
					return string.Format(CultureInfo.CurrentCulture, ECCNNotAllowed, eccn, licenseType);
				}
			}
			return string.Empty;
		}
		internal const string ECCNNotAllowed = "ECCN {0} is not allowed for License Type {1}";

		static string[] GetECCNNotAllowedList(ZString licenseType)
		{
			switch (licenseType)
			{
				case USAESLicenseCode.Codes.C32:
					var result = new[] { "1C351", "1C352", "1C353", "1C354" };
					return result;
				case USAESLicenseCode.Codes.C33:
					return new[] { "1C351", "1C352", "1C353", "1C354" };
				default:
					return Array.Empty<string>();
			}
		}

		#endregion

		static string Validate600SeriesECCN(ZString licenseType, ZString eccn)
		{
			var is600SeriesECCN = eccn.Length > 3 && eccn[2] == '6';
			if (is600SeriesECCN)
			{
				switch (licenseType)
				{
					case USAESLicenseCode.Codes.C43:
					case USAESLicenseCode.Codes.C46:
					case USAESLicenseCode.Codes.C51:
					case USAESLicenseCode.Codes.C57:
						return ECCN600SeriesNotEligible;
					case USAESLicenseCode.Codes.C45:
						var allowedECCN600SeriesForC45 = new ZString[] { "0A602", "0B602", "0D602", "0E602", "1A613" };
						if (!allowedECCN600SeriesForC45.Contains(eccn))
						{
							return ECCN600SeriesNotEligible;
						}
						return string.Empty;
				}
			}
			return string.Empty;
		}
		internal const string ECCN600SeriesNotEligible = "Items under 600 series ECCNs are not eligible under this license type.";

		static string Validate600SeriesDotYECCN(BusinessObjectFactory factory, ZString licenseType, ZString eccn, string destinationCountry)
		{
			if (destinationCountry != null && USAESLicenseCode.Is600SeriesDotYECCN(licenseType) && !eccn.IsEmpty && IsECCN600SeriesDotYNotEligible(factory, eccn, RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, destinationCountry))
			{
				return ECCN600SeriesDotYNotEligible;
			}
			return string.Empty;
		}

		static bool IsECCN600SeriesDotYNotEligible(BusinessObjectFactory factory, ZString eccnNumber, ZString attributeName, ZString destinationCountry)
		{
			var result = false;
			var eccnCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, eccnNumber, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, ZDateTime.Today);
			if (eccnCode != null && eccnCode.GetAttributesValues(attributeName).Any())
			{
				result = eccnCode.HasAttribute(attributeName, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.NotEligible) || destinationCountry.IsEmpty || !eccnCode.HasAttribute(attributeName, destinationCountry);
			}
			return result;
		}

		internal const string ECCN600SeriesDotYNotEligible = "For items under 600 series ECCNs in a .y paragraph, use C60(DY6), if no license is required for export";

		static string ValidateAdditional600SeriesECCNs(ZString licenseType, ZString eccn)
		{
			var isAdditional600SeriesECCN = additional600SeriesECCNs.Contains(eccn.ToString());
			if (isAdditional600SeriesECCN)
			{
				switch (licenseType)
				{
					case USAESLicenseCode.Codes.C30:
					case USAESLicenseCode.Codes.C31:
					case USAESLicenseCode.Codes.C32:
					case USAESLicenseCode.Codes.C35:
					case USAESLicenseCode.Codes.C40:
					case USAESLicenseCode.Codes.C41:
					case USAESLicenseCode.Codes.C42:
					case USAESLicenseCode.Codes.C44:
					case USAESLicenseCode.Codes.C45:
					case USAESLicenseCode.Codes.C59:
						return string.Empty;
					case USAESLicenseCode.Codes.C60:
						if (eccn == "1A613" || eccn == "1D613" || eccn == "1E613")
						{
							return string.Empty;
						}
						return Additional600ECCNSNotAllowed;
					default:
						return Additional600ECCNSNotAllowed;
				}
			}
			return string.Empty;
		}
		static readonly ImmutableArray<string> additional600SeriesECCNs = ImmutableArray.Create("0A604", "0A614", "0B604", "0B614", "0D604", "0D614", "0E604", "0E614", "1A613", "1B608", "1B613", "1C608", "1D608", "1D613", "1E608", "1E613", "9A604", "9B604", "9D604", "9E604",
			"0A602", "0B602", "0D602", "0E602");
		internal const string Additional600ECCNSNotAllowed = @"ECCNs 0A604, 0A614, 0B604, 0B614, 0D604, 0D614, 0E604, 0E614, 1A613, 1B608, 1B613, 1C608, 1D608, 1D613, 1E608, 1E613, 9A604, 9B604, 9D604, 9E604, 0A602, 0B602, 0D602 and 0E602
are not allowed for License Types other than C30, C31, C32, C35, C40, C41, C42, C44, C45, C59 and C60(only ECCNs 1A613, 1D613, 1E613).";

		static string ValidateAdditional515ECCNs(ZString licenseType, ZString eccn)
		{
			var isAdditional515ECCN = addtional515ECCNs.Contains(eccn.ToString());
			if (isAdditional515ECCN)
			{
				switch (licenseType)
				{
					case USAESLicenseCode.Codes.C30:
					case USAESLicenseCode.Codes.C31:
					case USAESLicenseCode.Codes.C32:
					case USAESLicenseCode.Codes.C35:
					case USAESLicenseCode.Codes.C40:
					case USAESLicenseCode.Codes.C41:
					case USAESLicenseCode.Codes.C42:
					case USAESLicenseCode.Codes.C44:
					case USAESLicenseCode.Codes.C45:
					case USAESLicenseCode.Codes.C46:
					case USAESLicenseCode.Codes.C59:
					case USAESLicenseCode.Codes.C60:
						return string.Empty;
					default:
						return Additional515ECCNSNotAllowed;
				}
			}
			return string.Empty;
		}
		static readonly ImmutableArray<string> addtional515ECCNs = ImmutableArray.Create("9A515", "9D515", "9E515");
		internal const string Additional515ECCNSNotAllowed = @"ECCNs 9A515, 9D515, and 9E515 are not allowed for License Types other than C30, C31, C32, C35, C40, C41, C42, C44, C45, C46, C59 and C60.";

		static string ValidateT12ECCN(ZString licenseType, ZString eccn)
		{
			if (licenseType == USAESLicenseCode.Codes.T12 && !eccn.IsEmpty && eccn != "EAR99")
			{
				return T12ECCNIsInvalidMessage;
			}
			return string.Empty;
		}
		internal const string T12ECCNIsInvalidMessage = "ECCN entered is invalid; valid ECCN is EAR99 or blank only.";
	}
}
