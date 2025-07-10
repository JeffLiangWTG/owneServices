using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBHeaderTest : TestCaseWithFactory
	{
		#region Calculation Logs Analyzer

		public void TestCalculationLogsAnalyzerType()
		{
			AssertEquals(typeof(CalculationLogsAnalyzer), AWBHeader.CalculationLogsAnalyzer.GetType());
		}

		[ExpectNoExceptions]
		public void TestCalculationLogsAnalyzer()
		{
			CalculationLog calculationLog = new CalculationLog();
			calculationLog.Unit = Core.Constants.Weight.Kilograms;
			calculationLog.Minimum = 100m;

			CalculationLogsWrapper wrapper = new CalculationLogsWrapper();
			wrapper.Logs.Add(calculationLog);

			DummyEnterpriseBusinessObject bizoWithLogs = Factory.New<DummyEnterpriseBusinessObject>();
			CalculationLogsLoader.Save(bizoWithLogs, wrapper);

			var headerMock = Factory.NewMoq<MockExportAWBHeader>();

			var analyzerMock = new Mock<CalculationLogsAnalyzer>(new object[] { headerMock.Object });
			analyzerMock.Protected().Setup<IEnumerable<BusinessObject>>("GetAllBusinessObjectsWithCalculationLogs").Returns(new List<BusinessObject> { bizoWithLogs });
			headerMock.Protected().Setup<CalculationLogsAnalyzer>("GetCalculationLogsAnalyzer").Returns(analyzerMock.Object);

			// RateLines populated by analyzer (Minimum line), default population logic not called (RateClass not called)
			headerMock.Object.Populate();
			analyzerMock.VerifyAll();
			headerMock.VerifyAll();

			headerMock = Factory.NewMoq<MockExportAWBHeader>();

			analyzerMock = new Mock<CalculationLogsAnalyzer>(new object[] { headerMock.Object });
			analyzerMock.Protected().Setup<IEnumerable<BusinessObject>>("GetAllBusinessObjectsWithCalculationLogs").Returns((IEnumerable<BusinessObject>)null);

			headerMock.Protected().Setup<CalculationLogsAnalyzer>("GetCalculationLogsAnalyzer").Returns(analyzerMock.Object);
			headerMock.Protected().Setup<ZString>("RateClass").Returns("A");

			// RateLines not populated by analyzer, default population logic was called (RateClass was called)
			headerMock.Object.Populate();
			analyzerMock.VerifyAll();
			headerMock.VerifyAll();
		}

		#endregion

		#region Rate Class

		public void TestRateClass()
		{
			MockHeaderForRateClassTesting header = Factory.New<MockHeaderForRateClassTesting>();
			header.RateLineWeightUnit_Exposed = Core.Constants.AWB.RateLineUQ.Kilos;
			header.RateLineChargeableWeight_Exposed = 10m;
			header.RateLineGrossWeight_Exposed = 100m;
			AssertEquals("RateClass based on chargeable", Core.Constants.AWB.RateClass.NormalCharge, header.RateClass);

			header.RateLineChargeableWeight_Exposed = 44m;
			AssertEquals("RateClass based on chargeable", Core.Constants.AWB.RateClass.NormalCharge, header.RateClass);

			header.RateLineChargeableWeight_Exposed = 45m;
			AssertEquals("RateClass based on chargeable", Core.Constants.AWB.RateClass.QuantityRate, header.RateClass);

			header.RateLineChargeableWeight_Exposed = 100m;
			header.RateLineGrossWeight_Exposed = 10m;
			AssertEquals("RateClass based on chargeable", Core.Constants.AWB.RateClass.QuantityRate, header.RateClass);

			header.RateLineWeightUnit_Exposed = Core.Constants.AWB.RateLineUQ.Pounds;
			header.RateLineChargeableWeight_Exposed = 80m;
			AssertEquals("RateClass depends on weight in kilograms: 80 pounds < 45 kilograms", Core.Constants.AWB.RateClass.NormalCharge, header.RateClass);

			header.RateLineChargeableWeight_Exposed = 120m;
			AssertEquals("RateClass depends on weight in kilograms: 120 pounds > 45 kilograms", Core.Constants.AWB.RateClass.QuantityRate, header.RateClass);
		}

		class MockHeaderForRateClassTesting : MockExportAWBHeader
		{
			public MockHeaderForRateClassTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZString RateClass
			{
				get { return base.RateClass; }
			}

			protected override ZDecimal RateLineChargeableWeight
			{
				get { return RateLineChargeableWeight_Exposed; }
			}
			public ZDecimal RateLineChargeableWeight_Exposed;

			protected override ZDecimal RateLineGrossWeight
			{
				get { return RateLineGrossWeight_Exposed; }
			}
			public ZDecimal RateLineGrossWeight_Exposed;

			protected override ZString RateLineWeightUnit
			{
				get { return RateLineWeightUnit_Exposed; }
			}
			public ZString RateLineWeightUnit_Exposed;
		}

		#endregion

		#region SecurityStatusIssuedBy

		public void TestEH_SecurityStatusIssuedByWhenSetEH_GS_NKSecurityStatusIssuedByCode()
		{
			AssertEquals("EH_SecurityStatusIssuedBy is empty by default", ZString.Empty, AWBHeader.EH_SecurityStatusIssuedBy);

			AWBHeader.EH_GS_NKSecurityStatusIssuedByCode = "ZZ";
			AssertEquals("When EH_GS_NKSecurityStatusIssuedByCode is ZZ, EH_SecurityStatusIssuedBy is CargoWise Web", "CargoWise Web", AWBHeader.EH_SecurityStatusIssuedBy);

			AWBHeader.EH_GS_NKSecurityStatusIssuedByCode = "C";
			AssertEquals("When EH_GS_NKSecurityStatusIssuedByCode is C, EH_SecurityStatusIssuedBy is Developer", "Developer", AWBHeader.EH_SecurityStatusIssuedBy);

			AWBHeader.EH_GS_NKSecurityStatusIssuedByCode = ZString.Empty;
			AssertEquals("When EH_GS_NKSecurityStatusIssuedByCode is empty, EH_SecurityStatusIssuedBy should be empty", ZString.Empty, AWBHeader.EH_SecurityStatusIssuedBy);
		}

		public void TestEH_SecurityStatusIssuedBy_ReadOnly()
		{
			Assert("EH_SecurityStatusIssuedBy is not read only by default", !AWBHeader.EH_SecurityStatusIssuedByInfo.ReadOnly);

			AWBHeader.EH_GS_NKSecurityStatusIssuedByCode = "ZZ";
			Assert("When EH_GS_NKSecurityStatusIssuedByCode has value, EH_SecurityStatusIssuedBy is read only", AWBHeader.EH_SecurityStatusIssuedByInfo.ReadOnly);

			AWBHeader.EH_GS_NKSecurityStatusIssuedByCode = string.Empty;
			Assert("When EH_GS_NKSecurityStatusIssuedByCode is blank, EH_SecurityStatusIssuedBy is not read only", !AWBHeader.EH_SecurityStatusIssuedByInfo.ReadOnly);
		}

		public void TestEH_SecurityStatusIssuedByAndEH_GS_NKSecurityStatusIssuedByCodeWhenIsAWBValuesOverriddenPropertyChanged()
		{
			const string errorMessage = "Person Screening is required.";
			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			forwardingConsol.JK_RL_NKLoadPort = "AUBNE";
			forwardingConsol.JK_RL_NKDischargePort = "CNSHA";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingConsol.PK;

			forwardingConsol.JK_OverrideSecurityDeclarationDefaults = true;
			awbHeader.EH_GS_NKSecurityStatusIssuedByCode = "ZZ";
			awbHeader.PopulateSecurityDeclarationIfNotOverridden();

			AssertEquals("EH_GS_NKSecurityStatusIssuedByCode is ZZ", "ZZ", awbHeader.EH_GS_NKSecurityStatusIssuedByCode);
			AssertEquals("EH_SecurityStatusIssuedBy is CargoWise Web", "CargoWise Web", awbHeader.EH_SecurityStatusIssuedBy);
			AssertNoMessageError(awbHeader.EH_SecurityStatusIssuedByInfo, errorMessage);

			forwardingConsol.IsCSDValuesOverriddenProperty = false;
			awbHeader.PopulateSecurityDeclarationIfNotOverridden();
			AssertEquals("EH_GS_NKSecurityStatusIssuedByCode is login user code by default", GlbStaff.CurrentUser.GS_Code, awbHeader.EH_GS_NKSecurityStatusIssuedByCode);
			AssertEquals("EH_SecurityStatusIssuedBy is login user fullname by default", GlbStaff.CurrentUser.GS_FullName, awbHeader.EH_SecurityStatusIssuedBy);
			AssertNoMessageError(awbHeader.EH_SecurityStatusIssuedByInfo, errorMessage);

			forwardingConsol.IsCSDValuesOverriddenProperty = true;
			awbHeader.EH_SecurityStatusIssuedBy = "";
			awbHeader.EH_GS_NKSecurityStatusIssuedByCode = ZString.Empty;
			awbHeader.PopulateSecurityDeclarationIfNotOverridden();
			AssertEquals("EH_GS_NKSecurityStatusIssuedByCode is empty", ZString.Empty, awbHeader.EH_GS_NKSecurityStatusIssuedByCode);
			AssertEquals("EH_SecurityStatusIssuedBy is empty", ZString.Empty, awbHeader.EH_SecurityStatusIssuedBy);
			AssertHasMessageError(awbHeader.EH_SecurityStatusIssuedByInfo, errorMessage);
		}

		#endregion

		public void TestDuplicateExchangeRateIsDeletedBeforeSaveWhenConvertingAmountsUsingJobExRateCurrencyConverter()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORGABC";
			var eurCurrency = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.EuropeanUnion)).First();
			var audCurrency = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Australia)).First();

			var eurBuyRate = eurCurrency.ExchangeRates.AddNew();
			eurBuyRate.RE_StartDate = ZDate.Today;
			eurBuyRate.RE_ExpiryDate = ZDate.Today.AddDays(10);
			eurBuyRate.RE_ExRateType = "BUY";
			eurBuyRate.RE_SellRate = 0.6m;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_JobNum = "Phony number";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var jobHeaderInNewFactory = newFactory.Load<JobHeader>(jobHeader.PK);
			var awbHeaderInNewFactory = newFactory.Load<MockHeaderForExchangeRateTesting>(AWBHeader.PK);
			var money = new Money(120m, eurCurrency);

			Assert("No exchange rates available", !((IExchangeRateSourceBase)jobHeaderInNewFactory).Any());
			awbHeaderInNewFactory.GetAmountBasedOnCurrencyObjectExposed(jobHeaderInNewFactory, audCurrency, money, org.PK, CostSell.Cost);
			var newExchangeRate = ((IExchangeRateSourceBase)jobHeaderInNewFactory).FirstOrDefault(x => x.CurrencyCode == Constants.CurrencyCodes.EuropeanUnion);
			AssertNotNull("New exchange rate is created", newExchangeRate);

			var sqlInsertRate =
				@"INSERT INTO dbo.JobExRate(JF_PK, JF_RX_NKRateCurrency, JF_JH, JF_BaseRate, JF_CFXMinimum, JF_CFXPercent, JF_IsTransformed, JF_OH_Org, JF_OrgType) VALUES
				(NEWID(), 'EUR', @JobPk, 0.6, 0, 0, 0, @Org, 'CRD')";

			using (var cmd = Db.Connection.Command(sqlInsertRate))
			{
				cmd.AddParameter("@JobPk", SqlDbType.UniqueIdentifier, jobHeader.PK.ToGuid());
				cmd.AddParameter("@Org", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			AssertNoExceptionThrown(() => newFactory.Save());
		}

		class MockHeaderForExchangeRateTesting : MockExportAWBHeader
		{
			public MockHeaderForExchangeRateTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDecimal GetAmountBasedOnCurrencyObjectExposed(JobHeader jobHeader, RefCurrency currencyObject, Money initialMonetaryAmount, ZGuid orgPK, CostSell costOrSell)
			{
				return base.GetAmountBasedOnCurrencyObject(jobHeader, currencyObject, initialMonetaryAmount, orgPK, costOrSell);
			}
		}

		public void TestHasOtherScreeningMethod()
		{
			var line1 = Factory.New<ExportAWBSecurityStatusLine>();
			line1.EAS_ScreeningMethod = ScreeningMethods.Codes.ExplosivesTraceDetectionEquipment;

			AWBHeader.CargoSecurityScreeningMethods.Add(line1);
			Assert(!AWBHeader.HasOtherScreeningMethod);

			var line2 = Factory.New<ExportAWBSecurityStatusLine>();
			line2.EAS_ScreeningMethod = ScreeningMethods.Codes.SubjectedToAnyOtherMeans;

			AWBHeader.CargoSecurityScreeningMethods.Add(line2);
			Assert(AWBHeader.HasOtherScreeningMethod);
		}

		public void TestSetReadonly()
		{
			AWBHeader.SetReadOnly(true);
			AssertEquals("Special Handling Items should be readonly", true, AWBHeader.AWBSpecialHandlingItems.ReadOnly);

			AWBHeader.SetReadOnly(false);
			AssertEquals("Special Handling Items should not be readonly", false, AWBHeader.AWBSpecialHandlingItems.ReadOnly);
		}

		public void TestSpecialHandlingItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			AWBHeader.SetConsol(consol);
			var specialHandlingItem = AWBHeader.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.EP_SpecialHandling = "EAP";
			AWBHeader.Factory.Save();

			AssertEquals(1, AWBHeader.AWBSpecialHandlingItems.Count);

			var newFactory = new BusinessObjectFactory();
			var newAWBHeader = newFactory.Load<MockExportAWBHeader>(AWBHeader.PK);
			AssertEquals(1, newAWBHeader.AWBSpecialHandlingItems.Count);
		}

		public void TestIATADescription()
		{
			AWBHeader.MaxOtherCharges = 3;
			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.House);

			AccChargeCode c1 = Factory.New<AccChargeCode>();
			c1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CH;
			c1.AC_Desc = "XYZ";

			AssertEquals("To ensure environment is bug-friendly.", Core.Constants.AWB.ChargeCodes.CH, ExportAWBHeader.Constants.ChargeCodes.PartialCollectCreditCard_PartialPrepaidCredit);

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1 }
			});
			AWBHeader.PopulateOtherCharges();
			AssertEquals("Accounting description is expected here. IATA descriptions are only expected when charges are grouped by IATA charge code", "XYZ", AWBHeader.AWBOtherCharges[0].EO_ChargeDescription);
		}

		public void TestAWBChargesGroupingResetsExistingUnchangedCharges()
		{
			AWBHeader.MaxOtherCharges = 3;
			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.House);
			ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var c1 = Factory.New<AccChargeCode>();
			c1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CI;
			c1.AC_Desc = "XYZ";

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap }
			});
			AWBHeader.PopulateOtherCharges();
			AssertEquals(100m, AWBHeader.AWBOtherCharges[0].EO_Amount);

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
									{
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap }
									});
			AWBHeader.PopulateOtherCharges();
			AssertEquals(200m, AWBHeader.AWBOtherCharges[0].EO_Amount);

			AWBHeader.AWBOtherCharges[0].HasChanges = false;    // to emulate it being loaded from saved data
			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap }
			});
			AWBHeader.PopulateOtherCharges();
			AssertEquals("Existing charges overwritten", 100m, AWBHeader.AWBOtherCharges[0].EO_Amount);
		}

		public void TestGroupByIATACode_Shipment()
		{
			AWBHeader.MaxOtherCharges = 6;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_PrepaidCollect = "COL";
			AWBHeader.SetConsol(consol);

			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.House);

			AddShipmentChargesToAwbHeader();
			AssertEquals(6, AWBHeader.AWBOtherCharges.Count);

			ExportAWBRegistry.Instance.MAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AddShipmentChargesToAwbHeader();
			AssertEquals("MAWB registry shouldnt group shipment charges", 6, AWBHeader.AWBOtherCharges.Count);

			ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AddShipmentChargesToAwbHeader();
			AssertEquals(4, AWBHeader.AWBOtherCharges.Count);

			var charge = FindCharge(Core.Constants.AWB.ChargeCodes.AC);

			AssertEquals("should use IATA description", "Animal container", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)300, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"A", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "PPD", charge.EO_PPDCLT);

			charge = FindCharge(Core.Constants.AWB.ChargeCodes.LA);

			AssertEquals("should use IATA description", "Live animals related services", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)1000, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"A", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "COL", charge.EO_PPDCLT);

			var blankCharges = FindCharges(ZString.Empty);

			AssertEquals("should not group blank IATA codes", 2, blankCharges.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "Test Charge Code Description|550", "Test Charge Code Description|820" },
				blankCharges.Select(blankCharge => ZString.Format("{0}|{1}", blankCharge.EO_ChargeDescription, blankCharge.EO_Amount)));

			var displayCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			displayCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "C";
			displayCollection[Core.Constants.AWB.ChargeCodes.LA].Entitlement = "C";
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			AddShipmentChargesToAwbHeader();
			AssertEquals(4, AWBHeader.AWBOtherCharges.Count);

			charge = FindCharge(Core.Constants.AWB.ChargeCodes.AC);

			AssertEquals("should use IATA description", "Animal container", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)300, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"C", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "PPD", charge.EO_PPDCLT);

			charge = FindCharge(Core.Constants.AWB.ChargeCodes.LA);

			AssertEquals("should use IATA description", "Live animals related services", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)1000, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"C", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "COL", charge.EO_PPDCLT);

			blankCharges = FindCharges(ZString.Empty);

			AssertEquals("should not group blank IATA codes", 2, blankCharges.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "Test Charge Code Description|550", "Test Charge Code Description|820" },
				blankCharges.Select(blankCharge => ZString.Format("{0}|{1}", blankCharge.EO_ChargeDescription, blankCharge.EO_Amount)));
		}

		[ExpectNoExceptions()]
		public void TestGroupByIATACode_Shipment_SameIATACode_CollectAndPrepaid_DifferentEntitlement()
		{
			AWBHeader.MaxOtherCharges = 6;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_PrepaidCollect = "COL";
			AWBHeader.SetConsol(consol);

			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.House);

			var c1 = Factory.New<AccChargeCode>();
			c1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CI;

			ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var displayCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			displayCollection[Core.Constants.AWB.ChargeCodes.CI].Entitlement = "C";
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			displayCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			displayCollection[Core.Constants.AWB.ChargeCodes.CI].Entitlement = "A";
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			AWBHeader.AWBOtherCharges.RemoveAndDeleteAll();
			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
									{
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(200), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(400), PrepaidCollect = "COL", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(600), PrepaidCollect = "COL", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
									});

			AWBHeader.PopulateOtherCharges();

			AssertEquals(2, AWBHeader.AWBOtherCharges.Count);

			var charges = FindCharges(Core.Constants.AWB.ChargeCodes.CI);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"Customs overtime fee and other charges|300|C|PPD",
				"Customs overtime fee and other charges|1000|A|COL"
			},
			charges.Select(charge => ZString.Format("{0}|{1}|{2}|{3}", charge.EO_ChargeDescription, charge.EO_Amount, charge.EO_EntitlementCode, charge.EO_PPDCLT)));

			displayCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			displayCollection[Core.Constants.AWB.ChargeCodes.CI].Entitlement = "C";
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			AWBHeader.AWBOtherCharges.RemoveAndDeleteAll();
			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
									{
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(200), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(400), PrepaidCollect = "COL", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
										new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(600), PrepaidCollect = "COL", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap },
									});

			AWBHeader.PopulateOtherCharges();

			AssertEquals(2, AWBHeader.AWBOtherCharges.Count);

			charges = FindCharges(Core.Constants.AWB.ChargeCodes.CI);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"PPD / Customs overtime fee and other charges|300|C|PPD",
				"COL / Customs overtime fee and other charges|1000|C|COL"
			},
			charges.Select(charge => ZString.Format("{0}|{1}|{2}|{3}", charge.EO_ChargeDescription, charge.EO_Amount, charge.EO_EntitlementCode, charge.EO_PPDCLT)));
		}

		public void TestGroupByIATACode_Consol()
		{
			AWBHeader.MaxOtherCharges = 6;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_PrepaidCollect = "COL";
			AWBHeader.SetConsol(consol);

			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.AgentMaster);

			AddConsolChargesToAwbHeader();
			AssertEquals(4, AWBHeader.AWBOtherCharges.Count);

			ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AddConsolChargesToAwbHeader();
			AssertEquals("HAWB registry shouldnt group consol charges", 4, AWBHeader.AWBOtherCharges.Count);

			ExportAWBRegistry.Instance.MAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AddConsolChargesToAwbHeader();
			AssertEquals(2, AWBHeader.AWBOtherCharges.Count);

			var charge = FindCharge(Core.Constants.AWB.ChargeCodes.AC);

			AssertEquals("should use IATA description", "Animal container", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)300, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"C", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "PPD", charge.EO_PPDCLT);

			charge = FindCharge(Core.Constants.AWB.ChargeCodes.LA);

			AssertEquals("should use IATA description", "Live animals related services", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)1000, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"C", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "COL", charge.EO_PPDCLT);

			var displayCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			displayCollection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "A";
			displayCollection[Core.Constants.AWB.ChargeCodes.LA].Entitlement = "A";
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);
			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			AddConsolChargesToAwbHeader();

			AssertEquals(2, AWBHeader.AWBOtherCharges.Count);

			charge = FindCharge(Core.Constants.AWB.ChargeCodes.AC);

			AssertEquals("should use IATA description", "Animal container", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)300, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"A", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "PPD", charge.EO_PPDCLT);

			charge = FindCharge(Core.Constants.AWB.ChargeCodes.LA);

			AssertEquals("should use IATA description", "Live animals related services", charge.EO_ChargeDescription);
			AssertEquals("should total amounts", (ZDecimal)1000, charge.EO_Amount);
			AssertEquals("should use correct enetitlement", (ZString)"A", charge.EO_EntitlementCode);
			AssertEquals("should group by PPDCLT", "COL", charge.EO_PPDCLT);
		}

		public void TestIATADescriptionsAreNotTranslated()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(new ResourceStringGetter(delegate(string key)
				{
					return new ResourceStringData(key, "do not use this translation");
				}));
				AWBHeader.MaxOtherCharges = 6;
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_PrepaidCollect = "COL";
				AWBHeader.SetConsol(consol);

				AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.AgentMaster);

				ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				ExportAWBRegistry.Instance.MAWBGroupOtherChargesByIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AddConsolChargesToAwbHeader();
			}

			AssertEquals("should use English IATA description", "Animal container", FindCharge(Core.Constants.AWB.ChargeCodes.AC).EO_ChargeDescription);
			AssertEquals("should use English IATA description", "Live animals related services", FindCharge(Core.Constants.AWB.ChargeCodes.LA).EO_ChargeDescription);
		}

		public void TestGetDisplayOptionInvalidIATACodeUsesMissingIATACodeDisplayOption_PrepaidShipment()
		{
			var displayCollectionHAWB = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.House);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "A";

			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			var template = GetTemplate("", "PPD");
			var option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			template = GetTemplate("XX", "PPD");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "C";

			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			template = GetTemplate("", "PPD");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);

			template = GetTemplate("XX", "PPD");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);
		}

		public void TestGetDisplayOptionInvalidIATACodeUsesMissingIATACodeDisplayOption_CollectShipment()
		{
			var displayCollectionHAWB = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.House);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "A";

			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			var template = GetTemplate("", "COL");
			var option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			template = GetTemplate("XX", "COL");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "C";

			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			template = GetTemplate("", "COL");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);

			template = GetTemplate("XX", "COL");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);
		}

		public void TestGetDisplayOptionInvalidIATACodeUsesMissingIATACodeDisplayOption_CollectConsol()
		{
			var displayCollectionHAWB = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.DirectMaster);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "A";

			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			var template = GetTemplate("", "COL");
			var option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			template = GetTemplate("XX", "COL");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "C";

			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			template = GetTemplate("", "COL");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);

			template = GetTemplate("XX", "COL");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);
		}

		public void TestGetDisplayOptionInvalidIATACodeUsesMissingIATACodeDisplayOption_PrepaidConsol()
		{
			var displayCollectionHAWB = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			AWBHeader.OverrideAWBType(ExportAWBHeader.TypeOfAWB.DirectMaster);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "A";

			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			var template = GetTemplate("", "PPD");
			var option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			template = GetTemplate("XX", "PPD");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Hide), option.Visibility);
			AssertEquals("A", option.Entitlement);

			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			displayCollectionHAWB[AWBDisplayOption.MissingIATACode].Entitlement = "C";

			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollectionHAWB);

			template = GetTemplate("", "PPD");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);

			template = GetTemplate("XX", "PPD");
			option = AWBHeader.GetDisplayOption(template.IATAChargeCode, template.PrepaidCollect);
			AssertEquals(nameof(AWBDisplayOptionVisibility.Show), option.Visibility);
			AssertEquals("C", option.Entitlement);
		}

		OtherChargeTemplate GetTemplate(ZString iataCode, string ppdclt)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_IATA_ChargeCodeMap = iataCode;

			return new OtherChargeTemplate(AWBHeader) { AccChargeCode = chargeCode, PrepaidCollect = ppdclt };
		}

		public void TestReferenceNumberIsReadOnly()
		{
			Assert("EH_ReferenceNumber should be read only", AWBHeader.EH_ReferenceNumberInfo.ReadOnly);
		}

		public void TestResetAddressPickerDropLists_ConsigneeAddressPickList()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			shipment.Consignee.MainAddress.OA_Address1 = "TEST";

			ExportAWBHeader aWBHeader = shipment.AWBHeader;

			AssertEquals(2, aWBHeader.ConsigneeAddressPickList.Count);

			OrgAddress orgAddress = shipment.Consignee.Addresses.AddNew();
			orgAddress.OA_Address1 = "TEST2";
			shipment.ResetAddressPickerDropLists();
			AssertEquals(3, shipment.AWBHeader.ConsigneeAddressPickList.Count);
		}

		public void TestResetAddressPickerDropLists_ShipperAddressPickList()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			shipment.Consignor.MainAddress.OA_Address1 = "TEST";

			ExportAWBHeader aWBHeader = shipment.AWBHeader;

			AssertEquals(2, aWBHeader.ShipperAddressPickList.Count);

			OrgAddress orgAddress = shipment.Consignor.Addresses.AddNew();
			orgAddress.OA_Address1 = "TEST2";
			shipment.ResetAddressPickerDropLists();
			AssertEquals(3, shipment.AWBHeader.ShipperAddressPickList.Count);
		}

		public void TestResetAddressPickerDropLists_AlsoNotifyAddressPickList()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			shipment.Consignee.MainAddress.OA_Address1 = "TEST";

			ExportAWBHeader aWBHeader = shipment.AWBHeader;

			AssertEquals("Documentary address is empty - so only consignee address expected", 1, aWBHeader.AlsoNotifyAddressPickList.Count);

			OrgAddress orgAddress = shipment.Consignee.Addresses.AddNew();
			orgAddress.OA_Address1 = "TEST2";
			shipment.ResetAddressPickerDropLists();
			AssertEquals(2, shipment.AWBHeader.AlsoNotifyAddressPickList.Count);
		}

		#region TraderTypesAndNumbers

		public void TestPopulateTraderTypesAndNumbers()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCode(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, Constants.CountryCodes.Argentina, Constants.CountryCodes.Argentina, 1, "CUI", "CUI", "AWB");
			CreateRefDocOrgCusCode(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Constants.CountryCodes.Brazil, Constants.CountryCodes.Brazil, 1, "CJN", "CJN", "AWB");

			AWBHeader.Origin = "ARBUE";
			AWBHeader.Destination = "BRBSE";
			AWBHeader.ExtraShipperDataForTesting = "AAA: NO123";
			AWBHeader.RegistrationNumberForTesting = "BBB: NO456";

			var taxCodeAR = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCodeAR.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCodeAR.OK_RN_NKCodeCountry = Constants.CountryCodes.Argentina;
			taxCodeAR.OK_CustomsRegNo = "1111";

			AWBHeader.Organisation.OH_Category = "NAT";
			var taxCodeBR = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCodeBR.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			taxCodeBR.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			taxCodeBR.OK_CustomsRegNo = "2222";

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			AWBHeader.Populate();

			AssertEquals(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("1111", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("TE +96(3)766 AAA: NO123", AWBHeader.EH_ShipperOverride5);
			AssertEquals("AR", AWBHeader.EH_ShipperTraderNoCountryCode);
			AssertEquals("NAT", AWBHeader.ShipperCategory);

			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("2222", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("TE +96(3)766 BBB: NO456", AWBHeader.EH_ConsigneeOverride5);
			AssertEquals("BR", AWBHeader.EH_ConsigneeTraderNoCountryCode);
			AssertEquals("NAT", AWBHeader.ConsigneeCategory);

			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("BR", AWBHeader.EH_AlsoNotifyTraderNoCountryCode);
			AssertEquals("NAT", AWBHeader.NotifyPartyCategory);

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AWBHeader.Populate();

			AssertEquals(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("1111", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("TE +96(1)723 CONTACT NAME AAA: NO123", AWBHeader.EH_ShipperOverride5);

			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("2222", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("TE +96(1)723 CONTACT NAME BBB: NO456", AWBHeader.EH_ConsigneeOverride5);

			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_AlsoNotifyTraderNoType);

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Pickup;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Delivery;
			AWBHeader.Populate();

			AssertEquals(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("1111", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("AAA: NO123", AWBHeader.EH_ShipperOverride5);

			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("2222", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("BBB: NO456", AWBHeader.EH_ConsigneeOverride5);

			AssertEquals(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_AlsoNotifyTraderNoType);
		}

		public void TestShipmentDocumentType()
		{
			AWBHeader.SetSupportedTaxDocumentType("HAW");

			CreateRefDocOrgCusCode(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, Constants.CountryCodes.Argentina, Constants.CountryCodes.Argentina, 1, "LABEL-B", "CUI", "HAW");

			CreateRefDocOrgCusCode(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Constants.CountryCodes.Brazil, Constants.CountryCodes.Brazil, 1, "LABEL-D", "CJN", "HAW");

			AWBHeader.Origin = "ARBUE";
			AWBHeader.Destination = "BRBSE";
			AWBHeader.ExtraShipperDataForTesting = "AAA: NO123";
			AWBHeader.RegistrationNumberForTesting = "BBB: NO456";

			var taxCodeAR = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCodeAR.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCodeAR.OK_RN_NKCodeCountry = Constants.CountryCodes.Argentina;
			taxCodeAR.OK_CustomsRegNo = "1111";

			AWBHeader.Organisation.OH_Category = "NAT";
			var taxCodeBR = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCodeBR.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			taxCodeBR.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			taxCodeBR.OK_CustomsRegNo = "2222";

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			AWBHeader.Populate();

			AssertEquals("LABEL-B", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("1111", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("TE +96(3)766 AAA: NO123", AWBHeader.EH_ShipperOverride5);
			AssertEquals("AR", AWBHeader.EH_ShipperTraderNoCountryCode);
			AssertEquals("NAT", AWBHeader.ShipperCategory);

			AssertEquals("LABEL-D", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("2222", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("TE +96(3)766 BBB: NO456", AWBHeader.EH_ConsigneeOverride5);
			AssertEquals("BR", AWBHeader.EH_ConsigneeTraderNoCountryCode);
			AssertEquals("NAT", AWBHeader.ConsigneeCategory);

			AssertEquals("LABEL-D", AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("BR", AWBHeader.EH_AlsoNotifyTraderNoCountryCode);
			AssertEquals("NAT", AWBHeader.NotifyPartyCategory);

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AWBHeader.Populate();

			AssertEquals("LABEL-B", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("1111", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("TE +96(1)723 CONTACT NAME AAA: NO123", AWBHeader.EH_ShipperOverride5);

			AssertEquals("LABEL-D", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("2222", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("TE +96(1)723 CONTACT NAME BBB: NO456", AWBHeader.EH_ConsigneeOverride5);

			AssertEquals("LABEL-D", AWBHeader.EH_AlsoNotifyTraderNoType);

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Pickup;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Delivery;
			AWBHeader.Populate();

			AssertEquals("LABEL-B", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("1111", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("AAA: NO123", AWBHeader.EH_ShipperOverride5);

			AssertEquals("LABEL-D", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("2222", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("BBB: NO456", AWBHeader.EH_ConsigneeOverride5);

			AssertEquals("LABEL-D", AWBHeader.EH_AlsoNotifyTraderNoType);
		}

		public void TestPopulateTraderNumbers_NotPopulateWhenExceedMaxLength()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCode(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, Constants.CountryCodes.Argentina, Constants.CountryCodes.Argentina, 1, "CUI", "CUI", "AWB");

			CreateRefDocOrgCusCode(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Constants.CountryCodes.Brazil, Constants.CountryCodes.Brazil, 1, "CJN", "CJN", "AWB");

			AWBHeader.Origin = "ARBUE";
			AWBHeader.Destination = "BRBSE";
			AWBHeader.ExtraShipperDataForTesting = "AAA: NO123";
			AWBHeader.RegistrationNumberForTesting = "BBB: NO456";

			var taxCodeAR = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCodeAR.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCodeAR.OK_RN_NKCodeCountry = Constants.CountryCodes.Argentina;
			taxCodeAR.OK_CustomsRegNo = "A";

			var taxCodeBR = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCodeBR.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			taxCodeBR.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			taxCodeBR.OK_CustomsRegNo = "B";

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("EH_ShipperTraderNoType", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, AWBHeader.EH_ShipperTraderNoType);
				AssertEquals("EH_ShipperTraderNo: Populate when tax number is less than 35 characters", "A", AWBHeader.EH_ShipperTraderNo);
				Assert("IsShipperTraderNoExceedingMaxLength", !AWBHeader.IsShipperTraderNoExceedingMaxLength);

				AssertEquals("EH_ConsigneeTraderNoType", BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("EH_ConsigneeTraderNo: Populate when tax number is less than 35 characters", "B", AWBHeader.EH_ConsigneeTraderNo);
				Assert("IsShipperTraderNoExceedingMaxLength", !AWBHeader.IsConsigneeTraderNoExceedingMaxLength);

				AssertEquals("EH_AlsoNotifyTraderNoType", BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_AlsoNotifyTraderNoType);
				AssertEquals("EH_AlsoNotifyTraderNo: Populate when tax number is less than 35 characters", "B", AWBHeader.EH_AlsoNotifyTraderNo);
				Assert("IsShipperTraderNoExceedingMaxLength", !AWBHeader.IsAlsoNotifyTraderNoExceedingMaxLength);
			});

			taxCodeAR.OK_CustomsRegNo = "12345678901234567890123456789012345A";
			taxCodeBR.OK_CustomsRegNo = "12345678901234567890123456789012345B";
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("EH_ShipperTraderNoType", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, AWBHeader.EH_ShipperTraderNoType);
				AssertEquals("EH_ShipperTraderNo: Not Populate when tax number is longer than 35 characters", string.Empty, AWBHeader.EH_ShipperTraderNo);
				Assert("IsShipperTraderNoExceedingMaxLength", AWBHeader.IsShipperTraderNoExceedingMaxLength);

				AssertEquals("EH_ConsigneeTraderNoType", BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("EH_ConsigneeTraderNo: Not Populate when tax number is longer than 35 characters", string.Empty, AWBHeader.EH_ConsigneeTraderNo);
				Assert("IsShipperTraderNoExceedingMaxLength", AWBHeader.IsConsigneeTraderNoExceedingMaxLength);

				AssertEquals("EH_AlsoNotifyTraderNoType", BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, AWBHeader.EH_AlsoNotifyTraderNoType);
				AssertEquals("EH_AlsoNotifyTraderNo: Not Populate when tax number is longer than 35 characters", string.Empty, AWBHeader.EH_AlsoNotifyTraderNo);
				Assert("IsShipperTraderNoExceedingMaxLength", AWBHeader.IsAlsoNotifyTraderNoExceedingMaxLength);
			});
		}

		public void TestShipperTraderTypeWithNo()
		{
			var header = Factory.New<MockExportAWBHeader>();

			header.EH_ShipperTraderNoType = "AAA";
			header.EH_ShipperTraderNo = "1111";

			AssertEquals("AAA: 1111", header.ShipperTraderTypeWithNoForTesting);

			header.EH_ShipperTraderNo = string.Empty;
			AssertEquals(string.Empty, header.ShipperTraderTypeWithNoForTesting);

			header.EH_ShipperTraderNoType = string.Empty;
			header.EH_ShipperTraderNo = "1111";

			AssertEquals("1111", (string)header.ShipperTraderTypeWithNoForTesting);
		}

		public void TestConsigneeTraderTypeWithNo()
		{
			var header = Factory.New<MockExportAWBHeader>();

			header.EH_ConsigneeTraderNoType = "BBB";
			header.EH_ConsigneeTraderNo = "2222";

			AssertEquals("BBB: 2222", header.ConsigneeTraderTypeWithNoForTesting);

			header.EH_ConsigneeTraderNo = string.Empty;
			AssertEquals(string.Empty, header.ConsigneeTraderTypeWithNoForTesting);

			header.EH_ConsigneeTraderNoType = string.Empty;
			header.EH_ConsigneeTraderNo = "2222";

			AssertEquals("2222", (string)header.ConsigneeTraderTypeWithNoForTesting);
		}

		public void TestAlsoNotifyTraderTypeWithNo()
		{
			var header = Factory.New<MockExportAWBHeader>();

			header.EH_AlsoNotifyTraderNoType = "CCC";
			header.EH_AlsoNotifyTraderNo = "3333";

			AssertEquals("CCC: 3333", header.AlsoNotifyTraderTypeWithNoForTesting);

			header.EH_AlsoNotifyTraderNo = string.Empty;
			AssertEquals(string.Empty, header.AlsoNotifyTraderTypeWithNoForTesting);

			header.EH_AlsoNotifyTraderNoType = string.Empty;
			header.EH_AlsoNotifyTraderNo = "3333";

			AssertEquals("3333", (string)header.AlsoNotifyTraderTypeWithNoForTesting);
		}

		public void TestShipperTraderTypeWithNo_FromRefTable_ForNotChina()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCodesForTesting();

			AWBHeader.Origin = "BRACR";
			AWBHeader.Destination = "USCHI";
			AWBHeader.Populate();

			AssertEquals("From short label of BR's CJN on 'AWB' document type", ZString.Empty, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("No CJN code on shipper", ZString.Empty, AWBHeader.EH_ShipperTraderNo);

			AWBHeader.Origin = "IDANR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", ZString.Empty, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("No PPN code on shipper", ZString.Empty, AWBHeader.EH_ShipperTraderNo);

			var cjnTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			cjnTaxCode.OK_CodeType = "CJN";
			cjnTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cjnTaxCode.OK_CustomsRegNo = "1111";

			var ppnTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			ppnTaxCode.OK_CodeType = "PPN";
			ppnTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Indonesia;
			ppnTaxCode.OK_CustomsRegNo = "2222";

			AWBHeader.Origin = "BRACR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", "Short CJN", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From CJN code on shipper", "1111", AWBHeader.EH_ShipperTraderNo);

			AWBHeader.Origin = "IDANR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", "Short PPN", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From PPN code on shipper", "2222", AWBHeader.EH_ShipperTraderNo);

			AWBHeader.Origin = "AUSYD";
			AWBHeader.Populate();
		}

		public void TestConsingeeTraderTypeWithNo_FromRefTable_ForNotChina()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCodesForTesting();

			AWBHeader.Origin = "USCHI";
			AWBHeader.Destination = "BRACR";
			AWBHeader.Populate();

			AssertEquals("From short label of BR's CJN on 'AWB' document type", ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("No CJN code on Consignee", ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);

			AWBHeader.Destination = "IDANR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("No PPN code on Consignee", ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);

			var cjnTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			cjnTaxCode.OK_CodeType = "CJN";
			cjnTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cjnTaxCode.OK_CustomsRegNo = "1111";

			var ppnTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			ppnTaxCode.OK_CodeType = "PPN";
			ppnTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Indonesia;
			ppnTaxCode.OK_CustomsRegNo = "2222";

			AWBHeader.Destination = "BRACR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", "Short CJN", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("From CJN code on Consignee", "1111", AWBHeader.EH_ConsigneeTraderNo);

			AWBHeader.Destination = "IDANR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", "Short PPN", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("From PPN code on Consignee", "2222", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestAlsoNotifyTraderTypeWithNo_FromRefTable_ForNotChina()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCodesForTesting();

			AWBHeader.Origin = "USCHI";
			AWBHeader.Destination = "BRACR";
			AWBHeader.Populate();

			AssertEquals("From short label of BR's CJN on 'AWB' document type", ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("No CJN code on AlsoNotify", ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);

			AWBHeader.Destination = "IDANR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("No PPN code on AlsoNotify", ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);

			var cjnTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			cjnTaxCode.OK_CodeType = "CJN";
			cjnTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			cjnTaxCode.OK_CustomsRegNo = "1111";

			var ppnTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			ppnTaxCode.OK_CodeType = "PPN";
			ppnTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Indonesia;
			ppnTaxCode.OK_CustomsRegNo = "2222";

			AWBHeader.Destination = "BRACR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", "Short CJN", AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("From CJN code on AlsoNotify", "1111", AWBHeader.EH_AlsoNotifyTraderNo);

			AWBHeader.Destination = "IDANR";
			AWBHeader.Populate();

			AssertEquals("From short label of ID's PPN on 'ALL' document type", "Short PPN", AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("From PPN code on AlsoNotify", "2222", AWBHeader.EH_AlsoNotifyTraderNo);

			AWBHeader.Destination = "AUSYD";
			AWBHeader.Populate();
		}

		public void TestPopulateTraderTypesAndNumbers_EUCountriesExcludingNorthernIreland_CountryCodePresentedInTaxNumber() => TestPopulateTraderTypesAndNumbers_EUCountriesExcludingNorthernIreland(true);
		public void TestPopulateTraderTypesAndNumbers_EUCountriesExcludingNorthernIreland_CountryCodeNotPresentedInTaxNumber() => TestPopulateTraderTypesAndNumbers_EUCountriesExcludingNorthernIreland(false);

		void TestPopulateTraderTypesAndNumbers_EUCountriesExcludingNorthernIreland(bool countryCodePresentedInTaxNumber)
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			var countryVatCodes = ExportAWBHeader.CountryVatCodeType.Value;

			foreach (var item in countryVatCodes.Where(x => x.Key != Constants.CountryCodes.UnitedKingdom && x.Key != Constants.CountryCodes.IsleOfMan))
			{
				var countryCode = item.Key;
				var vatCode = item.Value;
				var vatPrefix = countryCode;
				if (countryCode.Equals(Constants.CountryCodes.Greece))
				{
					vatPrefix = "EL";
				}
				else if (countryCode.Equals(Constants.CountryCodes.Monaco))
				{
					vatPrefix = Constants.CountryCodes.France;
				}

				CreateRefDocOrgCusCode(vatCode, countryCode, countryCode, 1, vatCode, vatCode, "AWB");

				var taxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
				taxCode.OK_CodeType = vatCode;
				taxCode.OK_RN_NKCodeCountry = countryCode;
				taxCode.OK_CustomsRegNo = (countryCodePresentedInTaxNumber ? vatPrefix : "") + "123456789123456789";

				var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.RL_RN_NKCountryCode = countryCode;

				AWBHeader.Origin = unloco.RL_Code;
				AWBHeader.Destination = unloco.RL_Code;

				AWBHeader.Populate();

				CombineAssertions(() =>
				{
					var expectedTraderNo = vatPrefix + "123456789123456789";

					AssertEquals("EH_ShipperTraderNoType", vatCode, AWBHeader.EH_ShipperTraderNoType);
					AssertEquals("EH_ShipperTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ShipperTraderNo);

					AssertEquals("EH_ConsigneeTraderNoType", vatCode, AWBHeader.EH_ConsigneeTraderNoType);
					AssertEquals("EH_ConsigneeTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ConsigneeTraderNo);

					AssertEquals("EH_AlsoNotifyTraderNoType", vatCode, AWBHeader.EH_AlsoNotifyTraderNoType);
					AssertEquals("EH_AlsoNotifyTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_AlsoNotifyTraderNo);
				});
			}
		}

		public void TestPopulateTraderTypesAndNumbers_NorthernIreland_CountryCodePresentedInTaxNumber() => TestPopulateTraderTypesAndNumbers_NorthernIreland(true);
		public void TestPopulateTraderTypesAndNumbers_NorthernIreland_CountryCodeNotPresentedInTaxNumber() => TestPopulateTraderTypesAndNumbers_NorthernIreland(false);

		void TestPopulateTraderTypesAndNumbers_NorthernIreland(bool countryCodePresentedInTaxNumber)
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			var countryCode = Constants.CountryCodes.UnitedKingdom;
			var vatCode = "VAT";
			var vatPrefix = "XI";

			CreateRefDocOrgCusCode(vatCode, countryCode, countryCode, 1, vatCode, vatCode, "AWB");

			var taxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCode.OK_CodeType = vatCode;
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = (countryCodePresentedInTaxNumber ? vatPrefix : "") + "123456789";

			var northernIrelandState = Factory.NewWithValidTestData<RefCountryStates>();
			northernIrelandState.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = countryCode;
			unloco.RL_RW = northernIrelandState.PK;

			AWBHeader.Origin = unloco.RL_Code;
			AWBHeader.Destination = unloco.RL_Code;

			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				var expectedTraderNo = vatPrefix + "123456789";

				AssertEquals("EH_ShipperTraderNoType", vatCode, AWBHeader.EH_ShipperTraderNoType);
				AssertEquals("EH_ShipperTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ShipperTraderNo);

				AssertEquals("EH_ConsigneeTraderNoType", vatCode, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("EH_ConsigneeTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ConsigneeTraderNo);

				AssertEquals("EH_AlsoNotifyTraderNoType", vatCode, AWBHeader.EH_AlsoNotifyTraderNoType);
				AssertEquals("EH_AlsoNotifyTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_AlsoNotifyTraderNo);
			});
		}

		public void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode_Austria() => TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode("AT", "UID", "AT1234567", "AT");
		public void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode_Cyprus() => TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode("CY", "VAT", "CY1234567", "CY");
		public void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode_Spain() => TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode("ES", "NIF", "ES1234567", "ES");
		public void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode_France() => TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode("FR", "TVA", "FR123456789", "FR");
		public void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode_Monaco() => TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode("MC", "TVA", "FR123456789", "FR");
		public void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode_Ireland() => TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode("IE", "VAT", "IE123456", "IE");
		public void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode_Netherlands() => TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode("NL", "BTW", "NL1234567890", "NL");

		void TestPopulateTraderTypesAndNumbers_EUVatNumberContainsLeadingCountryCode(string countryCode, string vatCode, string vatNumber, string vatPrefix)
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCode(vatCode, countryCode, countryCode, 1, vatCode, vatCode, "AWB");

			var taxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCode.OK_CodeType = vatCode;
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = vatNumber;

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = countryCode;

			AWBHeader.Origin = unloco.RL_Code;
			AWBHeader.Destination = unloco.RL_Code;

			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				var expectedTraderNo = vatPrefix + vatNumber;

				AssertEquals("EH_ShipperTraderNoType", vatCode, AWBHeader.EH_ShipperTraderNoType);
				AssertEquals("EH_ShipperTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ShipperTraderNo);

				AssertEquals("EH_ConsigneeTraderNoType", vatCode, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("EH_ConsigneeTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ConsigneeTraderNo);

				AssertEquals("EH_AlsoNotifyTraderNoType", vatCode, AWBHeader.EH_AlsoNotifyTraderNoType);
				AssertEquals("EH_AlsoNotifyTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_AlsoNotifyTraderNo);
			});
		}

		public void TestPopulateTraderTypesAndNumbers_UnitedKingdom_ShouldNotAppendCountryCode() => TestPopulateTraderTypesAndNumbers_UnitedKingdomAndIsleOfMan_ShouldNotAppendCountryCode(Constants.CountryCodes.UnitedKingdom);
		public void TestPopulateTraderTypesAndNumbers_IsleOfMan_ShouldNotAppendCountryCode() => TestPopulateTraderTypesAndNumbers_UnitedKingdomAndIsleOfMan_ShouldNotAppendCountryCode(Constants.CountryCodes.IsleOfMan);

		void TestPopulateTraderTypesAndNumbers_UnitedKingdomAndIsleOfMan_ShouldNotAppendCountryCode(string countryCode)
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			var vatCode = "VAT";

			CreateRefDocOrgCusCode(vatCode, countryCode, countryCode, 1, vatCode, vatCode, "AWB");

			var taxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCode.OK_CodeType = vatCode;
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = "123456789";

			var unitedKingdom = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			unitedKingdom.RN_EconomicGrouping = ZString.Empty;

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = countryCode;
			unloco.RL_RW = ZGuid.Empty;

			AWBHeader.Origin = unloco.RL_Code;
			AWBHeader.Destination = unloco.RL_Code;

			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				var expectedTraderNo = "123456789";

				AssertEquals("EH_ShipperTraderNoType", vatCode, AWBHeader.EH_ShipperTraderNoType);
				AssertEquals("EH_ShipperTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ShipperTraderNo);

				AssertEquals("EH_ConsigneeTraderNoType", vatCode, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("EH_ConsigneeTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_ConsigneeTraderNo);

				AssertEquals("EH_AlsoNotifyTraderNoType", vatCode, AWBHeader.EH_AlsoNotifyTraderNoType);
				AssertEquals("EH_AlsoNotifyTraderNo should have country code prefix", expectedTraderNo, AWBHeader.EH_AlsoNotifyTraderNo);
			});
		}

		public void TestShipperTraderTypeWithNo_FromRefTable_ForEgypt()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCodesForTesting();

			AWBHeader.Origin = "SGSIN";
			AWBHeader.Destination = "JMKIN";
			AWBHeader.Populate();

			AssertEquals("From SG ADI", ZString.Empty, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From SG ADI", ZString.Empty, AWBHeader.EH_ShipperTraderNo);

			AWBHeader.Destination = "EGCAI";
			AWBHeader.Populate();

			AssertEquals("From SG -> EG IVA", ZString.Empty, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From SG -> EG IVA", ZString.Empty, AWBHeader.EH_ShipperTraderNo);

			var ivaTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			ivaTaxCode.OK_CodeType = "IVA";
			ivaTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;
			ivaTaxCode.OK_CustomsRegNo = "8888";

			var ivbTaxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			ivbTaxCode.OK_CodeType = "IVB";
			ivbTaxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;
			ivbTaxCode.OK_CustomsRegNo = "9999";

			AWBHeader.Populate();

			AssertEquals("From IVA", "Short IVA", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From IVA", "8888", AWBHeader.EH_ShipperTraderNo);

			AWBHeader.Organisation.CustomsCodes.Remove(ivaTaxCode);
			AWBHeader.Populate();

			AssertEquals("From IVB as IVA is not present", "Short IVB", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From IVB as IVA is not present", "9999", AWBHeader.EH_ShipperTraderNo);
		}

		public void TestShipperTraderTypeWithNo_FromRefTable_ForEU()
		{
			CreateRefDocOrgCusCodesForTesting();

			AWBHeader.Origin = "NOALN";
			AWBHeader.Destination = "SGAYC";
			AWBHeader.Populate();

			AssertEquals("From NO", ZString.Empty, AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From NO", ZString.Empty, AWBHeader.EH_ShipperTraderNo);

			var code1 = AWBHeader.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();

			AssertEquals("From NO", "Short IOS", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("From NO", "123456789", AWBHeader.EH_ShipperTraderNo);
		}

		void CreateRefDocOrgCusCodesForTesting()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCode("CJN", "BR", "BR", 1, "Short CJN", "Long CJN", "AWB");

			CreateRefDocOrgCusCode("PPN", "ID", "ID", 1, "Short PPN", "Long PPN", "AWB");

			CreateRefDocOrgCusCode("ADI", "SG", "SG", 1, "Short ADI", "Long ADI", "AWB");

			CreateRefDocOrgCusCode("IVA", "EG", "SG", 1, "Short IVA", "Long IVA", "AWB");

			CreateRefDocOrgCusCode("IVB", "EG", "SG", 2, "Short IVB", "Long IVB", "AWB");

			CreateRefDocOrgCusCode("EOR", "DE", "DE", 1, "Short EOR", "Long EOR", "AWB");

			CreateRefDocOrgCusCode("IOS", "NO", "NO", 1, "Short IOS", "Long IOS", "AWB");
		}

		void CreateRefDocOrgCusCode(ZString code, ZString regulatingCountry, ZString codeCountry, ZByte priority, ZString shortLabel, ZString longLabel, ZString documentType)
		{
			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = code;
			orgCusCode.DOC_RN_NKRegulatingCountry = regulatingCountry;
			orgCusCode.DOC_RN_NKCodeCountry = codeCountry;
			orgCusCode.DOC_Priority = priority;
			orgCusCode.DOC_ShortLabel = shortLabel;
			orgCusCode.DOC_LongLabel = longLabel;
			orgCusCode.DOC_DocumentType = documentType;
		}

		#endregion

		public void TestPopulateAWBAddress_Override5InCHS()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				var orgAddress = Factory.New<OrgAddress>();
				orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
				orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
				orgAddress.OA_Address1 = "ADDRESSAWB";
				orgAddress.OA_Address2 = "ADDRESSAWB2";
				orgAddress.OA_City = "BRISBANE";
				orgAddress.OA_PostCode = "2266";
				orgAddress.OA_State = "NSW";
				orgAddress.OA_Phone = "+96852";
				orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

				AWBHeader.Organisation.Addresses.Add(orgAddress);
				AWBHeader.Populate();
			}

			Assert(AWBHeader.EH_ShipperOverride5.StartsWith("TE"));
			Assert(AWBHeader.EH_ConsigneeOverride5.StartsWith("TE"));
			Assert(AWBHeader.EH_NotifyOverride5.StartsWith("TE"));
		}

		public void TestPopulateShipperAWBAddress_FallbackAWBAddress()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ShipperAddress);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_ShipperName);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ShipperAddress2);
			AssertEquals("BRISBANE", AWBHeader.EH_ShipperPlace);
			AssertEquals("NSW", AWBHeader.EH_ShipperState);
			AssertEquals("2266", AWBHeader.EH_ShipperPostCode);
			AssertEquals("AU", AWBHeader.EH_ShipperCountryCode);
			AssertEquals("", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("+96852", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_ShipperOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ShipperOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ShipperOverride3);
			AssertEquals("BRISBANE NSW 2266 AU", AWBHeader.EH_ShipperOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_ShipperOverride5);
		}

		public void TestPopulateShipperAWBAddress_FallbackMainAWBAddress()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME1";
			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "SYDNEY";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ShipperAddress);
			AssertEquals("Using main AWB address to populate Shipper address", "AWBCOMPANYNAME1", AWBHeader.EH_ShipperName);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ShipperAddress2);
			AssertEquals("SYDNEY", AWBHeader.EH_ShipperPlace);
			AssertEquals("NSW", AWBHeader.EH_ShipperState);
			AssertEquals("2266", AWBHeader.EH_ShipperPostCode);
			AssertEquals("AU", AWBHeader.EH_ShipperCountryCode);
			AssertEquals("", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("+96852", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("AWBCOMPANYNAME1", AWBHeader.EH_ShipperOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ShipperOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ShipperOverride3);
			AssertEquals("SYDNEY NSW 2266 AU", AWBHeader.EH_ShipperOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_ShipperOverride5);

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ShipperAddress);
			AssertEquals("Using main AWB address to populate Shipper address", "AWBCOMPANYNAME1", AWBHeader.EH_ShipperName);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ShipperAddress2);
			AssertEquals("SYDNEY", AWBHeader.EH_ShipperPlace);
			AssertEquals("NSW", AWBHeader.EH_ShipperState);
			AssertEquals("2266", AWBHeader.EH_ShipperPostCode);
			AssertEquals("AU", AWBHeader.EH_ShipperCountryCode);
			AssertEquals("CONTACT NAME", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("+96852", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("AWBCOMPANYNAME1", AWBHeader.EH_ShipperOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ShipperOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ShipperOverride3);
			AssertEquals("SYDNEY NSW 2266 AU", AWBHeader.EH_ShipperOverride4);
			AssertEquals("TE +96852 CONTACT NAME", AWBHeader.EH_ShipperOverride5);
		}

		public void TestPopulateShipperAWBContactDetails_PopulatedBasedOnSelectedContact()
		{
			var shipperContact = Factory.New<OrgContact>();

			shipperContact.OC_OH = AWBHeader.Organisation.PK;
			shipperContact.OC_ContactName = "CONTACT NAME";
			shipperContact.OC_Phone = "+11111";
			shipperContact.OC_Fax = "+22222";

			var shipperAddress = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery, false);
			shipperAddress.OA_Phone = "+33333";
			shipperAddress.OA_Fax = "+44444";

			CombineAssertions(() =>
			{
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ShipperContactName);
				AssertEquals("Contact Phone Code is expected.", Core.Constants.AWB.ContactCodes.TELEPHONE, AWBHeader.EH_ShipperContactCode);
				AssertEquals("Contact Phone is expected.", "+11111", AWBHeader.EH_ShipperContactDetail);

				shipperContact.OC_Phone = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ShipperContactName);
				AssertEquals("Address Phone Code is expected, since Contact's one is not availalable.",
					Core.Constants.AWB.ContactCodes.TELEPHONE, AWBHeader.EH_ShipperContactCode);
				AssertEquals("Address Phone is expected, since Contact's one is not availalable.",
					"+33333", AWBHeader.EH_ShipperContactDetail);

				shipperAddress.OA_Phone = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ShipperContactName);
				AssertEquals("Contact Fax Code is expected.", Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_ShipperContactCode);
				AssertEquals("Contact Fax is expected.", "+22222", AWBHeader.EH_ShipperContactDetail);

				shipperContact.OC_Fax = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ShipperContactName);
				AssertEquals("Address Fax Code is expected, since Contact's one is not availalable.",
					Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_ShipperContactCode);
				AssertEquals("Address Fax is expected, since Contact's one is not availalable.",
					"+44444", AWBHeader.EH_ShipperContactDetail);
			});
		}

		public void TestPopulateConsigneeAWBAddress_FallbackAWBAddress()
		{
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ConsigneeAddress);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_ConsigneeName);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ConsigneeAddress2);
			AssertEquals("BRISBANE", AWBHeader.EH_ConsigneePlace);
			AssertEquals("NSW", AWBHeader.EH_ConsigneeState);
			AssertEquals("2266", AWBHeader.EH_ConsigneePostCode);
			AssertEquals("AU", AWBHeader.EH_ConsigneeCountryCode);
			AssertEquals("", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("+96852", AWBHeader.EH_ConsigneeContactDetail);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_ConsigneeOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ConsigneeOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ConsigneeOverride3);
			AssertEquals("BRISBANE NSW 2266 AU", AWBHeader.EH_ConsigneeOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_ConsigneeOverride5);

			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ConsigneeAddress);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_ConsigneeName);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ConsigneeAddress2);
			AssertEquals("BRISBANE", AWBHeader.EH_ConsigneePlace);
			AssertEquals("NSW", AWBHeader.EH_ConsigneeState);
			AssertEquals("2266", AWBHeader.EH_ConsigneePostCode);
			AssertEquals("AU", AWBHeader.EH_ConsigneeCountryCode);
			AssertEquals("CONTACT NAME", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("+96852", AWBHeader.EH_ConsigneeContactDetail);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_ConsigneeOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ConsigneeOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ConsigneeOverride3);
			AssertEquals("BRISBANE NSW 2266 AU", AWBHeader.EH_ConsigneeOverride4);
			AssertEquals("TE +96852 CONTACT NAME", AWBHeader.EH_ConsigneeOverride5);
		}

		public void TestPopulateConsigneeAWBAddress_FallbackMainAWBAddress()
		{
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME1";
			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "SYDNEY";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);

			Factory.Save();
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ConsigneeAddress);
			AssertEquals("Using main AWB address to populate Shipper address", "AWBCOMPANYNAME1", AWBHeader.EH_ConsigneeName);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ConsigneeAddress2);
			AssertEquals("SYDNEY", AWBHeader.EH_ConsigneePlace);
			AssertEquals("NSW", AWBHeader.EH_ConsigneeState);
			AssertEquals("2266", AWBHeader.EH_ConsigneePostCode);
			AssertEquals("AU", AWBHeader.EH_ConsigneeCountryCode);
			AssertEquals("", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("+96852", AWBHeader.EH_ConsigneeContactDetail);
			AssertEquals("AWBCOMPANYNAME1", AWBHeader.EH_ConsigneeOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_ConsigneeOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_ConsigneeOverride3);
			AssertEquals("SYDNEY NSW 2266 AU", AWBHeader.EH_ConsigneeOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_ConsigneeOverride5);
		}

		public void TestPopulateConsigneeAWBContactDetails_PopulatedBasedOnSelectedContact()
		{
			var consigneeContact = Factory.New<OrgContact>();

			consigneeContact.OC_OH = AWBHeader.Organisation.PK;
			consigneeContact.OC_ContactName = "CONTACT NAME";
			consigneeContact.OC_Phone = "+11111";
			consigneeContact.OC_Fax = "+22222";

			var consigneeAddress = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery, false);
			consigneeAddress.OA_Phone = "+33333";
			consigneeAddress.OA_Fax = "+44444";

			CombineAssertions(() =>
			{
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("Contact Phone Code is expected.", Core.Constants.AWB.ContactCodes.TELEPHONE, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("Contact Phone is expected.", "+11111", AWBHeader.EH_ConsigneeContactDetail);

				consigneeContact.OC_Phone = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("Address Phone Code is expected, since Contact's one is not availalable.",
					Core.Constants.AWB.ContactCodes.TELEPHONE, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("Address Phone is expected, since Contact's one is not availalable.",
					"+33333", AWBHeader.EH_ConsigneeContactDetail);

				consigneeAddress.OA_Phone = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("Contact Fax Code is expected.", Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("Contact Fax is expected.", "+22222", AWBHeader.EH_ConsigneeContactDetail);

				consigneeContact.OC_Fax = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("Address Fax Code is expected, since Contact's one is not availalable.",
					Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("Address Fax is expected, since Contact's one is not availalable.",
					"+44444", AWBHeader.EH_ConsigneeContactDetail);
			});
		}

		public void TestPopulateAlsoNotifyAWBAddress_FallbackAWBAddress()
		{
			var defaultAddress = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery);
			AWBHeader.Populate();
			AssertEquals(defaultAddress.PK, AWBHeader.EH_OA_ConsigneeAddress);

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_AlsoNotifyAddress);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_AlsoNotifyName);
			AssertEquals("ADDRESSAWB ADDRESSAWB2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("BRISBANE", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("NSW", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("2266", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("AU", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("CONTACT NAME", AWBHeader.EH_AlsoNotifyContactName);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96852", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_NotifyOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_NotifyOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("BRISBANE NSW AU", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96852 CONTACT NAME", AWBHeader.EH_NotifyOverride5);

			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "OFC: ADDRESSOFC";

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_AlsoNotifyAddress);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_AlsoNotifyName);
			AssertEquals("ADDRESSAWB ADDRESSAWB2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("BRISBANE", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("NSW", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("2266", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("AU", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("", AWBHeader.EH_AlsoNotifyContactName);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96852", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("AWBCOMPANYNAME", AWBHeader.EH_NotifyOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_NotifyOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("BRISBANE NSW AU", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_NotifyOverride5);
		}

		public void TestPopulateAlsoNotifyAWBAddress_FallbackMainAWBAddress()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME1";
			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "SYDNEY";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "OFC: ADDRESSOFC";

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_AlsoNotifyAddress);
			AssertEquals("Using main AWB address to populate Shipper address", "AWBCOMPANYNAME1", AWBHeader.EH_AlsoNotifyName);
			AssertEquals("ADDRESSAWB ADDRESSAWB2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("SYDNEY", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("NSW", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("2266", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("AU", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("", AWBHeader.EH_AlsoNotifyContactName);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96852", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("AWBCOMPANYNAME1", AWBHeader.EH_NotifyOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_NotifyOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("SYDNEY NSW AU", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_NotifyOverride5);
		}

		public void TestPopulateAlsoNotifyAWBAddress_FallbackToOrganisationFullNameOfAWBAddressWhenCompanyNameOverrideIsEmpty()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "SYDNEY";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.OH_FullName = "AWB ORG";
			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "PIC: ADDRESSPIC";

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_AlsoNotifyAddress);
			AssertEquals("Using organisation name when main AWB address does not have company name override", AWBHeader.Organisation.OH_FullName, AWBHeader.EH_AlsoNotifyName);
			AssertEquals("ADDRESSAWB ADDRESSAWB2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("SYDNEY", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("NSW", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("2266", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("AU", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("", AWBHeader.EH_AlsoNotifyContactName);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96852", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("AWB ORG", AWBHeader.EH_NotifyOverride1);
			AssertEquals("ADDRESSAWB", AWBHeader.EH_NotifyOverride2);
			AssertEquals("ADDRESSAWB2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("SYDNEY NSW AU", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_NotifyOverride5);
		}

		public void TestPopulateAlsoNotifyAWBContactDetails_PopulatedBasedOnSelectedContact()
		{
			var alsoNotifyContact = Factory.New<OrgContact>();

			alsoNotifyContact.OC_OH = AWBHeader.Organisation.PK;
			alsoNotifyContact.OC_ContactName = "CONTACT NAME";
			alsoNotifyContact.OC_Phone = "+11111";
			alsoNotifyContact.OC_Fax = "+22222";

			var alsoNotifyAddress = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery, false);
			alsoNotifyAddress.OA_Phone = "+33333";
			alsoNotifyAddress.OA_Fax = "+44444";

			CombineAssertions(() =>
			{
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_AlsoNotifyContactName);
				AssertEquals("Contact Phone Code is expected.", Core.Constants.AWB.ContactCodes.TELEPHONE, AWBHeader.EH_AlsoNotifyContactCode);
				AssertEquals("Contact Phone is expected.", "+11111", AWBHeader.EH_AlsoNotifyContactDetail);

				alsoNotifyContact.OC_Phone = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_AlsoNotifyContactName);
				AssertEquals("Address Phone Code is expected, since Contact's one is not availalable.",
					Core.Constants.AWB.ContactCodes.TELEPHONE, AWBHeader.EH_AlsoNotifyContactCode);
				AssertEquals("Address Phone is expected, since Contact's one is not availalable.",
					"+33333", AWBHeader.EH_AlsoNotifyContactDetail);

				alsoNotifyAddress.OA_Phone = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_AlsoNotifyContactName);
				AssertEquals("Contact Fax Code is expected.", Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_AlsoNotifyContactCode);
				AssertEquals("Contact Fax is expected.", "+22222", AWBHeader.EH_AlsoNotifyContactDetail);

				alsoNotifyContact.OC_Fax = ZString.Empty;
				AWBHeader.Populate();

				AssertEquals("CONTACT NAME", AWBHeader.EH_AlsoNotifyContactName);
				AssertEquals("Address Fax Code is expected, since Contact's one is not availalable.",
					Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_AlsoNotifyContactCode);
				AssertEquals("Address Fax is expected, since Contact's one is not availalable.",
					"+44444", AWBHeader.EH_AlsoNotifyContactDetail);
			});
		}

		public void TestPopulateShipperAddress_FallbackEnglishAddress()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Language = "ZH-CN";
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var englishAddress = orgAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = "EN";
			englishAddress.OTA_Address1 = "Address1";
			englishAddress.OTA_Address2 = "Address2";
			englishAddress.OTA_City = "City";
			englishAddress.OTA_PostCode = "PostCode";
			englishAddress.OTA_State = "State";
			englishAddress.OTA_CompanyName = "CompanyName";

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ShipperAddress);
			AssertEquals("CompanyName", AWBHeader.EH_ShipperName);
			AssertEquals("Address1", AWBHeader.EH_ShipperAddress);
			AssertEquals("Address2", AWBHeader.EH_ShipperAddress2);
			AssertEquals("City", AWBHeader.EH_ShipperPlace);
			AssertEquals("State", AWBHeader.EH_ShipperState);
			AssertEquals("PostCode", AWBHeader.EH_ShipperPostCode);
			AssertEquals("AU", AWBHeader.EH_ShipperCountryCode);
			AssertEquals("", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("+96852", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("CompanyName", AWBHeader.EH_ShipperOverride1);
			AssertEquals("Address1", AWBHeader.EH_ShipperOverride2);
			AssertEquals("Address2", AWBHeader.EH_ShipperOverride3);
			AssertEquals("City State PostCode AU", AWBHeader.EH_ShipperOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_ShipperOverride5);
		}

		public void TestPopulateConsigneeAddress_FallbackEnglishAddress()
		{
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Language = "ZH-CN";
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var englishAddress = orgAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = "EN";
			englishAddress.OTA_Address1 = "Address1";
			englishAddress.OTA_Address2 = "Address2";
			englishAddress.OTA_City = "City";
			englishAddress.OTA_PostCode = "PostCode";
			englishAddress.OTA_State = "State";
			englishAddress.OTA_CompanyName = "CompanyName";

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Populate();

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_ConsigneeAddress);
			AssertEquals("CompanyName", AWBHeader.EH_ConsigneeName);
			AssertEquals("Address1", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("Address2", AWBHeader.EH_ConsigneeAddress2);
			AssertEquals("City", AWBHeader.EH_ConsigneePlace);
			AssertEquals("State", AWBHeader.EH_ConsigneeState);
			AssertEquals("PostCode", AWBHeader.EH_ConsigneePostCode);
			AssertEquals("AU", AWBHeader.EH_ConsigneeCountryCode);
			AssertEquals("", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("+96852", AWBHeader.EH_ConsigneeContactDetail);
			AssertEquals("CompanyName", AWBHeader.EH_ConsigneeOverride1);
			AssertEquals("Address1", AWBHeader.EH_ConsigneeOverride2);
			AssertEquals("Address2", AWBHeader.EH_ConsigneeOverride3);
			AssertEquals("City State PostCode AU", AWBHeader.EH_ConsigneeOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_ConsigneeOverride5);
		}

		public void TestPopulateAlsoNotifyAddressFromJobDocAddress_FallbackEnglishAddress()
		{
			var defaultAddress = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery);
			defaultAddress.OA_Language = "ZH-CN";

			var englishAddress = defaultAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = "EN";
			englishAddress.OTA_Address1 = "Address1";
			englishAddress.OTA_Address2 = "Address2";
			englishAddress.OTA_City = "City";
			englishAddress.OTA_PostCode = "PostCode";
			englishAddress.OTA_State = "State";
			englishAddress.OTA_CompanyName = "CompanyName";

			AWBHeader.Populate();

			AssertEquals(defaultAddress.PK, AWBHeader.EH_OA_AlsoNotifyAddress);
			AssertEquals("CompanyName", AWBHeader.EH_AlsoNotifyName);
			AssertEquals("Address1 Address2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("City", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("State", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("PostCode", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("SG", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("CONTACT NAME", AWBHeader.EH_AlsoNotifyContactName);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96(1)723", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("CompanyName", AWBHeader.EH_NotifyOverride1);
			AssertEquals("Address1", AWBHeader.EH_NotifyOverride2);
			AssertEquals("Address2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("City State SG", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96(1)723 CONTACT NAME", AWBHeader.EH_NotifyOverride5);
		}

		public void TestPopulateAlsoNotifyAddressFromOrgAddress_FallbackEnglishAddress()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Language = "ZH-CN";
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var englishAddress = orgAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = "EN";
			englishAddress.OTA_Address1 = "Address1";
			englishAddress.OTA_Address2 = "Address2";
			englishAddress.OTA_City = "City";
			englishAddress.OTA_PostCode = "PostCode";
			englishAddress.OTA_State = "State";
			englishAddress.OTA_CompanyName = "CompanyName";

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "OFC: ADDRESSOFC";

			AssertEquals(orgAddress.PK, AWBHeader.EH_OA_AlsoNotifyAddress);
			AssertEquals("CompanyName", AWBHeader.EH_AlsoNotifyName);
			AssertEquals("Address1 Address2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("City", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("State", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("PostCode", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("AU", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("", AWBHeader.EH_AlsoNotifyContactName);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96852", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("CompanyName", AWBHeader.EH_NotifyOverride1);
			AssertEquals("Address1", AWBHeader.EH_NotifyOverride2);
			AssertEquals("Address2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("City State AU", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96852", AWBHeader.EH_NotifyOverride5);
		}

		public void TestGetShipperOrgHeaderForSavingAWBAddress()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var address = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			AWBHeader.Populate();

			AssertEquals(AWBHeader.EH_OA_ShipperAddress, address.PK);

			AWBHeader.EH_ShipperName = "OVERRIDENAME";
			AWBHeader.EH_ShipperAddress = "Override address1";
			AWBHeader.EH_ShipperAddress2 = "Override address2";
			AWBHeader.EH_ShipperPlace = "CNNJG";
			AWBHeader.EH_ShipperState = "JiangSu";
			AWBHeader.EH_ShipperPostCode = "223300";
			AWBHeader.EH_ShipperCountryCode = "CN";
			AWBHeader.EH_ShipperContactDetail = "+(86)86446245";

			var shipper = AWBHeader.GetShipperOrgHeaderForSavingAWBAddress(Factory);

			var addedOrgAddress = shipper.Addresses.Cast<OrgAddress>().First(x => x.OA_CompanyNameOverride == "OVERRIDENAME");
			AssertEquals("Override address1", addedOrgAddress.OA_Address1);
			AssertEquals("Override address2", addedOrgAddress.OA_Address2);
			AssertEquals("CNNJG", addedOrgAddress.OA_City);
			AssertEquals("JiangSu", addedOrgAddress.OA_State);
			AssertEquals("223300", addedOrgAddress.OA_PostCode);
			AssertEquals(address.OA_RL_NKRelatedPortCode, addedOrgAddress.OA_RL_NKRelatedPortCode);
			AssertEquals("+(86)86446245", addedOrgAddress.OA_Phone);
		}

		public void TestGetShipperOrgHeaderForSavingAWBAddress_DoesNotThrowOnAWBHeaderRemoval()
		{
			// Arrange
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var address = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			AWBHeader.Populate();

			AssertEquals(AWBHeader.EH_OA_ShipperAddress, address.PK);

			AWBHeader.EH_ShipperName = "OVERRIDENAME";
			AWBHeader.EH_ShipperAddress = "Override address1";
			AWBHeader.EH_ShipperAddress2 = "Override address2";
			AWBHeader.EH_ShipperPlace = "CNNJG";
			AWBHeader.EH_ShipperState = "JiangSu";
			AWBHeader.EH_ShipperPostCode = "223300";
			AWBHeader.EH_ShipperCountryCode = "CN";
			AWBHeader.EH_ShipperContactDetail = "+(86)86446245";

			AWBHeader.Delete();
			var expectedStackTrace = new System.Diagnostics.StackTrace(1, true).ToString().TrimEnd(System.Environment.NewLine.ToCharArray());

			// Act
			_ = AWBHeader.GetShipperOrgHeaderForSavingAWBAddress(Factory);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertEquals("AccessingRemovedAWBHeader", ErrorReporter.LastKeyReported);
				AssertContains(expectedStackTrace, ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		public void TestGetConsigneeOrgHeaderForSavingAWBAddress()
		{
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			var address = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			AWBHeader.Populate();

			AssertEquals(AWBHeader.EH_OA_ConsigneeAddress, address.PK);

			AWBHeader.EH_ConsigneeName = "OVERRIDENAME";
			AWBHeader.EH_ConsigneeAddress = "Override address1";
			AWBHeader.EH_ConsigneeAddress2 = "Override address2";
			AWBHeader.EH_ConsigneePlace = "CNNJG";
			AWBHeader.EH_ConsigneeState = "JiangSu";
			AWBHeader.EH_ConsigneePostCode = "223300";
			AWBHeader.EH_ConsigneeCountryCode = "CN";
			AWBHeader.EH_ConsigneeContactDetail = "+(86)86446245";

			var consignee = AWBHeader.GetConsigneeOrgHeaderForSavingAWBAddress(Factory);

			var addedOrgAddress = consignee.Addresses.Cast<OrgAddress>().First(x => x.OA_CompanyNameOverride == "OVERRIDENAME");
			AssertEquals("Override address1", addedOrgAddress.OA_Address1);
			AssertEquals("Override address2", addedOrgAddress.OA_Address2);
			AssertEquals("CNNJG", addedOrgAddress.OA_City);
			AssertEquals("JiangSu", addedOrgAddress.OA_State);
			AssertEquals("223300", addedOrgAddress.OA_PostCode);
			AssertEquals(address.OA_RL_NKRelatedPortCode, addedOrgAddress.OA_RL_NKRelatedPortCode);
			AssertEquals("+(86)86446245", addedOrgAddress.OA_Phone);
		}

		public void TestGetConsigneeOrgHeaderForSavingAWBAddress_DoesNotThrowOnAWBHeaderRemoval()
		{
			// Arrange
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			var address = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			AWBHeader.Populate();

			AssertEquals(AWBHeader.EH_OA_ConsigneeAddress, address.PK);

			AWBHeader.EH_ConsigneeName = "OVERRIDENAME";
			AWBHeader.EH_ConsigneeAddress = "Override address1";
			AWBHeader.EH_ConsigneeAddress2 = "Override address2";
			AWBHeader.EH_ConsigneePlace = "CNNJG";
			AWBHeader.EH_ConsigneeState = "JiangSu";
			AWBHeader.EH_ConsigneePostCode = "223300";
			AWBHeader.EH_ConsigneeCountryCode = "CN";
			AWBHeader.EH_ConsigneeContactDetail = "+(86)86446245";

			AWBHeader.Delete();
			var expectedStackTrace = new System.Diagnostics.StackTrace(1, true).ToString().TrimEnd(System.Environment.NewLine.ToCharArray());

			// Act
			_ = AWBHeader.GetConsigneeOrgHeaderForSavingAWBAddress(Factory);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertEquals("AccessingRemovedAWBHeader", ErrorReporter.LastKeyReported);
				AssertContains(expectedStackTrace, ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		public void TestGetAlsoNotifyOrgHeaderForSavingAWBAddress()
		{
			var address = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery);
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			AWBHeader.Populate();

			AssertEquals(AWBHeader.EH_OA_ConsigneeAddress, address.PK);

			AWBHeader.EH_AlsoNotifyName = "OVERRIDENAME";
			AWBHeader.EH_AlsoNotifyAddress = "Override address1";
			AWBHeader.EH_AlsoNotifyAddress2 = "Override address2";
			AWBHeader.EH_AlsoNotifyPlace = "CNNJG";
			AWBHeader.EH_AlsoNotifyState = "JiangSu";
			AWBHeader.EH_AlsoNotifyPostCode = "223300";
			AWBHeader.EH_AlsoNotifyCountryCode = "CN";
			AWBHeader.EH_AlsoNotifyContactDetail = "+(86)86446245";

			var alsoNotify = AWBHeader.GetAlsoNotifyOrgHeaderForSavingAWBAddress(Factory);

			var addedOrgAddress = alsoNotify.Addresses.Cast<OrgAddress>().First(x => x.OA_CompanyNameOverride == "OVERRIDENAME");
			AssertEquals("Override address1", addedOrgAddress.OA_Address1);
			AssertEquals("Override address2", addedOrgAddress.OA_Address2);
			AssertEquals("CNNJG", addedOrgAddress.OA_City);
			AssertEquals("JiangSu", addedOrgAddress.OA_State);
			AssertEquals("223300", addedOrgAddress.OA_PostCode);
			AssertEquals(address.OA_RL_NKRelatedPortCode, addedOrgAddress.OA_RL_NKRelatedPortCode);
			AssertEquals("+(86)86446245", addedOrgAddress.OA_Phone);
		}

		public void TestGetAlsoNotifyOrgHeaderForSavingAWBAddress_DoesNotThrowOnAWBHeaderRemoval()
		{
			// Arrange
			var address = AWBHeader.Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery);
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			AWBHeader.Populate();

			AssertEquals(AWBHeader.EH_OA_ConsigneeAddress, address.PK);

			AWBHeader.EH_AlsoNotifyName = "OVERRIDENAME";
			AWBHeader.EH_AlsoNotifyAddress = "Override address1";
			AWBHeader.EH_AlsoNotifyAddress2 = "Override address2";
			AWBHeader.EH_AlsoNotifyPlace = "CNNJG";
			AWBHeader.EH_AlsoNotifyState = "JiangSu";
			AWBHeader.EH_AlsoNotifyPostCode = "223300";
			AWBHeader.EH_AlsoNotifyCountryCode = "CN";
			AWBHeader.EH_AlsoNotifyContactDetail = "+(86)86446245";

			_ = AWBHeader.GetAlsoNotifyOrgHeaderForSavingAWBAddress(Factory);

			AWBHeader.Delete();
			var expectedStackTrace = new System.Diagnostics.StackTrace(1, true).ToString().TrimEnd(System.Environment.NewLine.ToCharArray());

			// Act
			_ = AWBHeader.GetConsigneeOrgHeaderForSavingAWBAddress(Factory);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertEquals("AccessingRemovedAWBHeader", ErrorReporter.LastKeyReported);
				AssertContains(expectedStackTrace, ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		public void TestGetCanOverrideCheckpoint()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			SecurityCheckpoint checkpoint = ((IDocAddresses)header).GetCanOverrideCheckpoint(JobDocAddress.New(header));

			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestEH_AirlineShortName()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ExportAWBHeader aWBHeader = shipment.AWBHeader;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";

			shipment.Consols.Add(consol);

			RefAirline airline = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			airline.RM_LabelShortName = "";
			AssertEquals("AirlineShortName should be blank", "", aWBHeader.EH_AirlineShortName);

			airline.RM_LabelShortName = "Airline Short Name";
			AssertEquals("AirlineShortName should be complete", "Airline Short Name", aWBHeader.EH_AirlineShortName);
		}

		public void TestEH_AirlineDefaultIdentifierForCneNfyNameAndPhone()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ExportAWBHeader aWBHeader = shipment.AWBHeader;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";

			shipment.Consols.Add(consol);

			RefAirline airline = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			airline.RM_ContactNameOCIIdentifier = "";
			airline.RM_ContactPhoneOCIIdentifier = "";
			AssertEquals("AirlineDefaultIdentifierForCneNfyName should be blank", "", aWBHeader.EH_AirlineDefaultIdentifierForCneNfyName);
			AssertEquals("AirlineDefaultIdentifierForCneNfyPhone should be blank", "", aWBHeader.EH_AirlineDefaultIdentifierForCneNfyPhone);

			airline.RM_ContactNameOCIIdentifier = "AB";
			airline.RM_ContactPhoneOCIIdentifier = "CD";
			AssertEquals("AirlineDefaultIdentifierForCneNfyName should be complete", "AB", aWBHeader.EH_AirlineDefaultIdentifierForCneNfyName);
			AssertEquals("AirlineDefaultIdentifierForCneNfyPhone should be complete", "CD", aWBHeader.EH_AirlineDefaultIdentifierForCneNfyPhone);
		}

		public void TestEH_AWBIssuePlace()
		{
			var newFactory = new BusinessObjectFactory();
			var uslaxBranch = newFactory.New<GlbBranch>();

			uslaxBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			uslaxBranch.GB_Code = "LAX";
			uslaxBranch.GB_RL_NKHomePort = "USLAX";
			uslaxBranch.GB_City = "Las Angeles";

			newFactory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, uslaxBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AWBHeader.Populate();
				AssertEquals("Las Angeles", AWBHeader.EH_AWBIssuePlace);

				uslaxBranch.GB_City = string.Empty;
				newFactory.Save();

				AWBHeader.Populate();
				AssertEquals(string.Empty, AWBHeader.EH_AWBIssuePlace);
			}
		}

		public void TestSetIssuedByAddress()
		{
			var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("NAME", "ADDRESS1", "ADDRESS2", "CITY", "STATE", "COUNTRYCODE", "PCODE"));
			AWBHeader.SetIssuedByAddress(issuedBy);

			AssertEquals("NAME", AWBHeader.EH_IssuingAgentName);
			AssertEquals("ADDRESS1", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("ADDRESS2, CITY, STATE, PCODE, COUNTRYCODE", AWBHeader.EH_IssuingAgentAddress2);

			issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("NAME", "ADDRESS1", "ADDRESS2", "", "STATE", "COUNTRYCODE", "PCODE"));
			AWBHeader.SetIssuedByAddress(issuedBy);
			AssertEquals("NAME", AWBHeader.EH_IssuingAgentName);
			AssertEquals("ADDRESS1", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("ADDRESS2, STATE, PCODE, COUNTRYCODE", AWBHeader.EH_IssuingAgentAddress2);

			issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("EAGLE DATAMATION INTERNATIONAL", "LEVEL 2, 184 BOURKE RD", "", "STATE", "ALEXANDRIA", "AU", "2015"));
			AWBHeader.SetIssuedByAddress(issuedBy);
			AssertEquals("EAGLE DATAMATION INTERNATIONAL", AWBHeader.EH_IssuingAgentName);
			AssertEquals("LEVEL 2, 184 BOURKE RD", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("STATE, ALEXANDRIA, 2015, AU", AWBHeader.EH_IssuingAgentAddress2);

			issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("NAME", "SOME LONG ADDRESS1 THAT TAKES UP SOME SP", "SOME LONG ADDRESS2 THAT TAKES UP SOME SP", "A LONG CITY", "STATE", "COUNTRYCODE", "PCODE"));
			AWBHeader.SetIssuedByAddress(issuedBy);
			AssertEquals("NAME", AWBHeader.EH_IssuingAgentName);
			AssertEquals("SOME LONG ADDRESS1 THAT TAKES UP SOME SP", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("SOME LONG ADDRESS2 THAT TAKES UP SOME SP, A LONG C", AWBHeader.EH_IssuingAgentAddress2);

			issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("NAME", "1234567890123456789012345678901234567890", "CRAP", "CITY", "STATE", "COUNTRYCODE", "PCODE"));
			AWBHeader.SetIssuedByAddress(issuedBy);

			AssertEquals("NAME", AWBHeader.EH_IssuingAgentName);
			AssertEquals("1234567890123456789012345678901234567890", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("CRAP, CITY, STATE, PCODE, COUNTRYCODE", AWBHeader.EH_IssuingAgentAddress2);
		}

		public void TestSetIssuedByAddress_TransformJPState()
		{
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(
				new ZQuery(RefCountryStatesSchema.RW_Code, "13"), JoinCondition.And,
				new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Constants.CountryCodes.Japan)));

			var translateJapanese = Factory.New<RefLanguageText>();
			translateJapanese.RLT_ColumnName = "RW_Description";
			translateJapanese.RLT_Language = SharedConstants.Languages.Japanese;
			translateJapanese.RLT_ParentId = state.PK;
			translateJapanese.RLT_ParentTableCode = "RW";
			translateJapanese.RLT_Text = "東京都";

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				var issuedBy = new BillIssuedByAirline(GetBillIssuedByAirLine("NAME", "ADDRESS1", "ADDRESS2", "CITY", "13", "Japan", "100-0012"));
				AWBHeader.SetIssuedByAddress(issuedBy);

				AssertEquals("ADDRESS2, CITY, TOKYO, 100-0012, JAPAN", AWBHeader.EH_IssuingAgentAddress2);
			}
		}

		RefAirline GetBillIssuedByAirLine(ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString country, ZString postCode)
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "NHK";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_RM_Airline = airline.PK;

			airline.RM_AirlineName1 = name;
			airline.RM_AddressLine1 = address1;
			airline.RM_AddressLine2 = address2;
			airline.RM_AirlineCity = city;
			airline.RM_AirlineState = state;
			airline.RM_AirlinePostalCode = postCode;
			airline.RM_AirlineCountry = country;

			return airline;
		}

		public void TestBookingFlightDates()
		{
			AssertEquals("Pre-condition: OverrideWayBillDefaults has to be false", false, AWBHeader.TestOverrideWayBillDefaults);
			AssertEquals(new ZDateTime(2005, 1, 1), AWBHeader.Booking1stFlightDate);
			AssertEquals(new ZDateTime(2005, 2, 2), AWBHeader.Booking2ndFlightDate);

			AWBHeader.TestOverrideWayBillDefaults = true;
			AWBHeader.EH_AWBIssueDate = new ZDateTime(2004, 02, 2);
			AWBHeader.EH_Booking1stFlightDate = "3";
			AssertEquals(new ZDateTime(2004, 2, 3), AWBHeader.Booking1stFlightDate);
			AWBHeader.EH_Booking2ndFlightDate = "1";
			AssertEquals(new ZDateTime(2004, 3, 1), AWBHeader.Booking2ndFlightDate);
			AWBHeader.EH_Booking1stFlightDate = "30";
			AssertEquals(new ZDateTime(2004, 2, 29), AWBHeader.Booking1stFlightDate);

			AWBHeader.EH_AWBIssueDate = new ZDateTime(2005, 1, 31);
			AWBHeader.EH_Booking2ndFlightDate = "30";
			AssertEquals(new ZDateTime(2005, 2, 28), AWBHeader.Booking2ndFlightDate);

			AWBHeader.EH_AWBIssueDate = new ZDateTime(2004, 12, 31);
			AWBHeader.EH_Booking1stFlightDate = "5";
			AssertEquals(new ZDateTime(2005, 1, 5), AWBHeader.Booking1stFlightDate);
			AWBHeader.EH_Booking2ndFlightDate = "31";
			AssertEquals(new ZDateTime(2004, 12, 31), AWBHeader.Booking2ndFlightDate);
		}

		[ExpectNoExceptions]
		public void TestBookingFlightDates_CorrectParsing()
		{
			AWBHeader.TestOverrideWayBillDefaults = true;
			AWBHeader.EH_Booking1stFlightDate = "0";
			AssertEquals(ZDateTime.Empty, AWBHeader.Booking1stFlightDate);
			AssertHasWarning(AWBHeader.EH_Booking1stFlightDateInfo, "Date value should be between 1 and 31.");

			AWBHeader.EH_Booking2ndFlightDate = "0";
			AssertEquals(ZDateTime.Empty, AWBHeader.Booking2ndFlightDate);
			AssertHasWarning(AWBHeader.EH_Booking2ndFlightDateInfo, "Date value should be between 1 and 31.");
		}

		public void TestHouseCurrencies()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ExportAWBHeader aWBHeader = shipment.AWBHeader;
			shipment.JS_RX_NKGoodsValueCurr = "NZD";
			shipment.JS_RX_NKInsuranceCurrency = "INR";

			aWBHeader.Populate();
			AssertEquals("NZD", aWBHeader.EH_HouseCustomsValueCurrency);
			AssertEquals("INR", aWBHeader.EH_HouseInsuranceValueCurrency);
		}

		public void TestEH_BookingFlightPaddedWhenLengthIsLessThanExpected()
		{
			AWBHeader.EH_Booking1stFlight = "290";
			AWBHeader.EH_Booking2ndFlight = "292";
			AssertEquals("290", AWBHeader.EH_Booking1stFlight);
			AssertEquals("292", AWBHeader.EH_Booking2ndFlight);

			AWBHeader.EH_Booking1stFlight = "2";
			AWBHeader.EH_Booking2ndFlight = "3";
			AssertEquals("002", AWBHeader.EH_Booking1stFlight);
			AssertEquals("003", AWBHeader.EH_Booking2ndFlight);

			AWBHeader.EH_Booking1stFlight = "39";
			AWBHeader.EH_Booking2ndFlight = "42";
			AssertEquals("039", AWBHeader.EH_Booking1stFlight);
			AssertEquals("042", AWBHeader.EH_Booking2ndFlight);

			AWBHeader.EH_Booking1stFlight = "2900B";
			AWBHeader.EH_Booking2ndFlight = "1214C";
			AssertEquals("2900B", AWBHeader.EH_Booking1stFlight);
			AssertEquals("1214C", AWBHeader.EH_Booking2ndFlight);

			AWBHeader.EH_Booking1stFlight = "";
			AWBHeader.EH_Booking2ndFlight = "";
			AssertEquals("Empty string should not be padded", "", AWBHeader.EH_Booking1stFlight);
			AssertEquals("Empty string should not be padded", "", AWBHeader.EH_Booking2ndFlight);
		}

		public void TestEH_BookingFlightDatePaddedWhenLengthIsLessThanExpected()
		{
			AWBHeader.EH_Booking1stFlightDate = "1";
			AWBHeader.EH_Booking2ndFlightDate = "2";
			AssertEquals("01", AWBHeader.EH_Booking1stFlightDate);
			AssertEquals("02", AWBHeader.EH_Booking2ndFlightDate);

			AWBHeader.EH_Booking1stFlightDate = "11";
			AWBHeader.EH_Booking2ndFlightDate = "12";
			AssertEquals("11", AWBHeader.EH_Booking1stFlightDate);
			AssertEquals("12", AWBHeader.EH_Booking2ndFlightDate);

			AWBHeader.EH_Booking1stFlightDate = "";
			AWBHeader.EH_Booking2ndFlightDate = "";
			AssertEquals("Empty string should not be padded", "", AWBHeader.EH_Booking1stFlightDate);
			AssertEquals("Empty string should not be padded", "", AWBHeader.EH_Booking2ndFlightDate);
		}

		#region Nature and Qty of Goods

		public void TestIndexOfFirstEmptyNatureAndQtyOfGoods()
		{
			AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(3, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(4, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(5, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(6, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(7, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(8, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(9, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(10, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(11, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(12, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(0, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);
		}

		public void TestIndexOfFirstEmptyNatureAndQtyOfGoods_WithMultiLineLithiumStatements()
		{
			AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(2, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI969;

			var numberOfLinesInLithiumPackType = AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsLithiumBattery.WrappedDescriptions.Count;

			AssertGreaterThan("PRE: Lithium Battery Pack Type 'PI969' is a multiline statement", numberOfLinesInLithiumPackType, 1);

			AssertEquals("LineNumberOfFirstEmptyNatureAndQtyOfGoods takes into account multiline lithium battery statements",
				2 + numberOfLinesInLithiumPackType, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);
		}

		public void TestPopulateRatelines_ClearInfomationBeforeRepopulate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ExportAWBHeader aWBHeader = shipment.AWBHeader;
			shipment.JS_RX_NKGoodsValueCurr = "NZD";
			shipment.JS_RX_NKInsuranceCurrency = "INR";

			aWBHeader.Populate();
			var rateLine = aWBHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1];
			rateLine.ER_CommodityItemNumber = "9999";
			aWBHeader.Populate();
			Assert(rateLine.ER_CommodityItemNumber != "9999");
		}

		public void TestAWBRateLinesType()
		{
			AssertEquals(typeof(ExportAWBRateLineCollection), AWBHeader.AWBRateLines.GetType());
		}

		public void TestLineNumberOfFirstEmptyRateLine()
		{
			AssertEquals(1, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[0].NatureAndQtyOfGoods.Text = "nature and qty of goods!";
			AssertEquals("NatureAndQtyOfGoods should not affect LineNumberOfFirstEmptyRateLine", 1, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[0].ER_CommodityItemNumber = "10";
			AssertEquals(2, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[1].ER_CommodityItemNumber = "10";
			AssertEquals(3, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[2].ER_CommodityItemNumber = "10";
			AssertEquals(4, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[3].ER_CommodityItemNumber = "10";
			AssertEquals(5, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[4].ER_CommodityItemNumber = "10";
			AssertEquals(6, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[5].ER_CommodityItemNumber = "10";
			AssertEquals(7, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[6].ER_CommodityItemNumber = "10";
			AssertEquals(8, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[7].ER_CommodityItemNumber = "10";
			AssertEquals(9, AWBHeader.LineNumberOfFirstEmptyRateLine);

			AWBHeader.AWBRateLines[8].ER_CommodityItemNumber = "10";
			AssertEquals(10, AWBHeader.LineNumberOfFirstEmptyRateLine);
		}

		public void TestExportStatementIsAddedToNatureAndQtyOfGoods()
		{
			var aWBHeaderMock = Factory.NewMoq<MockExportAWBHeader>();
			ZString[] exportStatements = new ZString[] { new ZString("EXPORT STATEMENT") };
			aWBHeaderMock.Protected().Setup<ZString[]>("ExportStatements").Returns(exportStatements);
			MockExportAWBHeader aWBHeader = aWBHeaderMock.Object;
			aWBHeader.Populate();
			AssertContains("NatureAndQtyOfGoods", "EXPORT STATEMENT", aWBHeader.NatureAndQtyOfGoods);
			aWBHeaderMock.VerifyAll();

			aWBHeaderMock.Reset();
			aWBHeader = aWBHeaderMock.Object;
			aWBHeader.Populate();
			AssertNotContains("NatureAndQtyOfGoods", "EXPORT STATEMENT", aWBHeader.NatureAndQtyOfGoods);
		}

		public void TestPopulateNatureAndQtyOfGoods()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ExportAWBHeader awbHeader = shipment.AWBHeader;
			shipment.JS_RL_NKOrigin = "AUSYD";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols.Add(consol);

			awbHeader.Populate();

			CountryExportStatementSettingCollection defaultValue = new CountryExportStatementSettingCollection();

			CountryExportStatementSetting sedSetting = defaultValue.AddNew();
			sedSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sedSetting.Statements.Add(new ExportStatementSetting(sedSetting, "GBH", "A user defined statement", "", "", "", "UDF", true, true, true, true, true, true));
			sedSetting.Statements.Add(new ExportStatementSetting(sedSetting, "MAT", "Mandatory statement", "", "", "", "MAN", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			Factory.Save();

			awbHeader.Populate();

			AssertContains("Mandatory statement", awbHeader.NatureAndQtyOfGoods);
		}

		public void TestPopulateItalyCodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

				ExportAWBHeader awbHeader = shipment.AWBHeader;
				OrgHeader consignor = Factory.New<OrgHeader>();
				shipment.ConsignorPK = consignor.PK;

				var consignorSIV = consignor.CustomsCodes.AddNew();
				consignorSIV.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				consignorSIV.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
				consignorSIV.OK_CustomsRegNo = "11223344551";

				awbHeader.Populate();
				AssertEquals(false, awbHeader.HasChanges);

				OrgHeader branchProxy = GlbCompany.CurrentCompany.OrgProxy;

				var ivaCC = branchProxy.CustomsCodes.AddNew();
				ivaCC.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				ivaCC.OK_CodeType = OrgCusCode.CodeTypes.IVA;
				ivaCC.OK_CustomsRegNo = "10987654321";

				awbHeader.Populate();
				AssertEquals(false, awbHeader.HasChanges);
			}
		}

		public void TestJobRevenueJournal_AreDiscarded()
		{
			var anotherBranch = GlbCompany.CurrentCompany.Branches.AddNew();
			var anotherBranchOrgProxy = anotherBranch.Factory.NewWithValidTestData<OrgHeader>();
			anotherBranch.GB_OH_OrgProxy = anotherBranchOrgProxy.PK;
			anotherBranch.Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes))
			{
				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = ZBool.True;
				consignor.OH_Code = "ORG_SHP";
				consignor.OH_FullName = "consignor company full name";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				shipment.ConsignorPK = consignor.PK;

				var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.LocalChargesPK = shipment.ConsignorPK;
				var awbHeader = shipment.AWBHeader;

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;

				var charge = job.Charges.AddNew();
				charge.JR_GC = GlbCompany.CurrentCompany.PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				charge.JR_AC = chargeCode.PK;
				charge.JR_JH = shipment.Job.PK;
				charge.JR_LocalSellAmt = 200;
				charge.JR_OSSellAmt = 200;
				charge.JR_OH_SellAccount = job.LocalChargesPK;
				Factory.Save();

				CombineAssertions("AWB shows 1 charge of 200 when JRJ hasn't been created.", () =>
				{
					AssertEquals("Pre-condition", 1, job.Charges.Count);
					awbHeader.Populate();
					AssertEquals(1, awbHeader.AWBOtherCharges.Count);
					AssertEquals(200m, awbHeader.AWBOtherCharges[0].EO_Amount);
				});

				charge.JR_LocalCostAmt = 150;
				charge.JR_OH_CostAccount = anotherBranchOrgProxy.PK;
				Factory.Save();

				CombineAssertions("AWB still shows 1 charge of 200 after JRJ has been created.", () =>
				{
					AssertEquals("Pre-condition", 2, job.Charges.Count);
					awbHeader.Populate();
					AssertEquals(1, awbHeader.AWBOtherCharges.Count);
					AssertEquals(200m, awbHeader.AWBOtherCharges[0].EO_Amount);
				});
			}
		}

		public void TestMergeWithExistingOtherCharges_HasChanges()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			JobHeader jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ExportAWBHeader awbHeader = shipment.AWBHeader;

			JobCharge charge1 = Factory.New<JobCharge>();
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			chargeCode1.AC_Desc = "Charge Code 1";
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_AC = chargeCode1.PK;
			charge1.JR_LocalCostAmt = 100;
			charge1.JR_LocalSellAmt = 50;
			charge1.JR_JH = shipment.Job.PK;

			awbHeader.Populate();
			AssertEquals(false, awbHeader.AWBOtherCharges.HasChanges);

			JobCharge charge2 = Factory.New<JobCharge>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AS;
			chargeCode2.AC_Desc = "Charge Code 2";
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_AC = chargeCode2.PK;
			charge2.JR_LocalCostAmt = 100;
			charge2.JR_LocalSellAmt = 50;
			charge2.JR_JH = shipment.Job.PK;

			awbHeader.Populate();
			AssertEquals(false, awbHeader.AWBOtherCharges.HasChanges);
		}

		public void TestMergeWithExistingOtherCharges_DeleteHasChanges()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_Code = "ORG_SHP";
			consignor.OH_FullName = "consignor company full name";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsignorPK = consignor.PK;

			var awbHeader = shipment.AWBHeader;

			var jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = shipment.ConsignorPK;

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			chargeCode1.AC_Desc = "Charge Code 1";
			chargeCode1.AC_Code = "DTD";
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_AC = chargeCode1.PK;
			charge1.JR_LocalCostAmt = 100;
			charge1.JR_OSCostAmt = 100;
			charge1.JR_LocalSellAmt = 50;
			charge1.JR_OSSellAmt = 50;
			charge1.JR_OH_SellAccount = jobHeader.LocalChargesPK;
			charge1.JR_JH = shipment.Job.PK;

			awbHeader.Populate();

			shipment.IsAWBValuesOverriddenProperty = true;

			Factory.Save();

			AssertEquals("prerequsite", true, awbHeader.AWBOtherCharges[0].IsInDatabase);

			charge1.Delete();
			shipment.IsAWBValuesOverriddenProperty = false;

			AssertEquals(false, awbHeader.AWBOtherCharges.HasChanges);

			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AS;
			chargeCode2.AC_Desc = "Charge Code 2";
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_AC = chargeCode2.PK;
			charge2.JR_LocalCostAmt = 120;
			charge2.JR_OSCostAmt = 120;
			charge2.JR_LocalSellAmt = 60;
			charge2.JR_OSSellAmt = 60;
			charge2.JR_OH_SellAccount = jobHeader.LocalChargesPK;
			charge2.JR_JH = shipment.Job.PK;

			awbHeader.Populate();

			AssertEquals("prerequsite", false, awbHeader.AWBOtherCharges[0].IsInDatabase);

			charge2.Delete();

			AssertEquals(false, awbHeader.AWBOtherCharges.HasChanges);
		}

		public void TestExportStatementsAreNotBeingCutOff()
		{
			CountryExportStatementSettingCollection defaultValue = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting sEDSetting = defaultValue.AddNew();
			sEDSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "GBH", "UDF statement", "", "", "", "UDF", true, true, true, true, true, true));
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "MAT", "Mandatory statement", "", "", "", "MAN", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.DocsAndCartage.JP_ExportStatement = "GBH";
			shipment.JS_GoodsDescription = "GOODS TEST";
			shipment.JS_RL_NKOrigin = "AUSYD";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols.Add(consol);
			ExportAWBHeader header = shipment.AWBHeader;

			AddPackLineDimension(shipment.OuterPackLines);
			AddPackLineDimension(shipment.OuterPackLines);
			AddPackLineDimension(shipment.OuterPackLines);
			AddPackLineDimension(shipment.OuterPackLines);
			AddPackLineDimension(shipment.OuterPackLines);
			AddPackLineDimension(shipment.OuterPackLines);
			AddPackLineDimension(shipment.OuterPackLines);
			AddPackLineDimension(shipment.OuterPackLines);
			shipment.JS_OuterPacks = 8;
			shipment.JS_TotalPackageCount = 24;

			Factory.Save();

			header.Populate();

			AssertEquals("GOODS TEST", header.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("Mandatory statement", header.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("UDF statement", header.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 3x3x3 CM x 1", header.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 4x4x4 CM x 1", header.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 5x5x5 CM x 1", header.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x6x6 CM x 1", header.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 7x7x7 CM x 1", header.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 8x8x8 CM x 1", header.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 9x9x9 CM x 1", header.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 10x10x10 CM x 1", header.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("24 SLAC", header.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestHarmonisedCodeOverrideDescription()
		{
			CountryExportStatementSettingCollection defaultValue = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting sEDSetting = defaultValue.AddNew();
			sEDSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "GBH", "UDF statement", "", "", "", "UDF", true, true, true, true, true, true));
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "MAT", "Mandatory statement", "", "", "", "MAN", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.DocsAndCartage.JP_ExportStatement = "GBH";
			shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			shipment.JS_GoodsDescription = "A\nB\nC\nD\nE\nF\nG\nH\nI\n";
			shipment.JS_RL_NKOrigin = "AUSYD";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "C0000001";
			container.JC_ContainerMode = "ULD";
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;

			shipment.Consols.Add(consol);
			ExportAWBHeader header = shipment.AWBHeader;

			shipment.JS_OuterPacks = 8;
			shipment.JS_TotalPackageCount = 27;
			foreach (ForwardingPackLine line in shipment.OuterPackLines)
			{
				line.JL_JC = container.PK;
				line.JL_HarmonisedCode = line.JL_Length.ToString();
			}
			shipment.UpdateShipmentFromOuterPackLines();
			Factory.Save();

			header.Populate();

			AssertEquals("A", header.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("B", header.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("C", header.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("D", header.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("E", header.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("F", header.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("G", header.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("H", header.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("I", header.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("Mandatory statement", header.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("UDF statement", header.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("27 SLAC", header.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		internal static void AddPackLineDimension(ForwardingPackLineCollection packLines)
		{
			var packLine = packLines.AddNew();
			packLine.JL_UnitOfDimension = "CM";
			packLine.JL_PackageCount = 1;
			var count = packLines.Count + 1;
			packLine.JL_Length = count;
			packLine.JL_Width = count;
			packLine.JL_Height = count;
		}

		#endregion

		public void TestWillFitInFreeSpace()
		{
			AssertEquals(false, AWBHeader.WillFitInFreeSpace(13));
			AssertEquals(true, AWBHeader.WillFitInFreeSpace(1));
			AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "Test";
			AssertEquals(true, AWBHeader.WillFitInFreeSpace(1));
			AssertEquals(false, AWBHeader.WillFitInFreeSpace(8));
		}

		public void TestGetDimensionText()
		{
			PackLine packLine = Factory.New<PackLine>();
			packLine.JL_Length = 5.234m;
			packLine.JL_Width = 6.234m;
			packLine.JL_Height = 7.234m;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			packLine.JL_PackageCount = 10;
			AssertEquals("DIMS 523x623x723 CM x 10", AWBHeader.GetDimensionText(packLine));

			packLine = Factory.New<PackLine>();
			packLine.JL_Length = 5m;
			packLine.JL_Width = 6m;
			packLine.JL_Height = 7m;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Centimetres;
			packLine.JL_PackageCount = 10;
			AssertEquals("DIMS 5x6x7 CM x 10", AWBHeader.GetDimensionText(packLine));

			packLine = Factory.New<PackLine>();
			packLine.JL_Length = 5m;
			packLine.JL_Width = 6m;
			packLine.JL_Height = 7m;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Feet;
			packLine.JL_PackageCount = 10;
			AssertEquals("DIMS 60x72x84 IN x 10", AWBHeader.GetDimensionText(packLine));

			packLine = Factory.New<PackLine>();
			packLine.JL_Length = 5m;
			packLine.JL_Width = 6m;
			packLine.JL_Height = 7m;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Yards;
			packLine.JL_PackageCount = 10;
			AssertEquals("DIMS 180x216x252 IN x 10", AWBHeader.GetDimensionText(packLine));

			packLine = Factory.New<PackLine>();
			packLine.JL_Length = 5m;
			packLine.JL_Width = 6m;
			packLine.JL_Height = 7m;
			packLine.JL_UnitOfDimension = "la";
			packLine.JL_PackageCount = 10;
			AssertEquals("", AWBHeader.GetDimensionText(packLine));
		}

		public void TestIsPrintingFinalNeutralMAWB()
		{
			AssertEquals("Printing Final MAWB false by default", ZBool.False, AWBHeader.IsPrintingFinalNeutralMAWB);
		}

		public void TestIsPrintingDraftNeutralMAWB()
		{
			AssertEquals("Printing Neutral MAWB false by default", ZBool.False, AWBHeader.IsPrintingDraftNeutralMAWB);
		}

		public void TestIsReprintingNeutralMAWB()
		{
			AssertEquals("RePrinting Neutral MAWB false by default", ZBool.False, AWBHeader.IsReprintingNeutralMAWB);
		}

		public void TestIsAWBOverridden()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeader = shipment.AWBHeader;

			shipment.IsAWBValuesOverriddenProperty = true;
			Assert(awbHeader.IsAWBOverridden);

			shipment.IsAWBValuesOverriddenProperty = false;
			Assert(!awbHeader.IsAWBOverridden);
		}

		public void TestIsAWBAndCSDOverridden()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeader = consol.AWBHeader;

			consol.IsAWBValuesOverriddenProperty = false;
			consol.IsCSDValuesOverriddenProperty = false;
			Assert("AWB fields should be read-only", awbHeader.EH_AWBIssueDateInfo.ReadOnly);
			Assert("Security Declaration fields should be read-only", awbHeader.EH_SecurityStatusIssueDateInfo.ReadOnly);

			consol.IsCSDValuesOverriddenProperty = true;
			Assert("AWB fields should be read-only", awbHeader.EH_AWBIssueDateInfo.ReadOnly);
			Assert("Security Declaration fields should NOT be read-only", !awbHeader.EH_SecurityStatusIssueDateInfo.ReadOnly);

			consol.IsAWBValuesOverriddenProperty = true;
			consol.IsCSDValuesOverriddenProperty = false;
			Assert("AWB fields should be NOT be read-only", !awbHeader.EH_AWBIssueDateInfo.ReadOnly);
			Assert("Security Declaration fields should be read-only", awbHeader.EH_SecurityStatusIssueDateInfo.ReadOnly);
		}

		public void TestEH_AreRateLinesOverridden_ReadOnly()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeader = shipment.AWBHeader;

			shipment.IsAWBValuesOverriddenProperty = true;
			Assert(awbHeader.IsAWBOverridden);
			Assert("EH_AreRateLinesOverridden should be always editable", !awbHeader.EH_AreRateLinesOverriddenInfo.ReadOnly);

			shipment.IsAWBValuesOverriddenProperty = false;
			Assert("EH_AreRateLinesOverridden should be always editable", !awbHeader.EH_AreRateLinesOverriddenInfo.ReadOnly);
		}

		public void TestPopulateRateLinesWhenEH_AreRateLinesOverriddenChanged()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			shipment.JS_TotalPackageCount = 4;
			shipment.JS_OuterPacks = 50;
			shipment.JS_ActualWeight = 4.4m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = "Nintendo Switch";

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.ALL;

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("EH_ShippingLoadAndCount", 4, (int)awbHeader.EH_ShippingLoadAndCount);
				AssertEquals("AWBRateLines[0].ER_NoOfPiecesOrRCP", "50", awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
				AssertEquals("AWBRateLines[0].ER_GrossWeight", 4.4m, awbHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", "K", awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs);
				AssertEquals("AWBRateLines[0].NatureAndQtyOfGoodsText.Text", "Nintendo Switch", awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsText.Text);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[0].ER_Total);
				AssertEquals("AWBRateLines[1].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[1].ER_Total);
				AssertEquals("EH_AsAgreed1st", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
				AssertEquals("EH_AsAgreed2nd", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
				AssertEquals("AWBRateLines[11].NatureAndQtyOfGoodsText.Text", "4 SLAC", awbHeader.AWBRateLines[11].NatureAndQtyOfGoodsText.Text);
			});

			shipment.IsAWBValuesOverriddenProperty = true;

			CombineAssertions(() =>
			{
				Assert("EH_AreRateLinesOverridden", awbHeader.EH_AreRateLinesOverridden);
				AssertEquals("EH_ShippingLoadAndCount", 4, (int)awbHeader.EH_ShippingLoadAndCount);
				AssertEquals("AWBRateLines[0].ER_NoOfPiecesOrRCP", "50", awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
				AssertEquals("AWBRateLines[0].ER_GrossWeight", 4.4m, awbHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", "K", awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs);
				AssertEquals("AWBRateLines[0].NatureAndQtyOfGoodsText.Text", "Nintendo Switch", awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsText.Text);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[0].ER_Total);
				AssertEquals("AWBRateLines[1].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[1].ER_Total);
				AssertEquals("EH_AsAgreed1st", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
				AssertEquals("EH_AsAgreed2nd", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
				AssertEquals("AWBRateLines[11].NatureAndQtyOfGoodsText.Text", "4 SLAC", awbHeader.AWBRateLines[11].NatureAndQtyOfGoodsText.Text);
			});

			awbHeader.EH_ShippingLoadAndCount = 3;
			awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "48";
			awbHeader.AWBRateLines[0].ER_GrossWeight = 3.2m;
			awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = "L";
			awbHeader.AWBRateLines[0].ER_Total = 12.1m;
			awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsText.Text = "Xbox Series X";
			awbHeader.AWBRateLines[1].ER_Total = 3.1m;
			awbHeader.AWBRateLines[11].NatureAndQtyOfGoodsText.Text = "2342";

			awbHeader.EH_AsAgreed1st = Constants.AWB.AsAgreedTypes.Codes.Collect;
			awbHeader.EH_AsAgreed2nd = Constants.AWB.AsAgreedTypes.Codes.Prepaid;

			shipment.IsAWBValuesOverriddenProperty = false;

			CombineAssertions(() =>
			{
				Assert("EH_AreRateLinesOverridden", awbHeader.EH_AreRateLinesOverridden);
				AssertEquals("EH_ShippingLoadAndCount", 3, (int)awbHeader.EH_ShippingLoadAndCount);
				AssertEquals("AWBRateLines[0].ER_NoOfPiecesOrRCP", "48", awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
				AssertEquals("AWBRateLines[0].ER_GrossWeight", 3.2m, awbHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", "L", awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs);
				AssertEquals("AWBRateLines[0].NatureAndQtyOfGoodsText.Text", "Xbox Series X", awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsText.Text);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", 12.1m, awbHeader.AWBRateLines[0].ER_Total);
				AssertEquals("AWBRateLines[1].ER_WeightInLBsOrKGs", 3.1m, awbHeader.AWBRateLines[1].ER_Total);
				AssertEquals("EH_AsAgreed1st", Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed1st);
				AssertEquals("EH_AsAgreed2nd", Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed2nd);
				AssertEquals("AWBRateLines[11].NatureAndQtyOfGoodsText.Text", "2342", awbHeader.AWBRateLines[11].NatureAndQtyOfGoodsText.Text);
			});

			awbHeader.EH_AreRateLinesOverridden = false;

			CombineAssertions(() =>
			{
				AssertEquals("EH_ShippingLoadAndCount", 4, (int)awbHeader.EH_ShippingLoadAndCount);
				AssertEquals("AWBRateLines[0].ER_NoOfPiecesOrRCP", "50", awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
				AssertEquals("AWBRateLines[0].ER_GrossWeight", 4.4m, awbHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", "K", awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs);
				AssertEquals("AWBRateLines[0].NatureAndQtyOfGoodsText.Text", "Nintendo Switch", awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsText.Text);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[0].ER_Total);
				AssertEquals("AWBRateLines[1].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[1].ER_Total);
				AssertEquals("EH_AsAgreed1st", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
				AssertEquals("EH_AsAgreed2nd", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
				AssertEquals("AWBRateLines[11].NatureAndQtyOfGoodsText.Text", "4 SLAC", awbHeader.AWBRateLines[11].NatureAndQtyOfGoodsText.Text);
			});

			shipment.IsAWBValuesOverriddenProperty = true;
			awbHeader.EH_AreRateLinesOverridden = false;

			shipment.JS_TotalPackageCount = 5;
			shipment.JS_OuterPacks = 55;
			shipment.JS_ActualWeight = 4.5m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = "Nintendo Switch X";

			awbHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("EH_ShippingLoadAndCount", 5, (int)awbHeader.EH_ShippingLoadAndCount);
				AssertEquals("AWBRateLines[0].ER_NoOfPiecesOrRCP", "55", awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
				AssertEquals("AWBRateLines[0].ER_GrossWeight", 4.5m, awbHeader.AWBRateLines[0].ER_GrossWeight);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", "K", awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs);
				AssertEquals("AWBRateLines[0].NatureAndQtyOfGoodsText.Text", "Nintendo Switch X", awbHeader.AWBRateLines[0].NatureAndQtyOfGoodsText.Text);
				AssertEquals("AWBRateLines[0].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[0].ER_Total);
				AssertEquals("AWBRateLines[1].ER_WeightInLBsOrKGs", 0m, awbHeader.AWBRateLines[1].ER_Total);
				AssertEquals("EH_AsAgreed1st", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
				AssertEquals("EH_AsAgreed2nd", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
				AssertEquals("AWBRateLines[11].NatureAndQtyOfGoodsText.Text", "5 SLAC", awbHeader.AWBRateLines[11].NatureAndQtyOfGoodsText.Text);
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestUniqueIndexFailureHandler()
		{
			ErrorReporter.Clear();
			var initialUserContext = Env.CurrentUserContext;

			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainShipmentAWBOverride.IsAllowed = true;
			securityInstance.MaintainConsolAWBOverride.IsAllowed = true;

			try
			{
				using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
				{
					var nonOperationalUser = Factory.New<GlbStaff>();
					nonOperationalUser.GS_Code = "XYD";
					nonOperationalUser.GS_LoginName = "XYDLoginName";
					nonOperationalUser.GS_IsOperational = false;
					nonOperationalUser.Factory.Save();

					Env.SetUserContext(new UserContext(nonOperationalUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
					ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

					Factory.RefreshEnabled = false;
					Factory.Save();

					shipment.JS_OverrideWaybillDefaults = true;
					ExportAWBHeader awb = shipment.AWBHeader;

					BusinessObjectFactory otherFactory = new BusinessObjectFactory();
					otherFactory.RefreshEnabled = false;

					ForwardingShipment shipmentInOtherFactory = otherFactory.Load<ForwardingShipment>(shipment.PK);
					ExportAWBHeader awbFromOtherFactory = shipmentInOtherFactory.AWBHeader;
					shipmentInOtherFactory.JS_OverrideWaybillDefaults = true;

					AssertNotEquals("precondition: should not be the same row.", awb.PK, awbFromOtherFactory.PK);

					Factory.Save();

					Env.SetUserContext(initialUserContext);

					try
					{
						otherFactory.Save();
						Fail("Save should fail due to Unique Index error");
					}
					catch (ZSaveException ex)
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						ZExceptionReporting.HandleSaveException(ex);

						ZString expectedMessage = ZString.Format(
							"Error '{0} ({1})' has created an Air Waybill form for 'HAWB No: ' while this form was open. Your conflicting changes have been discarded and the created Air Waybill form has been loaded. Review changes to the Air Waybill form and save again.",
							nonOperationalUser.GS_FullName, nonOperationalUser.GS_Code, shipmentInOtherFactory.AWBHeader.EH_ReferenceNumber
						);

						AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

						AssertEquals("awbFromOtherFactory should be deleted", true, awbFromOtherFactory.IsDeleted);
						AssertEquals("should have found the existing awb", awb.PK, shipmentInOtherFactory.AWBHeader.PK);
						AssertEquals("should be loaded as the correct type", awb.GetType(), shipmentInOtherFactory.AWBHeader.GetType());
						AssertEquals("awb should be in the correct factory", otherFactory, shipmentInOtherFactory.AWBHeader.Factory);
						AssertEquals("awb should not be deleted", false, shipmentInOtherFactory.AWBHeader.IsDeleted);

						otherFactory.Save(); // should pass this time.
					}
				}
			}
			finally
			{
				ErrorReporter.Clear();
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestAWBAgentSignature()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "Issuing Carrier Name";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			AssertEquals("Should take value from registry", "Issuing Carrier Name", consol.AWBHeader.EH_AWBAgentsSignature);
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAWBAgentSignature_ForHongKongExport_BorrowedMAWB()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "HKC";
			company.GC_Name = "HongKong Company";
			company.GC_RN_NKCountryCode = "HK";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			branch.GB_Code = "HKB";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var borrowedFrom = Factory.NewWithValidTestData<OrgHeader>();
				borrowedFrom.OH_Code = "BRW";
				borrowedFrom.OH_FullName = "borrowed-from company full name";

				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "176";
				mawb.JM_MAWB = "10000001";
				mawb.JM_GB = branch.PK;
				mawb.JM_ServiceLevel = "STD";
				mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
				mawb.JM_OA_From = borrowedFrom.MainAddress.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.MasterBillAirlinePrefix = "176";
				consol.JK_IsNeutralMaster = true;

				mawb.JM_ParentID = consol.PK;

				var initialUserContext = Env.CurrentUserContext;

				Factory.Save();

				try
				{
					Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "Issuing Carrier Name";

					consol.PopulateAWB();
					AssertEquals("AgentSignature should be issuing carrier name for borrowed MAWB cross trade when login branch is Hong Kong.", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, consol.AWBHeader.EH_AWBAgentsSignature);

					consol.JK_RL_NKDischargePort = "HKHKG";
					consol.PopulateAWB();
					AssertEquals("AgentSignature should be issuing carrier name for Hong Kong import borrowed MAWB when login branch is Hong Kong.", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, consol.AWBHeader.EH_AWBAgentsSignature);

					consol.JK_RL_NKLoadPort = "HKHKG";
					consol.JK_RL_NKDischargePort = "NZAKL";
					consol.PopulateAWB();
					AssertEquals("AgentSignature should be issuing carrier name for Hong Kong export borrowed MAWB when login branch is Hong Kong", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, consol.AWBHeader.EH_AWBAgentsSignature);

					mawb.JM_OA_From = ZGuid.Empty;
					consol.PopulateAWB();
					AssertEquals("AgentSignature should be issuing carrier name for non-borrowed MAWB Hong Kong export when login branch is Hong Kong.", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, consol.AWBHeader.EH_AWBAgentsSignature);
				}
				finally
				{
					Env.SetUserContext(initialUserContext);
				}
			}
		}

		public void TestAWBAgentSignature_MaxLength()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "Some super long issuing carrier agent name";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var aWBAgentsSignature = consol.AWBHeader.EH_AWBAgentsSignature;

			CombineAssertions("Should truncate based on max length", () =>
			{
				AssertEquals(consol.AWBHeader.AgentsSignatureMaxLength, aWBAgentsSignature.Length);
				AssertEquals("Some super long issuing carrier age", aWBAgentsSignature);
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAgentShippersSignature()
		{
			var initialUserContext = Env.CurrentUserContext;

			var bob = Factory.New<GlbStaff>();
			bob.GS_LoginName = "Bob";
			bob.GS_FullName = "Bob";
			bob.GS_Code = "BOB";

			var dot = Factory.New<GlbStaff>();
			dot.GS_LoginName = "Dot";
			dot.GS_FullName = "Dot";
			dot.GS_Code = "Dot";
			Factory.Save();

			try
			{
				Env.SetUserContext(new UserContext(bob.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				var awbHeader = consol.AWBHeader;
				AssertEquals("Bob", awbHeader.EH_ShippersSignature);

				Env.SetUserContext(new UserContext(dot.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				Factory.Save();
				AssertEquals("Should not change on save", "Bob", awbHeader.EH_ShippersSignature);
				consol.PopulateAWB();
				AssertEquals("Should change, it's not a system account", "Dot", awbHeader.EH_ShippersSignature);

				Env.SetUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				Factory.Save();
				AssertEquals("Should not be service user", "Dot", awbHeader.EH_ShippersSignature);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAgentShipperSignature_ShouldBeIssuingCarrierAgentName_ForHongKongExportBorrowedMAWB()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "HKC";
			company.GC_Name = "HongKong Company";
			company.GC_RN_NKCountryCode = "HK";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			branch.GB_Code = "HKB";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var borrowedFrom = Factory.NewWithValidTestData<OrgHeader>();
				borrowedFrom.OH_Code = "BRW";
				borrowedFrom.OH_FullName = "borrowed-from company full name";

				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "176";
				mawb.JM_MAWB = "10000001";
				mawb.JM_GB = branch.PK;
				mawb.JM_ServiceLevel = "STD";
				mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
				mawb.JM_OA_From = borrowedFrom.MainAddress.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.MasterBillAirlinePrefix = "176";
				consol.JK_IsNeutralMaster = true;

				mawb.JM_ParentID = consol.PK;

				Factory.Save();

				var initialUserContext = Env.CurrentUserContext;

				try
				{
					consol.PopulateAWB();
					AssertEquals("ShippersSignature should be login username for borrowed MAWB cross trade when login branch is Hong Kong.", GlbStaff.CurrentUser.GS_FullName, consol.AWBHeader.EH_ShippersSignature);

					consol.JK_RL_NKDischargePort = "HKHKG";
					consol.PopulateAWB();
					AssertEquals("ShippersSignature should be login username for Hong Kong import borrowed MAWB when login branch is Hong Kong.", GlbStaff.CurrentUser.GS_FullName, consol.AWBHeader.EH_ShippersSignature);

					Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "borrower-company full name";
					consol.JK_RL_NKLoadPort = "HKHKG";
					consol.JK_RL_NKDischargePort = "NZAKL";
					mawb.JM_ParentID = consol.PK;
					consol.PopulateAWB();
					AssertEquals("ShippersSignature should be IssuingCarrierAgentName registry for Hong Kong export borrowed MAWB when login branch is Hong Kong",
						Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName,
						consol.AWBHeader.EH_ShippersSignature);

					mawb.JM_OA_From = ZGuid.Empty;
					consol.PopulateAWB();
					AssertEquals("ShippersSignature should be login username for non-borrowed MAWB Hong Kong export when login branch is Hong Kong.", GlbStaff.CurrentUser.GS_FullName, consol.AWBHeader.EH_ShippersSignature);

					var dgnCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
					dgnCertificate.XZ_Type = Constants.StaffDefaultCertificateIDAndTrainingTypes.DGN;
					dgnCertificate.XZ_ExpiryOrDueDate = ZDateTime.Today.AddYears(1);
					dgnCertificate.XZ_RefNumber = "XYZ578 TRU 324 Z53466576";
					consol.PopulateAWB();
					const string expectedSignature = "CargoWise Support XYZ578 TRU 324 Z5";
					AssertEquals("Expected value's length should be equal to the max length of EH_ShippersSignature", 35, expectedSignature.Length);
					AssertEquals("ShippersSignature should be login username + DGN for non-borrowed MAWB Hong Kong export when login branch is Hong Kong.", expectedSignature, consol.AWBHeader.EH_ShippersSignature);
				}
				finally
				{
					Env.SetUserContext(initialUserContext);
				}
			}
		}

		#region Tax Test

		[ExpectNoExceptions]
		public void TestTaxesWhenNoTaxRate()
		{
			TestCaseHelper.ClearTable(JobChargeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccChargeCodeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTaxRateSchema.Constants.TableName);
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;
			decimal taxes = AWBHeader.EH_TaxesCOL;
		}

		public void TestTaxesPPD()
		{
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;

			AccChargeCode charge = Factory.New<AccChargeCode>();
			charge.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CH;
			charge.AC_Desc = "XYZ";

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(50), TaxAmount = new ZDecimal(5), PrepaidCollect = "PPD", AccChargeCode = charge }
			});

			AWBHeader.ResultEH_TotalWeightPPD = 10m;
			AWBHeader.EH_ValuationPPD = 20m;
			AWBHeader.ResultEH_OtherChargesDueAgentPPD = 30m;
			AWBHeader.ResultEH_OtherChargesDueCarrierPPD = 40m;

			AWBHeader.Populate();

			AssertEquals("Populate method which calls PopulateTaxAmounts should be used to calculate tax", 5m, AWBHeader.EH_TaxesPPD);

			ExportAWBOtherCharges newCharge = AWBHeader.AWBOtherCharges.AddNew();
			newCharge.EO_Amount = 43;
			AssertEquals("Retrospectively changing charges on AWB should not update tax", 5m, AWBHeader.EH_TaxesPPD);

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			AWBHeader.Populate();

			AssertEquals("Taxes should not be calculated when registry is set to no", 0m, AWBHeader.EH_TaxesPPD);
			AssertEquals("Tax should not be read only when registry is set to no", false, AWBHeader.EH_TaxesPPDInfo.ReadOnly);
		}

		public void TestTaxesCOL()
		{
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;

			AccChargeCode charge = Factory.New<AccChargeCode>();
			charge.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CH;
			charge.AC_Desc = "XYZ";

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(50), TaxAmount = new ZDecimal(5), PrepaidCollect = "COL", AccChargeCode = charge }
			});

			AWBHeader.ResultEH_TotalWeightCOL = 10m;
			AWBHeader.EH_ValuationCOL = 20m;
			AWBHeader.ResultEH_OtherChargesDueAgentCOL = 30m;
			AWBHeader.ResultEH_OtherChargesDueCarrierCOL = 40m;

			AWBHeader.Populate();

			AssertEquals("Populate method which calls PopulateTaxAmounts should be used to calculate tax", 5m, AWBHeader.EH_TaxesCOL);

			ExportAWBOtherCharges newCharge = AWBHeader.AWBOtherCharges.AddNew();
			newCharge.EO_Amount = 43;
			AssertEquals("Retrospectively changing charges on AWB should not update tax", 5m, AWBHeader.EH_TaxesCOL);

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			AWBHeader.Populate();

			AssertEquals("Taxes should not be calculated when registry is set to no", 0m, AWBHeader.EH_TaxesCOL);
			AssertEquals("Tax should not be read only when registry is set to no", false, AWBHeader.EH_TaxesCOLInfo.ReadOnly);
		}

		public void TestTaxesAreAutoCalculated()
		{
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;

			AccChargeCode charge = Factory.New<AccChargeCode>();
			charge.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CH;
			charge.AC_Desc = "XYZ";

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(50), TaxAmount = new ZDecimal(5), PrepaidCollect = "PPD", AccChargeCode = charge },
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(75), TaxAmount = new ZDecimal(10), PrepaidCollect = "COL", AccChargeCode = charge }
			});

			AWBHeader.Populate();

			AssertEquals("Precondition: TaxesPPD", 5m, AWBHeader.EH_TaxesPPD);
			AssertEquals("Precondition: TaxesCOL", 10m, AWBHeader.EH_TaxesCOL);

			AWBHeader.TestOverrideWayBillDefaults = true;

			AssertEquals("TaxesPPD defaulted to 5", 5m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL defaulted to 10", 10m, AWBHeader.EH_TaxesCOL);
			Assert("TaxesPPD is read/write", !AWBHeader.EH_TaxesPPDInfo.ReadOnly);
			Assert("TaxesCOL is read/write", !AWBHeader.EH_TaxesCOLInfo.ReadOnly);

			AWBHeader.EH_TaxesPPD = 100m;
			AWBHeader.EH_TaxesCOL = 200m;

			AssertEquals("TaxesPPD value is changed", 100m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL value is changed", 200m, AWBHeader.EH_TaxesCOL);

			AWBHeader.TestOverrideWayBillDefaults = false;
			AWBHeader.Populate();

			AssertEquals("TaxesPPD is reset", 5m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL is reset", 10m, AWBHeader.EH_TaxesCOL);

			AWBHeader.TestOverrideWayBillDefaults = true;

			AssertEquals("TaxesPPD defaulted to 5 again", 5m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL defaulted to 10 again", 10m, AWBHeader.EH_TaxesCOL);
		}

		public void TestTaxesAreNotAutoCalculated()
		{
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;

			AccChargeCode charge = Factory.New<AccChargeCode>();
			charge.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CH;
			charge.AC_Desc = "XYZ";

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(50), TaxAmount = new ZDecimal(5), PrepaidCollect = "PPD", AccChargeCode = charge },
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(75), TaxAmount = new ZDecimal(10), PrepaidCollect = "COL", AccChargeCode = charge }
			});

			AWBHeader.Populate();

			AssertEquals("TaxesPPD defaulted to 0", 0m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL defaulted to 0", 0m, AWBHeader.EH_TaxesCOL);

			AWBHeader.TestOverrideWayBillDefaults = true;

			AssertEquals("TaxesPPD remains 0", 0m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL remains 0", 0m, AWBHeader.EH_TaxesCOL);
			Assert("TaxesPPD is read/write", !AWBHeader.EH_TaxesPPDInfo.ReadOnly);
			Assert("TaxesCOL is read/write", !AWBHeader.EH_TaxesCOLInfo.ReadOnly);

			AWBHeader.EH_TaxesPPD = 100m;
			AWBHeader.EH_TaxesCOL = 200m;

			AssertEquals("TaxesPPD changed to 100", 100m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL changed to 200", 200m, AWBHeader.EH_TaxesCOL);

			AWBHeader.TestOverrideWayBillDefaults = false;
			AWBHeader.Populate();

			AssertEquals("TaxesPPD defaulted to 0 again", 0m, AWBHeader.EH_TaxesPPD);
			AssertEquals("TaxesCOL defaulted to 0 again", 0m, AWBHeader.EH_TaxesCOL);
		}

		public void TestCalculateTotalsForTaxFromFreightCharges()
		{
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;

			var charge = Factory.New<AccChargeCode>();
			charge.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.CH;
			charge.AC_Desc = "XYZ";

			AWBHeader.SetTemplates(new List<OtherChargeTemplate>
			{
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(50), TaxAmount = new ZDecimal(5), PrepaidCollect = "PPD", AccChargeCode = charge },
				new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(75), TaxAmount = new ZDecimal(10), PrepaidCollect = "COL", AccChargeCode = charge }
			});

			var chargePPD = Factory.New<JobCharge>();
			chargePPD.JR_GB = GlbBranch.CurrentBranch.PK;
			chargePPD.JR_AT_CostGSTRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", "RAT", 10).PK;
			chargePPD.JR_AT_SellGSTRate = chargePPD.JR_AT_CostGSTRate;
			chargePPD.JR_OSCostAmt = 50;
			chargePPD.JR_OSSellAmt = 100;

			var chargeCOL = Factory.New<JobCharge>();
			chargeCOL.JR_GB = GlbBranch.CurrentBranch.PK;
			chargeCOL.JR_AT_CostGSTRate = chargePPD.JR_AT_CostGSTRate;
			chargeCOL.JR_AT_SellGSTRate = chargePPD.JR_AT_SellGSTRate;
			chargeCOL.JR_OSCostAmt = 150;
			chargeCOL.JR_OSSellAmt = 200;

			AWBHeader.SetPrepaidFreightCharges(new[] { chargePPD });
			AWBHeader.SetCollectFreightCharges(new[] { chargeCOL });

			AWBHeader.Populate();

			AssertEquals("Precondition: TaxesPPD", 15m, AWBHeader.EH_TaxesPPD);
			AssertEquals("Precondition: TaxesCOL", 30m, AWBHeader.EH_TaxesCOL);
		}

		public void TestEH_RateClassLabelText()
		{
			AssertEquals("EH_RateClassLabelText", "Rate Class", AWBHeader.EH_RateClassLabelText);
		}

		#endregion

		public void TestChargeCodesList()
		{
			AssertEquals(25, AWBHeader.ChargeCodesList.Count);
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesCollect));
			AssertEquals("All Charges Collect", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesCollect));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesCollectByCreditCard));
			AssertEquals("All Charges Collect By Credit Card", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesCollectByCreditCard));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesCollectByGBL));
			AssertEquals("All Charges Collect By GBL", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesCollectByGBL));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash));
			AssertEquals("All Charges Prepaid Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCredit));
			AssertEquals("All Charges Prepaid Credit", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCredit));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidByCreditCard));
			AssertEquals("All Charges Prepaid By Credit Card", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidByCreditCard));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidByGBL));
			AssertEquals("All Charges Prepaid By GBL", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidByGBL));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.DestinationCollectCash));
			AssertEquals("Destination Collect Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.DestinationCollectCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.DestinationCollectCredit));
			AssertEquals("Destination Collect Credit", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.DestinationCollectCredit));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.DestinationCollectByMCO));
			AssertEquals("Destination Collect By MCO", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.DestinationCollectByMCO));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.NoCharge));
			AssertEquals("No Charge", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.NoCharge));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesCollect));
			AssertEquals("No Weight Charge - Other Charges Collect", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesCollect));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidByCreditCard));
			AssertEquals("No Weight Charge - Other Charges Prepaid By Credit Card", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidByCreditCard));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidByGBL));
			AssertEquals("No Weight Charge - Other Charges Prepaid By GBL", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidByGBL));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidCash));
			AssertEquals("No Weight Charge - Other Charges Prepaid Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidCredit));
			AssertEquals("No Weight Charge - Other Charges Prepaid Credit", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.NoWeightCharge_OtherChargesPrepaidCredit));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCredit_PartialPrepaidCash));
			AssertEquals("Partial Collect Credit - Partial Prepaid Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCredit_PartialPrepaidCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCredit_PartialPrepaidCredit));
			AssertEquals("Partial Collect Credit - Partial Prepaid Credit", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCredit_PartialPrepaidCredit));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCreditCard_PartialPrepaidCash));
			AssertEquals("Partial Collect Credit Card - Partial Prepaid Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCreditCard_PartialPrepaidCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCreditCard_PartialPrepaidCredit));
			AssertEquals("Partial Collect Credit Card - Partial Prepaid Credit", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialCollectCreditCard_PartialPrepaidCredit));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCash_PartialCollectCash));
			AssertEquals("Partial Prepaid Cash - Partial Collect Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCash_PartialCollectCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCredit_PartialCollectCash));
			AssertEquals("Partial Prepaid Credit - Partial Collect Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCredit_PartialCollectCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCash));
			AssertEquals("Partial Prepaid Credit Card - Partial Collect Cash", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCash));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCredit));
			AssertEquals("Partial Prepaid Credit Card - Partial Collect Credit", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCredit));
			Assert(AWBHeader.ChargeCodesList.ContainsCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCreditCard));
			AssertEquals("Partial Prepaid Credit Card - Partial Collect Credit Card", AWBHeader.ChargeCodesList.GetDescriptionFromCode(ExportAWBHeader.Constants.ChargeCodes.PartialPrepaidCreditCard_PartialCollectCreditCard));
		}

		public void TestEH_HandlingInformationInfo()
		{
			AssertEquals("Should equal 65 * 3, FWB restriction", 65 * 3, AWBHeader.EH_HandlingInformationInfo.MaxLength);
		}

		public void TestGettingAWBCallsPopulateOnce()
		{
			int hitCount = 0;
			ExportAWBHeader.OnPopulated = (s, e) =>
			{
				hitCount++;
			};

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			ExportAWBHeader awbHeaderConsol = consol.AWBHeader;
			AssertEquals(ZString.Format("ExportAWBHeader.Populate() method was executed {0} times; expected 1", hitCount), 1, hitCount);

			hitCount = 0;

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			ExportAWBHeader awbHeaderShipment = shipment.AWBHeader;
			AssertEquals(ZString.Format("ExportAWBHeader.Populate() method was executed {0} times; expected 1", hitCount), 1, hitCount);
		}

		public void TestSuspendChanges()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Factory.Save();

			Assert("Prequisite", !consol.HasChanges);

			ExportAWBHeader awbHeader = consol.AWBHeader;

			int hitCount = 0;
			EventHandler<HasChangesChangedEventArgs> registerEvent = (s, e) =>
			{
				if (e.ObjectJustWasChanged)
				{
					hitCount++;
				}
			};

			awbHeader.HasChangesChanged += registerEvent;

			awbHeader.Populate();

			Assert(!consol.HasChanges);
			Assert(!awbHeader.HasChanges);
			AssertEquals(ZString.Format("HasChangesChanged event was hit {0} times; expeced none", hitCount), 0, hitCount);
		}

		#region Address Tests

		#region Shipper Address Tests

		[ExpectNoExceptions]
		public void TestEH_ShipperDefaultAddressPicker_SetToInvalidValue()
		{
			AssertEquals("PICK SHIPPER ADDRESS", AWBHeader.EH_ShipperDefaultAddressPicker);
			AWBHeader.EH_ShipperDefaultAddressPicker = "Some Invalid Value";
		}

		public void TestEH_ShipperDefaultAddressPicker()
		{
			AssertEquals("PICK SHIPPER ADDRESS", AWBHeader.EH_ShipperDefaultAddressPicker);
			AWBHeader.EH_ShipperDefaultAddressPicker = "DOC: ADDRESSPAD";
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ShipperAddress2);

			AWBHeader.EH_ShipperDefaultAddressPicker = "PAD: ADDRESSPAD";
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ShipperAddress2);

			AWBHeader.EH_ShipperDefaultAddressPicker = "PIC: ADDRESSPIC";
			AssertEquals("ADDRESSPIC", AWBHeader.EH_ShipperAddress);
			AssertEquals("", AWBHeader.EH_ShipperAddress2);

			AWBHeader.EH_ShipperDefaultAddressPicker = "OFC: ADDRESSOFC";
			AssertEquals("ADDRESSOFC", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSOFC2", AWBHeader.EH_ShipperAddress2);
		}

		public void TestShipperAddressPickList()
		{
			AssertEquals(5, AWBHeader.ShipperAddressPickList.Count);
			AssertEquals("DOC: ADDRESSPAD", AWBHeader.ShipperAddressPickList[0].Code);
			AssertEquals("-1", AWBHeader.ShipperAddressPickList[0].Description);
			AssertEquals("OFC: ADDRESSOFC", AWBHeader.ShipperAddressPickList[1].Code);
			AssertEquals("0", AWBHeader.ShipperAddressPickList[1].Description);
			AssertEquals("PIC: ADDRESSPIC", AWBHeader.ShipperAddressPickList[2].Code);
			AssertEquals("1", AWBHeader.ShipperAddressPickList[2].Description);
			AssertEquals("DLV: ADDRESSDLV", AWBHeader.ShipperAddressPickList[3].Code);
			AssertEquals("2", AWBHeader.ShipperAddressPickList[3].Description);
			AssertEquals("PAD: ADDRESSPAD", AWBHeader.ShipperAddressPickList[4].Code);
			AssertEquals("3", AWBHeader.ShipperAddressPickList[4].Description);
		}

		public void TestShipperAddressDefaults()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			AWBHeader.Populate();
			AssertEquals("ACCT", AWBHeader.EH_ShipperAccount);

			AssertEquals("COMPANYNAME", AWBHeader.EH_ShipperName);
			AssertEquals("ADDRESSOFC", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSOFC2", AWBHeader.EH_ShipperAddress2);
			AssertEquals("BRISBANE", AWBHeader.EH_ShipperPlace);
			AssertEquals("NSW", AWBHeader.EH_ShipperState);
			AssertEquals("2006", AWBHeader.EH_ShipperPostCode);
			AssertEquals("AU", AWBHeader.EH_ShipperCountryCode);
			AssertEquals("", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("+96(3)766", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("COMPANYNAME", AWBHeader.EH_ShipperOverride1);
			AssertEquals("ADDRESSOFC", AWBHeader.EH_ShipperOverride2);
			AssertEquals("ADDRESSOFC2", AWBHeader.EH_ShipperOverride3);
			AssertEquals("BRISBANE NSW 2006 AU", AWBHeader.EH_ShipperOverride4);
			AssertEquals("TE +96(3)766", AWBHeader.EH_ShipperOverride5);

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AWBHeader.Populate();

			AssertEquals("CONAMEDOC", AWBHeader.EH_ShipperName);
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ShipperAddress);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ShipperAddress2);
			AssertEquals("SYDNEY", AWBHeader.EH_ShipperPlace);
			AssertEquals("VICTORIA", AWBHeader.EH_ShipperState);
			AssertEquals("1005", AWBHeader.EH_ShipperPostCode);
			AssertEquals("SG", AWBHeader.EH_ShipperCountryCode);
			AssertEquals("CONTACT NAME", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("+96(1)723", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("CONAMEDOC", AWBHeader.EH_ShipperOverride1);
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ShipperOverride2);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ShipperOverride3);
			AssertEquals("SYDNEY VICTORIA 1005 SG", AWBHeader.EH_ShipperOverride4);
			AssertEquals("TE +96(1)723 CONTACT NAME", AWBHeader.EH_ShipperOverride5);

			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Pickup;
			AWBHeader.Populate();
			AssertEquals("ADDRESSPIC", AWBHeader.EH_ShipperAddress);
		}

		public void TestShipperAddressDefaults_ShipperCountryCode()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Organisation.OH_RL_NKClosestPort = "USNYK";

			Factory.Save();
			AWBHeader.Populate();

			AssertEquals("AU", AWBHeader.EH_ShipperCountryCode);

			orgAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			AWBHeader.Populate();

			AssertEquals("NZ", AWBHeader.EH_ShipperCountryCode);
		}

		public void TestDocAddressesAreEmptyAfterPopulateHasBeenCalled()
		{
			AssertEquals("PreCondition: No addresses", 0, AWBHeader.DocAddresses.Count);
			AWBHeader.DocAddresses.AddNew();
			AssertEquals("PreCondition: one address", 1, AWBHeader.DocAddresses.Count);
			AWBHeader.DocAddresses.AddNew();
			AssertEquals("PreCondition: Two address", 2, AWBHeader.DocAddresses.Count);
			AWBHeader.Populate();
			AssertEquals("DocAddresses no more!", 0, AWBHeader.DocAddresses.Count);
		}

		#endregion

		#region Carrier Address Tests

		public void TestConsolOriginLOCO_NoFirstFlight()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainLeg = consol.Transports[0];
			mainLeg.JW_RL_NKLoadPort = "AUMEL";
			mainLeg.JW_RL_NKDiscPort = "CNSHA";
			mainLeg.JW_TransportMode = Core.Constants.TransportModes.Road;

			var awbHeader = consol.AWBHeader;
			awbHeader.Populate();
			AssertEquals("Origin code should be consol's load port Sydney.", "SYD", awbHeader.EH_AWBOriginCode);
		}

		public void TestConsolOriginLOCO_FirstFlightOnly()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var transportAfterFlight = consol.Transports.AddNew();
			transportAfterFlight.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			transportAfterFlight.JW_RL_NKLoadPort = "CNSHA";
			transportAfterFlight.JW_RL_NKDiscPort = "CNPEK";
			transportAfterFlight.JW_TransportMode = Core.Constants.TransportModes.Road;

			var awbHeader = consol.AWBHeader;
			awbHeader.Populate();
			AssertEquals("Origin code should be consol's load port Sydney. Should ignore transports after first flight", "SYD", awbHeader.EH_AWBOriginCode);
		}

		public void TestConsolOriginLOCO_PreCarriageLeg_DifferentCarrier()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var transportAfterFlight = consol.Transports.AddNew();
			transportAfterFlight.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			transportAfterFlight.JW_RL_NKLoadPort = "CNSHA";
			transportAfterFlight.JW_RL_NKDiscPort = "CNPEK";
			transportAfterFlight.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport1 = consol.Transports.AddNew();
			preCarriageTransport1.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			preCarriageTransport1.JW_RL_NKLoadPort = "AUSYD";
			preCarriageTransport1.JW_RL_NKDiscPort = "AUMEL";
			preCarriageTransport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			var awbHeader = consol.AWBHeader;
			awbHeader.Populate();
			AssertEquals("Pre-carriage with a different carrier comes before the first flight, origin code comes from the carrier's first transport", "MEL", awbHeader.EH_AWBOriginCode);
		}

		public void TestConsolOriginLOCO_TwoPreCarriageLegs_SecondWithSameCarrier()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var transportAfterFlight = consol.Transports.AddNew();
			transportAfterFlight.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			transportAfterFlight.JW_RL_NKLoadPort = "CNSHA";
			transportAfterFlight.JW_RL_NKDiscPort = "CNPEK";
			transportAfterFlight.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport1 = consol.Transports.AddNew();
			preCarriageTransport1.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			preCarriageTransport1.JW_RL_NKLoadPort = "AUSYD";
			preCarriageTransport1.JW_RL_NKDiscPort = "AUCBR";
			preCarriageTransport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport2 = consol.Transports.AddNew();
			preCarriageTransport2.JW_OA_CarrierAddress = mainCarrier.MainAddress.PK;
			preCarriageTransport2.JW_RL_NKLoadPort = "AUCBR";
			preCarriageTransport2.JW_RL_NKDiscPort = "AUMEL";
			preCarriageTransport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = consol.AWBHeader;
			awbHeader.Populate();
			AssertEquals("Should pick up from Canberra as it is now the carrier's first transport leg", "CBR", awbHeader.EH_AWBOriginCode);
		}

		public void TestConsolOriginLOCO_TwoPreCarriageLegs_SameOrEmptyCarrier()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var preCarriageTransport1 = consol.Transports.AddNew();
			preCarriageTransport1.JW_OA_CarrierAddress = mainCarrier.Addresses.AddNew().PK;
			preCarriageTransport1.JW_RL_NKLoadPort = "AUSYD";
			preCarriageTransport1.JW_RL_NKDiscPort = "AUCBR";
			preCarriageTransport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport2 = consol.Transports.AddNew();
			preCarriageTransport2.JW_RL_NKLoadPort = "AUCBR";
			preCarriageTransport2.JW_RL_NKDiscPort = "AUMEL";
			preCarriageTransport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = consol.AWBHeader;
			awbHeader.Populate();
			AssertEquals("Should use consol load port as all precarriage legs are empty or the same carrier", "SYD", awbHeader.EH_AWBOriginCode);
		}

		public void TestConsolOriginLOCOForUNLOCOWithoutIATA()
		{
			var locationWithoutIATA = Factory.New<RefUNLOCO>();
			locationWithoutIATA.RL_Code = "NOIAT";
			locationWithoutIATA.RL_PortName = "Royksopp";
			locationWithoutIATA.RL_NameWithDiacriticals = "Röyksopp";
			locationWithoutIATA.RL_HasAirport = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "NOIAT";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";

			var awbHeader = consol.AWBHeader;

			awbHeader.Populate();
			AssertEquals("Consol load port has no IATA code, origin code comes from the first transport", "SYD", awbHeader.EH_AWBOriginCode);
		}

		#endregion

		#region Consignee Address Tests

		[ExpectNoExceptions]
		public void TestEH_ConsigneeDefaultAddressPicker_SetToInvalidValue()
		{
			AssertEquals("PICK CONSIGNEE ADDRESS", AWBHeader.EH_ConsigneeDefaultAddressPicker);
			AWBHeader.EH_ConsigneeDefaultAddressPicker = "Some Invalid Value";
		}

		public void TestEH_ConsigneeDefaultAddressPicker()
		{
			AssertEquals("PICK CONSIGNEE ADDRESS", AWBHeader.EH_ConsigneeDefaultAddressPicker);
			AWBHeader.EH_ConsigneeDefaultAddressPicker = "DOC: ADDRESSPAD";
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ConsigneeAddress2);

			AWBHeader.EH_ConsigneeDefaultAddressPicker = "PAD: ADDRESSPAD";
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ConsigneeAddress2);

			AWBHeader.EH_ConsigneeDefaultAddressPicker = "DLV: ADDRESSDLV";
			AssertEquals("ADDRESSDLV", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("", AWBHeader.EH_ConsigneeAddress2);

			AWBHeader.EH_ConsigneeDefaultAddressPicker = "OFC: ADDRESSOFC";
			AssertEquals("ADDRESSOFC", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSOFC2", AWBHeader.EH_ConsigneeAddress2);
		}

		public void TestConsigneeAddressPickList()
		{
			AssertEquals(5, AWBHeader.ConsigneeAddressPickList.Count);
			AssertEquals("DOC: ADDRESSPAD", AWBHeader.ConsigneeAddressPickList[0].Code);
			AssertEquals("-1", AWBHeader.ConsigneeAddressPickList[0].Description);
			AssertEquals("OFC: ADDRESSOFC", AWBHeader.ConsigneeAddressPickList[1].Code);
			AssertEquals("0", AWBHeader.ConsigneeAddressPickList[1].Description);
			AssertEquals("PIC: ADDRESSPIC", AWBHeader.ConsigneeAddressPickList[2].Code);
			AssertEquals("1", AWBHeader.ConsigneeAddressPickList[2].Description);
			AssertEquals("DLV: ADDRESSDLV", AWBHeader.ConsigneeAddressPickList[3].Code);
			AssertEquals("2", AWBHeader.ConsigneeAddressPickList[3].Description);
			AssertEquals("PAD: ADDRESSPAD", AWBHeader.ConsigneeAddressPickList[4].Code);
			AssertEquals("3", AWBHeader.ConsigneeAddressPickList[4].Description);
		}

		public void TestConsigneeAddressDefaults()
		{
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			AWBHeader.Populate();
			AssertEquals("ACCT", AWBHeader.EH_ConsigneeAccount);

			AssertEquals("COMPANYNAME", AWBHeader.EH_ConsigneeName);
			AssertEquals("ADDRESSOFC", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSOFC2", AWBHeader.EH_ConsigneeAddress2);
			AssertEquals("BRISBANE", AWBHeader.EH_ConsigneePlace);
			AssertEquals("NSW", AWBHeader.EH_ConsigneeState);
			AssertEquals("2006", AWBHeader.EH_ConsigneePostCode);
			AssertEquals("AU", AWBHeader.EH_ConsigneeCountryCode);
			AssertEquals("", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("+96(3)766", AWBHeader.EH_ConsigneeContactDetail);
			AssertEquals("COMPANYNAME", AWBHeader.EH_ConsigneeOverride1);
			AssertEquals("ADDRESSOFC", AWBHeader.EH_ConsigneeOverride2);
			AssertEquals("ADDRESSOFC2", AWBHeader.EH_ConsigneeOverride3);
			AssertEquals("BRISBANE NSW 2006 AU", AWBHeader.EH_ConsigneeOverride4);
			AssertEquals("TE +96(3)766", AWBHeader.EH_ConsigneeOverride5);

			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AWBHeader.Populate();

			AssertEquals("CONAMEDOC", AWBHeader.EH_ConsigneeName);
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ConsigneeAddress);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ConsigneeAddress2);
			AssertEquals("SYDNEY", AWBHeader.EH_ConsigneePlace);
			AssertEquals("VICTORIA", AWBHeader.EH_ConsigneeState);
			AssertEquals("1005", AWBHeader.EH_ConsigneePostCode);
			AssertEquals("SG", AWBHeader.EH_ConsigneeCountryCode);
			AssertEquals("CONTACT NAME", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("+96(1)723", AWBHeader.EH_ConsigneeContactDetail);
			AssertEquals("CONAMEDOC", AWBHeader.EH_ConsigneeOverride1);
			AssertEquals("ADDRESSPAD", AWBHeader.EH_ConsigneeOverride2);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_ConsigneeOverride3);
			AssertEquals("SYDNEY VICTORIA 1005 SG", AWBHeader.EH_ConsigneeOverride4);
			AssertEquals("TE +96(1)723 CONTACT NAME", AWBHeader.EH_ConsigneeOverride5);

			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Delivery;
			AWBHeader.Populate();
			AssertEquals("ADDRESSDLV", AWBHeader.EH_ConsigneeAddress);
		}

		public void TestConsigneeAddressDefaults_ConsigneeCountryCode()
		{
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.AWB);

			AWBHeader.Organisation.Addresses.Add(orgAddress);

			Factory.Save();
			AWBHeader.Populate();

			AssertEquals("AU", AWBHeader.EH_ConsigneeCountryCode);

			orgAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			AWBHeader.Populate();

			AssertEquals("NZ", AWBHeader.EH_ConsigneeCountryCode);
		}

		#endregion

		#region AlsoNotify Address Tests

		[ExpectNoExceptions]
		public void TestEH_AlsoNotifyDefaultAddressPicker_SetToInvalidValue()
		{
			AssertEquals("PICK ALSO NOTIFY ADDRESS", AWBHeader.EH_AlsoNotifyDefaultAddressPicker);
			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "Some Invalid Value";
		}

		public void TestEH_AlsoNotifyDefaultAddressPicker()
		{
			AssertEquals("PICK ALSO NOTIFY ADDRESS", AWBHeader.EH_AlsoNotifyDefaultAddressPicker);
			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "DOC: ADDRESSPAD";
			AssertEquals("ADDRESSPAD ADDRESSPAD2", AWBHeader.EH_AlsoNotifyAddress);

			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "PAD: ADDRESSPAD";
			AssertEquals("ADDRESSPAD ADDRESSPAD2", AWBHeader.EH_AlsoNotifyAddress);

			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "DLV: ADDRESSDLV";
			AssertEquals("ADDRESSDLV", AWBHeader.EH_AlsoNotifyAddress);

			AWBHeader.EH_AlsoNotifyDefaultAddressPicker = "OFC: ADDRESSOFC";

			AssertEquals("COMPANYNAME", AWBHeader.EH_AlsoNotifyName);
			AssertEquals("ADDRESSOFC ADDRESSOFC2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("BRISBANE", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("NSW", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("2006", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("AU", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96(3)766", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("COMPANYNAME", AWBHeader.EH_NotifyOverride1);
			AssertEquals("ADDRESSOFC", AWBHeader.EH_NotifyOverride2);
			AssertEquals("ADDRESSOFC2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("BRISBANE NSW AU", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96(3)766", AWBHeader.EH_NotifyOverride5);
		}

		public void TestAlsoNotifyAddressPickList()
		{
			AssertEquals(5, AWBHeader.AlsoNotifyAddressPickList.Count);
			AssertEquals("DOC: ADDRESSPAD", AWBHeader.AlsoNotifyAddressPickList[0].Code);
			AssertEquals("-1", AWBHeader.AlsoNotifyAddressPickList[0].Description);
			AssertEquals("OFC: ADDRESSOFC", AWBHeader.AlsoNotifyAddressPickList[1].Code);
			AssertEquals("0", AWBHeader.AlsoNotifyAddressPickList[1].Description);
			AssertEquals("PIC: ADDRESSPIC", AWBHeader.AlsoNotifyAddressPickList[2].Code);
			AssertEquals("1", AWBHeader.AlsoNotifyAddressPickList[2].Description);
			AssertEquals("DLV: ADDRESSDLV", AWBHeader.AlsoNotifyAddressPickList[3].Code);
			AssertEquals("2", AWBHeader.AlsoNotifyAddressPickList[3].Description);
			AssertEquals("PAD: ADDRESSPAD", AWBHeader.AlsoNotifyAddressPickList[4].Code);
			AssertEquals("3", AWBHeader.AlsoNotifyAddressPickList[4].Description);
		}

		public void TestAlsoNotifyAddressDefaults()
		{
			AWBHeader.Populate();

			AssertEquals("CONAMEDOC", AWBHeader.EH_AlsoNotifyName);
			AssertEquals("ADDRESSPAD ADDRESSPAD2", AWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("SYDNEY", AWBHeader.EH_AlsoNotifyPlace);
			AssertEquals("VICTORIA", AWBHeader.EH_AlsoNotifyState);
			AssertEquals("1005", AWBHeader.EH_AlsoNotifyPostCode);
			AssertEquals("SG", AWBHeader.EH_AlsoNotifyCountryCode);
			AssertEquals("TE", AWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("+96(1)723", AWBHeader.EH_AlsoNotifyContactDetail);
			AssertEquals("CONAMEDOC", AWBHeader.EH_NotifyOverride1);
			AssertEquals("ADDRESSPAD", AWBHeader.EH_NotifyOverride2);
			AssertEquals("ADDRESSPAD2", AWBHeader.EH_NotifyOverride3);
			AssertEquals("SYDNEY VICTORIA SG", AWBHeader.EH_NotifyOverride4);
			AssertEquals("TE +96(1)723 CONTACT NAME", AWBHeader.EH_NotifyOverride5);
		}

		#endregion

		#region Test Override Reset

		public void TestAddressOverrides()
		{
			AWBHeader.Populate();

			Assert("Pre-condition: Override check box should be false by default", !AWBHeader.EH_IsShipperOverriden);
			Assert("Pre-condition: Override check box should be false by default", !AWBHeader.EH_IsConsigneeOverriden);
			Assert("Pre-condition: Override check box should be false by default", !AWBHeader.EH_IsNotifyOverriden);
			AssertEquals("Address is not overriden so should match", AWBHeader.EH_ShipperAddress, AWBHeader.EH_ShipperOverride2);
			AssertEquals("Address is not overriden so should match", AWBHeader.EH_ConsigneeAddress, AWBHeader.EH_ConsigneeOverride2);

			AWBHeader.EH_IsShipperOverriden = true;
			AWBHeader.EH_IsNotifyOverriden = true;
			AWBHeader.EH_ShipperDefaultAddressPicker = "DLV: ADDRESSDLV";
			AWBHeader.EH_ConsigneeDefaultAddressPicker = "DLV: ADDRESSDLV";

			Assert(AWBHeader.EH_IsShipperOverriden);
			Assert(!AWBHeader.EH_IsConsigneeOverriden);
			Assert(AWBHeader.EH_IsNotifyOverriden);
			AssertNotEquals("Overriden name should no longer matched changed name", AWBHeader.EH_ShipperAddress, AWBHeader.EH_ShipperOverride2);
			AssertEquals("Names should still match as consignee has not be overriden", AWBHeader.EH_ConsigneeAddress, AWBHeader.EH_ConsigneeOverride2);

			AWBHeader.Populate();
			Assert("Should be reset to false", !AWBHeader.EH_IsShipperOverriden);
			Assert("Should be reset to false", !AWBHeader.EH_IsConsigneeOverriden);
			Assert("Should be reset to false", !AWBHeader.EH_IsNotifyOverriden);
			AssertEquals("Once the AWB has been reset, these addresses should match again", AWBHeader.EH_ShipperAddress, AWBHeader.EH_ShipperOverride2);
			AssertEquals("Once the AWB has been reset, these addresses should match again", AWBHeader.EH_ConsigneeAddress, AWBHeader.EH_ConsigneeOverride2);
		}

		#endregion

		#endregion

		[TestDate(2023, 2, 22, 10, 11, 48)]
		public void TestPopulateIssueDate()
		{
			var now = ZDateTime.Now;
			var consolIssueDate = now.AddDays(2);
			var awbIssueDate = now.AddDays(20);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeader = shipment.AWBHeader;

			awbHeader.EH_AWBIssueDate = consolIssueDate;
			awbHeader.Populate();
			AssertEquals(now, awbHeader.EH_AWBIssueDate);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			awbHeader = consol.AWBHeader;

			consol.JK_OverrideWaybillDefaults = true;
			consol.JK_MasterBillIssueDate = consolIssueDate;
			awbHeader.EH_AWBIssueDate = awbIssueDate;
			awbHeader.Populate();
			AssertEquals(awbIssueDate, awbHeader.EH_AWBIssueDate);

			awbHeader.EH_AWBIssueDate = ZDateTime.Empty;
			awbHeader.Populate();
			AssertEquals("Time portion is discarded", consolIssueDate, awbHeader.EH_AWBIssueDate);

			consol.JK_MasterBillIssueDate = ZDateTime.Empty;
			awbHeader.EH_AWBIssueDate = ZDateTime.Empty;
			awbHeader.Populate();
			AssertEquals(now, awbHeader.EH_AWBIssueDate);

			consol.JK_OverrideWaybillDefaults = false;
			consol.JK_MasterBillIssueDate = consolIssueDate;
			awbHeader.Populate();
			AssertEquals("Time portion is discarded", consolIssueDate, awbHeader.EH_AWBIssueDate);

			consol.JK_MasterBillIssueDate = ZDateTime.Empty;
			awbHeader.Populate();
			AssertEquals(now, awbHeader.EH_AWBIssueDate);

			consol.Shipments.Add(shipment);
			awbHeader = shipment.AWBHeader;

			shipment.JS_OverrideWaybillDefaults = false;
			consol.JK_MasterBillIssueDate = consolIssueDate;
			awbHeader.EH_AWBIssueDate = awbIssueDate;
			awbHeader.Populate();
			AssertEquals("Time portion is discarded", consolIssueDate, awbHeader.EH_AWBIssueDate);

			consol.JK_MasterBillIssueDate = ZDateTime.Empty;
			awbHeader.Populate();
			AssertEquals(now, awbHeader.EH_AWBIssueDate);

			shipment.JS_OverrideWaybillDefaults = true;
			consol.JK_MasterBillIssueDate = consolIssueDate;
			awbHeader.EH_AWBIssueDate = awbIssueDate;
			awbHeader.Populate();
			AssertEquals(awbIssueDate, awbHeader.EH_AWBIssueDate);

			awbHeader.EH_AWBIssueDate = ZDateTime.Empty;
			awbHeader.Populate();
			AssertEquals("Time portion is discarded", consolIssueDate, awbHeader.EH_AWBIssueDate);

			consol.JK_MasterBillIssueDate = ZDateTime.Empty;
			awbHeader.EH_AWBIssueDate = ZDateTime.Empty;
			awbHeader.Populate();
			AssertEquals(now, awbHeader.EH_AWBIssueDate);
		}

		public void TestEH_ExtraShipperInfoLine2_HongKong()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_IsNeutralMaster = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_InspectionTypeCode = "UNK";

			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_MAWB = "10000001";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = consol.PK;

			var awbHeader = consol.AWBHeader;

			var borrowedFrom = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var knownShipperDetails = borrowedFrom.MainAddress.KnownShipperDetails.AddNew();
			knownShipperDetails.OV_EXApprovedOrMajorExporter = "RAN";
			knownShipperDetails.OV_EXApprovalNumber = "RA12345";
			knownShipperDetails.OV_OH_OrgHeader = borrowedFrom.PK;
			mawb.JM_OA_From = borrowedFrom.MainAddress.PK;

			Factory.Save();

			string originalRegistryText = Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText;
			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
				{
					awbHeader.ResetSupplyChainSecurityConfigurationForTesting();

					shipment.JS_InspectionTypeCode = "PHS";
					Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = "Short text < 64 chars";
					awbHeader.Populate();
					AssertEquals("Approval code is shown on line 2", "SPX", awbHeader.EH_ExtraShipperInfoLine2);

					shipment.JS_InspectionTypeCode = "UNK";
					Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = "Test Shipper Text greater than 64 characters but less than 128 characters";
					awbHeader.Populate();
					AssertEquals("No approval", "haracters UNK", awbHeader.EH_ExtraShipperInfoLine2);

					shipment.JS_InspectionTypeCode = "PHS";
					awbHeader.Populate();
					AssertEquals("SPX is appended", "haracters SPX", awbHeader.EH_ExtraShipperInfoLine2);

					Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = "Test Shipper Text greater than 64 characters and also greater than 128 characters so that it needs to be trimmed in order to append the security code";
					awbHeader.Populate();
					AssertEquals("Line is trimmed", "an 128 characters so that it needs to be trimmed in order to SPX", awbHeader.EH_ExtraShipperInfoLine2);
				}

				awbHeader.ResetSupplyChainSecurityConfigurationForTesting();
				awbHeader.Populate();
				AssertEquals("If not Hong Kong then SPX is not appended", "an 128 characters so that it needs to be trimmed in order to app", awbHeader.EH_ExtraShipperInfoLine2);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = originalRegistryText;
			}
		}

		public void TestAgentNameForDeletedMawb()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_IsNeutralMaster = true;

			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_MAWB = "10000001";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = consol.PK;

			ExportAWBHeader awbHeader = consol.AWBHeader;

			OrgHeader borrowedFrom = Factory.LoadTop1<OrgHeader>(new ZQuery());
			mawb.JM_OA_From = borrowedFrom.MainAddress.PK;

			AssertEquals(borrowedFrom.OH_FullName, awbHeader.EH_AgentName);

			mawb.Delete();

			AssertEquals(ZString.Empty, awbHeader.EH_AgentName);
		}

		public void TestAddExtraText()
		{
			var docDataProviderMock = new Mock<IBODocDataProvider>();

			var extraTestClassMock = docDataProviderMock.As<IExtraTextTestClass>();
			extraTestClassMock.Setup(extraTestClass => extraTestClass.Property).Returns("dummy");

			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();

			header.GetNewExtraTextMacroDataProviderImplementation = () =>
			{
				IDocWrapperContext context = Factory.GetDocWrapperContextManager();

				CombineAssertions(() =>
				{
					AssertEquals("DEP", context.DocumentDirection);
					AssertEquals("ALL", context.DocumentContactTypeCode);
				});

				return docDataProviderMock.Object;
			};

			ZString text = ZString.Empty;
			header.AddExtraText("testing <Property> value", ref text, null);
			AssertEquals("testing dummy value", text);
		}

		public void TestTSASecurityStatement_NoListedCountry_NoUSDischarge()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_HasListedCountry_NoUSDischarge()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "EGLXR";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatementForUSTerritories()
		{
			AssertTSASecurityStatement_ConsolLastDischargeUSTerritories("AUMEL", "GUGUM", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolLastDischargeUSTerritories("AUMEL", "PRSJU", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolLastDischargeUSTerritories("AUSYD", "ASPPG", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolLastDischargeUSTerritories("AUSYD", "VICHA", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolLastDischargeUSTerritories("AUSYD", "MPSPN", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolLastDischargeUSTerritories("EGLXR", "AUSYD", "");
			AssertTSASecurityStatement_ConsolLastDischargeUSTerritories("EGLXR", "GUGUM", "");

			AssertTSASecurityStatement_ConsolTransportUSTerritories("GUGUM", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolTransportUSTerritories("PRSJU", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolTransportUSTerritories("ASPPG", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolTransportUSTerritories("VICHA", "Test statement: Egypt");
			AssertTSASecurityStatement_ConsolTransportUSTerritories("MPSPN", "Test statement: Egypt");
		}

		void AssertTSASecurityStatement_ConsolLastDischargeUSTerritories(string loadPort, string dischargePort, string expectedTSAStatement)
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = loadPort;
				consol.JK_RL_NKDischargePort = dischargePort;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals(expectedTSAStatement, header.TSASecurityStatement);
			}
		}

		void AssertTSASecurityStatement_ConsolTransportUSTerritories(string transhippedPort, string expectedTSAStatement)
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				consol.Transports.AddNew().JW_RL_NKDiscPort = transhippedPort;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals(expectedTSAStatement, header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolFirstLoadListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "EGLXR";
				consol.JK_RL_NKDischargePort = "USLAX";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolFirstLoadNotListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolTransportLoadListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				consol.Transports.AddNew().JW_RL_NKLoadPort = "EGLXR";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolTransportLoadNotListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				consol.Transports.AddNew().JW_RL_NKLoadPort = "CNCAN";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolShipmentOriginListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				consol.Shipments.AddNew().JS_RL_NKOrigin = "EGLXR";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}
		public void TestTSASecurityStatement_ConsolShipmentOriginNotListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				consol.Shipments.AddNew().JS_RL_NKOrigin = "CNCAN";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolShipmentTransportListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				consol.Shipments.AddNew().Transports.AddNew().JW_RL_NKLoadPort = "EGLXR";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolShipmentTransportNotListed_ConsolLastDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				consol.Shipments.AddNew().Transports.AddNew().JW_RL_NKLoadPort = "CNCAN";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolFirstLoadListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "EGLXR";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolFirstLoadNotListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolTransportLoadListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				consol.Transports.AddNew().JW_RL_NKLoadPort = "EGLXR";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolTransportLoadNotListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				consol.Transports.AddNew().JW_RL_NKLoadPort = "AUSYD";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolShipmentOriginListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				consol.Shipments.AddNew().JS_RL_NKOrigin = "EGLXR";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolShipmentOriginNotListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				consol.Shipments.AddNew().JS_RL_NKOrigin = "AUSYD";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolShipmentTransportListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				consol.Shipments.AddNew().Transports.AddNew().JW_RL_NKLoadPort = "EGLXR";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.TSASecurityStatement);
			}
		}

		public void TestTSASecurityStatement_ConsolShipmentTransportNotListed_ConsolAirTransportDischargeUS()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Egypt }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNCAN";

				var airLeg = consol.Transports.AddNew();
				airLeg.JW_TransportMode = Constants.TransportModes.Air;
				airLeg.JW_RL_NKDiscPort = "USLAX";

				consol.Shipments.AddNew().Transports.AddNew().JW_RL_NKLoadPort = "AUSYD";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt", header.TSASecurityStatement);
			}
		}

		public void TestEH_AdditionalSecurityInformation()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.SyrianArabRepublic }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				AssertEquals("Test statement: Egypt, Syrian Arab Republic", consol.AWBHeader.AdditionalSecurityInformation);
				AssertEquals("Test statement: Egypt, Syrian Arab Republic", consol.AWBHeader.EH_AdditionalSecurityInformation);

				consol.IsCSDValuesOverriddenProperty = true;
				consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description, "Test Note.");
				var expectedText = @"Test statement: Egypt, Syrian Arab Republic

