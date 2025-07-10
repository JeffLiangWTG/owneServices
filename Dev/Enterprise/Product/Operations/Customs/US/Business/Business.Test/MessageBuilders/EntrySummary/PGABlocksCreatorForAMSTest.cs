using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForAMSTest : PGABlocksCreatorTest
	{
		/*In the example, 27,000 pounds of fresh kiwifruit of the variety ‘Hayward’ are being imported and are subject to inspection for quality requirements.  
		 * There are 1,000 27-pound cartons of 60count product. 
		 */
		public void TestAMSMO1MessageSetLayout()
		{
			SetUpData();

			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;
			declaration.US_FDAADTA = new ZDateTime(2015, 5, 27);

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST MO1";

			var mo1Line = amsLine.AMSLines.AddNew();
			mo1Line.US_InspecDateTime = new ZDateTime(2015, 5, 25, 12, 12, 11);
			mo1Line.US_InspecRemarks = "PLEASE CALL 30 MIN BEFFORE ARRIVAL";
			mo1Line.US_NetWeight = 270000m;
			mo1Line.US_NetWeightUQ = "LB";
			var applicant = GetImporterOrgHeader();
			mo1Line.US_OA_Applicant = applicant.MainAddress.PK;

			mo1Line.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			mo1Line.US_PackageWeight = 27m;
			mo1Line.US_PackageWeightUQ = "LB";
			mo1Line.US_Packages = 1000m;
			mo1Line.US_PackagesUQ = "CT";
			mo1Line.US_ProductNumber = "50303904";
			mo1Line.US_QtyPerPackage = 60m;
			mo1Line.US_QtyPerPackageUQ = "NO";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains(
@"PG01001AMSMO 1                           230.000                                
PG02PUNS 50303904                                                               
PG10                   KNZ TEST MO1                                             
PG19AP1                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG261000000100000CT                                                             
PG262000000002700LB                                                             
PG262000000006000NO                                                             
PG29LB 000027000000                                                             
PG30I052520151212    PLEASE CALL 30 MIN BEFFORE ARRIVAL                         
PG30A052720150000                                                               ", message.EM_FormattedMessageText);
		}

		/*In the example, 27,000 pounds of fresh kiwifruit of the variety ‘Hayward’ are being imported and are subject to inspection for quality requirements.  
		 * There are 1,000 27-pound cartons of 60count product. 
		 */
		public void TestAMSMO1MessageSetLayoutWithContainers()
		{
			SetUpData();

			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;
			declaration.US_FDAADTA = new ZDateTime(2015, 5, 27);

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "TST";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_RC = containerType.PK;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";

			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST MO1";

			var mo1Line = amsLine.AMSLines.AddNew();
			mo1Line.US_InspecDateTime = new ZDateTime(2015, 5, 25, 12, 12, 11);
			mo1Line.US_InspecRemarks = "PLEASE CALL 30 MIN BEFFORE ARRIVAL";
			mo1Line.US_NetWeight = 270000m;
			mo1Line.US_NetWeightUQ = "LB";
			var applicant = GetImporterOrgHeader();
			mo1Line.US_OA_Applicant = applicant.MainAddress.PK;

			mo1Line.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			mo1Line.US_PackageWeight = 27m;
			mo1Line.US_PackageWeightUQ = "LB";
			mo1Line.US_Packages = 1000m;
			mo1Line.US_PackagesUQ = "CT";
			mo1Line.US_ProductNumber = "50303904";
			mo1Line.US_QtyPerPackage = 60m;
			mo1Line.US_QtyPerPackageUQ = "NO";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains(
@"PG01001AMSMO 1                           230.000                                
PG02PUNS 50303904                                                               
PG10                   KNZ TEST MO1                                             
PG19AP1                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG261000000100000CT                                                             
PG262000000002700LB                                                             
PG262000000006000NO                                                             
PG27CONT1               140CONT2               2                                
PG29LB 000027000000                                                             
PG30I052520151212    PLEASE CALL 30 MIN BEFFORE ARRIVAL                         
PG30A052720150000                                                               ", message.EM_FormattedMessageText);
		}

		public void TestAMSMO2MessageWithFUNCS()
		{
			SetUpData();
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;
			declaration.US_FDAADTA = new ZDateTime(2015, 05, 29, 5, 5, 5);

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.AMSLines.RemoveAndDeleteAll();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "TEST MO2";

			var mo2Line = amsLine.AMSLines.AddNew();
			mo2Line.US_IsDocSubmitted = true;
			mo2Line.US_IssueDate = new ZDateTime(2015, 05, 25);
			mo2Line.US_Party = "Canadian Food Inspection Agency";
			mo2Line.US_UC_NKLocation = "CA";
			mo2Line.US_Weight = 40000m;
			mo2Line.US_WeightUQ = "LB";
			mo2Line.US_CertNumber = "E0000998";
			mo2Line.US_NetWeight = 50000m;
			mo2Line.US_NetWeightUQ = "KG";

			mo2Line.US_CertType = "AM3";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 2  Y                        230.000                                
PG02P                                                                           
PG10                   TEST MO2                                                 
PG13CANADIAN FOOD INSPECTION AGENCY    PR                                       
PG141AM3E0000998                          052520150000000400000000LB            
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG29KG 000005000000                                                             
PG30A05292015                                                                   ", message.EM_FormattedMessageText);

			var action2 = GetAction(declaration);
			var builder2 = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action2, UpdateActionCode.Add);
			var message2 = builder2.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 2  Y                        230.000                                
PG02P                                                                           
PG10                   TEST MO2                                                 
PG13CANADIAN FOOD INSPECTION AGENCY    PR                                       
PG141AM3E0000998                          052520150000000400000000LB            
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          ", message2.EM_FormattedMessageText);
		}

		/*In the example, 40,000 pounds of potatoes are being imported that have been previously inspected and are not subject to quality inspection at the border. */
		public void TestAMSMO2MessageSetLayout()
		{
			SetUpData();

			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST MO2";

			var mo2Line = amsLine.AMSLines.AddNew();
			mo2Line.US_IsDocSubmitted = true;
			mo2Line.US_IssueDate = new ZDateTime(2015, 05, 25);
			mo2Line.US_Party = "Canadian Food Inspection Agency";
			mo2Line.US_UC_NKLocation = "CA";
			mo2Line.US_Weight = 40000m;
			mo2Line.US_WeightUQ = "LB";
			mo2Line.US_CertNumber = "E0000998";
			mo2Line.US_CertType = "AM6";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 2  Y                        230.000                                
PG02P                                                                           
PG10                   KNZ TEST MO2                                             
PG13CANADIAN FOOD INSPECTION AGENCY    PR                                       
PG141AM6E0000998                          052520150000000400000000LB            
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         ", message.EM_FormattedMessageText);
		}

		/*In the example, 40,000 pounds of potatoes are being imported that have been previously inspected and are not subject to quality inspection at the border. */
		public void TestAMSMO2MessageSetLayoutWithContainers()
		{
			SetUpData();

			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "TST";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_RC = containerType.PK;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";

			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST MO2";

			var mo2Line = amsLine.AMSLines.AddNew();
			mo2Line.US_IsDocSubmitted = true;
			mo2Line.US_IssueDate = new ZDateTime(2015, 05, 25);
			mo2Line.US_Party = "Canadian Food Inspection Agency";
			mo2Line.US_UC_NKLocation = "CA";
			mo2Line.US_Weight = 40000m;
			mo2Line.US_WeightUQ = "LB";
			mo2Line.US_CertNumber = "E0000998";
			mo2Line.US_CertType = "AM6";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 2  Y                        230.000                                
PG02P                                                                           
PG10                   KNZ TEST MO2                                             
PG13CANADIAN FOOD INSPECTION AGENCY    PR                                       
PG141AM6E0000998                          052520150000000400000000LB            
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         ", message.EM_FormattedMessageText);
		}

		/*In the example, 40,000 pounds of potatoes are being imported for processing as a bulk load and are not subject to quality inspection. */
		public void TestAMSMO3MessageSetLayout()
		{
			SetUpData();

			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent

			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO3;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
			amsLine.US_CommercialDescription = "KNZ TEST MO3";
			var mo3Line = amsLine.AMSLines.AddNew();
			mo3Line.US_AuthorizationNumber = "39323";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 3                           025.000                                
PG02P                                                                           
PG10                   KNZ TEST MO3                                             
PG141AM239323                                                                   
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         ", message.EM_FormattedMessageText);
		}

		/*In the example, 40,000 pounds of potatoes are being imported for processing as a bulk load and are not subject to quality inspection.  
		 AMS has provided the entity reference numbers of BB11356 for the importer*/
		public void TestAMSMO4MessageSetLayout()
		{
			SetUpData();

			var ior = GetIOROrgHeader();
			ior.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "256", "US");
			declaration.IOROrgPK = ior.PK;

			var buyer = GetImporterOrgHeader();
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = buyer.PK;
			buyer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "425", "US");
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = buyer.PK;

			var cne = Factory.New<OrgHeader>();
			cne.OH_FullName = "ULTIMATE CONSIGNEE";
			cne.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AM0000001");
			var cneAddress = cne.Addresses.AddNew();
			cneAddress.OA_Address1 = "CNE ADDRESS 1";
			cneAddress.OA_Address2 = "CNE ADDRESS 2";
			cneAddress.OA_City = "CHARLESTON";
			cneAddress.OA_RL_NKRelatedPortCode = "US2CW";
			cneAddress.OA_State = "IL";
			cneAddress.OA_PostCode = "29492";
			invoiceLine.JI_OA_ConsigneeAddress = cneAddress.PK;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO4;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
			amsLine.US_CommercialDescription = "KNZ TEST MO4";
			var mo4Line = amsLine.AMSLines.AddNew();
			mo4Line.US_NetWeight = 22000m;
			mo4Line.US_NetWeightUQ = "LB";
			mo4Line.US_ProductNumber = "50401736";

			buyer.ClearAllNotifications();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 4                           025.000                                
PG02PUNS 50401736                                                               
PG10                   KNZ TEST MO4                                             
PG141AM2                                          0000000220000000LB            
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG19CN 331AM0000001      ULTIMATE CONSIGNEE              CNE ADDRESS 1          ", message.EM_FormattedMessageText);
		}

		/*In the example, 6,000 pounds of shelled pistachios are being imported and are subject to testing for quality requirements.  
		 There are three 2,000-lb containers of pistachios.*/
		public void TestAMSMO5MessageSetLayout()
		{
			SetUpData();
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO5;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST MO5";

			var mo5Line = amsLine.AMSLines.AddNew();
			mo5Line.US_InspecDateTime = new ZDateTime(2015, 5, 25, 12, 12, 11);
			mo5Line.US_InspecRemarks = "PLEASE CALL 30 MIN BEFFORE ARRIVAL";
			mo5Line.US_NetWeight = 6000m;
			mo5Line.US_NetWeightUQ = "LB";

			var applicant = GetImporterOrgHeader();
			mo5Line.US_OA_Applicant = applicant.MainAddress.PK;
			mo5Line.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			mo5Line.US_Packages = 3m;
			mo5Line.US_PackagesUQ = "MB";
			mo5Line.US_PackageWeight = 2000m;
			mo5Line.US_PackageWeightUQ = "LB";
			mo5Line.US_ProductNumber = "50102504";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 5                           230.000                                
PG02PUNS 50102504                                                               
PG10                   KNZ TEST MO5                                             
PG19AP1                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG21CB                                                                          
PG261000000000300MB                                                             
PG262000000200000LB                                                             
PG29LB 000000600000                                                             
PG30I052520151212    PLEASE CALL 30 MIN BEFFORE ARRIVAL                         ", message.EM_FormattedMessageText);
		}

		/*In the example, 6,000 pounds of shelled pistachios are being imported and are subject to testing for quality requirements.  
		 There are three 2,000-lb containers of pistachios.*/
		public void TestAMSMO5MessageSetLayoutWithContainers()
		{
			SetUpData();
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "TST";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_RC = containerType.PK;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";

			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO5;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST MO5";

			var mo5Line = amsLine.AMSLines.AddNew();
			mo5Line.US_InspecDateTime = new ZDateTime(2015, 5, 25, 12, 12, 11);
			mo5Line.US_InspecRemarks = "PLEASE CALL 30 MIN BEFFORE ARRIVAL";
			mo5Line.US_NetWeight = 6000m;
			mo5Line.US_NetWeightUQ = "LB";

			var applicant = GetImporterOrgHeader();
			mo5Line.US_OA_Applicant = applicant.MainAddress.PK;
			mo5Line.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			mo5Line.US_Packages = 3m;
			mo5Line.US_PackagesUQ = "MB";
			mo5Line.US_PackageWeight = 2000m;
			mo5Line.US_PackageWeightUQ = "LB";
			mo5Line.US_ProductNumber = "50102504";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 5                           230.000                                
PG02PUNS 50102504                                                               
PG10                   KNZ TEST MO5                                             
PG19AP1                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG21CB                                                                          
PG261000000000300MB                                                             
PG262000000200000LB                                                             
PG27CONT1               140CONT2               2                                
PG29LB 000000600000                                                             
PG30I052520151212    PLEASE CALL 30 MIN BEFFORE ARRIVAL                         ", message.EM_FormattedMessageText);
		}

		public void TestAMSPN1MessageSetLayout()
		{
			SetUpData();
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "TST";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_RC = containerType.PK;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";

			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.PN1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST PN1";

			var pn1Line = amsLine.AMSLines.AddNew();
			pn1Line.US_InspecDateTime = new ZDateTime(2015, 5, 25, 12, 12, 11);
			pn1Line.US_InspecRemarks = "PLEASE CALL 30 MIN BEFFORE ARRIVAL";
			pn1Line.US_NetWeight = 6000m;
			pn1Line.US_NetWeightUQ = "LB";

			var applicant = GetImporterOrgHeader();
			pn1Line.US_OA_Applicant = applicant.MainAddress.PK;
			pn1Line.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			pn1Line.US_Packages = 3m;
			pn1Line.US_PackagesUQ = "MB";
			pn1Line.US_PackageWeight = 2000m;
			pn1Line.US_PackageWeightUQ = "LB";
			pn1Line.US_ProductNumber = "50102504";

			var lotCode1 = pn1Line.LotCodes.AddNew();
			lotCode1.CY_Code = "3";
			lotCode1.CY_Data = "4-521413";

			var lotCode2 = pn1Line.LotCodes.AddNew();
			lotCode2.CY_Code = "3";
			lotCode2.CY_Data = "4-521412";

			var lotCode3 = pn1Line.LotCodes.AddNew();
			lotCode3.CY_Code = "3";
			lotCode3.CY_Data = "4-528743";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSPN 1                           230.000                                
PG02PUNS 50102504                                                               
PG10                   KNZ TEST PN1                                             
PG19AP1                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG21CB                                                                          
PG25          34-521413                                                         
PG25          34-521412                                                         
PG25          34-528743                                                         
PG261000000000300MB                                                             
PG262000000200000LB                                                             
PG27CONT1               140CONT2               2                                
PG29LB 000000600000                                                             
PG30I052520151212    PLEASE CALL 30 MIN BEFFORE ARRIVAL                         ", message.EM_FormattedMessageText);
		}

		/*In the example, 22,000 pounds of fresh table grapes of the variety ‘Tokay’ are being imported and are not subject to inspection for quality requirements. */
		public void TestAMSMO6MessageSetLayout()
		{
			SetUpData();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO6;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._250000;
			amsLine.US_CommercialDescription = "KNZ TEST MO6";

			var mo6Line = amsLine.AMSLines.AddNew();
			mo6Line.US_NetWeight = 22000m;
			mo6Line.US_NetWeightUQ = "LB";
			mo6Line.US_ProductNumber = "50303493";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 6                           250.000                                
PG02PUNS 50303493                                                               
PG10                   KNZ TEST MO6                                             
PG29LB 000002200000                                                             ", message.EM_FormattedMessageText);
		}

		/*The example shows an entry for in-shell large size table eggs in 6-egg consumer cartons in 900 cases, frozen, de-characterized inedible egg product in 76 100-pound drum containers, and hatching eggs on flats in 92 cases.*/
		public void TestAMSEG1MessageSetLayout()
		{
			SetUpData();
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.EG1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST EG1";

			var eg1Line1 = amsLine.AMSLines.AddNew();
			eg1Line1.US_ProductNumber = "50131612";
			eg1Line1.US_IsDocSubmitted = true;
			eg1Line1.US_OA_Applicant = GetImporterOrgHeader().MainAddress.PK;
			eg1Line1.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			eg1Line1.US_InspecDateTime = new ZDateTime(2015, 5, 28, 16, 45, 45);
			eg1Line1.US_InspecRemarks = "";
			eg1Line1.US_OuterPackage = 900m;
			eg1Line1.US_OuterPackageUQ = "CS";
			eg1Line1.US_InnerPackage = 30m;
			eg1Line1.US_InnerPackageUQ = "CT";
			eg1Line1.US_InnerAmount = 6m;
			eg1Line1.US_InnerAmountUQ = "NO";
			eg1Line1.US_TotalQuantity = 1350m;
			eg1Line1.US_TotalQuantityUQ = "DOZ";

			var eg1Line2 = amsLine.AMSLines.AddNew();
			eg1Line2.US_ProductNumber = "50131630";
			eg1Line2.US_IsDocSubmitted = true;
			eg1Line2.US_OA_Applicant = GetImporterOrgHeader().MainAddress.PK;
			eg1Line2.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			eg1Line2.US_InspecDateTime = new ZDateTime(2015, 5, 29, 12, 5, 30);
			eg1Line2.US_InspecRemarks = "UPON ARRIVAL, ENTER AT WAREHOUSE 2";
			eg1Line2.US_OuterPackage = 76m;
			eg1Line2.US_OuterPackageUQ = "DR";
			eg1Line2.US_InnerWeight = 100m;
			eg1Line2.US_InnerWeightUQ = "LB";
			eg1Line2.US_TotalWeight = 7600m;
			eg1Line2.US_TotalWeightUQ = "LB";

			var eg1Line3 = amsLine.AMSLines.AddNew();
			eg1Line3.US_ProductNumber = "50131630";
			eg1Line3.US_IsDocSubmitted = true;
			eg1Line3.US_OA_Applicant = GetImporterOrgHeader().MainAddress.PK;
			eg1Line3.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			eg1Line3.US_InspecDateTime = new ZDateTime(2015, 5, 30, 11, 17, 30);
			eg1Line3.US_InspecRemarks = "OPERATING HOURS 8AM TO";

			eg1Line3.US_OuterPackage = 92m;
			eg1Line3.US_OuterPackageUQ = "CS";
			eg1Line3.US_InnerPackage = 60m;
			eg1Line3.US_InnerPackageUQ = "NO";
			eg1Line3.US_TotalQuantity = 276m;
			eg1Line3.US_TotalQuantityUQ = "DOZ";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSEG 1  Y                        230.000                                
PG02PUNS 50131612                                                               
PG10                   KNZ TEST EG1                                             
PG19APP                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG261000000090000CS                                                             
PG262000000003000CT                                                             
PG263000000000600NO                                                             
PG29DOZ000000135000                                                             
PG30I052820151645                                                               
PG01002AMSEG 1  Y                        230.000                                
PG02PUNS 50131630                                                               
PG10                   KNZ TEST EG1                                             
PG19APP                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG261000000007600DR                                                             
PG262000000010000LB                                                             
PG29LB 000000760000                                                             
PG30I052920151205    UPON ARRIVAL, ENTER AT WAREHOUSE 2                         
PG01003AMSEG 1  Y                        230.000                                
PG02PUNS 50131630                                                               
PG10                   KNZ TEST EG1                                             
PG19APP                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG261000000009200CS                                                             
PG262000000006000NO                                                             
PG29DOZ000000027600                                                             
PG30I053020151117    OPERATING HOURS 8AM TO                                     ", message.EM_FormattedMessageText);
		}

		/*The example shows an entry for in-shell large size table eggs in 6-egg consumer cartons in 900 cases, frozen, de-characterized inedible egg product in 76 100-pound drum containers, and hatching eggs on flats in 92 cases.*/
		public void TestAMSEG1MessageSetLayoutWithContainers()
		{
			SetUpData();
			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "256", "US");//this should not be sent
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "331", "US");//this should not be sent
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "TST";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_RC = containerType.PK;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";

			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.EG1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "KNZ TEST EG1";

			var eg1Line1 = amsLine.AMSLines.AddNew();
			eg1Line1.US_ProductNumber = "50131612";
			eg1Line1.US_IsDocSubmitted = true;
			eg1Line1.US_OA_Applicant = GetImporterOrgHeader().MainAddress.PK;
			eg1Line1.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			eg1Line1.US_InspecDateTime = new ZDateTime(2015, 5, 28, 16, 45, 45);
			eg1Line1.US_InspecRemarks = "";
			eg1Line1.US_OuterPackage = 900m;
			eg1Line1.US_OuterPackageUQ = "CS";
			eg1Line1.US_InnerPackage = 30m;
			eg1Line1.US_InnerPackageUQ = "CT";
			eg1Line1.US_InnerAmount = 6m;
			eg1Line1.US_InnerAmountUQ = "NO";
			eg1Line1.US_TotalQuantity = 1350m;
			eg1Line1.US_TotalQuantityUQ = "DOZ";

			var eg1Line2 = amsLine.AMSLines.AddNew();
			eg1Line2.US_ProductNumber = "50131630";
			eg1Line2.US_IsDocSubmitted = true;
			eg1Line2.US_OA_Applicant = GetImporterOrgHeader().MainAddress.PK;
			eg1Line2.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			eg1Line2.US_InspecDateTime = new ZDateTime(2015, 5, 29, 12, 5, 30);
			eg1Line2.US_InspecRemarks = "UPON ARRIVAL, ENTER AT WAREHOUSE 2";
			eg1Line2.US_OuterPackage = 76m;
			eg1Line2.US_OuterPackageUQ = "DR";
			eg1Line2.US_InnerWeight = 100m;
			eg1Line2.US_InnerWeightUQ = "LB";
			eg1Line2.US_TotalWeight = 7600m;
			eg1Line2.US_TotalWeightUQ = "LB";

			var eg1Line3 = amsLine.AMSLines.AddNew();
			eg1Line3.US_ProductNumber = "50131630";
			eg1Line3.US_IsDocSubmitted = true;
			eg1Line3.US_OA_Applicant = GetImporterOrgHeader().MainAddress.PK;
			eg1Line3.US_OA_GoodsLocation = GetGoodLoactionOrgHeader().MainAddress.PK;
			eg1Line3.US_InspecDateTime = new ZDateTime(2015, 5, 30, 11, 17, 30);
			eg1Line3.US_InspecRemarks = "OPERATING HOURS 8AM TO";

			eg1Line3.US_OuterPackage = 92m;
			eg1Line3.US_OuterPackageUQ = "CS";
			eg1Line3.US_InnerPackage = 60m;
			eg1Line3.US_InnerPackageUQ = "NO";
			eg1Line3.US_TotalQuantity = 276m;
			eg1Line3.US_TotalQuantityUQ = "DOZ";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSEG 1  Y                        230.000                                
