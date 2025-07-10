using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefLocoMapCollection))]
	sealed class RefLocoMapCollectionTest : ActiveBusinessObjectCollectionTestCase<RefLocoMapCollection>
	{
		public void TestRefLocoMapGetLocalCustomsCode()
		{
			const string TestAirLocalPortCode = "1M";
			const string TestSeaLocalPortCode = "1S";
			const string TestMaiLocalPortCode = "1Z";

			RefLocoMap newAirLocoMap = Factory.New<RefLocoMap>();// NewCollection.AddNew();
			newAirLocoMap.RY_LocalPortCode = TestAirLocalPortCode;
			newAirLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Air;

			RefLocoMap newSeaLocoMap = Factory.New<RefLocoMap>();// NewCollection.AddNew();
			newSeaLocoMap.RY_LocalPortCode = TestSeaLocalPortCode;
			newSeaLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Sea;

			RefLocoMap newMaiLocoMap = Factory.New<RefLocoMap>();// NewCollection.AddNew();
			newMaiLocoMap.RY_LocalPortCode = TestMaiLocalPortCode;
			newMaiLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Mail;

			ZQuery filter = new ZQuery(RefLocoMapSchema.PK, newAirLocoMap.PK);
			filter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.PK, newSeaLocoMap.PK);
			filter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.PK, newMaiLocoMap.PK);

			RefLocoMapCollection newCollection = new RefLocoMapCollection(Factory, filter);

			AssertEquals("Get Local Port Code Air", newAirLocoMap.RY_LocalPortCode, newCollection.LocalCodeForUsage(AirSeaMailSystemUsageList.Codes.Air));
			AssertEquals("Get Local Port Code Sea", newSeaLocoMap.RY_LocalPortCode, newCollection.LocalCodeForUsage(AirSeaMailSystemUsageList.Codes.Sea));
			AssertEquals("Get Local Port Code Mail", newMaiLocoMap.RY_LocalPortCode, newCollection.LocalCodeForUsage(AirSeaMailSystemUsageList.Codes.Mail));
		}

		public void TestRLocalCodeForUsageAndCountry()
		{
			RefCountry australia = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia);
			RefCountry brazil = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			const string LocalPortCode1 = "Code1";
			const string LocalPortCode2 = "Code2";
			const string LocalPortCode3 = "Code3";
			const string LocalPortCode4 = "Code4";

			RefLocoMapCollection newCollection = new RefLocoMapCollection(Factory);
			RefLocoMap australiaAirLocoMap = newCollection.AddNew();
			australiaAirLocoMap.RY_LocalPortCode = LocalPortCode1;
			australiaAirLocoMap.RY_RN = australia.PK;
			australiaAirLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Air;

			RefLocoMap australiaSeaLocoMap = newCollection.AddNew();
			australiaSeaLocoMap.RY_LocalPortCode = LocalPortCode2;
			australiaSeaLocoMap.RY_RN = australia.PK;
			australiaSeaLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Sea;

			RefLocoMap brazilAirLocoMap = newCollection.AddNew();
			brazilAirLocoMap.RY_LocalPortCode = LocalPortCode3;
			brazilAirLocoMap.RY_RN = brazil.PK;
			brazilAirLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Air;

			RefLocoMap brazilSeaLocoMap = newCollection.AddNew();
			brazilSeaLocoMap.RY_LocalPortCode = LocalPortCode4;
			brazilSeaLocoMap.RY_RN = brazil.PK;
			brazilSeaLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Sea;

			ZQuery filter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, LocalPortCode1);
			filter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_LocalPortCode, LocalPortCode2);
			filter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_LocalPortCode, LocalPortCode3);
			filter.AddToFilter(JoinCondition.Or, RefLocoMapSchema.RY_LocalPortCode, LocalPortCode4);
			newCollection.AdditionalFilter = filter;

			AssertEquals("Get Local Port Code For Australia Air", LocalPortCode1, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Air, australia));
			AssertEquals("Get Local Port Code For Australia Air", LocalPortCode1, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Air, australia.RN_Code));
			AssertEquals("Get Local Port Code For Australia Sea", LocalPortCode2, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Sea, australia));
			AssertEquals("Get Local Port Code For Australia Sea", LocalPortCode2, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Sea, australia.RN_Code));
			AssertEquals("Get Local Port Code For Brazil Air", LocalPortCode3, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Air, brazil));
			AssertEquals("Get Local Port Code For Brazil Air", LocalPortCode3, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Air, brazil.RN_Code));
			AssertEquals("Get Local Port Code For Brazil Sea", LocalPortCode4, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Sea, brazil));
			AssertEquals("Get Local Port Code For Brazil Sea", LocalPortCode4, newCollection.LocalCodeForUsageAndCountry(AirSeaMailSystemUsageList.Codes.Sea, brazil.RN_Code));
		}
	}
}
