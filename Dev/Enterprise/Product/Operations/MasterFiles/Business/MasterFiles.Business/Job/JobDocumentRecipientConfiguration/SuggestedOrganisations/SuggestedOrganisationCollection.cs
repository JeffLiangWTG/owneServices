using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class SuggestedOrganisationCollection : NonPersistentBusinessObjectCollection<SuggestedOrganisation>
	{
		public SuggestedOrganisationCollection()
			: base()
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public override bool ReadOnly => true;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("AllowNew is false so this should not get called");
		}
	}
}
