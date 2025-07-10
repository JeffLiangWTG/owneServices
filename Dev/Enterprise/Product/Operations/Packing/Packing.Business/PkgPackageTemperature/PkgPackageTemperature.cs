using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Packing.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[CodeAlive("Instance is created by GlowPowerBiReportLinkBuilder.Factory")]
	public class PkgPackageTemperature : AutoPkgPackageTemperature
	{
		public PkgPackageTemperature(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }
	}
}
