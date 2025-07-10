using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccTransactionLinesValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckAL_RX_NKTransactionCurrency()
		{
			var line = GetNewLine();
			line.AL_RX_NKTransactionCurrency = "XXX";
			GetNewValidation(line).ValidateAL_TaxDate();
			AssertHasErrors("Invalid currency", line.AL_RX_NKTransactionCurrencyInfo);

			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			GetNewValidation(line).ValidateAL_TaxDate();
			AssertNoErrors("Valid currency", line.AL_RX_NKTransactionCurrencyInfo);
		}

		public void TestBranchDepartmentCombinationValidation_AccTransactionLinesValidation()
		{
			var factory = new BusinessObjectFactory();
			var bizObj = factory.NewWithValidTestData<AccTransactionLines>();
			bizObj.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(factory,
				(branch, department) => { bizObj.AL_GB = branch; bizObj.AL_GE = department; }, bizObj.AL_GEInfo);
		}

		#region TestCheckAL_TaxDate

		public void TestCheckAL_TaxDate()
		{
			const string errorMessage = "No rate found for selected date.";
			var line = GetNewLine();
			line.AL_AT = CreateTaxRate().PK;
			line.AL_TaxDate = ZDate.Empty;
			GetNewValidation(line).ValidateAL_TaxDate();
			if (IsTaxDateValidationAllowed)
			{
				AssertHasError(line.AL_TaxDateInfo, "Please enter a Tax Date.");
			}
			else
			{
				AssertNoErrors(line.AL_TaxDateInfo);
			}

			line.AL_TaxDate = ZDate.Today;
			GetNewValidation(line).ValidateAL_TaxDate();
			AssertNoErrors(line.AL_TaxDateInfo);

			line.AL_TaxDate = ZDate.Today.AddDays(5);
			GetNewValidation(line).ValidateAL_TaxDate();
			if (IsTaxDateValidationAllowed)
			{
				AssertHasError(line.AL_TaxDateInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(line.AL_TaxDateInfo);
			}

			line.AL_TaxDate = ZDate.Today.AddDays(1);
			GetNewValidation(line).ValidateAL_TaxDate();
			AssertNoErrors(line.AL_TaxDateInfo);

			line.AL_TaxDate = ZDate.Today.AddDays(5);
			GetNewValidation(line).ValidateAll();
			if (IsTaxDateValidationAllowed)
			{
				AssertHasError(line.AL_TaxDateInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(line.AL_TaxDateInfo);
			}

			line.AL_AT = ZGuid.Empty;
			GetNewValidation(line).ValidateAL_TaxDate();
			AssertNoErrors(line.AL_TaxDateInfo);

			AccTaxRate CreateTaxRate()
			{
				var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
				rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
				return rate;
			}
		}

		protected virtual bool IsTaxDateValidationAllowed => true;

		#endregion

		#region Test for Tax Id And Tax Message Mapping Validation

		public void TestTaxIDAndTaxMessageMappingValidation()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new AccountingTestObjectCreator(factory);
			var taxRate1 = factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TaxRate01";
			var taxMsg1 = factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			factory.Save();

			var header = factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			var line = GetNewLine(factory);
			line.FillWithValidTestData();
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AT = taxRate1.PK;
			line.AL_AH = header.PK;
			line.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;

			var helper = ObjectFactory.Get<ITaxIdAndTaxMessageMappingHelper>();
			var config = testObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Cost, taxRate1, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			line.AL_A9_VATClass = taxMsg2.PK;
			AssertEquals(GetErrorMsg("CST", "TaxRate01", "TaxMsg02"), helper.ValidateMappingForLine(line));

			line.AL_A9_VATClass = taxMsg1.PK;
			AssertEquals(null, helper.ValidateMappingForLine(line));

			line.AL_A9_VATClass = taxMsg1.PK;
			GetNewValidation(line).ValidateAll();
			AssertNoErrors(line.AL_A9_VATClassInfo);

			line.AL_A9_VATClass = taxMsg2.PK;
			GetNewValidation(line).ValidateAll();
			AssertEquals(true, line.AL_A9_VATClassInfo.HasError(GetErrorMsg("CST", "TaxRate01", "TaxMsg02")));

			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new TaxIdAndTaxMessageCombinationRulesConfiguration());
			factory.Save();
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			GetNewValidation(line).ValidateAll();
			AssertNoErrors(line.AL_A9_VATClassInfo);
		}

		#endregion

		protected virtual AccTransactionLinesValidation GetNewValidation(AccTransactionLines line) => new AccTransactionLinesValidation(line);
		protected virtual AccTransactionLines GetNewLine(BusinessObjectFactory factory = null) => factory != null ? factory.New<AccTransactionLines>() : Factory.New<AccTransactionLines>();

		string GetErrorMsg(string lineType, string taxId, string taxMsg) => $@"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type={lineType}, Tax ID={taxId}, Tax Message={taxMsg}";
	}
}
