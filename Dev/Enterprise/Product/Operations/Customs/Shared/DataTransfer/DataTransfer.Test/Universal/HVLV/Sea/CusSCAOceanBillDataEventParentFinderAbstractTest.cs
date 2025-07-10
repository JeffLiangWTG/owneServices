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
	[TestsSubclassesOf(typeof(CusSCAOceanBillDataEventParentFinder))]
	public abstract class CusSCAOceanBillDataEventParentFinderAbstractTest<TParentFinder, TOceanBill> : TestCaseWithFactory
			where TParentFinder : CusSCAOceanBillDataEventParentFinder
			where TOceanBill : BaseCusSCAOceanBill
	{
		public void TestOnlyNonEmptyOceanBillShouldBeTargetToThisModule()
		{
			var xmlEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	  <Event>
		<DataContext>
		  <Company>
			<Code>MEL</Code>
			<Country>
			  <Code>AU</Code>
			</Country>
		  </Company>
		</DataContext>
		<EventTime>2014-12-10T10:28:50</EventTime>
		<EventType>CAD</EventType>
		<ContextCollection>
		  <Context>
			<Type>MAWBNumber</Type>
			<Value>78446080506</Value>
		  </Context>
		  <Context>
			<Type>HAWBNumber</Type>
			<Value>4SU0204122</Value>
		  </Context>
		</ContextCollection>
	  </Event>
	</UniversalEvent>";

			CreateOceanBill(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, CorrectApplicationCode);
			var finder = GetNewParentFinder(Factory, new CusSCAOceanBillDataContextManager(), new DummyLogger());
			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var parents = finder.GetLogParentsForEvent(deserializer.Parse(xmlEvent));
			if (typeof(TParentFinder) == typeof(CusSCAOceanBillDataEventParentFinder))
			{
				AssertNull("no parents should be found by the base class", parents);
			}
			else
			{
				AssertNull("no parents should be found because OceanBill does not exist.", parents);
			}
		}

		public void TestFindParents()
		{
			var oceanBill = CreateSeveralOceanBillsAndReturnTheOnlyOneThatShouldBeFound();
			var finder = GetNewParentFinder(Factory, new CusSCAOceanBillDataContextManager(), new DummyLogger());
			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var parents = finder.GetLogParentsForEvent(deserializer.Parse(OceanBillEventXML));

			if (typeof(TParentFinder) == typeof(CusSCAOceanBillDataEventParentFinder))
			{
				AssertNull("no parents should be found by the base class", parents);
			}
			else
			{
				AssertNotNull("parents found", parents);
				AssertEquals("exactly one parent found", 1, parents.Length);
				AssertEquals("parent found correctly", oceanBill.PK, parents[0].PK);
			}
		}

		protected abstract TParentFinder GetNewParentFinder(BusinessObjectFactory factory, CusSCAOceanBillDataContextManager manager, IXmlImportLogger logger);

		protected abstract string OceanBillEventXML { get; }

		protected abstract ZString CorrectApplicationCode { get; }

		TOceanBill CreateSeveralOceanBillsAndReturnTheOnlyOneThatShouldBeFound()
		{
			var result = CreateOceanBill("Ocean Bill", "Voyage", "1357642", "master_house", CorrectApplicationCode);
			CreateOceanBill("Ocean Bill", "Voyage", "1357642", "master_house", "!@#");
			CreateOceanBill("Ocean_Bill", "Voyage", "1357642", "master_house", CorrectApplicationCode);
			CreateOceanBill("Ocean Bill", "Vo_age", "1357642", "master_house", CorrectApplicationCode);
			CreateOceanBill("Ocean Bill", "Voyage", "1357649", "master_house", CorrectApplicationCode);
			CreateOceanBill("Ocean Bill", "Voyage", "1357642", "master-house", CorrectApplicationCode);
			return result;
		}

		TOceanBill CreateOceanBill(ZString oceanBill, ZString voyage, ZString lloydsIMO, ZString masterHouseBill, ZString applicationCode)
		{
			var result = Factory.New<TOceanBill>();
			result.CB_OceanBill = oceanBill;
			result.CB_Voyage = voyage;
			result.CB_LloydsIMO = lloydsIMO;
			result.CB_MasterHouseBill = masterHouseBill;
			result.CB_ApplicationCode = applicationCode;
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
