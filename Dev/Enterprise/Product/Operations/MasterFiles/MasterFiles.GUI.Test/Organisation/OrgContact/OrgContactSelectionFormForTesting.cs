using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgContactSelectionFormForTesting : OrgContactSelectionForm
	{
		public OrgContactSelectionFormForTesting(FilteredContactsCollectionWrapper wrapper)
						: base(wrapper)
		{
		}

		public new ZGrid ContactGrid
		{
			get { return base.ContactGrid; }
		}

		public new ZCheckBox ShowInactiveContactsCheckBox
		{
			get { return base.ShowInactiveContactsCheckBox; }
		}
	}
}
