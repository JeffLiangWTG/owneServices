using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleChangeParentExtentions : TestCaseWithFactory
	{
		public void TestNoParents()
		{
			JobScheduleChange[] emptyArray = System.Array.Empty<JobScheduleChange>();
			ZQuery query = emptyArray.GetSailingFilter();
			AssertEquals("Should be a no-result query.", true, query.IsNoResultQuery);
		}

		public void TestMixedParents()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageFlight = "V1";
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYBAG";
			voyage1.GenerateSailings();

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "V2";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYBAG";
			voyage2.GenerateSailings();

			JobVoyage voyage3 = Factory.New<JobVoyage>();
			voyage3.JV_VoyageFlight = "V3";
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYBAG";
			voyage3.GenerateSailings();

			foreach (JobVoyage voyage in new[] { voyage1, voyage2, voyage3 })
			{
				foreach (VoyageOrigin origin in voyage.Origins)
				{
					NewChange(origin, ScheduleDateTypes.Codes.ETD);
				}

				foreach (VoyageDestination destination in voyage.Destinations)
				{
					NewChange(destination, ScheduleDateTypes.Codes.ETA);
				}

				foreach (JobSailing sailing in voyage.Sailings)
				{
					NewChange(sailing, ScheduleDateTypes.Codes.ETD);
					NewChange(sailing, ScheduleDateTypes.Codes.ETA);
				}
			}

			ZGuid[] parentPKs =
			{
				voyage1.Origins.GetOriginFromLoading("AUBNE").PK,
				voyage2.Destinations.GetDestinationFromDischarge("SGSIN").PK,
				voyage3.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "MYBAG").PK,
			};

			JobScheduleChange[] changes = Factory.Load<JobScheduleChange>(new ZQuery(JobScheduleChangeSchema.E7_ParentID, parentPKs));

			JobSailing[] expectedSailings =
			{
				voyage1.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN"),
				voyage1.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "MYBAG"),
				voyage2.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN"),
				voyage2.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "SGSIN"),
				voyage3.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "MYBAG"),
			};

			AssertContainsExactElementsInAnyOrder(
				(s) => string.Format("{0}: {1}-{2}", s.JX_JV_VoyageFlight, s.JX_JA_RL_NKPortOfLoading, s.JX_JB_RL_NKPortOfDischarge),
				expectedSailings,
				Factory.Load<JobSailing>(changes.GetSailingFilter()));
		}

		#region Implementation

		JobScheduleChange NewChange(IScheduleChangeParent parent, ZString dateType)
		{
			JobScheduleChange change = parent.Factory.New<JobScheduleChange>();
			change.E7_ParentID = parent.PK;
			change.E7_ParentTableCode = parent.TablePrefix;
			change.E7_DateType = dateType;
			return change;
		}

		#endregion
	}
}
