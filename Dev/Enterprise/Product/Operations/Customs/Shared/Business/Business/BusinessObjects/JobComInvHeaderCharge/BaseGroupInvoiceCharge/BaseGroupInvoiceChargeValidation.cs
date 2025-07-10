using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class BaseGroupInvoiceChargeValidation : JobComInvHeaderChargeValidation
	{
		public BaseGroupInvoiceChargeValidation(BaseGroupInvoiceCharge groupInvoiceCharge) : base(groupInvoiceCharge)
		{
		}

		public void ValidateJ7_Calc_IsIncludedInITOT()
		{
			ValidateCalculatedProperty(Parent.J7_Calc_IsIncludedInITOTInfo);
		}

		protected virtual void CheckJ7_Calc_IsIncludedInITOT()
		{
			if (!Parent.IsJ7_IsIncludedInITOTCalculated)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.J7_Calc_IsIncludedInITOTInfo);

				if (Parent.ChargeCode != null && Parent.HasAtLeastOneValidIncoTerm && Parent.IncludedInInvoiceFixedForAllInvoices() == null)
				{
					Parent.J7_Calc_IsIncludedInITOTInfo.AddMessageError(IsIncludedInLinesCannotBeAppliedForAllInvoices);
				}
			}
		}

		internal static string IsIncludedInLinesCannotBeAppliedForAllInvoices
		{
			get { return Res.GetString("719c9882-bb9a-1081-4c80-41af615054ff", @"This charge cannot be apportioned into its invoices as their Incoterms' relationships to this charge are different, therefore what you have indicated here is not applicable for all invoices.

You should enter this charge in 'Invoice Charges' and indicate 'Is Included in Lines/Is Included in Invoices' individually for each invoice."); }
		}

		#region CheckJ7_ChargeType()
		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			CheckIfThereAreInvoicesToBeApportioned();
			ValidateLandingChargeApportionable();
		}

		protected void CheckIfThereAreInvoicesToBeApportioned()
		{
			if (!SuppressApportionmentValidationAsThereAreNoInvoicesYet)
			{
				if (Parent.GroupInvoice.AllJobComInvoiceHeaders.Count == 0)
				{
					Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("daaa2f88-5b3a-46b2-97e6-4bd1b185705c", "Unable to apportion {0} charges because the current group contains no invoices", Parent.J7_ChargeType));
				}
				else if (Parent.J7_Amount > 0 && Parent.Currency != null || Parent.J7_Percentage > 0)
				{
					bool areThereInvoicesToApportionCurrentCharge = false;

					if (!Parent.IsFullApportionment)
					{
						foreach (BaseJobComInvoiceHeader aHeader in Parent.GroupInvoice.AllJobComInvoiceHeaders)
						{
							Money invoiceCharge = aHeader.Charges.GetCharge(Parent.ApportionChargeKey);
							areThereInvoicesToApportionCurrentCharge = invoiceCharge.Amount == 0;
							if (areThereInvoicesToApportionCurrentCharge)
							{
								break;
							}
						}
					}
					else
					{
						areThereInvoicesToApportionCurrentCharge = Parent.GroupInvoice.AllJobComInvoiceHeaders.Count > 0;
					}

					if (!areThereInvoicesToApportionCurrentCharge)
					{
						Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("e0f227af-82e4-4325-ab80-c7986301c1c1", "Unable to apportion {0} charges because all invoices in the group have the charge assigned", Parent.J7_ChargeType));
					}
				}
			}
		}

		#endregion

		#region Overrides
		protected new BaseGroupInvoiceCharge Parent
		{
			get { return (BaseGroupInvoiceCharge)base.Parent; }
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();
			var groupHeader = Parent.GroupInvoice;
			if (groupHeader != null && !groupHeader.AllJobComInvoiceHeaders.DoAllInvoicesHaveTheSameValuationDate)
			{
				var result = Res.GetString("1f881db8-7c6a-4caa-96aa-501a42af6ec6", @"The system will be using the Declaration valuation date ({0}) for apportionment purposes.
The calculation will be approximate as the invoices have different valuation dates. If you want to override the apportionment result, you can do so by entering values at Line Charges.", groupHeader.JobDeclaration.DateOfValuation.ToShortDateString());
				Parent.J7_AmountInfo.AddWarning(result);
			}
			ValidateGroupChargeBalance();
		}

		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			ValidateIfAllInvoiceLinesHaveDistributeByField();
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			ValidateCurrency();
		}

		#endregion

		#region Helping methods

		protected bool SuppressApportionmentValidationAsThereAreNoInvoicesYet
		{
			get
			{
				return Parent.GroupInvoice == null || (Parent.GroupInvoice.JobDeclaration != null && Parent.GroupInvoice.JobDeclaration.Invoices.Count == 0);
			}
		}

		protected void ValidateGroupChargeBalance()
		{
			if (!SuppressApportionmentValidationAsThereAreNoInvoicesYet)
			{
				if (Parent.GroupInvoice.JobDeclaration != null && !Parent.GroupInvoice.JobDeclaration.ApportionmentDirty)
				{
					if (!Parent.IsFullApportionment)
					{
						Money groupCharge = Parent.GroupInvoice.Charges.GetCharge(Parent.ApportionChargeKey);

						if (groupCharge.Amount != 0m && groupCharge.Currency != null)
						{
							Money totalCharge = Money.Empty;
							foreach (BaseJobComInvoiceHeader invoice in Parent.GroupInvoice.AllJobComInvoiceHeaders)
							{
								totalCharge = invoice.CurrencyConverter.Add(totalCharge, invoice.GroupCharges.GetCharge(Parent.ApportionChargeKey));
								totalCharge = invoice.CurrencyConverter.Add(totalCharge, invoice.Charges.GetCharge(Parent.ApportionChargeKey));
							}

							Money totalChargeInGroupCurrency = Parent.GroupInvoice.CurrencyConverter.ConvertExact(totalCharge, groupCharge.Currency);

							if (decimal.Round(totalCharge.Amount, 2) != decimal.Round(groupCharge.Amount, 2))
							{
								Parent.J7_AmountInfo.AddWarning(Res.GetString("dd226d4e-9cfa-4dac-a893-4387a3040d89", "All invoices charges {0} does not add up to this group charge {1}.", totalChargeInGroupCurrency.Amount.ToString(2), groupCharge.Amount.ToString(2)));
							}
						}
					}
				}
			}
		}

		protected void ValidateLandingChargeApportionable()
		{
			if (!SuppressApportionmentValidationAsThereAreNoInvoicesYet)
			{
				if (Parent.GroupInvoice != null && Parent.J7_ChargeType == CustomsChargeTypeList.Codes.LandingCharges)
				{
					bool invoiceAcceptThisCharge = false;
					foreach (BaseJobComInvoiceHeader invoice in Parent.GroupInvoice.AllJobComInvoiceHeaders)
					{
						var incoTerm = invoice.IncoTerm;
						if (!incoTerm.IsEmpty)
						{
							if (invoice.IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(incoTerm, Parent.ChargeCode))
							{
								invoiceAcceptThisCharge = true;
							}
						}
					}

					if (!invoiceAcceptThisCharge)
					{
						Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("252764e1-1ff4-7ca7-4685-2db20f471136", "No invoices' Incoterm allow this charge to be apportioned."));
					}
				}
			}
		}

		#endregion
	}
}
