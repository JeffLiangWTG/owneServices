using System;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class DocketExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestGetVolumeMeasure

		public void TestGetVolumeMeasure()
		{
			var receive = Factory.New<WhsReceive>();
			receive.WD_TotalCubicUnit = "CC";
			var measure = receive.GetVolumeMeasure();
			AssertEquals(nameof(measure.Name), "Volume", measure.Name);
			AssertEquals(nameof(measure.TotalUQ), "CC", measure.TotalUQ);
			AssertType<VolumeMeasure>(measure);
		}

		public void TestGetVolumeMeasure_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => ((WhsDocket)null).GetVolumeMeasure());
		}

		#endregion

		#region TestGetWeightMeasure

		public void TestGetWeightMeasure()
		{
			var receive = Factory.New<WhsReceive>();
			receive.WD_TotalWeightUnit = "G";
			var measure = receive.GetWeightMeasure();
			AssertEquals(nameof(measure.Name), "Weight", measure.Name);
			AssertEquals(nameof(measure.TotalUQ), "G", measure.TotalUQ);
			AssertType<WeightMeasure>(measure);
		}

		public void TestGetWeightMeasure_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => ((WhsDocket)null).GetWeightMeasure());
		}

		#endregion
	}
}
