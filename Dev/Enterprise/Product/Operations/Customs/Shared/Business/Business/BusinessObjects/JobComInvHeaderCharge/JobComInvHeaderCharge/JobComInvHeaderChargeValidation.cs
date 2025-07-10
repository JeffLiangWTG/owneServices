//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvHeaderChargeValidation
//
//    This class should be used for overriding validation in AutoJobComInvHeaderChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This is the base class for BaseInvoiceChargeValidation and BaseGroupInvoiceChargeValidation.
	/// </summary>
	public class JobComInvHeaderChargeValidation : Common.JobComInvHeaderChargeValidation
	{
		public JobComInvHeaderChargeValidation(BaseJobComInvHeaderCharge parent)
			: base(parent)
		{
		}

		public new BaseJobComInvHeaderCharge Parent
		{
			get { return (BaseJobComInvHeaderCharge)base.Parent; }
		}

		public ExternalMessageValidation MessageValidation
		{
			get { return messageValidation ?? (messageValidation = GetNewExternalMessageValidation()); }
		}
		ExternalMessageValidation messageValidation;

		protected virtual ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected void ValidateCurrency()
		{
			if (Parent.J7_Amount != 0)
			{
				MessageValidation.CheckEntered(Parent.J7_RX_NKCurrencyInfo);
				if (ShouldCheckExRates && !Parent.IsJ7_ExchangeRateUserEnterable && Parent.CurrencyConverter != null)
				{
					MessageValidation.ValidateExchangeRateExist(Parent.CurrencyConverter, Parent.J7_RX_NKCurrencyInfo);
				}
			}
		}

		protected virtual bool ShouldCheckExRates
		{
			get { return true; }
		}

		protected override void CheckJ7_IsDutiable()
		{
			base.CheckJ7_IsDutiable();
			ValidateNonDutiablePreFOBCharge();
		}

		protected void ValidateIfAllInvoiceLinesHaveDistributeByField()
		{
			var parent = Parent;
			var invoice = parent.Parent;
			if (parent.J7_DistributeBy != ChargeDistributeByList.Codes.Value && invoice != null)
			{
				bool allInvoiceLineHaveValueForDistributeBy = true;

				foreach (BaseJobComInvoiceLine invoiceLine in invoice.InvoiceLines)
				{
					IChargeApportionee apportionee = invoiceLine;
					if (apportionee.IsValidToApportionTo)
					{
						allInvoiceLineHaveValueForDistributeBy = apportionee.GetBaseValueToApportionOn(invoice.CurrencyConverter, parent.J7_DistributeBy) != 0m;
						if (!allInvoiceLineHaveValueForDistributeBy)
						{
							break;
						}
					}
				}

				if (!allInvoiceLineHaveValueForDistributeBy)
				{
					string fieldName = parent.Lookups.ChargeDistributionBy.GetDescriptionFromCode(parent.J7_DistributeBy) ?? "";

					parent.J7_DistributeByInfo.AddMessageError(Res.GetString("1146f1d9-947e-42e5-adee-6de71e80241d", "There are invoice lines that don't have a {0}. The apportionment of this charge won't be correct for the invoice lines.", fieldName.ToLower()));
				}

				if (parent.J7_DistributeBy == ChargeDistributeByList.Codes.Quantity && invoice.HasMultipleInvoiceUQs)
				{
					parent.J7_DistributeByInfo.AddWarning(MultipleInvoiceUQs);
				}
			}
		}

		protected void ValidateNonDutiablePreFOBCharge()
		{
			if (!Parent.J7_IsDutiable
				&& Parent.ChargeCode != null
				&& (Parent.J7_ChargeType == CustomsChargeTypeList.Codes.OtherCharges || Parent.J7_ChargeType == CustomsChargeTypeList.Codes.ForeignInlandFreight)
				&& Parent.Parent.JobDeclaration != null
				&& !Parent.Parent.JobDeclaration.CusContainers.HasContainerOfType("FCL"))
			{
				Parent.J7_IsDutiableInfo.AddWarning(NonDutiablePreFOBChargeForNoFCLContainer);
			}
		}

		internal protected static string NonDutiablePreFOBChargeForNoFCLContainer
		{
			get { return Res.GetString("962fee4e-48f8-4856-b6c8-a9ad17020fda", "You have a non-dutiable charge. But there is no FCL container in this declaration."); }
		}

		internal static string MultipleInvoiceUQs
		{
			get { return Res.GetString("e584b720-663d-4572-8999-c8b560d7ced4", "There are multiple invoice UQs. System will not convert between two different invoice UQs. The apportionment ratio will be determined by invoice quantities alone."); }
		}
	}
}
