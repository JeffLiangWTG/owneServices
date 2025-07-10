using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Business
{
	public class DuplicationModelDetailCollection : NonPersistentBusinessObjectCollection<DuplicationModelDetail>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}

		protected override bool AllowNewCore => false;
	}
}
