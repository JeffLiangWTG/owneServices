using System;
using System.Collections.Generic;
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
	[TestedType(typeof(MultiPhoneDiallerUserControlFormForTest))]
	public class MultiPhoneDiallerUserControlTest : ZFormBasherTest
	{
		#region CallButton

		[RequiresSTA]
		public void TestCallButton()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, true);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var phoneDiallerForTest = new TestPhoneDialler();
			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form.MultiPhoneDiallerUserControl.DefaultDialInfo = new PhoneDialInfo("02 12345678", "Mobile");
				form.Show();

				AssertEquals("Mobile", form.MultiPhoneDiallerUserControl.CallButton_Exposed.TextIgnoringInternalPadding);
				form.MultiPhoneDiallerUserControl.CallButton_Exposed.PerformClick();
				AssertEquals("callto:0212345678", phoneDiallerForTest.UriDialled);
			}
		}

		public void TestCallButton_NoDefaultDialInfo()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, true);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var phoneDiallerForTest = new TestPhoneDialler();
			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form.MultiPhoneDiallerUserControl.DefaultDialInfo = null;
				form.Show();

				AssertEquals("Call", form.MultiPhoneDiallerUserControl.CallButton_Exposed.TextIgnoringInternalPadding);
				form.MultiPhoneDiallerUserControl.CallButton_Exposed.PerformClick();
				AssertEquals("No phone number available.", UnitTestUserNotification.Instance.LastMessage.Text);
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

			var phoneDiallerForTest = new TestPhoneDialler();
			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form.MultiPhoneDiallerUserControl.DefaultDialInfo = new PhoneDialInfo("02 11119999", "Office");
				form.Show();

				AssertEquals("Office", form.MultiPhoneDiallerUserControl.CallButton_Exposed.TextIgnoringInternalPadding);
				form.MultiPhoneDiallerUserControl.CallButton_Exposed.PerformClick();
				AssertEquals("Phone Dialing URI Protocol must be set before phone calls can be established. You can modify this in the System Registry under " + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Category + "/" + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Caption + ".", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestCallButton_WithExtension()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			_ = dialingProtocols.AddNew("tel", (NoResString)"Lync", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Phone = "02 11119999";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";

			var workPhone = new PhoneContactItem(contact);
			workPhone.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			workPhone.OI_Address = "02 12345678";

			var extension = new PhoneContactItem(contact);
			extension.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
			extension.OI_Address = "112";

			var workPhone2 = new PhoneContactItem(contact);
			workPhone2.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			workPhone2.OI_Address = "04 87654321";

			var phoneDiallerForTest = new TestPhoneDialler();
			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form.MultiPhoneDiallerUserControl.DefaultDialInfo = new PhoneDialInfo(workPhone, extension);
				form.Show();

				AssertEquals("Work*", form.MultiPhoneDiallerUserControl.CallButton_Exposed.TextIgnoringInternalPadding);
			}

			using (var form2 = new MultiPhoneDiallerUserControlFormForTest())
			{
				form2.MultiPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form2.MultiPhoneDiallerUserControl.DefaultDialInfo = new PhoneDialInfo(workPhone2);
				form2.Show();

				AssertEquals("Work 2", form2.MultiPhoneDiallerUserControl.CallButton_Exposed.TextIgnoringInternalPadding);
			}
		}
		#endregion

		#region DropButton

		[RequiresSTA]
		public void TestDropButtonClick_NoPhone()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", true, true);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", false, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var contact = Factory.New<OrgContact>();

			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MultiPhoneDiallerUserControl.DropButton_Exposed.PerformClick();
				AssertEquals("No phone number available.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestDropButtonClick_NoDialingUriProtocol()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, false);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", false, false);
			using (SystemDataRegistry.Instance.PhoneDialingUriProtocols.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);
			}

			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MultiPhoneDiallerUserControl.DropButton_Exposed.PerformClick();
				AssertEquals("Phone Dialing URI Protocol must be set before phone calls can be established. You can modify this in the System Registry under " + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Category + "/" + SystemDataRegistry.Instance.PhoneDialingUriProtocols.Caption + ".", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestAlternativesContextMenu()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			dialingProtocols.AddNew("tel", (NoResString)"Lync", false, true);
			dialingProtocols.AddNew("callto", (NoResString)"Skype", false, false);
			dialingProtocols.AddNew("irc", (NoResString)"IRC", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var alternativeDialInfos = new[]
				{
					new PhoneDialInfo("02 11119999", "Office"),
					new PhoneDialInfo("02 12345678", "Work"),
					new PhoneDialInfo("02 98765432", "Work"),
					new PhoneDialInfo("02 33336666", "Home"),
				};

			var phoneDiallerForTest = new TestPhoneDialler();
			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form.MultiPhoneDiallerUserControl.AlternativePhoneDialInfos = alternativeDialInfos;
				form.Show();

				var firstLevelItems = form.MultiPhoneDiallerUserControl.AlternativesContextMenuItems_Exposed.Cast<ToolStripDropDownItem>().ToArray();
				AssertArrayEqualsByElements(new[]
					{
						"Call Home (02 33336666)",
						"Call Office (02 11119999)",
						"Call Work (02 12345678)",
						"Call Work (02 98765432)",
						"Call using Lync"
					},
					firstLevelItems.Select(item => item.Text).ToArray());

				firstLevelItems[0].PerformClick();
				AssertEquals("irc:0233336666", phoneDiallerForTest.UriDialled);
				firstLevelItems[1].PerformClick();
				AssertEquals("irc:0211119999", phoneDiallerForTest.UriDialled);
				firstLevelItems[2].PerformClick();
				AssertEquals("irc:0212345678", phoneDiallerForTest.UriDialled);
				firstLevelItems[3].PerformClick();
				AssertEquals("irc:0298765432", phoneDiallerForTest.UriDialled);

				var lyncDropDownItems = firstLevelItems[4].DropDownItems.Cast<ToolStripDropDownItem>().ToArray();
				AssertArrayEqualsByElements(new[]
					{
						"Call Home (02 33336666)",
						"Call Office (02 11119999)",
						"Call Work (02 12345678)",
						"Call Work (02 98765432)"
					},
					lyncDropDownItems.Select(item => item.Text).ToArray());

				lyncDropDownItems[0].PerformClick();
				AssertEquals("tel:0233336666", phoneDiallerForTest.UriDialled);
				lyncDropDownItems[1].PerformClick();
				AssertEquals("tel:0211119999", phoneDiallerForTest.UriDialled);
				lyncDropDownItems[2].PerformClick();
				AssertEquals("tel:0212345678", phoneDiallerForTest.UriDialled);
				lyncDropDownItems[3].PerformClick();
				AssertEquals("tel:0298765432", phoneDiallerForTest.UriDialled);
			}
		}

		[RequiresSTA]
		public void TestAlternativesContextMenu_OnlyIncludesSkypeNumbersForSkypeProtocols()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Phone = "02 11119999";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			var homePhone = contact.PhoneContactItems.AddNew();
			var skype1 = contact.PhoneContactItems.AddNew();
			var skype2 = contact.PhoneContactItems.AddNew();
			homePhone.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhone.OI_Address = "02 33336666";
			skype1.OI_Description = PhoneContactItemDescriptionList.Codes.Skype;
			skype1.OI_Address = "andrew@wisetechglobal.com";
			skype2.OI_Description = PhoneContactItemDescriptionList.Codes.Skype2;
			skype2.OI_Address = "andrew@cargowise.com";

			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, true);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var alternativeDialInfos = new[]
				{
					new PhoneDialInfo("02 11119999", "Office"),
					new PhoneDialInfo(homePhone),
					new PhoneDialInfo(skype1),
					new PhoneDialInfo(skype2)
				};

			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.AlternativePhoneDialInfos = alternativeDialInfos;
				form.Show();

				var firstLevelItems = form.MultiPhoneDiallerUserControl.AlternativesContextMenuItems_Exposed.Cast<ToolStripDropDownItem>().ToArray();
				AssertArrayEqualsByElements(new[]
					{
						"Call Home (02 33336666)",
						"Call Office (02 11119999)",
						"Call Skype (andrew@wisetechglobal.com)",
						"Call Skype 2 (andrew@cargowise.com)",
						"Call using Lync"
					},
					firstLevelItems.Select(item => item.Text).ToArray());

				var lyncDropDownItems = firstLevelItems[4].DropDownItems.Cast<ToolStripDropDownItem>().ToArray();
				AssertArrayEqualsByElements(new[]
					{
						"Call Home (02 33336666)",
						"Call Office (02 11119999)"
					},
					lyncDropDownItems.Select(item => item.Text).ToArray());
			}

			skypeProtocol.IsDefault = false;
			lyncProtocol.IsDefault = true;
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.AlternativePhoneDialInfos = alternativeDialInfos;
				form.Show();

				var firstLevelItems = form.MultiPhoneDiallerUserControl.AlternativesContextMenuItems_Exposed.Cast<ToolStripDropDownItem>().ToArray();
				AssertArrayEqualsByElements(new[]
					{
						"Call Home (02 33336666)",
						"Call Office (02 11119999)",
						"Call using Skype"
					},
					firstLevelItems.Select(item => item.Text).ToArray());

				var lyncDropDownItems = firstLevelItems[2].DropDownItems.Cast<ToolStripDropDownItem>().ToArray();
				AssertArrayEqualsByElements(new[]
					{
						"Call Home (02 33336666)",
						"Call Office (02 11119999)",
						"Call Skype (andrew@wisetechglobal.com)",
						"Call Skype 2 (andrew@cargowise.com)"
					},
					lyncDropDownItems.Select(item => item.Text).ToArray());
			}
		}

		public void TestAlternativesContextMenu_WithExtension()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			dialingProtocols.AddNew("tel", (NoResString)"Lync", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Phone = "02 11119999";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";

			var workPhone = new PhoneContactItem(contact);
			workPhone.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			workPhone.OI_Address = "02 12345678";

			var extension = new PhoneContactItem(contact);
			extension.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
			extension.OI_Address = "112";

			var workPhone2 = new PhoneContactItem(contact);
			workPhone2.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			workPhone2.OI_Address = "04 87654321";

			var alternativeDialInfos = new[]
				{
					new PhoneDialInfo(workPhone, extension),
					new PhoneDialInfo(workPhone2),
				};

			var phoneDiallerForTest = new TestPhoneDialler();
			using (var form = new MultiPhoneDiallerUserControlFormForTest())
			{
				form.MultiPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form.MultiPhoneDiallerUserControl.AlternativePhoneDialInfos = alternativeDialInfos;
				form.Show();

				var firstLevelItems = form.MultiPhoneDiallerUserControl.AlternativesContextMenuItems_Exposed.Cast<ToolStripDropDownItem>().ToArray();
				AssertArrayEqualsByElements(new[]
					{
						"Call Work (02 12345678) Ext. 112",
						"Call Work 2 (04 87654321)",
						"Call using Skype"
					},
					firstLevelItems.Select(item => item.Text).ToArray());
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var contact = Factory.New<OrgContact>();
			return new MultiPhoneDiallerUserControlFormForTest();
		}

		#region Classes

		public class MultiPhoneDiallerUserControlForTest : MultiPhoneDiallerUserControl
		{
			public PhoneDialInfo DefaultDialInfo;
			protected override PhoneDialInfo GetDefaultDialInfo()
			{
				return DefaultDialInfo;
			}

			public IEnumerable<PhoneDialInfo> AlternativePhoneDialInfos;
			protected override IEnumerable<PhoneDialInfo> AlternativePhoneDialInfoList
			{
				get { return AlternativePhoneDialInfos ?? Enumerable.Empty<PhoneDialInfo>(); }
			}

			public ZButton DropButton_Exposed
			{
				get { return DropButton; }
			}

			public ImageButtonWithoutTextInternalPadding CallButton_Exposed
			{
				get { return CallButton; }
			}

			public ToolStripItemCollection AlternativesContextMenuItems_Exposed
			{
				get { return AlternativesContextMenuItems; }
			}
		}

		public class MultiPhoneDiallerUserControlFormForTest : ZForm
		{
			public MultiPhoneDiallerUserControlFormForTest()
				: base()
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				MultiPhoneDiallerUserControl = new MultiPhoneDiallerUserControlForTest();
				Controls.Add(MultiPhoneDiallerUserControl);
				CaptionRenderingEnabled = true;
			}

			public MultiPhoneDiallerUserControlForTest MultiPhoneDiallerUserControl;
		}

		#endregion

		#endregion
	}
}
