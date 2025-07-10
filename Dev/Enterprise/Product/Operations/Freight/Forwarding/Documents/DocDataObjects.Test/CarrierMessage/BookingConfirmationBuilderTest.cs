using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class BookingConfirmationBuilderTest : TestCaseWithFactory
	{
		[SnailTest]
		public void TestBuild() => AssertBuild(false);

		[SnailTest]
		public void TestBuild_EnableCarrierUnlocoMapping() => AssertBuild(false);

		void AssertBuild(bool enableCarrierUnlocoMapping)
		{
			var consol = CreateConsol();
			var documentData = SendBookingRequestMessage(consol);
			var setPostedTime = (ZDateTime)EnvProxy.Instance.Time.CurrentUtcDateTime.AddHours(1);
			Factory.Save();

			AcknowledgeMessageWasSentToCarrier(documentData);
			ImportBookingConfirmation();

			var builder = new BookingConfirmationBuilder(consol, GetBuilderParameters(consol));
			var bookingConfirmation = builder.Build();
			AssertAddressesBuiltCorrectly(bookingConfirmation);
			AssertNotesBuiltCorrectly(bookingConfirmation);
			AssertNumbersBuiltCorrectly(bookingConfirmation);
			AssertTransportsBuiltCorrectly(bookingConfirmation);
			AssertUnlocosBuiltCorrectly(bookingConfirmation);
			AssertCodeDescriptionsBuiltCorrectly(bookingConfirmation);
			AssertIsDoorPropertiesBuiltCorrectly(bookingConfirmation);
			AssertContainersBuiltCorrectly(bookingConfirmation);
		}

		public void TestBuild_NullCollections()
		{
			var consol = CreateConsol();
			var documentData = SendBookingRequestMessage(consol);
			AcknowledgeMessageWasSentToCarrier(documentData);
			ImportBookingConfirmation(BookingConfirmationTestHelper.BookingConfirmationUXMLEmptyCollections_LinkOnly, true);

			var builder = new BookingConfirmationBuilder(consol, GetBuilderParameters(consol));
			var bookingConfirmation = builder.Build();
			AssertNullCollections(bookingConfirmation);
		}

		public void TestUniversalShipmentMissingContainerType()
		{
			var consol = CreateConsol();
			var documentData = SendBookingRequestMessage(consol);
			var setPostedTime = (ZDateTime)EnvProxy.Instance.Time.CurrentUtcDateTime.AddHours(1);
			Factory.Save();

			AcknowledgeMessageWasSentToCarrier(documentData);
			ImportBookingConfirmation(BookingConfirmationTestHelper.BookingConfirmationUXMLEmptyCollections_LinkOnly);

			var builder = new BookingConfirmationBuilder(consol, GetBuilderParameters(consol));
			AssertNoExceptionThrown(() => builder.Build());
		}

		public void TestNullUniversalShipment()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters();

			var builder = new BookingConfirmationBuilder(consol, parameters);
			AssertNoExceptionThrown(() => builder.Build());
		}

		public void TestCompareCarrierContractNumber()
		{
			var consol = CreateConsol(true);
			var documentData = SendBookingRequestMessage(consol);
			var setPostedTime = (ZDateTime)EnvProxy.Instance.Time.CurrentUtcDateTime.AddHours(1);
			Factory.Save();

			AcknowledgeMessageWasSentToCarrier(documentData);
			ImportBookingConfirmation();

			var builder = new BookingConfirmationBuilder(consol, GetBuilderParameters(consol));
			var bookingConfirmation = builder.Build();
			AssertEquals("222", bookingConfirmation.CarrierContractNumber.ImportableValue);
			AssertHasWarning(bookingConfirmation.CarrierContractNumber.ImportableValueInfo, "Requested Info: CQN11111");
		}

		#region TestContextWithCarrierUnlocoMapping

		public void TestContextWithCarrierUnlocoMapping_Agent()
		{
			var consol = CreateConsol();
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var documentData = SendBookingRequestMessage(consol);
			AcknowledgeMessageWasSentToCarrier(documentData);
			ImportBookingConfirmation(BookingConfirmationTestHelper.BookingConfirmationUXMLWithUnlocoMappings_LinkOnly);

			var mapping1 = consol.ShippingLine.CreatePatternMatchOverrideForTest();

			mapping1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping1.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping1.OO_ForeignCode = "AEZZZ";
			mapping1.OO_LocalCode = "AEJEA";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "AEYYY";
			mapping2.OO_LocalCode = "AEJEA";

			Factory.Save();

			var builder = new BookingConfirmationBuilder(consol, GetBuilderParameters(consol));
			var bookingConfirmation = builder.Build();
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(bookingConfirmation.PortOfLoad.Unlocos);
			AssertEquals("Mapping should be from the carrier's local code", "AEJEA", bookingConfirmation.PortOfLoad.Code);
		}

		public void TestContextWithCarrierUnlocoMapping_Coload()
		{
			var consol = CreateConsol();
			var documentData = SendBookingRequestMessage(consol);
			AcknowledgeMessageWasSentToCarrier(documentData);
			ImportBookingConfirmation(BookingConfirmationTestHelper.BookingConfirmationUXMLWithUnlocoMappings_LinkOnly);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var mapping1 = consol.ShippingLine.CreatePatternMatchOverrideForTest();

			mapping1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping1.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping1.OO_ForeignCode = "AEYYY";
			mapping1.OO_LocalCode = "AEJEA";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "AEZZZ";
			mapping2.OO_LocalCode = "AEJEA";

			Factory.Save();

			var builder = new BookingConfirmationBuilder(consol, GetBuilderParameters(consol));
			var bookingConfirmation = builder.Build();
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(bookingConfirmation.PortOfLoad.Unlocos);
			AssertEquals("Mapping should be from the co-load with's local code", "AEJEA", bookingConfirmation.PortOfLoad.Code);
		}

		#endregion

		#region Implementation

		void AssertAddressesBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			AssertEquals("Consignor Documentary Address Company", bookingConfirmation.Shipper.CompanyName);
			AssertHasWarning(bookingConfirmation.Shipper.CompanyNameInfo, "Requested Info: CONSIGNOR BR");
			AssertEquals("Shipping Line Address Company", bookingConfirmation.Carrier.CompanyName);
			AssertHasWarning(bookingConfirmation.Carrier.CompanyNameInfo, "Requested Info: CARRIER BR");
			AssertEquals("Consignee Address Company", bookingConfirmation.Consignee.CompanyName);
			AssertEquals("Notify Party Company", bookingConfirmation.NotifyParty.CompanyName);
			AssertHasWarning(bookingConfirmation.NotifyParty.CompanyNameInfo, "Requested Info: NOTIFYPARTY BR");
			AssertEquals("Departure CTO Address Company", bookingConfirmation.ContainerTerminalOperator.ImportableValue.CompanyName);
			AssertEquals("Consignor Pickup Delivery Address Company", bookingConfirmation.PickUpFrom.CompanyName);
			AssertEquals("Consignee Pickup Delivery Address Company", bookingConfirmation.DeliverTo.CompanyName);
		}

		void AssertNotesBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			AssertEquals("GENERAL INFO FOR THE BOOKING CONFIRMATION", bookingConfirmation.CarrierConfirmationNotes.ImportableValue);
		}

		void AssertNumbersBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			AssertEquals("CLDCR123456", bookingConfirmation.CarrierBookingReference.ImportableValue);
			AssertHasWarning(bookingConfirmation.CarrierBookingReference.ImportableValueInfo, "Requested Info: BR001");
			AssertEquals("Ye Olde Vessel", bookingConfirmation.VesselName);
			AssertHasWarning(bookingConfirmation.VesselNameInfo, "Requested Info: Black Hole");
			AssertEquals("6969", bookingConfirmation.LloydsIMO);
			AssertEquals("69420", bookingConfirmation.VoyageNumber);
			AssertHasWarning(bookingConfirmation.VoyageNumberInfo, "Requested Info: 222");
			AssertEquals("CLDBL123456", bookingConfirmation.BillOfLadingNumber.ImportableValue);
			AssertHasWarning(bookingConfirmation.BillOfLadingNumber.ImportableValueInfo, "Requested Info: BL091042970");

			AssertEquals("111", bookingConfirmation.FreightForwarderReferenceNumber);
			AssertHasWarning(bookingConfirmation.FreightForwarderReferenceNumberInfo, "Requested Info: C01329220");
			AssertEquals("222", bookingConfirmation.CarrierContractNumber.ImportableValue);
			AssertHasWarning(bookingConfirmation.CarrierContractNumber.ImportableValueInfo, "Requested Info: 11111");
			AssertEquals("333", bookingConfirmation.ShipperReferenceNumber);
			AssertHasWarning(bookingConfirmation.ShipperReferenceNumberInfo, "Requested Info: AR001");
			AssertEquals("444", bookingConfirmation.ContractNamedAccountNumber);
			AssertHasWarning(bookingConfirmation.ContractNamedAccountNumberInfo, "Requested Info: 22222");
		}

		void AssertTransportsBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			var transports = bookingConfirmation.Transports.ImportableValue;
			AssertEquals(3, transports.Count);

			AssertEquals("Transport 1 mode", "SEA", transports.ElementAt(0).Mode.Code);
			AssertHasWarning(((DocumentVisualizer.DocDataObjects.CodeDescription)transports.ElementAt(0).Mode).CodeInfo, "Requested Info: AIR");
			AssertHasWarning(transports.ElementAt(0).Vessel.NameInfo, "Requested Info: Dragon");
			AssertHasWarning(transports.ElementAt(0).ETDInfo, "Requested Info: 01-Dec-18 00:00:00");
			AssertEquals("Transport 2 mode", "RAI", transports.ElementAt(1).Mode.Code);
			AssertHasWarning(transports.ElementAt(1).Vessel.NameInfo, "Requested Info: Black Hole");
			AssertEquals("Transport 3 mode", ZString.Empty, transports.ElementAt(2).Mode.Code);

			AssertEquals("BEANR", bookingConfirmation.Origin.Code);
			AssertHasWarning(bookingConfirmation.Origin.CodeInfo, "Requested Info: AUSYD");
			AssertEquals(transports.First().ETD, bookingConfirmation.EarliestDepartureDate);

			AssertEquals("AUMEL", bookingConfirmation.Destination.Code);
			AssertHasWarning(bookingConfirmation.Destination.CodeInfo, "Requested Info: CNSHA");
			AssertEquals(transports.Last().ETA, bookingConfirmation.LatestDeliveryDate);
		}

		void AssertUnlocosBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			AssertEquals("FIHEL", bookingConfirmation.CarrierBookingOffice.Code);
			AssertEquals("AEJEA", bookingConfirmation.PlaceOfReceipt.Code);
			AssertHasWarning(bookingConfirmation.PlaceOfReceipt.CodeInfo, "Requested Info: AUSYD");
			AssertEquals("BEANR", bookingConfirmation.PlaceOfDelivery.Code);
			AssertHasWarning(bookingConfirmation.PlaceOfDelivery.CodeInfo, "Requested Info: CNSHA");
			AssertEquals("AEJEA", bookingConfirmation.PortOfLoad.Code);
			AssertHasWarning(bookingConfirmation.PortOfLoad.CodeInfo, "Requested Info: SGSIN");
			AssertEquals("BEANR", bookingConfirmation.PortOfDischarge.Code);
			AssertHasWarning(bookingConfirmation.PortOfDischarge.CodeInfo, "Requested Info: CNSHA");
		}

		void AssertCodeDescriptionsBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			AssertEquals(Core.Constants.AgentType.CoLoad, bookingConfirmation.ShipmentType.Code);
			AssertEquals(Core.Constants.ContainerModes.FCL, bookingConfirmation.ContainerMode.Code);
		}

		void AssertIsDoorPropertiesBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			AssertEquals(true, bookingConfirmation.IsDoorPickup);
			AssertEquals(true, bookingConfirmation.IsDoorDelivery);
		}

		void AssertContainersBuiltCorrectly(BookingConfirmation bookingConfirmation)
		{
			AssertEquals(3, bookingConfirmation.Containers.Count);

			var container1 = bookingConfirmation
				.Containers
				.Single(container => container.ImportableValue.Number == "TCLU1234567")
				.ImportableValue;
			AssertEquals(1, container1.ContainerCount);
			AssertEquals(2, container1.PackCount);
			AssertEquals(string.Empty, container1.Type.Code);
			AssertEquals("22R0", container1.Type.ISOCode);
			AssertEquals(350m, container1.NetWeight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, container1.NetWeight.Unit.Code);
			AssertEquals(1350m, container1.GrossWeight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, container1.GrossWeight.Unit.Code);
			AssertEquals(1000m, container1.TareWeight.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, container1.TareWeight.Unit.Code);
			AssertEquals(920m, container1.Volume.Value);
			AssertEquals(Core.Constants.Volume.CubicMetres, container1.Volume.Unit.Code);
			AssertEquals("AUSTRALIAN AIR EXPRESS PTY LTD", container1.DepartureContainerYard.CompanyName);
			AssertEquals("2017-11-29", container1.EmptyRequired.ToString("yyyy-MM-dd"));

			var container2 = bookingConfirmation
				.Containers
				.Single(container => container.ImportableValue.Number == "1234567")
				.ImportableValue;
			AssertEquals(1, container2.ContainerCount);
			AssertHasWarning(container2.ContainerCountInfo, "Requested Info: 2");
			AssertEquals(0, container2.PackCount);
			AssertEquals("6XX9", container2.Type.Code);
			AssertEquals(string.Empty, container2.Type.ISOCode);

			var container3 = bookingConfirmation
				.Containers
				.Single(container => container.ImportableValue.Number == string.Empty)
				.ImportableValue;
			AssertEquals(1, container3.ContainerCount);
			AssertHasWarning(container3.ContainerCountInfo, "Requested Info: 2");
			AssertEquals(0, container3.PackCount);
			AssertEquals("6XX9", container3.Type.Code);
			AssertEquals(string.Empty, container3.Type.ISOCode);
		}

		void AssertNullCollections(BookingConfirmation bookingConfirmation)
		{
			AssertEquals("NotesCollection", null, bookingConfirmation.CarrierConfirmationNotes);
			AssertNullOrEmpty("AdditionalReferencecollection", bookingConfirmation.ShipperReferenceNumber);
			AssertNullOrEmpty("AdditionalReferencecollection", bookingConfirmation.FreightForwarderReferenceNumber);
			AssertEquals("AdditionalReferencecollection", ZString.Empty, bookingConfirmation.CarrierContractNumber.ImportableValue);
			AssertNullOrEmpty("AdditionalReferencecollection", bookingConfirmation.ContractNamedAccountNumber);
			AssertEquals("ContainerCollection", null, bookingConfirmation.Containers);
			AssertEquals("IsDoorPickup", false, bookingConfirmation.IsDoorPickup);
			AssertEquals("IsDoorDelivery", false, bookingConfirmation.IsDoorDelivery);
		}

		ForwardingConsol CreateConsol(bool defaultCQN = false)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01329220";
			consol.JK_MasterBillNum = "BL091042970";
			consol.JK_AgentsReference = "AR001";
			consol.JK_BookingReference = "BR001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_CarrierContractNumber = "11111";

			var shipper = CreateOrgHeaderWithAddress("BR1", "Consignor BR", "Consignor Address1 BR", "Consignor Address2 BR");
			consol.MasterBillShipperOverrideDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var carrier = CreateOrgHeaderWithAddress("BR2", "Carrier BR", "Carrier Address1 BR", "Carrier Address2 BR");
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var notifyParty = CreateOrgHeaderWithAddress("BR4", "NotifyParty BR", "NotifyParty Address1 BR", "NotifyParty Address2 BR");
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var nacNumber = consol.Numbers.AddNew();
			nacNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			nacNumber.CE_EntryNum = "22222";

			if (defaultCQN)
			{
				var number = consol.Numbers.AddNew();
				number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CQN;
				number.CE_EntryNum = "CQN11111";
			}

			var shipment = consol.Shipments.AddNew();
			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 1;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1234567";
			container1.JC_ContainerCount = 2;
			container1.PackLines.Add(packingLine1);

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 1;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = string.Empty;
			container2.JC_ContainerCount = 2;
			container2.PackLines.Add(packingLine2);

			var transport = consol.Transports[0];
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "CNSHA";
			transport2.JW_Vessel = "Black Hole";
			transport2.JW_VoyageFlight = "222";

			Factory.Save();

			return consol;
		}

		OrgHeader CreateOrgHeaderWithAddress(ZString code, ZString name, ZString address1, ZString address2)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.OH_FullName = name;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = address1;
			orgHeader.MainAddress.Address2 = address2;
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2022";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			return orgHeader;
		}

		VisualizerDocumentData SendBookingRequestMessage(ForwardingConsol consol)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.Parent = consol;
			documentData.JDD_Name = ConsolDocumentDataStoreNames.SeaBookingRequest2;

			var parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = documentData
			};

			var bookingRequestBuilder = new BookingRequestBuilder(consol, parameters);
			var bookingRequest = bookingRequestBuilder.Build();

			var data = bookingRequest.MakeDynamic();
			var document = new DummyDocument();
			document.Data = data;
			document.DataContext = DataContext.BookingRequest;

			var uxmlDataObjectWriterStrategy = new DefaultDataObjectWriterStrategy();
			var uxmlDataObject = consol.GetSupporter().GetUniversalXmlDataObject(uxmlDataObjectWriterStrategy, document);

			Assert("UXml data object has been created successfully", uxmlDataObject.IsRight);

			var uxml = uxmlDataObject.Right.ToUniversalXml().WrapInInterchange();

			documentData.CreateMessageSentLog(ConsolDocumentNames.BookingRequest);
			documentData.CreateDataExportEventLog(uxml);

			return documentData;
		}

		void AcknowledgeMessageWasSentToCarrier(VisualizerDocumentData documentData)
		{
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					ConsolDocumentNames.BookingRequest),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					"Carrier")
			};

			documentData.Logs.CreateOrRecreateEventLog(
				Events.MessageAccepted,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);
		}

		void ImportBookingConfirmation(string bookingConfirmationUXML = "", bool setCollectionsToNull = false)
		{
			var message = Factory.CreateBookingConfirmationMessage(bookingConfirmationUXML);

			var logger = new UniversalXmlImportLogger();
			var universalFactory = new UniversalObjectFactory(Factory);
			message.ProcessUniversalMessage(universalFactory, logger);

			var logs = string.Join("\r\n", logger.Logs.Select(l => l.Message));

			if (setCollectionsToNull)
			{
				AssertContains(@"Universal Shipment data was linked to Consol C01329220 (Master Bill='BL091042970').
Successfully saved, but nothing was reported as being updated.", logs, true);
			}
			else
			{
				AssertContains("Universal Shipment data was linked to Consol C01329220 (Master Bill='BL091042970').", logs, true);
			}
		}

		IDocDataObjectParameters GetBuilderParameters(ForwardingConsol consol)
		{
			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BookingConfirmation);

			return new DummyDocDataObjectParameters
			{
				Data = supporter.GetAdditionalData(consol, menuItem).Right
			};
		}

		StmMenuItem CreateMenuItem(string context)
		{
			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = context;

			var menuItem = Factory.New<DocumentCommand>();
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			return menuItem;
		}

		sealed class UniversalXmlImportLogger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => logs;

			readonly List<ISimpleLog> logs = new List<ISimpleLog>();

			public void Clear() => logs.Clear();

			public void Log(LogType type, string message)
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					logs.Add(new SimpleLog(type, message));
				}
			}
		}

		#endregion

		public void TestBuildWithNullShipmentProperties()
		{
			var consol = CreateConsol();
			var documentData = SendBookingRequestMessage(consol);
			AcknowledgeMessageWasSentToCarrier(documentData);
			ImportBookingConfirmation();

			var parameters = GetBuilderParameters(consol);
			var builder = new BookingConfirmationBuilder(consol, parameters);
			var shipment = parameters.Data as UniversalShipment;
			AssertNotNull(shipment);

			shipment.SetAdditionalReferenceCollection(() => null);
			shipment.SetPackingLineCollection(() => null);
			shipment.SetContainerCollection(() => null);
			shipment.SetTransportLegCollection(() => null);
			shipment.SetOrganizationAddressCollection(() => null);
			shipment.SetNoteCollection(() => null);

			AssertNoExceptionThrown(() => builder.Build());
		}
	}
}
