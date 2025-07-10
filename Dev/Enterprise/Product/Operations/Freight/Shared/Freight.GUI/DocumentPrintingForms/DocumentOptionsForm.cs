using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.GUI
{
	[TestExcludeZWinFormHasTypedConstructor]
	public partial class DocumentOptionsForm : ZChildForm
	{
		DocumentOptionsForm()
		{
			InitializeComponent();
		}

		public DocumentOptionsForm(BusinessObject businessObject, Constants.DataContext dataContext)
			: base(businessObject)
		{
			LayoutControlsFromDataContext(dataContext);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Controls

		protected virtual Control[] GetControls(Constants.DataContext dataContext)
		{
			throw new NotImplementedException();
		}

		void LayoutControlsFromDataContext(Constants.DataContext dataContext)
		{
			Control current = null;
			Control last = null;
			int top = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);

			Control[] arrangedControls = GetControls(dataContext);
			for (int i = 0; i < arrangedControls.Length; i++)
			{
				if (arrangedControls != null)
				{
					current = arrangedControls[i];
					ControlDpiScalingHelper.SetTop(ref current, (last == null) ? top : last.Top + last.Height, false);
					current.Visible = true;
					current.TabIndex = i;
					last = current;
				}
			}

			ControlDpiScalingHelper.SetTop(ref OptionsGroupBox, top / 2, false);
			ControlDpiScalingHelper.SetHeight(ref OptionsGroupBox, top + (last == null ? 0 : last.Top + last.Height), false);
			ControlDpiScalingHelper.SetTop(ref PrintButton, OptionsGroupBox.Top + OptionsGroupBox.Height + top, false);
			ControlDpiScalingHelper.SetTop(ref CancelPrintButton, PrintButton.Top, false);
			ControlDpiScalingHelper.SetHeight(this, PrintButton.Top + PrintButton.Height + (2 * MainStatusBar.Height) + top, false);
		}

		#endregion

		#region Buttons

		void PrintButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.HasErrors())
			{
				Globals.Message.ShowError(Res.GetString("909a7534-1b99-4a8d-bba6-61b9407bce77", "All errors must be fixed before you proceed.") + System.Environment.NewLine + System.Environment.NewLine + ((BusinessObject)BusinessEntity).Notifications.GetErrors().ToUniqueMessageListString());
			}
			else
			{
				DialogResult = DialogResult.Yes;
			}
		}

		void CancelPrintButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		#endregion
	}
}
