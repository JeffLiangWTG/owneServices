using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Clients.EDI.Transforms.Native.FSU_FSA122NativeEventInternal
{
	public class HelperCSharpScripts
	{
		System.Collections.Generic.Dictionary<string, string> EnterpriseEventCodeDictionary;

		public string GetEdiEnterpriseEventCode(string cargoImpEventCode)
		{
			if (EnterpriseEventCodeDictionary == null)
			{
				EnterpriseEventCodeDictionary = new System.Collections.Generic.Dictionary<string, string>();
				EnterpriseEventCodeDictionary.Add("RCS", "OCR");
				EnterpriseEventCodeDictionary.Add("RCT", "TRF");
				EnterpriseEventCodeDictionary.Add("RCF", "ARV");
				EnterpriseEventCodeDictionary.Add("BKD", "BKD");
				EnterpriseEventCodeDictionary.Add("MAN", "MAN");
				EnterpriseEventCodeDictionary.Add("DEP", "DEP");
				EnterpriseEventCodeDictionary.Add("PRE", "PRE");
				EnterpriseEventCodeDictionary.Add("TRM", "TRF (Est)");
				EnterpriseEventCodeDictionary.Add("TFD", "TRF");
				EnterpriseEventCodeDictionary.Add("NFD", "CAD");
				EnterpriseEventCodeDictionary.Add("AWD", "ADR");
				EnterpriseEventCodeDictionary.Add("CCD", "CLR");
				EnterpriseEventCodeDictionary.Add("DLV", "RLS");
				EnterpriseEventCodeDictionary.Add("DIS", "CCD");
				EnterpriseEventCodeDictionary.Add("CRC", "ECM");
				EnterpriseEventCodeDictionary.Add("DDL", "RLS");
				EnterpriseEventCodeDictionary.Add("TGC", "TGC");
				EnterpriseEventCodeDictionary.Add("ARR", "ARV");
				EnterpriseEventCodeDictionary.Add("AWR", "ADR");
				EnterpriseEventCodeDictionary.Add("FOH", "HWT");
			}

			string result;
			EnterpriseEventCodeDictionary.TryGetValue(cargoImpEventCode, out result);
			return string.IsNullOrEmpty(result) ? string.Empty : result;
		}

		public string GetVolumeOfGoods(string volumnDetails)
		{
			string volume = string.Empty;
			string volumeCode = string.Empty;

			if (volumnDetails.Length > 2)
			{
				volumeCode = " " + volumnDetails.Substring(0, 2);

				for (int i = 2; i < volumnDetails.Length; i++)
				{
					if (volumnDetails[i] == 'D') break;

					volume += volumnDetails[i];
				}
			}

			return volume + volumeCode;
		}

		public string GetDensityGroup(string volumnDetails)
		{
			string result = string.Empty;
			bool foundDG = false;

			if (volumnDetails.Length > 2)
			{
				for (int i = 2; i < volumnDetails.Length; i++)
				{
					if (foundDG) result += volumnDetails[i];
					if (volumnDetails[i] == 'G') foundDG = true;
				}
			}

			return result;
		}

		public string GetIsPartial(string quantityDetails)
		{
			return (quantityDetails.Length > 0) && quantityDetails.Substring(0, 1) == "P" ? "Y" : string.Empty;
		}

		public string GetNumberOfPieces(string quantityDetails)
		{
			string result = string.Empty;

			for (int i = 1; i < quantityDetails.Length; i++)
			{
				if (quantityDetails[i] == 'K' || quantityDetails[i] == 'L') break;

				result += quantityDetails[i];
			}

			return result;
		}

		public string GetWeightOfGoods(string quantityDetails)
		{
			string result = string.Empty;
			string unit = "";
			bool hasWeight = false;

			for (int i = 1; i < quantityDetails.Length; i++)
			{
				if (hasWeight) { result += quantityDetails[i]; }
				if (quantityDetails[i] == 'K') { unit = "KG"; hasWeight = true; }
				if (quantityDetails[i] == 'L') { unit = "LB"; hasWeight = true; }
			}

			return result + unit;
		}

		public string FormatTime(string time)
		{
			return time.Length >= 4 ? time.Substring(0, 2) + ":" + time.Substring(2) : string.Empty;
		}

		public string FormatDate(string day, string monthName)
		{
			string result = string.Empty;

			if (day.Length > 0 && monthName.Length > 0)
			{
				int year = GetCurrentDateTime.Year;
				int currentMonth = GetCurrentDateTime.Month;
				string uppercaseMonthName = monthName.ToUpper();

				if ((currentMonth == 11 || currentMonth == 12) && (uppercaseMonthName == "JAN" || uppercaseMonthName == "FEB"))
				{
					year++;
				}
				else if ((currentMonth == 1 || currentMonth == 2) && (uppercaseMonthName == "NOV" || uppercaseMonthName == "DEC"))
				{
					year--;
				}

				result = day + "-" + monthName + "-" + year.ToString();   //DD-MMM-YYYY e.g. 15-Jan-2000
			}

			return result;
		}

		public string FormatDateTime(string day, string monthName, string time)
		{
			string dateString = FormatDate(day, monthName);
			string timeString = FormatTime(time);
			return (dateString + " " + timeString).Trim();
		}

		public int GetDaysToChange(string dayChangeIndicator)
		{
			switch (dayChangeIndicator.ToUpper())
			{
				case "P": return -1;
				case "N": return 1;
				case "S": return 2;
				case "T": return 3;
				case "A": return 4;
				case "B": return 5;
				case "C": return 6;
				case "D": return 7;
				case "E": return 8;
				case "F": return 9;
				case "G": return 10;
				case "H": return 11;
				case "I": return 12;
				case "J": return 13;
				case "K": return 14;
				case "L": return 15;
				default: return 0;
			}
		}

		public string FormatDateTimeOfDepartureOrArrival(string day, string monthName, string typeOfTimeIndicator, string time, string dayChangeIndicator)
		{
			return FormatDateTimeOfDepartureOrArrival(day, monthName, typeOfTimeIndicator, time, dayChangeIndicator, false);
		}

		public string FormatDateTimeOfDepartureOrArrival(string day, string monthName, string typeOfTimeIndicator, string time, string dayChangeIndicator, bool isToUseXMLStandFormat)
		{
			string result = string.Empty;

			string timeString = FormatTime(time);

			if (timeString.Length > 0)
			{
				string unadjustedDateString = FormatDate(day, monthName); //DD-MMM-YYYY e.g. 15-Jan-2000

				if (unadjustedDateString.Length > 0)
				{
					System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
					DateTime unadjustedDate;

					if (DateTime.TryParseExact(unadjustedDateString, "dd-MMM-yyyy", invariantCulture, System.Globalization.DateTimeStyles.None, out unadjustedDate))
					{
						DateTime adjustedDate = unadjustedDate.AddDays(GetDaysToChange(dayChangeIndicator));

						if (isToUseXMLStandFormat)
						{
							DateTime parsedTime;
							if (DateTime.TryParseExact(timeString, "HH:mm", invariantCulture, System.Globalization.DateTimeStyles.None, out parsedTime))
							{
								adjustedDate = adjustedDate.AddHours(parsedTime.Hour);
								adjustedDate = adjustedDate.AddMinutes(parsedTime.Minute);
								result = System.Xml.XmlConvert.ToString(adjustedDate, System.Xml.XmlDateTimeSerializationMode.Unspecified);	// yyyy-MM-ddTHH:mm:ss
							}
						}
						else
						{
							string dateString = adjustedDate.ToString("dd-MMM-yyyy", invariantCulture);
							if (typeOfTimeIndicator.Length > 0) typeOfTimeIndicator += " ";
							result = typeOfTimeIndicator + dateString + " " + timeString; //Indicator DD-MMM-YYYY HH:mm e.g. E 15-Jan-2000 17:18
						}
					}
				}
			}

			return result;
		}

		public string FormatXMLStandardDateTime(string day, string monthName, string time)
		{
			string result = string.Empty;

			string timeString = FormatTime(time);

			if (timeString.Length > 0)
			{
				string dateString = FormatDate(day, monthName); //DD-MMM-YYYY e.g. 15-Jan-2000

				if (dateString.Length > 0)
				{
					string dateTimeString = dateString + timeString;
					System.Globalization.CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
					DateTime parsedDate;

					if (DateTime.TryParseExact(dateTimeString, "dd-MMM-yyyyHH:mm", invariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
					{
						result = System.Xml.XmlConvert.ToString(parsedDate, System.Xml.XmlDateTimeSerializationMode.Unspecified);	// yyyy-MM-ddTHH:mm:ss
					}
				}
			}

			return result;
		}

		public string FormatCurrentDateTime()
		{
			return System.Xml.XmlConvert.ToString(GetCurrentDateTime, System.Xml.XmlDateTimeSerializationMode.Unspecified);	// yyyy-MM-ddTHH:mm:ss
		}

		public string GetOSIString(string OSILine1, string OSILine2)
		{
			string result = (OSILine1.Length > 0 && OSILine2.Length > 0) ? OSILine1 + " " + OSILine2 : OSILine1 + OSILine2;
			return result.Substring(0, Math.Min(130, result.Length));   //Truncate to 130 characters
		}

		public string GetULDString(string ULD1, string ULD2, string ULD3, string ULD4, string ULD5)
		{
			return ConcatULDStrings(ULD1, ULD2, ULD3, ULD4, ULD5);
		}

		public string ConcatULDStrings(params string[] ULDs)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string uld in ULDs)
			{
				if (!string.IsNullOrEmpty(uld))
				{
					if (builder.Length > 0)
					{
						builder.Append(", ");
					}

					builder.Append(uld);
				}
			}

			return builder.ToString();
		}

		public virtual DateTime GetCurrentDateTime
		{
			get { return DateTime.Now; }
		}
	}
}
