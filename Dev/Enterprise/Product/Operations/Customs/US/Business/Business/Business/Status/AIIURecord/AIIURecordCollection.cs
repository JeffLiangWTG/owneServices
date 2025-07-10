using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AIIURecordCollection : NonPersistentBusinessObjectCollection<AIIURecord>
	{
		public AIIURecordCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AIIURecord();
		}
	}
}
