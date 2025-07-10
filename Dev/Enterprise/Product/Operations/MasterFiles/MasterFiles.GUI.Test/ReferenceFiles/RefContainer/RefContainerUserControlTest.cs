using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.UserControls;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(RefContainer))]
	public class RefContainerUserControlTest : BasherTest
	{
		public override Form GetFormToBash() => CreateFormForTest();

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new RefContainerControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(container, ".");
			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = RefContainer.New(Factory);
			container.RC_ShippingMode = "SEA";
			Factory.Save();
		}

		RefContainer container;

		#endregion
	}
}
