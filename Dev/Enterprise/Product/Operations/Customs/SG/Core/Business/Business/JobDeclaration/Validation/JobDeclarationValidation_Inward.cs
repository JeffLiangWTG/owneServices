
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class JobDeclarationValidation_Inward : CUSDECValidation
	{
		public JobDeclarationValidation_Inward(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo, "Importer");
		}

		protected override void CheckJE_OH_Forwarder()
		{
			CheckForwarder();
		}
	}
}
