using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PTTMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2011, 12, 30)]
		public void TestPTTMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.FTZAdmissionNumber = "1530001|11|00000001";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.US_UI_NKCarrierSCAC = "ABC";
			declaration.JE_VoyageFlightNo = "0001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "ABC";
			var container1 = bill.Containers.AddNew();
			container1.CO_ContainerNumber = "CBA";

			var carrier = Factory.New<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
			bill.US_F_OH_PTTCarrier = carrier.PK;
			bill.US_F_Remarks = "Some remarks";

			var builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTMessage);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          F 0000000012FI123456789012W004            
11               15300011100000001                                              
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTArrival);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          L           FI123456789012W004            
11               15300011100000001                                              
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		public void TestCancelPTTMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.FTZAdmissionNumber = "1530001|11|00000001";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.US_UI_NKCarrierSCAC = "ABC";
			declaration.JE_VoyageFlightNo = "0001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "ABC";

			var carrier = Factory.New<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
			bill.US_F_OH_PTTCarrier = carrier.PK;
			bill.US_F_Remarks = "Some remarks";
			bill.USB_PermitToTransferID = "0000001";

			var builder = new PTTMessageBuilder(declaration, PTTSendingOption.CancellPTTMessage);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          K           FI123456789012W004            
11               15300011100000001                                              
12PID0000001                                                                    
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTUnArrival);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          M           FI123456789012W004            
11               15300011100000001                                              
12PID0000001                                                                    
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		public void TestPTTMessageForDirectDelivery()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.FTZZoneID = ZString.Empty;
			declaration.FTZControlNumber = ZString.Empty;
			declaration.FTZYear = "11";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.US_UI_NKCarrierSCAC = "ABC";
			declaration.JE_VoyageFlightNo = "0001";
			declaration.US_F_DirectDelivery = true;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "ABC";
			var container1 = bill.Containers.AddNew();
			container1.CO_ContainerNumber = "CBA";

			var carrier = Factory.New<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
			bill.US_F_OH_PTTCarrier = carrier.PK;
			bill.US_F_Remarks = "Some remarks";

			var builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTMessage);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          F 0000000012FI123456789012W004            
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTArrival);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          L           FI123456789012W004            
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		[TestDate(2021, 04, 30)]
		public void TestSendPTTMessageForBillWithSplitDetails()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var unloco = new RefUNLOCO.Loader(Factory).Load("USPHL");
			if (unloco == null)
			{
				unloco = Factory.New<RefUNLOCO>();
				unloco.RL_Code = "USPHL";
				unloco.RL_PortName = "Philadelphia";
				unloco.RL_IATA = "PHL";
				unloco.RL_RN_NKCountryCode = "US";
			}

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.FTZAdmissionNumber = "1530001|12|00000001";
			declaration.JE_MasterBill = "MB123456";
			declaration.JE_HouseBill = "HB0000001";
			declaration.JE_RL_NKPortOfArrival = "USPHL";

			var houseBill = declaration.PrimaryHouseBill;
			houseBill.CU_NoOfPacks = 12m;
			houseBill.CU_PackType = "KG";
			houseBill.US_SESplitShip = true;
			var splitDetail1 = houseBill.ITAndSplitDetails.AddNew();
			splitDetail1.US_CarrierCode = "ABC";
			splitDetail1.US_FlightNumber = "001";
			splitDetail1.US_NoOfPacks = 4;
			splitDetail1.US_ArrivalDate = ZDateTime.Today;
			var splitDetail2 = houseBill.ITAndSplitDetails.AddNew();
			splitDetail2.US_CarrierCode = "DEF";
			splitDetail2.US_FlightNumber = "002";
			splitDetail2.US_NoOfPacks = 7;
			splitDetail2.US_ArrivalDate = ZDateTime.Today.AddDays(1);

			var builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTMessage);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102MB123456HB0000001                  F 0000000004FI                PHL         
11               15300011200000001   ABC 001            20210430                
102MB123456HB0000001                  F 0000000007FI                PHL         
11               15300011200000001   DEF 002            20210501                
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTArrival);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102MB123456HB0000001                  L           FI                PHL         
11               15300011200000001   ABC 001            20210430                
102MB123456HB0000001                  L           FI                PHL         
11               15300011200000001   DEF 002            20210501                
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			builder = new PTTMessageBuilder(declaration, PTTSendingOption.CancellPTTMessage);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102MB123456HB0000001                  K           FI                PHL         
11               15300011200000001   ABC 001            20210430                
102MB123456HB0000001                  K           FI                PHL         
11               15300011200000001   DEF 002            20210501                
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTUnArrival);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102MB123456HB0000001                  M           FI                PHL         
11               15300011200000001   ABC 001            20210430                
102MB123456HB0000001                  M           FI                PHL         
11               15300011200000001   DEF 002            20210501                
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			houseBill.US_SESplitShip = false;
			builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTMessage);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102MB123456HB0000001                  F 0000000012FI                PHL         
11               15300011200000001                                              
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			builder = new PTTMessageBuilder(declaration, PTTSendingOption.SendPTTArrival);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102MB123456HB0000001                  L           FI                PHL         
11               15300011200000001                                              
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}
	}
}
