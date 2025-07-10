using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingPackLineMergerTest
			: BusinessObjectMergerTest<ForwardingPackLine>
	{
		public override void TestMerge()
		{
			BuildTestData();

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.CopyPersistentValuesFrom(packLine1);

			var list = new[] { packLine1, packLine2 };
			var merger = new ForwardingPackLineMerger(list, MergeOption.Merge, consol);
			merger.DoMerge();

			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertCollectionContains(packLine1, shipment.OuterPackLines);
			AssertCollectionContains(packLine3, shipment.OuterPackLines);
			AssertCollectionNotContains(packLine2, shipment.OuterPackLines);

			AssertValuesAfterMerge(packLine1);
		}

		public void TestMerge_DGItems()
		{
			BuildTestData();

			var undg1 = packLine1.UNDGs.AddNew();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "123a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg1.LinkDefault(subs);

			var undg2 = packLine2.UNDGs.AddNew();
			undg2.LinkDefault(subs);

			var list = new[] { packLine1, packLine2 };
			var merger = new ForwardingPackLineMerger(list, MergeOption.Merge, consol);
			merger.DoMerge();

			var expectedList = new[]
			{
				undg1.PK
			};

			AssertEquals("Should remove all UNDGs from packline2 for merge", 0, packLine2.PackLocations.Count);
			AssertContainsExactElementsInAnyOrder("Should just keep the first same UNDG item", expectedList, packLine1.UNDGs.GetPKs());
		}

		public void TestMerge_Locations()
		{
			BuildTestData();

			var location1 = packLine1.PackLocations.AddNew();
			location1.JQ_WarehouseLocation = "TestA";

			var location2 = packLine1.PackLocations.AddNew();
			location1.JQ_WarehouseLocation = "TestB";

			var location3 = packLine2.PackLocations.AddNew();
			location1.JQ_WarehouseLocation = "TestC";

			var location4 = packLine2.PackLocations.AddNew();
			location1.JQ_WarehouseLocation = "TestD";

			var list = new[] { packLine1, packLine2 };
			var merger = new ForwardingPackLineMerger(list, MergeOption.Merge, consol);
			merger.DoMerge();

			var expectedList = new[]
			{
				location1.PK,
				location2.PK,
				location3.PK,
				location4.PK
			};

			AssertEquals("Should remove all locations from packline2 for merge", 0, packLine2.PackLocations.Count);
			AssertContainsExactElementsInAnyOrder("Should merge with other packline's location", expectedList, packLine1.PackLocations.GetPKs());
		}

		public void TestMerge_MergeAll()
		{
			BuildTestData();

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.CopyPersistentValuesFrom(packLine1);
			packLine3.UNDGs.AddNew();

			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.CopyPersistentValuesFrom(packLine1);

			var packLine5 = shipment.OuterPackLines.AddNew();
			packLine5.CopyPersistentValuesFrom(packLine1);

			var container = consol.Containers.AddNew();
			packLine1.JL_JC = ZGuid.Empty;
			packLine2.JL_JC = ZGuid.Empty;
			packLine3.JL_JC = ZGuid.Empty;

			var packLine6 = shipment.OuterPackLines.AddNew();
			packLine6.CopyPersistentValuesFrom(packLine1);
			packLine6.JL_Description = "Other PackLine";

			AssertCollectionContains(packLine4, container.PackLines);
			AssertCollectionContains(packLine5, container.PackLines);
			AssertCollectionContains(packLine6, container.PackLines);

			var list = shipment.OuterPackLines.Cast<ForwardingPackLine>();
			var merger = new ForwardingPackLineMerger(list, MergeOption.MergeAll, consol);
			merger.DoMerge();

			AssertCollectionContains("Should merge packline1 with packline2", packLine1, shipment.OuterPackLines);
			AssertCollectionContains("Should contains packline3 as it has different DGItem amount", packLine3, shipment.OuterPackLines);
			AssertCollectionContains("Should merge packline4 with packline5", packLine4, shipment.OuterPackLines);
			AssertCollectionContains("Should contains packline6 as it has different value with other packlines", packLine6, shipment.OuterPackLines);

			AssertCollectionNotContains(packLine2, shipment.OuterPackLines);
			AssertCollectionNotContains(packLine5, shipment.OuterPackLines);

			AssertValuesAfterMerge(packLine1);
			AssertValuesAfterMerge(packLine4);
		}

		void AssertValuesAfterMerge(ForwardingPackLine packLine)
		{
			AssertEquals(168m, packLine.JL_OutturnedWeight);
			AssertEquals(30m, packLine.JL_OutturnedVolume);

			AssertEquals(260m, packLine.JL_ActualWeight);
			AssertEquals("KG", packLine.JL_ActualWeightUQ);
			AssertEquals(106m, packLine.JL_ActualVolume);
			AssertEquals("M3", packLine.JL_ActualVolumeUQ);

			AssertEquals(6, packLine.JL_Outturn);
			AssertEquals(8, packLine.JL_PackageCount);
			AssertEquals(4, packLine.JL_Pillaged);
			AssertEquals(16, packLine.JL_Damaged);
			AssertEquals(6m, packLine.JL_LoadingMeters);
		}

		public override void TestCheckMerge()
		{
			BuildTestData();

			var list = new[] { packLine1 };

			AssertCheckMergerResult(list, MergeOption.MergeAll, consol, string.Empty);
			AssertCheckMergerResult(list, MergeOption.Merge, consol, "Please choose at least two pack lines!");

			var otherShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var otherPackLine = otherShipment.OuterPackLines.AddNew();

			list = new[] { packLine1, otherPackLine };
			AssertCheckMergerResult(list, MergeOption.Merge, consol, "Unable to merge pack lines from different shipments.");

			var undg1 = packLine1.UNDGs.AddNew();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "123a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg1.LinkDefault(subs);

			list = new[] { packLine1, packLine2 };
			AssertCheckMergerResult(list, MergeOption.Merge, consol,
				"Pack lines containing Dangerous Goods items can be merged if they have only one DG item, all parameters are similar, and Volume, Weight and Package count are zero.");

			var undg2 = packLine2.UNDGs.AddNew();
			undg2.LinkDefault(subs);

			AssertCheckMergerResult(list, MergeOption.Merge, consol, string.Empty);

			packLine2.JL_Description = "Test";
			packLine1.JL_MarksAndNumbers = "Test";

			AssertCheckMergerResult(list, MergeOption.Merge, consol, @"The following fields are different:

Goods Description, Marks & Numbers");
		}

		void AssertCheckMergerResult(IList<ForwardingPackLine> list, MergeOption mergeOption, ForwardingConsol consol, string message)
		{
			var merger = new ForwardingPackLineMerger(list, mergeOption, consol);
			AssertEquals(message, merger.CheckMerger());
		}

		protected override IEnumerable<SchemaColumn> GetAllSchemaColumns()
		{
			return JobPackLinesSchema.All.Cast<SchemaColumn>();
		}

		protected override BusinessObjectMerger<ForwardingPackLine> GetNewMerger()
		{
			BuildTestData();

			var list = new[] { packLine1, packLine2 };
			return new ForwardingPackLineMerger(list, MergeOption.Merge, consol);
		}

		void BuildTestData()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			Factory.Save();

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol = shipment.Consols.AddNew();

			packLine1 = shipment.OuterPackLines.AddNew();
			packLine2 = shipment.OuterPackLines.AddNew();

			foreach (var packLine in shipment.OuterPackLines.Cast<ForwardingPackLine>())
			{
				packLine.JL_OutturnedWeight = 84m;
				packLine.JL_OutturnedVolume = 15m;

				packLine.JL_ActualWeight = 130m;
				packLine.JL_ActualWeightUQ = "KG";
				packLine.JL_ActualVolume = 53m;
				packLine.JL_ActualVolumeUQ = "M3";

				packLine.JL_Outturn = 3;
				packLine.JL_PackageCount = 4;
				packLine.JL_Pillaged = 2;
				packLine.JL_Damaged = 8;
				packLine.JL_LoadingMeters = 3;
			}
		}

		#region Impelement

		ForwardingShipment shipment;
		ForwardingPackLine packLine1;
		ForwardingPackLine packLine2;
		ForwardingConsol consol;

		#endregion
	}
}
