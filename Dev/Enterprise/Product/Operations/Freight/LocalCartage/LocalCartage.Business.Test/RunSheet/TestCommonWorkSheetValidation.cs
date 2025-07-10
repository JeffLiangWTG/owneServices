using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal class TestCommonWorkSheetValidation : BusinessObjectValidationTestCase
	{
		public void TestStartTimeInvalid()
		{
			var truck = Factory.New<RefEquipment>();
			var runSheet = Factory.New<CommonWorkSheet>();
			var now = ZDateTime.Today;
			runSheet.EY_StartTime = now;
			runSheet.EY_EndTime = now.AddDays(1);
			Assert(!runSheet.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet.EY_EndTimeInfo.HasErrors());
			runSheet.EY_StartTime = ZDateTime.Invalid;
			runSheet.EY_RQ_Truck = truck.PK;
			Assert(runSheet.EY_StartTimeInfo.HasErrors());
		}

		public void TestEndTimeInvalid()
		{
			var truck = Factory.New<RefEquipment>();
			var runSheet = Factory.New<CommonWorkSheet>();
			var now = ZDateTime.Today;
			runSheet.EY_StartTime = now;
			runSheet.EY_EndTime = now.AddDays(1);
			Assert(!runSheet.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet.EY_EndTimeInfo.HasErrors());
			runSheet.EY_EndTime = ZDateTime.Invalid;
			runSheet.EY_RQ_Truck = truck.PK;
			Assert(runSheet.EY_EndTimeInfo.HasErrors());
		}

		[TestDate(2010, 2, 1, 15, 6, 18)]
		public void TestCheckTruckWeightVolumeCapacity()
		{
			RefEquipment truck = Factory.New<RefEquipment>();
			truck.RQ_IsVehicle = true;
			truck.RQ_WeightCapacity = 2000;
			truck.RQ_WeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			truck.RQ_CubicCapacity = 5000;
			truck.RQ_CubicUnit = Enterprise.Core.Constants.Volume.Litre;
			CommonBookedCtgMove move1 = Factory.New<CommonBookedCtgMove>();
			CommonBookedCtgMove move2 = Factory.New<CommonBookedCtgMove>();
			CommonBookedCtgMove move3 = Factory.New<CommonBookedCtgMove>();
			CommonBookedCtgMove move4 = Factory.New<CommonBookedCtgMove>();
			CommonBookedCtgMove move5 = Factory.New<CommonBookedCtgMove>();
			move1.EW_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			move2.EW_WeightUQ = Enterprise.Core.Constants.Weight.Tonnes;
			move3.EW_WeightUQ = Enterprise.Core.Constants.Weight.Tonnes;
			move4.EW_WeightUQ = Enterprise.Core.Constants.Weight.Tonnes;
			move5.EW_WeightUQ = Enterprise.Core.Constants.Weight.Tonnes;
			move1.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			move2.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			move3.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			move4.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			move5.EW_VolumeUQ = Enterprise.Core.Constants.Volume.Litre;
			move1.EW_BookedVolume = 100;
			move1.EW_BookedWeight = 100; //100 kilos
			move2.EW_BookedVolume = 300;
			move2.EW_BookedWeight = 2; //2000 kilos
			move3.EW_BookedVolume = 3;
			move3.EW_BookedWeight = 5; //5000 kilos
			move4.EW_BookedVolume = 3;
			move4.EW_BookedWeight = 6; //5000 kilos
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			CommonCartageLeg leg3 = move3.CartageLegs.AddNew();
			CommonCartageLeg leg4 = move4.CartageLegs.AddNew();
			CommonCartageLeg leg5 = move5.CartageLegs.AddNew();
			leg1.JU_SplitDeliverySuffix = "l1";
			leg2.JU_SplitDeliverySuffix = "l2";
			leg3.JU_SplitDeliverySuffix = "l3";
			leg4.JU_SplitDeliverySuffix = "l4";
			leg5.JU_SplitDeliverySuffix = "l5";
			leg1.JU_PlannedPickupTime = ZDateTime.Now;
			leg2.JU_PlannedPickupTime = ZDateTime.Now.AddMinutes(20);
			leg2.JU_EstimatedDeliveryTime = ZDateTime.Now.AddMinutes(40);
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = ZDateTime.Now.AddDays(-1);
			runSheet.EY_EndTime = ZDateTime.Now.AddDays(1);
			runSheet.CartageLegs.Add(leg1);
			runSheet.CartageLegs.Add(leg2);
			runSheet.EY_RQ_Truck = truck.PK;
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 01-Feb-10 15:26:18 and 01-Feb-10 15:46:18, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 100.  The Port Transport Legs in this period are  /l1,/l2");
			////leg1 and leg2 exceed both the volume and weight and that should be reported twice, even though that means repeating the warnings for the same legs
			move1.EW_BookedVolume = truck.RQ_CubicCapacity + 20;
			move2.EW_BookedVolume = 0;
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 01-Feb-10 15:26:18 and 01-Feb-10 15:46:18, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 100.  The Port Transport Legs in this period are  /l1,/l2\r\nBetween 01-Feb-10 15:26:18 and 01-Feb-10 15:46:18, the vehicle's Cubic Capacity  of 5000 L has been exceeded by 20.  The Port Transport Legs in this period are  /l1,/l2");
			leg1.JU_EstimatedDeliveryTime = ZDateTime.Now.AddMinutes(30);
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 01-Feb-10 15:26:18 and 01-Feb-10 15:36:18, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 100.  The Port Transport Legs in this period are  /l1,/l2\r\nBetween 01-Feb-10 15:26:18 and 01-Feb-10 15:36:18, the vehicle's Cubic Capacity  of 5000 L has been exceeded by 20.  The Port Transport Legs in this period are  /l1,/l2");
			leg1.JU_EstimatedDeliveryTime = ZDateTime.Now.AddMinutes(33);
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 01-Feb-10 15:26:18 and 01-Feb-10 15:39:18, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 100.  The Port Transport Legs in this period are  /l1,/l2\r\nBetween 01-Feb-10 15:26:18 and 01-Feb-10 15:39:18, the vehicle's Cubic Capacity  of 5000 L has been exceeded by 20.  The Port Transport Legs in this period are  /l1,/l2");
			move1.EW_BookedVolume = truck.RQ_CubicCapacity - 20;
			move2.EW_BookedVolume = 30;
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 01-Feb-10 15:26:18 and 01-Feb-10 15:39:18, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 100.  The Port Transport Legs in this period are  /l1,/l2\r\nBetween 01-Feb-10 15:26:18 and 01-Feb-10 15:39:18, the vehicle's Cubic Capacity  of 5000 L has been exceeded by 10.  The Port Transport Legs in this period are  /l1,/l2");
			leg1.JU_EstimatedDeliveryTime = ZDateTime.Now;
			leg3.JU_PlannedPickupTime = ZDateTime.Now.AddHours(2);
			leg3.JU_EstimatedDeliveryTime = ZDateTime.Now.AddHours(5);
			runSheet.CartageLegs.Add(leg3);
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 01-Feb-10 17:06:18 and 01-Feb-10 20:06:18, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 3000.  The Port Transport Leg in this period is  /l3");
			leg1.JU_PlannedPickupTime = ZDateTime.Now;
			leg1.JU_EstimatedDeliveryTime = ZDateTime.Now.AddHours(6);
			leg2.JU_PlannedPickupTime = ZDateTime.Now.AddMinutes(30);
			leg2.JU_EstimatedDeliveryTime = leg1.JU_EstimatedDeliveryTime;
			leg3.JU_PlannedPickupTime = ZDateTime.Now.AddMinutes(45);
			leg3.JU_EstimatedDeliveryTime = ZDateTime.Now.AddHours(2);
			truck.RQ_WeightCapacity = 101; //leg1's units are 100 and 100, 2nd and 3rd leg will exceed
			truck.RQ_CubicCapacity = 101;
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 01-Feb-10 15:51:18 and 01-Feb-10 17:06:18, the vehicle's Weight Capacity  of 101 KG has been exceeded by 6999.  The Port Transport Legs in this period are  /l1,/l2,/l3\r\nBetween 01-Feb-10 15:51:18 and 01-Feb-10 17:06:18, the vehicle's Cubic Capacity  of 101 L has been exceeded by 4912.  The Port Transport Legs in this period are  /l1,/l2,/l3");
			//time		10am		1pm			2pm		3pm		4pm		5pm
			//leg1		-------------------------
			//leg2			---------------------						
			//leg3					----------------------------
			//leg4										------------
			//leg5														---
			//leg1 + leg2 + leg3 exceed weight & volume capacity between 1pm and 2pm (reported once). Leg2 + Leg3 exceed weight limit between 1pm and 2pm but it's already been reported so shouldn't be repeated.
			//leg3 + leg4 exceed VOlume & weight capacity between 3pm and 4pm reported once, leg4 exceeds VOLume limit by itself, don't report as it was reported before.
			//leg5 exceeds weight & volume should be reported by itself
			//error should be : "leg1 + leg2 + leg3 exceed weight between 1pm and 2pm, leg3 + leg4 exceed colume between 3pm and 4pm, leg5 exceeds weight & volume limit between 5pm and 6pm"
			truck.RQ_CubicCapacity = 5000;
			truck.RQ_WeightCapacity = 2000;
			move1.EW_BookedVolume = 100;
			move1.EW_BookedWeight = 100; //100 kilos
			move2.EW_BookedVolume = 300;
			move2.EW_BookedWeight = 2; //2000 kilos
			move3.EW_BookedVolume = 4900;
			move3.EW_BookedWeight = 2; //5000 kilos
			move4.EW_BookedVolume = 6000;
			move4.EW_BookedWeight = 1;
			move5.EW_BookedVolume = 10000;
			move5.EW_BookedWeight = 10000;
			ZDateTime tenAM = new ZDateTime(2010, 2, 2, 10, 0, 0);
			ZDateTime elevenAM = new ZDateTime(2010, 2, 2, 11, 0, 0);
			ZDateTime onePM = new ZDateTime(2010, 2, 2, 13, 0, 0);
			ZDateTime twoPM = new ZDateTime(2010, 2, 2, 14, 0, 0);
			ZDateTime threePM = new ZDateTime(2010, 2, 2, 15, 0, 0);
			ZDateTime fourPM = new ZDateTime(2010, 2, 2, 16, 0, 0);
			ZDateTime fivePM = new ZDateTime(2010, 2, 2, 17, 0, 0);
			ZDateTime sixPM = new ZDateTime(2010, 2, 2, 18, 0, 0);
			leg1.JU_PlannedPickupTime = tenAM;
			leg1.JU_EstimatedDeliveryTime = twoPM;
			leg2.JU_PlannedPickupTime = elevenAM;
			leg2.JU_EstimatedDeliveryTime = twoPM;
			leg3.JU_PlannedPickupTime = onePM;
			leg3.JU_EstimatedDeliveryTime = fourPM;
			leg4.JU_PlannedPickupTime = threePM;
			leg4.JU_EstimatedDeliveryTime = fourPM;
			leg5.JU_PlannedPickupTime = fivePM;
			leg5.JU_EstimatedDeliveryTime = sixPM;
			runSheet.CartageLegs.Add(leg4);
			runSheet.CartageLegs.Add(leg5);
			runSheet.Validation.ValidateEY_RQ_Truck();
			AssertHasWarning(runSheet.EY_RQ_TruckInfo, "Between 02-Feb-10 13:00:00 and 02-Feb-10 14:00:00, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 2100.  The Port Transport Legs in this period are  /l1,/l2,/l3\r\nBetween 02-Feb-10 13:00:00 and 02-Feb-10 14:00:00, the vehicle's Cubic Capacity  of 5000 L has been exceeded by 300.  The Port Transport Legs in this period are  /l1,/l2,/l3\r\nBetween 02-Feb-10 15:00:00 and 02-Feb-10 16:00:00, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 1000.  The Port Transport Legs in this period are  /l3,/l4\r\nBetween 02-Feb-10 15:00:00 and 02-Feb-10 16:00:00, the vehicle's Cubic Capacity  of 5000 L has been exceeded by 5900.  The Port Transport Legs in this period are  /l3,/l4\r\nBetween 02-Feb-10 17:00:00 and 02-Feb-10 18:00:00, the vehicle's Weight Capacity  of 2000 KG has been exceeded by 9998000.  The Port Transport Leg in this period is  /l5\r\nBetween 02-Feb-10 17:00:00 and 02-Feb-10 18:00:00, the vehicle's Cubic Capacity  of 5000 L has been exceeded by 5000.  The Port Transport Leg in this period is  /l5");
		}

		public void TestCheckStartEndTimeDontMatch()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			runSheet.RunPreSaveValidation();
			Assert(runSheet.EY_StartTimeInfo.HasErrors());
			Assert(runSheet.EY_EndTimeInfo.HasErrors());
			ZDateTime now = ZDateTime.Today;
			runSheet.EY_StartTime = now;
			runSheet.EY_EndTime = now.AddDays(1);
			Assert(!runSheet.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet.EY_EndTimeInfo.HasErrors());
			runSheet.EY_StartTime = now;
			runSheet.EY_EndTime = now;
			runSheet.RunPreSaveValidation();
			Assert(runSheet.EY_StartTimeInfo.HasErrors());
			Assert(runSheet.EY_EndTimeInfo.HasErrors());
		}

		public void TestCheckTruckDoesntOverrlapOtherRunSheets()
		{
			ZDateTime now = ZDateTime.Today;
			RefEquipment bobsTruck = Factory.New<RefEquipment>();
			bobsTruck.RQ_ShortCode = "BobT";
			bobsTruck.RQ_IsVehicle = true;
			RefEquipment billsTruck = Factory.New<RefEquipment>();
			billsTruck.RQ_ShortCode = "BillsT";
			billsTruck.RQ_IsVehicle = true;
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_StartTime = now;
			runSheet1.EY_EndTime = now.AddDays(1);
			runSheet1.RunPreSaveValidation();
			Assert(!runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			runSheet1.EY_RQ_Truck = bobsTruck.PK;
			runSheet1.EY_StartTime = now;
			runSheet1.EY_EndTime = now.AddDays(1);
			//Same Day/Dif Truck
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_RQ_Truck = billsTruck.PK;
			runSheet2.EY_StartTime = now;
			runSheet2.EY_EndTime = now.AddDays(1);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_RQ_TruckInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//Same Day/Truck
			runSheet2.EY_RQ_Truck = bobsTruck.PK;
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//1 Day before
			runSheet2.EY_StartTime = now.AddDays(-1);
			runSheet2.EY_EndTime = now;
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_RQ_TruckInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//1 Day after
			runSheet2.EY_StartTime = now.AddDays(1);
			runSheet2.EY_EndTime = now.AddDays(2);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_RQ_TruckInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//Middle of Day
			runSheet2.EY_StartTime = now.AddHours(5);
			runSheet2.EY_EndTime = now.AddHours(15);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//Overlap Morning
			runSheet2.EY_StartTime = now.AddHours(-5);
			runSheet2.EY_EndTime = now.AddHours(5);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//Overlap Arvo
			runSheet2.EY_StartTime = now.AddHours(5);
			runSheet2.EY_EndTime = now.AddHours(30);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_RQ_TruckInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
		}

		public void TestCheckDriverDoesntOverrlapOtherRunSheets()
		{
			GlbGroup driversGroup = Factory.New<GlbGroup>();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());
			ZDateTime now = ZDateTime.Today;
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_LoginName = "bob";
			bob.GS_Code = "bob";
			bob.Groups.Add(driversGroup);
			GlbStaff bil = Factory.New<GlbStaff>();
			bil.GS_LoginName = "bil";
			bil.GS_Code = "bil";
			bil.Groups.Add(driversGroup);
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_StartTime = now;
			runSheet1.EY_EndTime = now.AddDays(1);
			runSheet1.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			runSheet1.EY_GS_NKTruckDriver = bob.GS_Code;
			runSheet1.EY_StartTime = now;
			runSheet1.EY_EndTime = now.AddDays(1);
			//Same Day/Dif Truck
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_GS_NKTruckDriver = bil.GS_Code;
			runSheet2.EY_StartTime = now;
			runSheet2.EY_EndTime = now.AddDays(1);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//Same Day/Truck
			runSheet2.EY_GS_NKTruckDriver = bob.GS_Code;
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//1 Day before
			runSheet2.EY_StartTime = now.AddDays(-1);
			runSheet2.EY_EndTime = now;
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//1 Day after
			runSheet2.EY_StartTime = now.AddDays(1);
			runSheet2.EY_EndTime = now.AddDays(2);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//Middle of Day
			runSheet2.EY_StartTime = now.AddHours(5);
			runSheet2.EY_EndTime = now.AddHours(15);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//Overlap Morning
			runSheet2.EY_StartTime = now.AddHours(-5);
			runSheet2.EY_EndTime = now.AddHours(5);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//Overlap Arvo
			runSheet2.EY_StartTime = now.AddHours(5);
			runSheet2.EY_EndTime = now.AddHours(30);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
		}

		public void TestCheckDriverAndTruckDoesntOverrlapOtherRunSheets()
		{
			var driversGroup = Factory.New<GlbGroup>();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());
			var now = ZDateTime.Today;
			var bob = Factory.New<GlbStaff>();
			bob.GS_LoginName = "bob";
			bob.GS_Code = "bob";
			bob.Groups.Add(driversGroup);
			var bil = Factory.New<GlbStaff>();
			bil.GS_LoginName = "bil";
			bil.GS_Code = "bil";
			bil.Groups.Add(driversGroup);
			var bobsTruck = Factory.New<RefEquipment>();
			bobsTruck.RQ_ShortCode = "BobT";
			bobsTruck.RQ_IsVehicle = true;
			var billsTruck = Factory.New<RefEquipment>();
			billsTruck.RQ_ShortCode = "BillsT";
			billsTruck.RQ_IsVehicle = true;
			var runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_StartTime = now;
			runSheet1.EY_EndTime = now.AddDays(1);
			runSheet1.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			runSheet1.EY_GS_NKTruckDriver = bob.GS_Code;
			runSheet1.EY_RQ_Truck = bobsTruck.PK;
			runSheet1.EY_StartTime = now;
			runSheet1.EY_EndTime = now.AddDays(1);
			//Same Day/Dif Truck
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_GS_NKTruckDriver = bil.GS_Code;
			runSheet2.EY_RQ_Truck = billsTruck.PK;
			runSheet2.EY_StartTime = now;
			runSheet2.EY_EndTime = now.AddDays(1);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//Same Day/Truck
			runSheet2.EY_GS_NKTruckDriver = bob.GS_Code;
			runSheet2.EY_RQ_Truck = bobsTruck.PK;
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//1 Day before
			runSheet2.EY_StartTime = now.AddDays(-1);
			runSheet2.EY_EndTime = now;
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//1 Day after
			runSheet2.EY_StartTime = now.AddDays(1);
			runSheet2.EY_EndTime = now.AddDays(2);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(!runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(!runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(!runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(!runSheet2.EY_EndTimeInfo.HasErrors());
			//Middle of Day
			runSheet2.EY_StartTime = now.AddHours(5);
			runSheet2.EY_EndTime = now.AddHours(15);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//Overlap Morning
			runSheet2.EY_StartTime = now.AddHours(-5);
			runSheet2.EY_EndTime = now.AddHours(5);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
			//Overlap Arvo
			runSheet2.EY_StartTime = now.AddHours(5);
			runSheet2.EY_EndTime = now.AddHours(30);
			runSheet1.RunPreSaveValidation();
			runSheet2.RunPreSaveValidation();
			Assert(runSheet1.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet1.EY_StartTimeInfo.HasErrors());
			Assert(runSheet1.EY_EndTimeInfo.HasErrors());
			Assert(runSheet2.EY_GS_NKTruckDriverInfo.HasErrors());
			Assert(runSheet2.EY_StartTimeInfo.HasErrors());
			Assert(runSheet2.EY_EndTimeInfo.HasErrors());
		}

		public void TestErrorStatus()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			runSheet.ErrorStatus = "";
			Assert(runSheet.ErrorStatusInfo.HasErrors());
			runSheet.ErrorStatus = "sdf;ijh";
			Assert(runSheet.ErrorStatusInfo.HasErrors());
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Working;
			Assert(!runSheet.ErrorStatusInfo.HasErrors());
		}
	}
}
