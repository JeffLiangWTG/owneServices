using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentNewDetailsForm : ZChildForm
	{
		public DocumentNewDetailsForm(DocumentShipment businessObject)
			: base(businessObject)
		{
			InitializeComponent();
			DocumentShipment.SetDefaultsFromDataContext();
		}

		#region Buttons

		void PrintButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.HasErrors())
			{
				Globals.Message.ShowError((Res.GetString("a8f95db6-6a5a-4677-be7e-08da4f7cfe71", "All errors must be fixed before you proceed.") + "\n\n") + ((BusinessObject)BusinessEntity).Notifications.GetErrors().ToUniqueMessageListString());
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

		#region Business Object

		protected DocumentShipment DocumentShipment
		{
			get { return (DocumentShipment)BusinessEntity; }
		}

		#endregion
	}
}
