using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class DocumentCusContainerForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public DocumentCusContainerForm(DocumentCusContainerCollectionHeader businessObject)
			: base(businessObject)
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		void PrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Yes;
		}

		void CancelPrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == PrintButton && previousControl == CancelPrintButton;
		}
	}
}
