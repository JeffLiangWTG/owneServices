using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class NumbersControl : ZUserControl
	{
		public NumbersControl()
		{
			InitializeComponent();
		}

		[DefaultValue(true)]
		public bool DisplayDetailPanel
		{
			get { return numberDetailsPanel.Visible; }
			set { numberDetailsPanel.Visible = value; }
		}
	}
}
