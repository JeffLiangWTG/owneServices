using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TransportNonDependent))]
	sealed class TransportNonDependentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentType()
		{
			var transport = Factory.New<TransportNonDependent>();
			AssertEquals(typeof(CommonConsol), transport.ParentType);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var transport = new TransportNonDependentCollection(Factory).AddNew();
			return transport;
		}

		#endregion
	}
}
