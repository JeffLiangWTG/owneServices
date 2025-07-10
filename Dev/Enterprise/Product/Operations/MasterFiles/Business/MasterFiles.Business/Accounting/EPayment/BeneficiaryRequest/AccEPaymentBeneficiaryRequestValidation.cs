using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentBeneficiaryRequestValidation : AutoAccEPaymentBeneficiaryRequestValidation
	{
		public AccEPaymentBeneficiaryRequestValidation(AutoAccEPaymentBeneficiaryRequest parent) : base(parent)
		{
			Parent = parent as AccEPaymentBeneficiaryRequest;
		}

		protected new AccEPaymentBeneficiaryRequest Parent;

		protected override void CheckABR_GC_Company()
		{
			base.CheckABR_GC_Company();
			if (!Parent.ABR_GC_CompanyInfo.HasErrors() && Parent.Company == null)
			{
				Parent.ABR_GC_CompanyInfo.AddError(ResString.GetMultilingualString("A664A0D7-CF55-482A-9ED6-B9F2F004753B", "Beneficiary request must specify a valid Company."));
			}
		}

		protected override void CheckABR_InternalReference()
		{
			base.CheckABR_InternalReference();
			MandatoryValidation.CheckEntered(Parent.ABR_InternalReferenceInfo);
		}

		protected override void CheckABR_ProviderCode()
		{
			base.CheckABR_ProviderCode();
			MandatoryValidation.CheckEntered(Parent.ABR_ProviderCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ABR_ProviderCodeInfo);
		}

		protected override void CheckABR_Status()
		{
			base.CheckABR_Status();
			MandatoryValidation.CheckEntered(Parent.ABR_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ABR_StatusInfo);
		}

		protected override void CheckABR_ErrorDescription()
		{
			base.CheckABR_ErrorDescription();
			if (!Parent.ABR_ErrorDescriptionInfo.HasErrors() && !Parent.ABR_ErrorDescription.IsEmpty && Parent.ABR_Status != StatusCodes.Error)
			{
				Parent.ABR_ErrorDescriptionInfo.AddError(ResString.GetMultilingualString("2849E771-4BC2-4235-93A5-D86DF32D5261", "Error Description should only be recorded if the status is ERR."));
			}
		}
	}
}
