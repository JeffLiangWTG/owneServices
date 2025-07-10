using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ReferringPartyConfigurationCollection))]
	public class ReferringPartyConfigurationCollectionTest : RegistryBusinessObjectCollectionTestCase<ReferringPartyConfigurationCollection>
	{
		public void TestAllowRemoveAllowNew()
		{
			var testCollection = GetCollectionToTest();
			Assert(testCollection.AllowRemove);
			Assert(testCollection.AllowNew);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ReferringPartyConfigurationCollection GetCollectionToTest()
		{
			return new ReferringPartyConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReferringPartyConfiguration();
		}

		#endregion Implementation
	}
}
