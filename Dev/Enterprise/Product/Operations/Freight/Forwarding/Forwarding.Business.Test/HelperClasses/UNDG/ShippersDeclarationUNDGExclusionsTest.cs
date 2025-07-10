using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShippersDeclarationUNDGExclusionsTest : TestCaseWithFactory
	{
		public void TestDoesPackLineHaveExtraUNDGDetails_TrueIfUNDGPackCountGreaterThanZero()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			var undg = packLine.UNDGs.AddNew();
			undg.DI_PackageCount = 1;

			AssertEquals(true, ShippersDeclarationUNDGExclusions.DoesPackLineHaveExtraUNDGDetails(packLine));
		}

		public void TestDoesPackLineHaveExtraUNDGDetails_TrueIfMoreThanOneUNDG()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			packLine.UNDGs.AddNew();
			packLine.UNDGs.AddNew();

			AssertEquals(true, ShippersDeclarationUNDGExclusions.DoesPackLineHaveExtraUNDGDetails(packLine));
		}

		public void TestDoesPackLineHaveExtraUNDGDetails_FalseIfPackLineHasNoUNDG()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();

			AssertEquals(false, ShippersDeclarationUNDGExclusions.DoesPackLineHaveExtraUNDGDetails(packLine));
		}

		public void TestDoesPackLineHaveExtraUNDGDetails_FalseIfPackLineHasOneUNDGWithNoCountWeightOrVolume()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			var undg = packLine.UNDGs.AddNew();

			AssertEquals(false, ShippersDeclarationUNDGExclusions.DoesPackLineHaveExtraUNDGDetails(packLine));
		}

		public void TestGetCountOfDangerousPacks_DoesntIncludeDangerousPacksExcludedFromDeclarationByUNNO()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			packLine.JL_PackageCount = 15;

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First();
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgExcludedDataItem = packLine.UNDGs.AddNew();
			undgExcludedDataItem.LinkDefault(subs);
			undgExcludedDataItem.DI_PackageCount = 10;

			AssertEquals("GetCountOfDangerousPacks doesn't include dangerous packs excluded from a shippers declaration by UNNO.",
				0, ShippersDeclarationUNDGExclusions.GetCountOfDangerousPacks(packLine));

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "123";
			subs2.DG_Variant = "a";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgIncludedDataItem = packLine.UNDGs.AddNew();
			undgIncludedDataItem.LinkDefault(subs2);
			undgIncludedDataItem.DI_PackageCount = 2;

			AssertEquals("GetCountOfDangerousPacks doesn't include dangerous packs excluded from a shippers declaration by UNNO.",
				15, ShippersDeclarationUNDGExclusions.GetCountOfDangerousPacks(packLine));
		}

		public void TestGetCountOfDangerousPacks_OnePackLine()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			packLine.JL_PackageCount = 15;

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.LinkDefault(subs);

			AssertEquals("GetCountOfDangerousPacks for packLine with 1 DG (without weight/volume/count) returns packLine pack count.",
				15, ShippersDeclarationUNDGExclusions.GetCountOfDangerousPacks(packLine));

			undgDataItem.DI_PackageCount = 10;

			AssertEquals("GetCountOfDangerousPacks for packLine with 1 DG (with a count) returns DG pack count.",
				15, ShippersDeclarationUNDGExclusions.GetCountOfDangerousPacks(packLine));

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "123";
			subs2.DG_Variant = "b";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgDataItem2 = packLine.UNDGs.AddNew();
			undgDataItem2.LinkDefault(subs2);
			undgDataItem2.DI_PackageCount = 2;

			AssertEquals("GetCountOfDangerousPacks for packLine with 2 DGs (with weight and count respectively) returns DG pack count.",
				15, ShippersDeclarationUNDGExclusions.GetCountOfDangerousPacks(packLine));
		}

		public void TestGetCountOfDangerousPacks_MultiplePackLines()
		{
			var packLine1 = Factory.NewWithValidTestData<PackLine>();
			packLine1.JL_PackageCount = 15;

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgDataItem1 = packLine1.UNDGs.AddNew();
			undgDataItem1.LinkDefault(subs);

			var packLine2 = Factory.NewWithValidTestData<PackLine>();
			packLine2.JL_PackageCount = 7;

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "123";
			subs2.DG_Variant = "b";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgDataItem2 = packLine2.UNDGs.AddNew();
			undgDataItem2.LinkDefault(subs2);
			undgDataItem2.DI_PackageCount = 7;

			var packLine3 = Factory.NewWithValidTestData<PackLine>();
			packLine3.JL_PackageCount = 15;

			var subs3 = Factory.New<UNDGSubstance>();
			subs3.DG_UNNO = "123";
			subs3.DG_Variant = "c";
			subs3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgDataItem3a = packLine3.UNDGs.AddNew();
			undgDataItem3a.LinkDefault(subs3);
			undgDataItem3a.DI_PackageCount = 10;
			var undgDataItem3b = packLine3.UNDGs.AddNew();
			undgDataItem3b.LinkDefault(subs3);
			undgDataItem3b.DI_PackageCount = 5;

			var packLineCollection = new List<PackLine>() { packLine1, packLine2, packLine3 };

			AssertEquals("GetCountOfDangerousPacks for collection of packlines iterates through all to return DG pack count.",
				37, ShippersDeclarationUNDGExclusions.GetCountOfDangerousPacks(packLineCollection));
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithDG()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_UNNO = "123";
			undgIATASubstance.DG_Variant = "a";

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = undgIATASubstance.PK;

			AssertEquals("Packline with IATA dangerous good requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithDGLimitedQuantity()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_UNNO = "3464";
			undgIATASubstance.DG_Variant = "C";
			undgIATASubstance.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E1;

			CombineAssertions("Pre-Condition: Limited Quantity and not excluded from shippers declaration", () =>
			{
				Assert(ExceptedQuantityUtilities.IsSubstancePermittedInLimitedQuantities(undgIATASubstance));
				Assert(!ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.Contains(undgIATASubstance.DG_UNNO));
			});

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = undgIATASubstance.PK;

			Assert(
				"Packline with LQTY permitted should only require declaration if weight/volume exceeds limit",
				!ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline)
			);
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithoutDG()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();

			AssertEquals("Packline without any dangerous good does not require declaration",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithDG_WithoutDGSubstance()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();
			packline.UNDGs.AddNew();

			AssertEquals("Dangerous goods that do not reference a DG substance are ignored",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_UNNO = "123";
			undgIATASubstance.DG_Variant = "a";

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = undgIATASubstance.PK;

			AssertEquals("Dangerous goods that do not reference a DG substance are ignored whilst the other DG's with a substance are still considered",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithExcludedDG()
		{
			CombineAssertions("Packline with excluded dangerous goods does not require declaration", () =>
			{
				foreach (var unnoExclusion in ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList)
				{
					var packline = Factory.NewWithValidTestData<PackLine>();
					var undgDataItem = packline.UNDGs.AddNew();
					var subs = Factory.New<UNDGSubstance>();
					subs.DG_Code = unnoExclusion;
					subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
					undgDataItem.DI_DG = subs.PK;
					undgDataItem.LinkDefault(subs);

					AssertEquals($"Packline with dangerous good UN{unnoExclusion} does not require declaration",
						false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
				}
			});
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithExcludedAndNonExcludedDG()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_UNNO = "123";
			undgIATASubstance.DG_Variant = "a";
			var undgExcludedSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgExcludedSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgExcludedSubstance.DG_UNNO = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First();

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = undgIATASubstance.PK;
			var undgDataItem2 = packline.UNDGs.AddNew();
			undgDataItem2.DI_DG = undgExcludedSubstance.PK;

			AssertEquals($"Packline with excluded dangerous good and nonexcluded dangerous good requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestDoesPackLineRequireDeclaration_ForIATA()
		{
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_UNNO = "123";
			undgIATASubstance.DG_Variant = "a";

			var packline = Factory.NewWithValidTestData<PackLine>();
			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = undgIATASubstance.PK;

			AssertEquals("Packline with IATA dangerous good requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			var undgIMOSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIMOSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgIMOSubstance.DG_UNNO = "123";
			undgIMOSubstance.DG_Variant = "b";

			undgDataItem.DI_DG = undgIMOSubstance.PK;

			AssertEquals("Packline with non IATA dangerous good does not require declaration",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			var undgDataItem2 = packline.UNDGs.AddNew();
			undgDataItem2.DI_DG = undgIATASubstance.PK;

			AssertEquals("Packline with IATA and non IATA dangerous goods requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestGetCountOfDangerousPacks_DoesntIncludeDangerousPacksWithExceededWeightOrVolume()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();
			packline.JL_ActualWeight = 150;
			packline.JL_ActualWeightUQ = "KG";
			packline.JL_ActualVolume = 150;
			packline.JL_ActualVolumeUQ = "L";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			undgSubstance.DG_ExceptedQuantityCode = "E1";

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "456";
			subs2.DG_Variant = "b";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgSubstance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "b", "IMO").FirstOrDefault();
			undgSubstance2.DG_ExceptedQuantityCode = "E5";

			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem1.DI_DG = subs.PK;
			undgDataItem1.LinkDefault(subs);
			undgDataItem1.DI_UnitOfVolume = Constants.Volume.Litre;
			undgDataItem1.DI_UnitOfWeight = Constants.Weight.Kilograms;
			undgDataItem1.DI_DGWeight = 30;

			packline.UNDGs.Add(undgDataItem1);

			AssertEquals("Packline with Excepted Quantity UNDGs exceeding DG weight require declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			undgDataItem1.DI_DGWeight = 0;
			undgDataItem1.DI_DGVolume = 25;

			AssertEquals("Packline with Excepted Quantity UNDGs exceeding DG volume require declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			undgDataItem1.DI_DGVolume = 0.01;
			undgDataItem1.DI_DGWeight = 0.01;

			AssertEquals("Packline with Excepted Quantity UNDGs not exceeding DG weight does not require declaration",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			var undgDataItem2 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem2.DI_DG = subs.PK;
			undgDataItem2.LinkDefault(subs);
			undgDataItem2.DI_UnitOfWeight = Constants.Weight.Kilograms;
			undgDataItem2.DI_DGWeight = 0.03;   // Max quantity allowded is 0.03, exceeds with addition of next undg2

			packline.UNDGs.Add(undgDataItem2);

			AssertEquals("Same UNDGs in a packline, should have weight added together and exceeding weight/volume requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			undgDataItem2.DI_DGWeight = 0.02;

			AssertEquals("Same UNDGs in a packline, should have weight added together and not exceeding weight/volume does not requires declaration",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			var undgDataItem3 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem3.DI_DG = subs2.PK;
			undgDataItem3.LinkDefault(subs2);
			undgDataItem3.DI_UnitOfWeight = Constants.Weight.Kilograms;
			undgDataItem3.DI_DGWeight = 0.05;

			packline.UNDGs.Add(undgDataItem3);

			AssertEquals("Different UNDGs in a packline should not have weight added together",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			var undgDataItem4 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem4.DI_DG = subs2.PK;
			undgDataItem4.LinkDefault(subs2);
			undgDataItem4.DI_UnitOfVolume = Constants.Volume.Litre;
			undgDataItem4.DI_DGVolume = 0.9;

			packline.UNDGs.Add(undgDataItem4);

			AssertEquals("Smae UNDGs in a packline should have weight/volume added together and exceeding volume requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithMultipleExceptedQuantityUNDG_Inner()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			undgSubstance.DG_ExceptedQuantityCode = "E1";

			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem1.DI_DG = subs.PK;
			undgDataItem1.LinkDefault(subs);
			undgDataItem1.DI_UnitOfWeight = Constants.Weight.Grams;
			undgDataItem1.DI_DGWeight = 30;

			var undgDataItem2 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem2.DI_DG = subs.PK;
			undgDataItem2.LinkDefault(subs);
			undgDataItem2.DI_UnitOfVolume = Constants.Volume.CubicInches;
			undgDataItem2.DI_DGVolume = 1.83;

			var undgDataItem3 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem3.DI_DG = subs.PK;
			undgDataItem3.LinkDefault(subs);
			undgDataItem3.DI_UnitOfWeight = Constants.Weight.Kilograms;
			undgDataItem3.DI_UnitOfVolume = Constants.Volume.Litre;

			packline.UNDGs.Add(undgDataItem1);
			packline.UNDGs.Add(undgDataItem2);
			packline.UNDGs.Add(undgDataItem3);

			AssertEquals("Packline with Excepted Quantity UNDGs (all E1) weight and volume sum on/under inner limit does not require declaration",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			undgDataItem3.DI_DGWeight = 0.001;

			AssertEquals("Packline with Excepted Quantity UNDGs (all E1) weight sum over inner limit requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));

			undgDataItem3.DI_DGWeight = 0;
			undgDataItem3.DI_DGVolume = 0.001;

			AssertEquals("Packline with Excepted Quantity UNDGs (all E1) volume sum over inner limit requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithSingleExceptedQuantityUNDG()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").FirstOrDefault();
			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.DI_DG = subs.PK;
			undgDataItem.LinkDefault(subs);
			undgDataItem.DI_PackageCount = 1;

			foreach (var exceptedQuantity in ExceptedQuantityUtilities.MaximumInnerQuantityDictionary)
			{
				CombineAssertions($"Single dangerous packline with excepted quantity code {exceptedQuantity.Key} (therefore inner quantity limits)", () =>
				{
					undgSubstance.DG_ExceptedQuantityCode = exceptedQuantity.Key;

					ExceptedQuantityUNDGTester(packLine, undgDataItem, exceptedQuantity.Value);
				});
			}
		}

		void ExceptedQuantityUNDGTester(PackLine packLine, UNDGDataItem undgToManipulate, decimal quantityAtLimit)
		{
			undgToManipulate.DI_UnitOfWeight = Constants.Weight.Kilograms;
			undgToManipulate.DI_UnitOfVolume = Constants.Volume.Litre;

			undgToManipulate.DI_DGWeight = quantityAtLimit;
			undgToManipulate.DI_DGVolume = quantityAtLimit;

			AssertEquals("Packline with UNDG weight and volume on excepted limit does not require declaration",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packLine));

			undgToManipulate.DI_DGWeight -= new ZDecimal(0.001);
			undgToManipulate.DI_DGVolume -= new ZDecimal(0.001);

			AssertEquals("Packline with UNDG weight and volume below excepted limit does not require declaration",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packLine));

			undgToManipulate.DI_DGWeight = quantityAtLimit + new ZDecimal(0.001);
			undgToManipulate.DI_DGVolume = quantityAtLimit;

			AssertEquals("Packline with UNDG weight over excepted limit requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packLine));

			undgToManipulate.DI_DGWeight = quantityAtLimit;
			undgToManipulate.DI_DGVolume = quantityAtLimit + new ZDecimal(0.001);

			AssertEquals("Packline with UNDG volume over excepted limit requires declaration",
				true, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packLine));
		}

		public void TestDoesPackLineRequireDeclaration_PacklineWithoutUNDGSubstanceLinked()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();
			var undgIATASubstance = Factory.New<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_UNNO = "123";
			undgIATASubstance.DG_Variant = "a";

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = ZGuid.Empty;
			undgDataItem.LinkDefault(undgIATASubstance);
			undgDataItem.DI_PackageCount = 1;

			AssertEquals($"Packline without UNDGSubstance linked (empty DI_DG)",
				false, ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(packline));
		}

		public void TestRequireLithiumBatteriesUNDGsDeclaration()
		{
			void AssertLithiumBattery(ZString unno, ZString packingInstructionSection)
			{
				var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
				undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				undgIATASubstance.DG_UNNO = unno;

				var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
				dangerousGood.DI_DG = undgIATASubstance.PK;
				dangerousGood.DI_PackingInstructionSection = packingInstructionSection;
				dangerousGood.DI_PackageCount = 2;

				var packLine = Factory.NewWithValidTestData<PackLine>();
				packLine.UNDGs.Add(dangerousGood);

				Assert(ShippersDeclarationUNDGExclusions.RequireLithiumBatteriesUNDGsDeclaration(packLine));
			}
			AssertLithiumBattery(LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries, PackingInstructionSectionTypeList.Codes.SectionIA);
			AssertLithiumBattery(LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries, PackingInstructionSectionTypeList.Codes.SectionIB);
			AssertLithiumBattery(LithiumBatteryConstants.UNNOCodes.PackedLithiumMetalBatteries, PackingInstructionSectionTypeList.Codes.SectionI);
			AssertLithiumBattery(LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries, PackingInstructionSectionTypeList.Codes.SectionIA);
			AssertLithiumBattery(LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries, PackingInstructionSectionTypeList.Codes.SectionIB);
			AssertLithiumBattery(LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries, PackingInstructionSectionTypeList.Codes.SectionI);
		}
	}
}
