using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForOMCTest : PGABlocksCreatorTest
	{
		public void TestOMCForSample1()
		{
			SetUpData();
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			var omcHeader = invoiceLine.OMCHeaders.AddNew();
			omcHeader.US_NetWeight = 100m;
			omcHeader.US_DepartureDate = ZDateTime.BrettsBirthday;
			omcHeader.US_ElectronicImageSubmitted = true;
			omcHeader.US_DeclarationCode = "7A1";
			omcHeader.US_OA_Exporter = address1.PK;
			omcHeader.US_OA_ResponsibleGovernmentOfficial = address2.PK;
			omcHeader.US_NetWeightUQ = "KG";
			var aquaculture = omcHeader.AquacultureFacilities.AddNew();
			aquaculture.US_OA_AquacultureFacility = address3.PK;
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"OI                                                                              
PG01001OMCOMC   Y                                                               
PG02P                                                                           
PG06HRV                      09181971                                           
PG19AQF                  TEST OMC3                       ADDRESS3               
PG20                                     NJ3                  J3 US10111        
PG19EX                   TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG21EX                                                                          
PG22 924    7A1  EX     Y                                                       
PG19RGO                  TEST OMC                        ADDRESS2               
PG20                                     NJ                   JS US10110        
PG21RGO                                                                         
PG29KG 000000010000                                                             ", message.EM_FormattedMessageText);
		}

		public void TestOMCForSample2()
		{
			SetUpData();
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			var omcHeader = invoiceLine.OMCHeaders.AddNew();
			omcHeader.US_NetWeight = 100m;
			omcHeader.US_DepartureDate = ZDateTime.BrettsBirthday;
			omcHeader.US_ElectronicImageSubmitted = true;
			omcHeader.US_DeclarationCode = "7A2";
			omcHeader.US_OA_Exporter = address1.PK;
			omcHeader.US_OA_ResponsibleGovernmentOfficial = address2.PK;
			omcHeader.US_NetWeightUQ = "KG";
			var aquaculture1 = omcHeader.AquacultureFacilities.AddNew();
			aquaculture1.US_OA_AquacultureFacility = address3.PK;
			var aquaculture2 = omcHeader.AquacultureFacilities.AddNew();
			aquaculture2.US_OA_AquacultureFacility = address4.PK;
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
					@"OI                                                                              
PG01001OMCOMC   Y                                                               
PG02P                                                                           
PG06HRV                      09181971                                           
PG19EX                   TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              GA US30301        
PG21EX                                                                          
PG22 924    7A2  EX     Y                                                       
PG19RGO                  TEST OMC                        ADDRESS2               
PG20                                     NJ                   JS US10110        
PG21RGO                                                                         
PG29KG 000000010000                                                             ", message.EM_FormattedMessageText);
		}

		public void TestPRCountryForOMC()
		{
			SetUpData();
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;

			var address1 = exporter.Addresses.AddNew();
			address1.OA_Address1 = "1234 PEACHTREE STREET";
			address1.OA_City = "ATLANTA";
			address1.OA_State = "PR";
			address1.OA_RL_NKRelatedPortCode = "PRLAX";
			address1.OA_PostCode = "30301";
			address1.OA_CompanyNameOverride = "TEST ATF";

			var address2 = exporter.Addresses.AddNew();
			address2.OA_Address1 = "Address2";
			address2.OA_City = "NJ";
			address2.OA_State = "PR";
			address2.OA_RL_NKRelatedPortCode = "PRLAX";
			address2.OA_PostCode = "10110";
			address2.OA_CompanyNameOverride = "TEST OMC";

			var omcHeader = invoiceLine.OMCHeaders.AddNew();
			omcHeader.US_NetWeight = 100m;
			omcHeader.US_DepartureDate = ZDateTime.BrettsBirthday;
			omcHeader.US_ElectronicImageSubmitted = true;
			omcHeader.US_DeclarationCode = "7A1";
			omcHeader.US_OA_Exporter = address1.PK;
			omcHeader.US_OA_ResponsibleGovernmentOfficial = address2.PK;
			omcHeader.US_NetWeightUQ = "KG";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(@"PG19EX                   TEST ATF                        1234 PEACHTREE STREET  
PG20                                     ATLANTA              PR US30301        
PG21EX                                                                          
PG22 924    7A1  EX     Y                                                       
PG19RGO                  TEST OMC                        ADDRESS2               
PG20                                     NJ                   PR US10110        ", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "TOYOTA (JAPAN)";
			exporter.MainAddress.OA_Address1 = "MAIN ADDRESS";
			address1 = exporter.Addresses.AddNew();
			address1.OA_Address1 = "1234 PEACHTREE STREET";
			address1.OA_City = "ATLANTA";
			address1.OA_State = "GA";
			address1.OA_RL_NKRelatedPortCode = "USLAX";
			address1.OA_PostCode = "30301";
			address1.OA_CompanyNameOverride = "TEST ATF";

			address2 = exporter.Addresses.AddNew();
			address2.OA_Address1 = "Address2";
			address2.OA_City = "NJ";
			address2.OA_State = "JS";
			address2.OA_RL_NKRelatedPortCode = "USLAX";
			address2.OA_PostCode = "10110";
			address2.OA_CompanyNameOverride = "TEST OMC";

			address3 = exporter.Addresses.AddNew();
			address3.OA_Address1 = "Address3";
			address3.OA_City = "NJ3";
			address3.OA_State = "J3";
			address3.OA_RL_NKRelatedPortCode = "USLAX";
			address3.OA_PostCode = "10111";
			address3.OA_CompanyNameOverride = "TEST OMC3";

			address4 = exporter.Addresses.AddNew();
			address4.OA_Address1 = "Address4";
			address4.OA_City = "NJ4";
			address4.OA_State = "J4";
			address4.OA_RL_NKRelatedPortCode = "USLAX";
			address4.OA_PostCode = "10114";
			address4.OA_CompanyNameOverride = "TEST OMC4";
		}
		OrgHeader exporter;
		OrgAddress address1;
		OrgAddress address2;
		OrgAddress address3;
		OrgAddress address4;
	}
}
