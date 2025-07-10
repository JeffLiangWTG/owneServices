using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WebSecurityRightsListReportTest : TestCaseWithFactory
	{
		public void TestWebSecurityReportIsUpToDate()
		{
			Guid orgPK = Guid.NewGuid();
			const string orgCode = "TESTORG";

			string createOrgSQL = string.Format(@"
insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) values ('{0}', '{1}', 'Test Organisation')
insert into dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_WebAccessEnabled, OC_OH) values (NEWID(), 'Test User', 'test.user@cargowise.com', 1, 1, '{0}')",
				orgPK, orgCode);
			TestConnection.ExecuteNonQuery(createOrgSQL);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select * from Report_WebSecurityProfile('{0}', NULL, NULL, '')", orgCode));
			foreach (WebSecurityRight right in new WebSecurityRightsListForReportTest())
			{
				AssertEquals(right.Code, right.IsGrantedByDefault ? "Y" : "N", result.Rows[0][GetOutputColumnName(right.Code)]);
			}

			StringBuilder restrictRightsSQL = new StringBuilder();
			foreach (WebSecurityRight right in new WebSecurityRightsListForReportTest())
			{
				restrictRightsSQL.AppendLine(string.Format("insert into dbo.OrgSecurity (OX_PK, OX_SecurityItemName, OX_OH, OX_Granted) values (NEWID(), '{0}', '{1}', '{2}')",
					right, orgPK.ToString(), right.IsGrantedByDefault ? "0" : "1"));
			}
			TestConnection.ExecuteNonQuery(restrictRightsSQL.ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("select * from Report_WebSecurityProfile('{0}', NULL, NULL, '')", orgCode));
			foreach (WebSecurityRight right in new WebSecurityRightsListForReportTest())
			{
				AssertEquals(right.Description, (!right.IsGrantedByDefault) ? "Y" : "N", result.Rows[0][GetOutputColumnName(right.Code)]);
			}
		}

		static string GetOutputColumnName(string webSecurityRight)
		{
			string[] parts = webSecurityRight.Split(new[] { ' ', '(', ')', '/', '-', '&' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < parts.Length; i++)
			{
				parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
			}

			return string.Join("", parts);
		}

		class WebSecurityRightsListForReportTest : WebSecurityRightsList { }
	}
}
