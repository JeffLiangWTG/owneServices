using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	public class BaseTradeNetMessageTest : TestCaseWithFactory
	{
		public void TestCusMessage()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.INPDEC, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Declaration, Message.MessageSubType);
			AssertEquals(string.Empty, Message.MessageText);
		}

		public void TestMessageReference()
		{
			DataProvider.JobNumber = "B00001001";
			Message.BuildHeader();
			AssertEquals("WTGB00001001", Message.InNonPayment.Header.MessageReference);
			DataProvider.JobNumber = "SSIN090000168";
			Message.BuildHeader();
			AssertEquals("GSSIN090000168", Message.InNonPayment.Header.MessageReference);
			DataProvider.JobNumber = "DSSSIN09A0000168003T";
			Message.BuildHeader();
			AssertEquals("09A0000168003T", Message.InNonPayment.Header.MessageReference);
		}

		public void TestUniqueReferenceNumber()
		{
			Message.BuildHeader();
			var uniqueReferenceNumber = Message.InNonPayment.Header.UniqueReferenceNumber;
			AssertEquals(SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml, uniqueReferenceNumber.Date);
			AssertEquals(SGXmlEDIMessage.MessageNumberPlaceHolderXml, uniqueReferenceNumber.ID);
			AssertEquals(SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml, uniqueReferenceNumber.SequenceNumeric);
		}

		public void TestCargoPackingType()
		{
			DataProvider.CargoPackingType = CargoPackingCodeList.Codes.PackingType9;
			Message.BuildCargo();
			AssertEquals(CargoPackingCodeList.Codes.PackingType9, Message.InNonPayment.Cargo.CargoPackingType);
		}

		public void TestPort()
		{
			DataProvider.PortOfLoading = "AUSYD";
			DataProvider.PortOfDischarge = "JPTYO";
			DataProvider.HasInwardTransport = true;
			DataProvider.HasOutwardTransport = true;
			Message.BuildTransport();
			AssertEquals("AUSYD", Message.InNonPayment.Transport.InwardTransport.LoadingPort);
			AssertEquals("JPTYO", Message.InNonPayment.Transport.OutwardTransport.DischargePort);
		}

		public void TestReleaseLocation()
		{
			SGCPlaceTestClass CreatePlace(string code, string type, string nameAndAddress, bool isNonSystemNonLicenced = false)
			{
				return new SGCPlaceTestClass { AddressRequired = nameAndAddress.Length > 0, Code = code, NameAndAddress = nameAndAddress, Type = type, IsNonSystemNonLicenced = isNonSystemNonLicenced };
			}

			DataProvider.PlaceOfRelease = CreatePlace("VEN001", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "DUTIABLE LIQUOR LIC. WAREHOUSE LIC W/H ADDR1");
			Message.BuildCargo();
			var location = Message.InNonPayment.Cargo.ReleaseLocation;
			AssertEquals("VEN001", location.LocationCode);
			AssertEquals("DUTIABLE LIQUOR LIC. WAREHOUSE LIC W/H ADDR1", location.LocationName);
			DataProvider.PlaceOfRelease = CreatePlace("RSYC", SGCPlaces.Constants.PremiseType.SailingClub, "Royal Singapore Yacht Club addr1", true);
			Message.BuildCargo();
			location = Message.InNonPayment.Cargo.ReleaseLocation;
			AssertEquals("SC", location.LocationCode);
			AssertEquals("ROYAL SINGAPORE YACHT CLUB ADDR1", location.LocationName);
			DataProvider.PlaceOfRelease = CreatePlace("AT1B", "AT1B Type", "");
			Message.BuildCargo();
			location = Message.InNonPayment.Cargo.ReleaseLocation;
			AssertEquals("AT1B", location.LocationCode);
			AssertNull("Should not output when it's empty.", location.LocationName);
		}

		public void TestArrivalDate()
		{
			DataProvider.ArrivalDate = new ZDate(2011, 05, 10);
			DataProvider.HasInwardTransport = false;
			Message.BuildTransport();
			AssertNull(Message.InNonPayment.Transport);
			DataProvider.HasInwardTransport = true;
			Message.BuildTransport();
			AssertEquals("20110510", Message.InNonPayment.Transport.InwardTransport.ArrivalDate);
		}

		public void TestStartDate()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.BKT;
			DataProvider.StartDateOfBlanket = new ZDate(2011, 05, 19);
			Message.BuildCargo();
			AssertNull("Should default to null.", Message.InNonPayment.Cargo.ExhibitionTemporaryImportPeriod);
		}

		public void TestNetRegisterTonnage()
		{
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsOutwardDeclaration = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.OutwardVesselNRT = 35000;
			Message.BuildTransport();
			AssertEquals(35000m, Message.InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation.NetRegisterTonnage);
			Assert(Message.InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation.NetRegisterTonnageSpecified);
		}

		public void TestOutwardVesselType()
		{
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsOutwardDeclaration = true;
			DataProvider.HasOutwardTransport = true;
			DataProvider.OutwardTransportIdentifier = "China Star";
			DataProvider.OutwardJourneyIdentifier = "29E";
			DataProvider.OutwardVesselType = "CV";
			DataProvider.OutwardVesselNRT = 0;
			DataProvider.OutwardVesselNationality = "KR";
			Message.BuildTransport();
			AssertEquals("Outward ConveyanceReferenceNumber - voyage number", "29E", Message.InNonPayment.Transport.OutwardTransport.TransportMeans.TransportMode.ConveyanceReferenceNumber);
			AssertEquals("Outward TransportIdentifier - vessel name", "CHINA STAR", Message.InNonPayment.Transport.OutwardTransport.TransportMeans.TransportMode.TransportIdentifier);
			AssertEquals("Outward VesselType", "CV", Message.InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation.VesselType);
			AssertEquals(0m, Message.InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation.NetRegisterTonnage);
			AssertEquals(false, Message.InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation.NetRegisterTonnageSpecified);
			AssertEquals("Outward VesselNationality", "KR", Message.InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation.VesselNationality);
		}

		public void TestSummary()
		{
			DataProvider.TotalOuterPack = 100;
			DataProvider.TotalOuterPackUnitOfQty = "BOX";
			DataProvider.TotalGrossWeight = 111;
			DataProvider.TotalGrossWeightUnitOfQty = "T";
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(100.0000m, Message.InNonPayment.Summary.TotalOuterPack.Value);
				AssertEquals("BOX", Message.InNonPayment.Summary.TotalOuterPack.unitCode);
				AssertEquals(111000m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("KGM", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.TotalOuterPack = 50;
			DataProvider.TotalOuterPackUnitOfQty = "CTN";
			DataProvider.TotalGrossWeight = 1750;
			DataProvider.TotalGrossWeightUnitOfQty = "KG";
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(50.0000m, Message.InNonPayment.Summary.TotalOuterPack.Value);
				AssertEquals("CTN", Message.InNonPayment.Summary.TotalOuterPack.unitCode);
				AssertEquals(1750m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("KGM", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.IsSea = false;
			DataProvider.OutwardTransportCode = 0;
			DataProvider.TotalOuterPack = 244;
			DataProvider.TotalOuterPackUnitOfQty = "PLT";
			DataProvider.TotalGrossWeight = 1750;
			DataProvider.TotalGrossWeightUnitOfQty = "KG";
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(244.0000m, Message.InNonPayment.Summary.TotalOuterPack.Value);
				AssertEquals("PLT", Message.InNonPayment.Summary.TotalOuterPack.unitCode);
				AssertEquals(1750m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("KGM", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
		}

		public void TestWeightInRequiredUQ()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DUT;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.IsInwardDeclaration = true;
			DataProvider.TotalOuterPack = 100;
			DataProvider.TotalOuterPackUnitOfQty = "BOX";
			DataProvider.TotalGrossWeight = 1.3759;
			DataProvider.TotalGrossWeightUnitOfQty = "T";
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(100m, Message.InNonPayment.Summary.TotalOuterPack.Value);
				AssertEquals("BOX", Message.InNonPayment.Summary.TotalOuterPack.unitCode);
				AssertEquals("TradeNet 4.1 requires gross weight to 3 decimal places", 1375.900m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("KGM", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals("TradeNet 4.1 requires gross weight to 3 decimal places", 1.376m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("Inward Sea Declaration - Weight must be in TNE", "TNE", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.DRT;
			DataProvider.InwardTransportCode = 0;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.IsInwardDeclaration = false;
			DataProvider.IsOutwardDeclaration = true;
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(1375.900m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("Outward Air Declaration - Weight must be in KGM", "KGM", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(1.376m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("Outward Sea Declaration - Weight must be in TNE", "TNE", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TTI;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Air;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsInwardDeclaration = false;
			DataProvider.IsOutwardDeclaration = false;
			DataProvider.IsTranshipmentDeclaration = true;
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(1375.900m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("Transhipment Declaration with Inward Leg being Air - Weight must be in KGM", "KGM", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(1.376m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("Transhipment Declaration with Inward Leg being Sea - Weight must be in TNE", "TNE", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
			DataProvider.TotalGrossWeightUnitOfQty = "XXX";
			Message.BuildSummary();
			CombineAssertions(() =>
			{
				AssertEquals(1.376m, Message.InNonPayment.Summary.TotalGrossWeight.Value);
				AssertEquals("Weight unit should not be converted if invalid.", "XXX", Message.InNonPayment.Summary.TotalGrossWeight.unitCode);
			}

			);
		}

		public void TestClaimantParty()
		{
			var testClaimant = new OrganisationTestClass();
			testClaimant.Address = new OrganisationAddressTestClass("Org Address");
			testClaimant.Name = "CLAIMANTCOMPANYNAME";
			testClaimant.UEN = "197201770G";
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCO;
			DataProvider.ClaimantCode = "CLAIMANTCODE";
			DataProvider.ClaimantName = "CLAIMANTNAME";
			DataProvider.Claimant = testClaimant;
			Message.BuildParty();
			CombineAssertions(() =>
			{
				var claimantParty = Message.InNonPayment.Party.ClaimantParty;
				AssertEquals("197201770G", claimantParty.PartyDetail.PartyIdentification.ID);
				AssertEquals("CLAIMANTCODE", claimantParty.ClaimantInformation.CodeValue);
				AssertEquals("CLAIMANTNAME", claimantParty.ClaimantInformation.Name);
				AssertArrayEqualsByElements(new[] { "CLAIMANTCOMPANYNAME" }, claimantParty.PartyDetail.PartyName);
			}

			);
		}

		public void TestPartyAddressExcludesAdditionalAddressInformation()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "HEPL";
			consignee.OH_FullName = "Handmade Enterprises Pte. Ltd.";
			var mainAddress = consignee.Addresses[0];
			mainAddress.OA_Address1 = "AVENUE KING CHARLES II";
			mainAddress.OA_Address2 = "";
			mainAddress.PrimaryOrgAddressAdditionalInfoDetail = "C/- JOHN SMITH, FIRST FLOOR";
			mainAddress.OA_City = "Singapore";
			mainAddress.OA_PostCode = "59200";
			Factory.Save();
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCO;
			DataProvider.Consignee = new EntryOrganisationsInfo(consignee);
			Message.BuildParty();
			var consigneeParty = Message.InNonPayment.Party.ConsigneeParty;
			AssertEquals("HANDMADE ENTERPRISES PTE. LTD.", consigneeParty.PartyName[0]);
			AssertEquals("C/- JOHN SMITH, FIRST FLOOR AVENUE", consigneeParty.Address.AddressLine[0]);
			AssertEquals("KING CHARLES II SINGAPORE 59200", consigneeParty.Address.AddressLine[1]);
			mainAddress.OA_Address1 = "AVENUE KING CHARLES II, SPECIAL DIPLOMATIC AREA";
			mainAddress.OA_Address2 = "SOUTH CENTRAL HARBOURSIDE";
			Factory.Save();
			Message.BuildParty();
			consigneeParty = Message.InNonPayment.Party.ConsigneeParty;
			AssertEquals("HANDMADE ENTERPRISES PTE. LTD.", consigneeParty.PartyName[0]);
			AssertEquals("Long address should not include Additional Address Information when Address exceeds message capacity", "AVENUE KING CHARLES II, SPECIAL DIP", consigneeParty.Address.AddressLine[0]);
			AssertEquals("Long address", "LOMATIC AREA SOUTH CENTRAL HARBOURS", consigneeParty.Address.AddressLine[1]);
		}

		public void TestExporter()
		{
			var testExporter = new OrganisationTestClass();
			testExporter.Name = "C & A MEXICO S. DE R. L.";
			testExporter.Address = new OrganisationAddressTestClass("AV.CAMINO AL ITESO No.8350", "MEXICO R.F.C CME-", "961203-360", "MX", "");
			testExporter.UEN = "197201770G";
			DataProvider.IsOutwardDeclaration = true;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.APS;
			DataProvider.Exporter = testExporter;
			Message.BuildParty();
			CombineAssertions(() =>
			{
				var exporterParty = Message.InNonPayment.Party.ExporterParty;
				AssertEquals("197201770G", exporterParty.PartyDetail.PartyIdentification.ID);
				AssertArrayEqualsByElements(new[] { "C & A MEXICO S. DE R. L." }, exporterParty.PartyDetail.PartyName);
				AssertNull(exporterParty.Address);
				AssertNull(exporterParty.AddressLine);
			}

			);
		}

		public void TestImporterNameOverrideWithLongName()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "HYDRO ALUMINIUM MALAYSIA SDN BHD";
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "197201770G");
			var importerNameOverride = "HYDRO ALUMINIUM MALAYSIA SDN BHD C/O OIA GLOBAL LOGISTICS (S) PTE LTD";
			var testImporter = new EntryOrganisationsInfo(importer, importerNameOverride);
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SFZ;
			DataProvider.Importer = testImporter;
			Message.BuildParty();
			CombineAssertions(() =>
			{
				var importerParty = Message.InNonPayment.Party.ImporterParty;
				AssertEquals("197201770G", importerParty.PartyIdentification.ID);
				AssertArrayEqualsByElements(new[] { "HYDRO ALUMINIUM MALAYSIA SDN BHD C/", "O OIA GLOBAL LOGISTICS (S) PTE LTD" }, importerParty.PartyName);
			}

			);
		}

		public void TestTransportEquipment()
		{
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
			Message.BuildCargo();
			var equipments = Message.InNonPayment.Cargo.TransportEquipment;
			AssertEquals(3, equipments.Length);
			CombineAssertions("ABCU0039477", () =>
			{
				var equipment = equipments.First(c => c.EquipmentID == "ABCU0039477");
				AssertEquals("1", equipment.SequenceNumeric);
				AssertEquals("FCL20", equipment.SizeTypeCode);
				AssertEquals(35m, equipment.EquipmentWeightMeasureNumeric);
				AssertEquals("NA", equipment.TransportEquipmentSeal.SealID);
			}

			);
			CombineAssertions("NYKU2188343", () =>
			{
				var equipment = equipments.First(c => c.EquipmentID == "NYKU2188343");
				AssertEquals("2", equipment.SequenceNumeric);
				AssertEquals("LCL40", equipment.SizeTypeCode);
				AssertEquals(120m, equipment.EquipmentWeightMeasureNumeric);
				AssertEquals("SHP1233445", equipment.TransportEquipmentSeal.SealID);
			}

			);
			CombineAssertions("NYKU4798228", () =>
			{
				var equipment = equipments.First(c => c.EquipmentID == "NYKU4798228");
				AssertEquals("3", equipment.SequenceNumeric);
				AssertEquals("LCL40", equipment.SizeTypeCode);
				AssertEquals(25m, equipment.EquipmentWeightMeasureNumeric);
				AssertEquals("XX-03948", equipment.TransportEquipmentSeal.SealID);
			}

			);
		}

		public void TestDeclarantPartyAndDeclaringAgentParty()
		{
			var testDeclarant = new AgentInfoTestClass();
			testDeclarant.Name = "Test SG Broker";
			testDeclarant.Phone = "64 85721111";
			testDeclarant.Code = "@v13t001";
			DataProvider.Declarant = testDeclarant;
			Message.BuildParty();
			CombineAssertions(() =>
			{
				var declarantParty = Message.InNonPayment.Party.DeclarantParty;
				AssertEquals("64 85721111", declarantParty.Telephone);
				AssertEquals(" V13T001", declarantParty.PersonInformation.CodeValue);
				AssertEquals("TEST SG BROKER", declarantParty.PersonInformation.Name);
				var declaringAgentParty = Message.InNonPayment.Party.DeclaringAgentParty;
				AssertEquals(GlbCompany.CurrentCompany.GC_CustomsRegistrationNo, declaringAgentParty.PartyIdentification.ID);
				AssertArrayEqualsByElements(new string[] { GlbCompany.CurrentCompany.GC_Name.ToUpperInvariant() }, declaringAgentParty.PartyName);
			}

			);
		}

		public void TestSupportingDocumentReference()
		{
			var attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass();
			attachments[0].FileName = "AttDOC_0001 ÀÃÆŒÈ";
			attachments[0].DocType = SupportingDocumentTypeCodeList.Codes.DocType001;
			attachments[1] = new AttachmentsTestClass();
			attachments[1].FileName = "AttDOC_0002 ËÕÑÜÝ";
			attachments[1].DocType = SupportingDocumentTypeCodeList.Codes.DocType002;
			var addInfo = (SGCUSDECTestClass.ImplementsAdditionalMessageInformation)DataProvider.AdditionalMessageInformation;
			addInfo.SupportingDocuments = attachments;
			Message.BuildSupportingDocumentReference();
			CombineAssertions(() =>
			{
				var references = Message.InNonPayment.SupportingDocumentReference;
				var idAndNames = references.Select(c => $"{c.DocumentID}:{c.Filename}").ToArray();
				AssertArrayEqualsByElements(new[] { "001:ATTDOC_0001 AAAEOEE", "002:ATTDOC_0002 EONUY" }, idAndNames);
			}

			);
		}

		public void TestConvertToSingaporeCustomsRequiredWeightUnit()
		{
			var message = new TestTradeNetMessage(DataProvider);
			var sourceWeightInPounds = 10m;
			var (resultWeight, resultWeightUnit) = message.ConvertToSingaporeCustomsRequiredWeightUnit(sourceWeightInPounds, Core.Constants.Weight.Pounds, SGConstants.Weight.Kilograms);
			var expectedWeight = Core.Constants.Weight.Convert(sourceWeightInPounds, Core.Constants.Weight.Pounds, Core.Constants.Weight.Kilograms);
			AssertEquals(expectedWeight, resultWeight);
			AssertEquals(SGConstants.Weight.Kilograms, resultWeightUnit);
			var sourceWeightInShortTonnes = 100m;
			(resultWeight, resultWeightUnit) = message.ConvertToSingaporeCustomsRequiredWeightUnit(sourceWeightInShortTonnes, Core.Constants.Weight.ShortTons, SGConstants.Weight.Tonnes);
			expectedWeight = Core.Constants.Weight.Convert(sourceWeightInShortTonnes, Core.Constants.Weight.ShortTons, Core.Constants.Weight.Tonnes);
			AssertEquals(expectedWeight, resultWeight);
			AssertEquals(SGConstants.Weight.Tonnes, resultWeightUnit);
			var sourceWeightInInvalidUnit = 1000m;
			(resultWeight, resultWeightUnit) = message.ConvertToSingaporeCustomsRequiredWeightUnit(sourceWeightInInvalidUnit, "XXX", SGConstants.Weight.Kilograms);
			AssertEquals(sourceWeightInInvalidUnit, resultWeight);
			AssertEquals("XXX", resultWeightUnit);
		}

		public void TestBrandNameDefaultsToUNBRANDEDForImportDecs()
		{
			var message = new TestTradeNetMessage(DataProvider);
			DataProvider.IsImport = true;
			DataProvider.Items = new[] { new ItemsTestClass()
			{ BrandName = ZString.Empty } };
			message.BuildItem();
			AssertEquals(SGConstants.Unbranded, message.InNonPayment.Item[0].BrandName);
		}

		public void TestGoodsDescription()
		{
			var message = new TestTradeNetMessage(DataProvider);
			var items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].GoodsDescription = @"HANKOOK HORIZONTAL CNC LATHE
PROTURN 60BX1500 WITH ACCESSORIES";
			DataProvider.Items = items;
			message.BuildItem();
			AssertEquals("HANKOOK HORIZONTAL CNC LATHE PROTURN 60BX1500 WITH ACCESSORIES", message.InNonPayment.Item[0].GoodsDescription);
		}

		public void TestMarksAndNumbers()
		{
			var message = new TestTradeNetMessage(DataProvider);
			var items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].MarksAndNumbers = @"Note 2: Specify markings on cargo for marks & numbers, if any. Repeat at most 4 times and specify:
1st occurrence: 10 lines x 17 = 170 characters
2nd occurrence: 10 lines x 17 = 170 characters
3rd occurrence: 8 lines x 17 = 136 characters
4th occurrence: 3 lines x 12 = 36 characters.
This specification provides the definition of the In-Non-Payment/Customs Declaration ie. CUSDEC (INPDEC) message to be used in Electronic Data Interchange (EDI) between trading partners involved in Administration, commercial";
			DataProvider.Items = items;
			message.BuildItem();
			var shippingMarks = message.InNonPayment.Item[0].ShippingMarksInformation;
			AssertEquals("ShippingMarks - line 1", "NOTE 2: SPECIFY M", shippingMarks[0].ShippingMarks[0]);
			AssertEquals("ShippingMarks - line 2", "ARKINGS ON CARGO ", shippingMarks[0].ShippingMarks[1]);
			AssertEquals("ShippingMarks - line 3", "FOR MARKS & NUMBE", shippingMarks[0].ShippingMarks[2]);
			AssertEquals("ShippingMarks - line 4", "RS, IF ANY. REPEA", shippingMarks[0].ShippingMarks[3]);
			AssertEquals("ShippingMarks - line 5", "T AT MOST 4 TIMES", shippingMarks[0].ShippingMarks[4]);
			AssertEquals("ShippingMarks - line 6", " AND SPECIFY: 1ST", shippingMarks[0].ShippingMarks[5]);
			AssertEquals("ShippingMarks - line 7", " OCCURRENCE: 10 L", shippingMarks[0].ShippingMarks[6]);
			AssertEquals("ShippingMarks - line 8", "INES X 17 = 170 C", shippingMarks[0].ShippingMarks[7]);
			AssertEquals("ShippingMarks - line 9", "HARACTERS 2ND OCC", shippingMarks[0].ShippingMarks[8]);
			AssertEquals("ShippingMarks - line 10", "URRENCE: 10 LINES", shippingMarks[0].ShippingMarks[9]);
			AssertEquals("ShippingMarks - line 11", " X 17 = 170 CHARA", shippingMarks[1].ShippingMarks[0]);
			AssertEquals("ShippingMarks - line 12", "CTERS 3RD OCCURRE", shippingMarks[1].ShippingMarks[1]);
			AssertEquals("ShippingMarks - line 13", "NCE: 8 LINES X 17", shippingMarks[1].ShippingMarks[2]);
			AssertEquals("ShippingMarks - line 14", " = 136 CHARACTERS", shippingMarks[1].ShippingMarks[3]);
			AssertEquals("ShippingMarks - line 15", " 4TH OCCURRENCE: ", shippingMarks[1].ShippingMarks[4]);
			AssertEquals("ShippingMarks - line 16", "3 LINES X 12 = 36", shippingMarks[1].ShippingMarks[5]);
			AssertEquals("ShippingMarks - line 17", " CHARACTERS. THIS", shippingMarks[1].ShippingMarks[6]);
			AssertEquals("ShippingMarks - line 18", " SPECIFICATION PR", shippingMarks[1].ShippingMarks[7]);
			AssertEquals("ShippingMarks - line 19", "OVIDES THE DEFINI", shippingMarks[1].ShippingMarks[8]);
			AssertEquals("ShippingMarks - line 20", "TION OF THE ", shippingMarks[1].ShippingMarks[9]);
			AssertEquals("ShippingMarks - line 21", "IN-NON-PAYMENT/CU", shippingMarks[2].ShippingMarks[0]);
			AssertEquals("ShippingMarks - line 22", "STOMS DECLARATION", shippingMarks[2].ShippingMarks[1]);
			AssertEquals("ShippingMarks - line 23", " IE. CUSDEC (INPD", shippingMarks[2].ShippingMarks[2]);
			AssertEquals("ShippingMarks - line 24", "EC) MESSAGE TO BE", shippingMarks[2].ShippingMarks[3]);
			AssertEquals("ShippingMarks - line 25", " USED IN ELECTRON", shippingMarks[2].ShippingMarks[4]);
			AssertEquals("ShippingMarks - line 26", "IC DATA INTERCHAN", shippingMarks[2].ShippingMarks[5]);
			AssertEquals("ShippingMarks - line 27", "GE (EDI) BETWEEN ", shippingMarks[2].ShippingMarks[6]);
			AssertEquals("ShippingMarks - line 28", "TRADING PARTNERS ", shippingMarks[2].ShippingMarks[7]);
			AssertEquals("ShippingMarks - line 29", "INVOLVED IN", shippingMarks[3].ShippingMarks[0]);
			AssertEquals("ShippingMarks - line 30", "ADMINISTRATI", shippingMarks[3].ShippingMarks[1]);
			AssertEquals("ShippingMarks - line 31", "ON, COMMERCI", shippingMarks[3].ShippingMarks[2]);
		}

		public void TestTransactionValue()
		{
			var message = new TestTradeNetMessage(DataProvider);
			var items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].CustomsValue = 17500m;
			items[0].UnitPrice = 15000m;
			items[0].LSPValue = 720m;
			items[0].InvoiceCurrency = "SGD";
			items[0].IsMotorVehicle = false;
			DataProvider.Items = items;
			message.BuildItem();
			CombineAssertions(() =>
			{
				var transactionValue = message.InNonPayment.Item[0].TransactionValue;
				AssertEquals(17500m, transactionValue.ItemCIFFOBValue);
				Assert(transactionValue.ItemCIFFOBValueSpecified);
				AssertEquals(720m, transactionValue.LastSellingPriceValue);
				Assert(transactionValue.LastSellingPriceValueSpecified);
				AssertNull("transactionValue.UnitPriceValue", transactionValue.UnitPriceValue);
			}

			);
			items[0].IsMotorVehicle = true;
			message.BuildItem();
			CombineAssertions(() =>
			{
				var transactionValue = message.InNonPayment.Item[0].TransactionValue;
				AssertEquals(15000m, transactionValue.UnitPriceValue.Amount.Value);
				AssertEquals("SGD", transactionValue.UnitPriceValue.Amount.currencyID);
			}

			);
		}

		public void TestCASCProduct()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].IsMotorVehicle = true;
			var cASC = new ProductCodesTestClass[1];
			cASC[0] = new ProductCodesTestClass();
			cASC[0].ProductCode = "ProdCode_00001";
			cASC[0].ProductCodeQty = 3.5;
			cASC[0].ProductCodeUnitType = UnitOfQuantityCodeList.Codes.TNE;
			items[0].ProductCodes = cASC;
			DataProvider.Items = items;
			Message.BuildItem();
			var product = Message.InNonPayment.Item[0].CASCProduct[0];
			CombineAssertions(() =>
			{
				AssertEquals(3.5m, product.CASCProductQuantity.Value);
				AssertEquals("TNE", product.CASCProductQuantity.unitCode);
				AssertEquals("ProdCode_00001", product.CASCProductCode);
			}

			);
		}

		public void TestMotorVehicle()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].IsMotorVehicle = true;
			items[0].DateOfFirstRegistration = new ZDate(2005, 11, 23);
			items[0].EngineCapacity = 4800m;
			items[0].EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			DataProvider.Items = items;
			Message.IsSupportingRegistrationDateSection = true;
			Message.BuildItem();
			var motorVehicle = Message.InNonPayment.Item[0].MotorVehicle;
			CombineAssertions(() =>
			{
				AssertEquals(4800.00m, motorVehicle.EngineCapacity.Value);
				AssertEquals("CC", motorVehicle.EngineCapacity.unitCode);
				AssertEquals("20051123", motorVehicle.OriginalRegistrationDate);
			}

			);
			Message.IsSupportingRegistrationDateSection = false;
			Message.BuildItem();
			motorVehicle = Message.InNonPayment.Item[0].MotorVehicle;
			AssertNull(motorVehicle.OriginalRegistrationDate);
		}

		public void TestAdditionalCASCIdentification()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var testCodes1 = new CASCCode1Collection(invoiceLine);
			var chemicalPurity = Factory.New<CASCCode1>();
			chemicalPurity.CY_Data = "5";
			testCodes1.Add(chemicalPurity);
			items[0].CASCCodes1 = testCodes1;
			var testCodes2 = new CASCCode2Collection(invoiceLine);
			var maxQty = Factory.New<CASCCode2>();
			maxQty.CY_Data = "50";
			testCodes2.Add(maxQty);
			items[0].CASCCodes2 = testCodes2;
			items[0].ProductCodes = new ProductCodesTestClass[] { new ProductCodesTestClass { ProductCode = "TEST000" } };
			DataProvider.Items = items;
			Message.BuildItem();
			var additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[0];
			CombineAssertions(() =>
			{
				AssertEquals("5", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("50", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			}

			);
		}

		public void TestAdditionalCASCCodesOutputIndependantOfProductCode()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var testCodes1 = new CASCCode1Collection(invoiceLine);
			var chemicalPurity = Factory.New<CASCCode1>();
			chemicalPurity.CY_Data = "5";
			testCodes1.Add(chemicalPurity);
			items[0].CASCCodes1 = testCodes1;
			var testCodes2 = new CASCCode2Collection(invoiceLine);
			var maxQty = Factory.New<CASCCode2>();
			maxQty.CY_Data = "50";
			testCodes2.Add(maxQty);
			items[0].CASCCodes2 = testCodes2;
			DataProvider.Items = items;
			Message.BuildItem();
			var additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[0];
			CombineAssertions(() =>
			{
				AssertEquals("CASC code values should output into the message independant of the Product Code", "5", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("CASC code values should output into the message independant of the Product Code", "50", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			}

			);
		}

		public void TestIndependantCASCCodes()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var testCodes1 = new CASCCode1Collection(invoiceLine);
			var thymidineMaterials = Factory.New<CASCCode1>();
			thymidineMaterials.CY_Data = "OTHERS";
			testCodes1.Add(thymidineMaterials);
			items[0].CASCCodes1 = testCodes1;
			DataProvider.Items = items;
			Message.BuildItem();
			var additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[0];
			CombineAssertions(() =>
			{
				AssertEquals("CASC code values should output into the message independant of the Product Code", "OTHERS", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			}

			);
		}

		public void TestCASCCodeLength()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var testCodes1 = new CASCCode1Collection(invoiceLine);
			var chemicalPurity = Factory.New<CASCCode1>();
			chemicalPurity.CY_Data = "1234567890123456789A123456789B12345";
			testCodes1.Add(chemicalPurity);
			items[0].CASCCodes1 = testCodes1;
			var testCodes2 = new CASCCode2Collection(invoiceLine);
			var maxQty = Factory.New<CASCCode2>();
			maxQty.CY_Data = "THIS FIELD SHOULD ONLY ALLOW 35 CHARACTERS IN THE CUSTOMS MESSAGE";
			testCodes2.Add(maxQty);
			items[0].CASCCodes2 = testCodes2;
			items[0].ProductCodes = new ProductCodesTestClass[] { new ProductCodesTestClass { ProductCode = "TEST000" } };
			DataProvider.Items = items;
			Message.BuildItem();
			var additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[0];
			CombineAssertions(() =>
			{
				AssertEquals("1234567890123456789A123456789B12345", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("THIS FIELD SHOULD ONLY ALLOW 35 CHA", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			}

			);
		}

		public void TestCASCCodesPopulateIntoMessageInCY_OrderSequence()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var code1 = invoiceLine.CASCCode1s.AddNew();
			code1.CY_Data = "First CA/SC code entered";

			var code2 = invoiceLine.CASCCode1s.AddNew();
			code2.CY_Data = "Second code entered";

			var code3 = invoiceLine.CASCCode1s.AddNew();
			code3.CY_Data = "Third code";

			var code4 = invoiceLine.CASCCode1s.AddNew();
			code4.CY_Data = "Fourth code";

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition - CA/SC entered code 1", (ZShort)1, code1.CY_Order);
				AssertEquals("Pre-condition - CA/SC code 1", "First CA/SC code entered", code1.CY_Data);
				AssertEquals("Pre-condition - CA/SC entered code 2", (ZShort)2, code2.CY_Order);
				AssertEquals("Pre-condition - CA/SC code 2", "Second code entered", code2.CY_Data);
				AssertEquals("Pre-condition - CA/SC entered code 3", (ZShort)3, code3.CY_Order);
				AssertEquals("Pre-condition - CA/SC code 3", "Third code", code3.CY_Data);
				AssertEquals("Pre-condition - CA/SC entered code 4", (ZShort)4, code4.CY_Order);
				AssertEquals("Pre-condition - CA/SC code 4", "Fourth code", code4.CY_Data);
			});

			//Re-order codes from their entered sequence
			code2.CY_Order = 1;
			code4.CY_Order = 2;
			code1.CY_Order = 3;
			code3.CY_Order = 4;

			items[0].CASCCodes1 = invoiceLine.CASCCode1s;

			var testCodes2 = new CASCCode2Collection(invoiceLine);
			var maxQty = Factory.New<CASCCode2>();
			maxQty.CY_Data = "50";
			testCodes2.Add(maxQty);
			items[0].CASCCodes2 = testCodes2;
			items[0].ProductCodes = new ProductCodesTestClass[] { new ProductCodesTestClass { ProductCode = "TEST000" } };
			DataProvider.Items = items;
			Message.BuildItem();
			var additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[0];
			CombineAssertions(() =>
			{
				AssertEquals("Second code entered", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("50", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[1];
			CombineAssertions(() =>
			{
				AssertEquals("Fourth code", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[2];
			CombineAssertions(() =>
			{
				AssertEquals("First CA/SC code entered", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[3];
			CombineAssertions(() =>
			{
				AssertEquals("Third code", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});
		}

		public void TestCASCCodes2PopulateInSequence()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cascCode1_1 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_1.CY_Data = "CASC1 needed to access CS/SC code 2";
			var cascCode1_2 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_2.CY_Data = "Dummy CA/SC code1 required";
			var cascCode1_3 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_3.CY_Data = "Dummy CA/SC code1 required";
			var cascCode1_4 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_4.CY_Data = "Dummy CA/SC code1 required";

			var code1 = invoiceLine.CASCCode2s.AddNew();
			code1.CY_Data = "First CA/SC code2 entered";

			var code2 = invoiceLine.CASCCode2s.AddNew();
			code2.CY_Data = "Second code2 entered";

			var code3 = invoiceLine.CASCCode2s.AddNew();
			code3.CY_Data = "Third code2";

			var code4 = invoiceLine.CASCCode2s.AddNew();
			code4.CY_Data = "Fourth code2";

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition - CA/SC 2 entered code 1", (ZShort)1, code1.CY_Order);
				AssertEquals("Pre-condition - CA/SC 2 code 1", "First CA/SC code2 entered", code1.CY_Data);
				AssertEquals("Pre-condition - CA/SC 2 entered code 2", (ZShort)2, code2.CY_Order);
				AssertEquals("Pre-condition - CA/SC 2 code 2", "Second code2 entered", code2.CY_Data);
				AssertEquals("Pre-condition - CA/SC 2 entered code 3", (ZShort)3, code3.CY_Order);
				AssertEquals("Pre-condition - CA/SC 2 code 3", "Third code2", code3.CY_Data);
				AssertEquals("Pre-condition - CA/SC 2 entered code 4", (ZShort)4, code4.CY_Order);
				AssertEquals("Pre-condition - CA/SC 2 code 4", "Fourth code2", code4.CY_Data);
			});

			//Re-order CA/SC codes 2 from their entered sequence
			code3.CY_Order = 1;
			code2.CY_Order = 2;
			code4.CY_Order = 3;
			code1.CY_Order = 4;

			items[0].CASCCodes1 = invoiceLine.CASCCode1s;
			items[0].CASCCodes2 = invoiceLine.CASCCode2s;
			items[0].ProductCodes = new ProductCodesTestClass[] { new ProductCodesTestClass { ProductCode = "TEST000" } };
			DataProvider.Items = items;
			Message.BuildItem();
			var additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[0];
			CombineAssertions(() =>
			{
				AssertEquals("CASC1 needed to access CS/SC code 2", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("Third code2", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[1];
			CombineAssertions(() =>
			{
				AssertEquals("Dummy CA/SC code1 required", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("Second code2 entered", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[2];
			CombineAssertions(() =>
			{
				AssertEquals("Dummy CA/SC code1 required", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("Fourth code2", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[3];
			CombineAssertions(() =>
			{
				AssertEquals("Dummy CA/SC code1 required", additionalCASCIdentification.CASCCodeOne);
				AssertEquals("First CA/SC code2 entered", additionalCASCIdentification.CASCCodeTwo);
				AssertNull(additionalCASCIdentification.CASCCodeThree);
			});
		}

		public void TestCASCCodes3PopulateInSequence()
		{
			var items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cascCode1_1 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_1.CY_Data = "CASC1 needed to access CS/SC code 3";
			var cascCode1_2 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_2.CY_Data = "Dummy CA/SC code1 required";
			var cascCode1_3 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_3.CY_Data = "Dummy CA/SC code1 required";
			var cascCode1_4 = invoiceLine.CASCCode1s.AddNew();
			cascCode1_4.CY_Data = "Dummy CA/SC code1 required";

			var code1 = invoiceLine.CASCCode3s.AddNew();
			code1.CY_Data = "First CA/SC code3 entered";

			var code2 = invoiceLine.CASCCode3s.AddNew();
			code2.CY_Data = "Second code3 entered";

			var code3 = invoiceLine.CASCCode3s.AddNew();
			code3.CY_Data = "Third code3";

			var code4 = invoiceLine.CASCCode3s.AddNew();
			code4.CY_Data = "Fourth code3";

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition - CA/SC 3 entered code 1", (ZShort)1, code1.CY_Order);
				AssertEquals("Pre-condition - CA/SC 3 code 1", "First CA/SC code3 entered", code1.CY_Data);
				AssertEquals("Pre-condition - CA/SC 3 entered code 2", (ZShort)2, code2.CY_Order);
				AssertEquals("Pre-condition - CA/SC 3 code 2", "Second code3 entered", code2.CY_Data);
				AssertEquals("Pre-condition - CA/SC 3 entered code 3", (ZShort)3, code3.CY_Order);
				AssertEquals("Pre-condition - CA/SC 3 code 3", "Third code3", code3.CY_Data);
				AssertEquals("Pre-condition - CA/SC 3 entered code 4", (ZShort)4, code4.CY_Order);
				AssertEquals("Pre-condition - CA/SC 3 code 4", "Fourth code3", code4.CY_Data);
			});

			//Re-order CA/SC codes 3 from their entered sequence
			code4.CY_Order = 1;
			code3.CY_Order = 2;
			code2.CY_Order = 3;
			code1.CY_Order = 4;

			items[0].CASCCodes1 = invoiceLine.CASCCode1s;
			items[0].CASCCodes3 = invoiceLine.CASCCode3s;
			items[0].ProductCodes = new ProductCodesTestClass[] { new ProductCodesTestClass { ProductCode = "TEST000" } };
			DataProvider.Items = items;
			Message.BuildItem();
			var additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[0];
			CombineAssertions(() =>
			{
				AssertEquals("CASC1 needed to access CS/SC code 3", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertEquals("Fourth code3", additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[1];
			CombineAssertions(() =>
			{
				AssertEquals("Dummy CA/SC code1 required", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertEquals("Third code3", additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[2];
			CombineAssertions(() =>
			{
				AssertEquals("Dummy CA/SC code1 required", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertEquals("Second code3 entered", additionalCASCIdentification.CASCCodeThree);
			});

			additionalCASCIdentification = Message.InNonPayment.Item[0].CASCProduct[0].AdditionalCASCIdentification[3];
			CombineAssertions(() =>
			{
				AssertEquals("Dummy CA/SC code1 required", additionalCASCIdentification.CASCCodeOne);
				AssertNull(additionalCASCIdentification.CASCCodeTwo);
				AssertEquals("First CA/SC code3 entered", additionalCASCIdentification.CASCCodeThree);
			});
		}

		public void TestRemarks()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "Edifact should handle characters needing the escape character, eg: colon's and apostrophe's" , "second remark" , "third remark" };
			Message.BuildHeader();
			var remarks = Message.InNonPayment.Header.Remarks;
			AssertArrayEqualsByElements("Only two lines of Traders Remarks are allowed by default", new string[] { "EDIFACT SHOULD HANDLE CHARACTERS NEEDING THE ESCAPE CHARACTER, EG: COLON'S AND APOSTROPHE'S", "SECOND REMARK" }, remarks);

			DataProvider.TradersRemarksForMessage = new ZString[] { "First Remark" };
			Message.BuildHeader();
			remarks = Message.InNonPayment.Header.Remarks;
			AssertArrayEqualsByElements("Only one line of Traders Remarks will be in message", new string[] { "FIRST REMARK" }, remarks);
		}

		public void TestPreferentialCode()
		{
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass { TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.LTR, DutyUnitRateUnit = SGConstants.LPA, DutyUnitRate = 48m, DutyAmount = 0, PreferenceIndicator = PreferentialIndicatorCodeList.Codes.PRF, } };
			Message.BuildItem();
			AssertEquals(PreferentialIndicatorCodeList.Codes.PRF, Message.InNonPayment.Item[0].Tariff.PreferentialCode);
		}

		public void TestTariffCustomsDuty()
		{
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass { TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.LTR, DutyUnitRateUnit = SGConstants.LPA, DutyUnitRate = 48m, DutyAmount = 15m } };
			Message.BuildItem();
			var customsDuty = Message.InNonPayment.Item[0].Tariff.CustomsDuty;
			CombineAssertions(() =>
			{
				AssertEquals(48m, customsDuty.DutyRate);
				AssertEquals(15m, customsDuty.DutyAmount);
				Assert(customsDuty.DutyRateSpecified);
				Assert(customsDuty.DutyAmountSpecified);
				AssertEquals(string.Empty, customsDuty.DutyRateUnit);
			}

			);
		}

		public void TestTariffExciseDuty()
		{
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass { TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.LTR, DutyRateUnit = SGConstants.LPA, ExciseUnitRate = 48m, ExciseAmount = 100 } };
			Message.BuildItem();
			var exciseDuty = Message.InNonPayment.Item[0].Tariff.ExciseDuty;
			CombineAssertions(() =>
			{
				AssertEquals(48m, exciseDuty.DutyRate);
				AssertEquals(100m, exciseDuty.DutyAmount);
				Assert(exciseDuty.DutyRateSpecified);
				Assert(exciseDuty.DutyAmountSpecified);
				AssertEquals(SGConstants.LPA, exciseDuty.DutyRateUnit);
			}

			);
		}

		public void TestBuildUpdate()
		{
			DataProvider.PermitNoToUpdateOrCancel = "001";
			DataProvider.NumberOfRequestsForUpdate = 1;
			DataProvider.ReplacementPermitNumber = "002";
			DataProvider.AdditionalMessageInfo.UpdateIndicator = "TEST";
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "Test Reason For Amending";
			var update = Message.BuildUpdate();
			CombineAssertions(() =>
			{
				AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
				AssertEquals("UpdateIndicatorCode", "AME", update.UpdateIndicatorCode);
				AssertEquals("UpdatePermitNumber", "001", update.UpdatePermitNumber);
				AssertEquals("ReplacementPermitNumber", "002", update.ReplacementPermitNumber);
				AssertNull("update.Amendment", update.Amendment);
			}

			);
			Message.IsSupportingUpdateAmendmentSegment = true;
			update = Message.BuildUpdate();
			CombineAssertions(() =>
			{
				AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
				AssertEquals("UpdateIndicatorCode", "AME", update.UpdateIndicatorCode);
				AssertEquals("UpdatePermitNumber", "001", update.UpdatePermitNumber);
				AssertEquals("ReplacementPermitNumber", "002", update.ReplacementPermitNumber);
				AssertArrayEqualsByElements("AmendmentReason", new[] { "TEST REASON FOR AMENDING" }, update.Amendment.AmendmentReason);
				AssertEquals("PermitValidityExtensionIndicator", false, update.Amendment.PermitValidityExtensionIndicator);
				AssertNull("update.Amendment.ExtensionReason", update.Amendment.ExtensionReason);
			}

			);
			DataProvider.AdditionalMessageInfo.ExtendingTemporaryImportPeriod = true;
			DataProvider.AdditionalMessageInfo.ReasonForExtendingTemporaryImportPeriod = "Test Reason For Extending Temporary Import Period";
			update = Message.BuildUpdate();
			CombineAssertions(() =>
			{
				AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
				AssertEquals("UpdateIndicatorCode", "AME", update.UpdateIndicatorCode);
				AssertEquals("UpdatePermitNumber", "001", update.UpdatePermitNumber);
				AssertEquals("ReplacementPermitNumber", "002", update.ReplacementPermitNumber);
				AssertEquals("PermitValidityExtensionIndicator", true, update.Amendment.PermitValidityExtensionIndicator);
				AssertArrayEqualsByElements("AmendmentReason", new[] { "TEST REASON FOR AMENDING" }, update.Amendment.AmendmentReason);
				AssertNull("ExtensionReason", update.Amendment.ExtensionReason);
			}

			);
		}

		public void TestBuildCancellation()
		{
			DataProvider.JobNumber = "CD2020";
			DataProvider.DeclarantId = "SFZ";
			DataProvider.AdditionalRecipients = new ZString[] { "A00", "A11" };
			DataProvider.AdditionalMessageInfo.CancellationCode = "TST";
			DataProvider.Declarant = new AgentInfoTestClass { Name = "Test SG Broker", Phone = "64 85721111", Code = "V13t001" };
			DataProvider.AdditionalMessageInfo.SupportingDocuments = new AttachmentsTestClass[2] { new AttachmentsTestClass { FileName = "ATTDOC_0001", DocType = SupportingDocumentTypeCodeList.Codes.DocType001 }, new AttachmentsTestClass { FileName = "ATTDOC_0002", DocType = SupportingDocumentTypeCodeList.Codes.DocType002 } };
			var cancellation = Message.BuildCancellation();
			CombineAssertions(() =>
			{
				var header = cancellation.CancellationHeader;
				Assert("DeclarationIndicator", header.DeclarationIndicator);
				Assert("DeclarationIndicatorSpecified", header.DeclarationIndicatorSpecified);
				AssertEquals("MessageReference", "WTGCD2020", header.MessageReference);
				AssertEquals("DeclarantID", SGXmlEDIMessage.SendersReferencePlaceHolderXml, header.DeclarantID);
				AssertEquals("CommonAccessReference", CommonAccessReferenceCodeList.Codes.INPDEC, header.CommonAccessReference);
				AssertEquals("CancellationReasonCode", "TST", header.CancellationReasonCode);
				AssertArrayEqualsByElements("AdditionalRecipientID", new[] { "A00", "A11" }, header.AdditionalRecipientID);
				var uniqueReferenceNumber = header.UniqueReferenceNumber;
				AssertEquals("UniqueReferenceNumber ID", SGXmlEDIMessage.MessageNumberPlaceHolderXml, uniqueReferenceNumber.ID);
				AssertEquals("UniqueReferenceNumber Date", SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml, uniqueReferenceNumber.Date);
				AssertEquals("UniqueReferenceNumber SequenceNumeric", SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml, uniqueReferenceNumber.SequenceNumeric);
				var declarantParty = cancellation.DeclarantParty;
				AssertEquals("64 85721111", declarantParty.Telephone);
				AssertEquals("V13T001", declarantParty.PersonInformation.CodeValue);
				AssertEquals("TEST SG BROKER", declarantParty.PersonInformation.Name);
				var references = cancellation.SupportingDocumentReference;
				var idAndNames = references.Select(c => $"{c.DocumentID}:{c.Filename}").ToArray();
				AssertArrayEqualsByElements("SupportingDocumentReference", new[] { "001:ATTDOC_0001", "002:ATTDOC_0002" }, idAndNames);
			}

			);
		}

		#region Implementation
		TestTradeNetMessage Message => _message ?? (_message = new TestTradeNetMessage(DataProvider));
		TestTradeNetMessage _message;
		SGCUSDECTestClass DataProvider => dataProvider ?? (dataProvider = new SGCUSDECTestClass());
		SGCUSDECTestClass dataProvider;
		#region TradeNet Message
		public class TestTradeNetMessage : BaseTradeNetMessage<InNonPayment>
		{
			public TestTradeNetMessage(ISGCUSDEC customsDec) : base(customsDec)
			{
			}

			public void BuildHeader()
			{
				BuildHeader(InNonPayment);
			}

			public void BuildCargo()
			{
				BuildCargo(InNonPayment);
			}

			public void BuildTransport()
			{
				BuildTransport(InNonPayment);
			}

			public void BuildParty()
			{
				BuildParty(InNonPayment);
			}

			public void BuildSupportingDocumentReference()
			{
				BuildSupportingDocumentReference(InNonPayment);
			}

			public void BuildItem()
			{
				BuildItem(InNonPayment);
			}

			public void BuildSummary()
			{
				BuildSummary(InNonPayment);
			}

			public InNonPayment InNonPayment
			{
				get
				{
					if (inNonPayment == null)
					{
						var tradenetDeclaration = new TradenetDeclaration();
						var inboundMessage = tradenetDeclaration.InboundMessage = new TradenetDeclarationInboundMessage();
						inNonPayment = inboundMessage.InNonPayment = new InNonPayment();
					}

					return inNonPayment;
				}
			}

			InNonPayment inNonPayment;
			public bool IsSupportingRegistrationDateSection;
			protected override bool SupportsRegistrationDateSection => IsSupportingRegistrationDateSection;
			public bool IsSupportingUpdateAmendmentSegment;
			protected override bool SupportsUpdateAmendmentSection => IsSupportingUpdateAmendmentSegment;
			public override string MessageType => CommonAccessReferenceCodeList.Codes.INPDEC;
			public override string MessageSubType => CUSDECEDIMessage.Declaration;
			protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
			{
				var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
				inNonPayment = inboundMessage.InNonPayment = BuildDeclaration();
			}
		}
		#endregion
		#endregion
	}
}
