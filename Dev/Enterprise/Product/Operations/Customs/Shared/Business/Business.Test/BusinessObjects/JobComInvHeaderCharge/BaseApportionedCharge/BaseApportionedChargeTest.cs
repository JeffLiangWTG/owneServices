using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseApportionedCharge))]
	public class BaseApportionedChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDeciderForCharge()
		{
			AssertEquals("Type decider type", typeof(BaseApportionedChargeTypeDecider), BaseApportionedCharge.TypeDecider.GetType());
		}

		[ExpectNoExceptions]
		public void TestSupportsClone()
		{
			BaseApportionedCharge appCharge = Factory.New<BaseApportionedCharge>();
			appCharge.Clone();
		}

		public void TestSetDefaultValues()
		{
			BaseApportionedCharge appCharge = Factory.New<BaseApportionedCharge>();
			AssertEquals("IsApportioned should have been set", true, appCharge.J7_IsApportionedCharge);

			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			appCharge.Parent = invoice;
			AssertEquals("Parent table code", "JZ", appCharge.J7_ParentTableCode);
		}

		public void TestIsIncludedInITOTSetForLinesWhenChargeTypeIsSet()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			BaseInvoiceLineCharge invLineCharge = invoiceLine.Charges.AddNew();
			invLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			invLineCharge.J7_Amount = 100m;
			invLineCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Invoice has an apportioned charge", 1, invoice.GroupCharges.Count);
			AssertEquals("Invoice has an apportioned charge with OTH", "OTH", invoice.GroupCharges[0].J7_ChargeType);
			AssertEquals("Invoice's apportioned charge IncludedInLines", invLineCharge.J7_IsIncludedInITOT, invoice.GroupCharges[0].J7_IsIncludedInITOT);
		}

		public virtual void TestReApportionWhenInvoiceChargeAdded()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var decAndCode = GetDeclarationAndChargeCodeForTest();
				var testDec = decAndCode.Item1;
				var chargeCode = decAndCode.Item2;
				testDec.AutoCreateChargesBasedOnIncoTerm = false;
				var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				PrepareCharge(groupHeader.Charges.AddNew(chargeCode, 100, testDec.LocalCurrencyCode));
				invoice.JZ_InvoiceAmount = 100;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				testDec.ResumeApportionment();

				AssertEquals("PreCondition: one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("PreCondition: Apportioned Charge", 100m, invoice.GroupCharges[0].J7_Amount);

				PrepareCharge(invoice.Charges.AddNew(chargeCode, 60));
				testDec.ResumeApportionment();
				AssertEquals("Apportioned Charge is cleared", 0, invoice.GroupCharges.Count);

				BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
				invoice2.JZ_InvoiceAmount = 2000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				testDec.ResumeApportionment();
				AssertEquals("One apportioned Charge", 1, invoice2.GroupCharges.Count);
				AssertEquals("Apportioned amount", 40m, invoice2.GroupCharges[0].J7_Amount);
			}
		}

		protected virtual (BaseJobDeclaration, string) GetDeclarationAndChargeCodeForTest()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			return (testDec, CustomsChargeTypeList.Codes.OverseasFreight);
		}

		public void TestApportionedChargeSaved()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var decAndCode = GetDeclarationAndChargeCodeForTest();
				var testDec = decAndCode.Item1;
				var chargeCode = decAndCode.Item2;

				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				BaseJobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				var charge = groupHeader.Charges.AddNew(chargeCode, 100, testDec.LocalCurrencyCode);
				PrepareCharge(charge);
				invoice.JZ_InvoiceAmount = 100;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				ZQuery filter = new ZQuery(JobComInvHeaderChargeSchema.J7_ParentID, invoice.PK);
				BusinessObject[] chargeSaved = newFactory.Load(typeof(BaseApportionedCharge), filter);
				AssertEquals("One row saved", 1, chargeSaved.Length);
			}
		}

		protected virtual void PrepareCharge(JobComInvCharge charge)
		{
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BaseJobDeclaration testDec = factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			return invoice.GroupCharges.AddNew("ONS", 32.2m, testDec.LocalCurrencyCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			return invoice.GroupCharges.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion
	}
}
