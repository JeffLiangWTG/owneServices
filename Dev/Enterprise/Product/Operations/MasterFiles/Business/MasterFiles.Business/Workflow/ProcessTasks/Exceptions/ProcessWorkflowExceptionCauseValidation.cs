using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionCauseValidation : AutoProcessWorkflowExceptionCauseValidation
	{
		public ProcessWorkflowExceptionCauseValidation(AutoProcessWorkflowExceptionCause parent)
			: base(parent)
		{
		}

		protected override void CheckWEC_Code()
		{
			base.CheckWEC_Code();
			MandatoryValidation.CheckEntered(Parent.WEC_CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.WEC_CodeInfo, Parent.Type.Causes);
		}

		protected override void CheckWEC_Description()
		{
			base.CheckWEC_Description();
			MandatoryValidation.CheckEntered(Parent.WEC_DescriptionInfo);
		}
	}
}
