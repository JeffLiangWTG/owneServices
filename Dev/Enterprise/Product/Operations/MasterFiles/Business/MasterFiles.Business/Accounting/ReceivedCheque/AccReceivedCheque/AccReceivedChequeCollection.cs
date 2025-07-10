using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccReceivedChequeCollection : BusinessObjectCollection<AccReceivedCheque>
	{
		public AccReceivedChequeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
