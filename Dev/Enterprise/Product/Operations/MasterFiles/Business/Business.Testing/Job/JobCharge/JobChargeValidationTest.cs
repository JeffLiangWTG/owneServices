using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobChargeValidationTest : BusinessObjectValidationTestCase
	{
		#region Data Refresh Bus Update Validation Tests

		#region JR_GB

		public void TestJR_GBBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = branch2.GB_GC = branch3.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_GB, branch1.PK, branch2.PK, branch3.PK);
		}

		#endregion

		#region JR_GE

		public void TestJR_GEBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();
			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_GE, department1.PK, department2.PK, department3.PK);
		}

		#endregion

		#region JR_AC

		public void TestJR_ACBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_AC, chargeCode1.PK, chargeCode2.PK, chargeCode3.PK);
		}

		#endregion

		#region JR_E6

		public void TestJR_E6BeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			CreateConsolCosts(out BusinessObject consolCost1, out BusinessObject consolCost2, out BusinessObject consolCost3);
			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_E6, consolCost1.PK, consolCost2.PK, consolCost3.PK);
		}

		void CreateConsolCosts(out BusinessObject consolCost1, out BusinessObject consolCost2, out BusinessObject consolCost3)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consolCost1 = Factory.New(ObjectFactory.GetType<IJobConsolCost>(), new Guid("5e256fc4-ab44-495c-aaf8-220bbc8f2e7a"));
			consolCost2 = Factory.New(ObjectFactory.GetType<IJobConsolCost>(), new Guid("10b2cedf-7a82-4053-a36f-1fb1fe181dcb"));
			consolCost3 = Factory.New(ObjectFactory.GetType<IJobConsolCost>(), new Guid("9c6cea1c-f67b-4078-af23-9983cfa1f72f"));
			consolCost1.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost2.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost3.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost1[JobConsolCostSchema.Constants.E6_ParentID] = consol.PK;
				consolCost1[JobConsolCostSchema.E6_ParentTableCode.Name] = consol.TablePrefix;
				consolCost2[JobConsolCostSchema.E6_ParentID.Name] = consol.PK;
				consolCost2[JobConsolCostSchema.E6_ParentTableCode.Name] = consol.TablePrefix;
				consolCost3[JobConsolCostSchema.E6_ParentID.Name] = consol.PK;
				consolCost3[JobConsolCostSchema.E6_ParentTableCode.Name] = consol.TablePrefix;
			}
			finally
			{
				consolCost1.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				consolCost2.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				consolCost3.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCost1[JobConsolCostSchema.E6_AC_ChargeCode.Name] = chargeCode.PK;
			consolCost2[JobConsolCostSchema.E6_AC_ChargeCode.Name] = chargeCode.PK;
			consolCost3[JobConsolCostSchema.E6_AC_ChargeCode.Name] = chargeCode.PK;
			consolCost1[JobConsolCostSchema.E6_GC.Name] = GlbCompany.CurrentCompany.PK;
			consolCost2[JobConsolCostSchema.E6_GC.Name] = GlbCompany.CurrentCompany.PK;
			consolCost3[JobConsolCostSchema.E6_GC.Name] = GlbCompany.CurrentCompany.PK;
			Factory.Save();
		}

		#endregion

		#region JR_OH_CostAccount

		public void TestJR_OH_CostAccountBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_OH_CostAccount, org1.PK, org2.PK, org3.PK);
		}

		#endregion

		#region JR_OH_SellAccount

		public void TestJR_OH_SellAccountBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_OH_SellAccount, org1.PK, org2.PK, org3.PK);
		}

		#endregion

		#region JR_RX_NKCostCurrency

		public void TestJR_RX_NKCostCurrencyBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var currency1 = new ZString(Core.Constants.CurrencyCodes.UnitedStates);
			var currency2 = new ZString(Core.Constants.CurrencyCodes.NewZealand);
			var currency3 = new ZString(Core.Constants.CurrencyCodes.EuropeanUnion);
			Factory.Save();

			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_RX_NKCostCurrency, currency1, currency2, currency3);
		}

		#endregion

		#region JR_RX_NKSellCurrency

		public void TestJR_RX_NKSellCurrencyBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var currency1 = new ZString(Core.Constants.CurrencyCodes.UnitedStates);
			var currency2 = new ZString(Core.Constants.CurrencyCodes.NewZealand);
			var currency3 = new ZString(Core.Constants.CurrencyCodes.EuropeanUnion);
			Factory.Save();

			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_RX_NKSellCurrency, currency1, currency2, currency3);
		}

		#endregion

		#region JR_AL_APLine

		public void TestJR_AL_APLineBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_OSCostAmt, new ZDecimal(100), new ZDecimal(200), new ZDecimal(300));
		}

		#endregion

		#region JR_AL_ARLine

		public void TestJR_AL_ARLineBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_OSSellAmt, new ZDecimal(100), new ZDecimal(200), new ZDecimal(300));
		}

		#endregion

		#region JR_AT_CostGSTRate

		public void TestJR_AT_CostGSTRateBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var taxRate1 = CreateTaxRate("ABC", AccTaxRate.Types.Rated, ZString.Empty);
			var taxRate2 = CreateTaxRate("XYZ", AccTaxRate.Types.Rated, ZString.Empty);
			var taxRate3 = CreateTaxRate("PQR", AccTaxRate.Types.Rated, ZString.Empty);
			Factory.Save();

			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_AT_CostGSTRate, taxRate1.PK, taxRate2.PK, taxRate3.PK);
		}

		#endregion

		#region JR_AT_SellGSTRate

		public void TestJR_AT_SellGSTRateBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var taxRate1 = CreateTaxRate("ABC", AccTaxRate.Types.Rated, ZString.Empty);
			var taxRate2 = CreateTaxRate("XYZ", AccTaxRate.Types.Rated, ZString.Empty);
			var taxRate3 = CreateTaxRate("PQR", AccTaxRate.Types.Rated, ZString.Empty);
			Factory.Save();

			AssertPropertyBeingChangedByDataRefreshBus(JobChargeSchema.Constants.JR_AT_SellGSTRate, taxRate1.PK, taxRate2.PK, taxRate3.PK);
		}

		#endregion

		void AssertPropertyBeingChangedByDataRefreshBus(string propertyName, IZType value1, IZType value2, IZType value3, string associatedPropertyName = null)
		{
			var subscriberFactory = new BusinessObjectFactory();
			var publisherFactory = new BusinessObjectFactory();

			var subscriberCharge = subscriberFactory.NewWithValidTestData<JobCharge>();
			subscriberCharge[propertyName] = value1;
			if (propertyName == JobChargeSchema.Constants.JR_RX_NKSellCurrency)
			{
				var exchangeRates = (BusinessObjectCollection)subscriberCharge.Job["ExchangeRates"];
				var exRate = exchangeRates.First();
				exRate["JF_IsTransformed"] = true; //To avoid deleting this exRate when we change sell currency later.
			}
			subscriberFactory.Save();

			var publisherCharge = publisherFactory.Load<JobCharge>(subscriberCharge.PK);
			publisherCharge[propertyName] = value2;

			Assert(!subscriberCharge.HasChanges);
			publisherFactory.Save();

			subscriberCharge.RunPreSaveValidation();
			var expectedErrorMessage = "This record was modified by this user during another operation. Please cancel your changes and reload the form.";
			AssertNoRowError(subscriberCharge, expectedErrorMessage);
			Assert(!subscriberCharge.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet()));

			publisherCharge[propertyName] = value3;
			subscriberCharge.JR_Desc = "Test";
			Assert(subscriberCharge.HasChanges);

			publisherFactory.Save();
			subscriberCharge.RunPreSaveValidation();

			AssertHasRowError(subscriberCharge, expectedErrorMessage);

			Assert(subscriberCharge.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
		}

		#endregion

		public void TestJobChargeCheckJR_OSCostExRate()
		{
			var charge = Factory.New<JobCharge>();
			AssertEquals("AUD", charge.JR_RX_NKCostCurrency);
			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(1m, charge.JR_OSCostExRate);
			charge.Validation.ValidateJR_OSCostExRate();
			AssertNoErrors(charge.JR_OSCostExRateInfo);

			charge.JR_RX_NKCostCurrency = "USD";
			AssertNotEquals(charge.JR_RX_NKCostCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			charge.JR_OSCostExRate = 1.2m;
			charge.Validation.ValidateJR_OSCostExRate();
			AssertNoErrors(charge.JR_OSCostExRateInfo);
			charge.JR_OSCostExRate = 1m;
			charge.Validation.ValidateJR_OSCostExRate();
			AssertNoErrors(charge.JR_OSCostExRateInfo);

			charge.JR_RX_NKCostCurrency = "AUD";
			AssertEquals(charge.JR_RX_NKCostCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			charge.JR_OSCostExRate = 1.2m;
			charge.Validation.ValidateJR_OSCostExRate();
			var expectedMessage = @"Cost exchange rate has failed to update. When cost currency is local currency, cost exchange rate must equal 1. 
Please manually refresh the cost exchange rate by temporarily changing the cost currency to a foreign currency then saving your changes. 
Then change the cost currency to the currency of your choice.";
			AssertHasError(charge.JR_OSCostExRateInfo, expectedMessage);

			charge.JR_OSCostExRate = 0m;
			AssertEquals(charge.JR_RX_NKCostCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			charge.Validation.ValidateJR_OSCostExRate();
			AssertHasError(charge.JR_OSCostExRateInfo, expectedMessage);
		}

		public void TestJobChargeCheckJR_LocalCostAmt()
		{
			var maximumAllowedLineAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedLineAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				Assert("Precondition: JR_LocalCostAmt is less than registry setting.", Math.Abs(charge.JR_LocalCostAmt) < maximumAllowedLineAmount);

				charge.Validation.ValidateJR_LocalCostAmt();
				AssertNoErrors(charge.JR_LocalCostAmtInfo);

				charge.JR_LocalCostAmt = 51M;
				Assert("Precondition: JR_LocalCostAmt is greater than registry setting.", Math.Abs(charge.JR_LocalCostAmt) > maximumAllowedLineAmount);

				charge.Validation.ValidateJR_LocalCostAmt();

				var maximumAllowedLineAmountString = registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals());
				var expectedMessage = $"The job charge amount must be between -{maximumAllowedLineAmountString} and {maximumAllowedLineAmountString} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

				AssertHasError(charge.JR_LocalCostAmtInfo, expectedMessage);

				PostCostCharge(charge);
				charge.Validation.ValidateJR_LocalCostAmt();

				AssertNoErrors(charge.JR_LocalCostAmtInfo);
			}
		}

		public void TestJobChargeCheckJR_LocalSellAmt()
		{
			var maximumAllowedLineAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedLineAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				Assert("Precondition: JR_LocalSellAmt is less than registry setting.", Math.Abs(charge.JR_LocalSellAmt) < maximumAllowedLineAmount);

				charge.Validation.ValidateJR_LocalSellAmt();
				AssertNoErrors(charge.JR_LocalSellAmtInfo);

				charge.JR_LocalSellAmt = 51M;
				Assert("Precondition: JR_LocalSellAmt is greater than registry setting.", Math.Abs(charge.JR_LocalSellAmt) > maximumAllowedLineAmount);

				charge.Validation.ValidateJR_LocalSellAmt();

				var maximumAllowedLineAmountString = registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals());
				var expectedMessage = $"The job charge amount must be between -{maximumAllowedLineAmountString} and {maximumAllowedLineAmountString} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

				AssertHasError(charge.JR_LocalSellAmtInfo, expectedMessage);

				var line = Factory.NewWithValidTestData<AccTransactionLines>();
				line.AL_LineType = TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = line.PK;

				AssertEquals("Precondition", true, charge.IsRevenuePosted);

				charge.Validation.ValidateJR_LocalSellAmt();

				AssertNoErrors(charge.JR_LocalSellAmtInfo);
			}
		}

		public void TestJobChargeCheckJR_OSSellExRateFromAnotherCompanyWithDifferentCurrency()
		{
			Action<JobCharge, decimal> setExRateProperly = (c, r) =>
			{
				var prop = c.GetType().GetProperty("RevenueExchangeRate");
				var revenueExchangeRate = prop.GetValue(c);
				var method = revenueExchangeRate.GetType().GetMethod("SetBuyRate_ForTestOnly");
				method.Invoke(revenueExchangeRate, new object[] { r });
			};

			var companyACurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var companyBQuery = new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, companyACurrency);
			var companyB = Factory.LoadTop1<GlbCompany>(companyBQuery);
			JobCharge charge;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, companyB.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var companyBFactory = new BusinessObjectFactory();
				var jobHeader = companyBFactory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "001";
				charge = companyBFactory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = jobHeader.PK;
				charge.JR_AC = CreateChargeCode(Core.Constants.ChargeType.Margin).PK;
				charge.JR_RX_NKSellCurrency = companyACurrency;
				setExRateProperly(charge, 1.2m);
				companyBFactory.Save();

				AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKSellCurrency);
				AssertNotEquals(1m, charge.JR_OSSellExRate);
			}

			var chargeReloaded = Factory.Load<JobCharge>(charge.PK);
			chargeReloaded.JR_RX_NKSellCurrency = "USD"; //exchange rate can't be not equal 1 for a local currency
			setExRateProperly(chargeReloaded, 1.3m);

			AssertEquals(companyB.PK, chargeReloaded.Company.PK);
			AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, chargeReloaded.JR_RX_NKSellCurrency);
			AssertNotEquals(1m, chargeReloaded.JR_OSSellExRate);
			Assert(chargeReloaded.JR_OSSellExRateInfo.HasChanges);

			chargeReloaded.Validation.ValidateJR_OSSellExRate();
			AssertNoErrors(chargeReloaded.JR_OSSellExRateInfo);
		}

		public void TestJobChargeCheckJR_OSCostExRateFromAnotherCompanyWithDifferentCurrency()
		{
			var companyACurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var companyBQuery = new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, companyACurrency);
			var companyB = Factory.LoadTop1<GlbCompany>(companyBQuery);
			JobCharge charge;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, companyB.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var companyBFactory = new BusinessObjectFactory();
				var jobHeader = companyBFactory.NewJobForTesting<JobHeader>();
				jobHeader.JH_JobNum = "001";
				charge = companyBFactory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = jobHeader.PK;
				charge.JR_AC = CreateChargeCode(Core.Constants.ChargeType.Margin).PK;
				charge.JR_RX_NKCostCurrency = companyACurrency;
				charge.JR_OSCostExRate = 1.2m;
				companyBFactory.Save();

				AssertNotEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKCostCurrency);
				AssertNotEquals(1m, charge.JR_OSCostExRate);
			}

			var chargeReloaded = Factory.Load<JobCharge>(charge.PK);
			chargeReloaded.JR_OSCostExRate = 1.3m;

			AssertEquals(companyB.PK, chargeReloaded.Company.PK);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, chargeReloaded.JR_RX_NKCostCurrency);
			AssertNotEquals(1m, chargeReloaded.JR_OSCostExRate);
			Assert(chargeReloaded.JR_OSCostExRateInfo.HasChanges);

			chargeReloaded.Validation.ValidateJR_OSCostExRate();
			AssertNoErrors(chargeReloaded.JR_OSCostExRateInfo);
		}

		public void TestJobChargeCheckJR_OSSellExRate()
		{
			var charge = Factory.New<JobCharge>();
			AssertEquals("AUD", charge.JR_RX_NKSellCurrency);
			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(1m, charge.JR_OSSellExRate);
			charge.Validation.ValidateJR_OSSellExRate();
			AssertNoErrors(charge.JR_OSSellExRateInfo);

			charge.JR_RX_NKSellCurrency = "USD";
			AssertNotEquals(charge.JR_RX_NKSellCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			charge.JR_OSSellExRate = 1.2m;
			charge.Validation.ValidateJR_OSSellExRate();
			AssertNoErrors(charge.JR_OSSellExRateInfo);
			charge.JR_OSSellExRate = 1m;
			charge.Validation.ValidateJR_OSSellExRate();
			AssertNoErrors(charge.JR_OSSellExRateInfo);

			charge.JR_OSSellExRate = 1.2m;
			charge.JR_RX_NKSellCurrency = "AUD";
			AssertEquals(charge.JR_RX_NKSellCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("Exchange rate should be set to 1m", 1m, charge.JR_OSSellExRate);
		}

		public void TestCheckJR_OA_SellInvoiceAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			var addressForAnotherOrg = Factory.New<OrgAddress>();
			addressForAnotherOrg.OA_OH = ZGuid.NewZGuid();

			var charge = Factory.New<JobCharge>();
			charge.JR_OH_SellAccount = org.PK;

			charge.JR_OA_SellInvoiceAddress = address.PK;
			AssertNoErrors(charge.JR_OA_SellInvoiceAddressInfo);

			charge.JR_OA_SellInvoiceAddress = addressForAnotherOrg.PK;
			AssertHasError(charge.JR_OA_SellInvoiceAddressInfo, "The Sell Address must belong to the Debtor.");

			charge.JR_OA_SellInvoiceAddress = ZGuid.Empty;
			AssertNoErrors(charge.JR_OA_SellInvoiceAddressInfo);
		}

		public void TestCheckJR_OC_SellInvoiceContact()
		{
			var org = Factory.New<OrgHeader>();
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			var contactForAnotherOrg = Factory.New<OrgContact>();
			contactForAnotherOrg.OC_OH = ZGuid.NewZGuid();

			var charge = Factory.New<JobCharge>();
			charge.JR_OH_SellAccount = org.PK;

			charge.JR_OC_SellInvoiceContact = contact.PK;
			AssertNoErrors(charge.JR_OC_SellInvoiceContactInfo);

			charge.JR_OC_SellInvoiceContact = contactForAnotherOrg.PK;
			AssertHasError(charge.JR_OC_SellInvoiceContactInfo, "The Sell Contact must belong to the Debtor.");

			charge.JR_OC_SellInvoiceContact = ZGuid.Empty;
			AssertNoErrors(charge.JR_OC_SellInvoiceContactInfo);
		}

		public void TestCheckJR_APInvoiceNum()
		{
			var charge = Factory.New<JobCharge>();
			charge.JR_OSCostAmt = 10m;
			AssertNoErrors(charge.JR_APInvoiceNumInfo);
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_InvoiceNum] = "ABC123";

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_APInvoiceNum = "BBC000";
			AssertHasErrors(charge.JR_APInvoiceNumInfo);
			AssertHasError(charge.JR_APInvoiceNumInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_APInvoiceNum = "ABC123";
			AssertNoErrors(charge.JR_APInvoiceNumInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_APInvoiceNum = "BBC000";
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_APInvoiceNumInfo);
		}

		public void TestCheckJR_APInvoiceDate()
		{
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_APInvoiceDateInfo);
			ZDateTime today = ZDateTime.Today;
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_InvoiceDate] = today;

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_APInvoiceDate = today.AddDays(3);
			AssertHasErrors(charge.JR_APInvoiceDateInfo);
			AssertHasError(charge.JR_APInvoiceDateInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_APInvoiceDate = today;
			AssertNoErrors(charge.JR_APInvoiceDateInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_APInvoiceDate = today.AddDays(3);
			charge.Validation.ValidateJR_APInvoiceDate();
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_APInvoiceDateInfo);
		}

		public void TestCheckJR_APDocumentReceivedDate()
		{
			var charge = Factory.New<JobCharge>();
			charge.JR_APInvoiceNum = "Test001";
			AssertNoErrors(charge.JR_APInvoiceDateInfo);
			var today = ZDateTime.Today;
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_DocumentReceivedDate] = today;

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_APDocumentReceivedDate = today.AddDays(3);
			AssertHasErrors(charge.JR_APDocumentReceivedDateInfo);
			AssertHasError(charge.JR_APDocumentReceivedDateInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_APDocumentReceivedDate = today;
			AssertNoErrors(charge.JR_APDocumentReceivedDateInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_APDocumentReceivedDate = today.AddDays(3);
			AssertNoErrors(charge.JR_APDocumentReceivedDateInfo);
		}

		public void TestCheckJR_PaymentDate()
		{
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_PaymentDateInfo);
			ZDateTime today = ZDateTime.Today;
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_PaymentDate] = today;

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_PaymentDate = today.AddDays(3);
			AssertHasErrors(charge.JR_PaymentDateInfo);
			AssertHasError(charge.JR_PaymentDateInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_PaymentDate = today;
			AssertNoErrors(charge.JR_PaymentDateInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_PaymentDate = today.AddDays(3);
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_PaymentDateInfo);
		}

		public void TestCheckJR_OH_CostAccount()
		{
			var charge = Factory.New<JobCharge>();
			var org = Factory.New<OrgHeader>();
			var otherOrg = Factory.New<OrgHeader>();
			AssertNoErrors(charge.JR_OH_CostAccountInfo);
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_OH_Creditor] = org.PK;

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_OH_CostAccount = otherOrg.PK;
			AssertHasErrors(charge.JR_OH_CostAccountInfo);
			AssertHasError(charge.JR_OH_CostAccountInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_OH_CostAccount = org.PK;
			AssertNoErrors(charge.JR_OH_CostAccountInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_OH_CostAccount = otherOrg.PK;
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_OH_CostAccountInfo);
		}

		public void TestCheckJR_CostReference()
		{
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_CostReferenceInfo);
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_CostReference] = "ABC123";

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_CostReference = "BBC000";
			AssertHasErrors(charge.JR_CostReferenceInfo);
			AssertHasError(charge.JR_CostReferenceInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_CostReference = "ABC123";
			AssertNoErrors(charge.JR_CostReferenceInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_CostReference = "BBC000";
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_CostReferenceInfo);
		}

		public void TestCheckJR_AT_CostGSTRate()
		{
			var charge = Factory.New<JobCharge>();
			var tax = Factory.New<AccTaxRate>();
			var otherTax = Factory.New<AccTaxRate>();
			AssertNoErrors(charge.JR_OH_CostAccountInfo);
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_AT_TaxRate] = tax.PK;

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_AT_CostGSTRate = otherTax.PK;
			AssertHasErrors(charge.JR_AT_CostGSTRateInfo);
			AssertHasError(charge.JR_AT_CostGSTRateInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_AT_CostGSTRate = tax.PK;
			AssertNoErrors(charge.JR_AT_CostGSTRateInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_AT_CostGSTRate = otherTax.PK;
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_AT_CostGSTRateInfo);
		}

		public void TestCheckJR_A9_CostVATClass()
		{
			var charge = Factory.New<JobCharge>();
			var taxClass = Factory.New<AccInvMsg>();
			var otherTaxClass = Factory.New<AccInvMsg>();
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_A9_VATClass] = taxClass.PK;

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_A9_CostVATClass = otherTaxClass.PK;
			AssertHasErrors(charge.JR_A9_CostVATClassInfo);
			AssertHasError(charge.JR_A9_CostVATClassInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

			charge.JR_A9_CostVATClass = taxClass.PK;
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_A9_CostVATClass = otherTaxClass.PK;
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_A9_CostVATClassInfo);
		}

		public void TestCheckJR_CostTaxDate()
		{
			var validCostTaxDate = ZDate.Today;
			var invalidCostTaxDate = ZDate.Today.AddDays(3);
			var expectedErrorMessage = "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.";

			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_CostTaxDateInfo);

			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_TaxDate] = validCostTaxDate;

			Assert("Precondition: IschargePosted", !charge.IsCostPosted);

			charge.JR_CostTaxDate = invalidCostTaxDate;
			AssertHasErrors(charge.JR_CostTaxDateInfo);
			AssertHasError(charge.JR_CostTaxDateInfo, expectedErrorMessage);

			charge.JR_CostTaxDate = validCostTaxDate;
			AssertNoErrors(charge.JR_CostTaxDateInfo);

			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = TransactionLineTypes.Cost;

			charge.JR_AL_APLine = apLine.PK;
			Assert("Precondition: IschargePosted", charge.IsCostPosted);

			charge.JR_CostTaxDate = invalidCostTaxDate;
			AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_CostTaxDateInfo);
		}

		public void TestCheckJR_CostPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.New<JobCharge>();
				AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);
				var cost = CreateParentConsolCost(charge);
				cost[JobConsolCostSchema.Constants.E6_PlaceOfSupply] = "DL";

				Assert("Precondition: IschargePosted", !charge.IsCostPosted);

				charge.JR_CostPlaceOfSupply = "JH";
				AssertHasErrors(charge.JR_CostPlaceOfSupplyInfo);
				AssertHasError(charge.JR_CostPlaceOfSupplyInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

				charge.JR_CostPlaceOfSupply = "DL";
				AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);

				var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
				apLine.AL_LineType = TransactionLineTypes.Cost;

				charge.JR_AL_APLine = apLine.PK;
				Assert("Precondition: IschargePosted", charge.IsCostPosted);

				charge.JR_CostPlaceOfSupply = "BR";
				AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_CostPlaceOfSupplyInfo);

				charge.JR_CostPlaceOfSupply = "XX";
				AssertHasError(charge.JR_CostPlaceOfSupplyInfo, "Enter a valid Cost Fixed Place of Supply.");

				charge.JR_CostPlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				charge.JR_CostPlaceOfSupply = "";
				AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);
				Assert("Type must be reset to empty string when Place is set to empty", charge.JR_CostPlaceOfSupplyType.IsEmpty);
				AssertNoErrors(charge.JR_CostPlaceOfSupplyTypeInfo);
			}
		}

		public void TestCheckJR_CostPlaceOfSupply_NotValidated_WhenRegistryOff()
		{
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);
			var cost = CreateParentConsolCost(charge);
			cost[JobConsolCostSchema.Constants.E6_PlaceOfSupply] = "DL";

			charge.JR_CostPlaceOfSupply = "DL";
			AssertEquals("JR_CostPlaceOfSupply should be readonly when FPOS not enforced in registry", true, charge.JR_CostPlaceOfSupplyInfo.ReadOnly);
			AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);

			charge.JR_CostPlaceOfSupply = "XX";
			AssertNoErrors("Should not validate when FPOS not enforced in registry", charge.JR_CostPlaceOfSupplyInfo);
		}

		public void TestCheckJR_CostPlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.New<JobCharge>();
				AssertNoErrors(charge.JR_CostPlaceOfSupplyTypeInfo);
				var cost = CreateParentConsolCost(charge);
				cost[JobConsolCostSchema.Constants.E6_PlaceOfSupplyType] = PlaceOfSupplyTypes.PredefinedRule.Code;

				Assert("Precondition: IschargePosted", !charge.IsCostPosted);

				charge.JR_CostPlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				AssertHasErrors(charge.JR_CostPlaceOfSupplyTypeInfo);
				AssertHasError(charge.JR_CostPlaceOfSupplyTypeInfo, "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.");

				charge.JR_CostPlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				AssertNoErrors(charge.JR_CostPlaceOfSupplyTypeInfo);

				var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
				apLine.AL_LineType = TransactionLineTypes.Cost;

				charge.JR_AL_APLine = apLine.PK;
				Assert("Precondition: IschargePosted", charge.IsCostPosted);

				charge.JR_CostPlaceOfSupplyType = "";
				AssertNoErrors("Do not add error on posted ConsolCost", charge.JR_CostPlaceOfSupplyTypeInfo);

				charge.JR_CostPlaceOfSupplyType = "XXX";
				AssertHasError(charge.JR_CostPlaceOfSupplyTypeInfo, "Enter a valid selection.");

				charge.JR_CostPlaceOfSupply = "DL";
				AssertEquals(PlaceOfSupplyTypes.State.Code, charge.JR_CostPlaceOfSupplyType);
				AssertNoErrors("STA is a valid PlaceOfSupplyTypes", charge.JR_CostPlaceOfSupplyTypeInfo);

				charge.JR_CostPlaceOfSupplyType = "";
				AssertHasError(charge.JR_CostPlaceOfSupplyTypeInfo, "Please enter a value.");
				AssertEquals("Place is not reset on resetting Type", "DL", charge.JR_CostPlaceOfSupply);
				AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);
			}
		}

		public void TestCheckJR_CostPlaceOfSupplyType_NotValidated_WhenRegistryOff()
		{
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);

			charge.JR_SellPlaceOfSupplyType = "XXX";
			AssertNoErrors(charge.JR_CostPlaceOfSupplyInfo);
		}

		public void TestCheckJR_SellPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.New<JobCharge>();
				AssertNoErrors(charge.JR_SellPlaceOfSupplyInfo);

				charge.JR_SellPlaceOfSupply = "XX";
				AssertHasError(charge.JR_SellPlaceOfSupplyInfo, "Enter a valid Sell Fixed Place of Supply.");

				charge.JR_SellPlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				charge.JR_SellPlaceOfSupply = "";
				AssertNoErrors(charge.JR_SellPlaceOfSupplyInfo);
				Assert("Type must be reset to empty string when Place is set to empty", charge.JR_SellPlaceOfSupplyType.IsEmpty);
				AssertNoErrors(charge.JR_SellPlaceOfSupplyTypeInfo);
			}
		}

		public void TestCheckJR_SellPlaceOfSupply_NotValidated_WhenRegistryOff()
		{
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_SellPlaceOfSupplyInfo);

			charge.JR_CostPlaceOfSupply = "XX";
			AssertNoErrors(charge.JR_SellPlaceOfSupplyInfo);
		}

		public void TestCheckJR_SellPlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.New<JobCharge>();
				AssertNoErrors(charge.JR_SellPlaceOfSupplyTypeInfo);

				charge.JR_SellPlaceOfSupplyType = "XXX";
				AssertHasError(charge.JR_SellPlaceOfSupplyTypeInfo, "Enter a valid selection.");

				charge.JR_SellPlaceOfSupply = "DL";
				AssertEquals(PlaceOfSupplyTypes.State.Code, charge.JR_SellPlaceOfSupplyType);
				AssertNoErrors("STA is a valid PlaceOfSupplyTypes", charge.JR_SellPlaceOfSupplyTypeInfo);

				charge.JR_SellPlaceOfSupplyType = "";
				AssertHasError(charge.JR_SellPlaceOfSupplyTypeInfo, "Please enter a value.");
				AssertEquals("Place is not reset on resetting Type", "DL", charge.JR_SellPlaceOfSupply);
				AssertNoErrors(charge.JR_SellPlaceOfSupplyInfo);
			}
		}

		public void TestCheckJR_SellPlaceOfSupplyType_NotValidated_WhenRegistryIfNotSet()
		{
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_SellPlaceOfSupplyTypeInfo);

			charge.JR_SellPlaceOfSupplyType = "XXX";
			AssertNoErrors(charge.JR_SellPlaceOfSupplyTypeInfo);
		}

		public void TestJCCheckJR_LocalCostAmt()
		{
			var expectedErrorMessage = "Local amount cannot be zero when Overseas Cost Amount is non zero.";
			var charge = Factory.New<JobCharge>();
			AssertNoErrors(charge.JR_LocalCostAmtInfo);

			charge.JR_OSCostAmt = 210m;
			charge.JR_OSCostExRate = 0m;
			Assert(charge.JR_LocalCostAmt.IsEmpty);
			charge.Validation.ValidateJR_LocalCostAmt();
			AssertHasError(charge.JR_LocalCostAmtInfo, expectedErrorMessage);

			charge.JR_LocalCostAmt = 140m;
			charge.Validation.ValidateJR_LocalCostAmt();
			AssertEquals("Expected charge not to be posted", false, charge.IsCostPosted);
			AssertNoError("Expected no error now that local cost has a value", charge.JR_LocalCostAmtInfo, expectedErrorMessage);

			PostCostCharge(charge);
			charge.JR_LocalCostAmt = 0m;

			AssertNoError("Posted cost should not display error", charge.JR_LocalCostAmtInfo, expectedErrorMessage);
		}

		public void TestCheckJR_CostGovtChargeCode()
		{
			var mrgChargeCode = CreateChargeCode(Core.Constants.ChargeType.Margin);
			var cmtChargeCode = CreateChargeCode(Core.Constants.ChargeType.Comment);

			var gstTaxRate = CreateTaxRate("xGST", AccTaxRate.Types.Rated, AccTaxRate.ExtraTypes.StateGST);

			AccTaxRate serviceTax = null;
			AccTaxRate serviceNOTTax = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				serviceTax = CreateTaxRate("SER0", AccTaxRate.Types.ServiceTax, string.Empty);
				serviceNOTTax = CreateTaxRate("SERNOT", AccTaxRate.Types.NotReportable, AccTaxRate.ExtraTypes.ServiceTax);
			}

			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var expectedErrorMessage = "Please enter a Cost Government Charge Code.";
					var expectedWarningMessage = "Cost Government Charge Code is empty.";
					var charge = CreateJobCharge();
					AssertNoErrors(charge.JR_CostGovtChargeCodeInfo);

					//setup charge
					charge.JR_AC = mrgChargeCode.PK;
					charge.JR_AT_CostGSTRate = gstTaxRate.PK;

					charge.JR_CostGovtChargeCode = "GVTCC1";
					AssertNoErrors(charge.JR_CostGovtChargeCodeInfo);

					charge.JR_CostGovtChargeCode = "";

					if (enableGovtChargeCode)
					{
						AssertHasError(charge.JR_CostGovtChargeCodeInfo, expectedErrorMessage);

						var previousChargeCodePk = charge.JR_AC;
						var previousTaxId = charge.JR_AT_CostGSTRate;

						var transactionLine = Factory.New<AccTransactionLines>();
						charge.JR_AL_ARLine = transactionLine.PK;

						transactionLine.AL_LineType = TransactionLineTypes.Revenue;

						var header = Factory.New<AccTransactionHeader>();
						header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
						header.AH_InvoiceDate = ZDateTime.Now;
						transactionLine.AL_AH = header.PK;
						AssertEquals("Precondition: is posted with job revenue journal", true, charge.IsRevenuePostedWithManualJobRevenueJournal);

						charge.JR_CostGovtChargeCode = "";

						AssertNoErrors("Validation should skip cost posted with job revenue journal", charge.JR_CostGovtChargeCodeInfo);

						transactionLine.AL_LineType = TransactionLineTypes.WIP;
						transactionLine.AL_ReverseDate = ZDate.Today;
						transactionLine.AL_AH = ZGuid.Empty;
						charge.JR_AL_ARLine = ZGuid.Empty; //revert to previous state
						charge.Validation.ValidateJR_CostGovtChargeCode();
						AssertHasError(charge.JR_CostGovtChargeCodeInfo, expectedErrorMessage);

						charge.JR_AC = cmtChargeCode.PK;
						AssertNoErrors("field is optional if comment charge code is linked to job charge", charge.JR_CostGovtChargeCodeInfo);
						AssertHasWarning(charge.JR_CostGovtChargeCodeInfo, expectedWarningMessage);

						charge.JR_AC = previousChargeCodePk; //revert to previous state
						charge.JR_AT_CostGSTRate = previousTaxId;
						AssertHasError(charge.JR_CostGovtChargeCodeInfo, expectedErrorMessage);

						charge.JR_AT_CostGSTRate = ZGuid.Empty;
						AssertNoErrors("field is optional if no tax id is selected", charge.JR_CostGovtChargeCodeInfo);
						AssertHasWarning(charge.JR_CostGovtChargeCodeInfo, expectedWarningMessage);

						charge.JR_AT_CostGSTRate = previousTaxId; //revert to previous state
						AssertHasError(charge.JR_CostGovtChargeCodeInfo, expectedErrorMessage);

						using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
						{
							charge.JR_AT_CostGSTRate = serviceTax.PK;
							AssertNoErrors("field is optional if tax is service tax", charge.JR_CostGovtChargeCodeInfo);
							AssertHasWarning(charge.JR_CostGovtChargeCodeInfo, expectedWarningMessage);

							charge.JR_AT_CostGSTRate = previousTaxId; //revert to previous state
							AssertHasError(charge.JR_CostGovtChargeCodeInfo, expectedErrorMessage);

							charge.JR_AT_CostGSTRate = serviceNOTTax.PK;
							AssertNoErrors("field is optional if tax is service tax", charge.JR_CostGovtChargeCodeInfo);
							AssertHasWarning(charge.JR_CostGovtChargeCodeInfo, expectedWarningMessage);
						}
					}
					else
					{
						AssertNoErrors(charge.JR_CostGovtChargeCodeInfo);
					}
				}
			}
		}

		public void TestCheckJR_SellGovtChargeCode()
		{
			var mrgChargeCode = CreateChargeCode(Core.Constants.ChargeType.Margin);
			var cmtChargeCode = CreateChargeCode(Core.Constants.ChargeType.Comment);

			var gstTaxRate = CreateTaxRate("xGST", AccTaxRate.Types.Rated, AccTaxRate.ExtraTypes.StateGST);

			AccTaxRate serviceTax = null;
			AccTaxRate serviceNOTTax = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				serviceTax = CreateTaxRate("SER0", AccTaxRate.Types.ServiceTax, string.Empty);
				serviceNOTTax = CreateTaxRate("SERNOT", AccTaxRate.Types.NotReportable, AccTaxRate.ExtraTypes.ServiceTax);
			}

			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var expectedErrorMessage = "Please enter a Sell Government Charge Code.";
					var expectedWarningMessage = "Sell Government Charge Code is empty.";
					var charge = CreateJobCharge();
					AssertNoErrors(charge.JR_SellGovtChargeCodeInfo);

					//setup charge
					charge.JR_AC = mrgChargeCode.PK;
					charge.JR_AT_SellGSTRate = gstTaxRate.PK;

					charge.JR_SellGovtChargeCode = "GVTCC1";
					AssertNoErrors(charge.JR_SellGovtChargeCodeInfo);

					charge.JR_SellGovtChargeCode = "";

					if (enableGovtChargeCode)
					{
						AssertHasError(charge.JR_SellGovtChargeCodeInfo, expectedErrorMessage);

						var previousChargeCodePk = charge.JR_AC;
						var previousTaxId = charge.JR_AT_SellGSTRate;

						var line = Factory.New<AccTransactionLines>();
						var header = Factory.New<AccTransactionHeader>();

						charge.JR_AL_ARLine = line.PK;
						line.AL_AH = header.PK;

						header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
						AssertEquals("Precondition: revenue posted with manual job revenue journal", true, charge.IsRevenuePostedWithManualJobRevenueJournal);
						charge.JR_SellGovtChargeCode = "";
						AssertNoErrors("Validation should skip revenue posted with job revenue journal", charge.JR_SellGovtChargeCodeInfo);

						header.AH_TransactionType = TransactionTypes.Invoice;
						charge.JR_SellGovtChargeCode = "";
						AssertHasError(charge.JR_SellGovtChargeCodeInfo, expectedErrorMessage);

						header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
						header.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
						AssertEquals("Precondition: revenue posted with auto job revenue journal", true, charge.IsRevenuePostedWithAutoJobRevenueJournal);

						charge.JR_SellGovtChargeCode = "";
						AssertNoErrors("Validation should skip revenue posted with auto job revenue journal", charge.JR_SellGovtChargeCodeInfo);

						line.AL_LineType = TransactionLineTypes.WIP;
						line.AL_ReverseDate = ZDate.Today;
						line.AL_AH = ZGuid.Empty;
						charge.JR_AL_ARLine = ZGuid.Empty; //revert to previous state
						charge.Validation.ValidateJR_SellGovtChargeCode();
						AssertHasError(charge.JR_SellGovtChargeCodeInfo, expectedErrorMessage);

						charge.JR_AC = cmtChargeCode.PK;
						AssertNoErrors("field is optional if comment charge code is linked to job charge", charge.JR_SellGovtChargeCodeInfo);
						AssertHasWarning(charge.JR_SellGovtChargeCodeInfo, expectedWarningMessage);

						charge.JR_AC = previousChargeCodePk; //revert to previous state
						charge.JR_AT_SellGSTRate = previousTaxId;
						AssertHasError(charge.JR_SellGovtChargeCodeInfo, expectedErrorMessage);

						charge.JR_AT_SellGSTRate = ZGuid.Empty;
						AssertNoErrors("field is optional if no tax id is selected", charge.JR_SellGovtChargeCodeInfo);
						AssertHasWarning(charge.JR_SellGovtChargeCodeInfo, expectedWarningMessage);

						charge.JR_AT_SellGSTRate = previousTaxId; //revert to previous state
						AssertHasError(charge.JR_SellGovtChargeCodeInfo, expectedErrorMessage);

						using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
						{
							charge.JR_AT_SellGSTRate = serviceTax.PK;
							AssertNoErrors("field is optional if tax is service tax", charge.JR_SellGovtChargeCodeInfo);
							AssertHasWarning(charge.JR_SellGovtChargeCodeInfo, expectedWarningMessage);

							charge.JR_AT_SellGSTRate = previousTaxId; //revert to previous state
							AssertHasError(charge.JR_SellGovtChargeCodeInfo, expectedErrorMessage);

							charge.JR_AT_SellGSTRate = serviceNOTTax.PK;
							AssertNoErrors("field is optional if tax is service tax", charge.JR_SellGovtChargeCodeInfo);
							AssertHasWarning(charge.JR_SellGovtChargeCodeInfo, expectedWarningMessage);
						}
					}
					else
					{
						AssertNoErrors(charge.JR_SellGovtChargeCodeInfo);
					}
				}
			}
		}

		void CheckJR_CostGovtChargeCode_NoErrorsWhenPosted(ZGuid mrgChargeCode, ZGuid gstTaxRate)
		{
			const string warning = "Cost Government Charge Code is empty.";
			const string error = "Please enter a Cost Government Charge Code.";

			var charge = CreateJobCharge();
			charge.JR_AC = mrgChargeCode;
			charge.JR_AT_CostGSTRate = gstTaxRate;
			charge.JR_CostGovtChargeCode = string.Empty;
			AssertEquals("Not posted", false, charge.IsCostPosted);

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				charge.Validation.ValidateJR_CostGovtChargeCode();
				if (gstTaxRate.IsEmpty)
				{
					AssertHasWarning("Unposted empty JR_CostGovtChargeCode", charge.JR_CostGovtChargeCodeInfo, warning);
				}
				else
				{
					AssertHasError("Unposted empty JR_CostGovtChargeCode", charge.JR_CostGovtChargeCodeInfo, error);
				}
			}

			PostCostCharge(charge);

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				charge.Validation.ValidateJR_CostGovtChargeCode();
				AssertEquals(string.Empty, charge.JR_CostGovtChargeCode);
				AssertNoError("Posted JR_CostGovtChargeCode", charge.JR_CostGovtChargeCodeInfo, error);
				AssertNoWarning("Posted JR_CostGovtChargeCode", charge.JR_CostGovtChargeCodeInfo, warning);
			}
		}

		public void TestCheckJR_CostGovtChargeCode_NoErrorsWhenPosted()
		{
			var mrgChargeCode = CreateChargeCode(Core.Constants.ChargeType.Margin);
			CheckJR_CostGovtChargeCode_NoErrorsWhenPosted(mrgChargeCode.PK, ZGuid.Empty);

			var gstTaxRate = CreateTaxRate("xGST", AccTaxRate.Types.Rated, AccTaxRate.ExtraTypes.StateGST);
			CheckJR_CostGovtChargeCode_NoErrorsWhenPosted(mrgChargeCode.PK, gstTaxRate.PK);
		}

		void CheckJR_SellGovtChargeCode_NoErrorsWhenPosted(ZGuid mrgChargeCode, ZGuid gstTaxRate)
		{
			const string warning = "Sell Government Charge Code is empty.";
			const string error = "Please enter a Sell Government Charge Code.";

			var charge = CreateJobCharge();
			charge.JR_AC = mrgChargeCode;
			charge.JR_AT_SellGSTRate = gstTaxRate;
			charge.JR_SellGovtChargeCode = string.Empty;
			AssertEquals("Not posted", false, charge.IsRevenuePosted);

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				charge.Validation.ValidateJR_SellGovtChargeCode();
				if (gstTaxRate.IsEmpty)
				{
					AssertHasWarning("Unposted empty JR_SellGovtChargeCode", charge.JR_SellGovtChargeCodeInfo, warning);
				}
				else
				{
					AssertHasError("Unposted empty JR_SellGovtChargeCode", charge.JR_SellGovtChargeCodeInfo, error);
				}
			}

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			AssertEquals("Posted", true, charge.IsRevenuePosted);

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				charge.Validation.ValidateJR_SellGovtChargeCode();
				AssertEquals(string.Empty, charge.JR_SellGovtChargeCode);
				AssertNoError("Posted JR_SellGovtChargeCode", charge.JR_SellGovtChargeCodeInfo, error);
				AssertNoWarning("Posted JR_SellGovtChargeCode", charge.JR_SellGovtChargeCodeInfo, warning);
			}
		}

		public void TestCheckJR_SellGovtChargeCode_NoErrorsWhenPosted()
		{
			var mrgChargeCode = CreateChargeCode(Core.Constants.ChargeType.Margin);
			CheckJR_SellGovtChargeCode_NoErrorsWhenPosted(mrgChargeCode.PK, ZGuid.Empty);

			var gstTaxRate = CreateTaxRate("xGST", AccTaxRate.Types.Rated, AccTaxRate.ExtraTypes.StateGST);
			CheckJR_SellGovtChargeCode_NoErrorsWhenPosted(mrgChargeCode.PK, gstTaxRate.PK);
		}

		public virtual void TestCheckJR_GE()
		{
			var aaaBranch = Factory.NewWithValidTestData<GlbBranch>();
			var aaaDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			aaaBranch.GB_GC = Env.CurrentCompany.PK;
			Factory.Save();

			var charge1 = Factory.New<JobCharge>();
			charge1.JR_GB = aaaBranch.PK;
			charge1.JR_GE = bbbDepartment.PK;

			var charge2 = Factory.New<JobCharge>();
			charge2.JR_GB = aaaBranch.PK;
			charge2.JR_GE = bbbDepartment.PK;
			charge2.ReadOnly = true;

			AssertNoErrors(charge1.JR_GEInfo);
			AssertNoErrors(charge2.JR_GEInfo);

			aaaBranch.AllowedDepartments.DeleteAll();
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(aaaBranch, new GlbDepartment[] { aaaDepartment });

			charge1.Validation.ValidateJR_GE();
			charge2.Validation.ValidateJR_GE();
			var expectedErrorMessage = string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, bbbDepartment.GE_Code, aaaBranch.GB_Code);
			AssertEquals("Precondition", false, charge1.JR_GEInfo.ReadOnly);
			AssertHasError("Editable charge should display error", charge1.JR_GEInfo, expectedErrorMessage);

			AssertEquals("Precondition", true, charge2.JR_GEInfo.ReadOnly);
			AssertNoError("Readonly charge should not display error", charge2.JR_GEInfo, expectedErrorMessage);
		}

		public void TestBranchDepartmentCombinationValidation_JobChargeValidation()
		{
			var bizObj = Factory.NewWithValidTestData<JobCharge>();

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.JR_GB = branch; bizObj.JR_GE = department; }, bizObj.JR_GEInfo);
		}

		#region Implementation

		protected virtual JobCharge CreateJobCharge() => (JobCharge)Factory.New(GetExpectedBusinessObjectType());

		protected virtual Type GetExpectedBusinessObjectType() => typeof(JobCharge);

		protected AccChargeCode CreateChargeCode(string chargeType)
		{
			var chargeCode = new BusinessObjectFactory().NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "ZZ" + chargeType;
			chargeCode.AC_ChargeType = chargeType;

			chargeCode.Factory.Save();

			return chargeCode;
		}

		protected AccTaxRate CreateTaxRate(string code, string type, string extraType)
		{
			var taxRate = new BusinessObjectFactory().New<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Type = type;
			taxRate.AT_ExtraTaxRateType = extraType;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			taxRate.Factory.Save();

			return taxRate;
		}

		protected BusinessObject CreateParentConsolCost(JobCharge charge)
		{
			var result = (BusinessObject)Factory.New<IJobConsolCost>();
			charge.JR_E6 = result.PK;

			return result;
		}

		protected void PostCostCharge(JobCharge charge)
		{
			var costLine = Factory.NewWithValidTestData<AccTransactionLines>();
			costLine.AL_LineType = TransactionLineTypes.Cost;
			charge.JR_AL_APLine = costLine.PK;

			AssertEquals("Pre-condition: Cost Posted", true, charge.IsCostPosted);
		}

		#endregion
	}
}
