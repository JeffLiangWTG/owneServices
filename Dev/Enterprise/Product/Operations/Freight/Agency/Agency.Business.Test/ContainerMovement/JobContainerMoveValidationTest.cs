using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class JobContainerMoveValidationTest : BusinessObjectValidationTestCase
	{
		public void TestE9_MovementDate()
		{
			Movement.E9_MovementDate = ZDateTime.Invalid;
			AssertHasError(Movement.E9_MovementDateInfo, "Enter a valid Movement Date.");
			Movement.E9_MovementDate = ZDateTime.Today;
			AssertNoNotifications(Movement.E9_MovementDateInfo);
			Movement.E9_MovementDate = ZDateTime.Empty;
			AssertHasError(Movement.E9_MovementDateInfo, "Please enter a Movement Date.");
			Movement.E9_MovementDate = ZDateTime.Now.AddDays(-1);
			AssertNoNotifications(Movement.E9_MovementDateInfo);
			Movement.E9_MovementDate = ZDateTime.Now.AddDays(2);
			AssertHasWarning(Movement.E9_MovementDateInfo, "This date is in the future, you should only record movements that have actually taken place here.");
		}

		public void TestE9_MovementType()
		{
			Movement.E9_MovementType = "ZZZ";
			AssertHasError(Movement.E9_MovementTypeInfo, "Enter a valid Movement Type.");
			Movement.E9_MovementType = Movement.Lookups.MovementCodeList[0].Code;
			AssertNoErrors(Movement.E9_MovementTypeInfo);
			Movement.E9_MovementType = "";
			AssertHasError(Movement.E9_MovementTypeInfo, "Please enter a Movement Type.");
		}

		public void TestE9_ContainerQuality()
		{
			Movement.E9_ContainerQuality = Movement.Lookups.CleanCodeList[0].Code;
			AssertNoErrors(Movement.E9_ContainerQualityInfo);
			Movement.E9_ContainerQuality = "ZZZ";
			AssertHasErrors(Movement.E9_ContainerQualityInfo);
		}

		public void TestE9_ContainerCondition()
		{
			Movement.E9_ContainerCondition = Movement.Lookups.DamageCodeList[0].Code;
			AssertNoErrors(Movement.E9_ContainerConditionInfo);
			Movement.E9_ContainerCondition = "ZZZ";
			AssertHasErrors(Movement.E9_ContainerConditionInfo);
		}

		public void TestE9_OA_Depot()
		{
			Movement.E9_OA_Depot = Depot.MainAddress.PK;
			AssertNoErrors(Movement.E9_OA_DepotInfo);
			Movement.E9_OA_Depot = ZGuid.Empty;
			AssertHasError(Movement.E9_OA_DepotInfo, "Please enter an Address.");
		}

		public void TestE9_OH_Principal()
		{
			const string warningText = "No principal could be detected for this movement, it is strongly recommended that one be entered here.";
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_OH_DeliveryAgent = principal.PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_RC = Stock.R6_RC;
			container.JC_ContainerNum = Stock.R6_ContainerNum;
			Factory.Save();
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			Movement.E9_JV = voyage.PK;
			Movement.Validation.ValidateE9_OH_Principal();
			AssertNoNotifications("Value from shipment", Movement.E9_OH_PrincipalInfo);
			bill.JS_OH_DeliveryAgent = ZGuid.Empty;
			Movement.Validation.ValidateE9_OH_Principal();
			AssertHasWarning(Movement.E9_OH_PrincipalInfo, warningText);
			Movement.E9_OH_Principal = principal.PK;
			Movement.Validation.ValidateE9_OH_Principal();
			AssertNoNotifications("Value from movement", Movement.E9_OH_PrincipalInfo);
			Movement.E9_OH_Principal = ZGuid.Empty;
			Movement.Validation.ValidateE9_OH_Principal();
			AssertHasWarning(Movement.E9_OH_PrincipalInfo, warningText);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.DepotGateIn;
			Movement.Validation.ValidateE9_OH_Principal();
			AssertNoNotifications("Value unnessisary", Movement.E9_OH_PrincipalInfo);
		}

		public void TestE9_OH_ResponsibleParty()
		{
			const string warningText = "No responsible party could be detected for this movement, it is strongly recommended that one be entered here.";
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "client";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			JobHeader header = new JobHeader.Loader(bill).TryCreate();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_RC = Stock.R6_RC;
			container.JC_ContainerNum = Stock.R6_ContainerNum;
			Factory.Save();
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			Movement.E9_JV = voyage.PK;
			AssertNoNotifications("Value from shipment", Movement.E9_OH_ResponsiblePartyInfo);
			header.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Movement.Validation.ValidateE9_OH_ResponsibleParty();
			AssertHasWarning(Movement.E9_OH_ResponsiblePartyInfo, warningText);
			Movement.E9_OH_ResponsibleParty = client.PK;
			Movement.Validation.ValidateE9_OH_ResponsibleParty();
			AssertNoNotifications("Value from movement", Movement.E9_OH_ResponsiblePartyInfo);
			Movement.E9_OH_ResponsibleParty = ZGuid.Empty;
			Movement.Validation.ValidateE9_OH_ResponsibleParty();
			AssertHasWarning(Movement.E9_OH_ResponsiblePartyInfo, warningText);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.DepotGateIn;
			Movement.Validation.ValidateE9_OH_ResponsibleParty();
			AssertNoNotifications("Value unnessisary", Movement.E9_OH_ResponsiblePartyInfo);
		}

		[UseDummyDetentionStrategy]
		public void TestE9_DetentionDays()
		{
			DummyDetentionStrategy.Instance.GetDefaultDetentionDaysOverride = (m) => null;
			Movement.E9_DetentionDays = 5;
			AssertHasError(Movement.E9_DetentionDaysInfo, "Detention days are not supported on movements of this type.");
			Movement.E9_DetentionDays = 0;
			AssertNoNotifications(Movement.E9_DetentionDaysInfo);
			DummyDetentionStrategy.Instance.GetDefaultDetentionDaysOverride = (m) => 15;
			Movement.E9_DetentionDays = 10;
			AssertHasWarning(Movement.E9_DetentionDaysInfo, "Detention days are calculated to be 15.");
			Movement.E9_DetentionDays = 15;
			AssertNoNotifications(Movement.E9_DetentionDaysInfo);
		}

		#region E9_LeaseNumber
		public void TestE9_LeaseNumber_SingleHire()
		{
			const string noContractNumberWaring = @"The Lease Contract is valid up to the next Off-Hire movement.
Add Lease Contract No to the movements of leased container for better tracking and reporting.";
			const string mismatchedContractNumberWaring = @"The contract number entered does not match the contract number of the On-Hire movement.";
			var today = DateTime.Today;
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			AssertMovementResult("OnHire doesn't exist and no warning for Movement.", Movement, today.AddDays(-5));
			var onHire = Stock.Movements.AddNew();
			onHire.E9_MovementType = ContainerMovementTypes.Codes.OnHire;
			AssertMovementResult("OnHire exists with empty E9_MovementDate.", onHire, ZDateTime.Empty);
			Movement.Validation.ValidateE9_LeaseNumber();
			AssertNoNotifications("OnHire exists with empty E9_MovementDate, no warning for Movement.", Movement.E9_LeaseNumberInfo);
			AssertMovementResult("OnHire exists without contract number.", onHire, today.AddDays(-10));
			Movement.Validation.ValidateE9_LeaseNumber();
			AssertNoNotifications("OnHire exists without contract number, no warning for Movement.", Movement.E9_LeaseNumberInfo);
			onHire.E9_LeaseNumber = "CONTRACT1";
			Movement.Validation.ValidateE9_LeaseNumber();
			AssertHasWarning("Movement doesn't have contract number.", Movement.E9_LeaseNumberInfo, noContractNumberWaring);
			var offHire = Stock.Movements.AddNew();
			offHire.E9_MovementType = ContainerMovementTypes.Codes.OffHire;
			AssertMovementResult("OffHire exists without contract number.", offHire, today.AddDays(-2), expectWarning: noContractNumberWaring);
			Movement.Validation.ValidateE9_LeaseNumber();
			AssertHasWarning("Movement doesn't have contract number.", Movement.E9_LeaseNumberInfo, noContractNumberWaring);
			Movement.E9_LeaseNumber = "CONTRACT2";
			AssertHasWarning("Movement has a different contract number with OnHire.", Movement.E9_LeaseNumberInfo, mismatchedContractNumberWaring);
			offHire.E9_LeaseNumber = "CONTRACT2";
			AssertHasWarning("OffHire has a different contract number with OnHire.", offHire.E9_LeaseNumberInfo, mismatchedContractNumberWaring);
			Movement.E9_LeaseNumber = "CONTRACT1";
			AssertNoNotifications("Movement has a same contract number with OnHire.", Movement.E9_LeaseNumberInfo);
			offHire.E9_LeaseNumber = "CONTRACT1";
			AssertNoNotifications("OffHire has a same contract number with OnHire.", offHire.E9_LeaseNumberInfo);
			AssertMovementResult("Movement date is out of OnHire and OffHire.", Movement, today.AddDays(-20));
			AssertMovementResult("Movement date is out of OnHire and OffHire.", Movement, today);
		}

		public void TestE9_LeaseNumber_MultipleHires()
		{
			const string noContractNoWaring = @"The Lease Contract is valid up to the next Off-Hire movement.
Add Lease Contract No to the movements of leased container for better tracking and reporting.";
			const string mismatchWaring = @"The contract number entered does not match the contract number of the On-Hire movement.";
			var today = DateTime.Today;
			var onHire1 = Stock.Movements.AddNew();
			onHire1.E9_MovementType = ContainerMovementTypes.Codes.OnHire;
			onHire1.E9_MovementDate = today.AddDays(-50);
			onHire1.E9_LeaseNumber = "CONTRACT1";
			var offHire1 = Stock.Movements.AddNew();
			offHire1.E9_MovementType = ContainerMovementTypes.Codes.OffHire;
			offHire1.E9_MovementDate = today.AddDays(-40);
			var onHire2 = Stock.Movements.AddNew();
			onHire2.E9_MovementType = ContainerMovementTypes.Codes.OnHire;
			onHire2.E9_MovementDate = today.AddDays(-30);
			onHire2.E9_LeaseNumber = "CONTRACT2";
			var offHire2 = Stock.Movements.AddNew();
			offHire2.E9_MovementType = ContainerMovementTypes.Codes.OffHire;
			offHire2.E9_MovementDate = today.AddDays(-20);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			foreach (var daysOffset in new[] { -60, -35, -15 })
			{
				AssertMovementResult("Movement is out of the hire.", Movement, today.AddDays(daysOffset));
				AssertMovementResult("Movement is out of the hire.", Movement, today.AddDays(daysOffset), "BOOK");
				AssertMovementResult("Movement is out of the hire.", Movement, today.AddDays(daysOffset), "CONTRACT2");
			}

			foreach (var daysOffset in new[] { -45, -25 })
			{
				AssertMovementResult("Movement is in the hire and has no contract number.", Movement, today.AddDays(daysOffset), expectWarning: noContractNoWaring);
				AssertMovementResult("Movement is in the hire and has different contract number with the OnHire.", Movement, today.AddDays(daysOffset), "BOOK", mismatchWaring);
				AssertMovementResult("Movement is in the hire and has same contract number with the OnHire.", Movement, today.AddDays(daysOffset), daysOffset < -30 ? "CONTRACT1" : "CONTRACT2");
			}

			var onHire3 = Stock.Movements.AddNew();
			onHire3.E9_MovementType = ContainerMovementTypes.Codes.OnHire;
			onHire3.E9_MovementDate = today.AddDays(-10);
			onHire3.E9_LeaseNumber = "CONTRACT3";
			AssertMovementResult("Movement is in the hire and has no contract number.", Movement, today.AddDays(-5), expectWarning: noContractNoWaring);
			AssertMovementResult("Movement is in the hire and has different contract number with the OnHire.", Movement, today.AddDays(-5), "BOOK", mismatchWaring);
			AssertMovementResult("Movement is in the hire and has same contract number with the OnHire.", Movement, today.AddDays(-5), "CONTRACT3");
		}

		void AssertMovementResult(string message, ContainerMovement containterMovement, ZDateTime movementDate, string leaseNumber = null, string expectWarning = null)
		{
			containterMovement.E9_MovementDate = movementDate;
			containterMovement.E9_LeaseNumber = leaseNumber ?? string.Empty;
			if (string.IsNullOrEmpty(expectWarning))
			{
				AssertNoNotifications(message, containterMovement.E9_LeaseNumberInfo);
			}
			else
			{
				AssertHasWarning(message, containterMovement.E9_LeaseNumberInfo, expectWarning);
			}
		}

		#endregion
		#region Implementation
		RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "TEST4100013";
					stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock;
			}
		}

		RefContainerStock stock;
		ContainerMovement Movement
		{
			get
			{
				return movement ?? (movement = Stock.Movements.AddNew());
			}
		}

		ContainerMovement movement;
		OrgHeader Depot
		{
			get
			{
				return depot ?? (depot = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader depot;
		#endregion
	}
}
