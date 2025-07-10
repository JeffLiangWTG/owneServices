using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MaximumCreditLimitItem))]
	public class MaximumCreditLimitItemTest : RegistryBusinessObjectTemplateTestCase
	{
		ZGuid currencyPK1;
		ZGuid currencyPK2;

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			if (currencyPK1 == ZGuid.Empty && currencyPK2 == ZGuid.Empty)
			{
				var currency1 = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
				var currency2 = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

				currencyPK1 = currency1.PK;
				currencyPK2 = currency2.PK;
			}

			return new MaximumCreditLimitItem();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var maximumCreditLimitItem = new MaximumCreditLimitItem(new FallbackLevel(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			maximumCreditLimitItem.CreditLimit = "100";
			maximumCreditLimitItem.CurrencyPK = currencyPK1;

			return maximumCreditLimitItem;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public void TestValidateCreditLimit()
		{
			var item = new MaximumCreditLimitItem();
			item.CreditLimit = "5.5";
			AssertHasErrors("Credit Limit must be a natural number.", item.CreditLimitInfo);

			item.CreditLimit = string.Empty;
			AssertHasErrors("Credit Limit must be a natural number.", item.CreditLimitInfo);

			item.CreditLimit = "-100";
			AssertHasErrors("Credit Limit must be a natural number.", item.CreditLimitInfo);

			item.CreditLimit = " ";
			AssertHasErrors("Credit Limit must be a natural number.", item.CreditLimitInfo);

			item.CreditLimit = "100";
			AssertNoErrors(item.CreditLimitInfo);
		}

		public void TestValidateAtLeastOneReportSelected()
		{
			var item = new MaximumCreditLimitItemForTest();
			item.CreditLimit = "100";
			item.CurrencyPK = currencyPK1;
			item.CommercialBureauEnquiryEnabled = false;
			item.ComprehensiveReportEnabled = false;
			item.LatePaymentRiskEnabled = false;
			item.FailureRiskEnabled = false;

			AssertHasRowError(item, "Please select at least 1 Report Type");

			item.CommercialBureauEnquiryEnabled = true;
			AssertNoRowError(item, "Please select at least 1 Report Type");

			item.CommercialBureauEnquiryEnabled = false;
			item.ComprehensiveReportEnabled = true;
			AssertNoRowError(item, "Please select at least 1 Report Type");

			item.ComprehensiveReportEnabled = false;
			item.LatePaymentRiskEnabled = true;
			AssertNoRowError(item, "Please select at least 1 Report Type");

			item.LatePaymentRiskEnabled = false;
			item.FailureRiskEnabled = true;
			AssertNoRowError(item, "Please select at least 1 Report Type");
		}

		public void TestValidateCurrency()
		{
			var collection = new MaximumCreditLimitCollection();
			var item1 = new MaximumCreditLimitItemForTest();
			item1.CreditLimit = "100";
			item1.CurrencyPK = currencyPK1;

			var item2 = new MaximumCreditLimitItemForTest();
			item2.CreditLimit = "200";
			item2.CurrencyPK = currencyPK1;

			var item3 = new MaximumCreditLimitItemForTest();
			item3.CreditLimit = "300";
			item3.CurrencyPK = ZGuid.Empty;

			var item4 = new MaximumCreditLimitItemForTest();
			item4.CreditLimit = "400";
			item4.CurrencyPK = ZGuid.NewZGuid();

			var item5 = new MaximumCreditLimitItemForTest();
			item5.CreditLimit = "500";
			item5.CurrencyPK = currencyPK2;

			collection.Add(item1);
			collection.Add(item2);
			collection.Add(item3);
			collection.Add(item4);
			collection.Add(item5);

			item1.ValidateCurrencyPKForTest();

			CombineAssertions(() =>
			{
				AssertHasRowError(item1, "Settings for following Currency already exist - CNY");
				AssertHasRowError(item2, "Settings for following Currency already exist - CNY");
				AssertHasError(item3.CurrencyPKInfo, "Currency can not be empty.");
				AssertHasError(item4.CurrencyPKInfo, "Enter a valid Currency.");
				AssertNoErrors(item5.CurrencyPKInfo);
				AssertNoRowError(item5, "Settings for following Currency already exist - USD");
			});
		}

		public void TestDefaultAllReportTypesSelected()
		{
			var item1 = GetNewBusinessObject() as MaximumCreditLimitItem;
			item1.CurrencyPK = currencyPK1;
			item1.CreditLimit = "100";

			CombineAssertions(() =>
			{
				AssertEquals(true, item1.CommercialBureauEnquiryEnabled);
				AssertEquals(true, item1.ComprehensiveReportEnabled);
				AssertEquals(true, item1.LatePaymentRiskEnabled);
				AssertEquals(true, item1.FailureRiskEnabled);
			});

			var item2 = GetBusinessObjectToClone() as MaximumCreditLimitItem;

			CombineAssertions(() =>
			{
				AssertEquals(true, item2.CommercialBureauEnquiryEnabled);
				AssertEquals(true, item2.ComprehensiveReportEnabled);
				AssertEquals(true, item2.LatePaymentRiskEnabled);
				AssertEquals(true, item2.FailureRiskEnabled);
			});
		}
	}

	public class MaximumCreditLimitItemForTest : MaximumCreditLimitItem
	{
		public void ValidateCurrencyPKForTest() => ValidateCurrencyPK();
	}
}
