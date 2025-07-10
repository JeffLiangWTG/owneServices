using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementLine.Loader))]
	sealed class CusStatementLineLoaderTest : LoaderTestCase
	{
		public void TestCanDelete()
		{
			var statement = Factory.New<CusStatementHeader>();
			var line = statement.StatementLines.AddNew();

			Assert("CanDelete should return false if new grids are introduced later", !statement.CanDelete);
			Assert("CanDelete should return false if new grids are introduced later", !line.CanDelete);
		}

		public void TestIControllerIDProviderMembers()
		{
			var statement = Factory.New<CusStatementHeader>();
			var line = statement.StatementLines.AddNew();
			IControllerIDProvider provider = line;
			AssertEquals("ControllerID", ControllerIDs.Customs.CustomsStatement, provider.ControllerID);
			AssertEquals("BusinessObjectPK", statement.PK.ToGuid(), provider.BusinessObjectPK);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusStatementLine.Loader(Factory);
		}

		public void TestLoad()
		{
			var header1 = Factory.New<CusStatementHeader>();
			var line1 = header1.StatementLines.AddNew();
			line1.B3_EntryFilerCode = "XXX";
			line1.B3_EntryNum = "123";
			line1.B3_Status = StatementLineStatusList.Codes.Deleted;

			var header2 = Factory.New<CusStatementHeader>();
			var line2 = header2.StatementLines.AddNew();
			line2.B3_EntryFilerCode = "XXY";
			line2.B3_EntryNum = "123";

			var header3 = Factory.New<CusStatementHeader>();
			var line3 = header3.StatementLines.AddNew();
			line3.B3_EntryFilerCode = "XXX";
			line3.B3_EntryNum = "123";

			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var header4 = Factory.New<CusStatementHeader>();
			header4.B2_GC = caCompany.PK;
			var line4 = header4.StatementLines.AddNew();
			line4.B3_EntryFilerCode = "XXZ";
			line4.B3_EntryNum = "123";

			AssertEquals(line3, new CusStatementLine.Loader(Factory).LoadTop1NotDeleted("123", "XXX", GlbCompany.CurrentCompany.PK));
			AssertEquals(line2, new CusStatementLine.Loader(Factory).LoadTop1NotDeleted("123", "XXY", GlbCompany.CurrentCompany.PK));
			AssertNull(new CusStatementLine.Loader(Factory).LoadTop1NotDeleted("123", "XXZ", GlbCompany.CurrentCompany.PK));
		}
	}
}
