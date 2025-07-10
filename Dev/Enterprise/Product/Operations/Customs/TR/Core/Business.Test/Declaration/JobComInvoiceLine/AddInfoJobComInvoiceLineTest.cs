using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	sealed class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<AddInfoJobComInvoiceLineLookups>(addInfo.Lookups);
		}

		public void TestValidation()
		{
			AssertType<AddInfoJobComInvoiceLineValidation>(addInfo.Validation);
		}

		public void TestZG_InwardProcessingLicenseLineNumber_ReadOnlyAttribute()
		{
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_InwardProcessingLicenseLineNumber), false, attr => attr.Member == nameof(JobComInvoiceLine.IsNotImportOrExport));
		}

		public void TestZG_InwardProcessingLicenseLineNumber_MaxLength()
		{
			AssertEquals(14, invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo.MaxLength);
		}

		public void TestZG_InwardProcessingLicenseLineNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Inward Processing Line No", resourceStringData.Caption);
				AssertEquals("Short Caption", "Inward Pro. Line No", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ReturningGoodsReasonCode_ReadOnlyAttribute()
		{
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_ReturningGoodsReasonCode), false, attr => attr.Member == nameof(JobComInvoiceLine.IsNotImportOrExport));
		}

		public void TestZG_ReturningGoodsReasonCode_MaxLength()
		{
			AssertEquals(3, invoiceLine.ZG_ReturningGoodsReasonCodeInfo.MaxLength);
		}

		public void TestZG_ReturningGoodsReasonCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ReturningGoodsReasonCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Return Goods Reason", resourceStringData.Caption);
				AssertEquals("Short Caption", "Return Goods Reason", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ReturningGoodsReasonDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.ZG_ReturningGoodsReasonCode = "0";
			var expectedDetailText = "Test Detail For ReturningGoodsReasonCode";

			invoiceLine.ZG_ReturningGoodsReasonDetail = expectedDetailText;
			AssertEquals(expectedDetailText, invoiceLine.ZG_ReturningGoodsReasonDetail);
			AssertEquals(true, invoiceLine.ReturningGoodsReasonCodeVisibility);

			invoiceLine.ZG_ReturningGoodsReasonCode = "1";
			AssertEquals(ZString.Empty, invoiceLine.ZG_ReturningGoodsReasonDetail);
			AssertEquals(false, invoiceLine.ReturningGoodsReasonCodeVisibility);
		}

		public void TestZG_ReturningGoodsReasonDetail_MaxLength()
		{
			AssertEquals("Max Length", 100, invoiceLine.ZG_ReturningGoodsReasonDetailInfo.MaxLength);
		}

		public void TestZG_ReturningGoodsReasonDetail_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ReturningGoodsReasonDetailInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Details", resourceStringData.Caption);
				AssertEquals("Short Caption", "Details", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ReturnToOrigin_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ReturnToOriginInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Return to Origin", resourceStringData.Caption);
				AssertEquals("Short Caption", "Ret.to Origin", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_UsedGoodsCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_UsedGoodsCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Used Good", resourceStringData.Caption);
				AssertEquals("Short Caption", "Used Good", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_UsedGoodsCode_MaxLength()
		{
			AssertEquals(3, invoiceLine.ZG_UsedGoodsCodeInfo.MaxLength);
		}

		public void TestZG_ExportUnionAdditionalTariffCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExportUnionAdditionalTariffCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Additional Code", resourceStringData.Caption);
				AssertEquals("Short Caption", "Additional Code", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ExportUnionAdditionalTariffCode_MaxLength()
		{
			AssertEquals(15, invoiceLine.ZG_ExportUnionAdditionalTariffCodeInfo.MaxLength);
		}

		public void TestZG_ExcessStock_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExcessStockInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Excess Stock", resourceStringData.Caption);
				AssertEquals("Short Caption", "Excess Stock", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_SecondaryTreatedProduct_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_SecondaryTreatedProductInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Secondary Treated Product", resourceStringData.Caption);
				AssertEquals("Short Caption", "Sec.Treat.Pro.", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_RW_NKBorderTradeStateCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_RW_NKBorderTradeStateCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Border Trade City", resourceStringData.Caption);
				AssertEquals("Short Caption", "Border Trade City", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_RW_NKBorderTradeStateCode_MaxLength()
		{
			AssertEquals(2, invoiceLine.ZG_RW_NKBorderTradeStateCodeInfo.MaxLength);
		}

		public void TestZG_CommercialPaymentCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_CommercialPaymentCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[43] Valuation Method", resourceStringData.Caption);
				AssertEquals("Short Caption", "[43] Valuation", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_CommercialPaymentCode_MaxLength()
		{
			AssertEquals(2, invoiceLine.ZG_CommercialPaymentCodeInfo.MaxLength);
		}

		public void TestZG_CommercialPaymentNumber_MaxLength()
		{
			AssertEquals(20, invoiceLine.ZG_CommercialPaymentNumberInfo.MaxLength);
		}

		public void TestZG_ExportUnionDeferredInstallment_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExportUnionDeferredInstallmentInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[48.] Deferred Instalment", resourceStringData.Caption);
				AssertEquals("Short Caption", "Def.Instalment", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ExportUnionDeferredInstallment_MaxLength()
		{
			AssertEquals(250, invoiceLine.ZG_ExportUnionDeferredInstallmentInfo.MaxLength);
		}

		public void TestZG_ExportUnionEcological_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExportUnionEcologicalInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Ecological", resourceStringData.Caption);
				AssertEquals("Short Caption", "Ecological", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ExportUnionPackCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExportUnionPackCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Export Union Package Code", resourceStringData.Caption);
				AssertEquals("Short Caption", "Ex.Un.Pack Code", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ExportUnionPackCode_MaxLength()
		{
			AssertEquals(5, invoiceLine.ZG_ExportUnionPackCodeInfo.MaxLength);
		}

		public void TestZG_ExportUnionProductionYear_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExportUnionProductionYearInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Production Year", resourceStringData.Caption);
				AssertEquals("Short Caption", "Production Year", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ExportUnionThreadCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ExportUnionThreadCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Thread Code", resourceStringData.Caption);
				AssertEquals("Short Caption", "Thread Code", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ExportUnionThreadCode_MaxLength()
		{
			AssertEquals(5, invoiceLine.ZG_ExportUnionThreadCodeInfo.MaxLength);
		}

		public void TestZG_EntryExitPurposeCode_MaxLength()
		{
			AssertEquals(3, invoiceLine.ZG_EntryExitPurposeCodeInfo.MaxLength);
		}

		public void TestZG_EntryExitPurposeDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "2100";
			invoiceLine.ZG_EntryExitPurposeCode = "05";
			var expectedDetailText = "Test Detail For EntryExitPurposeCode";

			invoiceLine.ZG_EntryExitPurposeDetail = expectedDetailText;
			AssertEquals(expectedDetailText, invoiceLine.ZG_EntryExitPurposeDetail);
			AssertEquals(true, invoiceLine.IsEntryExitPurposeDetailVisibility);

			invoiceLine.ZG_EntryExitPurposeCode = "01";
			AssertEquals(ZString.Empty, invoiceLine.ZG_EntryExitPurposeDetail);
			AssertEquals(false, invoiceLine.IsEntryExitPurposeDetailVisibility);
		}

		public void TestZG_EntryExitPurposeDetail_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_EntryExitPurposeDetailInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Entry/Exit Purpose Detail", resourceStringData.Caption);
				AssertEquals("Short Caption", "Entry/Exit P.Det.", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_EntryExitPurposeDetail_MaxLength()
		{
			AssertEquals(100, invoiceLine.ZG_EntryExitPurposeDetailInfo.MaxLength);
		}

		public void TestZG_PriceType_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_PriceTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Price Type", resourceStringData.Caption);
				AssertEquals("Short Caption", "Price Type", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_PriceType_MaxLength()
		{
			AssertEquals(2, invoiceLine.ZG_PriceTypeInfo.MaxLength);
		}

		public void TestZG_ProcessingDescription_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.ZG_ProcessingDescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Processing Description", resourceStringData.Caption);
				AssertEquals("Short Caption", "Pro.Description", resourceStringData.ShortCaption);
			});
		}

		public void TestZG_ProcessingDescription_MaxLength()
		{
			AssertEquals(300, invoiceLine.ZG_ProcessingDescriptionInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => addInfo;

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			addInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
		}

		AddInfoJobComInvoiceLine addInfo;
		JobComInvoiceLine invoiceLine;
	}
}
