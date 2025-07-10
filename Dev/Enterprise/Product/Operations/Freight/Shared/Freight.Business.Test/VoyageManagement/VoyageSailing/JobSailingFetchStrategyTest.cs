using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobSailingFetchStrategyTest : TestCaseWithFactory
	{
		public void TestBashForView_JX_JB_RL_NKPortOfDischarge()
		{
			// JobVoyOrigin: 1
			BashFetchForView(JobSailing.Schema.JX_JA_RL_NKPortOfLoading, 1);
		}

		public void TestJX_JA_A_DEP()
		{
			// JobVoyOrigin: 1
			BashFetchForView(JobSailing.Schema.JX_JA_A_DEP, 1);
		}

		public void TestJX_JA_E_DEP()
		{
			// JobVoyOrigin: 1
			BashFetchForView(JobSailing.Schema.JX_JA_E_DEP, 1);
		}

		public void TestJX_JB_RL_NKPortOfDischarge()
		{
			// JobVoyDestination: 1
			BashFetchForView(JobSailing.Schema.JX_JB_RL_NKPortOfDischarge, 1);
		}

		public void TestJX_JB_A_ARV()
		{
			// JobVoyDestination: 1
			BashFetchForView(JobSailing.Schema.JX_JB_A_ARV, 1);
		}

		public void TestJX_JB_E_ARV()
		{
			// JobVoyDestination: 1
			BashFetchForView(JobSailing.Schema.JX_JB_E_ARV, 1);
		}

		public void TestJX_JV_VoyageFlight()
		{
			// JobVoyage: 1
			// JobVoyOrigin: 1
			BashFetchForView(JobSailing.Schema.JX_JV_VoyageFlight, 2);
		}

		public void TestJX_JV_NKVessel()
		{
			// JobVoyage: 1
			// JobVoyOrigin: 1
			BashFetchForView(JobSailing.Schema.JX_JV_NKVessel, 2);
		}

		public void TestExchangeRate()
		{
			// JobVoyage: 1
			// JobVoyageExRate: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			BashFetchForView(nameof(JobSailing.ExchangeRate), 4);
		}

		#region Implementation

		static void BashFetchForView(string bindToString, int maxDbHits)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			BaseJobSailing[] sailings = factory.Load<BaseJobSailing>(new ZQuery(JobSailingSchema.PK, CreateContainers()));

			factory.ResetDatabaseLoadCount();

			foreach (BaseJobSailing sailing in sailings)
			{
				sailing.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn("", bindToString) });
			}

			foreach (BaseJobSailing sailing in sailings)
			{
				HitProperty(sailing, bindToString);
			}

			AssertMaxDbHits(maxDbHits, factory);
		}

		static void HitProperty(object root, string pathStr)
		{
			string[] path = pathStr.Split(new char[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);

			object o = root;

			for (int i = 0; i < path.Length; i++)
			{
				if (o == null)
				{
					string message = string.Join("+", path, 0, i) + " returned null, you should populate your data better";
					throw new ApplicationException(message);
				}

				o = o.GetType().InvokeMember(path[i], BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, o, Array.Empty<object>());
			}
		}

		static List<ZGuid> CreateContainers()
		{
			List<ZGuid> result = new List<ZGuid>();
			BusinessObjectFactory factory = new BusinessObjectFactory();

			for (int i = 0; i < 12; i++)
			{
				OrgHeader carrier = factory.NewWithValidTestData<OrgHeader>();

				var vessel = factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;

				JobVoyage voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.JV_OH_Line = carrier.PK;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";

				var exRate = voyage.ExRates.AddNew();
				exRate.E8_RX_NKExCurrency = "EUR";
				exRate.E8_VoyageExchangeRate = 1.10m;
				exRate.E8_RL_NKPort = "ITMIL";

				exRate = voyage.ExRates.AddNew();
				exRate.E8_RX_NKExCurrency = "EUR";
				exRate.E8_VoyageExchangeRate = 1.12m;

				exRate = voyage.ExRates.AddNew();
				exRate.E8_RX_NKExCurrency = "SGD";
				exRate.E8_VoyageExchangeRate = 0.8m;

				voyage.GenerateSailings();

				foreach (JobSailing sailing in voyage.Sailings)
				{
					result.Add(sailing.PK);
				}
			}

			factory.Save();

			return result;
		}

		#endregion
	}
}