PG02PUNS 50131612                                                               
PG10                   KNZ TEST EG1                                             
PG19APP                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG261000000090000CS                                                             
PG262000000003000CT                                                             
PG263000000000600NO                                                             
PG27CONT1               140CONT2               2                                
PG29DOZ000000135000                                                             
PG30I052820151645                                                               
PG01002AMSEG 1  Y                        230.000                                
PG02PUNS 50131630                                                               
PG10                   KNZ TEST EG1                                             
PG19APP                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG261000000007600DR                                                             
PG262000000010000LB                                                             
PG27CONT1               140CONT2               2                                
PG29LB 000000760000                                                             
PG30I052920151205    UPON ARRIVAL, ENTER AT WAREHOUSE 2                         
PG01003AMSEG 1  Y                        230.000                                
PG02PUNS 50131630                                                               
PG10                   KNZ TEST EG1                                             
PG19APP                  BUYER                           BUYER ADDRESS 1        
PG20BUYER ADDRESS 2                      MELBOURN             MELUS2011         
PG21AP1BUYER CONTACT          04654321       BUYER EMAIL                        
PG19LG                   GOODS                           GOODS ADDRESS 1        
PG20GOODS ADDRESS 2                      MELBOURN             MEKUS2013         
PG21GC GOODS CONTACT          04654323       GOODS EMAIL                        
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR ALEXANDER THE GREAT04123456       IOR EMAIL                          
PG261000000009200CS                                                             
PG262000000006000NO                                                             
PG27CONT1               140CONT2               2                                
PG29DOZ000000027600                                                             
PG30I053020151117    OPERATING HOURS 8AM TO                                     ", message.EM_FormattedMessageText);
		}

		/*The example shows an entry for 2.760 dozen of hatching eggs on flats in 92 cases. */
		public void TestAMSEG2MessageSetLayout()
		{
			SetUpData();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.EG2;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._250000;
			amsLine.US_CommercialDescription = "KNZ TEST EG2";

			var eg2Line = amsLine.AMSLines.AddNew();
			eg2Line.US_IsDocSubmitted = true;
			eg2Line.US_PermitNumber = "1233333";
			eg2Line.US_ProductNumber = "50131612";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSEG 2  Y                        250.000                                
PG02PUNS 50131612                                                               
PG10                   KNZ TEST EG2                                             
PG141AM31233333                                                                 ", message.EM_FormattedMessageText);
		}

		/*Designates AMS as the recipient, provides the AMS program message code (MO7), and sets the Disclaimer flag to “B” to indicate that data is not required per agency guidance. */
		public void TestAMSMO7MessageSetLayout()
		{
			SetUpData();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 5, 28);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO7;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 7                                                                  
