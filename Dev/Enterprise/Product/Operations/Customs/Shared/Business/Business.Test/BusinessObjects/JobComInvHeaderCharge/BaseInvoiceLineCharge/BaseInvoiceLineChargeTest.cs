using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseInvoiceLineCharge))]
	public class BaseInvoiceLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUpdateJ7_IsIncludedInInvoiceWhenChargeTypeIsEntered()
		{
			BaseJobDeclaration declaration = GetJobDeclarationForTest();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncotermToTestIsIncludedInInvoice();

			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			BaseInvoiceLineCharge charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("not included in invoice", false, charge.J7_Calc_IsIncludedInInvoiceAmount);

			invoice.JZ_IncoTerm = "CIF";
			AssertEquals("included in invoice", true, charge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		protected virtual BaseJobDeclaration GetJobDeclarationForTest()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		protected virtual string GetIncotermToTestIsIncludedInInvoice() => "FOB";

		public virtual void TestSettingAmountWillUpdateCurrencyIfNotEntered()
		{
			BaseInvoiceLineCharge charge = Factory.New<BaseInvoiceLineCharge>();
			charge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			charge.J7_Amount = 1.2m;
			AssertEquals("J7_RX_NKCurrency", testDec.LocalCurrencyCode, charge.J7_RX_NKCurrency);
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			charge.Parent = invoiceLine;
			charge.J7_RX_NKCurrency = ZString.Empty;
			charge.J7_Amount = 1.3m;
			AssertEquals("J7_RX_NKCurrency", ZString.Empty, charge.J7_RX_NKCurrency);
			charge.J7_Amount = 1.2m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			charge.J7_RX_NKCurrency = ZString.Empty;
			charge.J7_Amount = 1.3m;
			AssertEquals("J7_RX_NKCurrency", testDec.LocalCurrencyCode, charge.J7_RX_NKCurrency);

			charge.J7_Amount = 1.2m;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Belarus;
			charge.J7_Amount = 1.3m;
			AssertEquals("J7_RX_NKCurrency", Core.Constants.CurrencyCodes.Belarus, charge.J7_RX_NKCurrency);
		}

		#region Implementation

		BaseJobDeclaration testDec;
		protected BaseJobComInvoiceHeader invoice;
		protected BaseJobComInvoiceLine invoiceLine;
		BaseInvoiceLineCharge charge;

		protected override void SetUp()
		{
			base.SetUp();
			SetupAllTestObjects();
		}

		protected virtual void SetupAllTestObjects()
		{
			testDec = Factory.New<BaseJobDeclaration>();
			invoice = testDec.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			charge = invoiceLine.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			SetupAllTestObjects();
			return charge;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<BaseInvoiceLineCharge>();
		}

		#endregion
	}
}
