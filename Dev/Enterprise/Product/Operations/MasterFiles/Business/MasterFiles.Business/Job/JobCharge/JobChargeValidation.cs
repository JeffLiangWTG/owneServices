//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobChargeValidation
//
//    This class should be used for overriding validation in AutoJobChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeValidation : AutoJobChargeValidation
	{
		public JobChargeValidation(AutoJobCharge parent) : base(parent)
		{
		}

		JobCharge ParentJobCharge
		{
			get { return base.Parent as JobCharge; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ValidateDataRefreshBusChanges(Parent);
		}

		public void ValidateJR_OSCostGSTAmt_Calc()
		{
			ValidateCalculatedProperty(ParentJobCharge.JR_OSCostGSTAmt_CalcInfo);
		}

		GlbCompany ParentCompany => parentCompany ?? (parentCompany = ParentJobCharge.Company);
		GlbCompany parentCompany;

		protected override void CheckJR_OSCostExRate()
		{
			base.CheckJR_OSCostExRate();

			if (ParentJobCharge != null && !ParentJobCharge.IsCostPosted && ParentCompany.GC_RX_NKLocalCurrency == Parent.JR_RX_NKCostCurrency && Parent.JR_OSCostExRate != 1m)
			{
				Parent.JR_OSCostExRateInfo.AddError(Res.GetString("9211d669-cba7-467d-b80b-80f12685234f", @"Cost exchange rate has failed to update. When cost currency is local currency, cost exchange rate must equal 1. 
Please manually refresh the cost exchange rate by temporarily changing the cost currency to a foreign currency then saving your changes. 
Then change the cost currency to the currency of your choice."));
			}
		}

		protected override void CheckJR_OSSellExRate()
		{
			base.CheckJR_OSSellExRate();

			if (ParentJobCharge != null && !ParentJobCharge.IsRevenuePosted && ParentCompany.GC_RX_NKLocalCurrency == Parent.JR_RX_NKSellCurrency && Parent.JR_OSSellExRate != 1m)
			{
				Parent.JR_OSSellExRateInfo.AddError(Res.GetString("d6c1f95a-0ff6-4975-bb8f-653920fced55", @"Sell exchange rate has failed to update. When sell currency is local currency, sell exchange rate must equal 1. 
Please manually refresh the sell exchange rate by temporarily changing the sell currency to a foreign currency then saving your changes. 
Then change the sell currency to the currency of your choice."));
			}
		}

		override protected void CheckJR_APInvoiceDateIsValidZDateTimeRange()
		{
			if (!Parent.JR_APInvoiceDateInfo.ReadOnly)
			{
				base.CheckJR_APInvoiceDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckJR_PaymentDateIsValidZDateTimeRange()
		{
			if (!Parent.JR_PaymentDateInfo.ReadOnly)
			{
				base.CheckJR_PaymentDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckJR_LocalCostAmt()
		{
			base.CheckJR_LocalCostAmt();

			if (ParentJobCharge != null && !ParentJobCharge.IsCostPosted)
			{
				if (Parent.JR_LocalCostAmt == 0m && Parent.JR_OSCostAmt != 0m)
				{
					Parent.JR_LocalCostAmtInfo.AddError(AccountingMasterFilesConstants.ChargeLocalCostAmountCannotBeZeroErrorMessage);
				}
				else
				{
					var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.JR_GC, CriticalValidationHelpers.MaximumAmountLevel.Charge, Parent.JR_LocalCostAmt);
					if (message != null)
					{
						Parent.JR_LocalCostAmtInfo.AddError(message);
					}
				}
			}
		}

		protected override void CheckJR_LocalSellAmt()
		{
			base.CheckJR_LocalSellAmt();

			if (ParentJobCharge != null && !ParentJobCharge.IsRevenuePosted)
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.JR_GC, CriticalValidationHelpers.MaximumAmountLevel.Charge, Parent.JR_LocalSellAmt);
				if (message != null)
				{
					Parent.JR_LocalSellAmtInfo.AddError(message);
				}
			}
		}

		protected override void CheckJR_OA_SellInvoiceAddress()
		{
			base.CheckJR_OA_SellInvoiceAddress();

			if (Parent.SellInvoiceAddress != null && Parent.SellInvoiceAddress.OA_OH != Parent.JR_OH_SellAccount)
			{
				var property = Parent.JR_OA_SellInvoiceAddressInfo;
				property.AddError(GetAddressContactErrorMessage(property));
			}
		}

		protected override void CheckJR_OC_SellInvoiceContact()
		{
			base.CheckJR_OC_SellInvoiceContact();

			if (Parent.SellInvoiceContact != null && Parent.SellInvoiceContact.OC_OH != Parent.JR_OH_SellAccount)
			{
				var property = Parent.JR_OC_SellInvoiceContactInfo;
				property.AddError(GetAddressContactErrorMessage(property));
			}
		}

		protected override void CheckJR_APInvoiceNum()
		{
			base.CheckJR_APInvoiceNum();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_APInvoiceNum != ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_InvoiceNum].ToString())
			{
				Parent.JR_APInvoiceNumInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_APDocumentReceivedDate()
		{
			base.CheckJR_APDocumentReceivedDate();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_APDocumentReceivedDate != (ZDateTime)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_DocumentReceivedDate])
			{
				Parent.JR_APDocumentReceivedDateInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_APInvoiceDate()
		{
			base.CheckJR_APInvoiceDate();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_APInvoiceDate != (ZDateTime)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_InvoiceDate])
			{
				Parent.JR_APInvoiceDateInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_PaymentDate()
		{
			base.CheckJR_PaymentDate();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				ParentJobCharge.JR_PaymentDate != (ZDateTime)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_PaymentDate])
			{
				Parent.JR_PaymentDateInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_OH_CostAccount()
		{
			base.CheckJR_OH_CostAccount();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_OH_CostAccount != (ZGuid)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_OH_Creditor])
			{
				Parent.JR_OH_CostAccountInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_CostReference()
		{
			base.CheckJR_CostReference();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_CostReference != ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_CostReference].ToString())
			{
				Parent.JR_CostReferenceInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_AT_CostGSTRate()
		{
			base.CheckJR_AT_CostGSTRate();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_AT_CostGSTRate != (ZGuid)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_AT_TaxRate])
			{
				Parent.JR_AT_CostGSTRateInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_CostTaxDate()
		{
			base.CheckJR_CostTaxDate();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_CostTaxDate != (ZDate)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_TaxDate])
			{
				Parent.JR_CostTaxDateInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_A9_CostVATClass()
		{
			base.CheckJR_A9_CostVATClass();
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_A9_CostVATClass != (ZGuid)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_A9_VATClass])
			{
				Parent.JR_A9_CostVATClassInfo.AddError(MustBeEqualToParentConsolCost);
			}
		}

		protected override void CheckJR_CostPlaceOfSupply()
		{
			base.CheckJR_CostPlaceOfSupply();
			if (!AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.Value.Cast<CodeDescriptionBool>().Any(code => code.Bool == ZBool.True))
			{
				return;
			}
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_CostPlaceOfSupply != (ZString)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_PlaceOfSupply])
			{
				Parent.JR_CostPlaceOfSupplyInfo.AddError(MustBeEqualToParentConsolCost);
			}
			else if (!Parent.JR_CostPlaceOfSupply.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JR_CostPlaceOfSupplyInfo);
			}
			else if (!Parent.JR_CostPlaceOfSupplyType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JR_CostPlaceOfSupplyInfo);
			}
		}

		protected override void CheckJR_CostPlaceOfSupplyType()
		{
			base.CheckJR_CostPlaceOfSupplyType();
			if (!AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.Value.Cast<CodeDescriptionBool>().Any(code => code.Bool == ZBool.True))
			{
				return;
			}
			if (ParentJobCharge != null && ParentJobCharge.ParentConsolCost != null &&
				!ParentJobCharge.IsCostPosted &&
				Parent.JR_CostPlaceOfSupplyType != (ZString)ParentJobCharge.ParentConsolCost[JobConsolCostSchema.Constants.E6_PlaceOfSupplyType])
			{
				Parent.JR_CostPlaceOfSupplyTypeInfo.AddError(MustBeEqualToParentConsolCost);
			}
			else if (!Parent.JR_CostPlaceOfSupplyType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JR_CostPlaceOfSupplyTypeInfo);
			}
			else if (!Parent.JR_CostPlaceOfSupply.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JR_CostPlaceOfSupplyTypeInfo);
			}
		}

		protected override void CheckJR_SellPlaceOfSupply()
		{
			base.CheckJR_SellPlaceOfSupply();
			if (!AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.Value.Cast<CodeDescriptionBool>().Any(code => code.Bool == ZBool.True))
			{
				return;
			}
			if (!Parent.JR_SellPlaceOfSupply.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JR_SellPlaceOfSupplyInfo);
			}
			else if (!Parent.JR_SellPlaceOfSupplyType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JR_SellPlaceOfSupplyInfo);
			}
		}

		protected override void CheckJR_SellPlaceOfSupplyType()
		{
			base.CheckJR_SellPlaceOfSupplyType();
			if (!AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.Value.Cast<CodeDescriptionBool>().Any(code => code.Bool == ZBool.True))
			{
				return;
			}
			if (!Parent.JR_SellPlaceOfSupplyType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JR_SellPlaceOfSupplyTypeInfo);
			}
			else if (!Parent.JR_SellPlaceOfSupply.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JR_SellPlaceOfSupplyTypeInfo);
			}
		}

		protected override void CheckJR_GE()
		{
			base.CheckJR_GE();
			if (!Parent.JR_GEInfo.ReadOnly)
			{
				GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Parent.JR_GEInfo, Parent.Branch, Parent.Department);
			}
		}

		protected override void CheckJR_GC()
		{
			base.CheckJR_GC();
			if (!Parent.JR_GC.IsValid)
			{
				Parent.JR_GCInfo.AddError(Res.GetString("6ed639a7-fc03-4307-a149-cd81fcaa5510", "Please enter valid company."));
			}
			else if (Parent.Branch != null && Parent.JR_GC != Parent.Branch.GB_GC)
			{
				Parent.JR_GCInfo.AddError(Res.GetString("4457da4b-ae64-49e7-8f87-687eb46ebae5", "The Company you entered doesn't match the Branch you entered."));
			}
		}

		protected override void CheckJR_CostGovtChargeCode()
		{
			if (ShouldValidateGovtChargeCode() && !ParentJobCharge.IsCostPosted)
			{
				if (IsGovernemntChargeCodeMandatory && Parent.CostGSTRate != null && !Parent.CostGSTRate.IsIndiaServiceTax)
				{
					base.CheckJR_CostGovtChargeCode();
					if (Parent.JR_CostGovtChargeCode.IsEmpty)
					{
						Parent.JR_CostGovtChargeCodeInfo.AddError(JR_CostGovtChargeCodeMandatoryMessage);
					}
				}
				else
				{
					if (Parent.JR_CostGovtChargeCode.IsEmpty)
					{
						Parent.JR_CostGovtChargeCodeInfo.AddWarning(JR_CostGovtChargeCodeWarningMessage);
					}
				}
			}
		}

		protected virtual string JR_CostGovtChargeCodeMandatoryMessage => Res.GetString("f04ca5e1-13dd-4bc9-9206-a2521c819de7", "Please enter a Cost Government Charge Code.");

		protected virtual string JR_CostGovtChargeCodeWarningMessage => Res.GetString("dec65064-8a3b-46fc-a7a5-832f483eb821", "Cost Government Charge Code is empty.");

		protected override void CheckJR_SellGovtChargeCode()
		{
			if (ShouldValidateGovtChargeCode() && !ParentJobCharge.IsRevenuePosted && !IsPostedApportionedCharge)
			{
				if (IsGovernemntChargeCodeMandatory && Parent.SellGSTRate != null && !Parent.SellGSTRate.IsIndiaServiceTax)
				{
					base.CheckJR_SellGovtChargeCode();
					if (Parent.JR_SellGovtChargeCode.IsEmpty)
					{
						Parent.JR_SellGovtChargeCodeInfo.AddError(JR_SellGovtChargeCodeMandatoryMessage);
					}
				}
				else
				{
					if (Parent.JR_SellGovtChargeCode.IsEmpty)
					{
						Parent.JR_SellGovtChargeCodeInfo.AddWarning(JR_SellGovtChargeCodeWarningMessage);
					}
				}
			}
		}

		bool IsPostedApportionedCharge => ParentJobCharge.JR_IsApportioned && ParentJobCharge.IsCostPosted;

		protected virtual string JR_SellGovtChargeCodeMandatoryMessage => Res.GetString("57fc4e77-08d2-4c2c-994b-4381143a1192.", "Please enter a Sell Government Charge Code.");

		protected virtual string JR_SellGovtChargeCodeWarningMessage => Res.GetString("a1e06bd9-c03e-466f-aa60-61e712ef05ea", "Sell Government Charge Code is empty.");

		protected sealed override void CheckJR_OSCostGSTAmt()
		{
			//JR_OSCostGSTAmt_Calc should be validated instead
		}

		bool ShouldValidateGovtChargeCode()
		{
			var parent = ParentJobCharge;
			if (parent != null && AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value && !(parent.IsRevenuePostedWithManualJobRevenueJournal || parent.IsRevenuePostedWithAutoJobRevenueJournal))
			{
				return true;
			}

			return false;
		}

		protected virtual bool AdditionalCheckForMandatoryGovChargeCode() => true;

		bool IsGovernemntChargeCodeMandatory
		{
			get
			{
				return Parent.ChargeCode != null && !Parent.ChargeCode.IsComment && AdditionalCheckForMandatoryGovChargeCode();
			}
		}

		string GetAddressContactErrorMessage(ZPropertyInfo property)
		{
			return Res.GetString("A919931C-406E-4A74-843A-50F5E2EC7DCD", "The {0} must belong to the {1}.", property.HumanReadableName, Parent.JR_OH_SellAccountInfo.HumanReadableName);
		}

		string MustBeEqualToParentConsolCost
		{
			get { return Res.GetString("17f4d6b7-875d-4e8a-b051-d7139efa727e", "Must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data."); }
		}
	}
}
