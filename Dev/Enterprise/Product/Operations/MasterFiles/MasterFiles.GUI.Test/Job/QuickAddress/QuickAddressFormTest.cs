using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(QuickAddressForm))]
	sealed class QuickAddressFormTest : ZFormBasherTest
	{
		#region TestPositioningControlNull

		public void TestPositioningControlNull()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			using (var frm = new Form())
			{
				frm.Size = new Size(400, 400);
				frm.Location = new Point(20, 40);

				using (var addrForm = new QuickAddressForm(host))
				{
					ZFormModaliser.Show(addrForm, frm);
					AssertNotEquals(0, addrForm.Location.X);
					AssertNotEquals(0, addrForm.Location.Y);
				}
			}
		}

		#endregion

		#region TestDocAddressCaption

		public void TestDocAddressCaption()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			using (var frm = new Form())
			using (var button = new ZButton())
			{
				frm.Controls.Add(button);
				using (var addrForm = new QuickAddressForm(host, button))
				{
					AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCTO), addrForm.NewAddressDocAddressControl.Text);

					host.AddressTypeCode = "CFS";
					AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCFS), addrForm.NewAddressDocAddressControl.Text);
				}
			}
		}

		#endregion

		#region TestCloseButtonHasValidation

		public void TestCloseButtonHasValidation()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			host.DocAddress.E2_CompanyName = "QWWE";
			host.DocAddress.E2_Address1 = ZString.Empty;
			using (var frm = new Form())
			{
				using (var addrForm = new QuickAddressForm(host))
				{
					ZFormModaliser.Show(addrForm, frm);
					var cancelButton = (ZButton)addrForm.Controls.Find("CancelButtonX", true).Single();
					cancelButton.PerformClick();

					AssertEquals(false, host.DocAddress.HasErrors);
				}

				using (var addrForm = new QuickAddressForm(host))
				{
					ZFormModaliser.Show(addrForm, frm);
					var closeButton = (ZButton)addrForm.Controls.Find("CancelButtonX", true).Single();
					closeButton.Text = "Close";
					closeButton.PerformClick();

					AssertEquals(true, host.DocAddress.HasErrors);
				}
			}
		}

		#endregion

		#region TestDocAddressPositioningControlOnLeftScreen

		public void TestDocAddressPositioningControlOnLeftScreen()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			using (var frm = new ZForm())
			using (var button = new ZButton())
			{
				frm.Size = new Size(400, 400);
				button.Size = new Size(20, 20);
				frm.Controls.Add(button);
				frm.Show();
				frm.Location = ControlDpiScalingHelper.NewScaledPoint(-1000, 0, false);
				using (var addrForm = new QuickAddressForm(host, button))
				{
					ZFormModaliser.Show(addrForm, frm);
					Assert("Left X is negative", addrForm.Location.X < 0);
				}
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			var button = new ZButton();
			buttonForm.Controls.Add(button);

			var form = new QuickAddressForm(host, button);
			MissingResourceStringChecker.ExcludeFromTest(form.NewAddressDocAddressControl);
			return form;
		}

		protected override void SetUp()
		{
			buttonForm = new Form();

			base.SetUp();
		}
		protected override void TearDown()
		{
			buttonForm.Dispose();

			base.TearDown();
		}

		Form buttonForm;

		#endregion

		#region DocAddressCreatorHelper

		DocAddressCreatorHelper DocAddressCreatorHelper
		{
			get { return docAddressCreatorHelper ?? (docAddressCreatorHelper = new DocAddressCreatorHelper(Factory)); }
		}
		DocAddressCreatorHelper docAddressCreatorHelper;

		#endregion
	}
}
