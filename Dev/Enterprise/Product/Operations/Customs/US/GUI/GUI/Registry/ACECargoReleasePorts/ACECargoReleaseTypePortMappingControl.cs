using System;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class ACECargoReleaseTypePortMappingControl : RegistryZUserControl
	{
		public ACECargoReleaseTypePortMappingControl()
		{
			InitializeComponent();

			AllPortsDropEdit.SelectedIndexChanged += new EventHandler(AllPortsDropEdit_SelectedIndexChanged);
		}

		void AllPortsDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshControlVisibility();
		}

		void RefreshControlVisibility()
		{
			MainGrid.ReadOnly = !string.IsNullOrEmpty(AllPortsDropEdit.Text);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MainGrid.ReadOnly = readOnly;
			AllPortsDropEdit.ReadOnly = readOnly;
		}
	}
}
