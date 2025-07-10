using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLinesValidation : AutoAccTransactionLinesValidation
	{
		public AccTransactionLinesValidation(AutoAccTransactionLines parent) : base(parent)
		{
		}

		protected override void CheckAL_RX_NKTransactionCurrency()
		{
			base.CheckAL_RX_NKTransactionCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.AL_RX_NKTransactionCurrencyInfo);
		}

		protected override void CheckAL_GC()
		{
			base.CheckAL_GC();
			if (!Parent.AL_GC.IsValid)
			{
				Parent.AL_GCInfo.AddError(Res.GetString("6ed639a7-fc03-4307-a149-cd81fcaa5510", "Please enter valid company."));
			}
			else if (Parent.Branch != null && Parent.AL_GC != Parent.Branch.GB_GC)
			{
				Parent.AL_GCInfo.AddError(Res.GetString("b10bd7e0-1730-4494-9290-33ce024d7d7b", "The Company you entered doesn't match Current Branch."));
			}
			else if (Parent.TransactionHeader != null && Parent.TransactionHeader.AH_GC.IsValid && Parent.AL_GC.IsValid && Parent.TransactionHeader.AH_GC != Parent.AL_GC)
			{
				Parent.AL_GCInfo.AddError(Res.GetString("1D98594B-0A5C-41B5-A1EC-371981B9B4CB", "The company of header: {0} is inconsistent with the company of line: {1}.", Parent.TransactionHeader.Company?.GC_Code, Parent.Company?.GC_Code));
			}
		}

		protected virtual bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return false;
			}
		}

		protected virtual INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				return CargoWise.EntityFramework.NotificationType.Error;
			}
		}

		protected override void CheckAL_GE()
		{
			base.CheckAL_GE();

			if ((ShouldValidateBranchDepartmentCombinationForParentInDatabase || !Parent.IsInDatabase)
				|| Parent.AL_GBInfo.HasChanges || Parent.AL_GEInfo.HasChanges)
			{
				GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Parent.AL_GEInfo, Parent.Branch, Parent.Department, NotificationTypeForBranchDepartmentCombination);
			}
		}

		protected override void CheckAL_TaxDate()
		{
			base.CheckAL_TaxDate();

			if (Parent?.TaxRate == null)
			{
				return;
			}

			MandatoryValidation.CheckEntered(Parent.AL_TaxDateInfo);
			if (!Parent.AL_TaxDateInfo.HasErrors())
			{
				var rateExists = Parent.TaxRate.DoesRateExists(Parent.AL_TaxDate);
				if (!rateExists)
				{
					Parent.AL_TaxDateInfo.AddError(Res.GetString("87fc0d41-ecd2-47af-a361-cc5256e25f1a", "No rate found for selected date."));
				}
			}
		}

		protected override void CheckAL_A9_VATClass()
		{
			base.CheckAL_A9_VATClass();
			if (!Parent.AL_A9_VATClassInfo.HasErrors())
			{
				CheckTaxIdAndTaxMessageMapping();
			}
		}

		void CheckTaxIdAndTaxMessageMapping()
		{
			var taxIDAndTaxMessageMappingHelper = ObjectFactory.Get<ITaxIdAndTaxMessageMappingHelper>();
			var errorMessage = taxIDAndTaxMessageMappingHelper?.ValidateMappingForLine((AccTransactionLines)Parent);
			if (errorMessage != null)
			{
				Parent.AL_A9_VATClassInfo.AddError(errorMessage);
			}
		}
	}
}
