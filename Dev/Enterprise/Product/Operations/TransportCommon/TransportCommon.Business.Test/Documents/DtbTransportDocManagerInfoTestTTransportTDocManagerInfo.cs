using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportDocManagerInfoTest<TTransport, TDocManagerInfo> : DocManagerInfoTestCase
			where TTransport : DtbTransport
			where TDocManagerInfo : DtbTransportDocManagerInfo<TTransport>
	{
		#region GetEmptyParentBusinessObject

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<TTransport>();
		}

		#endregion
	}
}
