using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonConsolDocumentSupporter))]
	sealed class CommonConsolDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestNew()
		{
			AssertNull(CommonConsolDocumentSupporter.New(null));

			AssertNotNull(CommonConsolDocumentSupporter.New(Consol));
		}

		public void TestQueryProvider()
		{
			CommonConsolDocumentSupporterForTest documentSupporter = new CommonConsolDocumentSupporterForTest(Factory.New<CommonConsol>());
			ICommonConsolDocumentSupporterQueryProvider queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is CommonConsolDocumentSupporterQueryProvider);

			Factory.SetValue<ICommonConsolDocumentSupporterQueryProvider, CommonConsolDocumentSupporterQueryProviderForTest>();

			documentSupporter = new CommonConsolDocumentSupporterForTest(Factory.New<CommonConsol>());
			queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is CommonConsolDocumentSupporterQueryProviderForTest);

			ICommonConsolDocumentSupporterQueryProvider queryProviderInSaveTransaction = null;
			Factory.Saving += _ => queryProviderInSaveTransaction = documentSupporter.QueryProvider;
			Factory.Save();
			AssertNotNull(queryProviderInSaveTransaction);
			Assert(queryProviderInSaveTransaction is CommonConsolDocumentSupporterQueryProvider);
		}

		public void TestGetFilterValue_CountryDirection()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("GB");
				Consol.JK_RL_NKLoadPort = "GBLHR";
				Consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("CountryDirection filter should return GBEXP for GB exports", "GBEXP", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CountryDirection));

				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "GBMAN";
				AssertEquals("CountryDirection filter should return GBIMP for GB imports", "GBIMP", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CountryDirection));

				GlbCompany.CurrentCompany.SetCountry("US");
				Consol.JK_RL_NKLoadPort = "USATL";
				Consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("CountryDirection filter should return USEXP for US exports", "USEXP", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CountryDirection));

				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "USATL";
				AssertEquals("CountryDirection filter should return USIMP for US imports", "USIMP", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CountryDirection));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestGetFilterValue_AUBLK()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
				AssertEquals("AUBLK filter should return 'Y'", ZBool.True.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				AssertEquals("AUBLK filter should return 'Y'", ZBool.True.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.Liquid;
				AssertEquals("AUBLK filter should return 'Y'", ZBool.True.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				AssertEquals("AUBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
				AssertEquals("AUBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				GlbCompany.CurrentCompany.SetCountry("NZ");
				Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
				AssertEquals("AUBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				AssertEquals("AUBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.Liquid;
				AssertEquals("AUBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				AssertEquals("AUBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
				AssertEquals("AUBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUBLK));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestGetFilterValue_AUNOTBLK()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.Liquid;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				AssertEquals("AUNOTBLK filter should return 'Y'", ZBool.True.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
				AssertEquals("AUNOTBLK filter should return 'Y'", ZBool.True.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				GlbCompany.CurrentCompany.SetCountry("NZ");
				Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.Liquid;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));

				Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
				AssertEquals("AUNOTBLK filter should return 'N'", ZBool.False.ToString(), Consol.DocumentSupporter.GetFilterValue(DocumentFilters.AUNOTBLK));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestContainersToSelectFrom()
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();

			Factory.Save(); //ContainersToSelectFrom loads from a Temp Factory

			AssertEquals("Should be 2 Containers to select from", 2, Consol.DocumentSupporterForTest.ContainersToSelectFromForTest.Count);
		}

		public void TestGetWrappersByContainer()
		{
			AssertEquals("Should be 0", 0, Consol.DocumentSupporterForTest.GetWrappersByContainerForTest(Consol.DocumentSupporterForTest.ContainersToSelectFromForTest).Length);

			CommonContainer container1 = Consol.Containers.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();

			Factory.Save(); //ContainersToSelectFrom loads from a Temp Factory

			ContainerToSelectFromForPrintingCollection containerToSelectFrom = Consol.DocumentSupporterForTest.ContainersToSelectFromForTest;

			AssertNotNull("Should not be null", Consol.DocumentSupporterForTest.GetWrappersByContainerForTest(containerToSelectFrom));
			AssertEquals("Should be 2 Container wrappers", 2, Consol.DocumentSupporterForTest.GetWrappersByContainerForTest(containerToSelectFrom).Length);
		}

		public void TestCreateContainerWrapper(CommonContainer container)
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			AssertEquals("Should create a DocContainer wrapper", "DocContainer", Consol.DocumentSupporterForTest.CreateContainerWrapperForTest(container1).GetType().Name);
		}

		public void TestGetChildCollectionByContainer()
		{
			ContainerToSelectFromForPrintingCollection containerToSelectFrom = Consol.DocumentSupporterForTest.ContainersToSelectFromForTest;

			AssertNotNull("Should not be null", Consol.DocumentSupporterForTest.GetChildCollectionByContainerForTest(containerToSelectFrom));
			AssertEquals("Should be 0", 0, Consol.DocumentSupporterForTest.GetChildCollectionByContainerForTest(containerToSelectFrom).Length);

			CommonContainer container1 = Consol.Containers.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();

			Factory.Save(); //ContainersToSelectFrom loads from a Temp Factory

			AssertEquals("Should be 2 Container BO's", 2, Consol.DocumentSupporterForTest.GetChildCollectionByContainerForTest(Consol.DocumentSupporterForTest.ContainersToSelectFromForTest).Length);
		}

		[TestDate(2009, 10, 10)]
		public void TestContainersCartageAdvicedUpdatedAfterCartageAdvicePrinting()
		{
			DocumentEventsForMenuForTest documentEventsForMenuForTest = new DocumentEventsForMenuForTest();
			CommonConsolDocumentSupporterForTest commonConsolDocumentSupporter = Consol.DocumentSupporterForTest;
			commonConsolDocumentSupporter.Initialise(documentEventsForMenuForTest);

			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Constants.ContainerModes.LCL;

			Factory.Save(); //ContainersToSelectFrom loads from a Temp Factory

			ContainerToSelectFromForPrintingCollection containerToSelectFrom = Consol.DocumentSupporterForTest.ContainersToSelectFromForTest;
			AssertEquals("Should be 2 Container BO's", 2, commonConsolDocumentSupporter.GetChildCollectionByContainerForTest(containerToSelectFrom).Length);

			DeliveryInstructionDestination destination = DeliveryInstructionDestination.Preview;
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Cartage Advice";
			menuItem.SU_DocumentDirection = nameof(DocumentDirection.DEP);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(destination, menuItem);

			documentEventsForMenuForTest.NotifyDocumentPrinted(eventArgs);
			AssertNotEquals("First container departure cartage adviced should be not now time", ZDateTime.Now, container1.JC_DepartureCartageAdvised);
			AssertNotEquals("Second container departure cartage adviced should be not now time", ZDateTime.Now, container2.JC_DepartureCartageAdvised);
			AssertNotEquals("First container arrival cartage adviced should be not now time", ZDateTime.Now, container1.JC_ArrivalCartageAdvised);
			AssertNotEquals("Second container arrival cartage adviced should be not now time", ZDateTime.Now, container2.JC_ArrivalCartageAdvised);

			destination = DeliveryInstructionDestination.Print;
			eventArgs = new DocumentPrintedEventArgs(destination, menuItem);

			menuItem.SU_MenuName = "Other name";
			documentEventsForMenuForTest.NotifyDocumentPrinted(eventArgs);
			AssertNotEquals("First container departure cartage adviced should be not now time", ZDateTime.Now, container1.JC_DepartureCartageAdvised);
			AssertNotEquals("Second container departure cartage adviced should be not now time", ZDateTime.Now, container2.JC_DepartureCartageAdvised);
			AssertNotEquals("First container arrival cartage adviced should be not now time", ZDateTime.Now, container1.JC_ArrivalCartageAdvised);
			AssertNotEquals("Second container arrival cartage adviced should be not now time", ZDateTime.Now, container2.JC_ArrivalCartageAdvised);

			menuItem.SU_MenuName = "Cartage Advice";
			documentEventsForMenuForTest.NotifyDocumentPrinted(eventArgs);
			AssertEquals("First container departure cartage adviced should be now time", ZDateTime.Now, container1.JC_DepartureCartageAdvised);
			AssertNotEquals("Second container departure cartage adviced should be not now time", ZDateTime.Now, container2.JC_DepartureCartageAdvised);
			AssertNotEquals("First container arrival cartage adviced should be not now time", ZDateTime.Now, container1.JC_ArrivalCartageAdvised);
			AssertNotEquals("Second container arrival cartage adviced should be not now time", ZDateTime.Now, container2.JC_ArrivalCartageAdvised);

			menuItem.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			documentEventsForMenuForTest.NotifyDocumentPrinted(eventArgs);
			AssertEquals("First container arrival cartage adviced should be now time", ZDateTime.Now, container1.JC_ArrivalCartageAdvised);
			AssertNotEquals("Second container arrival cartage adviced should be not now time", ZDateTime.Now, container2.JC_ArrivalCartageAdvised);
		}

		#region Containers Cartage Advice Update for "Multi-Container Cartage Advice" and "Multi-Container Cartage Advice with Receipt"

		[TestDate(2016, 11, 10)]
		public void TestContainersCartageAdvicedUpdatedAfterMultiContainerCartageAdvicePrinting()
		{
			AssertContainersCartageAdvicedUpdatedAfterMultiContainerCartageAdvicePrinting("Multi-Container Cartage Advice");
		}

		[TestDate(2016, 11, 10)]
		public void TestContainersCartageAdvicedUpdatedAfterMultiContainerCartageAdviceWithReceiptPrinting()
		{
			AssertContainersCartageAdvicedUpdatedAfterMultiContainerCartageAdvicePrinting("Multi-Container Cartage Advice w Receipt");
		}

		void AssertContainersCartageAdvicedUpdatedAfterMultiContainerCartageAdvicePrinting(string menuName)
		{
			var documentEventsForMenuForTest = new DocumentEventsForMenuForTest();
			var commonConsolDocumentSupporter = Consol.DocumentSupporterForTest;
			commonConsolDocumentSupporter.Initialise(documentEventsForMenuForTest);

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;
			var container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Constants.ContainerModes.LCL;

			Factory.Save(); //ContainersToSelectFrom loads from a Temp Factory

			var destination = DeliveryInstructionDestination.Preview;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = menuName;
			menuItem.SU_DocumentDirection = nameof(DocumentDirection.DEP);
			var eventArgs = new DocumentPrintedEventArgs(destination, menuItem);

			documentEventsForMenuForTest.NotifyDocumentPrinted(eventArgs);
			AssertNotEquals("First container departure cartage adviced should not be now time", ZDateTime.Now, container1.JC_DepartureCartageAdvised);
			AssertNotEquals("Second container departure cartage adviced should not be now time", ZDateTime.Now, container2.JC_DepartureCartageAdvised);
			AssertNotEquals("First container arrival cartage adviced should not be now time", ZDateTime.Now, container1.JC_ArrivalCartageAdvised);
			AssertNotEquals("Second container arrival cartage adviced should not be now time", ZDateTime.Now, container2.JC_ArrivalCartageAdvised);

			destination = DeliveryInstructionDestination.Print;
			eventArgs = new DocumentPrintedEventArgs(destination, menuItem);

			documentEventsForMenuForTest.NotifyDocumentPrinted(eventArgs);
			AssertEquals("First container departure cartage adviced should be now time", ZDateTime.Now, container1.JC_DepartureCartageAdvised);
			AssertNotEquals("Second container departure cartage adviced should be not now time", ZDateTime.Now, container2.JC_DepartureCartageAdvised);
			AssertNotEquals("First container arrival cartage adviced should be not now time", ZDateTime.Now, container1.JC_ArrivalCartageAdvised);
			AssertNotEquals("Second container arrival cartage adviced should be not now time", ZDateTime.Now, container2.JC_ArrivalCartageAdvised);

			container1.JC_DepartureCartageAdvised = ZDateTime.Empty;
			menuItem.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			documentEventsForMenuForTest.NotifyDocumentPrinted(eventArgs);
			AssertNotEquals("First container departure cartage adviced should not be now time", ZDateTime.Now, container1.JC_DepartureCartageAdvised);
			AssertNotEquals("Second container departure cartage adviced should not be now time", ZDateTime.Now, container2.JC_DepartureCartageAdvised);
			AssertEquals("First container arrival cartage adviced should be now time", ZDateTime.Now, container1.JC_ArrivalCartageAdvised);
			AssertNotEquals("Second container arrival cartage adviced should not be now time", ZDateTime.Now, container2.JC_ArrivalCartageAdvised);
		}

		#endregion

		public void TestCreateContainerDocumentSupportable()
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			AssertType("Should be of type CommonContainer", typeof(CommonConsolForTest.ContainerDocumentSupportForTest), Consol.DocumentSupporterForTest.CreateContainerDocumentSupportableForTest(container1));
		}

		public void TestGetContactOrganisation()
		{
			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			Consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			Consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			IDocumentDeliveryContact contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Type should be SendingForwarder", Consol.SendingForwarder.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportAirFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Type should be SendingForwarder", Consol.SendingForwarder.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportSeaFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Type should be SendingForwarder", Consol.SendingForwarder.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Payables, DocumentDirection.ANY);
			AssertEquals("Contact Type should be SendingForwarder", Consol.SendingForwarder.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Type should be ReceivingForwarder", Consol.ReceivingForwarder.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportAirFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Type should be ReceivingForwarder", Consol.ReceivingForwarder.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportSeaFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Type should be ReceivingForwarder", Consol.ReceivingForwarder.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Receivables, DocumentDirection.ANY);
			AssertEquals("Contact Type should be ReceivingForwarder", Consol.ReceivingForwarder.PK, contact.OrgHeader.PK);

			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgAddress departureCTO = org1.Addresses.AddNew();
			OrgAddress arrivalCTO = org2.Addresses.AddNew();
			Consol.JK_OA_DepartureCTOAddress = departureCTO.PK;
			Consol.JK_OA_ArrivalCTOAddress = arrivalCTO.PK;

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.CTO, DocumentDirection.DEP);
			AssertEquals("Contact Type should be departure CTO", org1.PK, contact.OrgHeader.PK);

			contact = Consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.CTO, DocumentDirection.ARV);
			AssertEquals("Contact Type should be Arrival CTO", org2.PK, contact.OrgHeader.PK);
		}

		public void TestReadOnlySetWhenCancelled()
		{
			CommonConsol consol = CommonConsol.New(Factory);

			consol.JK_IsCancelled = true;
			foreach (ZPropertyInfo property in consol.ZPropertyInfoHash)
			{
				AssertEquals("All properties should be read-only, no exception", true, property.ReadOnly);
			}
		}

		public void TestTransportMode()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = "";
			AssertEquals("AIR, '' -> AIR", Constants.TransportModes.Air, consol.DocumentSupporter.TransportMode);

			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			AssertEquals("AIR, LSE -> AIR", Constants.TransportModes.Air, consol.DocumentSupporter.TransportMode);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = "";
			AssertEquals("SEA, '' -> SEA", Constants.TransportModes.Sea, consol.DocumentSupporter.TransportMode);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals("SEA, LCL -> SEA", Constants.TransportModes.Sea, consol.DocumentSupporter.TransportMode);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals("SEA, FCL -> SEA", Constants.TransportModes.Sea, consol.DocumentSupporter.TransportMode);

			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			AssertEquals("SEA, BBK -> SEA", Constants.TransportModes.Sea, consol.DocumentSupporter.TransportMode);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals("ROA, FCL -> ROA", Constants.TransportModes.Road, consol.DocumentSupporter.TransportMode);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals("RAI, LCL -> RAI", Constants.TransportModes.Rail, consol.DocumentSupporter.TransportMode);
		}

		public void TestContainerMode()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals(Constants.ContainerModes.FCL, consol.DocumentSupporter.ContainerMode);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals(Constants.ContainerModes.LCL, consol.DocumentSupporter.ContainerMode);

			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			AssertEquals(Constants.ContainerModes.BreakBulk, consol.DocumentSupporter.ContainerMode);
		}

		[ExpectNoExceptions]
		public void TestGetContainerDocBusinessObjectsParameters()
		{
			var consol = Factory.New<CommonConsolForTest>();

			var queryProvider = new Mock<ICommonConsolDocumentSupporterQueryProvider>(MockBehavior.Strict);

			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false))
				.Returns(new ContainersToPrintOptions());

			consol.DocumentSupporterForTest.GetContainerDocBusinessObjects();

			queryProvider
				.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false),
					Times.Once);
		}

		public void TestGetContainerDocBusinessObjects()
		{
			CommonConsol consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			var queryProvider = new Mock<ICommonConsolDocumentSupporterQueryProvider>(MockBehavior.Strict);

			Factory.SetValue(() => queryProvider.Object);

			CommonContainer container = consol.Containers.AddNew();

			CommonShipment shipment1 = consol.Shipments.AddNew();
			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			PackLine packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;

			container.PackLines.Add(packLine1);
			container.PackLines.Add(packLine2);

			CommonShipment shipment2 = consol.Shipments.AddNew();
			PackLine packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			PackLine packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;

			container.PackLines.Remove(packLine3);
			container.PackLines.Remove(packLine4);

			Factory.Save();

			queryProvider
				.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false))
				.Returns(new ContainersToPrintOptions { ContainersToPrint = new[] { container }, IncludeUnContainerised = false });
			CommonConsolDocumentSupporterForTest documentSupporter = new CommonConsolDocumentSupporterForTest(consol);
			DocumentWrapper[] wrappers = documentSupporter.GetContainerDocBusinessObjects();
			AssertContainsExactElementsInAnyOrder(new[] { "DocContainer" }, wrappers.Select((wrapper) => wrapper.GetType().Name));
			queryProvider
				.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false), Times.Once);
		}

		public void TestLocalPortAndForeignPort()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var consolDocSupporter = new CommonConsolDocumentSupporter(consol);

			AssertEquals("Consignor should return load port as local port", "AUSYD", consolDocSupporter.LocalPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("Consignor should return discharge port as foreign port", "NZAKL", consolDocSupporter.ForeignPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("ExportDepot should return load port as local port", "AUSYD", consolDocSupporter.LocalPort(ContactType.ExportDepot, DocumentDirection.ANY));
			AssertEquals("ExportDepot should return discharge port as foreign port", "NZAKL", consolDocSupporter.ForeignPort(ContactType.ExportDepot, DocumentDirection.ANY));

			AssertEquals("Consignee should return discharge port as local port", "NZAKL", consolDocSupporter.LocalPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("Consignee should return load port as foreign port", "AUSYD", consolDocSupporter.ForeignPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("ImportDepot should return discharge port as local port", "NZAKL", consolDocSupporter.LocalPort(ContactType.ImportDepot, DocumentDirection.ANY));
			AssertEquals("ImportDepot should return load port as foreign port", "AUSYD", consolDocSupporter.ForeignPort(ContactType.ImportDepot, DocumentDirection.ANY));

			AssertEquals("Marketing DEP should return load port as local port", "AUSYD", consolDocSupporter.LocalPort(ContactType.Marketing, DocumentDirection.DEP));
			AssertEquals("Marketing DEP should return discharge port as foreign port", "NZAKL", consolDocSupporter.ForeignPort(ContactType.Marketing, DocumentDirection.DEP));

			AssertEquals("Marketing ARV should return discharge port as local port", "NZAKL", consolDocSupporter.LocalPort(ContactType.Marketing, DocumentDirection.ARV));
			AssertEquals("Marketing ARV should return load port as foreign port", "AUSYD", consolDocSupporter.ForeignPort(ContactType.Marketing, DocumentDirection.ARV));

			AssertEquals("Marketing ANY should return empty string for local port", ZString.Empty, consolDocSupporter.LocalPort(ContactType.Marketing, DocumentDirection.ANY));
			AssertEquals("Marketing ANY should return empty string for foreign port", ZString.Empty, consolDocSupporter.ForeignPort(ContactType.Marketing, DocumentDirection.ANY));
		}

		public void TestExportAndImportDepotContact()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			AssertEquals("No CFS or CTO provided, ExportDepotContact should be null", null, consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportDepot, DocumentDirection.ANY));
			AssertEquals("No CFS or CTO provided, ImportDepotContact should be null", null, consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportDepot, DocumentDirection.ANY));

			OrgAddress ctoExportOrg = Factory.New<OrgAddress>();
			ctoExportOrg.OA_OH = Factory.New<OrgHeader>().PK;
			ctoExportOrg.Header.OH_FullName = "CTO EXPORT ORG";
			consol.JK_OA_DepartureCTOAddress = ctoExportOrg.PK;
			OrgAddress ctoImportOrg = Factory.New<OrgAddress>();
			ctoImportOrg.OA_OH = Factory.New<OrgHeader>().PK;
			ctoImportOrg.Header.OH_FullName = "CTO IMPORT ORG";
			consol.JK_OA_ArrivalCTOAddress = ctoImportOrg.PK;

			AssertEquals("CTO provided, but Consol TransportMode + ConsolMode not set to SEA + FCL/BuyersConsol, ExportDepotContact should be null", null, consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportDepot, DocumentDirection.ANY));
			AssertEquals("CTO provided, but Consol TransportMode + ConsolMode not set to SEA + FCL/BuyersConsol, ImportDepotContact should be null", null, consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportDepot, DocumentDirection.ANY));

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			AssertEquals("CTO provided on SEA + FCL Consol, ExportDepotContact should be CTO", "CTO EXPORT ORG", consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportDepot, DocumentDirection.ANY).OrgHeader.FullName);
			AssertEquals("CTO provided on SEA + FCL Consol, ImportDepotContact should be CTO", "CTO IMPORT ORG", consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportDepot, DocumentDirection.ANY).OrgHeader.FullName);

			OrgAddress cfsExportOrg = Factory.New<OrgAddress>();
			cfsExportOrg.OA_OH = Factory.New<OrgHeader>().PK;
			cfsExportOrg.Header.OH_FullName = "CFS EXPORT ORG";
			consol.JK_OA_PackDepotAddress = cfsExportOrg.PK;
			OrgAddress cfsImportOrg = Factory.New<OrgAddress>();
			cfsImportOrg.OA_OH = Factory.New<OrgHeader>().PK;
			cfsImportOrg.Header.OH_FullName = "CFS IMPORT ORG";
			consol.JK_OA_UnpackDepotAddress = cfsImportOrg.PK;

			AssertEquals("Both CFS and CTO provided, ExportDepotContact should be CFS", "CFS EXPORT ORG", consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportDepot, DocumentDirection.ANY).OrgHeader.FullName);
			AssertEquals("Both CFS and CTO provided, ImportDepotContact should be CFS", "CFS IMPORT ORG", consol.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportDepot, DocumentDirection.ANY).OrgHeader.FullName);
		}

		#region Consol Cost Tests

		public void TestGetCorrectConsolCostCurrencyAndExRate()
		{
			GlbCompany nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			BusinessObject currentCompanyConsolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			currentCompanyConsolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			currentCompanyConsolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				currentCompanyConsolCost[JobConsolCostSchema.E6_ParentID] = Consol.PK;
				currentCompanyConsolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				currentCompanyConsolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			currentCompanyConsolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			currentCompanyConsolCost[JobConsolCostSchema.E6_RX_NKCurrency] = "USD";
			currentCompanyConsolCost[JobConsolCostSchema.E6_ExchangeRate] = 0.7831m;

			BusinessObject nonCurrentCompanyConsolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			nonCurrentCompanyConsolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			nonCurrentCompanyConsolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				nonCurrentCompanyConsolCost[JobConsolCostSchema.E6_ParentID] = Consol.PK;
				nonCurrentCompanyConsolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				nonCurrentCompanyConsolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			nonCurrentCompanyConsolCost[JobConsolCostSchema.E6_GC] = nonCurrentCompany.PK;
			nonCurrentCompanyConsolCost[JobConsolCostSchema.E6_RX_NKCurrency] = "GBP";
			nonCurrentCompanyConsolCost[JobConsolCostSchema.E6_ExchangeRate] = 0.3487m;

			Factory.Save();

			AssertEquals(0.7831m, Consol.FreightCostsExchangeRate);
			AssertEquals("USD", Consol.FreightCostsCurrency.RX_Code);
		}

		#endregion

		#region GetDocumentTitlesForPivot

		public void TestGetDocumentTitlesForPivot_LaserMAWB()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Blah Blah";
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Laser MAWB";
			menuItem.SU_IsSystemDefined = true;

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;
			pivot.SI_PrintCopyType = Enum.GetName(typeof(PrintCopyType), PrintCopyType.ALL);
			pivot.SI_DocumentTitle = "TEST";

			var consol = Factory.New<CommonConsol>();
			var docSupporter = new CommonConsolDocumentSupporter(consol);
			var result = docSupporter.GetDocumentTitlesForPivot("", consol, pivot);
			AssertEquals((short)0, result.CopyCount);

			var laserMAWBDocumentTitles = new ZString[] { "Original 1 - (for Issuing Carrier)", "Original 2 - (for Consignee)", "Original 3 - (for Shipper)"
				, "Copy 4 - (Delivery Receipt)", "Copy 5 - (Extra Copy)", "Copy 6 - (Extra Copy)"
				, "Copy 7 - (Extra Copy)", "Copy 8 - (for Agent)"

				, "ORIGINAL 1 - (TRANSPORTEUR ÉMETTEUR)", "ORIGINAL 2 - (POUR LE DESTINATAIRE)" ,"ORIGINAL 3 - (POUR L’EXPÉDITEUR)"
				, "COPY 4 – (RÉCÉPISSÉ DE LIVRAISON)", "COPY 5 – (COPIE SUPPLÉMENTAIRE)", "COPY 6 – (COPIE SUPPLÉMENTAIRE)"
				, "COPY 7 – (COPIE SUPPLÉMENTAIRE)", "COPY 8 – (POUR L’AGENT)"

				, "ORIGINAL 1  -(PARA EL TRANSPORTISTA EMISOR)", "ORIGINAL 2  -(PARA EL CONSIGNATARIO)", "ORIGINAL 3 - (PARA EL  EXPEDIDOR/REMITENTE)"
				, "COPIA 4 – (RECIBO DE ENTREGA)", "COPIA 5 – (COPIA ADICIONAL)", "COPIA 6 – (COPIA ADICIONAL)"
				, "COPIA 7 – (COPIA ADICIONAL)", "COPIA 8 – (PARA EL AGENTE)"

				, "Cover Page", "Email Copy", "Fax Copy" };

			foreach (var documentTitle in laserMAWBDocumentTitles)
			{
				pivot.SI_DocumentTitle = documentTitle;

				result = docSupporter.GetDocumentTitlesForPivot("", consol, pivot);
				AssertEquals((short)1, result.CopyCount);
			}

			menuItem.SU_IsSystemDefined = false;

			foreach (var documentTitle in laserMAWBDocumentTitles)
			{
				pivot.SI_DocumentTitle = documentTitle;

				result = docSupporter.GetDocumentTitlesForPivot("", consol, pivot);
				AssertNull(result);
			}
		}

		#endregion

		#region Implementation

		CommonConsolForTest Consol;

		protected override void SetUp()
		{
			Consol = Factory.New<CommonConsolForTest>();
			base.SetUp();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return CommonConsol.New(Factory);
		}

		class DocumentEventsForMenuForTest : IDocumentEventsForMenu
		{
			#region IDocumentEventsForMenu Members

			public bool CancelPrintRequest
			{
				get { throw new NotImplementedException(); }
			}

			public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(this, e);
			}

			public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(this, e);
				}
			}

			public void NotifyDocumentPrintRequested(IStmMenuItem menuItem)
			{
				if (DocumentPrintRequested != null)
				{
					DocumentPrintRequested(this, new DocumentCancelEventArgs(menuItem));
				}
			}

			public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(this, e);
				}
			}

			#endregion

			#region IDocumentEvents Members

			public event DocumentPrintedEventHandler DocumentPrePreviewed;

			public event DocumentPrintedEventHandler DocumentPrePrinted;

			public event DocumentCancelEventHandler DocumentPrintRequested;

			public event DocumentPrintedEventHandler DocumentPrinted;

			#endregion
		}

		#endregion
	}
}
