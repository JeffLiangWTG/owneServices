using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.TaxDateDefaultingOptionLookups;

namespace Enterprise.MasterFiles.Business
{
	public class TaxDateDefaultingOptionValidation : JobConfigurationSelectorValidation
	{
		public TaxDateDefaultingOptionValidation(TaxDateDefaultingOption parent) : base(parent)
		{
		}

		protected new TaxDateDefaultingOption Parent
		{
			get { return (TaxDateDefaultingOption)base.Parent; }
		}

		#region JobType

		public override void ValidateJobType()
		{
			Parent.JobTypeInfo.ClearAllNotifications();
			base.ValidateJobType();
			if (!Parent.JobTypeInfo.HasErrors())
			{
				ValidateTaxDateOption();
			}
			if (!Parent.JobTypeInfo.HasErrors())
			{
				ValidateLedger();
			}
		}

		protected override bool IsDuplicateJobParameter(IJobConfigurationSelector item)
		{
			var taxDateDefaultingOptionItem = (TaxDateDefaultingOption)item;

			return base.IsDuplicateJobParameter(item)
				&& (taxDateDefaultingOptionItem.Ledger == LedgerTypeAdditionalCodes.All || taxDateDefaultingOptionItem.Ledger == Parent.Ledger);
		}

		protected override string DuplicateJobParametersError
		{
			get
			{
				return Res.GetString("683ed831-3ddc-4190-abd5-bd6a1b1b12f4", "At least one more record already sets tax date option for the same Job parameters.");
			}
		}

		#endregion

		public void ValidateTaxDateOption()
		{
			Parent.TaxDateOptionInfo.ClearAllNotifications();
			if (!Parent.TaxDateOptionInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.TaxDateOptionInfo, Res.GetString("68661fb3-4735-405e-ad83-ceba53430846", "Tax Date Option"));
				ListValidation.ErrorIfInvalidCode(Parent.TaxDateOptionInfo, Parent.TaxDateOptionList);
			}
		}

		public void ValidateLedger()
		{
			Parent.LedgerInfo.ClearAllNotifications();
			if (!Parent.LedgerInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.LedgerInfo, Res.GetString("7c7f6525-9574-4b76-8f78-4307f4364722", "Ledger"));
				ListValidation.ErrorIfInvalidCode(Parent.LedgerInfo, Parent.LedgerList);
				if (!Parent.LedgerInfo.HasErrors() &&
					Parent.Ledger != LedgerTypes.AccountsPayable && Parent.JobType == JobInvoicingConsumerTypes.ForwardingConsolCode)
				{
					Parent.LedgerInfo.AddError(Res.GetString("b228056b-9f46-41f7-9419-eaffc22069fd", "Forwarding Consol can only have configuration for AP ledger."));
				}
			}
		}
	}
}
