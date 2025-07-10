using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateKG_EndTime

		public void TestValidateKG_EndTime()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			AssertNoErrors("Precondition", runSheet.KG_EndTimeInfo);

			runSheet.KG_EndTime = ZDateTimeOffset.Empty;
			runSheet.Validation.ValidateKG_EndTime();
			AssertHasErrors(runSheet.KG_EndTimeInfo);

			runSheet.KG_EndTime = ZDateTimeOffset.Now;
			AssertNoErrors(runSheet.KG_EndTimeInfo);

			runSheet.KG_StartTime = runSheet.KG_EndTime.AddDays(2);
			runSheet.Validation.ValidateKG_EndTime();
			AssertHasError(runSheet.KG_EndTimeInfo, "The End Time cannot be prior to the Start Time.");

			runSheet.KG_EndTime = runSheet.KG_StartTime.AddDays(1);
			runSheet.Validation.ValidateKG_EndTime();
			AssertNoErrors("Date range is now valid, should not have an error.", runSheet.KG_EndTimeInfo);
		}

		#endregion

		#region TestValidateKG_GS_NKTruckDriver

		[TestDate(2013, 1, 1)]
		public void TestValidateKG_GS_NKTruckDriver()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet.KG_StartTime = ZDateTimeOffset.Empty;
			runSheet.KG_EndTime = ZDateTimeOffset.Empty;
			AssertNoErrors("Precondition", runSheet.KG_GS_NKTruckDriverInfo);

			runSheet.KG_GS_NKTruckDriver = "XX";
			AssertHasErrors(runSheet.KG_GS_NKTruckDriverInfo);

			var driver = Helper.CreateDriver("Hefty Smurf", "Hefty");
			var driverGroup = helper.CreateDriverGroup("Drivers", driver);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());

			Factory.Save();

			runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			AssertNoErrors(runSheet.KG_GS_NKTruckDriverInfo);
			AssertStartAndEndTimeOverlapValidation_TruckOrDriver(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, r => r.KG_GS_NKTruckDriverInfo, runSheet.KG_GS_NKTruckDriver, "Hefty Smurf");
		}

		#endregion

		#region TestValidateKG_OH_TransportCo

		[TestDate(2013, 1, 1)]
		public void TestValidateKG_OH_TransportCo()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			AssertNoErrors("Precondition", runSheet.KG_OH_TransportCoInfo);

			runSheet.KG_OH_TransportCo = ZGuid.NewZGuid();
			AssertHasErrors(runSheet.KG_OH_TransportCoInfo);

			var transportCompany = Helper.CreateOrganisation("T1");
			transportCompany.OH_FullName = "T.CO";
			transportCompany.OH_IsShippingProvider = true;
			transportCompany.OH_IsLocalTransport = true;
			runSheet.KG_OH_TransportCo = transportCompany.PK;
			AssertNoErrors(runSheet.KG_OH_TransportCoInfo);

			var nonTransportCompany = Helper.CreateOrganisation("O1");
			runSheet.KG_OH_TransportCo = nonTransportCompany.PK;
			AssertHasErrors(runSheet.KG_OH_TransportCoInfo);

			runSheet.KG_OH_TransportCo = transportCompany.PK;
			Factory.Save();
			var runSheet2 = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet2.KG_OH_TransportCo = transportCompany.PK;
			AssertNoErrors(runSheet2.KG_OH_TransportCoInfo);
			AssertNoErrors(runSheet2.KG_AdHocDriversNameInfo);

			runSheet2.KG_AdHocDriversName = "Bob";
			AssertNoErrors(runSheet2.KG_OH_TransportCoInfo);

			Factory.Save();
			var runSheet3 = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet3.KG_OH_TransportCo = transportCompany.PK;
			runSheet3.KG_AdHocDriversName = "Bob";
			var errorMessage = $@"T.CO (Bob) is already booked for Run Sheet {runSheet2.KG_RunSheetNumber} between 00:00 and 23:59.
