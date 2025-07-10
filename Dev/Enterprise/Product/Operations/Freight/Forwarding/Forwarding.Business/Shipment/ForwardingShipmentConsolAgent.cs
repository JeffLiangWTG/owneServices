using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentWrapperWithConsolAgent : NonPersistentBusinessObject, IDocumentSupportable, IObsoleteValidation
	{
		public ForwardingShipmentWrapperWithConsolAgent(ForwardingShipment shipment, OrgHeader consolAgent, BusinessContext parentContext = BusinessContext.ConsolAgent)
			: base(shipment.Factory)
		{
			fShipment = shipment;
			fConsolAgentForARInvoice = consolAgent;
			BusinessContext = parentContext;
		}

		#region Related Objects

		public ForwardingShipment Shipment
		{
			get { return fShipment; }
		}
		readonly ForwardingShipment fShipment;

		public OrgHeader ConsolAgentForARInvoice
		{
			get { return fConsolAgentForARInvoice; }
		}
		readonly OrgHeader fConsolAgentForARInvoice;

		#endregion

		public override string TableName
		{
			get { return Shipment.TableName; }
		}

		public BusinessContext BusinessContext { get; set; }

		public DocumentSupporter DocumentSupporter
		{
			get { return new ForwardingShipmentWrapperWithConsolAgentDocumentSupporter(this); }
		}
	}
}
