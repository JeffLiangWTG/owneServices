using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Customs.SG.V4.GUI.CMDMessaging;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class CMDShipmentPlugIn : CMDPlugIn
	{
		public CMDShipmentPlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override CMDWrapperBase GetCMDWrapperBizO()
		{
			return new CMDShipmentWrapper(Shipment);
		}

		protected override Control GetNewUserControl()
		{
			return new CMDShipmentUserControl();
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected internal override ZString TransportMode
		{
			get { return Shipment.JS_TransportMode; }
		}

		protected internal override void HookEvents()
		{
			Shipment.JS_TransportModeInfo.ValueChanged += JS_TransportModeInfo_ValueChanged;
		}

		protected internal override void UnhookEvents()
		{
			Shipment.JS_TransportModeInfo.ValueChanged -= JS_TransportModeInfo_ValueChanged;
		}

		ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)HostBusinessEntity; }
		}

		void JS_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetEnabled();
		}
	}
}
