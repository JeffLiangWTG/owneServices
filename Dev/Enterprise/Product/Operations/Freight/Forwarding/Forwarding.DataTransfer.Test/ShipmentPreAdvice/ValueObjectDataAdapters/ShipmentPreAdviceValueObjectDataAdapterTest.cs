using System;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentPreAdviceValueObjectDataAdapter))]
	[TestDate(2005, 1, 2)]
	sealed class ShipmentPreAdviceValueObjectDataAdapterTest : ValueObjectDataAdapterTest<JobShipmentPreplanning, Xsd.Consol>
	{
		public void TestDataExportedEventRaised()
		{
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			IValueObjectDataAdapter dataAdapter = GetNewBizObjXmlDataAdapter();
			dataAdapter.ExportToValueObject(preplanning, new ValueObjectExportContext(new NotificationBuffer()));
			AssertNotNull("DataExport event raised after an export occurred", preplanning.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		#region Sample to Export

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var shipment = Factory.New<JobShipmentPreplanning>();
			shipment.PreAdviceTransports.RemoveAndDeleteAll();
			var emptyPreAdvicePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ShipmentPreAdvice.TestFiles.EmptyPreAdvice.xml");
			return new BusinessObjectAndExpectedOutputFileName(shipment, emptyPreAdvicePath, ValidationKind.None, "Empty");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var shipment = Factory.New<JobShipmentPreplanning>();
			shipment.PreAdviceTransports.RemoveAndDeleteAll();
			shipment.Containers.AddNew();
			var order = shipment.Orders.AddNew();
			order.JD_OrderNumber = "OrderNumber";
			var populatedPreAdviceWithEmptyFieldsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ShipmentPreAdvice.TestFiles.PopulatedPreAdviceWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(shipment, populatedPreAdviceWithEmptyFieldsPath, ValidationKind.None, "Populated with empty fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedPreAdvicePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ShipmentPreAdvice.TestFiles.PopulatedPreAdvice.xml");
			return new BusinessObjectAndExpectedOutputFileName(FullyPopulatedPreAdvice, populatedPreAdvicePath, ValidationKind.FactorySave, "Full populated");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			var preAdviceWithAttachedShipmentPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ShipmentPreAdvice.TestFiles.PreAdviceWithAttachedShipment.xml");
			var preAdviceWithContainerCountPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ShipmentPreAdvice.TestFiles.PreAdviceWithContainerCount.xml");
			return new BusinessObjectAndExpectedOutputFileName[]
			{
				new BusinessObjectAndExpectedOutputFileName(PreAdviceWithAttachedShipment, preAdviceWithAttachedShipmentPath, ValidationKind.Xsd, "Pre-advice with attached shipment"),
				new BusinessObjectAndExpectedOutputFileName(PreAdviceWithContainerCount, preAdviceWithContainerCountPath, ValidationKind.Xsd, "Pre-advice with container count"),
			};
		}

		JobShipmentPreplanning FullyPopulatedPreAdvice
		{
			get
			{
				if (fullyPopulatedPreAdvice == null)
				{
					fullyPopulatedPreAdvice = Factory.New<JobShipmentPreplanning>();
					fullyPopulatedPreAdvice.EF_PreshipID = "PreshipID";
					fullyPopulatedPreAdvice.EF_MasterBill = "MasterBill";
					fullyPopulatedPreAdvice.EF_HouseBill = "HouseBill";
					fullyPopulatedPreAdvice.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
					fullyPopulatedPreAdvice.EF_RL_NKPortLoad = "MYPKG";
					fullyPopulatedPreAdvice.EF_RL_NKPortDisch = "AUSYD";
					fullyPopulatedPreAdvice.EF_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
					fullyPopulatedPreAdvice.EF_OH_SendingAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
					fullyPopulatedPreAdvice.EF_OH_ReceivingAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
					fullyPopulatedPreAdvice.PreAdviceTransports.RemoveAndDeleteAll();

					fullyPopulatedPreAdvice.EF_ActualWeight = 2m;
					fullyPopulatedPreAdvice.EF_UnitOfWeight = Constants.Weight.Grams;
					fullyPopulatedPreAdvice.EF_ActualVolume = 3m;
					fullyPopulatedPreAdvice.EF_UnitOfVolume = Constants.Volume.CubicMetres;
					fullyPopulatedPreAdvice.EF_Packs = 4;
					fullyPopulatedPreAdvice.EF_F3_NKPackType = Constants.PkgUnit.Bag;

					OrderContainer containerWithCount = fullyPopulatedPreAdvice.Containers.AddNew();
					containerWithCount.J1_ContainerCount = 2;
					containerWithCount.J1_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
					OrderContainer containerWithNumber = fullyPopulatedPreAdvice.Containers.AddNew();
					containerWithNumber.J1_ContainerNumber = "ContainerNum";
					containerWithNumber.J1_RC = new RefContainer.Loader(Factory).LoadFromCode("40GP").PK;

					Order order = fullyPopulatedPreAdvice.Orders.AddNew();
					order.JD_OrderNumber = "OrderNumber";
				}
				return fullyPopulatedPreAdvice;
			}
		}
		JobShipmentPreplanning fullyPopulatedPreAdvice;

		JobShipmentPreplanning PreAdviceWithAttachedShipment
		{
			get
			{
				if (preAdviceWithAttachedShipment == null)
				{
					var shipment = Factory.New<ForwardingShipment>();
					preAdviceWithAttachedShipment = Factory.New<JobShipmentPreplanning>();
					preAdviceWithAttachedShipment.EF_JS = shipment.PK;
					preAdviceWithAttachedShipment.PreAdviceTransports.RemoveAndDeleteAll();
				}
				return preAdviceWithAttachedShipment;
			}
		}
		JobShipmentPreplanning preAdviceWithAttachedShipment;

		JobShipmentPreplanning PreAdviceWithContainerCount
		{
			get
			{
				if (preAdviceWithContainerCount == null)
				{
					preAdviceWithContainerCount = Factory.New<JobShipmentPreplanning>();
					OrderContainer container = preAdviceWithContainerCount.Containers.AddNew();
					container.J1_ContainerCount = 3;
					container.J1_RC = new RefContainer.Loader(Factory).LoadFromCode("40GP").PK;
					preAdviceWithContainerCount.PreAdviceTransports.RemoveAndDeleteAll();
				}
				return preAdviceWithContainerCount;
			}
		}
		JobShipmentPreplanning preAdviceWithContainerCount;

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Consols"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Consol"; }
		}

		protected override ValueObjectDataAdapter<JobShipmentPreplanning, Xsd.Consol> GetNewBizObjXmlDataAdapter()
		{
			return new ShipmentPreAdviceValueObjectDataAdapter();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
"ConsolIdentifier/ConsolIdentifierType",
"ConsolDetail/PortFirstArrival",
"ConsolDetail/DateCreated",
"ConsolDetail/PortFirstForeign",
"ConsolDetail/ConsolType",
"ConsolDetail/PaymentType",
"ConsolDetail/TransportMode",
"ConsolDetail/Containers",
"ConsolDetail/CoLoadWith/OwnerCode",
"ConsolDetail/CoLoadWith",
"ConsolDetail/Carrier",
"ConsolDetail/PortOfDischarge/ActualDateTime",
"ConsolDetail/PortOfDischarge/EstimatedDateTime",
"ConsolDetail/ContainerMode",
"ConsolDetail/Creditor",
"ConsolDetail/Arrival",
"ConsolDetail/ReceivingAgent",
"ConsolDetail/SendingAgent",
"ConsolDetail/PlannedLegs",
"ConsolDetail/PortOfLoading/ActualDateTime",
"ConsolDetail/PortOfLoading/EstimatedDateTime",
"ConsolDetail/PortLastForeign",
"ConsolDetail/Departure",
"ConsolDetail/AgentReference",
"ConsolDetail/BookingReference",
"ConsolDetail/Item",
"ConsolDetail/ExternalAgentReference",
"ConsolDetail/CustomsEntryNumbers",
"ConsolDetail/NumberOfOriginalBills",
"ConsolDetail/NumberOfCopyBills",
"ConsolDetail/ReleaseType",
"ConsolDetail/MasterBillIssueDate",
"ConsolDetail/CustomValues",
"ConsolDetail/Addresses",
"ConsolDetail/ReferenceNumbers/Type",
"ConsolDetail/ReferenceNumbers/Number",
"ConsolDetail/ReferenceNumbers/Country/Name",
"ConsolDetail/ReferenceNumbers/Country/Value",
"Shipments",
"Events",
"Notes",
"Documents",
"ARInvoices",
"CustomValues",
"AWBHeaders"
				};
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		#endregion
	}
}
