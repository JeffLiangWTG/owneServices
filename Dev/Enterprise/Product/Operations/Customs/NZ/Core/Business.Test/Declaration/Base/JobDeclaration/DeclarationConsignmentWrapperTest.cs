using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	sealed class DeclarationConsignmentWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new DeclarationConsignmentWrapper(null);
		}

		public void TestConsignmentIsGeneratedFromDeclaration()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_HouseBill = "HB00329";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			AssertEquals("BillNumber", "HB00329", creConsignment.BillNumber.BillNumber);
			AssertEquals("PortOfLoading", "NZAKL", creConsignment.PortOfLoading);
			AssertEquals("PortOfDischarge", "AUSYD", creConsignment.PortOfDischarge);
			AssertEquals("ConsignmentItems", 1, creConsignment.ConsignmentItems.Count());
			AssertNull("InternationalTranshipmentRequest", creConsignment.TranshipmentDetails);
		}

		public void TestConsignmentItemGeneratedFromDeclaration()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_HouseBill = "HB00329";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_GoodsDescription = "LOW VALUE GOODS";
			JobDeclaration.JE_ECI_InvoiceAmount = 100m;
			JobDeclaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD").PK;
			JobDeclaration.JE_TotalWeight = 5m;
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CT";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";

			AssertEquals("BillNumber", "HB00329", creConsignment.BillNumber.BillNumber);
			AssertEquals("PortOfLoading", "NZAKL", creConsignment.PortOfLoading);
			AssertEquals("PortOfDischarge", "AUSYD", creConsignment.PortOfDischarge);
			AssertEquals("ConsignmentItems", 1, creConsignment.ConsignmentItems.Count());

			AssertEquals("GoodsDescription", "LOW VALUE GOODS", creConsignmentItem.GoodsDescription);
			AssertEquals("CommodityValue", 100m, creConsignmentItem.Value);
			AssertEquals("CommodityValueCurrency", "NZD", creConsignmentItem.Currency);
			AssertEquals("ItemGrossWeightInKGM", 5m, creConsignmentItem.GrossWeightInKg);
			AssertEquals("NoOfPackages", 15, creConsignmentItem.PackageQty);
			AssertEquals("PackageType", "CT", creConsignmentItem.PackageType);
			AssertEquals("OriginCountry", "NZ", creConsignmentItem.GoodsOriginCountry);
		}

		public void TestHandlingInfo()
		{
			var expectedHandlingElement = @"<AdditionalInformation>
      <StatementDescription>Please take extra care when handling the goods.contains glass items.</StatementDescription>
      <StatementTypeCode>HAN</StatementTypeCode>
    </AdditionalInformation>";

			CreateImportSeaWriteOffDeclaration();
			var note = JobDeclaration.NotesOfDeclarationOrShipment.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_Table = JobDeclaration.TableName;
			note.ST_ParentID = JobDeclaration.PK;
			note.ST_NoteDataAsText = "Please take extra care when handling the goods.\ncontains glass items.";
			Factory.Save();

			var entryHeader = JobDeclaration.CusEntryHeader;
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			ECIWriteOff.CusEntryHeader header = (ECIWriteOff.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, JobDeclaration.eDocsForSelection, TSWTransactionTypes.Original, JobDeclaration.Factory, "ICR");
			var icrBuilder = new ICRMessageBuilder(new TSWEntryHeaderWrapper(header, additionalMessageInformation), TSWTransactionTypes.Original, "00009908C");
			var icrMessageString = icrBuilder.GetXMLMessage();

			AssertEquals("Handling Note should have been included in the message", true, icrMessageString.Contains(expectedHandlingElement));
		}

		public void TestMPIAccountInfo()
		{
			var expectedMPIAccountElement = @"<AdditionalInformation>
      <StatementDescription>12345,William Tell</StatementDescription>
      <StatementTypeCode>MAC</StatementTypeCode>
    </AdditionalInformation>";

			CreateImportSeaWriteOffDeclaration();
			JobDeclaration.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
			JobDeclaration.ZX_AccountNumber = "12345";
			JobDeclaration.ZX_AccountHolder = "William Tell";
			Factory.Save();

			var entryHeader = JobDeclaration.CusEntryHeader;
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			ECIWriteOff.CusEntryHeader header = (ECIWriteOff.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, JobDeclaration.eDocsForSelection, TSWTransactionTypes.Original, JobDeclaration.Factory, "ICR");
			var icrBuilder = new ICRMessageBuilder(new TSWEntryHeaderWrapper(header, additionalMessageInformation), TSWTransactionTypes.Original, "00009908C");
			var icrMessageString = icrBuilder.GetXMLMessage();

			AssertEquals("MPI Account details should have been included in the mesage", true, icrMessageString.Contains(expectedMPIAccountElement));
		}

		public void TestTotalConsignmentValueNZD()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_ECI_InvoiceAmount = 250m;
			AssertEquals("ConsignmentValueInNZD", 250m, creConsignment.ConsignmentValueInNZD);

			JobDeclaration.JE_ECI_InvoiceCurrency = ZGuid.Empty;
			AssertEquals("ConsignmentValueInNZDg5", 0m, creConsignment.ConsignmentValueInNZD);
		}

		public void TestWriteOffRequestForICR()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_ECI_InvoiceAmount = 1150;
			Assert("WriteOffRequest - NO - value is greater than Deminimus", !icrConsignment.WriteOffRequest);

			JobDeclaration.JE_ECI_InvoiceAmount = 0m;
			Assert("WriteOffRequest - NO - value is not entered", !icrConsignment.WriteOffRequest);

			JobDeclaration.JE_ECI_InvoiceAmount = 700m;
			Assert("WriteOffRequest - YES - value is entered and is below Deminimus value", icrConsignment.WriteOffRequest);
		}

		public void TestHandlingInfoIsTrimmed()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invalidNoteTxt = "awsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPO" +
				"EINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpin" +
				"ewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidf" +
				"owinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoer" +
				"ihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFN";

			var trimmedNoteTxt = "awsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPO" +
				"EINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpin" +
				"ewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidf" +
				"owinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoerihfwaoeidfowinedfpinewdpfinwPOEINDFWOEFNawsoer";

			JobDeclaration.Notes.AddNew(isCustomDescription: false, "Goods Handling Instructions", invalidNoteTxt);

			AssertEquals("Handling Info should be trimmed", trimmedNoteTxt, creConsignment.HandlingInfo);
		}

		public void TestMAFContainerStatements_WithData()
		{
			SetupMAFContainerStatement();
			JobDeclaration.JE_SendMCDContainerQuarantineDeclaration = true;
			JobDeclaration.JE_HaveMAFContainerDeclaration = true;
			JobDeclaration.JE_IsContainerClean = true;
			JobDeclaration.JE_IsWoodPackagingUsed = true;
			Assert("MAFContainerDeclaration", icrConsignment.MAFContainerDeclaration);

			int mafContainerStatementCount = 0;
			var stmt1 = ZString.Empty;
			var stmt2 = ZString.Empty;
			foreach (var mafStmt in icrConsignment.MAFContainerStatements)
			{
				mafContainerStatementCount++;
				if (mafStmt.StartsWith("MNHU0029382"))
				{
					stmt1 = mafStmt;
				}
				else
				{
					stmt2 = mafStmt;
				}
			}

			AssertEquals("Should be two container MAF statements created in wrapped ICR Consignment", 2, mafContainerStatementCount);
			AssertEquals("MAFContainerStatement", "MNHU0029382,YNYNN", stmt1);
			AssertEquals("MAFContainerStatement", "MNHU0149961,YNYNN", stmt2);
		}

		public void TestMAFContainerStatements_WithoutData()
		{
			SetupMAFContainerStatement();
			JobDeclaration.JE_SendMCDContainerQuarantineDeclaration = true;
			Assert("MAFContainerDeclaration", icrConsignment.MAFContainerDeclaration);

			int mafContainerStatementCount = 0;
			var stmt1 = ZString.Empty;
			var stmt2 = ZString.Empty;
			foreach (var mafStmt in icrConsignment.MAFContainerStatements)
			{
				mafContainerStatementCount++;
				if (mafStmt.StartsWith("MNHU0029382"))
				{
					stmt1 = mafStmt;
				}
				else
				{
					stmt2 = mafStmt;
				}
			}

			AssertEquals("Should be two container MAF statements created in wrapped ICR Consignment", 2, mafContainerStatementCount);
			AssertEquals("MAFContainerStatement", "MNHU0029382,", stmt1);
			AssertEquals("MAFContainerStatement", "MNHU0149961,", stmt2);
		}

		public void TestMAFContainerStatements_NotSending()
		{
			SetupMAFContainerStatement();
			JobDeclaration.JE_SendMCDContainerQuarantineDeclaration = false;
			Assert("MAFContainerDeclaration", !icrConsignment.MAFContainerDeclaration);
			Assert("MAFContainerStatements", !icrConsignment.MAFContainerStatements.Any());
		}

		void SetupMAFContainerStatement()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			JobDeclaration.JE_HouseBill = "HB00329";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "MNHU0029382";
			container1.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container1.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container1.CO_MPIApprovedSystemNumber = "75128";

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 2;
			packLine1.CW_PackType = "07";

			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "MNHU0149961";
			container2.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container2.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container2.CO_MPIApprovedSystemNumber = "73492";

			var packLine2 = JobDeclaration.Packages.AddNew();
			packLine2.CW_HouseBill = packLine1.CW_HouseBill;
			packLine2.CW_ContainerNoOrEquipmentNo = "MNHU0149961";
			packLine2.CW_PackQty = 1;
			packLine2.CW_PackType = "PK";
		}

		public void TestCREIdentifiers()
		{
			AssertEquals(Enumerable.Empty<ICommodity>(), creConsignmentItem.Identifiers);
		}

		public void TestClassifications()
		{
			AssertEquals(Enumerable.Empty<IClassification>(), creConsignmentItem.Classifications);
			AssertEquals(Enumerable.Empty<IClassification>(), icrConsignmentItem.Classifications);
		}

		public void TestCREWriteOffRequest()
		{
			Assert(creConsignment.WriteOffRequest);
		}

		public void TestCREInternationalTranshipmentRequest()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_MovementReason = ZString.Empty;
			Assert(!creConsignment.TranshipmentDetails.InternationalTranshipmentRequest);
			itrData.C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			Assert("If no Transhipment details have been entered, do not consider this a transhipment request", !creConsignment.TranshipmentDetails.InternationalTranshipmentRequest);
			itrData.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			Assert(creConsignment.TranshipmentDetails.InternationalTranshipmentRequest);
		}

		public void TestCREModeOfTransportForTransfer()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			AssertEquals("4", creConsignment.TranshipmentDetails.ModeOfTransportForTransfer);
		}

		public void TestCREITRImportCraft()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_TranshipBySeaVessel = "HYOGO MARU";
			AssertEquals("HYOGO MARU", creConsignment.TranshipmentDetails.ITRImportCraft);
		}

		public void TestCREITRImportMode()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertEquals("1", creConsignment.TranshipmentDetails.ITRImportMode);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber - there is only 1 consignment for a write-off declaration", (ZShort)0, creConsignment.SequenceNumber);
		}

		[TestDate(2018, 10, 16)]
		public void TestCREITRArrivalDate()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			var testDate = ZDateTime.Now;
			itrData.C4_ArrivalDate = testDate;
			AssertEquals(testDate, creConsignment.TranshipmentDetails.ITRArrivalDate);
		}

		public void TestCREITRVoyageFlight()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Multimodal;
			itrData.C4_FlightNo = "QF710";
			itrData.C4_TranshipBySeaVoyage = "293S";
			Assert(creConsignment.TranshipmentDetails.ITRVoyageFlight.IsEmpty);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			itrData.C4_TranshipBySeaVoyage = "293S";
			AssertEquals("293S", creConsignment.TranshipmentDetails.ITRVoyageFlight);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			itrData.C4_FlightNo = "QF710";
			AssertEquals("QF710", creConsignment.TranshipmentDetails.ITRVoyageFlight);
		}

		public void TestStuffingEstablishment()
		{
			CreateImportSeaWriteOffDeclaration();
			var wrappedDeclarationConsignment = new DeclarationConsignmentWrapper(JobDeclaration);
			IICRConsignment icrDeclaration = wrappedDeclarationConsignment;
			AssertNotNull(icrDeclaration);
			int packingLocations = 0;
			foreach (IOrganisation packingLocation in icrDeclaration.ContainerPackingLocations)
			{
				packingLocations++;
				AssertEquals("PackingLocation Name", "Container Packing Inc.", packingLocation.Name);
				AssertEquals("PackingLocation City", "Long Beach", packingLocation.City);
				AssertEquals("PackingLocation Street", "500 Mitchum St", packingLocation.Address);
				AssertEquals("PackingLocation CountryCode", "US", packingLocation.CountryCode);
				AssertEquals("PackingLocation PostCode", "57500", packingLocation.PostCode);
			}

			AssertEquals("Should be 2 packingLocations generated by wrapper", 2, packingLocations);
		}

		public void TestStuffingLocationInMessage()
		{
			var expectedStuffingLocationElement = @"<StuffingEstablishment>
      <Name>Container Packing Inc.</Name>
      <Address>
        <CityName>Long Beach</CityName>
        <CountryCode>US</CountryCode>
        <Line>500 Mitchum St</Line>
        <PostcodeID>57500</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name>Container Packing Inc.</Name>
      <Address>
        <CityName>Long Beach</CityName>
        <CountryCode>US</CountryCode>
        <Line>500 Mitchum St</Line>
        <PostcodeID>57500</PostcodeID>
      </Address>
    </StuffingEstablishment>";
			var expectedStuffingLocationPointerElement = @"<TransportEquipment>
      <SequenceNumeric>1</SequenceNumeric>
      <CharacteristicCode>45</CharacteristicCode>
      <FullnessCode>5</FullnessCode>
      <ID>MNHU0029382</ID>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>16B</DocumentSectionCode>
      </Pointer>
    </TransportEquipment>
    <TransportEquipment>
      <SequenceNumeric>2</SequenceNumeric>
      <CharacteristicCode>45</CharacteristicCode>
      <FullnessCode>5</FullnessCode>
      <ID>MNHU0149961</ID>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>2</SequenceNumeric>
        <DocumentSectionCode>16B</DocumentSectionCode>
      </Pointer>
    </TransportEquipment>";

			CreateImportSeaWriteOffDeclaration();
			var entryHeader = JobDeclaration.CusEntryHeader;
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			ECIWriteOff.CusEntryHeader header = (ECIWriteOff.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, JobDeclaration.eDocsForSelection, TSWTransactionTypes.Original, JobDeclaration.Factory, "ICR");
			var icrBuilder = new ICRMessageBuilder(new TSWEntryHeaderWrapper(header, additionalMessageInformation), TSWTransactionTypes.Original, "00009908C");
			var icrMessageString = icrBuilder.GetXMLMessage();

			AssertContains("ICR Message - stuffing establishment element should be sent for Sea ICR", expectedStuffingLocationElement, icrMessageString);
			AssertContains("ICR Message - stuffing establishment pointer element should be included for Sea ICR", expectedStuffingLocationPointerElement, icrMessageString);
		}

		[TestDate(2018, 07, 25)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestICRMessageStuffingEstablishment()
		{
			var packingOrg = Factory.NewWithValidTestData<OrgHeader>();
			packingOrg.OH_FullName = "MAERSK LINES";
			packingOrg.MainAddress.OA_Address1 = "1001 Link Rd";
			packingOrg.MainAddress.OA_City = "Port Botany";
			packingOrg.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			packingOrg.MainAddress.OA_PostCode = "2012";

			var packingOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			packingOrg2.OH_FullName = "CUNNARD";
			packingOrg2.MainAddress.OA_Address1 = "15 Wharf RD";
			packingOrg2.MainAddress.OA_City = "Port Botany";
			packingOrg2.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			packingOrg2.MainAddress.OA_PostCode = "2012";

			CreateImportSeaWriteOffDeclaration();
			var container1 = JobDeclaration.CusContainers[0];
			var container2 = JobDeclaration.CusContainers[1];
			container1.CO_OA_PackingLocation = packingOrg.Addresses.MainAddress.PK;
			container2.CO_OA_PackingLocation = packingOrg2.Addresses.MainAddress.PK;
			var entryHeader = JobDeclaration.CusEntryHeader;
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var header = (ECIWriteOff.CusEntryHeader)entryHeader;
			var additionalMessageInformation = new AdditionalMessageInformation(null, JobDeclaration.eDocsForSelection, TSWTransactionTypes.Original, JobDeclaration.Factory, "ICR");
			_ = new DeclarationConsignmentWrapper(JobDeclaration);
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009908C"))
			using (NZCustomsDataRegistry.Instance.EnableSeaICRFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var icrBuilder = new ICRMessageBuilder(new TSWEntryHeaderWrapper(header, additionalMessageInformation), TSWTransactionTypes.Original, "00009908C");
				AssertASCIIFileSameAsString(BaseSourcePath + @"\Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\Declaration\Base\JobDeclaration\TestFiles\TestICRStuffingLocationsMessage.txt", icrBuilder.GetXMLMessage());
			}
		}

		public void TestICRInternationalTranshipmentRequest()
		{
			AssertNull(icrConsignment.TranshipmentDetails);
			var trData = TranshipmentRequest.Create(JobDeclaration);
			trData.C4_MovementReason = ZString.Empty;
			Assert(!icrConsignment.TranshipmentDetails.InternationalTranshipmentRequest);
			trData.C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			Assert("If no Transhipment details have been entered, do not consider this a transhipment request", !icrConsignment.TranshipmentDetails.InternationalTranshipmentRequest);
			trData.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			Assert(icrConsignment.TranshipmentDetails.InternationalTranshipmentRequest);
		}

		public void TestICRModeOfTransportForTransfer()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			AssertEquals("4", icrConsignment.TranshipmentDetails.ModeOfTransportForTransfer);
		}

		public void TestICRITRImportCraft()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_TranshipBySeaVessel = "HYOGO MARU";
			AssertEquals("HYOGO MARU", icrConsignment.TranshipmentDetails.ITRImportCraft);
		}

		public void TestICRITRImportMode()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaOV;
			AssertEquals("1O", icrConsignment.TranshipmentDetails.ITRImportMode);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaCV;
			AssertEquals("1C", icrConsignment.TranshipmentDetails.ITRImportMode);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			AssertEquals("4", icrConsignment.TranshipmentDetails.ITRImportMode);
		}

		public void TestICRITRArrivalDate()
		{
			var testDate = ZDateTime.Now;
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_TranshipDepartureDate = testDate;
			AssertEquals("ITRDepartureDate", testDate, icrConsignment.TranshipmentDetails.ITRDepartureDate);
		}

		public void TestICRITRVoyageFlight()
		{
			AssertNull(creConsignment.TranshipmentDetails);
			var itrData = TranshipmentRequest.Create(JobDeclaration);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Multimodal;
			itrData.C4_FlightNo = "QF710";
			itrData.C4_TranshipBySeaVoyage = "293S";
			Assert(icrConsignment.TranshipmentDetails.ITRVoyageFlight.IsEmpty);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaOV;
			itrData.C4_TranshipBySeaVoyage = "293S";
			AssertEquals("293S", icrConsignment.TranshipmentDetails.ITRVoyageFlight);
			itrData.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			itrData.C4_FlightNo = "QF710";
			AssertEquals("QF710", icrConsignment.TranshipmentDetails.ITRVoyageFlight);
		}

		[TestDate(2019, 08, 30)]
		public void TestSupplierGSTNumberAndIsGSTPrePaid()
		{
			CreateImportSeaWriteOffDeclaration();
			AssertEquals(new ZDateTime(2019, 08, 30), JobDeclaration.JE_DateOfArrival);

			IICRConsignment icrDeclaration = new DeclarationConsignmentWrapper(JobDeclaration);
			Assert(icrDeclaration.IsGSTPrePaid.IsEmpty);
			Assert(icrDeclaration.VendorIdentifier.IsEmpty);

			var invoice = JobDeclaration.Invoices[0];
			invoice.JZ_IsGSTPrePaid = "Y";
			invoice.JZ_SupplierGSTNumber = "TEST  1-2.33";
			AssertEquals("Y", icrDeclaration.IsGSTPrePaid);
			AssertEquals("TEST1233", icrDeclaration.VendorIdentifier);

			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_IsGSTPrePaid = "Y";
			invoice2.JZ_SupplierGSTNumber = "TEST  1-2.33";

			AssertEquals("Y", icrDeclaration.IsGSTPrePaid);
			AssertEquals("TEST1233", icrDeclaration.VendorIdentifier);

			invoice2.JZ_IsGSTPrePaid = "N";
			invoice2.JZ_SupplierGSTNumber = "TEST 2468";

			Assert(icrDeclaration.IsGSTPrePaid.IsEmpty);
			Assert(icrDeclaration.VendorIdentifier.IsEmpty);
		}

		public void TestNotifyPartyCodes()
		{
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "WINTERFELL";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111A");
			JobDeclaration.JE_RL_NKPortOfDeliveryNotify = "DORNE";
			JobDeclaration.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty.PK;

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder("NotifyPartyCodes", new ZString[] { "DORNE", "11111A" }, creConsignment.NotifyPartyCodes);

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("NotifyPartyCodes", new ZString[] { "DORNE", "11111A" }, icrConsignment.NotifyPartyCodes);
		}

		public void TestIsLinkedEmptyContainer()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("IsLinkEmptyContainer - Declaration.HasContainersAndTheyreAllEmpty", false, icrConsignment.IsLinkEmptyContainer);

			var container = JobDeclaration.CusContainers.AddNew();
			AssertEquals("IsLinkEmptyContainer - Declaration.HasContainersAndTheyreAllEmpty", false, icrConsignment.IsLinkEmptyContainer);

			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("IsLinkEmptyContainer - Declaration.HasContainersAndTheyreAllEmpty", true, icrConsignment.IsLinkEmptyContainer);

			JobDeclaration.CusContainers.AddNew();
			AssertEquals("IsLinkEmptyContainer - Declaration.HasContainersAndTheyreAllEmpty", false, icrConsignment.IsLinkEmptyContainer);
		}

		public void TestDeliverToParty()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var deliverToParty = Factory.New<OrgHeader>();
			deliverToParty.OH_FullName = "John Smith Industries P/L";
			deliverToParty.OH_Code = "JOHSMIAKL";
			deliverToParty.OH_RL_NKClosestPort = "NZAKL";
			deliverToParty.MainAddress.OA_Address1 = "1 Main St.";
			JobDeclaration.DeliveryDestinationPartyDocAddress.OrganisationPK = deliverToParty.PK;
			AssertEquals("Deliver To Party", "John Smith Industries P/L", icrConsignment.DeliverToParty.Name);
		}

		public void TestATF()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var atfOrg = Factory.New<OrgHeader>();
			atfOrg.OH_FullName = "John Smith Industries P/L";
			atfOrg.OH_Code = "JOHSMIAKL";
			atfOrg.OH_RL_NKClosestPort = "NZAKL";
			atfOrg.MainAddress.OA_Address1 = "1 Main St.";
			var atfDepot1Address = atfOrg.Addresses.AddNew();
			atfDepot1Address.OA_Address1 = "JSI Depot 1";
			var atfDepot2Address = atfOrg.Addresses.AddNew();
			atfDepot2Address.OA_Address1 = "JSI Depot 2";
			var atfWarehouseAddress = atfOrg.Addresses.AddNew();
			atfWarehouseAddress.OA_Address1 = "JSI Warehouse";

			atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "7015H");
			var atf2 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "7039J");
			atf2.OK_OA_PremisesAddress = atfOrg.MainAddress.PK;
			var atf3 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "2075R");
			atf3.OK_OA_PremisesAddress = atfDepot1Address.PK;
			var atf4 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "4619W");
			atf4.OK_OA_PremisesAddress = atfDepot2Address.PK;
			var atf5 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "1270P");
			atf5.OK_OA_PremisesAddress = atfWarehouseAddress.PK;

			JobDeclaration.DeliveryDestinationPartyDocAddress.OrganisationPK = atfOrg.PK;
			AssertEquals("ATF Code from appropriate premise address (Main Address) is determined as '7039J'", "7039J", icrConsignment.ApprovedTransitionalFacilityCode);

			JobDeclaration.DeliveryDestinationPartyDocAddress.E2_OA_Address = atf4.OK_OA_PremisesAddress;
			AssertEquals("ATF Code from appropriate premise address (JSI Depot 2) is determined as '4619W'", "4619W", icrConsignment.ApprovedTransitionalFacilityCode);

			JobDeclaration.DeliveryDestinationPartyDocAddress.E2_OA_Address = atf5.OK_OA_PremisesAddress;
			AssertEquals("ATF Code from appropriate premise address (JSI Warehouse) is determined as '1270P'", "1270P", icrConsignment.ApprovedTransitionalFacilityCode);
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, icrConsignment.SupportingDocuments.Count());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var wrappedDeclaration = new DeclarationConsignmentWrapper(JobDeclaration);
			creConsignmentItem = wrappedDeclaration;
			icrConsignmentItem = wrappedDeclaration;
			icrConsignment = wrappedDeclaration;
			creConsignment = wrappedDeclaration;
		}
		ICREConsignment creConsignment;
		ICREConsignmentItem creConsignmentItem;
		IICRConsignment icrConsignment;
		IICRConsignmentItem icrConsignmentItem;

		void CreateImportSeaWriteOffDeclaration()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "ANL SHIPPING";

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			JobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			JobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "B00002309";
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			JobDeclaration.JE_VesselName = "Hyogo Maru";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";

			NZAddInfo addInfo = ((IHaveNZAddInfo)JobDeclaration).AddInfo;
			addInfo.ZN_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
			var wareHouseOrg = Factory.New<OrgHeader>();
			wareHouseOrg.OH_Code = "WAREHOUSE";
			OrgCusCode supplierCode = wareHouseOrg.CustomsCodes.AddNew();
			supplierCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			supplierCode.OK_CountryDefault = true;
			supplierCode.OK_CustomsRegNo = "2975W";
			supplierCode.OK_RN_NKCodeCountry = "NZ";
			JobDeclaration.WarehouseDocAddress.E2_OA_Address = wareHouseOrg.MainAddress.PK;

			var stuffingLocation = Factory.NewWithValidTestData<OrgHeader>();
			stuffingLocation.OH_FullName = "Container Packing Inc.";
			stuffingLocation.MainAddress.CompanyName = "Container Packing Inc.";
			stuffingLocation.MainAddress.OA_Address1 = "500 Mitchum St";
			stuffingLocation.MainAddress.OA_City = "Long Beach";
			stuffingLocation.MainAddress.OA_RL_NKRelatedPortCode = "USLBH";
			stuffingLocation.MainAddress.Postcode = "57500";

			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "MNHU0029382";
			container1.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container1.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container1.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container1.CO_MPIApprovedSystemNumber = "75128";
			container1.PackingLocationOrgPK = stuffingLocation.PK;

			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 2;
			packLine1.CW_PackType = "07";

			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "MNHU0149961";
			container2.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc40Ft;
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			container2.CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C45;
			container2.CO_MPIApprovedSystemNumber = "73492";
			container2.PackingLocationOrgPK = stuffingLocation.PK;

			var packLine2 = JobDeclaration.Packages.AddNew();
			packLine2.CW_HouseBill = packLine1.CW_HouseBill;
			packLine2.CW_ContainerNoOrEquipmentNo = "MNHU0149961";
			packLine2.CW_PackQty = 1;
			packLine2.CW_PackType = "PK";

			JobDeclaration.JE_SendMCDContainerQuarantineDeclaration = true;
			JobDeclaration.JE_HaveMAFContainerDeclaration = true;
			JobDeclaration.JE_IsContainerClean = true;
			JobDeclaration.JE_IsWoodPackagingUsed = true;

			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine1 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3307.30.00.00E";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_InvoiceQuantity = 2.0m;
			invoiceLine1.JI_InvoiceUQ = "07";
			invoiceLine1.JI_Description = "CHEMICALS";
			invoiceLine1.JI_CountryOfOrigin = "SG";
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			var invoiceLine2 = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3307.30.00.00E";
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.JI_InvoiceQuantity = 1.0m;
			invoiceLine2.JI_InvoiceUQ = "PK";
			invoiceLine2.JI_Description = "CHEMICALS";
			invoiceLine2.JI_CountryOfOrigin = "SG";
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		JobDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		JobDeclaration jobDeclaration;

		#endregion
	}
}