PG02P                                                                           
PG30A052820150000                                                               ", message.EM_FormattedMessageText);
		}

		public void TestAMSMO8MessageSetLayout()
		{
			SetUpData();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 5, 28);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO8;
			amsLine.US_NetWeight = 12m;
			amsLine.US_NetWeightUQ = "KG";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 8                                                                  
PG02P                                                                           
PG29KG 000000001200                                                             ", message.EM_FormattedMessageText);
		}

		/*Designates AMS as the recipient, provides the AMS program message code (MO8), and sets the Disclaimer flag to “B” to indicate that data is not required per agency guidance*/
		public void TestAMSMO8MessageSetLayoutForDisclaimed()
		{
			SetUpData();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimReason = "B";
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.MO8;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("PG01001AMSMO 8                                                                 B", message.EM_FormattedMessageText);
		}

		[TestDate(2021, 12, 01)]
		public void TestAMSDisclaimedReasonA()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0123456789";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = new ZDateTime(2021, 12, 31);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariff.UE_Tariff, new ZDateTime(2021, 01, 01), new ZDateTime(2022, 01, 01));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "EG1", tariffView);
			Factory.Save();

			SetUpData();

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("Disclaimed Reason is A.", "A", invoiceLine.US_AMSDisclaimReason);
			AssertEquals("Disclaim Program is EG1.", AMSProgramList.Codes.EG1, invoiceLine.US_AMSDisclaimProgram);
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("PG01001AMSEG 1                                                                 A", message.EM_FormattedMessageText);
		}

		public void TestAMSEntityNameAndEmailOverflow()
		{
			SetUpData();

			declaration.IOROrgPK = GetIOROrgHeader().PK;
			declaration.JE_GS_NKCusAgent = GetBroker().GS_Code;

			DeclarationTestHelper.AddPGAContact(declaration.IORWrapper.organisation, "IOR", "IAN TEST LONG ENTITY NAME FOR OVERFLOW", null, "THIS.IS.A.VERY.LONG.EMAIL@ABCDEFG.COM", null);

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "IAN TEST AMS PG60";

			var mo2Line = amsLine.AMSLines.AddNew();
			mo2Line.US_IsDocSubmitted = true;
			mo2Line.US_IssueDate = new ZDateTime(2015, 05, 25);
			mo2Line.US_Party = "Canadian Food Inspection Agency";
			mo2Line.US_UC_NKLocation = "CA";
			mo2Line.US_Weight = 40000m;
			mo2Line.US_WeightUQ = "LB";
			mo2Line.US_CertNumber = "E0000998";
			mo2Line.US_CertType = "AM6";
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSMO 2  Y                        230.000                                
PG02P                                                                           
PG10                   IAN TEST AMS PG60                                        
PG13CANADIAN FOOD INSPECTION AGENCY    PR                                       
PG141AM6E0000998                          052520150000000400000000LB            
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR IAN TEST LONG ENTIT04123456       THIS.IS.A.VERY.LONG.EMAIL@ABCDEFG.C
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          ", message.EM_FormattedMessageText);
		}

		public void TestAMSOR1MessageSetLayout()
		{
			SetUpData();

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "TEST EXPORTER";
			exporter.OH_Code = "TESTEXP";
			exporter.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AMS001");
			var expAddress = exporter.MainAddress;
			expAddress.OA_RL_NKRelatedPortCode = "US2CW";
			expAddress.OA_Address1 = "EXP ADDRESS 1";
			expAddress.OA_Address2 = "EXP ADDRESS 2";
			expAddress.OA_City = "SYDNEY";
			expAddress.OA_State = "NSW";
			expAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(expAddress, "JOEY", "YIN", "04 123456", "JOEYYIN@TEST.COM", "EXP FAX");
			invoiceLine.ForeignExporterOrgPK = exporter.PK;

			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.OR1;
			ams.US_USDAOrganicStandard = true;
			ams.US_EquivalentOrganicStandard = true;
			ams.US_IsElecImageSubmitted = true;
			ams.US_CerNumber = "CER123";
			ams.US_Date = new ZDateTime(2020, 07, 07);
			ams.US_NetWeight = 12.34;
			ams.US_NetWeightUQ = "KG";

			var cerBody = Factory.New<OrgHeader>();
			cerBody.OH_FullName = "TEST CERBODY";
			cerBody.OH_Code = "TESTCERB";
			cerBody.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AMS002");
			var cerBodyAddress = cerBody.MainAddress;
			cerBodyAddress.OA_RL_NKRelatedPortCode = "US2CW";
			cerBodyAddress.OA_Address1 = "CERB ADDRESS 1";
			cerBodyAddress.OA_Address2 = "CERB ADDRESS 2";
			cerBodyAddress.OA_City = "CHICAGO";
			cerBodyAddress.OA_State = "IL";
			cerBodyAddress.OA_PostCode = "2018";
			DeclarationTestHelper.AddPGAContact(cerBodyAddress, "ZHE", "YIN", "05 123456", "JOEYYIN@TEST.COM", "CERB FAX");
			ams.CertifyingBodyOrgPK = cerBody.PK;

			var recipient = Factory.New<OrgHeader>();
			recipient.OH_FullName = "TEST RECIPIENT";
			recipient.OH_Code = "TESTREC";
			recipient.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AMS003");
			var recAddress = recipient.MainAddress;
			recAddress.OA_RL_NKRelatedPortCode = "US2CW";
			recAddress.OA_Address1 = "REC ADDRESS 1";
			recAddress.OA_Address2 = "REC ADDRESS 2";
			recAddress.OA_City = "CHICAGO";
			recAddress.OA_State = "IL";
			recAddress.OA_PostCode = "2018";
			DeclarationTestHelper.AddPGAContact(recAddress, "ZHE", "YIN", "06 123456", "JOEYYIN@TEST.COM", "REC FAX");
			ams.RecipientOrgPK = recipient.PK;

			ams.US_IntendedUseCode = "INT456";
			ams.US_CommercialDescription = "COMMERCIAL DESCRIPTION";
			ams.US_Remarks = "REMARKSATTESTATIONS";

			var amsLine1 = ams.AMSLines.AddNew();
			amsLine1.US_LotEntity = LotNumberQualifierList.Codes._1;
			amsLine1.US_LotNumber = "LOT1";
			amsLine1.US_ProductLabel = "COMMERCIAL DESCRIPTION";

			var amsLine2 = ams.AMSLines.AddNew();
			amsLine2.US_LotEntity = LotNumberQualifierList.Codes._1;
			amsLine2.US_ProductLabel = "SECOND DESCRIPTION";

			var amsLine3 = ams.AMSLines.AddNew();
			amsLine3.US_LotNumber = "LOT1";
			amsLine3.US_ProductLabel = "THIRD DESCRIPTION";

			var amsLine4 = ams.AMSLines.AddNew();
			amsLine4.US_ProductLabel = "FOURTH DESCRIPTION";

			var fh1 = Factory.New<OrgHeader>();
			fh1.OH_FullName = "FINAL HANDLER 1";
			fh1.OH_Code = "TESTFH1";
			fh1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AMS004");
			var fh1Address = fh1.MainAddress;
			fh1Address.OA_RL_NKRelatedPortCode = "US2CW";
			fh1Address.OA_Address1 = "FH1 ADDRESS 1";
			fh1Address.OA_Address2 = "FH1 ADDRESS 2";
			fh1Address.OA_City = "CHICAGO";
			fh1Address.OA_State = "IL";
			fh1Address.OA_PostCode = "2019";
			DeclarationTestHelper.AddPGAContact(fh1Address, "ZHE", "YIN", "07 123456", "JOEYYIN@TEST.COM", "FH1 FAX");
			amsLine1.FinalHandlerOrgPK = fh1.PK;

			var cfh1 = Factory.New<OrgHeader>();
			cfh1.OH_FullName = "CERTIFYING FINAL HANDLER 1";
			cfh1.OH_Code = "TESTCFH1";
			cfh1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AMS005");
			var cfh1Address = cfh1.MainAddress;
			cfh1Address.OA_RL_NKRelatedPortCode = "US2CW";
			cfh1Address.OA_Address1 = "CFH1 ADDRESS 1";
			cfh1Address.OA_Address2 = "CFH1 ADDRESS 2";
			cfh1Address.OA_City = "CHICAGO";
			cfh1Address.OA_State = "IL";
			cfh1Address.OA_PostCode = "2020";
			DeclarationTestHelper.AddPGAContact(cfh1Address, "ZHE", "YIN", "08 123456", "JOEYYIN@TEST.COM", "CFH1 FAX");
			amsLine1.CerFinalHandlerOrgPK = cfh1.PK;

			var amsLine5 = ams.AMSLines.AddNew();
			amsLine5.US_LotEntity = LotNumberQualifierList.Codes._2;
			amsLine5.US_LotNumber = "LOT2";
			amsLine5.US_ProductLabel = "FIFTH DESCRIPTION";

			var fh2 = Factory.New<OrgHeader>();
			fh2.OH_FullName = "FINAL HANDLER 2";
			fh2.OH_Code = "TESTFH2";
			fh2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AMS006");
			var fh2Address = fh2.MainAddress;
			fh2Address.OA_RL_NKRelatedPortCode = "US2CW";
			fh2Address.OA_Address1 = "FH2 ADDRESS 1";
			fh2Address.OA_Address2 = "FH2 ADDRESS 2";
			fh2Address.OA_City = "CHICAGO";
			fh2Address.OA_State = "IL";
			fh2Address.OA_PostCode = "2021";
			DeclarationTestHelper.AddPGAContact(fh2Address, "ZHE", "YIN", "01 123456", "JOEYYIN@TEST.COM", "FH2 FAX");
			amsLine5.FinalHandlerOrgPK = fh2.PK;

			var cfh2 = Factory.New<OrgHeader>();
			cfh2.OH_FullName = "CERTIFYING FINAL HANDLER 2";
			cfh2.OH_Code = "TESTCFH2";
			cfh2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "AMS007");
			var cfh2Address = cfh2.MainAddress;
			cfh2Address.OA_RL_NKRelatedPortCode = "US2CW";
			cfh2Address.OA_Address1 = "CFH2 ADDRESS 1";
			cfh2Address.OA_Address2 = "CFH2 ADDRESS 2";
			cfh2Address.OA_City = "CHICAGO";
			cfh2Address.OA_State = "IL";
			cfh2Address.OA_PostCode = "2022";
			DeclarationTestHelper.AddPGAContact(cfh2Address, "ZHE", "YIN", "02 123456", "JOEYYIN@TEST.COM", "CFH2 FAX");
			amsLine5.CerFinalHandlerOrgPK = cfh2.PK;

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "TST";
			containerType.RC_Length = 40m;
			containerType.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_RC = containerType.PK;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";

			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSOR 1  Y                        INT456                                 
