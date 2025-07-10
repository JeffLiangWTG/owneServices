using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	/// <remarks>DO NOT MAKE ANY NEW CHAGNES TO THIS FORM.  This form has been replaced by DocketsLabelControlForm.cs.</remarks>
	public partial class LabelOptionsForm : ZChildForm
	{
		#region Constructors

		public LabelOptionsForm(WhsDocketLabelControl bO)
			: base(bO)
		{
		}

		#endregion

		#region Events

		protected virtual void PrintButton_Click(object sender, System.EventArgs e)
		{
			if (BusinessObject.HasErrors())
			{
				Globals.Message.ShowError(Res.GetString("f5bbee28-455b-48ea-8ad2-e5929f773501", "All errors must be fixed before you proceed.\r\n\r\n{0}",
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
				return Res.GetString("60f8b85d-4bc0-4c64-9b63-abd116cd4812", "Label Printing");
			}
		}

		protected BusinessObject BusinessObject
		{
			get { return (BusinessObject)this.BusinessEntity; }
		}

		#endregion
	}
}
