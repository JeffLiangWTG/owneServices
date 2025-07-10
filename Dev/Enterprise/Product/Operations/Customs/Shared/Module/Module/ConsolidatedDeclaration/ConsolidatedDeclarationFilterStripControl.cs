using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public sealed partial class ConsolidatedDeclarationFilterStripControl : ZFilterStripControl<MasterFiles.Module.WorkflowFilterStrip>
	{
		public ConsolidatedDeclarationFilterStripControl()
		{
			InitializeComponent();
		}

		public ConsolidatedDeclarationFilterStripControl(ConsolidatedDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