PG02P                                                                           
PG10                   COMMERCIAL DESCRIPTION                                   
PG10                   SECOND DESCRIPTION                                       
PG10                   THIRD DESCRIPTION                                        
PG10                   FOURTH DESCRIPTION                                       
PG10                   FIFTH DESCRIPTION                                        
PG141AM1CER123                           307072020                              
PG19EX 331AMS001         TEST EXPORTER                   EXP ADDRESS 1          
PG20EXP ADDRESS 2                        SYDNEY               NSWUS2017         
PG21   JOEY YIN               04123456       JOEYYIN@TEST.COM                   
PG19ORI331AMS002         TEST CERBODY                    CERB ADDRESS 1         
PG20CERB ADDRESS 2                       CHICAGO              IL US2018         
PG21   ZHE YIN                05123456       JOEYYIN@TEST.COM                   
PG19UC 331AMS003         TEST RECIPIENT                  REC ADDRESS 1          
PG20REC ADDRESS 2                        CHICAGO              IL US2018         
PG21   ZHE YIN                06123456       JOEYYIN@TEST.COM                   
PG19ORC331AMS005         CERTIFYING FINAL HANDLER 1      CFH1 ADDRESS 1         
PG20CFH1 ADDRESS 2                       CHICAGO              IL US2020         
PG21   ZHE YIN                08123456       JOEYYIN@TEST.COM                   
PG19ORP331AMS004         FINAL HANDLER 1                 FH1 ADDRESS 1          
PG20FH1 ADDRESS 2                        CHICAGO              IL US2019         
PG21   ZHE YIN                07123456       JOEYYIN@TEST.COM                   
PG19ORC331AMS007         CERTIFYING FINAL HANDLER 2      CFH2 ADDRESS 1         
PG20CFH2 ADDRESS 2                       CHICAGO              IL US2022         
PG21   ZHE YIN                02123456       JOEYYIN@TEST.COM                   
PG19ORP331AMS006         FINAL HANDLER 2                 FH2 ADDRESS 1          
PG20FH2 ADDRESS 2                        CHICAGO              IL US2021         
PG21   ZHE YIN                01123456       JOEYYIN@TEST.COM                   
PG22 16          CI AM4 Y                                                       
PG24AM1A10  REMARKSATTESTATIONS                                                 
PG24AM1A11                                                                      
PG25          1LOT1                                                             
PG25          1                                                                 
PG25           LOT1                                                             
PG25          2LOT2                                                             
PG27CONT1               140CONT2               2                                
PG29KG 000000001234                                                             ", message.EM_FormattedMessageText);
		}

		public void TestAMSNOPForDisclaimed()
		{
			SetUpData();
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NOPDisclaimReason = "A";
			invoiceLine.JI_Description = "NOP TEST";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        NOP TEST                                                              
PG01001AMSOR 1                                                                 A", message.EM_FormattedMessageText);
		}

		public void TestAMSOR2MessageSetLayout()
		{
			SetUpData();
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
			var ams1 = invoiceLine.AMSLines.AddNew();
			ams1.US_Program = AMSProgramList.Codes.OR2;
			ams1.US_IsElecImageSubmitted = true;
			ams1.US_NetWeight = 99.78m;
			ams1.US_NetWeightUQ = "KG";
			var amsLine1 = ams1.AMSLines.AddNew();
			amsLine1.US_CertType = LPCOTransactionTypeList.Codes.SingleUse;
			amsLine1.US_CertNumber = "123456789";
			var amsLine2 = ams1.AMSLines.AddNew();
			amsLine2.US_CertType = LPCOTransactionTypeList.Codes.Continuous;
			amsLine2.US_CertNumber = "987654321";
			var lotCode1 = ams1.LotCodes.AddNew();
			lotCode1.CY_Code = LotNumberQualifierList.Codes._1;
			lotCode1.CY_Data = "1122334455";
			var lotCode2 = ams1.LotCodes.AddNew();
			lotCode2.CY_Code = LotNumberQualifierList.Codes._3;
			lotCode2.CY_Data = "5544332211";
			var ams2 = invoiceLine.AMSLines.AddNew();
			ams2.US_Program = AMSProgramList.Codes.OR2;
			ams2.US_NetWeight = 13.55m;
			ams2.US_NetWeightUQ = "KG";
			var amsLine3 = ams2.AMSLines.AddNew();
			amsLine3.US_CertType = LPCOTransactionTypeList.Codes.Continuous;
			amsLine3.US_CertNumber = "111111111";
			var amsLine4 = ams2.AMSLines.AddNew();
			amsLine4.US_CertType = LPCOTransactionTypeList.Codes.SingleUse;
			amsLine4.US_CertNumber = "222222222";
			var lotCode3 = ams2.LotCodes.AddNew();
			lotCode3.CY_Code = LotNumberQualifierList.Codes._2;
			lotCode3.CY_Data = "66667777";
			var lotCode4 = ams2.LotCodes.AddNew();
			lotCode4.CY_Code = LotNumberQualifierList.Codes._4;
			lotCode4.CY_Data = "88889999";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG01001AMSOR 2  Y                                                               
PG02P                                                                           
PG141AM1123456789                                                               
PG142AM1987654321                                                               
PG25          11122334455                                                       
PG25          35544332211                                                       
PG29KG 000000009978                                                             
PG01002AMSOR 2                                                                  
PG02P                                                                           
PG142AM1111111111                                                               
PG141AM1222222222                                                               
PG25          266667777                                                         
PG25          488889999                                                         
PG29KG 000000001355                                                             ", message.EM_FormattedMessageText);
		}

		OrgHeader GetIOROrgHeader()
		{
			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREAT", "04 123456", "IOR EMAIL", "IOR FAX");

			return ior;
		}

		OrgHeader GetImporterOrgHeader()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "BUYER";
			var imAddress = importer.MainAddress;
			imAddress.OA_RL_NKRelatedPortCode = "US2CW";
			imAddress.OA_Address1 = "BUYER ADDRESS 1";
			imAddress.OA_Address2 = "BUYER ADDRESS 2";
			imAddress.OA_City = "MELBOURN";
			imAddress.OA_State = "MEL";
			imAddress.OA_PostCode = "2011";
			DeclarationTestHelper.AddPGAContact(importer, "BUYER", "CONTACT", "04 654321", "BUYER EMAIL", "BUYER FAX");
			return importer;
		}

		OrgHeader GetGoodLoactionOrgHeader()
		{
			var importer = Factory.New<OrgHeader>();
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;
			importer.OH_FullName = "GOODS";
			var imAddress = importer.MainAddress;
			imAddress.OA_RL_NKRelatedPortCode = "US2CW";
			imAddress.OA_Address1 = "GOODS ADDRESS 1";
			imAddress.OA_Address2 = "GOODS ADDRESS 2";
			imAddress.OA_City = "MELBOURN";
			imAddress.OA_State = "MEK";
			imAddress.OA_PostCode = "2013";
			DeclarationTestHelper.AddPGAContact(importer, "GOODS", "CONTACT", "04 654323", "GOODS EMAIL", "GOODS FAX");
			return importer;
		}

		GlbStaff GetBroker()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "KNZ";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";
			return broker;
		}
	}
}
