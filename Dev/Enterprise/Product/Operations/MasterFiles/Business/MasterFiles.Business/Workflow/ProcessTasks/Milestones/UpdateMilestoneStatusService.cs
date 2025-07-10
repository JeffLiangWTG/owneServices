using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// </summary>
	public class UpdateMilestoneStatusService
	{
		readonly Dictionary<ZGuid, MilestoneCollectionView> milestoneCollectionToUpdate = new Dictionary<ZGuid, MilestoneCollectionView>();

		public bool AddView(MilestoneCollectionView view)
		{
			var parent = view.Parent;
			if (parent != null && !parent.IsDeleted && !milestoneCollectionToUpdate.ContainsKey(parent.PK))
			{
				milestoneCollectionToUpdate[parent.PK] = view;
				return true;
			}
			return false;
		}

		public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var view in milestoneCollectionToUpdate.Values.Where(v => !v.Parent.IsDeleted))
			{
				view.SetMilestoneStatuses();
			}

			milestoneCollectionToUpdate.Clear();
		}
	}
}
