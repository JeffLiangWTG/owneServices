using System;
using CargoWise.EntityFramework.Testing;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	public class CycleCountProductFactTest : TestCaseWithFactory
	{
		public void TestNullCode_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountProductFact(Guid.NewGuid(), null, "CAT1", "COM1", "ABC1", 1m, "CNY"));
		}

		public void TestWhiteSpaceCode_Throws()
		{
			AssertExceptionThrown<ArgumentException>(() => new CycleCountProductFact(Guid.NewGuid(), " ", "CAT1", "COM1", "ABC1", 1m, "CNY"));
		}

		public void TestNullCategoryCode_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountProductFact(Guid.NewGuid(), "COD", null, "COM1", "ABC1", 1m, "CNY"));
		}

		public void TestNullCommodityCode_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountProductFact(Guid.NewGuid(), "COD", "CAT1", null, "ABC1", 1m, "CNY"));
		}

		public void TestNullABCCategoryCode_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountProductFact(Guid.NewGuid(), "COD", "CAT1", "COM1", null, 1m, "CNY"));
		}

		public void TestNullUnitPriceCurrency_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountProductFact(Guid.NewGuid(), "COD", "CAT1", "COM1", "ABC1", 1m, null));
		}

		public void TestFields()
		{
			var pk = Guid.NewGuid();
			var fact = new CycleCountProductFact(pk, "COD", "CAT1", "COM1", "ABC1", 1m, "CNY");

			AssertEquals(nameof(CycleCountProductFact.PK), pk, fact.PK);
			AssertEquals(nameof(ICycleCountProductFact.PK), pk, ((ICycleCountProductFact)fact).PK);
			AssertEquals(nameof(CycleCountProductFact.Code), "COD", fact.Code);
			AssertEquals(nameof(ICycleCountProductFact.Code), "COD", ((ICycleCountProductFact)fact).Code);
			AssertEquals(nameof(CycleCountProductFact.CategoryCode), "CAT1", fact.CategoryCode);
			AssertEquals(nameof(ICycleCountProductFact.CategoryCode), "CAT1", ((ICycleCountProductFact)fact).CategoryCode);
			AssertEquals(nameof(CycleCountProductFact.CommodityCode), "COM1", fact.CommodityCode);
			AssertEquals(nameof(ICycleCountProductFact.CommodityCode), "COM1", ((ICycleCountProductFact)fact).CommodityCode);
			AssertEquals(nameof(CycleCountProductFact.ABCCategoryCode), "ABC1", fact.ABCCategoryCode);
			AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "ABC1", ((ICycleCountProductFact)fact).ABCCategoryCode);
			AssertEquals(nameof(CycleCountProductFact.UnitPrice.Value), 1m, fact.UnitPrice.Value);
			AssertEquals(nameof(ICycleCountProductFact.UnitPrice.Value), 1m, ((ICycleCountProductFact)fact).UnitPrice.Value);
			AssertEquals(nameof(CycleCountProductFact.UnitPrice.Unit), "CNY", fact.UnitPrice.Unit);
			AssertEquals(nameof(ICycleCountProductFact.UnitPrice.Unit), "CNY", ((ICycleCountProductFact)fact).UnitPrice.Unit);
		}
	}
}
