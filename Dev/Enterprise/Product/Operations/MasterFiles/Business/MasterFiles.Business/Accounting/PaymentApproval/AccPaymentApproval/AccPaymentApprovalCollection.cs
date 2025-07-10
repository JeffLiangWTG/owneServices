using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccPaymentApprovalCollection : BusinessObjectCollection<AccPaymentApproval>
	{
		public AccPaymentApprovalCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
