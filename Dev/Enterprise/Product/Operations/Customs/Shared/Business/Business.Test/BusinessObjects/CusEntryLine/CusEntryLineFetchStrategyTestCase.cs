using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class CusEntryLineFetchStrategyTestCase : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			AssertEquals(line, strategy.Line);
		}

		public void TestFetchForView()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			line.CL_CH = entry.PK;
			Factory.Save();
			line.CL_CH = ZGuid.NewZGuid();
			int count = Factory.ActiveTableFetchHints;
			strategy.FetchForView(Array.Empty<TableColumn>());
			AssertEquals("Added 2 fetch hints", count + 2, Factory.ActiveTableFetchHints);
		}

		public void TestFetchForLoad()
		{
			line.CL_CH = ZGuid.NewZGuid();
			int count = Factory.ActiveTableFetchHints;
			strategy.FetchForLoad();
			AssertEquals("Added 1 fetch hint", count + 1, Factory.ActiveTableFetchHints);
		}

		CusEntryLine line;
		CusEntryLineFetchStrategy strategy;
		protected override void SetUp()
		{
			line = Factory.New<CusEntryLine>();
			strategy = new CusEntryLineFetchStrategy(line);
			base.SetUp();
		}
	}
}
