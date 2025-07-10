using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V3.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.SG.V3.GUI
{
	public class V3BrokeragePlugin : ZMutexedPlugIn
	{
		public V3BrokeragePlugin(ForwardingShipment shipment)
			: base(shipment)
		{
			this.shipment = shipment;
		}
		readonly ForwardingShipment shipment;

		public override ZGlobalMutex Mutex
		{
			get { return mutex ?? (mutex = Enterprise.Customs.Common.DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK)); }
		}
		ZGlobalMutex mutex;

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new V3CustomsBrokerageUserControl();
		}

		public override string Name
		{
			get { return "Brokerage"; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Broker; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return new V3Brokerage(shipment, new BusinessObjectFactory());
		}
	}
}
