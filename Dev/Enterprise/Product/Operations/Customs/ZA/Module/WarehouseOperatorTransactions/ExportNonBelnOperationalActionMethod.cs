using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Module
{
	class ExportNonBelnOperationalActionMethod : OperationalActionMethod
	{
		public ExportNonBelnOperationalActionMethod() : base(new ZGuid("BA7FA561-1B5F-41FC-920E-9FB20A2D3739"))
		{
		}

		public override string Name => "Export Non-BELN";

		public override string Description => "Export Non-BELN";

		public override bool HasControl => true;

		public override bool IsRunAgainDisabled => true;

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ExportNonBelnApplicator(factory, AskForInsufficientStockConfirmation);
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
