using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ImporterSecurityFilingMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateISF5()
		{
			Mock<IImporterSecurityFiling> mock5 = GetMockedObject();
			mock5.CallBase = true;
			List<ITariffData> tariffs = new List<ITariffData>();
			var mockTariffs = new Mock<ITariffData> { CallBase = true };
			mockTariffs.Setup(m => m.HarmonizedTariffNumber).Returns("1001211");
			mockTariffs.Setup(m => m.CountryOfOrigin).Returns("IT");
			ITariffData tariff = mockTariffs.Object;
			tariffs.Add(tariff);
			mock5.Setup(m => m.Tariffs).Returns(tariffs);
			mock5.Setup(m => m.CodeQualifier1).Returns("11");
			mock5.Setup(m => m.ForeignPortOfUnlading).Returns("8888");
			mock5.Setup(m => m.CodeQualifier2).Returns("22");
			mock5.Setup(m => m.PlaceOfDelivery).Returns("5678");
			mock5.Setup(m => m.SFSubmissionType).Returns("2");
			IImporterSecurityFiling iSF = mock5.Object;
			var relatedOrganizationData = new List<IISFDocAddress>(iSF.RelatedOrganizationData);
			var mockRelatedOrganizationData = new Mock<IISFDocAddress> { CallBase = true };
			mockRelatedOrganizationData.Setup(m => m.E2_AddressType).Returns(AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			mockRelatedOrganizationData.Setup(m => m.E2_CompanyNameTruncated).Returns("name");
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNumType).Returns(OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNum).Returns("Identifier");
			mockRelatedOrganizationData.Setup(m => m.E2_Address1).Returns("Address 1");
			mockRelatedOrganizationData.Setup(m => m.E2_Address2).Returns("Address 2");
			mockRelatedOrganizationData.Setup(m => m.E2_City).Returns("City");
			mockRelatedOrganizationData.Setup(m => m.E2_State).Returns("AD");
			mockRelatedOrganizationData.Setup(m => m.E2_Postcode).Returns("1018");
			mockRelatedOrganizationData.Setup(m => m.CountryCode).Returns("AD");
			var org = mockRelatedOrganizationData.Object;
			relatedOrganizationData.Add(org);
			mock5.Setup(m => m.RelatedOrganizationData).Returns(relatedOrganizationData);
			MQEDIMessage message = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(iSF, UpdateActionCode.Add).PopulateMessage();
			AssertMultilineASCIIEquals("Message text", @"B         SF                                               <<MSGNO PLACEHOLDER>>
SF10203ACTAEF1234567890123450911200810               AAADBND123456789012019   IT
SF13020000002012000000000321PCS00000034234K                                     
SF15BM3335                                                                      
SF20V1 RRR                                                                      
SF25DEYYY 122            5T                                                     
SF30ST SHIPTO1 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               NSW      2018           AU               
SF30ST SHIPTO2 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               VIC      3000           AU               
SF30BKPBOOKING1 NAME                                                            
SF3515BOOKING ADDRESS 1BOOKING ADDRESS 2 15                                     
SF36CITY                               NSW      2018           AU               
SF30BKPBOOKING2 NAME                                                            
SF3515BOOKING ADDRESS 1BOOKING ADDRESS 2 15                                     
SF36CITY                               VIC      3018           AU               
SF30ST SHIPTO3 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               TAS      4018           AU               
SF30ST SHIPTO4 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               WA       6018           AU               
SF30BKPNAME                                                                     
SF3515ADDRESS 1ADDRESS 2                 15                                     
SF36CITY                               AD       1018           AD               
SF401001211                                                                     
SF5011 8888           22 5678                                                   
Y         SF", message.EM_FormattedMessageText);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNumType).Returns(string.Empty);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNum).Returns(string.Empty);
			message = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(iSF, UpdateActionCode.Add).PopulateMessage();
			AssertMultilineASCIIEquals("Message text", @"B         SF                                               <<MSGNO PLACEHOLDER>>
SF10203ACTAEF1234567890123450911200810               AAADBND123456789012019   IT
SF13020000002012000000000321PCS00000034234K                                     
SF15BM3335                                                                      
SF20V1 RRR                                                                      
SF25DEYYY 122            5T                                                     
SF30ST SHIPTO1 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               NSW      2018           AU               
SF30ST SHIPTO2 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               VIC      3000           AU               
SF30BKPBOOKING1 NAME                                                            
SF3515BOOKING ADDRESS 1BOOKING ADDRESS 2 15                                     
SF36CITY                               NSW      2018           AU               
SF30BKPBOOKING2 NAME                                                            
SF3515BOOKING ADDRESS 1BOOKING ADDRESS 2 15                                     
SF36CITY                               VIC      3018           AU               
SF30ST SHIPTO3 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               TAS      4018           AU               
SF30ST SHIPTO4 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               WA       6018           AU               
SF30BKPNAME                                                                     
SF3515ADDRESS 1ADDRESS 2                 15                                     
SF36CITY                               AD       1018           AD               
SF401001211                                                                     
SF5011 8888           22 5678                                                   
Y         SF", message.EM_FormattedMessageText);
		}

		public void TestGenerateISF10()
		{
			var mock10 = GetMockedObject();
			var manufacturerData = new List<IManufacturerData>();
			var mockManufacturerData = new Mock<IManufacturerData> { CallBase = true };
			var mockRelatedOrganizationData = new Mock<IISFDocAddress> { CallBase = true };
			mockRelatedOrganizationData.Setup(m => m.E2_AddressType).Returns(AutoDocAddressTypes.Codes.Manufacturer);
			mockRelatedOrganizationData.Setup(m => m.E2_CompanyNameTruncated).Returns("name");
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNumType).Returns(OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNum).Returns("Identifier");
			mockRelatedOrganizationData.Setup(m => m.E2_Address1).Returns("Address 1");
			mockRelatedOrganizationData.Setup(m => m.E2_Address2).Returns("Address 2");
			mockRelatedOrganizationData.Setup(m => m.E2_City).Returns("City");
			mockRelatedOrganizationData.Setup(m => m.E2_State).Returns("AD");
			mockRelatedOrganizationData.Setup(m => m.E2_Postcode).Returns("1018");
			mockRelatedOrganizationData.Setup(m => m.CountryCode).Returns("AD");
			var org = mockRelatedOrganizationData.Object;
			mockManufacturerData.Setup(m => m.Manufacturer).Returns(org);
			var tariffs = new List<ITariffData>();
			var mockTariffs = new Mock<ITariffData> { CallBase = true };
			mockTariffs.Setup(m => m.HarmonizedTariffNumber).Returns("1001211");
			mockTariffs.Setup(m => m.CountryOfOrigin).Returns("IT");
			ITariffData tariff = mockTariffs.Object;
			tariffs.Add(tariff);
			mockManufacturerData.Setup(m => m.Tariffs).Returns(tariffs);
			IManufacturerData manufacturer = mockManufacturerData.Object;
			manufacturerData.Add(manufacturer);
			mock10.Setup(m => m.ManufacturerData).Returns(manufacturerData);
			mock10.Setup(m => m.ConsigneeFullName).Returns("BOB THE BUILDER");
			mock10.Setup(m => m.ConsigneeNumber).Returns("MID234322");
			mock10.Setup(m => m.ConsigneeNumberQualifier).Returns("MID");
			mock10.Setup(m => m.ConsigneePassportCountryOfIssue).Returns("AU");
			mock10.Setup(m => m.ConsigneePassportDateOfBirth).Returns(new ZDate(1980, 4, 25));
			mock10.Setup(m => m.SFSubmissionType).Returns("1");
			IImporterSecurityFiling iSF = mock10.Object;
			MQEDIMessage message = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(iSF, UpdateActionCode.Replace).PopulateMessage();
			AssertMultilineASCIIEquals("Message text", @"B         SF                                               <<MSGNO PLACEHOLDER>>
SF10103RCTAEF1234567890123450911200810S              AAADBND123456789012019   IT
SF13020000002012000000000321PCS00000034234K                                     
SF15BM3335                                                                      
SF20V1 RRR                                                                      
SF25DEYYY 122            5T                                                     
SF30IM WENDY THE DESTROYER                AEF123456789012345     IT09112008     
SF30CN BOB THE BUILDER                    MIDMID234322           AU04251980     
SF30ST SHIPTO1 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               NSW      2018           AU               
SF30ST SHIPTO2 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               VIC      3000           AU               
SF30SE SELLINGPARTY1 NAME                                                       
SF3515SELLINGPARTY ADDRESS 1SELLINGPARTY 15ADDRESS 2                            
SF36CITY                               NSW      2018           AU               
SF30SE SELLINGPARTY2 NAME                                                       
SF3515SELLINGPARTY ADDRESS 1SELLINGPARTY 15ADDRESS 2                            
SF36CITY                               VIC      3018           AU               
SF30BY BUYINGPARTY1 NAME                                                        
SF3515BUYINGPARTY ADDRESS 1BUYINGPARTY AD15DRESS 2                              
SF36CITY                               NSW      2018           AU               
SF30BY BUYINGPARTY2 NAME                                                        
SF3515BUYINGPARTY ADDRESS 1BUYINGPARTY AD15DRESS 2                              
SF36CITY                               VIC      3018           AU               
SF30LG STUFFING1 NAME                                                           
SF3515STUFFING ADDRESS 1STUFFING ADDRESS 152                                    
SF36CITY                               NSW      2018           AU               
SF30LG STUFFING2 NAME                                                           
SF3515STUFFING ADDRESS 1STUFFING ADDRESS 152                                    
SF36CITY                               VIC      3018           AU               
SF30CS CONSOLIDATOR1 NAME                                                       
SF3515CONSOLIDATOR ADDRESS 1CONSOLIDATOR 15ADDRESS 2                            
SF36CITY                               NSW      2018           AU               
SF30CS CONSOLIDATOR2 NAME                                                       
SF3515CONSOLIDATOR ADDRESS 1CONSOLIDATOR 15ADDRESS 2                            
SF36CITY                               VIC      3018           AU               
SF30CN BOB                                AEFCONSIGNEE123        AU09181971     
SF30ST SHIPTO3 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               TAS      4018           AU               
SF30ST SHIPTO4 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               WA       6018           AU               
SF30MF NAME                                                                     
SF3515ADDRESS 1ADDRESS 2                 15                                     
SF36CITY                               AD       1018           AD               
SF401001211   IT                                                                
Y         SF", message.EM_FormattedMessageText);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNumType).Returns(string.Empty);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNum).Returns(string.Empty);
			message = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(iSF, UpdateActionCode.Replace).PopulateMessage();
			AssertMultilineASCIIEquals("Message text", @"B         SF                                               <<MSGNO PLACEHOLDER>>
SF10103RCTAEF1234567890123450911200810S              AAADBND123456789012019   IT
SF13020000002012000000000321PCS00000034234K                                     
SF15BM3335                                                                      
SF20V1 RRR                                                                      
SF25DEYYY 122            5T                                                     
SF30IM WENDY THE DESTROYER                AEF123456789012345     IT09112008     
SF30CN BOB THE BUILDER                    MIDMID234322           AU04251980     
SF30ST SHIPTO1 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               NSW      2018           AU               
SF30ST SHIPTO2 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               VIC      3000           AU               
SF30SE SELLINGPARTY1 NAME                                                       
SF3515SELLINGPARTY ADDRESS 1SELLINGPARTY 15ADDRESS 2                            
SF36CITY                               NSW      2018           AU               
SF30SE SELLINGPARTY2 NAME                                                       
SF3515SELLINGPARTY ADDRESS 1SELLINGPARTY 15ADDRESS 2                            
SF36CITY                               VIC      3018           AU               
SF30BY BUYINGPARTY1 NAME                                                        
SF3515BUYINGPARTY ADDRESS 1BUYINGPARTY AD15DRESS 2                              
SF36CITY                               NSW      2018           AU               
SF30BY BUYINGPARTY2 NAME                                                        
SF3515BUYINGPARTY ADDRESS 1BUYINGPARTY AD15DRESS 2                              
SF36CITY                               VIC      3018           AU               
SF30LG STUFFING1 NAME                                                           
SF3515STUFFING ADDRESS 1STUFFING ADDRESS 152                                    
SF36CITY                               NSW      2018           AU               
SF30LG STUFFING2 NAME                                                           
SF3515STUFFING ADDRESS 1STUFFING ADDRESS 152                                    
SF36CITY                               VIC      3018           AU               
SF30CS CONSOLIDATOR1 NAME                                                       
SF3515CONSOLIDATOR ADDRESS 1CONSOLIDATOR 15ADDRESS 2                            
SF36CITY                               NSW      2018           AU               
SF30CS CONSOLIDATOR2 NAME                                                       
SF3515CONSOLIDATOR ADDRESS 1CONSOLIDATOR 15ADDRESS 2                            
SF36CITY                               VIC      3018           AU               
SF30CN BOB                                AEFCONSIGNEE123        AU09181971     
SF30ST SHIPTO3 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               TAS      4018           AU               
SF30ST SHIPTO4 NAME                                                             
SF3515SHIPTO ADDRESS 1SHIPTO ADDRESS 2   15                                     
SF36CITY                               WA       6018           AU               
SF30MF NAME                                                                     
SF3515ADDRESS 1ADDRESS 2                 15                                     
SF36CITY                               AD       1018           AD               
SF401001211   IT                                                                
Y         SF", message.EM_FormattedMessageText);
		}

		public void TestUpdateBBlockNumber()
		{
			ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var header = Factory.New<CusISFHeader>();
			bool oldAllowed = Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed;
			try
			{
				AddMessage(header, "2709", "!11111", new ZDateTime(2016, 12, 12), true, true);
				var builder = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(header, UpdateActionCode.Delete);
				var message = builder.PopulateMessage();
				AssertEquals(true, message.EM_MessageText.Contains("B  8888XJ5SF                                               <<MSGNO PLACEHOLDER>>"));
				AddMessage(header, "2709", "!11111", new ZDateTime(2016, 12, 12), true, true);
				var builder2 = new ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>(header, UpdateActionCode.Delete);
				message = builder2.PopulateMessage();
				AssertEquals(true, message.EM_MessageText.Contains("B018888XJ5SF                                               <<MSGNO PLACEHOLDER>>"));
			}
			finally
			{
				Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		MQEDIMessage AddMessage(CusISFHeader entry, ZString port, ZString messageNum, ZDateTime createTime, bool addISFAcceptedText, bool isACE)
		{
			string isfAcceptedText = addISFAcceptedText ? "SF9002   ISF ACCEPTED                                                           " : "";
			var message = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message.EM_MessageSubType = "ADD";
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = string.Format("B{0}{1}XJ5SN                                               {2}" +
				"SF10101ACTEI 91-013199000           11XJ5-19756574533    91-013199000   018     " +
				"SF15OBAPLU1112222                                                               " +
				"{3}Y  {1}XJ5SN00003", isACE ? "  " : "01", port.Left(4).PadRight(4), messageNum.Left(21).PadRight(21), isfAcceptedText);
			message.EM_SystemCreateTimeUtc = createTime;
			return message;
		}

		public void TestDOBIncludedForSSN()
		{
			var mock10 = GetMockedObject();
			mock10.Setup(m => m.IORNumberQualifier).Returns("34");
			mock10.Setup(m => m.SFSubmissionType).Returns("1");
			var manufacturerData = new List<IManufacturerData>();
			var mockManufacturerData = new Mock<IManufacturerData> { CallBase = true };
			var mockRelatedOrganizationData = new Mock<IISFDocAddress> { CallBase = true };
			mockRelatedOrganizationData.Setup(m => m.E2_AddressType).Returns(AutoDocAddressTypes.Codes.Manufacturer);
			mockRelatedOrganizationData.Setup(m => m.E2_CompanyNameTruncated).Returns("name");
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNumType).Returns(OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNum).Returns("Identifier");
			mockRelatedOrganizationData.Setup(m => m.E2_Address1).Returns("Address 1");
			mockRelatedOrganizationData.Setup(m => m.E2_Address2).Returns("Address 2");
			mockRelatedOrganizationData.Setup(m => m.E2_City).Returns("City");
			mockRelatedOrganizationData.Setup(m => m.E2_State).Returns("AD");
			mockRelatedOrganizationData.Setup(m => m.E2_Postcode).Returns("1018");
			mockRelatedOrganizationData.Setup(m => m.CountryCode).Returns("AD");
			var org = mockRelatedOrganizationData.Object;
			mockManufacturerData.Setup(m => m.Manufacturer).Returns(org);
			var tariffs = new List<ITariffData>();
			var mockTariffs = new Mock<ITariffData> { CallBase = true };
			mockTariffs.Setup(m => m.HarmonizedTariffNumber).Returns("1001211");
			mockTariffs.Setup(m => m.CountryOfOrigin).Returns("IT");
			ITariffData tariff = mockTariffs.Object;
			tariffs.Add(tariff);
			mockManufacturerData.Setup(m => m.Tariffs).Returns(tariffs);
			IManufacturerData manufacturer = mockManufacturerData.Object;
			manufacturerData.Add(manufacturer);
			mock10.Setup(m => m.ManufacturerData).Returns(manufacturerData);
			mock10.Setup(m => m.ConsigneeFullName).Returns("BOB THE BUILDER");
			mock10.Setup(m => m.ConsigneeNumber).Returns("MID234322");
			mock10.Setup(m => m.ConsigneeNumberQualifier).Returns("MID");
			mock10.Setup(m => m.ConsigneePassportCountryOfIssue).Returns("AU");
			mock10.Setup(m => m.ConsigneePassportDateOfBirth).Returns(new ZDate(1980, 4, 25));
			var iSF = mock10.Object;
			var message = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(iSF, UpdateActionCode.Add).PopulateMessage();
			var isf10 = (ISFSF10)message.MessageBlock.MessageBlocks.Find(x => x is ISFSF10);
			AssertEquals(new ZDate(2008, 9, 11), isf10.DateOfBirth);
		}

		public void TestMessageAreCreatedInRightBranch()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var branch1PK = branch1.PK.ToGuid();
			var mock10 = GetMockedObject();
			mock10.Setup(m => m.IORNumberQualifier).Returns("34");
			mock10.Setup(m => m.SFSubmissionType).Returns("1");
			var manufacturerData = new List<IManufacturerData>();
			var mockManufacturerData = new Mock<IManufacturerData> { CallBase = true };
			var mockRelatedOrganizationData = new Mock<IISFDocAddress> { CallBase = true };
			mockRelatedOrganizationData.Setup(m => m.E2_AddressType).Returns(AutoDocAddressTypes.Codes.Manufacturer);
			mockRelatedOrganizationData.Setup(m => m.E2_CompanyNameTruncated).Returns("name");
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNumType).Returns(OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			mockRelatedOrganizationData.Setup(m => m.E2_GovRegNum).Returns("Identifier");
			mockRelatedOrganizationData.Setup(m => m.E2_Address1).Returns("Address 1");
			mockRelatedOrganizationData.Setup(m => m.E2_Address2).Returns("Address 2");
			mockRelatedOrganizationData.Setup(m => m.E2_City).Returns("City");
			mockRelatedOrganizationData.Setup(m => m.E2_State).Returns("AD");
			mockRelatedOrganizationData.Setup(m => m.E2_Postcode).Returns("1018");
			mockRelatedOrganizationData.Setup(m => m.CountryCode).Returns("AD");
			var org = mockRelatedOrganizationData.Object;
			mockManufacturerData.Setup(m => m.Manufacturer).Returns(org);
			var tariffs = new List<ITariffData>();
			var mockTariffs = new Mock<ITariffData> { CallBase = true };
			mockTariffs.Setup(m => m.HarmonizedTariffNumber).Returns("1001211");
			mockTariffs.Setup(m => m.CountryOfOrigin).Returns("IT");
			ITariffData tariff = mockTariffs.Object;
			tariffs.Add(tariff);
			mockManufacturerData.Setup(m => m.Tariffs).Returns(tariffs);
			IManufacturerData manufacturer = mockManufacturerData.Object;
			manufacturerData.Add(manufacturer);
			mock10.Setup(m => m.ManufacturerData).Returns(manufacturerData);
			mock10.Setup(m => m.ConsigneeFullName).Returns("BOB THE BUILDER");
			mock10.Setup(m => m.ConsigneeNumber).Returns("MID234322");
			mock10.Setup(m => m.ConsigneeNumberQualifier).Returns("MID");
			mock10.Setup(m => m.ConsigneePassportCountryOfIssue).Returns("AU");
			mock10.Setup(m => m.ConsigneePassportDateOfBirth).Returns(new ZDate(1980, 4, 25));
			mock10.Setup(m => m.Branch).Returns(branch1);
			var iSF = mock10.Object;
			var message = new ImporterSecurityFilingMessageBuilder<ACEInputBlockControlGenerator, AABIInputB, AABIInputY>(iSF, UpdateActionCode.Add).PopulateMessage();
			AssertEquals(branch1.PK, message.EM_GB);
		}

		Mock<IImporterSecurityFiling> GetMockedObject()
		{
			var factory = new BusinessObjectFactory();
			var mockA = new Mock<IImporterSecurityFiling> { CallBase = true };
			var mockB = mockA.As<IMessageAttachee>();
			mockA.Setup(m => m.Messages).Returns(new EDIMessageCollection(factory.New<CusISFHeader>()));
			mockA.Setup(m => m.Branch).Returns(GlbBranch.CurrentBranch);
			mockB.Setup(m => m.Factory).Returns(factory);
			mockA.Setup(m => m.ShipmentTypeCode).Returns("03");
			mockA.Setup(m => m.ActionReasonCode).Returns("CT");
			mockA.Setup(m => m.IORNumberQualifier).Returns("AEF");
			mockA.Setup(m => m.IORNumber).Returns("123456789012345");
			mockA.Setup(m => m.DateOfBirth).Returns(new ZDate(2008, 9, 11));
			mockA.Setup(m => m.ModeOfTransportation).Returns("10");
			mockA.Setup(m => m.SFTransactionNumber).Returns("S");
			mockA.Setup(m => m.SCAC).Returns("AAAD");
			mockA.Setup(m => m.ISFImporterBondHolder).Returns("BND123456789012");
			mockA.Setup(m => m.ISFBondIndicator).Returns(ZBool.False);
			mockA.Setup(m => m.CountryOfIssuance).Returns("IT");
			mockA.Setup(m => m.ISFBondActivityCode).Returns("01");
			mockA.Setup(m => m.ISFBondType).Returns("9");
			mockA.Setup(m => m.ImporterFullName).Returns("WENDY THE DESTROYER");
			mockA.Setup(m => m.ShipmentSubType).Returns("02");
			mockA.Setup(m => m.EstimatedValue).Returns(20120);
			mockA.Setup(m => m.EstimatedQuantity).Returns(321);
			mockA.Setup(m => m.UnitOfMeasure).Returns("PCS");
			mockA.Setup(m => m.EstimatedWeight).Returns(34234);
			mockA.Setup(m => m.WeightQualifier).Returns("K");
			var shipmentReference = new List<IShipmentReferenceID>();
			var mockShipmentRef = new Mock<IShipmentReferenceID> { CallBase = true };
			mockShipmentRef.Setup(m => m.CodeQualifier).Returns(ShipmentReferenceIdentifierTypeList.Codes.HouseBillOfLading);
			mockShipmentRef.Setup(m => m.ShipmentReferenceIdentifier).Returns("3335");
			IShipmentReferenceID shipRef = mockShipmentRef.Object;
			shipmentReference.Add(shipRef);
			mockA.Setup(m => m.ShipmentIDs).Returns(shipmentReference);
			var refData = new List<IReferenceData>();
			var mockRefData = new Mock<IReferenceData> { CallBase = true };
			mockRefData.Setup(m => m.CodeQualifier).Returns(ReferenceDataCodeList.Codes.SuretyCode);
			mockRefData.Setup(m => m.ReferenceData).Returns("RRR");
			IReferenceData data = mockRefData.Object;
			refData.Add(data);
			mockA.Setup(m => m.ReferenceData).Returns(refData);
			var containerData = new List<IContainerData>();
			var mockContainerData = new Mock<IContainerData> { CallBase = true };
			mockContainerData.Setup(m => m.EquipmentDescriptionCode).Returns("DE");
			mockContainerData.Setup(m => m.EquipmentInitial).Returns("YYY");
			mockContainerData.Setup(m => m.EquipmentNumber).Returns("122");
			mockContainerData.Setup(m => m.EquipmentNumberCheckDigit).Returns("5");
			mockContainerData.Setup(m => m.EquipmentSizeTypeCode).Returns("T");
			IContainerData cont = mockContainerData.Object;
			containerData.Add(cont);
			mockA.Setup(m => m.ContainerData).Returns(containerData);
			var relatedOrganizationData = new List<IISFDocAddress>();
			var org = GetDocAddress(AutoDocAddressTypes.Codes.Manufacturer, "name", "Address 1", "Address 2", "City", "AD", "1018", "AD", OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "Identifier", "", "", ZDate.Empty, "", ZDate.Empty, "");
			relatedOrganizationData.Add(org);
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ShipToParty, "SHIPTO1 NAME", "SHIPTO ADDRESS 1", "SHIPTO ADDRESS 2", "CITY", "NSW", "2018", "AU", OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4, "DUNS+41123", "SHIPTO123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ShipToParty, "SHIPTO2 NAME", "SHIPTO ADDRESS 1", "SHIPTO ADDRESS 2", "CITY", "VIC", "3000", "AU", "", "", "SHIPTO123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.SellingParty, "SELLINGPARTY1 NAME", "SELLINGPARTY ADDRESS 1", "SELLINGPARTY ADDRESS 2", "CITY", "NSW", "2018", "AU", OrgCusCode.CodeTypes.CarrierCode, "CD34", "SELLING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.SellingParty, "SELLINGPARTY2 NAME", "SELLINGPARTY ADDRESS 1", "SELLINGPARTY ADDRESS 2", "CITY", "VIC", "3018", "AU", "", "", "SELLING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.BuyingParty, "BUYINGPARTY1 NAME", "BUYINGPARTY ADDRESS 1", "BUYINGPARTY ADDRESS 2", "CITY", "NSW", "2018", "AU", OrgCusCode.USACodeTypes.SocialSecurityNumber, "446854646", "BUYING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.BuyingParty, "BUYINGPARTY2 NAME", "BUYINGPARTY ADDRESS 1", "BUYINGPARTY ADDRESS 2", "CITY", "VIC", "3018", "AU", "", "", "BUYING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ScheduledContainerStuffingLocation, "STUFFING1 NAME", "STUFFING ADDRESS 1", "STUFFING ADDRESS 2", "CITY", "NSW", "2018", "AU", OrgCusCode.USACodeTypes.CBPAssignedNumber, "323dDSSD", "STUFFING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ScheduledContainerStuffingLocation, "STUFFING2 NAME", "STUFFING ADDRESS 1", "STUFFING ADDRESS 2", "CITY", "VIC", "3018", "AU", "", "", "STUFFING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.Consolidator, "CONSOLIDATOR1 NAME", "CONSOLIDATOR ADDRESS 1", "CONSOLIDATOR ADDRESS 2", "CITY", "NSW", "2018", "AU", OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, "CN234234", "CONSOLIDATOR123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.Consolidator, "CONSOLIDATOR2 NAME", "CONSOLIDATOR ADDRESS 1", "CONSOLIDATOR ADDRESS 2", "CITY", "VIC", "3018", "AU", "", "", "CONSOLIDATOR123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ConsigneeAddress, "CONSIGNEE1 NAME", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "CITY", "NSW", "2018", "AU", OrgCusCode.CodeTypes.PassportID, "PAS234234", "CONSIGNEE123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ConsigneeAddress, "CONSIGNEE2 NAME", "CONSIGNEE ADDRESS 1", "CONSIGNEE ADDRESS 2", "CITY", "VIC", "3018", "AU", "", "", "CONSIGNEE123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, "BOOKING1 NAME", "BOOKING ADDRESS 1", "BOOKING ADDRESS 2", "CITY", "NSW", "2018", "AU", OrgCusCode.USACodeTypes.FIRMSCode, "FR23", "BOOKING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, "BOOKING2 NAME", "BOOKING ADDRESS 1", "BOOKING ADDRESS 2", "CITY", "VIC", "3018", "AU", "", "", "BOOKING123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ShipToParty, "SHIPTO3 NAME", "SHIPTO ADDRESS 1", "SHIPTO ADDRESS 2", "CITY", "TAS", "4018", "AU", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "EI41123", "SHIPTO123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			relatedOrganizationData.Add(GetDocAddress(AutoDocAddressTypes.Codes.ShipToParty, "SHIPTO4 NAME", "SHIPTO ADDRESS 1", "SHIPTO ADDRESS 2", "CITY", "WA", "6018", "AU", OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS1123", "SHIPTO123", "AU", ZDate.BrettsBirthday, "SSN-56-8735", ZDate.BrettsBirthday.AddYears(2), "BOB"));
			mockA.Setup(m => m.RelatedOrganizationData).Returns(relatedOrganizationData);
			return mockA;
		}

		IISFDocAddress GetDocAddress(ZString addressType, ZString companyName, ZString address1, ZString address2, ZString city, ZString state, ZString postcode, ZString countryCode, ZString govRegNumType, ZString govRegNum, ZString passportID, ZString passportCountryOfIssue, ZDate passportDateOfBirth, ZString socialSecurityNumber, ZDate socialSecurityNumberDateOfBirth, ZString contact)
		{
			var mock = new Mock<IISFDocAddress> { CallBase = true };
			mock.Setup(m => m.E2_AddressType).Returns(addressType);
			mock.Setup(m => m.E2_CompanyNameTruncated).Returns(companyName);
			mock.Setup(m => m.E2_Address1).Returns(address1);
			mock.Setup(m => m.E2_Address2).Returns(address2);
			mock.Setup(m => m.E2_City).Returns(city);
			mock.Setup(m => m.E2_State).Returns(state);
			mock.Setup(m => m.E2_Postcode).Returns(postcode);
			mock.Setup(m => m.CountryCode).Returns(countryCode);
			mock.Setup(m => m.E2_GovRegNumType).Returns(govRegNumType);
			mock.Setup(m => m.E2_GovRegNum).Returns(govRegNum);
			mock.Setup(m => m.E2_PassportID).Returns(passportID);
			mock.Setup(m => m.E2_PassportCountryOfIssue).Returns(passportCountryOfIssue);
			mock.Setup(m => m.E2_PassportDateOfBirth).Returns(passportDateOfBirth);
			mock.Setup(m => m.E2_SocialSecurityNumber).Returns(socialSecurityNumber);
			mock.Setup(m => m.E2_SocialSecurityNumberDateOfBirth).Returns(socialSecurityNumberDateOfBirth);
			mock.Setup(m => m.E2_Contact).Returns(contact);
			return mock.Object;
		}
	}
}
