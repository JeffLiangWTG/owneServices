using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer
{
	public class RefAirlineParser
	{
		public RefAirlineParser(string filePath)
		{
			airlineRecordList = ReadFromFile(filePath);
		}

		public IEnumerable<RefAirline> GetAirlinesWithNumericalOrThreeLetterCode()
		{
			var refAirlineList = new List<RefAirline>();
			var numericalOrThreeLetterCodeDictionary = new Dictionary<string, RefAirline>();
			var airlineList = airlineRecordList.Where(x => !string.IsNullOrEmpty(x.PrefixOrAccountingCode) || !string.IsNullOrEmpty(x.ThreeLetterCode));
			foreach (var airlineRecord in airlineList)
			{
				var refAirline = ConvertToRefAirline(airlineRecord);
				var numericalCode = refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode;
				var threeLetterCode = refAirline.RM_ThreeLetterCode;
				var numericalOrThreeLetterCode = !string.IsNullOrEmpty(numericalCode) ? numericalCode : threeLetterCode;
				refAirline = !numericalOrThreeLetterCodeDictionary.ContainsKey(numericalOrThreeLetterCode) ? refAirline : SortDuplicateAirline(refAirline, numericalOrThreeLetterCodeDictionary[numericalOrThreeLetterCode]);
				numericalOrThreeLetterCodeDictionary[numericalOrThreeLetterCode] = refAirline;
			}

			refAirlineList.AddRange(numericalOrThreeLetterCodeDictionary.Values);
			return refAirlineList;
		}

		public IEnumerable<RefAirline> GetAirlinesWithAirlineName1()
		{
			var refAirlineList = new List<RefAirline>();
			var airlineNameDictionary = new Dictionary<string, RefAirline>();
			var airlineNameAirlineList = airlineRecordList.Where(x => (string.IsNullOrEmpty(x.PrefixOrAccountingCode) && string.IsNullOrEmpty(x.ThreeLetterCode)));
			foreach (var airlineRecord in airlineNameAirlineList)
			{
				var refAirline = ConvertToRefAirline(airlineRecord);
#pragma warning disable CA1308 // Normalize strings to uppercase
				var airlineName1 = refAirline.RM_AirlineName1.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
				refAirline = !airlineNameDictionary.ContainsKey(airlineName1) ? refAirline : SortDuplicateAirline(refAirline, airlineNameDictionary[airlineName1]);
				airlineNameDictionary[airlineName1] = refAirline;
			}

			refAirlineList.AddRange(airlineNameDictionary.Values);
			return refAirlineList;
		}

		static List<AirlineRecord> ReadFromFile(string filePath)
		{
			var airlineList = new List<AirlineRecord>();
			var contentArray = File.ReadAllLines(filePath);
			foreach (var content in contentArray)
			{
				if (string.IsNullOrEmpty(content))
				{
					continue;
				}
				var airlineRecord = ParseRecord(content);
				airlineList.Add(airlineRecord);
			}
			return airlineList;
		}

		static AirlineRecord ParseRecord(string content)
		{
			var airline = new AirlineRecord();
			airline.AirlineName1 = content.Substring(0, 40).Trim();
			airline.AirlineName2 = content.Substring(40, 40).Trim();
			airline.AccountingCode = content.Substring(80, 4).Trim();
			airline.ThreeLetterCode = content.Substring(84, 3).Trim();
			airline.TwoCharacterCode = content.Substring(87, 2).Trim();
			airline.DuplicateFlagIndicator = content.Substring(89, 1).Trim();
			airline.AddressLine1 = content.Substring(90, 40).Trim();
			airline.AddressLine2 = content.Substring(130, 40).Trim();
			airline.AirlineCity = content.Substring(170, 25).Trim();
			airline.AirlineState = content.Substring(195, 20).Trim();
			airline.AirlineCountry = content.Substring(215, 44).Trim();
			airline.AirlinePostalCode = content.Substring(259, 10).Trim();
			airline.ReservationsDeptTeleType = content.Substring(269, 8).Trim();
			airline.ReservationsContactName = content.Substring(277, 20).Trim();
			airline.ReservationsContactTitle = content.Substring(297, 20).Trim();
			airline.ReservationsContactTeleType = content.Substring(317, 8).Trim();
			airline.EmergencyTeleType = content.Substring(325, 8).Trim();
			airline.EmergencyContactName = content.Substring(333, 20).Trim();
			airline.EmergencyContactTitle = content.Substring(353, 20).Trim();
			airline.MembershipFlagSITA = content.Substring(373, 1).Trim();
			airline.MembershipFlagARINC = content.Substring(374, 1).Trim();
			airline.MembershipFlagIATA = content.Substring(375, 1).Trim();
			airline.MembershipFlagATA = content.Substring(376, 1).Trim();
			airline.TypeOfOperationsCode = content.Substring(377, 1).Trim();
			airline.AccountingSecondaryFlag = content.Substring(378, 1).Trim();
			airline.AirlinePrefix = content.Substring(379, 3).Trim();
			airline.AirlinePrefixSecondaryFlag = content.Substring(382, 1).Trim();
			airline.PrefixOrAccountingCode = !string.IsNullOrEmpty(airline.AccountingCode) ? airline.AccountingCode : airline.AirlinePrefix;

			return airline;
		}

		static RefAirline ConvertToRefAirline(AirlineRecord airline)
		{
			var refAirline = new RefAirline();
			refAirline.RM_AirlineName1 = airline.AirlineName1;
			refAirline.RM_AirlineName2 = airline.AirlineName2;
			refAirline.RM_AirlinePrefix = airline.AirlinePrefix;
			refAirline.RM_AccountingCode = airline.AccountingCode;
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = airline.PrefixOrAccountingCode;

			refAirline.RM_ThreeLetterCode = airline.ThreeLetterCode;
			refAirline.RM_TwoCharacterCode = airline.TwoCharacterCode;
			refAirline.RM_AddressLine1 = airline.AddressLine1;
			refAirline.RM_AddressLine2 = airline.AddressLine2;
			refAirline.RM_AirlineCity = airline.AirlineCity;
			refAirline.RM_AirlineState = airline.AirlineState;
			refAirline.RM_AirlineCountry = airline.AirlineCountry;
			refAirline.RM_AirlinePostalCode = airline.AirlinePostalCode;

			refAirline.RM_ReservationsDeptTeletype = airline.ReservationsDeptTeleType;
			refAirline.RM_ReservationsContactName = airline.ReservationsContactName;
			refAirline.RM_ReservationsContactTitle = airline.ReservationsContactTitle;
			refAirline.RM_ReservationsContactTeletype = airline.ReservationsContactTeleType;
			refAirline.RM_EmergencyTeletype = airline.EmergencyTeleType;
			refAirline.RM_EmergencyContactName = airline.EmergencyContactName;
			refAirline.RM_EmergencyContactTitle = airline.EmergencyContactTitle;
			refAirline.RM_TypeOfOperationsCode = airline.TypeOfOperationsCode;
			refAirline.RM_AccountingSecondaryFlag = airline.AccountingSecondaryFlag;
			refAirline.RM_AirlinePrefixSecondaryFlag = airline.AirlinePrefixSecondaryFlag;

			if (!string.IsNullOrEmpty(airline.DuplicateFlagIndicator) && airline.DuplicateFlagIndicator.Equals("*", StringComparison.OrdinalIgnoreCase))
			{
				refAirline.RM_DuplicateFlagIndicator = true;
			}
			if (!string.IsNullOrEmpty(airline.MembershipFlagSITA) && airline.MembershipFlagSITA.Equals("Y", StringComparison.OrdinalIgnoreCase))
			{
				refAirline.RM_MembershipFlagSITA = true;
			}
			if (!string.IsNullOrEmpty(airline.MembershipFlagARINC) && airline.MembershipFlagARINC.Equals("Y", StringComparison.OrdinalIgnoreCase))
			{
				refAirline.RM_MembershipFlagARINC = true;
			}
			if (!string.IsNullOrEmpty(airline.MembershipFlagIATA) && airline.MembershipFlagIATA.Equals("Y", StringComparison.OrdinalIgnoreCase))
			{
				refAirline.RM_MembershipFlagIATA = true;
			}
			if (!string.IsNullOrEmpty(airline.MembershipFlagATA) && airline.MembershipFlagATA.Equals("Y", StringComparison.OrdinalIgnoreCase))
			{
				refAirline.RM_MembershipFlagATA = true;
			}
			return refAirline;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness")]
		public static RefAirline SortDuplicateAirline(RefAirline airline1, RefAirline airline2)
		{
			var order = airline1.RM_ThreeLetterCode.CompareTo(airline2.RM_ThreeLetterCode);
			if (order == 0)
			{
				order = airline1.RM_AirlineName1.CompareTo(airline2.RM_AirlineName1);
			}
			if (order == 0)
			{
				order = airline1.RM_AirlineState.CompareTo(airline2.RM_AirlineState);
			}
			return order > 0 ? airline1 : airline2;
		}

		readonly List<AirlineRecord> airlineRecordList;
	}
}
