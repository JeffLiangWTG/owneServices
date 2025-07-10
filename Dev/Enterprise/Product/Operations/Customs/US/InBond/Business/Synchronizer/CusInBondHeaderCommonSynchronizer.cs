using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public abstract class CusInBondHeaderCommonSynchronizer : BusinessObjectSynchroniser
	{
		protected CusInBondHeaderCommonSynchronizer(CusInBondHeader destination, BusinessObject source)
			: base(destination, source)
		{
		}

		protected new CusInBondHeader Destination
		{
			get { return (CusInBondHeader)base.Destination; }
		}

		public abstract ForwardingConsol RelevantConsol { get; }
	}
}
