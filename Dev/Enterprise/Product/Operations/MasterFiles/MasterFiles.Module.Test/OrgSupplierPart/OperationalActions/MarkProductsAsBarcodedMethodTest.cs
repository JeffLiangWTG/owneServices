using Enterprise.MasterFiles.Module;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.MasterFiles.Module.Testing
{
	[TestedType(typeof(MarkProductsAsBarcodedMethod))]
	sealed class MarkProductsAsBarcodedMethodTest : OperationalActionMethodTest<MarkProductsAsBarcodedMethod>
	{
		protected override MarkProductsAsBarcodedMethod NewMethod()
		{
			return new MarkProductsAsBarcodedMethod();
		}
	}
}
