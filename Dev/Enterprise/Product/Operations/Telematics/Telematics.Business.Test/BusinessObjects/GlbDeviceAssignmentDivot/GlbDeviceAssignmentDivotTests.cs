using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceAssignmentDivot))]
	class GlbDeviceAssignmentDivotTests : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var device = factory.NewWithValidTestData<GlbDevice>();
			var divot = factory.NewWithValidTestData<GlbDeviceAssignmentDivot>();

			divot.V7_V3_Device = device.PK;

			divot.V7_StartTimeUtc = DateTime.UtcNow.AddDays(-1);
			divot.V7_EndTimeUtc = DateTime.UtcNow.AddDays(1);

			divot.V7_ParentTableCode = GlbStaffSchema.Constants.Prefix;

			divot.Device.V3_MobileServicesIdentifier = new byte[] { 1 };
			divot.Device.V3_HumanReadableIdentifier = "one";

			return divot;
		}
	}
}
