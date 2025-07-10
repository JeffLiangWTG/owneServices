using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.GUI
{
	public class UserSailingManagerQueryProvider : ISailingManagerQueryProvider
	{
		public static void Register(BusinessObjectFactory factory)
		{
			SailingManagerQueryProviderFactory.Set(factory, new UserSailingManagerQueryProvider());
		}

		public SailingManagerUpdateMode QueryFreshMatchBehaviour(QueryFreshMatchBehaviourArgs queryArgs)
		{
			if (queryArgs.RequestedDate == queryArgs.FoundDate ||
				queryArgs.AddCheckpoint != null && !queryArgs.AddCheckpoint.IsAllowed && queryArgs.EditCheckpoint != null && !queryArgs.EditCheckpoint.IsAllowed)
			{
				selectedByUser = false;
				return SailingManagerUpdateMode.ScheduleUnchanged;
			}
			else
			{
				selectedByUser = true;
				return ScheduleUpdateDialog.ShowDialog(queryArgs);
			}
		}

		public bool SelectedByUser => selectedByUser;

		bool selectedByUser;
	}
}
