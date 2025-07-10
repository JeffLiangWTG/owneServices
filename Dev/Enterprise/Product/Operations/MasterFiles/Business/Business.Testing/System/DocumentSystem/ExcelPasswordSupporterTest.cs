using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExcelPasswordSupporter))]
	public class ExcelPasswordSupporterTest : TestCaseWithFactory
	{
		public void TestExcelPasswordSupporter()
		{
			var parent = Factory.NewWithValidTestData<GlbStaff>();
			var excelPasswordSupporter = new ExcelPasswordSupporter(parent);
			excelPasswordSupporter.ExcelPasswordForOpening = "123";
			excelPasswordSupporter.ExcelPasswordForModifying = "456";
			Factory.Save();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var parentInDb = factory.Load<GlbStaff>(parent.PK);
			var excelPasswordSupporterReload = new ExcelPasswordSupporter(parentInDb);

			AssertEquals("123", excelPasswordSupporterReload.ExcelPasswordForOpening);
			AssertEquals("456", excelPasswordSupporterReload.ExcelPasswordForModifying);
		}

		public void TestExcelPasswordSupporterLogActionForOrgContact()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_ContactName = "Test contact";
			Factory.Save();

			orgContact.ExcelPasswordForOpening = "123";
			orgContact.ExcelPasswordForModifying = "456";
			Factory.Save();

			orgContact.ExcelPasswordForOpening = "";
			orgContact.ExcelPasswordForModifying = "";
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, orgContact.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EPC");

			var logs = new BusinessObjectFactory() { RefreshEnabled = false }.Load<StmALog>(query);
			AssertEquals(4, logs.Length);
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Open Password was Changed, user = {Env.CurrentUser.PK}").Count());
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Modify Password was Changed, user = {Env.CurrentUser.PK}").Count());
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Open Password was Removed, user = {Env.CurrentUser.PK}").Count());
			AssertEquals(1, logs.Where(x => x.SL_Reference == $"Org Contact = {orgContact.PK} Test contact Excel Modify Password was Removed, user = {Env.CurrentUser.PK}").Count());
		}
	}
}
