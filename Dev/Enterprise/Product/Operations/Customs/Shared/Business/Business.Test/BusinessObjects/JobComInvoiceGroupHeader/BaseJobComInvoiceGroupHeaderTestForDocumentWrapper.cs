using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceGroupHeaderTestForDocumentWrapper : TestCaseWithFactory
	{
		public void TestCalcCommission()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.Commission, "CalcCommission", "CalcCommissionCurrency");
		}

		public void TestCalcDiscount()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.Discount, "CalcDiscount", "CalcDiscountCurrency");
		}

		public void TestCalcOverseasFreight()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OverseasFreight, "CalcOverseasFreight", "CalcOverseasFreightCurrency");
		}

		public void TestCalcOverseasInsurance()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OverseasInsurance, "CalcOverseasInsurance", "CalcOverseasInsuranceCurrency");
		}

		public void TestCalcForeignInlandFreight()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.ForeignInlandFreight, "CalcForeignInlandFreight", "CalcForeignInlandFreightCurrency");
		}

		public void TestCalcPackingCosts()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.PackingCost, "CalcPackingCosts", "CalcPackingCostsCurrency");
		}

		public void TestCalcOtherCharges1()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OtherCharges, "CalcOtherCharges1", "CalcOtherCharges1Currency");
		}

		public void TestCalcOtherCharges2()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.OtherCharges, "CalcOtherCharges2", "CalcOtherCharges2Currency");
		}

		public void TestCalcLandingCharges()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.LandingCharges, "CalcLandingCharges", "CalcLandingChargesCurrency");
		}

		public void TestCalcExWorks()
		{
			AssertCalcFields(CustomsChargeTypeList.Codes.ExWorks, "CalcExWorks", "CalcExWorksCurrency");
		}

		protected void AssertCalcFields(string chargeName, string calcAmountName, string calcCurrencyName)
		{
			if (groupHeader.IncoTermAndChargeFactory.GetAllCharges().Any(x => x.Code == chargeName))
			{
				BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(chargeName, 100, testDec.LocalCurrencyCode);

				if (calcAmountName == "CalcOtherCharges2")
				{
					charge.J7_IsDutiable = false;
					charge.J7_IsGSTApplicable = false;
				}

				ZDecimal actualAmount = (ZDecimal)groupHeader[calcAmountName];
				ZGuid actualCurrency = (ZGuid)groupHeader[calcCurrencyName];
				AssertEquals(calcAmountName, 100m, actualAmount);
				RefCurrency currency = Factory.Load<RefCurrency>(actualCurrency);
				AssertEquals(calcCurrencyName, testDec.LocalCurrencyCode, currency != null ? currency.RX_Code : ZString.Empty);
			}
			else
			{
				Assert("No applicable", true);
			}
		}

		#region Implementation

		protected BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
		}

		#endregion
	}
}
