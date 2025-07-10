using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyageOriginDependentCollection : VoyagePortsCollection<VoyageOrigin>
	{
		public VoyageOriginDependentCollection(JobVoyage parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		protected override SchemaGuidColumn SailingFKColumn
		{
			get { return JobSailingSchema.JX_JA; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			UnhookVoyagePortToParentVoyage(bizOAdded as VoyageOrigin);
			HookVoyagePortToParentVoyage(bizOAdded as VoyageOrigin);
			Voyage.AddInvalidDateChangeLogIfNeeded(bizOAdded);
#pragma warning restore CS0618 // Type or member is obsolete

			base.OnAdded(bizOAdded);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			UnhookVoyagePortToParentVoyage(bizO as VoyageOrigin);
			Voyage.RemoveInvalidDateChangeLogIfNeeded(bizO);
#pragma warning restore CS0618 // Type or member is obsolete

			base.OnRemoved(bizO);
		}

		public VoyageOrigin AddNew(string inPort)
		{
			var instance = AddNew();
			instance.JA_RL_NKPortOfLoading = inPort;
#pragma warning disable CS0618 // Type or member is obsolete
			HookVoyagePortToParentVoyage(instance);
#pragma warning restore CS0618 // Type or member is obsolete

			return instance;
		}

		public VoyageOrigin GetOriginFromLoading(ZString loadPort)
		{
			return this.Cast<VoyageOrigin>().FirstOrDefault(origin => origin.JA_RL_NKPortOfLoading == loadPort);
		}

		#region Value Changes Event Handler

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes", false)]
		void HookVoyagePortToParentVoyage(VoyageOrigin origin)
		{
			origin.JA_E_DEPInfo.ValueChanged += Voyage.OriginETDOrDestinationETAChanged_ValueChanged;
		}

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes", false)]
		void UnhookVoyagePortToParentVoyage(VoyageOrigin origin)
		{
			origin.JA_E_DEPInfo.ValueChanged -= Voyage.OriginETDOrDestinationETAChanged_ValueChanged;
		}

		#endregion
	}
}
