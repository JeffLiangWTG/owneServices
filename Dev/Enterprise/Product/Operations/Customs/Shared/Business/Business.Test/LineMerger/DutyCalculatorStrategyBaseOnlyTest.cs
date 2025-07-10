using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	sealed class DutyCalculatorStrategyBaseOnlyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DutyCalculatorStrategy(null));
		}

		public override void TestCalculateDuties()
		{
			SetupReferenceData();
			SetupReferenceData("EXC", "RC2", "CV + RC1");
			var dutyCalculatorStrategy = GetDutyCalculatorStrategy();

			CombineAssertions(() =>
			{
				var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
				AssertEquals("Precondition: entryLine1", 0, entryLine1.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();
				AssertEquals("entryLine1: Duty & Excise calculated and added to Fee", 2, entryLine1.Fees.Count);
				AssertEquals("entryLine1: DTY/RC1 - RateCode", "RC1", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("entryLine1: DTY/RC1 - Amout = CV * 0.8", 17.60m, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("entryLine1: EXC/RC2 - RateCode", "RC2", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("entryLine1: EXC/RC2 - Amout = (CV + DTY) * 0.8", 31.68m, entryLine1.Fees[1].CF_ChargeAmount);
			});
		}

		public void TestDutyDecimalPlaceDefaultByDeclarationLocalCurrencyCode()
		{
			CombineAssertions(() =>
			{
				var currency1 = Factory.New<RefCurrency>();
				currency1.RX_ISOSubUnitRatio = 10;
				currency1.RX_Code = "C1";
				var currency2 = Factory.New<RefCurrency>();
				currency2.RX_ISOSubUnitRatio = 1000;
				currency2.RX_Code = "C2";

				AssertEquals("Precondition C1", 1, currency1.ISODecimals);
				AssertEquals("Precondition C2", 3, currency2.ISODecimals);

				var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
				mockDeclaration.Protected().Setup<ZString>("LocalCurrencyCodeCore").Returns("C1");
				var declaration = mockDeclaration.Object;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				var entryLine = entry.AllEntryLines.AddNew();

				var mockInvoice = Factory.NewMoq<BaseJobComInvoiceHeader>();
				mockInvoice.Protected().Setup<ZString>("LocalCurrencyCodeCore").Returns("C2");
				var invoice = mockInvoice.Object;
				invoice.JZ_JE = declaration.PK;
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var strategy = new DutyCalculatorStrategy(declaration);
				var dutyDecimalPlaceField = strategy.GetType().GetProperty("DutyDecimalPlace", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertEquals("Use declaration LocalCurrency", 1, dutyDecimalPlaceField.GetValue(strategy));
			});
		}

		public void TestCalculateDuties_NoUniversalTariff()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = GetDutyCalculatorStrategy();

			CombineAssertions(() =>
			{
				var entryLine5 = GetEntryHeader(Declaration, "XXX").MergedLines[0];
				AssertEquals("Precondition: entryLine5", 0, entryLine5.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();
				AssertEquals("entryLine5: No linked valid universalTariff and not calculate", 0, entryLine5.Fees.Count);
			});
		}

		public void TestCalculateDuties_NoApplicableRate()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = GetDutyCalculatorStrategy();

			CombineAssertions(() =>
			{
				var entryLine4 = GetEntryHeader(Declaration, "0104400000").MergedLines[0];
				AssertEquals("Precondition: entryLine4", 0, entryLine4.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();
				AssertEquals("entryLine4: No applicable rate and not calculate", 0, entryLine4.Fees.Count);
			});
		}

		public void TestCalculateDuties_DutyIsZero()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = GetDutyCalculatorStrategy();

			CombineAssertions(() =>
			{
				var entryLine3 = GetEntryHeader(Declaration, "0103300000").MergedLines[0];
				AssertEquals("Precondition: entryLine3", 0, entryLine3.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();
				AssertEquals("entryLine3: Duty = 0 and not add to fee", 0, entryLine3.Fees.Count);
			});
		}

		public void TestCalculateDuties_NegativeDuty()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = GetDutyCalculatorStrategy();

			CombineAssertions(() =>
			{
				var entryLine2 = GetEntryHeader(Declaration, "0102200000").MergedLines[0];
				AssertEquals("Precondition: entryLine2", 0, entryLine2.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();
				AssertEquals("entryLine2: Duty is negative and rounded to 0. and not add to fee", 0, entryLine2.Fees.Count);
			});
		}

		public void TestCalculateDuties_OverrideDutyDecimalPlaces()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = new DutyCalculatorStrategyWithOverrideDutyDecimalPlaces(Declaration);

			CombineAssertions(() =>
			{
				var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
				AssertEquals("Precondition: entryLine1", 0, entryLine1.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();

				AssertEquals("entryLine1: Duty calculated and added to Fee", 1, entryLine1.Fees.Count);
				AssertEquals("entryLine1: OverrideDutyDecimalPlaces = 0", 18m, entryLine1.Fees[0].CF_ChargeAmount);
			});
		}

		public void TestCalculateDuties_ShouldTruncate()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = new DutyCalculatorStrategyOverrideShouldTruncate(Declaration);

			CombineAssertions(() =>
			{
				var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
				var entryLine2 = GetEntryHeader(Declaration, "0102200000").MergedLines[0];
				AssertEquals("Precondition: entryLine1", 0, entryLine1.Fees.Count);
				AssertEquals("Precondition: entryLine2", 0, entryLine2.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();

				AssertEquals("entryLine1: Duty calculated and added to Fee", 1, entryLine1.Fees.Count);
				AssertEquals("entryLine1: ShouldTruncate = true", 17m, entryLine1.Fees[0].CF_ChargeAmount);

				AssertEquals("entryLine2.Duty calculated and added to Fee", 1, entryLine2.Fees.Count);
				AssertEquals("entryLine2: ShouldTruncate = true", -5m, entryLine2.Fees[0].CF_ChargeAmount);
			});
		}

		public void TestCalculateDuties_CanBeNegative()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = new DutyCalculatorStrategyOverrideCanBeNegative(Declaration);

			CombineAssertions(() =>
			{
				var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
				var entryLine2 = GetEntryHeader(Declaration, "0102200000").MergedLines[0];
				AssertEquals("Precondition: entryLine1", 0, entryLine1.Fees.Count);
				AssertEquals("Precondition: entryLine2", 0, entryLine2.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();

				AssertEquals("entryLine1: Duty calculated and added to Fee", 1, entryLine1.Fees.Count);
				AssertEquals("entryLine1: CanBeNegative = true", 17.60m, entryLine1.Fees[0].CF_ChargeAmount);

				AssertEquals("entryLine2.Duty calculated and added to Fee", 1, entryLine2.Fees.Count);
				AssertEquals("entryLine2: CanBeNegative = true", -5.00m, entryLine2.Fees[0].CF_ChargeAmount);
			});
		}

		public void TestCalculateDuties_QuestionAndAnswer()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = new DutyCalculatorStrategy(Declaration);
			var strategyType = dutyCalculatorStrategy.GetType();
			var calculateMethod = strategyType.GetMethod("Calculate", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(IUniversalRateCalcData), typeof(ZString), typeof(int), typeof(ZString), typeof(ZDecimal) }, null);
			var getEntryLineUniversalDataMethod = strategyType.GetMethod("GetEntryLineUniversalData", BindingFlags.Instance | BindingFlags.NonPublic);

			var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
			var entryLine3 = GetEntryHeader(Declaration, "0103300000").MergedLines[0];
			ZString formulaSpecificQuestion = "User Enter Value";
			ZDecimal formulaSpecificValue = 60m;

			CombineAssertions(() =>
			{
				var universalData = getEntryLineUniversalDataMethod.Invoke(dutyCalculatorStrategy, new object[] { entryLine1 });
				var amount = calculateMethod.Invoke(dutyCalculatorStrategy, new object[] { universalData, (ZString)"VFD * 0.8", 2, formulaSpecificQuestion, formulaSpecificValue });
				AssertEquals("entryLine1: no FormulaSpecificQuestion on rateformula", 17.60m, amount);

				universalData = getEntryLineUniversalDataMethod.Invoke(dutyCalculatorStrategy, new object[] { entryLine3 });
				amount = calculateMethod.Invoke(dutyCalculatorStrategy, new object[] { universalData, (ZString)@"({DECIMAL(6,3):""User Enter Value""})*0.1", 2, formulaSpecificQuestion, formulaSpecificValue });
				AssertEquals("entryLine3: has FormulaSpecificQuestion = (60m)*0.1", 6.00m, amount);
			});
		}

		public void TestCalculateDuties_ShouldCalculateDutiesByDefault()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = new DutyCalculatorStrategy(Declaration);

			CombineAssertions(() =>
			{
				var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
				AssertEquals("Precondition: entryLine1", 0, entryLine1.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();

				AssertEquals("ShouldCalculateDuties = false and not calculate", 0, entryLine1.Fees.Count);
			});
		}

		public void TestCalculateDuties_EntryLineFeeType()
		{
			SetupReferenceData();
			var dutyCalculatorStrategy = new DutyCalculatorStrategyOverrideEntryLineFeeType(Declaration);

			CombineAssertions(() =>
			{
				var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
				AssertEquals("Precondition: entryLine1", 0, entryLine1.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();

				AssertEquals("entryLine1: Duty calculated and added to Fee", 1, entryLine1.Fees.Count);
				AssertEquals("entryLine1: Calculated", 17.60m, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("entryLine1: EntryLineFeeType", "DTY", entryLine1.Fees[0].CF_ChargeType);
			});
		}

		public void TestCalculateValueForVAT()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 500m;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 300m;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				AssertEquals("invoiceLine1.JI_Calc_ValueForVat", 200m, invoiceLine1.JI_Calc_ValueForVat);
				AssertEquals("invoiceLine2.JI_Calc_ValueForVat", 300m, invoiceLine2.JI_Calc_ValueForVat);
				var entryLine = invoiceLine1.CusEntryLine;
				AssertEquals("CL_ValueForVAT is calculated", 500m, entryLine.CL_ValueForVAT);
			});
		}

		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategyOverrideShouldCalculateDutiesByDefault(Declaration);

		protected override BaseJobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			CreateEntryLineAndInvoiceLine(declaration, TariffCode_0101100000, 22m);
			CreateEntryLineAndInvoiceLine(declaration, TariffCode_0102200000, -10m);
			CreateEntryLineAndInvoiceLine(declaration, TariffCode_0103300000, 100m);
			CreateEntryLineAndInvoiceLine(declaration, TariffCode_0104400000, 100m);
			CreateEntryLineAndInvoiceLine(declaration, "XXX", 100m);

			return declaration;
		}

		protected override bool ExpectedShouldCalculateDuties => true;

		sealed class DutyCalculatorStrategyWithOverrideDutyDecimalPlaces : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyWithOverrideDutyDecimalPlaces(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override int DutyDecimalPlace => 0;

			protected override bool ShouldCalculateDuties => true;
		}

		sealed class DutyCalculatorStrategyOverrideShouldTruncate : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyOverrideShouldTruncate(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override bool ShouldTruncate => true;

			protected override bool CanBeNegative => true;

			protected override bool ShouldCalculateDuties => true;
		}

		sealed class DutyCalculatorStrategyOverrideCanBeNegative : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyOverrideCanBeNegative(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override bool CanBeNegative => true;

			protected override bool ShouldCalculateDuties => true;
		}

		sealed class DutyCalculatorStrategyOverrideEntryLineFeeType : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyOverrideEntryLineFeeType(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override ZString GetEntryLineFeeType(RateView rate) => "DTY";

			protected override bool ShouldCalculateDuties => true;
		}

		sealed class DutyCalculatorStrategyOverrideShouldCalculateDutiesByDefault : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyOverrideShouldCalculateDutiesByDefault(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override bool ShouldCalculateDuties => true;

			protected override IEnumerable<IZZRateSelectionCriteria> GetRateSelectionCriteria(BaseJobComInvoiceLine invLine)
			{
				yield return invLine.DutyRateSelectionCriteria;
				yield return new BaseJobComInvoiceLine.RateSelectionCriteria<BaseJobComInvoiceLine>(invLine, "EXC", "RC2");
			}
		}
	}
}
