using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageBookedCtgMoveBehaviorStrategyTest : TestCaseWithFactory
	{
		public void TestLooseBookedMoves()
		{
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
			containerMove.E4_E5_ToOrg = cYD.PK;
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
			looseMove.E4_E5_ToOrg = cNE.PK;
			CommonCartageLegType looseLeg = iSMY.LooseCartageLegTypes.AddNew();
			looseLeg.E4_ContainerMode = "LSE";
			looseLeg.E4_E5_FromOrg = cFS.PK;
			looseLeg.E4_E5_ToOrg = cNE.PK;
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "ISM1";
			CommonBookedCtgMove cartageMove = cartage.LooseBookedMoves.AddNew();
			AssertEquals(cartage.SecondDocAddress.PK, cartageMove.EW_E2PickupAddressID);
			AssertEquals(cartage.FourthDocAddress.PK, cartageMove.EW_E2DeliveryAddressID);
		}

		public void TestDefaultDropMode()
		{
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
			containerMove.E4_E5_ToOrg = cYD.PK;
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
			looseMove.E4_E5_ToOrg = cNE.PK;
			looseMove.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.HandHaulier;
			CommonCartageLegType looseLeg = iSMY.LooseCartageLegTypes.AddNew();
			looseLeg.E4_ContainerMode = "LSE";
			looseLeg.E4_E5_FromOrg = cFS.PK;
			looseLeg.E4_E5_ToOrg = cNE.PK;
			//Fallback: Cartage Type
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "ISM1";
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			var containerCartageMove = cartage.ContainerBookedMoves.AddNew();
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
		}

		public void TestContainerBookedMoves()
		{
			SetupCartage();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "ISM1";
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var cartageMove = cartage.GetBookedMoves(container)[0];
			AssertEquals(cartage.FirstDocAddress.PK, cartageMove.EW_E2PickupAddressID);
			AssertEquals(cartage.SecondDocAddress.PK, cartageMove.EW_E2WaitPointAddressID);
			AssertEquals(cartage.ThirdDocAddress.PK, cartageMove.EW_E2DeliveryAddressID);
		}

		void SetupCartage()
		{
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
			containerMove.E4_E5_ToOrg = cYD.PK;
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
			looseMove.E4_E5_ToOrg = cNE.PK;
			CommonCartageLegType looseLeg = iSMY.LooseCartageLegTypes.AddNew();
			looseLeg.E4_ContainerMode = "LSE";
			looseLeg.E4_E5_FromOrg = cFS.PK;
			looseLeg.E4_E5_ToOrg = cNE.PK;
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
