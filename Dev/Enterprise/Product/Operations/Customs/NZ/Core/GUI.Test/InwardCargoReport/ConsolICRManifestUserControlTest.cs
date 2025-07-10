using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(FakeForm))]
	public class ConsolICRManifestUserControlTest : ZFormBasherTest
	{
		public void TestMessageStatusText()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestStatus = new ICRManifestStatus(consol);
			using (var form = new FakeForm(manifestStatus))
			{
				form.Show();
				var messageStatusTextBox = form.FindSingle<ZTextBox>("E2_MessageStatusBoundTextBox");
				AssertEquals(LowValueManifestStatusList.Descriptions.NotSentToCustoms, messageStatusTextBox.Text);
				Assert(messageStatusTextBox.ReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ICRManifestStatus manifestStatus = new ICRManifestStatus(consol);
			return new FakeForm(manifestStatus);
		}

		class FakeForm : ZForm
		{
			public FakeForm(ICRManifestStatus manifestStatus) : base(manifestStatus)
			{
				this.manifestStatus = manifestStatus;
			}

			readonly ICRManifestStatus manifestStatus;
			ConsolICRManifestUserControl consolManifestUserControl;
			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.CaptionRenderingEnabled = true;
				this.consolManifestUserControl = new ConsolICRManifestUserControl(manifestStatus);
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
