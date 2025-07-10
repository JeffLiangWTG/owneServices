using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefLanguageTextPageEntryCollection : NonPersistentBusinessObjectCollection<RefLanguageTextPageEntry>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
