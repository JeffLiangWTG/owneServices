using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackLineCollection : OuterPackLineCollection, Integration.Forwarding.IForwardingPackLineCollection
	{
		public ForwardingPackLineCollection(ForwardingShipment master)
			: base(master, master.Factory)
		{
		}

		#region Properties

		public override bool ReadOnly
		{
			get { return !SuspendReadOnly && base.ReadOnly; }
		}

		public bool SuspendReadOnly
		{
			get; set;
		}

		#endregion

		#region Implementation

		public new ForwardingPackLine AddNew()
		{
			return (ForwardingPackLine)base.AddNew();
		}

		public new ForwardingPackLine this[int index]
		{
			get { return (ForwardingPackLine)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ForwardingPackLine);
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			PackLineConfirmationConcurrencyCheck.Register(Shipment.Factory);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			TriggerSynchronise(bizO);
		}

		public override void Add(BusinessObject bizO)
		{
			if (!IsLoading)
			{
				using (((ForwardingShipment)Shipment)?.MonitorRequireTEUChange(new CO2eStatusChangedReason((NoResString)"Packline added")))
				{
					base.Add(bizO);
				}
			}
			else
			{
				base.Add(bizO);
			}
		}

		void TriggerSynchronise(BusinessObject bizO)
		{
			if (bizO.IsInDatabase || bizO.HasChanges)
			{
				ForwardingShipment shipment = Master as ForwardingShipment;
				if (shipment != null && shipment.PackLineSynchroniser != null)
				{
					shipment.PackLineSynchroniser.MarkSyncDirty();
				}
			}
		}

		#endregion
	}
}
