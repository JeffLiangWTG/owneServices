using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobChargeFetchStrategyTest : TestCaseWithFactory
	{
		public void TestJobChargeAttribFetchHint()
		{
			/*
			JobHeader: 3
			JobChargeAttrib: 1
			JobCharge: 1

			Hits: 5/5
			*/
			AccountingAssertionHelper.AssertFetchHint<JobCharge, JobChargeAttrib>(JobChargeAttribSchema.EC_JR, 5);
		}

		public void TestJobChargeTargetFetchHint()
		{
			var charges = new List<JobCharge>();

			var factory = new BusinessObjectFactory();
			for (int i = 0; i < 3; i++)
			{
				var charge = factory.NewWithValidTestData<JobCharge>();
				var jct = factory.NewWithValidTestData<JobChargeTarget>();
				jct.JRT_JR = charge.PK;
				jct.JRT_RelatedJobID = ZGuid.NewZGuid();
				jct.JRT_RelatedJobTableCode = "JS";
				jct.JRT_InvoiceTargetID = ZGuid.NewZGuid();
				jct.JRT_InvoiceTargetTableCode = "JK";

				charges.Add(charge);
			}

			factory.Save();

			factory = new BusinessObjectFactory();
			factory.ResetDatabaseLoadCount();

			var masterBizosInNewFactory = factory.Load<JobCharge>(new ZQuery(JobChargeSchema.PK, charges.Select(x => x.PK)));
			foreach (var masterBizo in masterBizosInNewFactory)
			{
				var childBizosInNewFactory = factory.Load<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, masterBizo.PK));
				AssertEquals(1, childBizosInNewFactory.Length);
			}

			//			JobChargeTarget: 1
			//			JobCharge: 1
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertMaxDbHits(2, factory);
		}

		public void TestThereIsNoFetchHintToLoadOtherJobCharges()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var chargeInNewFactory = newFactory.Load<JobCharge>(charge.PK);
			AssertEquals("Loading JobCHarge should not add any Fetch Hints for other Job CHarges", 0, newFactory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName));
		}
	}
}
