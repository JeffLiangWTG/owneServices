using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMMessageChooserValidation : MessageChooserValidation
	{
		public AIMMessageChooserValidation(AIMMessageChooser parent) : base(parent)
		{
		}

		AIMMessageChooser Chooser => (AIMMessageChooser)Parent;

		public void ValidateReason()
		{
			ValidateCalculatedProperty(Chooser.ReasonInfo);
		}

		public void ValidateRequestCode()
		{
			ValidateCalculatedProperty(Chooser.RequestCodeInfo);
		}

		protected void CheckReason()
		{
			var chooser = Chooser;

			if (chooser.IsChangeOrCancellation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(chooser.ReasonInfo);
			}
		}

		protected void CheckRequestCode()
		{
			var chooser = Chooser;

			if (chooser.IsFreightStatusQuery)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(chooser.RequestCodeInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReason();
			ValidateRequestCode();
		}
	}
}
