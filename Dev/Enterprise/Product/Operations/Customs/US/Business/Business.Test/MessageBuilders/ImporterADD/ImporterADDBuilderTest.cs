using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImporterADDBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateImportConsigneeCreateUpdateMessage()
		{
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_Address1 = "TEST MAILING ADDRESS";
			organisation.MainAddress.OA_City = "CHICAGO";
			organisation.MainAddress.OA_State = "IL";
			organisation.MainAddress.OA_PostCode = "60091";
			organisation.MainAddress.OA_RN_NKCountryCode = "US";

			var address2 = organisation.Addresses.AddNew();
			address2.OA_Address1 = "TEST PHYSICAL ADDRESS";
			address2.OA_Address2 = "SECONDARY ADDRESS";
			address2.OA_City = "LONDON";
			address2.OA_PostCode = "1234";
			address2.OA_RN_NKCountryCode = "GB";

			var wrapper = OrgHeaderWrapper.New(organisation);
			var messageData = new OrgAddressMessageData(wrapper);

			messageData.US_ActionCode = ImporterADDActionCodeList.Codes.AddImporterNumber;
			messageData.US_ImporterNumber = "89-002203600";
			messageData.US_ImporterType = ImporterTypeList.Codes.Corporation;
			messageData.US_ImporterName = "THIS IS A LONG IMPORTER NAME WITH LENGTH GREATER THAN 32";

			messageData.US_NameQualifier = ImporterADDNameQualifierList.Codes.A_Division;
			messageData.US_AlternativeImporterName = "A&S FURNISHING BEDDING DIVISION";
			messageData.US_OA_Address1 = organisation.MainAddress.PK;
			messageData.US_AddressType1 = ImporterAddressTypesList.Codes._01;

			messageData.US_OA_Address2 = address2.PK;
			messageData.US_AddressType2 = ImporterAddressTypesList.Codes._08;
			messageData.US_AddressExplanation2 = "EXPLANATION";

			messageData.US_ImporterPhoneNumber = "+6423456789";
			messageData.US_PhoneExtension = "001";
			messageData.US_ImporterEmail = "THISISAVERYLONGEMAILADDRESS@ABC.COM";
			messageData.US_ImporterWebsite = "https://www.abcdefghijklmnopqrst12345.com";
			messageData.US_NoSSNIndicator = true;
			messageData.US_NotAppliedIndicator = true;
			messageData.US_UtlIORIndicator = true;
			messageData.US_UtlOtherIndicator = true;
			messageData.US_UtlOtherDescription = "OTHER DESC";
			messageData.US_NumberOfEntries = NumberOfEntriesPlanningList.Codes._02;
			messageData.US_ProgramCode1 = ImporterProgramCodeList.Codes.CTPAT;
			messageData.US_ProgramCode2 = ImporterProgramCodeList.Codes.ISA;
			messageData.US_BusinessDescription = "I SELL PRODUCTS";
			messageData.US_NAICSCode = "123456";
			messageData.US_DUNS = "987654321";
			messageData.US_FilerCode = "XJ5";
			messageData.US_YearEstablished = "2018";
			messageData.US_BankName = "THIS IS A LONG BANK NAME THAT EXCEED 30 CHARACTERS";
			messageData.US_BankRoutingNo = "1234567890A";
			messageData.US_BankCity = "THIS IS A LONG CITY NAME THAT LENGTH EXCEED 30 CHARACTERS";
			messageData.US_BankCountry = "CA";
			messageData.US_BankState = "AB";
			messageData.US_CountryISOCode = "US";
			messageData.US_StateCode = "AK";
			messageData.US_CertificateReference = "123ABC321CBA";

			var piiContact = organisation.Contacts.AddNew();
			piiContact.OC_ContactName = "THISISALONGPERSONIDENTIFYFIRSTNAME THISISALONGPERSONIDENTIFYLASTNAME";
			piiContact.OC_Title = "PRESIDENT";
			piiContact.OC_Phone = "+6423456001";
			var piiData = messageData.PIIs.AddNew();
			piiData.US_OC_Contact = piiContact.PK;
			piiData.US_Email = "THIS IS A LONG PERSON IDENTIFY EMAIL";
			piiData.US_SSN = "123-12-5436";
			piiData.US_PassportNo = "E512031578";
			piiData.US_ExpirationDate = new ZDateTime(2018, 12, 31);
			piiData.US_CountryOfIssuance = "CN";
			piiData.US_PassportType = PassportTypesList.Codes._01;
			piiData.US_Extension = "201";

			var relatedBusiness1 = messageData.RelatedBusinessItems.AddNew();
			relatedBusiness1.US_RelatedBusiness = ImporterRelatedBusinessTypeList.Codes._01;
			relatedBusiness1.US_NameOfEntity = "THIS IS A LONG RELATED BUSINESS NAME";
			relatedBusiness1.US_Number = "321-12-8542";
			var relatedBusiness2 = messageData.RelatedBusinessItems.AddNew();
			relatedBusiness2.US_RelatedBusiness = ImporterRelatedBusinessTypeList.Codes._02;
			relatedBusiness2.US_NameOfEntity = "CURRENT BUSINESS";
			relatedBusiness2.US_Number = "181101-02342";

			var individualContact = organisation.Contacts.AddNew();
			individualContact.OC_ContactName = "THISISALONGINDIVIDUALFIRSTNAME THISISALONGINDIVIDUALLASTNAME";
			individualContact.OC_Title = "MANAGER";
			individualContact.OC_Phone = "+6423456011";
			messageData.US_OC_CertifyIndividual = individualContact.PK;
			messageData.US_IndividualPhone = "+6423456002";
			messageData.US_BrokerName = "THIS IS A LONG BROKER NAME WHICH LENGTH EXCEED";
			messageData.US_BrokerPhone = "+6423456012";
			messageData.US_AcknowledgeAndSign = true;

			var message = new ImporterADDBuilder(messageData).Generate();
			AssertEquals("Message type is set", ACEApplicationIdentifierCodeList.Codes.AddNewCBPFormCBPF5106DatatotheImporterFile, message.EM_MessageType);
			AssertMultilineASCIIEquals(@"B  8888XJ5TP                                               <<MSGNO PLACEHOLDER>>
T1A89-002203600THIS IS A LONG IMPORTER NAME WITTEST MAILING ADDRESS            C
TADIVA&S FURNISHING BEDDING DIVISION                                            
T2                                            CHICAGO              IL60091    US
T3THIS IS A LONG IMPORTER NAME W                                                
TNIN1ITH LENGTH GREATER THAN 32                                                 
TBTEST PHYSICAL ADDRESS                                                         
TCLONDON               FN1234     GB                                            
TD2X   XOTHER DESC     CTPATISA            +6423456789    001    X X            
TE1               8EXPLANATION    I SELL PRODUCTS                               
TFTHISISAVERYLONGEMAILADDRESS@ABHTTPS://WWW.ABCDEFGHIJKLMNOPQR                  
TNCE1C.COM                                                                      
TNCW1ST12345.COM                                                                
TG123456987654321XJ52018AKUS123ABC321CBA                                        
THTHIS IS A LONG BANK NAME THAT 1234567890ATHIS IS A LONG CITY NAME THAT ABCA   
TNBN1EXCEED 30 CHARACTERS                                                       
TNBC1LENGTH EXCEED 30 CHARACTERS                                                
TI01THISISALONGPERSONIDENTIFYLASTNPRESIDENT             123125436               
TNCN1AME, THISISALONGPERSONIDENTIFYFIRSTNAME                                    
TJ01E512031578   12312018CN16423456001     201   THIS IS A LONG PERSON IDENTIFY 
TNCE2 EMAIL                                                                     
TK011THIS IS A LONG RELATED BUSINES321-12-8542                                  
TNNE1S NAME                                                                     
TK022CURRENT BUSINESS              181101-02342                                 
TLXTHISISALONGINDIVIDUALLASTNAME,MANAGER                                        
TNIN2 THISISALONGINDIVIDUALFIRSTNAME                                            
TMTHIS IS A LONG BROKER NAME WHI+6423456002    +6423456012                      
TNBN2CH LENGTH EXCEED                                                           
Y  8888XJ5TP", message.EM_FormattedMessageText);
		}
	}
}
