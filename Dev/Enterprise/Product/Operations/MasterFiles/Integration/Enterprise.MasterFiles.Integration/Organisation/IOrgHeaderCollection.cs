using System.Collections;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgHeaderCollection : IList
	{
		bool AllowNewTemporaryOrganisations { get; }
		void SetDefaultsForNewChild(object bizObj);
	}
}