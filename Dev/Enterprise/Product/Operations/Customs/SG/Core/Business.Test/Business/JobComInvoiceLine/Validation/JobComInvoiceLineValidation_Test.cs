using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineValidation_Test : BusinessObjectValidationTestCase
	{
		public void TestLineNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertEquals("just checking", (ZShort)1, invoiceLine.JI_LineNo);
			for (int i = 1; i < 50; i++)
			{
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Description = "line number " + i.ToString();
			}

			AssertEquals("just checking", (ZShort)50, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("just checking", (ZShort)51, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasErrors());
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertHasMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals((ZShort)52, invoiceLine.JI_LineNo);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals((ZShort)53, invoiceLine.JI_LineNo);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasWarnings());
			AssertNoWarning(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.CheckEntryLineLimitForDeclaration);
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "line number 53";
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasWarnings());
			AssertHasWarning("When merging lines, message provided on invoice line should be a warning", invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.CheckEntryLineLimitForDeclaration);
		}

		public void TestCheckJI_LineNo_MaxForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			AssertEquals("JI_LineNo", (ZShort)1, invoiceLine.JI_LineNo);
			for (int i = 1; i < 50; i++)
			{
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Description = "line number " + i.ToString();
			}

			AssertEquals("JI_LineNo", (ZShort)50, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("JI_LineNo", (ZShort)51, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasErrors());
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertHasMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals((ZShort)52, invoiceLine.JI_LineNo);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasErrors());
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals((ZShort)53, invoiceLine.JI_LineNo);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasWarnings());
			AssertNoWarning(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.CheckEntryLineLimitForDeclaration);
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "line number 53";
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasWarnings());
			AssertHasWarning(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.CheckEntryLineLimitForDeclaration);
		}

		public void TestCheckJI_LineNo_MaxWithMultipleInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV-1";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV-2";
			var invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			AssertEquals("JI_LineNo", (ZShort)1, invoiceLine.JI_LineNo);
			for (int i = 1; i < 30; i++)
			{
				invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Description = "Inv1 - line number " + i.ToString();
			}

			AssertEquals("JI_LineNo", (ZShort)30, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			for (int i = 1; i < 17; i++)
			{
				invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Description = "Inv2 - line number " + i.ToString();
			}

			for (int i = 17; i < 21; i++)
			{
				invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			}

			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			AssertEquals("JI_LineNo", (ZShort)21, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertHasMessageError("For all declarations, maximum lines is 50 regardless of different invoices", invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			AssertEquals((ZShort)22, invoiceLine.JI_LineNo);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoMessageError("For all declarations, maximum lines is 50 regardless of different invoices", invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			AssertEquals((ZShort)23, invoiceLine.JI_LineNo);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasWarnings());
			AssertNoWarning(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.CheckEntryLineLimitForDeclaration);
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "new line 1";
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "new line 2";
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "new line 3";
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "new line 4";
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "new line 5";
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasWarnings());
			AssertHasWarning(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.CheckEntryLineLimitForDeclaration);
		}

		public void TestCountryOfOrigin()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			InvoiceLine.JI_JZ = invoiceHeader.PK;
			Validation.ValidateJI_CountryOfOrigin();
			AssertEquals(true, InvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());
			AssertNoWarning(InvoiceLine.JI_CountryOfOriginInfo, JobDeclarationValidation.Circular18_2010);
			InvoiceLine.JI_CountryOfOrigin = "ZZ";
			Validation.ValidateJI_CountryOfOrigin();
			AssertEquals(true, InvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());
			AssertNoWarning(InvoiceLine.JI_CountryOfOriginInfo, JobDeclarationValidation.Circular18_2010);
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			Validation.ValidateJI_CountryOfOrigin();
			AssertEquals(false, InvoiceLine.JI_CountryOfOriginInfo.HasMessageErrors());
			AssertNoWarning(InvoiceLine.JI_CountryOfOriginInfo, JobDeclarationValidation.Circular18_2010);
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaNorth;
			AssertHasWarning(InvoiceLine.JI_CountryOfOriginInfo, JobDeclarationValidation.Circular18_2010);
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Iran;
			AssertHasWarning(InvoiceLine.JI_CountryOfOriginInfo, JobDeclarationValidation.Circular18_2010);
			InvoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaNorth;
			AssertNoWarning("Warning should not be shown if declaration has already cleared TradeNet", InvoiceLine.JI_CountryOfOriginInfo, JobDeclarationValidation.Circular18_2010);
		}

		public void TestCountryOfOriginWhenCertificateOfOrigin()
		{
			var warningMessage = "When declaring a Certificate of Origin, the Origin must be Singapore (SG), except for Certificate Types 3, 17, 22, 26, 28 & 30.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			declaration.SG_Cert1Type = "16";
			var invoiceHeader = declaration.Invoices.AddNew();
			InvoiceLine.JI_JZ = invoiceHeader.PK;
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Singapore;
			AssertNoWarning(InvoiceLine.JI_CountryOfOriginInfo, warningMessage);
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Thailand;
			AssertHasWarning(InvoiceLine.JI_CountryOfOriginInfo, warningMessage);
			declaration.SG_Cert1Type = "28";
			Validation.ValidateJI_CountryOfOrigin();
			AssertNoWarning("Country of Origin does not need to be SG for this Certificate type", InvoiceLine.JI_CountryOfOriginInfo, warningMessage);
		}

		public void TestGoodsDescription()
		{
			Validation.ValidateJI_Description();
			AssertEquals(true, InvoiceLine.JI_DescriptionInfo.HasMessageErrors());
			InvoiceLine.JI_Description = "TEST";
			Validation.ValidateJI_Description();
			AssertEquals(false, InvoiceLine.JI_DescriptionInfo.HasMessageErrors());
		}

		public void TestCheckJI_Tariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			InvoiceLine.JI_JZ = invoiceHeader.PK;
			InvoiceLine.JI_Tariff = "12345678";
			Validation.ValidateJI_Tariff();
			AssertHasMessageError("Tariff does not exist", invoiceLine.JI_TariffInfo, "The tariff code cannot be found in the customs tariff code list.");
			var testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "12345678", ZDateTime.MinSmallDateTimeValue, ZDateTime.Now.AddYears(-1), "Test Tariff - expired", compositeKey: "HC00000001");
			Factory.Save();
			var formatedTariff = new TariffFormatter().Format(testTariff.ZZ1_TariffCode);
			Factory.ClearQueryCache();
			Factory.ClearCachedValue<TariffView>(string.Join("_", "LoadLatestCachedTariff", Core.Constants.CountryCodes.Singapore, tariffType.ZZI_TariffType, "12345678"));
			InvoiceLine.JI_Tariff = "12345678";
			Validation.ValidateJI_Tariff();
			AssertHasMessageError("Tariff has expired", InvoiceLine.JI_TariffInfo, "This Tariff is no longer valid for use as it has expired.");
			testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "23456781", ZDateTime.Now.AddYears(1), ZDateTime.Now.AddYears(5), "Test Tariff - not yet implemented", compositeKey: "HC00000002");
			Factory.Save();
			InvoiceLine.JI_Tariff = "23456781";
			Validation.ValidateJI_Tariff();
			AssertHasMessageError("Tariff has not yet come into implementation", InvoiceLine.JI_TariffInfo, string.Format("This Tariff is not yet valid for use. It does not become active until {0}.", testTariff.ZZ1_StartDate.ToLongTimeString()));
			testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "34567812", ZDateTime.Now.AddYears(-1), ZDateTime.Now.AddYears(1), "Test Tariff - valid", compositeKey: "HC00000003");
			Factory.Save();
			InvoiceLine.JI_Tariff = "01029010";
			Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageErrors());
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			InvoiceLine.JI_Tariff = "34567812";
			Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageErrors());
			InvoiceLine.JI_PartNo = "NEWPART";
			InvoiceLine.JI_Tariff = "";
			InvoiceLine.JI_CC = ZGuid.Empty;
			Validation.ValidateJI_Tariff();
			AssertHasWarning("Tariff has warning", InvoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			InvoiceLine.JI_Tariff = "8008";
			Validation.ValidateJI_Tariff();
			AssertNoWarning("Tariff has warning", InvoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			var classification = Factory.New<Classification>();
			classification.CC_LookupCode = "NEWCODE";
			classification.CC_IsActive = true;
			Factory.Save();
			InvoiceLine.JI_CC = classification.PK;
			InvoiceLine.JI_Tariff = ZString.Empty;
			Validation.ValidateJI_Tariff();
			AssertNoWarning("Tariff has warning", InvoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
		}

		public void TestTariffCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			InvoiceLine.JI_JZ = invoiceHeader.PK;
			Validation.ValidateJI_Tariff();
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasMessageErrors());
			InvoiceLine.JI_Tariff = "00000000";
			Validation.ValidateJI_Tariff();
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasMessageErrors());
			InvoiceLine.JI_Tariff = "01029010";
			CusLineTariffDetail lineTariffDetail = InvoiceLine.ProductCodes.AddNew();
			lineTariffDetail.BZ_Tariff = "TEST";
			Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageErrors());
			AssertEquals(true, InvoiceLine.ProductCodes[0].BZ_TariffInfo.HasWarnings());
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarnings());
			InvoiceLine.JI_Tariff = "01039200";
			Validation.ValidateJI_Tariff();
			AssertEquals(true, InvoiceLine.ProductCodes[0].BZ_TariffInfo.HasWarnings());
			InvoiceLine.JI_Tariff = "01039200";
			Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarnings());
			InvoiceLine.JI_Tariff = "05079010";
			Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarnings());
			InvoiceLine.JI_Tariff = "22030090";
			Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarnings());
			InvoiceLine.JI_Tariff = "42010000";
			Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarnings());
			InvoiceLine.ProductCodes.RemoveAndDeleteAll();
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			InvoiceLine.JI_Tariff = "03011199";
			Validation.ValidateJI_Tariff();
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasWarnings());
			CusLineTariffDetail productCode = InvoiceLine.ProductCodes.AddNew();
			productCode.BZ_Tariff = "FFO0SK1SHAR";
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarnings());
		}

		public void TestTariffsUnderControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			InvoiceLine.JI_JZ = invoiceHeader.PK;
			InvoiceLine.JI_Tariff = "01012100";
			Validation.ValidateJI_Tariff();
			AssertHasWarning("This Tariff is under Import Control", InvoiceLine.JI_TariffInfo, "This Tariff item appears to be a controlled HS Code for Import declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
			var productCode = InvoiceLine.ProductCodes.AddNew();
			productCode.BZ_Tariff = "VBA0HO";
			AssertNoWarning("This Tariff is under Import Control - product code now entered", InvoiceLine.JI_TariffInfo, "This Tariff item appears to be a controlled HS Code for Import declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
			InvoiceLine.ProductCodes.RemoveAndDeleteAll();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.JI_Tariff = "16053000";
			AssertHasWarning("This Tariff is under Export Control", InvoiceLine.JI_TariffInfo, "This Tariff item appears to be a controlled HS Code for Export declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
			productCode = InvoiceLine.ProductCodes.AddNew();
			productCode.BZ_Tariff = "FCN0LX";
			AssertNoWarning("This Tariff is under Export Control - product code now entered", InvoiceLine.JI_TariffInfo, "This Tariff item appears to be a controlled HS Code for Export declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
			InvoiceLine.ProductCodes.RemoveAndDeleteAll();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			InvoiceLine.JI_Tariff = "25174900";
			AssertHasWarning("This Tariff is under Transhipment Control", InvoiceLine.JI_TariffInfo, "This Tariff item appears to be a controlled HS Code for Transhipment/Movement declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
			productCode = InvoiceLine.ProductCodes.AddNew();
			productCode.BZ_Tariff = "BCAOTHERG3";
			AssertNoWarning("This Tariff is under Transhipment Control - product code now entered", InvoiceLine.JI_TariffInfo, "This Tariff item appears to be a controlled HS Code for Transhipment/Movement declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
		}

		public void TestInvoiceQuantity()
		{
			InvoiceLine.JI_InvoiceQuantity = -1m;
			Validation.ValidateJI_InvoiceQuantity();
			AssertEquals(true, InvoiceLine.JI_InvoiceQuantityInfo.HasErrors());
			InvoiceLine.JI_InvoiceQuantity = 100m;
			Validation.ValidateJI_InvoiceQuantity();
			AssertEquals(false, InvoiceLine.JI_InvoiceQuantityInfo.HasNotifications());
		}

		public void TestInvoiceUQ()
		{
			InvoiceLine.JI_InvoiceUQ = "ABC";
			Validation.ValidateJI_InvoiceUQ();
			AssertEquals(true, InvoiceLine.JI_InvoiceUQInfo.HasMessageErrors());
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateJI_InvoiceUQ();
			AssertEquals(false, InvoiceLine.JI_InvoiceUQInfo.HasMessageErrors());
		}

		public void TestCustomsQuantity()
		{
			Validation.ValidateJI_CustomsQuantity();
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.HasMessageErrors());
			InvoiceLine.JI_CustomsQuantity = 100m;
			Validation.ValidateJI_CustomsQuantity();
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.HasMessageErrors());
		}

		public void TestCustomsUnitQty()
		{
			InvoiceLine.JI_CustomsUnitQty = "ABC";
			Validation.ValidateJI_CustomsUnitQty();
			AssertEquals(true, InvoiceLine.JI_CustomsUnitQtyInfo.HasMessageErrors());
			InvoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.CEN;
			Validation.ValidateJI_CustomsUnitQty();
			AssertEquals(false, InvoiceLine.JI_CustomsUnitQtyInfo.HasMessageErrors());
		}

		public void TestCustomsLineQtyErrorsIfGreaterThanShipmentQty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalWeight = 0.776m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 0.778m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertHasWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 0.776 T - Line weight, in equivalent unit of qty: 0.778 T)");
			invoiceLine.JI_CustomsQuantity = 0.776m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 0.776 T - Line weight, in equivalent unit of qty: 0.778 T)");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_TotalWeight = 150m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine.JI_CustomsQuantity = 150m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 150 KG - Line weight, in equivalent unit of qty: 160 KG)");
			invoiceLine.JI_CustomsQuantity = 160m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertHasWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 150 KG - Line weight, in equivalent unit of qty: 160 KG)");
			declaration.JE_TotalWeight = 150m;
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.BOX;
			invoiceLine.JI_CustomsQuantity = 200m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoMessageErrors("When unit of quantities differ, validation is irrelevant", declaration.JE_TotalWeightInfo);
		}

		public void TestCustomsLineQtyErrorsIfGreaterThanShipmentQtyWhenMultipleLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalWeight = 0.776m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 0.778m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertHasWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			invoiceLine.JI_CustomsQuantity = 0.776m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			invoiceLine.JI_CustomsQuantity = 0.5m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 778.000 KG)");
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "93020000";
			invoiceLine2.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine2.JI_CustomsQuantity = 300m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertHasWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 800.0 KG)");
			invoiceLine2.JI_CustomsQuantity = 276m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 776 KG - Line weight, in equivalent unit of qty: 800.0 KG)");
		}

		public void TestCustomsLineQtyErrorsWhenShipmentWeightNeedsConversion()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalWeight = 2000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Pounds;
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "93020000";
			invoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.TNE;
			invoiceLine.JI_CustomsQuantity = 10m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertHasWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 10000 KG)");
			invoiceLine.JI_CustomsQuantity = 0.9m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 10000 KG)");
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "93020000";
			invoiceLine2.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			invoiceLine2.JI_CustomsQuantity = 4000m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertHasWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 4900.0 KG)");
			invoiceLine2.JI_CustomsQuantity = 7m;
			declaration.Validation.ValidateJE_TotalWeight();
			AssertNoWarning(declaration.JE_TotalWeightInfo, "Total Weight declared is less than the Customs Qty entered on the invoice line(s).\r\n(Total Weight, in reportable unit of qty: 907.18474 KG - Line weight, in equivalent unit of qty: 4900.0 KG)");
		}

		public void TestHazMatCodeQualifier()
		{
			InvoiceLine.JI_HazMatCodeQualifier = "A";
			Validation.ValidateJI_HazMatCodeQualifier();
			AssertEquals(true, InvoiceLine.JI_HazMatCodeQualifierInfo.HasMessageErrors());
			InvoiceLine.JI_HazMatCodeQualifier = DGIndicatorCodeList.Codes.N;
			Validation.ValidateJI_HazMatCodeQualifier();
			AssertEquals(false, InvoiceLine.JI_HazMatCodeQualifierInfo.HasMessageErrors());
		}

		public void TestLinePrice()
		{
			InvoiceLine.JI_LinePrice = -1m;
			Validation.ValidateJI_LinePrice();
			AssertEquals(true, InvoiceLine.JI_LinePriceInfo.HasErrors());
			InvoiceLine.JI_LinePrice = 0m;
			Validation.ValidateJI_LinePrice();
			AssertEquals(false, InvoiceLine.JI_LinePriceInfo.HasNotifications());
		}

		public void TestUnitPrice()
		{
			InvoiceLine.UnitPrice = -15m;
			Validation.ValidateUnitPrice();
			AssertEquals(true, InvoiceLine.UnitPriceInfo.HasErrors());
			InvoiceLine.UnitPrice = 0m;
			Validation.ValidateUnitPrice();
			AssertEquals(false, InvoiceLine.UnitPriceInfo.HasNotifications());
		}

		public void TestGrossWeight()
		{
			InvoiceLine.JI_Weight = -200m;
			InvoiceLine.JI_WeightUQ = "KG";
			Validation.ValidateJI_Weight();
			AssertEquals(true, InvoiceLine.JI_WeightInfo.HasMessageErrors());
			InvoiceLine.JI_Weight = 0m;
			Validation.ValidateUnitPrice();
			AssertEquals(false, InvoiceLine.JI_WeightInfo.HasMessageErrors());
		}

		public void TestJI_PrimaryPreference()
		{
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrors("User can blank this field out if they desire", InvoiceLine.JI_PrimaryPreferenceInfo);
			InvoiceLine.JI_PrimaryPreference = "123";
			AssertHasMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			AssertHasMessageError("PRI is code for Export", InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.STD;
			AssertNoMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			AssertNoMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			AssertHasMessageError("PRF is code for Import - not valid for Export job", InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.STD;
			AssertNoMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			AssertNoMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestProcedureCode()
		{
			var zz6 = Factory.New<RefCusProcedure>();
			zz6.ZZ6_ProcedureCode = "110";
			zz6.ZZ6_Concession = "1000";
			zz6.ZZ6_ZZZ_NKDataGrouping = "SG";
			zz6.ZZ6_ShipmentType = "IPT";
			zz6.ZZ6_Group = "GST";
			zz6.ZZ6_ZZZ_NKDataGrouping = "SG";
			zz6.ZZ6_StartDate = ZDate.Today.AddDays(-10);
			zz6.ZZ6_EndDate = ZDate.Today.AddDays(10);
			Validation.ValidateJI_Procedure();
			AssertEquals(false, InvoiceLine.JI_ProcedureInfo.HasMessageErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			helper.LoadOrCreateNewTariff(tariffType, "01029010");
			helper.LoadOrCreateNewTariff(tariffType, "01039200");
			helper.LoadOrCreateNewTariff(tariffType, "05079010");
			helper.LoadOrCreateNewTariff(tariffType, "22030090");
			helper.LoadOrCreateNewTariff(tariffType, "42010000");
			var tariff = helper.LoadOrCreateNewTariff(tariffType, "03011199");
			var com = helper.CreateCommodity(tariff, "FFO0BR1PBEU");
			helper.CreateCommodity(tariff, "FFO0SK1SHAR");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, "Y", com);
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "01012100");
			var com1 = helper.CreateCommodity(tariff1, "com1");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, "Y", com1);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "16053000");
			var com2 = helper.CreateCommodity(tariff2, "com2");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISEXPORTCONTROL, "Y", com2);
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "25174900");
			var com3 = helper.CreateCommodity(tariff3, "com3");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISTRANSHIPMENTCONTROL, "Y", com3);
			Factory.Save();

			SGCertificateTypeHelper.CreateCertificateTypes(Factory);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		RefCusTariffType tariffType;
		#region Implementation
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
		#region InvoiceHeader
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region InvoiceLine
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
		protected JobComInvoiceLineValidation Validation
		{
			get
			{
				return InvoiceLine.Validation;
			}
		}
		#endregion
	}
}
