using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class MasterDataMatchFinderTest : TestCaseWithFactory
	{
		public void TestGetGlbPeronByEmailDomain()
		{
			var factory = new BusinessObjectFactory();
			var list = GlbPersonDeduplicationTestData.NewValidTestData(factory);
			var per = list[0];
			var per2 = list[1];
			var mail = "Anyone@wisetechglobal.com";
			var hash = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(mail));
			var pmd = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd.PMD_PER = per.PK;
			pmd.PMD_HashedValue = hash;
			var pmd1 = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd1.PMD_PER = per2.PK;
			pmd1.PMD_HashedValue = hash;
			factory.Save();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var duplicationFinder = new GlbPersonDuplicationFinder(null, false);

				var targets = MasterDataMatchFinder.GetGlbPeronByEmailDomain(mail);

				AssertEquals(2, targets.Count());
				AssertContainsExactElementsInAnyOrder(new[] { per.PK, per2.PK }, targets.Select(x => x.PK));
			}
		}

		public void TestGetOrgHeaderByEmailDomain()
		{
			var factory = new BusinessObjectFactory();
			var list = OrgHeaderDeduplicationTestData.NewValidTestData(factory);
			var org = list[0];
			var org2 = list[1];
			var mail = "Anyone@wisetechglobal.com";
			var hash = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(mail));
			var pmd = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd.PMD_OH = org.PK;
			pmd.PMD_HashedValue = hash;
			var pmd1 = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd1.PMD_OH = org2.PK;
			pmd1.PMD_HashedValue = hash;
			factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)org).ShouldRunDeduplication = true;

				var targets = MasterDataMatchFinder.GetOrgHeaderByEmailDomain(mail);

				AssertEquals(2, targets.Count());
				AssertContainsExactElementsInAnyOrder(new[] { org.PK, org2.PK }, targets.Select(x => x.PK));
			}
		}
	}
}