Test Note.";
				AssertEquals(expectedText, consol.AWBHeader.AdditionalSecurityInformation);
				AssertEquals("Test statement: Egypt, Syrian Arab Republic", consol.AWBHeader.EH_AdditionalSecurityInformation);
			}
		}

		public void TestEH_ScheduledArrivalDate_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "GBDRT";
				consol.JK_RL_NKDischargePort = "USLAX";
				var awbHeader = consol.AWBHeader;
				awbHeader.EH_AgentApprovalNumber = "BLAH123";
				awbHeader.EH_RN_NKAgentApprovalCountryCode = Constants.CountryCodes.UnitedKingdom;
				var awbActions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.All);
				awbActions.SendFWB = true;
				awbActions.PrintConsignmentSecurityDeclaration = true;

				var addressCountryData = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				addressCountryData.OV_EXApprovalNumber = "12345-01";
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				Assert(consol.AWBHeader.EH_ScheduledArrivalDateInfo.ReadOnly);

				consol.IsCSDValuesOverriddenProperty = true;
				var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();
				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				awbHeader.CargoSecurityScreeningMethods.AddNew().EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;
				AssertEquals(securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				Assert(consol.AWBHeader.EH_ScheduledArrivalDateInfo.ReadOnly);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertEquals(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft, awbHeader.EH_SecurityStatus);
				Assert(!consol.AWBHeader.EH_ScheduledArrivalDateInfo.ReadOnly);

				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				Assert(consol.AWBHeader.EH_ScheduledArrivalDateInfo.ReadOnly);
			}
		}

		public void TestAdditionalSecurityInformation_HasTSAStatement_NoNotes()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.SyrianArabRepublic }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test statement: Egypt, Syrian Arab Republic", header.AdditionalSecurityInformation);
			}
		}

		public void TestAdditionalSecurityInformation_HasTSAStatement_HasNotes()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.SyrianArabRepublic }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CNCAN";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description, "Test Note.");

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				var expectedText = @"Test statement: Egypt, Syrian Arab Republic

