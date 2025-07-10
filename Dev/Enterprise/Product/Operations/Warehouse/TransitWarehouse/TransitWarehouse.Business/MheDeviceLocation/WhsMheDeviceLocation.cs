using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[CodeAlive("This Business Object is used in Glow.")]
	public class WhsMheDeviceLocation : AutoWhsMheDeviceLocation
	{
		public WhsMheDeviceLocation(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}
	}
}
