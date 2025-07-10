using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class PrintMAWBBarcodeLabelsActionMethod : OperationalActionMethod
	{
		public PrintMAWBBarcodeLabelsActionMethod()
			: base(new ZGuid("9dcb1524-6dbb-459a-aa4a-958d7b6885f7"))
		{
		}

		public override string Name
		{
			get { return Res.GetString("1d7e6d0d-9d20-4e4d-8453-fcb96231c685", "Print MAWB Barcode Labels"); }
		}

		public override string Description
		{
			get { return Res.GetString("5876714b-cdde-4706-8908-ec552f25accc", "Allows for the printing of Master Air Waybill barcode labels"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new PrintMAWBBarcodeLabelsMethodApplicator((AWBPrintSettings)settings, factory);
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
			return new AWBPrintSettingsControl(false);
		}
	}
}
