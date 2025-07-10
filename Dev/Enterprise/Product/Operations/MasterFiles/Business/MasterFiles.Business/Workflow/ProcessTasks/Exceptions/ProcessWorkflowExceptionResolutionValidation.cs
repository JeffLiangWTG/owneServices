using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionResolutionValidation : AutoProcessWorkflowExceptionResolutionValidation
	{
		public ProcessWorkflowExceptionResolutionValidation(AutoProcessWorkflowExceptionResolution parent)
			: base(parent)
		{
		}

		protected override void CheckWER_Code()
		{
			base.CheckWER_Code();
			MandatoryValidation.CheckEntered(Parent.WER_CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.WER_CodeInfo, Parent.Type.Resolutions);
		}

		protected override void CheckWER_Description()
		{
			base.CheckWER_Description();
			MandatoryValidation.CheckEntered(Parent.WER_DescriptionInfo);
		}
	}
}
