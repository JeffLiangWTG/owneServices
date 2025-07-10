using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.UserControls;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefContainerForm))]
	sealed class RefContainerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefContainerForm(RefContainer.New(Factory));
		}

		public void TestCodeMapsGrid()
		{
			RefContainerForm form;
			using (form = new RefContainerForm(RefContainer.New(Factory)))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0];
				var mainTabPage = mainTabControl.Controls.Find("MainTabPage", true)[0];
				var codeMapsGroupBox = mainTabPage.Controls.Find("CodeMapsGroupBox", true)[0];
				var codeMapsGrid = codeMapsGroupBox.Controls.Find("CodeMapsGrid", true)[0];
				var isoTypeGroupBox = mainTabPage.Controls.Find("ISOTypeGroupBox", true)[0];
				var isoTypeFindBox = mainTabPage.Controls.Find("ISOTypeFindBox", true)[0];

				Assert(codeMapsGrid.Visible);
				Assert(isoTypeFindBox.GetType() == typeof(ZCodeFindBox));
			}
		}

		public void TestContainerControl_Sea()
		{
			var seaContainer = RefContainer.New(Factory);
			seaContainer.RC_ShippingMode = "SEA";
			using (var form = new RefContainerForm(seaContainer))
			{
				form.Show();
				var containerControl = form.Controls.Find("ContainerControl", true)[0];
				Assert(containerControl.Visible);
				Assert(containerControl.GetType() == typeof(RefContainerControl));
			}
		}

		public void TestContainerControl_Air()
		{
			var airContainer = RefContainer.New(Factory);
			airContainer.RC_ShippingMode = "AIR";
			using (var form = new RefContainerForm(airContainer))
			{
				form.Show();
				var containerControl = form.Controls.Find("ContainerControl", true)[0];
				Assert(containerControl.Visible);
				Assert(containerControl.GetType() == typeof(AirRefContainerControl));
			}
		}
	}
}
