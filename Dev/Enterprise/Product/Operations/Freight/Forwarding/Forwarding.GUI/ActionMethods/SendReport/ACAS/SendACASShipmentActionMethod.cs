using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class SendACASShipmentActionMethod : OperationalActionMethod
	{
		public SendACASShipmentActionMethod()
			: base(new ZGuid("8648C209-BA08-4FBF-8865-5CCB67A97862"))
		{
		}

		public override string Name
		{
			get { return Res.GetString("477CA2CD-668F-4184-878C-A26BFDF91CE7", "Send ACAS Shipment Report (US)"); }
		}

		public override string Description
		{
			get { return Res.GetString("0ACE8405-CE26-4E9E-BAE5-BAD6E148CA42", "for US air cargo imports reporting"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new SendACASShipmentMethodApplicator((DocDataObjectSendingMessageSettings)settings, factory);
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
