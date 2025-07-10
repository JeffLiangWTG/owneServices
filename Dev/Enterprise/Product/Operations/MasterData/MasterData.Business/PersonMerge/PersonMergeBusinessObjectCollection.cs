using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Business
{
	public class PersonMergeBusinessObjectCollection : NonPersistentBusinessObjectCollection<PersonMergeBusinessObject>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PersonMergeBusinessObject();
		}
	}
}
