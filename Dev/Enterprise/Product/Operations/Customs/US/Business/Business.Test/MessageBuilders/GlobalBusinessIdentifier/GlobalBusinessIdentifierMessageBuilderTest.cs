using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class GlobalBusinessIdentifierMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateOriginalMessage()
		{
			var messageData = CreateGlobalBusinessIdentifierData();
			AssertEquals(0, messageData.Wrapper.Messages.Count);

			var messageBuilder = new GlobalBusinessIdentifierMessageBuilder(messageData, GlobalBusinessIdentifierMessageType.Original);
			var message = messageBuilder.Generate();
			AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIAdd, messageData.SubmissionStatus);
			AssertEquals(1, messageData.Wrapper.Messages.Count);
			AssertEquals(ApplicationCodeList.Codes.USCustomsImport, message.EM_ApplicationCode);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(@"B         GE                                               <<MSGNO PLACEHOLDER>>
GE10ATEST 001                                                                   
GE20DUNS111111                                                                  
GE20GLN 222222                                                                  
GE20LEI 333333                                                                  
GE21MFSHSEEXPKDR                                                                
GE30ADDRESS 1                                                                   
GE31ADDRESS 2                                                                   
GE32HOUSTON                            TX                      US2017           
GE4024235006955                                                                 
GE41HTTP://WWW.TEST.COM                                                         
GE50MID444444                                                                   
GE50AEO555555                                                                   
Y         GE", message.EM_FormattedMessageText);
		}

		public void TestGenerateUpdateMessage()
		{
			var messageData = CreateGlobalBusinessIdentifierData();
			AssertEquals(0, messageData.Wrapper.Messages.Count);

			var messageBuilder = new GlobalBusinessIdentifierMessageBuilder(messageData, GlobalBusinessIdentifierMessageType.Update);
			var message = messageBuilder.Generate();
			AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIUpdate, messageData.SubmissionStatus);
			AssertEquals(1, messageData.Wrapper.Messages.Count);
			AssertEquals(ApplicationCodeList.Codes.USCustomsImport, message.EM_ApplicationCode);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(@"B         GE                                               <<MSGNO PLACEHOLDER>>
GE10UTEST 001                                                                   
GE20DUNS111111                                                                  
GE20GLN 222222                                                                  
GE20LEI 333333                                                                  
GE21MFSHSEEXPKDR                                                                
GE30ADDRESS 1                                                                   
GE31ADDRESS 2                                                                   
GE32HOUSTON                            TX                      US2017           
GE4024235006955                                                                 
GE41HTTP://WWW.TEST.COM                                                         
GE50MID444444                                                                   
GE50AEO555555                                                                   
Y         GE", message.EM_FormattedMessageText);
		}

		public void TestGenerateDeletionMessage()
		{
			var messageData = CreateGlobalBusinessIdentifierData();
			AssertEquals(0, messageData.Wrapper.Messages.Count);

			var messageBuilder = new GlobalBusinessIdentifierMessageBuilder(messageData, GlobalBusinessIdentifierMessageType.Delete);
			var message = messageBuilder.Generate();
			AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIDelete, messageData.SubmissionStatus);
			AssertEquals(1, messageData.Wrapper.Messages.Count);
			AssertEquals(ApplicationCodeList.Codes.USCustomsImport, message.EM_ApplicationCode);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(@"B         GE                                               <<MSGNO PLACEHOLDER>>
GE10DTEST 001                                                                   
GE20DUNS111111                                                                  
GE20GLN 222222                                                                  
GE20LEI 333333                                                                  
Y         GE", message.EM_FormattedMessageText);
		}

		public void TestSendManufacturerID()
		{
			var messageData = CreateGlobalBusinessIdentifierData();
			var orgCusCode = messageData.Organization.CustomsCodes.GetOrgCusCode(OrgCusCode.USACodeTypes.ManufacturerID, messageData.Organization.Country);
			orgCusCode.Delete();

			var messageBuilder = new GlobalBusinessIdentifierMessageBuilder(messageData, GlobalBusinessIdentifierMessageType.Original);
			var message = messageBuilder.Generate();
			AssertEquals(@"B         GE                                               <<MSGNO PLACEHOLDER>>
GE10ATEST 001                                                                   
GE20DUNS111111                                                                  
GE20GLN 222222                                                                  
GE20LEI 333333                                                                  
GE21MFSHSEEXPKDR                                                                
GE30ADDRESS 1                                                                   
GE31ADDRESS 2                                                                   
GE32HOUSTON                            TX                      US2017           
GE4024235006955                                                                 
GE41HTTP://WWW.TEST.COM                                                         
GE50AEO555555                                                                   
Y         GE", message.EM_FormattedMessageText);

			var secondAddress = messageData.Organization.Addresses.AddNew();
			secondAddress.OA_CompanyNameOverride = "Override Company";
			secondAddress.OA_Address1 = "123 Second St";
			secondAddress.OA_City = "HH";
			secondAddress.OA_State = "Hamburg";
			secondAddress.OA_PostCode = "10001";
			secondAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "00000004", Core.Constants.CountryCodes.UnitedStates);

			messageData = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(messageData.Organization));
			messageBuilder = new GlobalBusinessIdentifierMessageBuilder(messageData, GlobalBusinessIdentifierMessageType.Original);
			message = messageBuilder.Generate();
			AssertEquals(@"B         GE                                               <<MSGNO PLACEHOLDER>>
GE10ATEST 001                                                                   
GE20DUNS111111                                                                  
GE20GLN 222222                                                                  
GE20LEI 333333                                                                  
GE21  SH                                                                        
GE30ADDRESS 1                                                                   
GE31ADDRESS 2                                                                   
GE32HOUSTON                            TX                      US2017           
GE4024235006955                                                                 
GE41HTTP://WWW.TEST.COM                                                         
GE50AEO555555                                                                   
Y         GE", message.EM_FormattedMessageText);

			messageData = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(messageData.Organization));
			messageData.US_IsManufacturer = true;
			messageData.AddressDetails.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "444444", Core.Constants.CountryCodes.UnitedStates);
			messageBuilder = new GlobalBusinessIdentifierMessageBuilder(messageData, GlobalBusinessIdentifierMessageType.Original);
			message = messageBuilder.Generate();
			AssertEquals(@"B         GE                                               <<MSGNO PLACEHOLDER>>
GE10ATEST 001                                                                   
GE20DUNS111111                                                                  
GE20GLN 222222                                                                  
GE20LEI 333333                                                                  
GE21MFSH                                                                        
GE30ADDRESS 1                                                                   
GE31ADDRESS 2                                                                   
GE32HOUSTON                            TX                      US2017           
GE4024235006955                                                                 
GE41HTTP://WWW.TEST.COM                                                         
GE50MID444444                                                                   
GE50AEO555555                                                                   
Y         GE", message.EM_FormattedMessageText);
		}

		GlobalBusinessIdentifierData CreateGlobalBusinessIdentifierData()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TST001";
			org.OH_FullName = "Test 001";
			org.OH_RL_NKClosestPort = "USLAX";
			org.OH_IsConsignor = true;

			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			address.OA_State = USStateList.Codes.Texas;
			address.OA_City = "Houston";
			address.OA_PostCode = "2017";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_Phone = "+1 (242) 35006955";

			var url = org.MainWebURL;
			url.PU_URL = "http://www.test.com";

			var country = org.Country;
			org.SetCustomsCode(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, country, "111111");
			org.SetCustomsCode(OrgCusCode.USACodeTypes.GlobalLocationNumber, country, "222222");
			org.SetCustomsCode(OrgCusCode.USACodeTypes.LegalEntityIdentifier, country, "333333");
			org.SetCustomsCode(OrgCusCode.USACodeTypes.ManufacturerID, country, "444444");
			org.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, country, "555555");

			var data = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(org));
			data.US_IsSeller = true;
			data.US_IsExporter = true;
			data.US_IsPackager = true;
			data.US_IsDistributor = true;

			return data;
		}
	}
}
