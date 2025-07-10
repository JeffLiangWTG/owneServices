using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions.Testing
{
	[TestedType(typeof(USInBondOperationalActionMethodProvider))]
	sealed class USInBondOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Services.OperationalActions.Support.ActionMethodProviderID ID => Enterprise.Services.OperationalActions.Support.ActionMethodProviderIDs.USInBond;

		public void TestNewMethods()
		{
			var provider = new USInBondOperationalActionMethodProvider();
			var expectedMethodTypes = new Type[]
			{
				typeof(SendDepartureAddOperationalActionMethod),
				typeof(BatchMarkAsClosedOperationActionMethod),
			};
			AssertContainsExactElementsInAnyOrder(expectedMethodTypes, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
