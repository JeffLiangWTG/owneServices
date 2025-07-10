using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FZConcurrenceMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2012, 03, 09)]
		public void TestConcurrenceMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "ABC";
			var container1 = bill.Containers.AddNew();
			container1.CO_ContainerNumber = "CBA";

			var carrier = Factory.New<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
			declaration.DeliveryOrPickupCartageCoPK = carrier.PK;

			bill.US_F_Remarks = "Other remarks"; // Should not be used by message population
			bill.US_F_FZ10Remarks = "Some remarks";

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "6005006";
			itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V1006003";

			var action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.A };

			var builder = new FZConcurrenceMessageBuilder(action);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
10115300011200000001                  A 0000000012FI                            
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };
			builder = new FZConcurrenceMessageBuilder(action);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          B 0000000012FI            W004            
11               15300011200000001                                              
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.C };
			builder = new FZConcurrenceMessageBuilder(action);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
1036005006                            C 0000000012FI            W004            
103V1006003                           C           FI            W004            
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		public void TestConcurrenceMessageUserSet()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;

			bill.US_F_Remarks = "Other remarks"; // Should not be used by message population
			bill.US_F_FZ10Remarks = "Some remarks";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "222256789";
			bill2.CU_NoOfPacks = 14m;
			bill2.US_F_Remarks = "Other2 remarks"; // Should not be used by message population
			bill2.US_F_FZ10Remarks = "Some2 remarks";

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "6005006";
			itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V1006003";

			//send by FTZ
			var action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.A };
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.A)
				{
					obj.MB_Send = true;
					obj.MB_ConcurrenceQty = 30m;
				}
			}
			var builder = new FZConcurrenceMessageBuilder(action);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
10115300011200000001                  A 0000000030FI                            
20SOME REMARKS                                                                  
20SOME2 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			//send by bol 1
			action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.B)
				{
					obj.MB_Send = true;
					obj.MB_ConcurrenceQty = 15m;
				}
			}
			builder = new FZConcurrenceMessageBuilder(action);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          B 0000000015FI            W004            
11               15300011200000001                                              
20SOME REMARKS                                                                  
102222256789                          B 0000000015FI            W004            
11               15300011200000001                                              
20SOME2 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			//send by bol , only send one bill numbers
			action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.B)
				{
					if (obj.MB_Identifier == "123456789")
					{
						obj.MB_Send = false;
						obj.MB_ConcurrenceQty = 15m;
					}
					else
					{
						obj.MB_Send = true;
						obj.MB_ConcurrenceQty = 16m;
					}
				}
			}
			builder = new FZConcurrenceMessageBuilder(action);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102222256789                          B 0000000016FI            W004            
11               15300011200000001                                              
20SOME2 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			//send by in-bond
			action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.C };
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.C)
				{
					if (obj.MB_Identifier == "6005006")
					{
						obj.MB_Send = true;
						obj.MB_ConcurrenceQty = 17m;
					}
					else
					{
						obj.MB_Send = true;
						obj.MB_ConcurrenceQty = 12m;
					}
				}
			}
			builder = new FZConcurrenceMessageBuilder(action);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
20SOME2 REMARKS                                                                 
1036005006                            C 0000000017FI            W004            
103V1006003                           C 0000000012FI            W004            
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			//only send one it numbers
			action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.C };
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.C)
				{
					if (obj.MB_Identifier == "6005006")
					{
						obj.MB_Send = true;
						obj.MB_ConcurrenceQty = 15m;
					}
					else
					{
						obj.MB_Send = false;
						obj.MB_ConcurrenceQty = 12m;
					}
				}
			}
			builder = new FZConcurrenceMessageBuilder(action);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
20SOME2 REMARKS                                                                 
1036005006                            C 0000000015FI            W004            
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			//send by bol , only send one bill numbers, using MessageSendingObjectsView
			action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.B)
				{
					if (obj.MB_Identifier == "123456789")
					{
						obj.MB_Send = false;
						obj.MB_ConcurrenceQty = 15m;
					}
					else
					{
						obj.MB_Send = true;
						obj.MB_ConcurrenceQty = 16m;
					}
				}
			}
			builder = new FZConcurrenceMessageBuilder(action);
			message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102222256789                          B 0000000016FI            W004            
11               15300011200000001                                              
20SOME2 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			bill3.CU_BillNum = "222256799";
			bill3.CU_NoOfPacks = 15m;
			bill3.US_F_Remarks = "Other3 remarks"; // Should not be used by message population
			bill3.US_F_FZ10Remarks = "Some3 remarks";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				//send by bol 3
				action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };
				foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
				{
					if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.B)
					{
						obj.MB_Send = true;
						obj.MB_ConcurrenceQty = 15m;
					}
				}
				builder = new FZConcurrenceMessageBuilder(action);
				message = builder.PopulateMessage();
				AssertEquals(
	@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          B 0000000015FI            W004            
11               15300011200000001                                              
20SOME REMARKS                                                                  
102222256789                          B 0000000015FI            W004            
11               15300011200000001                                              
20SOME2 REMARKS                                                                 
102222256799                          B 0000000015FI            W004            
11               15300011200000001                                              
20SOME3 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				//send by bol 3
				action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };
				foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
				{
					if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.B)
					{
						obj.MB_Send = true;
						obj.MB_ConcurrenceQty = 15m;
					}
				}
				builder = new FZConcurrenceMessageBuilder(action);
				message = builder.PopulateMessage();
				AssertEquals(
	@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          B 0000000015FI            W004            
11               15300011200000001                                              
20SOME REMARKS                                                                  
102222256789                          B 0000000015FI            W004            
11               15300011200000001                                              
20SOME2 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);
			}
		}

		public void TestConcurrenceMessageUserSetWithLastTimeDataA()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;

			bill.US_F_Remarks = "Other remarks"; // Should not be used by message population
			bill.US_F_FZ10Remarks = "Some remarks";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "222256789";
			bill2.CU_NoOfPacks = 14m;
			bill2.US_F_Remarks = "Other2 remarks"; // Should not be used by message population
			bill2.US_F_FZ10Remarks = "Some2 remarks";

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "6005006";
			itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V1006003";

			declaration.US_FTZConcurrenceQty = 31m;
			var action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.A };

			//load last time saved ConcurrenceQty,user set ConcurrenceQty and save back
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.A)
				{
					obj.MB_Send = true;
					AssertEquals(obj.MB_ConcurrenceQty, 31m);
					obj.MB_ConcurrenceQty = 44m;
				}
			}
			var builder = new FZConcurrenceMessageBuilder(action);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