Multiple Transport Company Run Sheets may be booked for the same day if the Driver Name is not specified or unique.";
			AssertHasError(runSheet3.KG_OH_TransportCoInfo, errorMessage);
			AssertHasError(runSheet3.KG_AdHocDriversNameInfo, errorMessage);
		}

		#endregion

		#region TestValidateKG_RQ_Truck

		[TestDate(2013, 1, 1)]
		public void TestValidateKG_RQ_Truck()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet.KG_StartTime = ZDateTimeOffset.Empty;
			runSheet.KG_EndTime = ZDateTimeOffset.Empty;
			AssertNoErrors("Precondition", runSheet.KG_RQ_TruckInfo);

			runSheet.KG_RQ_Truck = ZGuid.NewZGuid();
			AssertHasErrors(runSheet.KG_RQ_TruckInfo);

			runSheet.KG_RQ_Truck = Helper.CreateVehicle("V1").PK;
			AssertNoErrors(runSheet.KG_RQ_TruckInfo);
			runSheet.Truck.RQ_Description = "Truck A";
			AssertStartAndEndTimeOverlapValidation_TruckOrDriver(DtbConsignmentRunSheetSchema.KG_RQ_Truck, r => r.KG_RQ_TruckInfo, runSheet.KG_RQ_Truck, "Truck A");
		}

		#region AssertStartAndEndTimeOverlapValidation_TruckOrDriver

		void AssertStartAndEndTimeOverlapValidation_TruckOrDriver(SchemaColumn column, GetPropertyInfoDelegate getPropertyInfo, IZType truckOrDriver, ZString description)
		{
			var runSheet1 = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet1.KG_StartTime = ZDateTimeOffset.Empty;
			runSheet1.KG_EndTime = ZDateTimeOffset.Empty;
			runSheet1.KG_RunSheetNumber = "1";

			runSheet1[column] = truckOrDriver;

			AssertNoErrors("Empty times, should not have an error.", getPropertyInfo(runSheet1));
			runSheet1.KG_StartTime = ZDateTimeOffset.Today;
			runSheet1.KG_EndTime = ZDateTimeOffset.Today.AddHours(6);

			var runSheet2 = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet2.KG_RunSheetNumber = "2";
			runSheet2.KG_StartTime = ZDateTimeOffset.Today;
			runSheet2.KG_EndTime = ZDateTimeOffset.Today.AddHours(6);
			runSheet2[column] = truckOrDriver;
			AssertHasError(getPropertyInfo(runSheet2), description + " is already booked for Run Sheet 1 between 00:00 and 06:00."); // start and end same time as runsheet1

			runSheet2.KG_StartTime = ZDateTimeOffset.Today.AddHours(3);
			runSheet2.KG_EndTime = ZDateTimeOffset.Today.AddHours(6);
			AssertHasError(getPropertyInfo(runSheet2), description + " is already booked for Run Sheet 1 between 00:00 and 06:00."); // end time same, starts within runsheet1

			runSheet2.KG_StartTime = ZDateTimeOffset.Today.AddHours(3);
			runSheet2.KG_EndTime = ZDateTimeOffset.Today.AddHours(7);
			AssertHasError(getPropertyInfo(runSheet2), description + " is already booked for Run Sheet 1 between 00:00 and 06:00."); // ends after, starts within runsheet1

			runSheet2.KG_StartTime = ZDateTimeOffset.Today;
			AssertHasError(getPropertyInfo(runSheet2), description + " is already booked for Run Sheet 1 between 00:00 and 06:00."); // starts same time, ends after runsheet1

			runSheet2[column] = truckOrDriver.Default;
			AssertNoErrors("Empty selection, should not have an error.", getPropertyInfo(runSheet2));

			runSheet2[column] = truckOrDriver;
			runSheet2.KG_StartTime = ZDateTimeOffset.Today.AddHours(-1);
			runSheet2.KG_EndTime = ZDateTimeOffset.Today;
			AssertNoErrors("Selection is now valid, should not have an error.", getPropertyInfo(runSheet2)); // starts before, ends at same time as runsheet1 starts

			runSheet2.KG_StartTime = ZDateTimeOffset.Today.AddHours(6);
			runSheet2.KG_EndTime = ZDateTimeOffset.Today.EndOfDay();
			AssertNoErrors("Selection is now valid, should not have an error.", getPropertyInfo(runSheet2)); // starts when runsheet 1 ends (exactly)
			Factory.Save(); // cache problems

			var otherFactory = new BusinessObjectFactory();
			var runSheet3 = otherFactory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet3.KG_RunSheetNumber = "3";
			runSheet3.KG_StartTime = ZDateTimeOffset.Today.AddHours(-1); // starts before both runsheets
			runSheet3.KG_EndTime = ZDateTimeOffset.Today.AddDays(1); // ends after both runsheets
			runSheet3[column] = truckOrDriver;
			AssertHasError(getPropertyInfo(runSheet3),
				description + " is already booked for Run Sheet 1 between 00:00 and 06:00." + "\r\n" +
				description + " is already booked for Run Sheet 2 between 06:00 and 23:59.");

			runSheet3.KG_StartTime = runSheet2.KG_EndTime;
			runSheet3.KG_EndTime = ZDateTimeOffset.Today.AddDays(2); // sometime in future
			AssertNoErrors("Selection now valid, should not have an error.", getPropertyInfo(runSheet3)); // same as earlier, starts when runsheet 2 ends

			runSheet3.KG_StartTime = ZDateTimeOffset.Today;
			runSheet3.KG_EndTime = ZDateTimeOffset.Today.AddHours(5);
			AssertHasError(getPropertyInfo(runSheet3), description + " is already booked for Run Sheet 1 between 00:00 and 06:00."); // starts same time, ends before runsheet1

			runSheet3.KG_StartTime = ZDateTimeOffset.Today.AddDays(-1);
			runSheet3.KG_EndTime = ZDateTimeOffset.Today;
			AssertNoErrors("Selection now valid, should not have an error.", getPropertyInfo(runSheet3)); // same as earlier, ends when the other starts

			var runSheet1InOtherFactory = otherFactory.Load<DtbConsignmentRunSheet>(runSheet1.PK);
			runSheet1InOtherFactory.KG_StartTime = ZDateTimeOffset.Today.AddDays(-1);
			runSheet3.KG_StartTime = ZDateTimeOffset.Today.AddHours(3); // starts within runsheet 1
			runSheet3.KG_EndTime = ZDateTimeOffset.Today.AddHours(6.5); // ends within both
			AssertHasError(getPropertyInfo(runSheet3),
				description + " is already booked for Run Sheet 1 between 31-Dec-12 00:00 and 01-Jan-13 06:00." + "\r\n" +
				description + " is already booked for Run Sheet 2 between 06:00 and 23:59.");
		}

		delegate ZPropertyInfo GetPropertyInfoDelegate(DtbConsignmentRunSheet runSheet);

		#endregion

		#endregion

		#region TestValidateKG_StartTime

		public void TestValidateKG_StartTime()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			AssertNoErrors("Precondition", runSheet.KG_StartTimeInfo);

			runSheet.KG_StartTime = ZDateTimeOffset.Empty;
			runSheet.Validation.ValidateKG_StartTime();
			AssertHasErrors(runSheet.KG_StartTimeInfo);

			runSheet.KG_StartTime = ZDateTimeOffset.Now;
			AssertNoErrors(runSheet.KG_StartTimeInfo);

			runSheet.KG_EndTime = runSheet.KG_StartTime.AddDays(-2);
			runSheet.Validation.ValidateKG_StartTime();
			AssertHasError(runSheet.KG_StartTimeInfo, "The Start Time cannot be after the End Time.");

			runSheet.KG_EndTime = runSheet.KG_StartTime.AddDays(1);
			runSheet.Validation.ValidateKG_StartTime();
			AssertNoErrors("Date range is now valid, should not have an error.", runSheet.KG_StartTimeInfo);
		}

		#endregion

		#region TestValidateKG_Duration

		public void TestValidateKG_Duration()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet.KG_Duration = ZDateTime.Now.AddYears(-11);
			AssertNoErrors("This is a duration date field, it can be more than 10 years old", runSheet.KG_DurationInfo);
		}

		#endregion

		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
