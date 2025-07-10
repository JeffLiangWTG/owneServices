using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgMiscServCollection))]
	sealed class OrgMiscServCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgMiscServCollection(Organisation, Factory);
		}

		public void TestDefaultsForChildren()
		{
			Organisation.OH_RL_NKClosestPort = "AUSYD";
			OrgMiscServCollection miscServCollection = new OrgMiscServCollection(Organisation, Factory);
			OrgMiscServ miscServ = miscServCollection.AddNew();

			AssertEquals(miscServ.OM_RN_NKEXDefaultCntryOfOrigin, Core.Constants.CountryCodes.Australia);
		}

		OrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.New<OrgHeader>()); }
		}
		OrgHeader organisation;
	}
}
