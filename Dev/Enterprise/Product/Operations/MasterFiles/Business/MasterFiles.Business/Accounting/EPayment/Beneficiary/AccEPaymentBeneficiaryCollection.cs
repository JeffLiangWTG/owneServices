using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentBeneficiaryCollection : BusinessObjectCollection<AccEPaymentBeneficiary>
	{
		public AccEPaymentBeneficiaryCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}
		public AccEPaymentBeneficiaryCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore => false;
	}
}
