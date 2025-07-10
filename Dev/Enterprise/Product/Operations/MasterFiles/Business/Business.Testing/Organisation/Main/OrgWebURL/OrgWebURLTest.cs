using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgWebURL))]
	sealed class OrgWebURLTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgWebURL>();
		}

		public void TestIsSavedByFactory()
		{
			OrgWebURL url = Factory.New<OrgWebURL>();
			Assert(url.IsSavedByFactory);

			url.PU_IsPrimary = true;
			url.PU_URL = "";
			Assert(!url.IsSavedByFactory);

			url.PU_IsPrimary = false;
			url.PU_URL = "";
			Assert(url.IsSavedByFactory);

			url.PU_IsPrimary = true;
			url.PU_URL = "www";
			Assert(url.IsSavedByFactory);
		}

		public void TestMainDefaultAdded()
		{
			OrgWebURL url = Factory.New<OrgWebURL>();
			Assert("Default value", !url.MainDefaultAdded);
		}

		public void TestSettingURLForOrgInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = true;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				var url = testOrg.OrgWebURLs.AddNew();
				url.PU_URL = "http://www.wisetech.com";

				//Assert
				AssertEquals(true, dedupeStarted);
			}
		}

		public void TestSettingURLForOrgDoesNotInvokeDeduplication()
		{
			//Arrange
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = false;

			//Act
			var url = testOrg.OrgWebURLs.AddNew();
			url.PU_URL = "http://www.wisetech.com";

			//Assert
			AssertEquals(false, dedupeStarted);
		}

		public void TestDelete()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var url = org.OrgWebURLs.AddNew();
			var patternMatchingDomain = Factory.NewWithValidTestData<PatternMatchingDomain>();

			url.PU_URL = "http://www.wisetech.com";

			patternMatchingDomain.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast(url.PU_URL);
			patternMatchingDomain.PMD_OH = org.PK;
			patternMatchingDomain.PMD_ParentId = url.PK;
			patternMatchingDomain.PMD_ParentTableCode = url.TablePrefix;
			patternMatchingDomain.PMD_RN_NKCountryCode = "AU";

			Factory.Save();

			url.Delete();

			var checkUrl = Factory.Load<OrgWebURL>(url.PK);
			var checkPatternTable = Factory.LoadTop1<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, url.PK));

			AssertNull(checkUrl);
			AssertNull(checkPatternTable);
		}

		public void TestNoAuditLog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var url = org.OrgWebURLs.AddNew();

			url.PU_URL = "http://www.wisetech.com";

			Factory.Save();

			var stmLogData = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, url.PK));
			AssertEquals(0, stmLogData.Length);
		}
	}
}
