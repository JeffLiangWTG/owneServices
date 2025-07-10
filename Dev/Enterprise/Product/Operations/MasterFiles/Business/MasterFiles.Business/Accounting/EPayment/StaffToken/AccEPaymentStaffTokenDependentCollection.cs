using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Accounting.EPayment
{
	public class AccEPaymentStaffTokenDependentCollection : DependentBusinessObjectCollection<AccEPaymentStaffToken, AccBankAccount>
	{
		public AccEPaymentStaffTokenDependentCollection(AccBankAccount parent)
			: base(parent)
		{ }
	}
}
