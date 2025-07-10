using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalContainerType = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ContainerLoadListContainerDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestContainerLink()
		{
			var (consol, supplierBooking) = CreateSupplierBookingWithAttachedContainers();

			var linkManager = new ContainerLoadListContainerLinkManager();
			ReadIntoCollection(consol, linkManager, CreateContainerDataObject(1, "DWZI4407574"));

			AssertEquals(consol.Containers[1].PK, linkManager.GetContainer(1).PK);
			AssertEquals(5, consol.Containers.Count);
			AssertEquals(5, supplierBooking.Containers.Count);
		}

		void ReadIntoCollection(ForwardingConsol consol, ContainerLoadListContainerLinkManager linkManager, UniversalContainer containerDataObject)
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, consol.JK_UniqueConsignRef);
			dataObject.SetContainerCollection(() => new DataObjectList<UniversalContainer>() { containerDataObject });

			new ContainerLoadListContainerDataObjectReader(containerDataObject, consol, linkManager, Logger, Factory).ReadIntoBusinessObject();
		}

		public void TestCheck()
		{
			var containerLoadList = Factory.BOFactory.New<CYContainerLoadList>();
			var supplierBooking = Factory.BOFactory.New<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			containerLoadList.CLH_JSB_Booking = supplierBooking.PK;

			var consol = Factory.BOFactory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CON1";
			var linkManager = new ContainerLoadListContainerLinkManager();
			var dataObject = new UniversalContainer();

			AssertLogWarning(consol, linkManager, dataObject, "Container link must be valid value in CON1.");

			dataObject.Link = 0;
			AssertLogWarning(consol, linkManager, dataObject, "Container link must be valid value in CON1.");

			linkManager.CollectContainerLink(consol.Containers.AddNew(), new UniversalContainer { Link = 1 });
			dataObject.Link = 1;
			AssertLogWarning(consol, linkManager, dataObject, "Duplicated container link 1.");

			var container = consol.Containers[0];

			dataObject.Link = 2;
			AssertLogWarning(consol, linkManager, dataObject, "Container should have container number for container link 2.");

			var loadLine = container.ContainerLoadListLines.AddNew();
			loadLine.CLL_CLH_LoadListHeader = containerLoadList.PK;
			AssertLogWarning(consol, linkManager, dataObject, "");
		}

		public void TestRead_Successful_WithOneSeal()
		{
			var consol = CreateConsolWithOneContainer();
			var container = CreateContainerDataObject(1, "DWZI4407574");

			container.Seal = "TESTSEAL1";
			container.SealPartyType = new CodeDescriptionPair { Code = ContainerSealParties.Codes.CarrierShippingLine };

			var linkManager = new ContainerLoadListContainerLinkManager();

			ReadIntoCollection(consol, linkManager, container);

			AssertEquals(container.Seal, consol.Containers[0].JC_SealNum);
			AssertEquals(container.SealPartyType.Code, consol.Containers[0].JC_SealParty);
			AssertNullOrEmpty(consol.Containers[0].JC_AdditionalSealNum);
			AssertNullOrEmpty(consol.Containers[0].JC_AdditionalSealParty);
			AssertNullOrEmpty(consol.Containers[0].JC_Additional2SealNum);
			AssertNullOrEmpty(consol.Containers[0].JC_Additional2SealParty);
		}

		public void TestRead_Successful_WithTwoSeals()
		{
			var consol = CreateConsolWithOneContainer();
			var container = CreateContainerDataObject(1, "DWZI4407574");
			container.Seal = "TESTSEAL1";
			container.SealPartyType = new CodeDescriptionPair { Code = ContainerSealParties.Codes.CarrierShippingLine };
			container.SecondSeal = "TESTSEAL2";
			container.SecondSealPartyType = new CodeDescriptionPair { Code = ContainerSealParties.Codes.Customs };

			var linkManager = new ContainerLoadListContainerLinkManager();

			ReadIntoCollection(consol, linkManager, container);

			AssertEquals(container.Seal, consol.Containers[0].JC_SealNum);
			AssertEquals(container.SealPartyType.Code, consol.Containers[0].JC_SealParty);
			AssertEquals(container.SecondSeal, consol.Containers[0].JC_AdditionalSealNum);
			AssertEquals(container.SecondSealPartyType.Code, consol.Containers[0].JC_AdditionalSealParty);
			AssertNullOrEmpty(consol.Containers[0].JC_Additional2SealNum);
			AssertNullOrEmpty(consol.Containers[0].JC_Additional2SealParty);
		}

		public void TestRead_Successful_WithThreeSeals()
		{
			var consol = CreateConsolWithOneContainer();
			var container = CreateContainerDataObject(1, "DWZI4407574");
			container.Seal = "TESTSEAL1";
			container.SealPartyType = new CodeDescriptionPair { Code = ContainerSealParties.Codes.CarrierShippingLine };
			container.SecondSeal = "TESTSEAL2";
			container.SecondSealPartyType = new CodeDescriptionPair { Code = ContainerSealParties.Codes.Customs };
			container.ThirdSeal = "TESTSEAL3";
			container.ThirdSealPartyType = new CodeDescriptionPair { Code = ContainerSealParties.Codes.Quarantine };

			var linkManager = new ContainerLoadListContainerLinkManager();

			ReadIntoCollection(consol, linkManager, container);

			AssertEquals(container.Seal, consol.Containers[0].JC_SealNum);
			AssertEquals(container.SealPartyType.Code, consol.Containers[0].JC_SealParty);
			AssertEquals(container.SecondSeal, consol.Containers[0].JC_AdditionalSealNum);
			AssertEquals(container.SecondSealPartyType.Code, consol.Containers[0].JC_AdditionalSealParty);
			AssertEquals(container.ThirdSeal, consol.Containers[0].JC_Additional2SealNum);
			AssertEquals(container.ThirdSealPartyType.Code, consol.Containers[0].JC_Additional2SealParty);
		}

		void AssertLogWarning(ForwardingConsol consol, ContainerLoadListContainerLinkManager linkManager, UniversalContainer dataObject, string warningMessage)
		{
			Logger.ClearLogs();
			new ContainerLoadListContainerDataObjectReader(dataObject, consol, linkManager, Logger, Factory).ReadIntoBusinessObject();
			AssertContains(warningMessage, Logger.GetWarnings());
		}

		UniversalContainer CreateContainerDataObject(int link, string containerNumber, int containerCount = 1, string containerType = "20GP")
		{
			return new UniversalContainer
			{
				Link = link,
				ContainerNumber = containerNumber,
				ContainerCount = containerCount,
				ContainerType = new UniversalContainerType { Code = containerType }
			};
		}

		(ForwardingConsol, JobSupplierBooking) CreateSupplierBookingWithAttachedContainers()
		{
			var consol = CreateConsolWithContainers();

			var supplierBooking = Factory.BOFactory.New<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;

			supplierBooking.Containers.AddRange(consol.Containers);

			return (consol, supplierBooking);
		}

		ForwardingConsol CreateConsolWithContainers()
		{
			var refContainer20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var refContainer40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var consol = Factory.BOFactory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CON1";
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CZDU0876174";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "DWZI4407574";
			container2.JC_RC = refContainer20GP.PK;
			var container3 = consol.Containers.AddNew();
			container3.JC_RC = refContainer20GP.PK;
			container3.JC_ContainerCount = 3;
			var container4 = consol.Containers.AddNew();
			container4.JC_RC = refContainer20GP.PK;
			container4.JC_ContainerCount = 3;
			var container5 = consol.Containers.AddNew();
			container5.JC_RC = refContainer20GP.PK;
			container5.JC_ContainerCount = 3;

			return consol;
		}

		ForwardingConsol CreateConsolWithOneContainer()
		{
			var refContainer20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var consol = Factory.BOFactory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CON1";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "DWZI4407574";
			container.JC_RC = refContainer20GP.PK;

			var supplierBooking = Factory.BOFactory.New<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = SupplierBookingLoadMode.ContainerYard;

			supplierBooking.Containers.AddRange(consol.Containers);

			return consol;
		}
	}
}
