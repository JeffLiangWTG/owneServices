using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Testing
{
	using System;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	public class MessageBuilderMessageBuilderFromEntryHeaderTest : MessageBuilders.Testing.MessageBuilderFromEntryHeaderTest
	{
		public void TestExportSeaSingleContainerWithOnePackLine()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();

			declaration.DisableDefaultPackingInformation = false;
			declaration.JE_TotalNoOfPacksPackType = ZString.Empty;

			declaration.JE_ECI_InvoiceAmount = 999.00m;
			declaration.JE_ECI_InvoiceCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "NZD").PK;
			declaration.JE_GoodsDescription = "RALLY CAR & SPARES - BEING RETURNED";
			declaration.JE_TotalWeight = 5000m;
			declaration.JE_TotalWeightUnit = Constants.Weight.Kilograms;
			declaration.JE_TotalNoOfPacks = 4;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.CusEntryHeader.CH_CustomsMessageRemarks = "Rally parts and car being reexported";
			decCreator.SetupTestContainer("MSKU2558743", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 5000m);
			declaration.Bills.Sort(CusDecHouseBillSchema.CU_BillType.Name, System.ComponentModel.ListSortDirection.Descending);

			CreateMessageAndTestResultAgainstString(Message_ExportSeaSingleContainerWithOnePackLine, ECIMessageGenerator.MessageTypes.Original, (CusEntryHeader)declaration.CusEntryHeader);
		}
		#region Message_ExportSeaSingleContainerWithOnePackLine
		const string Message_ExportSeaSingleContainerWithOnePackLine = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
FTX+AAI+++RALLY PARTS AND CAR BEING REEXPORTED
TDT+20+109+1+++++:::BUNGA BIDARA
LOC+22+NZAKL
DTM+132:20040101:102
GIS+40:105:143
GIS+N:109:143
EQD+CN+MSKU2558743+21+++5
GIS+N:63:143
GIS+N:67:143
CNT+10:1
CNT+16:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+NZAKL
LOC+11+AUSYD
LOC+27+NZ
NAD+CN++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
NAD+CZ++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
GID+1+4:PK
FTX+AAA+++RALLY CAR & SPARES - BEING RETURNED
MEA+WT+AAG+KGM:5000.000
MOA+14:999.00:NZD
SGP+MSKU2558743
LOC+4+NZAKL
UNT+28+<<MSGNO PLACEHOLDER>>";
		#endregion

		public void TestMOAStillGetsGeneratedWhenTheresAZeroValueEntered()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			declaration.JE_ECI_InvoiceAmount = 0.00m;
			declaration.JE_ECI_InvoiceCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "NZD").PK;
			declaration.JE_GoodsDescription = "DOCS";
			CreateMessageAndTestResultAgainstString(MessageMOAStillGetsGeneratedWhenTheresAZeroValueEntered, ECIMessageGenerator.MessageTypes.Original, (CusEntryHeader)declaration.CusEntryHeader);
		}
		#region MessageMOAStillGetsGeneratedWhenTheresAZeroValueEntered
		const string MessageMOAStillGetsGeneratedWhenTheresAZeroValueEntered = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20++4+++++:::QF117
