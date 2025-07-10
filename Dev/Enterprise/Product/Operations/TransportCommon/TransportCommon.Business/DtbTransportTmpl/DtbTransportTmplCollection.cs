using CargoWise.EntityFramework;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportTmplCollection : ActiveBusinessObjectCollection<DtbTransportTmpl>
	{
		protected DtbTransportTmplCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected DtbTransportTmplCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
