using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZJobComInvoiceLineValidationTest : CommonImportJobComInvoiceLineValidationTest
	{
		public void TestCheckJI_CustomsSecondQuantity()
		{
			AssertJI_CustomsSecondQuantity(invoiceLine);
		}

		public void TestCheckJI_LinePrice()
		{
			invoiceLine.JI_Tariff = "6402195061";
			invoiceLine.JI_LinePrice = 44010m;
			invoiceLine.JI_CustomsQuantity = 6750m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_LinePrice();
			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);

			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format("The unit price ${0} calculated with the customs value divided by the first quantity of entry lines is outside of the range the selected tariff allows", unitPrice));

			invoiceLine.JI_LinePrice = 43103.29;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoNotifications("No notifications related to decimal points.", invoiceLine.JI_LinePriceInfo);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111122";
			tariff.UE_Unit1 = "KG";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;

			invoiceLine.JI_CustomsQuantity = -1m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, ValidationConstants.NegativeAmountNotAllowed);

			invoiceLine.JI_Tariff = "0000111122";

			invoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Barrels;
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);

			invoiceLine.JI_CustomsQuantity = 12.3m;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);
		}

		public void TestCheckJI_Tariff()
		{
			AssertMIDAgainstCountryOfOrigin(invoiceLine);
		}

		public void TestCheckJI_Description()
		{
			invoiceLine.JI_Description = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public override void TestLinePriceBoundaryForNotMergedLines()
		{
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

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_LinePrice = 13m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IDutyData cusEntryLine = invoiceLine.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, " > 12.0000");

			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_LinePrice();
			cusEntryLine = invoiceLine.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			var errorText = string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " > 12.0000", unitPrice);
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, errorText);
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, " > 12.0000");
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOwner(org);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 0m;
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));

			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));

			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Foreign Trade Zone"));

			var whsPack = declaration.WHSPacks.AddNew();
			whsPack.US_PackageQty = 1;
			var whsPackLine = declaration.WHSPackLines.AddNew(invoiceLine);
			whsPackLine.US_PackedQty = 100m;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Foreign Trade Zone"));

			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Foreign Trade Zone"));

			invoiceLine.JI_InvoiceQuantity = 100m;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, FTZJobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Foreign Trade Zone"));
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Foreign Trade Zone"));
		}

		public void TestCheckJI_BondedWhsQuantityAndCheckBondedWhsQuantityForGUI()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOwner(org);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			AssertHasMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			AssertNoMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			AssertHasMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			invoiceLine.JI_PartNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			AssertNoMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
			AssertHasMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Foreign Trade Zone"));
		}

		public void TestCheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff unable to be found. ");

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2208609999";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-20);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(-10);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff unable to be found. ");
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff was found but is not valid");

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2208609998";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-20);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
		}

		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}
	}
}
