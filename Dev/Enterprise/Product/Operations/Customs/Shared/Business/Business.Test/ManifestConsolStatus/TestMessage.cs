using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class TestMessage : EDIMessage
	{
		public TestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return "1";
		}

		protected override string GetSendersReference()
		{
			return ((ForwardingConsol)EM_LinkedObject).JK_UniqueConsignRef;
		}
	}
}
