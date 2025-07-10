using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountLocationVariance))]
	public class WhsCycleCountLocationVarianceTest : WhsBusinessObjectTestCase
	{
		#region TestWhsProduct

		public void TestWhsProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var variance = Factory.New<WhsCycleCountLocationVariance>();

			AssertNull("Precondition: Product is null", variance.Product);
			AssertNull("WhsProduct is null", variance.WhsProduct);

			variance.WCC_OP_Product = data.Part1.PK;
			AssertNotNull("WhsProduct should not be null", variance.WhsProduct);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var cycleCount =
				Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation, CycleCountGranularity.Codes.PalletCount, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(2), "AAA");
			var cycleCountVariance =
				Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, true, 5);
			cycleCountVariance.WCC_IsCountingPalletsOnly = true;

			return cycleCountVariance;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		#endregion

		#region TestWCC_PalletID_MaxLength

		public void TestWCC_PalletID_MaxLength()
		{
			var whsCycleCountLocationVariance = Factory.New<WhsCycleCountLocationVariance>();
			AssertNoExceptionThrown(() =>
				whsCycleCountLocationVariance.WCC_PalletID = "123456789012345678901234567890");
		}

		#endregion

		#region TestWCC_PartAttrib1_MaxLength

		public void TestWCC_PartAttrib1_MaxLength()
		{
			var whsCycleCountLocationVariance = Factory.New<WhsCycleCountLocationVariance>();
			AssertNoExceptionThrown(() =>
				whsCycleCountLocationVariance.WCC_PartAttrib1 = "".PadLeft(WhsCycleCountLocationVarianceSchema.WCC_PartAttrib1.MaxLength, 'A'));
		}

		#endregion

		#region TestWCC_PartAttrib1_Exceed_MaxLength

		public void TestWCC_PartAttrib1_Exceed_MaxLength()
		{
			var whsCycleCountLocationVariance = Factory.New<WhsCycleCountLocationVariance>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					whsCycleCountLocationVariance.WCC_PartAttrib1 = ZString.Replicate('A', WhsCycleCountLocationVarianceSchema.WCC_PartAttrib1.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWCC_PartAttrib2_MaxLength

		public void TestWCC_PartAttrib2_MaxLength()
		{
			var whsCycleCountLocationVariance = Factory.New<WhsCycleCountLocationVariance>();
			AssertNoExceptionThrown(() =>
				whsCycleCountLocationVariance.WCC_PartAttrib2 = "".PadLeft(WhsCycleCountLocationVarianceSchema.WCC_PartAttrib2.MaxLength, 'A'));
		}

		#endregion

		#region TestWCC_PartAttrib2_Exceed_MaxLength

		public void TestWCC_PartAttrib2_Exceed_MaxLength()
		{
			var whsCycleCountLocationVariance = Factory.New<WhsCycleCountLocationVariance>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					whsCycleCountLocationVariance.WCC_PartAttrib2 = ZString.Replicate('A', WhsCycleCountLocationVarianceSchema.WCC_PartAttrib2.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWCC_PartAttrib3_MaxLength

		public void TestWCC_PartAttrib3_MaxLength()
		{
			var whsCycleCountLocationVariance = Factory.New<WhsCycleCountLocationVariance>();
			AssertNoExceptionThrown(() =>
				whsCycleCountLocationVariance.WCC_PartAttrib3 = "".PadLeft(WhsCycleCountLocationVarianceSchema.WCC_PartAttrib3.MaxLength, 'A'));
		}

		#endregion

		#region TestWCC_PartAttrib3_Exceed_MaxLength

		public void TestWCC_PartAttrib3_Exceed_MaxLength()
		{
			var whsCycleCountLocationVariance = Factory.New<WhsCycleCountLocationVariance>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					whsCycleCountLocationVariance.WCC_PartAttrib3 = ZString.Replicate('A', WhsCycleCountLocationVarianceSchema.WCC_PartAttrib3.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion
	}

	#region Triggers_WhsCycleCountLocationVarianceTest class

	public class Triggers_WhsCycleCountLocationVarianceTest : WhsTestCaseWithFactory
	{
		#region TestTG_WhsCycleCountVariance_CannotBeModifiedOrDeleted

		[ExpectNoExceptions]
		public void TestTG_WhsCycleCountVariance_CannotBeModifiedOrDeleted_Update()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithPalletID, DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(10), "AAA");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				data.Org1, data.Part1, 10m, 1m);
			Factory.Save();

			variance.WCC_ExpectedQty =
				15m; // variance records should not be changed once created other than status and related adjustment.
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsCycleCountLocationVariance.PreventModifyOfKeyFieldsTriggerError, true), "Trigger should prevent modify of key fields.");
		}

		[ExpectNoExceptions]
		public void TestTG_WhsCycleCountVariance_CannotBeModifiedOrDeleted_Delete()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithPalletID, DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(10), "AAA");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				data.Org1, data.Part1, 10m, 1m);
			Factory.Save();

			variance.Delete(); // variance records cannot be deleted once created
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsCycleCountLocationVariance.PreventDeleteTriggerError, true), "Trigger should prevent delete variance records.");
		}

		#endregion
	}

	[TestedType(typeof(WhsCycleCountLocationVariance))]
	public class
		Triggers_PreventVariancesWithDifferentStatusTest : DeferrableTriggerTestCase<WhsCycleCountLocationVariance>
	{
		#region TestTG_WhsCycleCountLocationVariance_HasSameStatus

		[ExpectNoExceptions]
		public void TestTG_WhsCycleCountLocationVariance_HasSameStatus_Insert()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithPalletID, DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(10), "AAA");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				data.Org1, data.Part1, 10m, 1m);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
				CycleCountVarianceStatus.Codes.Rejected, data.Org1, data.Part1, 10m, 1m);

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsCycleCountLocationVariance.PreventVariancesWithDifferentStatus, true), "Trigger should prevent inserting variance records each with different status.");
		}

		[ExpectNoExceptions]
		public void TestTG_WhsCycleCountLocationVariance_HasSameStatus_Update()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithPalletID, DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(10), "AAA");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				data.Org1, data.Part1, 10m, 1m);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				data.Org1, data.Part1, 10m, 1m);
			Factory.Save();

			variance1.WCC_Status = CycleCountVarianceStatus.Codes.Rejected;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsCycleCountLocationVariance.PreventVariancesWithDifferentStatus, true), "Trigger should prevent updating variance records to each with different status.");
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}

	#endregion
}
