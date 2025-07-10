using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class EntryCreationStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
	{
		//TODO: revisit to make sure all fields are included as well
		public void TestLineMergeKey()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			InvoiceLine.SG_DutyPercentageRate = 1m;
			InvoiceLine.SG_DutyUnitRate = 2m;
			InvoiceLine.SG_ExciseUnitRate = 3m;
			InvoiceLine.SG_ExcisePercentageRate = 4m;
			InvoiceLine.SG_LastSellingPrice = 5m;
			InvoiceLine.SG_PercAlcohol = 6m;
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 7m;
			InvoiceLine.UnitPrice = 8m;
			InvoiceLine.JI_BrandName = "BrandName";
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			InvoiceLine.SG_LotNo = "LotNo";
			InvoiceLine.JI_HazMatCodeQualifier = "N";
			InvoiceLine.SG_ESNDPIndicator = "ES";
			InvoiceLine.SG_CategoryCode = "CATCODE";
			InvoiceLine.SG_EndUseCode1 = "EU1";
			InvoiceLine.SG_EndUseCode2 = "EU2";
			InvoiceLine.SG_EndUseCode3 = "EU3";
			InvoiceLine.SG_EndUseDescription = "EndUseDescription";
			InvoiceLine.JI_Description = "Description";
			InvoiceLine.JI_Tariff = "Tariff";
			InvoiceLine.JI_CustomsUnitQty = "UQ1";
			InvoiceLine.JI_Model = "Model";
			InvoiceLine.JI_InvoiceUQ = "UQ2";
			InvoiceLine.MarksAndNumbers = "MarksAndNumbers";
			InvoiceLine.SG_OuterPackQuantityUnit = "UQ3";
			InvoiceLine.SG_InPackQuantityUnit = "UQ4";
			InvoiceLine.SG_InnerPackQuantityUnit = "UQ5";
			InvoiceLine.SG_InmostPackQuantityUnit = "UQ6";
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			InvoiceLine.SG_PreviousLotNo = "PreviousLotNo";
			InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "UQ7";
			InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = "UQ8";
			InvoiceLine.SG_ManufacturingCostStatementDate = ZDateTime.Today;
			InvoiceLine.SG_CertItemValue = 9m;
			InvoiceLine.SG_PercContent = 10;
			InvoiceLine.SG_TextileQuotaQuantity = 11;
			InvoiceLine.SG_OtherTaxUnitRate = 12m;
			InvoiceLine.SG_OtherTaxPercentageRate = 13m;
			InvoiceLine.CertItemDescription = "";
			InvoiceLine.SG_CertItemQuantityUnit = "UQ9";
			InvoiceLine.SG_CertOriginCriterion1 = "OriginCriterion1";
			InvoiceLine.SG_CertOriginCriterion2 = "OriginCriterion2";
			InvoiceLine.SG_CertOriginCriterion3 = "OriginCriterion3";
			InvoiceLine.SG_TextileCatCode = "TCC";
			InvoiceLine.SG_TextileQuotaQuantityUnit = "U10";
			InvoiceLine.SG_InwardHAWB = "InwardHAWB";
			InvoiceLine.SG_InwardMAWB = "InwardMAWB";
			InvoiceLine.SG_OutwardHAWB = "OutwardHAWB";
			InvoiceLine.JI_OutwardMAWB = "OutwardMAWB";
			InvoiceLine.SG_TobaccoMultiplier = 7;
			InvoiceLine.SG_TariffCommodityType = "TOB";
			CusLineTariffDetail productCode1 = InvoiceLine.ProductCodes.AddNew();
			CusLineTariffDetail productCode2 = InvoiceLine.ProductCodes.AddNew();
			Customs.Business.MergeKey mergeKey = EntryCreationStrategy.GetKeyForLine(InvoiceLine);
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_DutyPercentageRate));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_DutyUnitRate));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_ExciseUnitRate));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_ExcisePercentageRate));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_LastSellingPrice));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_PercAlcohol));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_UnitDutiableWGTVOLQTY));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.UnitPrice));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_BrandName));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_CountryOfOrigin));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_LotNo));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_HazMatCodeQualifier));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_ESNDPIndicator));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_CategoryCode));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_EndUseCode1));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_EndUseCode2));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_EndUseCode3));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_EndUseDescription));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_Description));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_Tariff));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_CustomsUnitQty));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_Model));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_InvoiceUQ));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.MarksAndNumbers));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_OuterPackQuantityUnit));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_InPackQuantityUnit));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_InnerPackQuantityUnit));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_InmostPackQuantityUnit));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_PrimaryPreference));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_PreviousLotNo));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_ManufacturingCostStatementDate));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_CertItemValue));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_PercContent));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_TextileQuotaQuantity));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.CertItemDescription));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_CertItemQuantityUnit));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_CertOriginCriterion1));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_CertOriginCriterion2));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_CertOriginCriterion3));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_TextileCatCode));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_TextileQuotaQuantityUnit));
			AssertEquals(true, mergeKey.Contains(productCode1.PK));
			AssertEquals(true, mergeKey.Contains(productCode2.PK));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_InwardHAWB));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_InwardMAWB));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_OutwardHAWB));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_OutwardMAWB));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_TobaccoMultiplier));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_TariffCommodityType));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_OtherTaxUnitRate));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_OtherTaxPercentageRate));
		}

		public void TestNonIPTandINPMergeKey()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			InvoiceLine.SG_LastSellingPrice = 5m;
			InvoiceLine.UnitPrice = 3.5m;
			Customs.Business.MergeKey mergeKey = EntryCreationStrategy.GetKeyForLine(InvoiceLine);
			AssertEquals(false, mergeKey.Contains(InvoiceLine.SG_LastSellingPrice));
			AssertEquals(false, mergeKey.Contains(InvoiceLine.UnitPrice));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			mergeKey = EntryCreationStrategy.GetKeyForLine(InvoiceLine);
			AssertEquals(false, mergeKey.Contains(InvoiceLine.SG_LastSellingPrice));
			AssertEquals(false, mergeKey.Contains(InvoiceLine.UnitPrice));
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			mergeKey = EntryCreationStrategy.GetKeyForLine(InvoiceLine);
			AssertEquals(true, mergeKey.Contains(InvoiceLine.SG_LastSellingPrice));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.UnitPrice));
		}

		public override void TestGetKeyForHeader()
		{
			Assert(true);
		}

		#region Setup
		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
		#region InvoiceHeader
		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region InvoiceLine
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
		#region EntryCreationStrategy
		EntryCreationStrategy EntryCreationStrategy
		{
			get
			{
				return entryCreationStrategy ?? (entryCreationStrategy = new EntryCreationStrategy(Declaration));
			}
		}

		EntryCreationStrategy entryCreationStrategy;
		#endregion
		#endregion
	}
}
