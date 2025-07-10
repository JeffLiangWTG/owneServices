using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business.PackageTrackingView
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[CodeAlive("This Business Object is used in Glow.")]
	public class PackageTrackingView : AutoPackageTrackingView, ICanDelete
	{
		public PackageTrackingView(BusinessObjectFactory factory, DataRow row)
			   : base(factory, row)
		{
		}
	}
}
