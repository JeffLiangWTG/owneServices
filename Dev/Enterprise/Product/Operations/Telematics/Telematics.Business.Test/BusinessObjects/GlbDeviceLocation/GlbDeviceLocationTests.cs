using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceLocation))]
	class GlbDeviceLocationBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var location = (GlbDeviceLocation)base.GetNewBusinessObjectForDeleteTest(factory);

			location.V2_AccuracyInMetres = 1;
			location.V2_MeasurementTimeUtc = ZDateTime.UtcNow;

			return location;
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();

			var device = factory.NewWithValidTestData<GlbDevice>();
			var location = factory.New<GlbDeviceLocation>();
			location.V2_V3_Device = device.PK;
			location.V2_AccuracyInMetres = 1;
			location.V2_MeasurementTimeUtc = ZDateTime.UtcNow;
			factory.Save();
			return location;
		}
	}

	class GlbDeviceLocationTests : TestCaseWithFactory
	{
		public void TestMeasurementTimeToBranchLocal_AheadOfUTC()
		{
			GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort = RefUNLOCO.LoadFromIATA(Factory, "TLV").Code;

			var location = Factory.New<GlbDeviceLocation>();

			location.V2_MeasurementTimeUtc = new ZDateTime(2016, 08, 14, 23, 20, 08, DateTimeKind.Utc);

			AssertEquals(new ZDateTime(2016, 08, 15, 02, 20, 08, DateTimeKind.Local), location.MeasurementTimeLocal);
		}

		public void TestMeasurementTimeToBranchLocal_BehindUTC()
		{
			GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort = RefUNLOCO.LoadFromIATA(Factory, "JFK").Code;

			var location = Factory.New<GlbDeviceLocation>();

			location.V2_MeasurementTimeUtc = new ZDateTime(2016, 08, 15, 03, 36, 08, DateTimeKind.Utc);

			AssertEquals(new ZDateTime(2016, 08, 14, 23, 36, 08, DateTimeKind.Local), location.MeasurementTimeLocal);
		}

		public void TestLocationIsEmpty()
		{
			// Arrange

			// Act
			var location = Factory.New<GlbDeviceLocation>().V2_Location;

			// Assert
			AssertEquals(ZGeography.Empty, location);
		}
	}
}
