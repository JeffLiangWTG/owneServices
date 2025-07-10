using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers
{
	public class ContactTest : TestCaseWithFactory
	{
		public void TestSendPasswordInstructionsEmailToContact()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com/Portals");

			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var result = controller.SendPasswordInstructionsEmail(contact.PK.ToGuid());

			AssertEquals(HttpStatusCode.OK, result.StatusCode);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals($"{Env.CurrentCompany.Name} Password Set", sentEmail.Subject);
			AssertEquals("PleaseDoNotReply@wisetechglobal.com", sentEmail.FromAddress);
			AssertContains($"https://glow.com/Portals", sentEmail.Body);
			AssertContains($"resetPassword", sentEmail.Body);
		}

		public void TestSendPasswordInstructionsEmailToContact_FromCurrentUserAddress()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com/Portals");

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@example.com";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var result = controller.SendPasswordInstructionsEmail(contact.PK.ToGuid(), true);

				AssertEquals(HttpStatusCode.OK, result.StatusCode);

				var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals($"{Env.CurrentCompany.Name} Password Set", sentEmail.Subject);
				AssertEquals(Env.CurrentUser.EmailAddress, sentEmail.FromAddress);
				AssertContains($"https://glow.com/Portals", sentEmail.Body);
				AssertContains($"resetPassword", sentEmail.Body);
			}
		}

		public void TestSendPasswordInstructionsEmailToContact_ContactNotExist()
		{
			var result = controller.SendPasswordInstructionsEmail(Guid.NewGuid());
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
			var content = JsonConvert.DeserializeObject<ProblemDetails>(result.Content.ReadAsStringAsync().Result);
			AssertEquals("Title", "Contact doesn't exist", content.Title);
			AssertEquals("Type", ProblemType.MissingUser, content.Type);
			AssertEquals("Status", (int)HttpStatusCode.NotFound, content.Status);
		}

		public void TestSyncPersons()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var originalContactName1 = "contact1";
			var originalContactName2 = "contact2";

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = originalContactName1;
			contact1.OC_Email = "email1@testing.com";
			contact1.OC_Gender = "M";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = originalContactName2;
			contact2.OC_Email = "email2@testing.com";
			contact2.OC_Gender = "M";
			Factory.Save();

			AssertEquals("Precondition", contact1.OC_ContactName, contact1.Person.PER_FullName);
			AssertEquals("Precondition", contact2.OC_ContactName, contact2.Person.PER_FullName);

			var updatedContactName1 = "extra1";
			var updatedContactName2 = "extra2";

			var sqlText1 =
$@"UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName1}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
WHERE {OrgContactSchema.Constants.PK} = @contact1PK;

UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName2}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
WHERE {OrgContactSchema.Constants.PK} = @contact2PK;
";

			using (var cmd = Db.Connection.Command(sqlText1))
			{
				cmd.AddParameter("@contact1PK", SqlDbType.UniqueIdentifier, contact1.PK.ToGuid());
				cmd.AddParameter("@contact2PK", SqlDbType.UniqueIdentifier, contact2.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			contact1.Reload();
			contact2.Reload();
			AssertEquals("Precondition", originalContactName1, contact1.Person.PER_FullName);
			AssertEquals("Precondition", updatedContactName1, contact1.OC_ContactName);
			AssertEquals("Precondition", "M", contact1.Person.PER_Gender);
			AssertEquals("Precondition", "F", contact1.OC_Gender);

			var contactPKs = new Guid[] { contact1.PK.ToGuid(), contact2.PK.ToGuid() };
			var result = controller.SyncPersons(contactPKs);
			AssertEquals(HttpStatusCode.OK, result.StatusCode);

			contact1.Reload();
			contact2.Reload();
			AssertEquals("Should be updated", updatedContactName1, contact1.Person.PER_FullName);
			AssertEquals("Should be updated", updatedContactName2, contact2.Person.PER_FullName);
			AssertEquals("Should be updated", "F", contact1.Person.PER_Gender);
			AssertEquals("Should be updated", "F", contact2.Person.PER_Gender);
		}

		public void TestSyncPersons_ContactsDoNotExist()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var originalContactName1 = "contact1";
			var originalContactName2 = "contact2";

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = originalContactName1;
			contact1.OC_Email = "email1@testing.com";
			contact1.OC_Gender = "M";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = originalContactName2;
			contact2.OC_Email = "email2@testing.com";
			contact2.OC_Gender = "M";
			Factory.Save();

			AssertEquals("Precondition", contact1.OC_ContactName, contact1.Person.PER_FullName);
			AssertEquals("Precondition", contact2.OC_ContactName, contact2.Person.PER_FullName);

			var updatedContactName1 = "extra1";
			var updatedContactName2 = "extra2";

			var sqlText1 =
$@"UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName1}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
WHERE {OrgContactSchema.Constants.PK} = @contact1PK;

UPDATE {OrgContactSchema.Constants.SqlSchemaName}.{OrgContactSchema.Constants.TableName}
SET {OrgContactSchema.Constants.OC_ContactName} = '{updatedContactName2}'
,{OrgContactSchema.Constants.OC_Gender} = 'F'
WHERE {OrgContactSchema.Constants.PK} = @contact2PK;
";

			using (var cmd = Db.Connection.Command(sqlText1))
			{
				cmd.AddParameter("@contact1PK", SqlDbType.UniqueIdentifier, contact1.PK.ToGuid());
				cmd.AddParameter("@contact2PK", SqlDbType.UniqueIdentifier, contact2.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			contact1.Reload();
			contact2.Reload();
			AssertEquals("Precondition", originalContactName1, contact1.Person.PER_FullName);
			AssertEquals("Precondition", updatedContactName1, contact1.OC_ContactName);
			AssertEquals("Precondition", "M", contact1.Person.PER_Gender);
			AssertEquals("Precondition", "F", contact1.OC_Gender);

			var missingContactPK1 = Guid.NewGuid();
			var missingContactPK2 = Guid.NewGuid();
			var contactPKs = new Guid[] { contact1.PK.ToGuid(), contact2.PK.ToGuid(), missingContactPK1, missingContactPK2 };
			var result = controller.SyncPersons(contactPKs);
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);

			contact1.Reload();
			contact2.Reload();
			AssertEquals("Should be updated", updatedContactName1, contact1.Person.PER_FullName);
			AssertEquals("Should be updated", updatedContactName2, contact2.Person.PER_FullName);
			AssertEquals("Should be updated", "F", contact1.Person.PER_Gender);
			AssertEquals("Should be updated", "F", contact2.Person.PER_Gender);

			AssertEquals("Should report missing contacts", "Missing contact(s) for person sync", ErrorReporter.LastKeyReported);
			AssertEquals("Should report missing contacts", $"The following contact(s) are missing: {missingContactPK1},{missingContactPK2}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		protected override void SetUp()
		{
			base.SetUp();

			controller = new ContactController();
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		protected override void TearDown()
		{
			base.TearDown();

			controller?.Dispose();
		}

		ContactController controller;
	}
}
