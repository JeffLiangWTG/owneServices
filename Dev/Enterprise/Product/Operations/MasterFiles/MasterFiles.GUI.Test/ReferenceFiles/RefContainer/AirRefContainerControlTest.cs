using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.UserControls;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(RefContainer))]
	public class AirRefContainerUserControlTest : BasherTest
	{
		public void TestDimensionsTabs()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var dimensionsTabControl = form.Controls.Find("DimensionsTabControl", true)[0];
				AssertNotNull("Dimensions tab control should not be null", dimensionsTabControl);
				var baseDimensionsTabPage = dimensionsTabControl.Controls.Find("BaseDimensionsTabPage", false)[0];
				var insideDimensionsTabPage = dimensionsTabControl.Controls.Find("InsideDimensionsTabPage", false)[0];
				AssertNotNull("Base Dimensions tab page should not be null", baseDimensionsTabPage);
				AssertNotNull("Inside Dimensions tab page should not be null", insideDimensionsTabPage);
			}
		}

		public override Form GetFormToBash() => CreateFormForTest();

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AirRefContainerControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(container, ".");
			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = RefContainer.New(Factory);
			container.RC_ShippingMode = "AIR";
			Factory.Save();
		}

		RefContainer container;

		#endregion
	}
}
