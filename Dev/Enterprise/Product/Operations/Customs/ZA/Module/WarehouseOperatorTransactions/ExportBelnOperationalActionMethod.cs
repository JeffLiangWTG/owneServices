using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Module
{
	class ExportBelnOperationalActionMethod : OperationalActionMethod
	{
		public ExportBelnOperationalActionMethod() : base(new ZGuid("BD7EF96F-6779-4747-A8E3-7DDCB1FC24E9"))
		{
		}

		public override string Name => "Export BELN";

		public override string Description => "Export BELN";

		public override bool HasControl => true;

		public override bool IsRunAgainDisabled => true;

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ExportBelnApplicator(factory, AskForInsufficientStockConfirmation);
		}

		public override IComponent NewGuiControl()
		{
			return new ExportOperationalActionUserControl();
		}

		bool AskForInsufficientStockConfirmation()
		{
			return Globals.Message.ShowConfirmation("Insufficient stock found for some products. Details are displayed in the progress log. If you continue with the process there will be partial extractions for these products. Please enter yes to continue with the process.", Name, "yes", System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK;
		}
	}
}
