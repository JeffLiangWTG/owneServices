using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BulkDetentionChild))]
	internal class BulkDetentionChildTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType("lookups class should be of the correct type", typeof(BulkDetentionChildLookups), Child.Lookups);
			AssertSame("Should cache the lookups class", Child.Lookups, Child.Lookups);
		}

		public void TestCreateDetentionInvoice_WithoutCreatingInvoice()
		{
			OrganisationsDataRegistry.Instance.UseARSettlementGroupCreditLimit.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			mock.Setup(m => m.APAccountGroup).Returns(Factory.NewWithValidTestData<OrgCreditorGroup>().PK.ToGuid());
			mock.Setup(m => m.ARAccountGroup).Returns(Factory.NewWithValidTestData<OrgDebtorGroup>().PK.ToGuid());
			var stringRegistryItem = new StringRegistryItem("IncludeUnpostedRevenueInCreditLimitCalculation", // This is a registry item name
 null, null, null, RegistryStorageFlags.System, Core.Constants.CreditLimitChecking.Posted);
			mock.Setup(m => m.IncludeUnpostedRevenueInCreditLimitCalculation).Returns(stringRegistryItem);
			using (ObjectFactory.Substitute(mock.Object))
			{
				RefContainerStock stock = CreateStock("TEST4100013", "20GP");
				ContainerMovement movement = CreateImpDetMovement(stock, 3);
				Factory.Save();
				Factory.ResetDatabaseLoadCount();
				Child.DetentionType = DetentionInvoiceType.Codes.Import;
				Child.ClientPK = Client.PK;
				Child.PrincipalPK = Principal.PK;
				Child.MovementPKs.Add(movement.PK);
				Child.IsSelected = true;
				BusinessObjectFactory createFactory = new BusinessObjectFactory();
				Child.CreateDetentionInvoice(createFactory);
				createFactory.Save();
				AssertMaxDbHits("Should not have loaded anything in the form factory.", 0, Factory);
				BusinessObjectFactory resultFactory = new BusinessObjectFactory();
				ContainerMovement resultMovement = resultFactory.Load<ContainerMovement>(movement.PK);
				ContainerDetention resultDetention = resultMovement.Detention;
				AssertNotNull("should be attached to a detention", resultDetention);
				AssertNull("should not have created a job header", resultDetention.Job);
			}
		}

		public void TestCreateDetentionInvoice_CreatingInvoice()
		{
			RefContainerStock stock = CreateStock("TEST4100013", "20GP");
			ContainerMovement movement = CreateImpDetMovement(stock, 3);
			Factory.Save();
			Factory.ResetDatabaseLoadCount();
			Child.DetentionType = DetentionInvoiceType.Codes.Import;
			Child.ClientPK = Client.PK;
			Child.PrincipalPK = Principal.PK;
			Child.MovementPKs.Add(movement.PK);
			Child.IsSelected = true;
			BusinessObjectFactory createFactory = new BusinessObjectFactory();
			Child.CreateDetentionInvoice(createFactory);
			createFactory.Save();
			AssertMaxDbHits("Should not have loaded anything in the form factory.", 0, Factory);
			BusinessObjectFactory resultFactory = new BusinessObjectFactory();
			ContainerMovement resultMovement = resultFactory.Load<ContainerMovement>(movement.PK);
			ContainerDetention resultDetention = resultMovement.Detention;
			AssertNotNull("should be attached to a detention", resultDetention);
			AssertNotNull("should have created a job header", resultDetention.Job);
			AssertEquals("Job.JH_OA_LocalChargesAddr", Client.MainAddress.PK, resultDetention.Job.JH_OA_LocalChargesAddr);
		}

		public void TestIsSelectedReadOnly()
		{
			BulkDetentionChild child = new BulkDetentionChild(Factory);
			AssertEquals(false, child.IsSelectedInfo.ReadOnly);
			child.IsLocked = true;
			AssertEquals(true, child.IsSelectedInfo.ReadOnly);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkDetentionChild(Factory);
		}

		BulkDetentionChild Child
		{
			get
			{
				return child ?? (child = new BulkDetentionChild(Factory));
			}
		}

		BulkDetentionChild child;
		OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "Client1";
				}

				return client;
			}
		}

		OrgHeader client;
		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = Factory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "principal";
				}

				return principal;
			}
		}

		OrgHeader principal;
		RefContainerStock CreateStock(string containerNum, string containerType)
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = containerNum;
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			return stock;
		}

		JobSailing CreateSailing(string origin, string destination)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = origin;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = destination;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		OrgHeader CreateDepot(string detentionPort)
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "Depot";
			depot.OH_RL_NKClosestPort = detentionPort;
			return depot;
		}

		ContainerMovement CreateImpDetMovement(RefContainerStock stock, short days)
		{
			return CreateDetMovement(stock, CreateDepot("AUBNE"), CreateSailing("NLAMS", "AUBNE"), ContainerMovementTypes.Codes.YardGateIn, days);
		}

		ContainerMovement CreateDetMovement(RefContainerStock stock, OrgHeader depot, JobSailing sailing, string movementType, short days)
		{
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = movementType;
			if (depot != null)
			{
				movement.E9_OA_Depot = depot.MainAddress.PK;
			}

			if (sailing != null)
			{
				movement.E9_JV = sailing.Origin.JA_JV;
				BillOfLading bill = Factory.New<BillOfLading>();
				bill.JS_OH_DeliveryAgent = Principal.PK;
				bill.JS_JX = sailing.PK;
				JobHeader job = new JobHeader.Loader(bill).TryCreate();
				job.JH_OA_LocalChargesAddr = Client.MainAddress.PK;
				BillOfLadingContainer container = bill.RealContainers.AddNew();
				container.JC_ContainerNum = stock.R6_ContainerNum;
				container.JC_RC = stock.R6_RC;
			}

			movement.E9_DetentionDays = days;
			return movement;
		}
		#endregion
	}
}
