using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class DeactivateCompanyMessageBox : ZMessageBox
	{
		public DeactivateCompanyMessageBox()
		: base(Content, Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
		{
			Button1.Text = Res.GetString("4B46AD64-2390-4756-871F-01584167271D", "Deactivate");
			Button1.BackColor = System.Drawing.Color.Red;
		}

		static string Caption => ResString.GetMultilingualString("E6724150-D0AC-458F-BADB-B73BE8D72E99", "Warning");

		static string Content => ResString.GetMultilingualString("B4C65C21-10A0-478E-A7DD-72C27CB242FD", @"Deactivating this Company will also Deactivate all associated Branches.

Would you like to continue?");
	}
}
