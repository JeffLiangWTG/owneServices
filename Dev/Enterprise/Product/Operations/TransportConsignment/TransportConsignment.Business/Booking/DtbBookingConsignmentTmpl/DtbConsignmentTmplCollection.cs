using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentTmplCollection : DtbTransportTmplCollection
	{
		public DtbConsignmentTmplCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DtbConsignmentTmplCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public new DtbConsignmentTmpl this[int index]
		{
			get { return (DtbConsignmentTmpl)base[index]; }
		}
	}
}
