using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class SendCCTShipmentActionMethod : OperationalActionMethod
	{
		public SendCCTShipmentActionMethod()
			: base(new ZGuid("E4C9ACE1-042D-4804-83FD-2C01CDABC5A9"))
		{
		}

		public override string Name
		{
			get { return Res.GetString("3C6C0DD0-4001-4E76-BC53-7B612B2228B8", "Send CCT Shipment Report (BR)"); }
		}

		public override string Description
		{
			get { return Res.GetString("2D20201B-7827-4D92-8AA6-48AE0ECD134A", "for Brazil air cargo imports reporting"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new SendCCTShipmentMethodApplicator((DocDataObjectSendingMessageSettings)settings, factory);
		}

		public override bool HasSettings
		{
			get { return true; }
		}

		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new DocDataObjectSendingMessageSettings();
		}

		public override IComponent NewSettingsControl()
		{
			return new DocDataObjectSendingMessageSettingsControl(true);
		}
	}
}
