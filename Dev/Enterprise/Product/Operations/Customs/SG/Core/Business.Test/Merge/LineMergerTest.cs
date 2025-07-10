using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		#region Duty/Excise
		[TestDate(2014, 05, 01)]
		public void TestCalculateExcise_PerUnitRate()
		{
			AssertDutyAmount(MessageTypeCodeList.Codes.INP, "22042111", 5338.96m, 0m);
		}

		public void TestCalculateExcise_PercentageRate()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			InvoiceLine.JI_Tariff = "87033121";
			InvoiceLine.JI_LinePrice = 1000m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = InvoiceLine.JI_Tariff;
			invoiceLine1.JI_LinePrice = 500m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals(311.17m, entryLine.ExciseAmount);
		}

		[TestDate(2014, 05, 01)]
		public void TestCalculateDutyAndExcise_PerUnitRate()
		{
			AssertDutyAmount(MessageTypeCodeList.Codes.INP, "22030010", 3640.20m, 970.72m);
		}

		public void TestCalculateDutyIsPreferredCountry()
		{
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			InvoiceLine.JI_Tariff = "87032357";
			InvoiceLine.SG_LastSellingPrice = 1000m;
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals(0m, entryLine.ExciseAmount);
			AssertEquals(0m, entryLine.CL_DutyPercent);
		}

		[TestDate(2012, 1, 31)]
		public void TestCalculateDutyIsPreferredCountry_ExciseStillApplies()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DUT;
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			InvoiceLine.JI_Tariff = "22030090";
			InvoiceLine.JI_CustomsQuantity = 9000m;
			InvoiceLine.JI_CustomsUnitQty = "LTR";
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 0.375;
			InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = "LTR";
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 9000m;
			InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "LTR";
			InvoiceLine.SG_PercAlcohol = 6.5m;
			AssertEquals("pre condition", 16m, InvoiceLine.SG_DutyUnitRate);
			AssertEquals("pre condition", 48m, InvoiceLine.SG_ExciseUnitRate);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(28080m, InvoiceLine.JI_Calc_ExciseAmount);
			AssertEquals(1965.60m, InvoiceLine.JI_Calc_GSTVATAmount);
			AssertEquals(0m, InvoiceLine.JI_Calc_DutyAmount);
		}

		public void TestCalculateDuty_NonDutyDeclarationType()
		{
			AssertDutyAmount(MessageTypeCodeList.Codes.OUT, "22030010", 0m, 0m);
		}

		void AssertDutyAmount(string messageType, string tariffNumber, ZDecimal expectedExcise, ZDecimal expectedDuty)
		{
			Declaration.JE_MessageType = messageType;
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffNumber;
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 20.52m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = invoiceLine.JI_Tariff;
			invoiceLine1.SG_TotalDutiableWGTVOLQTY = 40.15;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals(expectedExcise, entryLine.ExciseAmount);
			AssertEquals(expectedDuty, entryLine.DutyAmount);
			if (expectedDuty > 0)
			{
				var tariff = UniversalReferenceDataHelper.LoadBestMatch(Factory, tariffNumber, invoiceLine1.EffectiveDateForDutyRate);
				var rate = tariff.GetDutyRate(invoiceLine);
				AssertEquals(rate.UnitRate, entryLine.CL_FlatAmount);
				var expectedUQ = rate.UnitQty == SGConstants.LPA ? UnitOfQuantityCodeList.Codes.LTR : rate.UnitQty.ToString();
				AssertEquals(expectedUQ, entryLine.CL_FlatAmountUQ);
			}
			else
			{
				AssertEquals(0m, entryLine.CL_FlatAmount);
				AssertEquals("", entryLine.CL_FlatAmountUQ);
				AssertEquals(0m, entryLine.CL_DutyPercent);
			}
		}

		[TestDate(2013, 01, 01)]
		public void TestExciseTobaccoMultiplierApplied()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "24011020";
			AssertEquals("Pre-condition: current Excise Rate", 347.00m, invoiceLine.SG_ExciseUnitRate);
			invoiceLine.SG_TobaccoMultiplier = 3;
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 20.52m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = invoiceLine.JI_Tariff;
			AssertEquals("Pre-condition: current Excise Rate", 347.00m, invoiceLine1.SG_ExciseUnitRate);
			invoiceLine1.SG_TobaccoMultiplier = 2;
			invoiceLine1.SG_TotalDutiableWGTVOLQTY = 40.15;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			CusEntryLine entryLine1 = entryHeader.MergedLines[1];
			AssertEquals(21361.32m, entryLine.ExciseAmount);
			AssertEquals(27864.10m, entryLine1.ExciseAmount);
		}

		[TestDate(2013, 03, 01)]
		public void TestTobaccoExcise()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "24011020";
			AssertEquals("Excise Rate for this Tariff between Feb 2013 & Feb 2018", 352.00m, invoiceLine.SG_ExciseUnitRate);
		}

		[TestDate(2018, 02, 22)]
		public void TestExciseTobaccoMultiplierApplied3()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "24011020";
			AssertEquals("Pre-condition: current Excise Rate", 388.00m, invoiceLine.SG_ExciseUnitRate);
			invoiceLine.SG_TobaccoMultiplier = 3;
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 20.52m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = invoiceLine.JI_Tariff;
			AssertEquals("Pre-condition: current Excise Rate", 388.00m, invoiceLine1.SG_ExciseUnitRate);
			invoiceLine1.SG_TobaccoMultiplier = 2;
			invoiceLine1.SG_TotalDutiableWGTVOLQTY = 40.15;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			CusEntryLine entryLine1 = entryHeader.MergedLines[1];
			AssertEquals(23885.28m, entryLine.ExciseAmount);
			AssertEquals(31156.4m, entryLine1.ExciseAmount);
		}

		public void TestExcisePercAlcoholApplied()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "22082020";
			invoiceLine.SG_PercAlcohol = 33.123m;
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 20.52m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = invoiceLine.JI_Tariff;
			invoiceLine1.SG_PercAlcohol = 50m;
			invoiceLine1.SG_TotalDutiableWGTVOLQTY = 40.15;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			CusEntryLine entryLine1 = entryHeader.MergedLines[1];
			AssertEquals(0m, entryLine.ExciseAmount);
			AssertEquals(0m, entryLine1.ExciseAmount);
		}

		[TestDate(2014, 05, 01)]
		public void TestDutyPercAlcoholApplied()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceLine invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "22030010";
			invoiceLine.SG_PercAlcohol = 5.5m;
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 100m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = invoiceLine.JI_Tariff;
			invoiceLine1.SG_PercAlcohol = 10m;
			invoiceLine1.SG_TotalDutiableWGTVOLQTY = 200;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			CusEntryLine entryLine1 = entryHeader.MergedLines[1];
			AssertEquals(88M, entryLine.DutyAmount);
			AssertEquals(320M, entryLine1.DutyAmount);
		}

		#endregion

		#region GST
		public void TestGSTWhenLSPUsed()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Declaration.SG_SupplyIndicator = "Y";
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			InvoiceLine.JI_Tariff = "87032393";
			InvoiceLine.JI_LinePrice = 1000m;
			InvoiceLine.SG_LastSellingPrice = 1570m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("GST Calc should be based on LSP rather than CustomsValue (CIF)", 109.90m, entryLine.GSTVATAmount);
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "87029099";
			invoiceLine2.JI_LinePrice = 15000m;
			invoiceLine2.SG_LastSellingPrice = 18500m;
			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "87032393";
			invoiceLine3.JI_LinePrice = 1000m;
			invoiceLine3.SG_LastSellingPrice = 1570m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			var entryLine2 = entryHeader.MergedLines[1];
			AssertEquals("GST Calc should be based on LSP rather than CustomsValue (CIF)", 219.80m, entryLine.GSTVATAmount);
			AssertEquals("GST Calc should be based on LSP rather than CustomsValue (CIF)", 1295m, entryLine2.GSTVATAmount);
		}

		public void TestGSTForExemptPresidentCode()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ExemptPlaceCodePresident;
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			InvoiceLine.JI_Tariff = "87032393";
			InvoiceLine.JI_LinePrice = 1000m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("GST Calc should be based soley on CustomsValue (CIF - 1055.87) for 'President' SGCPlace of Receipt Code", 73.91m, entryLine.GSTVATAmount);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			AssertEquals("GST Calc should be based soley on CustomsValue (CIF - 1055.87) for 'President' SGCPlace of Receipt Code", 73.91m, entryLine.GSTVATAmount);
		}

		public void TestGSTWhenLSPUsedAndDutyExemptionClaimed()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Declaration.SG_DutyExempt = true;
			Declaration.SG_SupplyIndicator = "Y";
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.FOB;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceLine.JI_Tariff = "22042111";
			InvoiceLine.JI_LinePrice = 1000m;
			InvoiceLine.SG_PercAlcohol = 75m;
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 40.15;
			InvoiceLine.SG_LastSellingPrice = 2550m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals("Duty Exemption GST Calc - excise should still be calculated", 2649.90m, entryLine.ExciseAmount);
			AssertEquals("Duty Exemption GST Calc should be based on LSP rather than CustomsValue (CIF)", 178.5m, entryLine.GSTVATAmount);
		}

		public void TestGSTWhenDutyExemptionClaimed()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Declaration.SG_DutyExempt = true;
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.FOB;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceLine.JI_Tariff = "22042111";
			InvoiceLine.JI_LinePrice = 1000m;
			InvoiceLine.SG_PercAlcohol = 75m;
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 40.15;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals("Duty Exemption GST Calc - excise should still be calculated", 2649.90m, entryLine.ExciseAmount);
			AssertEquals("Duty Exemption GST Calc should not include excise", 70m, entryLine.GSTVATAmount);
		}

		public void TestGSTWhenDutyExemptionClaimed_GTR()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.FOB;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceLine.JI_Tariff = "22042111";
			InvoiceLine.JI_LinePrice = 144m;
			InvoiceLine.SG_PercAlcohol = 11.6m;
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 27m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("Duty Exemption GST Calc - excise should still be calculated", 275.62m, entryLine.ExciseAmount);
			AssertEquals("Duty Exemption GST Calc should not include excise", 10.08m, entryLine.GSTVATAmount);
		}

		public void TestCalculateGST()
		{
			AssertGST(MessageTypeCodeList.Codes.INP, 130.69m);
		}

		public void TestCalculateGST_NonGSTDeclarationType()
		{
			AssertGST(MessageTypeCodeList.Codes.OUT, 0m);
		}

		void AssertGST(string messageType, ZDecimal expectedGST)
		{
			Declaration.JE_MessageType = messageType;
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.SouthAfrica);
			InvoiceLine.JI_Tariff = "87032311";
			InvoiceLine.JI_LinePrice = 1000m;
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = InvoiceLine.JI_Tariff;
			invoiceLine1.JI_LinePrice = 500m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals(expectedGST, entryLine.GSTVATAmount);
		}

		#endregion

		#region Other
		public void TestCalculateOther()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceLine.JI_Tariff = "87033121";
			InvoiceLine.JI_LinePrice = 1000m;
			invoiceLine.SG_OtherTaxPercentageRate = 8.5m;
			invoiceLine.SG_OtherTaxUnitRate = 5m;
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 125m;
			var invoiceLineWithLiquor = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineWithLiquor.JI_Tariff = "22030010";
			invoiceLineWithLiquor.SG_PercAlcohol = 10m;
			invoiceLineWithLiquor.JI_LinePrice = 1000m;
			invoiceLineWithLiquor.SG_OtherTaxPercentageRate = 8.5m;
			invoiceLineWithLiquor.SG_OtherTaxUnitRate = 5m;
			invoiceLineWithLiquor.SG_TotalDutiableWGTVOLQTY = 125m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineWithLiquor = entryHeader.MergedLines[1];
			AssertEquals("OtherTaxAmount = (1000 * (8.5 / 100)) + (125 * 5)", 710.00m, entryLine.OtherTaxAmount);
			AssertEquals("OtherTaxAmount = (1000 * (8.5 / 100)) + (125 * 5 * (10 / 100))", 147.50m, entryLineWithLiquor.OtherTaxAmount);
		}

		#endregion

		public void TestLineNumbersAreAssigned()
		{
			InvoiceHeader.JZ_InvoiceNumber = "10";
			InvoiceLine.JI_Tariff = "87032357";
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals((short)1, entryHeader.CH_HighestLineNumber);
			AssertEquals((short)1, entryLine.CL_LineNumber);
			JobComInvoiceLine invoiceLine1 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "12345678";
			Declaration.DoMerge();
			CusEntryLine entryLine1 = entryHeader.MergedLines[1];
			AssertEquals((short)2, entryHeader.CH_HighestLineNumber);
			AssertEquals((short)1, entryLine.CL_LineNumber);
			AssertEquals((short)2, entryLine1.CL_LineNumber);
		}

		#region Declaration / InvoiceHeader / InvoiceLine
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MergeBy = "TRF";
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			var refCurrecny = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.SouthAfrica);
			refCurrecny.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.558659m);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "Goods and Services Tax");
			var stdPreference = helper.CreatePreferenceForCountryAndGrouping(PreferentialIndicatorCodeList.Codes.STD, "STANDARD", Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore);
			var prfPreference = helper.CreatePreferenceForCountryAndGrouping(PreferentialIndicatorCodeList.Codes.PRF, "PRF", Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Singapore, "All Countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "22030010");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff1);
			helper.CreateTariffExciseRate(tariff1, 60, SGConstants.LPA);
			helper.CreateDutyRate(tariff1, stdPreference, 16, SGConstants.LPA);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "22030090");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff2);
			helper.CreateTariffExciseRate(tariff2, 48, SGConstants.LPA);
			helper.CreateDutyRate(tariff2, prfPreference, 16, SGConstants.LPA);
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "87033121");
			helper.CreateTariffExciseRate(tariff3, 20);
			var tariff4 = helper.LoadOrCreateNewTariff(tariffType, "22042111");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff4);
			helper.CreateTariffExciseRate(tariff4, 88, SGConstants.LPA);
			var tariff5 = helper.LoadOrCreateNewTariff(tariffType, "87032311");
			helper.CreateTariffExciseRate(tariff5, 20);
			var tariff6 = helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "24011020", new ZDateTime(2012, 2, 17), new ZDateTime(2013, 2, 15));
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff6);
			helper.CreateTariffExciseRate(tariff6, 347, SGConstants.Weight.Kilograms);
			var tariff7 = helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "24011020", new ZDateTime(2018, 2, 19), new ZDateTime(2018, 6, 23));
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff7);
			helper.CreateTariffExciseRate(tariff7, 388, SGConstants.Weight.Kilograms);
			var tariff8 = helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "24011020", new ZDateTime(2013, 2, 25), new ZDateTime(2018, 2, 19));
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff8);
			helper.CreateTariffExciseRate(tariff8, 352, SGConstants.Weight.Kilograms);
			Factory.Save();
		}
		#endregion

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);
	}
}
