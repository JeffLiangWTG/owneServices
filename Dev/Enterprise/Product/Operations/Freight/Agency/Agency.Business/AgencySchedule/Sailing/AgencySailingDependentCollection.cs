using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencySailingDependentCollection : AgencyAllocationItemCollection<AgencySailing, JobSailing>
	{
		public AgencySailingDependentCollection(AgencyPrincipal principal)
			: base(principal.Schedule.Voyage.Sailings)
		{
			this.principal = principal;
		}

		#region Implementation

		protected override void OnRemoved(BusinessObject bizObj)
		{
			AgencySailing sailing = (AgencySailing)bizObj;
			base.OnRemoved(sailing);
			sailing.Principal = null;
		}

		protected override void OnAdded(BusinessObject bizObj)
		{
			AgencySailing sailing = (AgencySailing)bizObj;
			base.OnAdded(sailing);
			sailing.Principal = principal;
		}

		protected override void HookKeyChangeEvents(JobSailing bizObj)
		{
			base.HookKeyChangeEvents(bizObj);
			bizObj.JX_JA_RL_NKPortOfLoadingInfo.ValueChanged += new EventHandler(KeyValueChangedHandler);
		}

		protected override void UnHookKeyChangeEvents(JobSailing bizObj)
		{
			base.UnHookKeyChangeEvents(bizObj);
			bizObj.JX_JA_RL_NKPortOfLoadingInfo.ValueChanged -= new EventHandler(KeyValueChangedHandler);
		}

		protected override bool ShouldBeInThisCollection(JobSailing bizObj)
		{
			return bizObj.Origin != null && bizObj.Origin.JA_RL_NKPortOfLoading.StartsWith(principal.VoyageCountry.J0_RN_NKCountry, StringComparison.OrdinalIgnoreCase);
		}

		protected override AgencySailing WrapElement(JobSailing bizObj)
		{
			return new AgencySailing(bizObj, principal.PrincipalPK);
		}

		readonly AgencyPrincipal principal;

		#endregion
	}
}
