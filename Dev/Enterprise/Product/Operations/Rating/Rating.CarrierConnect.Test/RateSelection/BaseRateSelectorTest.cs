using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;
using FluentAssertions;
using FluentAssertions.Execution;
using WiseRates.Tools;

namespace Enterprise.Rating.CarrierConnect.Test
{
	public class BaseRateSelectorTest : RatingTestCase
	{
		UrsHelper ursHelper;
		protected UrsHelper UrsHelper => ursHelper ??= new (Factory, TestHelper);

		protected void PerformSearchAndAssert(RateQueryDto query, ForwardingConsol consol, ICollection<RateResultDtoAssertion> expected, IRateSelectorProviderFactory providerFactory = null)
		{
			var logger = new MemoryLogger();
			var criteria = new RatingCriteria(consol.RatingAdapter, new BusinessObjectFactory());
			var rateSelectorMetrics = new RateSelectorMetricsModel();
			var response = PerformAutoRate(query, criteria, logger, rateSelectorMetrics, providerFactory);

			FormatAndAssertResults(expected, logger, response);
		}

		protected void PerformSearchAndAssert(RateQueryDto query, ICollection<RateResultDtoAssertion> expected, IRateSelectorProviderFactory providerFactory = null)
		{
			var logger = new MemoryLogger();
			var response = PerformRateSearch(query, logger, providerFactory);

			FormatAndAssertResults(expected, logger, response);
		}

		void FormatAndAssertResults(ICollection<RateResultDtoAssertion> expected, MemoryLogger logger, RateSearchResponseDto response)
		{
			var msgBuilder = new ZStringBuilder();
			msgBuilder.AppendLine();
			msgBuilder.AppendLine();
			msgBuilder.AppendLine();
			msgBuilder.AppendLine("**** RATE SEARCH LOGS *****");
			msgBuilder.Append(string.Join("\r\n", logger.Logs.Select(log => log.ToString())));
			msgBuilder.AppendLine("\r\n**** END *****");

			AssertResponseContainsRates(expected, response, msgBuilder.ToString());
		}

		protected void AssertResponseEquals(ICollection<RateResultDto> expected, RateSearchResponseDto actualResponse)
		{
			var actual = actualResponse.Rates;
			using (new AssertionScope())
			{
				actual.Should().HaveSameCount(expected);
				actual.Should().BeEquivalentTo(expected,
					options => options
						.Using<ICollection<RateChargeDto>>(charges =>
							charges.Expectation
								.Should()
								.BeEquivalentTo(charges.Subject,
									options => options
										.Excluding(charge => charge.Description)
										.Excluding(charge => charge.AutoRateInfo)
										.Excluding(charge => charge.ChargeID)))
						.WhenTypeIs<ICollection<RateChargeDto>>());
			}

			Assert("This test uses FluentAssertions", true);
		}

		protected void AssertResponseContainsRates(ICollection<RateResultDtoAssertion> expected, RateSearchResponseDto actualResponse, string log = "")
		{
			var missing = expected.ToList();
			var unexpected = new List<RateResultDto>();
			var actual = actualResponse.Rates;

			foreach (var result in actual)
			{
				var chargePks = result.Charges.Select(charge => charge.SourcePK).ToList();
				var expectedMatchIndex = missing.FindIndex(exp =>
					result.SourcePKs.SequenceEqualIgnoringOrder(exp.RateEntries.Select(re => re.PK.ToGuid()))
					&& chargePks.SequenceEqualIgnoringOrder(exp.RateLines.Select(rl => rl.PK.ToGuid()))
					&& (exp.TransportProvider == null || result.ServiceProvider.OrgCode == exp.TransportProvider.OH_Code)
					&& (exp.Containers == null || result.ContainerTypes.SequenceEqualIgnoringOrder(exp.Containers))
					&& (exp.CarrierContractNumber == null || result.CarrierContractNumber == exp.CarrierContractNumber)
					&& (exp.CarrierServiceLevel == null || result.CarrierServiceLevel == exp.CarrierServiceLevel)
					&& ContainerCommoditySetEquals(exp.PerContainerCommodity, result.PerContainerCommodity));

				if (expectedMatchIndex >= 0)
				{
					if (missing[expectedMatchIndex].Charges.Count > 0)
					{
						AssertContainsCharges(missing[expectedMatchIndex].Charges, result.Charges);
					}

					missing.RemoveAt(expectedMatchIndex);
				}
				else
				{
					unexpected.Add(result);
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals($"{missing.Count} expected result(s) are missing.\n{string.Join("\n\n", missing.Select(rate => rate.ToString()))}{log}", 0, missing.Count);
				AssertEquals($"{unexpected.Count} unexpected result(s) were found.\n{string.Join("\n\n", unexpected.Select(GetDtoName))}{log}", 0, unexpected.Count);
			});
		}

