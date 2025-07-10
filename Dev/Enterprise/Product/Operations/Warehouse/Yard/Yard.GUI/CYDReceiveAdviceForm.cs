
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class CYDReceiveAdviceForm : ZTemplateForm
	{
		public CYDReceiveAdviceForm(CYDReceiveAdvice receiveAdvice) : base(receiveAdvice)
		{
			InitializeComponent();

			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)ReceiveAdvice).InvoicingSupporter);
			zWorkflowTabPage.Initialize(ReceiveAdvice);
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region GLOW link open in browser

		void GlowLinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/CYDReceiveAdvice", ReceiveAdvice.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", ReceiveAdvice.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		#endregion

		#region ReceiveAdvice

		CYDReceiveAdvice ReceiveAdvice
		{
			get { return (CYDReceiveAdvice)DataSource; }
		}

		#endregion
	}
}
