using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(PhoneDiallerUserControlFormForTest))]
	sealed class PhoneDiallerUserControlTest : ZFormBasherTest
	{
		#region CallButton

		public void TestCallButton()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, true);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Address = "02 12345678";

			using (var form = new PhoneDiallerUserControlFormForTest(phoneItem))
			{
				form.Show();

				form.ContactPhoneItemDiallerUserControl.CallButton_Exposed.PerformClick();
				AssertEquals("callto:0212345678", form.ContactPhoneItemDiallerUserControl.PhoneDiallerForTest.UriDialled);
			}
		}

		public void TestCallButton_NoNumberEntered()
		{
			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Address = "";
			using (var form = new PhoneDiallerUserControlFormForTest(phoneItem))
			{
				form.Show();

				form.ContactPhoneItemDiallerUserControl.CallButton_Exposed.PerformClick();
				AssertEquals("No phone number entered.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCallButton_NoDialingUriProtocol()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, false);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", false, false);
			using (SystemDataRegistry.Instance.PhoneDialingUriProtocols.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);
			}

			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Address = "02 12345678";
			using (var form = new PhoneDiallerUserControlFormForTest(phoneItem))
			{
				form.Show();

				form.ContactPhoneItemDiallerUserControl.CallButton_Exposed.PerformClick();
				AssertEquals("Phone Dialing URI Protocol must be set before phone calls can be established. You can modify this in the System Registry under " + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Category + "/" + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Caption + ".", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region DropButton / DialingProtocolsContextMenu

		public void TestDropButton()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", true, true);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", false, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Address = "02 12345678";
			using (var form = new PhoneDiallerUserControlFormForTest(phoneItem))
			{
				form.Show();

				form.ContactPhoneItemDiallerUserControl.DropButton_Exposed.PerformClick();
				AssertArrayEqualsByElements(
					new[] { "Call using Lync", "Call using Skype" },
					form.ContactPhoneItemDiallerUserControl.DialingProtocolsContextMenuItems_Exposed.Cast<ZToolStripMenuItem>().Select(menuItem => menuItem.Text).ToArray());

				form.ContactPhoneItemDiallerUserControl.DropButton_Exposed.ContextMenuStrip.Close();
			}
		}

		public void TestDropButton_NoNumberEntered()
		{
			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Address = "";
			using (var form = new PhoneDiallerUserControlFormForTest(phoneItem))
			{
				form.Show();

				form.ContactPhoneItemDiallerUserControl.DropButton_Exposed.PerformClick();
				AssertEquals("No phone number entered.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDropButton_NoDialingUriProtocol()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, false);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", false, false);
			using (SystemDataRegistry.Instance.PhoneDialingUriProtocols.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);
			}

			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Address = "02 12345678";
			using (var form = new PhoneDiallerUserControlFormForTest(phoneItem))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ContactPhoneItemDiallerUserControl.DropButton_Exposed.PerformClick();
				AssertEquals("Phone Dialing URI Protocol must be set before phone calls can be established. You can modify this in the System Registry under " + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Category + "/" + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Caption + ".", UnitTestUserNotification.Instance.LastMessage.Text);
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
			return new PhoneDiallerUserControlFormForTest(phoneItem);
		}

		#region Classes

		public class PhoneDiallerUserControlForTest : PhoneDiallerUserControl
		{
			public TestPhoneDialler PhoneDiallerForTest = new TestPhoneDialler();

			protected override PhoneDialler GetNewPhoneDialler()
			{
				return PhoneDiallerForTest;
			}

			public ZButton DropButton_Exposed
			{
				get { return DropButton; }
			}

			public ZButton CallButton_Exposed
			{
				get { return CallButton; }
			}

			public ToolStripItemCollection DialingProtocolsContextMenuItems_Exposed
			{
				get { return DialingProtocolsContextMenuItems; }
			}
		}

		public class PhoneDiallerUserControlFormForTest : ZForm
		{
			public PhoneDiallerUserControlFormForTest(PhoneContactItem contactItem)
				: base(contactItem)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				ContactPhoneItemDiallerUserControl = new PhoneDiallerUserControlForTest();
				Controls.Add(ContactPhoneItemDiallerUserControl);
				BindingSource.SetBindingMember(ContactPhoneItemDiallerUserControl, "OI_Address");
				CaptionRenderingEnabled = true;
			}

			public PhoneDiallerUserControlForTest ContactPhoneItemDiallerUserControl;
		}

		#endregion

		#endregion
	}
}
