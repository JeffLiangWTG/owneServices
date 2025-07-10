using System;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.Declaration.InterfaceImplementations.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;

	class CusEntryHeaderCustomsChargesTest : TestCaseWithFactory
	{
		public void TestWhenNZExportEntryFeeIsSpecified()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			NZCustomsDataRegistry.Instance.ExportEntryFeeChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			declaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "NZD";
			declaration.InvoiceLines.AddNew().JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CusEntryHeader;

			var charges = ((Accounting.Integration.ICustomsCharges)new CusEntryHeaderCustomsCharges(entry)).GetCustomsCharges(null);
			AssertEquals(1, charges.Length);
			AssertEquals(chargeCode.PK, charges[0].ChargeCodePK);

			var chargesCache = ((Accounting.Integration.IAccInvoiceDataProvider)entry).CustomsCharges;
			AssertEquals(1, chargesCache.Length);
			var charges2 = chargesCache[0].GetCustomsCharges(null);
			AssertEquals(1, charges2.Length);
			AssertEquals(chargeCode.PK, charges2[0].ChargeCodePK);
		}

		public void TestAutoRateNormallyForImport()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			NZCustomsDataRegistry.Instance.ExportEntryFeeChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes.Completion;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			declaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "NZD";
			declaration.InvoiceLines.AddNew().JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CusEntryHeader;

			var charges = ((Accounting.Integration.ICustomsCharges)new CusEntryHeaderCustomsCharges(entry)).GetCustomsCharges(null);
			AssertNotEquals(0, charges.Length);
		}

		public void TestWhenNZExportEntryFeeIsNotSpecified()
		{
			NZCustomsDataRegistry.Instance.ExportEntryFeeChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			declaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "NZD";
			declaration.InvoiceLines.AddNew().JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CusEntryHeader;

			var charges = ((Accounting.Integration.ICustomsCharges)new CusEntryHeaderCustomsCharges(entry)).GetCustomsCharges(null);
			AssertEquals(0, charges.Length);
		}

		public void TestWhenClientPaysButNZExportEntryFeeIsNotSpecified()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			Factory.Save();

			NZCustomsDataRegistry.Instance.ExportEntryFeeChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.ClientDeferred;
			declaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "NZD";
			declaration.InvoiceLines.AddNew().JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CusEntryHeader;

			var charges = ((Accounting.Integration.ICustomsCharges)new CusEntryHeaderCustomsCharges(entry)).GetCustomsCharges(null);
			AssertEquals("Payment method is disregarded. Nothing to autorate as the registry item 'ExportEntryFeeChargeCode' is not set", 0, charges.Length);
		}
	}
}
