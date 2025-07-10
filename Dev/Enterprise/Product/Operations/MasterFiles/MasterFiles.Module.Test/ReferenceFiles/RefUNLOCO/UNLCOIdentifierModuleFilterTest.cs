using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO.Testing
{
	[TestedType(typeof(UNLCOIdentifierModuleFilter))]
	sealed class UNLCOIdentifierModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetAndJoinCondition()
		{
			var unloco1 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco2 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco3 = Factory.NewWithValidTestData<Business.RefUNLOCO>();

			var unlocoTestPKs = new HashSet<ZGuid>()
			{
				unloco1.PK,
				unloco2.PK,
				unloco3.PK
			};

			unloco1.RL_HasAirport = true;
			unloco1.RL_HasStore = true;

			unloco2.RL_HasStore = true;
			unloco2.RL_HasBorderCrossing = true;

			unloco3.RL_HasAirport = false;
			unloco3.RL_HasStore = false;

			Factory.Save();

			var filter = new UNLCOIdentifierModuleFilter();
			filter.Property0 = true; //RL_Airport
			filter.Property8 = true; //RL_Store
			filter.IsActive = true;

			filter.ShowAddOrRadioBox = true;
			filter.AndJoinCondition = true;
			filter.OrJoinCondition = false;

			var unlocoCollection = new RefUNLOCOCollection(Factory, filter.Query);
			var relevantResults = unlocoCollection.Where(ul => unlocoTestPKs.Contains(ul.PK));

			AssertContainsExactElementsInAnyOrder("AND Functionality: ", new[] { unloco1 }, relevantResults);
		}

		public void TestSetOrJoinCondition()
		{
			var unloco1 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco2 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco3 = Factory.NewWithValidTestData<Business.RefUNLOCO>();

			var unlocoTestPKs = new HashSet<ZGuid>()
			{
				unloco1.PK,
				unloco2.PK,
				unloco3.PK
			};

			unloco1.RL_HasPost = true;
			unloco1.RL_HasStore = true;

			unloco2.RL_HasStore = true;
			unloco2.RL_HasBorderCrossing = true;

			unloco3.RL_HasPost = false;
			unloco3.RL_HasStore = false;

			Factory.Save();

			var filter = new UNLCOIdentifierModuleFilter();
			filter.Property9 = true; //RL_Post
			filter.Property8 = true; //RL_Store
			filter.IsActive = true;

			filter.ShowAddOrRadioBox = true;
			filter.AndJoinCondition = false;
			filter.OrJoinCondition = true;

			var unlocoCollection = new RefUNLOCOCollection(Factory, filter.Query);
			var relevantResults = unlocoCollection.Where(ul => unlocoTestPKs.Contains(ul.PK));

			AssertContainsExactElementsInAnyOrder("OR Functionality: ", new[] { unloco1, unloco2 }, relevantResults);
		}

		public void TestUNLOCO()
		{
			var unloco1 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco2 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco3 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco4 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco5 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco6 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco7 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco8 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco9 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco10 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco11 = Factory.NewWithValidTestData<Business.RefUNLOCO>();
			var unloco12 = Factory.NewWithValidTestData<Business.RefUNLOCO>();

			unloco1.RL_HasAirport = true;
			unloco2.RL_HasRail = true;
			unloco3.RL_HasRoad = true;
			unloco4.RL_HasSeaport = true;
			unloco5.RL_HasTerminal = true;
			unloco6.RL_HasOutport = true;
			unloco7.RL_HasDischarge = true;
			unloco8.RL_HasUnload = true;
			unloco9.RL_HasStore = true;
			unloco10.RL_HasPost = true;
			unloco11.RL_HasCustomsLodge = true;
			unloco12.RL_HasBorderCrossing = true;

			Factory.Save();

			var filter = new UNLCOIdentifierModuleFilter();
			filter.Property0 = true; //RL_Airport
			filter.IsActive = true;

			var unlocoCollection = new RefUNLOCOCollection(Factory, filter.Query);

			Assert("Expect collection to contain unloco1.", unlocoCollection.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection.Contains(unloco2));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = true; //RL_Rail

			var unlocoCollection1 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection1.Contains(unloco1));
			Assert("Expect collection to contain unloco2.", unlocoCollection1.Contains(unloco2));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection1.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = true; //RL_Road

			var unlocoCollection2 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection to contain unloco1.", !unlocoCollection2.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection2.Contains(unloco2));
			Assert("Expect collection to contain unloco3.", unlocoCollection2.Contains(unloco3));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = true; //RL_Seaport

			var unlocoCollection3 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection3.Contains(unloco1));
			Assert("Expect collection to contain unloco2.", !unlocoCollection3.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection3.Contains(unloco3));
			Assert("Expect collection to contain unloco4.", unlocoCollection3.Contains(unloco4));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = true; //RL_HasTerminal

			var unlocoCollection4 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection4.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection4.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection4.Contains(unloco3));
			Assert("Expect collection to contain unloco5.", unlocoCollection4.Contains(unloco5));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection4.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = true; //RL_HasOutport

			var unlocoCollection5 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection5.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection5.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection5.Contains(unloco3));
			Assert("Expect collection to contain unloco6.", unlocoCollection5.Contains(unloco6));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection5.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = true; //RL_HasDischarge

			var unlocoCollection6 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection6.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection6.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection6.Contains(unloco3));
			Assert("Expect collection     to contain unloco7.", unlocoCollection6.Contains(unloco7));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection6.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = true; //RL_HasUnload

			var unlocoCollection7 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection7.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection7.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection7.Contains(unloco3));
			Assert("Expect collection to contain unloco8.", unlocoCollection7.Contains(unloco8));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection7.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = true; //RL_HasStore

			var unlocoCollection8 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection8.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection8.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection8.Contains(unloco3));
			Assert("Expect collection to contain unloco9.", unlocoCollection8.Contains(unloco9));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection8.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = false;
			filter.Property9 = true; //RL_HasPost

			var unlocoCollection9 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection9.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection9.Contains(unloco2));
			Assert("Expect collection to contain unloco10.", unlocoCollection9.Contains(unloco10));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection9.Contains(unloco3));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection9.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = false;
			filter.Property9 = false;
			filter.Property10 = true; //RL_HasCustomsLodge

			var unlocoCollection10 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection10.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection10.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection10.Contains(unloco3));
			Assert("Expect collection to contain unloco11.", unlocoCollection10.Contains(unloco11));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection10.Contains(unloco12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = false;
			filter.Property9 = false;
			filter.Property10 = false;
			filter.Property11 = true; //RL_HasBorderCrossing

			var unlocoCollection11 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection11.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection11.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection11.Contains(unloco3));
			Assert("Expect collection not to contain unloco11.", !unlocoCollection11.Contains(unloco11));
			Assert("Expect collection to contain unloco12.", unlocoCollection11.Contains(unloco12));

			//all true
			filter.Property0 = true;
			filter.Property1 = true;
			filter.Property2 = true;
			filter.Property3 = true;
			filter.Property4 = true;
			filter.Property5 = true;
			filter.Property6 = true;
			filter.Property7 = true;
			filter.Property8 = true;
			filter.Property9 = true;
			filter.Property10 = true;
			filter.Property11 = true;

			var unlocoCollection12 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection not to contain unloco1.", !unlocoCollection12.Contains(unloco1));
			Assert("Expect collection not to contain unloco2.", !unlocoCollection12.Contains(unloco2));
			Assert("Expect collection not to contain unloco3.", !unlocoCollection12.Contains(unloco3));
			Assert("Expect collection not to contain unloco4.", !unlocoCollection12.Contains(unloco4));
			Assert("Expect collection not to contain unloco5.", !unlocoCollection12.Contains(unloco5));
			Assert("Expect collection not to contain unloco6.", !unlocoCollection12.Contains(unloco6));
			Assert("Expect collection not to contain unloco7.", !unlocoCollection12.Contains(unloco7));
			Assert("Expect collection not to contain unloco8.", !unlocoCollection12.Contains(unloco8));
			Assert("Expect collection not to contain unloco9.", !unlocoCollection12.Contains(unloco9));
			Assert("Expect collection not to contain unloco10.", !unlocoCollection12.Contains(unloco10));
			Assert("Expect collection not to contain unloco11.", !unlocoCollection12.Contains(unloco11));
			Assert("Expect collection not to contain unloco12.", !unlocoCollection12.Contains(unloco12));

			// all false
			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = false;
			filter.Property9 = false;
			filter.Property10 = false;
			filter.Property11 = false;

			var unlocoCollection13 = new RefUNLOCOCollection(Factory, filter.Query);
			Assert("Expect collection to contain unloco1.", unlocoCollection13.Contains(unloco1));
			Assert("Expect collection to contain unloco2.", unlocoCollection13.Contains(unloco2));
			Assert("Expect collection to contain unloco3.", unlocoCollection13.Contains(unloco3));
			Assert("Expect collection to contain unloco4.", unlocoCollection13.Contains(unloco4));
			Assert("Expect collection to contain unloco5.", unlocoCollection13.Contains(unloco5));
			Assert("Expect collection to contain unloco6.", unlocoCollection13.Contains(unloco6));
			Assert("Expect collection to contain unloco7.", unlocoCollection13.Contains(unloco7));
			Assert("Expect collection to contain unloco8.", unlocoCollection13.Contains(unloco8));
			Assert("Expect collection to contain unloco9.", unlocoCollection13.Contains(unloco9));
			Assert("Expect collection to contain unloco10.", unlocoCollection13.Contains(unloco10));
			Assert("Expect collection to contain unloco11.", unlocoCollection13.Contains(unloco11));
			Assert("Expect collection to contain unloco12.", unlocoCollection13.Contains(unloco12));
		}
	}
}
