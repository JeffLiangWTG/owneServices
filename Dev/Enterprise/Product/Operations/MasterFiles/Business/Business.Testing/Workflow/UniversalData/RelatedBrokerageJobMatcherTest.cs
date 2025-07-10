using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.UniversalData.Testing
{
	sealed class RelatedBrokerageJobMatcherTest : TestCaseWithFactory
	{
		public void TestMatchJob()
		{
			var job = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Supplier] = org.PK;

			var imp = Factory.NewWithValidTestData<OrgHeader>();
			job[JobDeclarationSchema.JE_OH_Importer] = imp.PK;
			job[JobDeclarationSchema.JE_MasterBill] = "MB002";

			Factory.Save();

			var matchingCriteria = new Dictionary<RelatedJobMatcherKeys, ZString>();
			matchingCriteria.Add(RelatedJobMatcherKeys.Consignee, imp.PK.ToString());
			matchingCriteria.Add(RelatedJobMatcherKeys.Consignor, org.PK.ToString());
			matchingCriteria.Add(RelatedJobMatcherKeys.BookingRefNumber, "333");
			matchingCriteria.Add(RelatedJobMatcherKeys.BillOfLading, "MB002");
			matchingCriteria.Add(RelatedJobMatcherKeys.ITN, "");

			var matcher = RelatedBrokerageJobMatcher.New();
			AssertEquals(job.PK, matcher.GetMatchingJob(matchingCriteria).PK);
		}
	}
}
