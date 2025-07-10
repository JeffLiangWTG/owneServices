using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class JobMawbForm : ZTemplateForm
	{
		public JobMawbForm(JobMawb businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			PlugIns.AddJobInvoicing(businessEntity.InvoicingSupporter);
			DisplayErrorsInMessagePanel = true;

#if DEBUG
			MainTabPage.BindingOrFirstShown +=
				delegate
				{
					TypeDescriptor.AddAttributes(Airline2LetterCodeLabel, new SuppressFormsLocalizedTestAttribute());
				};
#endif
			ParentJobNumberLabel.AllowOverlap(ReferenceTypeLabel);
			currentMAWB = businessEntity;
			currentMAWB.UncheckPrintedNeutralMAWB += new CancelEventHandler(JobMAWB_UncheckPrintedNeutralMAWB);
		}

		void JobMAWB_UncheckPrintedNeutralMAWB(object sender, CancelEventArgs e)
		{
			var result = Globals.Message.Show(
				Res.GetString("ec60d6b0-6cd8-4665-8f44-8228f9ca37aa", "Unchecking this box means this MAWB Number will be returned to the MAWB Stock. Do you want to continue?"),
				Res.GetString("c523807d-3db1-4567-99c0-059fc84669ac", "Neutral Printed MAWB"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			e.Cancel = result != DialogResult.Yes;
			this.Refresh();
		}

		protected JobMawb currentMAWB;

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (currentMAWB != null)
				{
					currentMAWB.UncheckPrintedNeutralMAWB -= new CancelEventHandler(JobMAWB_UncheckPrintedNeutralMAWB);
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
