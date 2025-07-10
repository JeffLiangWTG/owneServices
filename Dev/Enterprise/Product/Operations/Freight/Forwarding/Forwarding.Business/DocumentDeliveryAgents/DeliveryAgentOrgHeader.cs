using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeliveryAgentOrgHeader : OrgHeader
	{
		public DeliveryAgentOrgHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override DocumentSupporter DocumentSupporter
		{
			get { return new DeliveryAgentOrgHeaderDocumentSupporter(this); }
		}

		protected override bool EnableLightValidationIfAvailable
		{
			// this is a dodgy fix and should be concidered only temporary until the inheritance can be fixed.
			get { return false; }
		}

		#endregion

		#region Implementation

		internal ConsolForwardingShipmentCollection Shipments
		{
			get { return shipments; }
		}

		internal ForwardingConsol Consol
		{
			get { return consol; }
		}

		public void LoadShipmentCollection(ForwardingConsol parent)
		{
			this.consol = parent;
			shipments = new ConsolForwardingShipmentCollection(parent);

			ZQuery filter = new ZQuery(JobShipmentSchema.JS_OH_DeliveryAgent, SQLComparisonOperator.Equal, PK);

			if (parent != null && PK == parent.ReceivingForwarderPK)
			{
				ZQuery deliveryAgentIsNull = new ZQuery(JobShipmentSchema.JS_OH_DeliveryAgent, null);
				filter.AddToFilter(deliveryAgentIsNull, JoinCondition.Or);
			}

			shipments.Load(filter);
		}

		ConsolForwardingShipmentCollection shipments;
		ForwardingConsol consol;

		#endregion
	}
}
