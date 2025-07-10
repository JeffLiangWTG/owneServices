using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityProfileCollection))]
	sealed class OrgSecurityProfileCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrgSecurityProfileCollection>
	{
		public void TestAllowRemoveAllowNew()
		{
			var testCollection = GetCollectionToTest();
			Assert(testCollection.AllowRemove);
			Assert(!testCollection.AllowNew);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OrgSecurityProfileCollection GetCollectionToTest()
		{
			return new OrgSecurityProfileCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgSecurityProfile();
		}

		#endregion Implementation
	}
}
