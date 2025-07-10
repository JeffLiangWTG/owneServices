using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbHolidayDependentCollection : ActiveBusinessObjectCollection<GlbHoliday>
	{
		public GlbHolidayDependentCollection(GlbBranch branch, BusinessObjectFactory factory)
			: base(factory, branch, new ZQuery(), GlbHolidaySchema.GH_ParentID)
		{
		}

		#region Contains Holiday

		public bool Contains(ZDate date)
		{
			foreach (GlbHoliday holiday in this)
			{
				if (holiday.MatchesDate(date))
				{
					return true;
				}
			}
			return false;
		}

		#endregion
	}
}
