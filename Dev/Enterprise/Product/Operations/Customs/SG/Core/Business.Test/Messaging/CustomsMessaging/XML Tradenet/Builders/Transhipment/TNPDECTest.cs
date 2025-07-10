using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class TNPDECTest : TestCaseWithFactory
	{
		public void TestStorageLocation()
		{
			DataProvider.PlaceOfStorage = "AUSYD";
			DataProvider.Is2bStoredBWCY = true;
			DataProvider.IsStorageInFTZ = false;
			Message.BuildCargo(TNPDEC);
			AssertEquals("AUSYD", TNPDEC.Cargo.StorageLocation.LocationCode);
			DataProvider.Is2bStoredBWCY = false;
			DataProvider.IsStorageInFTZ = true;
			Message.BuildCargo(TNPDEC);
			AssertEquals("AUSYD", TNPDEC.Cargo.StorageLocation.LocationCode);
			DataProvider.Is2bStoredBWCY = false;
			DataProvider.IsStorageInFTZ = false;
			TNPDEC.Cargo.StorageLocation = null;
			Message.BuildCargo(TNPDEC);
			AssertNull(TNPDEC.Cargo.StorageLocation);
		}

		public void TestPlaceOfStorage()
		{
			DataProvider.IsStorageInFTZ = true;
			DataProvider.PlaceOfStorage = "EXAREA";
			Message.BuildCargo(TNPDEC);
			AssertEquals("Outward Vessel Berth", "EXAREA", TNPDEC.Cargo.StorageLocation.LocationCode);
		}

		public void TestNextPortOfCall()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.NextPortOfCall = "THBKK";
			Message.BuildTransport(TNPDEC);
			var additionalVesselInformation = TNPDEC.Transport.OutwardTransport.AdditionalVesselInformation;
			AssertEquals("THBKK", additionalVesselInformation.LoadingNextPort);
		}

		public void TestFinalPortOfCall()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.HasLiquorOrTobacco = true;
			DataProvider.FinalPortOfCall = "MYPTK";
			Message.BuildTransport(TNPDEC);
			var additionalVesselInformation = TNPDEC.Transport.OutwardTransport.AdditionalVesselInformation;
			AssertEquals("MYPTK", additionalVesselInformation.LoadingFinalPort);
		}

		public void TestCountryOfFinalDestination()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.CountryOfFinalDestination = "HK";
			Message.BuildTransport(TNPDEC);
			var outwardTransport = TNPDEC.Transport.OutwardTransport;
			AssertNull("S/b no Country of Final Dest. (Seastores)", outwardTransport.FinalDestinationCountry);
			DataProvider.IsSeaStoreDeclaration = false;
			DataProvider.IsForStorage = true;
			Message.BuildTransport(TNPDEC);
			outwardTransport = TNPDEC.Transport.OutwardTransport;
			AssertEquals("This requirement appears to have changed, (refer CS00187631). Message should have Country of Final Dest. even if Place of Storage entered.", "HK", outwardTransport.FinalDestinationCountry);
			DataProvider.IsForStorage = false;
			Message.BuildTransport(TNPDEC);
			outwardTransport = TNPDEC.Transport.OutwardTransport;
			AssertEquals("Country of Final Dest.", "HK", outwardTransport.FinalDestinationCountry);
		}

		[TestDate(2020, 05, 26)]
		public void TestTransportEquipmentAndRemovalStartDate()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REM;
			DataProvider.HasInwardTransport = true;
			DataProvider.ArrivalDate = ZDate.Today;
			DataProvider.StartDateOfCargoRemoval = ZDate.Today;
			var containers = new ContainersTestClass[3];
			containers[0] = new ContainersTestClass();
			containers[0].ContainerNumber = "ABCU0039477";
			containers[0].ContainerType = "FCL";
			containers[0].ContainerSize = 20;
			containers[0].ContainerWeight = 35;
			containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1] = new ContainersTestClass();
			containers[1].ContainerNumber = "NYKU2188343";
			containers[1].ContainerType = "LCL";
			containers[1].ContainerSize = 40;
			containers[1].ContainerWeight = 120;
			containers[1].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1].SealNumber = "SHP1233445";
			containers[2] = new ContainersTestClass();
			containers[2].ContainerNumber = "NYKU4798228";
			containers[2].ContainerType = "LCL";
			containers[2].ContainerSize = 40;
			containers[2].ContainerWeight = 24780;
			containers[2].ContainerWeightUnit = Core.Constants.Weight.Kilograms;
			containers[2].SealNumber = "XX-03948";
			DataProvider.Containers = containers;
			Message.BuildCargo(TNPDEC);
			var cargo = TNPDEC.Cargo;
			var containerEquipment = cargo.TransportEquipment;
			AssertEquals(3, containerEquipment.Length);
			CombineAssertions("ABCU0039477", () =>
			{
				var equipment = containerEquipment.First(c => c.EquipmentID == "ABCU0039477");
				AssertEquals("1", equipment.SequenceNumeric);
				AssertEquals("FCL20", equipment.SizeTypeCode);
				AssertEquals(35m, equipment.EquipmentWeightMeasureNumeric);
				AssertEquals("NA", equipment.TransportEquipmentSeal.SealID);
				AssertEquals("removalDate", "20200526", cargo.RemovalStartDate);
			}

			);
			CombineAssertions("NYKU2188343", () =>
			{
				var equipment = containerEquipment.First(c => c.EquipmentID == "NYKU2188343");
				AssertEquals("2", equipment.SequenceNumeric);
				AssertEquals("LCL40", equipment.SizeTypeCode);
				AssertEquals(120m, equipment.EquipmentWeightMeasureNumeric);
				AssertEquals("SHP1233445", equipment.TransportEquipmentSeal.SealID);
				AssertEquals("removalDate", "20200526", cargo.RemovalStartDate);
			}

			);
			CombineAssertions("NYKU4798228", () =>
			{
				var equipment = containerEquipment.First(c => c.EquipmentID == "NYKU4798228");
				AssertEquals("3", equipment.SequenceNumeric);
				AssertEquals("LCL40", equipment.SizeTypeCode);
				AssertEquals(25m, equipment.EquipmentWeightMeasureNumeric);
				AssertEquals("XX-03948", equipment.TransportEquipmentSeal.SealID);
				AssertEquals("removalDate", "20200526", cargo.RemovalStartDate);
			}

			);
		}

		public void TestExhibitionTemporaryImportPeriodStartDate()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKT;
			DataProvider.StartDateOfBlanket = new ZDate(2011, 05, 19);
			Message.BuildCargo(TNPDEC);
			AssertNull("Should be null.", TNPDEC.Cargo.ExhibitionTemporaryImportPeriod);
		}

		public void TestExporterIsNotIncludedInTNPDec()
		{
			var testExporter = new OrganisationTestClass();
			testExporter.Name = "C & A MEXICO S. DE R. L.";
			testExporter.Address = new OrganisationAddressTestClass("AV.CAMINO AL ITESO No.8350", "MEXICO R.F.C CME-", "961203-360", "MX", "");
			testExporter.UEN = "197201770G";
			DataProvider.IsOutwardDeclaration = true;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.IGM;
			DataProvider.Exporter = testExporter;
			Message.BuildParty(TNPDEC);
			AssertNull("ExporterParty is not required in TNP declaration message", TNPDEC.Party.ExporterParty);
		}

		public void TestHandlingAgentIsIncludedInTNPDec()
		{
			var handlingAgent = new OrganisationTestClass();
			handlingAgent.Name = "Anglo-Eastern Agency (Singapore) Pte. Ltd.";
			handlingAgent.UEN = "197201770G";
			DataProvider.IsOutwardDeclaration = true;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCI;
			DataProvider.HandlingAgent = handlingAgent;
			Message.BuildParty(TNPDEC);
			var messageHandlingAgent = TNPDEC.Party.HandlingAgentParty;
			AssertNotNull("Handling Agent Party is required in the message when entered in a TNP declaration", messageHandlingAgent);
			AssertEquals("Handling Agent - PartyIdentification", "197201770G", messageHandlingAgent.PartyIdentification.ID);
			AssertEquals("Handling Agent - PartyName", "ANGLO-EASTERN AGENCY (SINGAPORE) PTE. LTD.", messageHandlingAgent.PartyName[0]);
		}

		public void TestGenerateContainerDetails()
		{
			ContainersTestClass[] containers = new ContainersTestClass[2];
			containers[0] = new ContainersTestClass();
			containers[1] = new ContainersTestClass();
			containers[0].ContainerNumber = "MSKU3824281";
			containers[0].ContainerType = "FCL";
			containers[0].ContainerSize = 40;
			containers[0].ContainerWeight = 5.8m;
			containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1].ContainerNumber = "PONU5582963";
			containers[1].ContainerType = "LCL";
			containers[1].ContainerSize = 20;
			containers[1].ContainerWeight = 3;
			containers[1].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			containers[1].SealNumber = "SHP1233445";
			DataProvider.Containers = containers;
			Message.BuildCargo(TNPDEC);
			var transportEquipment = TNPDEC.Cargo.TransportEquipment;
			bool container1DetailsGenerated = false;
			bool container2DetailsGenerated = false;
			foreach (TransportEquipment containerDetails in transportEquipment)
			{
				if (containerDetails.EquipmentID == "MSKU3824281")
				{
					container1DetailsGenerated = true;
					AssertEquals("FCL40", containerDetails.SizeTypeCode);
					AssertEquals("Weight must be in whole numbers", 6m, containerDetails.EquipmentWeightMeasureNumeric);
					AssertEquals("NA", containerDetails.TransportEquipmentSeal.SealID);
				}

				if (containerDetails.EquipmentID == "PONU5582963")
				{
					container2DetailsGenerated = true;
					AssertEquals("LCL20", containerDetails.SizeTypeCode);
					AssertEquals(3m, containerDetails.EquipmentWeightMeasureNumeric);
					AssertEquals("SHP1233445", containerDetails.TransportEquipmentSeal.SealID);
				}
			}

			Assert(container1DetailsGenerated);
			Assert(container2DetailsGenerated);
		}

		public void TestCPCsForSeastores()
		{
			var cpc = Declaration.CPCs.AddNew();
			cpc.SG_CPCCode = "7723000";
			cpc.SG_PC1 = "12";
			cpc.SG_PC2 = "6";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.IGM;
			DataProvider.CPCs = CusEntry.CPCs;
			Message.BuildHeader(TNPDEC);
			var cpcs = TNPDEC.Header.CustomsProcedureCodeInformation;
			AssertEquals("1 CPC code should have been generated", 1, cpcs.Length);
			CustomsProcedureCodeInformation prodedureCodeInfo = cpcs[0];
			AssertEquals("CPC", "7723000", prodedureCodeInfo.CustomsProcedureCode);
			AssertEquals("PC1", "12", prodedureCodeInfo.CPCProcessingCode[0].ProcessingCodeOne);
			AssertEquals("PC2", "6", prodedureCodeInfo.CPCProcessingCode[0].ProcessingCodeTwo);
			AssertEquals("PC3", null, prodedureCodeInfo.CPCProcessingCode[0].ProcessingCodeThree);
		}

		public void TestSeaTransportElements()
		{
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.InwardTransportIdentifier = "IV2232324";
			DataProvider.OutwardTransportIdentifier = "OV010101";
			DataProvider.OutwardJourneyIdentifier = "V555555";
			DataProvider.OutwardVesselType = "CV";
			DataProvider.OutwardVesselNationality = "SG";
			DataProvider.TowingVesselName = "TW00101";
			Message.BuildTransport(TNPDEC);
			var inwardTransport = TNPDEC.Transport.InwardTransport;
			AssertEquals(1, inwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("IV2232324", inwardTransport.TransportMeans.TransportMode.TransportIdentifier);
			var outwardTransport = TNPDEC.Transport.OutwardTransport;
			AssertEquals(1, outwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("V555555", outwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
			AssertEquals("OV010101", outwardTransport.TransportMeans.TransportMode.TransportIdentifier);
			var additionalVesselInformation = TNPDEC.Transport.OutwardTransport.AdditionalVesselInformation;
			AssertEquals("CV", additionalVesselInformation.VesselType);
			AssertEquals("SG", additionalVesselInformation.VesselNationality);
			AssertEquals("TW00101", additionalVesselInformation.TowingVessel.VesselName);
			//CUSDECMessage msg = MessageBuilder.CusdecMessage;
			//AssertEquals("TDT+3+NA+1+++++:::IV2232324'TDT+12+V555555+1+CV++++:::OV010101'TDT+24+NA++++++:::TW00101'", msg.Group4.ToString(new UNOASGCharacterSet()));
			//DataProvider.InwardJourneyIdentifier = "220E";
			//DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTF;
			//DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			//DataProvider.OutwardJourneyIdentifier = "SQ190";
			//DataProvider.IsSeaStoreDeclaration = true;
			//DataProvider.HasLiquorOrTobacco = false;
			//ResetMessageBuilder();
			//msg = MessageBuilder.CusdecMessage;
			//AssertEquals("TDT+3+220E+1+++++:::IV2232324'TDT+12+SQ190+4+CV++++:::OV010101'", msg.Group4.ToString(new UNOASGCharacterSet()));
			//DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			//DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			//DataProvider.HasLiquorOrTobacco = true;
			//ResetMessageBuilder();
			//msg = MessageBuilder.CusdecMessage;
			//AssertEquals("TDT+3+220E+1+++++:::IV2232324'TDT+12+SQ190+1+CV++++:::OV010101'TPL+::::SG'TDT+24+NA++++++:::TW00101'", msg.Group4.ToString(new UNOASGCharacterSet()));
		}

		public void TestSeaAirTransportElements()
		{
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.InwardTransportIdentifier = "IV2232324";
			DataProvider.InwardJourneyIdentifier = "220E";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTF;
			DataProvider.OutwardJourneyIdentifier = "SQ190";
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.HasLiquorOrTobacco = false;
			Message.BuildTransport(TNPDEC);
			var inwardTransport = TNPDEC.Transport.InwardTransport;
			AssertEquals(1, inwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("220E", inwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
			AssertEquals("IV2232324", inwardTransport.TransportMeans.TransportMode.TransportIdentifier);
			var outwardTransport = TNPDEC.Transport.OutwardTransport;
			AssertEquals(4, outwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("SQ190", outwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
		}

		public void TestSeaLiqourTobaccoTransportElements()
		{
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.InwardTransportIdentifier = "IV2232324";
			DataProvider.InwardJourneyIdentifier = "220E";
			DataProvider.OutwardTransportIdentifier = "OV010101";
			DataProvider.OutwardJourneyIdentifier = "V555555";
			DataProvider.OutwardVesselType = "CV";
			DataProvider.OutwardVesselNationality = "SG";
			DataProvider.TowingVesselName = "TW00101";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			DataProvider.HasLiquorOrTobacco = true;
			Message.BuildTransport(TNPDEC);
			var inwardTransport = TNPDEC.Transport.InwardTransport;
			AssertEquals(1, inwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("220E", inwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
			AssertEquals("IV2232324", inwardTransport.TransportMeans.TransportMode.TransportIdentifier);
			var outwardTransport = TNPDEC.Transport.OutwardTransport;
			AssertEquals(1, outwardTransport.TransportMeans.TransportMode.ModeCode);
			AssertEquals("V555555", outwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
			AssertEquals("OV010101", outwardTransport.TransportMeans.TransportMode.TransportIdentifier);
			var additionalVesselInformation = TNPDEC.Transport.OutwardTransport.AdditionalVesselInformation;
			AssertEquals("CV", additionalVesselInformation.VesselType);
			AssertEquals("SG", additionalVesselInformation.VesselNationality);
			AssertEquals("TW00101", additionalVesselInformation.TowingVessel.VesselName);
		}

		public void TestGenerateAttachmentElements()
		{
			AttachmentsTestClass[] attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass();
			attachments[0].FileName = "ATTDOC_0001";
			attachments[0].DocType = SupportingDocumentTypeCodeList.Codes.DocType001;
			attachments[1] = new AttachmentsTestClass();
			attachments[1].FileName = "ATTDOC_0002";
			attachments[1].DocType = SupportingDocumentTypeCodeList.Codes.DocType999;
			SGCUSDECTestClass.ImplementsAdditionalMessageInformation addInfo = (SGCUSDECTestClass.ImplementsAdditionalMessageInformation)DataProvider.AdditionalMessageInformation;
			addInfo.SupportingDocuments = attachments;
			DataProvider.InwardMasterBill = "77777";
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardMasterBill = "5555";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			Message.BuildSupportingDocumentReference(TNPDEC);
			var supportingDocuments = TNPDEC.SupportingDocumentReference;
			AssertEquals("2 attached document elements should have been generated", 2, supportingDocuments.Length);
			AssertEquals("DocType001", "001", supportingDocuments[0].DocumentID);
			AssertEquals("Attached document file name", "ATTDOC_0001", supportingDocuments[0].Filename);
			AssertEquals("DocType999", "999", supportingDocuments[1].DocumentID);
			AssertEquals("Attached document file name", "ATTDOC_0002", supportingDocuments[1].Filename);
		}

		public void TestBuildStrategicGoodsElements()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			ProductCodesTestClass[] cASC = new ProductCodesTestClass[1];
			items[0].ProductCodes = cASC;
			items[0].IsStrategic = true;
			items[0].CategoryCode = "ML6";
			items[0].EndUseDescription = "TO AID IN ACCESSING INACCESSABLE TERRAIN DURING HUMANITARIAN ASSISTANCE";
			items[0].EndUseCode1 = CA_SC1CodeList.Codes.MIL;
			items[0].EndUseCode2 = CA_SC2CodeList.Codes.GOV;
			items[0].EndUseCode3 = CA_SC3CodeList.Codes.NMD;
			DataProvider.Items = items;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.InwardTransportIdentifier = "IV2232324";
			DataProvider.InwardJourneyIdentifier = "220E";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTF;
			DataProvider.OutwardJourneyIdentifier = "773S";
			Message.BuildItem(TNPDEC);
			var item1 = TNPDEC.Item[0];
			var cascProduct = item1.CASCProduct[0];
			AssertEquals("Strategic goods declaration - product code", "ML6", cascProduct.CASCProductCode);
			AssertEquals("MIL", cascProduct.AdditionalCASCIdentification[0].CASCCodeOne);
			AssertEquals("GOV", cascProduct.AdditionalCASCIdentification[0].CASCCodeTwo);
			AssertEquals("NMD", cascProduct.AdditionalCASCIdentification[0].CASCCodeThree);
			AssertEquals("TO AID IN ACCESSING INACCESSABLE TERRAIN DURING HUMANITARIAN ASSISTANCE", cascProduct.EndUseDescription.EndUseLine);
		}

		public void TestMessageTypeAndSubType()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.TNPDEC, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Declaration, Message.MessageSubType);
		}

		TNPDEC Message => message ?? (message = new TNPDEC(DataProvider));
		TNPDEC message;
		TNPDECTestClass DataProvider => dataProvider ?? (dataProvider = new TNPDECTestClass());
		TNPDECTestClass dataProvider;
		TranshipmentMovement TNPDEC
		{
			get
			{
				if (tnpdecMessage == null)
				{
					var tradenetDeclaration = new TradenetDeclaration();
					var inboundMessage = tradenetDeclaration.InboundMessage = new TradenetDeclarationInboundMessage();
					tnpdecMessage = inboundMessage.TranshipmentMovement = new TranshipmentMovement();
				}

				return tnpdecMessage;
			}
		}

		TranshipmentMovement tnpdecMessage;
		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
		#region EntryHeader
		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
					fEntryHeader.Declaration.AdditionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
				}

				return fEntryHeader;
			}
		}

		CusEntryHeader fEntryHeader;
		#endregion
		#region Interface Objects
		ISGCUSDEC CusEntry
		{
			get
			{
				return EntryHeader;
			}
		}
		#endregion
	}
}
