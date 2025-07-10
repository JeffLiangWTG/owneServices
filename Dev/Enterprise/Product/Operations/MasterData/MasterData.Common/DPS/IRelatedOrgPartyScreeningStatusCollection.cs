using System.Collections;

namespace Enterprise.MasterData.Common
{
	public interface IRelatedOrgPartyScreeningStatusCollection : IList
	{
		new IRelatedOrgPartyScreeningStatus this[int index] { get; }
	}
}
