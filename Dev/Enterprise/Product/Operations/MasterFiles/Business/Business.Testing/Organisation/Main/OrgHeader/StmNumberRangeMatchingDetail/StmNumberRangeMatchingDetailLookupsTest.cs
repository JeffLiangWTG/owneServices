using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmNumberRangeMatchingDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestRangeTypes

		public void TestRangeTypes()
		{
			var matchingDetails = Factory.New<StmNumberRangeMatchingDetail>();

			matchingDetails.NRM_OwnerTableCode = OrgHeaderSchema.Constants.Prefix;
			var codes = matchingDetails.Lookups.RangeTypes;
			AssertContainsExactElementsInExactOrder(new[] { OrgConstants.NumberFountains.Code.TransportReferenceNumbers }, codes.GetAllCodes());
			AssertSame(codes, matchingDetails.Lookups.RangeTypes);

			matchingDetails.NRM_OwnerTableCode = GlbStaffSchema.Constants.Prefix;
			AssertNotSame(codes, matchingDetails.Lookups.RangeTypes);

			codes = matchingDetails.Lookups.RangeTypes;
			AssertContainsExactElementsInExactOrder(new[] { OrgConstants.NumberFountains.Code.PatentNumber }, codes.GetAllCodes());
			AssertSame(codes, matchingDetails.Lookups.RangeTypes);
		}

		#endregion

		#region TestClients

		public void TestClients()
		{
			var clients = new StmNumberRangeMatchingDetailLookups(Factory.New<StmNumberRangeMatchingDetail>()).Clients;
			AssertNotNull(clients);
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), clients.GetType());
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			var warehouses = new StmNumberRangeMatchingDetailLookups(Factory.New<StmNumberRangeMatchingDetail>()).Warehouses;
			AssertNotNull(warehouses);
			AssertEquals(ObjectFactory.GetType<IWhsWarehouseCollection>(), warehouses.GetType());
		}

		#endregion

		#region TestCustomsAreaList

		public void TestCustomsAreaList()
		{
			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Customs Facilities", Core.Constants.CountryCodes.Mexico);
			var item1 = helper.CreateNewOrGetExistingCusCodeList(Enterprise.Core.Constants.CountryCodes.Mexico, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "123", "Customs Facilities Test1", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			var item2 = helper.CreateNewOrGetExistingCusCodeList(Enterprise.Core.Constants.CountryCodes.Mexico, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "45", "Customs Facilities Test2", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			var matchingDetails = Factory.New<StmNumberRangeMatchingDetail>();
			AssertEquals("Should get the CustomsAreaList from ZZ Ref Database.", "ZZRefCusCodeListCombinedCollection", matchingDetails.Lookups.CustomsAreaList.GetType().Name);
			var list = matchingDetails.Lookups.CustomsAreaList as BusinessObjectCollection;
			list.Load();
			AssertContainsExactElementsInAnyOrder(new[] { item1.PK, item2.PK }, list.GetPKs());
		}

		#endregion

		#region TestPrefixes

		public void TestPrefixes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgOther = Factory.NewWithValidTestData<OrgHeader>();
			CreateStmNumber(org, "AAA");
			CreateStmNumber(org, "");
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition", new ZString[] { "", "AAA" }, org.Fountains.Select(f => f.SN_Prefix));

			var matchingDetails = Factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			matchingDetails.NRM_OwnerId = org.PK;
			matchingDetails.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			matchingDetails.NRM_Prefix = "AAA";

			var lookups = new StmNumberRangeMatchingDetailLookups(matchingDetails);
			AssertArrayEqualsByElements("Should have empty one.", new String[] { "", "AAA" }, lookups.Prefixes.GetAllCodes());

			CreateStmNumber(org, "CCC", OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers);
			Factory.Save();
			matchingDetails.Owner.Fountains.RefreshFromDb();
			AssertArrayEqualsByElements("Should not show CCC(type is not match).", new String[] { "", "AAA" }, lookups.Prefixes.GetAllCodes());

			CreateStmNumber(org, "BBB");
			Factory.Save();
			matchingDetails.Owner.Fountains.RefreshFromDb();
			AssertArrayEqualsByElements("Should update the cache.", new String[] { "", "AAA", "BBB" }, lookups.Prefixes.GetAllCodes());

			CreateStmNumber(org, "ABA");
			Factory.Save();
			matchingDetails.Owner.Fountains.RefreshFromDb();
			AssertArrayEqualsByElements("Should be in right order.", new String[] { "", "AAA", "ABA", "BBB" }, lookups.Prefixes.GetAllCodes());

			matchingDetails.NRM_RangeType = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			matchingDetails.Owner.Fountains.RefreshFromDb();
			AssertArrayEqualsByElements("SSC is not valid type, just to test filter includs SN_Type.", new String[] { "CCC" }, new StmNumberRangeMatchingDetailLookups(matchingDetails).Prefixes.GetAllCodes());
		}

		ViewStmNums CreateStmNumber(OrgHeader org, string prefix, string type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers)
		{
			var stmNums = new BusinessObjectFactory().New<OrganisationViewStmNums>();
			stmNums.SN_Type = type;
			stmNums.SN_Prefix = prefix;
			stmNums.SN_Owner = org.PK;
			stmNums.Factory.Save();
			var q = new ZQuery(ViewStmNumsSchema.SN_Name, stmNums.SN_Name);
			q.AddToFilter(ViewStmNumsSchema.SN_Owner, stmNums.SN_Owner);
			return Factory.LoadTop1<OrganisationViewStmNums>(q);
		}

		#endregion

		#region TestPrefixesDifferentOrganisation

		public void TestPrefixesDifferentOrganisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			CreateStmNumber(org1, "AAA");
			CreateStmNumber(org2, "BBB");
			Factory.Save();

			var matchingDetails = Factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			matchingDetails.NRM_OwnerId = org1.PK;
			matchingDetails.NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;

			var lookups = new StmNumberRangeMatchingDetailLookups(matchingDetails);
			AssertEquals("AAA", lookups.Prefixes.CodesAsString);

			matchingDetails.NRM_OwnerId = org2.PK;
			AssertEquals("Should not use cache for org1 and should create new list", "BBB", lookups.Prefixes.CodesAsString);
		}

		#endregion
	}
}
