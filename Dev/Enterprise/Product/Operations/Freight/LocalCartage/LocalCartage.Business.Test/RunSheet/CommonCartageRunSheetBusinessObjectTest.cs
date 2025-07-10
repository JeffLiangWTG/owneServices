using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageRunSheetBusinessObjectTest : TestCaseWithFactory
	{
		[TestDate(2008, 1, 1)]
		public void TestSetDefaultValues()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("StartTime should be today", new ZDateTime(2008, 1, 1), runSheet.EY_StartTime);
			AssertEquals("StartTime should be today", new ZDateTime(2008, 1, 1, 23, 59, 0), runSheet.EY_EndTime);
		}

		public void TestDefaultDriverFromVehicle()
		{
			GlbStaff david = TestHelper.CreateStaff("DD", "David");
			RefEquipment truck = TestHelper.CreateTruck("Truck");
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("Driver should be empty", String.Empty, runSheet.EY_GS_NKTruckDriver);
			runSheet.EY_RQ_Truck = truck.PK;
			AssertEquals("Driver should be empty, truck doesn't have a default driver", String.Empty, runSheet.EY_GS_NKTruckDriver);
			truck.RQ_GS_NKPreferredDriver = david.GS_Code;
			runSheet.EY_RQ_Truck = ZGuid.Empty;
			runSheet.EY_RQ_Truck = truck.PK;
			AssertEquals("Driver should David", david.GS_Code, runSheet.EY_GS_NKTruckDriver);
			GlbStaff bob = TestHelper.CreateStaff("BB", "Bob");
			RefEquipment bobsTruck = TestHelper.CreateTruck("BobsTruck");
			bobsTruck.RQ_GS_NKPreferredDriver = bob.GS_Code;
			runSheet.EY_RQ_Truck = bobsTruck.PK;
			AssertEquals("Driver should still be David, don't override", david.GS_Code, runSheet.EY_GS_NKTruckDriver);
		}

		CartageTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new CartageTestHelper(Factory));
			}
		}

		CartageTestHelper testHelper;

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
