using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForHFCTest : PGABlocksCreatorTest
	{
		public void TestHFCForDisclaimed()
		{
			SetUpData();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_HFCDisclaimReason = "B";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("PG01001EPAHFC                                                                  B", message.EM_FormattedMessageText);
		}

		public void TestHFC_OneHeaderWithOneLine_ASHRAENumberIsEmpty()
		{
			SetUpData();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader.US_CertifyingIndividual = EntityRoleCodeList.Codes.Importer;
			hfcHeader.US_HFCImageSent = true;
			hfcHeader.US_NetWeight = 648m;
			var hfcDetails = hfcHeader.USHFCDetails.AddNew();
			hfcDetails.US_LPCONumber = "Pro Num";
			hfcDetails.US_NameOfActiveIngredient = "Ingredient1";
			hfcDetails.US_ActiveIngredientPercentage = 66m;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"PG01001EPAHFC   Y                                                               
PG02P                                                                           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI                                                                          
PG19CN                   ULTIMATE CONSIGNEE                                     
PG21CN CNE                                                                      
PG22             CI EP4 Y                                                       
PG29KG 000000064800                                                             
PG02CCAS PRO NUM                                                                
PG04YINGREDIENT1                                                         0660000
", message.EM_FormattedMessageText);
		}

		public void TestHFC_OneHeader_ASHRAENumberIsNOTEmpty()
		{
			SetUpData();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader.US_ASHRAENumber = "R-001A";
			hfcHeader.US_CertifyingIndividual = EntityRoleCodeList.Codes.Consignee;
			hfcHeader.US_HFCImageSent = true;
			hfcHeader.US_NetWeight = 648m;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"PG01001EPAHFC   Y                                                               
PG02P                                                                           
PG07                                                        ASHR-001A           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG19CN                   ULTIMATE CONSIGNEE                                     
PG21CN CNE                                                                      
PG55CI                                                                          
PG22             CI EP4 Y                                                       
PG29KG 000000064800                                                             
", message.EM_FormattedMessageText);
		}

		public void TestHFCForMultipleHeadersWithMultipleLines()
		{
			SetUpData();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var hfcHeader1 = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader1.US_ASHRAENumber = "R-001A";
			hfcHeader1.US_CertifyingIndividual = EntityRoleCodeList.Codes.Importer;
			hfcHeader1.US_HFCImageSent = true;
			hfcHeader1.US_NetWeight = 648m;
			var hfcDetail1 = hfcHeader1.USHFCDetails.AddNew();
			hfcDetail1.US_LPCONumber = "Numb1";
			hfcDetail1.US_NameOfActiveIngredient = "Ingre1";
			hfcDetail1.US_ActiveIngredientPercentage = 10m;
			var hfcHeader2 = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader2.US_ASHRAENumber = "";
			hfcHeader2.US_CertifyingIndividual = EntityRoleCodeList.Codes.Consignee;
			hfcHeader2.US_HFCImageSent = false;
			hfcHeader2.US_NetWeight = 998m;
			var hfcDetail2 = hfcHeader2.USHFCDetails.AddNew();
			hfcDetail2.US_LPCONumber = "Numb2";
			hfcDetail2.US_ActiveIngredientPercentage = 20m;
			var hfcDetail3 = hfcHeader2.USHFCDetails.AddNew();
			hfcDetail3.US_LPCONumber = "Numb3";
			hfcDetail3.US_ActiveIngredientPercentage = 30m;
			var hfcDetail4 = hfcHeader2.USHFCDetails.AddNew();
			hfcDetail4.US_LPCONumber = "Numb4";
			hfcDetail4.US_NameOfActiveIngredient = "Ingre4";
			var hfcHeader3 = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader3.US_ASHRAENumber = "R-003A";
			hfcHeader3.US_CertifyingIndividual = EntityRoleCodeList.Codes.CustomsBroker;
			hfcHeader3.US_HFCImageSent = false;
			hfcHeader3.US_NetWeight = 118m;
			var hfcHeader4 = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader4.US_ASHRAENumber = "";
			hfcHeader4.US_CertifyingIndividual = EntityRoleCodeList.Codes.CustomsBroker;
			hfcHeader4.US_HFCImageSent = true;
			hfcHeader4.US_NetWeight = 328m;
			var hfcDetail5 = hfcHeader4.USHFCDetails.AddNew();
			hfcDetail5.US_LPCONumber = "Numb5";
			hfcDetail5.US_NameOfActiveIngredient = "Ingre5";
			hfcDetail5.US_ActiveIngredientPercentage = 50m;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"PG01001EPAHFC   Y                                                               
PG02P                                                                           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI                                                                          
PG19CN                   ULTIMATE CONSIGNEE                                     
PG21CN CNE                                                                      
PG22             CI EP4 Y                                                       
PG29KG 000000064800                                                             
PG02CCAS NUMB1                                                                  
PG04YINGRE1                                                              0100000
PG01002EPAHFC                                                                   
PG02P                                                                           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG19CN                   ULTIMATE CONSIGNEE                                     
PG21CN CNE                                                                      
PG55CI                                                                          
PG22             CI EP4 Y                                                       
PG29KG 000000099800                                                             
PG02CCAS NUMB2                                                                  
PG04Y                                                                    0200000
PG02CCAS NUMB3                                                                  
PG04Y                                                                    0300000
PG02CCAS NUMB4                                                                  
PG04YINGRE4                                                                     
PG01003EPAHFC                                                                   
PG02P                                                                           
PG07                                                        ASHR-003A           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG19CN                   ULTIMATE CONSIGNEE                                     
PG21CN CNE                                                                      
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG21CI                                                                          
PG22             CI EP4 Y                                                       
PG29KG 000000011800                                                             
PG01004EPAHFC   Y                                                               
PG02P                                                                           
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG19CN                   ULTIMATE CONSIGNEE                                     
PG21CN CNE                                                                      
PG19CI                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG21CI                                                                          
PG22             CI EP4 Y                                                       
PG29KG 000000032800                                                             
PG02CCAS NUMB5                                                                  
PG04YINGRE5                                                              0500000
", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			GlbStaff.CurrentUser.GS_Code = "HXU";
			GlbStaff.CurrentUser.GS_FullName = "HXU TEST";
			GlbStaff.CurrentUser.GS_WorkPhone = "04 010101";
			GlbStaff.CurrentUser.GS_EmailAddress = "HXU TEST EMAIL";

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "", "04 123456", "IOR EMAIL", null);

			var cne = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress = cne.MainAddress.PK;
			cne.OH_FullName = "ULTIMATE CONSIGNEE";
			var cneAddress = cne.MainAddress;
			cneAddress.OA_Address1 = "CNE ADDRESS 1";
			cneAddress.OA_Address2 = "CNE ADDRESS 2";
			cneAddress.OA_City = "CHARLESTON";
			cneAddress.OA_RL_NKRelatedPortCode = "US2CW";
			cneAddress.OA_State = "IL";
			cneAddress.OA_PostCode = "29492";
			DeclarationTestHelper.AddPGAContact(cne, "CNE", "", "05 654321", "CNE EMAIL", null);
		}
	}
}
