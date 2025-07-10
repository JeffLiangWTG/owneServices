using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ContainerPenaltyExclusionPopup : KForm, ICaptionRenderingSupport
	{
		public ContainerPenaltyExclusionPopup()
		{
			InitializeComponent();
		}

		protected override void OnDeactivate(EventArgs e)
		{
			ParentWinForm.Activate();
			ParentWinForm.Focus();
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			ParentWinForm.Activate();
			ParentWinForm.Focus();
		}

		public void ShowPopup()
		{
			Show();
			Activate();
			Focus();
		}

		public Form ParentWinForm { get; set; }

		#region ICaptionRenderingSupport Members

		[Category(ZGUIConstants.DesignerCategory)]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool? CaptionRenderingEnabled
		{
			get { return captionRenderingEnabled; }
			set
			{
				if (captionRenderingEnabled != value)
				{
					captionRenderingEnabled = value;
					OnCaptionRenderingEnabledChanged(EventArgs.Empty);
				}
			}
		}
		bool? captionRenderingEnabled;

		public event EventHandler CaptionRenderingEnabledChanged;

		void OnCaptionRenderingEnabledChanged(EventArgs e)
		{
			if (CaptionRenderingEnabledChanged != null)
			{
				CaptionRenderingEnabledChanged(this, e);
			}
		}

		#endregion
	}
}
