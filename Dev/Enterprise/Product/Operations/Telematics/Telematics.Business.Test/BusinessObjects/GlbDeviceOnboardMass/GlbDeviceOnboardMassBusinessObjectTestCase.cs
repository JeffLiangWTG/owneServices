using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceOnboardMass))]
	class GlbDeviceOnboardMassBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var record = (GlbDeviceOnboardMass)base.GetNewBusinessObjectForDeleteTest(factory);
			record.SubEquipment.TSE_Type = "O";
			return record;
		}
	}
}
