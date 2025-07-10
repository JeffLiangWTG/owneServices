using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR.Testing
{
	sealed class ShipmentDossierEventProcessorTest : TestCaseWithFactory
	{
		#region TestOnResetToOriginal

		public void TestOnMessageSent_SetStatusOfERCToMSN_AllRefsInDossier()
		{
			var shipment = CreateShipment();
			var consol = CreateConsol(isImport: true);
			shipment.Consols.Add(consol);

			var notSendAgentRefs = new ZString[] { "ERC_123", "ERC_223", "ERC_12" }.ToList();
			var dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();

			foreach (var entryNum in shipment.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(e => e.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC)))
			{
				AssertNullOrEmpty("Status is empty, becase DOS not yet send", entryNum.CE_EntryStatus);
			}

			var dynamicData = dossier.MakeDynamic();
			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns("Shipment Dossier");
			document.SetupGet(d => d.DataContext).Returns(DataContext.FRPortsIntegrationDossier);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			foreach (var entryNum in shipment.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(e => e.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC)))
			{
				AssertEquals("Status is MSN, because DOS is send", Events.MessageSentCode, entryNum.CE_EntryStatus);
			}
		}

		public void TestOnMessageSent_SetStatusOfERCToMSN_NotAllRefsInDossier()
		{
			var shipment = CreateShipment();
			var consol = CreateConsol(isImport: true);
			shipment.Consols.Add(consol);

			var notSendAgentRefs = new ZString[] { "ERC_123", "ERC_223", "ERC_12" }.ToList();
			var dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			dossier.AgentReference = "ERC_123, ERC_223";
			dossier.ECVReference = "ECV_123, ECV_223";

			foreach (var entryNum in shipment.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(e => e.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC)))
			{
				AssertNullOrEmpty("Status is empty, becase DOS not yet send", entryNum.CE_EntryStatus);
			}

			var dynamicData = dossier.MakeDynamic();
			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns("Shipment Dossier");
			document.SetupGet(d => d.DataContext).Returns(DataContext.FRPortsIntegrationDossier);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			foreach (var entryNum in shipment.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.AdditionalReferenceNumbers.Cast<CusEntryNumber>().Where(e => e.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC)))
			{
				if (entryNum.CE_EntryNum == "ERC_12")
				{
					AssertNullOrEmpty("Status is empty, becase ERC_12 not in builder references", entryNum.CE_EntryStatus);
				}
				else
				{
					AssertEquals("Status is MSN, because DOS is send", Events.MessageSentCode, entryNum.CE_EntryStatus);
				}
			}
		}

		ShipmentDossierBuilder GetNewShipmentDossierBuilder(ForwardingShipment shipment, object data)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport,
				Data = data
			};
			return new ShipmentDossierBuilder(shipment, parameters);
		}

		#endregion

		#region Implementation

		ForwardingConsol CreateConsol(bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			if (isImport)
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "FRPRA";
			}
			else
			{
				consol.JK_RL_NKLoadPort = "FRPRA";
				consol.JK_RL_NKDischargePort = "AUSYD";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			if (isAddressAvailableToCreate)
			{
				CreateAddresses(consol);
			}

			return consol;
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_UnitOfVolume = "D3";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1000;
			packline1.JL_Width = 1000;
			packline1.JL_Height = 300;
			packline1.JL_UnitOfDimension = "CM";
			packline1.JL_HarmonisedCode = "WHISKY";
			packline1.JL_RefNumber = "AMR-57";
			packline1.JL_ExportRefNumber = "AMRUT57";
			packline1.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";
			packline1.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packline1.JL_LastKnownTransitWarehouseStatus = "RCV";

			var ecv1 = packline1.AdditionalReferenceNumbers.AddNew();
			ecv1.CE_EntryStatus = string.Empty;
			ecv1.CE_EntryNum = "ECV_123";
			ecv1.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			ecv1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var erc1 = packline1.AdditionalReferenceNumbers.AddNew();
			erc1.CE_EntryStatus = string.Empty;
			erc1.CE_EntryNum = "ERC_123";
			erc1.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			erc1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 15;
			packline2.JL_Width = 10;
			packline2.JL_Height = 3;
			packline2.JL_UnitOfDimension = "M";
			packline2.JL_HarmonisedCode = "WHISKY";
			packline2.JL_RefNumber = "AMR-43";
			packline2.JL_ExportRefNumber = "AMRUT43";
			packline2.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline2.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline2.JL_LastKnownTransitWarehouseStatus = "RCV";

			var ecv2 = packline2.AdditionalReferenceNumbers.AddNew();
			ecv2.CE_EntryStatus = string.Empty;
			ecv2.CE_EntryNum = "ECV_223";
			ecv2.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			ecv2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var erc2 = packline2.AdditionalReferenceNumbers.AddNew();
			erc2.CE_EntryStatus = string.Empty;
			erc2.CE_EntryNum = "ERC_223";
			erc2.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			erc2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 6;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 600;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 450;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Length = 15;
			packline3.JL_Width = 10;
			packline3.JL_Height = 3;
			packline3.JL_UnitOfDimension = "M";
			packline3.JL_HarmonisedCode = "WHISKY";
			packline3.JL_RefNumber = "AMR-43";
			packline3.JL_ExportRefNumber = "AMRUT43";
			packline3.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline3.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline3.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline3.JL_LastKnownTransitWarehouseStatus = "RCV";

			var ecv3 = packline3.AdditionalReferenceNumbers.AddNew();
			ecv3.CE_EntryStatus = string.Empty;
			ecv3.CE_EntryNum = "ECV_12";
			ecv3.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			ecv3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var erc3 = packline3.AdditionalReferenceNumbers.AddNew();
			erc3.CE_EntryStatus = string.Empty;
			erc3.CE_EntryNum = "ERC_12";
			erc3.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			erc3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			Factory.Save();

			return shipment;
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "YUMMY";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 200";
			receivingForwarder.MainAddress.Address2 = "55 Why Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}

		#endregion
	}
}
