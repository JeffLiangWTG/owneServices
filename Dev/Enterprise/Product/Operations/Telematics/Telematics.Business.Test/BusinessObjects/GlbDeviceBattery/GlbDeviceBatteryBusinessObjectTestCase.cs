using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDeviceBattery))]
	class GlbDeviceBatteryBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		const int MinGdbVoltage = 2;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var o = (GlbDeviceBattery)base.GetBusinessObjectForFetchForLoad();
			o.GDB_Voltage = MinGdbVoltage;
			return o;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (GlbDeviceBattery)base.GetNewBusinessObjectForDeleteTest(factory);
			o.GDB_Voltage = MinGdbVoltage;
			return o;
		}
	}
}
