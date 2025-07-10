using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GUI
{
	/// <summary>
	/// Base PlugIn to Shipment when the relationship is one-to-one from dbo.JobDeclaration to JobShipment
	/// JE_JS is the link
	/// </summary>
	public class BrokeragePlugInOneToOne : BaseBrokeragePlugIn
	{
		public BrokeragePlugInOneToOne(ForwardingShipment shipment) : base(shipment)
		{
		}

		#region PlugIn Members

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			QueryAddDeclaration();
		}

		#endregion
	}
}
