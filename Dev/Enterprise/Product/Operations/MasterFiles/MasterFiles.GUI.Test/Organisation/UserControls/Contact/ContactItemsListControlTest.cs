using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ContactItemsListControlFormForTest))]
	sealed class ContactItemsListControlTest : ZFormBasherTest
	{
		#region RefreshContactItemStrips

		public void RefreshContactItemStrips_Sorted()
		{
			var contact = Factory.New<OrgContact>();
			var emailItem1 = contact.EmailContactItems.AddNew();
			emailItem1.OI_Description = EmailContactItemDescriptionList.Codes.Other;
			emailItem1.OI_Address = "andrew@live.com";
			var emailItem2 = contact.EmailContactItems.AddNew();
			emailItem2.OI_Description = EmailContactItemDescriptionList.Codes.Other;
			emailItem2.OI_Address = "andrew@cargowise.com";
			var emailItem3 = contact.EmailContactItems.AddNew();
			emailItem3.OI_Description = EmailContactItemDescriptionList.Codes.Other;
			emailItem3.OI_Address = "andrew@wisetechglobal.com";
			var emailItem4 = contact.EmailContactItems.AddNew();
			emailItem4.OI_Description = EmailContactItemDescriptionList.Codes.Main;
			emailItem4.OI_Address = "andrew@zzz.com";

			var phoneItem1 = contact.PhoneContactItems.AddNew();
			phoneItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			phoneItem1.OI_Address = "02 123456789";
			var phoneItem2 = contact.PhoneContactItems.AddNew();
			phoneItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			phoneItem2.OI_Address = "02 98765432";
			var phoneItem3 = contact.PhoneContactItems.AddNew();
			phoneItem3.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			phoneItem3.OI_Address = "04 12345678";
			var phoneItem4 = contact.PhoneContactItems.AddNew();
			phoneItem4.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;
			phoneItem4.OI_Address = "03 98765432";

			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();

				AssertArrayEqualsByElements(
					new[] { "andrew@zzz.com", "andrew@cargowise.com", "andrew@wisetechglobal.com", "andrew@live.com" },
					form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.OfType<EmailContactItemStrip>().Select(strip => strip.CurrentDataItem.OI_Address.ToString()).ToArray());

				AssertArrayEqualsByElements(
					new[] { "04 12345678", "02 123456789", "02 98765432", "03 98765432" },
					form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.OfType<PhoneContactItemStrip>().Select(strip => strip.CurrentDataItem.OI_Address.ToString()).ToArray());
			}
		}

		#endregion

		#region AddContactItemButtons

		public void TestAddEmailItemButton()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var emailItem1 = contact.EmailContactItems.AddNew(EmailContactItemDescriptionList.Codes.Main);
			emailItem1.OI_Address = "andrew@cargowise.com";
			Factory.Save();

			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();

				var itemsBeforeAddButtonClick = contact.EmailContactItems.ToArray();
				var itemStripsBeforeAddButtonClick = new HashSet<EmailContactItemStrip>(form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.OfType<EmailContactItemStrip>());
				form.ContactItemsListControl.AddEmailItemButton_Exposed.PerformClick();

				AssertEquals("Should have added new email item", itemsBeforeAddButtonClick.Length + 1, contact.EmailContactItems.Count);

				var newItem = contact.EmailContactItems.Items.Single(item => !itemsBeforeAddButtonClick.Contains(item));
				AssertEquals("Initial description", EmailContactItemDescriptionList.Codes.Other, newItem.OI_Description);

				var itemStripsAfterAddButtonClick = form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.OfType<EmailContactItemStrip>();
				AssertEquals("Should have added 1 contact item strip", itemStripsBeforeAddButtonClick.Count + 1, itemStripsAfterAddButtonClick.Count());

				var addedItemStrip = itemStripsAfterAddButtonClick.Single(item => !itemStripsBeforeAddButtonClick.Contains(item));
				Assert("Should focus on Description drop edit of newly added item", addedItemStrip.DescriptionDropDownList.CodeBox.Focused);
			}
		}

		public void TestAddPhoneItemButton()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var phoneItem1 = contact.PhoneContactItems.AddNew(PhoneContactItemDescriptionList.Codes.Mobile);
			phoneItem1.OI_Address = "andrew@cargowise.com";
			Factory.Save();

			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();

				var itemsBeforeAddButtonClick = contact.PhoneContactItems.ToArray();
				var itemStripsBeforeAddButtonClick = new HashSet<PhoneContactItemStrip>(form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.OfType<PhoneContactItemStrip>());
				form.ContactItemsListControl.AddPhoneItemButton_Exposed.PerformClick();

				AssertEquals("Should have added new phone item", itemsBeforeAddButtonClick.Length + 1, contact.PhoneContactItems.Count);

				var newItem = contact.PhoneContactItems.Items.Single(item => !itemsBeforeAddButtonClick.Contains(item));
				AssertEquals("Initial description", PhoneContactItemDescriptionList.Codes.Work2, newItem.OI_Description);

				var itemStripsAfterAddButtonClick = form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.OfType<PhoneContactItemStrip>();
				AssertEquals("Should have added 1 contact item strip", itemStripsBeforeAddButtonClick.Count + 1, itemStripsAfterAddButtonClick.Count());

				var addedItemStrip = itemStripsAfterAddButtonClick.Single(item => !itemStripsBeforeAddButtonClick.Contains(item));
				Assert("Should focus on Description drop edit of newly added item", addedItemStrip.DescriptionDropDownList.CodeBox.Focused);
			}
		}

		#endregion

		#region DeliveryStatusControls

		[TestDate(2015, 3, 17, 10, 23, 44)]
		public void TestDeliveryStatusControls()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Sm";
			contact.OC_Email = "x@x.com";
			contact.IsNDR = true;
			contact.EmailAddress.GI_DeliveryStatus = "NDR";
			contact.EmailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;

			var contactGood = Factory.NewWithValidTestData<OrgContact>();
			contactGood.OC_ContactName = "ed";
			contactGood.OC_Email = "edward@cargowise.com";
			Factory.Save();

			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();
				AssertEquals("Contact Main Email address is invalid", "Non-Delivery Receipt: 17-Mar-15 20:23", form.ContactItemsListControl.DeliveryStatusLabel_Exposed.Text);
				AssertEquals("Label forecolor should be warning color", ZArchitecture.GUI.Notifications.NotificationColorScheme.GetFontColor(NotificationType.Warning), form.ContactItemsListControl.DeliveryStatusLabel_Exposed.ForeColor);
				AssertEquals("Delivery status button should be visible", true, form.ContactItemsListControl.DeliveryStatusButton_Exposed.Visible);
				AssertImageEquals("Should be warning icon", Icons.GetIcon(IconTypes.Warning).ToBitmap(), form.ContactItemsListControl.DeliveryStatusButton_Exposed.BackgroundImage);
			}

			using (var form = new ContactItemsListControlFormForTest(contactGood))
			{
				form.Show();
				AssertEquals("Contact Main Email address is not verified yet", "Unverified", form.ContactItemsListControl.DeliveryStatusLabel_Exposed.Text);
				AssertEquals("Label forecolor should be black", System.Drawing.Color.Black, form.ContactItemsListControl.DeliveryStatusLabel_Exposed.ForeColor);
				AssertEquals("Delivery status button should be visible", true, form.ContactItemsListControl.DeliveryStatusButton_Exposed.Visible);
				AssertImageEquals("Should be the unverified image", Properties.Resources.Unverified, form.ContactItemsListControl.DeliveryStatusButton_Exposed.BackgroundImage);
			}

			contact.EmailAddress.GI_DeliveryStatus = "VLD";
			Factory.Save();
			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();
				AssertEquals("Contact Main Email address is verified", "Verified", form.ContactItemsListControl.DeliveryStatusLabel_Exposed.Text);
				AssertEquals("Label forecolor should be black", System.Drawing.Color.Black, form.ContactItemsListControl.DeliveryStatusLabel_Exposed.ForeColor);
				AssertEquals("Delivery status button should be visible", true, form.ContactItemsListControl.DeliveryStatusButton_Exposed.Visible);
				AssertImageEquals("Should be the verfied image", Properties.Resources.Verified, form.ContactItemsListControl.DeliveryStatusButton_Exposed.BackgroundImage);
			}

			contact.OC_Email = "";
			Factory.Save();
			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();
				AssertEquals("Should not display any text", "", form.ContactItemsListControl.DeliveryStatusLabel_Exposed.Text);
				AssertEquals("Delivery status button should not be visible", false, form.ContactItemsListControl.DeliveryStatusButton_Exposed.Visible);
			}
		}

		[TestDate(2015, 3, 17, 10, 23, 44)]
		public void TestUpdateDeliveryStatusControlsOnIsNDRChanged()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Sm";
			contact.OC_Email = "x@x.com";
			contact.IsNDR = true;
			contact.EmailAddress.GI_DeliveryStatus = "NDR";
			contact.EmailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;

			var contactGood = Factory.NewWithValidTestData<OrgContact>();
			contactGood.OC_ContactName = "ed";
			contactGood.OC_Email = "edward@cargowise.com";
			Factory.Save();

			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();
				AssertEquals("Precondition", true, contact.IsNDR);
				AssertEquals("Contact Main Email address is invalid", "Non-Delivery Receipt: 17-Mar-15 20:23", form.ContactItemsListControl.DeliveryStatusLabel_Exposed.Text);
				AssertEquals("Label forecolor should be warning color", ZArchitecture.GUI.Notifications.NotificationColorScheme.GetFontColor(NotificationType.Warning), form.ContactItemsListControl.DeliveryStatusLabel_Exposed.ForeColor);
				AssertEquals("Delivery status button should be visible", true, form.ContactItemsListControl.DeliveryStatusButton_Exposed.Visible);
				AssertImageEquals("Should be warning icon", Icons.GetIcon(IconTypes.Warning).ToBitmap(), form.ContactItemsListControl.DeliveryStatusButton_Exposed.BackgroundImage);

				contact.IsNDR = false;
				AssertEquals("Should have updated", "Unverified", form.ContactItemsListControl.DeliveryStatusLabel_Exposed.Text);
				AssertImageEquals("Delivery status button icon should be Unverified", Properties.Resources.Unverified, form.ContactItemsListControl.DeliveryStatusButton_Exposed.NormalBackgroundImage);
				AssertImageEquals("Delivery status button icon when moused over should be Unverified", Properties.Resources.Unverified, form.ContactItemsListControl.DeliveryStatusButton_Exposed.HotBackgroundImage);
			}
		}

		#endregion

		#region MandatoryContactItems

		public void TestMandatoryContactItems_AfterUnsuccessfulSave()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();

				var mandatoryEmailItemStrips = form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.OfType<ContactItemStrip>().ToArray();
				var mandatoryPhoneItemStrips = form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.OfType<ContactItemStrip>().ToArray();
				CombineAssertions("Precondition: Mandatory ContactItemStrips should have a datasource", () =>
				{
					foreach (var itemStrip in mandatoryEmailItemStrips.Union(mandatoryPhoneItemStrips))
					{
						AssertNotNull(itemStrip.CurrentDataItem);
					}
				});

				contact.OC_Fax_Formatted = "invalid number";
				var saveResult = form.FireSaveButton();
				AssertEquals("Precondition: Should not have saved due to validation error", ContinueWithSave.No, saveResult);

				CombineAssertions("Mandatory ContactItemStrips should still have a datasource", () =>
				{
					foreach (var itemStrip in mandatoryEmailItemStrips.Union(mandatoryPhoneItemStrips))
					{
						AssertNotNull(itemStrip.CurrentDataItem);
					}
				});
			}
		}

		public void TestAddAndDeleteMandatoryContactItems_OnCurrentDataItemChanged()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var emailItem2a = contact2.EmailContactItems[0];
			emailItem2a.OI_Description = EmailContactItemDescriptionList.Codes.Main;
			emailItem2a.OI_Address = "test@cargowise.test";
			var emailItem2 = contact2.EmailContactItems.AddNew();
			emailItem2.OI_Description = EmailContactItemDescriptionList.Codes.Other;
			emailItem2.OI_Address = "richard@cargowise.com";
			var phoneItem2 = contact2.PhoneContactItems.AddNew();
			phoneItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			phoneItem2.OI_Address = "02 98765432";
			var phoneItem2b = contact2.PhoneContactItems.AddNew();
			phoneItem2b.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile2;
			phoneItem2b.OI_Address = "04 12345678";

			Factory.Save();

			using (var form = new ContactItemsListControlFormForTest(contact))
			{
				form.Show();
				{
					AssertArrayEqualsByElements("Should have added mandatory item strips",
						new[] { "[MAI] [" + contact.EmailContactItems[0].OI_Address + "]" },
						form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.OfType<ContactItemStrip>()
							.OrderByDescending(form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.GetChildIndex)
							.Select(itemStrip => itemStrip.CurrentDataItem)
							.Select(item => string.Format("[{0}] [{1}]", item.OI_Description, item.OI_Address))
							.ToArray());

					AssertArrayEqualsByElements("Should have added mandatory item strips",
						new[]
						{
							"[WRK1] []",
							"[MOB1] []",
							"[HOM] []",
						},
						form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.OfType<ContactItemStrip>()
							.OrderByDescending(form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.GetChildIndex)
							.Select(itemStrip => itemStrip.CurrentDataItem)
							.Select(item => string.Format("[{0}] [{1}]", item.OI_Description, item.OI_Address))
							.ToArray());
				}
				AssertEquals("Adding empty item strip should not set HasChanges", false, contact.HasChanges);

				form.SetDataBinding(contact2, ".");
				{
					AssertArrayEqualsByElements("Should have loaded ContactItemStrips for new dataitem, and added mandatory item strips",
						new[]
						{
							"[MAI] [test@cargowise.test]",
							"[OTH] [richard@cargowise.com]"
						},
						form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.OfType<ContactItemStrip>()
							.OrderByDescending(form.ContactItemsListControl.EmailItemsPanel_Exposed.Controls.GetChildIndex)
							.Select(itemStrip => itemStrip.CurrentDataItem)
							.Select(item => string.Format("[{0}] [{1}]", item.OI_Description, item.OI_Address))
							.ToArray());

					AssertArrayEqualsByElements("Should have loaded ContactItemStrips for new dataitem, and added mandatory item strips",
						new[]
						{
							"[WRK1] []",
							"[MOB1] [02 98765432]",
							"[HOM] []",
							"[MOB2] [04 12345678]",
						},
						form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.OfType<ContactItemStrip>()
							.OrderByDescending(form.ContactItemsListControl.PhoneItemsPanel_Exposed.Controls.GetChildIndex)
							.Select(itemStrip => itemStrip.CurrentDataItem)
							.Select(item => string.Format("[{0}] [{1}]", item.OI_Description, item.OI_Address))
							.ToArray());
				}
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			return new ContactItemsListControlFormForTest(contact);
		}

		public class ContactItemsListControlForTesting : ContactItemsListControl
		{
			public ZPanel EmailItemsPanel_Exposed
			{
				get { return EmailItemsPanel; }
			}

			public ZPanel PhoneItemsPanel_Exposed
			{
				get { return PhoneItemsPanel; }
			}

			public ZButton AddEmailItemButton_Exposed
			{
				get { return AddEmailItemButton; }
			}

			public ZButton AddPhoneItemButton_Exposed
			{
				get { return AddPhoneItemButton; }
			}

			public ZLabel DeliveryStatusLabel_Exposed
			{
				get { return DeliveryStatusLabel; }
			}

			public ZImageButton DeliveryStatusButton_Exposed
			{
				get { return DeliveryStatusButton; }
			}
		}

		public class ContactItemsListControlFormForTest : ZForm
		{
			public ContactItemsListControlFormForTest(OrgContact contact)
				: base(contact)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				ContactItemsListControl = new ContactItemsListControlForTesting();
				Controls.Add(ContactItemsListControl);
				BindingSource.SetBindingMember(ContactItemsListControl, ".");
				CaptionRenderingEnabled = true;
			}

			public ContactItemsListControlForTesting ContactItemsListControl;
		}

		#endregion
	}
}
