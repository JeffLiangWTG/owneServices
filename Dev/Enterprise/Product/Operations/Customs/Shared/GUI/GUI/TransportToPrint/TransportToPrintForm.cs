using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TransportToPrintForm : ZChildForm
	{
		public TransportToPrintForm(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
		}

		protected BaseJobDeclaration Declaration
		{
			get { return (BaseJobDeclaration)BusinessEntity; }
		}

		#region Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

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

		#region Implementation

		void PrintSelectedButton_Click(object sender, System.EventArgs e)
		{
			if (TransportsGrid.SelectedRowCount == 0)
			{
				Globals.Message.Show(Res.GetString("f96886f7-0aa2-48ff-973f-5abf2fcdbaf6", "No Transport selected"));
				return;
			}
			else
			{
				Declaration.TransportToPrint = (Transport)TransportsGrid.SelectedElements[0];
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		#endregion
	}
}
