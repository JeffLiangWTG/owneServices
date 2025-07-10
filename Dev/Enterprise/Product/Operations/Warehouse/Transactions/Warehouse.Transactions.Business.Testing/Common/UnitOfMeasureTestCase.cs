using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class UnitOfMeasureTestCase<T> : WhsTestCaseWithFactory
		where T : UnitOfMeasure, IUnitOfMeasure
	{
		#region TestDecimalPlacesForRounding

		public void TestDecimalPlacesForRounding()
		{
			AssertEquals(nameof(UnitOfMeasure.DecimalPlacesForRounding), ExpectedDecimalPlacesForRounding,
				GetNewMeasure("Measure", "UNT").DecimalPlacesForRounding);
		}

		protected abstract int ExpectedDecimalPlacesForRounding { get; }

		#endregion

		#region TestGetQuantityFromProduct_DoesNotAcceptNull

		public void TestGetQuantityFromProduct_DoesNotAcceptNull()
		{
			var measure = GetNewMeasure("Measure", "UNT");
			AssertExceptionThrown<ArgumentNullException>(() => measure.GetQuantityFromProduct(null));
		}

		#endregion

		#region TestGetUQFromProduct_DoesNotAcceptNull

		public void TestGetUQFromProduct_DoesNotAcceptNull()
		{
			var measure = GetNewMeasure("Measure", "UNT");
			AssertExceptionThrown<ArgumentNullException>(() => measure.GetUQFromProduct(null));
		}

		#endregion

		#region TestNameAndTotalUQ

		public void TestNameAndTotalUQ()
		{
			var measure = GetNewMeasure("Measure", "UNT");
			AssertEquals(nameof(measure.Name), "Measure", measure.Name);
			AssertEquals(nameof(measure.TotalUQ), "UNT", measure.TotalUQ);

			var dummy = Factory.New<Dummy>();
			AssertEquals(nameof(measure.Name), "TestHumanName",
				GetNewMeasure(dummy.TestPropertyInfoWithHumanReadableName, "UNT").Name);
		}

		class Dummy : DummyBusinessObject
		{
			public Dummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZPropertyInfo TestPropertyInfoWithHumanReadableName => Z0_CodeInfo;
			public override ZPropertyInfo Z0_CodeInfo => GetZPropertyInfo(nameof(Z0_Code), "TestHumanName");
		}

		#endregion

		#region TestConstructorDoesNotTakeInvalidArgs

		public void TestConstructorDoesNotTakeInvalidArgs()
		{
			AssertExceptionThrown(typeof(ArgumentException),
				"Unit of Measure Name is mandatory.\r\nParameter name: name", () => GetNewMeasure("", "UNT"));
			AssertExceptionThrown<ArgumentNullException>(() => GetNewMeasure((ZPropertyInfo)null, "UNT"));
		}

		#endregion

		#region Implementation

		protected abstract T GetNewMeasure(ZString name, ZString totalUQ);
		protected abstract T GetNewMeasure(ZPropertyInfo totalQuantityInfo, ZString totalUQ);

		#endregion
	}
}