10115300011200000001                  A 0000000044FI                            
20SOME REMARKS                                                                  
20SOME2 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		public void TestConcurrenceMessageUserSetWithLastTimeDataB()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;

			bill.US_F_Remarks = "Other remarks"; // Should not be used by message population
			bill.US_F_FZ10Remarks = "Some remarks";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "222256789";
			bill2.CU_NoOfPacks = 14m;
			bill2.US_F_Remarks = "Other2 remarks"; // Should not be used by message population
			bill2.US_F_FZ10Remarks = "Some2 remarks";

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "6005006";
			itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V1006003";

			//send by bill number
			bill.US_FTZConcurrenceQty = 15m;
			bill2.US_FTZConcurrenceQty = 15m;

			var action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };

			//load last time saved ConcurrenceQty,user set ConcurrenceQty and save back
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.B)
				{
					obj.MB_Send = true;
					AssertEquals(obj.MB_ConcurrenceQty, 15m);
					obj.MB_ConcurrenceQty = 18m;
				}
			}
			var builder = new FZConcurrenceMessageBuilder(action);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102123456789                          B 0000000018FI            W004            
11               15300011200000001                                              
20SOME REMARKS                                                                  
102222256789                          B 0000000018FI            W004            
11               15300011200000001                                              
20SOME2 REMARKS                                                                 
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		public void TestConcurrenceMessageUserSetWithLastTimeDataC()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);
			declaration.FTZAdmissionNumber = "1530001|12|00000001";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;

			bill.US_F_Remarks = "Other remarks"; // Should not be used by message population
			bill.US_F_FZ10Remarks = "Some remarks";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "222256789";
			bill2.CU_NoOfPacks = 14m;
			bill2.US_F_Remarks = "Other2 remarks"; // Should not be used by message population
			bill2.US_F_FZ10Remarks = "Some2 remarks";

			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "6005006";
			var itNo2 = bill.ITAndSplitDetails.AddNew();
			itNo2.US_ITNumber = "V1006003";

			//send by bill number
			itNo.US_FTZConcurrenceQty = 15m;
			itNo2.US_FTZConcurrenceQty = 12m;

			var action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.C };
			//load last time saved ConcurrenceQty,user set ConcurrenceQty and save back
			foreach (FZConcurrenceMessageSendingObject obj in action.MessageSendingObjectsView)
			{
				if (obj.MB_US_ActionCode == FTZActionCodeList.Codes.C)
				{
					if (obj.MB_Identifier == "6005006")
					{
						obj.MB_Send = true;
						AssertEquals(obj.MB_ConcurrenceQty, 15m);
						obj.MB_ConcurrenceQty = 16m;
					}
					else
					{
						obj.MB_Send = false;
						AssertEquals(obj.MB_ConcurrenceQty, 12m);
						obj.MB_ConcurrenceQty = 17m;
					}
				}
			}
			var builder = new FZConcurrenceMessageBuilder(action);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
20SOME2 REMARKS                                                                 
1036005006                            C 0000000016FI            W004            
20SOME REMARKS                                                                  
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}

		[TestDate(2021, 01, 01)]
		public void TestSendConcurrenceMessageForBillWitSplitShipmentDetails()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.FTZAdmissionNumber = "1530001|12|00000001";
			declaration.JE_MasterBill = "MB123456";
			declaration.JE_HouseBill = "HB0000001";

			var houseBill = declaration.PrimaryHouseBill;
			houseBill.CU_NoOfPacks = 12m;
			houseBill.CU_PackType = "KG";
			houseBill.US_FTZConcurrenceQty = 15m;
			houseBill.US_SESplitShip = true;

			var splitDetail1 = houseBill.ITAndSplitDetails.AddNew();
			splitDetail1.US_CarrierCode = "ABC";
			splitDetail1.US_FlightNumber = "001";
			splitDetail1.US_ArrivalDate = ZDateTime.Today;
			var splitDetail2 = houseBill.ITAndSplitDetails.AddNew();
			splitDetail2.US_CarrierCode = "DEF";
			splitDetail2.US_FlightNumber = "002";
			splitDetail2.US_ArrivalDate = ZDateTime.Today.AddDays(1);

			var action = new FZEventAction(declaration, FZEventType.Concur) { US_ActionCode = FTZActionCodeList.Codes.B };
			var builder = new FZConcurrenceMessageBuilder(action);
			var message = builder.PopulateMessage();
			AssertEquals(
@"B  8888XJ5FZ                                               <<MSGNO PLACEHOLDER>>
102MB123456HB0000001                  B 0000000012FI                            
11               15300011200000001   ABC 001            20210101                
102MB123456HB0000001                  B 0000000012FI                            
11               15300011200000001   DEF 002            20210102                
Y  8888XJ5FZ", message.EM_FormattedMessageText);
		}
	}
}
