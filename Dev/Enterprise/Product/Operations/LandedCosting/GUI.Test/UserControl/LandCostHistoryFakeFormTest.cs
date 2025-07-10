using System.Drawing;
using System.Windows.Forms;
using Enterprise.LandedCosting.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.LandedCosting.GUI.Testing
{
	[TestedType(typeof(LandCostHistoryFakeForm))]
	sealed class LandCostHistoryFakeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var lCHeader = Factory.New<LandedCostHeader>();
			var testCollection = new GenericLandedCostHistoryCollection(lCHeader);
			return new LandCostHistoryFakeForm(testCollection);
		}

		[CodeAlive("used in LandCostHistoryFakeFormTest")]
		sealed class LandCostHistoryFakeForm : ZForm
		{
			public LandCostHistoryFakeForm(GenericLandedCostHistoryCollection testCollection) : base(testCollection)
			{
				CaptionRenderingEnabled = true;
			}

			LandCostHistoryUserControl landCostHistoryUserControl;

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				landCostHistoryUserControl = new LandCostHistoryUserControl();
				landCostHistoryUserControl.Dock = DockStyle.Fill;
				Size = new Size(1024, 680);
				Controls.Add(landCostHistoryUserControl);
				DataSourceAssemblyName = "Enterprise.LandedCosting.Business";
				DataSourceTypeName = "Enterprise.LandedCosting.Business.GenericLandedCostHistoryCollection";
				Name = "LandCostHistoryUserControl";
			}
		}
	}
}
