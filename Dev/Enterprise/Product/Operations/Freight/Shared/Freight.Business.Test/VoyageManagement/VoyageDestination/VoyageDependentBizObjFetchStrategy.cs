using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageDependentBizObjFetchStrategy : TestCaseWithFactory
	{
		public void TestFetchForLoad_Destination()
		{
			Factory.ResetDatabaseLoadCount();
			Factory.Load<VoyageDestination>(new ZQuery(JobVoyDestinationSchema.PK, destinationGuid));

			var dbHits = new Dictionary<string, int>
			{
				{ JobVoyDestinationSchema.Constants.TableName, 1 }
			};

			AssertDbHits(dbHits, Factory);

			var factoryFetchHints = Factory.GetAllFetchHintedTableNames().ToArray();

			AssertEquals(2, factoryFetchHints.Length);
			AssertCollectionContains("Expected JobVoyage fetch hint on load", JobVoyageSchema.Constants.TableName, factoryFetchHints);
			AssertCollectionContains("GenCustomAddOnValue fetch hint is added by 'UserDefinedValues' attribute on class", GenCustomAddOnValueSchema.Constants.TableName, factoryFetchHints);
		}

		public void TestFetchForLoad_Origin()
		{
			Factory.ResetDatabaseLoadCount();
			Factory.Load<VoyageOrigin>(new ZQuery(JobVoyOriginSchema.PK, originGuid));

			var dbHits = new Dictionary<string, int>
			{
				{ JobVoyOriginSchema.Constants.TableName, 1 }
			};

			AssertDbHits(dbHits, Factory);

			var factoryFetchHints = Factory.GetAllFetchHintedTableNames().ToArray();

			AssertEquals(2, factoryFetchHints.Length);
			AssertCollectionContains("Expected JobVoyage fetch hint on load", JobVoyageSchema.Constants.TableName, factoryFetchHints);
			AssertCollectionContains("GenCustomAddOnValue fetch hint is added by 'UserDefinedValues' attribute on class", GenCustomAddOnValueSchema.Constants.TableName, factoryFetchHints);
		}

		protected override void SetUp()
		{
			var creatorFactory = new BusinessObjectFactory();

			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			var voyage = creatorFactory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "564234";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "GBSUN";
			originGuid = origin.PK;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destinationGuid = destination.PK;

			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "CNSHA";

			voyage.GenerateSailings();

			creatorFactory.Save();
		}

		ZGuid destinationGuid;
		ZGuid originGuid;
	}
}
