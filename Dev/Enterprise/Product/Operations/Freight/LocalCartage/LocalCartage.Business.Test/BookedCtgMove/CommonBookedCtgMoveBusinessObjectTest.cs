using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonBookedCtgMove))]
	public class CommonBookedCtgMoveBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetAddressDropMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			var cneOrgHeader = Factory.New<OrgHeader>();
			cneOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			cneOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			var move = cartage.LooseBookedMoves.AddNew();
			var cneDocAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			cneDocAddress.E2_OA_Address = cneOrgHeader.MainAddress.PK;
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandUnloadLoad, move.GetAddressDropMode(cneDocAddress));
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, move.GetAddressDropMode(cneDocAddress));
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			AssertEquals(Constants.FCLEquipmentNeeded.Trailer, containerMove.GetAddressDropMode(cneDocAddress));
			var moveWithoutParent = Factory.New<CommonBookedCtgMove>();
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandUnloadLoad, moveWithoutParent.GetAddressDropMode(cneDocAddress));
			AssertEquals("", moveWithoutParent.GetAddressDropMode(null));
		}

		public void TestCreateDefaultLegs_Containerised()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage("ISM1", 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			move.CartageLegs.DeleteAll();
			move.CreateDefaultLegs();
			AssertEquals(2, move.CartageLegs.Count);
			var leg1 = move.CartageLegs[0];
			AssertEquals(cartage.FirstDocAddress.PK, leg1.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg1.JU_E2WaitPointAddressID);
			AssertEquals(cartage.SecondDocAddress.PK, leg1.JU_E2DeliveryAddressID);
			var leg2 = move.CartageLegs[1];
			AssertEquals(cartage.SecondDocAddress.PK, leg2.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg2.JU_E2WaitPointAddressID);
			AssertEquals(cartage.ThirdDocAddress.PK, leg2.JU_E2DeliveryAddressID);
		}

		public void TestCreateDefaultLegs_Loose()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage("ISM1", 1);
			var move = cartage.LooseBookedMoves[0];
			move.CartageLegs.DeleteAll();
			move.CreateDefaultLegs();
			AssertEquals(1, move.CartageLegs.Count);
			var leg1 = move.CartageLegs[0];
			AssertEquals(cartage.SecondDocAddress.PK, leg1.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg1.JU_E2WaitPointAddressID);
			AssertEquals(cartage.FourthDocAddress.PK, leg1.JU_E2DeliveryAddressID);
		}

		public void TesttDefaultAddresses_Containerised()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage("ISM1", 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			move.DefaultAddresses();
			AssertEquals(cartage.FirstDocAddress.PK, move.EW_E2PickupAddressID);
			AssertEquals(cartage.SecondDocAddress.PK, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
		}

		public void TestDefaultAddresses_Loose()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage("ISM1", 1);
			AssertEquals(DocAddressType.LocalCartageCTO, cartage.FirstDocAddress.DocAddressType);
			AssertEquals(DocAddressType.LocalCartageCFS, cartage.SecondDocAddress.DocAddressType);
			AssertEquals(DocAddressType.LocalCartageYard, cartage.ThirdDocAddress.DocAddressType);
			AssertEquals(DocAddressType.LocalCartageImporter, cartage.FourthDocAddress.DocAddressType);
			var move = cartage.LooseBookedMoves[0];
			move.DefaultAddresses();
			AssertEquals(1, move.CartageLegs.Count);
			AssertEquals(cartage.SecondDocAddress.PK, move.EW_E2PickupAddressID);
			AssertEquals(cartage.FourthDocAddress.PK, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
		}

		public void TestClearDeletedDocAddress()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "CNR Address", "2000", "Sydney", "AUSYD", true);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "CNE Address", "2000", "Sydney", "AUSYD", true);
			move.EW_E2PickupAddressID = cnr.PK;
			move.EW_E2WaitPointAddressID = cne.PK;
			move.EW_E2DeliveryAddressID = cne.PK;
			AssertNotNull(move.EW_E2PickupAddressID);
			AssertNotNull(move.EW_E2WaitPointAddressID);
			AssertNotNull(move.EW_E2DeliveryAddressID);
			move.ClearDeletedDocAddress(cnr.PK);
			AssertEquals(ZGuid.Empty, move.EW_E2PickupAddressID);
			AssertNotNull(move.EW_E2WaitPointAddressID);
			AssertNotNull(move.EW_E2DeliveryAddressID);
			move.ClearDeletedDocAddress(cne.PK);
			AssertEquals(ZGuid.Empty, move.EW_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
		}

		public void TestEW_JC_Container()
		{
			var move = Factory.New<CommonBookedCtgMove>();
			var container1 = Factory.New<CommonContainer>();
			var container2 = Factory.New<CommonContainer>();
			AssertNoExceptionThrown("Initial container should be able to set.", () => move.EW_JC_Container = container1.PK);
			AssertExceptionThrown("Container should not be changed on a booked move.", typeof(InvalidOperationException), "Container should not be changed unless it's removed.", () => move.EW_JC_Container = container2.PK);
			AssertNoExceptionThrown("Container on a booked move can be removed.", () => move.EW_JC_Container = ZGuid.Empty);
			AssertNoExceptionThrown("When container is not assiged it should be able to assign a container.", () => move.EW_JC_Container = container2.PK);
		}

		public void TestDelete_ContainerIsDeleted()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			move.Delete();
			Assert("Deleting a move should delete its container", container.IsDeleted);
			Assert("Move is deleted", move.IsDeleted);
		}

		public void TestDelete_NoContainer_NoExceptionThrown()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = Factory.New<CommonBookedCtgMove>();
			move.EW_JJ = cartage.PK;
			AssertNotNull("Precondition", move.Cartage);
			AssertNull("Precondition", move.Container);
			AssertNoExceptionThrown("Should not try deleting container if it does not exist", () => move.Delete());
			Assert("Move is deleted", move.IsDeleted);
		}

		public void TestDelete_NoCartage_ContainerIsNotDeleted()
		{
			var move = Factory.New<CommonBookedCtgMove>();
			var container = Factory.New<CommonContainer>();
			move.EW_JC_Container = container.PK;
			AssertNotNull("Precondition", move.Container);
			AssertNull("Precondition", move.Cartage);
			move.Delete();
			AssertEquals("Container should not be deleted", false, container.IsDeleted);
			Assert("Move is deleted", move.IsDeleted);
		}

		public void TestDelete_CartageHasParent_ContainerIsNotDeleted()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var cartage = Factory.New<CartageForTest>();
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			move.Cartage.SetParent(dummyParent);
			Assert("Precondition", move.Cartage.HasParent);
			move.Delete();
			AssertEquals("Container should not be deleted", false, container.IsDeleted);
			Assert("Move is deleted", move.IsDeleted);
		}

		public void TestDelete_CartageLegsAreDeleted()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var cartage = Helper.CreateCartage("ISM1", 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			move.CartageLegs.DeleteAll();
			move.CreateDefaultLegs();
			AssertEquals("Precondition", 2, move.CartageLegs.Count);
			var leg1 = move.CartageLegs[0];
			var leg2 = move.CartageLegs[1];
			move.Delete();
			CombineAssertions("Should delete cartage legs", () =>
			{
				Assert(leg1.IsDeleted);
				Assert(leg2.IsDeleted);
			});
			Assert("Move is deleted", move.IsDeleted);
		}

		public void TestSetDropModeToCartageDropModeFallbackIfEmpty()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.Haulier;
			var cneOrgHeader = Factory.New<OrgHeader>();
			cneOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cneOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var cneDocAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			cneDocAddress.E2_OA_Address = cneOrgHeader.MainAddress.PK;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			looseMove.EW_E2PickupAddressID = cneDocAddress.PK;
			looseMove.EW_DropMode = "";
			looseMove.SetDropModeToCartageDropModeFallbackIfEmpty();
			AssertEquals("Takes parent cartage drop mode", Constants.LCLAIREquipmentNeeded.Premise, looseMove.EW_DropMode);
			cartage.JJ_DropMode = "";
			looseMove.SetDropModeToCartageDropModeFallbackIfEmpty();
			AssertEquals("Takes the loose drop mode from Address", Constants.LCLAIREquipmentNeeded.HandHaulier, looseMove.EW_DropMode);
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = "";
			looseMove.SetDropModeToCartageDropModeFallbackIfEmpty();
			AssertEquals("Use fallback Defaulting to Cartage Type", Constants.LCLAIREquipmentNeeded.Haulier, looseMove.EW_DropMode);
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = "";
			looseMove.SetDropModeToCartageDropModeFallbackIfEmpty();
			AssertEquals("Should not change drop mode because all fallbacks are empty", Constants.LCLAIREquipmentNeeded.Haulier, looseMove.EW_DropMode);
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			looseMove.EW_DropMode = "";
			((ISupportDataImporting)cartage).IsImportingData = true;
			looseMove.SetDropModeToCartageDropModeFallbackIfEmpty();
			AssertEquals("Does not set drop mode when importing", "", looseMove.EW_DropMode);
			((ISupportDataImporting)cartage).IsImportingData = false;
			looseMove.EW_JJ = ZGuid.Empty;
			looseMove.SetDropModeToCartageDropModeFallbackIfEmpty();
			AssertEquals("Does not set drop mode when cartage is null", "", looseMove.EW_DropMode);
		}

		public void TestCommonBookedCtgMoveBehaviourStrategySetsDropModeCorrectly()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			var cneOrgHeader = Factory.New<OrgHeader>();
			cneOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cneOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Haulier;
			var bookedMove = cartage.LooseBookedMoves.AddNew();
			var cneDocAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			var cfsAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS);
			cfsAddress.E2_OA_Address = cneOrgHeader.MainAddress.PK;
			cneDocAddress.E2_OA_Address = cneOrgHeader.MainAddress.PK;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.SetNotificationSubscriber(notify);
			bookedMove.EW_DropMode = "";
			bookedMove.EW_E2DeliveryAddressID = cneDocAddress.PK;
			AssertNull("Since Drop Mode was empty defaulting was done without asking", notify.LastEventYesNoArgs);
			AssertEquals("move1.EW_DropMode", Constants.LCLAIREquipmentNeeded.HandHaulier, bookedMove.EW_DropMode);
			bookedMove.EW_E2DeliveryAddressID = ZGuid.Empty;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = "";
			bookedMove.EW_E2DeliveryAddressID = cneDocAddress.PK;
			AssertNull("Since Drop Mode on address was empty no popup was triggered", notify.LastEventYesNoArgs);
			bookedMove.EW_E2DeliveryAddressID = ZGuid.Empty;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			bookedMove.EW_E2DeliveryAddressID = cneDocAddress.PK;
			AssertEquals("Caption of Popup is correct", "Populate Booked Move Drop Mode", notify.LastEventYesNoArgs.Caption);
			AssertEquals("Message of Popup is correct", "Would you like to set the Booked Move Drop Mode with the Consignee Drop Mode 'PSL'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Answered No so Drop Mode stays the same", Constants.LCLAIREquipmentNeeded.HandHaulier, bookedMove.EW_DropMode);
			bookedMove.EW_E2DeliveryAddressID = ZGuid.Empty;
			notify.ResponseToDialogs = true;
			bookedMove.EW_E2DeliveryAddressID = cneDocAddress.PK;
			AssertEquals("Answered Yes so Drop Mode changes", Constants.LCLAIREquipmentNeeded.Premise, bookedMove.EW_DropMode);
			bookedMove.EW_E2DeliveryAddressID = ZGuid.Empty;
			var previousEventArgs = notify.LastEventYesNoArgs;
			bookedMove.EW_E2DeliveryAddressID = cneDocAddress.PK;
			AssertEquals("Address has same Drop Mode so no popup", previousEventArgs, notify.LastEventYesNoArgs);
			AssertEquals("Address has same Drop Mode so no changes", Constants.LCLAIREquipmentNeeded.Premise, bookedMove.EW_DropMode);
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Haulier;
			bookedMove.EW_E2PickupAddressID = cfsAddress.PK;
			AssertEquals("CFS was changed so no popup", previousEventArgs, notify.LastEventYesNoArgs);
			AssertEquals("CFS was changed so no changes to drop mode", Constants.LCLAIREquipmentNeeded.Premise, bookedMove.EW_DropMode);
			bookedMove.EW_DropMode = "";
			bookedMove.EW_E2PickupAddressID = ZGuid.Empty;
			bookedMove.EW_E2PickupAddressID = cfsAddress.PK;
			AssertEquals("CFS was changed so no popup", previousEventArgs, notify.LastEventYesNoArgs);
			AssertEquals("CFS was changed so no changes to drop mode", "", bookedMove.EW_DropMode);
			bookedMove.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			bookedMove.EW_E2PickupAddressID = cneDocAddress.PK;
			AssertNotEquals("Address has different Drop Mode", previousEventArgs, notify.LastEventYesNoArgs);
			AssertEquals("Answered Yes so Drop Mode changes", Constants.LCLAIREquipmentNeeded.HandHaulier, bookedMove.EW_DropMode);
			AssertEquals("Precondition", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			cartage.AddJobDocAddress(cneDocAddress.PK);
			bookedMove.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			((IDocAddresses)cartage).DocAddressChanged(cneDocAddress);
			AssertEquals("Drop mode was not changed on Cartage when triggering DocAddressChange from Booked Move", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.EW_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			notify = new TestNotify();
			notify.ResponseToDialogs = true;
			cartage.SetNotificationSubscriber(notify);
			containerMove.EW_E2PickupAddressID = ZGuid.Empty;
			containerMove.EW_E2PickupAddressID = cfsAddress.PK;
			AssertEquals("CFS should trigger popup if it is the requested Address Type", "Would you like to set the Booked Move Drop Mode with the CFS Drop Mode 'LOF'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Answer was yes so drop mode should change", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove.EW_DropMode);
		}

		public void TestPopupIsNotTriggeredWhenAddressesAreBeingDefaulted()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var cneOrgHeader = Factory.New<OrgHeader>();
			cneOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = cneOrgHeader.MainAddress.PK;
			var bookedMove = cartage.ContainerBookedMoves.AddNew();
			var newAddress = cneOrgHeader.Addresses.AddNew();
			newAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			address.E2_OA_Address = newAddress.PK;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			cartage.SetNotificationSubscriber(notify);
			bookedMove.DefaultAddresses();
			AssertNull("Since Addresses were defaulting, no popup was triggered", notify.LastEventYesNoArgs);
			AssertEquals("bookedMove.EW_DropMode", Constants.FCLEquipmentNeeded.LiftOffOn, bookedMove.EW_DropMode);
		}

		public void TestDefaultDropMode()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonCartageType iSMY = Factory.New<CommonCartageType>();
			iSMY.E3_JobType = "ISM1";
			CommonCartageOrg cTO = iSMY.CommonCartageOrganisations.AddNew();
			cTO.E5_OrgType = "CTO";
			CommonCartageOrg cFS = iSMY.CommonCartageOrganisations.AddNew();
			cFS.E5_OrgType = "CFS";
			CommonCartageOrg cNE = iSMY.CommonCartageOrganisations.AddNew();
			cNE.E5_OrgType = "CNE";
			CommonCartageOrg cYD = iSMY.CommonCartageOrganisations.AddNew();
			cYD.E5_OrgType = "CYD";
			CommonCartageLegType containerMove = iSMY.ContainerizedBookedMoveTypes.AddNew();
			containerMove.E4_ContainerMode = "CNT";
			containerMove.E4_E5_FromOrg = cTO.PK;
			containerMove.E4_E5_WaitPointOrg = cFS.PK;
			//containerMove.E4_E5_ToOrg = CYD.PK;
			containerMove.E4_EquipmentGroup = Constants.FCLEquipmentNeeded.SideLoader;
			CommonCartageLegType containerLeg1 = iSMY.ContainerizedCartageLegTypes.AddNew();
			containerLeg1.E4_ContainerMode = "CNT";
			containerLeg1.E4_E5_FromOrg = cTO.PK;
			containerLeg1.E4_E5_ToOrg = cFS.PK;
			CommonCartageLegType containerLeg2 = iSMY.ContainerizedCartageLegTypes.AddNew();
			containerLeg2.E4_ContainerMode = "CNT";
			containerLeg2.E4_E5_FromOrg = cFS.PK;
			containerLeg2.E4_E5_ToOrg = cYD.PK;
			CommonCartageLegType looseMove = iSMY.LooseBookedMoveTypes.AddNew();
			looseMove.E4_ContainerMode = "LSE";
			looseMove.E4_E5_FromOrg = cFS.PK;
			looseMove.E4_E5_WaitPointOrg = cNE.PK;
			looseMove.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.HandHaulier;
			CommonCartageLegType looseLeg = iSMY.LooseCartageLegTypes.AddNew();
			looseLeg.E4_ContainerMode = "LSE";
			looseLeg.E4_E5_FromOrg = cFS.PK;
			looseLeg.E4_E5_ToOrg = cNE.PK;
			//Fallback: Cartage Type
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "ISM1";
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			CommonBookedCtgMove containerCartageMove = cartage.ContainerBookedMoves.AddNew();
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, containerCartageMove.EW_DropMode);
			CommonBookedCtgMove looseCartageMove = cartage.LooseBookedMoves.AddNew();
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, looseCartageMove.EW_DropMode);
			//Fallback: Cartage
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			containerCartageMove = cartage.ContainerBookedMoves.AddNew();
			AssertEquals("Don't use, cause Cartage has LCL drop mode, use cartage type", Constants.FCLEquipmentNeeded.SideLoader, containerCartageMove.EW_DropMode);
			looseCartageMove = cartage.LooseBookedMoves.AddNew();
			AssertEquals("Should use cartage drop mode cause it's LCL", Constants.LCLAIREquipmentNeeded.HandUnloadLoad, looseCartageMove.EW_DropMode);
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			containerCartageMove = cartage.ContainerBookedMoves.AddNew();
			AssertEquals("Should use cartage drop mode cause it's FCL", Constants.FCLEquipmentNeeded.LiftOffOn, containerCartageMove.EW_DropMode);
			looseCartageMove = cartage.LooseBookedMoves.AddNew();
			AssertEquals("Don't use, cause Cartage has FCL drop mode, use cartage type", Constants.LCLAIREquipmentNeeded.HandHaulier, looseCartageMove.EW_DropMode);
			//Address Fallback:
			OrgHeader ctoOrgHeader = Factory.New<OrgHeader>();
			ctoOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			ctoOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			ctoOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			OrgHeader cfsOrgHeader = Factory.New<OrgHeader>();
			cfsOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			cfsOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cfsOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			OrgHeader cydOrgHeader = Factory.New<OrgHeader>();
			cydOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			cydOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cydOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			OrgHeader cneOrgHeader = Factory.New<OrgHeader>();
			cneOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Haulier;
			cneOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.FirstDocAddress.E2_OA_Address = ctoOrgHeader.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cfsOrgHeader.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = cydOrgHeader.MainAddress.PK;
			cartage.FourthDocAddress.E2_OA_Address = cneOrgHeader.MainAddress.PK;
			AssertEquals("Change to address drop mode", Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
			containerCartageMove = cartage.ContainerBookedMoves.AddNew();
			AssertEquals("Don't change, use cartage drop mode", Constants.FCLEquipmentNeeded.LiftOffOn, containerCartageMove.EW_DropMode);
			looseCartageMove = cartage.LooseBookedMoves.AddNew();
			AssertEquals("Use Address drop mode", Constants.LCLAIREquipmentNeeded.Haulier, looseCartageMove.EW_DropMode);
			OrgAddress cneOrgAddress2 = cneOrgHeader.Addresses.AddNew();
			cneOrgAddress2.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			cneOrgAddress2.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			cneOrgAddress2.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			looseCartageMove.EW_E2WaitPointAddressID = cneOrgAddress2.PK;
			AssertEquals("Use Address drop mode", Constants.LCLAIREquipmentNeeded.Premise, looseCartageMove.EW_DropMode);
		}

		public void TestRegisteredEditable()
		{
			var cartage = Factory.New<CommonCartage>();
			Assert(!cartage.IsRoot);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLSHPtoCTO;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			move.CartageLegs.DeleteAll();
			var leg = move.CartageLegs.AddNew();
			Assert(move.IsRegisteredEditableChildObject(container));
			Assert(!move.IsRegisteredEditableChildObject(leg));
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			var move_NewFactory = newFactory.Load<CommonBookedCtgMove>(move.PK);
			Assert(!cartage_NewFactory.IsRoot);
			Assert(move_NewFactory.IsRegisteredEditableChildObject(move_NewFactory.Container));
			Assert(!move_NewFactory.IsRegisteredEditableChildObject(move_NewFactory.CartageLegs));
			newFactory = new BusinessObjectFactory();
			cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			move_NewFactory = newFactory.Load<CommonBookedCtgMove>(move.PK);
			cartage_NewFactory.IsRoot = true;
			Assert(move_NewFactory.IsRegisteredEditableChildObject(move_NewFactory.Container));
			Assert(move_NewFactory.IsRegisteredEditableChildObject(move_NewFactory.CartageLegs));
		}

		public void TestShowRequestedPickup()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			CommonBookedCtgMove move = cartage.ContainerBookedMoves.AddNew();
			CommonContainer container1 = move.Container;
			Assert(move.ShowRequestedPickup);
			CommonBookedCtgMove looseMove = cartage.LooseBookedMoves.AddNew();
			Assert(looseMove.ShowRequestedPickup);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			container1 = cartage.ContainerBookedMoves.AddNew().Container;
			Assert(move.ShowRequestedPickup);
			looseMove = cartage.LooseBookedMoves.AddNew();
			Assert(!looseMove.ShowRequestedPickup);
		}

		public void TestShowRequestedDelivery()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container1 = move.Container;
			Assert(move.ShowRequestedDelivery);
			var looseMove = cartage.LooseBookedMoves.AddNew();
			Assert(looseMove.ShowRequestedDelivery);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			container1 = cartage.ContainerBookedMoves.AddNew().Container;
			Assert(move.ShowRequestedDelivery);
			looseMove = cartage.LooseBookedMoves.AddNew();
			Assert(!looseMove.ShowRequestedDelivery);
		}

		public void TestIsAddressBooking()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container1 = move.Container;
			move.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			Assert("Does NOT match booking address type", !move.IsPickupAddressBooking);
			Assert("Does match", move.IsDeliveryAddressBooking);
			move.EW_E2PickupAddressID = cartage.SecondDocAddress.PK;
			move.EW_E2WaitPointAddressID = cartage.FirstDocAddress.PK;
			Assert("Does match", move.IsPickupAddressBooking);
			Assert("Does NOT match booking address type", !move.IsDeliveryAddressBooking);
		}

		public void TestConfirmIsIDistanceConsumer()
		{
			AssertEquals(true, Factory.New<CommonBookedCtgMove>() is IDistanceCalculationConsumer);
		}

		public void TestIDistanceCalculationConsumer_Checkpoint()
		{
			AssertEquals(Env.Security.RoadDistanceCalculationServiceLocalTransport, ((IDistanceCalculationConsumer)Factory.New<CommonBookedCtgMove>()).Checkpoint);
		}

		public void TestSetCalculatedDistance()
		{
			OrgAddress packAddress = Factory.New<OrgAddress>();
			packAddress.OA_City = "Pack";
			packAddress.OA_Address1 = "Address";
			OrgAddress unpackAddress = Factory.New<OrgAddress>();
			unpackAddress.OA_City = "Unpack";
			unpackAddress.OA_Address1 = "Address";
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.BookedMovesCollection.AddNew();
			NotificationBuffer notifications = new NotificationBuffer();
			FreightDistanceCalculator calculator = new FreightDistanceCalculator(move, notifications);
			JobDocAddress pickupDocAddress = Factory.New<JobDocAddress>();
			pickupDocAddress.E2_OA_Address = packAddress.PK;
			cartage.DocAddresses.Add(pickupDocAddress);
			move.EW_E2PickupAddressID = pickupDocAddress.PK;
			JobDocAddress deliveryDocAddress = Factory.New<JobDocAddress>();
			deliveryDocAddress.E2_OA_Address = unpackAddress.PK;
			cartage.DocAddresses.Add(deliveryDocAddress);
			move.EW_E2WaitPointAddressID = deliveryDocAddress.PK;
			move.EW_DistanceUnit = Constants.Length.Miles;
			calculator.SetCalculatedDistance();
			AssertEquals(new ZDecimal("PackAddressAustraliaUnpackAddressAustralia".Length), move.EW_Distance);
			AssertEquals(Constants.Length.Miles, move.EW_DistanceUnit);
			move.EW_DistanceUnit = Constants.Length.Kilometres;
			calculator.SetCalculatedDistance();
			AssertEquals(new ZDecimal("PackAddressAustraliaUnpackAddressAustralia".Length), move.EW_Distance);
			AssertEquals(Constants.Length.Kilometres, move.EW_DistanceUnit);
			move.EW_DistanceUnit = Constants.Length.Kilometres;
			calculator = new FreightDistanceCalculator(move, notifications);
			calculator.SetCalculatedDistance();
			AssertEquals(new ZDecimal("PackAddressAustraliaUnpackAddressAustralia".Length), move.EW_Distance);
			AssertEquals(Constants.Length.Kilometres, move.EW_DistanceUnit);
		}

		public void TestSetupDistanceCalculationConfiguration()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(consignor, "CNS", "10", "AAA");
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(consignee, "CNE", "20", "BBB");
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.BookedMovesCollection.AddNew();
			NotificationBuffer notifications = new NotificationBuffer();
			FreightDistanceCalculator calculator = new FreightDistanceCalculator(move, notifications);
			OrgAddress packAddress = Factory.New<OrgAddress>();
			packAddress.OA_City = "Pack";
			packAddress.OA_Address1 = "Address";
			OrgAddress unpackAddress = Factory.New<OrgAddress>();
			unpackAddress.OA_City = "Unpack";
			unpackAddress.OA_Address1 = "Address";
			JobDocAddress pickupDocAddress = Factory.New<JobDocAddress>();
			pickupDocAddress.E2_OA_Address = packAddress.PK;
			cartage.DocAddresses.Add(pickupDocAddress);
			pickupDocAddress.OrganisationPK = consignor.PK;
			move.EW_E2PickupAddressID = pickupDocAddress.PK;
			JobDocAddress deliveryDocAddress = Factory.New<JobDocAddress>();
			deliveryDocAddress.E2_OA_Address = unpackAddress.PK;
			cartage.DocAddresses.Add(deliveryDocAddress);
			deliveryDocAddress.OrganisationPK = consignee.PK;
			move.EW_E2WaitPointAddressID = deliveryDocAddress.PK;
			new JobHeader.Loader(move.Cartage).TryLoadOrCreate();
			move.Cartage.LocalClientPK = consignor.PK;
			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)move).DistanceCalculationConfig, consignor);
		}

		void SetupClientDistanceConfig(OrgHeader client, string provider, string version, string method)
		{
			client.MiscServ.OM_CMDistanceCalculationProvider = provider;
			client.MiscServ.OM_CMDistanceCalculationVersion = version;
			client.MiscServ.OM_CMDistanceCalculationMethod = method;
		}

		void AssertDistanceConfigIsFromClient(DistanceCalculationConfiguration config, OrgHeader client)
		{
			AssertEquals("Provider", client.MiscServ.OM_CMDistanceCalculationProvider, config.ProviderCode);
			AssertEquals("Version", client.MiscServ.OM_CMDistanceCalculationVersion, config.ProviderVersion);
			AssertEquals("Method", client.MiscServ.OM_CMDistanceCalculationMethod, config.CalculationMethod);
		}

		public void TestPackLineVolume()
		{
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			move.EW_DimUnit = Constants.Length.Centimetres;
			move.EW_VolumeUQ = Constants.Volume.CubicMetres;
			move.EW_BookedVolume = 1.5m;
			move.EW_BookedPackCount = 0;
			move.EW_BookedLength = 50m;
			move.EW_BookedWidth = 50m;
			move.EW_BookedHeight = 50m;
			AssertEquals("Only calculate Volume if Lgth, Wdth, Hgt, Pkgs all > 0.", 1.5m, move.EW_BookedVolume);
			move.EW_BookedPackCount = 10;
			AssertEquals("Calculate Volume from Lgth, Wdth, Hgt, Pkgs.", 1.25m, move.EW_BookedVolume);
			move.EW_BookedLength = 0.5m;
			move.EW_BookedWidth = 0.5m;
			move.EW_BookedHeight = 0.5m;
			move.EW_DimUnit = Constants.Length.Metres;
			AssertEquals("Dimension unit changed to CM.", 1.25m, move.EW_BookedVolume);
			move.EW_VolumeUQ = Constants.Volume.CubicFeet;
			AssertEquals("Volume unit changed to CF.", 44.143m, ZArchitecture.Core.Utilities.Round(move.EW_BookedVolume, 3));
		}

		public void TestIdentifier()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_ContainerNum = "C1010";
			AssertEquals("C1010", move.Identifier);
			move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedPackCount = 10;
			move.EW_BookedWeight = 15m;
			move.EW_BookedVolume = 20m;
			AssertEquals("10 PLT/15 KG/20 M3", move.Identifier);
		}

		public void TestIsContainerised()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			AssertEquals(true, move.IsContainerised);
			var cartage2 = Factory.New<CommonCartage>();
			var container2 = cartage2.ContainerBookedMoves.AddNew().Container;
			var corruptMove = cartage2.LooseBookedMoves.AddNew();
			var container2PK = container2.PK;
			container2.Delete();
			corruptMove.EW_JC_Container = container2PK;
			AssertEquals(false, corruptMove.EW_JC_Container.IsEmpty);
			AssertEquals(false, corruptMove.IsContainerised);
		}

		public void TestSettingE2()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Org1";
			OrgAddress org1Address1 = org1.MainAddress;
			org1Address1.OA_Code = "org1Address1";
			OrgAddress org1Address2 = org1.Addresses.AddNew();
			org1Address2.OA_Code = "org1Address2";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Org2";
			OrgAddress org2Address1 = org2.MainAddress;
			org2Address1.OA_Code = "org2Address1";
			OrgAddress org2Address2 = org2.Addresses.AddNew();
			org2Address2.OA_Code = "org2Address2";
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "Org3";
			OrgAddress org3Address1 = org3.MainAddress;
			org3Address1.OA_Code = "org3Address1";
			OrgAddress org3Address2 = org3.Addresses.AddNew();
			org3Address2.OA_Code = "org3Address2";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.FirstDocAddress.E2_OA_Address = org1Address1.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2Address1.PK;
			AssertEquals(2, cartage.DocAddresses.Count);
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			move1.EW_E2DeliveryAddressID = org1Address2.PK;
			AssertNotEquals(org1Address2.PK, move1.EW_E2DeliveryAddressID);
			JobDocAddress org1docAddress2 = Factory.Load<JobDocAddress>(move1.EW_E2DeliveryAddressID);
			AssertEquals(3, cartage.DocAddresses.Count);
			move1.EW_E2DeliveryAddressID = cartage.FirstDocAddress.PK;
			AssertEquals(3, cartage.DocAddresses.Count);
		}

		public void TestServices()
		{
			var service = Factory.New<CartageJobService>();
			var bookedCtgMove = Factory.New<CommonBookedCtgMove>();
			service.ES_ParentID = bookedCtgMove.PK;
			service.ES_ParentTableCode = bookedCtgMove.TablePrefix;
			AssertCollectionContains("Collection was not loaded and/or the relationship filter is incorrect.", service, bookedCtgMove.Services);
			AssertEquals(typeof(CartageJobService), bookedCtgMove.Services.TypeOfElements);
			AssertEquals(true, bookedCtgMove.IsRegisteredEditableChildObject(bookedCtgMove.Services));
		}

		public void TestServiceBranch()
		{
			var move = Factory.New<CommonBookedCtgMove>();
			var iHaveServices = (IHaveServices)move;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var cartage = Factory.New<CommonCartage>();
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			cartage.JJ_GB = branch.PK;
			move.EW_JJ = cartage.PK;
			AssertEquals("Service branch", branch.PK, iHaveServices.ServiceBranch.PK);
		}

		public void TestFreightMode()
		{
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_ShippingTransportMode = Constants.TransportModes.All;
			var move1 = Factory.New<CommonBookedCtgMove>();
			move1.EW_JJ = cartage1.PK;
			var container1 = Factory.New<CommonContainer>();
			move1.EW_JC_Container = container1.PK;
			AssertEquals(FreightMode.FRO, move1.FreightMode);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			var move2 = Factory.New<CommonBookedCtgMove>();
			move2.EW_JJ = cartage2.PK;
			AssertEquals(FreightMode.LSE, move2.FreightMode);
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_ShippingTransportMode = Constants.TransportModes.All;
			var move3 = Factory.New<CommonBookedCtgMove>();
			move3.EW_JJ = cartage3.PK;
			AssertEquals(FreightMode.LRO, move3.FreightMode);
		}

		public void TestUniversalCopyAttributes()
		{
			var bookedCtgMove = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			var componentType = bookedCtgMove.GetType();
			Assert("CommonBookedCtgMove should have UniversalCopyWithExtendedEntitiesAttribute.", componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Length > 0);
			var cartageLegsInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "CartageLegs");
			Assert("CartageLegs collection should have UniversalCopyCollectionEntityAttribute.", cartageLegsInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).First() != null);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommonCartage cartage = factory.New<CommonCartage>();
			CommonBookedCtgMove bookedMove = cartage.LooseBookedMoves.AddNew();
			return bookedMove;
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;

		class TestNotify : INotifications, INotificationSubscriberQueryUser
		{
			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
			}

			void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
			{
				var queryUserArgs = e as QueryUserYesNoEventArgs;
				if (queryUserArgs != null)
				{
					LastEventYesNoArgs = queryUserArgs;
					queryUserArgs.Response = ResponseToDialogs;
				}
			}

			public List<INotification> Notifications = new List<INotification>();
			public bool ResponseToDialogs { get; set; }

			public QueryUserYesNoEventArgs LastEventYesNoArgs { get; private set; }
		}
	}
}
