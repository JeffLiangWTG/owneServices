using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class BorderCargoReleaseMessageBuilderTest : EntrySummaryMessageBuilderAbstractTest
	{
		public void TestEndToEndTestWithLineLevelUltimateConsignee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.US_EnableCRL = true;

			declaration.US_EntryFilerCode = "XJ5";

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "AAAAAA123");

			OrgHeader ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "BBBBBB123");

			OrgHeader ultimateConsignee3 = Factory.New<OrgHeader>();
			ultimateConsignee3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "CCCCCC123");

			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee3.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);

			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals("EIN number for declaration ultimate consignee should not be part of messages", false, message.EM_MessageText.Contains("AAAAAA123"));
			AssertEquals("EIN number for invoiceLine1 ultimate consignee should be part of messages", true, message.EM_MessageText.Contains("BBBBBB123"));
			AssertEquals("EIN number for invoiceLine2 ultimate consignee should be part of messages", true, message.EM_MessageText.Contains("CCCCCC123"));
		}

		[TestDate(2007, 11, 27, 10, 30, 35)]
		public void TestEndToEnd()
		{
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("1");
			CusEntryHeader[] entries = declaration.CustomsEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease));
			AssertEquals(1, entries.Length);
			CusEntryHeader bcrEntry = entries[0];
			AssertEquals("Messages", 1, bcrEntry.Messages.Count);
			AssertNotEquals("EntryNumber", "", bcrEntry.EntryNumber);
			MQEDIMessage message = (MQEDIMessage)bcrEntry.Messages[0];
			ABIInputBlockControlGenerator generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			generator.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			AssertEquals("Application Identifier", ApplicationIdentifierCodeList.Codes.BorderCargoRelease, generator.B.ApplicationIdentifier);
			AssertEquals("MessageBlocks.Count", 3, generator.MessageBlocks.Count);
			AssertBCR01((BCR01)generator.MessageBlocks[0], bcrEntry);
			AssertBCR0M((BCR0M)generator.MessageBlocks[1]);
			AssertBCR02((BCR02)generator.MessageBlocks[2]);
		}

		[TestDate(2007, 11, 27, 10, 30, 35)]
		public void TestCreatingMessageWithOGAData()
		{
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot.US_DOTTireBrandName = "TRADE NAME";
			dot.US_DOTPassport = "124HJ";
			dot.US_DOTCountryOfOrigin = "AU";
			dot.US_DOTBondSuretyCode = "111";
			dot.US_DOTPriorApproval = true;
			dot.US_DOTImpSubstStatement = true;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTTireID = "ZZZ";
			dot.US_DOTCommercialDesc = "DESCRIPTION1";
			DOTVIN dotvin = dot.DOTVINs.AddNew();//empty line

			dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			dot.US_DOTCommercialDesc = "DESCRIPTION2";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE1";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE2";

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8706005000";
			invoiceLine2.ImportTariff.UE_OGACodes = "DT1";
			invoiceLine2.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;

			MergeAndSend("OGADataTest", false, true);

			CusEntryHeader[] entries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.BorderCargoRelease);
			AssertEquals(1, entries.Length);
			CusEntryHeader bcrEntry = entries[0];
			AssertEquals("Messages", 1, bcrEntry.Messages.Count);
			MQEDIMessage message = (MQEDIMessage)bcrEntry.Messages[0];
			helper.MessageMustContainElement<OGADT01>(message);
			helper.MessageMustContainElement<OGADT02>(message);
			//helper.MessageMustContainElement<OGADT03>(message);
			helper.MessageMustContainElement<AENSOI>(message);
			helper.MessageMustContainElement<OGAOA>(message);
			helper.MessageMustContainElement<OGAFD01>(message);
			helper.MessageMustContainElement<OGAFD02>(message);
			helper.MessageMustContainElement<OGAFD03>(message);
			helper.MessageMustContainElement<OGAFD04>(message);
		}

		[TestDate(2007, 11, 27, 10, 30, 35)]
		public void TestOGADisclaimedDataComesBeforeOGADeclaredData()
		{
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.ImportTariff.UE_OGACodes = "FD1DT1";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot.US_DOTTireBrandName = "TRADE NAME";
			dot.US_DOTPassport = "124HJ";
			dot.US_DOTCountryOfOrigin = "AU";
			dot.US_DOTBondSuretyCode = "111";
			dot.US_DOTPriorApproval = true;
			dot.US_DOTImpSubstStatement = true;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTTireID = "ZZZ";
			dot.US_DOTCommercialDesc = "DESCRIPTION1";
			DOTVIN dotvin = dot.DOTVINs.AddNew();//empty line

			dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			dot.US_DOTCommercialDesc = "DESCRIPTION2";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE1";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE2";

			PGA pGA = invoiceLine.LaceyActLines.AddNew();
			pGA = DeclarationTestHelper.CreateLaceyActData(pGA);

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8706005000";
			invoiceLine2.ImportTariff.UE_OGACodes = "DT1";
			invoiceLine2.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;

			MergeAndSend("OGADataOrderTest", false, true);

			CusEntryHeader[] entries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.BorderCargoRelease);
			AssertEquals(1, entries.Length);
			CusEntryHeader bcrEntry = entries[0];
			AssertEquals("Messages", 1, bcrEntry.Messages.Count);
			MQEDIMessage message = (MQEDIMessage)bcrEntry.Messages[0];
			string messageExpected = @"B018888XJ5HN                                               EDIEDIDAT_1          