Test Note.";
				AssertEquals(expectedText, header.AdditionalSecurityInformation);
			}
		}

		public void TestAdditionalSecurityInformation_NoTSAStatement_HasNotes()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.SyrianArabRepublic }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "EGLXR";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description, "Test Note.");

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("Test Note.", header.AdditionalSecurityInformation);
			}
		}

		public void TestAdditionalSecurityInformation_NoTSAStatement_NoNotes()
		{
			using (FreightDataRegistry.Instance.AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.SyrianArabRepublic }))
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test statement: <TSASecurityStatementCountriesText>"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "EGLXR";
				consol.JK_RL_NKDischargePort = "USLAX";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals("", header.AdditionalSecurityInformation);
			}
		}

		public void TestShipperTraderTypeWithNo_FromRefTable_ForChina()
		{
			AWBHeader.SetSupportedTaxDocumentType("AWB");

			CreateRefDocOrgCusCode(OrgCusCode.ChinaCodeTypes.USC, Constants.CountryCodes.China, Constants.CountryCodes.China, 1, "USC", "USC", "ESI");

			AWBHeader.Origin = "CNSHA";
			AWBHeader.Destination = "ALBUT";

			var taxCode = AWBHeader.Organisation.CustomsCodes.AddNew();
			taxCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.China;
			taxCode.OK_CustomsRegNo = "1111";

			AWBHeader.Populate();

			CombineAssertions("CN codes are not returned, even though they are still codes with type other than AWB/HAW present in the RefDb.", () =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperTraderNoType);
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperTraderNo);
			});
		}

		public void TestSetDocumentSettings()
		{
			var actions = new MockAWBActions(AWBActions.ActionsModeType.None, Factory)
			{
				LabelStartRange = 1,
				LabelEndRange = 2,
				LabelTotalPacks = 3,
				MAWBLabelStartRange = 4,
				MAWBLabelTotalPacks = 5,
				DocumentSize = "a",
				PrintOptionalInformation = false
			};
			var header = Factory.New<MockExportAWBHeader>();
			header.SetAWBActions(actions);
			actions.SetAWB(header);

			Assert(!header.DocumentSettingsPopulated);
			AssertEquals(1, header.LabelStartRange);
			AssertEquals(2, header.LabelEndRange);
			AssertEquals(3, header.LabelTotalPacks);
			AssertEquals(4, header.MAWBLabelStartRange);
			AssertEquals(5, header.MAWBLabelTotalPacks);
			AssertEquals("a", header.DocumentSize);
			Assert(!header.PrintOptionalInformation);
			Assert(header.DocumentSettingsPopulated);

			actions.LabelStartRange = 6;
			actions.LabelEndRange = 7;
			actions.LabelTotalPacks = 8;
			actions.MAWBLabelStartRange = 9;
			actions.MAWBLabelTotalPacks = 10;
			actions.DocumentSize = "b";
			actions.PrintOptionalInformation = true;
			AssertEquals(1, header.LabelStartRange);
			AssertEquals(2, header.LabelEndRange);
			AssertEquals(3, header.LabelTotalPacks);
			AssertEquals(4, header.MAWBLabelStartRange);
			AssertEquals(5, header.MAWBLabelTotalPacks);
			AssertEquals("a", header.DocumentSize);
			Assert(!header.PrintOptionalInformation);

			header.LabelStartRange = 11;
			header.LabelEndRange = 12;
			header.LabelTotalPacks = 13;
			header.MAWBLabelStartRange = 14;
			header.MAWBLabelTotalPacks = 15;
			header.DocumentSize = "c";
			header.PrintOptionalInformation = true;
			AssertEquals(11, header.LabelStartRange);
			AssertEquals(12, header.LabelEndRange);
			AssertEquals(13, header.LabelTotalPacks);
			AssertEquals(14, header.MAWBLabelStartRange);
			AssertEquals(15, header.MAWBLabelTotalPacks);
			AssertEquals("c", header.DocumentSize);
			Assert(header.PrintOptionalInformation);
		}

		public void TestDocumentSettingsPopulated()
		{
			AWBHeader.DocumentSettingsPopulated = true;
			Factory.Save();
			Assert(!AWBHeader.DocumentSettingsPopulated);
		}

		public void TestIsImportToExportFromTransitingThroughUnitedArabEmirates()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Category = OrgConstants.Category.Government;
			shipment.ConsigneePK = consigneeOrg.PK;

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipperOrg.OH_Category = OrgConstants.Category.Government;
			shipment.ConsignorPK = shipperOrg.PK;

			var header = shipment.AWBHeader;
			shipment.JS_RL_NKOrigin = "AEAAN";
			shipment.JS_RL_NKDestination = "AUSYD";
			header.Populate();
			Assert("Origin: UAE", header.IsImportToExportFromTransitingThroughUnitedArabEmirates);

			consigneeOrg.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header.Populate();
			Assert("Consignee: NaturalPersonIndividual", !header.IsImportToExportFromTransitingThroughUnitedArabEmirates);

			consigneeOrg.OH_Category = OrgConstants.Category.Government;
			shipperOrg.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header.Populate();
			Assert("Consignor: NaturalPersonIndividual", !header.IsImportToExportFromTransitingThroughUnitedArabEmirates);

			shipperOrg.OH_Category = OrgConstants.Category.Government;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AEAAN";
			header.Populate();
			Assert("Destination: UAE", header.IsImportToExportFromTransitingThroughUnitedArabEmirates);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			var departureLeg = shipment.TransportsIncludingRelated.AddNew();
			departureLeg.JW_RL_NKLoadPort = "AUSYD";
			departureLeg.JW_RL_NKDiscPort = "AEAAN";
			header.Populate();
			Assert("DiscPort: UAE", header.IsImportToExportFromTransitingThroughUnitedArabEmirates);

			shipment.TransportsIncludingRelated.DeleteAll();
			var arrivalLeg = shipment.TransportsIncludingRelated.AddNew();
			arrivalLeg.JW_RL_NKLoadPort = "AEAAN";
			arrivalLeg.JW_RL_NKDiscPort = "CNSHA";
			header.Populate();
			Assert("LoadPort: UAE", header.IsImportToExportFromTransitingThroughUnitedArabEmirates);

			shipment.TransportsIncludingRelated.DeleteAll();
			header.Populate();
			Assert(!header.IsImportToExportFromTransitingThroughUnitedArabEmirates);
		}

		public void TestSetIsExportingData()
		{
			Assert("Default", !AWBHeader.IsExportingData);

			using (AWBHeader.SetIsExportingData())
			{
				Assert("SetIsExportingData 1st", AWBHeader.IsExportingData);
				using (AWBHeader.SetIsExportingData())
				{
					Assert("SetIsExportingData 2nd", AWBHeader.IsExportingData);
				}
				Assert("After SetIsExportingData 2nd", AWBHeader.IsExportingData);
			}

			Assert("After SetIsExportingData 1st", !AWBHeader.IsExportingData);
		}

		public interface IExtraTextTestClass
		{
			ZString Property { get; }
		}

		#region Implementation

		string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			var sql = @"IF (OBJECT_ID('Constraint_EH_Table_NoCheck') IS NOT NULL)
