using System.Collections.Generic;

namespace Enterprise.Customs.Business
{
	public interface ISynchroniserReadOnlyMembersProvider
	{
		List<string> SynchroniserReadOnlyMembers { get; }
	}
}
