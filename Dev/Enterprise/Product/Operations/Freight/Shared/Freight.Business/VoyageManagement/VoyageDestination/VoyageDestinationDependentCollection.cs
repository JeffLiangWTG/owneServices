using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyageDestinationDependentCollection : VoyagePortsCollection<VoyageDestination>
	{
		public VoyageDestinationDependentCollection(JobVoyage parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		protected override SchemaGuidColumn SailingFKColumn
		{
			get { return JobSailingSchema.JX_JB; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			UnhookVoyagePortToParentVoyage(bizOAdded as VoyageDestination);
			HookVoyagePortToParentVoyage(bizOAdded as VoyageDestination);
			Voyage.AddInvalidDateChangeLogIfNeeded(bizOAdded);
#pragma warning restore CS0618 // Type or member is obsolete

			base.OnAdded(bizOAdded);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			UnhookVoyagePortToParentVoyage(bizO as VoyageDestination);
			Voyage.RemoveInvalidDateChangeLogIfNeeded(bizO);
#pragma warning restore CS0618 // Type or member is obsolete

			base.OnRemoved(bizO);
		}

		public VoyageDestination AddNew(string inPort)
		{
			var instance = AddNew();
			instance.JB_RL_NKPortOfDischarge = inPort;
#pragma warning disable CS0618 // Type or member is obsolete
			HookVoyagePortToParentVoyage(instance);
#pragma warning restore CS0618 // Type or member is obsolete

			return instance;
		}

		public VoyageDestination GetDestinationFromDischarge(ZString dischargePort)
		{
			return this.Cast<VoyageDestination>().FirstOrDefault(destination => destination.JB_RL_NKPortOfDischarge == dischargePort);
		}

		#region Value Changes Event Handler

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes", false)]
		void HookVoyagePortToParentVoyage(VoyageDestination destination)
		{
			destination.JB_E_ARVInfo.ValueChanged += Voyage.OriginETDOrDestinationETAChanged_ValueChanged;
		}

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes", false)]
		void UnhookVoyagePortToParentVoyage(VoyageDestination destination)
		{
			destination.JB_E_ARVInfo.ValueChanged -= Voyage.OriginETDOrDestinationETAChanged_ValueChanged;
		}

		#endregion
	}
}
