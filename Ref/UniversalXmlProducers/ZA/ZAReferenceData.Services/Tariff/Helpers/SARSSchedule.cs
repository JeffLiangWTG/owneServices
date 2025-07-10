using System.Collections.Generic;
using System.Linq;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers
{
	public class SARSSchedule
	{
		public int Schedule { get; private set; }
		public int Part { get; private set; }
		public string Section { get; private set; }
		public string DutyFormula { get; private set; }
		public string TariffTypeOverride { get; private set; }

		public int ItemNumberRangeLow { get; private set; }
		public int ItemNumberRangeHigh { get; private set; }
		public bool HasImportCountry { get; private set; }
		public bool HasNoTradeGroup { get; private set; }
		public bool CreateRelationship { get; private set; }
		public bool IsValidForStandardRate { get; private set; }
		public string RateType { get; private set; }

		SARSSchedule(int schedule, int part, int itemNumberRangeLow, int itemNumberRangeHigh, string rateType,
			string section = "",
			string dutyFormula = "",
			string tariffType = "",
			bool hasImportCountry = false,
			bool hasNoTradeGroup = false,
			bool createRelationship = true,
			bool isValidForStandardRate = true)
		{
			Schedule = schedule;
			Part = part;
			Section = section;
			DutyFormula = dutyFormula;
			TariffTypeOverride = tariffType;
			ItemNumberRangeLow = itemNumberRangeLow;
			ItemNumberRangeHigh = itemNumberRangeHigh;
			HasImportCountry = hasImportCountry;
			HasNoTradeGroup = hasNoTradeGroup;
			CreateRelationship = createRelationship;
			IsValidForStandardRate = isValidForStandardRate;
			RateType = rateType;
		}

		public string ScheduleType => string.IsNullOrEmpty(Section) ? $"{Schedule}P{Part}" : $"{Schedule}{Part}{Section}";

		public string GetTariffType()
		{
			var result = ScheduleType;

			if (!string.IsNullOrEmpty(TariffTypeOverride))
			{
				result = TariffTypeOverride;
			}

			return result;
		}

		public static SARSSchedule Get(string scheduleTypeCode, string itemNumber)
		{
			if (ScheduleMapping == null)
			{
				ScheduleMapping = SetupDictionary();
			}

			var code = FormatCode(scheduleTypeCode);

			if (ScheduleMapping.ContainsKey(code))
			{
				return ScheduleMapping[code];
			}
			else if ((itemNumber?.Length ?? 0) >= 3)
			{
				if (int.TryParse(itemNumber.Substring(0, 3), out var value))
				{
					var schedule = ScheduleMapping.Values.Where(x => x.ItemNumberRangeLow <= value && value <= x.ItemNumberRangeHigh).FirstOrDefault();

					if (schedule != null)
					{
						return schedule;
					}
				}
			}

			return BlankSchedule;
		}

		static string FormatCode(string scheduleTypeCode)
		{
			var result = scheduleTypeCode ?? string.Empty;

			if (result.Length > 1 && result[1] != 'P')
			{
				result = result[0] + "P" + result.Substring(1);
			}

			return result;
		}

		static Dictionary<string, SARSSchedule> ScheduleMapping;
		static Dictionary<string, SARSSchedule> SetupDictionary() => new Dictionary<string, SARSSchedule>()
		{
			{ "1P1", S1P1 },
			{ "1P2A", S1P2A },
			{ "1P2B", S1P2B },
			{ "1P3A", S1P3A },
			{ "1P3B", S1P3B },
			{ "1P3C", S1P3C },
			{ "1P3D", S1P3D },
			{ "1P3E", S1P3E },
			{ "1P5A", S1P5A },
			{ "1P5B", S1P5B },
			{ "1P6A", S1P6A },
			{ "1P7A", S1P7A },
			{ "1P8", S1P8 },
			{ "2P1", S2P1 },
			{ "2P2", S2P2 },
			{ "2P3", S2P3 },
			{ "3P1", S3P1 },
			{ "3P2", S3P2 },
			{ "4P1", S4P1 },
			{ "4P2", S4P2 },
			{ "4P3", S4P3 },
			{ "4P4", S4P4 },
			{ "4P5", S4P5 },
			{ "4P6", S4P6 },
			{ "5P1", S5P1 },
			{ "5P2", S5P2 },
			{ "5P3", S5P3 },
			{ "5P4", S5P4 },
			{ "5P5", S5P5 },
			{ "5P6", S5P6 },
			{ "6P1A", S6P1A },
			{ "6P1B", S6P1B },
			{ "6P1C", S6P1C },
			{ "6P1D", S6P1D },
			{ "6P1E", S6P1E },
			{ "6P1F", S6P1F },
			{ "6P1G", S6P1G },
			{ "6P2", S6P2 },
			{ "6P3", S6P3 },
			{ "6P4", S6P4 },
			{ "6P5", S6P5 },
			{ "6P6", S6P6 }
		};

		// Schedule 1 - CUSTOMS, EXCISE AND SALES DUTIES AND SURCHARGE
		public static readonly SARSSchedule S1P1 = new SARSSchedule(1, 1, 0, -1, RateTypes.Duty, createRelationship: false); // CUSTOMS DUTY - 0101-9999
		static readonly SARSSchedule S1P2A = new SARSSchedule(1, 2, 104, 108, RateTypes.Excise, "A", hasNoTradeGroup: true); // SPECIFIC EXCISE DUTIES ON LOCALLY MANUFACTURED OR ON IMPORTED GOODS OF THE SAME CLASS OR KIND - 104-108
		static readonly SARSSchedule S1P2B = new SARSSchedule(1, 2, 118, 130, RateTypes.AdValoremExcise, "B", hasNoTradeGroup: true); // AD VALOREM EXCISE DUTIES ON LOCALLY MANUFACTURED GOODS OR ON IMPORTED GOODS OF THE SAME CLASS OR KIND - 118-130
		static readonly SARSSchedule S1P3A = new SARSSchedule(1, 3, 147, 147, RateTypes.Levy, "A", hasNoTradeGroup: true); // ENVIRONMENTAL LEVY - 147-147
		static readonly SARSSchedule S1P3B = new SARSSchedule(1, 3, 148, 148, RateTypes.Levy, "B", hasNoTradeGroup: true); // ENVIRONMENTAL LEVY - 148-148
		static readonly SARSSchedule S1P3C = new SARSSchedule(1, 3, 149, 149, RateTypes.Levy, "C", hasNoTradeGroup: true); // ENVIRONMENTAL LEVY ON ELECTRICAL FILAMENT LAMPS - 149-149
		static readonly SARSSchedule S1P3D = new SARSSchedule(1, 3, 151, 151, RateTypes.Levy, "D", hasNoTradeGroup: true); // ENVIRONMENTAL LEVY ON CARBON DIOXIDE (CO2) EMISSIONS OF MOTOR VEHICLES - 151-151
		static readonly SARSSchedule S1P3E = new SARSSchedule(1, 3, 152, 155, RateTypes.Levy, "E", hasNoTradeGroup: true); // ENVIRONMENTAL LEVY - 152-155
		static readonly SARSSchedule S1P5A = new SARSSchedule(1, 5, 195, 195, RateTypes.Levy, "A", hasNoTradeGroup: true); // FUEL LEVY - 195-195
		static readonly SARSSchedule S1P5B = new SARSSchedule(1, 5, 197, 197, RateTypes.Levy, "B", hasNoTradeGroup: true); // ROAD ACCIDENT FUND LEVY - 197-197
		static readonly SARSSchedule S1P6A = new SARSSchedule(1, 6, 193, 193, RateTypes.Duty, "A"); // EXPORT DUTY ON SCRAP METAL - 193-193
		static readonly SARSSchedule S1P7A = new SARSSchedule(1, 7, 191, 191, RateTypes.Levy, "A", hasNoTradeGroup: true); // HEALTH PROMOTION LEVY - 191-191
		static readonly SARSSchedule S1P8 = new SARSSchedule(1, 8, 196, 196, RateTypes.Levy, hasNoTradeGroup: true); // ORDINARY LEVY - 196

		// Schedule 2 - ANTI-DUMPING AND COUNTERVAILING DUTIES ON IMPORTED GOODS
		static readonly SARSSchedule S2P1 = new SARSSchedule(2, 1, 201, 217, RateTypes.AntiDumping, hasImportCountry: true); // ANTI-DUMPING DUTIES ON IMPORTED GOODS - 201-217
		static readonly SARSSchedule S2P2 = new SARSSchedule(2, 2, 0, -1, RateTypes.AntiDumping, hasImportCountry: true); // COUNTERVAILING DUTIES ON IMPORTED GOODS -n/a
		static readonly SARSSchedule S2P3 = new SARSSchedule(2, 3, 250, 260, RateTypes.AntiDumping, hasImportCountry: true); // SAFEGUARD DUTIES ON IMPORTED GOODS - 250-260

		// Schedule 3 - INDUSTRIAL REBATES OF CUSTOMS DUTIES
		static readonly SARSSchedule S3P1 = new SARSSchedule(3, 1, 303, 321, RateTypes.Rebate, dutyFormula: "1P1", hasNoTradeGroup: true); // GOODS USED IN THE MANUFACTURE OF OTHER GOODS - 303-321
		static readonly SARSSchedule S3P2 = new SARSSchedule(3, 2, 334, 392, RateTypes.Rebate, dutyFormula: "1P1", hasNoTradeGroup: true); // GOODS USED IN THE MANUFACTURE OF OTHER GOODS FOR EXPORT - 334-392

		// Schedule 4 - REBATES AND REFUNDS OF CUSTOMS DUTIES, EXCISE DUTIES, FUEL LEVY, ROAD ACCIDENT FUND LEVY, ENVIRONMENTAL LEVY AND HEALTH PROMOTION LEVY
		static readonly SARSSchedule S4P1 = new SARSSchedule(4, 1, 403, 414, RateTypes.Rebate, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", hasNoTradeGroup: true); // SPECIFIC REBATES OF CUSTOMS DUTIES - 403-414
		static readonly SARSSchedule S4P2 = new SARSSchedule(4, 2, 460, 460, RateTypes.Rebate, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", hasNoTradeGroup: true); // TEMPORARY REBATES OF CUSTOMS DUTIES - 460-460
		static readonly SARSSchedule S4P3 = new SARSSchedule(4, 3, 470, 490, RateTypes.Rebate, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", hasNoTradeGroup: true); // GOODS TEMPORARILY ADMITTED UNDER REBATE OF CUSTOMS DUTIES - 470-490
		static readonly SARSSchedule S4P4 = new SARSSchedule(4, 4, 495, 496, RateTypes.Rebate, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", hasNoTradeGroup: true); // REBATES OF FUEL LEVY - 495-496
		static readonly SARSSchedule S4P5 = new SARSSchedule(4, 5, 497, 497, RateTypes.Rebate, dutyFormula: "13A+13B+13C+13D+13E", hasNoTradeGroup: true); // REBATES OF ENVIRONMENTAL LEVY - 497-497
		static readonly SARSSchedule S4P6 = new SARSSchedule(4, 6, 498, 498, RateTypes.Rebate, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", hasNoTradeGroup: true); // IMPORTED GOODS ADMITTED UNDER REBATE OF DUTY FOR USE IN THE CUSTOMS CONTROLLED AREA ("CCA") CONTEMPLATED IN SECTION 21A - 498-498

		// Schedule 5 - SPECIFIC DRAWBACKS AND REFUNDS OF CUSTOMS DUTIES, FUEL LEVY AND HEALTH PROMOTION LEVY
		static readonly SARSSchedule S5P1 = new SARSSchedule(5, 1, 501, 521, RateTypes.Refund, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", isValidForStandardRate: false); // SPECIFIC DRAWBACKS OF CUSTOMS DUTIES - 501-521
		static readonly SARSSchedule S5P2 = new SARSSchedule(5, 2, 522, 522, RateTypes.Refund, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", isValidForStandardRate: false); // REFUNDS OF CUSTOMS DUTIES ON GOODS EXPORTED IN THE SAME CONDITION AS IMPORTED - 522-522
		static readonly SARSSchedule S5P3 = new SARSSchedule(5, 3, 532, 538, RateTypes.Refund, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", isValidForStandardRate: false); // MISCELLANEOUS REFUNDS OF CUSTOMS DUTIES AND FUEL LEVY - 532-538
		static readonly SARSSchedule S5P4 = new SARSSchedule(5, 4, 540, 541, RateTypes.Refund, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", isValidForStandardRate: false); // REFUNDS OF FUEL LEVY - 540-540
		static readonly SARSSchedule S5P5 = new SARSSchedule(5, 5, 550, 551, RateTypes.Refund, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", isValidForStandardRate: false); // DRAWBACKS AND REFUNDS OF ENVIRONMENTAL LEVY ON IMPORTED GOODS - 550-551
		static readonly SARSSchedule S5P6 = new SARSSchedule(5, 6, 560, 561, RateTypes.Refund, dutyFormula: "1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B", isValidForStandardRate: false); // DRAWBACKS AND REFUNDS OF HEALTH PROMOTION LEVY ON IMPORTED GOODS - 560-561

		// Schedule 6 - REBATES AND REFUNDS OF EXCISE DUTIES, FUEL LEVY, ROAD ACCIDENT FUND LEVY, ENVIRONMENTAL LEVY AND HEALTH PROMOTION LEVY
		static readonly SARSSchedule S6P1A = new SARSSchedule(6, 1, 618, 618, RateTypes.Refund, "A", dutyFormula: "12A+12B", tariffType: Schedules.S6P1, isValidForStandardRate: false); // REBATES AND REFUNDS OF SPECIFIC EXCISE DUTIES - 618
		static readonly SARSSchedule S6P1B = new SARSSchedule(6, 1, 619, 619, RateTypes.Refund, "B", dutyFormula: "12A+12B", tariffType: Schedules.S6P1, isValidForStandardRate: false); // REBATES AND REFUNDS OF SPECIFIC EXCISE DUTIES ON BEER MADE FROM MALT AND TRADITIONAL AFRICAN BEER - 619
		static readonly SARSSchedule S6P1C = new SARSSchedule(6, 1, 620, 620, RateTypes.Refund, "C", dutyFormula: "12A+12B", tariffType: Schedules.S6P1, isValidForStandardRate: false); // REBATES AND REFUNDS OF SPECIFIC EXCISE DUTIES ON WINE AND OTHER FERMENTED BEVERAGES (EXCLUDING BEER MADE FROM MALT AND TRADITIONAL AFRICAN BEER), MIXTURES OF FERMENTED BEVERAGES AND MIXTURES OF FERMENTED BEVERAGES AND NON-ALCOHOLIC BEVERAGES NOT ELSEWHERE SPECIFIED OR INCLUDED - 620
		static readonly SARSSchedule S6P1D = new SARSSchedule(6, 1, 621, 621, RateTypes.Refund, "D", dutyFormula: "12A+12B", tariffType: Schedules.S6P1, isValidForStandardRate: false); // REBATES AND REFUNDS OF SPECIFIC EXCISE DUTIES ON SPIRITS AND SPIRITUOUS BEVERAGES - 621
		static readonly SARSSchedule S6P1E = new SARSSchedule(6, 1, 622, 622, RateTypes.Refund, "E", dutyFormula: "12A+12B", tariffType: Schedules.S6P1, isValidForStandardRate: false); // REBATES AND REFUNDS OF SPECIFIC EXCISE DUTIES ON MANUFACTURED TOBACCO AND TOBACCO SUBSTITUTE PRODUCTS - 622
		static readonly SARSSchedule S6P1F = new SARSSchedule(6, 1, 623, 623, RateTypes.Refund, "F", dutyFormula: "12A+12B", tariffType: Schedules.S6P1, isValidForStandardRate: false); // REBATES AND REFUNDS OF SPECIFIC EXCISE DUTIES ON MINERAL PRODUCTS - 623
		static readonly SARSSchedule S6P1G = new SARSSchedule(6, 1, 624, 624, RateTypes.Refund, "G", dutyFormula: "12A+12B", tariffType: Schedules.S6P1, isValidForStandardRate: false); // MISCELLANEOUS REBATES AND REFUNDS OF SPECIFIC EXCISE DUTIES - 624
		static readonly SARSSchedule S6P2 = new SARSSchedule(6, 2, 630, 635, RateTypes.Refund, dutyFormula: "12A+12B", isValidForStandardRate: false); // REBATES AND REFUNDS OF AD VALOREM EXCISE DUTIES - 630-635
		static readonly SARSSchedule S6P3 = new SARSSchedule(6, 3, 670, 672, RateTypes.Refund, dutyFormula: "15A+15B", isValidForStandardRate: false); // REBATES AND REFUNDS OF FUEL LEVY AND ROAD ACCIDENT FUND LEVY - 670-671
		static readonly SARSSchedule S6P4 = new SARSSchedule(6, 4, 680, 681, RateTypes.Refund, dutyFormula: "12A+12B", isValidForStandardRate: false); // REBATES AND REFUNDS OF ENVIRONMENTAL LEVY ON ENVIRONMENTAL LEVY GOODS MANUFACTURED IN THE REPUBLIC - 680-681
		static readonly SARSSchedule S6P5 = new SARSSchedule(6, 5, 690, 691, RateTypes.Refund, dutyFormula: "12A+12B", isValidForStandardRate: false); // REBATES AND REFUND ON HEALTH PROMOTION LEVY - 690-691
		static readonly SARSSchedule S6P6 = new SARSSchedule(6, 6, 692, 692, RateTypes.Refund, dutyFormula: "13F", isValidForStandardRate: false); // REBATES AND REFUNDS ON CARBON TAX - 692

		static readonly SARSSchedule BlankSchedule = new SARSSchedule(0, 0, 0, -1, string.Empty); // not found

		public static List<SARSSchedule> GetAllSchedules()
		{
			if (ScheduleMapping == null)
			{
				ScheduleMapping = SetupDictionary();
			}

			return ScheduleMapping.Values.ToList();
		}

		static class RateTypes
		{
			public const string AdValoremExcise = "EX1";
			public const string AntiDumping = "ADD";
			public const string Duty = "DTY";
			public const string Excise = "EXC";
			public const string Levy = "LVY";
			public const string Penalties = "PEN";
			public const string ProvisionalPayments = "PRP";
			public const string Rebate = "REB";
			public const string Refund = "REF";
		}
	}
}
