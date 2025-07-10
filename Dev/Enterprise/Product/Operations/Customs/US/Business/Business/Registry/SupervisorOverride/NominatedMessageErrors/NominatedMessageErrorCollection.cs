using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public class NominatedMessageErrorCollection : NonPersistentBusinessObjectCollection<NominatedMessageError>
	{
		public NominatedMessageErrorCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NominatedMessageError();
		}
	}
}
