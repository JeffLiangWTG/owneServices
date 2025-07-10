using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DocketUNDGValidationHelperTest : WhsTestCaseWithFactory
	{
		#region TestCheckUNDGTotalsForWarehouse_DangerousGoodsManagement_Enable

		public void TestCheckUNDGTotalsForWarehouse_DangerousGoodsManagement_Enabled()
		{
			TestCheckUNDGTotalsForWarehouse_DangerousGoodsManagementCore(true, true);
		}

		public void TestCheckUNDGTotalsForWarehouse_DangerousGoodsManagement_NotEnabled()
		{
			TestCheckUNDGTotalsForWarehouse_DangerousGoodsManagementCore(false, false);
		}

		void TestCheckUNDGTotalsForWarehouse_DangerousGoodsManagementCore(bool isDangerousGoodsManagementEnabled, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDangerousGoodsManagementEnabled;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 2, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Substance Code 'AAA'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_LimitBySubstance

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstance_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstanceCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstance_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstanceCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitBySubstanceCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Substance Code 'AAA'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstance_MultipleSubstanceLimit_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstance_MultipleSubstanceLimitCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstance_MultipleSubstanceLimit_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstance_MultipleSubstanceLimitCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitBySubstance_MultipleSubstanceLimitCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode1 = "AAA";
			var undgCode2 = "BBB";
			var substance1 = Helper.CreateUNDGSubstance("1234", "1234", undgCode1);
			substance1.DG_Standard = "IMO";
			var substance2 = Helper.CreateUNDGSubstance("1235", "1234", undgCode2);
			substance2.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 2m, "KG", 3m, "M3", substance2.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode1, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, undgCode2, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			var expectedMessage = string.Empty;
			if (shouldHaveError)
			{
				expectedMessage = @"Substance Code 'AAA'
Substance Code 'BBB'
";
			}

			AssertEquals(expectedMessage, validationMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_LimitByCountryReference

		public void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByCountryReferenceCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByCountryReferenceCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitByCountryReferenceCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Country Reference 'AU'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleSubstance_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleSubstanceCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleSubstance_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleSubstanceCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleSubstanceCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var substance = Helper.CreateUNDGSubstance("1234", "1", "AAA");
			substance.DG_Standard = "IMO";

			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 0m, "M3", substance.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance, 1m, "KG", 0m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part2.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			var reference = Helper.CreateCountryReference();
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 0m);
			warehouse.UNDGLimits.Add(undgLimit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 9m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Country Reference '1234'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleCountryReferenceLimit_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleCountryReferenceLimitCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleCountryReferenceLimit_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleCountryReferenceLimitCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitByCountryReference_MultipleCountryReferenceLimitCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode1 = "AAA";
			var substance1 = Helper.CreateUNDGSubstance("1234", "1234", undgCode1);
			substance1.DG_Standard = "IMO";
			var undgCode2 = "BBB";
			var substance2 = Helper.CreateUNDGSubstance("1234", "1234", undgCode2);
			substance2.DG_Standard = "IMO";

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 2m, "KG", 3m, "M3", substance2.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard);
			var reference1 = Helper.CreateCountryReference(referenceCode: "AU1");
			var reference2 = Helper.CreateCountryReference(referenceCode: "AU2");
			Helper.CreateUNDGCountryReferencePivot(reference1.PK, substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard);
			Helper.CreateUNDGCountryReferencePivot(reference2.PK, substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference1, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference2, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = string.Empty;
			if (shouldHaveError)
			{
				expectedMessage = @"Country Reference 'AU1'
Country Reference 'AU2'
";
			}
			AssertEquals(expectedMessage, validationMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_LimitByClass

		public void TestCheckUNDGTotalsForWarehouse_LimitByClass_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByClassCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByClass_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByClassCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitByClassCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, "AAA");
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Class Code '1'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleSubstance_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleSubstanceCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleSubstance_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleSubstanceCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleSubstanceCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgClass = "1";
			var substance1 = Helper.CreateUNDGSubstance("1234", undgClass, "AAA");
			substance1.DG_Standard = "IMO";
			var substance2 = Helper.CreateUNDGSubstance("1235", undgClass, "BBB");
			substance2.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance1, 1m, "KG", 0m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance2, 1m, "KG", 0m, "M3", substance2.DG_Class);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 0m);
			warehouse.UNDGLimits.Add(undgLimit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 9m, data.Whs1.DefaultLocation, "Pallet-1");
			receive1.FinaliseDocket();
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Class Code '1'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleClassLimit_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleClassLimitCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleClassLimit_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleClassLimitCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitByClass_MultipleClassLimitCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgClass1 = "1";
			var substance1 = Helper.CreateUNDGSubstance("1234", undgClass1, "AAA");
			substance1.DG_Standard = "IMO";
			var undgClass2 = "2";
			var substance2 = Helper.CreateUNDGSubstance("1235", undgClass2, "BBB");
			substance2.DG_Standard = "IMO";

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 2m, "KG", 3m, "M3", substance2.DG_Class);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass1, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass2, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = string.Empty;
			if (shouldHaveError)
			{
				expectedMessage = @"Class Code '1'
Class Code '2'
";
			}

			AssertEquals(expectedMessage, validationMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClassCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClassCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClassCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 2m, "KG", 3m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = string.Empty;
			if (shouldHaveError)
			{
				expectedMessage = @"Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
";
			}

			AssertEquals(expectedMessage, validationMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_MultipleSubstances

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_MultipleSubstances_UnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_MultipleSubstancesCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_MultipleSubstances_OverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_MultipleSubstancesCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitBySubstanceAndCountryReferenceAndClass_MultipleSubstancesCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode1 = "AAA";
			var undgCode2 = "BBB";
			var undgClass1 = "1";
			var undgClass2 = "2";
			var substance1 = Helper.CreateUNDGSubstance("1234", undgClass1, undgCode1);
			substance1.DG_Standard = "IMO";
			var substance2 = Helper.CreateUNDGSubstance("1235", undgClass2, undgCode2);
			substance2.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance1.DG_UNNO, substance1.DG_Variant,
				substance1.DG_Standard);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 2m, "KG", 3m, "M3", substance2.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance2.DG_UNNO, substance2.DG_Variant,
				substance2.DG_Standard);

			var reference1 = Helper.CreateCountryReference(referenceCode: "AU1");
			Helper.CreateUNDGCountryReferencePivot(reference1.PK, substance1.DG_UNNO, substance1.DG_Variant,
				substance1.DG_Standard);

			var reference2 = Helper.CreateCountryReference(referenceCode: "AU2");
			Helper.CreateUNDGCountryReferencePivot(reference2.PK, substance2.DG_UNNO, substance2.DG_Variant,
				substance2.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 =
				Helper.CreateWhsUNDGLimit(warehouse, undgCode1, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference1,
				totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass1, null,
				totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var undgLimit4 =
				Helper.CreateWhsUNDGLimit(warehouse, undgCode2, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit4);

			var undgLimit5 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference2,
				totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit5);

			var undgLimit6 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass2, null,
				totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit6);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 =
				Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-1");
			Factory.Save();

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, receivedQuantity,
				data.Whs1.DefaultLocation, "Pallet-2");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive2.Factory, receive2.Warehouse, receive2.PK, receive2.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = string.Empty;
			if (shouldHaveError)
			{
				expectedMessage = @"Substance Code 'AAA'
Substance Code 'BBB'
Country Reference 'AU1'
Country Reference 'AU2'
Class Code '1'
Class Code '2'
";
			}

			AssertEquals(expectedMessage, validationMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_LimitUq

		public void TestCheckUNDGTotalsForWarehouse_LimitUq_WeightUnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_Uoq_WeightCore(4m, false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitUq_WeightOverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_Uoq_WeightCore(5m, true);
		}

		void TestCheckUNDGTotalsForWarehouse_Uoq_WeightCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 0m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalWeightLimitUQ: Constants.Weight.Pounds, totalVolumeLimit: 0m);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-1");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive.Factory, receive.Warehouse, receive.PK, receive.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Substance Code 'AAA'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitUq_VolumeUnderLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitUq_VolumeCore(1m, shouldHaveError: false);
		}

		public void TestCheckUNDGTotalsForWarehouse_LimitUq_VolumeOverLimit()
		{
			TestCheckUNDGTotalsForWarehouse_LimitUq_VolumeCore(2m, shouldHaveError: true);
		}

		void TestCheckUNDGTotalsForWarehouse_LimitUq_VolumeCore(decimal receivedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 0m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 0m, totalVolumeLimit: 1000m, totalVolumeLimitUQ: Constants.Volume.Litre);
			warehouse.UNDGLimits.Add(undgLimit);
			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, receivedQuantity, data.Whs1.DefaultLocation, "Pallet-1");
			var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive.Factory, receive.Warehouse, receive.PK, receive.Lines.ToArray()).OverLimitMessage;

			// Assert
			var expectedMessage = shouldHaveError ? "Substance Code 'AAA'\r\n" : string.Empty;
			AssertEquals(expectedMessage, validationMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_DBHits

		public void TestCheckUNDGTotalsForWarehouse_DBHits()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsDangerousGoodsManagementEnabled = true;

			var substance1 = Helper.CreateUNDGSubstance("1001", "1234", "S1");
			var substance2 = Helper.CreateUNDGSubstance("1002", "1234", "S2");
			var substance3 = Helper.CreateUNDGSubstance("1003", "1234", "S3");
			var substance4 = Helper.CreateUNDGSubstance("1004", "1234", "S4");
			var substance5 = Helper.CreateUNDGSubstance("1005", "1234", "S5");
			var substance6 = Helper.CreateUNDGSubstance("1006", "1234", "S6");
			var substance7 = Helper.CreateUNDGSubstance("1007", "1234", "S7");
			var substance8 = Helper.CreateUNDGSubstance("1008", "1234", "S8");
			var substance9 = Helper.CreateUNDGSubstance("1009", "1234", "S9");
			var substance10 = Helper.CreateUNDGSubstance("1010", "1234", "S10");
			substance1.DG_Standard = "IMO";
			substance2.DG_Standard = "IMO";
			substance3.DG_Standard = "IMO";
			substance4.DG_Standard = "IMO";
			substance5.DG_Standard = "IMO";
			substance6.DG_Standard = "IMO";
			substance7.DG_Standard = "IMO";
			substance8.DG_Standard = "IMO";
			substance9.DG_Standard = "IMO";
			substance10.DG_Standard = "IMO";

			Helper.CreateUNDGDataItem(data.Part1, substance1, 2m, "KG", 3m, "M3", substance1.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance2, 2m, "KG", 3m, "M3", substance2.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance3, 2m, "KG", 3m, "M3", substance3.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance4, 2m, "KG", 3m, "M3", substance4.DG_Class);
			Helper.CreateUNDGDataItem(data.Part1, substance5, 2m, "KG", 3m, "M3", substance5.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance6, 2m, "KG", 3m, "M3", substance6.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance7, 2m, "KG", 3m, "M3", substance7.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance8, 2m, "KG", 3m, "M3", substance8.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance9, 2m, "KG", 3m, "M3", substance9.DG_Class);
			Helper.CreateUNDGDataItem(data.Part2, substance10, 2m, "KG", 3m, "M3", substance10.DG_Class);

			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance2.DG_UNNO, substance2.DG_Variant, substance2.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance3.DG_UNNO, substance3.DG_Variant, substance3.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance4.DG_UNNO, substance4.DG_Variant, substance4.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance5.DG_UNNO, substance5.DG_Variant, substance5.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part2.PK, "DI", substance6.DG_UNNO, substance6.DG_Variant, substance6.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part2.PK, "DI", substance7.DG_UNNO, substance7.DG_Variant, substance7.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part2.PK, "DI", substance8.DG_UNNO, substance8.DG_Variant, substance8.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part2.PK, "DI", substance9.DG_UNNO, substance9.DG_Variant, substance9.DG_Standard);
			Helper.CreateUNDGSubstancePivot(data.Part2.PK, "DI", substance10.DG_UNNO, substance10.DG_Variant, substance10.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance1.DG_UNNO, substance1.DG_Variant, substance1.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "S1", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "S2", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, "S3", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit4 = Helper.CreateWhsUNDGLimit(warehouse, "S4", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit5 = Helper.CreateWhsUNDGLimit(warehouse, "S5", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit6 = Helper.CreateWhsUNDGLimit(warehouse, "S6", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit7 = Helper.CreateWhsUNDGLimit(warehouse, "S7", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit8 = Helper.CreateWhsUNDGLimit(warehouse, "S8", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit9 = Helper.CreateWhsUNDGLimit(warehouse, "S9", totalWeightLimit: 10m, totalVolumeLimit: 20m);
			var undgLimit10 = Helper.CreateWhsUNDGLimit(warehouse, "S10", totalWeightLimit: 10m, totalVolumeLimit: 20m);

			warehouse.UNDGLimits.Add(undgLimit1);
			warehouse.UNDGLimits.Add(undgLimit2);
			warehouse.UNDGLimits.Add(undgLimit3);
			warehouse.UNDGLimits.Add(undgLimit4);
			warehouse.UNDGLimits.Add(undgLimit5);
			warehouse.UNDGLimits.Add(undgLimit6);
			warehouse.UNDGLimits.Add(undgLimit7);
			warehouse.UNDGLimits.Add(undgLimit8);
			warehouse.UNDGLimits.Add(undgLimit9);
			warehouse.UNDGLimits.Add(undgLimit10);

			var undgLimit11 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit11);

			var undgLimit12 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, "1", null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit12);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, data.Whs1.DefaultLocation, "Pallet-11");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 5m, data.Whs1.DefaultLocation, "Pallet-12");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 6m, data.Whs1.DefaultLocation, "Pallet-13");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 7m, data.Whs1.DefaultLocation, "Pallet-14");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 8m, data.Whs1.DefaultLocation, "Pallet-15");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 9m, data.Whs1.DefaultLocation, "Pallet-16");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation, "Pallet-17");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 11m, data.Whs1.DefaultLocation, "Pallet-18");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 12m, data.Whs1.DefaultLocation, "Pallet-19");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 13m, data.Whs1.DefaultLocation, "Pallet-20");

			// Act
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation, "Pallet-21");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation, "Pallet-22");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation, "Pallet-23");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation, "Pallet-24");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation, "Pallet-25");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation, "Pallet-26");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, data.Whs1.DefaultLocation, "Pallet-27");

			Factory.Save();

			// Assert
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsUNDGLimitSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive2.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(newFactory, receiveInNewFactory.Warehouse, receiveInNewFactory.PK, receiveInNewFactory.Lines.ToArray()).OverLimitMessage;
				var expectedMessage = @"Substance Code 'S1'