BEGIN
	ALTER TABLE dbo.ExportAWBHeader NOCHECK CONSTRAINT Constraint_EH_Table_NoCheck
END";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void SetUp()
		{
			base.SetUp();
			if (!string.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}
			AWBHeader = Factory.New<MockExportAWBHeader>();
		}
		string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}

		protected override void FinalTearDown()
		{
			var sql = @"IF (OBJECT_ID('Constraint_EH_Table_NoCheck') IS NOT NULL)
BEGIN
	ALTER TABLE dbo.ExportAWBHeader CHECK CONSTRAINT Constraint_EH_Table_NoCheck
END";
			TestConnection.ExecuteNonQuery(sql);
			base.FinalTearDown();
		}

		MockExportAWBHeader AWBHeader;

		void AddConsolChargesToAwbHeader()
		{
			AddChargesToAwbHeader(false);
		}

		void AddShipmentChargesToAwbHeader()
		{
			AddChargesToAwbHeader(true);
		}

		void AddChargesToAwbHeader(bool includeBlank)
		{
			AccChargeCode c1 = Factory.New<AccChargeCode>();
			c1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			c1.AC_Desc = "Test Charge Code Description";

			//This is the IATA code that has "A" entitlement by default for consol
			AccChargeCode c2 = Factory.New<AccChargeCode>();
			c2.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.LA;

			AccChargeCode c3 = Factory.New<AccChargeCode>();
			c3.AC_IATA_ChargeCodeMap = "";
			c3.AC_Desc = "Test Charge Code Description";

			AWBHeader.AWBOtherCharges.RemoveAndDeleteAll();

			List<OtherChargeTemplate> templates = new List<OtherChargeTemplate>();

			templates.Add(new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(100), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap });
			templates.Add(new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(200), PrepaidCollect = "PPD", AccChargeCode = c1, IATAChargeCodeProvider = () => c1.AC_IATA_ChargeCodeMap });
			templates.Add(new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(400), PrepaidCollect = "COL", AccChargeCode = c2, IATAChargeCodeProvider = () => c2.AC_IATA_ChargeCodeMap });
			templates.Add(new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(600), PrepaidCollect = "COL", AccChargeCode = c2, IATAChargeCodeProvider = () => c2.AC_IATA_ChargeCodeMap });

			if (includeBlank)
			{
				templates.Add(new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(550), PrepaidCollect = "PPD", AccChargeCode = c3, IATAChargeCodeProvider = () => c3.AC_IATA_ChargeCodeMap });
				templates.Add(new OtherChargeTemplate(AWBHeader) { ChargeAmountProvider = () => new ZDecimal(820), PrepaidCollect = "PPD", AccChargeCode = c3, IATAChargeCodeProvider = () => c3.AC_IATA_ChargeCodeMap });
			}

			AWBHeader.SetTemplates(templates);

			AWBHeader.PopulateOtherCharges();
		}

		ExportAWBOtherCharges FindCharge(ZString iataChargeCode)
		{
			return AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.First(charge => charge.EO_ChargeCode == iataChargeCode);
		}

		ExportAWBOtherCharges[] FindCharges(ZString iataChargeCode)
		{
			return AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.Where(charge => charge.EO_ChargeCode == iataChargeCode)
				.ToArray();
		}

		#endregion
	}
}
