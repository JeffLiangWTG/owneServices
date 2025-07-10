using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class ErrorsRecord : AutoErrorsRecord
	{
		public ErrorsRecord(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ErrorsRecord()
			: base()
		{
		}

		public ErrorsRecord(CBPEDIMessage message)
			: base(message.Factory)
		{
			this.message = message;
		}

		public readonly CBPEDIMessage message;
	}
}
