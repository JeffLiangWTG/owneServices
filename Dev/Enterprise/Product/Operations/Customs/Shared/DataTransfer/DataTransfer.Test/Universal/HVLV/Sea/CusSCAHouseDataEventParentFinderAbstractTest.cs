using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	[TestsSubclassesOf(typeof(CusSCAHouseDataEventParentFinder))]
	public abstract class CusSCAHouseDataEventParentFinderAbstractTest<TParentFinder, THouseBill> : TestCaseWithFactory
			where TParentFinder : CusSCAHouseDataEventParentFinder
			where THouseBill : BaseCusSCAHouse
	{
		public void TestOnlyNonEmptyHouseBillShouldBeTargetToThisModule()
		{
			var finder = GetNewParentFinder(Factory, new CusSCAHouseDataContextManager(), new DummyLogger());
			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var parents = finder.GetLogParentsForEvent(deserializer.Parse(HouseBillEventXML));
			if (typeof(TParentFinder) == typeof(CusSCAHouseDataEventParentFinder))
			{
				AssertNull("no parents should be found by the base class", parents);
			}
			else
			{
				AssertNull("no parents should be found because houseBill does not exist.", parents);
			}
		}

		public void TestFindParents()
		{
			var houseBill = CreateSeveralHouseBillsAndReturnTheOnlyOneThatShouldBeFound();
			var finder = GetNewParentFinder(Factory, new CusSCAHouseDataContextManager(), new DummyLogger());
			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var parents = finder.GetLogParentsForEvent(deserializer.Parse(HouseBillEventXML));

			if (typeof(TParentFinder) == typeof(CusSCAHouseDataEventParentFinder))
			{
				AssertNull("no parents should be found by the base class", parents);
			}
			else
			{
				AssertNotNull("parents found", parents);
				AssertEquals("exactly one parent found", 1, parents.Length);
				AssertEquals("parent found correctly", houseBill.PK, parents[0].PK);
			}
		}

		protected abstract TParentFinder GetNewParentFinder(BusinessObjectFactory factory, CusSCAHouseDataContextManager manager, IXmlImportLogger logger);

		protected abstract string HouseBillEventXML { get; }

		THouseBill CreateSeveralHouseBillsAndReturnTheOnlyOneThatShouldBeFound()
		{
			var result = CreateHouseBill("House Bill 1", "master_house 1");
			CreateHouseBill("House Bill 2", "master_house 2");
			CreateHouseBill("House Bill 1", "master_house 2");
			return result;
		}

		THouseBill CreateHouseBill(ZString houseBill, ZString masterHouseBill)
		{
			var result = Factory.New<THouseBill>();
			result.CA_HouseBill = houseBill;
			result.CA_MasterHouseBill = masterHouseBill;
			return result;
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		protected TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
