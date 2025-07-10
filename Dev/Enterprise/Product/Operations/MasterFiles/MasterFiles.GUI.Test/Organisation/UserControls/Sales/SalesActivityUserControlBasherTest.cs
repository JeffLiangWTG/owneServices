using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesActivityUserControlFormForBash))]
	sealed class SalesActivityUserControlBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			return new SalesActivityUserControlFormForBash(org);
		}

		internal class SalesActivityUserControlFormForBash : ZForm
		{
			internal SalesActivityUserControlFormForBash(OrgHeader org)
				: base(org)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				SalesActivityUserControl.Dock = DockStyle.Fill;
				Controls.Add(SalesActivityUserControl);
				BindingSource.SetBindingMember(SalesActivityUserControl, ".");
				CaptionRenderingEnabled = true;
			}

			readonly SalesActivityUserControl SalesActivityUserControl = new SalesActivityUserControl();
		}

		#endregion
	}
}
