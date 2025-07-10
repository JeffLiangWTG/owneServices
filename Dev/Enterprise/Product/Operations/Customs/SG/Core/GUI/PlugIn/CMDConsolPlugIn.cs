using System;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class CMDConsolPlugIn : CMDPlugIn
	{
		public CMDConsolPlugIn(ForwardingConsol consol)
			: base(consol)
		{
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		protected override void SetEnabled()
		{
			base.SetEnabled();
			Enabled = Enabled && (Consol.IsImport() || Consol.IsExport());
		}

		protected internal override ZString TransportMode
		{
			get { return Consol.JK_TransportMode; }
		}

		protected internal override void HookEvents()
		{
			Consol.JK_TransportModeInfo.ValueChanged += JK_TransportModeInfo_ValueChanged;
			Consol.JK_RL_NKLoadPortInfo.ValueChanged += JK_RL_NKLoadPortInfo_ValueChanged;
			Consol.JK_RL_NKDischargePortInfo.ValueChanged += JK_RL_NKDischargePortInfo_ValueChanged;
		}

		protected internal override void UnhookEvents()
		{
			Consol.JK_TransportModeInfo.ValueChanged -= JK_TransportModeInfo_ValueChanged;
			Consol.JK_RL_NKLoadPortInfo.ValueChanged -= JK_RL_NKLoadPortInfo_ValueChanged;
			Consol.JK_RL_NKDischargePortInfo.ValueChanged -= JK_RL_NKDischargePortInfo_ValueChanged;
		}

		protected override CMDWrapperBase GetCMDWrapperBizO()
		{
			return new CMDConsolWrapper(Consol);
		}

		void JK_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetEnabled();
		}

		void JK_RL_NKLoadPortInfo_ValueChanged(object sender, EventArgs e)
		{
			SetEnabled();
		}

		void JK_RL_NKDischargePortInfo_ValueChanged(object sender, EventArgs e)
		{
			SetEnabled();
		}

		ForwardingConsol Consol
		{
			get { return (ForwardingConsol)HostBusinessEntity; }
		}
	}
}
