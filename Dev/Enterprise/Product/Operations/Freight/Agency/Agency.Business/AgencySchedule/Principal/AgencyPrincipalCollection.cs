using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyPrincipalCollection : AgencyAllocationItemCollection<AgencyPrincipal, OrgHeader>
	{
		public AgencyPrincipalCollection(AgencyCountry schedule)
			: base(schedule.Principals)
		{
			this.schedule = schedule;
		}

		#region Implementation

		protected override void OnRemoved(BusinessObject bizObj)
		{
			AgencyPrincipal principal = (AgencyPrincipal)bizObj;
			base.OnRemoved(principal);
			principal.Delete();
		}

		protected override void OnAdded(BusinessObject bizObj)
		{
			AgencyPrincipal principal = (AgencyPrincipal)bizObj;
			principal.Schedule = schedule;

			base.OnAdded(bizObj);
		}

		protected override AgencyPrincipal WrapElement(OrgHeader bizObj)
		{
			AgencyPrincipal principal = new AgencyPrincipal(bizObj);
			principal.Schedule = schedule;
			return principal;
		}

		readonly AgencyCountry schedule;

		#endregion
	}
}
