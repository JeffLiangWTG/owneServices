using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ContactPhoneDiallerUserControlTest : TestCaseWithFactory
	{
		#region CallButton

		public void TestCallButton_NoWorkOrOfficePhone()
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var lyncProtocol = dialingProtocols.AddNew("tel", (NoResString)"Lync", false, true);
			var skypeProtocol = dialingProtocols.AddNew("callto", (NoResString)"Skype", true, true);
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);

			var contact = Factory.New<OrgContact>();
			using (var form = new ContactPhoneDiallerUserControlFormForTest(contact))
			{
				form.Show();

				AssertEquals("Call", form.ContactPhoneDiallerUserControl.CallButton_Exposed.TextIgnoringInternalPadding);
				form.ContactPhoneDiallerUserControl.CallButton_Exposed.PerformClick();
				AssertEquals("Contact does not have a Work or Office phone contact details.", UnitTestUserNotification.Instance.LastMessage.Text);
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
			using (var form = new ContactPhoneDiallerUserControlFormForTest(contact))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ContactPhoneDiallerUserControl.DropButton_Exposed.PerformClick();
				AssertEquals("No phone contact details available.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		#region Classes

		public class ContactPhoneDiallerUserControlForTest : ContactPhoneDiallerUserControl
		{
			public ZButton DropButton_Exposed
			{
				get { return DropButton; }
			}

			public ImageButtonWithoutTextInternalPadding CallButton_Exposed
			{
				get { return CallButton; }
			}
		}

		public class ContactPhoneDiallerUserControlFormForTest : ZForm
		{
			public ContactPhoneDiallerUserControlFormForTest(OrgContact contact)
				: base(contact)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				ContactPhoneDiallerUserControl = new ContactPhoneDiallerUserControlForTest();
				Controls.Add(ContactPhoneDiallerUserControl);
				BindingSource.SetBindingMember(ContactPhoneDiallerUserControl, "PK");
				CaptionRenderingEnabled = true;
			}

			public ContactPhoneDiallerUserControlForTest ContactPhoneDiallerUserControl;
		}

		#endregion

		#endregion
	}
}
