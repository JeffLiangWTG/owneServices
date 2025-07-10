using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.CarrierConnect;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Rating.Business.RatingUsageCollector;
using static Enterprise.Rating.CarrierConnect.RateSelection.Models.RateQueryDto;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.Testing;

[UseSnapshotProtection]
public class GlowRateSelectorTest : RatingTestCase
{
	#region Filter Pre-Fill

	[TestDate(2024, 12, 1, 10, 0, 0)]
	public void TestSendNoContainerizedContextDataToBrowserWhenShowDialog()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
		{
			InvokeActionAndCheckBaseInfo(MockIAutoRating(CreateNotContainerizedRateableMeasureSet(), "LSE"));
		}

		var rateQueryFilterData = (RateQueryFilterData)testRunner.UpdateFilterData;
		var rateQueryDto = rateQueryFilterData.RateQueryDto;
		var jobInfo = rateQueryDto.JobInfo;
		AssertNotNull(jobInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Containers count should should be 1", 1, jobInfo.Containers.Count());
			var jobInfoContainer = jobInfo.Containers.First();
			AssertEquals("GEN, HAZ", jobInfoContainer.Commodity);

			AssertEquals("PackLines count should should be 1", 1, jobInfoContainer.PackLines.Length);
			var jobInfoPackLine = jobInfoContainer.PackLines[0];
			AssertEquals("Commodity should be set correctly", "GEN, HAZ", jobInfoPackLine.Commodity);
			AssertEquals("ChargeableOverride should be set correctly", 1666m, jobInfoPackLine.ChargeableOverride);
			AssertEquals("ChargeableUnit should be set correctly", "KG", jobInfoPackLine.ChargeableUnit);
		});
	}

	[TestDate(2024, 12, 1, 10, 0, 0)]
	public void TestSendContainerizedContextDataToBrowserWhenShowDialog()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
		{
			InvokeActionAndCheckBaseInfo(MockIAutoRating(CreateContainerizedRateableMeasureSet(), "ULD"));
		}

		var rateQueryFilterData = (RateQueryFilterData)testRunner.UpdateFilterData;
		var rateQueryDto = rateQueryFilterData.RateQueryDto;
		var jobInfo = rateQueryDto.JobInfo;
		AssertNotNull(jobInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Containers count should should be 1", 1, jobInfo.Containers.Count());
			var jobInfoContainer = jobInfo.Containers.First();
			AssertEquals("ContainerType should be set correctly", "AAP", jobInfoContainer.ContainerType);
			AssertEquals("Number should be set correctly", "CAPT0830228", jobInfoContainer.Number);
			AssertEquals("Count should be set correctly", 1, jobInfoContainer.Count);
			AssertEquals("Commodity should be set correctly", "APRD", jobInfoContainer.Commodity);

			AssertEquals("PackLines length should should be 1", 1, jobInfoContainer.PackLines.Length);
			var jobInfoPackLine = jobInfoContainer.PackLines[0];
			AssertEquals("Count should be set correctly", 10, jobInfoPackLine.Count);
			AssertContainsExactElementsInAnyOrder("Commodity should be set correctly", "APRD", jobInfoPackLine.Commodity);
			AssertEquals("Weight should be set correctly", 20m, jobInfoPackLine.Weight);
			AssertEquals("WeightUnit should be set correctly", "KG", jobInfoPackLine.WeightUnit);
			AssertEquals("Volume should be set correctly", 30m, jobInfoPackLine.Volume);
			AssertEquals("VolumeUnit should be set correctly", "M3", jobInfoPackLine.VolumeUnit);
		});
	}

	void InvokeActionAndCheckBaseInfo(IAutoRating autoRating)
	{
		var criteria = new RatingCriteria(autoRating, Factory);

		testRunner.Run(criteria);

		AssertNotNull(testRunner.UpdateFilterData);
		AssertType<RateQueryFilterData>(testRunner.UpdateFilterData);

		var rateQueryFilterData = (RateQueryFilterData)testRunner.UpdateFilterData;
		var rateQueryDto = rateQueryFilterData.RateQueryDto;
		CombineAssertions(() =>
		{
			AssertEquals("JobID should be set correctly", autoRating.JobID, rateQueryFilterData.JobID);
			AssertEquals("JobShortcutUrl should be set correctly", "", rateQueryFilterData.JobShortcutUrl);
			AssertEquals("JobType should be set correctly", autoRating.AdapterType, rateQueryFilterData.JobType);
			AssertEquals("TransportMode should be set correctly", autoRating.InvoicingSupporter.TransportMode, rateQueryDto.TransportMode);
			AssertEquals("ContainerMode should be set correctly", autoRating.InvoicingSupporter.ContainerMode, rateQueryDto.ContainerMode);
			AssertEquals("Origin should be set correctly", autoRating.InvoicingSupporter.Origin.Code, rateQueryDto.Origin);
			AssertEquals("Destination should be set correctly", autoRating.InvoicingSupporter.Destination.Code, rateQueryDto.Destination);
			AssertEquals("Context should be set correctly", RateSearchContext.AutoRating, rateQueryDto.Context);
			AssertArrayEqualsByElements("RateTypes should be set correctly", ["Forwarding"], rateQueryDto.RateTypes);
			AssertEquals("EffectiveDate should be set correctly", ZDateTime.BrettsBirthday.ToDateTime(), rateQueryDto.EffectiveDate);
		});

		var rateFilterDto = rateQueryFilterData.RateFilterDto;
		CombineAssertions(() =>
		{
			AssertEquals("ServiceProviderCode count should should be 1", 1, rateFilterDto.ServiceProviderCode.Count);
			AssertEquals("CarrierContractNumber count should should be 1", 1, rateFilterDto.CarrierContractNumber.Count);
			AssertEquals("CarrierServiceLevel count should should be 1", 1, rateFilterDto.CarrierServiceLevel.Count);
			AssertEquals("NamedAccount count should should be 1", 1, rateFilterDto.NamedAccount.Count);
			AssertEquals("PaymentTerm count should should be 1", 1, rateFilterDto.PaymentTerm.Count);
			AssertCollectionContains("ServiceProviderCode should contains CARRIER1234", "CARRIER1234", rateFilterDto.ServiceProviderCode);
			AssertCollectionContains("Consignee should contains CONSIGNEE1", "CONSIGNEE1", rateFilterDto.Consignee);
			AssertCollectionContains("Consignee should contains CONSIGNOR1", "CONSIGNOR1", rateFilterDto.Consignor);
			AssertEquals("CarrierContractNumber should be set correctly", "123456", rateFilterDto.CarrierContractNumber[0]);
			AssertEquals("CarrierServiceLevel should be set correctly", "STD", rateFilterDto.CarrierServiceLevel[0]);
			AssertEquals("NamedAccount should be set correctly", "Named Account", rateFilterDto.NamedAccount[0]);
			AssertEquals("PaymentTerm should be set correctly", "PDD", rateFilterDto.PaymentTerm[0]);
		});
	}

	#endregion

	#region Apply Charges

	public void TestApplyCharges_WithAllCharges_AppliesAllCharges()
	{
		SetupCostingForTransportProvider1();

		RateResultDto resultDto = null;
		testRunner.Run(new RatingCriteria(CreateForwardingConsol().RatingAdapter, Factory), () =>
		{
			resultDto = SearchForRatesAndCheckResult(new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
			});
			testRunner.SendMessageFromBrowser(GlowRateSelectorMessageType.ApplyRatesRequest, new ApplyRateRequestDto
			{
				AutoratingDate = null,
				RateId = resultDto!.ResultId,
				ChargesToApply = resultDto.Charges.Select(charge => charge.ChargeID).ToList(),
				JobChangesAccepted = [],
			});
		});

		var result = testRunner.GlowRateSelectorResult;
		CombineAssertions(() =>
		{
			AssertEquals("Outcome should be ApplyRates", GlowRateSelectorOutcome.ApplyRates, result.Outcome);
			AssertEquals("Both charges should be applied", 2, result.SelectedCharges.Count);
			AssertEquals("Correct rate should be applied", resultDto.ResultId, result.Rate.ResultId);
		});
	}

	public void TestApplyCharges_WithOptionalCharges_OnlyAppliesSelectedCharges()
	{
		SetupCostingForTransportProvider1();

		RateResultDto resultDto = null;
		testRunner.Run(new RatingCriteria(CreateForwardingConsol().RatingAdapter, Factory), () =>
		{
			resultDto = SearchForRatesAndCheckResult(new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
			});
			testRunner.SendMessageFromBrowser(GlowRateSelectorMessageType.ApplyRatesRequest, new ApplyRateRequestDto
			{
				AutoratingDate = null,
				RateId = resultDto!.ResultId,
				ChargesToApply = [resultDto!.Charges.First(charge => charge.ChargeCode.ChargeCode == "FRT").ChargeID],
				JobChangesAccepted = [],
			});
		});

		var result = testRunner.GlowRateSelectorResult;
		CombineAssertions(() =>
		{
			AssertEquals("Outcome should be ApplyRates", GlowRateSelectorOutcome.ApplyRates, result!.Outcome);
			AssertEquals("Only the selected charge should be applied", 1, result.SelectedCharges!.Count);
			AssertEquals("Correct rate should be applied", resultDto.ResultId, result.Rate.ResultId);
		});
	}

	public void TestApplyCharges_AppliesRelatedCharges()
	{
		// Arrange
		var consol = CreateForwardingConsol();
		consol.JK_OA_CreditorAddress = ZGuid.Empty;
		consol.JK_CarrierContractNumber = ZString.Empty;

		var costing = Helper.NewCosting(Consignor);
		var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true, commodity: "GEN");
		costingEntry.AddFlatRateLine("BAF", 10, "AUD");

		SetupCostingForTransportProvider1();

		var criteria = new RatingCriteria(consol.RatingAdapter, Factory);
		criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(Consignor, ["Test"]));

		Factory.Save();

		// Act
		testRunner.Run(criteria, () =>
		{
			var resultDto = SearchForRatesAndCheckResult(new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
			});
			testRunner.SendMessageFromBrowser(GlowRateSelectorMessageType.ApplyRatesRequest, new ApplyRateRequestDto
			{
				AutoratingDate = null,
				RateId = resultDto!.ResultId,
				ChargesToApply = [.. resultDto.Charges.Select(charge => charge.ChargeID)],
				JobChangesAccepted = [new() { Type = JobConfirmationType.Carrier, Accepted = true }],
			});
		});

		// Assert
		var result = testRunner.GlowRateSelectorResult;
		AssertNotNull(result);
		AssertEquals("Outcome should be ApplyRates", GlowRateSelectorOutcome.ApplyRates, result!.Outcome);
		AssertContainsExactElementsInAnyOrder("Related charge should be present in selected charges",
			["FSC", "FRT", "BAF"],
			result.SelectedCharges!.Select(charge => charge.ChargeCode.AC_Code));
	}

	public void TestApplyZeroCharges_IsTrue_AppliesZeroCharges()
	{
		SetupCostingEntryWithZeroAmountCharges();

		RateResultDto resultDto = null;
		testRunner.Run(new RatingCriteria(CreateForwardingConsol().RatingAdapter, Factory), () =>
		{
			resultDto = SearchForRatesAndCheckResult(new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
			});
			testRunner.SendMessageFromBrowser(GlowRateSelectorMessageType.ApplyRatesRequest, new ApplyRateRequestDto
			{
				AutoratingDate = null,
				RateId = resultDto!.ResultId,
				ChargesToApply = resultDto.Charges.Select(charge => charge.ChargeID).ToList(),
				JobChangesAccepted = [],
				ApplyZeroCharges = true,
			});
		});

		var result = testRunner.GlowRateSelectorResult;
		CombineAssertions(() =>
		{
			AssertEquals("Outcome should be ApplyRates", GlowRateSelectorOutcome.ApplyRates, result.Outcome);
			AssertEquals("All charges should be applied", 2, result.SelectedCharges.Count);
			AssertEquals("Correct rate should be applied", resultDto.ResultId, result.Rate.ResultId);
		});
	}

	public void TestApplyZeroCharges_IsFalse_DoesNotAppliesZeroCharges()
	{
		SetupCostingEntryWithZeroAmountCharges();

		RateResultDto resultDto = null;
		testRunner.Run(new RatingCriteria(CreateForwardingConsol().RatingAdapter, Factory), () =>
		{
			resultDto = SearchForRatesAndCheckResult(new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
			});
			testRunner.SendMessageFromBrowser(GlowRateSelectorMessageType.ApplyRatesRequest, new ApplyRateRequestDto
			{
				AutoratingDate = null,
				RateId = resultDto!.ResultId,
				ChargesToApply = resultDto.Charges.Select(charge => charge.ChargeID).ToList(),
				JobChangesAccepted = [],
				ApplyZeroCharges = false,
			});
		});

		var result = testRunner.GlowRateSelectorResult;
		CombineAssertions(() =>
		{
			AssertEquals("Outcome should be ApplyRates", GlowRateSelectorOutcome.ApplyRates, result.Outcome);
			AssertEquals("Non-Zero charges not should be applied", 1, result.SelectedCharges.Count);
			AssertEquals("Correct rate should be applied", resultDto.ResultId, result.Rate.ResultId);
		});
	}

	#endregion

	#region Rate Selector Reporting

	public void TestReportRateSelector_WhenApplyingRates() =>
		TestReportRateSelector(() =>
		{
			var resultDto = SearchForRatesAndCheckResult(new RateQueryDto
			{
				Origin = "AUMEL",
				Destination = "USLAX",
			});
			testRunner.SendMessageFromBrowser(GlowRateSelectorMessageType.ApplyRatesRequest, new ApplyRateRequestDto
			{
				AutoratingDate = null,
				RateId = resultDto!.ResultId,
				ChargesToApply = resultDto.Charges.Select(charge => charge.ChargeID).ToList(),
				JobChangesAccepted = [],
			});
		}, "Select");

	public void TestReportRateSelector_WhenAutoratingWithoutSelection() =>
		TestReportRateSelector(() =>
			testRunner.SendMessageFromBrowser(
				GlowRateSelectorMessageType.AbortSessionRequest,
				new AbortSessionRequestDto { ContinueAutorating = true })
		, "Skip");

	public void TestReportRateSelector_WhenAbortingSession() =>
		TestReportRateSelector(() =>
			testRunner.SendMessageFromBrowser(
				GlowRateSelectorMessageType.AbortSessionRequest,
				new AbortSessionRequestDto { ContinueAutorating = false })
		, "Cancel");

	void TestReportRateSelector(Action action, string expectedUsageAction)
	{
		SetupCostingForTransportProvider1();
		testRunner.Run(new RatingCriteria(CreateForwardingConsol().RatingAdapter, Factory), action);

		var actualAction = new UsageCollectorTestHelper(Factory)
			.LoadUsageMessages(UsageFeatures.Codes.RateSelector)[0]
			.GetProperty<string>(UsageProperties.Action);

		AssertEquals($"Reported action should be '{expectedUsageAction}'.", expectedUsageAction, actualAction);
	}

	public void TestReportRateSelectorSearch_WhenURSEnabled()
	{
		using (ObjectFactory.Substitute(MockURSFeatureHelper(true)))
		{
			var cw1Header = SetupCostingForTransportProvider1();
			var ursHeader = SetupUrsCostingForTransportProvider1();
			var mockProviderFactory = UrsHelper.MockProviderFactory([
				UrsHelper.MockProvider(cw1Header.ChildRateEntries.ToList(), RateProvider.CW1),
				UrsHelper.MockProvider(ursHeader.ChildRateEntries.ToList(), RateProvider.URS)
			]);
			ObjectFactory.Substitute(mockProviderFactory.Object);

			testRunner.Run(
				new RatingCriteria(CreateForwardingConsol().RatingAdapter, Factory),
				() => SearchForRatesAndCheckResult(new RateQueryDto
				{
					Origin = "AUMEL",
					Destination = "USLAX",
				}));

			var actual = new UsageCollectorTestHelper(Factory)
				.LoadUsageMessages(UsageFeatures.Codes.RateSelectorSearch)[0]
				.GetProperty<UsageRatesSearchResult>(UsageProperties.RatesSearchResult);

			CombineAssertions(() =>
			{
				AssertEquals("URS total rates is logged", 1, actual.URS.TotalRates);
				AssertEquals("CW1 total rates is logged", 1, actual.CW1.TotalRates);
			});
		}
	}

	public void TestReportRateSelectorSearch_WhenURSDisabled()
	{
		using (ObjectFactory.Substitute(MockURSFeatureHelper(false)))
		{
			SetupCostingForTransportProvider1();

			testRunner.Run(
				new RatingCriteria(CreateForwardingConsol().RatingAdapter, Factory),
				() => SearchForRatesAndCheckResult(new RateQueryDto
				{
					Origin = "AUMEL",
					Destination = "USLAX",
				}));

			var actual = new UsageCollectorTestHelper(Factory)
				.LoadUsageMessages(UsageFeatures.Codes.RateSelectorSearch)[0]
				.GetProperty<UsageRatesSearchResult>(UsageProperties.RatesSearchResult);

			CombineAssertions(() =>
			{
				AssertEquals("URS total rates is not logged", 0, actual.URS.TotalRates);
				AssertEquals("CW1 total rates is logged", 1, actual.CW1.TotalRates);
			});
		}
	}

	#endregion

	#region Implementation

	GlowRateSelectorTestRunner testRunner;
	IDisposable nonTransactionedDisposable;

	UrsHelper ursHelper;
	UrsHelper UrsHelper => ursHelper ??= new (Factory, Helper);

	protected override void SetUp()
	{
		testRunner = new();
		// Required for 'Browser' thread to load BusinessObjects saved from main thread.
		nonTransactionedDisposable = RunNonTransactioned();
	}

	protected override void TearDown()
	{
		testRunner.Dispose();
		nonTransactionedDisposable.Dispose();
	}

	#endregion

	#region Helpers

	RateResultDto SearchForRatesAndCheckResult(RateQueryDto query)
	{
		testRunner.SendMessageFromBrowser(GlowRateSelectorMessageType.RateSearchRequest, query);

		var sentToBrowser = PollingHelper.PollUntilNotNull(() => testRunner.GetLastMessageSentToBrowser(), testRunner.CancellationToken);
		Assert(sentToBrowser.HasValue);
		var asyncResponse = (AsyncRateResponseDto)sentToBrowser!.Value.data;
		AssertNotNull(asyncResponse);

		var result = PollingHelper.PollUntilNotNull(() =>
			new BusinessObjectFactory().Load<RateSearchResult>(new ZGuid(asyncResponse.RequestId)),
			testRunner.CancellationToken);
		AssertNotNull("RateSearchResult should be created by PerformRateSearch", result);

		var resultDto = JsonConvert.DeserializeObject<RateSearchResponseDto>(result!.RR_JsonContent.ToUTF8())!.Rates.FirstOrDefault();
		AssertNotNull("No rates were within the result DTO", resultDto);

		return resultDto!;
	}

	Costing SetupCostingForTransportProvider1()
	{
		var costing = Helper.NewCosting(TransportProvider1);
		var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true, commodity: "GEN");
		costingEntry.AddFlatRateLine("FRT", 20m, "AUD");
		costingEntry.AddFlatRateLine("FSC", 50m, "AUD");
		Factory.Save();
		return costing;
	}

	Costing SetupCostingEntryWithZeroAmountCharges()
	{
		var costing = Helper.NewCosting(TransportProvider1);
		var entry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true, commodity: "GEN");
		entry.AddFlatRateLine("FRT", 20m, "AUD");
		entry.AddFlatRateLine("FSC", 0m, "AUD");
		Factory.Save();
		return costing;
	}

	UrsRatingHeader SetupUrsCostingForTransportProvider1()
	{
		var (entry, line) = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", containerCode: "20GP");
		return new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1)) { ChildRateEntries = [entry] };
	}

	ForwardingConsol CreateForwardingConsol(string origin = "AUMEL", string destination = "USLAX")
	{
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_RL_NKLoadPort = origin;
		consol.JK_RL_NKDischargePort = destination;
		consol.JK_TransportMode = Constants.TransportModes.Sea;
		consol.JK_ConsolMode = Constants.ContainerModes.FCL;
		consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
		consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
		consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
		consol.JK_CarrierContractNumber = "Test123";
		consol.JK_PrepaidCollect = "PPD";

		var container = consol.Containers.AddNew();
		container.JC_ContainerNum = "TEST1234";
		container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

		var shipment = consol.Shipments.AddNew();
		shipment.JS_RL_NKOrigin = origin;
		shipment.JS_RL_NKDestination = destination;
		shipment.JS_TransportMode = Constants.TransportModes.Sea;
		shipment.JS_PackingMode = Constants.ContainerModes.FCL;
		shipment.JS_ActualVolume = 0.5M;
		shipment.JS_ActualWeight = 55M;
		shipment.ConsigneePK = Consignee.PK;
		shipment.ConsignorPK = Consignor.PK;

		Factory.Save();
		return consol;
	}

	IAutoRating MockIAutoRating(RateableMeasureSet rateableMeasureSet, string containerMode)
	{
		var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
		invoicingSupporterMock.SetupGet(f => f.TransportMode).Returns("AIR");
		invoicingSupporterMock.SetupGet(f => f.ContainerMode).Returns(containerMode);

		var origin = Factory.New<RefUNLOCO>();
		origin.Code = "AUKAX";
		invoicingSupporterMock.SetupGet(t => t.Origin).Returns(origin);

		var destination = Factory.New<RefUNLOCO>();
		destination.Code = "USSGE";
		invoicingSupporterMock.SetupGet(t => t.Destination).Returns(destination);
		invoicingSupporterMock.SetupGet(t => t.ETD).Returns(ZDateTime.Now);

		var refAirline = Factory.NewWithPrimaryKey<RefAirline>(Guid.NewGuid());
		refAirline.RM_TwoCharacterCode = "OM";
		var orgMiscServ = Factory.New<OrgMiscServ>();
		orgMiscServ.OM_RM_Airline = refAirline.PK;

		var carrier = Factory.New<OrgHeader>();
		carrier.OH_Code = "CARRIER1234";
		carrier.OH_FullName = "Carrier Name";
		carrier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "USCO");
		carrier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "USC");
		carrier.MiscServ = orgMiscServ;

		var credit = Factory.New<OrgHeader>();
		credit.OH_Code = "CREDITOR1234";
		credit.OH_FullName = "Creditor Name";
		credit.SetLocalCustomsCode(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "USEO");
		credit.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "USE");
		credit.MiscServ = orgMiscServ;
		var creditors = new Creditors { ["dummy"] = new OrgPrioritizedList(OrgWithSource.New(credit, ["Creditor"])) };

		var serviceLevelRatingInformation = new ServiceLevelRatingInformation(new ServiceLevelInfo("STD", ServiceLevelType.Carrier));

		var paymentTermInfos = new PaymentTermInfos();
		paymentTermInfos.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Cost, "PDD"));

		var debtorOrgs = new DebtorOrgCollection
		{
			[RatingDebtorOrgTypes.CNE] = Consignee,
			[RatingDebtorOrgTypes.CNR] = Consignor
		};

		var autoRatingMock = new Mock<IAutoRating>();
		var manualRateSectionMock = autoRatingMock.As<IManualRateSelectionSupporter>();
		autoRatingMock.SetupGet(a => a.JobID).Returns("JobID1234");
		autoRatingMock.SetupGet(a => a.AdapterType).Returns(AdapterType.Consolidation);
		autoRatingMock.SetupGet(a => a.RateTypeToUse).Returns(RateType.Forwarding);
		autoRatingMock.SetupGet(a => a.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
		autoRatingMock.SetupGet(a => a.RateableMeasures).Returns(rateableMeasureSet);
		autoRatingMock.SetupGet(a => a.Carrier).Returns(carrier);
		autoRatingMock.SetupGet(a => a.Creditors).Returns(creditors);
		autoRatingMock.SetupGet(a => a.DebtorOrgs).Returns(debtorOrgs);
		autoRatingMock.SetupGet(a => a.PossibleServiceProviders).Returns([carrier]);
		autoRatingMock.SetupGet(a => a.CarrierContractNumbers).Returns(["123456"]);
		autoRatingMock.SetupGet(a => a.ServiceLevel).Returns(serviceLevelRatingInformation);
		autoRatingMock.SetupGet(a => a.NamedAccount).Returns("Named Account");
		autoRatingMock.SetupGet(a => a.PaymentTerm).Returns(paymentTermInfos);
		autoRatingMock.SetupGet(a => a.JobDatesProvider).Returns(new TestJobDatesProvider(origin, ZDateTime.BrettsBirthday));
		autoRatingMock.SetupGet(a => a.Origin).Returns(origin);
		autoRatingMock.SetupGet(a => a.Destination).Returns(destination);
		manualRateSectionMock.SetupGet(a => a.DefaultFilterValueForOrigin).Returns(origin);
		manualRateSectionMock.SetupGet(a => a.DefaultFilterValueForDestination).Returns(destination);

		return autoRatingMock.Object;
	}

	RateableMeasureSet CreateNotContainerizedRateableMeasureSet()
	{
		var rateableMeasureSet = new RateableMeasureSet(AdapterType.Consolidation);

		var packagesPart = new RateablePartList();
		var packagePartGEN = new RateablePart();
		packagePartGEN.CommodityCode = "GEN";
		packagesPart.AddPart(packagePartGEN);

		var packagePartHAZ = new RateablePart();
		packagePartHAZ.CommodityCode = "HAZ";
		packagesPart.AddPart(packagePartHAZ);
		rateableMeasureSet.AddPartList(MeasureType.Package, packagesPart);

		rateableMeasureSet.SetQuantity(MeasureType.Chargeable, 1666m, "KG");
		rateableMeasureSet.SetQuantity(MeasureType.JobWeight, 20m, "KG");
		rateableMeasureSet.SetQuantity(MeasureType.JobVolume, 30m, "M3");

		return rateableMeasureSet;
	}

	RateableMeasureSet CreateContainerizedRateableMeasureSet()
	{
		var rateableMeasureSet = new RateableMeasureSet(AdapterType.Consolidation);

		var refContainer = Factory.NewWithValidTestData<RefContainer>();
		refContainer.RC_Code = "AAP";
		Factory.Save();
		var containerInfo = new MeasureInfo.ContainerInfo(containerNumber: "CAPT0830228", containerCount: 1, packages: 10, weight: 20, weightUnit: "KG", volume: 30, volumeUnit: "M3");
		rateableMeasureSet.CreateContainerList(includeOwnership: true, includeContainerNumber: true);
		rateableMeasureSet.AddContainerGroup(refContainer.PK, "APRD", [containerInfo]);

		return rateableMeasureSet;
	}

	IFeatureControlManager MockURSFeatureHelper(bool enabled)
	{
		var ursRule = new RatingFeatureHelper.Urs.UrsFeatureRule { Enabled = enabled, Url = "https://fcm.cargowise.com" };
		var featureDataMock = new Mock<IFeatureData>();
		var featureControlMock = new Mock<IFeatureControlManager>();
		featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
		featureControlMock
			.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None))
			.Returns(Task.FromResult(featureDataMock.Object));

		return featureControlMock.Object;
	}

	#endregion
}

class TestJobDatesProvider(BusinessObject parent, ZDateTime costingAutoratingDate) : JobDatesProvider<BusinessObject>(parent)
{
	protected override ZDateTime GetCostingAutoratingDateOverrideCore() => costingAutoratingDate;
}
