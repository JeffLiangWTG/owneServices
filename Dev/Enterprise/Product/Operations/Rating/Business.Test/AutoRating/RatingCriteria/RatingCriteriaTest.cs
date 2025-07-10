using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingCriteriaTest : RatingTestCase
	{
		public void TestAgentClientRelatedParties()
		{
			var criteria = new TestRatingCriteria();
			criteria.OverseasAgent = Factory.New<OrgHeader>();
			criteria.LocalClient = Factory.New<OrgHeader>();
			criteria.Consignee = Factory.New<OrgHeader>();
			criteria.Consignor = Factory.New<OrgHeader>();

			criteria.JobDirection = Directions.Import;
			AssertContainsExactElementsInAnyOrder(new[] { criteria.OverseasAgent, criteria.Consignor }, criteria.AgentWithRelatedParty);
			AssertContainsExactElementsInAnyOrder(new[] { criteria.LocalClient, criteria.Consignee }, criteria.BillToWithRelatedParty);

			criteria.JobDirection = Directions.Export;
			AssertContainsExactElementsInAnyOrder(new[] { criteria.OverseasAgent, criteria.Consignee }, criteria.AgentWithRelatedParty);
			AssertContainsExactElementsInAnyOrder(new[] { criteria.LocalClient, criteria.Consignor }, criteria.BillToWithRelatedParty);

			criteria.Consignor = criteria.LocalClient;
			AssertContainsExactElementsInAnyOrder(new[] { criteria.OverseasAgent, criteria.Consignee }, criteria.AgentWithRelatedParty);
			AssertContainsExactElementsInAnyOrder(new[] { criteria.LocalClient }, criteria.BillToWithRelatedParty);
		}

		public void TestCreditors_WhenInvalidCreditor_ThrowAutoratingException_WithHumanReadableName()
		{
			var mockCriteria = new Mock<TestRatingCriteria>();
			mockCriteria.CallBase = true;
			mockCriteria
				.Protected()
				.Setup("GetCreditorsCore")
				.Throws(new OrgWithSourceNotFoundException("bbb", true, "aaa", ZGuid.BrettsGuid));

			var exception = AssertExceptionThrown<AutoRaterException>(() => mockCriteria.Object.Creditors.Any());

			AssertEquals("There was an invalid reference detected. Invalid reference in aaa", exception.Message);
		}

		public void TestCreditors_WhenInvalidCreditor_ThrowAutoratingException_WithoutHumanReadableName()
		{
			var mockCriteria = new Mock<TestRatingCriteria>();
			mockCriteria.CallBase = true;
			mockCriteria
				.Protected()
				.Setup("GetCreditorsCore")
				.Throws(new OrgWithSourceNotFoundException("bbb", false, "aaa", ZGuid.BrettsGuid));

			var exception = AssertExceptionThrown<AutoRaterException>(() => mockCriteria.Object.Creditors.Any());

			AssertEquals("There was an invalid reference detected. Invalid reference in bbb", exception.Message);
		}

		public void TestToXml_WhenExceptionHappens_SerializesExceptionInstead()
		{
			var mockCriteria = new Mock<TestRatingCriteria>();
			mockCriteria.CallBase = true;
			mockCriteria.Setup(x => x.GetRatingAdapter(It.IsAny<bool>())).Throws(new Exception("Today is Friday"));

			var criteria = mockCriteria.Object;
			var xmlString = criteria.ToXML();
			AssertContains("<Message>Today is Friday</Message>", xmlString);
			AssertContains("<Source>Moq</Source>", xmlString);
		}

		public void TestToXml_WhenExceptionHappensDuringExceptionSerializeing_ProvideSimpleMessage()
		{
			var mockException = new Mock<Exception>();
			mockException.Setup(x => x.Message).Returns("Today is Friday");
			mockException.Setup(x => x.Source).Throws(new Exception("This isnt fair!"));

			var mockCriteria = new Mock<TestRatingCriteria>();
			mockCriteria.CallBase = true;
			mockCriteria.Setup(x => x.GetRatingAdapter(It.IsAny<bool>())).Throws(mockException.Object);

			var criteria = mockCriteria.Object;
			var simpleMessage = criteria.ToXML();
			AssertEquals("ToXML failed and the exception could not be serialised with: This isnt fair!", simpleMessage);
		}

		public void TestGetChargedPartiesForSpotQuote()
		{
			var criteria = new TestRatingCriteria();
			criteria.ConsumerType = JobInvoicingConsumerTypes.OneOffQuotation;
			criteria.LocalClient = Factory.New<OrgHeader>();
			criteria.Consignee = Factory.New<OrgHeader>();
			criteria.Consignor = Factory.New<OrgHeader>();

			using (RatingDataRegistry.Instance.EnableOverseasAgentInOneOffQuote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				criteria.JobDirection = Directions.Export;
				criteria.PaymentTerm = new PaymentTermInfos();
				criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
				AssertEquals(ChargedParty.Agent | ChargedParty.Consignee, criteria.ChargesPaidBy(criteria.Origin, criteria.Destination, "ORG", CostSell.Revenue));

				criteria.JobDirection = Directions.Import;
				AssertEquals(ChargedParty.Consignee | ChargedParty.LocalClient, criteria.ChargesPaidBy(criteria.Origin, criteria.Destination, "ORG", CostSell.Revenue));

				criteria.PaymentTerm = new PaymentTermInfos();
				criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "DDP"));
				AssertEquals(ChargedParty.Agent | ChargedParty.Consignor, criteria.ChargesPaidBy(criteria.Origin, criteria.Destination, "DST", CostSell.Revenue));

				criteria.JobDirection = Directions.Export;
				AssertEquals(ChargedParty.Consignor | ChargedParty.LocalClient, criteria.ChargesPaidBy(criteria.Origin, criteria.Destination, "DST", CostSell.Revenue));
			}
		}

		public void TestGetChargePartyForCrossTradeJob()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var configs = new List<ICrossTradeDebtorDefaultingConfigurationItem>()
			{
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, false, true, ChargedPartyForCrossTradeJob.LocalClient),
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, true, false, ChargedPartyForCrossTradeJob.Agent),
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.AirSea, true, false, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToAgent),
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.SeaAir, false, true, ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToLocalClient)
			};

			var configProvider = new Mock<ICrossTradeDebtorDefaultingConfigurationProvider>(MockBehavior.Strict);
			configProvider.Setup(p => p.GetConfiguration()).Returns(configs);

			var controllingCustomerOrg = objectCreator.CreateOrgHeader("ControlDR", isCreditor: false, isDebtor: true, isAPGSTApplicable: false, isAPWHTApplicable: false, isARGSTApplicable: true, isARWHTApplicable: false);

			using (ObjectFactory.Substitute(configProvider.Object))
			{
				AssertCrossTradeChargeParty("Charge paid by Local Client", objectCreator, "S0001", "AIR", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, null, ChargedParty.LocalClient);
				AssertCrossTradeChargeParty("Charge paid by Agent", objectCreator, "S0002", "SEA", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, null, ChargedParty.Agent);
				AssertCrossTradeChargeParty("Charge paid by Controlling Customer", objectCreator, "S0003", "FAS", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg, ChargedParty.ControllingCustomerFallingBackToAgent);
				AssertCrossTradeChargeParty("Charge paid by Controlling Customer", objectCreator, "S0004", "FSA", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: controllingCustomerOrg, ChargedParty.ControllingCustomerFallingBackToLocalClient);
				AssertCrossTradeChargeParty("Charge paid by Agent cause there is no Controlling Customer", objectCreator, "S0005", "FAS", paymentTerm: "FOB", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null, ChargedParty.Agent);
				AssertCrossTradeChargeParty("Charge paid by Local Client cause there is no Controlling Customer", objectCreator, "S0006", "FSA", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, controllingCustomer: null, ChargedParty.LocalClient);
				AssertCrossTradeChargeParty("No charge party is assign cause no registry is set", objectCreator, "S0007", "SEA", paymentTerm: "CFR", localClient: objectCreator.Debtor, agent: objectCreator.Debtor1, null, ChargedParty.None);
			}
		}

		void AssertCrossTradeChargeParty(string message, TestObjectCreator objectCreator, string shipmentNumber, string transportMode, string paymentTerm, OrgHeader localClient, OrgHeader agent, OrgHeader controllingCustomer, ChargedParty party)
		{
			var shipment = objectCreator.CreateShipment(shipmentNumber, origin: "USCHI", destination: "ITALL", transportMode: transportMode);
			shipment.JS_INCO = paymentTerm;
			var job = objectCreator.CreateJob(shipment, localClientOrg: localClient, 1.0M, agentOrg: agent, 1.0M);

			if (controllingCustomer != null)
			{
				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			}

			var charge = objectCreator.CreateCharge(job, objectCreator.CC1, 200M, 200M);
			charge.JR_AC = objectCreator.FRT.PK;

			var criteria = new TestRatingCriteria(shipment.RatingAdapter);
			criteria.JobDirection = Directions.CrossTrade;
			criteria.PaymentTerm = new PaymentTermInfos();
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, paymentTerm));

			Factory.Save();

			AssertEquals(message, party, criteria.ChargesPaidBy(criteria.Origin, criteria.Destination, "FRT", CostSell.Revenue));
		}

		ICrossTradeDebtorDefaultingConfigurationItem GetConfigurationItem(ZString jobType, ZString transportMode, bool isCollect, bool isPrepaid, ChargedPartyForCrossTradeJob chargedParty)
		{
			var mockConfigItem = new Mock<ICrossTradeDebtorDefaultingConfigurationItem>(MockBehavior.Strict);
			mockConfigItem.Setup(c => c.JobTypeCode).Returns(jobType);
			mockConfigItem.Setup(c => c.TransportModeCode).Returns(transportMode);
			mockConfigItem.Setup(c => c.IsCollect).Returns(isCollect);
			mockConfigItem.Setup(c => c.IsPrepaid).Returns(isPrepaid);
			mockConfigItem.Setup(c => c.BillToParty).Returns(chargedParty);
			return mockConfigItem.Object;
		}

		[ExpectNoExceptions()]
		public void TestGetDebtors()
		{
			var criteria = new TestRatingCriteria();
			var helper = new TestHelper(Factory);

			var consignee = helper.NewOrgHeader();
			var consignor = helper.NewOrgHeader();
			var agent = helper.NewOrgHeader();
			var controllingCustomer = helper.NewOrgHeader();
			var testRate = helper.NewClientRate(Helper.NewOrgHeader());

			criteria.OverseasAgent = agent;
			criteria.LocalClient = testRate.Header;
			criteria.Consignee = consignee;
			criteria.Consignor = consignor;
			criteria.DebtorOrgs[RatingDebtorOrgTypes.CCUS] = controllingCustomer;

			var debtors = criteria.GetDebtors();

			AssertEquals(RatingDebtorOrgTypes.AG, debtors.Where(x => x.OrgHeader.PK == agent.PK).Select(x => x.RatingDebtorOrgTypes).First());
			AssertEquals(RatingDebtorOrgTypes.LC, debtors.Where(x => x.OrgHeader.PK == criteria.LocalClient.PK).Select(x => x.RatingDebtorOrgTypes).First());
			AssertEquals(RatingDebtorOrgTypes.CNR, debtors.Where(x => x.OrgHeader.PK == consignor.PK).Select(x => x.RatingDebtorOrgTypes).First());
			AssertEquals(RatingDebtorOrgTypes.CNE, debtors.Where(x => x.OrgHeader.PK == consignee.PK).Select(x => x.RatingDebtorOrgTypes).First());
			AssertEquals(RatingDebtorOrgTypes.CCUS, debtors.Where(x => x.OrgHeader.PK == controllingCustomer.PK).Select(x => x.RatingDebtorOrgTypes).First());

			criteria.ImportBroker = criteria.LocalClient;
			debtors = criteria.GetDebtors();

			AssertEquals(RatingDebtorOrgTypes.LCBK, debtors.Where(x => x.OrgHeader.PK == criteria.LocalClient.PK).Select(x => x.RatingDebtorOrgTypes).First());
			AssertEquals(false, debtors.Any(x => x.RatingDebtorOrgTypes == RatingDebtorOrgTypes.LC));
		}

		public void TestDirection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var criteria = new TestRatingCriteria();
			AssertEquals(OrgRateTariffLevel.Directions.ALL, criteria.Direction);

			criteria.JobDirection = Directions.Import;
			AssertEquals(OrgRateTariffLevel.Directions.IMP, criteria.Direction);

			criteria.JobDirection = Directions.Domestic;
			AssertEquals(OrgRateTariffLevel.Directions.ALL, criteria.Direction);

			criteria.JobDirection = Directions.Export;
			AssertEquals(OrgRateTariffLevel.Directions.EXP, criteria.Direction);
		}

		public void TestConvertWithNullCurrencies()
		{
			var criteria = new TestRatingCriteria();
			var rateLine = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry("AIR")
				.RateLines[0];

			RefCurrency amountCurrency;
			ZDecimal amount = 500m;

			amountCurrency = null;
			rateLine.TL_RX_NKCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			var result = criteria.Convert(ref amount, amountCurrency, rateLine, ExchangeRateType.Sell);
			Assert(!result);

			amountCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			rateLine.TL_RX_NKCurrency = ZString.Empty;
			result = criteria.Convert(ref amount, amountCurrency, rateLine, ExchangeRateType.Buy);
			Assert(!result);

			amountCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			rateLine.TL_RX_NKCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			result = criteria.Convert(ref amount, amountCurrency, rateLine, ExchangeRateType.Sell);
			Assert(result);
		}

		public void TestConvertIsUsingExchangeRateType()
		{
			var criteria = new TestRatingCriteria();
			criteria.CurrencyConverter = new DummyCurrencyConverter(Factory);

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.UnitedStates);
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR").RateLines[0];
			rateLine.TL_RX_NKCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			Assert("Prerequisite", usdCurrency.RX_Code != rateLine.Currency.RX_Code);

			var originalCurrencyConverterRateType = criteria.CurrencyConverter.RateType;

			ZDecimal amount = 100m;
			criteria.Convert(ref amount, usdCurrency, rateLine, ExchangeRateType.Sell);
			AssertEquals("Sell rate was used", 50m, amount);
			AssertEquals("Original converter's rate type was restored", originalCurrencyConverterRateType, criteria.CurrencyConverter.RateType);

			amount = 100m;
			criteria.Convert(ref amount, usdCurrency, rateLine, ExchangeRateType.Buy);
			AssertEquals("Buy rate was used", 200m, amount);
			AssertEquals("Original converter's rate type was restored", originalCurrencyConverterRateType, criteria.CurrencyConverter.RateType);
		}

		public void TestConvertIsUsingClientSpecificExchangeRates()
		{
			var criteria = new TestRatingCriteria();
			criteria.CurrencyConverter = new DummyCurrencyConverter(Factory);
			criteria.SetDebtorPK(ZGuid.NewZGuid());

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.UnitedStates);
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR").RateLines[0];
			rateLine.TL_RX_NKCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			Assert("Prerequisite", usdCurrency.RX_Code != rateLine.Currency.RX_Code);

			var originalCurrencyConverterRateType = criteria.CurrencyConverter.RateType;

			ZDecimal amount = 100m;
			criteria.Convert(ref amount, usdCurrency, rateLine, ExchangeRateType.Sell);
			AssertEquals("Sell rate was used with an organization", 47.62m, amount);
			AssertEquals("Original converter's rate type was restored", originalCurrencyConverterRateType, criteria.CurrencyConverter.RateType);

			amount = 100m;
			criteria.Convert(ref amount, usdCurrency, rateLine, ExchangeRateType.Buy);
			AssertEquals("Buy rate was used with an organization", 250m, amount);
			AssertEquals("Original converter's rate type was restored", originalCurrencyConverterRateType, criteria.CurrencyConverter.RateType);
		}

		public void TestGetFreightLeg()
		{
			var rate = Factory.New<ClientRate>();
			var entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "", "20GP");
			var entry2 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "SGSIN", "", "20GP");
			var entry3 = rate.AddRateEntry("FCL", "SEA", "SGSIN", "GBLON", "", "20GP");
			var entry4 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			var entry5 = rate.AddRateEntry("ORG", "SEA", "AUSYD", "", "", "20GP");
			var entry6 = rate.AddRateEntry("DST", "SEA", "", "GBLON", "", "20GP");

			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP20, null);

			AssertEquals(1, criteria.GetFreightLeg(entry1));
			AssertEquals(1, criteria.GetFreightLeg(entry2));
			AssertEquals(1, criteria.GetFreightLeg(entry3));
			AssertEquals(1, criteria.GetFreightLeg(entry4));
			AssertEquals(0, criteria.GetFreightLeg(entry5));
			AssertEquals(0, criteria.GetFreightLeg(entry6));

			criteria.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));

			AssertEquals(1, criteria.GetFreightLeg(entry1));
			AssertEquals(1, criteria.GetFreightLeg(entry2));
			AssertEquals(2, criteria.GetFreightLeg(entry3));
			AssertEquals(1, criteria.GetFreightLeg(entry4));
			AssertEquals(0, criteria.GetFreightLeg(entry5));
			AssertEquals(0, criteria.GetFreightLeg(entry6));

			var entry7 = rate.AddRateEntry("FCL", "SEA", "AUEC", "SG", "", "20GP");
			var entry8 = rate.AddRateEntry("FCL", "SEA", "SG", "GB", "", "20GP");

			AssertEquals(1, criteria.GetFreightLeg(entry7));
			AssertEquals(2, criteria.GetFreightLeg(entry8));

			var entry9 = rate.AddRateEntry("DST", "SEA", "AUSYD", "GBLON", "", "20GP");
			AssertEquals(0, criteria.GetFreightLeg(entry9));
		}

		public void TestDefaultCartageEquipment()
		{
			var criteria = new TestRatingCriteria();

			AssertEquals(Constants.FCLEquipmentNeeded.WaitForUnpack, criteria.DeliveryCartageEquipment);
			AssertEquals(Constants.FCLEquipmentNeeded.WaitForUnpack, criteria.PickupCartageEquipment);

			criteria.FreightMode = FreightMode.AIR;
			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, criteria.DeliveryCartageEquipment);
			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, criteria.PickupCartageEquipment);

			criteria.RateTypeToUse = RateType.Warehouse;
			AssertEquals("", criteria.DeliveryCartageEquipment);
			AssertEquals("", criteria.PickupCartageEquipment);
		}

		public void TestChargesPaidAlways()
		{
			var criteria = new TestRatingCriteria();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("AIR", "LSE", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");
			var testLine = testEntry.RateLines[0];
			AssertEquals(ChargedParty.None, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(testLine.ChargeCode, criteria.ConsumerType == null || criteria.ConsumerType.OverseasAgentApplicable));

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.ChargeCodes["FRT"].PK.ToString());
			AssertEquals(ChargedParty.Agent, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(testLine.ChargeCode, criteria.ConsumerType == null || criteria.ConsumerType.OverseasAgentApplicable));

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.ChargeCodes["FRT"].PK.ToString());
			AssertEquals(ChargedParty.LocalClient, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(testLine.ChargeCode, criteria.ConsumerType == null || criteria.ConsumerType.OverseasAgentApplicable));
		}

		public void TestTimeIsNotNull()
		{
			var autoRatingMock = new Mock<IAutoRating>();
			var jobServices = new JobServicesCollection();
			IRateableMeasureSet measures = new RateableMeasureSet();
			autoRatingMock.Setup(x => x.RateableMeasures).Returns(measures);
			autoRatingMock.Setup(x => x.JobServices).Returns(jobServices);

			var criteria = new RatingCriteria(autoRatingMock.Object, Factory, false);

			var emptyChargeCode = Factory.New<AccChargeCode>();

			AssertNotNull(criteria.Time(emptyChargeCode));
		}

		public void TestGetApplicableValues()
		{
			var criteria = new TestRatingCriteria();
			var client = Helper.NewOrgHeader();
			client.OH_Code = "DUMMY1";
			var testRate = Helper.NewClientRate(client);
			var testEntry = testRate.AddRateEntry("AIR", "LSE", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");
			var testLine = testEntry.RateLines[0];
			var testLineItem = testLine.RateLineItems.AddNew();
			testLineItem.TM_Text = "bnd";

			Money money = null;
			var logger = new Mock<ILogger>();
			AssertNoExceptionThrown("", () => money = criteria.GetApplicableValue(logger.Object, testLineItem));
			AssertEquals(true, money.IsValid);

			testLineItem.TM_Text = ZString.Empty;
			var message = "Cannot complete Auto-Rating as this Job has been matched to an invalid Rate Line. Please either correct the 'Apply to' field or delete the 'FRT' Rate Line that uses a 'CMB' Calculator with currently an Apply To of '' on Client Rate DUMMY1.";

			AssertExceptionThrown(typeof(AutoRaterException), message, () => money = criteria.GetApplicableValue(logger.Object, testLineItem));

			testLineItem.TM_Text = CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods;

			AssertNoExceptionThrown("Has valid ApplyToValue so no exception expected", () => money = criteria.GetApplicableValue(logger.Object, testLineItem));
			AssertEquals(true, money.IsValid);
		}

		public void TestCustomsValue_WhenNoEntries_ShowAutoratingLogWarning()
		{
			var currency = GlbCompany.CurrentCompany.LocalCurrency;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var logger = new Mock<ILogger>();
			var criteria = new TestRatingCriteria();
			var rateLine = new Mock<IRateLine>();
			rateLine.Setup(l => l.Currency).Returns(currency);
			rateLine.Setup(l => l.ChargeCode).Returns(chargeCode);

			var customsValue = criteria.CustomsValue(logger.Object, rateLine.Object);

			logger.Verify(x => x.Log(LogType.Warning, "Customs Value is only available upon action of MERGE (Generate Entries) is performed. Please perform the merging and try again."), Moq.Times.Once());
			AssertEquals(new Money(0, currency), customsValue);
		}

		public void TestGetLeg()
		{
			var rate = Factory.New<ClientRate>();
			var entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "", "20GP");
			var entry2 = rate.AddRateEntry("ORG", "SEA", "AUSYD", "", "", "20GP");
			var entry3 = rate.AddRateEntry("DST", "SEA", "", "GBLON", "", "20GP");

			var criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP20, null);

			AssertEquals("AUSYD-GBLON", criteria.GetLeg(entry1).ToString());
			AssertEquals("AUSYD-GBLON", criteria.GetLeg(entry2).ToString());
			AssertEquals("AUSYD-GBLON", criteria.GetLeg(entry2).ToString());

			criteria.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));

			var entry4 = rate.AddRateEntry("FCL", "SEA", "SGSIN", "GBLON", "", "20GP");
			var entry5 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "SGSIN", "", "20GP");
			var entry6 = rate.AddRateEntry("FCL", "SEA", "SG", "", "", "20GP");

			AssertEquals("SGSIN-GBLON", criteria.GetLeg(entry4).ToString());
			AssertEquals("AUSYD-SGSIN", criteria.GetLeg(entry5).ToString());
			AssertEquals("SGSIN-GBLON", criteria.GetLeg(entry6).ToString());
		}

		public void TestGetContainer()
		{
			var criteria = new TestRatingCriteria();
			criteria.FreightMode = FreightMode.FCL;

			AssertEquals(0, criteria.GetContainers().Length);

			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var containers = new TestContainers(Factory, container1.PK, 3, container2.PK, 2);
			containers.PopulateContainerList(criteria.RateableMeasures);

			var actualContainers = criteria.GetContainers();
			AssertContainsExactElementsInAnyOrder(new[] { container1.PK, container2.PK }, actualContainers.Select(c => c.PK));
		}

		#region TestIsWarehouseStorage

		public void TestIsWarehouseStorage()
		{
			var autoRatingMock = new Mock<IAutoRating>().Object;
			var criteria = new RatingCriteria(autoRatingMock, Factory, false);
			AssertEquals(false, criteria.IsWarehouseStorage);

			criteria.ValuesCanBeSet = true;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			AssertEquals(true, criteria.IsWarehouseStorage);

			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseOutwards;
			AssertEquals(false, criteria.IsWarehouseStorage);
		}

		#endregion

		#region TestIsWarehouseHandling

		public void TestIsWarehouseHandling()
		{
			var autoRatingMock = new Mock<IAutoRating>().Object;
			var criteria = new RatingCriteria(autoRatingMock, Factory, false);
			AssertEquals(false, criteria.IsWarehouseHandling);

			criteria.ValuesCanBeSet = true;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			AssertEquals(true, criteria.IsWarehouseHandling);

			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseOutwards;
			AssertEquals(true, criteria.IsWarehouseHandling);

			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			AssertEquals(false, criteria.IsWarehouseHandling);
		}

		#endregion

		#region TestIsTransitTransportationUnit

		public void TestIsTransitTransportationUnit()
		{
			var autoRatingMock = new Mock<IAutoRating>().Object;
			var criteria = new RatingCriteria(autoRatingMock, Factory, false);
			AssertEquals(false, criteria.IsTransitTransportationUnit);

			criteria.ValuesCanBeSet = true;
			criteria.ConsumerType = JobInvoicingConsumerTypes.TransitReceiveTransportationUnit;
			AssertEquals(true, criteria.IsTransitTransportationUnit);

			criteria.ConsumerType = JobInvoicingConsumerTypes.TransitDispatchTransportationUnit;
			AssertEquals(true, criteria.IsTransitTransportationUnit);
		}

		#endregion

		public void TestAllDistinctCarriers()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();
			var carrier3 = Factory.New<OrgHeader>();

			var criteria = new TestRatingCriteria();
			AssertEquals(0, criteria.AllDistinctCarriers.Count());

			criteria.Carrier = carrier1;
			AssertContainsExactElementsInAnyOrder(new[] { carrier1 }, criteria.AllDistinctCarriers);

			criteria.Carrier = null;
			criteria.PossibleCarriers = new[] { carrier2, carrier3 };
			AssertContainsExactElementsInAnyOrder(new[] { carrier2, carrier3 }, criteria.AllDistinctCarriers);

			criteria.Carrier = carrier1;
			criteria.PossibleCarriers = new[] { carrier2, carrier3 };
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2, carrier3 }, criteria.AllDistinctCarriers);

			// no duplicates
			criteria.Carrier = carrier1;
			criteria.PossibleCarriers = new[] { carrier1, carrier1, carrier2 };
			AssertContainsExactElementsInAnyOrder(new[] { carrier1, carrier2 }, criteria.AllDistinctCarriers);
		}

		public void TestZoneOwnerOrganizations_ContainsPossibleCarriers()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();
			var carrier3 = Factory.New<OrgHeader>();

			var criteria = new TestRatingCriteria();
			AssertEquals("carrier1", 0, criteria.ZoneOwnerOrganizations().Count(x => x == carrier1));

			criteria.PossibleCarriers = new[] { carrier1, carrier2, carrier3 };
			var actual = criteria.ZoneOwnerOrganizations();
			AssertEquals("carrier1", 1, actual.Count(x => x == carrier1));
			AssertEquals("carrier2", 1, actual.Count(x => x == carrier2));
			AssertEquals("carrier3", 1, actual.Count(x => x == carrier3));
		}

		public void TestGetClientContractNumbersForFiltering_HasDialogService()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: Array.Empty<string>(),
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: null);

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: new[] { "" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: new[] { "AAA" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "AAA");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: new[] { "", "AAA" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "AAA");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: new[] { "AAA", "AAA" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "AAA");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: new[] { "", "BBB", "CCC" },
				selectedSingleClientContractNumber: "",
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: true,
				expectedSingleClientContractNumber: "");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: new[] { "", "BBB", "CCC" },
				selectedSingleClientContractNumber: "BBB",
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: true,
				expectedSingleClientContractNumber: "BBB");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: true,
				clientContractNumbers: new[] { "", "BBB", "CCC" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: true,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "???"); // expectedSingleClientContractNumber won't be checked as we expected cancelled exception
		}

		public void TestGetClientContractNumbersForFiltering_HasNoDialogService()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: false,
				clientContractNumbers: Array.Empty<string>(),
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: null);

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: false,
				clientContractNumbers: new[] { "" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: false,
				clientContractNumbers: new[] { "AAA" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "AAA");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: false,
				clientContractNumbers: new[] { "", "AAA" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "AAA");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: false,
				clientContractNumbers: new[] { "AAA", "AAA" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: "AAA");

			AssertGetClientContractNumbersForFiltering(
				shipment,
				hasDialogService: false,
				clientContractNumbers: new[] { "", "BBB", "CCC" },
				selectedSingleClientContractNumber: null,
				expectedRatingCancelledException: false,
				expectedSelectSingleClientContractNumberIsCalled: false,
				expectedSingleClientContractNumber: null);
		}

		static void AssertGetClientContractNumbersForFiltering(
			ForwardingShipment shipment,
			bool hasDialogService,
			IEnumerable<string> clientContractNumbers,
			string selectedSingleClientContractNumber,
			bool expectedRatingCancelledException,
			bool expectedSelectSingleClientContractNumberIsCalled,
			string expectedSingleClientContractNumber)
		{
			Mock<IDialogService> mockDialogService = null;
			if (hasDialogService)
			{
				mockDialogService = new Mock<IDialogService>();
				mockDialogService
					.Setup(x => x.SelectSingleClientContractNumber(clientContractNumbers))
					.Returns(selectedSingleClientContractNumber);
			}

			var criteria = new TestRatingCriteria(shipment.RatingAdapter);

			if (expectedRatingCancelledException)
			{
				AssertExceptionThrown<AutoRater.RatingCancelledException>(() => criteria.GetSingleClientContractNumber(clientContractNumbers, mockDialogService?.Object));
			}
			else
			{
				var resultNumber = criteria.GetSingleClientContractNumber(clientContractNumbers, mockDialogService?.Object);
				if (expectedSelectSingleClientContractNumberIsCalled)
				{
					mockDialogService.Verify(x => x.SelectSingleClientContractNumber(clientContractNumbers), Times.Once, "DialogService should ask for a number.");
				}
				AssertEquals("Result did not match with the expected contract number", expectedSingleClientContractNumber, resultNumber);
			}
		}

		#region GetEffectiveDate

		public void TestGetJobDateTypeFromRegistrySTD()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Standard
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var expectedJobDateType = new AutoRateDate { DateType = JobDateTypes.Codes.DepartureDate, IsFallbackDisabled = false };
			var expectedJobDateTypes = new[] { expectedJobDateType };
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Origin, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.OriginBrokerage, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.OriginBrokerageOnly, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Loading, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Insurance, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.ShippingDisbursements, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Transport, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.TransportBooking, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.WHSOutwards, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.CFSLoadList, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.YardGateOut, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.TRWDispatch, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit, expectedJobDateTypes);

			expectedJobDateType.DateType = JobDateTypes.Codes.ArrivalDate;
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Unloading, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Destination, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.WHSInwards, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.WHSStorage, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Brokerage, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.BrokerageOnly, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.ContainerStorage, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.CustomsDuty, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.CFSShipment, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.YardGateIn, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.YardStorage, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.TRWReceive, expectedJobDateTypes);
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit, expectedJobDateTypes);
		}

		public void TestGetJobDateTypeFromRegistryARV()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Arrival
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var expectedJobDateType = new AutoRateDate { DateType = JobDateTypes.Codes.ArrivalDate, IsFallbackDisabled = false };
			var expectedJobDateTypes = new[] { expectedJobDateType };
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);

			Assert("This test uses FluentAssertions", condition: true);
		}

		public void TestGetJobDateTypeFromRegistryDEP()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Departure
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var expectedJobDateType = new AutoRateDate { DateType = JobDateTypes.Codes.DepartureDate, IsFallbackDisabled = false };
			var expectedJobDateTypes = new[] { expectedJobDateType };
			TestGetRankedJobDateTypes(null, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);

			Assert("This test uses FluentAssertions", condition: true);
		}

		public void TestGetJobDateTypeFromRegistryCUS()
		{
			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ShipmentCode,
				Mode = Core.Constants.TransportModes.Air,
				DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.AWBIssueDate,
			};

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Custom
			};
			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var criteria = new TestRatingCriteria();
			criteria.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			criteria.FreightMode = FreightMode.AIR;
			criteria.JobDirection = Directions.Export;

			var expectedJobDateType = new AutoRateDate { DateType = JobDateTypes.Codes.AWBIssueDate, IsFallbackDisabled = false };
			var expectedJobDateTypes = new[] { expectedJobDateType };
			TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes, message: "Criteria matches AutoRateDate");

			criteria.FreightMode = FreightMode.SEA;
			expectedJobDateType.DateType = JobDateTypes.Codes.DepartureDate;
			TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes,
				message: "Criteria does not match AutoRateDate THEN it should fall back to STD (Use Departure Date for Loading, Origin, Freight and Insurance charges, and Arrival Date for Unloading and Destination charges)");

			Assert("This test uses FluentAssertions", condition: true);
		}

		public void TestRegistryCustom_OrRegistryStd_AccordingToRateType()
		{
			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Mode = "AIR",
				DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.AWBIssueDate,
				RateType = JobRateTypes.Codes.All,
			};

			var config = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Custom,
			};

			var freightChargeGroup = config.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>()
				.First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);

			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var criteria = new TestRatingCriteria();
			criteria.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			criteria.FreightMode = FreightMode.AIR;
			criteria.JobDirection = Directions.Export;

			var expectedJobDateType = new AutoRateDate { DateType = JobDateTypes.Codes.AWBIssueDate, IsFallbackDisabled = false };
			var expectedJobDateTypes = new[] { expectedJobDateType };
			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();
				TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);
			}

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();
				TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);
			}

			autoRateDate.RateType = JobRateTypes.Codes.Revenue;
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();
				TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);
			}

			expectedJobDateType.DateType = JobDateTypes.Codes.DepartureDate;
			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();
				TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);
			}

			autoRateDate.RateType = JobRateTypes.Codes.Cost;
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();
				TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);
			}

			expectedJobDateType.DateType = JobDateTypes.Codes.AWBIssueDate;
			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();
				TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);
			}

			Assert("This test uses FluentAssertions", condition: true);
		}

		public void TestGetJobDateTypeFromRegistryCUS_MatchingContainerMode()
		{
			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ForwardingConsolCode,
				Mode = Core.Constants.TransportModes.Air,
				DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.AWBIssueDate,
				ContainerMode = Core.Constants.ContainerModes.Loose
			};

			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Custom,
			};

			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>()
				.First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);

			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var criteria = new TestRatingCriteria();
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.FreightMode = FreightMode.AIR;
			criteria.JobDirection = Directions.Export;
			criteria.ContainerMode = Core.Constants.ContainerModes.Loose;

			var expectedJobDateType = new AutoRateDate { DateType = JobDateTypes.Codes.AWBIssueDate, IsFallbackDisabled = false };
			var expectedJobDateTypes = new[] { expectedJobDateType };
			TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);

			criteria.ContainerMode = Core.Constants.ContainerModes.ULD;
			expectedJobDateType.DateType = JobDateTypes.Codes.DepartureDate;
			TestGetRankedJobDateTypes(criteria, ChargeCodeGroupList.Codes.Freight, expectedJobDateTypes);

			Assert("This test uses FluentAssertions", condition: true);
		}

		public void TestConsumerTypeNullReferenceOnJobDateTypeFromCustomCode()
		{
			var autoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Mode = "AIR",
				DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export,
				DateType = JobDateTypes.Codes.AWBIssueDate
			};

			var config = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Custom,
			};

			var freightChargeGroup = config.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>()
				.First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);

			freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var criteria = new TestRatingCriteria();
			criteria.FreightMode = FreightMode.AIR;
			criteria.JobDirection = Directions.Export;
			criteria.ConsumerType = null;

			AssertNoExceptionThrown(
				"There should not be exception when criteria.ConsumerType is null",
				() => criteria.GetRankedDateTypes_ExposedForTest(null, ChargeCodeGroupList.Codes.Freight, isCosting: true));
		}

		static void TestGetRankedJobDateTypes(
			RatingCriteria testCriteria,
			string chargeCodeGroup,
			IEnumerable<IAutoRateDate> expectedJobDateTypes,
			string message = default,
			bool isCosting = true)
		{
			if (testCriteria == null)
			{
				testCriteria = new TestRatingCriteria();
			}
			var rankedJobDateTypes = testCriteria.GetRankedDateTypes_ExposedForTest(null, chargeCodeGroup, isCosting);

			var actual = rankedJobDateTypes
				.Select(x => $"{x.DateType}|{x.IsFallbackDisabled}")
				.ToArray();

			var expected = expectedJobDateTypes
				.Select(x => $"{x.DateType}|{x.IsFallbackDisabled}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				message ?? "The ranked job date types should match the expected collection.",
				expected,
				actual
			);
		}

		public void TestGetEffectiveDateWithFallback_NonStandardRegistry_EmptyLine()
		{
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Core.Constants.RatingDateFilterTypes.Codes.Arrival
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			var testCriteria = new TestRatingCriteria();

			AssertNoExceptionThrown(() => testCriteria.GetEffectiveDateWithFallback(null, ChargeCodeGroupList.Codes.Freight));
			var result = testCriteria.GetEffectiveDateWithFallback(null, ChargeCodeGroupList.Codes.Freight);
			AssertEquals(ZDate.Empty, result.date);
		}

		[TestDate(2025, 4, 4)]
		public void TestGetEffectiveDate_AutoratingDateOverride()
		{
			var costingDate = ZDate.Today.AddDays(1);
			var revenueDate = ZDate.Today.AddDays(2);

			var mockJobDatesProvider = new Mock<IJobDatesProvider>();
			mockJobDatesProvider
				.Setup(x => x.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride, It.IsAny<string>()))
				.Returns(costingDate);
			mockJobDatesProvider
				.Setup(x => x.GetJobDateByType(JobDateTypes.Codes.RevenueAutoratingDateOverride, It.IsAny<string>()))
				.Returns(revenueDate);

			var testCriteria = new TestRatingCriteria();
			testCriteria.JobDatesProvider = mockJobDatesProvider.Object;
			AssertEquals(costingDate, testCriteria.GetEffectiveDate(isCosting: true).date);
			AssertEquals(revenueDate, testCriteria.GetEffectiveDate(isCosting: false).date);
		}

		#endregion

		#region Implementation

		public class DummyCurrencyConverter : CurrencyConverter, IJobExRateCurrencyConverter
		{
			public DummyCurrencyConverter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZDecimal GetExchangeRate(ICurrency currency)
			{
				if (currency.Code == "USD")
				{
					return RateType == ExchangeRateType.Buy
						? 0.5m
						: 2m;
				}

				return 0m;
			}

			public override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate)
			{
				foundRateDate = ZDateTime.Today;
				return GetExchangeRate(currency);
			}

			public ZDecimal GetExchangeRate(ICurrency currency, ZGuid orgPK, CostSell costOrSell)
			{
				var rate = GetExchangeRate(currency);

				if (!orgPK.IsEmpty)
				{
					rate += costOrSell == CostSell.Cost ? (-0.1m) : (+0.1m);
				}

				return rate;
			}

			public Money ConvertExact(Money monetaryAmount, ICurrency destinationCurrency, ZGuid orgPK, CostSell costOrSell, bool roundToDestinationCurrencyDecimals = true)
				=> ConvertExact(monetaryAmount, destinationCurrency, currency => GetExchangeRate(currency, orgPK, costOrSell), roundToDestinationCurrencyDecimals);
		}

		#endregion
	}

	public class TestRatingCriteria : RatingCriteria
	{
		public void ClearCache()
		{
			cache = null;
		}

		public override List<ILocation> SortedOverridenPlannedDischarge => SortedOverridenPlannedDischargeForTest;
		public List<ILocation> SortedOverridenPlannedDischargeForTest { get; set; } = new List<ILocation>();

		public override List<ILocation> SortedOverridenPlannedLoad => SortedOverridenPlannedLoadForTest;
		public List<ILocation> SortedOverridenPlannedLoadForTest { get; set; } = new List<ILocation>();

		public override ILocation PlannedLoad(CostSell costSell) => PlannedLoadForTest;
		public ILocation PlannedLoadForTest { get; set; }

		public override ILocation PlannedDischarge(CostSell costSell) => PlannedDischargeForTest;
		public ILocation PlannedDischargeForTest { get; set; }

		public override bool SkipFreightCharge { get; set; }

		public TestRatingCriteria(IAutoRating consumer)
			: base(consumer, new BusinessObjectFactory(), false)
		{
			ValuesCanBeSet = true;

			ConsumerType = JobInvoicingConsumerTypes.Shipment;
			RateTypeToUse = RateType.Forwarding;

			StatusInformation = new AutoRatingStatusInfo(ZBool.True, ZString.Empty);

			ChargeCodeGroups = new ChargeCodeGroupCollection();
			ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Freight);
			ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Origin);
			ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Destination);

			ServiceLevel = new ServiceLevelRatingInformation(new ServiceLevelInfo("STD", ServiceLevelType.Client));
			PaymentTerm = new PaymentTermInfos();
			PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "DDP"));
			RateableMeasures = new RateableMeasureSet(AdapterType.Shipment);
			JobDatesProvider = new JobDatesProvider<DummyBusinessObject>(Factory.NewWithValidTestData<DummyBusinessObject>());
			CurrencyConverter = new RatingCriteriaTest.DummyCurrencyConverter(Factory);
			MonetaryValues = new MoneyType();
			AdapterType = AdapterType.Shipment;
		}

		public TestRatingCriteria()
			: this(null)
		{
		}

		public TestRatingCriteria
			(
				ZString origin,
				ZString destination,
				FreightMode freightMode,
				ZInt containerCount,
				RefContainer containerType,
				ZDecimal weight,
				ZString weightUnit,
				ZDecimal volume,
				ZString volumeUnit,
				OrgHeader billTo,
				ZString paymentTermOverride,
				bool? applicableToPaymentTermFiltering = null,
				IAutoRating consumer = null
			)
			: this(consumer)
		{
			Origin = LocationHelper.GetLocationFromString(origin, Factory);
			Destination = LocationHelper.GetLocationFromString(destination, Factory);
			JobDirection = ImportExportHelper.GetJobDirection(origin, destination);
			FreightMode = freightMode;
			ContainerMode = freightMode.ToString();
			var measures = RateableMeasures;
			if (containerCount > 0)
			{
				new TestContainers(Factory, containerType != null ? containerType.PK : ZGuid.Empty, (int)containerCount)
					.PopulateContainerList(measures);
			}
			measures.SetQuantity(MeasureType.Weight, weight, weightUnit);
			measures.SetQuantity(MeasureType.Volume, volume, volumeUnit);
			this.LocalClient = billTo;
			this.paymentTermOverride = paymentTermOverride;
			this.overriddenPaymentTermFilteringApplicability = applicableToPaymentTermFiltering;
		}

		public TestRatingCriteria(ZString origin, ZString destination, ZInt containerCount, RefContainer containerType, OrgHeader billTo, IAutoRating consumer = null)
			: this(origin, destination, FreightMode.FCL, containerCount, containerType, 0m, "KG", 0m, "M3", billTo, "", null, consumer)
		{ }

		public TestRatingCriteria(ZString origin, ZString destination, FreightMode freightMode, ZDecimal weightInKG, ZDecimal volumeInM3, OrgHeader billTo, bool? applicableToPaymentTermFiltering = null, IAutoRating consumer = null)
			: this(origin, destination, freightMode, 0, null, weightInKG, "KG", volumeInM3, "M3", billTo, "", applicableToPaymentTermFiltering, consumer)
		{ }

		public void SetSpotRateInfo(ZDecimal amount, ZString currency, ZString autoratingMode)
		{
			var money = new Money(amount, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency));
			SellSpotRateInfo = new SpotRateInfo(money, autoratingMode, AutoratedValueType.ClientRate);
			cache = null;
		}

		public void SetMeasure(ZDecimal value, MeasureType type)
		{
			RateableMeasures.SetQuantity(type, value, string.Empty);
		}

		public void SetTime(TimeInfo value)
		{
			RateableMeasures.Time = value;
		}

		public override PaymentTermInfos PaymentTerm { get; set; }

		public void SetSupplier(OrgHeader supplier)
		{
			transportProviders = Creditors.New(OrgWithSource.New(supplier, new List<string>() { "Supplier" }), OrgWithSource.New(Carrier, new List<string>() { "Carrier" }));
			cache = null;
		}

		public override GlbCompany Company
		{
			get { return company ?? base.Company; }
			set { company = value; }
		}

		GlbCompany company;

		public void SetCommodityCode(ZString value)
		{
			if ((FreightMode & FreightMode.NonContainerised) != 0)
			{
				var measures = RateableMeasures;
				measures.SetWeightWithCommodity(measures.GetActual(MeasureType.Weight), measures.GetUnit(MeasureType.Weight), value);
				measures.SetVolumeWithCommodity(measures.GetActual(MeasureType.Volume), measures.GetUnit(MeasureType.Volume), value);
			}

			cache = null;
		}

		public override bool CanExpandMacros
		{
			get { return HandleExpandMacro != null; }
		}

		public override string ExpandMacro(string macro)
		{
			return HandleExpandMacro == null ? null : HandleExpandMacro(macro);
		}

		public Converter<string, string> HandleExpandMacro { get; set; }

		public override Collection<IBusiness> AutoRatedFor
		{
			get { return autoRatedFor ?? (autoRatedFor = new Collection<IBusiness> { new TestAutoRatedForObject() }); }
		}

		public void SetAutoRatedFor(Collection<IBusiness> mockAutoRatedFor)
		{
			autoRatedFor = mockAutoRatedFor;
		}

		Collection<IBusiness> autoRatedFor;

		public void SetCarrierContractNumber(params ZString[] contractNumbers)
		{
			CarrierContractNumbers = CarrierContractNumbers.Concat(contractNumbers);
		}

		public void SetClientContractNumbers(params ZString[] contractNumbers)
		{
			ClientContractNumbers = ClientContractNumbers.Concat(contractNumbers);
		}

		public void SetShouldUseCarrierContractDateFilter(bool shouldUseCarrierContractDateFilter)
		{
			ShouldUseCarrierContractDateFilter = shouldUseCarrierContractDateFilter;
		}

		public void SetNamedAccount(ZString namedAccount)
		{
			this.namedAccount = namedAccount;
		}
		ZString namedAccount;

		public override ZString NamedAccount => namedAccount;

		public override ZGuid GetDebtorPK(AccChargeCode chargeCode) => debtorPK ?? base.GetDebtorPK(chargeCode);

		public void SetDebtorPK(ZGuid? orgPK) => debtorPK = orgPK;

		ZGuid? debtorPK;

		public override IContractNumberConfiguration GetContractNumberConfiguration(CostSell costOrSell)
		{
			if (ShouldReturnContractNumberConfiguration)
			{
				return new ContractNumberConfiguration(this);
			}
			return null;
		}

		class ContractNumberConfiguration : IContractNumberConfiguration
		{
			public ContractNumberConfiguration(TestRatingCriteria criteria)
			{
				this.criteria = criteria;
			}

			readonly TestRatingCriteria criteria;

			public bool ShouldAddContractNumberQueryFilter => criteria.ShouldAddContractNumberQueryFilter;
			public bool ShouldApplySpecificAdapterContractNumberFilter => criteria.ShouldApplySpecificAdapterContractNumberFilter;
			public bool ShouldIgnoreJobClientContractNumbers => criteria.ShouldIgnoreJobClientContractNumbers;
			public bool ShouldIgnoreJobCarrierContractNumbers => criteria.ShouldIgnoreJobCarrierContractNumbers;
			public bool ShouldMatchJobBlankContractNumber => criteria.ShouldMatchJobBlankContractNumber;
			public bool ShouldUseCarrierContractDateFilter => criteria.ShouldUseCarrierContractDateFilter;
		}

		public bool ShouldAddContractNumberQueryFilter
		{
			get
			{
				return shouldAddContractNumberQueryFilter;
			}
			set => shouldAddContractNumberQueryFilter = value;
		}
		bool shouldAddContractNumberQueryFilter;

		public bool ShouldApplySpecificAdapterContractNumberFilter;
		public bool ShouldIgnoreJobClientContractNumbers;
		public bool ShouldIgnoreJobCarrierContractNumbers;
		public bool ShouldMatchJobBlankContractNumber;
		public bool ShouldUseCarrierContractDateFilter;
		public bool ShouldReturnContractNumberConfiguration = true;

		public override ZString ShipmentConsolidationStatus =>
			overriddenShipmentConsolidationStatus ?? base.ShipmentConsolidationStatus;

		public void SetShipmentConsolidationStatus(ZString status) =>
			overriddenShipmentConsolidationStatus = status;

		string overriddenShipmentConsolidationStatus;

		readonly ZString paymentTermOverride;
		public override ZString PaymentTermOverride
		{
			get
			{
				return paymentTermOverride;
			}
		}

		readonly bool? overriddenPaymentTermFilteringApplicability;

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			if (overriddenPaymentTermFilteringApplicability.HasValue)
			{
				return overriddenPaymentTermFilteringApplicability.Value;
			}

			return base.IsApplicableToPaymentTermFiltering(chargeCodeGroup, costOrSell);
		}

		public override IAutoRatingChargeInfo[] GetExistingCharges(bool fromAllCompanies = false)
		{
			return overridenCharges?.ToArray() ?? base.GetExistingCharges(fromAllCompanies);
		}

		public void SetExistingCharges(IEnumerable<IAutoRatingChargeInfo> charges)
		{
			overridenCharges = charges;
		}

		IEnumerable<IAutoRatingChargeInfo> overridenCharges;

		public override ZString GatewayServiceLevelFilteredReason(ZString gatewayServiceLevel, ZGuid gatewayAgentPk)
			=> (overridenGatewayServiceLevelFilteredReason != null)
				? overridenGatewayServiceLevelFilteredReason.Invoke(gatewayServiceLevel, gatewayAgentPk)
				: base.GatewayServiceLevelFilteredReason(gatewayServiceLevel, gatewayAgentPk);

		public Func<ZString /* gatewayServiceLevel */, ZGuid /* gatewayAgentPk */, ZString /* errorMessage */> overridenGatewayServiceLevelFilteredReason { get; set; }

		public override ILocation GetVia(CostSell costOrSell) => viaForTest ?? base.GetVia(costOrSell);
		public void SetVia(ILocation via) => viaForTest = via;
		ILocation viaForTest;

		public override ILocation GetFirstLoad(CostSell costOrSell) => firstLoadForTest ?? base.GetFirstLoad(costOrSell);
		public void SetFirstLoad(ILocation firstLoad) => firstLoadForTest = firstLoad;
		ILocation firstLoadForTest;

		public override ILocation GetLastDischarge(CostSell costOrSell) => lastDischargeForTest ?? base.GetLastDischarge(costOrSell);
		public void SetLastDischarge(ILocation lastDischarge) => lastDischargeForTest = lastDischarge;
		ILocation lastDischargeForTest;

		public override ILocation GetFirstRouteSetLoad(CostSell costOrSell) => firstRouteSetLoadForTest ?? base.GetFirstRouteSetLoad(costOrSell);
		public void SetFirstRouteSetLoad(ILocation firstRouteSetLoad) => firstRouteSetLoadForTest = firstRouteSetLoad;
		ILocation firstRouteSetLoadForTest;

		public override ILocation GetLastRouteSetDischarge(CostSell costOrSell) => lastRouteSetDischargeForTest ?? base.GetLastRouteSetDischarge(costOrSell);
		public void SetLastRouteSetDischarge(ILocation lastRouteSetDischarge) => lastRouteSetDischargeForTest = lastRouteSetDischarge;
		ILocation lastRouteSetDischargeForTest;
	}

	public class GatewayBillingSupporterForTest : IGatewayBillingSupporter
	{
		public GatewayBillingSupporterForTest()
		{ }

		public GatewayBillingSupporterForTest(bool enabled, ICompany company = null) =>
			SetIsGatewayBillingEnabled(enabled, company);

		public GatewayBillingSupporterForTest(IOrgHeader sendingAgent, IOrgHeader receivingAgent) =>
			SetSendingReceivingAgents(sendingAgent, receivingAgent);

		public GatewayBillingSupporterForTest(IOrgHeader sendingAgent, IOrgHeader receivingAgent, bool enabled, ICompany company = null)
		{
			SetSendingReceivingAgents(sendingAgent, receivingAgent);
			SetIsGatewayBillingEnabled(enabled, company);
		}

		public bool IsGatewayBillingEnabled(ICompany company = null)
		{
			if (gatewayBillingEnableds.TryGetValue((company ?? GlbCompany.CurrentCompany).PK, out var enabled))
			{
				return enabled;
			}

			return false;
		}

		public (IOrgHeader sendingAgent, IOrgHeader receivingAgent) GatewayAgent(ICompany company = null) =>
			(sendingAgent, receivingAgent);

		public void SetIsGatewayBillingEnabled(bool enabled, ICompany company = null)
		{
			company = company ?? GlbCompany.CurrentCompany;
			gatewayBillingEnableds[company.PK] = enabled;
		}

		public void SetSendingReceivingAgents(IOrgHeader sendingAgent, IOrgHeader receivingAgent)
		{
			this.sendingAgent = sendingAgent;
			this.receivingAgent = receivingAgent;
		}

		readonly Dictionary<Guid, bool> gatewayBillingEnableds = new Dictionary<Guid, bool>();
		IOrgHeader sendingAgent;
		IOrgHeader receivingAgent;
	}

	public class TestAutoRatedForObject : NonPersistentBusinessObject
	{
		public ZString Name
		{
			get;
			set;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Name.IsEmpty ? base.HumanReadableNameCore : Name; }
		}
	}
}
