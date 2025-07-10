using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EmailContactItemStripFormForTest))]
	sealed class EmailContactItemStripTest : ZFormBasherTest
	{
		#region ReadOnly

		public void TestRefreshReadOnlyControls()
		{
			var emailItem = GetNewEmailContactItem();
			using (var form = new EmailContactItemStripFormForTest(emailItem))
			{
				form.Show();

				form.EmailItemStrip.ReadOnly = true;
				AssertEquals(true, form.EmailItemStrip.AddressTextBox_Exposed.ReadOnly);

				form.EmailItemStrip.ReadOnly = false;
				AssertEquals(false, form.EmailItemStrip.AddressTextBox_Exposed.ReadOnly);

				form.SetDataBinding(null, "");
				AssertEquals(true, form.EmailItemStrip.AddressTextBox_Exposed.ReadOnly);
			}
		}

		#endregion

		#region OnCurrentDataItemChanged

		public void TestOnCurrentDataItemChanged()
		{
			var emailItem = GetNewEmailContactItem();
			emailItem.OI_Address = "email@server.com";
			using (var form = new EmailContactItemStripFormForTest(emailItem))
			{
				form.Show();
				AssertEquals(emailItem.OI_Address, form.EmailItemStrip.AddressTextBox_Exposed.Text);

				form.EmailItemStrip.SetDataBinding(null, "");
				AssertEquals(true, string.IsNullOrEmpty(form.EmailItemStrip.AddressTextBox_Exposed.Text));
			}
		}

		#endregion

		#region Implementation

		EmailContactItem GetNewEmailContactItem()
		{
			var contact = Factory.New<OrgContact>();
			return contact.EmailContactItems.AddNew();
		}

		protected override Form GetFormToBashCore()
		{
			var emailItem = GetNewEmailContactItem();
			return new EmailContactItemStripFormForTest(emailItem);
		}

		public class EmailContactItemStripForTesting : EmailContactItemStrip
		{
			public ZTextBox AddressTextBox_Exposed
			{
				get { return AddressTextBox; }
			}
		}

		public class EmailContactItemStripFormForTest : ZForm
		{
			public EmailContactItemStripFormForTest(EmailContactItem emailItem)
				: base(emailItem)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				EmailItemStrip = new EmailContactItemStripForTesting();
				Controls.Add(EmailItemStrip);
				BindingSource.SetBindingMember(EmailItemStrip, ".");
				CaptionRenderingEnabled = true;
			}

			public EmailContactItemStripForTesting EmailItemStrip;
		}

		#endregion
	}
}
