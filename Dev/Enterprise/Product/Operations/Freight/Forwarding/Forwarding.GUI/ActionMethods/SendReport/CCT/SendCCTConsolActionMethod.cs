using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class SendCCTConsolActionMethod : OperationalActionMethod
	{
		public SendCCTConsolActionMethod()
			: base(new ZGuid("CDB2B65E-6CFF-4403-8211-CDDE033BD154"))
		{
		}

		public override string Name
		{
			get { return Res.GetString("0514A42B-E2CE-4D80-8CC3-65A34AFCB71B", "Send CCT House Manifest (BR)"); }
		}

		public override string Description
		{
			get { return Res.GetString("D06A4EBC-6BA9-4366-B3B8-0D2CEF9994DF", "for Brazil air cargo imports"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new SendCCTConsolMethodApplicator((DocDataObjectSendingMessageSettings)settings, factory);
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
