using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportDocManagerInfo<T> : DocManagerInfo
		where T : DtbTransport
	{
		protected DtbTransportDocManagerInfo(T transport, ZString docManagerCode)
			: base(transport, docManagerCode)
		{
		}

		#region Transport

		protected T Transport
		{
			get { return (T)BusinessEntity; }
		}

		#endregion
	}
}
