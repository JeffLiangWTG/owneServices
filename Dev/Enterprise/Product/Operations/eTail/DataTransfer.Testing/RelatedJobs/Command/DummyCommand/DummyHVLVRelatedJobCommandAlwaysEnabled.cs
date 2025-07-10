using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup(Testing)")]
	public class DummyHVLVRelatedJobCommandAlwaysEnabled : BaseHVLVRelatedJobCommand
	{
		public DummyHVLVRelatedJobCommandAlwaysEnabled(ForwardingShipment shipment)
			: base(shipment)
		{
			if (shipment is ForwardingShipmentForCommandTesting shipmentForTesting)
			{
				ShipmentForTesting = shipmentForTesting;

				var customJob = (ShipmentForTesting?.HasCustomsRelatedJob == true) ? ShipmentForTesting.Factory.New<DummyBusinessObject>() : null;
				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				header.GenPivotCollection.AddRelatedIfNotExist(customJob);
			}
		}

		readonly ForwardingShipmentForCommandTesting ShipmentForTesting;

		public override MultilingualString RelatedJobName => (NoResString)"Always Enabled Job";

		public override CustomsRelatedBusinessObjectConverter Converter => throw new NotImplementedException();

		public override string UsageCode => "DME";

		protected override Type RelatedCustomsJobType => typeof(DummyBaseBusinessObject);
		protected override bool AllowOpen => ShipmentForTesting?.AllowOpen ?? false;
		protected override bool AllowSync => ShipmentForTesting?.AllowSync ?? false;
	}

	public class ForwardingShipmentForCommandTesting : ForwardingShipment
	{
		public ForwardingShipmentForCommandTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool HasCustomsRelatedJob { get; set; }

		public bool AllowOpen { get; set; }

		public bool AllowSync { get; set; }
	}
}
