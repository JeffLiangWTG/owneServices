using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class ContainerDetentionAdviceProviderTest : TestCaseWithFactory
	{
		public void TestReshipRequest()
		{
			const string ContainerNumber = "TEST4100013";
			ZDateTime now = ZDateTime.Now;
			var depot = GetOrgHeader("AUBNE").MainAddress;
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage1.JV_VoyageFlight = "001";
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = now.AddDays(-7);
			destination1.JB_AvailabilityDate = now.AddDays(-6);
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First().RV_FK;
			voyage2.JV_VoyageFlight = "002";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage1.Sailings[0].PK;
			bill.ConsignorPK = Client1.PK;
			bill.ConsigneePK = Client2.PK;
			JobHeader header = new JobHeader.Loader(bill).TryCreate();
			header.JH_OA_LocalChargesAddr = Client2.MainAddress.PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerNum = ContainerNumber;
			container.JC_EmptyReturnedBy = now.AddDays(4);
			ContainerMovement movement1 = NewMovement(ContainerNumber, depot, ContainerMovementTypes.Codes.WharfGateOut);
			movement1.E9_MovementDate = now.AddDays(-5);
			movement1.E9_JV = voyage1.PK;
			Factory.Save();
			AssertContainersAndMovements(new BillOfLadingContainer[] { container }, Array.Empty<ContainerMovement>());
			ContainerMovement movement2 = NewMovement(ContainerNumber, depot, ContainerMovementTypes.Codes.ReShipRequested);
			movement2.E9_MovementDate = now.AddDays(-4);
			movement2.E9_JV = voyage1.PK;
			Factory.Save();
			AssertContainersAndMovements(Array.Empty<BillOfLadingContainer>(), new ContainerMovement[] { movement2 });
			ContainerMovement movement3 = NewMovement(ContainerNumber, depot, ContainerMovementTypes.Codes.ReturnedUnshipped);
			movement3.E9_MovementDate = now.AddDays(-3);
			movement3.E9_JV = voyage2.PK;
			Factory.Save();
			AssertContainersAndMovements(Array.Empty<BillOfLadingContainer>(), Array.Empty<ContainerMovement>());
		}

		public void TestMultipleImportContainersPerBill()
		{
			ZDateTime now = ZDateTime.Now;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "001";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = now.AddDays(-7);
			destination.JB_AvailabilityDate = now.AddDays(-6);
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.ConsignorPK = Client1.PK;
			bill.ConsigneePK = Client2.PK;
			JobHeader header = new JobHeader.Loader(bill).TryCreate();
			header.JH_OA_LocalChargesAddr = Client2.MainAddress.PK;
			BillOfLadingContainer container1 = bill.RealContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_EmptyReturnedBy = now.AddDays(4);
			BillOfLadingContainer container2 = bill.RealContainers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_EmptyReturnedBy = now.AddDays(4);
			Factory.Save();
			AssertContainersAndMovements(new BillOfLadingContainer[] { container1, container2 }, Array.Empty<ContainerMovement>());
		}

		public void TestNonDateFiltering()
		{
			// Containers
			var cnfExpectedC = NewContainer("TEST4300011", "NLAMS", "AUBNE");
			cnfExpectedC.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			var wfiExpectedC = NewContainer("TEST4300027", "NLAMS", "AUBNE");
			wfiExpectedC.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;
			var bookedConatinerC = NewContainer("TEST4300053", "NLAMS", "AUBNE");
			bookedConatinerC.JC_Purpose = ContainerBookedStatus.Codes.Booked;
			var bookingC = NewContainer("TEST4300069", "NLAMS", "AUBNE");
			bookingC.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var exportC = NewContainer("TEST4300074", "AUBNE", "NLAMS");
			var cancelledC = NewContainer("TEST4300080", "NLAMS", "AUBNE");
			cancelledC.Booking.JS_IsCancelled = true;
			var shipperOwnedC = NewContainer("TEST4300095", "NLAMS", "AUBNE");
			shipperOwnedC.JC_IsShipperOwned = true;
			var returnedC = NewContainer("TEST4300109", "NLAMS", "AUBNE", Today.AddDays(-2), Today.AddDays(-1));
			// Movements
			var localDepot1 = GetOrgHeader("AUBNE").MainAddress;
			var localDepot2 = GetOrgAddress("AUBNE", "AUSYD");
			var localDepot3 = GetOrgAddress("AUBNE", ZString.Empty);
			var localDepot4 = GetOrgAddress("NLAMS", "AUBNE");
			var foreignDepot1 = GetOrgHeader("NLAMS").MainAddress;
			var foreignDepot2 = GetOrgAddress("NLAMS", "NLAMS");
			var foreignDepot3 = GetOrgAddress("NLAMS", ZString.Empty);
			var foreignDepot4 = GetOrgAddress("AUBNE", "NLAMS");
			var movement1 = NewMovement("movement1", localDepot1, ContainerMovementTypes.Codes.YardGateOut);
			var movement2 = NewMovement("movement2", localDepot2, ContainerMovementTypes.Codes.ReShipRequested);
			var movement3 = NewMovement("movement3", localDepot3, ContainerMovementTypes.Codes.YardGateOut);
			var movement4 = NewMovement("movement4", localDepot4, ContainerMovementTypes.Codes.YardGateOut);
			var movement5 = NewMovement("movement5", localDepot1, ContainerMovementTypes.Codes.YardGateIn);
			var movement6 = NewMovement("movement6", foreignDepot1, ContainerMovementTypes.Codes.YardGateOut);
			var movement7 = NewMovement("movement7", foreignDepot2, ContainerMovementTypes.Codes.YardGateOut);
			var movement8 = NewMovement("movement8", foreignDepot3, ContainerMovementTypes.Codes.YardGateOut);
			var movement9 = NewMovement("movement9", foreignDepot4, ContainerMovementTypes.Codes.YardGateOut);
			var movement10 = NewMovement("movement10", localDepot1, ContainerMovementTypes.Codes.YardGateOut);
			movement10.Stock.R6_OwnerType = "SHP";
			Factory.Save();
			var containers = new List<BillOfLadingContainer>();
			var movements = new List<ContainerMovement>();
			foreach (var advice in new ContainerDetentionAdviceProvider(Company1, Today))
			{
				containers.AddRange(advice.Containers);
				movements.AddRange(advice.Movements);
			}

			AssertContainsExactElementsInAnyOrder("Should find these containers", BusinessObjectEqualityComparer<BillOfLadingContainer>.IgnoreFactoryComparer, (c) => c.JC_ContainerNum, new BillOfLadingContainer[] { cnfExpectedC, wfiExpectedC }, containers);
			AssertContainsExactElementsInAnyOrder("Should find these movements", BusinessObjectEqualityComparer<ContainerMovement>.IgnoreFactoryComparer, (m) => m.E9_OtherLocation, new ContainerMovement[] { movement1, movement2, movement3, movement4 }, movements);
		}

		public void TestDateFiltering()
		{
			var depot = GetOrgHeader("AUBNE").MainAddress;
			var container1 = NewContainer("container1", "NLAMS", "AUBNE", Today.AddDays(-1), ZDateTime.Empty);
			var container2 = NewContainer("container2", "NLAMS", "AUBNE", Today.AddDays(-2), ZDateTime.Empty);
			var container3 = NewContainer("container3", "NLAMS", "AUBNE", Today.AddDays(-2), Today.AddDays(1));
			var container4 = NewContainer("container4", "NLAMS", "AUBNE", ZDateTime.Empty, ZDateTime.Empty);
			var container5 = NewContainer("container5", "NLAMS", "AUBNE", Today.AddDays(1), ZDateTime.Empty);
			var container6 = NewContainer("container6", "NLAMS", "AUBNE", Today.AddDays(-2), Today.AddDays(-1));
			var container7 = NewContainer("container7", "NLAMS", "AUBNE", Today.AddDays(1), ZDateTime.Empty, Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-3));
			AddTransportLeg(container7.Booking, "AUBNE", "AUSYD", Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-2), ZDateTime.Empty);
			var container8 = NewContainer("container8", "NLAMS", "AUBNE", Today.AddDays(-2), ZDateTime.Empty, Today.AddDays(-1), Today.AddDays(-3), ZDateTime.Empty, ZDateTime.Empty);
			AddTransportLeg(container8.Booking, "AUBNE", "AUSYD", Today.AddDays(1), Today.AddDays(-2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			var container9 = NewContainer("container9", "NLAMS", "AUBNE", Today.AddDays(-2), ZDateTime.Empty, Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-3));
			AddTransportLeg(container9.Booking, "AUBNE", "AUSYD", ZDateTime.Empty, Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-1), Today.AddDays(-2));
			var container10 = NewContainer("container10", "NLAMS", "AUBNE", ZDateTime.Empty, ZDateTime.Empty, Today.AddDays(-1), ZDateTime.Empty, Today.AddDays(-1), Today.AddDays(-4));
			AddTransportLeg(container10.Booking, "AUBNE", "AUSYD", ZDateTime.Empty, Today.AddDays(-1), Today.AddDays(-1), ZDateTime.Empty, Today.AddDays(-3));
			AddTransportLeg(container10.Booking, GetVoyage(), "AUSYD", "AUMEL", Today.AddDays(-1), Today.AddDays(-2), ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			var container11 = NewContainer("container11", "NLAMS", "AUBNE", Today.AddDays(-2), Today);
			var container12 = NewContainer("container12", "NLAMS", "AUBNE", Today.AddDays(-1), Today);
			var movement1 = NewMovement("movement1", depot, ZDateTime.Empty, ZDateTime.Empty);
			var movement2 = NewMovement("movement2", depot, ZDateTime.Empty, Today.AddDays(-1));
			var movement3 = NewMovement("movement3", depot, ZDateTime.Empty, Today.AddDays(1));
			var movement4 = NewMovement("movement4", depot, Today.AddDays(-2), ZDateTime.Empty);
			var movement5 = NewMovement("movement5", depot, Today.AddDays(-2), Today.AddDays(-1));
			var movement6 = NewMovement("movement6", depot, Today.AddDays(-2), Today.AddDays(1));
			var movement7 = NewMovement("movement7", depot, Today.AddDays(1), ZDateTime.Empty);
			var movement8 = NewMovement("movement8", depot, Today.AddDays(1), Today.AddDays(2));
			var movement9 = NewMovement("movement9", depot, Today.AddDays(-1), Today);
			Factory.Save();
			var containers = new List<BillOfLadingContainer>();
			var movements = new List<ContainerMovement>();
			foreach (var advice in new ContainerDetentionAdviceProvider(GlbCompany.CurrentCompany, Today))
			{
				containers.AddRange(advice.Containers);
				movements.AddRange(advice.Movements);
			}

			AssertContainsExactElementsInAnyOrder("Should find these containers", BusinessObjectEqualityComparer<BillOfLadingContainer>.IgnoreFactoryComparer, (c) => c.JC_ContainerNum, new BillOfLadingContainer[] { container1, container2, container3, container7, container10 }, containers);
			AssertContainsExactElementsInAnyOrder("Should find these movements", BusinessObjectEqualityComparer<ContainerMovement>.IgnoreFactoryComparer, (m) => m.E9_OtherLocation, new ContainerMovement[] { movement4, movement6 }, movements);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGrouping()
		{
			var depot = GetOrgHeader("AUBNE").MainAddress;
			var container1 = NewContainer("container1", "NLAMS", "AUBNE", Client4, Client1, null, null);
			var container2 = NewContainer("container2", "NLAMS", "AUCNS", Client4, Client3, Branch1, Client1);
			var container3 = NewContainer("container3", "NLAMS", "AUSYD", Client4, Client1, Branch1, Client3);
			var container4 = NewContainer("container4", "NLAMS", "AUMEL", Client4, Client3, Branch1, Client1);
			var container5 = NewContainer("container5", "NLAMS", "AUCNS", Client4, null, null, null);
			var container6 = NewContainer("container6", "NLAMS", "AUBNE", Client4, null, Branch1, Client3);
			var container7 = NewContainer("container7", "NLAMS", "AUBNE", Client4, null, null, null);
			var container8 = NewContainer("container8", "NLAMS", "AUSYD", Client4, null, null, null);
			var container9 = NewContainer("container9", "NLAMS", "AUBNE", Client4, null, Branch2, Client1);
			var container10 = NewContainer("container10", "AUBNE", "NLAMS", Client4, Client1, null, null);
			var movement1 = NewMovement("movement1", depot, null, Client3, null, null, Client1);
			var movement2 = NewMovement("movement2", depot, null, Client3, Branch1, Client1, Client2);
			var movement3 = NewMovement("movement3", depot, Client4, Client3, null, null, null);
			var movement4 = NewMovement("movement4", depot, null, Client3, null, null, null);
			var movement5 = NewMovement("movement5", depot, Client4, Client3, Branch1, Client1, null);
			var movement6 = NewMovement("movement6", depot, null, null, null, null, null);
			SetClient(AttachContainer(movement6), Branch1, Client2);
			SetClient(AttachContainer(movement6), Branch1, Client4);
			Factory.Save();
			var provider = new ContainerDetentionAdviceProvider(Company1, Today);
			var advices = new List<DetentionAdviceHeader>(provider).ToArray();
			AssertContainsExactElementsInAnyOrder("should group correctly", BusinessObjectEqualityComparer<OrgHeader>.IgnoreFactoryComparer, (o) => o == null ? "<NULL>" : o.OH_Code.ToString(), new OrgHeader[] { Client1, Client2, Client3, Client4, null }, Array.ConvertAll(advices, (a) => a.Client));
			var expectedContainers = new Dictionary<ZGuid, IEnumerable<BillOfLadingContainer>>();
			expectedContainers.Add(Client1.PK, new BillOfLadingContainer[] { container1, container2, container4 });
			expectedContainers.Add(Client2.PK, Array.Empty<BillOfLadingContainer>());
			expectedContainers.Add(Client3.PK, new BillOfLadingContainer[] { container3, container6 });
			expectedContainers.Add(Client4.PK, Array.Empty<BillOfLadingContainer>());
			expectedContainers.Add(ZGuid.Empty, new BillOfLadingContainer[] { container7, container8, container9, container5 });
			var expectedMovements = new Dictionary<ZGuid, IEnumerable<ContainerMovement>>();
			expectedMovements.Add(Client1.PK, new ContainerMovement[] { movement1, movement5 });
			expectedMovements.Add(Client2.PK, new ContainerMovement[] { movement2, movement6 });
			expectedMovements.Add(Client3.PK, Array.Empty<ContainerMovement>());
			expectedMovements.Add(Client4.PK, new ContainerMovement[] { movement3, movement6 });
			expectedMovements.Add(ZGuid.Empty, new ContainerMovement[] { movement4 });
			CombineAssertions(delegate
			{
				foreach (var advice in advices)
				{
					string label;
					ZGuid pk;
					if (advice.Client == null)
					{
						label = "<NULL>";
						pk = ZGuid.Empty;
					}
					else
					{
						label = advice.Client.OH_Code;
						pk = advice.Client.PK;
					}

					AssertContainsExactElementsInAnyOrder(label + " should have the following containers", BusinessObjectEqualityComparer<BillOfLadingContainer>.IgnoreFactoryComparer, (c) => c.JC_ContainerNum, expectedContainers[pk], advice.Containers);
					AssertContainsExactElementsInAnyOrder(label + " should have the following movements", BusinessObjectEqualityComparer<ContainerMovement>.IgnoreFactoryComparer, (m) => m.E9_OtherLocation, expectedMovements[pk], advice.Movements);
				}
			});
		}

		#region Implementation
		void AssertContainersAndMovements(IEnumerable<BillOfLadingContainer> expectedContainers, IEnumerable<ContainerMovement> expectedMovements)
		{
			List<BillOfLadingContainer> actualContainers = new List<BillOfLadingContainer>();
			List<ContainerMovement> actualMovements = new List<ContainerMovement>();
			foreach (DetentionAdviceHeader advice in new ContainerDetentionAdviceProvider(Company1, Today))
			{
				actualContainers.AddRange(advice.Containers);
				actualMovements.AddRange(advice.Movements);
			}

			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("Containers: ", BusinessObjectEqualityComparer<BillOfLadingContainer>.IgnoreFactoryComparer, (c) => c.JC_ContainerNum, expectedContainers, actualContainers);
				AssertContainsExactElementsInAnyOrder("Movements: ", BusinessObjectEqualityComparer<ContainerMovement>.IgnoreFactoryComparer, (m) => m.E9_OtherLocation, expectedMovements, actualMovements);
			});
		}

		int index;
		readonly ZDateTime Today = ZDateTime.Today;
		GlbCompany Company1
		{
			get
			{
				if (company1 == null)
				{
					company1 = Factory.New<GlbCompany>();
					company1.GC_Code = "C1";
					company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				}

				return company1;
			}
		}

		GlbCompany company1;
		GlbCompany Company2
		{
			get
			{
				if (company2 == null)
				{
					company2 = Factory.New<GlbCompany>();
					company2.GC_Code = "C2";
					company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				}

				return company2;
			}
		}

		GlbCompany company2;
		GlbBranch Branch1
		{
			get
			{
				if (branch1 == null)
				{
					branch1 = Company1.Branches.AddNew();
					branch1.GB_Code = "B1";
				}

				return branch1;
			}
		}

		GlbBranch branch1;
		GlbBranch Branch2
		{
			get
			{
				if (branch2 == null)
				{
					branch2 = Company2.Branches.AddNew();
					branch2.GB_Code = "B2";
				}

				return branch2;
			}
		}

		GlbBranch branch2;
		OrgHeader Client1
		{
			get
			{
				if (client1 == null)
				{
					client1 = Factory.NewWithValidTestData<OrgHeader>();
					client1.OH_Code = "Client1";
				}

				return client1;
			}
		}

		OrgHeader client1;
		OrgHeader Client2
		{
			get
			{
				if (client2 == null)
				{
					client2 = Factory.NewWithValidTestData<OrgHeader>();
					client2.OH_Code = "Client2";
				}

				return client2;
			}
		}

		OrgHeader client2;
		OrgHeader Client3
		{
			get
			{
				if (client3 == null)
				{
					client3 = Factory.NewWithValidTestData<OrgHeader>();
					client3.OH_Code = "Client3";
				}

				return client3;
			}
		}

		OrgHeader client3;
		OrgHeader Client4
		{
			get
			{
				if (client4 == null)
				{
					client4 = Factory.NewWithValidTestData<OrgHeader>();
					client4.OH_Code = "Client4";
				}

				return client4;
			}
		}

		OrgHeader client4;
		ContainerMovement NewMovement(ZString containerNum, OrgAddress depot, ZString movementType)
		{
			return NewMovement(containerNum, depot, null, movementType);
		}

		ContainerMovement NewMovement(ZString containerNum, OrgAddress depot, OrgHeader consignor, OrgHeader consignee, GlbBranch branch, OrgHeader client, OrgHeader responsibleParty)
		{
			var movement = NewMovement(containerNum, depot, responsibleParty, ContainerMovementTypes.Codes.YardGateOut);
			if (branch != null && client != null || consignor != null || consignee != null)
			{
				var container = AttachContainer(movement);
				if (branch != null && client != null)
				{
					SetClient(container, branch, client);
				}

				container.Booking.ConsignorPK = (consignor != null) ? consignor.PK : ZGuid.Empty;
				container.Booking.ConsigneePK = (consignee != null) ? consignee.PK : ZGuid.Empty;
			}

			return movement;
		}

		ContainerMovement NewMovement(ZString containerNum, OrgAddress depot, OrgHeader responsibleParty, ZString movementType)
		{
			var movement = NewMovement(containerNum, depot, Today.AddDays(-1), ZDateTime.Empty, movementType);
			if (responsibleParty != null)
			{
				movement.E9_OH_ResponsibleParty = responsibleParty.PK;
			}

			return movement;
		}

		ContainerMovement NewMovement(ZString containerNum, OrgAddress depot, ZDateTime released, ZDateTime returned)
		{
			return NewMovement(containerNum, depot, released, returned, ContainerMovementTypes.Codes.YardGateOut);
		}

		ContainerMovement NewMovement(ZString containerNum, OrgAddress depot, ZDateTime released, ZDateTime returned, ZString movementType)
		{
			var stock = GetStock(containerNum);
			var releaseMovement = stock.Movements.AddNew();
			releaseMovement.E9_MovementType = movementType;
			releaseMovement.E9_MovementDate = released;
			releaseMovement.E9_OA_Depot = depot.PK;
			releaseMovement.E9_OtherLocation = ZString.Format("{0} - {1}", containerNum, releaseMovement.E9_MovementType); // For Assertion
			if (!returned.IsEmpty)
			{
				var returnedMovement = stock.Movements.AddNew();
				returnedMovement.E9_MovementType = ContainerMovementTypes.Codes.ReturnedUnshipped;
				returnedMovement.E9_MovementDate = returned;
				returnedMovement.E9_OA_Depot = depot.PK;
				returnedMovement.E9_OtherLocation = ZString.Format("{0} - {1}", containerNum, returnedMovement.E9_MovementType); // For Assertion
			}

			return releaseMovement;
		}

		BillOfLadingContainer NewContainer(ZString containerNum, ZString load, ZString discharge)
		{
			return NewContainer(containerNum, load, discharge, null, null, null, null);
		}

		BillOfLadingContainer NewContainer(ZString containerNum, ZString load, ZString discharge, OrgHeader consignor, OrgHeader consignee, GlbBranch branch, OrgHeader client)
		{
			var container = NewContainer(containerNum, load, discharge, Today.AddDays(-1), ZDateTime.Empty);
			if (branch != null && client != null)
			{
				SetClient(container, branch, client);
			}

			container.Booking.ConsignorPK = (consignor != null) ? consignor.PK : ZGuid.Empty;
			container.Booking.ConsigneePK = (consignee != null) ? consignee.PK : ZGuid.Empty;
			return container;
		}

		BillOfLadingContainer NewContainer(ZString containerNum, ZString load, ZString discharge, ZDateTime available, ZDateTime returned)
		{
			return NewContainer(containerNum, load, discharge, available, returned, Today.AddDays(-8), Today.AddDays(-7), Today.AddDays(-6), Today.AddDays(-5));
		}

		BillOfLadingContainer NewContainer(ZString containerNum, ZString load, ZString discharge, ZDateTime available, ZDateTime returned, ZDateTime eTD, ZDateTime aTD, ZDateTime eTA, ZDateTime aTA)
		{
			var stock = GetStock(containerNum);
			var voyage = GetVoyage();
			var sailing = GetOrCreateSailing(voyage, load, discharge, eTD, aTD, eTA, aTA);
			sailing.Destination.JB_AvailabilityDate = available;
			var bill = Factory.New<BillOfLading>();
			bill.JS_RL_NKOrigin = load;
			bill.JS_RL_NKDestination = discharge;
			bill.JS_JX = sailing.PK;
			var container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = containerNum;
			if (!returned.IsEmpty)
			{
				var movement = stock.Movements.AddNew();
				movement.E9_JV = voyage.PK;
				movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement.E9_MovementDate = returned;
			}

			return container;
		}

		RefContainerStock GetStock(ZString containerNum)
		{
			var stock = Factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, containerNum));
			if (stock == null)
			{
				stock = Factory.New<RefContainerStock>();
				stock.R6_ContainerNum = containerNum;
				stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			}

			return stock;
		}

		OrgHeader GetOrgHeader(ZString closestPort)
		{
			return GetOrgHeader("Org" + index++, closestPort);
		}

		OrgHeader GetOrgHeader(ZString code, ZString closestPort)
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = code;
			header.OH_RL_NKClosestPort = closestPort;
			return header;
		}

		OrgAddress GetOrgAddress(ZString headerPort, ZString addressPort)
		{
			var address = GetOrgHeader(headerPort).Addresses.AddNew();
			address.OA_Code = "ADDR" + index++;
			address.OA_Address1 = address.OA_Code;
			address.OA_RL_NKRelatedPortCode = addressPort;
			return address;
		}

		void SetClient(BillOfLadingContainer container, GlbBranch branch, OrgHeader localClient)
		{
			var header = new JobHeader.Loader(container.Booking).TryLoadOrCreate(branch);
			header.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
		}

		BillOfLadingContainer AttachContainer(ContainerMovement movement)
		{
			var voyage = movement.Voyage;
			if (voyage == null)
			{
				voyage = GetVoyage();
				movement.E9_JV = voyage.PK;
			}

			var bill = Factory.New<BillOfLading>();
			bill.JS_JX = GetOrCreateSailing(voyage, "NZAKL", "AUBNE", Today.AddDays(-8), Today.AddDays(-7), Today.AddDays(-6), Today.AddDays(-5)).PK;
			var container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = movement.Stock.R6_ContainerNum;
			container.JC_RC = movement.Stock.R6_RC;
			return container;
		}

		JobVoyage GetVoyage()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel" + index++;
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = index++.ToString();
			return voyage;
		}

		JobSailing GetOrCreateSailing(JobVoyage voyage, ZString load, ZString discharge, ZDateTime eTD, ZDateTime aTD, ZDateTime eTA, ZDateTime aTA)
		{
			var origin = voyage.Origins.GetOriginFromLoading(load);
			if (origin == null)
			{
				origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = load;
			}

			origin.JA_E_DEP = eTD;
			origin.JA_A_DEP = aTD;
			var destination = voyage.Destinations.GetDestinationFromDischarge(discharge);
			if (destination == null)
			{
				destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = discharge;
			}

			destination.JB_E_ARV = eTA;
			destination.JB_A_ARV = aTA;
			return voyage.Sailings.GetSailingFromLoadAndDischarge(load, discharge);
		}

		void AddTransportLeg(BillOfLading bill, ZString origin, ZString destination, ZDateTime available, ZDateTime eTD, ZDateTime aTD, ZDateTime eTA, ZDateTime aTA)
		{
			AddTransportLeg(bill, bill.Sailing.Voyage, origin, destination, available, eTD, aTD, eTA, aTA);
		}

		void AddTransportLeg(BillOfLading bill, JobVoyage voyage, ZString origin, ZString destination, ZDateTime available, ZDateTime eTD, ZDateTime aTD, ZDateTime eTA, ZDateTime aTA)
		{
			var sailing = GetOrCreateSailing(voyage, origin, destination, eTD, aTD, eTA, aTA);
			sailing.Destination.JB_AvailabilityDate = available;
			var transport = bill.TransportsIncludingRelated.AddNew();
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
		}
		#endregion
	}
}
