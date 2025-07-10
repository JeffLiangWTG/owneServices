using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class DocketsLabelOptionForm : ZChildForm
	{
		#region Constructors

		public DocketsLabelOptionForm(WhsDocketsLabelControl bO)
			: base(bO)
		{
		}

		#endregion

		#region Events

		protected virtual void PrintButton_Click(object sender, System.EventArgs e)
		{
			if (((WhsDocketsLabelControl)BusinessEntity).Lines.HasErrors())
			{
				Globals.Message.ShowError(Res.GetString("490b8e3d-1a73-49d7-b156-0422dfa972cb",
					"All errors must be fixed before you proceed.\r\n\r\n{0}",
					BusinessObject.Notifications.GetErrors().ToUniqueMessageListString()));
			}
			else
			{
				DialogResult = DialogResult.Yes;
			}
		}

		protected virtual void CancelPrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		#endregion

		#region Implementation

		public override string FormHeading
		{
			get
			{
				return Res.GetString("5f8a7b5c-3f52-4fb1-a158-0ae172d095d8", "Labels Printing");
			}
		}

		protected BusinessObject BusinessObject
		{
			get { return (BusinessObject)this.BusinessEntity; }
		}

		#endregion
	}
}
