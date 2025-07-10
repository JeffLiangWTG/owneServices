using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class ErrorsRecordCollection : NonPersistentBusinessObjectCollection<ErrorsRecord>
	{
		public ErrorsRecordCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ErrorsRecord();
		}
	}
}
