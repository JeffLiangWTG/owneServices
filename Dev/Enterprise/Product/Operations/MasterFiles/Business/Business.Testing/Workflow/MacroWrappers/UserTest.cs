using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class UserTest : TestCaseWithFactory
	{
		public void TestUser()
		{
			using (var image = new Bitmap(1, 1))
			{
				var today = ZDateTime.Today;
				var glbStaff = Factory.New<GlbStaff>();
				glbStaff.GS_Code = "XYZ";
				glbStaff.GS_FullName = "Russel Dussell";
				glbStaff.GS_WorkPhone = "02 8888 8888";
				glbStaff.GS_EmailAddress = "rus@ty.co";
				glbStaff.GS_FaxNum = "02 9999 9999";
				glbStaff.GS_IsDeveloper = true;
				glbStaff.SignatureImage = image;

				var groupBO = glbStaff.AllGroups.AddNew();
				groupBO.GG_Code = "XYZ";
				groupBO.GG_Desc = "Group XYZ";

				var certificateBO = glbStaff.Certificates.AddNew();
				certificateBO.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.APP;
				certificateBO.XZ_ExpiryOrDueDate = today;

				var user = new User(glbStaff);
				var certificate = user.Certificates.Single();
				var group = user.Groups.Single(x => x.Code == "XYZ");

				CombineAssertions(() =>
				{
					AssertEquals("Code", "XYZ", user.Code);
					AssertEquals("Name", "Russel Dussell", user.Name);
					AssertEquals("Phone", "02 8888 8888", user.Phone);
					AssertEquals("Email", "rus@ty.co", user.Email);
					AssertEquals("Fax", "02 9999 9999", user.Fax);
					AssertEquals("IsDeveloper", true, user.IsDeveloper);
					AssertNotNull("Signature", user.Signature);
					AssertEquals("Certificates.Type.Code", "APP", certificate.Type.Code);
					AssertEquals("Certificates.Type.Description", "Airport Pass", certificate.Type.Description);
					AssertEquals("Certificates.ExpiryDate", today, certificate.ExpiryDate);
					AssertEquals("Group Code", "XYZ", group.Code);
					AssertEquals("Group Description", "Group XYZ", group.Description);
				});
			}
		}

		public void TestNullUser()
		{
			var user = new User(null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Code", user.Code);
				AssertNullOrEmpty("Name", user.Name);
				AssertNullOrEmpty("Phone", user.Phone);
				AssertNullOrEmpty("Email", user.Email);
				AssertNullOrEmpty("Fax", user.Fax);
				AssertEquals("IsDeveloper", false, user.IsDeveloper);
				AssertNull("Signature", user.Signature);
				Assert("Certificates", !user.Certificates.Any());
				Assert("Groups", !user.Groups.Any());
			});
		}
	}
}
