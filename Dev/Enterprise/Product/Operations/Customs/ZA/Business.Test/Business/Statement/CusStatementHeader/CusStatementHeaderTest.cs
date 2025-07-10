using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusStatementHeader))]
	sealed class CusStatementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deleting object should have an exception/error because of a trigger.", true);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusStatementHeader>();
	}

	[TestedType(typeof(CusStatementHeader.Loader))]
	sealed class CusStatementHeaderLoaderTest : LoaderTestCase
	{
		public void TestLoadCusStatementHeader()
		{
			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_AccountNo = "12345678";
			header1.B2_ProcessPort = "ABC";
			header1.B2_EntryFilerCode = "87654321";
			header1.B2_DueDate = new ZDateTime(2011, 09, 01);
			header1.B2_ProcessDate = new ZDateTime(2011, 08, 01);
			header1.B2_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertEquals(header1.PK, loader.Load("12345678", "ABC", "87654321", new ZDateTime(2011, 08, 01), new ZDateTime(2011, 09, 01)).PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new CusStatementHeader.Loader(Factory);
		}
		CusStatementHeader.Loader loader;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusStatementHeader.Loader(Factory);
		}
	}
}
