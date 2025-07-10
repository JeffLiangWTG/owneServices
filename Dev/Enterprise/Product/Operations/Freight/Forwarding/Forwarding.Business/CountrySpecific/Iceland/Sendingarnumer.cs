using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class Sendingarnumer
	{
		public Sendingarnumer(ZString code)
		{
			FormatCode(code);
		}

		public Sendingarnumer(ZString code, ZString delimiter)
		{
			Delimiter = delimiter;
			FormatCode(code);
		}

		public const string AirExpressPrefix = "H";

		public ZString CodeWithoutCheckDigit
		{
			get
			{
				return GetCode(true);
			}
		}

		public ZString Code
		{
			get
			{
				return GetCode(false);
			}
		}

		public void FormatCode(ZString code)
		{
			InitialiseCodeBox();
			FormatCodeCore(code);
		}

		public ZString CarrierCode
		{
			get
			{
				return CodeBox[CarrierCodeKey];
			}
			set
			{
				if (value.Length == CarrierCodeLength)
				{
					CodeBox[CarrierCodeKey] = value.Trim();
				}
			}
		}

		public ZString VesselCodeFlightNumber
		{
			get
			{
				return CodeBox[VesselCodeFlightNumberKey];
			}
			set
			{
				if (value.Length == VesselCodeFlightNumberLength)
				{
					CodeBox[VesselCodeFlightNumberKey] = value.Trim();
				}
			}
		}

		public ZDateTime ArrivalDepartureDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (ArrivalDepartureDayMonth.Length == 4
					&& ArrivalDepartureDayMonth.IsNumbersOnlyOrEmpty
					&& ArrivalDepartureYear.Length == 1
					&& ArrivalDepartureYear.IsNumbersOnlyOrEmpty)
				{
					ZDateTime now = ZDateTime.Now;
					ZInt yearPortion = ZInt.ParseEmptyAsZero(ArrivalDepartureYear);
					ZInt monthPortion = ZInt.ParseEmptyAsZero(ArrivalDepartureDayMonth.SubstringSafe(2, 2));
					ZInt dayPortion = ZInt.ParseEmptyAsZero(ArrivalDepartureDayMonth.SubstringSafe(0, 2));

					ZInt currentYear = now.Year;
					int modYear = currentYear % 10;
					ZInt lastYear = (currentYear - 1) % 10;
					ZInt nextYear = (currentYear + 1) % 10;
					ZInt realYear = -1;

					if (yearPortion == modYear)
					{
						realYear = currentYear;
					}
					else if (yearPortion == lastYear)
					{
						realYear = currentYear - 1;
					}
					else if (yearPortion == nextYear)
					{
						realYear = currentYear + 1;
					}

					try
					{
						if (realYear >= 0)
						{
							if (!CheckMonthAndDayAreValid(realYear, monthPortion, dayPortion))
							{
								result = ZDateTime.Invalid;
							}
							else
							{
								result = new ZDateTime(realYear, monthPortion, dayPortion);
							}
						}
					}
					catch (ZTypeValueException)
					{
						result = ZDateTime.Invalid;
					}
				}

				return result;
			}
			set
			{
				if (value.IsValid)
				{
					ZString dateAstString = value.ToString("ddMMy", CultureInfo.InvariantCulture);
					ArrivalDepartureDayMonth = dateAstString.SubstringSafe(0, 4);
					ArrivalDepartureYear = dateAstString.SubstringSafe(4, 1);
				}
			}
		}

		bool CheckMonthAndDayAreValid(ZInt year, ZInt month, ZInt day)
		{
			return month >= 1
				&& month <= 12
				&& day >= 1
				&& day <= DateTime.DaysInMonth(year, month);
		}

		public ZString ArrivalDepartureDayMonth
		{
			get
			{
				return CodeBox[ArrivalDepartureDayMonthKey];
			}
			set
			{
				if (value.Length == ArrivalDepartureDayMonthLength)
				{
					CodeBox[ArrivalDepartureDayMonthKey] = value.Trim();
				}
			}
		}

		public ZString ArrivalDepartureYear
		{
			get
			{
				return CodeBox[ArrivalDepartureYearKey];
			}
			set
			{
				if (value.Length == ArrivalDepartureYearLength)
				{
					CodeBox[ArrivalDepartureYearKey] = value.Trim();
				}
			}
		}

		public ZString PortOfLoadingCountryCode
		{
			get
			{
				return CodeBox[PortOfLoadingCountryCodeKey];
			}
			set
			{
				if (value.Length == PortOfLoadingCountryCodeLength)
				{
					CodeBox[PortOfLoadingCountryCodeKey] = value.Trim();
				}
			}
		}

		public ZString PortOfLoadingPortCode
		{
			get
			{
				return CodeBox[PortOfLoadingPortCodeKey];
			}
			set
			{
				if (value.Length == PortOfLoadingPortCodeLength)
				{
					CodeBox[PortOfLoadingPortCodeKey] = value.Trim();
				}
			}
		}

		public RefUNLOCO PortOfLoadingUNLOCO
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, PortOfLoadingCountryCode + PortOfLoadingPortCode);
			}
		}

		public ZString CarrierNumber
		{
			get
			{
				return CodeBox[CarrierNumberKey];
			}
			set
			{
				if (value.Length == CarrierNumberLength)
				{
					CodeBox[CarrierNumberKey] = value.Trim();
				}
			}
		}

		public ZString CheckDigit
		{
			get
			{
				return CodeBox[CheckDigitKey];
			}
			set
			{
				if (value.Length == CheckDigitLength)
				{
					CodeBox[CheckDigitKey] = value.Trim();
				}
			}
		}

		public ZBool HasValidLength
		{
			get
			{
				return Code.Length == CodeLength;
			}
		}

		public ZBool IsAir
		{
			get
			{
				return !VesselCodeFlightNumber.IsEmpty && VesselCodeFlightNumber.IsNumbersOnlyOrEmpty;
			}
		}

		public ZBool IsSea
		{
			get
			{
				return !VesselCodeFlightNumber.IsEmpty && VesselCodeFlightNumber.IsLettersAndNumbersOnlyOrEmpty;
			}
		}

		public ZBool IsImport
		{
			get
			{
				return !PortOfLoadingCountryCode.IsEmpty && PortOfLoadingCountryCode != Core.Constants.CountryCodes.Iceland;
			}
		}

		public ZBool IsExport
		{
			get
			{
				return !IsImport;
			}
		}

		public ZString GenerateCheckDigitFromCode()
		{
			char checkDigit;
			int asciiValue;
			int sumAsciiValue = 0;
			ZString strippedCode = Code.Replace(Delimiter, "").ToUpper();

			for (int i = 0; i < strippedCode.Length - 1; i++)
			{
				asciiValue = strippedCode[i];

				if (asciiValue >= 48 && asciiValue <= 57)
				{
					asciiValue -= 48;
				}
				else if (asciiValue == 32)
				{
					asciiValue = 36;
				}
				else
				{
					asciiValue -= 55;
				}

				sumAsciiValue += (i + 1) * asciiValue;
			}

			asciiValue = sumAsciiValue % 37;

			if (asciiValue <= 9)
			{
				checkDigit = (char)(asciiValue + 48);
			}
			else if (asciiValue <= 35)
			{
				checkDigit = (char)(asciiValue + 55);
			}
			else
			{
				checkDigit = 'Æ';
			}

			return new ZString(checkDigit);
		}

		#region Implementation

		#region Factory
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}
		BusinessObjectFactory factory;
		#endregion

		#region Constants
		public readonly string Delimiter = "-";
		const string CarrierCodeKey = "CarrierCodeKey";
		const string VesselCodeFlightNumberKey = "VesselCodeFlightNumberKey";
		const string ArrivalDepartureDayMonthKey = "ArrivalDepartureDayMonthKey";
		const string ArrivalDepartureYearKey = "ArrivalDepartureYearKey";
		const string PortOfLoadingCountryCodeKey = "PortOfLoadingCountryCodeKey";
		const string PortOfLoadingPortCodeKey = "PortOfLoadingPortCodeKey";
		const string CarrierNumberKey = "CarrierNumberKey";
		const string CheckDigitKey = "CheckDigitKey";
		#endregion

		#region CodeBox
		Dictionary<string, ZString> CodeBox
		{
			get
			{
				if (codeBox == null)
				{
					codeBox = new Dictionary<string, ZString>();
					codeBox.Add(CarrierCodeKey, ZString.Empty);
					codeBox.Add(VesselCodeFlightNumberKey, ZString.Empty);
					codeBox.Add(ArrivalDepartureDayMonthKey, ZString.Empty);
					codeBox.Add(ArrivalDepartureYearKey, ZString.Empty);
					codeBox.Add(PortOfLoadingCountryCodeKey, ZString.Empty);
					codeBox.Add(PortOfLoadingPortCodeKey, ZString.Empty);
					codeBox.Add(CarrierNumberKey, ZString.Empty);
					codeBox.Add(CheckDigitKey, ZString.Empty);
				}
				return codeBox;
			}
		}
		Dictionary<string, ZString> codeBox;
		#endregion

		#region Code Formatter

		#region Constants
		// Format: A-BBB-CCCC-D-EE-FFF-GGGG-H

		const int CodeLength = 26;
		public const int CodeLengthWithoutDelimiter = 19;

		const int IndexOfCarrierCode = 0;
		const int CarrierCodeLength = 1;

		const int IndexOfVesselCodeFlightNumber = 1;
		const int VesselCodeFlightNumberLength = 3;

		const int IndexOfArrivalDepartureDayMonth = 4;
		const int ArrivalDepartureDayMonthLength = 4;

		const int IndexOfArrivalDepartureYear = 8;
		const int ArrivalDepartureYearLength = 1;

		const int IndexOfPortOfLoadingCountryCode = 9;
		const int PortOfLoadingCountryCodeLength = 2;

		const int IndexOfPortOfLoadingPortCode = 11;
		const int PortOfLoadingPortCodeLength = 3;

		const int IndexOfCarrierNumber = 14;
		const int CarrierNumberLength = 4;

		const int IndexOfCheckDigit = 18;
		const int CheckDigitLength = 1;

		#endregion

		void InitialiseCodeBox()
		{
			CodeBox[CarrierCodeKey] = ZString.Empty;
			CodeBox[VesselCodeFlightNumberKey] = ZString.Empty;
			CodeBox[ArrivalDepartureDayMonthKey] = ZString.Empty;
			CodeBox[ArrivalDepartureYearKey] = ZString.Empty;
			CodeBox[PortOfLoadingCountryCodeKey] = ZString.Empty;
			CodeBox[PortOfLoadingPortCodeKey] = ZString.Empty;
			CodeBox[CarrierNumberKey] = ZString.Empty;
			CodeBox[CheckDigitKey] = ZString.Empty;
		}

		ZString GetCode(ZBool doNotIncludeCheckDigit)
		{
			ZString result = ZString.Empty;

			foreach (ZString key in CodeBox.Keys)
			{
				ZString value = CodeBox[key];

				if (value.IsEmpty || (key == CheckDigitKey && doNotIncludeCheckDigit))
				{
					break;
				}
				else
				{
					result += value + Delimiter;
				}
			}

			return result.Trim(Delimiter.ToCharArray());
		}

		ZString StripNonAlphaNumericChars(ZString value)
		{
			ZString result = ZString.Empty;

			for (int i = 0; i < value.Length; i++)
			{
				if (char.IsLetterOrDigit(value[i]))
				{
					result += value[i];
				}
			}

			return result;
		}

		void FormatCodeCore(ZString unformattedCode)
		{
			unformattedCode = StripNonAlphaNumericChars(unformattedCode);

			if (unformattedCode.Length > 0)
			{
				int i = 0;
				do
				{
					if (i == IndexOfCarrierCode)
					{
						CodeBox[CarrierCodeKey] = unformattedCode.SubstringSafe(i, CarrierCodeLength);
						i = IndexOfCarrierCode + CarrierCodeLength;
					}
					else if (i == IndexOfVesselCodeFlightNumber)
					{
						CodeBox[VesselCodeFlightNumberKey] = unformattedCode.SubstringSafe(i, VesselCodeFlightNumberLength);
						i = IndexOfVesselCodeFlightNumber + VesselCodeFlightNumberLength;
					}
					else if (i == IndexOfArrivalDepartureDayMonth)
					{
						CodeBox[ArrivalDepartureDayMonthKey] = unformattedCode.SubstringSafe(i, ArrivalDepartureDayMonthLength);
						i = IndexOfArrivalDepartureDayMonth + ArrivalDepartureDayMonthLength;
					}
					else if (i == IndexOfArrivalDepartureYear)
					{
						CodeBox[ArrivalDepartureYearKey] = unformattedCode.SubstringSafe(i, ArrivalDepartureYearLength);
						i = IndexOfArrivalDepartureYear + ArrivalDepartureYearLength;
					}
					else if (i == IndexOfPortOfLoadingCountryCode)
					{
						CodeBox[PortOfLoadingCountryCodeKey] = unformattedCode.SubstringSafe(i, PortOfLoadingCountryCodeLength);
						i = IndexOfPortOfLoadingCountryCode + PortOfLoadingCountryCodeLength;
					}
					else if (i == IndexOfPortOfLoadingPortCode)
					{
						CodeBox[PortOfLoadingPortCodeKey] = unformattedCode.SubstringSafe(i, PortOfLoadingPortCodeLength);
						i = IndexOfPortOfLoadingPortCode + PortOfLoadingPortCodeLength;
					}
					else if (i == IndexOfCarrierNumber)
					{
						CodeBox[CarrierNumberKey] = unformattedCode.SubstringSafe(i, CarrierNumberLength);
						i = IndexOfCarrierNumber + CarrierNumberLength;
					}
					else if (i == IndexOfCheckDigit)
					{
						CodeBox[CheckDigitKey] = unformattedCode.SubstringSafe(i, CheckDigitLength);
						break;
					}
				}
				while (i < unformattedCode.Length);
			}
		}
		#endregion

		#endregion
	}
}
