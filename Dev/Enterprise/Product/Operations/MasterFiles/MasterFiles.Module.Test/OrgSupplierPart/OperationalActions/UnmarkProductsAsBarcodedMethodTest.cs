using Enterprise.MasterFiles.Module;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.MasterFiles.Module.Testing
{
	[TestedType(typeof(UnmarkProductsAsBarcodedMethod))]
	sealed class UnmarkProductsAsBarcodedMethodTest : OperationalActionMethodTest<UnmarkProductsAsBarcodedMethod>
	{
		protected override UnmarkProductsAsBarcodedMethod NewMethod()
		{
			return new UnmarkProductsAsBarcodedMethod();
		}
	}
}
