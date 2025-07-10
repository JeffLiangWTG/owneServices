using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class WriteOffResponseConsol : WriteOffResponse
	{
		internal WriteOffResponseConsol(BaseTSWResponse response)
			: base(response)
		{
		}

		public ForwardingConsol Consol
		{
			get { return (ForwardingConsol)LinkedObject; }
		}

		#region Overrides

		protected override IEnumerable<ZString> ConsignmentIDs => Consol.Shipments.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill);

		// TO DO: Store this somewhere
		protected override ZString CustomsDeliveryInstructionsCore
		{
			get { return ZString.Empty; }
			set { }
		}

		protected override ZString MasterBillCore
		{
			get { return Consol.JK_MasterBillNum; }
		}

		protected override Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence)
		{
			return new ConsignmentShipment(this, id, status);
		}

		protected override string GetJobID()
		{
			return Consol.JK_UniqueConsignRef;
		}

		protected override bool GetIsImportEntry() => Consol.JobDirection == Enterprise.MasterFiles.Business.Directions.Import;

		protected override string GetJobName()
		{
			return "ICR Consol";
		}

		protected override void SetCustomsStatus(ZString newValue)
		{
			ManifestStatus.E2_MessageStatus = newValue;
		}

		protected override void SetECINumber(ZString newValue)
		{
			ManifestStatus.SetCustomsEntryNumber(newValue);
		}

		#endregion // Overrides

		#region Implementation

		ICRManifestStatus ManifestStatus
		{
			get { return manifestStatus ?? (manifestStatus = new ICRManifestStatus(Consol)); }
		}
		ICRManifestStatus manifestStatus;

		#endregion // Implementation
	}
}

// Tested in ICRMessageProcessorConsolTest
