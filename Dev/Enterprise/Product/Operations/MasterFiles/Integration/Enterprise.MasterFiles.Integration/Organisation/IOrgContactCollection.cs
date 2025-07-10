using System.Collections;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgContactCollection : ICollection
	{
		bool UseOrgCodeFilter { get; set; }
	}
}
