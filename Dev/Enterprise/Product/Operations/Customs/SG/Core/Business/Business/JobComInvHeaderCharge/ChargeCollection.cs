
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ChargeCollection : BusinessObjectCollection<Customs.Business.BaseJobComInvHeaderCharge>
	{
		public ChargeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject AddNewCore()
		{
			return null;
		}
	}
}
