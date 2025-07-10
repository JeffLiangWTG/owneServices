using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(PhoneContactItemStripFormForTest))]
	sealed class PhoneContactItemStripTest : ZFormBasherTest
	{
		public void TestContactPhoneItemDiallerVisibility()
		{
			var phoneItem = GetNewPhoneContactItem();
			using (var form = new PhoneContactItemStripFormForTest(phoneItem))
			{
				form.Show();

				AssertEquals("Precondition", false, phoneItem.IsCallable);
				AssertEquals(false, form.PhoneItemStrip.ContactPhoneItemDialler_Exposed.Visible);

				phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
				AssertEquals("Precondition", true, phoneItem.IsCallable);
				AssertEquals(true, form.PhoneItemStrip.ContactPhoneItemDialler_Exposed.Visible);

				form.SetDataBinding(null, "");
				AssertEquals(false, form.PhoneItemStrip.ContactPhoneItemDialler_Exposed.Visible);
			}
		}

		public void TestAccessContactWhenDeleted()
		{
			var phoneItem = GetNewPhoneContactItem();

			using (var form = new PhoneContactItemStripFormForTest(phoneItem))
			{
				form.Show();
				phoneItem.Delete();

				AssertNoExceptionThrown("Should not access a property on a deleted business object.", () => form.PhoneItemStrip.deleteButton_Click(new object(), new EventArgs()));
			}
		}

		#region ReadOnly

		public void TestRefreshReadOnlyControls()
		{
			var phoneItem = GetNewPhoneContactItem();
			using (var form = new PhoneContactItemStripFormForTest(phoneItem))
			{
				form.Show();

				form.PhoneItemStrip.ReadOnly = true;
				AssertEquals(true, form.PhoneItemStrip.AddressTextBox_Exposed.ReadOnly);
				AssertEquals(true, form.PhoneItemStrip.ContactPhoneItemDialler_Exposed.Enabled);

				form.PhoneItemStrip.ReadOnly = false;
				AssertEquals(false, form.PhoneItemStrip.AddressTextBox_Exposed.ReadOnly);
				AssertEquals(true, form.PhoneItemStrip.ContactPhoneItemDialler_Exposed.Enabled);

				form.SetDataBinding(null, "");
				AssertEquals(true, form.PhoneItemStrip.AddressTextBox_Exposed.ReadOnly);
				AssertEquals(true, form.PhoneItemStrip.ContactPhoneItemDialler_Exposed.Enabled);
			}
		}

		[RequiresSTA]
		public void TestNumberTextBoxMaxLengthSetCorrectly()
		{
			var phoneItem = GetNewPhoneContactItem();
			using (var form = new PhoneContactItemStripFormForTest(phoneItem))
			{
				form.Show();
				var currentDataItem = form.PhoneItemStrip.CurrentDataItem;

				// Assert: NumberTextBoxMaxLength is set correctly on phone extension and skype
				currentDataItem.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
				form.PhoneItemStrip.DescriptionDropDownList_TextChanged_ForTest();
				AssertEquals(AutoOrgContact.Schema.OC_PhoneExtensionMaxLength, form.PhoneItemStrip.PhoneNumberControlForTest.NumberTextBoxMaxLength);

				currentDataItem.OI_Description = PhoneContactItemDescriptionList.Codes.Skype2;
				form.PhoneItemStrip.DescriptionDropDownList_TextChanged_ForTest();
				AssertEquals(AutoOrgContact.Schema.OC_EmailMaxLength, form.PhoneItemStrip.PhoneNumberControlForTest.NumberTextBoxMaxLength);

				// Assert: NumberTextBoxMaxLength is cancelled on other phone number type
				currentDataItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
				form.PhoneItemStrip.DescriptionDropDownList_TextChanged_ForTest();
				AssertEquals(0, form.PhoneItemStrip.PhoneNumberControlForTest.NumberTextBoxMaxLength);
			}
		}

		#endregion

		#region OI_Address_Formatted

		public void TestOI_Address_Formatted()
		{
			var phoneItem = GetNewPhoneContactItem();
			using (var form = new PhoneContactItemStripFormForTest(phoneItem))
			{
				form.Show();

				var bindTo = form.PhoneItemStrip.AddressTextBox_Exposed.BindTo;
				AssertEquals("AddressTextBox should be bound to FormattedForBinding", "FormattedForBinding", bindTo);
			}
		}

		#endregion

		#region Implementation

		PhoneContactItem GetNewPhoneContactItem()
		{
			var contact = Factory.New<OrgContact>();
			return contact.PhoneContactItems.AddNew();
		}

		protected override Form GetFormToBashCore()
		{
			var phoneItem = GetNewPhoneContactItem();
			return new PhoneContactItemStripFormForTest(phoneItem);
		}

		public class PhoneContactItemStripForTesting : PhoneContactItemStrip
		{
			public PhoneDiallerUserControl ContactPhoneItemDialler_Exposed
			{
				get { return PhoneNumberControl.Controls.Find("PhoneDiallerControl", false)[0] as PhoneDiallerUserControl; }
			}

			public new void deleteButton_Click(object sender, EventArgs e)
			{
				base.deleteButton_Click(sender, e);
			}

			public ZTextBox AddressTextBox_Exposed
			{
				get { return PhoneNumberControl.Controls.Find("NumberTextBox", false)[0] as ZTextBox; }
			}

			public PhoneNumberUserControl PhoneNumberControlForTest => PhoneNumberControl;
			public PhoneContactItem CurrentDataItemForTest => CurrentDataItem;
			public void DescriptionDropDownList_TextChanged_ForTest()
			{
				DescriptionDropDownList_TextChanged(null, null);
			}
		}

		public class PhoneContactItemStripFormForTest : ZForm
		{
			public PhoneContactItemStripFormForTest(PhoneContactItem phoneItem)
				: base(phoneItem)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				PhoneItemStrip = new PhoneContactItemStripForTesting();
				Controls.Add(PhoneItemStrip);
				BindingSource.SetBindingMember(PhoneItemStrip, ".");
				CaptionRenderingEnabled = true;
			}

			public PhoneContactItemStripForTesting PhoneItemStrip;
		}

		#endregion
	}
}
