using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class MiscOptionsUserControl : Customs.GUI.BaseMiscOptionsUserControl
	{
		protected internal ZArchitecture.GUI.ZDateEdit BOESightDateDateEdit;
		protected internal ZArchitecture.ZTextBox BOESightNumberTextBox;
		protected internal ZArchitecture.GUI.ZGroupBox BOESightGroupBox;
		MasterFiles.GUI.ZOrganisationControl agentControl;
		ZArchitecture.GUI.ZDropEdit vATClaimBackIndicatorDropEdit;

		public MiscOptionsUserControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(vATClaimBackIndicatorDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			//			zLabelRemovalTransportCode.Visible = JobDeclaration.IsImport;
			//			zDropEditRemovalTransportCode.Visible = JobDeclaration.IsImport;
			BOESightGroupBox.Visible = JobDeclaration.IsImport;
		}
	}
}

