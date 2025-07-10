using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CopyRecipientsColumnStyleTest : TestCaseWithFactory
	{
		public void TestEditControl()
		{
			using (var columnStyle = new CopyRecipientsColumnStyle<OrgDocumentCopyRecipient, OrgDocumentCopyRecipientCollection, OrgDocument>(new CopyRecipientsColumnStyleInfo<OrgDocumentCopyRecipient, OrgDocumentCopyRecipientCollection, OrgDocument>()))
			{
				AssertEquals(typeof(CopyRecipientsFindBox<OrgDocumentCopyRecipient, OrgDocument>), columnStyle.EditControl.GetType());
			}
			using (var columnStyle = new NonPersistentCopyRecipientsColumnStyle<DocDeliveryContact>(new NonPersistentCopyRecipientsColumnStyleInfo<DocDeliveryContact>()))
			{
				AssertEquals(typeof(NonPersistentCopyRecipientsFindBox), columnStyle.EditControl.GetType());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestRegisterAddEmailAddressHotKey_CopyRecipient()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "blah@blah.org";
			Factory.Save();

			Env.SetUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK));

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var dummy = contact.Documents.AddNew();
			dummy.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			dummy.OD_CarbonCopyRecipientsAsString = "another@blah.org";

			using (var zForm = new ZForm(contact))
			using (var testGrid = new ZGrid())
			{
				var columnInfo = new CopyRecipientsColumnStyleInfo<OrgDocumentCopyRecipient, OrgDocumentCopyRecipientCollection, OrgDocument>()
				{
					ColumnName = "OD_CarbonCopyRecipientsAsString",
					GetCopyRecipients = (doc) => doc.CarbonCopyRecipients
				};

				testGrid.Columns.Add(columnInfo);
				testGrid.RefreshTableStyles();

				var columnStyle = (ZTextBoxColumnStyle)testGrid.Columns[columnInfo.ColumnName].ColumnStyle;

				testGrid.BindTo = "Documents";
				zForm.Controls.Add(testGrid);
				zForm.Show();

				testGrid.BeginEdit(columnStyle, 0);
				columnStyle.Hotkeys.ProcessCmdKey(columnStyle, Keys.Control | Keys.E);

				testGrid.EndEdit(columnStyle, 0, false);

				AssertEquals("blah@blah.org", dummy.OD_CarbonCopyRecipientsAsString);

				var emails = dummy.CarbonCopyRecipients.Select(recipient => recipient.ODR_EmailAddress.ToString()).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "blah@blah.org" }, emails);
			}
		}

		class NonPersistentContactMaster : NonPersistentBusinessObject
		{
			public NonPersistentContactMaster(BusinessObjectFactory factory)
			{
				Recipients = new DocDeliveryContactCollection(factory);
				RegisterEditableChildObject(Recipients);
			}

			public DocDeliveryContactCollection Recipients { get; }
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestRegisterAddEmailAddressHotKey_NonPersistentCopyRecipient()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "blah@blah.org";
			Factory.Save();

			Env.SetUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK));

			var master = new NonPersistentContactMaster(Factory);
			var contact = master.Recipients.AddNew();
			contact.DeliveryMethod = Constants.ContactNotifyModes.Email;

			using (var zForm = new ZForm(master))
			using (var testGrid = new ZGrid())
			{
				var columnInfo = new NonPersistentCopyRecipientsColumnStyleInfo<DocDeliveryContact>()
				{
					ColumnName = "EmailCarbonCopyRecipientsAsString",
					GetCopyRecipients = docDeliveryContact => docDeliveryContact.EmailCarbonCopyRecipients,
				};

				testGrid.Columns.Add(columnInfo);
				testGrid.RefreshTableStyles();

				var columnStyle = (ZTextBoxColumnStyle)testGrid.Columns[columnInfo.ColumnName].ColumnStyle;

				testGrid.BindTo = "Recipients";
				zForm.Controls.Add(testGrid);
				zForm.Show();

				testGrid.BeginEdit(columnStyle, 0);
				columnStyle.Hotkeys.ProcessCmdKey(columnStyle, Keys.Control | Keys.E);

				testGrid.EndEdit(columnStyle, 0, false);

				AssertEquals("blah@blah.org", contact.EmailCarbonCopyRecipientsAsString);
			}
		}
	}
}
