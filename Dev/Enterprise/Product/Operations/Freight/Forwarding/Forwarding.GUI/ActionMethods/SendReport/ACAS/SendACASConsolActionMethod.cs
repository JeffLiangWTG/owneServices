using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class SendACASConsolActionMethod : OperationalActionMethod
	{
		public SendACASConsolActionMethod()
			: base(new ZGuid("818E0416-DFD6-467D-8E0B-CF19D554A2CB"))
		{
		}

		public override string Name
		{
			get { return Res.GetString("6B3F404A-950A-430B-A45E-41C90F6A9165", "Send ACAS House Checklist (US)"); }
		}

		public override string Description
		{
			get { return Res.GetString("353273F5-5474-4068-8481-98B1FBEAFC24", "for US air cargo imports"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new SendACASConsolMethodApplicator((DocDataObjectSendingMessageSettings)settings, factory);
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
