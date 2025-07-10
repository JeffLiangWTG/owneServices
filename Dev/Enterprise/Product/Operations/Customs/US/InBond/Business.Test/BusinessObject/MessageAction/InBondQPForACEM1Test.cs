using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class InBondQPForACEM1Test : TestCaseWithFactory
	{
		public void TestEndToEndSend()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "OTT1";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 323.25m;
			moveHeader.BM_InBondCarrierID = "123456789012";
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_HouseBillNumber = "10987654321";
			bill.B0_PortOfLadingKCode = "35987";
			bill.B0_ManifestQty = 436;
			bill.B0_ManifestUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			bill.B0_Weight = 974.64m;
			bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill.B0_Volume = 3.54m;
			bill.B0_VolumeUQ = VolumeUnitList.Codes.Cord;
			bill.B0_PlaceOfReceiptDCode = "9853";
			bill.ForeignShipper.E2_AddressOverride = ZBool.True;
			bill.ForeignShipper.E2_CompanyName = "BOB THE BUILDER";
			bill.ForeignShipper.E2_RN_NKCountryCode = ZString.Empty;
			bill.Consignee.E2_AddressOverride = ZBool.True;
			bill.Consignee.E2_CompanyName = "WENDY THE DESTROYER";
			bill.Consignee.E2_RN_NKCountryCode = ZString.Empty;
			bill.NotifyParty.E2_AddressOverride = ZBool.True;
			bill.NotifyParty.E2_CompanyName = "JACK THE NOBODY";
			bill.NotifyParty.E2_RN_NKCountryCode = ZString.Empty;
			var ref1 = bill.AdditionalReferences.AddNew();
			ref1.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			ref1.BR_ReferenceNum = "2011CQ003948458";
			var ref2 = bill.AdditionalReferences.AddNew();
			ref2.BR_Qualifier = ReferenceQualifierList.Codes.FP;
			ref2.BR_ReferenceNum = "FP342332";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			moveDetail.B9_PreviousITType = EntryTypeList.Codes.ConsumptionFTZ;
			moveDetail.B9_PreviousITNumber = "IT3234232";
			moveDetail.B9_InBoundQty = 124;
			var snp1 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.First, "SNP1");
			var snp2 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.Second, "SNP2");
			var snp3 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.Third, "SNP3");
			var snp4 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.Fourth, "SNP4");
			var refContType = Factory.New<RefContainer>();
			refContType.RC_Code = "RCD";
			refContType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "TORU324234";
			container.BC_RC = refContType.PK;
			container.BC_Seal1 = "SL1";
			container.BC_Seal2 = "SL2";
			var commodity1 = container.Commodities.AddNew();
			commodity1.BY_HarmonisedTariff = "3020453123";
			commodity1.BY_MonetaryValue = 1500m;
			commodity1.BY_Description = "DESC 2342";
			commodity1.BY_GrossWeight = 42.43m;
			commodity1.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			commodity1.BY_PieceCount = 492;
			commodity1.BY_MarksAndNumbers = "MARKS AND FUNNY NUMBERS";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "B!@34";
			org.OH_FullName = "THIS IS THE COMPANY";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB BROWN";
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1!10";
			subs.DG_Variant = "c";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "2.1";
			subs.DG_FlashPoint = "-10";
			subs.DG_PSN = "SD WHO WHERE";
			var undg = container.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGFlashPoint = -10m;
			undg.DI_TechnicalName = "WHAT TECH NAME";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			var message = sendingObject.Send();
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                               <<MSGNO PLACEHOLDER>>
10A62123456789   OTT112341234500000323123456789012 Y                            
20ABC 11  APL VESSEL             1234A       5687100212                         
30A 0001    12345678901     10987654321                             0000000124  
328888XJ5  SNP2     SNP3     SNP4                                               
33FEN2011CQ003948458                                                            
33FP FP342332                                                                   
40359870000000436AE   0000000975LB0000000004DD9853                              
65                                                                              
65TORU324234    SL1            SL2            40                                
703020453123 000015000000000043LB                                               
710000000492DESC 2342                                    AE                     
72MARKS AND FUNNY NUMBERS                                                       
75UN1!10    2.1 ISD WHO WHERE                  BOB BROWN               010CEN   
76                             WHAT TECH NAME                                   
Y  8888XJ5QP", message.EM_FormattedMessageText);
			sendingObject = new InBondMessageSendingObject(moveHeader, bill, InBondMessageType.DepartureBillDelete);
			message = sendingObject.Send();
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                               <<MSGNO PLACEHOLDER>>
10B62123456789   OTT112341234500000323123456789012 Y                            
30D 0001    12345678901     10987654321                                         
Y  8888XJ5QP", message.EM_FormattedMessageText);
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureDelete);
			message = sendingObject.Send();
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                               <<MSGNO PLACEHOLDER>>
10D62123456789   OTT1         00000000                                          
Y  8888XJ5QP", message.EM_FormattedMessageText);
			moveDetail.B9_PreviousITType = EntryTypeList.Codes.ImmediateTransportation;
			Factory.Save();
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			message = sendingObject.Send();
			AssertMultilineASCIIEquals("Previous In-Bond Number should not be sent, because FTZ move: BH_FTZMove", @"B  8888XJ5QP                                               <<MSGNO PLACEHOLDER>>
10A62123456789   OTT112341234500000323123456789012 Y                            
20ABC 11  APL VESSEL             1234A       5687100212                         
30A 0001    12345678901     10987654321                 IT3234232   0000000124  
328888XJ5  SNP2     SNP3     SNP4                                               
33FEN2011CQ003948458                                                            
33FP FP342332                                                                   
40359870000000436AE   0000000975LB0000000004DD9853                              
65                                                                              
65TORU324234    SL1            SL2            40                                
703020453123 000015000000000043LB                                               
710000000492DESC 2342                                    AE                     
72MARKS AND FUNNY NUMBERS                                                       
75UN1!10    2.1 ISD WHO WHERE                  BOB BROWN               010CEN   
76                             WHAT TECH NAME                                   
Y  8888XJ5QP", message.EM_FormattedMessageText);
			header.BH_FTZMove = ZBool.False;
			Factory.Save();
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			message = sendingObject.Send();
			AssertMultilineASCIIEquals("Previous In-Bond Number can be sent", @"B  8888XJ5QP                                               <<MSGNO PLACEHOLDER>>
10A62123456789   OTT112341234500000323123456789012 Y                            
20ABC 11  APL VESSEL             1234A       5687100212                         
30A 0001    12345678901     10987654321                 IT3234232   0000000124  
328888XJ5  SNP2     SNP3     SNP4                                               
33FEN2011CQ003948458                                                            
33FP FP342332                                                                   
40359870000000436AE   0000000975LB0000000004DD9853                              
65                                                                              
65TORU324234    SL1            SL2            40                                
703020453123 000015000000000043LB                                               
710000000492DESC 2342                                    AE                     
72MARKS AND FUNNY NUMBERS                                                       
75UN1!10    2.1 ISD WHO WHERE                  BOB BROWN               010CEN   
76                             WHAT TECH NAME                                   
Y  8888XJ5QP", message.EM_FormattedMessageText);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			sendingObject = new InBondMessageSendingObject(moveHeader, bill, InBondMessageType.AirBillDelete);
			message = sendingObject.Send();
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                               <<MSGNO PLACEHOLDER>>
10B62123456789   OTT112341234500000323123456789012 Y                            
30D 0001123 45678901        10987654321                                         
Y  8888XJ5QP", message.EM_FormattedMessageText);
		}

		public void TestInBondDeleteMessageParametersComeFromAddMessage()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.InBondNumber = "777888125";
			moveHeader.BM_InBondCarrierSCAC = "NFCT";
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var outgoingMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = @"B018888XJ5QP                                               <<MSGNO PLACEHOLDER>>10A62123456789   OTT112341234500000323123456789012 Y                            20ABC 11  APL VESSEL             1234A       5687100212                         30A 0001ABC 12345678901                                             0000000124  Y  8888XJ5QP00014";
			Factory.Save();
			moveHeader.Reload();
			moveHeader.Messages.Load();
			var deleteMessageSendingObject = new InBondDeleteMessageSendingObject(moveHeader);
			var inbondQPHeader = (IInBondQPHeader)deleteMessageSendingObject;
			AssertNotNull(inbondQPHeader);
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureDelete);
			var message = sendingObject.Send();
			var aaa = message.EM_FormattedMessageText;
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("delete message", @"B  8888XJ5QP                                               <<MSGNO PLACEHOLDER>>
10D62123456789   OTT1         00000000                                          
Y  8888XJ5QP", message.EM_FormattedMessageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}
	}
}
