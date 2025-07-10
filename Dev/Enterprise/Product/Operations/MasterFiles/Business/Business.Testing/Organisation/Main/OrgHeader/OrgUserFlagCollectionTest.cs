using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgUserFlagCollection))]
	sealed class OrgUserFlagCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgUserFlagCollection>
	{
		public void TestConstructor()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new OrgUserFlagCollection(org);
			AssertEquals(OrgUserFlagType.All.Count(), collection.Count);
		}

		#region Implementation

		protected override OrgUserFlagCollection GetCollectionToTest()
		{
			return new OrgUserFlagCollection(Org);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgUserFlag(Org, OrgUserFlagType.All.First());
		}

		OrgHeader Org
		{
			get { return org ?? (org = Factory.New<OrgHeader>()); }
		}
		OrgHeader org;

		#endregion
	}
}
