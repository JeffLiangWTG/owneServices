using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AIIERecordCollection : NonPersistentBusinessObjectCollection<AIIERecord>
	{
		public AIIERecordCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AIIERecord();
		}
	}
}