		string GetDtoName(RateResultDto dto)
		{
			var chargesInfo = string.Join(", ", dto.Charges.Select(charge =>
				$"{charge.ChargeCode}-{charge.ContainerType ?? "N/A"}-{charge.CommodityCode ?? "N/A"}"));

			var perContainerCommodityInfo = dto.PerContainerCommodity != null
				? "\nPerContainerCommodity: " + string.Join(", ", dto.PerContainerCommodity.Select(pcc => $"{pcc.Container}-{pcc.Commodity}-{pcc.Remarks}-{pcc.ChargeableFactor}-{pcc.AddOn}-{pcc.RateType}-{pcc.RateType2}"))
				: "";

			return $"{dto.Origin}-{dto.Destination}-{dto.TransportMode}-{dto.ContainerMode}-{dto.ServiceProvider?.OrgCode ?? "N/A"}-{dto.CarrierContractNumber ?? "N/A"}\n" +
				   chargesInfo +
				   perContainerCommodityInfo;
		}

		public static bool ContainerCommoditySetEquals(IEnumerable<PerContainerCommodityDto> first, IEnumerable<PerContainerCommodityDto> second)
		{
			return first == null || SortPerContainerCommodity(second).SequenceEqual(SortPerContainerCommodity(first));
		}

		static IEnumerable<PerContainerCommodityDto> SortPerContainerCommodity(IEnumerable<PerContainerCommodityDto> commodities)
		{
			return commodities
				.OrderBy(x => x.Container)
				.ThenBy(x => x.Commodity)
				.ThenBy(x => x.Remarks)
				.ThenBy(x => x.ChargeableFactor)
				.ThenBy(x => x.AddOn)
				.ThenBy(x => x.RateType)
				.ThenBy(x => x.RateType2);
		}

		protected void AssertContainsCharges(ICollection<RateChargeDtoAssertion> expected, ICollection<RateChargeDto> actual) =>
			actual.Should().BeEquivalentTo(expected, options => options.Excluding(ctx => !expected.First().HasValue(ctx.Name)));

		protected RateQueryDto CreateRateQuery(string transportMode, string containerMode, string origin, string destination, ZDateTime? effectiveDate = null) =>
			new()
			{
				TransportMode = transportMode,
				ContainerMode = containerMode,
				Origin = origin,
				Destination = destination,
				EffectiveDate = effectiveDate?.ToDateTime(),
				Context = RateQueryDto.RateSearchContext.RateSearch,
				RateTypes = ["Forwarding"],
				JobInfo = new JobInfoDto
				{
					Containers = Array.Empty<JobContainerDto>()
				}
			};

		protected RateQueryDto CreateRateQuery(ForwardingConsol consol, string origin = null, string destination = null) =>
			new()
			{
				TransportMode = consol.JK_TransportMode,
				ContainerMode = consol.JK_ConsolMode,
				Origin = origin ?? consol.JK_RL_NKLoadPort,
				Destination = destination ?? consol.JK_RL_NKDischargePort,
				EffectiveDate = new DateTime(),
				Context = RateQueryDto.RateSearchContext.RateSearch,
				RateTypes = ["Forwarding"],
				JobInfo = new JobInfoDto
				{
					Containers = Array.Empty<JobContainerDto>()
				}
			};

		protected RateSearchResponseDto PerformAutoRate(
			RateQueryDto rateQueryDto,
			RatingCriteria criteria,
			ILogger logger = null,
			RateSelectorMetricsModel rateSelectorMetrics = null,
			IRateSelectorProviderFactory providerFactory = null
		) {
			using (_Rating.Start(new LoggerDecorator(logger)))
			using (_Rating.StartCost())
			using (RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				if (providerFactory is not null)
				{
					ObjectFactory.Substitute(providerFactory);
				}

				return new RateSelectorService(logger).SearchAndCalculateRatesUsingCriteria(criteria, rateQueryDto, rateSelectorMetrics, string.Empty);
			}
		}

		protected RateSearchResponseDto PerformRateSearch(RateQueryDto rateQueryDto, ILogger logger = null, IRateSelectorProviderFactory providerFactory = null)
		{
			using (RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				if (providerFactory is not null)
				{
					ObjectFactory.Substitute(providerFactory);
				}
				return new RateSelectorService(logger).SearchAndCalculateRates(rateQueryDto);
			}
		}

		protected TestHelper TestHelper => testHelper ??= new TestHelper(Factory);
		TestHelper testHelper;
	}
}
