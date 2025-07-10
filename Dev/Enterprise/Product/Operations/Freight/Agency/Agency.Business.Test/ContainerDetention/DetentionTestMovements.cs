using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DetentionTestMovements
	{
		public static DetentionTestMovements CreateAndSave(BusinessObjectFactory factory, ZString localPort, ZString osPort)
		{
			DetentionTestMovements result = new DetentionTestMovements(factory);
			result.LocalPort = localPort;
			result.OSPort = osPort;
			result.SetupCompanies();
			result.SetupOrgs();
			result.SetupImportMovements();
			result.SetupExportMovements();
			factory.Save();
			return result;
		}

		DetentionTestMovements(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public ZString LocalPort { get; private set; }

		public ZString OSPort { get; private set; }

		public GlbCompany CompanyLO { get; private set; }

		public GlbCompany CompanyOS { get; private set; }

		public GlbBranch BranchLO { get; private set; }

		public GlbBranch BranchOS { get; private set; }

		public OrgHeader Client1 { get; private set; }

		public OrgHeader Client2 { get; private set; }

		public OrgHeader Client3 { get; private set; }

		public OrgHeader Principal1 { get; private set; }

		public OrgHeader Principal2 { get; private set; }

		public OrgHeader Principal3 { get; private set; }

		public OrgHeader DepotLO { get; private set; }

		public OrgHeader DepotOS { get; private set; }

		public ContainerMovement ImportP1C1 { get; private set; }

		public ContainerMovement ImportP1C2 { get; private set; }

		public ContainerMovement ImportP2C1 { get; private set; }

		public ContainerMovement ImportP1C1OS { get; private set; }

		public ContainerMovement ImportOP3C1 { get; private set; }

		public ContainerMovement ImportOP1C1 { get; private set; }

		public ContainerMovement ImportP1OC3 { get; private set; }

		public ContainerMovement ImportP1OC1 { get; private set; }

		public ContainerMovement ImportFP1C1 { get; private set; }

		public ContainerMovement ImportFP1C2 { get; private set; }

		public ContainerMovement ExportP1C1 { get; private set; }

		public ContainerMovement ExportP1C2 { get; private set; }

		public ContainerMovement ExportP2C1 { get; private set; }

		public ContainerMovement ExportP1C1OS { get; private set; }

		public ContainerMovement ExportOP3C1 { get; private set; }

		public ContainerMovement ExportOP1C1 { get; private set; }

		public ContainerMovement ExportP1OC3 { get; private set; }

		public ContainerMovement ExportP1OC1 { get; private set; }

		public ContainerMovement ExportFP1C1 { get; private set; }

		public ContainerMovement ExportFP1C2 { get; private set; }

		void SetupCompanies()
		{
			CompanyLO = factory.New<GlbCompany>();
			CompanyLO.GC_Code = "LOC";
			CompanyLO.GC_RN_NKCountryCode = LocalPort.Left(2);
			CompanyOS = factory.New<GlbCompany>();
			CompanyOS.GC_Code = "OSC";
			CompanyOS.GC_RN_NKCountryCode = OSPort.Left(2);
			BranchLO = CompanyLO.Branches.AddNew();
			BranchLO.GB_Code = "LOB";
			BranchLO.GB_RL_NKHomePort = LocalPort;
			BranchOS = CompanyOS.Branches.AddNew();
			BranchOS.GB_Code = "OSB";
			BranchOS.GB_RL_NKHomePort = OSPort;
		}

		void SetupOrgs()
		{
			Client1 = factory.NewWithValidTestData<OrgHeader>();
			Client1.OH_Code = "Client1";
			Client2 = factory.NewWithValidTestData<OrgHeader>();
			Client2.OH_Code = "Client2";
			Client3 = factory.NewWithValidTestData<OrgHeader>();
			Client3.OH_Code = "Client3";
			Principal1 = factory.NewWithValidTestData<OrgHeader>();
			Principal1.OH_Code = "Principal1";
			Principal2 = factory.NewWithValidTestData<OrgHeader>();
			Principal2.OH_Code = "Principal2";
			Principal3 = factory.NewWithValidTestData<OrgHeader>();
			Principal3.OH_Code = "Principal3";
			DepotLO = factory.NewWithValidTestData<OrgHeader>();
			DepotLO.OH_Code = "DepotLO";
			DepotLO.OH_RL_NKClosestPort = LocalPort;
			DepotOS = factory.NewWithValidTestData<OrgHeader>();
			DepotOS.OH_Code = "DepotOS";
			DepotOS.OH_RL_NKClosestPort = OSPort;
		}

		void SetupImportMovements()
		{
			JobSailing sailing = NewSailing(OSPort, LocalPort);
			AgencyShipmentContainer p1c1 = NewContainerWithMovement("IMP-P1C1", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer p1c2 = NewContainerWithMovement("IMP-P1C2", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer p2c1 = NewContainerWithMovement("IMP-P2C1", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal2, DepotLO.MainAddress);
			AgencyShipmentContainer p1c1OS = NewContainerWithMovement("IMP-P1C1-OS", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotOS.MainAddress);
			AgencyShipmentContainer op3c1 = NewContainerWithMovement("IMP-OP3C1", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer op1c1 = NewContainerWithMovement("IMP-OP1C1", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal2, DepotLO.MainAddress);
			AgencyShipmentContainer p1oc3 = NewContainerWithMovement("IMP-P1OC3", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer p1oc1 = NewContainerWithMovement("IMP-P1OC1", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notShipping = NewContainerWithMovement("IMP-NOS", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notConfirmed = NewContainerWithMovement("IMP-NOC", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer shipperOwned = NewContainerWithMovement("IMP-SHO", sailing, true, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notOverdue = NewContainerWithMovement("IMP-NID", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 0, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notReal = NewContainerWithMovement("IMP-NOR", sailing, false, false, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer alreadyInvoiced = NewContainerWithMovement("IMP-INV", sailing, false, true, ContainerMovementTypes.Codes.YardGateIn, 5, Principal1, DepotLO.MainAddress);
			ImportFP1C1 = NewFreeMovement("IMP-FP1C1", ContainerMovementTypes.Codes.ReShipRequested, 5, Principal1, Client1, DepotLO.MainAddress);
			ImportFP1C2 = NewFreeMovement("IMP-FP1C2", ContainerMovementTypes.Codes.ReShipRequested, 5, Principal1, Client2, DepotOS.MainAddress);
			notShipping.Booking.JS_IsShipping = false;
			notConfirmed.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			op3c1.Movements[0].E9_OH_Principal = Principal3.PK;
			op3c1.Movements[0].E9_DetentionDays = 5;
			op1c1.Movements[0].E9_OH_Principal = Principal1.PK;
			op1c1.Movements[0].E9_DetentionDays = 5;
			p1oc3.Movements[0].E9_OH_ResponsibleParty = Client3.PK;
			p1oc3.Movements[0].E9_DetentionDays = 5;
			p1oc1.Movements[0].E9_OH_ResponsibleParty = Client1.PK;
			p1oc1.Movements[0].E9_DetentionDays = 5;
			SetLocalClient(p1c1, BranchLO, Client1.PK);
			SetLocalClient(p1c2, BranchLO, Client2.PK);
			SetLocalClient(p2c1, BranchLO, Client1.PK);
			SetLocalClient(p1c1OS, BranchLO, Client1.PK);
			SetLocalClient(op3c1, BranchLO, Client1.PK);
			SetLocalClient(op1c1, BranchLO, Client1.PK);
			SetLocalClient(p1oc3, BranchLO, Client1.PK);
			SetLocalClient(p1oc1, BranchLO, Client2.PK);
			SetLocalClient(notShipping, BranchLO, Client1.PK);
			SetLocalClient(notConfirmed, BranchLO, Client1.PK);
			SetLocalClient(shipperOwned, BranchLO, Client1.PK);
			SetLocalClient(alreadyInvoiced, BranchLO, Client1.PK);
			SetLocalClient(notOverdue, BranchLO, Client1.PK);
			SetLocalClient(notReal, BranchLO, Client1.PK);
			SetLocalClient(p1c1, BranchOS, Client1.PK);
			SetLocalClient(p1c2, BranchOS, Client1.PK);
			SetLocalClient(p2c1, BranchOS, Client1.PK);
			SetLocalClient(p1c1OS, BranchOS, Client1.PK);
			SetLocalClient(op3c1, BranchOS, Client1.PK);
			SetLocalClient(op1c1, BranchOS, Client1.PK);
			SetLocalClient(p1oc3, BranchOS, Client1.PK);
			SetLocalClient(p1oc1, BranchOS, Client1.PK);
			SetLocalClient(notShipping, BranchOS, Client1.PK);
			SetLocalClient(notConfirmed, BranchOS, Client1.PK);
			SetLocalClient(shipperOwned, BranchOS, Client1.PK);
			SetLocalClient(alreadyInvoiced, BranchOS, Client1.PK);
			SetLocalClient(notOverdue, BranchOS, Client1.PK);
			SetLocalClient(notReal, BranchOS, Client1.PK);
			AddToInvoice("IMP-INV", alreadyInvoiced);
			ImportP1C1 = p1c1.Movements[0];
			ImportP1C2 = p1c2.Movements[0];
			ImportP2C1 = p2c1.Movements[0];
			ImportP1C1OS = p1c1OS.Movements[0];
			ImportOP3C1 = op3c1.Movements[0];
			ImportOP1C1 = op1c1.Movements[0];
			ImportP1OC3 = p1oc3.Movements[0];
			ImportP1OC1 = p1oc1.Movements[0];
		}

		void SetupExportMovements()
		{
			JobSailing sailing = NewSailing(LocalPort, OSPort);
			AgencyShipmentContainer p1c1 = NewContainerWithMovement("EXP-P1C1", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer p1c2 = NewContainerWithMovement("EXP-P1C2", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer p2c1 = NewContainerWithMovement("EXP-P2C1", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal2, DepotLO.MainAddress);
			AgencyShipmentContainer p1c1OS = NewContainerWithMovement("EXP-P1C1-OS", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotOS.MainAddress);
			AgencyShipmentContainer op3c1 = NewContainerWithMovement("EXP-OP3C1", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer op1c1 = NewContainerWithMovement("EXP-OP1C1", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal2, DepotLO.MainAddress);
			AgencyShipmentContainer p1oc3 = NewContainerWithMovement("EXP-P1OC3", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer p1oc1 = NewContainerWithMovement("EXP-P1OC1", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notShipping = NewContainerWithMovement("EXP-NOS", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notConfirmed = NewContainerWithMovement("EXP-NOC", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer shipperOwned = NewContainerWithMovement("EXP-SHO", sailing, true, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notOverdue = NewContainerWithMovement("EXP-NID", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 0, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer notReal = NewContainerWithMovement("EXP-NOR", sailing, false, false, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			AgencyShipmentContainer alreadyInvoiced = NewContainerWithMovement("EXP-INV", sailing, false, true, ContainerMovementTypes.Codes.WharfGateIn, 5, Principal1, DepotLO.MainAddress);
			ExportFP1C1 = NewFreeMovement("EXP-FP1C1", ContainerMovementTypes.Codes.ReturnedUnshipped, 5, Principal1, Client1, DepotLO.MainAddress);
			ExportFP1C2 = NewFreeMovement("EXP-FP1C2", ContainerMovementTypes.Codes.ReturnedUnshipped, 5, Principal1, Client2, DepotOS.MainAddress);
			notShipping.Booking.JS_IsShipping = false;
			notConfirmed.Booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			op3c1.Movements[0].E9_OH_Principal = Principal3.PK;
			op3c1.Movements[0].E9_DetentionDays = 5;
			op1c1.Movements[0].E9_OH_Principal = Principal1.PK;
			op1c1.Movements[0].E9_DetentionDays = 5;
			p1oc3.Movements[0].E9_OH_ResponsibleParty = Client3.PK;
			p1oc3.Movements[0].E9_DetentionDays = 5;
			p1oc1.Movements[0].E9_OH_ResponsibleParty = Client1.PK;
			p1oc1.Movements[0].E9_DetentionDays = 5;
			SetLocalClient(p1c1, BranchLO, Client1.PK);
			SetLocalClient(p1c2, BranchLO, Client2.PK);
			SetLocalClient(p2c1, BranchLO, Client1.PK);
			SetLocalClient(p1c1OS, BranchLO, Client1.PK);
			SetLocalClient(op3c1, BranchLO, Client1.PK);
			SetLocalClient(op1c1, BranchLO, Client1.PK);
			SetLocalClient(p1oc3, BranchLO, Client1.PK);
			SetLocalClient(p1oc1, BranchLO, Client2.PK);
			SetLocalClient(notShipping, BranchLO, Client1.PK);
			SetLocalClient(notConfirmed, BranchLO, Client1.PK);
			SetLocalClient(shipperOwned, BranchLO, Client1.PK);
			SetLocalClient(alreadyInvoiced, BranchLO, Client1.PK);
			SetLocalClient(notOverdue, BranchLO, Client1.PK);
			SetLocalClient(p1c1, BranchOS, Client1.PK);
			SetLocalClient(p1c2, BranchOS, Client1.PK);
			SetLocalClient(p2c1, BranchOS, Client1.PK);
			SetLocalClient(p1c1OS, BranchOS, Client1.PK);
			SetLocalClient(op3c1, BranchOS, Client1.PK);
			SetLocalClient(op1c1, BranchOS, Client1.PK);
			SetLocalClient(p1oc3, BranchOS, Client1.PK);
			SetLocalClient(p1oc1, BranchOS, Client1.PK);
			SetLocalClient(notShipping, BranchOS, Client1.PK);
			SetLocalClient(notConfirmed, BranchOS, Client1.PK);
			SetLocalClient(shipperOwned, BranchOS, Client1.PK);
			SetLocalClient(alreadyInvoiced, BranchOS, Client1.PK);
			SetLocalClient(notOverdue, BranchOS, Client1.PK);
			AddToInvoice("EXP-INV", alreadyInvoiced);
			ExportP1C1 = p1c1.Movements[0];
			ExportP1C2 = p1c2.Movements[0];
			ExportP2C1 = p2c1.Movements[0];
			ExportP1C1OS = p1c1OS.Movements[0];
			ExportOP3C1 = op3c1.Movements[0];
			ExportOP1C1 = op1c1.Movements[0];
			ExportP1OC3 = p1oc3.Movements[0];
			ExportP1OC1 = p1oc1.Movements[0];
		}

		void SetLocalClient(AgencyShipmentContainer container, GlbBranch branch, ZGuid localClient)
		{
			OrgHeader org = factory.Load<OrgHeader>(localClient);
			JobHeader header = factory.NewJobForTesting<JobHeader>();
			header.JH_GC = branch.GB_GC;
			header.JH_GB = branch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_OA_LocalChargesAddr = org != null ? org.MainAddress.PK : ZGuid.Empty;
			header.JH_ParentID = container.JC_JS_FCLBookingOnlyLink;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_JobNum = container.JC_ContainerNum;
		}

		void AddToInvoice(string jobNum, params AgencyShipmentContainer[] containers)
		{
			if (containers.Length > 0)
			{
				ContainerDetention detention = factory.New<ContainerDetention>();
				detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
				detention.NC_JobNumber = jobNum;
				detention.NC_OH_Client = factory.NewWithValidTestData<OrgHeader>().PK;
				detention.NC_OH_Principal = factory.NewWithValidTestData<OrgHeader>().PK;
				foreach (AgencyShipmentContainer container in containers)
				{
					foreach (ContainerMovement movement in container.Movements)
					{
						movement.E9_NC = detention.PK;
					}
				}
			}
		}

		ContainerMovement NewFreeMovement(string name, string movementType, int detention, OrgHeader principal, OrgHeader client, OrgAddress depot)
		{
			RefContainerStock stock = factory.New<RefContainerStock>();
			stock.R6_ContainerNum = name;
			stock.R6_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = movementType;
			movement.E9_OH_Principal = principal.PK;
			movement.E9_OH_ResponsibleParty = client.PK;
			movement.E9_OA_Depot = depot.PK;
			movement.E9_DetentionDays = (short)detention;
			movement.E9_OtherLocation = name;
			return movement;
		}

		AgencyShipmentContainer NewContainerWithMovement(string name, JobSailing sailing, bool isShipperOwned, bool isReal, string movementType, int detention, OrgHeader principal, OrgAddress depot)
		{
			RefContainerStock stock = factory.New<RefContainerStock>();
			stock.R6_ContainerNum = name;
			stock.R6_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipment shipment = factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_RL_NKOrigin = sailing.JX_JA_RL_NKPortOfLoading;
			shipment.JS_RL_NKDestination = sailing.JX_JB_RL_NKPortOfDischarge;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_JX = sailing.PK;
			AgencyShipmentContainerDependentCollection containers = isReal ? shipment.RealContainers : shipment.BookedContainers;
			AgencyShipmentContainer container = containers.AddNew();
			container.JC_ContainerNum = name;
			container.JC_IsShipperOwned = isShipperOwned;
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_JV = sailing.Origin.JA_JV;
			movement.E9_MovementType = movementType;
			movement.E9_DetentionDays = (short)detention;
			movement.E9_OtherLocation = name;
			movement.E9_OA_Depot = depot.PK;
			return container;
		}

		JobSailing NewSailing(string origin, string destination)
		{
			JobVoyage voyage = factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = origin;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = destination;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		readonly BusinessObjectFactory factory;
	}
}
