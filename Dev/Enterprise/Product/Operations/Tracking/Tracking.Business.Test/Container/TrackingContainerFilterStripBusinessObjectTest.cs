using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingContainerFilterStripBusinessObject))]
	public class TrackingContainerFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestContainerNumber_DoesNotExceedMaxLengths()
		{
			var filter = (ModuleTextFilter)TestFilterStrip["Container Number"];
			var expectedContainerNum = new string('1', JobContainerSchema.JC_ContainerNum.MaxLength);
			var expectedSubFilter1 = $"JC_ContainerNum = '{expectedContainerNum}'";

			filter.IsActive = true;
			filter.Property = expectedContainerNum + "1";

			AssertContains(expectedSubFilter1, TestFilterStrip.Filter.LiteralTextADO);
		}

		#region Test Number Filters

		public void TestNumberFilters()
		{
			string consolNumber = "C00001234";
			string containerNumber = "C00001234";
			string masterBillNumber = "80234240";
			string shipmentNumber = "S00001234";
			string portTransportRef = "123";
			string declarationNumber = "B00001000";

			TestConsol.JK_UniqueConsignRef = consolNumber;
			TestContainer.JC_ContainerNum = containerNumber;
			TestContainer.JC_DepartureCartageRef = portTransportRef;
			TestConsol.JK_MasterBillNum = masterBillNumber;
			TestShipment.JS_UniqueConsignRef = shipmentNumber;
			TestDeclaration.JE_DeclarationReference = declarationNumber;

			TestShipment.ConsigneePK = TestOrg.PK;
			TestOrder.BuyerPK = TestOrg2.PK;
			Factory.Save();

			AssertContainerIsFound("Precondition: no filters finds the container", true);
			AssertNumberFiltering("Consol Number", consolNumber);
			AssertNumberFiltering("Container Number", containerNumber);
			AssertNumberFiltering("Port Transport Ref", portTransportRef);
			AssertNumberFiltering("Master Bill Number", masterBillNumber);
			AssertNumberFiltering("Shipment Number", shipmentNumber);

			TestContainer.JC_JK = ZGuid.Empty;
			TestCusContainer.CO_JC = TestContainer.PK;
			Factory.Save();

			AssertNumberFiltering("Shipment Number", declarationNumber);
		}

		void AssertNumberFiltering(ZString filterName, ZString value)
		{
			ModuleTextFilter numberFilter = (ModuleTextFilter)TestFilterStrip[filterName];
			try
			{
				numberFilter.IsActive = true;

				numberFilter.Property = ZString.Empty;
				AssertContainerIsFound("Should find container when " + filterName + " is [" + ZString.Empty + "]", true);

				numberFilter.Property = value;
				AssertContainerIsFound("Should find container when " + filterName + " is [" + value + "]", true);

				ZString somethingElse = new ZString("SomethingElse");
				numberFilter.Property = somethingElse;
				AssertContainerIsFound("Should not find container when " + filterName + " is [" + somethingElse + "]", false);

				numberFilter.Property = ZString.Empty;
				AssertContainerIsFound("Should find container when " + filterName + " is [" + ZString.Empty + "]", true);
			}
			finally
			{
				numberFilter.IsActive = false;
			}
		}

		#endregion

		#region Test Date Filters

		public void TestDateFilters()
		{
			Factory.Save();
			AssertContainerIsFound("Precondition: Searching with no filters should find the container", true);

			ZDateTime baseDate = new ZDateTime(2007, 01, 01);
			ZDateTime actualDehire = baseDate.AddDays(1);
			ZDateTime actualDelivery = baseDate.AddDays(2);
			ZDateTime available = baseDate.AddDays(3);
			ZDateTime confirmedDelivery = baseDate.AddDays(4);
			ZDateTime emptyReady = baseDate.AddDays(5);
			ZDateTime eTA = baseDate.AddDays(6);
			ZDateTime emptyReturnRequired = baseDate.AddDays(7);
			ZDateTime emptyPickup = baseDate.AddDays(7);
			ZDateTime requiredDelivery = baseDate.AddDays(9);
			ZDateTime slotDate = baseDate.AddDays(10);

			TestContainer.ActualDehire = actualDehire;
			TestContainer.ActualDelivery = actualDelivery;
			TestContainer.JC_FCLAvailable = available;
			TestContainer.ConfirmedDelivery = confirmedDelivery;
			TestContainer.EmptyReady = emptyReady;
			TestConsol.Transports.AddNew();
			TestConsol.Transports.MostInterestingTransport.JW_ETA = eTA;
			TestContainer.JC_EmptyReturnedBy = emptyReturnRequired;
			TestContainer.EmptyPickup = emptyPickup;
			TestContainer.RequiredDelivery = requiredDelivery;
			TestContainer.JC_ArrivalSlotDateTime = slotDate;

			Factory.Save();
			AssertContainerIsFound("Precondition: Searching with no filters should find the container", true);

			AssertDateFiltering("Actual De-hire", TrackingContainer.Schema.ActualDehire, actualDehire);
			AssertDateFiltering("Actual Delivery", TrackingContainer.Schema.ActualDelivery, actualDelivery);
			AssertDateFiltering("Available", TrackingContainer.Schema.Available, available);
			AssertDateFiltering("Confirmed Delivery", TrackingContainer.Schema.ConfirmedDelivery, confirmedDelivery);
			AssertDateFiltering("Empty Ready", TrackingContainer.Schema.EmptyReady, emptyReady);
			AssertDateFiltering("ETA", TrackingContainer.Schema.ETA, eTA);
			AssertDateFiltering("Empty Return Required", TrackingContainer.Schema.EmptyReturnRequired, emptyReturnRequired);
			AssertDateFiltering("Actual Empty Pickup", TrackingContainer.Schema.EmptyPickup, emptyPickup);
			AssertDateFiltering("Required Delivery", TrackingContainer.Schema.RequiredDelivery, requiredDelivery);
			AssertDateFiltering("Slot Date", TrackingContainer.Schema.SlotDate, slotDate);
		}

		void AssertDateFiltering(ZString filterDesc, ZString propertyName, ZDateTime dateTime)
		{
			ModuleDateFilter dateFilter = (ModuleDateFilter)TestFilterStrip[filterDesc];
			try
			{
				dateFilter.IsActive = true;
				dateFilter.PropertySearch = "Date range";

				string preconditionMessage = "Precondition: Container's Date (" + filterDesc + ") should be set correctly ";
				AssertEquals(preconditionMessage, dateTime, (ZDateTime)TestContainer[propertyName]);

				dateFilter.Property1 = ZDateTime.Empty;
				dateFilter.Property2 = ZDateTime.Empty;
				AssertContainerIsFound("Precondition: Container should be found with no filtering", true);

				dateFilter.Property1 = dateTime.AddDays(-3);
				dateFilter.Property2 = dateTime.AddDays(3);
				AssertContainerIsFound("Date: " + filterDesc + " (Correct 'From' Date Specified)", true);

				dateFilter.Property1 = dateTime.AddDays(-14);
				dateFilter.Property2 = dateTime.AddDays(-7);
				AssertContainerIsFound("Date: " + filterDesc + " (Incorrect 'From' & To Date Specified)", false);

				dateFilter.Property1 = ZDateTime.Empty;
				dateFilter.Property2 = ZDateTime.Empty;
			}
			finally
			{
				dateFilter.IsActive = false;
			}
		}

		public void TestETAFilterLoadsContainersFromStandAloneDeclarations()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();

			BaseCusContainer cusContainer = Factory.NewWithValidTestData<BaseCusContainer>();
			cusContainer.CO_JC = container.PK;

			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			cusContainer.CO_JE = declaration.PK;

			ZDateTime testDate = ZDateTime.Today;

			declaration.JE_DateOfArrival = testDate;
			Factory.Save();

			Assert(declaration.IsStandAlone);
			Assert(declaration.IsInDatabase);
			Assert(cusContainer.IsInDatabase);
			Assert(container.IsInDatabase);

			AssertEquals(container.CustomsContainer.PK, cusContainer.PK);
			AssertEquals(container.CustomsContainer.Declaration.PK, declaration.PK);

			ModuleDateFilter dateFilter = (ModuleDateFilter)TestFilterStrip["ETA"];
			try
			{
				dateFilter.IsActive = true;
				dateFilter.PropertySearch = "Date range";

				dateFilter.Property1 = ZDateTime.Empty;
				dateFilter.Property2 = ZDateTime.Empty;
				AssertContainerIsFound("Precondition: Container should be found with no filtering", true);

				dateFilter.Property1 = testDate.AddDays(-3);
				dateFilter.Property2 = testDate.AddDays(3);
				AssertContainerIsFound("Date: " + "ETA" + " (Correct 'From' Date Specified)", true);

				dateFilter.Property1 = testDate.AddDays(-14);
				dateFilter.Property2 = testDate.AddDays(-7);
				AssertContainerIsFound("Date: " + "ETA" + " (Incorrect 'From' & To Date Specified)", false);

				dateFilter.Property1 = ZDateTime.Empty;
				dateFilter.Property2 = ZDateTime.Empty;
			}
			finally
			{
				dateFilter.IsActive = false;
			}
		}

		#endregion

		#region Test Container Status

		public void TestContainerStatus()
		{
			var testContainer1 = Factory.NewWithValidTestData<TrackingContainer>();
			var testContainer2 = Factory.NewWithValidTestData<TrackingContainer>();
			var testContainer3 = Factory.NewWithValidTestData<TrackingContainer>();

			testContainer1.JC_ContainerStatus = "";
			testContainer2.JC_ContainerStatus = "AVL";
			testContainer3.JC_ContainerStatus = "APP";

			Factory.Save();

			ModuleTextFilter modeFilter = (ModuleTextFilter)TestFilterStrip["Container Status"];
			AssertNotNull(modeFilter);

			modeFilter.IsActive = true;
			modeFilter.Property = "";

			var results = Factory.Load<TrackingContainer>(TestFilterStrip.Filter);
			AssertCollectionContains(testContainer1, results);
			AssertCollectionContains(testContainer2, results);
			AssertCollectionContains(testContainer3, results);

			modeFilter.Property = "AVL";
			results = Factory.Load<TrackingContainer>(TestFilterStrip.Filter);
			AssertCollectionNotContains(testContainer1, results);
			AssertCollectionContains(testContainer2, results);
			AssertCollectionNotContains(testContainer3, results);

			modeFilter.Property = "APP";
			results = Factory.Load<TrackingContainer>(TestFilterStrip.Filter);
			AssertCollectionNotContains(testContainer1, results);
			AssertCollectionNotContains(testContainer2, results);
			AssertCollectionContains(testContainer3, results);

			modeFilter.Property = "WAS";
			results = Factory.Load<TrackingContainer>(TestFilterStrip.Filter);
			AssertCollectionNotContains(testContainer1, results);
			AssertCollectionNotContains(testContainer2, results);
			AssertCollectionNotContains(testContainer3, results);
		}

		#endregion

		#region Test Mode Filter

		public void TestModeFilter()
		{
			ModuleTextFilter modeFilter = (ModuleTextFilter)TestFilterStrip["Mode"];

			TestContainer.JC_ContainerMode = "ABC";
			TestShipment.ConsigneePK = TestOrg.PK;
			Factory.Save();

			try
			{
				modeFilter.IsActive = true;
				modeFilter.Property = "";
				AssertContainerIsFound("Container should be found with no filtering", true);

				modeFilter.Property = "ABC";
				AssertContainerIsFound("Container should be found when filtering with correct mode", true);

				modeFilter.Property = "XYZ";
				AssertContainerIsFound("Container should not be found when filtering with some other mode", false);

				modeFilter.Property = "";
			}
			finally
			{
				modeFilter.IsActive = false;
			}
		}

		#endregion Test Mode Filter

		#region Test Port Filters

		public void TestPortFilters()
		{
			TestConsol.JK_RL_NKLoadPort = "NZAKL";
			TestConsol.JK_RL_NKDischargePort = "AUSYD";
			TestShipment.ConsigneePK = TestOrg.PK;
			Factory.Save();

			AssertContainerIsFound("Precondition: Searching with no filters should find the container", true);
			AssertLocationFiltering("", "", true);

			AssertLocationFiltering("NZAKL", "", true);
			AssertLocationFiltering("", "AUSYD", true);
			AssertLocationFiltering("NZAKL", "AUSYD", true);

			AssertLocationFiltering("AUSYD", "", false);
			AssertLocationFiltering("", "NZAKL", false);
			AssertLocationFiltering("AUSYD", "NZAKL", false);

			AssertLocationFiltering("NZ", "", true);
			AssertLocationFiltering("", "AU", true);
			AssertLocationFiltering("NZ", "AU", true);

			AssertLocationFiltering("AU", "", false);
			AssertLocationFiltering("", "NZ", false);
			AssertLocationFiltering("AU", "NZ", false);
		}

		public void TestPortFilters_Declaration()
		{
			TestDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			TestDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			TestShipment.ConsigneePK = TestOrg.PK;
			TestContainer.JC_JK = ZGuid.Empty;
			TestCusContainer.CO_JC = TestContainer.PK;
			TestCusContainer.CO_JE = TestDeclaration.PK;
			Factory.Save();

			AssertContainerIsFound("Precondition: Searching with no filters should find the container", true);
			AssertLocationFiltering("", "", true);

			AssertLocationFiltering("NZAKL", "", true);
			AssertLocationFiltering("", "AUSYD", true);
			AssertLocationFiltering("NZAKL", "AUSYD", true);

			AssertLocationFiltering("AUSYD", "", false);
			AssertLocationFiltering("", "NZAKL", false);
			AssertLocationFiltering("AUSYD", "NZAKL", false);

			AssertLocationFiltering("NZ", "", true);
			AssertLocationFiltering("", "AU", true);
			AssertLocationFiltering("NZ", "AU", true);

			AssertLocationFiltering("AU", "", false);
			AssertLocationFiltering("", "NZ", false);
			AssertLocationFiltering("AU", "NZ", false);
		}

		void AssertLocationFiltering(ZString loadPort, ZString dischargePort, bool expected)
		{
			var loadDischargeFilter = ((ModuleLocationFilter)TestFilterStrip["Load / Discharge"]);

			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = ZString.Empty;

			try
			{
				loadDischargeFilter.IsActive = true;

				if (loadPort.IsValid && !loadPort.IsEmpty)
				{
					loadDischargeFilter.Property1 = loadPort;
				}
				if (dischargePort.IsValid && !dischargePort.IsEmpty)
				{
					loadDischargeFilter.Property2 = dischargePort;
				}

				AssertContainerIsFound(ZString.Format((expected ? "Container should be found when load port is {0} and discharge port is {1}" : "Container should not be found when load port is {0} and discharge port is {1}"), loadPort, dischargePort), expected);
			}
			finally
			{
				loadDischargeFilter.IsActive = false;
			}
		}

		#endregion Test Port Filters

		#region Test Consignee Filter

		public void TestConsigneeFilter()
		{
			ModuleGuidFilter consigneeFilter = (ModuleGuidFilter)TestFilterStrip["Consignee"];
			try
			{
				consigneeFilter.IsActive = true;
				TestShipment.ConsigneePK = TestOrg.PK;
				Factory.Save();

				consigneeFilter.Property = ZGuid.Empty;
				AssertContainerIsFound("Container should be found with no filtering", true);

				consigneeFilter.Property = TestOrg.PK;
				AssertContainerIsFound("Filtering by TestOrg (Looking for Shipment Consignee)", true);

				consigneeFilter.Property = ZGuid.NewZGuid();
				AssertContainerIsFound("Filtering by some random ZGuid (Looking for Shipment Consignee)", false);
			}
			finally
			{
				consigneeFilter.IsActive = false;
			}
		}

		#endregion Test Consignee Filter

		#region Test Type Query

		public void TestTypeFilter()
		{
			ModuleTextFilter typeFilter = (ModuleTextFilter)TestFilterStrip["Container Type"];

			RefContainer type1 = Factory.NewWithValidTestData<RefContainer>();
			type1.RC_Code = "ABC";

			RefContainer type2 = Factory.NewWithValidTestData<RefContainer>();
			type2.RC_Code = "XYZ";

			TestContainer.JC_RC = type1.PK;
			TestShipment.ConsigneePK = TestOrg.PK;
			Factory.Save();

			try
			{
				typeFilter.IsActive = true;

				typeFilter.Property = type1.RC_Code;
				AssertContainerIsFound("Container should be found when searching with correct Type", true);

				typeFilter.Property = type2.RC_Code;
				AssertContainerIsFound("Container should not be found when searching with incorrect Type", false);
			}
			finally
			{
				typeFilter.Property = ZString.Empty;
				typeFilter.IsActive = false;
			}
		}

		#endregion Test Type Query

		#region Test Workflow Filters

		public void TestWorkflowFilters()
		{
			AssertNotNull("Filterstrip contains workflow filters", TestFilterStrip["Milestone Date"]);
			AssertNotNull("Filterstrip contains workflow filters", TestFilterStrip["Milestone Completed"]);
			AssertNotNull("Filterstrip contains workflow filters", TestFilterStrip["Next Milestone"]);
			AssertNotNull("Filterstrip contains workflow filters", TestFilterStrip["Last Completed Milestone"]);
		}

		#endregion

		#region Implementation

		void AssertContainerIsFound(string message, bool expected)
		{
			AssertEquals(message, expected, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length > 0);
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateTestData();
			Factory.Save();

			TestFilterStrip = (TrackingContainerFilterStripBusinessObject)GetNewBusinessObject();
			TestFilterStrip.CurrentOrg = TestOrg.PK;
		}

		#endregion

		#region Test Data

		OrgHeader TestOrg;
		OrgHeader TestOrg2;
		TrackingContainerFilterStripBusinessObject TestFilterStrip;
		TrackingConsol TestConsol;
		TrackingContainer TestContainer;
		TrackingOrder TestOrder;
		TrackingShipment TestShipment;
		PackLine TestPackLine;
		JobContainerPackPivot TestPackPivot;
		JobSailing TestSail;
		JobVoyage TestVoyage;
		VoyageOrigin TestVoyageOrigin;
		VoyageDestination TestVoyageDestination;
		Transport TestTransport;
		Customs.US.Business.JobDeclaration TestDeclaration;
		Customs.US.Business.CusContainer TestCusContainer;

		void CreateTestData()
		{
			TestOrg = CreateOrganisation("TESTORG1");
			TestOrg2 = CreateOrganisation("TESTORG2");
			TestConsol = CreateConsol();
			TestContainer = CreateContainer(TestConsol);
			TestShipment = CreateShipment();
			TestOrder = CreateOrder(TestShipment);
			TestPackLine = CreatePackLine(TestShipment);
			TestPackPivot = CreatePackPivot(TestPackLine, TestConsol);
			TestSail = CreateSailing(TestContainer);
			TestVoyage = CreateVoyage(TestSail);
			TestVoyageOrigin = CreateVoyageOrigin(TestSail, TestVoyage);
			TestVoyageDestination = CreateVoyageDestination(TestSail, TestVoyage);
			TestTransport = GetMostInterestingTransportFromConsol(TestConsol);
			TestDeclaration = CreateDeclaration();
			TestCusContainer = CreateCusContainer(TestDeclaration);
		}

		Transport GetMostInterestingTransportFromConsol(TrackingConsol consol)
		{
			Transport transport = consol.Transports.MostInterestingTransport;
			return transport;
		}
		JobVoyage CreateVoyage(JobSailing sailing)
		{
			return sailing.Voyage;
		}
		VoyageOrigin CreateVoyageOrigin(JobSailing sailing, JobVoyage voyage)
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			voyage.Origins.Add(origin);
			return origin;
		}
		VoyageDestination CreateVoyageDestination(JobSailing sailing, JobVoyage voyage)
		{
			VoyageDestination destination = Factory.New<VoyageDestination>();
			voyage.Destinations.Add(destination);
			return destination;
		}
		JobSailing CreateSailing(TrackingContainer container)
		{
			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			TestConsol.Transports[0].JW_JX = sailing.PK;
			TestConsol.Containers.Add(container);
			return sailing;
		}
		PackLine CreatePackLine(TrackingShipment shipment)
		{
			PackLine packline = Factory.New<PackLine>();
			packline.JL_FreightMode = FreightConstants.OuterPackType;
			packline.JL_JS = shipment.PK;
			return packline;
		}
		JobContainerPackPivot CreatePackPivot(PackLine packline, TrackingConsol consol)
		{
			if (packline.Shipment != null && consol != null)
			{
				consol.Shipments.Add(packline.Shipment);
			}
			JobContainerPackPivot packPivot = Factory.New<JobContainerPackPivot>();
			packPivot.J6_JL = packline.PK;
			packPivot.J6_JC = TestContainer.PK;
			return packPivot;
		}

		Customs.US.Business.JobDeclaration CreateDeclaration()
		{
			Customs.US.Business.JobDeclaration declaration = Factory.NewWithValidTestData<Customs.US.Business.JobDeclaration>();
			return declaration;
		}

		Customs.US.Business.CusContainer CreateCusContainer(Customs.US.Business.JobDeclaration declaration)
		{
			Customs.US.Business.CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "N123";
			container.CO_Seal = "S123";
			container.CO_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			return container;
		}

		TrackingOrder CreateOrder(TrackingShipment shipment)
		{
			TrackingOrder order = Factory.NewWithValidTestData<TrackingOrder>();
			order.JD_JS = shipment.PK;
			return order;
		}

		OrgHeader CreateOrganisation(string oH_Code)
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = oH_Code;
			return header;
		}

		TrackingConsol CreateConsol()
		{
			TrackingConsol consol = Factory.NewWithValidTestData<TrackingConsol>();
			return consol;
		}

		TrackingContainer CreateContainer(TrackingConsol consol)
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_JK = consol.PK;
			return container;
		}

		TrackingShipment CreateShipment()
		{
			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			return shipment;
		}

		#endregion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingContainerFilterStripBusinessObject();
		}
	}
}
