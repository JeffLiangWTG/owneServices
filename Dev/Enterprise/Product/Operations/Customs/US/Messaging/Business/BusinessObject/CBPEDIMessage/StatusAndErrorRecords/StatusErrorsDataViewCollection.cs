using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class StatusErrorsDataViewCollection : NonPersistentBusinessObjectCollection<ErrorsRecord>
	{
		public StatusErrorsDataViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ErrorsRecord();
		}
	}
}
