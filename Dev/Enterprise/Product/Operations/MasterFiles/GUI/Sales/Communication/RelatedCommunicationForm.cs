using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RelatedCommunicationForm : ZChildForm
	{
		public RelatedCommunicationForm(OrgSalesCallCollection collection)
			: base(collection)
		{
			InitializeComponent();

			this.communicationGrid.InnerGrid.ColorContextKey = RelatedCommunicationColorContextKey;

			if (!DesignModeFinder.IsDesigning)
			{
				communicationGridPanel.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
				communicationGrid.ShowNotesCheckBox.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
				CloseButtonUserControl.SaveButton.Visible = false;
				CloseButtonUserControl.SaveAndCloseButton.Visible = false;
				CloseButtonUserControl.CloseButton.Click += (s, e) => { Close(); };
			}
		}

		public const string RelatedCommunicationColorContextKey = "RelatedCommunication";

		public override string FormVerb
		{
			get { return string.Empty; }
		}
	}
}
