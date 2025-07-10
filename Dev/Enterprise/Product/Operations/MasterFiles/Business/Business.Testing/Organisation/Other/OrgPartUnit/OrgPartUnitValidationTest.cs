using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPartUnitValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckForSelfReferenceConversions

		public void TestCheckForSelfReferenceConversions()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();

			unit1.OF_PackType = "PK1";
			unit1.OF_ParentPackType = "PK1";
			AssertHasError(unit1.OF_PackTypeInfo, OrgPartUnitValidation.SelfReferenceUnitError);
			AssertHasError(unit1.OF_ParentPackTypeInfo, OrgPartUnitValidation.SelfReferenceUnitError);

			unit1.OF_ParentPackType = "PK2";
			AssertNoError(unit1.OF_PackTypeInfo, OrgPartUnitValidation.SelfReferenceUnitError);
			AssertNoError(unit1.OF_ParentPackTypeInfo, OrgPartUnitValidation.SelfReferenceUnitError);

			unit1.OF_PackType = "PK2";
			AssertHasError(unit1.OF_PackTypeInfo, OrgPartUnitValidation.SelfReferenceUnitError);
			AssertHasError(unit1.OF_ParentPackTypeInfo, OrgPartUnitValidation.SelfReferenceUnitError);
		}

		#endregion

		#region TestCheckForDuplicateConversions

		public void TestCheckForDuplicateConversions()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();
			var unit2 = part.PartUnits.AddNew();
			var unit3 = part.PartUnits.AddNew();

			unit1.OF_PackType = "PK1";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit1.OF_ParentPackType = "PK2";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit2.OF_PackType = "PK3";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit2.OF_ParentPackType = "PK4";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit3.OF_PackType = "PK5";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit3.OF_ParentPackType = "PK2";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit3.OF_PackType = "PK1";
			AssertUnitErrors(unit1, unit2, unit3, false, false, false, false, true, true);
			unit3.OF_PackType = "PK5";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit2.OF_PackType = "PK5";
			AssertNoUnitErrors(unit1, unit2, unit3);
			unit2.OF_ParentPackType = "PK2";
			AssertUnitErrors(unit1, unit2, unit3, false, false, true, true, false, false);
		}

		#endregion

		#region TestCheckForLoops

		public void TestCheckForLoops()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();
			var unit2 = part.PartUnits.AddNew();
			var unit3 = part.PartUnits.AddNew();

			//
			//   PK1 - PK2
			//    \    /
			//     PK3
			unit1.OF_PackType = "PK1";
			unit1.OF_ParentPackType = "PK2";
			unit2.OF_PackType = "PK2";
			unit2.OF_ParentPackType = "PK3";
			unit3.OF_PackType = "PK3";
			unit3.OF_ParentPackType = "PK1";
			unit1.RunPreSaveValidation();
			unit2.RunPreSaveValidation();
			unit3.RunPreSaveValidation();
			AssertHasWarning(unit1.OF_PackTypeInfo, "This pack type is part of the loop: PK1->PK2->PK3->PK1.");
			AssertHasWarning(unit2.OF_PackTypeInfo, "This pack type is part of the loop: PK2->PK3->PK1->PK2.");
			AssertHasWarning(unit3.OF_PackTypeInfo, "This pack type is part of the loop: PK3->PK1->PK2->PK3.");
		}

		#endregion

		#region TestCheckForLoopsWithTail

		public void TestCheckForLoopsWithTail()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "PK0";
			var unit0 = part.PartUnits.AddNew();
			var unit1 = part.PartUnits.AddNew();
			var unit2 = part.PartUnits.AddNew();
			var unit3 = part.PartUnits.AddNew();

			//
			//  PK0 - PK1 - PK2
			//          \    /
			//           PK3
			unit0.OF_PackType = "PK0";
			unit0.OF_ParentPackType = "PK1";
			unit1.OF_PackType = "PK1";
			unit1.OF_ParentPackType = "PK2";
			unit2.OF_PackType = "PK2";
			unit2.OF_ParentPackType = "PK3";
			unit3.OF_PackType = "PK3";
			unit3.OF_ParentPackType = "PK1";

			unit0.RunPreSaveValidation();
			unit1.RunPreSaveValidation();
			unit2.RunPreSaveValidation();
			unit3.RunPreSaveValidation();

			AssertNoWarningContaining(unit0.OF_PackTypeInfo, "This pack type is part of the loop:");
			AssertHasWarningContaining(unit1.OF_PackTypeInfo, "This pack type is part of the loop:");
			AssertHasWarning(unit1.OF_PackTypeInfo, "This pack type is part of the loop: PK1->PK2->PK3->PK1.");
			AssertHasWarning(unit2.OF_PackTypeInfo, "This pack type is part of the loop: PK2->PK3->PK1->PK2.");
			AssertHasWarning(unit3.OF_PackTypeInfo, "This pack type is part of the loop: PK3->PK1->PK2->PK3.");
		}

		#endregion

		#region TestCheckForUnreachablePackType

		public void TestCheckForUnreachablePackType()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();
			var unit2 = part.PartUnits.AddNew();
			var unitWeight = part.PartUnits.AddNew();
			var unitVolume = part.PartUnits.AddNew();

			part.OP_StockKeepingUnit = "PK1";
			unit1.OF_PackType = "PK1";
			unit1.OF_ParentPackType = "PK2";
			unit2.OF_PackType = "PK7";
			unit2.OF_ParentPackType = "PK8";
			unitWeight.OF_PackType = "KG";
			unitWeight.OF_ParentPackType = "PK7";
			unitVolume.OF_PackType = "L";
			unitVolume.OF_ParentPackType = "PK7";

			unit1.RunPreSaveValidation();
			unit2.RunPreSaveValidation();

			AssertNoWarningContaining(unit1.OF_PackTypeInfo, "There is no conversion between");
			AssertHasWarningContaining(unit2.OF_PackTypeInfo, "There is no conversion between");
			AssertEquals(true, unit2.OF_PackTypeInfo.HasNotifications());
			AssertHasWarning(unit2.OF_PackTypeInfo, "There is no conversion between PK7 and the Stock Unit of the product, PK1.");
			AssertHasWarning(unit2.OF_ParentPackTypeInfo, "There is no conversion between PK8 and the Stock Unit of the product, PK1.");
			AssertNoWarnings("This conversion contains weight and should be ignored for this validation", unitWeight.OF_PackTypeInfo);
			AssertNoWarnings("This conversion contains volume and should be ignored for this validation", unitVolume.OF_PackTypeInfo);
		}

		public void TestCheckForUnreachablePackType_CaseInsensitive()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();
			var unit2 = part.PartUnits.AddNew();
			var unit3 = part.PartUnits.AddNew();
			var unit4 = part.PartUnits.AddNew();

			part.OP_StockKeepingUnit = "pk1";
			unit1.OF_PackType = "PK1";
			unit1.OF_ParentPackType = "PK2";
			unit1.OF_QuantityInParent = 2;
			unit2.OF_PackType = "pk2";
			unit2.OF_ParentPackType = "PK3";
			unit2.OF_QuantityInParent = 5;
			unit3.OF_PackType = "PK7";
			unit3.OF_ParentPackType = "PK8";
			unit3.OF_QuantityInParent = 10;
			unit4.OF_PackType = "m3";
			unit4.OF_ParentPackType = "kg";
			unit4.OF_QuantityInParent = 20;

			unit1.RunPreSaveValidation();
			unit2.RunPreSaveValidation();
			unit3.RunPreSaveValidation();
			unit4.RunPreSaveValidation();

			var message = "There is no conversion between";
			CombineAssertions(() =>
			{
				AssertNoWarningContaining(unit1.OF_PackTypeInfo, message);
				AssertNoWarningContaining(unit1.OF_ParentPackTypeInfo, message);
				AssertNoWarningContaining(unit2.OF_PackTypeInfo, message);
				AssertNoWarningContaining(unit2.OF_ParentPackTypeInfo, message);
				AssertHasWarningContaining(unit3.OF_PackTypeInfo, message);
				AssertHasWarning(unit3.OF_PackTypeInfo, $"{message} PK7 and the Stock Unit of the product, PK1.");
				AssertHasWarning(unit3.OF_ParentPackTypeInfo, $"{message} PK8 and the Stock Unit of the product, PK1.");
				AssertNoWarningContaining(unit4.OF_PackTypeInfo, message);
				AssertNoWarningContaining(unit4.OF_ParentPackTypeInfo, message);
			});
		}

		#endregion

		#region TestCheckForWrongMetricWeights

		public void TestCheckForWrongMetricWeights()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();
			part.OP_StockKeepingUnit = "KG";
			unit1.OF_QuantityInParent = 999;
			unit1.OF_PackType = "G";
			unit1.OF_ParentPackType = "KG";

			unit1.RunPreSaveValidation();

			AssertEquals(true, unit1.OF_PackTypeInfo.HasNotifications());
			AssertHasWarning(unit1.OF_QuantityInParentInfo, "Incorrect metric weight conversion. Should be 1000 G in KG.");
			AssertHasWarning(unit1.OF_PackTypeInfo, "Incorrect metric weight conversion. Should be 1000 G in KG.");
			AssertHasWarning(unit1.OF_ParentPackTypeInfo, "Incorrect metric weight conversion. Should be 1000 G in KG.");
		}

		#endregion

		#region TestCheckForValidPackTypes

		public void TestCheckOF_PackType_InvalidPackType()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();
			var unit2 = part.PartUnits.AddNew();
			unit1.OF_PackType = "kg";
			unit1.OF_ParentPackType = "XXX";
			unit2.OF_PackType = "YYY";
			unit2.OF_ParentPackType = "UNT";

			unit1.RunPreSaveValidation();
			unit2.RunPreSaveValidation();

			var message = "is not a valid pack type.";
			AssertNoWarningContaining(unit1.OF_PackTypeInfo, message);
			AssertHasWarning(unit1.OF_ParentPackTypeInfo, $"XXX {message}");
			AssertHasWarning(unit2.OF_PackTypeInfo, $"YYY {message}");
			AssertNoWarningContaining(unit2.OF_ParentPackTypeInfo, message);

			AssertHasWarningContaining(unit1.OF_ParentPackTypeInfo, message);
		}

		public void TestCheckOF_PackType_InvalidPackTypes_ErrorRegistryEnabled()
		{
			using (RawDataRegistry.Instance.UnitConversionPackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var part = Factory.New<OrgSupplierPart>();
				var unit1 = part.PartUnits.AddNew();
				var unit2 = part.PartUnits.AddNew();
				unit1.OF_PackType = "plt";
				unit1.OF_ParentPackType = "XXX";
				unit2.OF_PackType = "YYY";
				unit2.OF_ParentPackType = "L";

				unit1.RunPreSaveValidation();
				unit2.RunPreSaveValidation();

				var message = "is not a valid pack type.";
				AssertNoErrors(unit1.OF_PackTypeInfo);
				AssertHasError(unit1.OF_ParentPackTypeInfo, $"XXX {message}");
				AssertHasError(unit2.OF_PackTypeInfo, $"YYY {message}");
				AssertNoErrors(unit2.OF_ParentPackTypeInfo);
			}
		}

		public void TestCheckForUnreachablePackTypeAndCheckIfValidPackType_WarningOrder()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit1 = part.PartUnits.AddNew();

			part.OP_StockKeepingUnit = "PK1";
			unit1.OF_PackType = "XXX";
			unit1.OF_ParentPackType = "YYY";

			unit1.RunPreSaveValidation();

			var messageInvalidPackType = "is not a valid pack type.";
			var messageUnreachablePackType = "There is no conversion between";
			AssertHasWarning(unit1.OF_PackTypeInfo, $"XXX {messageInvalidPackType}");
			AssertHasWarning(unit1.OF_PackTypeInfo, $"{messageUnreachablePackType} XXX and the Stock Unit of the product, PK1.");
			AssertHasWarning(unit1.OF_ParentPackTypeInfo, $"YYY {messageInvalidPackType}");
			AssertHasWarning(unit1.OF_ParentPackTypeInfo, $"{messageUnreachablePackType} YYY and the Stock Unit of the product, PK1.");

			var packTypeMessages = unit1.OF_PackTypeInfo.Notifications.ToMessageListString();
			var parentPackTypeMessages = unit1.OF_ParentPackTypeInfo.Notifications.ToMessageListString();
			Assert("Invalid pack type warning should come before conversion path warning", packTypeMessages.IndexOf(messageInvalidPackType) < packTypeMessages.IndexOf(messageUnreachablePackType));
			Assert("Invalid pack type warning should come before conversion path warning", parentPackTypeMessages.IndexOf(messageInvalidPackType) < parentPackTypeMessages.IndexOf(messageUnreachablePackType));
		}

		#endregion

		#region TestHasPendingPicksByUOM

		public void TestHasPendingPicksByUOM()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			Factory.Save();
			warehouse[WhsWarehouseSchema.WW_IsPickByUOMEnabled.Name] = true;
			var clientPK = helper.CreateClient("CL");
			var part = (OrgSupplierPart)helper.CreateProduct(clientPK, "BEER");
			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = "UNT";
			partUnit.OF_QuantityInParent = 4;
			partUnit.OF_ParentPackType = "BOX";
			var receivePK = helper.CreateWhsReceive(clientPK, warehouse.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);
			Factory.Save();

			var orderPK = helper.CreateWhsOrder(clientPK, warehouse.PK, "O1", null);
			helper.CreateWhsOrderLine(orderPK, part.PK, 10);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });
			Factory.Save();

			warehouse[WhsWarehouseSchema.WW_IsPickByUOMEnabled.Name] = false;
			Factory.Save();

			var otherFactory1 = new BusinessObjectFactory();
			var partUnitInOtherFactory1 = otherFactory1.Load<OrgPartUnit>(partUnit.PK);
			Assert("Even though pick by UOM is disabled for Warehouse - the pick with packtypes still exists.", OrgPartUnitValidation.HasPendingPicksByUOM(partUnitInOtherFactory1));

			helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();

			var otherFactory2 = new BusinessObjectFactory();
			var partUnitInOtherFactory2 = otherFactory2.Load<OrgPartUnit>(partUnit.PK);
			AssertEquals("Pick is no longer pending.", false, OrgPartUnitValidation.HasPendingPicksByUOM(partUnitInOtherFactory2));
		}

		#endregion

		#region TestPendingUOMPicksDetected

		public void TestPendingUOMPicksDetected()
		{
			var partUnit = GetBoxPartUnitForTestingPendingUOMPicks(Factory);

			Assert(OrgPartUnitValidation.HasPendingPicksByUOM(partUnit));
			AssertEquals("Precondition:", 4m, partUnit.OF_QuantityInParent);
			AssertNoErrors("Precondition:", partUnit.OF_QuantityInParentInfo);

			partUnit.OF_QuantityInParent = 5;
			AssertHasError("Quantity change should result in error.", partUnit.OF_QuantityInParentInfo, OrgPartUnitValidation.PendingUOMPicksDetected);

			partUnit.OF_QuantityInParent = 4;
			AssertNoErrors("If quantity reverted to original - error should be cleared.", partUnit.OF_QuantityInParentInfo);

			AssertEquals("Precondition: Pack Type unchanged.", "UNT", partUnit.OF_PackType);
			AssertNoErrors("Precondition:", partUnit.OF_PackTypeInfo);
			partUnit.OF_PackType = "CAS";
			AssertHasError("Pack type change should result in error.", partUnit.OF_PackTypeInfo, OrgPartUnitValidation.PendingUOMPicksDetected);

			partUnit.OF_PackType = "UNT";
			AssertNoErrors("If pack type reverted to original - error should be cleared.", partUnit.OF_PackTypeInfo);

			AssertEquals("Precondition: Parent Pack Type unchanged.", "BOX", partUnit.OF_ParentPackType);
			AssertNoErrors("Precondition:", partUnit.OF_ParentPackTypeInfo);
			partUnit.OF_ParentPackType = "CAS";
			AssertHasError("Parent pack type change should result in error.", partUnit.OF_ParentPackTypeInfo, OrgPartUnitValidation.PendingUOMPicksDetected);

			partUnit.OF_ParentPackType = "BOX";
			AssertNoErrors("If parent pack type reverted to original - error should be cleared.", partUnit.OF_ParentPackTypeInfo);
		}

		#endregion

		#region TestPendingUOMPicksIgnoredForWeightConversions

		public void TestPendingUOMPicksIgnoredForWeightConversions()
		{
			var partUnit = GetBoxPartUnitForTestingPendingUOMPicks(Factory);
			Assert(OrgPartUnitValidation.HasPendingPicksByUOM(partUnit));

			var weightUnit = partUnit.SupplierPart.PartUnits.AddNew();
			weightUnit.OF_PackType = "UNT";
			weightUnit.OF_QuantityInParent = 3;
			weightUnit.OF_ParentPackType = "KG";

			var weightUnitInverted = partUnit.SupplierPart.PartUnits.AddNew();
			weightUnitInverted.OF_PackType = "OZ";
			weightUnitInverted.OF_QuantityInParent = 0.5;
			weightUnitInverted.OF_ParentPackType = "UNT";

			Factory.Save();

			AssertNoErrors("Precondition:", weightUnit);
			weightUnit.OF_QuantityInParent = 4;
			AssertNoErrors("Quantity change for weight unit should not cause PendingUOMPicksDetected error.", weightUnit.OF_QuantityInParentInfo);

			weightUnit.OF_PackType = "CAS";
			AssertNoErrors("Pack type change for weight unit should not cause PendingUOMPicksDetected error.", weightUnit.OF_PackTypeInfo);

			weightUnit.OF_ParentPackType = "G";
			AssertNoErrors("Parent pack type change for weight unit should not cause PendingUOMPicksDetected error.", weightUnit.OF_ParentPackTypeInfo);

			AssertNoErrors("Precondition:", weightUnitInverted);
			weightUnitInverted.OF_QuantityInParent = 0.6;
			AssertNoErrors("Quantity change for weight unit should not cause PendingUOMPicksDetected error.", weightUnitInverted.OF_QuantityInParentInfo);

			weightUnitInverted.OF_PackType = "G";
			AssertNoErrors("Pack type change for weight unit should not cause PendingUOMPicksDetected error.", weightUnitInverted.OF_PackTypeInfo);

			weightUnitInverted.OF_ParentPackType = "CAS";
			AssertNoErrors("Parent pack type change for weight unit should not cause PendingUOMPicksDetected error.", weightUnitInverted.OF_ParentPackTypeInfo);
		}

		#endregion

		#region TestPendingUOMPicksIgnoredForVolumeConversions

		public void TestPendingUOMPicksIgnoredForVolumeConversions()
		{
			var partUnit = GetBoxPartUnitForTestingPendingUOMPicks(Factory);
			Assert(OrgPartUnitValidation.HasPendingPicksByUOM(partUnit));

			var volumeUnit = partUnit.SupplierPart.PartUnits.AddNew();
			volumeUnit.OF_PackType = "UNT";
			volumeUnit.OF_QuantityInParent = 3;
			volumeUnit.OF_ParentPackType = "M3";

			var volumeUnitInverted = partUnit.SupplierPart.PartUnits.AddNew();
			volumeUnitInverted.OF_PackType = "L";
			volumeUnitInverted.OF_QuantityInParent = 0.5;
			volumeUnitInverted.OF_ParentPackType = "UNT";

			Factory.Save();

			AssertNoErrors("Precondition:", volumeUnit);
			volumeUnit.OF_QuantityInParent = 4;
			AssertNoErrors("Quantity change for volume unit should not cause PendingUOMPicksDetected error.", volumeUnit.OF_QuantityInParentInfo);

			volumeUnit.OF_PackType = "CAS";
			AssertNoErrors("Pack type change for volume unit should not cause PendingUOMPicksDetected error.", volumeUnit.OF_PackTypeInfo);

			volumeUnit.OF_ParentPackType = "D3";
			AssertNoErrors("Parent pack type change for volume unit should not cause PendingUOMPicksDetected error.", volumeUnit.OF_ParentPackTypeInfo);

			AssertNoErrors("Precondition:", volumeUnitInverted);
			volumeUnitInverted.OF_QuantityInParent = 0.6;
			AssertNoErrors("Quantity change for volume unit should not cause PendingUOMPicksDetected error.", volumeUnitInverted.OF_QuantityInParentInfo);

			volumeUnitInverted.OF_PackType = "ML";
			AssertNoErrors("Pack type change for volume unit should not cause PendingUOMPicksDetected error.", volumeUnitInverted.OF_PackTypeInfo);

			volumeUnitInverted.OF_ParentPackType = "CAS";
			AssertNoErrors("Parent pack type change for volume unit should not cause PendingUOMPicksDetected error.", volumeUnitInverted.OF_ParentPackTypeInfo);
		}

		#endregion

		#region TestPendingUOMPicksIgnoredForNewPartUnits

		public void TestPendingUOMPicksIgnoredForNewPartUnits()
		{
			var partUnit = GetBoxPartUnitForTestingPendingUOMPicks(Factory);
			Assert(OrgPartUnitValidation.HasPendingPicksByUOM(partUnit));

			var newUnit = partUnit.SupplierPart.PartUnits.AddNew();
			newUnit.OF_PackType = "UNT";
			newUnit.OF_QuantityInParent = 30;
			newUnit.OF_ParentPackType = "PLT";

			Factory.Save();

			AssertNoErrors("Precondition:", newUnit);
			newUnit.OF_QuantityInParent = 31;
			AssertHasError(newUnit.OF_QuantityInParentInfo, OrgPartUnitValidation.PendingUOMPicksDetected);
		}

		#endregion

		#region TestDifferentValuesForSameParentPackType

		public void TestDifferentValuesForSameParentPackType()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_MeasureUQ = Core.Constants.Length.Metres;
			part.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			part.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			var unit1 = part.PartUnits.AddNew();
			var unit2 = part.PartUnits.AddNew();

			unit1.OF_PackType = "UNT";
			unit1.OF_ParentPackType = "PLT";
			unit1.OF_QuantityInParent = 40;

			unit2.OF_PackType = "PKG";
			unit2.OF_ParentPackType = "PLT";
			unit2.OF_QuantityInParent = 4;

			AssertNoErrors(unit1);
			AssertNoErrors(unit2);

			DoActionAndAssertErrorExists(() => unit1.OF_Cubic = 1, unit1.OF_CubicInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Depth = 2, unit1.OF_DepthInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Height = 3, unit1.OF_HeightInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Width = 4, unit1.OF_WidthInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Weight = 5, unit1.OF_WeightInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");

			DoActionAndAssertErrorIsGone(() => { unit2.OF_Cubic = 1; unit1.Validation.ValidateOF_Cubic(); }, unit1.OF_CubicInfo);
			DoActionAndAssertErrorIsGone(() => { unit2.OF_Depth = 2; unit1.Validation.ValidateOF_Depth(); }, unit1.OF_DepthInfo);
			DoActionAndAssertErrorIsGone(() => { unit2.OF_Height = 3; unit1.Validation.ValidateOF_Height(); }, unit1.OF_HeightInfo);
			DoActionAndAssertErrorIsGone(() => { unit2.OF_Width = 4; unit1.Validation.ValidateOF_Width(); }, unit1.OF_WidthInfo);
			DoActionAndAssertErrorIsGone(() => { unit2.OF_Weight = 5; unit1.Validation.ValidateOF_Weight(); }, unit1.OF_WeightInfo);

			DoActionAndAssertErrorExists(() => unit1.OF_Cubic = 5, unit1.OF_CubicInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Depth = 6, unit1.OF_DepthInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Height = 7, unit1.OF_HeightInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Width = 8, unit1.OF_WidthInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");
			DoActionAndAssertErrorExists(() => unit1.OF_Weight = 9, unit1.OF_WeightInfo, "Should not have different values for '{0}' column for same Parent Pack Type 'PLT'.");

			unit2.Delete();

			DoActionAndAssertErrorIsGone(() => unit1.Validation.ValidateOF_Cubic(), unit1.OF_CubicInfo);
			DoActionAndAssertErrorIsGone(() => unit1.Validation.ValidateOF_Depth(), unit1.OF_DepthInfo);
			DoActionAndAssertErrorIsGone(() => unit1.Validation.ValidateOF_Height(), unit1.OF_HeightInfo);
			DoActionAndAssertErrorIsGone(() => unit1.Validation.ValidateOF_Width(), unit1.OF_WidthInfo);
			DoActionAndAssertErrorIsGone(() => unit1.Validation.ValidateOF_Weight(), unit1.OF_WeightInfo);
		}

		static void DoActionAndAssertErrorExists(Action action, ZPropertyInfo propertyInfo, string expectedErrorTemplate)
		{
			AssertNoErrors(propertyInfo);
			action();
			AssertHasError("Should have error on " + propertyInfo.Name, propertyInfo, string.Format(expectedErrorTemplate, propertyInfo.Description));
		}

		static void DoActionAndAssertErrorIsGone(Action action, ZPropertyInfo propertyInfo)
		{
			action();
			AssertNoErrors(propertyInfo);
		}

		#endregion

		#region TestMeasureValuesForStockUnit

		public void TestMeasureValuesForStockUnit()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_MeasureUQ = Core.Constants.Length.Metres;
			part.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			part.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			var unit = part.PartUnits.AddNew();
			unit.OF_PackType = "PLT";
			unit.OF_ParentPackType = "UNT";
			unit.OF_QuantityInParent = 0.01;

			AssertNoErrors(unit);
			part.OP_Cubic = 1;
			part.OP_Depth = 2;
			part.OP_Height = 3;
			part.OP_Width = 4;
			part.OP_Weight = 5;

			unit.Validation.ValidateAll();
			AssertNoErrors(unit);

			DoActionAndAssertErrorExists(() => unit.OF_Cubic = 6, unit.OF_CubicInfo, "Value for '{0}' should match corresponding property of a product definition as 'UNT' is a Stock Unit.");
			DoActionAndAssertErrorExists(() => unit.OF_Depth = 7, unit.OF_DepthInfo, "Value for '{0}' should match corresponding property of a product definition as 'UNT' is a Stock Unit.");
			DoActionAndAssertErrorExists(() => unit.OF_Height = 8, unit.OF_HeightInfo, "Value for '{0}' should match corresponding property of a product definition as 'UNT' is a Stock Unit.");
			DoActionAndAssertErrorExists(() => unit.OF_Width = 9, unit.OF_WidthInfo, "Value for '{0}' should match corresponding property of a product definition as 'UNT' is a Stock Unit.");
			DoActionAndAssertErrorExists(() => unit.OF_Weight = 10, unit.OF_WeightInfo, "Value for '{0}' should match corresponding property of a product definition as 'UNT' is a Stock Unit.");

			DoActionAndAssertErrorIsGone(() => unit.OF_Cubic = 1, unit.OF_CubicInfo);
			DoActionAndAssertErrorIsGone(() => unit.OF_Depth = 2, unit.OF_DepthInfo);
			DoActionAndAssertErrorIsGone(() => unit.OF_Height = 3, unit.OF_HeightInfo);
			DoActionAndAssertErrorIsGone(() => unit.OF_Width = 4, unit.OF_WidthInfo);
			DoActionAndAssertErrorIsGone(() => unit.OF_Weight = 5, unit.OF_WeightInfo);
		}

		#endregion

		#region TestDepth_ProductShouldHaveMeasureUQ

		public void TestDepth_ProductShouldHaveMeasureUQ()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit = part.PartUnits.AddNew();

			unit.Validation.ValidateOF_Depth();
			AssertNoErrors(unit.OF_DepthInfo);

			unit.OF_Depth = 1m;
			AssertHasError(unit.OF_DepthInfo, "Product Measurement UQ is required.");

			part.OP_MeasureUQ = Core.Constants.Length.Metres;
			unit.Validation.ValidateOF_Depth();
			AssertNoErrors(unit.OF_DepthInfo);
		}

		#endregion

		#region TestHeight_ProductShouldHaveMeasureUQ

		public void TestHeight_ProductShouldHaveMeasureUQ()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit = part.PartUnits.AddNew();

			unit.Validation.ValidateOF_Height();
			AssertNoErrors(unit.OF_HeightInfo);

			unit.OF_Height = 1m;
			AssertHasError(unit.OF_HeightInfo, "Product Measurement UQ is required.");

			part.OP_MeasureUQ = Core.Constants.Length.Metres;
			unit.Validation.ValidateOF_Height();
			AssertNoErrors(unit.OF_HeightInfo);
		}

		#endregion

		#region TestWidth_ProductShouldHaveMeasureUQ

		public void TestWidth_ProductShouldHaveMeasureUQ()
		{
			var part = Factory.New<OrgSupplierPart>();
			var unit = part.PartUnits.AddNew();

			unit.Validation.ValidateOF_Width();
			AssertNoErrors(unit.OF_WidthInfo);

			unit.OF_Width = 1m;
			AssertHasError(unit.OF_WidthInfo, "Product Measurement UQ is required.");

			part.OP_MeasureUQ = Core.Constants.Length.Metres;
			unit.Validation.ValidateOF_Width();
			AssertNoErrors(unit.OF_WidthInfo);
		}

		#endregion

		#region TestWeight_ProductShouldHaveWeightUQ

		public void TestWeight_ProductShouldHaveWeightUQ()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_WeightUQ = "";

			var unit = part.PartUnits.AddNew();

			unit.Validation.ValidateOF_Weight();
			AssertNoErrors(unit.OF_WeightInfo);

			unit.OF_Weight = 1m;
			AssertHasError(unit.OF_WeightInfo, "Product Weight UQ is required.");

			part.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			unit.Validation.ValidateOF_Weight();
			AssertNoErrors(unit.OF_WeightInfo);
		}

		#endregion

		#region TestCubic_ProductShouldHaveCubicUQ

		public void TestCubic_ProductShouldHaveCubicUQ()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_CubicUQ = "";

			var unit = part.PartUnits.AddNew();

			unit.Validation.ValidateOF_Cubic();
			AssertNoErrors(unit.OF_CubicInfo);

			unit.OF_Cubic = 1m;
			AssertHasError(unit.OF_CubicInfo, "Product Cubic UQ is required.");

			part.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			unit.Validation.ValidateOF_Cubic();
			AssertNoErrors(unit.OF_CubicInfo);
		}

		#endregion

		#region Test unit conversion for component used to build kit on sales order

		#region TestCantAddUnitConversionsIfThereAreBOMPicksOnTheFly

		public void TestCantAddUnitConversionsIfThereAreBOMPicksOnTheFly()
		{
			var component = Factory.NewWithValidTestData<OrgSupplierPart>();
			component.OP_PartNum = "Wheel";
			component.OP_Desc = "Wheel for a bike";

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(Factory, component.PK)).Returns(false);
				var partUnit = component.PartUnits.AddNew();
				partUnit.OF_PackType = "UNT";
				partUnit.OF_QuantityInParent = 4;
				partUnit.OF_ParentPackType = "CAS";
				partUnit.RunPreSaveValidation();
				AssertNoErrors("Should be no error if there are no picks on sales order.", partUnit);

				mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(Factory, component.PK)).Returns(true); // change stub value
				partUnit.Validation.ValidateAll();

				var errorMessage = OrgPartUnitValidation.PickOnSalesOrderDetected;
				AssertHasError("Should have an error.", partUnit.OF_QuantityInParentInfo, errorMessage);
				AssertHasError("Should have an error.", partUnit.OF_PackTypeInfo, errorMessage);
				AssertHasError("Should have an error.", partUnit.OF_ParentPackTypeInfo, errorMessage);
			}
		}

		#endregion

		#region TestCantDeleteUnitConversionsIfThereAreBOMPicksOnTheFly

		public void TestCantDeleteUnitConversionsIfThereAreBOMPicksOnTheFly()
		{
			var component = Factory.NewWithValidTestData<OrgSupplierPart>();
			component.OP_PartNum = "Wheel";
			component.OP_Desc = "Wheel for a bike";

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(Factory, component.PK)).Returns(false);
				var partUnit = component.PartUnits.AddNew();
				partUnit.OF_PackType = "UNT";
				partUnit.OF_QuantityInParent = 4;
				partUnit.OF_ParentPackType = "CAS";

				Factory.Save();
				Assert(partUnit.IsInDatabase);
				AssertNoErrors("Should be no error if there are no picks on sales order.", component.OP_PartNumInfo);
				Assert("We can delete unit conversion as there is no picks on sales order for a component.", partUnit.CanDelete);

				mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(Factory, component.PK)).Returns(true); // change stub value
				Assert("If there are picks on sales order for this component - we shouldn't be able to delete unit conversions.", !partUnit.CanDelete);
				AssertEquals(OrgPartUnitValidation.PickOnSalesOrderDetected, partUnit.ReasonForNotAbleToDelete);
			}
		}

		#endregion

		#region TestCantChangeUnitConversionsIfThereAreBOMPicksOnTheFly

		public void TestCantChangeUnitConversionsIfThereAreBOMPicksOnTheFly()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.FillWithValidTestData();
			part.OP_PartNum = "Leg";
			part.OP_Desc = "Table leg";
			part.OP_MeasureUQ = Core.Constants.Length.Metres;
			part.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			part.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			Factory.Save();

			var mockPickBomDetector = new Mock<IWhsPickOnSalesOrderDetector>();
			using (ObjectFactory.Substitute(mockPickBomDetector.Object))
			{
				mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(Factory, part.PK)).Returns(false);
				var partUnit = part.PartUnits.AddNew();
				partUnit.OF_PackType = "UNT";
				partUnit.OF_QuantityInParent = 4;
				partUnit.OF_ParentPackType = "CAS";

				Factory.Save();

				Assert("Precondition: product unit saved.", !partUnit.HasChanges && partUnit.IsInDatabase);
				AssertNoErrors(partUnit);

				// now let's assume there are picks on sales order for this product
				mockPickBomDetector.Setup(s => s.IsComponentUsedToBuiltKitOnSalesOrder(Factory, part.PK)).Returns(true); // change stub value
				partUnit.RunPreSaveValidation();
				AssertNoErrors("As there are no changes - no errors should be reported.", partUnit);

				// changing unit conversions for BOM part can affect on quantity of kits which should be build on the fly. So we must prevent it.
				partUnit.OF_QuantityInParent = 5;
				partUnit.OF_PackType = "PKG";
				partUnit.OF_ParentPackType = "PLT";

				var errorMessage = OrgPartUnitValidation.PickOnSalesOrderDetected;
				AssertHasError("Should have an error.", partUnit.OF_QuantityInParentInfo, errorMessage);
				AssertHasError("Should have an error.", partUnit.OF_PackTypeInfo, errorMessage);
				AssertHasError("Should have an error.", partUnit.OF_ParentPackTypeInfo, errorMessage);

				// reverting the change
				partUnit.OF_QuantityInParent = 4;
				partUnit.OF_PackType = "UNT";
				partUnit.OF_ParentPackType = "CAS";
				AssertNoErrors(partUnit);

				partUnit.OF_Weight = 1;
				partUnit.OF_Width = 2;
				partUnit.OF_Cubic = 3;
				partUnit.OF_Height = 4;
				partUnit.OF_Depth = 5;
				partUnit.Validation.ValidateAll();
				AssertNoErrors("Changing other fields should not trigger error.", partUnit);
			}
		}

		#endregion

		#endregion

		#region Test Helper Methods

		internal static OrgPartUnit GetBoxPartUnitForTestingPendingUOMPicks(BusinessObjectFactory factory)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			factory.Save();
			warehouse[WhsWarehouseSchema.WW_IsPickByUOMEnabled.Name] = true;
			var clientPK = helper.CreateClient("CL");
			var part = (OrgSupplierPart)helper.CreateProduct(clientPK, "BEER");
			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = "UNT";
			partUnit.OF_QuantityInParent = 4;
			partUnit.OF_ParentPackType = "BOX";
			var receivePK = helper.CreateWhsReceive(clientPK, warehouse.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);
			factory.Save();

			var orderPK = helper.CreateWhsOrder(clientPK, warehouse.PK, "O1", null);
			helper.CreateWhsOrderLine(orderPK, part.PK, 10);
			helper.CreateWhsPick(new[] { orderPK });

			factory.Save();

			return partUnit;
		}

		void AssertNoUnitErrors(OrgPartUnit unit1, OrgPartUnit unit2, OrgPartUnit unit3)
		{
			AssertUnitErrors(unit1, unit2, unit3, false, false, false, false, false, false);
		}

		void AssertUnitErrors(OrgPartUnit unit1, OrgPartUnit unit2, OrgPartUnit unit3, bool package1, bool parentPackage1, bool package2, bool parentPackage2, bool package3, bool parentPackage3)
		{
			AssertEquals(package1, unit1.OF_PackTypeInfo.HasError(OrgPartUnitValidation.DuplicateUnitError));
			AssertEquals(parentPackage1, unit1.OF_ParentPackTypeInfo.HasError(OrgPartUnitValidation.DuplicateUnitError));
			AssertEquals(package2, unit2.OF_PackTypeInfo.HasError(OrgPartUnitValidation.DuplicateUnitError));
			AssertEquals(parentPackage2, unit2.OF_ParentPackTypeInfo.HasError(OrgPartUnitValidation.DuplicateUnitError));
			AssertEquals(package3, unit3.OF_PackTypeInfo.HasError(OrgPartUnitValidation.DuplicateUnitError));
			AssertEquals(parentPackage3, unit3.OF_ParentPackTypeInfo.HasError(OrgPartUnitValidation.DuplicateUnitError));
		}

		#endregion
	}
}
