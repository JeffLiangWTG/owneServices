using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelDeviceLocationWithEntity))]
	class TelDeviceLocationWithEntityTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			// This bizo is for a db View, so can't save or delete
		}

		public override bool EnableAllowSpatialTypesAttributeTest => false;

		ITelematicsTestHelper helper;
		ITelematicsTestHelper Helper
		{
			get { return helper ?? (helper = ObjectFactory.Get<ITelematicsTestHelper>("ITelematicsTestHelper", Factory)); }
		}

		const double LongitudeWTG = 151.19513;
		const double LatitudeWTG = -33.91629;

		[TestDate(2017, 06, 13)]
		public void TestReturnsCurrentOperatorForCurrentLoggedInUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";
			var truck = Factory.NewWithValidTestData<RefEquipment>();
			var measurementTimeUtc = ZDateTime.UtcNow;
			var startTime = new ZDateTimeOffset(measurementTimeUtc).AddMinutes(-6);
			Helper.CreateTelEdgeAssociation(truck.PK, "RQ", staff.PK, "GS", 0, startTime, ZDateTimeOffset.Empty, "opt");
			Factory.Save();

			var telDeviceLocationWithEntity = Factory.New<TelDeviceLocationWithEntity>();
			telDeviceLocationWithEntity.TLL_ParentID = truck.PK;
			telDeviceLocationWithEntity.TLL_ParentTableCode = "RQ";
			telDeviceLocationWithEntity.TLL_Location = ZGeography.CreatePoint(LongitudeWTG, LatitudeWTG);
			telDeviceLocationWithEntity.TLL_MeasurementTimeUtc = measurementTimeUtc;

			var telEdge = telDeviceLocationWithEntity.GetOperator();

			AssertNotNull("GetOperator should return matching TelEdge", telEdge);
			AssertEquals("GS", telEdge.TE_EntityTableCodeTo.ToString());
			AssertEquals(staff.PK, telEdge.TE_EntityIdTo);
		}

		[TestDate(2017, 06, 13)]
		public void TestReturnsCurrentOperatorForPreviousLoggedInUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";

			var staffOther = Factory.NewWithValidTestData<GlbStaff>();
			staffOther.GS_Code = "TS2";
			staffOther.GS_LoginName = "TS2";
			staffOther.StaffPlainTextPassword = "TEST";

			var truck = Factory.NewWithValidTestData<RefEquipment>();
			var truckOther = Factory.NewWithValidTestData<RefEquipment>();

			var telDeviceLocationWithEntity = Factory.New<TelDeviceLocationWithEntity>();
			telDeviceLocationWithEntity.TLL_ParentID = truck.PK;
			telDeviceLocationWithEntity.TLL_ParentTableCode = "RQ";
			telDeviceLocationWithEntity.TLL_Location = ZGeography.CreatePoint(LongitudeWTG, LatitudeWTG);
			telDeviceLocationWithEntity.TLL_MeasurementTimeUtc = ZDateTime.UtcNow;

			var zDateTimeOffset = new ZDateTimeOffset(telDeviceLocationWithEntity.TLL_MeasurementTimeUtc);

			// Expected result
			Helper.CreateTelEdgeAssociation(truck.PK, "RQ", staff.PK, "GS", 0, zDateTimeOffset.AddMinutes(-6), zDateTimeOffset.AddHours(2), "opt");

			// Other associations
			Helper.CreateTelEdgeAssociation(truck.PK, "RQ", staffOther.PK, "GS", 0, zDateTimeOffset.AddDays(-1), zDateTimeOffset.AddDays(-1).AddHours(2), "opt");
			Helper.CreateTelEdgeAssociation(truck.PK, "RQ", staffOther.PK, "GS", 0, zDateTimeOffset.AddHours(-2), zDateTimeOffset.AddHours(-1), "opt");
			Helper.CreateTelEdgeAssociation(truckOther.PK, "RQ", staffOther.PK, "GS", 0, zDateTimeOffset.AddHours(-4), zDateTimeOffset.AddHours(-3), "opt");

			var telEdge = telDeviceLocationWithEntity.GetOperator();
			AssertNotNull("GetOperator should return matching TelEdge", telEdge);
			AssertEquals("GS", telEdge.TE_EntityTableCodeTo.ToString());
			AssertEquals(staff.PK, telEdge.TE_EntityIdTo);
		}
	}
}
