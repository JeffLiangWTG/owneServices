using System;
using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVBookingHeaderCreateTestLoadListForm : ZChildForm
	{
		public HVLVBookingHeaderCreateTestLoadListForm(HVLVBookingHeader bookingHeader)
		{
			InitializeComponent();
			BookingHeader = bookingHeader;
			textBoxBookingHeaderReference.Text = BookingHeader.HVH_BookingReference;
		}

		readonly HVLVBookingHeader BookingHeader;

		public HVLVOriginLoadList CreatedLoadList { get; private set; }

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (ModifierKeys == Keys.None && keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessDialogKey(keyData);
		}

		void BtnOK_Click(object sender, EventArgs e)
		{
			try
			{
				CreatedLoadList = HVLVBookingHeaderTestLoadlistCreator.CreateLoadList(BookingHeader, checkBoxCreateHVLVOuterPackage.Checked);
				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}
	}
}
