using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class EntryHeaderWithDeactivatedCollection : ActiveBusinessObjectCollection<CusEntryHeader>
	{
		public EntryHeaderWithDeactivatedCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)Relationship.Master; }
		}

		protected override bool MatchesFilterCore(CusEntryHeader element, bool fetchOnlyFromLocalCache)
		{
			bool result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);

			if (result)
			{
				result = Declaration.ShowDeactivatedEntries || element.IsActive;
			}
			return result;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
