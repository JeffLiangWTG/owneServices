using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRelatedPartyCollection))]
	class OrgRelatedPartyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var collection = new OrgRelatedPartyCollection(Factory, new ZQuery());
			AssertEquals(false, collection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgRelatedPartyCollection(Factory, new ZQuery());
		}

		protected OrgHeader GetOrganisation(ZString name)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.OH_RL_NKClosestPort = "AUSYD";
			return result;
		}
	}
}
