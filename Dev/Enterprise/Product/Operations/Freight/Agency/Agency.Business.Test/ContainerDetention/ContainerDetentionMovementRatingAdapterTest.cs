using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovement))]
	internal sealed class ContainerDetentionMovementRatingAdapterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJobDatesProvider()
		{
			AssertType<DetentionContainerRatingJobDatesProvider>(((IAutoRating)Adapter).JobDatesProvider);
		}

		public void TestImportBroker()
		{
			AssertNull(((IAutoRating)Adapter).ImportBroker);
		}

		public void TestExportBroker()
		{
			AssertNull(((IAutoRating)Adapter).ExportBroker);
		}

		public void TestCanExpandMacros()
		{
			AssertEquals(true, ((IAutoRatingDescriptionMacroExpander)Adapter).CanExpandMacros);
		}

		public void TestExpandMacroMinimal()
		{
			Stock.R6_ContainerNum = "";
			Stock.R6_RC = ZGuid.Empty;
			Movement.E9_DetentionDays = 0;
			IAutoRatingDescriptionMacroExpander expander = Adapter;
			AssertEquals("Vessel", "", expander.ExpandMacro("vessel"));
			AssertEquals("Voyage", "", expander.ExpandMacro("voyage"));
			AssertEquals("Origin", "", expander.ExpandMacro("origin"));
			AssertEquals("Destination", "", expander.ExpandMacro("destination"));
			AssertEquals("DetentionPort", "", expander.ExpandMacro("detentionport"));
			AssertEquals("Container Number", "", expander.ExpandMacro("containernum"));
			AssertEquals("Container Type", "", expander.ExpandMacro("containertype"));
			AssertEquals("Detention Days", "0", expander.ExpandMacro("detentiondays"));
			AssertEquals("BillNum", "", expander.ExpandMacro("billnum"));
			AssertEquals("Invalid", null, expander.ExpandMacro("xxx"));
		}

		public void TestExpandMacro()
		{
			Stock.R6_ContainerNum = "TEST4100013";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "081";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_RL_NKClosestPort = "AUBNE";
			Shipment.JS_RL_NKOrigin = "AUMEL";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_JX = voyage.Sailings[0].PK;
			Shipment.JS_HouseBill = "ABILL";
			Container.JC_ContainerNum = "TEST4100013";
			Container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Movement.E9_DetentionDays = 9;
			Movement.E9_OA_Depot = depot.MainAddress.PK;
			Movement.E9_JV = voyage.PK;
			Factory.Save();
			IAutoRatingDescriptionMacroExpander expander = Adapter;
			AssertEquals("Vessel", "MAJAPAHIT", expander.ExpandMacro("vessel"));
			AssertEquals("Voyage", "081", expander.ExpandMacro("voyage"));
			AssertEquals("Origin", "AUMEL", expander.ExpandMacro("origin"));
			AssertEquals("Destination", "AUSYD", expander.ExpandMacro("destination"));
			AssertEquals("DetentionPort", "AUBNE", expander.ExpandMacro("detentionport"));
			AssertEquals("Container Number", "TEST4100013", expander.ExpandMacro("containernum"));
			AssertEquals("Container Type", "20GP", expander.ExpandMacro("containertype"));
			AssertEquals("Detention Days", "9", expander.ExpandMacro("detentiondays"));
			AssertEquals("BillNum", "ABILL", expander.ExpandMacro("billnum"));
			AssertEquals("Invalid", null, expander.ExpandMacro("xxx"));
			var shipment2 = Factory.New<BillOfLading>();
			shipment2.JS_JX = voyage.Sailings[0].PK;
			shipment2.JS_HouseBill = "ANOTHERBILL";
			var container2 = shipment2.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100013";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			AssertEquals("BillNum", "ABILL, ANOTHERBILL", expander.ExpandMacro("billnum"));
		}

		public void TestChargeCodeGroups()
		{
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertContainsExactElementsInAnyOrder("Charge code groups for exports.", new[] { ChargeCodeGroupList.Codes.Origin }, ((IAutoRating)Adapter).ChargeCodeGroups);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertContainsExactElementsInAnyOrder("Charge code groups for imports.", new[] { ChargeCodeGroupList.Codes.Destination }, ((IAutoRating)Adapter).ChargeCodeGroups);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.DepotGateIn;
			AssertContainsExactElementsInAnyOrder("Charge code group for 'others'.", System.Array.Empty<string>(), ((IAutoRating)Adapter).ChargeCodeGroups);
		}

		public void TestConsumerType()
		{
			AssertEquals(JobInvoicingConsumerTypes.AgencyDetentionInvoice, ((IAutoRating)Adapter).ConsumerType);
		}

		public void TestRateTypeToUse()
		{
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertEquals("Exports", RateType.ShippingExportDetention, ((IAutoRating)Adapter).RateTypeToUse);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertEquals("Imports", RateType.ShippingImportDetention, ((IAutoRating)Adapter).RateTypeToUse);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.DepotGateIn;
			AssertEquals("Others", (RateType)0, ((IAutoRating)Adapter).RateTypeToUse);
		}

		public void TestDestination()
		{
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_RL_NKClosestPort = "NLAMS";
			var address = depot.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "";
			address.OA_Address1 = "aoeu";
			Movement.E9_OA_Depot = address.PK;
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertEquals("NLAMS", Adapter.Destination.Code);
			depot.MainAddress.OA_RL_NKRelatedPortCode = "AUCNS";
			AssertEquals("AUCNS", Adapter.Destination.Code);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertNull(Adapter.Destination);
		}

		public void TestOrigin()
		{
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_RL_NKClosestPort = "NLAMS";
			var address = depot.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "";
			address.OA_Address1 = "aoeu";
			Movement.E9_OA_Depot = address.PK;
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertEquals("NLAMS", Adapter.Origin.Code);
			depot.MainAddress.OA_RL_NKRelatedPortCode = "AUCNS";
			AssertEquals("AUCNS", Adapter.Origin.Code);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertNull(Adapter.Origin);
		}

		public void TestCarrier()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			Detention.NC_OH_Principal = principal.PK;
			AssertEquals(principal, Adapter.Carrier);
		}

		public void TestConsignor()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			Detention.NC_OH_Client = client.PK;
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertEquals(client, Adapter.Consignor);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertEquals(null, Adapter.Consignor);
		}

		public void TestConsignee()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			Detention.NC_OH_Client = client.PK;
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertEquals(null, Adapter.Consignee);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertEquals(client, Adapter.Consignee);
		}

		public void TestFreightMode()
		{
			AssertEquals(FreightMode.SEA | FreightMode.Containerised, ((IAutoRating)Adapter).FreightMode);
		}

		public void TestAdapterTypeAndID()
		{
			AssertEquals(AdapterType.ContainerMovement, ((IAutoRating)Adapter).AdapterType);
			AssertEquals($"{Movement.Stock.R6_ContainerNum}/{Movement.E9_MovementType}/{Movement.E9_MovementDate.ToShortDateString()}", ((IAutoRating)Adapter).OperationalJobCode);
			AssertEquals(Movement.E9_MovementType, ((IAutoRating)Adapter).JobID);
		}

		public void TestIsImport()
		{
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertEquals("Export", false, Adapter.IsImport());
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertEquals("Import", true, Adapter.IsImport());
		}

		public void TestMeasures()
		{
			Movement.E9_DetentionDays = 10;
			var rateableMeasures = (RateableMeasureSet)Adapter.RateableMeasures;
			AssertEquals("should have one container with correct type", container.JC_RC, rateableMeasures.GetContainerTypePKs().Single());
			AssertEquals("should have a proper number of detention days", Movement.E9_DetentionDays, rateableMeasures.Time.Span.Days);
		}

		public void TestJobServices()
		{
			var jobServices = Adapter.JobServices;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST"; //, 
			var timeInfo = jobServices.Time(chargeCode);
			AssertEquals("should not have TimeInfo for an incorrect charge code", null, timeInfo);
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.ContainerDetention;
			timeInfo = jobServices.Time(chargeCode);
			AssertEquals("should have a proper number of detention days", Movement.E9_DetentionDays, timeInfo.Span.Days);
		}

		public void TestDebtorsOrgs()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.JS_RL_NKOrigin = "AUMEL";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPK = consignor.PK;
			Detention.NC_OH_Client = localClient.PK;
			Detention.NC_OH_Principal = principal.PK;
			var job = new JobHeader.Loader(detention).TryCreateWithMutex();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Factory.Save();
			AssertEquals(3, ((IAutoRating)Adapter).DebtorOrgs.Count);
			AssertEquals(localClient, ((IAutoRating)Adapter).DebtorOrgs[RatingDebtorOrgTypes.LC]);
			AssertEquals(consignee, ((IAutoRating)Adapter).DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertEquals(consignor, ((IAutoRating)Adapter).DebtorOrgs[RatingDebtorOrgTypes.CNR]);
		}

		#region Implementation
		RefContainerStock Stock
		{
			get
			{
				SetupDetentionAndRelatedObjectsIfNeeded();
				return stock;
			}
		}

		BillOfLading Shipment
		{
			get
			{
				SetupDetentionAndRelatedObjectsIfNeeded();
				return shipment;
			}
		}

		BillOfLadingContainer Container
		{
			get
			{
				SetupDetentionAndRelatedObjectsIfNeeded();
				return container;
			}
		}

		ContainerMovement Movement
		{
			get
			{
				SetupDetentionAndRelatedObjectsIfNeeded();
				return movement;
			}
		}

		ContainerDetention Detention
		{
			get
			{
				SetupDetentionAndRelatedObjectsIfNeeded();
				return detention;
			}
		}

		ContainerDetentionMovementRatingAdapter Adapter => adapter ?? (adapter = new ContainerDetentionMovementRatingAdapter(Movement));
		void SetupDetentionAndRelatedObjectsIfNeeded()
		{
			if (detention == null)
			{
				stock = Factory.New<RefContainerStock>();
				stock.R6_ContainerNum = "TEST4100013";
				stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				var exportChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				exportChargeCode.AC_Code = "BLATICUS";
				var principal = Factory.NewWithValidTestData<OrgHeader>();
				var client = Factory.NewWithValidTestData<OrgHeader>();
				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.GenerateSailings();
				shipment = Factory.New<BillOfLading>();
				shipment.JS_JX = voyage.Sailings[0].PK;
				container = shipment.RealContainers.AddNew();
				container.JC_ContainerNum = stock.R6_ContainerNum;
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				movement = stock.Movements.AddNew();
				movement.E9_JV = voyage.PK;
				movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement.E9_MovementDate = ZDateTime.Now.AddDays(-1);
				movement.E9_DetentionDays = 5;
				detention = Factory.New<ContainerDetention>();
				detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
				detention.NC_GC = GlbCompany.CurrentCompany.PK;
				detention.NC_OH_Client = client.PK;
				detention.NC_OH_Principal = principal.PK;
				detention.Movements.Add(movement);
				Factory.Save();
			}
		}

		RefContainerStock stock;
		BillOfLading shipment;
		BillOfLadingContainer container;
		ContainerMovement movement;
		ContainerDetention detention;
		ContainerDetentionMovementRatingAdapter adapter;
		#endregion
	}
}
