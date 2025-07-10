using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class CommonImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCS00329831ForImportTariff()
		{
			var declaration = GetTestJobDeclaration();

			var invoiceHeader = declaration.Invoices.AddNew();
			AddInvoiceLineByLinePrice(invoiceHeader, 12.00, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 12.04, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 9.40, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 9.40, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 10.56, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 9.32, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 11.00, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 11.00, "7013.99.5000");
			AddInvoiceLineByLinePrice(invoiceHeader, 4.96, "7013.99.5000");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			foreach (var line in invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				AssertNoMessageErrorContaining(line.JI_LinePriceInfo, " > 0.3000 and <= 3.0000");
			}

			var line10 = AddInvoiceLineByLinePrice(invoiceHeader, 400, "7013.99.5000");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = line10.CusEntryLine;
			var unitPrice = ((ZDecimal)(line10.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			var errorText = string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 0.3000 and <= 3.0000", unitPrice);

			foreach (var line in invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				AssertHasMessageError("All invoice lines merged into the CusEntryLine will get a message error", line.JI_LinePriceInfo, errorText);
				AssertHasMessageErrorContaining(line.JI_LinePriceInfo, " > 0.3000 and <= 3.0000");
			}
		}

		public void TestCS00362499DerivedTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 24000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8215200000";
			invoiceLine.JI_CustomsQuantity = 90m;
			invoiceLine.JI_LinePrice = 0m;
			AssertEquals("PreCondition:Derived Computation", ComputationCodeList.Codes.Derived, invoiceLine.ImportTariff.UE_DutyComputationCode);

			var line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8215990500";
			line2.JI_CustomsQuantity = 90m;
			line2.JI_LinePrice = 505.35m;
			AssertNotNull(line2.ImportTariff);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			line2.Validation.ValidateJI_LinePrice();
			AssertNoNotifications(line2.JI_LinePriceInfo);
		}

		public void TestCS00329831ForSupTariff()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ProductForTesting";
			var declaration = GetTestJobDeclaration();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoiceHeader = declaration.Invoices.AddNew();

			var line3 = AddInvoiceLineByLinePrice(invoiceHeader, 25.00, "7013.99.5000");
			line3.JI_PartNo = product.OP_PartNum;
			line3.US_SupTariff = "7013.88.5000";
			line3.US_SupQty1 = 100;
			line3.US_SupUQ1 = "NO";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IDutyData cusEntryLine = line3.CusEntryLine.ParentLine;
			var unitPrice = ((ZDecimal)(line3.CusEntryLine.ParentLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			var errorText = string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "supplementary tariff", " > 0.5000 and <= 10.0000", unitPrice);
			AssertHasMessageError(line3.JI_LinePriceInfo, errorText);
		}

		public void TestRestrictedTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.New<OrgHeader>();
			declaration.IOROrgPK = importer.PK;
			declaration.IORWrapper.RestrictedTariffs.AddNew(RestrictedCodeTypeList.Codes.RestrictedTariff, "8703");

			var message = "is not allowed for Importer of Record. For more details please refer to Importer of Record > Details > Config > US Defaults";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8703240066";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, message);

			invoiceLine.JI_Tariff = "6202110010";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, message);
		}

		public void TestCheckCustomsQtyWeightImbalanceForEmptyGrossWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoiceLine.JI_Tariff = "4707900000";
			invoiceLine.JI_CustomsQuantity = 100m;
			//whether gross weight should be entered or not is validated in a different routine. This validation should run only when gross weight is entered
			Assert(!invoiceLine.JI_WeightInfo.HasWarning(CommonImportJobComInvoiceLineValidation.WeightImbalance));
			Assert(!invoiceLine.JI_WeightInfo.HasWarning(CommonImportJobComInvoiceLineValidation.EquivalentWeightImbalance));
		}

		public void TestCheckCustomsQtyWeightImbalance()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoiceLine.JI_Tariff = "4707900000";
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoWarning(invoiceLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.WeightImbalance);
			AssertNoWarning(invoiceLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.EquivalentWeightImbalance);

			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1250000m;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertHasWarning(invoiceLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.EquivalentWeightImbalance);

			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine.JI_CustomsUnitQty = invoiceLine.JI_WeightUQ;
			invoiceLine.JI_CustomsQuantity = 1250m;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertHasWarning(invoiceLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.WeightImbalance);

			invoiceLine.JI_CustomsQuantity = 950m;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoWarning(invoiceLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.WeightImbalance);

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			var reconInvHeader = reconDec.Invoices.AddNew();
			var reconInvLine = reconInvHeader.JobComInvoiceLines.AddNew();

			reconInvLine.JI_Tariff = "4707900000";
			reconInvLine.JI_Weight = 1000m;
			reconInvLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			reconInvLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			reconInvLine.JI_CustomsQuantity = 1250000m;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoWarning("Customs Qty / Gross Weight imbalance validation should not apply to Recon jobs.", reconInvLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.EquivalentWeightImbalance);

			var drawbackDec = Factory.New<JobDeclaration>();
			drawbackDec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			Assert(drawbackDec.IsDrawback);
			var drawbackInvHeader = drawbackDec.Invoices.AddNew();
			var drawbackInvLine = drawbackInvHeader.JobComInvoiceLines.AddNew();

			drawbackInvLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			drawbackInvLine.JI_CustomsQuantity = 10000m;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoWarning("Customs Qty / Gross Weight imbalance validation should not apply to Drawback jobs.", drawbackInvLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.WeightImbalance);

			drawbackInvLine.JI_CustomsUnitQty = Core.Constants.Weight.Tonnes;
			drawbackInvLine.JI_CustomsQuantity = 10m;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoWarning("Customs Qty / Gross Weight imbalance validation should not apply to Drawback jobs.", drawbackInvLine.JI_WeightInfo, CommonImportJobComInvoiceLineValidation.EquivalentWeightImbalance);
		}

		public virtual void TestLinePriceBoundaryForNotMergedLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			var tariff = new USCTariff.Loader(Factory).LoadBestMatch("6404119040", ZDateTime.Today);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "6404119040";
				tariff.UE_DateFrom = ZDateTime.Today.AddDays(-20);
				tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			}
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "113";
			tariffValue.UA_ValueLowBounds = 12m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1m;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			invoiceLine1.JI_LinePrice = 13m;
			invoiceLine1.JI_CustomsQuantity = 1m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IDutyData cusEntryLine = invoiceLine1.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine1.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			var errorText = string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 12.0000", unitPrice);
			AssertNoMessageErrorContaining(invoiceLine1.JI_LinePriceInfo, " > 12.0000");

			invoiceLine1.JI_CustomsQuantity = 2m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			cusEntryLine = invoiceLine1.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine1.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			errorText = string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 12.0000", unitPrice, unitPrice);
			AssertHasMessageError(invoiceLine1.JI_LinePriceInfo, errorText);
			AssertHasMessageErrorContaining(invoiceLine1.JI_LinePriceInfo, " > 12.0000");
		}

		protected void AssertJI_CustomsSecondQuantity(JobComInvoiceLine invoiceLine)
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_Unit2 = "LTR";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsSecondQuantity = -1m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.NegativeAmountNotAllowed);

			var childInvoiceLine = invoiceLine.InvoiceHeader.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = invoiceLine.PK;
			childInvoiceLine.JI_Tariff = tariff.UE_Tariff;
			childInvoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertHasWarning(childInvoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);
		}

		protected void AssertMIDAgainstCountryOfOrigin(JobComInvoiceLine invoiceLine)
		{
			USCRule rule = Factory.New<USCRule>();
			rule.U0_Code = TariffRuleList.Codes.TextileEntryMID;

			USCTariffRule tariff_rule = Factory.New<USCTariffRule>();
			tariff_rule.U1_RuleCode = TariffRuleList.Codes.TextileEntryMID;
			tariff_rule.U1_Tariff = "6110202079";
			tariff_rule.U1_DateFrom = new ZDate(2000, 1, 1);
			tariff_rule.U1_DateTo = ZDateTime.Today.AddDays(1);

			USCTariff tariff_textile = Factory.New<USCTariff>();
			tariff_textile.UE_Tariff = "6110202079";
			tariff_textile.UE_DateFrom = new ZDate(2005, 1, 1);
			tariff_textile.UE_DateTo = ZDateTime.Today.AddDays(1);

			var party = Factory.New<OrgHeader>();

			var mainAddress = party.MainAddress;
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.JI_Tariff = "6110.20.2079";
			AssertNoMessageError("For a Textile Entry the MID must match the Country of Origin.", invoiceLine.JI_TariffInfo, CommonImportJobComInvoiceLineValidation.MIDNotMatchCountry);

			cusCode.OK_CustomsRegNo = "US34567";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("For a Textile Entry the MID does not must match the Country of Origin.", invoiceLine.JI_TariffInfo, CommonImportJobComInvoiceLineValidation.MIDNotMatchCountry);

			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			cusCode.OK_CustomsRegNo = "XC34567934JT";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarning("For a Textile Entry when Canadian provences there should be a warning if the MID does not match the province of origin.", invoiceLine.JI_TariffInfo, CommonImportJobComInvoiceLineValidation.MIDNotMatchProvence);

			cusCode.OK_CustomsRegNo = "XO34567934JT";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning("For a Textile Entry when Canadian provences there should be a warning if the MID does not match the province of origin.", invoiceLine.JI_TariffInfo, CommonImportJobComInvoiceLineValidation.MIDNotMatchProvence);
		}

		protected void AssertJI_OA_ManufacturerAddressForMID(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			string messageError = string.Format(OrganisationValidation.ManufacturerIDMissing, party.MainAddress.OA_Code);
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);

			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);
		}

		protected void AssertJI_OA_ManufacturerAddressForMIDAgainstCanadianProvince(JobComInvoiceLine invoiceLine)
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;

			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;

			cusCode.OK_CustomsRegNo = "AU34567";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XY;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);

			cusCode.OK_CustomsRegNo = "XO1";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
		}

		JobDeclaration GetTestJobDeclaration()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7013995000";
			tariff.UE_Unit1 = "NO";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "7013885000";
			tariff2.UE_Unit1 = "NO";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariffValue = Factory.New<USCTariffValue>();
			tariffValue.UA_UE = tariff.PK;
			tariffValue.UA_ValueEditCode = "111";
			tariffValue.UA_ValueLowBounds = 0.30000;
			tariffValue.UA_ValueHighBounds = 3.00000;

			var tariffValue2 = Factory.New<USCTariffValue>();
			tariffValue2.UA_UE = tariff2.PK;
			tariffValue2.UA_ValueEditCode = "111";
			tariffValue2.UA_ValueLowBounds = 0.50000;
			tariffValue2.UA_ValueHighBounds = 10.00000;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EstimatedEntryDate = ZDateTime.Now;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			return declaration;
		}

		JobComInvoiceLine AddInvoiceLineByLinePrice(JobComInvoiceHeader header, ZDecimal linePrice, string tariff)
		{
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test1";
			invoiceLine.JI_FormattedTariff = tariff;
			invoiceLine.JI_CustomsQuantity = 4;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_LinePrice = linePrice;
			return invoiceLine;
		}
	}
}
