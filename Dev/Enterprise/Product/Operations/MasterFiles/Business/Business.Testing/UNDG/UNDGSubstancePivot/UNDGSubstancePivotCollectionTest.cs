using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGSubstancePivotCollection))]
	sealed class UNDGSubstancePivotCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGSubstancePivotCollection>
	{
		public void TestPopulation()
		{
			var item = Factory.NewWithValidTestData<UNDGDataItem>();

			var substance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1.DG_UNNO = "9999";
			substance1.DG_Variant = "1";
			substance1.DG_Mode = Constants.TransportModes.Air;

			var substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2.DG_UNNO = "9999";
			substance2.DG_Variant = "2";
			substance2.DG_Mode = Constants.TransportModes.Air;

			var substance3 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance3.DG_UNNO = "9999";
			substance3.DG_Variant = "3";
			substance3.DG_Mode = Constants.TransportModes.Air;

			CreatePivot(item, substance1);
			CreatePivot(item, substance2, false);
			CreatePivot(item, substance3, false);

			Factory.Save();

			var collection = item.UNDGSubstancePivotCollection;
			AssertEquals("Collection has 3 items", 3, collection.Count);
		}

		public void TestCorrectItemsAreReturned()
		{
			var item1 = Factory.NewWithValidTestData<UNDGDataItem>();

			var substance1_1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1_1.DG_UNNO = "9999";
			substance1_1.DG_Variant = "1";
			substance1_1.DG_Mode = Constants.TransportModes.Air;

			var substance1_2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1_2.DG_UNNO = "9999";
			substance1_2.DG_Variant = "2";
			substance1_2.DG_Mode = Constants.TransportModes.Air;

			var substance1_3 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1_3.DG_UNNO = "9999";
			substance1_3.DG_Variant = "3";
			substance1_3.DG_Mode = Constants.TransportModes.Air;

			CreatePivot(item1, substance1_1);
			CreatePivot(item1, substance1_2, false);
			CreatePivot(item1, substance1_3, false);

			var item2 = Factory.NewWithValidTestData<UNDGDataItem>();

			var substance2_1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2_1.DG_UNNO = "8888";
			substance2_1.DG_Variant = "1";
			substance2_1.DG_Mode = Constants.TransportModes.Air;

			var substance2_2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2_2.DG_UNNO = "8888";
			substance2_2.DG_Variant = "2";
			substance2_2.DG_Mode = Constants.TransportModes.Air;

			var substance2_3 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2_3.DG_UNNO = "8888";
			substance2_3.DG_Variant = "3";
			substance2_3.DG_Mode = Constants.TransportModes.Air;

			CreatePivot(item2, substance2_1);

			Factory.Save();

			var collection1 = item1.UNDGSubstancePivotCollection;
			var collection2 = item2.UNDGSubstancePivotCollection;

			AssertEquals("Collection1 has 3 items", 3, collection1.Count);
			AssertEquals("Collection2 has 1 items", 1, collection2.Count);
		}

		public void TestDefaultSubstance()
		{
			var item1 = Factory.NewWithValidTestData<UNDGDataItem>();

			var substance1 = Factory.New<UNDGSubstance>();
			substance1.DG_UNNO = "9999";
			substance1.DG_Variant = "1";
			substance1.DG_Mode = "Air";

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_UNNO = "9999";
			substance2.DG_Variant = "2";
			substance2.DG_Mode = "Air";

			var substance3 = Factory.New<UNDGSubstance>();
			substance3.DG_UNNO = "9999";
			substance3.DG_Variant = "3";
			substance3.DG_Mode = "Air";

			CreatePivot(item1, substance1, false);
			CreatePivot(item1, substance2, true);
			CreatePivot(item1, substance3, false);

			Factory.Save();

			AssertEquals("Collection shoudl return default substance", substance2, item1.UNDGSubstancePivotCollection.DefaultSubstance);
		}

		public void TestUpdatePivotCollection()
		{
			var collection = GetCollectionToTest();
			collection.AddNew();

			AssertEquals("Collection contains pivot", 1, collection.Count);

			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			collection.UpdateDefaultPivot(substance);

			AssertEquals("Collection still contains 1 pivot", 1, collection.Count);

			collection.UpdateDefaultPivot(null);
			AssertEquals("Collection contains no pivots", 0, collection.Count);
		}

		public void TestPivotModesArePopulatedCorrectly()
		{
			var item1 = Factory.NewWithValidTestData<UNDGDataItem>();

			var airSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			airSubstance.DG_UNNO = "9999";
			airSubstance.DG_Variant = "1";
			airSubstance.DG_Mode = Constants.TransportModes.Air;
			airSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			item1.UNDGSubstancePivotCollection.AddPivotFromSubstance(airSubstance);
			AssertEquals("Standard should be IATA", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, item1.UNDGSubstancePivotCollection.First().DP_Standard);

			var item2 = Factory.NewWithValidTestData<UNDGDataItem>();

			var seaSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			seaSubstance.DG_UNNO = "9999";
			seaSubstance.DG_Variant = "2";
			seaSubstance.DG_Mode = Constants.TransportModes.Sea;
			seaSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			item2.UNDGSubstancePivotCollection.AddPivotFromSubstance(seaSubstance);
			AssertEquals("Standard should be IMO", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, item2.UNDGSubstancePivotCollection.First().DP_Standard);

			var item3 = Factory.NewWithValidTestData<UNDGDataItem>();

			var roadSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			roadSubstance.DG_UNNO = "9999";
			roadSubstance.DG_Variant = "3";
			roadSubstance.DG_Mode = Constants.TransportModes.Road;
			roadSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR;

			item3.UNDGSubstancePivotCollection.AddPivotFromSubstance(roadSubstance);
			AssertEquals("Standard should be ADR", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR, item3.UNDGSubstancePivotCollection.First().DP_Standard);
		}

		void CreatePivot(UNDGDataItem dataItem, UNDGSubstance substance, bool isDefault = true)
		{
			var pivot = Factory.NewWithValidTestData<UNDGSubstancePivot>();
			pivot.DP_ParentId = dataItem.PK;
			pivot.DP_ParentTableCode = "DI";
			pivot.DP_UNNO = substance.DG_UNNO;
			pivot.DP_Variant = substance.DG_Variant;
			pivot.DP_IsDefault = isDefault;
			pivot.DP_Standard = substance.DG_Standard;
		}

		protected override UNDGSubstancePivotCollection GetCollectionToTest()
		{
			return Factory.New<UNDGDataItem>().UNDGSubstancePivotCollection;
		}
	}
}
