using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class UnitOfMeasureConverterTest : WhsTestCaseWithFactory
	{
		#region TestGetQuantityFromLine_DoesNotAcceptInvalidArgs

		public void TestGetQuantityFromLine_DoesNotAcceptInvalidArgs()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				UnitOfMeasureConverter.GetQuantityFromLine(null, new VolumeMeasure("Volume", "M3"),
					new ProductAndQuantity()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UnitOfMeasureConverter.GetQuantityFromLine(Factory.New<WhsReceive>(), null, new ProductAndQuantity()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UnitOfMeasureConverter.GetQuantityFromLine(Factory.New<WhsReceive>(), new VolumeMeasure("Volume", "M3"),
					null));
		}

		#endregion

		#region TestGetQuantityFromLine_Volume

		public void TestGetQuantityFromLine_Volume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line = new ProductAndQuantity { ProductPK = data.Part1.PK, Quantity = 5m };

			data.Part1.OP_Cubic = 1m;
			data.Part1.OP_CubicUQ = "M3";
			var volumeMeasure = new VolumeMeasure("Volume", "CC");
			AssertEquals("Volume should be converted to the total UQ.", 5000000.000m,
				UnitOfMeasureConverter.GetQuantityFromLine(receive, volumeMeasure, line));
		}

		#endregion

		#region TestGetQuantityFromLine_Weight

		public void TestGetQuantityFromLine_Weight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line = new ProductAndQuantity { ProductPK = data.Part1.PK, Quantity = 5m };

			data.Part1.OP_Weight = 10m;
			data.Part1.OP_WeightUQ = "KG";
			var volumeMeasure = new WeightMeasure("Weight", "LB");
			AssertEquals("Weight should be converted to the total UQ.", 110.23m,
				UnitOfMeasureConverter.GetQuantityFromLine(receive, volumeMeasure, line));
		}

		#endregion

		#region TestGetTotalQuantityFromLines_DoesNotAcceptInvalidArgs

		public void TestGetTotalQuantityFromLines_DoesNotAcceptInvalidArgs()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				UnitOfMeasureConverter.GetTotalQuantityFromLines(null, new VolumeMeasure("Volume", "M3"),
					Enumerable.Empty<ILineWithProductAndQuantity>()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UnitOfMeasureConverter.GetTotalQuantityFromLines(Factory.New<WhsReceive>(), null,
					Enumerable.Empty<ILineWithProductAndQuantity>()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UnitOfMeasureConverter.GetTotalQuantityFromLines(Factory.New<WhsReceive>(),
					new VolumeMeasure("Volume", "M3"), null));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UnitOfMeasureConverter.GetTotalQuantityFromLines(Factory.New<WhsReceive>(),
					new VolumeMeasure("Volume", "M3"), new ILineWithProductAndQuantity[] { null }));
		}

		#endregion

		#region TestGetTotalQuantityFromLines_InvalidProductsAreIgnored

		public void TestGetTotalQuantityFromLines_InvalidProductsAreIgnored()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line1 = new ProductAndQuantity { ProductPK = data.Part1.PK, Quantity = 5m };
			var line2 = new ProductAndQuantity { Quantity = 8m };
			var line3 = new ProductAndQuantity { ProductPK = ZGuid.NewZGuid(), Quantity = 7m };
			var line4 = new ProductAndQuantity { Quantity = 1m };
			var line5 = new ProductAndQuantity { ProductPK = line3.ProductPK, Quantity = 1m };

			data.Part1.OP_Cubic = 10000m;
			data.Part1.OP_CubicUQ = "CC";

			var volumeMeasure = new VolumeMeasure("Volume", "CC");
			AssertEquals("Volume should ignore invalid Products.", 50000.000m,
				UnitOfMeasureConverter.GetTotalQuantityFromLines(receive, volumeMeasure,
					new[] { line1, line2, line3, line4, line5 }));
		}

		#endregion

		#region TestGetTotalQuantityFromLines_OverflowIsHandled

		public void TestGetTotalQuantityFromLines_OverflowIsHandled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line = new ProductAndQuantity { ProductPK = data.Part1.PK, Quantity = decimal.MaxValue };

			data.Part1.OP_Cubic = 2m;
			data.Part1.OP_CubicUQ = "CC";

			var volumeMeasure = new VolumeMeasure("Crazy", "CC");
			AssertEquals("Volume should not be calculated when overflowed.", 0m,
				UnitOfMeasureConverter.GetTotalQuantityFromLines(receive, volumeMeasure, new[] { line }));
			AssertHasRowError(receive,
				"Attempt to overflow capacity of Crazy. Please validate your setup and restart the process.");
		}

		#endregion

		#region TestGetTotalQuantityFromLines_Rounding

		public void TestGetTotalQuantityFromLines_Rounding()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line = new ProductAndQuantity { ProductPK = data.Part1.PK, Quantity = 1m };

			data.Part1.OP_Cubic = 1m;
			data.Part1.OP_CubicUQ = "M3";
			var volumeMeasure = new VolumeMeasure("Volume", "CF");
			AssertEquals("Original unrounded Conversion should be different to the rounded version.", 35.3144754035m,
				data.Part1.UnitConverter.Convert(line.Quantity, "M3", "CF"));
			AssertEquals("Volume should be converted to the total UQ.", 35.314m,
				UnitOfMeasureConverter.GetQuantityFromLine(receive, volumeMeasure, line));
		}

		#endregion

		#region TestGetTotalQuantityFromLines_Volume

		public void TestGetTotalQuantityFromLines_Volume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line1 = new ProductAndQuantity { ProductPK = data.Part1.PK, Quantity = 5m };
			var line2 = new ProductAndQuantity { ProductPK = data.Part2.PK, Quantity = 8m };
			var line3 = new ProductAndQuantity { ProductPK = data.Part2.PK, Quantity = 7m };

			data.Part1.OP_Cubic = 10000m;
			data.Part1.OP_CubicUQ = "CC";
			data.Part2.OP_Cubic = 1m;
			data.Part2.OP_CubicUQ = "M3";

			var volumeMeasure = new VolumeMeasure("Volume", "CC");
			AssertEquals("Volume should be added up and converted to the total UQ.", 15050000.000m,
				UnitOfMeasureConverter.GetTotalQuantityFromLines(receive, volumeMeasure,
					new[] { line1, line2, line3 }));
		}

		#endregion

		#region TestGetTotalQuantityFromLines_Weight

		public void TestGetTotalQuantityFromLines_Weight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			var line1 = new ProductAndQuantity { ProductPK = data.Part1.PK, Quantity = 5m };
			var line2 = new ProductAndQuantity { ProductPK = data.Part2.PK, Quantity = 8m };
			var line3 = new ProductAndQuantity { ProductPK = data.Part2.PK, Quantity = 7m };

			data.Part1.OP_Weight = 100m;
			data.Part1.OP_WeightUQ = "LB";
			data.Part2.OP_Weight = 10m;
			data.Part2.OP_WeightUQ = "KG";

			var weightMeasure = new WeightMeasure("Weight", "LB");
			AssertEquals("Weight should be added up and converted to the total UQ.", 830.69m,
				UnitOfMeasureConverter.GetTotalQuantityFromLines(receive, weightMeasure,
					new[] { line1, line2, line3 }));
		}

		#endregion

		#region Implementation

		class ProductAndQuantity : ILineWithProductAndQuantity
		{
			public ZDecimal Quantity { get; set; }

			public ZGuid ProductPK { get; set; }
		}

		#endregion
	}
}
