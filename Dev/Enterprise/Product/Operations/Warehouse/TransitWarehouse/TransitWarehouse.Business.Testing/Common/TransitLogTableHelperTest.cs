using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class TransitLogTableHelperTest<BOType, ColumnEnum> : TestCaseWithFactory where ColumnEnum : struct
	{
		public abstract void TestGetColumns();

		public void TestGetColumn_GivenNullEntity_ReturnsEmptyStringForRow()
		{
			var tableHelper = GetTableHelper();
			var ids = Enum.GetValues(typeof(ColumnEnum));
			foreach (ColumnEnum id in ids)
			{
				var column = tableHelper.GetColumn(new BOType[] { default(BOType) }, id);
				AssertContainsExactElementsInExactOrder(new ZString[] { ZString.Empty }, column.Values);
			}
		}

		public void TestGetColumn_GivenNoEntities_ReturnsEmptyListForRows()
		{
			var tableHelper = GetTableHelper();
			var ids = Enum.GetValues(typeof(ColumnEnum));
			foreach (ColumnEnum id in ids)
			{
				var column = tableHelper.GetColumn(Array.Empty<BOType>(), id);
				AssertContainsExactElementsInExactOrder(Array.Empty<ZString>(), column.Values);
			}
		}

		public virtual void TestGetTable_Sorted()
		{
			var helper = GetTableHelper();

			var (bo1, id1) = CreateTestBO(1);
			var (bo2, id2) = CreateTestBO(2);
			var (bo3, id3) = CreateTestBO(3);
			var (bo4, id4) = CreateTestBO(4);

			Factory.Save();

			var packageStates = new BOType[] { bo2, bo1, bo4, bo3 };
			var result = helper.GetTable(null, packageStates, DefaultColumn);

			//		Columns as passed in:
			//		2
			//		1
			//		4
			//		3

			var expected = new ZString(
$@"
{DefaultColumnTitle}
{id1}
{id2}
{id3}
{id4}");
			AssertEquals(expected, result);
		}

		public void TestGetTable_Formatting()
		{
			var helper = GetTableHelper();

			var (bo1, id1) = CreateTestBO(1);
			var (bo2, id2) = CreateTestBO(2);

			Factory.Save();

			var log = new ZStringBuilder();
			var bos = new BOType[] { bo1, bo2 };
			log.AppendLine("First Log.");
			log.AppendLine(helper.GetTable("Here are some data displayed in a table:", bos, DefaultColumn));
			log.AppendLine(helper.GetTable(null, bos, DefaultColumn));
			log.AppendLine("Log between tables.");
			var singleLogWithMultipleTables =
ZString.Format(@"Multiple tables in a single log.
{0}
{1}",
	helper.GetTable("Table1:", bos, DefaultColumn), helper.GetTable("Table2:", bos, DefaultColumn));
			log.AppendLine(singleLogWithMultipleTables);

			log.AppendLine(helper.GetTable("Final log:", bos, DefaultColumn));

			var expected = new ZString(
$@"First Log.
Here are some data displayed in a table:
{DefaultColumnTitle}
{id1}
{id2}

{DefaultColumnTitle}
{id1}
{id2}
Log between tables.
Multiple tables in a single log.
Table1:
{DefaultColumnTitle}
{id1}
{id2}
Table2:
{DefaultColumnTitle}
{id1}
{id2}
Final log:
{DefaultColumnTitle}
{id1}
{id2}
");
			AssertEquals(expected, log.ToString());
		}

		public void TestGetTable_GivenTooManyRows_TruncatesTable()
		{
			var helper = GetTableHelper();
			helper.MaximumRows = 10;
			List<BOType> bos = new List<BOType>();
			var ids = new List<ZString>();
			for (var n = 1; n < helper.MaximumRows + 2; n++)
			{
				var (bo, id) = CreateTestBO(n);
				ids.Add(id);
				bos.Add(bo);
			}

			Factory.Save();

			var expected = new ZString(
				$@"
{DefaultColumnTitle}
{string.Join("\r\n", ids.Take(helper.MaximumRows))}
<Displaying top 10 of 11 records>");
			var result = helper.GetTable(null, bos, DefaultColumn);
			AssertEquals(expected, result);
		}

		public void TestGetTable_GivenNoColumns_ReturnsEmptyString()
		{
			var helper = GetTableHelper();
			BOType[] nullArray = null;
			AssertEquals(ZString.Empty, helper.GetTable("", nullArray));
			AssertEquals(ZString.Empty, helper.GetTable("", Array.Empty<BOType>()));

			var businessObject = CreateTestBO(1).BusinessObject;

			Factory.Save();

			AssertEquals(ZString.Empty, helper.GetTable(null, new BOType[] { businessObject }));
		}

		public void TestMinimumColumnWidth()
		{
			var helper = GetTableHelper();
			AssertEquals(ExpectedMinimumColumnWidth, helper.MinimumColumnWidth);
		}

		protected virtual int ExpectedMinimumColumnWidth { get; set; } = 15;

		protected abstract TransitLogTableHelper<BOType, ColumnEnum> GetTableHelper();
		protected abstract (BOType BusinessObject, string expectedDisplayId) CreateTestBO(int id);

		protected ZString DefaultColumnTitle => GetDefaultTitle();
		protected virtual ZString GetDefaultTitle() => DefaultColumn.ToString();
		protected ColumnEnum DefaultColumn => GetDefaultColumn();
		protected abstract ColumnEnum GetDefaultColumn();

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		protected WhsTransitTestHelper helper;
	}
}
