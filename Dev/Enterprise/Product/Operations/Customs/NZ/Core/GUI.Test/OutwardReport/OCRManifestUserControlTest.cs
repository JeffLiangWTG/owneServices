using System.Windows.Forms;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(FakeForm))]
	public class OCRManifestUserControlTest : ZFormBasherTest
	{
		public void TestMessageStatusText()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestStatus = new OutwardReportManifestStatus(consol);
			using (var form = new FakeForm(manifestStatus))
			{
				form.Show();
				var messageStatusTextBox = form.FindSingle<ZTextBox>("E2_MessageStatusBoundTextBox");
				AssertEquals(OutwardReportStatusList.Descriptions.NotSent, messageStatusTextBox.Text);
				Assert(messageStatusTextBox.ReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestStatus = new OutwardReportManifestStatus(consol);
			return new FakeForm(manifestStatus);
		}

		class OCRManifestUserControlForTest : OCRManifestUserControl
		{
			public OCRManifestUserControlForTest(OutwardReportManifestStatus manifestStatus) : base(manifestStatus)
			{
				MissingResourceStringChecker.ExcludeFromTest(HistoryGroupBox);
				MissingResourceStringChecker.ExcludeFromTest(MessageTextGroupBox);
			}
		}

		class FakeForm : ZForm
		{
			public FakeForm(OutwardReportManifestStatus manifestStatus) : base(manifestStatus)
			{
				this.manifestStatus = manifestStatus;
			}

			readonly OutwardReportManifestStatus manifestStatus;
			OCRManifestUserControl oCRManifestUserControl;
			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.CaptionRenderingEnabled = true;
				this.oCRManifestUserControl = new OCRManifestUserControlForTest(manifestStatus);
				this.oCRManifestUserControl.Dock = DockStyle.Fill;
				this.Size = new System.Drawing.Size(1024, 680);
				this.Controls.Add(this.oCRManifestUserControl);
				this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
				this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus";
				this.Name = "OCRManifestUserControl";
			}
		}
	}
}
