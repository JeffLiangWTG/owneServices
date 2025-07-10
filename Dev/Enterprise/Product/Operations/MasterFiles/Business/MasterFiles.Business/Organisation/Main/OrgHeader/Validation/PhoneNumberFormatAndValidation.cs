using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneNumberFormatAndValidation : ValidationProvider
	{
		#region Constants

		public const char PlusPhoneNumberPrefix = '+';
		public const string ValidPhoneNumberChars = "0123456789+() -";
		public const string PhoneNumberDelimitersWithSpace = "() -";
		public const string PhoneNumberDelimiters = "()-";
		public const char AreaCodeBeginDelimiter = '(';
		public const char AreaCodeEndDelimiter = ')';
		public const char CountryAreaSpacer = ' ';
		public const char AreaLocalSpacer = ' ';
		public const char LocalSeparator = '-';
		public const char PrefixSeparator = ',';
		public const int LocalNumberGroupingSize = 4;
		public const int SpecialNumberGroupingSize = 3;
		public const int MobileNumberGroupingSize = 3;
		public const int AUMobileAreaCodeLength = 3;
		public const int NZMobileAreaCodeLength = 2;
		public const int MonacoMobileAreaCodeLength = 1;
		public const int SloveniaMobileAreaCodeLength = 1;
		public const int SpecialNumberAreaCodeLength = 0;

		public static string NumbersCanOnlyContainValidCharacters
		{
			get { return Res.GetString("5822a2ce-3532-4d35-9e9e-1e4f09992c54", "Phone/Fax/Mobile numbers can only contain number characters, +, -, (, ) and spaces."); }
		}

		public readonly string AUMobileStart = "614";
		public readonly string NZMobileStart = "642";
		public readonly string MonacoMobileStart = "3774";
		public readonly string SloveniaMobileStart = "3864";

		#endregion

		public PhoneNumberFormatAndValidation()
			: base()
		{
		}

		public PhoneNumberFormatAndValidation(INotificationType notificationLevel)
			: this()
		{
			this.NotificationLevel = notificationLevel;
		}

		#region Properties

		#region Factory

		BusinessObjectFactory fFactory;
		protected new BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		#endregion

		#region CurrentBranch

		protected GlbBranch fCurrentBranch;
		protected virtual GlbBranch CurrentBranch
		{
			get { return GlbBranch.CurrentBranch; }
		}

		#endregion

		#region LocalCountryISOCode

		ZString fLocalCountryISOCode = ZString.Empty;
		protected ZString LocalCountryISOCode
		{
			get
			{
				if (fLocalCountryISOCode.IsEmpty)
				{
					fLocalCountryISOCode = CurrentBranch.GB_RL_NKHomePort.Left(2);
				}
				return fLocalCountryISOCode;
			}
		}

		#endregion

		#region LocalCountryDialingCode

		ZString fLocalCountryDialingCode = ZString.Empty;
		protected ZString LocalCountryDialingCode
		{
			get
			{
				if (fLocalCountryDialingCode.IsEmpty)
				{
					fLocalCountryDialingCode = LocalCountryDataRow.CountryDialingCode;
				}
				return fLocalCountryDialingCode;
			}
		}

		#endregion

		#region LocalCountryInternationalPrefix

		protected ZString LocalCountryInternationalPrefix
		{
			get { return LocalCountryDataRow.InternationalPrefix; }
		}

		#endregion

		#region LocalCountryNationalPrefix

		ZString fLocalCountryNationalPrefix = ZString.Empty;
		protected ZString LocalCountryNationalPrefix
		{
			get
			{
				if (fLocalCountryNationalPrefix.IsEmpty)
				{
					fLocalCountryNationalPrefix = LocalCountryDataRow.NationalPrefix;
				}
				return fLocalCountryNationalPrefix;
			}
		}

		#endregion

		#region LocalAreaCode

		ZString fLocalAreaCode = ZString.Empty;
		protected ZString LocalAreaCode
		{
			get
			{
				if (fLocalAreaCode.IsEmpty)
				{
					fLocalAreaCode = GetAreaCodeFromUNLOCO();
				}
				return fLocalAreaCode;
			}
		}

		#endregion

		#region  LocalCountryDataRow

		PhoneInfo fLocalCountryDataRow;
		protected PhoneInfo LocalCountryDataRow
		{
			get
			{
				if (fLocalCountryDataRow == null)
				{
					fLocalCountryDataRow = PhoneInfo.GetPhoneInfoByCountryIsoCode(LocalCountryISOCode);
				}

				return fLocalCountryDataRow;
			}
		}

		#endregion

		#region TargetAreaCode

		protected ZString TargetAreaCode
		{
			get
			{
				ZString fTargetAreaCode = GetAreaCodeFromUNLOCO();
				return !fTargetAreaCode.IsEmpty ? fTargetAreaCode : GetAreaCodeFromTargetISO();
			}
		}

		#endregion

		#region TargetCountryISOCode

		ZString fTargetCountryISOCode = ZString.Empty;
		protected ZString TargetCountryISOCode
		{
			get
			{
				if (TargetUNLOCO != null)
				{
					fTargetCountryISOCode = TargetUNLOCO.RL_RN_NKCountryCode;
				}
				else
				{
					PhoneInfo dialledCode = FindCountryRowFromPhoneNumber(PhNumber);
					fTargetCountryISOCode = (dialledCode != null) ? (ZString)dialledCode.CountryIsoCode : LocalCountryISOCode;
				}
				return fTargetCountryISOCode;
			}
		}

		#endregion

		#region TargetCountryDialingCode

		ZString fTargetCountryDialingCode = ZString.Empty;
		protected ZString TargetCountryDialingCode
		{
			get
			{
				if (fTargetCountryDialingCode.IsEmpty && TargetCountryDataRow != null)
				{
					fTargetCountryDialingCode = TargetCountryDataRow.CountryDialingCode;
				}
				return fTargetCountryDialingCode;
			}
		}

		#endregion

		#region TargetCountryNationalPrefix

		ZString fTargetCountryNationalPrefix = ZString.Empty;
		protected ZString TargetCountryNationalPrefix
		{
			get
			{
				if (fTargetCountryNationalPrefix.IsEmpty && TargetCountryDataRow != null)
				{
					fTargetCountryNationalPrefix = TargetCountryDataRow.NationalPrefix;
				}
				return fTargetCountryNationalPrefix;
			}
		}

		#endregion

		#region TargetCountryAreaCodeLength

		ZInt fTargetCountryMinAreaCodeLength = 0;
		protected ZInt TargetCountryMinAreaCodeLength
		{
			get
			{
				if (fTargetCountryMinAreaCodeLength == 0 && TargetCountryDataRow != null)
				{
					fTargetCountryMinAreaCodeLength = TargetCountryDataRow.MinAreaCodeLength;
				}
				return fTargetCountryMinAreaCodeLength;
			}
		}

		ZInt fTargetCountryMaxAreaCodeLength = 0;
		protected ZInt TargetCountryMaxAreaCodeLength
		{
			get
			{
				if (fTargetCountryMaxAreaCodeLength == 0 && TargetCountryDataRow != null)
				{
					fTargetCountryMaxAreaCodeLength = TargetCountryDataRow.MaxAreaCodeLength;
				}
				return fTargetCountryMaxAreaCodeLength;
			}
		}

		#endregion

		#region TargetCountryLocalNumberLength

		ZInt fTargetCountryLocalNumberLength = 0;
		protected ZInt TargetCountryLocalNumberLength
		{
			get
			{
				if (fTargetCountryLocalNumberLength == 0 && TargetCountryDataRow != null)
				{
					fTargetCountryLocalNumberLength = TargetCountryDataRow.LocalNumberLength;
				}
				return fTargetCountryLocalNumberLength;
			}
		}

		#endregion

		#region TargetCountrySpecialPrefixes

		ZString fTargetCountrySpecialPrefixes = ZString.Empty;
		protected ZString TargetCountrySpecialPrefixes
		{
			get
			{
				if (fTargetCountrySpecialPrefixes.IsEmpty && TargetCountryDataRow != null)
				{
					fTargetCountrySpecialPrefixes = fTargetCountryDataRow.SpecialPrefixes;
				}
				return fTargetCountrySpecialPrefixes;
			}
		}

		#endregion

		#region TargetCountryDataRow

		PhoneInfo fTargetCountryDataRow;
		protected PhoneInfo TargetCountryDataRow
		{
			get
			{
				if (fTargetCountryDataRow == null)
				{
					fTargetCountryDataRow = PhoneInfo.GetPhoneInfoByCountryIsoCode(TargetCountryISOCode);
				}

				return fTargetCountryDataRow;
			}
		}

		#endregion

		#region NominatedUserAreaCode

		protected ZString NominatedUserAreaCode
		{
			get { return ExtractAreaCode(RawPhoneNumber); }
		}

		#endregion

		#region PhNumber

		ZString fPhNumber;
		protected ZString PhNumber
		{
			get { return fPhNumber; }
			set { fPhNumber = value; }
		}

		#endregion

		#region RawPhoneNumber

		ZString fRawPhoneNumber;
		protected ZString RawPhoneNumber
		{
			get { return fRawPhoneNumber; }
			set { fRawPhoneNumber = value; }
		}

		bool targetCountryNationalPrefixWasStripped;

		#endregion

		#region TargetUNLOCO

		RefUNLOCO fTargetUNLOCO;
#if DEBUG
		internal
#else
		protected
#endif
 RefUNLOCO TargetUNLOCO
		{
			get { return fTargetUNLOCO; }
			set
			{
				fTargetUNLOCO = value;
				fTargetCountryDataRow = null;
				fTargetCountryMinAreaCodeLength = 0;
				fTargetCountryMaxAreaCodeLength = 0;
				fTargetCountryDialingCode = ZString.Empty;
				fTargetCountryISOCode = ZString.Empty;
				fTargetCountryLocalNumberLength = 0;
				fTargetCountryNationalPrefix = ZString.Empty;
				fTargetCountrySpecialPrefixes = ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Convert to International Format

		public ZString ConvertToInternationalFormattedPhoneNumber(ZString phoneNumber, RefUNLOCO unLoco)
		{
			targetCountryNationalPrefixWasStripped = false;
			if (phoneNumber.Length > 0 && ContainsOnlyValidCharacters(phoneNumber))
			{
				RawPhoneNumber = phoneNumber;
				TargetUNLOCO = unLoco;
				PhNumber = phoneNumber; // we need this line. Contry code in StripInvalidCharsFromNumber(...) gets determined by PhNumber
				PhNumber = StripInvalidCharsFromNumber(PhNumber);
				ConvertToInternationalPhoneNumber();
				ConvertToCorrectFormat();
				return PhNumber;
			}

			return phoneNumber;
		}

		ZString StripInvalidCharsFromNumber(ZString phoneNumber)
		{
			string delimiters = (TargetCountryDataRow != null && TargetCountryDataRow.DoNotFormatLocalNumber) ? PhoneNumberDelimiters : PhoneNumberDelimitersWithSpace;
			ZString number = phoneNumber.ExcludeChars(delimiters);
			return !number.IsEmpty ? number[0] + number.SubstringSafe(1).ExcludeChars("+") : "";
		}

		#region FindCountryRowFromPhoneNumber

		PhoneInfo FindCountryRowFromPhoneNumber(ZString phoneNumber)
		{
			PhoneInfo result = null;
			if (phoneNumber.Length > 1 && phoneNumber.Substring(0, 1).Equals("+"))
			{
				result = PhoneInfo.GetPhoneInfoByCountryDialingCode(phoneNumber.Substring(1, 1));
				if (result == null && phoneNumber.Length > 2)
				{
					result = PhoneInfo.GetPhoneInfoByCountryDialingCode(phoneNumber.Substring(1, 2));
				}
				if (result == null && phoneNumber.Length > 3)
				{
					result = PhoneInfo.GetPhoneInfoByCountryDialingCode(phoneNumber.Substring(1, 3));
				}
			}

			return result;
		}

		#endregion

		#region ConvertToInternationalPhoneNumber

		void ConvertToInternationalPhoneNumber()
		{
			if (CheckNoNationalPrefixSpecialCase())
			{
				PhNumber = TargetCountryNationalPrefix + PhNumber;
			}
			ConvertGeneralToInternationalPhoneNumber();
		}

		#endregion

		#region CheckNoNationalPrefixSpecialCase

		ZBool CheckNoNationalPrefixSpecialCase()
		{
			bool result = false;
			for (int areaCodeLength = TargetCountryMinAreaCodeLength; areaCodeLength <= TargetCountryMaxAreaCodeLength; areaCodeLength++)
			{
				result = result || PhNumber.Length == (areaCodeLength + TargetCountryLocalNumberLength);
			}
			return result;
		}

		#endregion

		#region ConvertGeneralToInternationalPhoneNumber

		void ConvertGeneralToInternationalPhoneNumber()
		{
			if (!PhNumber.StartsWith(PlusPhoneNumberPrefix.ToString()))
			{
				if (GetDisputedCountryAreaCodeByCountry(PhNumber) != string.Empty)
				{
					PhNumber = PlusPhoneNumberPrefix + PhNumber;
					return;
				}
				if (TargetCountryDialingCode.Length != 0 && PhNumber.StartsWith(TargetCountryDialingCode))
				{
					if (TargetCountryLocalNumberLength != 0 && PhNumber.Substring(TargetCountryDialingCode.Length).Length < TargetCountryLocalNumberLength && TargetCountrySpecialPrefixes.Length == 0)
					{
						PhNumber = TargetCountryDialingCode + PhNumber;
					}
					else if (TargetCountryDataRow.HasVariableLocalNumberLength && GetAreaCodeFromCountry(PhNumber) != string.Empty)
					{
						string areaCode = GetAreaCodeFromCountry(PhNumber);
						if (PhNumber.Substring(areaCode.Length).Length >= TargetCountryDataRow.MinLocalNumberLength && PhNumber.Substring(areaCode.Length).Length <= TargetCountryDataRow.MaxLocalNumberLength)
						{
							PhNumber = TargetCountryDialingCode + PhNumber;
						}
					}
				}
				else if (LocalCountryInternationalPrefix.Length != 0 && StartsWithPrefix(PhNumber, LocalCountryInternationalPrefix).Length > 0)
				{
					PhNumber = PhNumber.Substring(StartsWithPrefix(PhNumber, LocalCountryInternationalPrefix).Length);
					TargetUNLOCO = null;
				}
				else if (TargetCountryNationalPrefix.Length != 0 && PhNumber.StartsWith(TargetCountryNationalPrefix))
				{
					PhNumber = PhNumber.Substring(TargetCountryNationalPrefix.Length);
					PhNumber = TargetCountryDialingCode + PhNumber;
					targetCountryNationalPrefixWasStripped = true;
				}
				else if (TargetCountrySpecialPrefixes.Length != 0
					&& StartsWithPrefix(PhNumber, TargetCountrySpecialPrefixes).Length > 0)
				{
					PhNumber = TargetCountryDialingCode + PhNumber;
				}
				else if (NominatedUserAreaCode.Length != 0)
				{
					PhNumber = TargetCountryDialingCode + PhNumber;
				}
				else if (TargetCountryDataRow != null && TargetCountryDataRow.HasVariableLocalNumberLength && GetAreaCodeFromCountry(PhNumber) != string.Empty)
				{
					PhNumber = TargetCountryDialingCode + PhNumber;
				}
				else if (!PhNumber.StartsWith(TargetCountryNationalPrefix))
				{
					PhNumber = TargetCountryDialingCode + TargetAreaCode + PhNumber;
				}
				else
				{
					PhNumber = TargetCountryDialingCode + PhNumber;
				}
				PhNumber = PlusPhoneNumberPrefix + PhNumber;
			}
		}

		ZString StartsWithPrefix(ZString number, ZString prefixField)
		{
			ZString matchingPrefix = ZString.Empty;

			if (prefixField.Length > 0)
			{
				ZString[] prefixes = prefixField.Split(PrefixSeparator);
				foreach (ZString prefix in prefixes)
				{
					if (number.StartsWith(prefix))
					{
						matchingPrefix = prefix;
						break;
					}
				}
			}
			return matchingPrefix;
		}

		#endregion

		#endregion

		#region Convert to Correct Format

		#region ConvertToCorrectFormat

		void ConvertToCorrectFormat()
		{
			if (PhNumber.Length >= 8)
			{
				if (PhNumber.StartsWith(PlusPhoneNumberPrefix + AUMobileStart)
					|| PhNumber.StartsWith(PlusPhoneNumberPrefix + NZMobileStart) || PhNumber.StartsWith(PlusPhoneNumberPrefix + MonacoMobileStart) || PhNumber.StartsWith(PlusPhoneNumberPrefix + SloveniaMobileStart))
				{
					ConvertToMobileFormat();
				}
				else if (StartsWithPrefix(PhNumber.SubstringSafe(TargetCountryDialingCode.Length + 1), TargetCountrySpecialPrefixes).Length > 0)
				{
					ConvertToSpecialFormat();
				}
				else
				{
					ConvertToGeneralFormat();
				}
			}
			else
			{
				PhNumber = RawPhoneNumber;
			}
		}

		#endregion

		#region ConvertToSpecialFormat

		void ConvertToSpecialFormat()
		{
			ZString formattedPhoneNumber;
			ZString countryCode = PhNumber.SubstringSafe(0, TargetCountryDialingCode.Length + 1);
			ZString specialNumber = PhNumber.SubstringSafe(countryCode.Length);
			ZString formattedSpecialNumber = ZString.Empty;

			ZInt specialNumberDelimiterStartPosition = specialNumber.Length % SpecialNumberGroupingSize;

			for (int i = 0; i < specialNumber.Length; i++)
			{
				if (i != 0 && (specialNumberDelimiterStartPosition == i % SpecialNumberGroupingSize))
				{
					formattedSpecialNumber = formattedSpecialNumber + LocalSeparator;
				}
				formattedSpecialNumber = formattedSpecialNumber + specialNumber[i];
			}

			formattedPhoneNumber = countryCode + CountryAreaSpacer;
			PhNumber = formattedPhoneNumber + formattedSpecialNumber;
		}

		#endregion

		#region ConvertToMobileFormat

		void ConvertToMobileFormat()
		{
			ZString formattedPhoneNumber;
			ZString countryCode = PhNumber.SubstringSafe(0, TargetCountryDialingCode.Length + 1);
			ZString areaCode = ZString.Empty;
			if (PhNumber.StartsWith(PlusPhoneNumberPrefix + NZMobileStart))
			{
				areaCode = PhNumber.SubstringSafe(countryCode.Length, NZMobileAreaCodeLength);
			}
			else if (PhNumber.StartsWith(PlusPhoneNumberPrefix + AUMobileStart))
			{
				areaCode = PhNumber.SubstringSafe(countryCode.Length, AUMobileAreaCodeLength);
			}
			else if (PhNumber.StartsWith(PlusPhoneNumberPrefix + MonacoMobileStart))
			{
				areaCode = PhNumber.SubstringSafe(countryCode.Length, MonacoMobileAreaCodeLength);
			}
			else if (PhNumber.StartsWith(PlusPhoneNumberPrefix + SloveniaMobileStart))
			{
				areaCode = PhNumber.SubstringSafe(countryCode.Length, SloveniaMobileAreaCodeLength);
			}

			ZString localNumber = PhNumber.SubstringSafe(areaCode.Length + countryCode.Length);
			ZString formattedLocalNumber = ZString.Empty;

			ZInt localNumberDelimiterStartPosition = localNumber.Length % MobileNumberGroupingSize;

			for (int i = 0; i < localNumber.Length; i++)
			{
				if (i != 0 && (localNumberDelimiterStartPosition == i % MobileNumberGroupingSize))
				{
					formattedLocalNumber = formattedLocalNumber + LocalSeparator;
				}
				formattedLocalNumber = formattedLocalNumber + localNumber[i];
			}

			formattedPhoneNumber = countryCode + CountryAreaSpacer;
			if (areaCode.Length > 0)
			{
				formattedPhoneNumber = formattedPhoneNumber + AreaCodeBeginDelimiter + areaCode + AreaCodeEndDelimiter + AreaLocalSpacer;
			}
			PhNumber = formattedPhoneNumber + formattedLocalNumber;
		}

		#endregion

		#region ConvertToGeneralFormat
#if DEBUG
		protected
#endif
 void ConvertToGeneralFormat()
		{
			ZString countryCode = PhNumber.SubstringSafe(0, TargetCountryDialingCode.Length + 1);
			ZString formattedPhoneNumber;
			ZString purePhoneNumber = PhNumber.ExcludeChars(PhoneNumberDelimitersWithSpace);
			ZInt areaCodeLength = NominatedUserAreaCode.Length;
			if (areaCodeLength == 0)
			{
				areaCodeLength = TargetCountryMaxAreaCodeLength;
				for (int possibleAreaCodeLength = TargetCountryMinAreaCodeLength; possibleAreaCodeLength < TargetCountryMaxAreaCodeLength; possibleAreaCodeLength++)
				{
					if (purePhoneNumber.Length - countryCode.Length - TargetCountryLocalNumberLength == possibleAreaCodeLength)
					{
						areaCodeLength = possibleAreaCodeLength;
						break;
					}
				}
			}

			ZString areaCode;
			if (TargetCountryDataRow != null && TargetCountryDataRow.HasVariableLocalNumberLength)
			{
				ZString phoneNumber = purePhoneNumber.SubstringSafe(countryCode.Length);
				areaCode = GetAreaCodeFromCountry(phoneNumber);
			}
			else
			{
				areaCode = purePhoneNumber.SubstringSafe(countryCode.Length, areaCodeLength);
			}

			int localNumberStartIndex = areaCode.Length + countryCode.Length;
			ZString localNumber = (NominatedUserAreaCode.Length > 0 || areaCode.Length > 0) ? purePhoneNumber.SubstringSafe(localNumberStartIndex) : PhNumber.SubstringSafe(localNumberStartIndex);
			ZString formattedLocalNumber = ZString.Empty;
			if (TargetCountryDataRow != null && !TargetCountryDataRow.UseOptionalPrefixFormatting && !TargetCountryDataRow.DoNotFormatLocalNumber)
			{
				ZInt localNumberDelimiterStartPosition = localNumber.Length % LocalNumberGroupingSize;
				for (int i = 0; i < localNumber.Length; i++)
				{
					if (i != 0 && (localNumberDelimiterStartPosition == i % LocalNumberGroupingSize))
					{
						formattedLocalNumber = formattedLocalNumber + LocalSeparator;
					}
					formattedLocalNumber = formattedLocalNumber + localNumber[i];
				}
				formattedPhoneNumber = countryCode + CountryAreaSpacer;
				if (areaCodeLength > 0)
				{
					formattedPhoneNumber = formattedPhoneNumber + AreaCodeBeginDelimiter + areaCode + AreaCodeEndDelimiter + AreaLocalSpacer;
				}
			}
			else if (NominatedUserAreaCode.Length == 0 && areaCode.Length > 0 && TargetCountryDataRow.DoNotFormatLocalNumber)
			{
				formattedPhoneNumber = countryCode + CountryAreaSpacer + areaCode + AreaLocalSpacer + localNumber.TrimStart();
			}
			else
			{
				ZString formattedNominatedUserAreaCode = (NominatedUserAreaCode.Length > 0) ? (ZString)(AreaCodeBeginDelimiter + NominatedUserAreaCode + AreaCodeEndDelimiter + AreaLocalSpacer) : ZString.Empty;
				formattedPhoneNumber = countryCode + CountryAreaSpacer + formattedNominatedUserAreaCode + localNumber.TrimStart();
			}
			PhNumber = formattedPhoneNumber + formattedLocalNumber;
		}

		#endregion

		#endregion

		#region GetAreaCode

		ZString GetAreaCodeFromTargetISO()
		{
			return TargetCountryDataRow != null ? new ZString().PadLeft(TargetCountryDataRow.MaxAreaCodeLength, '*') : ZString.Empty;
		}

		protected ZString GetAreaCodeFromUNLOCO()
		{
			ZString code = "";

			if (TargetCountryMinAreaCodeLength > 0 || TargetCountryMaxAreaCodeLength > 0)
			{
				if (TargetUNLOCO != null)
				{
					string stateCode = TargetUNLOCO.CountryStates != null ? TargetUNLOCO.CountryStates.RW_Code : null;
					code = PhoneInfo.GetAreaCodeByCountryIsoCodeAndAreaIdentifierInOrderOfImportance(TargetCountryISOCode, TargetUNLOCO.RL_Code, stateCode, TargetUNLOCO.RL_PortName);
				}

				if (code.IsEmpty)
				{
					code = new ZString().PadLeft(TargetCountryMaxAreaCodeLength, '*');
				}
			}

			return code;
		}

		protected ZString GetAreaCodeFromCountry(ZString phoneNumber)
		{
			return PhoneInfo.GetAreaCodeByCountryIsoCodeAndPhoneNumber(TargetCountryISOCode, phoneNumber);
		}

		protected ZString GetDisputedCountryAreaCodeByCountry(ZString phoneNumber)
		{
			return PhoneInfo.GetDisputedCountryAreaCodeByCountryIsoCodeAndPhoneNumber(TargetCountryISOCode, phoneNumber);
		}

		protected ZString ExtractAreaCode(ZString phoneNumber)
		{
			ZString areaCode = ZString.Empty;
			ZInt beginPlaceHolder = phoneNumber.IndexOf(AreaCodeBeginDelimiter);
			ZInt endPlaceHolder = phoneNumber.IndexOf(AreaCodeEndDelimiter);
			if (endPlaceHolder != -1 && beginPlaceHolder != -1 && (endPlaceHolder - beginPlaceHolder) > 1)
			{
				ZInt areaCodeStartPlaceHolder = beginPlaceHolder + 1;
				areaCode = phoneNumber.SubstringSafe(areaCodeStartPlaceHolder, (endPlaceHolder - areaCodeStartPlaceHolder));
			}
			return targetCountryNationalPrefixWasStripped ? areaCode.SubstringSafe(TargetCountryNationalPrefix.Length) : areaCode;
		}

		#endregion

		#region Validation Methods

		#region CheckValidPhoneNumber

		internal void CheckValidPhoneNumber(ZPropertyInfo propertyInfo, RefUNLOCO unLoco)
		{
			PhNumber = (ZString)propertyInfo.Value;
			TargetUNLOCO = unLoco;
			if (unLoco == null)
			{
				propertyInfo.AddNotification(NotificationLevel, Res.GetString("6d207f95-2efa-49e6-9224-427461b14539", "Please enter a valid UNLOCO in order to properly validate the phone number."));
			}
			if (unLoco != null && unLoco.RL_Code == "ZZZZZ")
			{
				propertyInfo.AddNotification(NotificationLevel, Res.GetString("8d7b8279-e99f-449b-9544-f048d6c92a4a", "\"ZZZZZ\" is not a valid UNLOCO - please enter a valid UNLOCO in order to properly validate the phone number."));
			}
			else
			{
				if (PhNumber.Length > 0)
				{
					if (TargetAreaCode.Contains('*') && PhNumber.Contains("(" + TargetAreaCode + ")"))
					{
						propertyInfo.AddNotification(NotificationLevel, Res.GetString("90c1e1cc-0062-4311-baca-ca7107625743", "An area code needs to be entered for this number."));
					}
					else if (ContainsOnlyValidCharacters(PhNumber) && PhNumber.StartsWith(PlusPhoneNumberPrefix.ToString())
						&& PhNumber.ExcludeChars(PhoneNumberDelimitersWithSpace).Length >= 8)
					{
						if (TargetUNLOCO != null && TargetUNLOCO.IsInDatabase && !IsValidCountryCode() && GetDisputedCountryAreaCodeByCountry(PhNumber.ExcludeChars(PhoneNumberDelimitersWithSpace + PlusPhoneNumberPrefix)) == string.Empty)
						{
							propertyInfo.AddWarning(Res.GetString("ad59bcc5-15d6-4486-80ec-7b21b5d3fc0e", "The country/region code for \"{0}\" does not match the country/region code for {1}", PhNumber, TargetUNLOCO.RL_Code));
						}
						if (TargetCountryLocalNumberLength > 0
							&& PhNumber.ExcludeChars(PhoneNumberDelimitersWithSpace).Length < (TargetCountryMinAreaCodeLength + TargetCountryLocalNumberLength + TargetCountryDialingCode.Length + PlusPhoneNumberPrefix.ToString().Length))
						{
							propertyInfo.AddWarning(Res.GetString("d96dc07e-e3f4-41ab-b63a-415b72a162d3", "The length of the number does not match the standard length of phone numbers for this country/region."));
						}
					}
					else if (!ContainsOnlyValidCharacters(PhNumber))
					{
						propertyInfo.AddError(NumbersCanOnlyContainValidCharacters);
					}
					else if (!PhNumber.StartsWith(PlusPhoneNumberPrefix.ToString()))
					{
						propertyInfo.AddNotification(NotificationLevel, Res.GetString("b0d9f382-b758-4e2e-b3a7-1925ea270279", "Phone/Fax Numbers should be in the following format:\r\n+ <Country/Region Code> <Area Code> <Local number>."));
					}
					else if (PhNumber.ExcludeChars(PhoneNumberDelimitersWithSpace).Length < 8)
					{
						propertyInfo.AddNotification(NotificationLevel, Res.GetString("af897e0e-0844-42ab-81a6-5bfafbe925a1", "Phone/Fax Number is not a complete number."));
					}
					else
					{
						propertyInfo.AddNotification(NotificationLevel, Res.GetString("20278936-f7b8-478a-bdd1-9d817ead4445", "\"{0}\" is not a valid phone number.", propertyInfo.Value));
					}
				}
			}
		}

		ZBool IsValidCountryCode()
		{
			ZBool validCountryCode = true;
			if (!TargetCountryDialingCode.IsEmpty)
			{
				for (int i = 0; i < TargetCountryDialingCode.Length; i++)
				{
					if (PhNumber[i + 1] != TargetCountryDialingCode[i])
					{
						validCountryCode = false;
						break;
					}
				}
			}
			return validCountryCode;
		}

		#endregion

		#region ContainsOnlyValidCharacters

		internal ZBool ContainsOnlyValidCharacters(ZString number)
		{
			ZString invalidChars = number.ExcludeChars(ValidPhoneNumberChars);
			return (invalidChars.Length == 0 && PlusPrefixIsCorrect(number));
		}

		ZBool PlusPrefixIsCorrect(ZString number)
		{
			int numPlusChars = number.KeepChars(PlusPhoneNumberPrefix.ToString()).Length;
			ZBool correct = numPlusChars < 2;
			if (numPlusChars == 1)
			{
				correct = number[0] == PlusPhoneNumberPrefix;
			}
			return correct;
		}

		#endregion

		#region Perform Number Validation

		/// <summary>
		/// This method will validate a phone number depending on whether it needs to.
		/// It will check the OrgUsePhoneNumberFormatting registry setting.
		/// </summary>
		/// <param name="numberToCheck">The property of the phone number to validate</param>
		/// <param name="homePort">The port that the number should be validated against</param>
		/// <param name="homePortCannotBeNull">Determines whether the home port is allowed to be null (and hence validated against)</param>
		public void PerformNumberValidation(ZPropertyInfo numberToCheck, RefUNLOCO homePort, bool homePortCannotBeNull)
		{
			if (!numberToCheck.Value.IsEmpty)
			{
				if (Env.Registry.OrgUsePhoneNumberFormatting && (homePort != null || homePortCannotBeNull))
				{
					CheckValidPhoneNumber(numberToCheck, homePort);
				}
				else if (!ContainsOnlyValidCharacters(numberToCheck.Value.ToString()))
				{
					numberToCheck.AddError(NumbersCanOnlyContainValidCharacters);
				}
			}
		}

		#endregion

		#endregion

		protected readonly INotificationType NotificationLevel = CargoWise.EntityFramework.NotificationType.Error;
	}
}
