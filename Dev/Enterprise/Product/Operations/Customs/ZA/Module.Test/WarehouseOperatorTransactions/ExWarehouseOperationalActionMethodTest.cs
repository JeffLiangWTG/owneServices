using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ExWarehouseOperationalActionMethod))]
	class ExWarehouseOperationalActionMethodTest : OperationalActionMethodTest<ExWarehouseOperationalActionMethod>
	{
		protected override ExWarehouseOperationalActionMethod NewMethod() => new ExWarehouseOperationalActionMethod();
	}
}