Substance Code 'S2'
Substance Code 'S3'
Substance Code 'S4'
Substance Code 'S5'
Country Reference 'AU'
Class Code '1'
";
				AssertEquals(expectedMessage, validationMessage.ToString());
			}
		}

		#endregion

		#region TestBuildDocketUNDGValidationCache

		public void TestBuildDocketUNDGValidationCache()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 50;

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 100m, totalVolumeLimit: 110m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 210m, totalVolumeLimit: 220m);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.Save();

			// Act
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 200m, data.Whs1.DefaultLocation, "Pallet-1");
			var cache = DocketUNDGValidationHelper.BuildDocketUNDGValidationCache(receive);

			// Assert
			var overlimitUNDG = cache.DocketOverLimitUNDGs.Single(x => !x.UNDGSubStance.IsEmpty);
			CombineAssertions(() =>
			{
				AssertEquals(substance.PK, overlimitUNDG.UNDGSubStance);
				AssertEquals(0m, overlimitUNDG.TotalWeight);
				AssertEquals(100m, overlimitUNDG.TotalWeightLimit);
				AssertEquals("KG", overlimitUNDG.TotalWeightLimitUQ);
				AssertEquals(0m, overlimitUNDG.TotalVolume);
				AssertEquals(110m, overlimitUNDG.TotalVolumeLimit);
				AssertEquals("M3", overlimitUNDG.TotalVolumeLimitUQ);
			});

			var overThresholdUNDG = cache.DocketOverThresholdUNDGs.Single(x => !x.UNDGCountryReference.IsEmpty);
			CombineAssertions(() =>
			{
				AssertEquals(reference.PK, overThresholdUNDG.UNDGCountryReference);
				AssertEquals(0m, overThresholdUNDG.TotalWeight);
				AssertEquals(210m, overThresholdUNDG.TotalWeightLimit);
				AssertEquals("KG", overThresholdUNDG.TotalWeightLimitUQ);
				AssertEquals(0m, overThresholdUNDG.TotalVolume);
				AssertEquals(220m, overThresholdUNDG.TotalVolumeLimit);
				AssertEquals("M3", overThresholdUNDG.TotalVolumeLimitUQ);
			});

			var productInfosItem = cache.ProductUNDGInfos.ElementAt(0);
			var productUNDGInfos = productInfosItem.Value[0];
			CombineAssertions(() =>
			{
				AssertEquals(data.Part1.PK, productInfosItem.Key);
				AssertEquals(data.Part1.PK, productUNDGInfos.ProductPK);
				AssertEquals(substance.PK, productUNDGInfos.DG_PK);
				AssertEquals("AAA", productUNDGInfos.DG_Code);
				AssertEquals(reference.PK, productUNDGInfos.DCR_PK);
				AssertEquals("AU", productUNDGInfos.DCR_Code);
				AssertEquals("1", productUNDGInfos.UNDGClass);
				AssertEquals(1m, productUNDGInfos.DG_Weight);
				AssertEquals("KG", productUNDGInfos.DG_WeightUQ);
				AssertEquals(1m, productUNDGInfos.DG_Volume);
				AssertEquals("M3", productUNDGInfos.DG_VolumeUQ);
			});

			var dgCode = cache.DGCodes.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals(substance.PK, dgCode.Key);
				AssertEquals("AAA", dgCode.Value);
			});

			var dcrCode = cache.DCRCodes.ElementAt(0);
			CombineAssertions(() =>
			{
				AssertEquals(reference.PK, dcrCode.Key);
				AssertEquals("AU", dcrCode.Value);
			});
		}

		#endregion

		#region TestCheckUNDGTotalsForWarehouse_UseDocketUNDGValidationCache

		public void TestCheckUNDGTotalsForWarehouse_UseDocketUNDGValidationCache()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 50;

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 100m, totalVolumeLimit: 110m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 210m, totalVolumeLimit: 220m);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.Save();

			// Act
			var cache = BuildCache(data.Part1.PK, substance.PK, reference.PK);
			var result = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(cache);

			// Assert
			AssertEquals("Substance Code 'AAA'\r\n", result.OverLimitMessage);
		}

		#endregion

		#region TestCheckUNDGTotalsForProductsInWarehouse

		public void TestCheckUNDGTotalsForProductsInWarehouse()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 50;

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 100m, totalVolumeLimit: 110m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);
			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 210m, totalVolumeLimit: 220m);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.Save();

			// Act
			var cache = BuildCache(data.Part1.PK, substance.PK, reference.PK);
			var docketLineProductUNDGInfos = cache.ProductUNDGInfos[data.Part1.PK];
			var result = DocketUNDGValidationHelper.CheckUNDGTotalsForProductsInWarehouse(cache, docketLineProductUNDGInfos);

			// Assert
			AssertEquals("Substance Code 'AAA'\r\n", result.OverLimitMessage);
		}

		#endregion

		#region Implementation

		DocketUNDGValidationCache BuildCache(ZGuid productPK, ZGuid substancePK, ZGuid countryReferencePK)
		{
			var docketOverLimitUNDG = new WhsWarehouseUNDGTotalsInfo(substancePK, ZGuid.Empty, ZString.Empty, 0m, 100m, "KG", 0m, 100m, "M3");
			var docketOverLimitUNDGs = new[] { docketOverLimitUNDG };

			var docketOverThresholdUNDG = new WhsWarehouseUNDGTotalsInfo(ZGuid.Empty, countryReferencePK, ZString.Empty, 0m, 100m, "KG", 0m, 100m, "M3");
			var docketOverThresholdUNDGs = new[] { docketOverThresholdUNDG };

			var productInfo = new WhsProductUNDGInfo(productPK, substancePK, "AAA", countryReferencePK, "AU", "1", 1m, "KG", 1m, "M3");
			var productInfos = new Dictionary<ZGuid, WhsProductUNDGInfo[]>
			{
				{ productPK, new[] { productInfo } }
			};

			var dgCodes = new Dictionary<ZGuid, ZString> { { substancePK, "AAA" } };
			var dcrCodes = new Dictionary<ZGuid, ZString>() { { countryReferencePK, "AU" } };

			return new DocketUNDGValidationCache(docketOverLimitUNDGs, docketOverThresholdUNDGs, productInfos, dgCodes, dcrCodes);
		}

		#endregion
	}
}
