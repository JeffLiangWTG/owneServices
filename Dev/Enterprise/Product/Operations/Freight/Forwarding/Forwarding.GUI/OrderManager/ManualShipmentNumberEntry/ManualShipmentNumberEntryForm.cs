using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class ManualShipmentNumberEntryForm : ZChildForm
	{
		public ManualShipmentNumberEntryForm(ShipmentNumberEntries businessObject)
			: base(businessObject)
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
		}

		public override string FormVerb
		{
			get
			{
				return ZString.Empty;
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(BusinessEntity, (NoResString)"consol", Res.GetString("c1841b87-424c-46bb-bc3b-82b3cffe95ad", "create"), Res.GetString("80490ca6-109a-4e4e-ab39-2b6e178b44d2", "created"), includeIgnoreOption); // Hard-coded constant
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
				try
				{
					BusinessEntity.Factory.Save();
					Close();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
