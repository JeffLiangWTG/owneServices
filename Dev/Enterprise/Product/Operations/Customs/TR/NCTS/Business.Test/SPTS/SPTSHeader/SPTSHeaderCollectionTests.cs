using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSHeaderCollection))]
	class SPTSHeaderCollectionTests : ActiveBusinessObjectCollectionTestCase<SPTSHeaderCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(SPTSHeaderCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<SPTSHeader>();
		}

		public void TestGetBusinessObjectFromCountryCode()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.SetCountry("TR");
			var otherBranch = Factory.New<GlbBranch>();
			otherBranch.GB_RL_NKHomePort = "Other";
			otherBranch.GB_GC = otherCompany.PK;
			otherBranch.GB_Code = "OTH";
			var nctsMovement = Factory.New<SPTSHeader>();
			nctsMovement.BH_JobReference = "LRN";
			nctsMovement.BH_GB = GlbBranch.CurrentBranch.PK;
			nctsMovement.BH_ApplicationCode = "SPT";
			Factory.Save();

			var collection = new SPTSHeaderCollection(Factory, GlbCompany.CurrentCompany);
			AssertEquals(1, collection.Count);
			AssertEquals("LRN", collection[0].BH_JobReference);
			nctsMovement.BH_GB = otherBranch.PK;
			Factory.Save();
			collection = new SPTSHeaderCollection(Factory, GlbCompany.CurrentCompany);
			AssertEquals(0, collection.Count);
		}
	}
}