LOC+22+NZAKL
DTM+132:20040101:102
GIS+10:105:143
GIS+N:109:143
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+AUSYD
LOC+11+NZAKL
LOC+27+AU
NAD+CN++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
NAD+CZ++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
GID+1+2:PK
FTX+AAA+++DOCS
MEA+WT+AAG+KGM:20.000
MOA+14:0.00:NZD
LOC+4+AUSYD
UNT+22+<<MSGNO PLACEHOLDER>>";
		#endregion

		public void TestMultiContainerECIGeneratesWithLoosePackages()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 4000m, 10, "PK");
			decCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 7000m, 15, "PK");
			decCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.Empty, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 0, "PK");
			declaration.JE_TotalWeight = 21000m;
			declaration.JE_TotalWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			declaration.JE_ECI_InvoiceAmount = 70.00m;
			Package package = declaration.Packages.AddNew();
			package.CW_HouseBill = declaration.PrimaryHouseBill.CU_BillUniqueCode;
			package.CW_PackQty = 20;
			package.CW_PackType = "PK";

			AssertEquals("Precondition: JobDeclaration.JE_DeclaredWeight", 21000m, declaration.JE_DeclaredWeight);

			CreateMessageAndTestResultAgainstString(MessageMultiContainerECIGeneratesWithLoosePackages, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region MessageMultiContainerECIGeneratesWithLoosePackages
		const string MessageMultiContainerECIGeneratesWithLoosePackages =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20+109+1+++++:::BUNGA BIDARA
LOC+22+NZAKL
DTM+132:20040101:102
GIS+40:105:143
GIS+N:109:143
EQD+CN+OOCL0000006+21+++5
GIS+N:63:143
GIS+N:67:143
EQD+CN+OOCL0000011+21+++5
GIS+N:63:143
GIS+N:67:143
EQD+CN+OOCL0000034+21+++4
GIS+N:63:143
GIS+N:67:143
CNT+10:1
CNT+16:3
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+NZAKL
LOC+11+AUSYD
LOC+27+NZ
NAD+CN++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
NAD+CZ++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
GID+1+10:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:4000.000
MOA+14:13.33:NZD
SGP+OOCL0000006
LOC+4+NZAKL
GID+2+15:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:7000.000
MOA+14:23.33:NZD
SGP+OOCL0000011
LOC+4+NZAKL
GID+3+0:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:0.000
MOA+14:0.00:NZD
SGP+OOCL0000034
LOC+4+NZAKL
GID+4+20:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:10000.000
MOA+14:33.34:NZD
LOC+4+NZAKL
UNT+50+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestMultiContainerECIGeneratesWithEmptyPackingGroup()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 4000m, 10, "PK");
			decCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 7000m, 15, "PK");
			decCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.Empty, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 0, "PK");
			declaration.JE_TotalWeight = 21000m;
			declaration.JE_TotalWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			declaration.JE_ECI_InvoiceAmount = 70.00m;

			declaration.PrimaryHouseBill.PackingGroups.AddNew();

			AssertEquals("Precondition: JobDeclaration.JE_DeclaredWeight", 21000m, declaration.JE_DeclaredWeight);

			CreateMessageAndTestResultAgainstString(MessageMultiContainerECIGeneratesWithEmptyPackingGroup, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region MessageMultiContainerECIGeneratesWithEmptyPackingGroup
		const string MessageMultiContainerECIGeneratesWithEmptyPackingGroup =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20+109+1+++++:::BUNGA BIDARA
LOC+22+NZAKL
DTM+132:20040101:102
GIS+40:105:143
GIS+N:109:143
EQD+CN+OOCL0000006+21+++5
GIS+N:63:143
GIS+N:67:143
EQD+CN+OOCL0000011+21+++5
GIS+N:63:143
GIS+N:67:143
EQD+CN+OOCL0000034+21+++4
GIS+N:63:143
GIS+N:67:143
CNT+10:1
CNT+16:3
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+NZAKL
LOC+11+AUSYD
LOC+27+NZ
NAD+CN++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
NAD+CZ++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
GID+1+10:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:4000.000
MOA+14:13.33:NZD
SGP+OOCL0000006
LOC+4+NZAKL
GID+2+15:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:7000.000
MOA+14:23.33:NZD
SGP+OOCL0000011
LOC+4+NZAKL
GID+3+0:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:0.000
MOA+14:0.00:NZD
SGP+OOCL0000034
LOC+4+NZAKL
UNT+45+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestMultiContainerECIGeneratesOneGoodsSectionForEachContainerWithOnlyOneSGPPerSection()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 4000m, 10, "PK");
			decCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 7000m, 15, "PK");
			decCreator.SetupTestContainer("OOCL0000027", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 10000m, 20, "PK");
			decCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.Empty, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 0, "PK");
			declaration.JE_TotalWeight = 21000m;
			declaration.JE_TotalWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			declaration.JE_ECI_InvoiceAmount = 70.00m;
			AssertEquals("Precondition: JobDeclaration.JE_DeclaredWeight", 21000m, declaration.JE_DeclaredWeight);

			CreateMessageAndTestResultAgainstString(MessageMultiContainerECIGeneratesOneGoodsSectionForEachContainerWithOnlyOneSGPPerSection, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region MessageMultiContainerECIGeneratesOneGoodsSectionForEachContainerWithOnlyOneSGPPerSection
		const string MessageMultiContainerECIGeneratesOneGoodsSectionForEachContainerWithOnlyOneSGPPerSection =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20+109+1+++++:::BUNGA BIDARA
LOC+22+NZAKL
DTM+132:20040101:102
GIS+40:105:143
GIS+N:109:143
EQD+CN+OOCL0000006+21+++5
GIS+N:63:143
GIS+N:67:143
EQD+CN+OOCL0000011+21+++5
GIS+N:63:143
GIS+N:67:143
EQD+CN+OOCL0000027+21+++5
GIS+N:63:143
GIS+N:67:143
EQD+CN+OOCL0000034+21+++4
GIS+N:63:143
GIS+N:67:143
CNT+10:1
CNT+16:4
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+NZAKL
LOC+11+AUSYD
LOC+27+NZ
NAD+CN++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
NAD+CZ++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
GID+1+10:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:4000.000
MOA+14:13.33:NZD
SGP+OOCL0000006
LOC+4+NZAKL
GID+2+15:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:7000.000
MOA+14:23.33:NZD
SGP+OOCL0000011
LOC+4+NZAKL
GID+3+20:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:10000.000
MOA+14:33.34:NZD
SGP+OOCL0000027
LOC+4+NZAKL
GID+4+0:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:0.000
MOA+14:0.00:NZD
SGP+OOCL0000034
LOC+4+NZAKL
UNT+54+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestLowerCaseGoodsDescriptionGetsSentToCustomsAsUpperCase()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration.DeclarationNumber = "12345678";
			declaration.JE_GoodsDescription = "i want to brEAK free";

			CreateMessageAndTestResultAgainstString(MessageLowerCaseGoodsDescriptionGetsSentToCustomsAsUpperCase, ECIMessageGenerator.MessageTypes.ReplaceConsignment, EntryHeader);
		}
		#region MessageLowerCaseGoodsDescriptionGetsSentToCustomsAsUpperCase
		const string MessageLowerCaseGoodsDescriptionGetsSentToCustomsAsUpperCase = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN+12345678
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+21
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+AUSYD
LOC+11+NZAKL
LOC+27+AU
NAD+CN++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
NAD+CZ++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
GID+1+2:PK
FTX+AAA+++I WANT TO BREAK FREE
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
LOC+4+AUSYD
UNT+16+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestEmptyGoodsDescriptionStillIncludesAllPlussesBeforeTheActualText()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration.DeclarationNumber = "12345678";
			declaration.JE_GoodsDescription = "";

			CreateMessageAndTestResultAgainstString(MessageEmptyGoodsDescriptionStillIncludesAllPlussesBeforeTheActualText, ECIMessageGenerator.MessageTypes.ReplaceConsignment, EntryHeader);
		}
		#region MessageEmptyGoodsDescriptionStillIncludesAllPlussesBeforeTheActualText
		const string MessageEmptyGoodsDescriptionStillIncludesAllPlussesBeforeTheActualText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN+12345678
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+21
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+AUSYD
LOC+11+NZAKL
LOC+27+AU
NAD+CN++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
NAD+CZ++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
GID+1+2:PK
FTX+AAA+++  
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
LOC+4+AUSYD
UNT+16+<<MSGNO PLACEHOLDER>>
";
		#endregion

		[ExpectNoExceptions]
		public override void TestEmptyCusEntryHeaderThrowsNoExceptions()
		{
			messageBuilder = new MessageBuilderFromEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			AssertNoExceptionThrown(delegate
			{ string generatedMessage = messageBuilder.GetMessageText(); });
		}

		public void TestSetMessageType()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();

			messageBuilder = new MessageBuilderFromEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			messageBuilder.GenerateMessage();

			AssertEquals("EM_MessageType should be set", NZCMessage.MessageTypes.ECIWriteOff.MessageType, EntryHeader.Messages[0].EM_MessageType);
		}

		public void TestCreateMessageImportAir()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();

			CreateMessageAndTestResultAgainstString(CreateMessageImportAir, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region CreateMessageImportAir
		const string CreateMessageImportAir = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20++4+++++:::QF117
LOC+22+NZAKL
DTM+132:20040101:102
GIS+10:105:143
GIS+N:109:143
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+AUSYD
LOC+11+NZAKL
LOC+27+AU
NAD+CN++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
NAD+CZ++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
GID+1+2:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
LOC+4+AUSYD
UNT+22+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestRepHeaderMessageImportAir()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration.DeclarationNumber = "12345678";

			CreateMessageAndTestResultAgainstString(RepHeaderImportAir, ECIMessageGenerator.MessageTypes.ReplaceHeader, EntryHeader);
		}
		#region RepHeaderImportAir
		const string RepHeaderImportAir = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN+12345678
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+20
NAD+CA++DHL INTERNATIONAL LTD
TDT+20++4+++++:::QF117
LOC+22+NZAKL
DTM+132:20040101:102
GIS+10:105:143
GIS+N:109:143
UNT+9+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestRepLineMessageImportAir()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration.DeclarationNumber = "12345678";

			CreateMessageAndTestResultAgainstString(RepLineImportAir, ECIMessageGenerator.MessageTypes.ReplaceConsignment, EntryHeader);
		}
		#region RepLineImportAir
		const string RepLineImportAir = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN+12345678
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+21
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+AUSYD
LOC+11+NZAKL
LOC+27+AU
NAD+CN++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
NAD+CZ++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
GID+1+2:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
LOC+4+AUSYD
UNT+16+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestCreateMessageImportAirWithOneLineAddress()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.OSParty.MainAddress.OA_City = decCreator.OSParty.MainAddress.OA_Address2;
			decCreator.OSParty.MainAddress.OA_Address2 = "";

			CreateMessageAndTestResultAgainstString(CreateMessageImportAirWithOneLineAddress, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region CreateMessageImportAirWithOneLineAddress
		const string CreateMessageImportAirWithOneLineAddress = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20++4+++++:::QF117
LOC+22+NZAKL
DTM+132:20040101:102
GIS+10:105:143
GIS+N:109:143
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+AUSYD
LOC+11+NZAKL
LOC+27+AU
NAD+CN++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
NAD+CZ++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD NSW
GID+1+2:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
LOC+4+AUSYD
UNT+22+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestCreateMessageExportAir()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.SetupTestAddRemarks();

			CreateMessageAndTestResultAgainstString(CreateMessageExportAir, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region CreateMessageExportAir
		const string CreateMessageExportAir = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
FTX+AAI+++I LIKE REMARKS
TDT+20++4+++++:::QF117
LOC+22+NZAKL
DTM+132:20040101:102
GIS+40:105:143
GIS+N:109:143
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+NZAKL
LOC+11+AUSYD
LOC+27+NZ
NAD+CN++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
NAD+CZ++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
GID+1+2:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
LOC+4+NZAKL
UNT+23+<<MSGNO PLACEHOLDER>>";
		#endregion

		public void TestCreateMessageExportAirWithContainer()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.SetupTestAddRemarks();
			decCreator.SetupTestContainer();

			CreateMessageAndTestResultAgainstString(CreateMessageExportAir, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}

		public void TestCreateMessageExportSeaNoContainer()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();

			CreateMessageAndTestResultAgainstString(CreateMessageExportSea, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region CreateMessageExportSea
		const string CreateMessageExportSea = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20+109+1+++++:::BUNGA BIDARA
LOC+22+NZAKL
DTM+132:20040101:102
GIS+40:105:143
GIS+N:109:143
CNT+10:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+NZAKL
LOC+11+AUSYD
LOC+27+NZ
NAD+CN++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
NAD+CZ++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
GID+1+2:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
LOC+4+NZAKL
UNT+22+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestCreateMessageExportSeaWithContainer()
		{
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.SetupTestContainer();

			CreateMessageAndTestResultAgainstString(CreateMessageExportSeaContainerised, ECIMessageGenerator.MessageTypes.Original, EntryHeader);
		}
		#region CreateMessageExportSeaContainerised
		const string CreateMessageExportSeaContainerised = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:98A:UN
BGM+785+<<SENDERS REFERENCE PLACE HOLDER>>+9
NAD+CA++DHL INTERNATIONAL LTD
TDT+20+109+1+++++:::BUNGA BIDARA
LOC+22+NZAKL
DTM+132:20040101:102
GIS+40:105:143
GIS+N:109:143
EQD+CN+OOCL0000006+21+++5
GIS+N:63:143
GIS+N:67:143
CNT+10:1
CNT+16:1
CNI+1
RFF+HWB:HOUSETEST123
LOC+9+NZAKL
LOC+11+AUSYD
LOC+27+NZ
NAD+CN++TEST SUPPLIER AU:TEST CODE FOR NZ CUSTOMS:LOCATED IN AUSYD:NSW
NAD+CZ++ADULT BOOKS LTD:TEST CODE FOR NZ CUSTOMS:LOCATED IN NZAKL:AUK
GID+1+2:PK
FTX+AAA+++DRIED VET BILLS
MEA+WT+AAG+KGM:20.000
MOA+14:100.00:NZD
SGP+OOCL0000006
LOC+4+NZAKL
UNT+27+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public override void TestGenerateTestMessage()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();

			AssertNotNull(declaration.JobComInvoiceGroupHeaders[0]);

			messageBuilder = new MessageBuilderFromEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			messageBuilder.GetMessageText();
			AssertEquals("0", messageBuilder.PostedMessageNumber);
			messageBuilder.GenerateMessage();
			Assert(messageBuilder.PostedMessageNumber != "0");

			EDIMessage eDIMessageBusObj = messageBuilder.MessageBusinessObject;
			AssertNotNull(eDIMessageBusObj);
			AssertEquals(false, eDIMessageBusObj.IsInDatabase);
			eDIMessageBusObj.Factory.Save();
			AssertEquals("SendersReferencePlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder));
			AssertEquals("MessageNumberPlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder));
			AssertEquals("EM_Status was not Queued", NZCMessage.Status.Queued, eDIMessageBusObj.EM_Status);
			AssertEquals("EM_TestMessage Incorrectly Set", true, eDIMessageBusObj.EM_IsTestMessage);
			AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, EntryHeader.CH_EntryStatus);
			AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("EM_MessageSubType Incorrectly Set", NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original, eDIMessageBusObj.EM_MessageSubType);
		}

		public override void TestGenerateLiveMessage()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();

			AssertNotNull(declaration.JobComInvoiceGroupHeaders[0]);

			messageBuilder = new MessageBuilderFromEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			messageBuilder.GetMessageText();
			AssertEquals("0", messageBuilder.PostedMessageNumber);
			messageBuilder.GenerateMessage();
			Assert(messageBuilder.PostedMessageNumber != "0");

			EDIMessage eDIMessageBusObj = messageBuilder.MessageBusinessObject;
			AssertNotNull(eDIMessageBusObj);
			AssertEquals(false, eDIMessageBusObj.IsInDatabase);
			eDIMessageBusObj.Factory.Save();
			AssertEquals("SendersReferencePlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder));
			AssertEquals("MessageNumberPlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder));
			AssertEquals("EM_Status was not Queued", NZCMessage.Status.Queued, eDIMessageBusObj.EM_Status);
			AssertEquals("EM_TestMessage Incorrectly Set", false, eDIMessageBusObj.EM_IsTestMessage);
			AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, EntryHeader.CH_EntryStatus);
			AssertEquals(LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("EM_MessageSubType Incorrectly Set", NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original, eDIMessageBusObj.EM_MessageSubType);
		}

		public void TestEDITransmitDateGetsWrittenBackWhenPostingAMessage()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(2);
			AssertEquals("Precondition: JobDeclaration.JE_EDITransmitDate", declaration.CachedTodaysDate.AddDays(2), declaration.JE_EDITransmitDate);

			messageBuilder = new MessageBuilderFromEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			AssertEquals("JobDeclaration.JE_EDITransmitDate after post", declaration.CachedTodaysDate, declaration.JE_EDITransmitDate);
		}

		public void TestSetParentMessagingStatusAfterMessagePostingSetsJE_EntrySubmittedDate()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(2);
			AssertEquals("Precondition: JobDeclaration.JE_EDITransmitDate", declaration.CachedTodaysDate.AddDays(2), declaration.JE_EDITransmitDate);

			messageBuilder = new MessageBuilderFromEntryHeader(EntryHeader, ECIMessageGenerator.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			AssertEquals("JobDeclaration.JE_EntrySubmittedDate after post", declaration.CachedTodaysDate, declaration.JE_EntrySubmittedDate);
		}

		public void TestCarrierNameLenghtForTSW()
		{
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "VIRGIN AUSTRALIA INTERNATIONAL AIRLINES PTY LTD";
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			CusEntryHeader entryHeader = (CusEntryHeader)declaration.CusEntryHeader;

			var messageBuilder = new MessageBuilderFromEntryHeader(entryHeader, ECIMessageGenerator.MessageTypes.Original);
			var messageGenerated = messageBuilder.GetMessageText();
			AssertContains("Message Generated", "NAD+CA++VIRGIN AUSTRALIA INTERNATIONAL AIRL'", messageGenerated);
		}

		#region Implementation
		protected new MessageBuilderFromEntryHeader messageBuilder
		{
			get { return (MessageBuilderFromEntryHeader)base.messageBuilder; }
			set { base.messageBuilder = value; }
		}

		protected new TestECIWriteOffCreator decCreator
		{
			get { return (TestECIWriteOffCreator)base.decCreator; }
			set { base.decCreator = value; }
		}

		protected CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)declaration.CusEntryHeader; }
		}

		protected override void SetupJobDeclarationAndTestCreator()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			decCreator = new TestECIWriteOffCreator(declaration);
		}

		protected void CreateMessageAndTestResultAgainstString(string expectedMessageText, ECIMessageGenerator.MessageTypes messageType, CusEntryHeader entryHeader)
		{
			messageBuilder = new MessageBuilderFromEntryHeader(entryHeader, messageType);
			TestMessageBuilderResultAgainstString(expectedMessageText.Replace("'", ""));
		}
		#endregion
	}
}
