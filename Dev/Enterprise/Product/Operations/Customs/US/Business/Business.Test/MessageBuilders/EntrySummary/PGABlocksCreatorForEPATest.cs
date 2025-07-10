using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForEPATest : PGABlocksCreatorTest
	{
		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesSample1()
		{
			/*
			 A shipment of Toyota Sedans is imported into the United States. 
			 The shipment’s import code is “A”. Trade is required to provide Test Group Name/Engine Family Name, 
			 EPA Certification Number, Certificate Expiration Date, and Model Year. Exhibit 2-5 depicts the 
			 high-level structure of Vehicles and Engines Sample 1 depicted in Exhibit 2-6. */

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "TOYOTA SEDANS";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_OA_Owner = manufacturer.MainAddress.PK;
			vehicleLine.US_OA_StorageLocation = importer.MainAddress.PK;
			vehicleLine.US_ModelYear = "2014";
			vehicleLine.US_VehicleModel = "PRIUS";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VNEElectronicImage = true;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._12;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "JTDBBADU5B0865DK0";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_IdentityNumber = "JTDBBADU5B1865DK1";

			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_IdentityNumber = "JTDBBADU5B1865DK2";

			var vehicleDetails4 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails4.US_IdentityNumber = "JTDBBADU5B1865DK3";

			var vehicleDetails5 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails5.US_IdentityNumber = "JTDBBADU5B1865DK4";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        TOYOTA SEDANS                                                         
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP2A                                                                        
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK1
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK2
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK3
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK4
PG07                                   PRIUS          122013AKGJTDBBADU5B0865DK0
PG10           V06     2014                                                     
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                  TOYOTA (JAPAN)                  1234 PEACHTREE STREET  
PG20                                     ATLANTA              CA US30301        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG19STL                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21STLJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG22 942         CI EP2 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          "));
		}

		public void TestVNEEngineWithExemptionNumber()
		{
			SetUpData();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VehicleExemptionNumber = "EXEMPTIONNUMBER";
			vehicleLine.US_BondExemption = "Y";
			vehicleLine.US_BodyType = CommodityVehicleQualifierCodesList.Codes.V00;

			importer = declaration.Importer;
			importer.MainAddress.OA_Address1 = "1234 MARPLELEAF AVENUE";
			importer.MainAddress.OA_Address2 = "";
			importer.MainAddress.OA_Email = "";
			importer.MainAddress.OA_Phone = "5551230123";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "92169";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "SAN DIEGO";
			importer.MainAddress.OA_State = "CA";
			DeclarationTestHelper.AddPGAContact(importer, "ANGELO", "GIOVANNI", "5551230123", "ANGELO.GIOVANNI@TEST.COM", null);
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();

			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = new ZDateTime(2018, 05, 01);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("ACE Entry Summary message including EPA data",
@"OI        LAMBORGINI AVENTADOR                                                  
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP1E1Y                                                                      
PG24EP2O                                                                        
PG07                                   EM1000         122013ENNMHO-874200       
PG10           V00                                                              
PG14 EP9EXEMPTIONNUMBER                                                         
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                                                  1234 MARPLELEAF AVENUE 
PG20                                     SAN DIEGO            CA US92169        
PG21DFPANGELO GIOVANNI        5551230123     ANGELO.GIOVANNI@TEST.COM           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 942         CI EP2 Y05012018                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          
6249900003464                                                                   
", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesSample2()
		{
			/*
			 A shipment of Toyota Engines for off-road use is imported into the United States. 
			 Exhibit 2-7 illustrates the high-level structure of Vehicles and Engines Sample 2 depicted in Exhibit 2-8. */

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "TOYOTA ENGINES";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_EnginePower = 13m;
			vehicleLine.US_EnginePowerUQ = EnginePowerUQList.Codes.KW;
			vehicleLine.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicleLine.US_CertOfConformity = "7CSCC1234ABD";
			vehicleLine.US_CertOfConformityExpiryDate = new ZDateTime(2020, 12, 31);
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_EngineNumber = "MHO-874201";

			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_EngineNumber = "MHO-874202";

			var vehicleDetails4 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails4.US_EngineNumber = "MHO-874203";

			var vehicleDetails5 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails5.US_EngineNumber = "MHO-874204";

			var vehicleDetails6 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails6.US_EngineNumber = "MHO-874205";

			var vehicleDetails7 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails7.US_EngineNumber = "MHO-874206";

			var vehicleDetails8 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails8.US_EngineNumber = "MHO-874207";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        TOYOTA ENGINES                                                        
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP222                                                                       
PG07                                   EM1000         122013ENNMHO-874201       
PG07                                   EM1000         122013ENNMHO-874202       
PG07                                   EM1000         122013ENNMHO-874203       
PG07                                   EM1000         122013ENNMHO-874204       
PG07                                   EM1000         122013ENNMHO-874205       
PG07                                   EM1000         122013ENNMHO-874206       
PG07                                   EM1000         122013ENNMHO-874207       
PG07                                   EM1000         122013ENNMHO-874200       
PG10           V03 KW  13                                                       
PG14 EP47CSCC1234ABD                     112312020                              
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 943         CI EP1 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesSample3ForEPAV8()
		{
			/*
			 A shipment of certified Toyota Engines for Off-road Use is imported into the United States. 
			 Trade is required to specify if the import is exempt from bond, the Bond Issuer NAIC#, 
			 Bond Policy Number, and Bond Issuer State. Exhibit 2-9 illustrates the high-level structure 
			 of Vehicles and Engines Sample 3 depicted in Exhibit 2-10.*/

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "CERTIFIED TOYOTA ENGINES";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicleLine.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicleLine.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicleLine.US_VehicleModel = "EM1001";
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_BondPolicyNo = "123456";
			vehicleLine.US_NAICNo = "23456";
			vehicleLine.US_StateOfIssue = "GA";
			vehicleLine.US_BondExemption = YesNoDefaultList.Codes.No;
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VNEElectronicImage = true;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_EngineModel = "EM1001";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 22);
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";
			vehicleDetails.US_VehicleManufacturer = "";
			vehicleDetails.US_MfrDateType = ManufactureDateTypeList.Codes.ENG;

			vehicleLine.US_EnginePower = 2000;
			vehicleLine.US_EnginePowerUQ = "KW";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_EngineNumber = "MHO-874201";

			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_EngineNumber = "MHO-874202";

			var vehicleDetails4 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails4.US_EngineNumber = "MHO-874203";

			var vehicleDetails5 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails5.US_EngineNumber = "MHO-874204";

			var vehicleDetails6 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails6.US_EngineNumber = "MHO-874205";

			var vehicleDetails7 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails7.US_EngineNumber = "MHO-874206";

			var vehicleDetails8 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails8.US_EngineNumber = "MHO-874207";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        CERTIFIED TOYOTA ENGINES                                              
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP1E1N                                                                      
PG24EP21                                                                        
PG24EP3G                                                                        
PG07                                   EM1001         122013ENNMHO-874201       
PG07                                   EM1001         122013ENNMHO-874202       
PG07                                   EM1001         122013ENNMHO-874203       
PG07                                   EM1001         122013ENNMHO-874204       
PG07                                   EM1001         122013ENNMHO-874205       
PG07                                   EM1001         122013ENNMHO-874206       
PG07                                   EM1001         122013ENNMHO-874207       
PG07                                   EM1001         122013ENNMHO-874200       
PG10           V03 KW  2000                                                     
PG10           V05 ENG                                                          
PG14 EP49EPAV01.0ABC                                                            
PG14 EP7123456                                                                  
PG19MF                   TOYOTA (JAPAN)                                         
PG19NAI   23456                                                                 
PG20                                                          GA                
PG19DFP                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 943         CI EP1 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesSample3()
		{
			/*
			 A shipment of certified Toyota Engines for Off-road Use is imported into the United States. 
			 Trade is required to specify if the import is exempt from bond, the Bond Issuer NAIC#, 
			 Bond Policy Number, and Bond Issuer State. Exhibit 2-9 illustrates the high-level structure 
			 of Vehicles and Engines Sample 3 depicted in Exhibit 2-10.*/

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "CERTIFIED TOYOTA ENGINES";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicleLine.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicleLine.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicleLine.US_VehicleModel = "EM1001";
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_BondPolicyNo = "123456";
			vehicleLine.US_NAICNo = "23456";
			vehicleLine.US_StateOfIssue = "GA";
			vehicleLine.US_BondExemption = YesNoDefaultList.Codes.No;
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VNEElectronicImage = true;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_EngineModel = "EM1001";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 22);
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";
			vehicleDetails.US_MfrDateType = ManufactureDateTypeList.Codes.ENG;

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_EngineNumber = "MHO-874201";

			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_EngineNumber = "MHO-874202";

			var vehicleDetails4 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails4.US_EngineNumber = "MHO-874203";

			var vehicleDetails5 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails5.US_EngineNumber = "MHO-874204";

			var vehicleDetails6 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails6.US_EngineNumber = "MHO-874205";

			var vehicleDetails7 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails7.US_EngineNumber = "MHO-874206";

			var vehicleDetails8 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails8.US_EngineNumber = "MHO-874207";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        CERTIFIED TOYOTA ENGINES                                              
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP1E1N                                                                      
PG24EP21                                                                        
PG24EP3G                                                                        
PG07                                   EM1001         122013ENNMHO-874201       
PG07                                   EM1001         122013ENNMHO-874202       
PG07                                   EM1001         122013ENNMHO-874203       
PG07                                   EM1001         122013ENNMHO-874204       
PG07                                   EM1001         122013ENNMHO-874205       
PG07                                   EM1001         122013ENNMHO-874206       
PG07                                   EM1001         122013ENNMHO-874207       
PG07                                   EM1001         122013ENNMHO-874200       
PG10           V05 ENG                                                          
PG14 EP49EPAV01.0ABC                                                            
PG14 EP7123456                                                                  
PG19MF                   TOYOTA (JAPAN)                                         
PG19NAI   23456                                                                 
PG20                                                          GA                
PG19DFP                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 943         CI EP1 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesSample4EPAV8()
		{
			/*
			Certified Kubota Engines that have been installed in tractors are imported into the United States. 
			Exhibit 2-11 illustrates the high-level structure of Vehicles and Engines Sample 4 depicted in Exhibit 2-12.*/
			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "CERTIFIED KUBOTA ENGINES IN TRACTORS";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_IndustryCode = IndustryCodesList.Codes.H;
			vehicleLine.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicleLine.US_BondExemption = YesNoDefaultList.Codes.Yes;
			vehicleLine.US_BodyType = CommodityVehicleQualifierCodesList.Codes.V02;
			vehicleLine.US_BodyDescription = "GRAIN HARVESTING";
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicleLine.US_VehicleModel = "TRACTOR 3000";
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VNEElectronicImage = true;
			vehicleLine.US_EnginePower = 2000;
			vehicleLine.US_EnginePowerUQ = "KW";

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2014";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			vehicleDetails.US_IdentityNumber = "MHO-123-456";

			vehicleDetails.US_EngineModel = "EM1002";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 22);
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineManufacturer = "KUBOTA (JAPAN)";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			vehicleDetails2.US_IdentityNumber = "MHO-123-457";
			vehicleDetails2.US_EngineNumber = "MHO-874201";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        CERTIFIED KUBOTA ENGINES IN TRACTORS                                  
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP1E1Y                                                                      
PG24EP21                                                                        
PG24EP3H                                                                        
PG50                                                                            
PG07                                   TRACTOR 3000   012014SE MHO-123-457      
PG07                                   EM1002         122013ENNMHO-874201       
PG51                                                                            
PG07                                   TRACTOR 3000   012014SE MHO-123-456      
PG10           V03 KW  2000                                                     
PG10           V02     GRAIN HARVESTING                                         
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   TOYOTA (JAPAN)                                         
PG02C                                                                           
PG07                                   EM1002         122013ENNMHO-874200       
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   KUBOTA (JAPAN)                                         
PG19DFP                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 943         CI EP1 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesSample4()
		{
			/*
			Certified Kubota Engines that have been installed in tractors are imported into the United States. 
			Exhibit 2-11 illustrates the high-level structure of Vehicles and Engines Sample 4 depicted in Exhibit 2-12.*/

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "CERTIFIED KUBOTA ENGINES IN TRACTORS";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_IndustryCode = IndustryCodesList.Codes.H;
			vehicleLine.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicleLine.US_BondExemption = YesNoDefaultList.Codes.Yes;
			vehicleLine.US_BodyType = CommodityVehicleQualifierCodesList.Codes.V02;
			vehicleLine.US_BodyDescription = "GRAIN HARVESTING";
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicleLine.US_VehicleModel = "TRACTOR 3000";
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VNEElectronicImage = true;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2014";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			vehicleDetails.US_IdentityNumber = "MHO-123-456";

			vehicleDetails.US_EngineModel = "EM1002";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 22);
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineManufacturer = "KUBOTA (JAPAN)";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			vehicleDetails2.US_IdentityNumber = "MHO-123-457";
			vehicleDetails2.US_EngineNumber = "MHO-874201";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        CERTIFIED KUBOTA ENGINES IN TRACTORS                                  
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP1E1Y                                                                      
PG24EP21                                                                        
PG24EP3H                                                                        
PG50                                                                            
PG07                                   TRACTOR 3000   012014SE MHO-123-457      
PG07                                   EM1002         122013ENNMHO-874201       
PG51                                                                            
PG07                                   TRACTOR 3000   012014SE MHO-123-456      
PG10           V02     GRAIN HARVESTING                                         
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   TOYOTA (JAPAN)                                         
PG02C                                                                           
PG07                                   EM1002         122013ENNMHO-874200       
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   KUBOTA (JAPAN)                                         
PG19DFP                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 943         CI EP1 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesSample5()
		{
			/*
			A shipment containing a Lamborghini Aventador is imported into the United States by a 
			Non- resident for personal use for less than a year. Exhibit 2-13 illustrates the 
			high-level structure of Vehicles and Engines Sample 5 depicted in Exhibit 2-14.*/

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			importer = declaration.Importer;
			importer.MainAddress.OA_Address1 = "1234 MARPLELEAF AVENUE";
			importer.MainAddress.OA_Address2 = "";
			importer.MainAddress.OA_Email = "";
			importer.MainAddress.OA_Phone = "5551230123";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "92169";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "SAN DIEGO";
			importer.MainAddress.OA_State = "CA";
			DeclarationTestHelper.AddPGAContact(importer, "ANGELO", "GIOVANNI", "5551230123", "ANGELO.GIOVANNI@TEST.COM", null);
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "ZHWUU16M4CLAU5577";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        LAMBORGINI AVENTADOR                                                  
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2O                                                                        
PG07                                   LP700-4        012013AKGZHWUU16M4CLAU5577
PG14 EP92013-JUNE-LD-NONRES-450                                                 
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                                                  1234 MARPLELEAF AVENUE 
PG20                                     SAN DIEGO            CA US92169        
PG21DFPANGELO GIOVANNI        5551230123     ANGELO.GIOVANNI@TEST.COM           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 942         CI EP2 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesFor98Tariffs()
		{
			SetUpData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();

			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8703105060"));
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "8703105060";
			}
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "EP3";

			var tariff2 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff2 == null)
			{
				tariff2 = Factory.New<USCTariff>();
				tariff2.UE_Tariff = "9801008000";
			}
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_OGACodes = "FD3";
			tariff2.UE_PGACodes = "EP3";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = tariff2.UE_Tariff;
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION Test 98/99";
			invoiceLine.US_SPI = "AU";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vne = invoiceLine.VehicleLines.AddNew();
			vne.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vne.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vne.US_VehicleModel = "Test";
			vne.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains(@"OI        COMMERCIAL DESCRIPTION TEST 98/99                                     
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2O                                                                        
PG22 942         CI EP2 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          
508703105060 0000000000 0000000000 000000000000NO                               ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesWithCertifyingIndividualIsOWN()
		{
			/*
			 A shipment of Toyota Sedans is imported into the United States. 
			 The shipment’s import code is “A”. Trade is required to provide Test Group Name/Engine Family Name, 
			 EPA Certification Number, Certificate Expiration Date, and Model Year. Exhibit 2-5 depicts the 
			 high-level structure of Vehicles and Engines Sample 1 depicted in Exhibit 2-6. */

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "TOYOTA SEDANS";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_OA_Owner = manufacturer.MainAddress.PK;
			vehicleLine.US_OA_StorageLocation = importer.MainAddress.PK;
			vehicleLine.US_ModelYear = "2014";
			vehicleLine.US_VehicleModel = "PRIUS";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.Owner;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._12;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "JTDBBADU5B0865DK0";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_IdentityNumber = "JTDBBADU5B1865DK1";

			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_IdentityNumber = "JTDBBADU5B1865DK2";

			var vehicleDetails4 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails4.US_IdentityNumber = "JTDBBADU5B1865DK3";

			var vehicleDetails5 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails5.US_IdentityNumber = "JTDBBADU5B1865DK4";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        TOYOTA SEDANS                                                         
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2A                                                                        
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK1
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK2
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK3
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK4
PG07                                   PRIUS          122013AKGJTDBBADU5B0865DK0
PG10           V06     2014                                                     
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                  TOYOTA (JAPAN)                  1234 PEACHTREE STREET  
PG20                                     ATLANTA              CA US30301        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG55CI                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG19STL                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21STLJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG22 942         CI EP2 Y05142015                                               "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesWithCertifyingIndividualIsIMP()
		{
			/*
			 A shipment of Toyota Sedans is imported into the United States. 
			 The shipment’s import code is “A”. Trade is required to provide Test Group Name/Engine Family Name, 
			 EPA Certification Number, Certificate Expiration Date, and Model Year. Exhibit 2-5 depicts the 
			 high-level structure of Vehicles and Engines Sample 1 depicted in Exhibit 2-6. */

			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "TOYOTA SEDANS";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_OA_Owner = manufacturer.MainAddress.PK;
			vehicleLine.US_OA_StorageLocation = importer.MainAddress.PK;
			vehicleLine.US_ModelYear = "2014";
			vehicleLine.US_VehicleModel = "PRIUS";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._12;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "JTDBBADU5B0865DK0";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_IdentityNumber = "JTDBBADU5B1865DK1";

			var vehicleDetails3 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails3.US_IdentityNumber = "JTDBBADU5B1865DK2";

			var vehicleDetails4 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails4.US_IdentityNumber = "JTDBBADU5B1865DK3";

			var vehicleDetails5 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails5.US_IdentityNumber = "JTDBBADU5B1865DK4";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("ACE Entry Summary message including EPA data", message.EM_FormattedMessageText.Contains(@"OI        TOYOTA SEDANS                                                         
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2A                                                                        
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK1
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK2
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK3
PG07                                   PRIUS          122013AKGJTDBBADU5B1865DK4
PG07                                   PRIUS          122013AKGJTDBBADU5B0865DK0
PG10           V06     2014                                                     
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                  TOYOTA (JAPAN)                  1234 PEACHTREE STREET  
PG20                                     ATLANTA              CA US30301        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG55CI                                                                          
PG19STL                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21STLJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG22 942         CI EP2 Y05142015                                               "));
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesFor3520_1ForEPAV8()
		{
			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			importer = declaration.Importer;
			importer.MainAddress.OA_Address1 = "1234 MARPLELEAF AVENUE";
			importer.MainAddress.OA_Address2 = "";
			importer.MainAddress.OA_Email = "";
			importer.MainAddress.OA_Phone = "5551230123";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "92169";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "SAN DIEGO";
			importer.MainAddress.OA_State = "CA";
			DeclarationTestHelper.AddPGAContact(importer, "ANGELO", "GIOVANNI", "5551230123", "ANGELO.GIOVANNI@TEST.COM", null);
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "ZHWUU16M4CLAU5577";
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("ACE Entry Summary message including EPA data",
@"OI        LAMBORGINI AVENTADOR                                                  
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2O                                                                        
PG07                                   LP700-4        012013AKGZHWUU16M4CLAU5577
PG14 EP92013-JUNE-LD-NONRES-450                                                 
PG19MF                   TOYOTA (JAPAN)                                         
PG02C                                                                           
PG07                                   EM1000         122013ENNMHO-874200       
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                                                  1234 MARPLELEAF AVENUE 
PG20                                     SAN DIEGO            CA US92169        
PG21DFPANGELO GIOVANNI        5551230123     ANGELO.GIOVANNI@TEST.COM           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 942         CI EP2 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          
6249900003464                                                                   
", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 14)]
		public void TestVehiclesAndEnginesFor3520_1()
		{
			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			importer = declaration.Importer;
			importer.MainAddress.OA_Address1 = "1234 MARPLELEAF AVENUE";
			importer.MainAddress.OA_Address2 = "";
			importer.MainAddress.OA_Email = "";
			importer.MainAddress.OA_Phone = "5551230123";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "92169";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "SAN DIEGO";
			importer.MainAddress.OA_State = "CA";
			DeclarationTestHelper.AddPGAContact(importer, "ANGELO", "GIOVANNI", "5551230123", "ANGELO.GIOVANNI@TEST.COM", null);
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "ZHWUU16M4CLAU5577";
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("ACE Entry Summary message including EPA data",
@"OI        LAMBORGINI AVENTADOR                                                  
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2O                                                                        
PG07                                   LP700-4        012013AKGZHWUU16M4CLAU5577
PG14 EP92013-JUNE-LD-NONRES-450                                                 
PG19MF                   TOYOTA (JAPAN)                                         
PG02C                                                                           
PG07                                   EM1000         122013ENNMHO-874200       
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                                                  1234 MARPLELEAF AVENUE 
PG20                                     SAN DIEGO            CA US92169        
PG21DFPANGELO GIOVANNI        5551230123     ANGELO.GIOVANNI@TEST.COM           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG22 942         CI EP2 Y05142015                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          
6249900003464                                                                   
", message.EM_FormattedMessageText);
		}

		public void TestVNEWithAdditionalNumbers()
		{
			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "V0000001";
			vehicleDetails.US_EngineNumber = "E0000001";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			var additionalNumber1 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber1.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber1.CY_Data = "1V0000002";

			var additionalNumber2 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber2.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber2.CY_Data = "1V0000003";

			var additionalNumber3 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber3.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber3.CY_Data = "1V0000004";

			var additionalNumber4 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber4.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber4.CY_Data = "1V0000005";

			var additionalNumber5 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber5.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber5.CY_Data = "1V0000006";

			var additionalNumber6 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber6.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber6.CY_Data = "1E0000002";

			var additionalNumber7 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber7.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber7.CY_Data = "1E0000003";

			var additionalNumber8 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber8.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber8.CY_Data = "1E0000004";

			var additionalNumber9 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber9.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber9.CY_Data = "1E0000005";

			var additionalNumber10 = vehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber10.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber10.CY_Data = "1E0000006";

			var vehicleDetails2 = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails2.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails2.US_BuildYear = "2013";
			vehicleDetails2.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails2.US_IdentityNumber = "V0000001";
			vehicleDetails2.US_EngineNumber = "E0000001";
			vehicleDetails2.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails2.US_EngineModel = "EM1000";
			vehicleDetails2.US_EngineManufacturer = "TOYOTA (JAPAN)";

			var additionalNumber12 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber12.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber12.CY_Data = "2V0000002";

			var additionalNumber22 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber22.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber22.CY_Data = "2V0000003";

			var additionalNumber32 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber32.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber32.CY_Data = "2V0000004";

			var additionalNumber42 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber42.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber42.CY_Data = "2V0000005";

			var additionalNumber52 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber52.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber52.CY_Data = "2V0000006";

			var additionalNumber62 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber62.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber62.CY_Data = "2E0000002";

			var additionalNumber72 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber72.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber72.CY_Data = "2E0000003";

			var additionalNumber82 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber82.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber82.CY_Data = "2E0000004";

			var additionalNumber92 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber92.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber92.CY_Data = "2E0000005";

			var additionalNumber102 = vehicleDetails2.AdditionalNumbers.AddNew();
			additionalNumber102.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber102.CY_Data = "2E0000006";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var messageText = message.EM_FormattedMessageText;

			AssertContains(
@"PG50                                                                            
PG07                                   LP700-4        012013AKGV0000001         
PG082V0000002        2V0000003        2V0000004        2V0000005                
PG082V0000006                                                                   
PG07                                   EM1000         122013ENNE0000001         
PG082E0000002        2E0000003        2E0000004        2E0000005                
PG082E0000006                                                                   
PG51                                                                            
PG07                                   LP700-4        012013AKGV0000001         
PG081V0000002        1V0000003        1V0000004        1V0000005                
PG081V0000006                                                                   
PG14 EP92013-JUNE-LD-NONRES-450                                                 
PG19MF                   TOYOTA (JAPAN)                                         
PG02C                                                                           
PG07                                   EM1000         122013ENNE0000001         
PG081E0000002        1E0000003        1E0000004        1E0000005                
PG081E0000006                                                                   
PG19MF                   TOYOTA (JAPAN)                                         ", messageText);
		}

		public void TestPRCountryForVehicles()
		{
			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;

			importer = declaration.Importer;
			importer.MainAddress.OA_Address1 = "1234 MARPLELEAF AVENUE";
			importer.MainAddress.OA_Address2 = "";
			importer.MainAddress.OA_Email = "";
			importer.MainAddress.OA_Phone = "5551230123";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "92169";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "PRLAX";
			importer.MainAddress.OA_City = "SAN DIEGO";
			importer.MainAddress.OA_State = "PR";
			DeclarationTestHelper.AddPGAContact(importer, "ANGELO", "GIOVANNI", "5551230123", "ANGELO.GIOVANNI@TEST.COM", null);
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "ZHWUU16M4CLAU5577";
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("ACE Entry Summary message including EPA data",
@"PG19DFP                                                  1234 MARPLELEAF AVENUE 
PG20                                     SAN DIEGO            PR US92169        ", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 07, 01)]
		public void TestVNEManufacturerNameAndEmailOverflowForEPAV8()
		{
			SetUpData();
			declaration.US_EnableCRL = false;

			DeclarationTestHelper.AddPGAContact(declaration.IORWrapper.organisation, "IOR", "IAN TEST LONG OVERLOW NAME", null, "THIS.IS.A.VERY.LONG.EMAIL@ABCDEFGHI.COM", null);

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "ZHWUU16M4CLAU5577";
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("PG60 blocks should be generated",
@"OI        LAMBORGINI AVENTADOR                                                  
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2O                                                                        
PG07                                   LP700-4        012013AKGZHWUU16M4CLAU5577
PG14 EP92013-JUNE-LD-NONRES-450                                                 
PG19MF                   TOYOTA (JAPAN)                                         
PG02C                                                                           
PG07                                   EM1000         122013ENNMHO-874200       
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR IAN TEST LONG OVERL04123456       THIS.IS.A.VERY.LONG.EMAIL@ABCDEFGHI
PG60INAOW NAME                                                                  
PG60EMA.COM                                                                     
PG55CI                                                                          
PG22 942         CI EP2 Y07012016                                               
6249900003464                                                                   
", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 07, 01)]
		public void TestVNEManufacturerNameAndEmailOverflow()
		{
			SetUpData();
			declaration.US_EnableCRL = false;

			var iorWrapper = declaration.IORWrapper;
			DeclarationTestHelper.AddPGAContact(iorWrapper.organisation, "IOR", "IAN TEST LONG OVERLOW NAME", null, "THIS.IS.A.VERY.LONG.EMAIL@ABCDEFGHI.COM", null);

			invoiceLine.JI_Description = "LAMBORGINI AVENTADOR";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_VehicleExemptionNumber = "2013-JUNE-LD-NONRES-450";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.O;
			vehicleLine.US_VehicleModel = "LP700-4";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._01;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "ZHWUU16M4CLAU5577";
			vehicleDetails.US_EngineNumber = "MHO-874200";
			vehicleDetails.US_EngineBuildDate = new ZDateTime(2013, 12, 01);
			vehicleDetails.US_EngineModel = "EM1000";
			vehicleDetails.US_EngineManufacturer = "TOYOTA (JAPAN)";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains("PG60 blocks should be generated",
@"OI        LAMBORGINI AVENTADOR                                                  
PG01001EPAVNE                                                                   
PG02P                                                                           
PG24EP2O                                                                        
PG07                                   LP700-4        012013AKGZHWUU16M4CLAU5577
PG14 EP92013-JUNE-LD-NONRES-450                                                 
PG19MF                   TOYOTA (JAPAN)                                         
PG02C                                                                           
PG07                                   EM1000         122013ENNMHO-874200       
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR IAN TEST LONG OVERL04123456       THIS.IS.A.VERY.LONG.EMAIL@ABCDEFGHI
PG60INAOW NAME                                                                  
PG60EMA.COM                                                                     
PG55CI                                                                          
PG22 942         CI EP2 Y07012016                                               
6249900003464                                                                   
", message.EM_FormattedMessageText);
		}

		public void TestCertifyingDateIsSetToCurrentDateIfEmpty()
		{
			SetUpData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "SE";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.VehicleLines.AddNew();

			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var today = ZDate.Today;
			AssertContains("PG22             CI EP2 Y" + today.ToString("MMddyyyy"), message.EM_FormattedMessageText);

			invoiceLine.InvoiceHeader.US_VNESignDate = new ZDateTime(2016, 11, 29);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("PG22             CI EP2 Y11292016", message.EM_FormattedMessageText);
		}

		[TestDate(2020, 04, 15)]
		public void TestSendVNEFor06FTZWithoutCertifyCargoRelease()
		{
			SetUpData();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;

			invoiceLine.JI_Description = "TOYOTA SEDANS";
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_OA_Owner = manufacturer.MainAddress.PK;
			vehicleLine.US_OA_StorageLocation = importer.MainAddress.PK;
			vehicleLine.US_ModelYear = "2014";
			vehicleLine.US_VehicleModel = "PRIUS";
			vehicleLine.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			vehicleLine.US_CertOfConformity = "9EPAV01.0ABC";
			vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			vehicleLine.US_VNEElectronicImage = true;

			var vehicleDetails = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_BuildMonth = MonthList.Codes._12;
			vehicleDetails.US_BuildYear = "2013";
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "JTDBBADU5B0865DK0";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration, false);
			invoiceLine.InvoiceHeader.US_VNESignDate = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including VNE data for entry type 06 non-weekly entry", @"OI        TOYOTA SEDANS                                                         
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP2A                                                                        
PG07                                   PRIUS          122013AKGJTDBBADU5B0865DK0
PG10           V06     2014                                                     
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                  TOYOTA (JAPAN)                  1234 PEACHTREE STREET  
PG20                                     ATLANTA              CA US30301        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG19STL                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21STLJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG22 942         CI EP2 Y04152020                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          ", message.EM_FormattedMessageText);

			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including VNE data for entry type 06 weekly entry", @"OI        TOYOTA SEDANS                                                         
PG01001EPAVNE   Y                                                               
PG02P                                                                           
PG24EP2A                                                                        
PG07                                   PRIUS          122013AKGJTDBBADU5B0865DK0
PG10           V06     2014                                                     
PG14 EP49EPAV01.0ABC                                                            
PG19MF                   TOYOTA (JAPAN)                                         
PG19DFP                  TOYOTA (JAPAN)                  1234 PEACHTREE STREET  
PG20                                     ATLANTA              CA US30301        
PG21DFPJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR NAME               04123456       IOR EMAIL                          
PG19STL                                                  IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21STLJANE SMITH             7062345678     J.SMITH@TOYOTAAMERICA.COM          
PG22 942         CI EP2 Y04152020                                               
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CI                                                                          ", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			importer.MainAddress.OA_Address1 = "IMPORTER STREET 1";
			importer.MainAddress.OA_Address2 = "IMPORTER STREET 2";
			importer.MainAddress.OA_Email = "importer@test.com";
			importer.MainAddress.OA_Phone = "6934568700";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "96358";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_RN_NKCountryCode = "US";
			importer.MainAddress.OA_City = "LOS ANGELES";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "936528466", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(importer, "JANE", "SMITH", "7062345678", "J.SMITH@TOYOTAAMERICA.COM", null);
			declaration.JE_OH_Importer = importer.PK;

			var ior = Factory.New<OrgHeader>();
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_RL_NKRelatedPortCode = "USLAX";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "NAME", "04 123456", "IOR EMAIL", "IOR FAX");
			declaration.IOROrgPK = ior.PK;

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";

			manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "TOYOTA (JAPAN)";
			var address = manufacturer.MainAddress;
			address.OA_Address1 = "1234 PEACHTREE STREET";
			address.OA_City = "ATLANTA";
			address.OA_State = "GA";
			address.OA_RL_NKRelatedPortCode = "USLAX";
			address.OA_PostCode = "30301";
			DeclarationTestHelper.AddPGAContact(manufacturer, "JANE", "SMITH", "7062345678", "J.SMITH@TOYOTAAMERICA.COM", null);
			invoiceLine.JI_OA_ManufacturerAddress = address.PK;
		}
		OrgHeader manufacturer;
		OrgHeader importer;
	}
}
