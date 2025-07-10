using System;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.PortHubs.GUI
{
	public partial class PortHubStripControl : StripControl
	{
		public PortHubStripControl(PortHubSelectionCollectionWrapper collectionWrapper)
			: base(new PortHubFilterStripBusinessObject())
		{
			this.collectionWrapper = collectionWrapper;
			InitializeComponent();
			FilterStripsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			HideControlsWeDontNeed();
			ToolStripFindDropButton.ButtonClick += FindButtonClick;

			CoveringLabel.AllowOverlap(ToolStrip);
			CoveringLabel.AllowOverlap(FilterStripsPanel);
			ToolStrip.AllowOverlap(FilterStripsPanel);
			ToolStripColourPicker.AllowOverlap(FilterStripsPanel);
			AddStripButton.AllowOverlap(FilterStripsPanel);
		}

		readonly PortHubSelectionCollectionWrapper collectionWrapper;

		void HideControlsWeDontNeed()
		{
			ToolStripSaveLayoutButton.Visible = false;
			ToolStripManageDropButton.Visible = false;
			ToolStripHelp.Visible = false;
		}

		void FindButtonClick(object sender, EventArgs e)
		{
			if (collectionWrapper.HasChanges)
			{
				Globals.Message.Show(Res.GetString("f8d70b93-3938-4adb-8307-19423020cdea", "There are changes on this form. Please save and try again."));
			}
			else
			{
				collectionWrapper.Collection.Load(FilterBusinessObject.Filter);
			}
		}
	}
}
