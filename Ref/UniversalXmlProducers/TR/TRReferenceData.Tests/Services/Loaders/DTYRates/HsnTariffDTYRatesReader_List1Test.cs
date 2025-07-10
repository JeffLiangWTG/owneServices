using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	[TestFixture]
	public class HsnTariffDTYRatesReader_List1Test : HsnTariffDTYRatesReaderBaseTest<HsnTariffDTYRatesReader>
	{
		protected override HsnTariffDTYRatesReader CreateReader(string tempFolder, ILogger logger)
		{
			var mockReader = new Mock<HsnTariffDTYRatesReader_List1>(tempFolder, logger) { CallBase = true };

			mockReader.Protected()
				.Setup<string>("ListFileName")
				.Returns("DTYRateList.xlsx");

			mockReader.Protected()
				.Setup<string>("ExplanationFileName")
				.Returns("DTYRateExplanation.xlsx");

			return mockReader.Object;
		}
		protected override string ListFileName => "DTYRateList.xlsx";
		protected override string ExplanationFileName => "DTYRateExplanation.xlsx";

		protected override Dictionary<string, (List<HsnTariffDTYRatePreferenceRule>, List<HsnTariffDTYRateFootnoteRule> )> ExpectedRulesByTariffCode =>
		new()
		{
			["020110000000"] = (
				new List<HsnTariffDTYRatePreferenceRule>
				{
					new HsnTariffDTYRatePreferenceRule(
						codeInExcel: "AB",
						preference: "AT",
						formula: "VFD*40/100",
						rateType: "DTY",
						rateCode: "10",
						startDate: DateTime.Parse("2023-01-01 00:00", CultureInfo.InvariantCulture),
						endDate: DateTime.Parse("2076-06-06 23:59", CultureInfo.InvariantCulture),
						footnotes: []),
					new HsnTariffDTYRatePreferenceRule(
						codeInExcel: "BK",
						preference: "BK",
						formula: "VFD*40/100",
						rateType: "DTY",
						rateCode: "10",
						startDate: DateTime.Parse("2023-01-01 00:00", CultureInfo.InvariantCulture),
						endDate: DateTime.Parse("2076-06-06 23:59", CultureInfo.InvariantCulture),
						footnotes: [])
				},
				new List<HsnTariffDTYRateFootnoteRule>()),

			["080111000000"] = (
				new List<HsnTariffDTYRatePreferenceRule>
				{
					new HsnTariffDTYRatePreferenceRule(
						codeInExcel: "AB",
						preference: "AT",
						formula: "VFD*30/100",
						rateType: "DTY",
						rateCode: "10",
						startDate: DateTime.Parse("2023-01-01 00:00", CultureInfo.InvariantCulture),
						endDate: DateTime.Parse("2076-06-06 23:59", CultureInfo.InvariantCulture),
						footnotes: [] )
				},
				new List<HsnTariffDTYRateFootnoteRule>
				{
					new HsnTariffDTYRateFootnoteRule(
						section: "8.FASIL",
						footnoteCode: "1",
						excludingTradingPartners: "None",
						tradingPartners: "CL",
						additionalCode: string.Empty,
						formula: "VFD*30/100*0.50",
						rateType: "DTY",
						rateCode: "10",
						startDate: DateTime.Parse("2023-01-01 00:00", CultureInfo.InvariantCulture),
						endDate: DateTime.Parse("2076-06-06 23:59", CultureInfo.InvariantCulture)
					)
				}
			)
		};
	}
}
