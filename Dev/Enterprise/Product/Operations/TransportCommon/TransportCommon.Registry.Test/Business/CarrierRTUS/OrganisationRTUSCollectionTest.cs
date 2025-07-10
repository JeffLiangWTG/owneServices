using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(OrganisationRTUSCollection))]
	class OrganisationRTUSCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrganisationRTUSCollection>
	{
		#region AllowNew

		public void TestAllowNew()
		{
			AssertEquals("Allow new rows", true, Collection.AllowNew);
		}

		#endregion

		#region SetDefaultsForNewChild

		public void TestSetDefaultsForNewChild()
		{
			AssertEquals("RTUS list default should be 3GT", CBAList.Codes.TMS3G, Collection.AddNew().CBACode);
			AssertEquals("Url default should be Not supported Message", "This CBA is not yet supported", Collection.AddNew().Url);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override OrganisationRTUSCollection GetCollectionToTest()
		{
			return new OrganisationRTUSCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrganisationRTUSOption();
		}

		#endregion
	}
}
