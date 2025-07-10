using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DetentionExportStrategy))]
	internal class DetentionExportStrategyTest : DetentionStrategyBaseTest<DetentionExportStrategy>
	{
		public void TestDefaultDetentionDays()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZDateTime now = ZDateTime.Now.ToSmallDateTime();
				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_Code = "Consignor";
				OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
				principal.OH_Code = "Principal";
				OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
				depot.OH_Code = "Depot";
				depot.OH_RL_NKClosestPort = "AUBNE";
				OrgContainerDetention detentionFree = principal.CarrierContainerPenalties.AddNew();
				detentionFree.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detentionFree.PD_OH_Client = consignor.PK;
				detentionFree.PD_FreeDays = 15;
				detentionFree.PD_ContainerType = "20F";
				detentionFree.PD_DetentionPortOrCountry = "AU";
				ExportShipment.JS_OH_DeliveryAgent = principal.PK;
				ExportShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				ExportContainer.JC_ContainerNum = Stock.R6_ContainerNum;
				Stock.Container.RC_FreightRateClass = "20F";
				Factory.Save();
				ContainerMovement oldYGO = Stock.Movements.AddNew();
				oldYGO.E9_MovementDate = now.AddDays(-40);
				oldYGO.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
				ContainerMovement oldYGI = Stock.Movements.AddNew();
				oldYGI.E9_MovementDate = now.AddDays(-30);
				oldYGI.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				ContainerMovement lastYGO = Stock.Movements.AddNew();
				lastYGO.E9_MovementDate = now.AddDays(-20);
				lastYGO.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
				Movement.E9_MovementDate = now.AddDays(-1);
				Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
				Movement.E9_OA_Depot = depot.MainAddress.PK;
				Movement.E9_JV = Voyage.PK;
				Movement.E9_DetentionDays = 2;
				AssertEquals((short)5, MovementType.GetDefaultDetentionDays(Movement));
			}
		}

		public void TestDetentionPeriod()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_Code = "Consignor";
				OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
				principal.OH_Code = "Principal";
				OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
				depot.OH_Code = "Depot";
				depot.OH_RL_NKClosestPort = "AUBNE";
				OrgContainerDetention detentionFree = principal.CarrierContainerPenalties.AddNew();
				detentionFree.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detentionFree.PD_OH_Client = consignor.PK;
				detentionFree.PD_FreeDays = 15;
				detentionFree.PD_ContainerType = "20F";
				detentionFree.PD_DetentionPortOrCountry = "AU";
				ZDateTime referenceDate = ZDateTime.Now.ToSmallDateTime();
				ZDateTime released = referenceDate.AddDays(-30);
				ZDateTime lastfreeday = released.AddDays(15 - 1); // the last of 15 free days including the day of release.
				ZDateTime returned = lastfreeday.AddDays(15); // 15 days in detention
				ExportShipment.JS_OH_DeliveryAgent = principal.PK;
				ExportShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				ExportContainer.JC_ContainerNum = Stock.R6_ContainerNum;
				Stock.Container.RC_FreightRateClass = "20F";
				Factory.Save();
				ContainerMovement oldYGO = Stock.Movements.AddNew();
				oldYGO.E9_MovementDate = ZDateTime.Now.AddDays(-50);
				oldYGO.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
				ContainerMovement oldYGI = Stock.Movements.AddNew();
				oldYGI.E9_MovementDate = ZDateTime.Now.AddDays(-40);
				oldYGI.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				ContainerMovement lastYGO = Stock.Movements.AddNew();
				lastYGO.E9_MovementDate = released;
				lastYGO.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
				Movement.E9_MovementDate = returned;
				Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
				Movement.E9_OA_Depot = depot.MainAddress.PK;
				Movement.E9_JV = Voyage.PK;
				Movement.E9_DetentionDays = 2;
				AssertEquals("start of detention free period", released, MovementType.GetStartOfDetentionFreePeriod(Movement));
				AssertEquals("start of detention period", lastfreeday.AddDays(1), MovementType.GetStartOfDetentionPeriod(Movement));
				AssertEquals((short)15, MovementType.GetDetentionFreeDays(Movement));
			}
		}

		[TestDate(2015, 11, 10)]
		public void TestDefaultDetentionDays_NotAppliedWhenMovementEventsDatesAreOutOfOrder()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				var depot = Factory.NewWithValidTestData<OrgHeader>();
				depot.OH_Code = "Depot";
				depot.OH_RL_NKClosestPort = "AUBNE";
				ExportShipment.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
				ExportShipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				ExportContainer.JC_ContainerNum = Stock.R6_ContainerNum;
				Stock.Container.RC_FreightRateClass = "20F";
				Factory.Save();
				var journey1Date = ZDateTime.Today.AddDays(-730).ToSmallDateTime();
				CreateNewMovement(Stock, ContainerMovementTypes.Codes.WharfGateOut, journey1Date.AddDays(-15));
				CreateNewMovement(Stock, ContainerMovementTypes.Codes.YardGateIn, journey1Date.AddDays(-10));
				CreateNewMovement(Stock, ContainerMovementTypes.Codes.YardGateOut, journey1Date.AddDays(-5));
				CreateNewMovement(Stock, ContainerMovementTypes.Codes.WharfGateIn, journey1Date);
				var journey2Date = ZDateTime.Today.AddDays(-10).ToSmallDateTime();
				CreateNewMovement(Stock, ContainerMovementTypes.Codes.WharfGateOut, journey2Date.AddDays(-24));
				CreateNewMovement(Stock, ContainerMovementTypes.Codes.YardGateIn, journey2Date.AddDays(-12));
				var latestWharfInMovement = CreateNewMovement(Stock, ContainerMovementTypes.Codes.WharfGateIn, journey2Date);
				AssertEquals("Not be able to default the detention date as the latest WGO has occurred after the latest YGI", (short)0, MovementType.GetDefaultDetentionDays(latestWharfInMovement));
				CreateNewMovement(Stock, ContainerMovementTypes.Codes.YardGateOut, journey2Date.AddDays(-12));
				AssertEquals("There is a YGO event after the WGO so movements are not longer disjointed", (short)3, MovementType.GetDefaultDetentionDays(latestWharfInMovement));
				var latestLoadMovement = CreateNewMovement(Stock, ContainerMovementTypes.Codes.Load, journey2Date.AddDays(5));
				AssertEquals("Detention Date should not be affected by events after WGO", (short)3, MovementType.GetDefaultDetentionDays(latestWharfInMovement));
				latestLoadMovement.E9_MovementDate = latestWharfInMovement.E9_MovementDate.AddDays(-1);
				AssertEquals("Not be able to default the detention date as the latest WGO has occurred after the latest LOD", (short)0, MovementType.GetDefaultDetentionDays(latestWharfInMovement));
			}
		}

		static ContainerMovement CreateNewMovement(RefContainerStock stock, ZString type, ZDateTime date)
		{
			var movement = stock.Movements.AddNew();
			movement.E9_MovementType = type;
			movement.E9_MovementDate = date;
			return movement;
		}

		public void TestGetReleaseDate()
		{
			Movement.E9_MovementDate = ZDateTime.Invalid;
			AssertNoExceptionThrown(() => MovementType.GetStartOfDetentionFreePeriod(Movement));
		}

		#region Implementation
		protected override string[] GetDetentionTypes()
		{
			return new string[] { DetentionInvoiceType.Codes.Export, };
		}
		#endregion
	}
}
