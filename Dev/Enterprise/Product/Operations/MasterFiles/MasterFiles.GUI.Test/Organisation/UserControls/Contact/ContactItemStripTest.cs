using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ContactItemStripFormForTest))]
	sealed class ContactItemStripTest : ZFormBasherTest
	{
		#region IsMandatory

		public void TestIsMandatory()
		{
			var contactItem = GetNewContactItem();
			using (var form = new ContactItemStripFormForTest(contactItem))
			{
				form.Show();

				form.ContactItemStrip.IsMandatory = false;
				AssertEquals("DeleteButton.Visible", true, form.ContactItemStrip.DeleteButton_Exposed.Visible);

				form.ContactItemStrip.IsMandatory = true;
				AssertEquals("DeleteButton.Visible", false, form.ContactItemStrip.DeleteButton_Exposed.Visible);
			}
		}

		#endregion

		#region ReadOnly

		[RequiresSTA]
		public void TestReadOnly()
		{
			var contactItem = GetNewContactItem();
			contactItem.ReadOnly = false;

			using (var form = new ContactItemStripFormForTest(contactItem))
			{
				form.Show();

				AssertEquals("Should not be ReadOnly by default", false, form.ContactItemStrip.ReadOnly);

				form.ContactItemStrip.ReadOnly = true;
				AssertEquals(true, form.ContactItemStrip.ReadOnly);
			}

			contactItem.ReadOnly = true;
			using (var form = new ContactItemStripFormForTest(contactItem))
			{
				form.Show();

				form.ContactItemStrip.ReadOnly = false;
				AssertEquals("Control should be ReadOnly because ContactItem is readonly", true, form.ContactItemStrip.ReadOnly);

				form.ContactItemStrip.ReadOnly = true;
				AssertEquals(true, form.ContactItemStrip.ReadOnly);
			}
		}

		public void TestRefreshReadOnlyControls()
		{
			var contactItem = GetNewContactItem();
			var readOnlyContactItem = GetNewContactItem();
			readOnlyContactItem.ReadOnly = true;

			using (var form = new ContactItemStripFormForTest(contactItem))
			{
				form.Show();
				Application.DoEvents();

				form.ContactItemStrip.ReadOnly = true;
				AssertEquals(false, form.ContactItemStrip.DescriptionDropDownList_Exposed.Enabled);
				AssertEquals(false, form.ContactItemStrip.DeleteButton_Exposed.Enabled);

				form.ContactItemStrip.ReadOnly = false;
				AssertEquals(true, form.ContactItemStrip.DescriptionDropDownList_Exposed.Enabled);
				AssertEquals(true, form.ContactItemStrip.DeleteButton_Exposed.Enabled);

				form.SetDataBinding(readOnlyContactItem, "");
				AssertEquals(false, form.ContactItemStrip.DescriptionDropDownList_Exposed.Enabled);
				AssertEquals(false, form.ContactItemStrip.DeleteButton_Exposed.Enabled);
			}
		}

		#endregion

		#region Implementation

		ContactItemProxy GetNewContactItem()
		{
			var orgContactItem = Factory.New<OrgContactItem>();
			orgContactItem.OI_ContactItemType = OrgContactItemTypes.Codes.Email;
			return new EmailContactItem(orgContactItem);
		}

		protected override Form GetFormToBashCore()
		{
			var contactItem = GetNewContactItem();
			return new ContactItemStripFormForTest(contactItem);
		}

		public class ContactItemStripForTesting : ContactItemStrip
		{
			public ZDropEditWithFixedWidth DescriptionDropDownList_Exposed
			{
				get { return DescriptionDropDownList; }
			}

			public ZButton DeleteButton_Exposed
			{
				get { return deleteButton; }
			}
		}

		public class ContactItemStripFormForTest : ZForm
		{
			public ContactItemStripFormForTest(ContactItemProxy contactItem)
				: base(contactItem)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				ContactItemStrip = new ContactItemStripForTesting();
				Controls.Add(ContactItemStrip);
				BindingSource.SetBindingMember(ContactItemStrip, ".");
				CaptionRenderingEnabled = true;
			}

			public ContactItemStripForTesting ContactItemStrip;
		}

		#endregion
	}
}
