using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CopyRecipientsEmailCollectionForm))]
	sealed class CopyRecipientsEmailCollectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			orgDocument.OD_CarbonCopyRecipientsAsString = "test@test.com";
			orgDocument.OD_OC = orgContact.PK;
			Factory.Save();

			return new CopyRecipientsEmailCollectionForm(orgDocument.CarbonCopyRecipients, OrgDocumentCopyRecipientSchema.ODR_EmailAddress.Name);
		}

		[RequiresSTA]
		public void TestCorrectColumns()
		{
			using (var testForm = (CopyRecipientsEmailCollectionForm)GetFormToBash())
			{
				AssertEquals(1, testForm.Grid_CopyRecipients.Columns.Count);
				AssertEquals(OrgDocumentCopyRecipientSchema.ODR_EmailAddress.Name, testForm.Grid_CopyRecipients.Columns[0].ColumnName);
			}
		}
	}
}
