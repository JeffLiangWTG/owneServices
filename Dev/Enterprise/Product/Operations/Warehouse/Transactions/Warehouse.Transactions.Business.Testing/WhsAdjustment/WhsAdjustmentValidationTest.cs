using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAdjustmentValidationTest : WhsDocketValidationTestCase<WhsAdjustment>
	{
		#region TestCheckWD_DocketType

		public void TestCheckWD_DocketType()
		{
			Docket.WD_DocketType = "";
			AssertEquals("Docket type validation accepting blank", true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Adjustment;
			AssertEquals("Docket type validation not accepting valid docket type: " + Docket.WD_DocketType, false, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Transfer;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Receive;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Order;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			// Also test a dodgy random type
			Docket.WD_DocketType = ";;;";
			AssertEquals("Docket type validation accepting junk", true, Docket.WD_DocketTypeInfo.HasErrors());
		}

		#endregion

		#region TestCheckWD_DocketStatus

		public void TestCheckWD_DocketStatus()
		{
			Docket.WD_DocketStatus = "";
			AssertEquals("Docket status validation accepting blank", true, Docket.WD_DocketStatusInfo.HasErrors());

			// here we test each type, but only Receive should be accepted
			for (int i = 0; i < Docket.Statuses.Count; i++)
			{
				Docket.WD_DocketStatus = Docket.Statuses[i].Code;
				if ((Docket.WD_DocketStatus == DocketStatus.Codes.New) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Entered) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Finalised))
				{
					AssertEquals("Docket status validation NOT accepting valid docket status: " + Docket.WD_DocketStatus, false, Docket.WD_DocketStatusInfo.HasErrors());
				}
				else
				{
					AssertEquals("Docket status validation accepting invalid docket status: " + Docket.WD_DocketStatus, true, Docket.WD_DocketStatusInfo.HasErrors());
				}
			}

			// Also test a dodgy random type
			Docket.WD_DocketStatus = ";;;";
			AssertEquals("Docket status validation accepting junk", true, Docket.WD_DocketStatusInfo.HasErrors());
		}

		#endregion

		#region TestCheckOwnershipAdjustedClient

		public void TestCheckOwnershipAdjustedClient()
		{
			var client = Factory.New<OrgHeader>();
			var adjustment = GetNewBusinessObject();
			var childAdjustment = GetNewBusinessObject();
			childAdjustment.WD_WD_ParentDocket = adjustment.PK;
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;

			adjustment.OwnershipAdjustedClientPK = ZGuid.Empty;
			AssertNoErrors("No errors should be there since Adjustment is a new Adjustment type.", adjustment.OwnershipAdjustedClientPKInfo);

			adjustment.OwnershipAdjustedClientPK = client.PK;
			AssertNoErrors("No errors should be there since Adjustment is a new Adjustment type.", adjustment.OwnershipAdjustedClientPKInfo);

			adjustment.OwnershipAdjustedClientPK = ZGuid.NewZGuid();
			AssertNoErrors("No errors should be there since Adjustment is a new Adjustment type.", adjustment.OwnershipAdjustedClientPKInfo);

			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			adjustment.OwnershipAdjustedClientPK = ZGuid.Empty;
			AssertHasError(adjustment.OwnershipAdjustedClientPKInfo, "Please enter a New Client.");

			adjustment.OwnershipAdjustedClientPK = client.PK;
			AssertNoErrors(adjustment.OwnershipAdjustedClientPKInfo);

			adjustment.WD_OH_Client = client.PK;
			adjustment.OwnershipAdjustedClientPK = client.PK;
			AssertHasError(adjustment.OwnershipAdjustedClientPKInfo, "Ownership Adjusted Client can't be same as Old Client.");

			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			adjustment.OwnershipAdjustedClientPK = client.PK;
			AssertNoErrors("Since adjustment is finalised, it should not trigger validation.", adjustment.OwnershipAdjustedClientPKInfo);
		}

		#endregion

		#region TestFinaliseDocket_UNDG_Adjustment

		public void TestFinaliseDocket_UNDG_AdjustmentIn_UnderLimit()
		{
			TestFinaliseDocket_UNDG_AdjustmentCore(1m, shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_AdjustmentIn_OverLimit()
		{
			TestFinaliseDocket_UNDG_AdjustmentCore(2m, shouldHaveError: true);
		}

		public void TestFinaliseDocket_UNDG_AdjustmentOut()
		{
			TestFinaliseDocket_UNDG_AdjustmentCore(-1m, shouldHaveError: false);
		}

		void TestFinaliseDocket_UNDG_AdjustmentCore(decimal adjustedQuantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 9m, location, "Pallet-1");
			Factory.Save();

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, adjustedQuantity, location);
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_F3_NKPackType = "XXX";
			adjustment.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, adjustment.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", adjustment.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, adjustment.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_ChangeProduct

		public void TestFinaliseDocket_UNDG_ChangeProduct()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 0m);
			warehouse.UNDGLimits.Add(undgLimit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "Pallet-1");
			Factory.Save();

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 11m, location);
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_F3_NKPackType = "XXX";
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, adjustment.WD_DocketStatusInfo.HasErrors());
			AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", adjustment.WD_DocketStatusInfo.GetErrors().Single().Message);

			line.WE_OP = data.Part2.PK;
			adjustment.FinaliseDocketWithoutUserConfirmation();

			// Assert
			AssertEquals(false, adjustment.WD_DocketStatusInfo.HasErrors());
		}

		#endregion

		#region TestFinaliseDocket_UNDG_UQ

		public void TestFinaliseDocket_UNDG_WeightUQ_UnderLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitWeightUQ: "T", shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_WeightUQ_OverLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitWeightUQ: "G", shouldHaveError: true);
		}

		public void TestFinaliseDocket_UNDG_VolumetUQ_UnderLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitVolumeUQ: "ML", shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_VolumeUQ_OverLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitVolumeUQ: "L", shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_UQCore(string undgLimitWeightUQ = "KG", string undgLimitVolumeUQ = "M3", bool shouldHaveError = false)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 1, totalWeightLimitUQ: undgLimitWeightUQ, totalVolumeLimit: 2, totalVolumeLimitUQ: undgLimitVolumeUQ);
			warehouse.UNDGLimits.Add(undgLimit);

			Factory.Save();

			// Act
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1, location);
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_F3_NKPackType = "XXX";
			adjustment.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, adjustment.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", adjustment.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, adjustment.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> ValidSubTypeForBondedWarehouse => new[] { AdjustmentType.Codes.Customs };

		protected override void AssertWD_OH_ClientCreditCheck(bool isForWeb)
		{
			Assert("Adjustments don't require credit check", true);
		}

		#endregion
	}
}
