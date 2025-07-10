//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccExchangeRateConfigurationViewValidation
//
//    This class should be used for overriding validation in AutoAccExchangeRateConfigurationViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;

	public class AccExchangeRateConfigurationViewValidation : AutoAccExchangeRateConfigurationViewValidation
	{
		public AccExchangeRateConfigurationViewValidation(AutoAccExchangeRateConfigurationView parent) : base(parent)
		{
		}

		protected override void CheckJCE_Ledger()
		{
			base.CheckJCE_Ledger();
			ListValidation.ErrorIfInvalidCode(Parent.JCE_LedgerInfo);
			if (Parent.JCE_JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code && Parent.JCE_Ledger != LedgerTypes.AccountsPayable)
			{
				Parent.JCE_LedgerInfo.AddError(Res.GetString("39207914-3a82-4272-b80b-5d86d1355829", "Forwarding Consol can only have configuration for AP ledger."));
			}
			CheckItIsNotDuplicate();
		}

		protected override void CheckJCE_LedgerIsNotEmpty()
		{
			if (!Parent.JCE_ParentID.IsEmpty)
			{
				base.CheckJCE_LedgerIsNotEmpty();
			}
		}

		protected override void CheckJCE_JobType()
		{
			base.CheckJCE_JobType();
			ListValidation.ErrorIfInvalidCode(Parent.JCE_JobTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckJCE_ServiceDirection()
		{
			base.CheckJCE_ServiceDirection();
			ListValidation.ErrorIfInvalidCode(Parent.JCE_ServiceDirectionInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckJCE_TransportMode()
		{
			base.CheckJCE_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.JCE_TransportModeInfo);
			CheckItIsNotDuplicate();
		}

		//This will be removed when we ensure the transition from using JCE_ExRateType to JCT_ExRateType work correctly in WI00857561

		protected override void CheckJCE_ExRateTypeIsNotEmpty()
		{
			return;
		}

		protected override void CheckJCE_ExRateTypeIsWesternEuropean()
		{
			return;
		}

		protected override void CheckJCE_Preference()
		{
			base.CheckJCE_Preference();
			ListValidation.ErrorIfInvalidCode(Parent.JCE_PreferenceInfo);
		}

		protected override void CheckJCE_InvoiceCurrencyType()
		{
			base.CheckJCE_InvoiceCurrencyType();
			ListValidation.ErrorIfInvalidCode(Parent.JCE_InvoiceCurrencyTypeInfo);
			CheckItIsNotDuplicate();
			CheckIncorrectInvoiceCurrencyTypeConfigCombination();
		}

		protected override void CheckJCE_InvoiceCurrencyTypeIsNotEmpty()
		{
		}

		protected override void CheckJCE_OffsetIsNotEmpty()
		{
		}

		protected override void CheckJCE_ParentTableCodeIsNotEmpty()
		{
			if (!Parent.JCE_ParentID.IsEmpty)
			{
				base.CheckJCE_ParentTableCodeIsNotEmpty();
			}
		}

		#region JCE_Calc_CurrencyType

		public void ValidateJCE_Calc_CurrencyType()
		{
			ValidateCalculatedProperty(Parent.JCE_Calc_CurrencyTypeInfo);
		}

		protected void CheckJCE_Calc_CurrencyType()
		{
			MandatoryValidation.CheckEntered(Parent.JCE_Calc_CurrencyTypeInfo);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JCE_Calc_CurrencyTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JCE_Calc_CurrencyTypeInfo);

			if (((IBusinessObjectInternals)Parent).IsInPreSaveValidation)	//Defer the validation to "Validate All" or "Save Form", according to the specification.
			{
				if (!Parent.JCE_Calc_CurrencyTypeInfo.HasErrors())
				{
					CheckItIsNotDuplicate();
				}

				if (!Parent.JCE_Calc_CurrencyTypeInfo.HasErrors())
				{
					if (Parent.IsCurrencyTypeAll && Parent.HasCurrency)
					{
						Parent.JCE_Calc_CurrencyTypeInfo.AddError(Res.GetString("2403AC23-D434-4B61-9183-A1DC28B82EC0", "Cannot set Currency Selection to ALL. Please remove Custom Currency Configuration."));
					}
					else if (Parent.IsCurrencyTypeCUR && !Parent.HasCurrency)
					{
						Parent.JCE_Calc_CurrencyTypeInfo.AddError(Res.GetString("94F0B987-E6FA-494B-8F41-05DCD9D9ADDA", "Cannot set Currency Selection to CUR. Please add at least one Currency Configuration."));
					}
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJCE_Calc_CurrencyType();
		}

		void CheckItIsNotDuplicate()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			var parentCollection = (AccExchangeRateConfigurationCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is AccExchangeRateConfigurationCollection);

			if (parentCollection == null)
			{
				return;
			}

			if (parentCollection.Cast<AccExchangeRateConfiguration>().Any(c => c != Parent && Parent.IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		void CheckIncorrectInvoiceCurrencyTypeConfigCombination()
		{
			var incorrectCombinationErrorMessage = Res.GetString("196ebc65-2177-4855-bacf-fef182b30c89", @"You cannot have duplicate configurations with invoice currency type equals LOC, FOR and ALL at the same time.
It should only allow configuration at the same time when: LOC & FOR / LOC & Blank / FOR & Blank");
			Parent.RemoveRowError(incorrectCombinationErrorMessage);

			if (!Parent.HasRowErrors)
			{
				var parentCollection = (AccExchangeRateConfigurationCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is AccExchangeRateConfigurationCollection);

				if (parentCollection == null)
				{
					return;
				}

				var duplicateConfigsWithoutInvoiceCurrencyType = parentCollection.Cast<AccExchangeRateConfiguration>().Where(c => c != Parent
					&& Parent.JCE_GC == c.JCE_GC
					&& Parent.JCE_ParentTableCode == c.JCE_ParentTableCode
					&& Parent.JCE_ParentID == c.JCE_ParentID
					&& Parent.JCE_Ledger == c.JCE_Ledger
					&& Parent.JCE_JobType == c.JCE_JobType
					&& Parent.JCE_ServiceDirection == c.JCE_ServiceDirection
					&& Parent.JCE_TransportMode == c.JCE_TransportMode);

				if (duplicateConfigsWithoutInvoiceCurrencyType.Select(x => x.JCE_InvoiceCurrencyType).Distinct().Count() > 1)
				{
					Parent.AddRowError(incorrectCombinationErrorMessage);
				}
			}
		}

		new AccExchangeRateConfiguration Parent => (AccExchangeRateConfiguration)base.Parent;

		internal static string IsDuplicateErrorString => Res.GetString("01d7da8b-7ed7-4eed-89d9-d7de4dee16ea", "At least one more record already sets Exchange Rate details for the same Job parameters.");
	}
}
