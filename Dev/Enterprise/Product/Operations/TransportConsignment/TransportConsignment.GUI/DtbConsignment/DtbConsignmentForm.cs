using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbConsignmentForm : ZTemplateForm
	{
		public DtbConsignmentForm(DtbConsignment consignment) : base(consignment)
		{
			InitializeComponent();
			AddPlugIns();
			ControllerID = ControllerIDs.DtbConsignment;
			WorkflowTabPage.Initialize(consignment);
		}

		void AddPlugIns()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.JobInvoicing);
			PlugIns.Add(ControllerIDs.DocumentVisualizer);
		}

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenInBrowser(Consignment.PK);
		}

		static void OpenInBrowser(ZGuid consignmentPk)
		{
			var urlResult = GlowHelper.GenerateGotoGlowUrlForExistingEntityAsync(GlowHelper.EntityName.Consignment, consignmentPk).Result;

			if (urlResult.ErrorMessage != null)
			{
				var errorMessage = ResString.GetMultilingualString("75ba2d9a-a1c6-4f68-94b2-7445ffabbe73", "This Consignment cannot be opened in a browser.");
				Globals.Message.ShowError(errorMessage + System.Environment.NewLine + urlResult.ErrorMessage);
				return;
			}

			WebUrlLauncher.Launch(urlResult.Uri.ToString());
		}

		public override string FormCaption => BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption;

		DtbConsignment Consignment => (DtbConsignment)DataSource;
	}
}
