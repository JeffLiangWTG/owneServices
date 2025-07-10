using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ForwardingShipmentSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.MaintainShipment;

		public override string SingularElementNoun
		{
			get { return Res.GetString("Forwarding|ShipmentActionSupporter|PluralElementNoun", "Shipment"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("Forwarding|ShipmentActionSupporter|SingularElementNoun", "Shipments"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Shipment);
			list.Add(ActionMethodProviderIDs.Accounting);
			list.Add(ActionMethodProviderIDs.HVLV);
			list.Add(ActionMethodProviderIDs.Brokerage);
			list.Add(ActionMethodProviderIDs.SendAdvancedAirCargoReport);
			list.Add(ActionMethodProviderIDs.DtbBookingParent);
		}

		public override Type RootType
		{
			get { return typeof(ForwardingShipment); }
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.MaintainShipmentJobInvoicing; }
		}

		#endregion
	}
}
