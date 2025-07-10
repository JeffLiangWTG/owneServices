using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class SalesRepFilterBusinessObject : GlbStaffFilterBusinessObject
	{
		#region System

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(GlbStaffSchema.GS_IsSalesRep, ZBool.True);
				return query;
			}
		}

		#endregion

		/// <summary>
		/// Hide the flags because they are not suitable for the Sales Rep module
		/// </summary>
		protected override void AddFlagsFilters(ModuleFilterCollection filters)
		{
		}

		protected override bool IsActiveStatusFilterAlwaysApplied()
		{
			return false;
		}
	}
}
