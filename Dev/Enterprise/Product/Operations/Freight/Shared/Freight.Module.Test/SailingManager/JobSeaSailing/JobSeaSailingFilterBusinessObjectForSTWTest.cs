using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Module.Testing
{
	sealed class JobSeaSailingFilterBusinessObjectForSTWTest : TestCaseWithFactory
	{
		public void TestAMSStowPlanStatusFilter()
		{
			Destination1.StowPlanMessageStatus = MessageStatusListSTW.Codes.Acceptance;
			Destination2.StowPlanMessageStatus = MessageStatusListSTW.Codes.Rejection;

			Factory.Save();

			var filterBO = new JobSeaSailingFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[JobSeaSailingFilterBusinessObject.SeaFilterTypes.StowPlanStatus];
			filter.IsActive = true;
			filter.Property = MessageStatusListSTW.Codes.Acceptance;

			Sailings.Load(filter.Query);
			Assert(Sailings.Contains(Sailing1));
			Assert(!Sailings.Contains(Sailing2));
			Assert(!Sailings.Contains(Sailing3));
		}

		public void TestHasStowPlanFilter()
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.StowPlan;
			message.EM_LinkUniqueID = Destination1.PK;
			message.EM_LinkTable = Destination1.TablePrefix;

			Factory.Save();

			var filterBO = new JobSeaSailingFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[JobSeaSailingFilterBusinessObject.SeaFilterTypes.HasStowPlan];
			filter.IsActive = true;
			filter.Property = "HAS";
			Assert(Sailing1.MatchesFilter(filterBO.Filter));
			Assert(!Sailing2.MatchesFilter(filterBO.Filter));
			Assert(!Sailing3.MatchesFilter(filterBO.Filter));

			filter.Property = "NOT";
			Assert(!Sailing1.MatchesFilter(filterBO.Filter));
			Assert(Sailing2.MatchesFilter(filterBO.Filter));
			Assert(Sailing3.MatchesFilter(filterBO.Filter));

			filter.Property = "ALL";
			Assert(Sailing1.MatchesFilter(filterBO.Filter));
			Assert(Sailing2.MatchesFilter(filterBO.Filter));
			Assert(Sailing3.MatchesFilter(filterBO.Filter));
		}

		VoyageDestination Destination1;
		VoyageDestination Destination2;
		JobSailing Sailing1;
		JobSailing Sailing2;
		JobSailing Sailing3;
		JobSailingCollection Sailings;
		protected override void SetUp()
		{
			base.SetUp();
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = ZDateTime.Today.AddDays(10);
			Destination1 = voyage.Destinations.AddNew();
			Destination1.JB_E_ARV = ZDateTime.Today.AddDays(20);
			Destination1.JB_RL_NKPortOfDischarge = "USLAX";
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "CAAAD";
			origin2.JA_E_DEP = ZDateTime.Today.AddDays(30);
			Destination2 = voyage.Destinations.AddNew();
			Destination2.JB_RL_NKPortOfDischarge = "USCHI";
			Destination2.JB_E_ARV = ZDateTime.Today.AddDays(40);

			voyage.GenerateSailings();
			Sailing1 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "USLAX");
			Sailing2 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "USCHI");
			Sailing3 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "CAAAD" && x.JX_JB_RL_NKPortOfDischarge == "USCHI");

			Sailings = new JobSailingCollection(Factory);
		}
	}
}
