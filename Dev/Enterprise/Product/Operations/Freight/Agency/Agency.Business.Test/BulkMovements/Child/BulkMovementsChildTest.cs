using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BulkMovementsChild))]
	internal class BulkMovementsChildTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsDuplicate()
		{
			ZDateTime now = ZDateTime.Now;
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			BulkMovementsChild child1 = AddChild(header, "TEST4100013", ContainerMovementTypes.Codes.WharfGateIn, now.AddDays(-1));
			BulkMovementsChild child2 = AddChild(header, "TEST4100029", ContainerMovementTypes.Codes.WharfGateIn, now.AddDays(-1));
			BulkMovementsChild child3 = AddChild(header, "TEST4100013", ContainerMovementTypes.Codes.WharfGateOut, now.AddDays(-1));
			BulkMovementsChild child4 = AddChild(header, "TEST4100013", ContainerMovementTypes.Codes.WharfGateIn, now.AddDays(-1).AddMinutes(1));
			BulkMovementsChild child5 = AddChild(header, "TEST4100013", ContainerMovementTypes.Codes.WharfGateIn, now.AddDays(-1));
			CombineAssertions(delegate
			{
				AssertEquals("child1.IsDuplicate", true, child1.IsDuplicate);
				AssertEquals("child2.IsDuplicate", false, child2.IsDuplicate);
				AssertEquals("child3.IsDuplicate", false, child3.IsDuplicate);
				AssertEquals("child4.IsDuplicate", false, child4.IsDuplicate);
				AssertEquals("child5.IsDuplicate", true, child5.IsDuplicate);
			});
		}

		public void TestValidateDepotOrgPK()
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			BulkMovementsChild child = header.Children.AddNew();
			const string errorMissing = "Please enter a value.";
			const string errorInvalid = "Enter a valid selection.";
			child.DepotAddressPK_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Invalid);
			AssertHasError(child.DepotAddressPK_ZAddress.OrgPKInfo, errorInvalid);
			child.DepotAddressPK_ZAddress.SetOrgWithoutSettingDefaultAddress(depot.PK);
			AssertNoNotifications(child.DepotAddressPK_ZAddress.OrgPKInfo);
			child.DepotAddressPK_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			AssertHasError(child.DepotAddressPK_ZAddress.OrgPKInfo, errorMissing);
		}

		public void TestVesselVoyage()
		{
			BulkMovementsChild child = new BulkMovementsHeader(Factory).Children.AddNew();
			AssertEquals("", child.VesselName);
			AssertEquals("", child.VoyageNo);
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "098";
			child.VoyagePK = voyage.PK;
			AssertEquals("MAJAPAHIT", child.VesselName);
			AssertEquals("098", child.VoyageNo);
		}

		public void TestGenerate_NewStock()
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "Depot";
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";
			OrgHeader responsible = Factory.NewWithValidTestData<OrgHeader>();
			responsible.OH_Code = "Responsible";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "098";
			Factory.Save();
			BulkMovementsChild child = new BulkMovementsHeader(Factory).Children.AddNew();
			child.ContainerNum = "TEST4100013";
			child.ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			child.OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			child.VoyagePK = voyage.PK;
			child.DepotAddressPK = depot.MainAddress.PK;
			child.Condition = DefaultContainerCleanList.Codes.Cotton;
			child.Damage = DefaultContainerDamageList.Codes.NotAvailable;
			child.ContainerIsEmpty = true;
			child.MovementType = ContainerMovementTypes.Codes.YardGateIn;
			child.MovementDate = ZDateTime.Now.ToSmallDateTime().AddDays(-1);
			child.PrincipalPK = principal.PK;
			child.ResponsiblePartyPK = responsible.PK;
			child.LeaseContractNo = "CONTRACT1";
			BusinessObjectFactory createFactory = new BusinessObjectFactory();
			Factory.ResetDatabaseLoadCount();
			child.Generate(createFactory);
			AssertMaxDbHits("should not hav loaded anything in the BO factory.", 0, Factory);
			ContainerMovement[] movements = createFactory.Load<ContainerMovement>(new ZQuery()
			{ FetchOnlyFromLocalCache = true });
			AssertEquals("should have created 1 movement", 1, movements.Length);
			ContainerMovement movement = movements[0];
			CombineAssertions(delegate
			{
				AssertEquals("movement should not be saved", false, movements[0].IsInDatabase);
				AssertNotNull("Stock", movement.Stock);
				if (movement.Stock != null)
				{
					AssertEquals("Conatiner Number", "TEST4100013", movement.Stock.R6_ContainerNum);
					AssertEquals("Container Type", Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK, movement.Stock.R6_RC);
					AssertEquals("Owner Type", Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned, movement.Stock.R6_OwnerType);
				}

				AssertEquals("Movement Type", ContainerMovementTypes.Codes.YardGateIn, movement.E9_MovementType);
				AssertEquals("Movement Date", child.MovementDate, movement.E9_MovementDate);
				AssertEquals("Container Condition", DefaultContainerCleanList.Codes.Cotton, movement.E9_ContainerQuality);
				AssertEquals("Container Damage", DefaultContainerDamageList.Codes.NotAvailable, movement.E9_ContainerCondition);
				AssertEquals("Voyage", voyage.PK, movement.E9_JV);
				AssertEquals("Depot", depot.MainAddress.PK, movement.E9_OA_Depot);
				AssertEquals("Is Empty", true, movement.E9_ContainerIsEmpty);
				AssertEquals("Principal", principal.PK, movement.E9_OH_Principal);
				AssertEquals("Responsible Party", responsible.PK, movement.E9_OH_ResponsibleParty);
				AssertEquals("Lease Contract No", "CONTRACT1", movement.E9_LeaseNumber);
			});
		}

		public void TestGenerate_WithStock()
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "Depot";
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";
			OrgHeader responsible = Factory.NewWithValidTestData<OrgHeader>();
			responsible.OH_Code = "Responsible";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "098";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			BulkMovementsChild child = new BulkMovementsHeader(Factory).Children.AddNew();
			child.ContainerNum = stock.R6_ContainerNum;
			child.ContainerType = stock.R6_RC;
			child.OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			child.VoyagePK = voyage.PK;
			child.DepotAddressPK = depot.MainAddress.PK;
			child.Condition = DefaultContainerCleanList.Codes.Cotton;
			child.Damage = DefaultContainerDamageList.Codes.NotAvailable;
			child.ContainerIsEmpty = true;
			child.MovementType = ContainerMovementTypes.Codes.YardGateIn;
			child.MovementDate = ZDateTime.Now.ToSmallDateTime().AddDays(-1);
			child.PrincipalPK = principal.PK;
			child.ResponsiblePartyPK = responsible.PK;
			child.LeaseContractNo = "CONTRACT2";
			BusinessObjectFactory createFactory = new BusinessObjectFactory();
			Factory.ResetDatabaseLoadCount();
			child.Generate(createFactory);
			AssertMaxDbHits("should not hav loaded anything in the BO factory.", 0, Factory);
			ContainerMovement[] movements = createFactory.Load<ContainerMovement>(new ZQuery()
			{ FetchOnlyFromLocalCache = true });
			AssertEquals("should have created 1 movement", 1, movements.Length);
			ContainerMovement movement = movements[0];
			CombineAssertions(delegate
			{
				AssertEquals("movement should not be saved", false, movements[0].IsInDatabase);
				AssertEquals("Stock", stock.PK, movement.E9_R6);
				AssertEquals("Movement Type", ContainerMovementTypes.Codes.YardGateIn, movement.E9_MovementType);
				AssertEquals("Movement Date", child.MovementDate, movement.E9_MovementDate);
				AssertEquals("Container Condition", DefaultContainerCleanList.Codes.Cotton, movement.E9_ContainerQuality);
				AssertEquals("Container Damage", DefaultContainerDamageList.Codes.NotAvailable, movement.E9_ContainerCondition);
				AssertEquals("Voyage", voyage.PK, movement.E9_JV);
				AssertEquals("Depot", depot.MainAddress.PK, movement.E9_OA_Depot);
				AssertEquals("Is Empty", true, movement.E9_ContainerIsEmpty);
				AssertEquals("Principal", principal.PK, movement.E9_OH_Principal);
				AssertEquals("Responsible Party", responsible.PK, movement.E9_OH_ResponsibleParty);
				AssertEquals("Lease Contract No", "CONTRACT2", movement.E9_LeaseNumber);
			});
		}

		[ExpectNoExceptions]
		public void TestMovementDateValidation()
		{
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			AddChild(new BulkMovementsHeader(Factory), "TEST4100013", ContainerMovementTypes.Codes.WharfGateIn, new ZDateTime(2130, 8, 27, 11, 0, 0));
		}

		public void TestFromStock()
		{
			ZGuid gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ZGuid gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = gp20;
			stock1.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100029";
			stock2.R6_RC = gp40;
			stock2.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.Leased;
			Factory.Save();
			BulkMovementsChild child = new BulkMovementsHeader(Factory).Children.AddNew();
			CombineAssertions(delegate
			{
				child.ContainerNum = "TEST4100035";
				AssertEquals("Container Type", ZGuid.Empty, child.ContainerType);
				AssertEquals("Container Type Read-Only", false, child.ContainerTypeInfo.ReadOnly);
				AssertEquals("Owner Type", ZString.Empty, child.OwnerType);
				AssertEquals("Owner Type Read-Only", false, child.OwnerTypeInfo.ReadOnly);
			});
			CombineAssertions(delegate
			{
				child.ContainerNum = "TEST4100029";
				AssertEquals("Container Type", gp40, child.ContainerType);
				AssertEquals("Container Type Read-Only", true, child.ContainerTypeInfo.ReadOnly);
				AssertEquals("Owner Type", Enterprise.Core.Constants.ContainerOwnership.Codes.Leased, child.OwnerType);
				AssertEquals("Owner Type Read-Only", true, child.OwnerTypeInfo.ReadOnly);
			});
			CombineAssertions(delegate
			{
				child.ContainerNum = "TEST4100013";
				AssertEquals("Container Type", gp20, child.ContainerType);
				AssertEquals("Container Type Read-Only", true, child.ContainerTypeInfo.ReadOnly);
				AssertEquals("Owner Type", Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned, child.OwnerType);
				AssertEquals("Owner Type Read-Only", true, child.OwnerTypeInfo.ReadOnly);
			});
			CombineAssertions(delegate
			{
				child.ContainerNum = "TEST4100035";
				AssertEquals("Container Type", ZGuid.Empty, child.ContainerType);
				AssertEquals("Container Type Read-Only", false, child.ContainerTypeInfo.ReadOnly);
				AssertEquals("Owner Type", ZString.Empty, child.OwnerType);
				AssertEquals("Owner Type Read-Only", false, child.OwnerTypeInfo.ReadOnly);
			});
		}

		#region Implementation
		BulkMovementsChild AddChild(BulkMovementsHeader header, string containerNumber, string movementType, ZDateTime movementDate)
		{
			BulkMovementsChild child = header.Children.AddNew();
			child.ContainerNum = containerNumber;
			child.MovementType = movementType;
			child.MovementDate = movementDate;
			return child;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkMovementsHeader(Factory).Children.AddNew();
		}
		#endregion
	}
}
