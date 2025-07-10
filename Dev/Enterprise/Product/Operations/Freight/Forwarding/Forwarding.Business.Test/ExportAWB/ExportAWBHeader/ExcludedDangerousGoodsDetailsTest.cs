using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Business.AWB.ExcludedDangerousGoodsDetails;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExcludedDangerousGoodsDetailsTest : TestCaseWithFactory
	{
		public void TestRequiresMeasurementDetailsUNDGs()
		{
			var unnoDoesNotRequireMeasurementDetails = "9999";
			AssertEquals(true, RequiresMeasurementDetailsUNDGs(ShippersDeclarationUNDGExclusions.UNNOCodes.UN1845));
			AssertEquals("Precondition", false, ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.Contains(unnoDoesNotRequireMeasurementDetails));
			AssertEquals(false, RequiresMeasurementDetailsUNDGs(unnoDoesNotRequireMeasurementDetails));
		}

		public void TestGetExcludedDangerousGoodDetailsElements_NoWeight()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 0;
			undg1.DI_DGWeight = 0;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					0,
					Weight.Kilograms,
					0,
					"Poison",
					false
				)
			});

			AssertNestedSequencesEqual("Expecting undgs to have no weight", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_SubstancePrefixIsUsed()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstanceCFR>();
			undgExcludedSubstance1.CFR_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.CFR_Prefix = "NA";
			undgExcludedSubstance1.CFR_PSN = "Poison";

			Factory.Save();

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 0;
			undg1.DI_DGWeight = 0;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"NA",
					firstShippersDeclarationUNDGExclusionCode,
					0,
					Weight.Kilograms,
					0,
					"Poison",
					false
				)
			});

			AssertNestedSequencesEqual("Expecting undgs to have no weight", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_PacklineWeight()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			packLineA.JL_ActualWeight = 1;
			packLineA.JL_ActualWeightUQ = Weight.Kilograms;
			packLineA.JL_PackageCount = 2;

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 0;
			undg1.DI_DGWeight = 0;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			packLineB.JL_ActualVolume = 2;
			packLineB.JL_ActualVolumeUQ = Volume.CubicMetres;
			packLineB.JL_PackageCount = 3;

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 0;
			undg2.DI_DGWeight = 0;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					1,
					Weight.Kilograms,
					2,
					"Poison",
					false
				),
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					2,
					Volume.CubicMetres,
					3,
					"Poison",
					false
				),
			});

			AssertNestedSequencesEqual("Expecting weight and volume", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_UNDGWeight()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 15;
			undg2.DI_DGVolume = 2.345;
			undg2.DI_UnitOfVolume = Volume.MegaLitre;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					1.234,
					Weight.Kilograms,
					5,
					"Poison",
					false
				),
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					2.345,
					Volume.MegaLitre,
					15,
					"Poison",
					false
				),
			});

			AssertNestedSequencesEqual("Expecting weight and volume", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_NoMerge()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			var lastShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.Last(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var undgExcludedSubstance2 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance2.DG_UNNO = lastShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance2.DG_PSN = "Cyanide";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 2.345;
			undg2.DI_UnitOfWeight = Volume.MegaLitre;

			var packLineC = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg3 = packLineC.UNDGs.AddNew();
			undg3.DI_DG = undgExcludedSubstance2.PK;
			undg3.LinkDefault(undgExcludedSubstance2);
			undg3.DI_PackageCount = 15;
			undg3.DI_DGVolume = 2.345;
			undg3.DI_UnitOfVolume = Volume.MegaLitre;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot3 = undg3.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot3.DP_IsDefault = true;
			uNDGSubstancePivot3.DP_UNNO = lastShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);
			AssertEquals("Precondition", undg3.Substance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					1.234,
					Weight.Kilograms,
					5,
					"Poison",
					false
				),
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					2.345,
					Volume.MegaLitre,
					6,
					"Poison",
					false
				),
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					lastShippersDeclarationUNDGExclusionCode,
					0,
					Weight.Kilograms,
					15,
					"Cyanide",
					false
				),
			});

			AssertNestedSequencesEqual("Expecting undgs to be grouped and merged", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_Merge()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 1.234;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					1.234,
					Weight.Kilograms,
					11,
					"Poison",
					false
				)
			});

			AssertNestedSequencesEqual("Expecting undgs to be grouped and merged", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_MultipleShipmentsDoNotMerge()
		{
			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "JMKIN";

			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Consol.Shipments[1].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[1].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 1.234;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					1.234,
					Weight.Kilograms,
					5,
					"Poison",
					false
				)
			});
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					1.234,
					Weight.Kilograms,
					6,
					"Poison",
					false
				)
			});

			AssertNestedSequencesEqual("Expecting undgs to be grouped and merged", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_DoesNotRequireMeasurementDetailsUNDGs_MergePackages()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGVolume = 2.345;
			undg2.DI_UnitOfVolume = Volume.CubicMetres;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					0,
					Weight.Kilograms,
					11,
					"Poison",
					false
				)
			});

			AssertNestedSequencesEqual("Expecting undgs to be grouped and merged", expectedResult, actualResult);
		}

		public void TestGetExcludedDangerousGoodDetailsElements_DoesNotRequireMeasurementDetailsUNDGs_DoNotMergePackages()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));

			var lastShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.Last(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var undgExcludedSubstance2 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance2.DG_UNNO = lastShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance2.DG_PSN = "Cyanide";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance2.PK;
			undg2.LinkDefault(undgExcludedSubstance2);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGVolume = 2.345;
			undg2.DI_UnitOfVolume = Volume.CubicMetres;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot1.DP_Variant = "Z";

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = lastShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot2.DP_Variant = "Z";

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);
			AssertEquals("Precondition", undg1.UNDGSubstance != null, true);
			AssertEquals("Precondition", undg2.UNDGSubstance != null, true);

			var actualResult = GetExcludedDangerousGoodDetailsElements(AWBHeader.Consol);
			var expectedResult = new List<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>>();
			expectedResult.Add(new List<UNDGNatureAndQuantityOfGoodsElement> {
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					firstShippersDeclarationUNDGExclusionCode,
					0,
					Weight.Kilograms,
					5,
					"Poison",
					false
				),
				new UNDGNatureAndQuantityOfGoodsElement(
					"UN",
					lastShippersDeclarationUNDGExclusionCode,
					0,
					Weight.Kilograms,
					6,
					"Cyanide",
					false
				)
			});

			AssertNestedSequencesEqual("Expecting undgs to not be grouped and merged", expectedResult, actualResult);
		}

		public void TestGetDetailLinesFromPackLineUNDGNatureAndQuantityOfGoodsElement_RequiresMeasurementDetails()
		{
			var shippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => RequiresMeasurementDetailsUNDGs(x));

			var undgNatureAndQuantityOfGoodsElement = new UNDGNatureAndQuantityOfGoodsElement(
				"UN",
				shippersDeclarationUNDGExclusionCode,
				1,
				Weight.Kilograms,
				2,
				"Poison",
				true
			);

			var expectedResult = new List<DetailLine>
			{
				new("UN " + shippersDeclarationUNDGExclusionCode + " (2x1KG)"),
				new("Poison", isText: true),
				new("Dangerous Goods in"),
				new("Excepted Quantities")
			};
			var actualResult = GetDetailLinesFromPackLineUNDGNatureAndQuantityOfGoodsElement(undgNatureAndQuantityOfGoodsElement);

			AssertSequencesEqual(expectedResult, actualResult);
		}

		public void TestGetDetailLinesFromPackLineUNDGNatureAndQuantityOfGoodsElement_DoesNotRequiresMeasurementDetails()
		{
			var shippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !RequiresMeasurementDetailsUNDGs(x));

			var undgNatureAndQuantityOfGoodsElement = new UNDGNatureAndQuantityOfGoodsElement(
				"UN",
				shippersDeclarationUNDGExclusionCode,
				1,
				Weight.Kilograms,
				2,
				"Poison",
				true
			);

			var expectedResult = new List<DetailLine>
			{
				new("UN " + shippersDeclarationUNDGExclusionCode + " (2 PKG)"),
				new("Poison", isText: true),
				new("Dangerous Goods in"),
				new("Excepted Quantities")
			};
			var actualResult = GetDetailLinesFromPackLineUNDGNatureAndQuantityOfGoodsElement(undgNatureAndQuantityOfGoodsElement);

			AssertSequencesEqual(expectedResult, actualResult);
		}

		public void AssertNestedSequencesEqual(ZString message,
			IEnumerable<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>> expectedResult,
			IEnumerable<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>> actualResult)
		{
			var expectedCount = expectedResult.Count();
			var actualCount = actualResult.Count();
			AssertEquals("Sequences have different lengths", expectedCount, actualCount);
			for (var i = 0; i < expectedCount; i++)
			{
				AssertSequencesEqual(message, expectedResult.ElementAt(i), actualResult.ElementAt(i));
			}
		}

		bool RequiresMeasurementDetailsUNDGs(ZString unno)
		{
			return unno == ShippersDeclarationUNDGExclusions.UNNOCodes.UN1845;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpForDeparturePort("JMKIN");
		}

		void SetUpForDeparturePort(ZString departurePort)
		{
			if (savedCountry.IsEmpty)
			{
				savedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}

			var departureCountry = departurePort.Left(2);

			GlbCompany.CurrentCompany.SetCountry(departureCountry);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = departurePort;

			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = departurePort;

			shipment.Consols.AddNew();
			shipment.Consols[0].JK_RL_NKLoadPort = departurePort;

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignor.PK;

			AWBHeader.EH_ParentID = shipment.Consols[0].PK;
			AWBHeader.Consol.JK_AgentType = AgentType.Agent;
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.SetCountry(savedCountry);
		}

		ZString savedCountry;

		ConsolExportAWBHeaderForTest AWBHeader;
	}
}
