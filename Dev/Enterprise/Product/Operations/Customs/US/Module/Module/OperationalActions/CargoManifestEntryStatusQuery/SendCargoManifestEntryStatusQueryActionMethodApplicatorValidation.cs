using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation : USActionMethodApplicatorValidation
	{
		public SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation(SendCargoManifestEntryStatusQueryActionMethodApplicator parent)
			: base(parent)
		{
		}

		public new SendCargoManifestEntryStatusQueryActionMethodApplicator Parent => (SendCargoManifestEntryStatusQueryActionMethodApplicator)base.Parent;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();

			ValidateOutputOption();
			ValidateAction();
		}

		public void ValidateAction()
		{
			zValidationInternals.Validate(Parent.ActionInfo, new RunValidationInvoker(this.CheckAction));
		}

		public void ValidateOutputOption()
		{
			zValidationInternals.Validate(Parent.OutputOptionInfo, new RunValidationInvoker(this.CheckOutputOption));
		}

		protected void CheckAction()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ActionInfo, Parent.Lookups.ActionList);
			MandatoryValidation.CheckEntered(Parent.ActionInfo);
		}

		protected void CheckOutputOption()
		{
			ListValidation.ErrorIfInvalidCode(Parent.OutputOptionInfo, Parent.Lookups.OutputOptionList);

			if (Parent.UpdateEntryWithResults && !Parent.OutputOption.IsEmpty && !Parent.OutputOption.EqualsIgnoringCase(LimitOutputCodeList.Codes._0MostRecentResults))
			{
				Parent.OutputOptionInfo.AddError(OutputOptionErrorMessage);
			}
		}

		internal const string OutputOptionErrorMessage = "Output Option should be 'Most recent results' if Update Entry with Results is ticked.";
	}
}
