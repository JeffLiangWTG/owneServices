using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BulkMovementsChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMovementType()
		{
			Child.MovementType = "XXX";
			AssertHasError(Child.MovementTypeInfo, "Enter a valid Movement Type.");
			Child.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertNoNotifications(Child.MovementTypeInfo);
			Child.MovementType = ZString.Empty;
			AssertHasError(Child.MovementTypeInfo, "Please enter a Movement Type.");
		}

		public void TestOwnerType()
		{
			Child.OwnerType = "XXX";
			AssertHasError(Child.OwnerTypeInfo, "Enter a valid Owner Type.");
			Child.OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			AssertNoNotifications(Child.OwnerTypeInfo);
			Child.OwnerType = "";
			AssertHasError(Child.OwnerTypeInfo, "Please enter an Owner Type.");
		}

		public void TestContainerNum_NotAllowed()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Child.ContainerNum = "TEST4100012";
			AssertHasError(Child.ContainerNumInfo, "Container number does not have a valid check (last) digit. The check digit should be 3.");
			Child.ContainerNum = "TEST4100013";
			AssertNoNotifications(Child.ContainerNumInfo);
			Child.ContainerNum = "";
			AssertHasError(Child.ContainerNumInfo, "Please enter a Container Number.");
		}

		public void TestContainerNum_Allowed()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Child.ContainerNum = "TEST4100012";
			AssertHasWarning(Child.ContainerNumInfo, "Container number does not have a valid check (last) digit. The check digit should be 3.");
			Child.ContainerNum = "TEST4100013";
			AssertNoNotifications(Child.ContainerNumInfo);
			Child.ContainerNum = "";
			AssertHasError(Child.ContainerNumInfo, "Please enter a Container Number.");
		}

		public void TestCondition()
		{
			Child.Condition = DefaultContainerCleanList.Codes.Cotton;
			AssertNoNotifications(Child.ConditionInfo);
			Child.Condition = "XXX";
			AssertHasError(Child.ConditionInfo, "Enter a valid Container Condition.");
			Child.Condition = ZString.Empty;
			AssertNoNotifications(Child.ConditionInfo);
		}

		public void TestDamage()
		{
			Child.Damage = DefaultContainerDamageList.Codes.Damaged;
			AssertNoNotifications(Child.DamageInfo);
			Child.Damage = "XXX";
			AssertHasError(Child.DamageInfo, "Enter a valid Container Damage.");
			Child.Damage = ZString.Empty;
			AssertNoNotifications(Child.DamageInfo);
		}

		public void TestContainerType()
		{
			Child.ContainerType = ZGuid.NewZGuid();
			AssertHasError(Child.ContainerTypeInfo, "Enter a valid Container Type.");
			Child.ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertNoNotifications(Child.ContainerTypeInfo);
			Child.ContainerType = ZGuid.Empty;
			AssertHasError(Child.ContainerTypeInfo, "Please enter a Container Type.");
		}

		public void TestDepotAddressPK()
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "Depot";
			depot.OH_IsMiscFreightServices = true;
			depot.OH_IsContainerYard = true;
			Factory.Save();
			Child.DepotAddressPK = ZGuid.Invalid;
			AssertHasError(Child.DepotAddressPKInfo, "Enter a valid Depot Address.");
			Child.DepotAddressPK = depot.MainAddress.PK;
			AssertNoNotifications(Child.DepotAddressPKInfo);
			Child.DepotAddressPK = ZGuid.Empty;
			AssertHasError(Child.DepotAddressPKInfo, "Please enter a Depot Address.");
		}

		public void TestMovementDate()
		{
			Child.MovementDate = ZDateTime.Now.AddDays(1);
			AssertHasWarning(Child.MovementDateInfo, "This date is in the future, you should only record movements that have actually taken place.");
			Child.MovementDate = ZDateTime.Now;
			AssertNoNotifications(Child.MovementDateInfo);
			Child.MovementDate = ZDateTime.Empty;
			AssertHasError(Child.MovementDateInfo, "Please enter a Movement Date.");
		}

		public void TestPrincipalPK()
		{
			OrgHeader principal = BaseAgencyTest.NewPrincipal(Factory);
			Factory.Save();
			Child.PrincipalPK = principal.PK;
			AssertNoNotifications(Child.PrincipalPKInfo);
			Child.PrincipalPK = ZGuid.NewZGuid();
			AssertHasError(Child.PrincipalPKInfo, "Enter a valid Principal.");
			Child.PrincipalPK = ZGuid.Empty;
			AssertNoNotifications(Child.PrincipalPKInfo);
		}

		public void TestResponsiblePartyPK()
		{
			OrgHeader party = Factory.NewWithValidTestData<OrgHeader>();
			party.OH_Code = "Party";
			Factory.Save();
			Child.ResponsiblePartyPK = party.PK;
			AssertNoNotifications(Child.ResponsiblePartyPKInfo);
			Child.ResponsiblePartyPK = ZGuid.NewZGuid();
			AssertHasError(Child.ResponsiblePartyPKInfo, "Enter a valid Responsible Party.");
			Child.ResponsiblePartyPK = ZGuid.Empty;
			AssertNoNotifications(Child.ResponsiblePartyPKInfo);
		}

		public void TestDuplicates_Movement()
		{
			ZDateTime now = ZDateTime.Now.AddDays(-1).ToSmallDateTime();
			Child.ContainerNum = "TEST4100013";
			Child.ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Child.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Child.MovementDate = now;
			Child.Validation.ValidateAll();
			const string Error = "A movement with this type, date and time already exists for this container.";
			AssertNoNotifications("No Duplicate: Number", Child.ContainerNumInfo);
			AssertNoNotifications("No Duplicate: Type", Child.MovementTypeInfo);
			AssertNoNotifications("No Duplicate: Date", Child.MovementDateInfo);
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Child.ContainerType;
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement.E9_MovementDate = now;
			Child.Validation.ValidateAll();
			AssertHasWarning("Duplicate: Number", Child.ContainerNumInfo, Error);
			AssertHasWarning("Duplicate: Type", Child.MovementTypeInfo, Error);
			AssertHasWarning("Duplicate: Date", Child.MovementDateInfo, Error);
			Child.IsGenerated = true;
			Child.Validation.ValidateAll();
			AssertNoNotifications("Generated: Number", Child.ContainerNumInfo);
			AssertNoNotifications("Generated: Type", Child.MovementTypeInfo);
			AssertNoNotifications("Generated: Date", Child.MovementDateInfo);
		}

		public void TestDuplicates_Sibling()
		{
			ZDateTime now = ZDateTime.Now;
			Child.ContainerNum = "TEST4100013";
			Child.ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Child.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Child.MovementDate = now;
			Child.Validation.ValidateAll();
			const string Error = "A movement with this type, date and time already exists for this container.";
			AssertNoNotifications("No Duplicate: Number", Child.ContainerNumInfo);
			AssertNoNotifications("No Duplicate: Type", Child.MovementTypeInfo);
			AssertNoNotifications("No Duplicate: Date", Child.MovementDateInfo);
			BulkMovementsChild sibling = Child.Header.Children.AddNew();
			sibling.ContainerNum = Child.ContainerNum;
			sibling.ContainerType = Child.ContainerType;
			sibling.MovementType = Child.MovementType;
			sibling.MovementDate = Child.MovementDate;
			Child.Validation.ValidateAll();
			AssertHasWarning("Duplicate: Number", Child.ContainerNumInfo, Error);
			AssertHasWarning("Duplicate: Type", Child.MovementTypeInfo, Error);
			AssertHasWarning("Duplicate: Date", Child.MovementDateInfo, Error);
		}

		#region Implementation
		BulkMovementsHeader Header
		{
			get
			{
				return header ?? (header = new BulkMovementsHeader(Factory));
			}
		}

		BulkMovementsHeader header;
		BulkMovementsChild Child
		{
			get
			{
				return child ?? (child = Header.Children.AddNew());
			}
		}

		BulkMovementsChild child;
		#endregion
	}
}
