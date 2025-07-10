using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Business.Testing
{
	public class DummyTransportParent : DummyBusinessObject, ITransportParent
	{
		public DummyTransportParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public ZString TypeCode
		{
			get { return "DMY"; }
		}

		public TransportSupporter TransportSupporter
		{
			get { return new DummyTransportSupporter(this); }
		}

		public TransportCollection Transports
		{
			get { return transports ?? (transports = new TransportCollection(this)); }
		}
		TransportCollection transports;

		class DummyTransportSupporter : TransportSupporter<DummyTransportParent>
		{
			public DummyTransportSupporter(DummyTransportParent parent)
				: base(parent)
			{ }

			public override ZString Description
			{
				get { return ""; }
			}

			public override ZString ConsignmentRef
			{
				get { return ""; }
			}

			public override ZString TransportMode
			{
				get { return ""; }
			}

			public override ZString ContainerMode
			{
				get { return ""; }
			}

			public override ZString BillOfLading
			{
				get { return ""; }
			}

			public override ZGuid ShippingLine
			{
				get { return ZGuid.Empty; }
				set { }
			}

			public override bool SupportETD => false;

			public override bool SupportVoyageFlight => false;

			protected override void NotifyVesselChangedCore(Transport transport, ZString previousValue)
			{
				base.NotifyVesselChangedCore(transport, previousValue);
				Parent.NotifyVesselCalled++;
			}

			public override SecurityCheckpoint DistanceCalculationCheckpoint
			{
				get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
			}
		}

		public int NotifyVesselCalled;

		public Directions JobDirection
		{
			get { return Directions.Export; }
		}

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
		}
	}
}
