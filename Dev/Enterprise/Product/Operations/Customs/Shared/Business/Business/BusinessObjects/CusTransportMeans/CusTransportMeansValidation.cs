using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusTransportMeansValidation : AutoCusTransportMeansValidation
	{
		public CusTransportMeansValidation(AutoCusTransportMeans parent)
			: base(parent)
		{
		}

		protected new CusTransportMeans Parent => (CusTransportMeans)base.Parent;

		protected override void CheckTPM_TransportState()
		{
			base.CheckTPM_TransportState();

			ListValidation.ErrorIfInvalidCode(Parent.TPM_TransportStateInfo, Parent.Lookups.TransportStateList);
		}
	}
}