01A8888XJ5000000142191-013199000889191-01319900011210701 AAAA                   
0M            OBLOGADATAOR                        00000001PK         AAAA       
02AU4421909720AUSOUPAC195PAD                                                    
OA  FD0                                                                         
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 
OI        DESCRIPTION1                                                          
DT010012AY124HJ              AU111YYVZZZTRADE NAME                              
OI        DESCRIPTION2                                                          
DT0100208Y                                                                      
DT02MAKE1                                                                       
DT0100308Y                                                                      
DT02MAKE2                                                                       
02AU8706005000AUSOUPAC195PAD                                                    
OA  DT0                                                                         
Y  8888XJ5HN00019";
			AssertMultilineASCIIEquals("Disclaimed OGA data must be immediately after tariff & before declared OGA data", messageExpected, message.EM_FormattedMessageText);
		}

		public void TestEndToEndWithEncryptedConsigneeNumber()
		{
			declaration.Invoices.DeleteAll();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_Code = "ULT" + new Random().Next(1000000).ToString();
			ultimateConsignee.OH_FullName = "MR Ultimate Consignee";

			OrgCusCode ultimateConsigneeECNCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeECNCode.OK_CodeType = OrgCusCode.USACodeTypes.EncryptedConsigneeNumber;
			ultimateConsigneeECNCode.OK_CustomsRegNo = "-T16WHSL5CXR";
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.GetEntryWithType(Enterprise.Customs.Common.US.ImportMessageStatusList.MessageType.EntrySummary)[0];
			CusEntryHeader bcrEntry = declaration.ActiveEntryHeaders.GetEntryWithType(Enterprise.Customs.Common.US.ImportMessageStatusList.MessageType.BorderCargoRelease)[0];
			bcrEntry.US_UseConsigneeNameAddress = true;
			bcrEntry.CH_BGMReference = "USE C'NEE 1";
			PopulateMessage(bcrEntry, false);

			MQEDIMessage message = (MQEDIMessage)bcrEntry.Messages[0];
			ABIInputBlockControlGenerator generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			generator.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			AssertEquals("Application Identifier", ApplicationIdentifierCodeList.Codes.BorderCargoRelease, generator.B.ApplicationIdentifier);
			AssertEquals("MessageBlocks.Count", 3, generator.MessageBlocks.Count);

			BCR01 bcr01 = (BCR01)generator.MessageBlocks[0];

			AssertEquals("UpdateActionCode", "A", bcr01.UpdateActionCode);
			AssertEquals("DistrictPortOfEntry", "8888", bcr01.DistrictPortOfEntry);
			AssertEquals("FilerCode", "XJ5", bcr01.FilerCode);
			AssertEquals("ModeOfTransportationMOTCode", "21", bcr01.ModeOfTransportationMOTCode);
			AssertEquals("ImporterOfRecord", "91-013199000", bcr01.ImporterOfRecord);
			AssertEquals("BondType", 8, bcr01.BondType);
			AssertEquals("SuretyCode", "891", bcr01.SuretyCode);
			AssertEquals("UltimateConsignee", "-T16WHSL5CXR", bcr01.UltimateConsignee);
			AssertEquals("Consignee Name Address", ZString.Empty, bcr01.ConsigneeNameAndAddress);

			ultimateConsignee.CustomsCodes.RemoveAndDeleteAll();
			CusEntryHeader bcrEntry1 = declaration.CustomsEntryHeaders.AddNew();
			bcrEntry1.Declaration.US_EntryType = line.ImportEntryType;
			bcrEntry1.CH_CH_PrimeEntry = ensEntry.PK;
			CusEntryLine entryLine1 = bcrEntry1.MergedLines.AddNew();
			bcrEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			bcrEntry1.US_UseConsigneeNameAddress = true;
			bcrEntry.CH_BGMReference = "USE C'NEE 2";
			line.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);

			PopulateMessage(bcrEntry1, false);
			message = (MQEDIMessage)bcrEntry1.Messages[0];
			generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			generator.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			AssertEquals("Application Identifier", ApplicationIdentifierCodeList.Codes.BorderCargoRelease, generator.B.ApplicationIdentifier);

			bcr01 = (BCR01)generator.MessageBlocks[0];

			AssertEquals("UltimateConsignee", ZString.Empty, bcr01.UltimateConsignee);
			AssertEquals("Consignee Name Address", "1", bcr01.ConsigneeNameAndAddress);
			Factory.Save();
		}

		public void TestEndToEndWithPGAData()
		{
			declaration.Invoices.DeleteAll();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			PGA pGA = line.LaceyActLines.AddNew();
			pGA = DeclarationTestHelper.CreateLaceyActData(pGA);

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader bcrEntry = declaration.ActiveEntryHeaders.GetEntryWithType(Enterprise.Customs.Common.US.ImportMessageStatusList.MessageType.BorderCargoRelease)[0];
			PopulateMessage(bcrEntry, false);

			MQEDIMessage message = (MQEDIMessage)bcrEntry.Messages[0];
			ABIInputBlockControlGenerator generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			generator.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			AssertEquals("Application Identifier", ApplicationIdentifierCodeList.Codes.BorderCargoRelease, generator.B.ApplicationIdentifier);
			AssertEquals("MessageBlocks.Count", 9, generator.MessageBlocks.Count);

			AssertEquals("OI        SOFTWOOD PULPWOOD                                                     ", generator.MessageBlocks[3].Serialise());
			AssertEquals("PG01001AP                                                                       ", generator.MessageBlocks[4].Serialise());
			AssertEquals("PG04 SPRUCE                                              000000010000M3   020000", generator.MessageBlocks[5].Serialise());
			AssertEquals("PG05PICEA                 GLAUCA                                                ", generator.MessageBlocks[6].Serialise());
			AssertEquals("PG06HRVCA                                                                       ", generator.MessageBlocks[7].Serialise());
			AssertEquals("PG25                                               000000010000                 ", generator.MessageBlocks[8].Serialise());
		}

		// WI00025244
		public void TestEndToEndWithFDADataAndFDAValueWithSupTariff()
		{
			declaration.Invoices.DeleteAll();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.US_SupTariff = "9904.17.45"; // no FDA requirement
			line.JI_Tariff = "1901.90.5400"; // FD4
			line.JI_LinePrice = 10000m;
			line.US_UC_NKCountryOfOrigin = "AU";

			FDA fda = line.FDAs.AddNew();
			fda.US_FDAValue = 10000m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader bcrEntry = declaration.ActiveEntryHeaders.GetEntryWithType(Enterprise.Customs.Common.US.ImportMessageStatusList.MessageType.BorderCargoRelease)[0];
			PopulateMessage(bcrEntry, false);

			MQEDIMessage message = (MQEDIMessage)bcrEntry.Messages[0];
			ABIInputBlockControlGenerator generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			generator.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			AssertEquals("Application Identifier", ApplicationIdentifierCodeList.Codes.BorderCargoRelease, generator.B.ApplicationIdentifier);

			List<MessageBlock> fd03s = generator.MessageBlocks.FindAll(x => x is OGAFD03);
			AssertEquals(1, fd03s.Count);

			AssertEquals("PreCondition", 0m, line.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease, false).CL_CustomsValue);
			AssertEquals(10000m, ((OGAFD03)fd03s[0]).FDAValueByFDALine);

			List<MessageBlock> bcr02s = generator.MessageBlocks.FindAll(x => x is BCR02);
			AssertEquals(2, bcr02s.Count);

			foreach (BCR02 bcr02 in bcr02s)
			{
				AssertEquals(0m, bcr02.LineItemValue);
			}
		}

		[TestDate(2010, 8, 9)]
		public void TestPGADataBeforeOGAData()
		{
			declaration.Invoices.DeleteAll();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			PGA pGA = line.LaceyActLines.AddNew();
			pGA = DeclarationTestHelper.CreateLaceyActData(pGA);

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.FillWithValidTestData();
			line.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			helper.SetUpFDARequiredData(line, manufacturer, Factory);

			line.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			DOT dot = line.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			dot.US_DOTCommercialDesc = "O VH,>4<=6CYL,IN VL>2.8<=3";
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			DOTVIN dotVin = dot.DOTVINs.AddNew();
			dotVin.US_DOTYear = 1999;
			dotVin.US_DOTVIN = "IEWR87EWRKJWER87";

			line.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			FCC fcc = line.FCCs.AddNew();
			fcc.US_FCCID = "HELLO";
			fcc.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._03;
			fcc.US_FCCModel = "MODEL";
			fcc.US_FCCQty = 1;
			fcc.US_FCCTradeName = "TRADE NAME";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader bcrEntry = declaration.ActiveEntryHeaders.GetEntryWithType(Enterprise.Customs.Common.US.ImportMessageStatusList.MessageType.BorderCargoRelease)[0];
			PopulateMessage(bcrEntry, false);

			MQEDIMessage message = (MQEDIMessage)bcrEntry.Messages[0];
			AssertEquals("PGA blocks should be before other OGA blocks",
						@"B018888XJ5HN                                               <<MSGNO PLACEHOLDER>>
01A8888XJ5<E#PLCH>2191-013199000889191-01319900008031001 AAAA                   
0M                                                00000001PK         AAAA       
02                                                                              
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 
OI        O VH,>4<=6CYL,IN VL>2.8<=3                                            
DT0100105Y                          V                                           
DT02                              1999IEWR87EWRKJWER87                          
OI        TEST                                                                  
FD0100124DCS18   AU                                                             
FD020000900000KG                                                                
FD030000000000                                                                  
FD04              JESSIE JAM3273958841                                          
Y  8888XJ5HN00017", message.EM_FormattedMessageText);
		}

		public void TestProcessingDistrictPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2786", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2720", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var coll = new EntryProcessingPortsMappingCollection();
			var mapping = coll.AddNew();
			mapping.EntryPort = "2786";
			mapping.ProcessingPort = "2720";

			var declaration = Factory.New<JobDeclaration>();
			USCustomsDataRegistry.Instance.EntryProcessingPortMappings.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, coll);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_SchDEntry = "3409";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Border Cargo Release Entry", true, entry.IsBorderCargoRelease);

			PopulateMessage(entry, true);
			var message = (MQEDIMessage)entry.Messages[0];
			var generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			generator.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			AssertEquals("Application Identifier", ApplicationIdentifierCodeList.Codes.BorderCargoRelease, generator.B.ApplicationIdentifier);
			AssertEquals("ProcessingDistrictPortCode", declaration.US_SchDEntry, generator.B.ProcessingDistrictPortCode);

			var bcr01 = (BCR01)generator.MessageBlocks[0];

			AssertEquals("DistrictPortOfEntry", declaration.US_SchDEntry, bcr01.DistrictPortOfEntry);
		}

		protected override void PopulateMessage(CusEntryHeader entryHeader, bool certifyCargoRelease)
		{
			if (entryHeader.IsBorderCargoRelease)
			{
				BorderCargoReleaseMessageBuilder messageBuilder = new BorderCargoReleaseMessageBuilder(entryHeader, UpdateActionCode.Add);
				MQEDIMessage message = messageBuilder.PopulateMessage();
				entryHeader.Messages.Add(message);
			}
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "54901", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "L888", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			base.SetUp();
			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.RailContainer;

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EnableCRL = true;
			declaration.US_US_NKLocationOfGoods = "L888";
			declaration.US_EntryDate = declaration.JE_ExportDate.AddDays(1);
		}

		void AssertBCR02(BCR02 bcr02)
		{
			AssertEquals("CountryOfOrigin", "AU", bcr02.CountryOfOrigin);
			AssertEquals("TariffNumber", "4421909720", bcr02.TariffNumber);
			AssertEquals("ManufacturerIDCode", "AUSOUPAC195PAD", bcr02.ManufacturerIDCode);
			AssertEquals("UltimateConsignee", "", bcr02.UltimateConsignee);
			AssertEquals("LineItemValue is always zero as advised by Customs. This is to avoid 'VGO' rejections", 0m, bcr02.LineItemValue);
		}

		void AssertBCR0M(BCR0M bcr0m)
		{
			AssertEquals("MasterBillNumber", "OBL1", bcr0m.MasterBillNumber);
			AssertEquals("HouseBillNumber", ZString.Empty, bcr0m.HouseBillNumber);
			AssertEquals("SubHouseBillNumber", ZString.Empty, bcr0m.SubHouseBillNumber);
			AssertEquals("Quantity", 1, bcr0m.Quantity);
			AssertEquals("Unit", DeclarationTestHelper.BillUS_ManifestUQForTest, bcr0m.Unit);
			AssertEquals("IssuerOfMasterBillNumber", "AAAA", bcr0m.IssuerOfMasterBillNumber);
			AssertEquals("IssuerCodeOfHouseBillNumber", ZString.Empty, bcr0m.IssuerCodeOfHouseBillNumber);
		}

		void AssertBCR01(BCR01 bcr01, CusEntryHeader bcrEntry)
		{
			AssertEquals("UpdateActionCode", "A", bcr01.UpdateActionCode);
			AssertEquals("DistrictPortOfEntry", "8888", bcr01.DistrictPortOfEntry);
			AssertEquals("FilerCode", "XJ5", bcr01.FilerCode);
			AssertEquals("EntryNumber", bcrEntry.EntryNumber, bcr01.EntryNumber);
			AssertEquals("ModeOfTransportationMOTCode", "21", bcr01.ModeOfTransportationMOTCode);
			AssertEquals("ImporterOfRecord", "91-013199000", bcr01.ImporterOfRecord);
			AssertEquals("BondType", 8, bcr01.BondType);
			AssertEquals("SuretyCode", "891", bcr01.SuretyCode);
			AssertEquals("UltimateConsignee", "91-013199000", bcr01.UltimateConsignee);
			AssertEquals("DateOfArrival", declaration.US_EntryDate.Date, bcr01.DateOfArrival);
			AssertEquals("EntryType", "01", bcr01.EntryType);
			AssertEquals("EntryImmediateDeliveryIndicator", ZInt.Zero, bcr01.EntryImmediateDeliveryIndicator);
			AssertEquals("CarrierCode", "AAAA", bcr01.CarrierCode);
			AssertEquals("ConsigneeNameAndAddress", "", bcr01.ConsigneeNameAndAddress);
		}
	}
}
