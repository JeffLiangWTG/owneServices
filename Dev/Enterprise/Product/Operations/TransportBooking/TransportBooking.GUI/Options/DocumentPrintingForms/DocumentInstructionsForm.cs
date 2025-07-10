using System;
using System.Windows.Forms;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class DocumentInstructionsForm : ZChildForm
	{
		public DocumentInstructionsForm(DocumentInstructions businessObject)
			: base(businessObject)
		{
			InitializeComponent();

			PrintButton.Click += PrintButton_Click;
			CancelPrintButton.Click += (s, e) => DialogResult = DialogResult.No;
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			if (instructionsGrid.DataSource is DocumentInstructions instructions
				&& instructions.InstructionToPrint == null)
			{
				Globals.Message.Show(Res.GetString("087caeef-fd69-11ef-afee-faab184aec1d", "Please select an instruction."));
			}
			else
			{
				DialogResult = DialogResult.Yes;
			}
		}
	}
}
