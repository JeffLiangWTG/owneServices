using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaignItemContactCollection : IActiveBusinessObjectCollection, IEnumerable<IGlbCompanyCampaignItem>
	{
		new IGlbCompanyCampaignItem this[int index] { get; }
	}
}
