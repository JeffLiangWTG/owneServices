using System;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(JobContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>))]
	public class JobContainerXmlDataAdapterTest : ValueObjectDataAdapterTest<CommonContainer, Xsd.Container>
	{
		public void TestImportContainerWeight()
		{
			Xsd.Container container1 = new Xsd.Container()
			{
				ContainerNumber = "TEST4100001",
				ContainerType = new Xsd.ContainerType() { ContainerCode = "20GP", ISOCode = "22G0" },
				GrossWeight = new Xsd.DimensionValue() { DimensionType = Constants.Weight.Tonnes, Value = 30 },
				NetWeight = new Xsd.DimensionValue() { DimensionType = Constants.Weight.Kilograms, Value = 27000 },
				Weight = 4000,
			};

			Xsd.Container container2 = new Xsd.Container()
			{
				ContainerNumber = "TEST4100002",
				ContainerType = new Xsd.ContainerType() { ContainerCode = "20GP", ISOCode = "22G0" },
				GrossWeight = new Xsd.DimensionValue() { DimensionType = Constants.Weight.Tonnes, Value = 30 },
				Weight = 4000,
			};

			Xsd.Container container3 = new Xsd.Container()
			{
				ContainerNumber = "TEST4100003",
				ContainerType = new Xsd.ContainerType() { ContainerCode = "20GP", ISOCode = "22G0" },
				NetWeight = new Xsd.DimensionValue() { DimensionType = Constants.Weight.Kilograms, Value = 27000 },
				Weight = 4000,
			};

			Xsd.Container container4 = new Xsd.Container()
			{
				ContainerNumber = "TEST4100004",
				ContainerType = new Xsd.ContainerType() { ContainerCode = "20GP", ISOCode = "22G0" },
				Weight = 4000,
			};

			JobContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter = new JobContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			CommonContainer[] containers =
			{
				adapter.CreateOrUpdateFromValueObject(container1, context),
				adapter.CreateOrUpdateFromValueObject(container2, context),
				adapter.CreateOrUpdateFromValueObject(container3, context),
				adapter.CreateOrUpdateFromValueObject(container4, context),
			};

			StringBuilder builder = new StringBuilder();

			foreach (CommonContainer container in containers)
			{
				builder.AppendLine();

				builder.Append("Container Number : ");
				builder.AppendLine(container.JC_ContainerNum);
				builder.Append("Unit of Weight   : ");
				builder.AppendLine(container.JC_GrossWeightUQ);
				builder.Append("Gross Weight     : ");
				builder.AppendLine(container.JC_GrossWeight.ToString("0.000"));
				builder.Append("Net Weight       : ");
				builder.AppendLine(container.JC_Calc_NetWeight.ToString("0.000"));
				builder.Append("Tare Weight      : ");
				builder.AppendLine(container.JC_TareWeight.ToString("0.000"));
			}

			const string expected = @"
Container Number : TEST4100001
Unit of Weight   : T
Gross Weight     : 30.000
Net Weight       : 27.000
Tare Weight      : 3.000

Container Number : TEST4100002
Unit of Weight   : T
Gross Weight     : 30.000
Net Weight       : 27.720
Tare Weight      : 2.280

Container Number : TEST4100003
Unit of Weight   : KG
Gross Weight     : 29280.000
Net Weight       : 27000.000
Tare Weight      : 2280.000

Container Number : TEST4100004
Unit of Weight   : KG
Gross Weight     : 4000.000
Net Weight       : 1720.000
Tare Weight      : 2280.000
";

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestImportWeights_OutOfRange()
		{
			var outOfSqlRangeDecimal = 9876543210.1M;
			var netWeight = 1000M;
			var expectedWeight = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").RC_TareWeight;

			var containerXsd = new Xsd.Container()
			{
				ContainerNumber = "TEST4100001",
				ContainerType = new Xsd.ContainerType() { ContainerCode = "20GP", ISOCode = "22G0" },
				GrossWeight = new Xsd.DimensionValue() { Value = outOfSqlRangeDecimal },
				NetWeight = new Xsd.DimensionValue() { Value = netWeight },
			};

			var adapter = new JobContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>();
			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);

			var container = adapter.CreateOrUpdateFromValueObject(containerXsd, context);

			AssertEquals("JC_GrossWeight", expectedWeight, container.JC_GrossWeight);
			AssertEquals("JC_TareWeight", expectedWeight, container.JC_TareWeight);
			AssertEquals("JC_Calc_NetWeight", 0M, container.JC_Calc_NetWeight);

			var grossWeight = 2000M;
			containerXsd.GrossWeight.Value = grossWeight;

			container = adapter.CreateOrUpdateFromValueObject(containerXsd, context);

			AssertEquals("JC_GrossWeight", grossWeight, container.JC_GrossWeight);
			AssertEquals("JC_TareWeight", grossWeight - netWeight, container.JC_TareWeight);
			AssertEquals("JC_Calc_NetWeight", netWeight, container.JC_Calc_NetWeight);
		}

		#region test correct registry defaults used for testing

		protected SystemDataRegistry SystemRegistry
		{
			get
			{
				return SystemDataRegistry.Instance;
			}
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			notify = new NotificationBuffer();
			testDate = new ZDateTime(2007, 9, 28);
		}

		protected NotificationBuffer notify;
		protected ZDateTime testDate;

		#endregion

		#region Overrides for base test

		protected override ValueObjectDataAdapter<CommonContainer, Xsd.Container> GetNewBizObjXmlDataAdapter()
		{
			return new JobContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get
			{
				return "Containers";
			}
		}

		protected override string ExpectedRootElementName
		{
			get
			{
				return "Container";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected string BaseTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Freight\Shared\Freight.DataTransfer\Freight\Testing\";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptyJobContainer.xml", "EmptyJobContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(Factory.New<CommonContainer>(), expectedOutputFilename, ValidationKind.None, "Empty container");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetFullyPopulatedBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			CommonContainer freightContainer = Factory.New<CommonContainer>();

			freightContainer.JC_ContainerNum = "containernum";
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20PL");
			refContainer.RC_USContainerCode = "U1";
			refContainer.SetCountrySpecificContainerCode("U2", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			freightContainer.JC_RC = refContainer.PK;
			freightContainer.JC_SealNum = "SealNum";
			freightContainer.JC_AdditionalSealNum = "Seal2";
			freightContainer.JC_Additional2SealNum = "Seal3";
			freightContainer.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			freightContainer.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
			freightContainer.JC_ArrivalEstimatedDelivery = new ZDateTime(2005, 1, 2);
			freightContainer.JC_LCLAvailable = new ZDateTime(2005, 11, 23, 16, 27, 54);
			freightContainer.JC_FCLAvailable = new ZDateTime(2005, 11, 24, 10, 20, 44);
			freightContainer.JC_ReleaseNum = "BookingRef";
			freightContainer.JC_RH_NKContainerCommodityCode = "ABCD";
			freightContainer.JC_SetPointTemp = 36.6M;
			freightContainer.JC_SetPointTempUnit = "C";
			freightContainer.JC_HumidityPercent = 20;
			freightContainer.JC_AirVentFlow = 10;
			freightContainer.JC_AirVentFlowRateUnit = "2L";
			freightContainer.JC_IsShipperOwned = true;

			#region Export Process
			freightContainer.JC_EmptyRequired = testDate.AddDays(20);
			freightContainer.JC_DepartureEstimatedPickup = testDate.AddDays(21);
			freightContainer.JC_DepartureCartageAdvised = testDate.AddDays(22);
			freightContainer.JC_DepartureSlotReference = "export slot ref";
			freightContainer.JC_DepartureSlotDateTime = testDate.AddDays(23);
			freightContainer.JC_DepartureCartageRef = "export cart ref";
			freightContainer.JC_ContainerYardEmptyPickupGateOut = testDate.AddDays(24);
			freightContainer.JC_FCLWharfGateIn = testDate.AddDays(25);
			freightContainer.JC_DepartureCartageComplete = testDate.AddDays(26);
			freightContainer.JC_FCLOnBoardVessel = testDate.AddDays(27);
			freightContainer.DepartureTruckWaitTime = testDate.AddDays(28);

			OrgHeader pickupFromOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupFromOrg.OH_FullName = "pickup from org";
			OrgAddress pickupFromOrgAddress = pickupFromOrg.Addresses.AddNew(OrgAddressType.Office, true);
			pickupFromOrgAddress.OA_Address1 = "pickup from addr 1";
			freightContainer.JC_OA_DepartureContainerYardAddress = pickupFromOrgAddress.PK;
			#endregion

			#region Import Process
			freightContainer.JC_ContainerImportDORelease = "import rel no";
			freightContainer.JC_FCLAvailable = testDate.AddDays(1);
			freightContainer.JC_ArrivalCTOStorageStartDate = testDate.AddDays(2);
			freightContainer.JC_LCLAvailable = testDate.AddDays(3);
			freightContainer.JC_LCLStorageCommences = testDate.AddDays(4);
			freightContainer.JC_FCLUnloadFromVessel = testDate.AddDays(5);
			freightContainer.JC_ArrivalSlotDateTime = testDate.AddDays(6);
			freightContainer.JC_ArrivalSlotReference = "import slot ref";
			freightContainer.JC_ArrivalCartageRef = "import cart ref";
			freightContainer.JC_FCLWharfGateOut = testDate.AddDays(7);
			freightContainer.JC_ArrivalEstimatedDelivery = testDate.AddDays(8);
			freightContainer.JC_ArrivalCartageAdvised = testDate.AddDays(9);
			freightContainer.JC_ArrivalCartageComplete = testDate.AddDays(10);
			freightContainer.JC_EmptyReadyForReturn = testDate.AddDays(11);
			freightContainer.JC_EmptyReturnedBy = testDate.AddDays(12);
			freightContainer.JC_ContainerYardEmptyReturnGateIn = testDate.AddDays(13);

			freightContainer.ArrivalTruckWaitTime = testDate.AddDays(14);
			freightContainer.ArrivalTruckWaitCost = 14M;

			OrgHeader deliverEmptyToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverEmptyToOrg.OH_FullName = "deliver empty to org";
			OrgAddress deliverEmptyToOrgAddress = deliverEmptyToOrg.Addresses.AddNew(OrgAddressType.Office, true);
			deliverEmptyToOrgAddress.OA_Address1 = "deliver empty to add 1";
			freightContainer.JC_OA_ArrivalContainerYardAddress = deliverEmptyToOrgAddress.PK;
			#endregion

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedJobContainer.xml", "PopulatedJobContainer.xml");
			return new BusinessObjectAndExpectedOutputFileName(freightContainer, expectedOutputFilename, ValidationKind.Xsd, "Populated container");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"BookingReference",	// deprecated
					"ImportProcess/DeliverEmptyTo/Organisation",
					"ExportProcess/PickupEmptyFrom/Organisation",
					"ExportProcess/BookingReference",	// deprecated
					"Custom",
					"AirVentFlow",
					"AirVentFlowRateUnit",

					// Covered in ConsolContainerValueObjectDataAdapter
					"IsArrivingAtCTOByRail",
					"IsEmptyContainer",
					"IsDamaged",
					"ImportProcess/DetentionCharge",
					"ImportProcess/DetentionDays",
					"ImportProcess/StorageCharge",
					"ImportProcess/StorageDays",
					"ImportProcess/PickupByRail",
					"ImportProcess/HeldForFCLTransitStaging",
					"ImportProcess/DemurrageCharge",
					"ExportProcess/IsArrivingAtCTOByRail",
					"ExportProcess/DemurrageCharge",
				};
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		#endregion

	}
}
