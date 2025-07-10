using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobComInvHeaderChargeTest : TestCaseWithFactory
	{
		public void TestSetDefaultValuesForSystem()
		{
			BaseJobComInvHeaderCharge charge = Factory.New<BaseInvoiceCharge>();
			AssertEquals(false, charge.J7_IsSystem);
		}

		public void TestJ7_Calc_IsIncludedInInvoiceAmountAndReadOnly()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();

			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount);

			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			AssertEquals("J7_IsNotIncludedInInvoice", false, invoiceCharge.J7_IsNotIncludedInInvoice);

			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			BaseInvoiceLineCharge invoiceLineCharge = invoiceLine.Charges.AddNew();

			invoiceLineCharge.J7_IsNotIncludedInInvoice = true;
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount);

			invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			AssertEquals("J7_IsNotIncludedInInvoice", false, invoiceLineCharge.J7_IsNotIncludedInInvoice);
		}

		public virtual void TestIsJ7_ExchangeRateUserEnterable()
		{
			testCharge.IsJ7_ExchangeRateUserEnterable = true;
			AssertEquals(ChargeExchangeRateTypeList.Codes.FixedRate, testCharge.J7_ExchangeRateType);

			testCharge.IsJ7_ExchangeRateUserEnterable = false;
			AssertEquals(ZString.Empty, testCharge.J7_ExchangeRateType);

			testCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			AssertEquals(true, testCharge.IsJ7_ExchangeRateUserEnterable);

			testCharge.J7_ExchangeRateType = ZString.Empty;
			AssertEquals(false, testCharge.IsJ7_ExchangeRateUserEnterable);
		}

		public virtual void TestIsDiscount()
		{
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			AssertEquals("IsDiscount", true, testCharge.IsDiscount);

			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("IsDiscount", false, testCharge.IsDiscount);
		}

		public void TestJ7_ChargeDescription()
		{
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertEquals("J7_ChargeDescription", CustomsChargeTypeList.Descriptions.Commission, testCharge.J7_ChargeDescription);

			testCharge.J7_ChargeDescription = "Blah Blah";
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertEquals("J7_ChargeDescription", "Blah Blah", testCharge.J7_ChargeDescription);

			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			var expectedAdditionChargeDescription = new ZString(CustomsChargeTypeList.Descriptions.AdditionCharge).Left(testCharge.J7_ChargeDescriptionInfo.MaxLength).TrimEnd(' ');
			AssertEquals("J7_ChargeDescription", expectedAdditionChargeDescription, testCharge.J7_ChargeDescription);
		}

		public virtual void TestIsJ7_ExchangeRateUserEnterableInfoReadOnly()
		{
			testCharge.J7_RX_NKCurrency = ZString.Empty;
			AssertEquals(true, testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);

			testCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Afghanistan;
			AssertEquals(false, testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);

			testCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			AssertEquals(true, testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);
		}

		public void TestJ7_ExchangeRateInfoReadOnly()
		{
			SetIsJ7_ExchangeRateUserEnterable(testCharge, false);
			AssertEquals(true, testCharge.J7_ExchangeRateInfo.ReadOnly);

			SetIsJ7_ExchangeRateUserEnterable(testCharge, true);
			AssertEquals(false, testCharge.J7_ExchangeRateInfo.ReadOnly);
		}

		public virtual void TestJ7_ExchangeRateTypeInfoReadOnly()
		{
			testCharge.J7_RX_NKCurrency = ZString.Empty;
			AssertEquals(true, testCharge.J7_ExchangeRateTypeInfo.ReadOnly);

			testCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Afghanistan;
			AssertEquals(false, testCharge.J7_ExchangeRateTypeInfo.ReadOnly);

			testCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			AssertEquals(true, testCharge.J7_ExchangeRateTypeInfo.ReadOnly);
		}

		public void TestFixedExchangeRate()
		{
			testCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Bulgaria;
			testCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			testCharge.J7_ExchangeRate = 0.1m;
			testCharge.J7_Amount = 100m;
			AssertEquals("IsJ7_ExchangeRateUserEnterable", true, testCharge.IsJ7_ExchangeRateUserEnterable);
			AssertEquals("IsJ7_ExchangeRateUserEnterableInfo.ReadOnly", false, testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);
			AssertEquals("Money.Amount", 1000m, testCharge.Money.Amount);
			AssertEquals("Money.Currency", testDec.LocalCurrencyCode, testCharge.Money.Currency.Code);

			testCharge.J7_ExchangeRateType = ZString.Empty;
			AssertEquals("IsJ7_ExchangeRateUserEnterable", false, testCharge.IsJ7_ExchangeRateUserEnterable);
			AssertEquals("IsJ7_ExchangeRateUserEnterableInfo.ReadOnly", false, testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);
			AssertEquals("Money.Amount", 100m, testCharge.Money.Amount);
			AssertEquals("Money.Currency", Core.Constants.CurrencyCodes.Bulgaria, testCharge.Money.Currency.Code);

			testCharge.J7_RX_NKCurrency = ZString.Empty;
			AssertEquals("IsJ7_ExchangeRateUserEnterable", false, testCharge.IsJ7_ExchangeRateUserEnterable);
			AssertEquals("IsJ7_ExchangeRateUserEnterableInfo.ReadOnly", true, testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);
			AssertEquals("Money.Amount", 100m, testCharge.Money.Amount);
			AssertNull("Money.Currency", testCharge.Money.Currency);
		}

		[ExpectNoExceptions]
		public void TestSupportClone()
		{
			testCharge.Clone();
		}

		public void TestTypeDecider()
		{
			AssertEquals("TypeDecider", typeof(BaseJobComInvHeaderChargeTypeDecider), BaseJobComInvHeaderCharge.TypeDecider.GetType());
		}

		public void TestAccessingParentAfterDeleteDoesNotCauseExceptions()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew();
			AssertEquals("Parent", invoice, charge.Parent);
			charge.Delete();
			AssertNull("Parent of Charge after deleted. It does not access J7_ParentID", charge.Parent);
		}

		public void TestIsEmpty()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew();
			AssertEquals("IsEmpty", true, charge.IsEmpty);

			charge.J7_Amount = 10m;
			AssertEquals("IsEmpty", false, charge.IsEmpty);

			charge.J7_Amount = 0m;
			charge.J7_Percentage = 10m;
			AssertEquals("IsEmpty", false, charge.IsEmpty);
		}

		public void TestIsIncludedInITOTDeemedForThisCharge()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge);
			AssertEquals("Customs Charge ADD deems 'Is Included In Lines'", false, charge.ChargeCode.IsIncludedInITOTDeemedForThisCharge);
			AssertEquals("ADD should not be included in ITOT", false, charge.J7_IsIncludedInITOT);

			charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge);
			AssertEquals("Customs Charge DED deems 'IsIncludedInLines", false, charge.ChargeCode.IsIncludedInITOTDeemedForThisCharge);
			AssertEquals("DED should be included in ITOT", true, charge.J7_IsIncludedInITOT);
		}

		public void TestDefaultCurrencyToInvoiceCurr()
		{
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			testCharge.J7_Amount = 100m;
			AssertEquals("Currency set", testDec.LocalCurrencyCode, testCharge.J7_RX_NKCurrency);
		}

		public void TestMoney()
		{
			testCharge.J7_Amount = 100m;
			testCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			AssertEquals("Amount", 100m, testCharge.Money.Amount);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.UnitedStates, testCharge.Money.Currency.Code);

			testCharge.J7_ExchangeRateType = "FIX";
			AssertEquals(true, testCharge.IsJ7_ExchangeRateUserEnterable);
			testCharge.J7_ExchangeRate = 0.5m;
			AssertEquals("Amount", 200m, testCharge.Money.Amount);
			AssertEquals("Currency", testDec.LocalCurrencyCode, testCharge.Money.Currency.Code);

			testCharge.Parent = null;
			AssertEquals("Amount", 100m, testCharge.Money.Amount);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.UnitedStates, testCharge.Money.Currency.Code);
		}

		public void TestChargeKey()
		{
			BaseInvoiceCharge charge = Factory.New<BaseInvoiceCharge>();

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			AssertEquals("ChargeKey", charge.J7_ChargeType, charge.ChargeKey.ChargeCode);

			charge.J7_IsDutiable = !charge.J7_IsDutiable;
			AssertEquals("ChargeKey", charge.J7_IsDutiable, charge.ChargeKey.IsDutiable);

			charge.J7_IsGSTApplicable = !charge.J7_IsGSTApplicable;
			AssertEquals("ChargeKey", charge.J7_IsGSTApplicable, charge.ChargeKey.IsVATible);
		}

		public void TestAdditionDeductionChargeReadOnly()
		{
			BaseJobComInvHeaderCharge addition = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100);
			BaseJobComInvHeaderCharge deduction = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100);

			AssertEquals("ChargeType", false, addition.J7_ChargeTypeInfo.ReadOnly);
			AssertEquals("Duty", true, addition.J7_IsDutiableInfo.ReadOnly);
			AssertEquals("GST", true, addition.J7_IsGSTApplicableInfo.ReadOnly);

			AssertEquals("Included in ITOT", false, addition.J7_IsIncludedInITOT);
			AssertEquals("Included readonly", false, addition.J7_IsIncludedInITOTInfo.ReadOnly);

			AssertEquals("ChargeType", false, deduction.J7_ChargeTypeInfo.ReadOnly);
			AssertEquals("Duty", true, deduction.J7_IsDutiableInfo.ReadOnly);
			AssertEquals("GST", true, deduction.J7_IsGSTApplicableInfo.ReadOnly);

			AssertEquals("Included in ITOT", true, deduction.J7_IsIncludedInITOT);
			AssertEquals("Included readonly", false, deduction.J7_IsIncludedInITOTInfo.ReadOnly);
		}

		public void TestParent()
		{
			testCharge.Parent = null;
			AssertEquals("J7_ParentID removed", ZGuid.Empty, testCharge.J7_ParentID);
			AssertEquals("J7_ParentTableCode removed", "", testCharge.J7_ParentTableCode);
			AssertEquals("Parent object", null, testCharge.Parent);

			testCharge.Parent = invoice;
			AssertEquals("J7_ParentID set", invoice.PK, testCharge.J7_ParentID);
			AssertEquals("J7_ParentTableCode set", "JZ", testCharge.J7_ParentTableCode);
			AssertEquals("Parent object", invoice, testCharge.Parent);
		}

		public void TestSetDefaultValues()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
				BaseGroupInvoiceCharge testGroupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				AssertEquals("Distribute-by should be 'VAL'", ChargeDistributeByList.Codes.Value, testGroupCharge.J7_DistributeBy);
				AssertEquals("Apportionment type should be 'PUA'", ApportionmentTypeList.Codes.PartialApportionment, testGroupCharge.J7_FullOrPartialApportionment);
			}
		}

		public void TestDistributeBy()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			BaseGroupInvoiceCharge testGroupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			AssertEquals("Distribute by should be 'VAL'", ChargeDistributeByList.Codes.Value, testGroupCharge.J7_DistributeBy);

			testGroupCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			AssertEquals("Distribute by should retain what users have entered", ChargeDistributeByList.Codes.Weight, testGroupCharge.J7_DistributeBy);
		}

		public void TestRefreshChargeCodeChargeKeyAndApportionChargeKey()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseGroupInvoiceCharge testGroupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			testGroupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("ChargeKey.ChargeType refreshed", CustomsChargeTypeList.Codes.OverseasFreight, testGroupCharge.ChargeKey.ChargeCode);
			AssertEquals("ApportionmentChargeKey.ChargeType refreshed", CustomsChargeTypeList.Codes.OverseasFreight, testGroupCharge.ApportionChargeKey.ChargeKey.ChargeCode);

			testGroupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("ChargeKey.ChargeType refreshed", CustomsChargeTypeList.Codes.OverseasInsurance, testGroupCharge.ChargeKey.ChargeCode);
			AssertEquals("ApportionmentChargeKey.ChargeType refreshed", CustomsChargeTypeList.Codes.OverseasInsurance, testGroupCharge.ApportionChargeKey.ChargeKey.ChargeCode);

			testGroupCharge.J7_IsDutiable = !testGroupCharge.J7_IsDutiable;
			AssertEquals("IsDutiable is refreshed", testGroupCharge.J7_IsDutiable, testGroupCharge.ApportionChargeKey.ChargeKey.IsDutiable);
			AssertEquals("ApportionmentChargeKey.ChargeType", CustomsChargeTypeList.Codes.OverseasInsurance, testGroupCharge.ApportionChargeKey.ChargeKey.ChargeCode);
			AssertEquals("ApportionmentChargeKey.ApportionType", ApportionmentTypeList.Codes.PartialApportionment, testGroupCharge.ApportionChargeKey.ApportionType);

			testGroupCharge.J7_IsGSTApplicable = !testGroupCharge.J7_IsGSTApplicable;
			AssertEquals("IsDutiable is refreshed", testGroupCharge.J7_IsGSTApplicable, testGroupCharge.ApportionChargeKey.ChargeKey.IsVATible);
			AssertEquals("IsDutiable", testGroupCharge.J7_IsDutiable, testGroupCharge.ApportionChargeKey.ChargeKey.IsDutiable);
			AssertEquals("ApportionmentChargeKey.ChargeType", CustomsChargeTypeList.Codes.OverseasInsurance, testGroupCharge.ApportionChargeKey.ChargeKey.ChargeCode);
			AssertEquals("ApportionmentChargeKey.ApportionType", ApportionmentTypeList.Codes.PartialApportionment, testGroupCharge.ApportionChargeKey.ApportionType);

			testGroupCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
			AssertEquals("ApportionmentChargeKey.ChargeType", CustomsChargeTypeList.Codes.OverseasInsurance, testGroupCharge.ApportionChargeKey.ChargeKey.ChargeCode);
			AssertEquals("ApportionmentChargeKey.ApportionType refreshed", ApportionmentTypeList.Codes.FullApportionment, testGroupCharge.ApportionChargeKey.ApportionType);

			testGroupCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;
			AssertEquals("ApportionmentChargeKey.ChargeType", CustomsChargeTypeList.Codes.OverseasInsurance, testGroupCharge.ApportionChargeKey.ChargeKey.ChargeCode);
			AssertEquals("ApportionmentChargeKey.ApportionType refreshed", ApportionmentTypeList.Codes.PartialApportionment, testGroupCharge.ApportionChargeKey.ApportionType);
		}

		public void TestWithKey()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseGroupInvoiceCharge testGroupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			testGroupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			testGroupCharge.J7_IsDutiable = false;
			testGroupCharge.J7_IsGSTApplicable = true;
			testGroupCharge.J7_FullOrPartialApportionment = "";

			AssertEquals("WithKey with ChargeKey", true, testGroupCharge.WithKey(testGroupCharge.ChargeKey));
			AssertEquals("WithKey with AnotherChargeKey", false, testGroupCharge.WithKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, true, true)));

			AssertEquals("WithKey with ApportionChargeKey", true, testGroupCharge.WithKey(testGroupCharge.ApportionChargeKey));
			AssertEquals("WithKey with Another Apportion ChargeKey", false, testGroupCharge.WithKey(new ApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m, false, "", false)));
		}

		public void TestIsFullApportionment()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseGroupInvoiceCharge testGroupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			AssertEquals("IsFullApportionment", false, testGroupCharge.IsFullApportionment);

			testGroupCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;
			AssertEquals("IsFullApportionment", false, testGroupCharge.IsFullApportionment);

			testGroupCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
			AssertEquals("IsFullApportionment", true, testGroupCharge.IsFullApportionment);
		}

		public void TestNoOfDecimalsForPercentage()
		{
			AssertEquals("Number of decimals for percentage", 5, testCharge.NoOfDecimalsForPercentage);
		}

		[ExpectNoExceptions]
		public void TestAccessingParentAfterDeleteDoesNotCauseExceptionsWhenParentIsNull()
		{
			var charge = invoice.Charges.AddNew();
			Factory.Save();
			var newCharge = NewFactory().Load<JobComInvCharge>(charge.PK);
			NUnit.Framework.Assert.That(newCharge.Parent, Is.Not.EqualTo(default(ICommonInvoice)), "Parent - should not be [null]");
			newCharge.Parent = null;
			newCharge.Delete();
			NUnit.Framework.Assert.That(newCharge.Parent, Is.EqualTo(default(ICommonInvoice)), "Parent of Charge after deleted. It does not access J7_ParentTableCode - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestConcurrencyPolicySet()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();

			NUnit.Framework.Assert.That(invoiceCharge.J7_AmountInfo.ConcurrencyPolicy, Is.EqualTo(ConcurrencyPolicy.Default), "Default strategy - not set yet");
			invoiceCharge.J7_IsApportionedCharge = true;
			Factory.Save();
			invoiceCharge.OnSaving();
			NUnit.Framework.Assert.That(invoiceCharge.J7_AmountInfo.ConcurrencyPolicy, Is.EqualTo(ConcurrencyPolicy.Ignore), "Just ignore");
		}

		[ExpectNoExceptions]
		public void TestAllChargesAddedWillHaveTheSameRelationshipWithCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceReload = newFactory.Load<BaseJobComInvoiceHeader>(invoice.PK);
			var collection = invoiceReload.Charges;
			NUnit.Framework.Assert.That(collection[0].Parent, Is.EqualTo(invoiceReload).Using(CustomComparers.TypeComparison));

			var charge2 = newFactory.New<BaseInvoiceCharge>();
			collection.Add(charge2);
			NUnit.Framework.Assert.That(charge2.Parent, Is.EqualTo(invoiceReload).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsThisPartOfTheCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();

			NUnit.Framework.Assert.That(charge.J7_IsApportionedCharge, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "PreCondition:IsApportioned");

			var apportionedCharge = Factory.New<BaseApportionedCharge>();
			apportionedCharge.Parent = invoice;
			NUnit.Framework.Assert.That(apportionedCharge.J7_IsApportionedCharge, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "PreCondition:IsApportioned");

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var invoiceLoaded = factory2.Load<BaseJobComInvoiceHeader>(invoice.PK);
			NUnit.Framework.Assert.That(invoiceLoaded.Charges.Contains(charge.PK), Is.EqualTo(true));
			NUnit.Framework.Assert.That(invoiceLoaded.Charges.Contains(apportionedCharge.PK), Is.EqualTo(false));

			NUnit.Framework.Assert.That(invoiceLoaded.GroupCharges.Contains(charge.PK), Is.EqualTo(false));
			NUnit.Framework.Assert.That(invoiceLoaded.GroupCharges.Contains(apportionedCharge.PK), Is.EqualTo(true));
		}

		#region Implementation

		BaseJobComInvHeaderCharge testCharge;
		BaseJobComInvoiceHeader invoice;
		BaseJobDeclaration testDec;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;

			testCharge = invoice.Charges.AddNew();
		}

		protected virtual void SetIsJ7_ExchangeRateUserEnterable(BaseJobComInvHeaderCharge charge, bool value)
		{
			charge.IsJ7_ExchangeRateUserEnterable = value;
		}

		#endregion
	}
}
