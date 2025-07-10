using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ErrorsRecordCollection : Messaging.Business.ErrorsRecordCollection
	{
		public ErrorsRecordCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new ErrorsRecord AddNew()
		{
			return (ErrorsRecord)base.AddNew();
		}

		public new ErrorsRecord this[int index]
		{
			get { return (ErrorsRecord)base[index]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ErrorsRecord();
		}
	}
}
