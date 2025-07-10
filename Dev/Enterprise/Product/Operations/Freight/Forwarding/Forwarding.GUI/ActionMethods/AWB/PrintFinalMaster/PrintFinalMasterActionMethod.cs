using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class PrintFinalMasterActionMethod : OperationalActionMethod
	{
		public PrintFinalMasterActionMethod()
			: base(new ZGuid("a0330ef4-2343-4d18-91a9-3a27207d9822"))
		{
		}

		public override string Name
		{
			get { return Res.GetString("a0330ef4-2343-4d18-91a9-3a27207d9822", "Print Final Master"); }
		}

		public override string Description
		{
			get { return Res.GetString("aecca7e3-3dc2-4e9c-8fc6-164e5c0f053a", "Allows for the printing and/or electronic sending of Air Waybills"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new PrintFinalMasterMethodApplicator((AWBPrintSettings)settings, factory);
		}

		public override bool HasSettings
		{
			get { return true; }
		}

		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new AWBPrintSettings();
		}

		public override IComponent NewSettingsControl()
		{
			return new AWBPrintSettingsControl(true);
		}
	}
}
