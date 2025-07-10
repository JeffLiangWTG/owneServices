using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportLookups : DtbBookingLookups
	{
		protected DtbTransportLookups(DtbTransport parent)
			: base(parent)
		{
		}

		#region BookingTemplates

		public DtbTransportTmplCollection BookingTemplates
		{
			get { return GetTransportTemplates(); }
		}

		protected abstract DtbTransportTmplCollection GetTransportTemplates();

		#endregion

		#region BindToLists

		public TransportBindToLists BindToLists
		{
			get { return GetBindToLists(); }
		}

		protected virtual TransportBindToLists GetBindToLists()
		{
			return Factory.GetCachedValue("TransportCommon|BindToLists", () => new TransportBindToLists(Factory));
		}

		#endregion
	}
}
