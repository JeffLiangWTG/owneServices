using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressQueryHelperTest : TestCaseWithFactory
	{
		public void TestJobDocAddressFieldSearchDBSubQuery()
		{
			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(DocAddressTypes.Codes.ConsigneeAddress, SQLComparisonOperator.StartsWith, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, "12");
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertEquals(0, loadedDocAddress.Length);

			var parent123 = CreateDocAddressParent("O1", "123");
			var parentABC = CreateDocAddressParent("O2", "ABC");
			var parentXYZ = CreateDocAddressParent("O3", "XYZ");

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress3 = CreateOrgHeaderWithJobDocAddress(parentXYZ, DocAddressType.ConsigneeAddress, "12");

			Factory.Save();

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(DocAddressTypes.Codes.ConsigneeAddress, SQLComparisonOperator.StartsWith, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, "12");
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var expectedParents = new ZGuid[] { parent123.PK, parentXYZ.PK };
			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertContainsExactElementsInAnyOrder(expectedParents, loadedDocAddress.Select(d => d.PK));

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(DocAddressTypes.Codes.ConsigneeAddress, SQLComparisonOperator.StartsWith, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, "ABC");
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			expectedParents = new ZGuid[] { parentABC.PK };
			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertContainsExactElementsInAnyOrder(expectedParents, loadedDocAddress.Select(d => d.PK));
		}

		public void TestJobDocAddressFieldSearchDBSubQuery_AddressOverride()
		{
			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(DocAddressTypes.Codes.ConsigneeAddress, SQLComparisonOperator.Equal, OrgAddressSchema.OA_RN_NKCountryCode, JobDocAddressSchema.E2_RN_NKCountryCode, "UK");
			var parentQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			var zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.PK);
			zDBOnlySubQuery.AddSubQuery(JobDocAddressSchema.E2_ParentID, jobDocAddressSubQuery, JoinCondition.And);
			parentQuery.AddSubQuery(zDBOnlySubQuery, JoinCondition.And);
			var loadedDocAddress = Factory.Load<JobDocAddress>(parentQuery);
			AssertEquals(0, loadedDocAddress.Length);

			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			docAddress1.E2_ParentID = ZGuid.NewZGuid();
			docAddress1.E2_AddressType = "CEA";
			docAddress1.E2_OA_Address = org1.MainAddress.PK;
			docAddress1.E2_AddressOverride = false;
			docAddress1.E2_RN_NKCountryCode = "UK";
			org1.MainAddress.OA_RN_NKCountryCode = "UK";

			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			docAddress2.E2_ParentID = ZGuid.NewZGuid();
			docAddress2.E2_AddressType = "CEA";
			docAddress2.E2_OA_Address = org2.MainAddress.PK;
			docAddress2.E2_AddressOverride = false;
			docAddress2.E2_RN_NKCountryCode = "NZ";
			org2.MainAddress.OA_RN_NKCountryCode = "UK";

			var docAddress3 = Factory.NewWithValidTestData<JobDocAddress>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			docAddress3.E2_ParentID = ZGuid.NewZGuid();
			docAddress3.E2_AddressType = "CEA";
			docAddress3.E2_OA_Address = org3.MainAddress.PK;
			docAddress3.E2_AddressOverride = false;
			docAddress3.E2_RN_NKCountryCode = "UK";
			org3.MainAddress.OA_RN_NKCountryCode = "NZ";

			var docAddress4 = Factory.NewWithValidTestData<JobDocAddress>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			docAddress4.E2_ParentID = ZGuid.NewZGuid();
			docAddress4.E2_AddressType = "CEA";
			docAddress4.E2_OA_Address = org4.MainAddress.PK;
			docAddress4.E2_AddressOverride = false;
			docAddress4.E2_RN_NKCountryCode = "NZ";
			org4.MainAddress.OA_RN_NKCountryCode = "NZ";

			var docAddressOverride1 = Factory.NewWithValidTestData<JobDocAddress>();
			var orgOverride1 = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride1.MainAddress.OA_RN_NKCountryCode = "UK";
			docAddressOverride1.E2_ParentID = ZGuid.NewZGuid();
			docAddressOverride1.E2_AddressType = "CEA";
			docAddressOverride1.E2_OA_Address = orgOverride1.MainAddress.PK;
			docAddressOverride1.E2_AddressOverride = true;
			docAddressOverride1.E2_RN_NKCountryCode = "UK";

			var docAddressOverride2 = Factory.NewWithValidTestData<JobDocAddress>();
			var orgOverride2 = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride2.MainAddress.OA_RN_NKCountryCode = "NZ";
			docAddressOverride2.E2_ParentID = ZGuid.NewZGuid();
			docAddressOverride2.E2_AddressType = "CEA";
			docAddressOverride2.E2_OA_Address = orgOverride2.MainAddress.PK;
			docAddressOverride2.E2_AddressOverride = true;
			docAddressOverride2.E2_RN_NKCountryCode = "UK";

			var docAddressOverride3 = Factory.NewWithValidTestData<JobDocAddress>();
			var orgOverride3 = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride3.MainAddress.OA_RN_NKCountryCode = "UK";
			docAddressOverride3.E2_ParentID = ZGuid.NewZGuid();
			docAddressOverride3.E2_AddressType = "CEA";
			docAddressOverride3.E2_OA_Address = orgOverride3.MainAddress.PK;
			docAddressOverride3.E2_AddressOverride = true;
			docAddressOverride3.E2_RN_NKCountryCode = "NZ";

			var docAddressOverride4 = Factory.NewWithValidTestData<JobDocAddress>();
			var orgOverride4 = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride4.MainAddress.OA_RN_NKCountryCode = "NZ";
			docAddressOverride4.E2_ParentID = ZGuid.NewZGuid();
			docAddressOverride4.E2_AddressType = "CEA";
			docAddressOverride4.E2_OA_Address = orgOverride4.MainAddress.PK;
			docAddressOverride4.E2_AddressOverride = true;
			docAddressOverride4.E2_RN_NKCountryCode = "NZ";

			Factory.Save();

			var loadedDocAddress2 = Factory.Load<JobDocAddress>(parentQuery);
			var expectedDocAddressPKs = new ZGuid[] { docAddress1.PK, docAddress2.PK, docAddressOverride1.PK, docAddressOverride2.PK };
			AssertEquals(4, loadedDocAddress2.Length);
			AssertContainsExactElementsInAnyOrder(expectedDocAddressPKs, loadedDocAddress2.Select(da => da.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery()
		{
			var parent123 = CreateDocAddressParent("O1", "123");
			var parentABC = CreateDocAddressParent("O2", "ABC");

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.ConsigneeAddress, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, parent123.PK);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parent123.PK));

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, parentABC.PK);
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parentABC.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery_WithSQLComparisonOperator()
		{
			var parent123 = CreateDocAddressParent("O1", "123");
			var parentABC = CreateDocAddressParent("O2", "ABC");

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.ConsigneeAddress, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, parent123.PK, SQLComparisonOperator.Equal);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parent123.PK));

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, parentABC.PK, SQLComparisonOperator.Equal);
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parentABC.PK));

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, parentABC.PK, SQLComparisonOperator.NotEqual);
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parent123.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery_NoAddressType()
		{
			var parent123 = CreateDocAddressParent("O1", "123");
			var parentABC = CreateDocAddressParent("O2", "ABC");

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.CarrierBookingAgent, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(parent123.PK);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parent123.PK));

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(parentABC.PK);
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parentABC.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery_WithSQLComparisonOperator_NoAddressType()
		{
			var parent123 = CreateDocAddressParent("O1", "123");
			var parentABC = CreateDocAddressParent("O2", "ABC");

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.CarrierBookingAgent, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(string.Empty, parent123.PK, SQLComparisonOperator.Equal);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parent123.PK));

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(string.Empty, parentABC.PK, SQLComparisonOperator.Equal);
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parentABC.PK));

			jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(string.Empty, parentABC.PK, SQLComparisonOperator.NotEqual);
			parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parent123.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery_WithSQLComparisonOperator_IsBlank()
		{
			var parent123 = CreateDocAddressParent("O1", "TestJobDocAddressOrgHeaderParentSubQuery 123");
			var parentABC = CreateDocAddressParent("O2", "TestJobDocAddressOrgHeaderParentSubQuery ABC");
			var orgWithNoDocAddress = CreateDocAddressParent("O3", "TestJobDocAddressOrgHeaderParentSubQuery No Address");

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.CarrierBookingAgent, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(string.Empty, parent123.PK, SQLComparisonOperator.IsBlank);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Like, "TestJobDocAddressOrgHeaderParentSubQuery%"); // adding to avoid test from loading all existing OrgHeaders on test DB
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == orgWithNoDocAddress.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery_WithSQLComparisonOperator_IsBlank_WithAddressType()
		{
			var parent123 = CreateDocAddressParent("O1", "TestJobDocAddressOrgHeaderParentSubQuery 123");
			var parentABC = CreateDocAddressParent("O2", "TestJobDocAddressOrgHeaderParentSubQuery ABC");
			var orgWithNoDocAddress = CreateDocAddressParent("O3", "TestJobDocAddressOrgHeaderParentSubQuery No Address");

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.CarrierBookingAgent, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, parent123.PK, SQLComparisonOperator.IsBlank);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Like, "TestJobDocAddressOrgHeaderParentSubQuery%"); // adding to avoid test from loading all existing OrgHeaders on test DB
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertContainsExactElementsInAnyOrder(new[] { orgWithNoDocAddress.PK, parentABC.PK }, loadedDocAddress.Select(da => da.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery_WithSQLComparisonOperator_IsNotBlank()
		{
			var parent123 = CreateDocAddressParent("O1", "123");
			var parentABC = CreateDocAddressParent("O2", "ABC");
			var orgWithNoDocAddress = Factory.NewWithValidTestData<OrgHeader>();

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.CarrierBookingAgent, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(string.Empty, parent123.PK, SQLComparisonOperator.IsNotBlank);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertContainsExactElementsInAnyOrder(new[] { parent123.PK, parentABC.PK }, loadedDocAddress.Select(da => da.PK));
		}

		public void TestJobDocAddressOrgHeaderParentSubQuery_WithSQLComparisonOperator_IsNotBlank_WithAddressType()
		{
			var parent123 = CreateDocAddressParent("O1", "123");
			var parentABC = CreateDocAddressParent("O2", "ABC");
			var orgWithNoDocAddress = Factory.NewWithValidTestData<OrgHeader>();

			var jobDocAddress1 = CreateOrgHeaderWithJobDocAddress(parent123, DocAddressType.ConsigneeAddress, ZString.Empty);
			var jobDocAddress2 = CreateOrgHeaderWithJobDocAddress(parentABC, DocAddressType.CarrierBookingAgent, ZString.Empty);

			Factory.Save();

			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, parent123.PK, SQLComparisonOperator.IsNotBlank);
			var parentQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			parentQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);

			var loadedDocAddress = Factory.Load<OrgHeader>(parentQuery);
			AssertNotNull(loadedDocAddress.Single(d => d.PK == parent123.PK));
		}

		#region Implementation

		MockJobDocAddressParentForLookupsTest CreateDocAddressParent(ZString code, ZString name)
		{
			var parent = Factory.New<MockJobDocAddressParentForLookupsTest>();
			parent.OH_Code = code;
			parent.OH_FullName = name;

			return parent;
		}

		JobDocAddress CreateOrgHeaderWithJobDocAddress(MockJobDocAddressParentForLookupsTest docAddressParent, DocAddressType jobDocAddressType, ZString jobDocAddressOverrideName)
		{
			var jobDocAddress = docAddressParent.DocAddresses.FindOrCreateWithDocAddressType(jobDocAddressType);
			jobDocAddress.E2_OA_Address = docAddressParent.MainAddress.PK;

			if (!jobDocAddressOverrideName.IsEmpty)
			{
				jobDocAddress.E2_AddressOverride = true;
				jobDocAddress.E2_CompanyName = jobDocAddressOverrideName;
			}

			return jobDocAddress;
		}

		#endregion
	}
}
