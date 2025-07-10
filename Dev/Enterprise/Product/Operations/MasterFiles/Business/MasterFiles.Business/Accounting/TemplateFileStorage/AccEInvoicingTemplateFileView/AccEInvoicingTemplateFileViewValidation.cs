//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccEInvoicingTemplateFileViewValidation
//
//    This class should be used for overriding validation in AutoAccEInvoicingTemplateFileViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccEInvoicingTemplateFileViewValidation : AutoAccEInvoicingTemplateFileViewValidation
	{
		public AccEInvoicingTemplateFileViewValidation(AutoAccEInvoicingTemplateFileView parent) : base(parent)
		{
		}

		protected override void CheckETF_JobType()
		{
			base.CheckETF_JobType();
			MandatoryValidation.CheckEntered(Parent.ETF_JobTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETF_JobTypeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckETF_TransportMode()
		{
			base.CheckETF_TransportMode();
			MandatoryValidation.CheckEntered(Parent.ETF_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETF_TransportModeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckETF_TemplateCode()
		{
			base.CheckETF_TemplateCode();
			MandatoryValidation.CheckEntered(Parent.ETF_TemplateCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETF_TemplateCodeInfo);
		}

		protected override void CheckETF_ParentTableCodeIsNotEmpty()
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckItIsNotDuplicate();
		}

		void CheckItIsNotDuplicate()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);

			var parentCollection = (AccEInvoicingTemplateFileViewCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault();

			if (parentCollection != null && parentCollection.Cast<AccEInvoicingTemplateFileView>().Any(c => c != Parent && ((AccEInvoicingTemplateFileView)Parent).IsDuplicateOf(c)))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
		}

		public static string IsDuplicateErrorString => Res.GetString("AccEInvoicingTemplateFileViewValidation|DuplicateError", "At least one more record already sets Template File for the same Job parameters.");
	}
}
