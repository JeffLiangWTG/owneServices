using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class AllocateWeightForm : ZChildForm
	{
		readonly IEnumerable<BaseJobComInvoiceLine> invoiceLines;

		public AllocateWeightForm(AllocateWeight allocateWeight)
			: base(allocateWeight)
		{
			invoiceLines = allocateWeight.InvoiceLines;
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (!checkShouldPopUpMessage())
			{
				DialogResult = DialogResult.OK;
			}
		}

		bool checkShouldPopUpMessage()
		{
			var result = false;
			var allocateWeight = (AllocateWeight)BusinessEntity;
			var message = new ZStringBuilder();
			if (allocateWeight.NetWeight == 0 && allocateWeight.GrossWeight == 0)
			{
				message.AppendLine(Res.GetString("2FF3BA96-0C2E-4BA9-A3C2-5FB7568E9AF6", "No action is taken."));
			}
			else
			{
				if (allocateWeight.NetWeight > 0 && !allocateWeight.OverrideExisting && invoiceLines.All(c => c.JI_NetWeight > 0))
				{
					message.AppendLine(Res.GetString("2E33194A-2F5E-4FB3-B102-C1AA4A4F9A17", "Net weight cannot be allocated because all invoice lines have net weight."));
				}
				if (allocateWeight.GrossWeight > 0 && !allocateWeight.OverrideExisting && invoiceLines.All(c => c.JI_Weight > 0))
				{
					message.AppendLine(Res.GetString("D0BF6C34-0694-4BF2-8BD0-456EA79A8482", "Gross weight cannot be allocated because all invoice lines have gross weight."));
				}
				if (AllocateWeightHelper.UnableAllocated(allocateWeight, invoiceLines))
				{
					if (allocateWeight.IsPrice)
					{
						message.AppendLine(Res.GetString("63481308-1E90-4389-B2D3-247351110C49", "Weight cannot be allocated by Price because all invoice prices of the selected lines are 0."));
					}
					else if (allocateWeight.IsQuantity)
					{
						message.AppendLine(Res.GetString("DD3F9650-014B-4076-A543-6E9C4B0785A6", "Weight cannot be allocated by Quantity because all invoice quantities of the selected lines are 0."));
					}
				}
			}
			if (!message.IsEmpty)
			{
				result = true;
				Globals.Message.ShowInformation(message.ToString());
			}
			return result;
		}
	}
}
