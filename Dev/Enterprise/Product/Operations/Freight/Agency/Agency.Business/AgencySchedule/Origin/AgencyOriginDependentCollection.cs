using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyOriginDependentCollection : AgencyAllocationItemCollection<AgencyOrigin, VoyageOrigin>
	{
		public AgencyOriginDependentCollection(AgencyPrincipal principal)
			: base(principal.Schedule.Voyage.Origins)
		{
			this.principal = principal;
		}

		#region Implementation

		protected override void OnRemoved(BusinessObject bizObj)
		{
			AgencyOrigin origin = (AgencyOrigin)bizObj;
			base.OnRemoved(origin);
			origin.Principal = null;
		}

		protected override void OnAdded(BusinessObject bizObj)
		{
			AgencyOrigin origin = (AgencyOrigin)bizObj;
			base.OnAdded(origin);
			origin.Principal = principal;
		}

		protected override void HookKeyChangeEvents(VoyageOrigin bizObj)
		{
			base.HookKeyChangeEvents(bizObj);
			bizObj.JA_RL_NKPortOfLoadingInfo.ValueChanged += new EventHandler(KeyValueChangedHandler);
		}

		protected override void UnHookKeyChangeEvents(VoyageOrigin bizObj)
		{
			base.UnHookKeyChangeEvents(bizObj);
			bizObj.JA_RL_NKPortOfLoadingInfo.ValueChanged -= new EventHandler(KeyValueChangedHandler);
		}

		protected override AgencyOrigin WrapElement(VoyageOrigin bizObj)
		{
			return new AgencyOrigin(bizObj, principal.PrincipalPK);
		}

		protected override bool ShouldBeInThisCollection(VoyageOrigin bizObj)
		{
			return bizObj.JA_RL_NKPortOfLoading.StartsWith(principal.VoyageCountry.J0_RN_NKCountry, StringComparison.OrdinalIgnoreCase);
		}

		readonly AgencyPrincipal principal;

		#endregion
	}
}
