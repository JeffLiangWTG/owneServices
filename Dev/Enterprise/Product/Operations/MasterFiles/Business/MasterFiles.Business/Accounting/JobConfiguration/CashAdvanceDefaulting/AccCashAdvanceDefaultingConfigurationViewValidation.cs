using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceDefaultingConfigurationViewValidation : AutoAccCashAdvanceDefaultingConfigurationViewValidation
	{
		public AccCashAdvanceDefaultingConfigurationViewValidation(AutoAccCashAdvanceDefaultingConfigurationView parent) : base(parent)
		{
		}

		protected override void CheckCAC_Ledger()
		{
			base.CheckCAC_Ledger();
			ListValidation.ErrorIfInvalidCode(Parent.CAC_LedgerInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckCAC_LedgerIsNotEmpty()
		{
			//Ledger should not be empty if it is has Organization Level
			if (Parent.CAC_ParentTableCode == OrgHeaderSchema.Constants.Prefix)
			{
				base.CheckCAC_LedgerIsNotEmpty();
			}
		}

		protected override void CheckCAC_JobType()
		{
			base.CheckCAC_JobType();
			ListValidation.ErrorIfInvalidCode(Parent.CAC_JobTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckCAC_ServiceDirection()
		{
			base.CheckCAC_ServiceDirection();
			ListValidation.ErrorIfInvalidCode(Parent.CAC_ServiceDirectionInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckCAC_TransportMode()
		{
			base.CheckCAC_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.CAC_TransportModeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckCAC_DefaultingOption()
		{
			base.CheckCAC_DefaultingOption();
			ListValidation.ErrorIfInvalidCode(Parent.CAC_DefaultingOptionInfo);
		}

		protected override void CheckCAC_ParentTableCodeIsNotEmpty()
		{
			// CAC_ParentTableCode may be empty.
		}

		void CheckItIsNotDuplicate()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			var parentCollection = (AccCashAdvanceDefaultingConfigurationCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is AccCashAdvanceDefaultingConfigurationCollection);

			if (parentCollection == null)
			{
				return;
			}

			if (parentCollection.Cast<AccCashAdvanceDefaultingConfiguration>().Any(c => c != Parent && ((AccCashAdvanceDefaultingConfiguration)Parent).IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		internal static string IsDuplicateErrorString => Res.GetString("fb7d82df-be35-4763-9c2a-5944634451bb", "Another record already sets Advance Payment Defaulting Configuration for the same Job parameters.");
	}
}
