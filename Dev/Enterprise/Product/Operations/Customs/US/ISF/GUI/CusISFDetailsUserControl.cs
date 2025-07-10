using System;
using System.Windows.Forms;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	public partial class CusISFDetailsUserControl : ZUserControl
	{
		public CusISFDetailsUserControl()
		{
			InitializeComponent();
			SCACCodeFindBox.GetCountryCode = () => Core.Constants.CountryCodes.UnitedStates;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddISFUserControl();
		}

		void AddISFUserControl()
		{
			zTabISFType.Controls.Clear();

			if (CurrentDataItem != null && ((CusISFHeader)CurrentDataItem).IsISF5Entry)
			{
				zTabISFType.Text = "ISF 5";
				zTabISFType.Controls.Add(ISF5UserControl);
			}
			else
			{
				zTabISFType.Text = "ISF 10";
				zTabISFType.Controls.Add(ISF10UserControl);
			}

			zTabISFType.ResumeLayout(false);
		}

		#region ISF5UserControl

		ISF5UserControl ISF5UserControl
		{
			get
			{
				if (isf5UserControl == null)
				{
					isf5UserControl = new ISF5UserControl();
					isf5UserControl.Dock = DockStyle.Fill;
				}

				return isf5UserControl;
			}
		}
		ISF5UserControl isf5UserControl;

		#endregion

		#region ISF10UserControl

		ISF10UserControl ISF10UserControl
		{
			get
			{
				if (isf10UserControl == null)
				{
					isf10UserControl = new ISF10UserControl();
					isf10UserControl.Dock = DockStyle.Fill;
				}

				return isf10UserControl;
			}
		}
		ISF10UserControl isf10UserControl;

		#endregion

	}
}
