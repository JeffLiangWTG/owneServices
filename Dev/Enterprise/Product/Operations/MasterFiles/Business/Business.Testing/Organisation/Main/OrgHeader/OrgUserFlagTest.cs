using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgUserFlag))]
	sealed class OrgUserFlagTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestIsSelected()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsUserFlag1 = true;
			org.OH_IsUserFlag2 = false;

			var userFlagType1 = OrgUserFlagType.All.First(x => x.FlagNumber == "01");
			var userFlagType2 = OrgUserFlagType.All.First(x => x.FlagNumber == "02");
			var userFlag1 = new OrgUserFlag(org, userFlagType1);
			var userFlag2 = new OrgUserFlag(org, userFlagType2);

			AssertEquals(true, userFlag1.IsSelected);
			AssertEquals(false, userFlag2.IsSelected);

			userFlag1.IsSelected = false;
			userFlag2.IsSelected = true;

			AssertEquals(false, org.OH_IsUserFlag1);
			AssertEquals(true, org.OH_IsUserFlag2);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgUserFlag(Factory.New<OrgHeader>(), OrgUserFlagType.All.First());
		}

		#endregion
	}
}
