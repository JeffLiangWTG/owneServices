using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsWarehouseUNDGTotalsHelperTest : WhsTestCaseWithFactory
	{
		#region TestLoadWhsWarehouseUNDGTotalsBySubstance

		public void TestLoadWhsWarehouseUNDGTotalsBySubstance()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsBySubstance(Factory, data.Whs1.PK, substance.PK);

			// Assert
			var result = results.Single();
			CombineAssertions(() =>
			{
				AssertEquals(20m, result.TotalWeight);
				AssertEquals("KG", result.TotalWeightUQ);
				AssertEquals(30m, result.TotalVolume);
				AssertEquals("M3", result.TotalVolumeUQ);
			});
		}

		#endregion

		#region TestLoadWhsWarehouseUNDGTotalsByCountryReference

		public void TestLoadWhsWarehouseUNDGTotalsByCountryReference()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 65;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);

			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsByCountryReference(Factory, data.Whs1.PK, countryReference.PK);

			// Assert
			var result = results.Single();
			CombineAssertions(() =>
			{
				AssertEquals(20m, result.TotalWeight);
				AssertEquals("KG", result.TotalWeightUQ);
				AssertEquals(30m, result.TotalVolume);
				AssertEquals("M3", result.TotalVolumeUQ);
			});
		}

		#endregion

		#region TestLoadWhsWarehouseUNDGTotalsByClass

		public void TestLoadWhsWarehouseUNDGTotalsByClass()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 65;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var undgClass = "1";
			var results = WhsWarehouseUNDGTotalsHelper.LoadWhsWarehouseUNDGTotalsByClass(Factory, data.Whs1.PK, undgClass);

			// Assert
			var result = results.Single();
			CombineAssertions(() =>
			{
				AssertEquals(20m, result.TotalWeight);
				AssertEquals("KG", result.TotalWeightUQ);
				AssertEquals(30m, result.TotalVolume);
				AssertEquals("M3", result.TotalVolumeUQ);
			});
		}

		#endregion

		#region TestLoadWhsWarehouseUNDGTotals

		public void TestLoadWhsWarehouseUNDGTotals()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 65;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);
			Helper.CreateWhsUNDGLimit(data.Whs1, "", "", countryReference, 40, "KG", 50, "M3");

			var undgClass = "1";
			Helper.CreateWhsUNDGLimit(data.Whs1, "", undgClass, null, 50, "KG", 60, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadUNDGTotalsForWarehouseInventory(Factory, data.Whs1.PK);

			// Assert
			AssertEquals(3, results.Count);
			var undgTotalsBySubstance = results.Single(x => x.UNDGSubStance.IsValid);
			var undgTotalsByCountryReference = results.Single(x => x.UNDGCountryReference.IsValid);
			var undgTotalsByClass = results.Single(x => !x.UNDGClass.IsEmpty);

			AssertQueryResult(undgTotalsBySubstance, substance.PK, ZGuid.Empty, ZString.Empty, 20m, 30m, "KG", 30m, 40m, "M3");
			AssertQueryResult(undgTotalsByCountryReference, ZGuid.Empty,  countryReference.PK, ZString.Empty, 20m, 40m, "KG", 30m, 50m, "M3");
			AssertQueryResult(undgTotalsByClass, ZGuid.Empty, ZGuid.Empty, "1", 20m, 50m, "KG", 30m, 60m, "M3");

			void AssertQueryResult(
				WhsWarehouseUNDGTotalsInfo warehouseUNDGTotalsInfo,
				ZGuid undgSubstance,
				ZGuid undgCountryReference,
				ZString undgClass,
				ZDecimal totalWeight,
				ZDecimal totalWeightLimit,
				ZString totalWeightLimitUQ,
				ZDecimal totalVolume,
				ZDecimal totalVolumeLimit,
				ZString totalVolumeLimitUQ
			) => CombineAssertions(() =>
			{
				AssertEquals(undgSubstance, warehouseUNDGTotalsInfo.UNDGSubStance);
				AssertEquals(undgCountryReference, warehouseUNDGTotalsInfo.UNDGCountryReference);
				AssertEquals(undgClass, warehouseUNDGTotalsInfo.UNDGClass);
				AssertEquals(totalWeight, warehouseUNDGTotalsInfo.TotalWeight);
				AssertEquals(totalWeightLimit, warehouseUNDGTotalsInfo.TotalWeightLimit);
				AssertEquals(totalWeightLimitUQ, warehouseUNDGTotalsInfo.TotalWeightLimitUQ);
				AssertEquals(totalVolume, warehouseUNDGTotalsInfo.TotalVolume);
				AssertEquals(totalVolumeLimit, warehouseUNDGTotalsInfo.TotalVolumeLimit);
				AssertEquals(totalVolumeLimitUQ, warehouseUNDGTotalsInfo.TotalVolumeLimitUQ);
			});
		}

		#endregion

		#region TestLoadProductsUNDGInfo

		public void TestLoadProductsUNDGInfo_StandardUQ()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 65;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "LB", 3m, "L", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results =
				WhsWarehouseUNDGTotalsHelper.LoadProductsUNDGInfo(Factory, new[] { data.Part1.PK }, "KG", "M3");

			// Assert
			var result = results.First();
			CombineAssertions(() =>
			{
				AssertEquals(data.Part1.PK, result.ProductPK);
				AssertEquals(substance.PK, result.DG_PK);
				AssertEquals(countryReference.PK, result.DCR_PK);
				AssertEquals("1", result.UNDGClass);
				AssertEquals(0.9072m, Math.Round(result.DG_Weight, 4));
				AssertEquals("KG", result.DG_WeightUQ);
				AssertEquals(0.003m, result.DG_Volume);
				AssertEquals("M3", result.DG_VolumeUQ);
			});
		}

		public void TestLoadProductsUNDGInfo_CustomUQ()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;
			data.Whs1.WW_DGThresholdPercentage = 65;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").FirstOrDefault();
			AssertEquals("1.1D", substance.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "LB", 3m, "L", substance.DG_Class);
			Helper.CreateWhsUNDGLimit(data.Whs1, substance, string.Empty, null, 30, "KG", 40, "M3");

			var countryReference = Helper.CreateCountryReference();
			Helper.CreateCountryReferencePivot(countryReference, substance);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			// Act
			var results = WhsWarehouseUNDGTotalsHelper.LoadProductsUNDGInfo(Factory, new[] { data.Part1.PK }, "LB", "L");

			// Assert
			var result = results.First();
			CombineAssertions(() =>
			{
				AssertEquals(data.Part1.PK, result.ProductPK);
				AssertEquals(substance.PK, result.DG_PK);
				AssertEquals(countryReference.PK, result.DCR_PK);
				AssertEquals("1", result.UNDGClass);
				AssertEquals(2m, result.DG_Weight);
				AssertEquals("LB", result.DG_WeightUQ);
				AssertEquals(3m, result.DG_Volume);
				AssertEquals("L", result.DG_VolumeUQ);
			});
		}

		#endregion

		#region TestLoadUNDGSubstanceCodes

		public void TestLoadUNDGSubstanceCodes_Empty()
		{
			var undgCode1 = "EEE";
			var undgClass1 = "1";
			var substance1 = Helper.CreateUNDGSubstance("s1", undgClass1, undgCode1);

			var undgCode2 = "DDD";
			var undgClass2 = "2";
			var substance2 = Helper.CreateUNDGSubstance("s2", undgClass2, undgCode2);

			var undgCode3 = "CCC";
			var undgClass3 = "3";
			var substance3 = Helper.CreateUNDGSubstance("s3", undgClass3, undgCode3);

			var undgCode4 = "BBB";
			var undgClass4 = "4";
			var substance4 = Helper.CreateUNDGSubstance("s4", undgClass4, undgCode4);

			var undgCode5 = "FFF";
			var undgClass5 = "5";
			var substance5 = Helper.CreateUNDGSubstance("s5", undgClass5, undgCode5);

			Factory.Save();

			var data = WhsWarehouseUNDGTotalsHelper.LoadUNDGSubstanceCodes(Factory, new[] { ZGuid.Empty });
			AssertEquals(0, data.Count);
		}

		public void TestLoadUNDGSubstanceCodes_Multiple()
		{
			var undgCode1 = "EEE";
			var undgClass1 = "1";
			var substance1 = Helper.CreateUNDGSubstance("s1", undgClass1, undgCode1);

			var undgCode2 = "DDD";
			var undgClass2 = "2";
			var substance2 = Helper.CreateUNDGSubstance("s2", undgClass2, undgCode2);

			var undgCode3 = "CCC";
			var undgClass3 = "3";
			var substance3 = Helper.CreateUNDGSubstance("s3", undgClass3, undgCode3);

			var undgCode4 = "BBB";
			var undgClass4 = "4";
			var substance4 = Helper.CreateUNDGSubstance("s4", undgClass4, undgCode4);

			var undgCode5 = "FFF";
			var undgClass5 = "5";
			var substance5 = Helper.CreateUNDGSubstance("s5", undgClass5, undgCode5);

			Factory.Save();

			var data = WhsWarehouseUNDGTotalsHelper.LoadUNDGSubstanceCodes(Factory, new[] { substance1.PK, substance3.PK, substance5.PK });
			CombineAssertions(() =>
			{
				AssertEquals(3, data.Count);
				AssertEquals(undgCode1, data[substance1.PK]);
				AssertEquals(undgCode3, data[substance3.PK]);
				AssertEquals(undgCode5, data[substance5.PK]);
			});
		}

		#endregion

		#region TestLoadUNDGCountryReferenceCodes

		public void TestLoadUNDGCountryReferenceCodes_Empty()
		{
			var reference1 = Helper.CreateCountryReference(referenceCode: "AU");
			var reference2 = Helper.CreateCountryReference(referenceCode: "NZ");
			var reference3 = Helper.CreateCountryReference(referenceCode: "SG");
			var reference4 = Helper.CreateCountryReference(referenceCode: "MY");
			var reference5 = Helper.CreateCountryReference(referenceCode: "JP");
			Factory.Save();

			var data = WhsWarehouseUNDGTotalsHelper.LoadUNDGCountryReferenceCodes(Factory, new[] { ZGuid.Empty });
			AssertEquals(0, data.Count);
		}

		public void TestLoadUNDGCountryReferenceCodes_Multiple()
		{
			var reference1 = Helper.CreateCountryReference(referenceCode: "AU");
			var reference2 = Helper.CreateCountryReference(referenceCode: "NZ");
			var reference3 = Helper.CreateCountryReference(referenceCode: "SG");
			var reference4 = Helper.CreateCountryReference(referenceCode: "MY");
			var reference5 = Helper.CreateCountryReference(referenceCode: "JP");
			Factory.Save();

			var data = WhsWarehouseUNDGTotalsHelper.LoadUNDGCountryReferenceCodes(Factory, new[] { reference1.PK, reference3.PK, reference5.PK });
			CombineAssertions(() =>
			{
				AssertEquals(3, data.Count);
				AssertEquals("AU", data[reference1.PK]);
				AssertEquals("SG", data[reference3.PK]);
				AssertEquals("JP", data[reference5.PK]);
			});
		}

		#endregion
	}
}
