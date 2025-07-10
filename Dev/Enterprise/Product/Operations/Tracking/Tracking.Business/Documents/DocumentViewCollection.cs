using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class DocumentViewCollection : NonPersistentBusinessObjectCollection<DocumentView>
	{
		public DocumentViewCollection(BusinessObjectFactory factory) : base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentView(Factory);
		}
	}
}
