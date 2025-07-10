using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GeneratePickActionMethod))]
	public class GeneratePickActionMethodTest : OperationalActionMethodTest<GeneratePickActionMethod>
	{
		protected override GeneratePickActionMethod NewMethod()
		{
			return new GeneratePickActionMethod();
		}
	}
}
