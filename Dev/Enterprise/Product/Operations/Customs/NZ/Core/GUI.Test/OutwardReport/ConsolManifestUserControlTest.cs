using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(FakeForm))]
	public class ConsolManifestUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestStatus = new OutwardReportManifestStatus(consol);
			return new FakeForm(manifestStatus);
		}

		class ConsolManifestUserControlForTest : ConsolManifestUserControl
		{
			public ConsolManifestUserControlForTest(OutwardReportManifestStatus manifestStatus)
			: base(manifestStatus)
			{
				MissingResourceStringChecker.ExcludeFromTest(MessageTextGroupBox);
				MissingResourceStringChecker.ExcludeFromTest(HistoryGroupBox);
				MissingResourceStringChecker.ExcludeFromTest(StatusLabel);
				MissingResourceStringChecker.ExcludeFromTest(messagingModeDropEdit);
			}
		}

		class FakeForm : ZForm
		{
			public FakeForm(OutwardReportManifestStatus manifestStatus) : base(manifestStatus)
			{
				this.manifestStatus = manifestStatus;
			}

			readonly OutwardReportManifestStatus manifestStatus;
			ConsolManifestUserControl consolManifestUserControl;
			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.CaptionRenderingEnabled = true;
				this.consolManifestUserControl = new ConsolManifestUserControlForTest(manifestStatus);
				this.consolManifestUserControl.Dock = DockStyle.Fill;
				this.Size = new System.Drawing.Size(1024, 680);
				this.Controls.Add(this.consolManifestUserControl);
				this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
				this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus";
				this.Name = "ConsolManifestUserControl";
			}
		}
	}
}
