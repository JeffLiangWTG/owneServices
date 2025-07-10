using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FTZMessageBlockBuilderTest : TestCaseWithFactory
	{
		[TestDate(2011, 11, 02)]
		public void TestSendDeleteMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZAdmissionNumber = "1530100|11|00000001";
			declaration.US_SchDEntry = "1234";
			declaration.US_F_DirectDelivery = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_F_RoutingDetails = "3311ABC12";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(1);
			declaration.IOROrgPK = TestOrg.PK;
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;

			var bill = declaration.Bills.AddNew();
			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "12345678901";
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "APLU123412";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;
			invoice.JZ_OH_Buyer = TestOrg.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Delete);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("Only block 10 should be created",
				"B      SV9FT                                               <<MSGNO PLACEHOLDER>>" +
				"10D1530100  1100000001N1234YXJ53311ABC12887766554433    123456789012            " +
				"Y      SV9FT", message.EM_MessageText);
		}

		[TestDate(2011, 11, 02)]
		public void TestFTZFT10()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZAdmissionNumber = "1530100|11|00000001";
			declaration.US_SchDEntry = "1234";
			declaration.US_F_DirectDelivery = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_F_RoutingDetails = "3311ABC12";
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(1);
			declaration.IOROrgPK = TestOrg.PK;
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;
			declaration.US_US_NKLocationOfGoods = "5678";
			var orgIOR = Factory.NewWithValidTestData<OrgHeader>();
			orgIOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "11-216951301");
			declaration.IOROrgPK = orgIOR.PK;

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBlockBuilder(ftzMessageSendingObject);
			var messageBlocks = new List<MessageBlock>(builder.Build(ftzMessageSendingObject.ActionCode));
			Assert("No. of blocks", messageBlocks.Count > 0);
			AssertEquals("FTZFT10_01", typeof(FTZFT10_01).FullName, messageBlocks[0].GetType().FullName);
			var ft10_01 = messageBlocks[0] as FTZFT10_01;
			AssertEquals("ZoneID", "1530100", ft10_01.ZoneID);
			AssertEquals("Calendar Year", 11, ft10_01.CalendarYear);
			AssertEquals("Control Number", "00000001", ft10_01.ControlNumber);
			AssertEquals("PortCode", "1234", ft10_01.PortCode);
			AssertEquals("DirectDeliveryIndicator", YesNoList.Codes.Yes, ft10_01.DirectDeliveryIndicator);
			AssertEquals("ABIFilerCode", "XJ5", ft10_01.ABIFilerCode);
			AssertEquals("ABIRoutingCode", "3311ABC12", ft10_01.ABIRoutingCode);
			AssertEquals("IRSIdentifier", "887766554433", ft10_01.ZoneOperatorIdentifierformerlyIRSIdentifier);
			AssertEquals("ExpandedZoneIDIndicator", YesNoList.Codes.No, ft10_01.ExpandedZoneIDIndicator);
			AssertEquals("FIRMSIdentifier", "5678", ft10_01.FIRMSIdentifier);
			AssertEquals("ApplicantForAdmission", "11-216951301", ft10_01.ApplicantForAdmission);
		}

		public void TestFTZFT11_12()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().JZ_CU_RelatedHouseBill = declaration.Bills.AddNew().PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Replace);
			ftzMessageSendingObject.US_FTZContactName = "Dan Brown";
			ftzMessageSendingObject.US_FTZContactPhone = "0288108815";
			ftzMessageSendingObject.US_DeleteConveyance = true;
			ftzMessageSendingObject.US_ChangeOrAddBillOfLading = true;
			ftzMessageSendingObject.US_DeleteBillOfLading = true;
			ftzMessageSendingObject.US_ChangeOrAddHTSLine = true;
			ftzMessageSendingObject.US_DeleteHTSLine = true;
			ftzMessageSendingObject.US_ChangeAdmittedQuantity = true;
			ftzMessageSendingObject.US_CancelOrAddPTT = true;
			ftzMessageSendingObject.US_OtherReason = true;
			ftzMessageSendingObject.US_Remarks = "Another Reason";
			var builder = new FTZMessageBlockBuilder(ftzMessageSendingObject);
			var messageBlocks = new List<MessageBlock>(builder.Build(ftzMessageSendingObject.ActionCode));
			AssertEquals("No. of blocks", 5, messageBlocks.Count);
			AssertEquals("FTZFT20", typeof(FTZFT20).FullName, messageBlocks[3].GetType().FullName);

			messageBlocks = new List<MessageBlock>(builder.Build(ftzMessageSendingObject.ActionCode));
			AssertEquals("No. of blocks", 5, messageBlocks.Count);
			AssertEquals("FTZFT11", typeof(FTZFT11).FullName, messageBlocks[1].GetType().FullName);
			var ft11 = messageBlocks[1] as FTZFT11;
			AssertEquals("ContactName", "Dan Brown", ft11.ContactName);
			AssertEquals("ContactPhone", "0288108815", ft11.ContactPhone);
			AssertEquals("ReasonCode", "02", ft11.ReasonCode);
			AssertEquals("AdditionalReasonCodes", "03040506070809", ft11.AdditionalReasonCodes);
			AssertEquals("FTZFT12", typeof(FTZFT12).FullName, messageBlocks[2].GetType().FullName);
			var ft12 = messageBlocks[2] as FTZFT12;
			AssertEquals("Remarks", "Another Reason", ft12.Remarks);

			var messageBuilder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = messageBuilder.PopulateMessage();
			AssertContains("Block 11 should be included", @"11DAN BROWN                               0288108815     0203040506070809       ", message.EM_MessageText);
			AssertContains("Block 12 should be included", @"12ANOTHER REASON                                                                ", message.EM_MessageText);
		}

		public void TestFTZFT20()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().JZ_CU_RelatedHouseBill = declaration.Bills.AddNew().PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
			var uscarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery());
			if (uscarrier == null)
			{
				uscarrier = Factory.New<USCarrierCombined>();
				uscarrier.UI_Code = "MKAR";
				uscarrier.UI_ModeOfTransportation = "40";
			}
			uscarrier.UI_Name = "LASER TRANSPORT INC";

			declaration.JE_MasterBillIssuerSCAC = uscarrier.UI_Code;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
			declaration.JE_VoyageFlightNo = "1234567890";
			declaration.US_DateOfExport = new ZDateTime(2011, 10, 24);
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 25);
			declaration.US_SchDArrival = "1234";
			declaration.US_EntryDate = new ZDateTime(2011, 10, 26);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2011, 10, 27);

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBlockBuilder(ftzMessageSendingObject);
			var messageBlocks = new List<MessageBlock>(builder.Build(ftzMessageSendingObject.ActionCode));
			Assert("No. of blocks", messageBlocks.Count > 1);
			AssertEquals("FTZFT20", typeof(FTZFT20).FullName, messageBlocks[1].GetType().FullName);
			var ft20 = messageBlocks[1] as FTZFT20;

			AssertEquals("AdmissionType", FTZAdmissionTypeCodeList.Codes.RegularAdmission, ft20.AdmissionType);
			AssertEquals("ModeOfTransportation", TransportModeCodes.Codes.AirNonContainer, ft20.ModeOfTransportation);
			AssertEquals("SCACIdentifierOrAirwayBillPrefixOfImportingCarrier", uscarrier.UI_Code, ft20.SCACIdentifierOrAirlineCarrierCodeOfImportingCarrier);
			AssertEquals("ConveyanceName", uscarrier.UI_Name, ft20.ConveyanceName);
			AssertEquals("ExportDate", new ZDate(2011, 10, 24), ft20.ExportDate);
			AssertEquals("ImportDate", new ZDate(2011, 10, 25), ft20.ImportDate);
			AssertEquals("PortOfUnlading", "1234", ft20.PortOfUnlading);
			AssertEquals("EstimatedDateOfArrival", new ZDate(2011, 10, 25), ft20.ScheduledDateOfArrival);
		}

		[TestDate(2000, 1, 1, 12, 0, 0)]
		public void TestFTZFT20_Splits()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;

				var bill1 = declaration.Bills.AddNew();
				bill1.US_SESplitShip = true;
				bill1.CU_NoOfPacks = 155m;
				var carrier1 = Factory.New<USCarrierCombined>();
				carrier1.UI_Code = "C1";
				carrier1.UI_Name = "Air Company 1";
				var split1 = bill1.ITAndSplitDetails.AddNew();
				split1.US_ITNumber = "1";
				split1.US_ArrivalDate = ZDateTime.Now;
				split1.US_CarrierCode = "C1";
				split1.US_FlightNumber = "FLT01";
				split1.US_NoOfPacks = 100;
				var split2 = bill1.ITAndSplitDetails.AddNew();
				split2.US_ITNumber = "2";
				split2.US_ArrivalDate = ZDateTime.Now.AddDays(1);
				split2.US_CarrierCode = "C1";
				split2.US_FlightNumber = "FLT02";
				split2.US_NoOfPacks = 55;

				var bill2 = declaration.Bills.AddNew();
				bill2.US_SESplitShip = true;
				bill2.CU_NoOfPacks = 10000m;
				var carrier2 = Factory.New<USCarrierCombined>();
				carrier2.UI_Code = "C2";
				carrier2.UI_Name = "Air Company 2";
				var split3 = bill2.ITAndSplitDetails.AddNew();
				split3.US_ITNumber = "3";
				split3.US_ArrivalDate = ZDateTime.Now;
				split3.US_CarrierCode = "C2";
				split3.US_FlightNumber = "FLT11";
				split3.US_NoOfPacks = 4000;
				var split4 = bill2.ITAndSplitDetails.AddNew();
				split4.US_ITNumber = "4";
				split4.US_ArrivalDate = ZDateTime.Now.AddDays(3);
				split4.US_CarrierCode = "C2";
				split4.US_FlightNumber = "FLT12";
				split4.US_NoOfPacks = 6000;

				var splits = new[] { split1, split2, split3, split4 };
				var carries = new Dictionary<ZString, ZString>() { { carrier1.UI_Code, carrier1.UI_Name }, { carrier2.UI_Code, carrier2.UI_Name } };

				var invoice3 = declaration.Invoices.AddNew();
				invoice3.JZ_CU_RelatedHouseBill = bill2.PK;
				invoice3.US_SplitShipmentDetail = "C2/FLT11/01-JAN-00";
				invoice3.InvoiceLines.AddNew().JI_Tariff = "00001";
				var invoice4 = declaration.Invoices.AddNew();
				invoice4.JZ_CU_RelatedHouseBill = bill2.PK;
				invoice4.US_SplitShipmentDetail = "C2/FLT12/04-JAN-00";
				invoice4.InvoiceLines.AddNew().JI_Tariff = "00002";
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_CU_RelatedHouseBill = bill1.PK;
				invoice1.US_SplitShipmentDetail = "C1/FLT01/01-JAN-00";
				invoice1.InvoiceLines.AddNew().JI_Tariff = "00003";
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_CU_RelatedHouseBill = bill1.PK;
				invoice2.US_SplitShipmentDetail = "C1/FLT02/02-JAN-00";
				invoice2.InvoiceLines.AddNew().JI_Tariff = "00004";

				var carrier3 = Factory.New<USCarrierCombined>();
				carrier3.UI_Code = "C0";
				carrier3.UI_Name = "Air Company 0";
				declaration.JE_MasterBillIssuerSCAC = "C0";
				declaration.JE_VoyageFlightNo = "FLT00";
				declaration.US_DateOfExport = ZDateTime.Now.AddDays(-1).Date;
				declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(10).Date;
				declaration.US_SchDArrival = "1234";
				var bill0 = declaration.Bills.AddNew();
				bill0.US_SESplitShip = false;
				bill0.CU_NoOfPacks = 333m;

				var invoice0 = declaration.Invoices.AddNew();
				invoice0.JZ_CU_RelatedHouseBill = bill0.PK;
				invoice0.InvoiceLines.AddNew().JI_Tariff = "00000";

				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				var sendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
				var builder = new FTZMessageBlockBuilder(sendingObject);
				var messageBlocks = new List<MessageBlock>(builder.Build(sendingObject.ActionCode));

				int ft20Idx = 0;
				FTZFT20 ft20 = null;
				foreach (var block in messageBlocks)
				{
					if (block is FTZFT20)
					{
						ft20 = block as FTZFT20;
						AssertEquals("AdmissionType", FTZAdmissionTypeCodeList.Codes.RegularAdmission, ft20.AdmissionType);
						AssertEquals("ModeOfTransportation", TransportModeCodes.Codes.AirNonContainer, ft20.ModeOfTransportation);
						AssertEquals("ExportDate", ZDateTime.Now.AddDays(-1).Date, ft20.ExportDate);

						AssertEquals("PortOfUnlading", "1234", ft20.PortOfUnlading);
						if (ft20Idx++ == 0)
						{
							AssertEquals("SCACIdentifierOrAirwayBillPrefixOfImportingCarrier", "C0", ft20.SCACIdentifierOrAirlineCarrierCodeOfImportingCarrier);
							AssertEquals("ConveyanceName", "Air Company 0", ft20.ConveyanceName);
							AssertEquals("VoyageTripFlightNumber", "FLT00", ft20.VoyageTripFlightNumber);
							AssertEquals("ImportDate", ZDateTime.Now.AddDays(10).Date, ft20.ImportDate);
							AssertEquals("EstimatedDateOfArrival", ZDateTime.Now.AddDays(10).Date, ft20.ScheduledDateOfArrival);
						}
						else
						{
							var split = splits.SingleOrDefault(_ => _.US_FlightNumber == ft20.VoyageTripFlightNumber);
							AssertEquals("SCACIdentifierOrAirwayBillPrefixOfImportingCarrier", split.US_CarrierCode, ft20.SCACIdentifierOrAirlineCarrierCodeOfImportingCarrier);
							AssertEquals("ConveyanceName", carries[split.US_CarrierCode], ft20.ConveyanceName);
							AssertEquals("ImportDate", split.US_ArrivalDate.Date, ft20.ImportDate);
							AssertEquals("EstimatedDateOfArrival", split.US_ArrivalDate.Date, ft20.ScheduledDateOfArrival);
						}
					}
					else if (block is FTZFT40 ft40)
					{
						if (ft20Idx == 1)
						{
							AssertEquals("Quantity", 333m, ft40.Quantity);
						}
						else
						{
							switch (ft20.VoyageTripFlightNumber)
							{
								case "FLT01":
									AssertEquals("Quantity", 100m, ft40.Quantity);
									break;
								case "FLT02":
									AssertEquals("Quantity", 55m, ft40.Quantity);
									break;
								case "FLT11":
									AssertEquals("Quantity", 4000m, ft40.Quantity);
									break;
								case "FLT12":
									AssertEquals("Quantity", 6000m, ft40.Quantity);
									break;
								default:
									Fail("Unexpected split");
									break;
							}
						}
					}
					else if (block is FTZFT50 ft50)
					{
						if (ft20Idx == 1)
						{
							AssertEquals("Line Tariff for declaration", "00000", ft50.HarmonizedTariffScheduleNumber);
						}
						else
						{
							switch (ft20.VoyageTripFlightNumber)
							{
								case "FLT01":
									AssertEquals("Line Tariff for split1", "00003", ft50.HarmonizedTariffScheduleNumber);
									break;
								case "FLT02":
									AssertEquals("Line Tariff for split2", "00004", ft50.HarmonizedTariffScheduleNumber);
									break;
								case "FLT11":
									AssertEquals("Line Tariff for split3", "00001", ft50.HarmonizedTariffScheduleNumber);
									break;
								case "FLT12":
									AssertEquals("Line Tariff for split4", "00002", ft50.HarmonizedTariffScheduleNumber);
									break;
								default:
									Fail("Unexpected split");
									break;
							}
						}
					}
				}
			});
		}

		public void TestFTZFT40()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_MasterBill = "1234567890123456789";

			var bill = declaration.Bills.AddNew();
			var masterBill = declaration.PrimaryMasterBill;
			bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_CU_ParentBill = masterBill.PK;
			bill.US_SESplitShip = false;

			bill.CU_BillNum = "123456789012345";
			bill.CU_NoOfPacks = 1000m;
			declaration.US_UC_NKCountryOfExport = "AU";
			declaration.US_SchDLoading = "12345";
			declaration.US_US_NKLocationOfGoods = "1234";

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBlockBuilder(ftzMessageSendingObject);
			var messageBlocks = new List<MessageBlock>(builder.Build(ftzMessageSendingObject.ActionCode));

			Assert("No. of blocks", messageBlocks.Count > 2);
			AssertEquals("FTZFT40_01", typeof(FTZFT40_01).FullName, messageBlocks[2].GetType().FullName);
			var ft40_01 = messageBlocks[2] as FTZFT40_01;
			AssertEquals("BillOfLadingOrAirwayBill", "1234567890123456789", ft40_01.BillOfLadingOrAirWaybill);
			AssertEquals("HouseBill", "123456789012", ft40_01.HouseBill);
			AssertEquals("Quantity", 1000m, ft40_01.Quantity);
			AssertEquals("CountryOfExport", "AU", ft40_01.CountryOfExport);
			AssertEquals("ForeignLoadPort", "12345", ft40_01.ForeignLoadPort);

			var messageBuilder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = messageBuilder.PopulateMessage();
			AssertContains("Block 40 should be included", @"401234567890123456789                123456789012        0000001000AU12345      ", message.EM_MessageText);
		}

		public void TestHouseBillOfFTZFT40WhenAMSHBREIsffective()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_MasterBill = "1234567890123456789";

			var bill = declaration.Bills.AddNew();
			var masterBill = declaration.PrimaryMasterBill;
			bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_CU_ParentBill = masterBill.PK;
			bill.US_UI_NKBillIssuerSCAC = "ABCD";
			bill.CU_HouseBill = "123456789012345";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
				var builder = new FTZMessageBlockBuilder(ftzMessageSendingObject);
				var ft40_01 = builder.Build(ftzMessageSendingObject.ActionCode).OfType<FTZFT40_01>().FirstOrDefault();
				AssertEquals("HouseBill", "ABCD123456789012", ft40_01.HouseBill);
			}
		}

		public void TestFTZT41()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var bill = declaration.Bills.AddNew();

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "12345678901";

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBlockBuilder(ftzMessageSendingObject);
			var messageBlocks = new List<MessageBlock>(builder.Build(ftzMessageSendingObject.ActionCode));
			Assert("No. of blocks", messageBlocks.Count > 3);

			AssertEquals("FTZFT41", typeof(FTZFT41).FullName, messageBlocks[3].GetType().FullName);
			var ft40 = messageBlocks[3] as FTZFT41;
			AssertEquals("ITNumber", "12345678901", ft40.ITNumber);
		}

		[TestDate(2011, 11, 02)]
		public void TestFTZT42_43()
		{
			//can send 42 for bill and message will be accepted. 
			//Outgoing:
			//B012501SV9FT                                               115564               
			//10A153000111000000072501YSV9         13-147927000                               
			//20A11ALPUTINGLEV MAERSK         DE244          2011121920111219110120111219     
			//40XXXAJSOCT0111                                          0000000019FR60267W004  
			//4291-013199000                                                                  
			//50000018456301020    AU000000000010NO 000000000000                              
			//5100000000100000000000000000000000N00000000                                     
			//60                                             IM 13-147927000                  
			//Y  2501SV9FT00007

			//Response:
			//B002501SV9NF                                               115564               
			//90A153000111000000072501Y                                                       
			//91B4 1  15300011100000007                  1112202324                           
			//95     FTZ PAPERLESS ADMISSION                                                  
			//Y  2501SV9NF00003                                                               
			//B002501SV9NF                                               115564               
			//90A153000111000000072501Y                                                       
			//91BF 1  15300011100000007                  1112202324                           
			//95     FTZ ADMISSION AUTHORIZED                                                 
			//Y  2501SV9NF00003                                                               
			//B012501SV9NF                                               115564               
			//90A153000111000000072501Y                                                       
			//91B1 1  15300011100000007                  1112202324                           
			//95  083 ZONE ADMISSION DATA ACCEPTED                                            
			//Y  2501SV9NF00001 

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_IncludePTT = true;
			declaration.DeliveryOrPickupCartageCoPK = TestOrg.PK;
			declaration.FTZControlNumber = "00000001";

			var bill = declaration.Bills.AddNew();
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "APLU123412";

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("Blocks 42 and 43 should be included",
				@"B      SV9FT                                               <<MSGNO PLACEHOLDER>>10A         " +
				"1100000001N    NSV9                                                 " +
				"20                                                                              " +
				"40                                                                              " +
				"42123456789012                                                                  " +
				"43APLU123412                                                                    " +
				"Y      SV9FT", message.EM_MessageText);

			declaration.US_F_IncludePTT = false;

			ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			message = builder.PopulateMessage();
			AssertEquals("Blocks 42 and 43 should not be included, because PTT is not included in admission",
				@"B      SV9FT                                               <<MSGNO PLACEHOLDER>>10A         " +
				"1100000001N    NSV9                                                 " +
				"20                                                                              " +
				"40                                                                              " +
				"Y      SV9FT", message.EM_MessageText);
		}

		[TestDate(2011, 11, 02)]
		public void TestFTZT50_51()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.IOROrgPK = TestOrg.PK;
			declaration.FTZControlNumber = "00000001";
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;

			var bill = declaration.Bills.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "D";
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 1001m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee.CY_FeeAmount = 99m;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_Description = "a description";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("Blocks 50 and 51 should be included",
				@"B      SV9FT                                               <<MSGNO PLACEHOLDER>>" +
				"10A         1100000001N    NSV9         887766554433    123456789012            " +
				"20                                                                              " +
				"40                                                                              " +
				"50000011234567890B  HAU000000010000KG 000000001000D  123                        " +
				"5100000001000000000010010000000000Z00009900                                     " +
				"60A DESCRIPTION                                MID9876543210987654321012        " +
				"Y      SV9FT", message.EM_MessageText);
		}

		[TestDate(2011, 11, 02)]
		public void TestFTZT60And61()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = TestOrg.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.FTZControlNumber = "00000001";
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;

			var orgIOR = Factory.NewWithValidTestData<OrgHeader>();
			orgIOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "11-216951301");
			declaration.IOROrgPK = orgIOR.PK;

			var bill = declaration.Bills.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7102310000";
			invoiceLine.US_MiscPermitNo = "123456789";
			invoiceLine.US_LicenseType = "DIA";

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;

			invoiceLine.JI_Description = "A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("Blocks 60 and 61 should be included",
				@"B      SV9FT                                               <<MSGNO PLACEHOLDER>>" +
				"10A         1100000001N    NSV9         887766554433    11-216951301            " +
				"20                                                                              " +
				"40                                                                              " +
				"50000017102310000      000000000000CAR                                          " +
				"5100000000000000000000010000000000 00000000                                     " +
				"60A VERY LONG DESCRIPTION A VERY LONG DESCRIPTIMID9876543210987654321012        " +
				"60ON A VERY LONG DESCRIPTION A VERY LONG DESCRIDIA123456789                     " +
				"61PTION A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION A VERY LONG DESCRIPTION " +
				"Y      SV9FT", message.EM_MessageText);
		}

		[TestDate(2022, 12, 31)]
		public void TestGenerateLineLevelMessagesForSea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.IOROrgPK = TestOrg.PK;
			declaration.FTZControlNumber = "00000001";
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "1234567890123456789";

			var bill = declaration.Bills.AddNew();
			var masterBill = declaration.PrimaryMasterBill;
			bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_CU_ParentBill = masterBill.PK;
			bill.US_UI_NKBillIssuerSCAC = "ABCD";
			bill.CU_HouseBill = "123456789";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "D";
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 1001m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee.CY_FeeAmount = 99m;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_Description = "a description";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
				var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
				var message = builder.PopulateMessage();
				AssertEquals("GenerateLineLevelMessages should be called",
					@"B      SV9FT                                               <<MSGNO PLACEHOLDER>>" +
					"10A         2200000001N    NSV9         887766554433    123456789012            " +
					"20                                                                              " +
					"401234567890123456789                ABCD123456789                              " +
					"50000011234567890B  HAU000000010000KG 000000001000D  123                        " +
					"5100000001000000000010010000000000Z00009900                                     " +
					"60A DESCRIPTION                                MID9876543210987654321012        " +
					"Y      SV9FT", message.EM_MessageText);
			}
		}

		[TestDate(2011, 11, 02)]
		public void TestSendTemporaryDeposit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.TemporaryDeposit;
			declaration.FTZControlNumber = "00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "Bill10012";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;
			invoice.JZ_OH_Buyer = TestOrg.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("Blocks 50 and 51 should not be included, because Temporary Deposit",
				@"B      SV9FT                                               <<MSGNO PLACEHOLDER>>" +
"10A         1100000001N    NSV9                                                 " +
"20T                                                                             " +
"40BILL10012                                                                     " +
"Y      SV9FT", message.EM_MessageText);
		}

		[TestDate(2011, 11, 02)]
		public void TestWithLongSPI()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.IOROrgPK = TestOrg.PK;
			declaration.FTZControlNumber = "00000001";
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;
			declaration.JE_VoyageFlightNo = "12";

			var bill = declaration.Bills.AddNew();
			bill.US_SESplitShip = false;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = "B#";

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "D";
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 1001m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee.CY_FeeAmount = 99m;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_Description = "a description";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("Blocks 50 and 51 should be included",
				@"B      SV9FT                                               <<MSGNO PLACEHOLDER>>" +
				"10A         1100000001N    NSV9         887766554433    123456789012            " +
				"20                              0012                                            " +
				"40                                                                              " +
				"50000011234567890 B#HAU000000010000KG 000000001000D  123                        " +
				"61B#                                                                            " +
				"5100000001000000000010010000000000Z00009900                                     " +
				"60A DESCRIPTION                                MID9876543210987654321012        " +
				"Y      SV9FT", message.EM_MessageText);
		}

		OrgHeader warehouseOrg;
		OrgHeader WarehouseOrg
		{
			get
			{
				if (warehouseOrg == null)
				{
					warehouseOrg = Factory.New<OrgHeader>();
					warehouseOrg.FillWithValidTestData();
					warehouseOrg.OH_FullName = "Warehouse Organisation";
					warehouseOrg.OH_IsWarehouseClient = true;
					warehouseOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "887766554433");
				}
				return warehouseOrg;
			}
		}

		OrgHeader testOrg;
		OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>();
					testOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
				}
				return testOrg;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("SV9");
		}
	}
}
