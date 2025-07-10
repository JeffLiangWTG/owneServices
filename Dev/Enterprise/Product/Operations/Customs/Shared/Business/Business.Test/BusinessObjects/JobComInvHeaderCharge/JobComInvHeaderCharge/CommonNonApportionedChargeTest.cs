using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	class CommonNonApportionedChargeTest : TestCaseWithFactory
	{
		public void TestDefaultOverseasInsurancePrecentageIfApplicable()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			var groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;

			CombineAssertions("Registry default value", () =>
			{
				AssertEquals("invoiceCharge", 0m, invoiceCharge.J7_Percentage);
				AssertEquals("groupCharge", 0m, groupCharge.J7_Percentage);
				AssertEquals("invoiceLineCharge", 0m, invoiceLineCharge.J7_Percentage);
			});

			using (CustomsDataRegistry.Instance.DefaultInsuranceRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0.45m))
			{
				invoiceCharge = invoice.Charges.AddNew();
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
				groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
				invoiceLineCharge = invoiceLine.Charges.AddNew();
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
				CombineAssertions("Override value", () =>
				{
					AssertEquals("invoiceCharge", 0.45m, invoiceCharge.J7_Percentage);
					AssertEquals("groupCharge", 0.45m, groupCharge.J7_Percentage);
					AssertEquals("invoiceLineCharge", 0m, invoiceLineCharge.J7_Percentage);
				});

				invoiceCharge = invoice.Charges.AddNew();
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				invoiceLineCharge = invoiceLine.Charges.AddNew();
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				CombineAssertions("Other charge types", () =>
				{
					AssertEquals("invoiceCharge", 0m, invoiceCharge.J7_Percentage);
					AssertEquals("groupCharge", 0m, groupCharge.J7_Percentage);
					AssertEquals("invoiceLineCharge", 0m, invoiceLineCharge.J7_Percentage);
				});
			}

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_Code = "US1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "US1";
			declaration.JE_GB = branch.PK;
			using (CustomsDataRegistry.Instance.DefaultInsuranceRate.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, 0.6m))
			{
				invoiceCharge = invoice.Charges.AddNew();
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
				groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
				CombineAssertions("Branch US1", () =>
				{
					AssertEquals("invoiceCharge", 0.6m, invoiceCharge.J7_Percentage);
					AssertEquals("groupCharge", 0.6m, groupCharge.J7_Percentage);
				});
			}
		}

		public void TestJ7_IsIncludedInITOTDefaultWhenJ7_ChargeTypeIsSet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();

			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			BaseInvoiceLineCharge invoiceLineCharge = invoiceLine.Charges.AddNew();

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			AssertEquals("J7_IsIncludedInITOT", false, invoiceCharge.J7_IsIncludedInITOT);
			AssertEquals("J7_IsIncludedInITOT", false, invoiceLineCharge.J7_IsIncludedInITOT);
			AssertEquals("J7_IsIncludedInITOT readonly", false, invoiceCharge.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("J7_IsIncludedInITOT readonly", false, invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			AssertEquals("J7_IsIncludedInITOT", true, invoiceCharge.J7_IsIncludedInITOT);
			AssertEquals("J7_IsIncludedInITOT", true, invoiceLineCharge.J7_IsIncludedInITOT);
			AssertEquals("J7_IsIncludedInITOT readonly", false, invoiceCharge.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("J7_IsIncludedInITOT readonly", false, invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly);
		}

		public void TestJ7_Calc_IncludedInInvoiceAmountReadonly()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			BaseGroupInvoiceCharge groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			AssertEquals(true, groupCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();

			BaseJobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			BaseInvoiceLineCharge invoiceLineCharge = invoiceLine.Charges.AddNew();

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			AssertEquals(false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			AssertEquals(false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			AssertEquals(false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			AssertEquals(false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals(true, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals(true, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertEquals(false, invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			AssertEquals(false, invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
		}

		public void TestJ7_IsIncludedInLinesAndJ7_IsNotIncludedInInvoice()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_IsIncludedInITOT = true;
			AssertEquals("Should be included in invoice as well", false, invoiceCharge.J7_IsNotIncludedInInvoice);

			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			AssertEquals("Should not be included in line", false, invoiceCharge.J7_IsIncludedInITOT);
		}

		public void TestMarkApportionmentDirtyWhenChanged()
		{
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_Amount.Name, new ZDecimal(100m));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_RX_NKCurrency.Name, new ZString("AUD"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_ChargeType.Name, new ZString("OFT"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_DistributeBy.Name, new ZString("XXX"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_Percentage.Name, new ZDecimal(10m));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_FullOrPartialApportionment.Name, new ZString("XXX"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsNotIncludedInInvoice.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsIncludedInITOT.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_DistributeBy.Name, new ZString("XXX"));
		}

		void AssertApportionmentDirty(string fieldNameToChangeApportionmentDirtiness, IZType valueToAssign)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);

				BaseGroupInvoiceCharge testGroupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				AssertEquals("Apportionment is not dirty yet", false, testDec.ApportionmentDirty);

				testGroupCharge[fieldNameToChangeApportionmentDirtiness] = valueToAssign;
				AssertEquals("Apportionment is dirty yet", true, testDec.ApportionmentDirty);
			}
		}

		public void TestMarkApportionmentDirtyWhenIsJ7_ExchangeRateUserEnterableChanged()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
				BaseGroupInvoiceCharge testGroupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				SetIsJ7_ExchangeRateUserEnterable(testGroupCharge, false);
				AssertEquals("Apportionment is not dirty yet", false, testDec.ApportionmentDirty);

				SetIsJ7_ExchangeRateUserEnterable(testGroupCharge, true);
				AssertEquals("Apportionment is dirty yet", true, testDec.ApportionmentDirty);

				testDec.ApportionmentDirty = false;
				SetIsJ7_ExchangeRateUserEnterable(testGroupCharge, false);
				AssertEquals("Apportionment is not dirty yet", true, testDec.ApportionmentDirty);
			}
		}

		public void TestMarkApportionmentDirtyWhenExchangeRateChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			BaseGroupInvoiceCharge groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_ExchangeRateType = ZString.Empty;
			declaration.ApportionmentDirty = false;

			groupCharge.J7_ExchangeRateType = "BZS";
			AssertEquals("Apportionment is dirty yet", false, declaration.ApportionmentDirty);

			groupCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			AssertEquals("Apportionment is dirty yet", true, declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			groupCharge.J7_ExchangeRateType = "BZS";
			AssertEquals("Apportionment is dirty yet", true, declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			groupCharge.J7_ExchangeRateType = ZString.Empty;
			AssertEquals("Apportionment is dirty yet", false, declaration.ApportionmentDirty);
		}

		public void TestMarkApportionmentDirtyWhenExchangeRateTypeChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			BaseGroupInvoiceCharge groupCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			SetIsJ7_ExchangeRateUserEnterable(groupCharge, false);
			groupCharge.J7_ExchangeRate = 0.323m;
			declaration.ApportionmentDirty = false;

			AssertEquals("IsJ7_ExchangeRateUserEnterable", false, groupCharge.IsJ7_ExchangeRateUserEnterable);
			groupCharge.J7_ExchangeRate = 0.123m;
			AssertEquals("Apportionment is dirty yet", false, declaration.ApportionmentDirty);

			groupCharge.J7_ExchangeRate = 0.323m;
			AssertEquals("Apportionment is dirty yet", false, declaration.ApportionmentDirty);

			SetIsJ7_ExchangeRateUserEnterable(groupCharge, true);
			declaration.ApportionmentDirty = false;

			AssertEquals("IsJ7_ExchangeRateUserEnterable", true, groupCharge.IsJ7_ExchangeRateUserEnterable);
			groupCharge.J7_ExchangeRate = 0.123m;
			AssertEquals("Apportionment is dirty yet", true, declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			groupCharge.J7_ExchangeRate = 0.323m;
			AssertEquals("Apportionment is dirty yet", true, declaration.ApportionmentDirty);
		}

		public void TestIsJ7_ExchangeRateUserEnterableInfoReadOnly()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseGroupInvoiceCharge charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_Percentage = 10m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			AssertEquals(true, charge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);

			charge.J7_Percentage = ZDecimal.Zero;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Afghanistan;
			AssertEquals(false, charge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);

			charge.J7_RX_NKCurrency = ZString.Empty;
			AssertEquals(true, charge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly);
		}

		public void TestJ7_ExchangeRateInfoReadOnly()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseGroupInvoiceCharge charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_Percentage = 10m;
			SetIsJ7_ExchangeRateUserEnterable(charge, true);
			AssertEquals(true, charge.J7_ExchangeRateInfo.ReadOnly);

			charge.J7_Percentage = ZDecimal.Zero;
			SetIsJ7_ExchangeRateUserEnterable(charge, true);
			AssertEquals(false, charge.J7_ExchangeRateInfo.ReadOnly);

			SetIsJ7_ExchangeRateUserEnterable(charge, false);
			AssertEquals(true, charge.J7_ExchangeRateInfo.ReadOnly);
		}

		public void TestJ7_ExchangeRateTypeInfoReadOnly()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseGroupInvoiceCharge charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_Percentage = 10m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			AssertEquals(true, charge.J7_ExchangeRateTypeInfo.ReadOnly);

			charge.J7_Percentage = ZDecimal.Zero;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Afghanistan;
			AssertEquals(false, charge.J7_ExchangeRateTypeInfo.ReadOnly);

			charge.J7_RX_NKCurrency = ZString.Empty;
			AssertEquals(true, charge.J7_ExchangeRateTypeInfo.ReadOnly);
		}

		public void TestIDefaultLandedCostInput()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseGroupInvoiceCharge groupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			AssertEquals("ChargeDescription", CustomsChargeTypeList.Descriptions.OverseasFreight.ToString().ToUpper() + " from Entry", ((IDefaultLandedCostInput)groupCharge).ChargeDescription);
			AssertEquals("AmountToDistribute Amount", 100m, ((IDefaultLandedCostInput)groupCharge).AmountToDistribute.Amount);
			AssertEquals("AmountToDistribute Currency", testDec.LocalCurrencyCode, ((IDefaultLandedCostInput)groupCharge).AmountToDistribute.Currency.Code);
			AssertEquals("FKToChargeCode", ZGuid.Empty, ((IDefaultLandedCostInput)groupCharge).FKToChargeCode);
			AssertEquals("ExchangeRate", 1m, ((IDefaultLandedCostInput)groupCharge).ExchangeRate);
			AssertEquals("IsValidToImport", true, ((IDefaultLandedCostInput)groupCharge).IsValidToImport);

			groupCharge.J7_AdjustedCharge = true;
			AssertEquals("IsValidToImport", false, ((IDefaultLandedCostInput)groupCharge).IsValidToImport);

			groupCharge.J7_AdjustedCharge = false;
			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PullLandedCostingDataFromBillingTabOnly.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals("IsValidToImport", false, ((IDefaultLandedCostInput)groupCharge).IsValidToImport);
			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PullLandedCostingDataFromBillingTabOnly.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			AssertEquals("ChargeDescription", "Deduction (or Discount) from Entry", ((IDefaultLandedCostInput)groupCharge).ChargeDescription);
			AssertEquals("AmountToDistribute Amount", -100m, ((IDefaultLandedCostInput)groupCharge).AmountToDistribute.Amount);

			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			AssertEquals("ChargeDescription", "Deduction (or Discount) from Entry", ((IDefaultLandedCostInput)groupCharge).ChargeDescription);
			AssertEquals("AmountToDistribute Amount", -100m, ((IDefaultLandedCostInput)groupCharge).AmountToDistribute.Amount);
		}

		public void TestOverseasFreightNotImportedToLCIfJobInvoicingHasOFT()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseGroupInvoiceCharge groupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			AssertEquals("PreCondition:IsValidToImport", true, ((IDefaultLandedCostInput)groupCharge).IsValidToImport);

			JobCharge charge = Factory.New<JobCharge>();
			charge.FillWithValidTestData();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			var jobInvoicing = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			charge.JR_JH = jobInvoicing.PK;
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = testDec.PK;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseGroupInvoiceCharge groupChargeLoaded = factory2.Load<BaseGroupInvoiceCharge>(groupCharge.PK);
			AssertEquals("IsValidToImport as the jobInvoicing has a Freight charge", false, ((IDefaultLandedCostInput)groupChargeLoaded).IsValidToImport);
		}

		public void TestReadOnly()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);

			BaseGroupInvoiceCharge groupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_IsDutiable = true;
			AssertEquals("IsGSTapplicable readonly", true, groupCharge.J7_IsGSTApplicableInfo.ReadOnly);

			BaseGroupInvoiceCharge groupCharge2 = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			AssertEquals("IsDutiable Readonly", true, groupCharge2.J7_IsDutiableInfo.ReadOnly);
			AssertEquals("IsGSTapplicable readonly", true, groupCharge2.J7_IsGSTApplicableInfo.ReadOnly);

			BaseGroupInvoiceCharge groupCharge3 = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge3.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			AssertEquals("IsDutiable Readonly", true, groupCharge3.J7_IsDutiableInfo.ReadOnly);
			AssertEquals("IsGSTapplicable readonly", true, groupCharge3.J7_IsGSTApplicableInfo.ReadOnly);
		}

		public void TestIsDutiable()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseGroupInvoiceCharge groupCharge = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_IsDutiable = true;
			AssertEquals("IsGSTApplicable", true, groupCharge.J7_IsGSTApplicable);
		}

		protected virtual void SetIsJ7_ExchangeRateUserEnterable(BaseJobComInvHeaderCharge charge, bool value)
		{
			charge.IsJ7_ExchangeRateUserEnterable = value;
		}
	}
}
