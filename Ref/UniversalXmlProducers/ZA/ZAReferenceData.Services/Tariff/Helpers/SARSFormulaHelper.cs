using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers
{
	public static class SARSFormulaHelper
	{
		internal class FormulaData
		{
			public TariffData Tariff { get; set; }
			public Rate Rate { get; set; }
			public string Formula { get; set; } = string.Empty;
			public string UnitOfMeasureOriginal { get; set; } = string.Empty;
			public string UnitOfMeasureConverted { get; set; } = string.Empty;
			public string FormulaNumber { get; set; } = string.Empty;

			public FormulaData(TariffData tariff, Rate rate)
			{
				Tariff = tariff;
				Rate = rate;
			}
		}

		public static bool PopulateFormulaData(TariffData tariff, Rate rate)
		{
			var data = new FormulaData(tariff, rate);

			var matched = Process(data);

			rate.Formula = data.Formula;
			rate.UnitOfMeasureOriginal = data.UnitOfMeasureOriginal;
			rate.UnitOfMeasureConverted = data.UnitOfMeasureConverted;

			return matched;
		}

		static bool Process(FormulaData data)
		{
			switch (data.Rate.FormulaCode)
			{
				case "1001":
					return ProcessCommon(data, Actions1001());
				case "1216":
					return ProcessCommon(data, Actions1216());
				case "1302":
					return ProcessCommon(data, Actions1302());
				case "1352":
					return ProcessCommon(data, Actions1352());
				case "1354":
					return ProcessCommon(data, Actions1354());
				case "1556":
					return ProcessCommon(data, Actions1556());
				case "1564":
					return ProcessCommon(data, Actions1564());
				case "1600":
					return ProcessCommon(data, Actions1600());
				case "3408":
					return ProcessCommon(data, Actions3408());
				case "3410":
					return ProcessCommon(data, Actions3410());
				case "3423":
					return ProcessCommon(data, Actions3423());
				case "3425":
					return ProcessCommon(data, Actions3425());
				case "3436":
					return ProcessCommon(data, Actions3436());
				case "3440":
					return ProcessCommon(data, Actions3440());
				case "4555":
					return ProcessCommon(data, Actions4555());
				case "4558":
					return ProcessCommon(data, Actions4558());
				case "4559":
					return ProcessCommon(data, Actions4559());
				case "4560":
					return ProcessCommon(data, Actions4560());
				case "4567":
					return ProcessCommon(data, Actions4567());
				case "4570":
					return ProcessCommon(data, Actions4570());
				case "9999":
					return ProcessCommon(data, Actions9999());

				default:
					return false;
			}
		}

		static bool ProcessCommon(FormulaData data, List<(int regCode, string regEx, Func<FormulaData, bool> criteria, Action<FormulaData, Match> action)> regExActions, string overrideDescription = "")
		{
			var match = false;

			var descripToMatch = (string.IsNullOrEmpty(overrideDescription) ? data.Rate.Description : overrideDescription).ToUpperInvariant();

			foreach (var regAc in regExActions)
			{
				if (regAc.criteria == null || regAc.criteria(data))
				{
					var regMatches = new Regex(regAc.regEx).Match(descripToMatch);
					if (regMatches.Success)
					{
						regAc.action.Invoke(data, regMatches);

						var formulaCode = FormatCode(regAc.regCode);
						if (string.IsNullOrEmpty(data.FormulaNumber))
						{
							data.FormulaNumber = formulaCode;
						}
						else
						{
							data.FormulaNumber = $"{data.FormulaNumber}/{formulaCode}";
						}

						match = true;
						break;
					}
				}
			}

			return match;
		}

		static string FormatCode(int code) => $"C{code:0000}";

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1001()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (1, "^FREE$", null, (data, matches) => data.Formula = "0" ) },
				{ (2, "^FULL DUTY$", null, (data, mmatches) => data.Formula = data.Tariff.Schedule.DutyFormula ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1216()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (3, "^FULL DUTY LESS ([R]{0,1})([0-9,. ]*)([ ]{0,1})([C]{0,1})/(([^%]*?)( AND.*){1}|(([^% ]*?)( ([^% ]*?)){0,1}))$", null, (data, matches) =>
					{
						var unitO = matches.Groups[6].Value + matches.Groups[8].Value;
						var value = ConvertValue(matches.Groups[2].Value, matches.Groups[1].Value, unitO);
						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);

						data.Formula = $"MAX({data.Tariff.Schedule.DutyFormula} - ({value:G29} * [{unitC}]), 0)";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) },
				{ (4, "^FULL FUEL LEVY LESS ([R]{0,1})([0-9,. ]*)([ ]{0,1})([C]{0,1})/(([^%]*?)( AND.*){1}|(([^% ]*?)( ([^% ]*?)){0,1}))$", null, (data, matches) =>
					{
						var unitO = matches.Groups[6].Value + matches.Groups[8].Value;
						var value = ConvertValue(matches.Groups[2].Value, matches.Groups[1].Value, unitO);
						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);

						data.Formula = $"MAX(MAX(15A - ({value:G29} * [{unitC}]), 0) + 15B, 0)";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) },
				{ (5, "^([R]{0,1})([0-9,. ]*)([ ]{0,1})([C]{0,1})/(([^%]*?)( AND.*){1}|(([^% ]*?)( ([^% ]*?)){0,1}))$", null, (data, matches) =>
					{
						var unitO = matches.Groups[6].Value + matches.Groups[8].Value;
						var value = ConvertValue(matches.Groups[2].Value, matches.Groups[1].Value, unitO);
						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);

						var unitValueMatch = new Regex("([0-9]+)(.+)").Match(unitC);
						if (unitValueMatch.Success)
						{
							var noOfUnits = CleanValue(unitValueMatch.Groups[1].Value);
							if (noOfUnits != 0)
							{
								unitO = unitValueMatch.Groups[2].Value;
								unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);
								value = value / noOfUnits;
							}
						}

						data.Formula = $"{value:G29} * [{unitC}]";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1302()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (6, "^FULL DUTY LESS {0,1}([0-9,. ]*)([ ]{0,1})[%]$", null, (data, matches) =>
					{
						var value = ConvertPercentage(matches.Groups[1].Value);
						data.Formula = $"MAX({data.Tariff.Schedule.DutyFormula} - ({value:G29} * {ValueForDuty}), 0)";
					}
				) },
				{ (7, "^{0,1}([0-9,. ]*)([ ]{0,1})[%]$", null, (data, matches) =>
					{
						var value = ConvertPercentage(matches.Groups[1].Value);
						data.Formula = $"{value:G29} * {ValueForDuty}";
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1352()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (8, "^NOT EXCEEDING ([0-9,. ]*)([ ]{0,1})[%]$", null, (data, matches) =>
					{
						var value = ConvertPercentage(matches.Groups[1].Value);
						data.Formula = $"MIN({data.Tariff.Schedule.DutyFormula}, {value:G29} * {ValueForDuty})";
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1354()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (9, "^((THE)|(FULL)) DUTY IN PART ([0-9]{1,2}) OF SCHEDULE NO. ([0-9]{1,2}) LESS ([0-9,. ]*)([ ]{0,1})[%]$", null, (data, matches) =>
					{
						var schedule = matches.Groups[5].Value;
						var part = matches.Groups[4].Value;
						var value = ConvertPercentage(matches.Groups[6].Value);
						data.Formula = $"MAX(({schedule}P{part} - {value:G29} * {ValueForDuty}), 0)";
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1556()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (10, "^FULL DUTY LESS THE DUTY IN SECTION ([A-Z]{1,2}) OF PART ([0-9]{1,2}) OF SCHEDULE NO. ([0-9]{1,2})$", null, (data, matches) => FullDutyLess(data, matches) ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1564()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (11, "^FULL DUTY LESS THE DUTY IN SECTION ([A-Z]{1,2}) OF PART ([0-9]{1,2}) OF SCHEDULE NO. ([0-9]{1,2})$", null, (data, matches) => FullDutyLess(data, matches) ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions1600()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (12, "^(.*)EMISSIONS$", null, (data, matches) => data.Formula = "Carbon Emissions for ZA Factories, paid on eFiling only" ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions3408()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (13, "^([0-9,. ]*)[%] OR ([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1})/([^ ]*)$", null, (data, matches) =>
					{
						var percentage = ConvertPercentage(matches.Groups[1].Value);
						var unitO = matches.Groups[6].Value;
						var value = ConvertValue(matches.Groups[3].Value, matches.Groups[2].Value, unitO);

						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);
						data.Formula = $"MAX({percentage} * {ValueForDuty}, {value:G29} * [{unitC}])";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions3410()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (14, "^([0-9,. ]*)[%] OR ([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1})/([^ ]*)( LESS )(.*?)[%]$", null, (data, matches) =>
					{
						var percentage = ConvertPercentage(matches.Groups[1].Value);
						var unitO = matches.Groups[6].Value;
						var value = ConvertValue(matches.Groups[3].Value, matches.Groups[2].Value, unitO);
						var lessPercentage = ConvertPercentage(matches.Groups[8].Value);

						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);
						data.Formula = $"MAX({percentage} * {ValueForDuty}, MAX(({value:G29} * [{unitC}] - {lessPercentage} * {ValueForDuty}), 0))";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions3423()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (15, "^([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1})/([^ ]*)( LESS )([0-9,. ]*)[%] WITH A MAXIMUM OF ([0-9,. ]*)[%]$", null, (data, matches) =>
					{
						var lessPercentage = ConvertPercentage(matches.Groups[7].Value);
						var unitO = matches.Groups[5].Value;
						var value = ConvertValue(matches.Groups[2].Value, matches.Groups[1].Value, unitO);
						var percentage = ConvertPercentage(matches.Groups[8].Value);

						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);
						data.Formula = $"MIN(MAX(({value:G29} * [{unitC}] - {lessPercentage} * {ValueForDuty}), 0), {percentage} * {ValueForDuty})";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions3425()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (16, "^([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1})/([^ ]*) WITH A MAXIMUM OF ([0-9,. ]*)[%]$", null, (data, matches) =>
					{
						var unitO = matches.Groups[5].Value;
						var value = ConvertValue(matches.Groups[2].Value, matches.Groups[1].Value, unitO);
						var percentage = ConvertPercentage(matches.Groups[6].Value);

						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);
						data.Formula = $"MIN({value:G29} * [{unitC}], {percentage} * {ValueForDuty})";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions3436()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (17, "^\\(SEE NOTE [1,2]{1} TO THIS PART\\)$", (data) => data.Tariff.Schedule.Schedule == 1 && data.Tariff.Schedule.Part == 2 && data.Tariff.Schedule.Section == "B", (data, matches) =>
					{
						data.Formula = "MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)";
					}
				) },
				{ (18, "^\\{\\(([0-9,. ]*) X (.{1})\\) - ([0-9,. ]*)\\}% WITH A MAXIMUM OF ([0-9,. ]*)[%](.*)$", (data) => data.Tariff.Schedule.Schedule == 1 && data.Tariff.Schedule.Part == 2, (data, matches) =>
					{
						data.Formula = "MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)";
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions3440()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (19, "^([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1}) PER (.*) CO[?�]?(?:<SUB>2</SUB>)? ? EMISSIONS EXCEEDING ([0-9,. ]*)(.*)$", null, (data, matches) =>
					{
						var unitO = matches.Groups[5].Value;
						var value = ConvertValue(matches.Groups[2].Value, matches.Groups[1].Value, unitO);
						var unitExcessQty = CleanValue(matches.Groups[6].Value);

						var unitC = SARSMappingHelper.GetUnitOfMeasure(unitO);
						data.Formula = $"{value:G29} * MAX(([{unitC}] - {unitExcessQty}), 0)";
						data.UnitOfMeasureOriginal = unitO;
						data.UnitOfMeasureConverted = unitC;
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions4555()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (20, "^FULL DUTY LESS THE AMOUNT OF ANY REBATE, REFUND AND DRAWBACK GRANTED PREVIOUSLY AND LESS THE DUTY ON THE COST OF PROCESSING OR REPAIR$", null, (data, matches) => RebateAmount(data) ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions4558()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (21, "^NOT EXCEEDING THE DUTY IN SECTION ([A-Z]{1,2}) OF PART ([0-9]{1,2}) OF SCHEDULE NO. ([0-9]{1,2})$", null, (data, matches) => RebateAmount(data) ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions4559()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (22, "^FULL DUTY LESS THE DUTY PAID ON ENTRY$", null, (data, matches) => RebateAmount(data) ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions4560()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (23, "^FULL DUTY LESS THE AMOUNT OF ANY REBATE, REFUND AND DRAWBACK GRANTED PREVIOUSLY$", null, (data, matches) => RebateAmount(data) ) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions4567()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (24, "^FULL DUTY IN PART ([0-9]{1,2}) OF SCHEDULE NO. ([0-9]{1,2})$", null, (data, matches) =>
					{
						var schedule = matches.Groups[2].Value;
						var part = matches.Groups[1].Value;
						data.Formula = $"{schedule}P{part}";
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions4570()
		{
			return new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (25, "^([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1})/(GRAM OF THE SUGAR CONTENT THAT EXCEEDS )([0-9,. ]*)(.*)(/100ML)$", null, (data, matches) =>
					{
						var unitValue = matches.Groups[6].Value;
						var value = ConvertValue(matches.Groups[2].Value, matches.Groups[1].Value, string.Empty);
						data.Formula = $"[LI] * 10 * ({value:G29} * MAX(([GJ] - {unitValue}), 0))"; // MD2 - Should be reworked to use the actual values in the formula description
						data.UnitOfMeasureConverted = "GJ";
					}
				) }
			};
		}

		internal static List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)> Actions9999()
		{
			var result = new List<(int, string, Func<FormulaData, bool> criteria, Action<FormulaData, Match>)>
			{
				{ (26, "^THE RATE OF DUTY SPECIFIED IN RESPECT OF THOSE GOODS IN PARTS 1 AND 2[ ]{0,1}OF SCHEDULE NO. 1$", null, (data, matches) => Schedule1Part8(data) ) },
				{ (27, "^THE RATE OF DUTY REFERRED TO IN RESPECT OF VEHICLES OF HEADING 87.03 IN PART 1 AND 2 OF SCHEDULE NO. 1$", null, (data, matches) => Schedule1Part8(data) ) },
				{ (28, "^(THE FULL ANTI-DUMPING DUTY)|(FULL ANTI-DUMPING DUTY)$", null, (data, matches) => data.Formula = "2P1+2P2+2P3" ) },
				{ (29, "^FULL DUTY LESS THE GREATER OF (([0-9,. ]*)[%] OR ([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1})/([^ ]*))$", null, (data, matches) => RebateAmount(data) ) },
				{ (30, "^NOT EXCEEDING THE (DUTY AS)|(DUTIES) CALCULATED IN TERMS OF THE NOTES TO THIS REBATE ITEM$", null, (data, matches) => RebateAmount(data) ) },
				{ (31, "^NOT EXCEEDING THE DUTY IN PART 1 OF SCHEDULE NO. 1 CALCULATED ON THE VALUE REFLECTED ON THE (IMPORT REBATE CREDIT CERTIFICATES)|(PRCC) ISSUED IN THE[ ]{0,1}NAME[ ]{0,1}OF[ ]{0,1}THE IMPORTER AND SUBJECT TO THE NOTE TO THIS ITEM$", null, (data, matches) => RebateAmount(data) ) },
				{ (32, "^NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULE[ ]{0,1}NO. 1 CALCULATED ON THE VALUE REFLECTED ON ANY (IMPORT REBATE CREDIT CERTIFICATES)|(PRCC) ISSUED IN THE[ ]{0,1}NAME[ ]{0,1}OF[ ]{0,1}THE IMPORTER$", null, (data, matches) => RebateAmount(data) ) },
				{ (33, "^NOT EXCEEDING THE DUTY APPLICABLE TO SUCH GOODS IN PART 1 OF SCHEDULE[ ]{0,1}NO. 1 CALCULATED ON THE VALUE REFLECTED ON THE (IMPORT REBATE CREDIT CERTIFICATES)|(PRCC)$", null, (data, matches) => RebateAmount(data) ) },
				{ (34, "^NOT EXCEEDING THE DUTY IN EXCESS OF THE AMOUNT OF DUTY THAT WOULD HAVE[ ]{0,1}BEEN DUE HAD THE GOODS BEEN IMPORTED IN A SINGLE CONSIGNMENT$", null, (data, matches) => RebateAmount(data) ) },
				{ (35, "^FULL DUTY IN SCHEDULE NO. 1 AND SCHEDULE NO. 2$", null, (data, matches) => data.Formula = "1P1+2P1+2P2+2P3" ) },
				{ (36, "^FULL FUEL LEVY AND ROAD ACCIDENT FUND LEVY[ ]{0,1}(SUBJECT TO NOTE [0-9]{1,2}){0,1}$", null, (data, matches) => data.Formula = "15A+15B" ) },
				{ (37, "^THE DUTY IN PART 2A OF SCHEDULE NO. 1$", null, (data, matches) => data.Formula = "12A" ) },
				{ (38, "^NOT EXCEEDING DUTY PAYABLE PER QUARTER FOR EXCISE DUTY PURPOSE$", null, (data, matches) => data.Formula = $"MIN(({data.Tariff.Schedule.DutyFormula}), {{\"Duty payable per quarter for Excise duty purposes\"}})" ) },
				{ (39, "^(([R,r]{0,1})([0-9,. ]*)([ ]{0,1})([C,c]{0,1})/(([^%]*?))) SPIRITS IN THE MIXTURE$", null, (data, matches) => ProcessCommon(data,  Actions1216(), matches.Groups[1].Value) ) },
				{ (40, "^\\(SEE NOTE [1,2]{1} TO THIS PART\\)$", (data) => data.Tariff.Schedule.Schedule == 1 && data.Tariff.Schedule.Part == 2 && data.Tariff.Schedule.Section == "B", (data, matches) =>
					{
						data.Formula = "MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)";
					}
				) },
				// Defaults
				{ (41, "^(.*)$", (FormulaData data) => data.Tariff.Schedule.Schedule == 3 ||
												   data.Tariff.Schedule.Schedule == 4 ||
												   data.Tariff.Schedule.Schedule == 6, (data, matches) => RebateAmount(data) ) },
				{ (42, "^(.*)$", (FormulaData data) => data.Tariff.Schedule.Schedule == 5 && data.Tariff.Schedule.Part == 1, (data, matches) => data.Formula = "{\"Drawback Amount\"}" ) },
				{ (43, "^(.*)$", (FormulaData data) => data.Tariff.Schedule.Schedule == 5 && data.Tariff.Schedule.Part > 1 && data.Tariff.Schedule.Part < 5, (data, matches) => data.Formula = "{\"Refund Amount\"}" ) },
				{ (44, "^(.*)$", (FormulaData data) => data.Tariff.Schedule.Schedule == 5, (data, matches) => data.Formula = "{\"Refund or Drawback Amount\"}" ) },
				{ (45, "^(.*)$", null, (data, matches) => data.Formula = matches.Groups[0].Value ) }
			};

			return result;
		}

		static void FullDutyLess(FormulaData data, Match matches)
		{
			var schedule = matches.Groups[3].Value;
			var part = matches.Groups[2].Value;
			var section = matches.Groups[1].Value;

			var sps = $"{schedule}{part}{section}";
			data.Formula = data.Tariff.Schedule.DutyFormula.Replace(sps, "0");
		}
		static void RebateAmount(FormulaData data) => data.Formula = "{\"Rebate Amount\"}";

		static void Schedule1Part8(FormulaData data) => data.Formula = "1P1+12A+12B";

		static decimal CleanValue(string valueStr)
		{
			return Convert.ToDecimal(valueStr.Replace(",", ".").Replace(" ", ""), CultureInfo.InvariantCulture);
		}

		static decimal ConvertPercentage(string valueStr)
		{
			return CleanValue(valueStr) / 100.0m;
		}

		static decimal ConvertValue(string valueStr, string randInd, string unit)
		{
			var value = CleanValue(valueStr);

			if (randInd != "R")
			{
				value /= 100.0m;
			}

			if (UnitValueMultiplier.TryGetValue(unit, out var multiplier))
			{
				value *= multiplier;
			}

			return value;
		}

		static Dictionary<string, decimal> UnitValueMultiplier = new Dictionary<string, decimal>
		{
			{ "10CIGARETTES", 0.1m },
			{ "10STICKS", 0.1m }
		};

		const string ValueForDuty = "VFD";
	}
}
