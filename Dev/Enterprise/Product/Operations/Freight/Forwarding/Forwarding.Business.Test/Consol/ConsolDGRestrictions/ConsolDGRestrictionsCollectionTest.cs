using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolDGRestrictionsCollection))]
	sealed class ConsolDGRestrictionsCollectionTest : ActiveBusinessObjectCollectionTestCase<ConsolDGRestrictionsCollection>
	{
		#region Implementation

		protected override ConsolDGRestrictionsCollection GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new ConsolDGRestrictionsCollection(consol);
		}

		#endregion
	}
}
