using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	public abstract partial class ReleaseSequenceControl : ZUserControl
	{
		public ReleaseSequenceControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				var colorTheme = SystemDataRegistry.Instance.ColorTheme;
				ReleaseSequenceNameBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				ReleaseSequenceValueBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				ReleaseSequenceInvestmentBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				ReleaseSequencePositionBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				ReleaseSequenceDateBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
			}
		}

		protected abstract IBMReleaseSequence ReleaseSequence { get; }

		protected abstract Type BindingDataSourceType { get; }

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			OpenSequenceButton.Enabled = ReleaseSequence != null;
		}

		void OpenSequenceButton_Click(object sender, EventArgs e)
		{
			if (ReleaseSequence != null)
			{
				ZControllerFactory.Create(ControllerIDs.BMReleaseSequence).ShowEditForm(ReleaseSequence as BusinessObject);
			}
		}
	}
}
