using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForATFTest : PGABlocksCreatorTest
	{
		public void TestATFExample()
		{
			SetUpData();
			declaration.US_EnableCRL = false;

			invoiceLine.JI_Description = "ATF TEST";
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;

			declaration.US_SchDEntry = "1101";
			declaration.US_EntryDate = new ZDateTime(2015, 12, 31);
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			var atfLine = invoiceLine.ATFLines.AddNew();

			atfLine.US_Quantity = 100m;
			atfLine.US_CategoryCode = "API";
			atfLine.US_ExtendedDescription = "DISCRIPTION";
			atfLine.US_FFLNumber = "1-23-456-78-9A-01234";
			atfLine.US_FELNumber = "1-23-456-78-9A-01234";
			atfLine.US_PermitNumber = "123456789";
			atfLine.US_AECANumber = "A-12-345-6789";
			atfLine.US_Model = "MODEL";
			atfLine.US_CaliberGaugeSize = "CALIBER";
			atfLine.US_BarrelLength = 1800m;
			atfLine.US_OverallLength = 1800m;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("ACE Entry Summary message including EPA data",
@"OI        ATF TEST                                                              
PG01001ATFATF    Y                                                              
PG02P                                                                           
PG50                                                                            
PG07MODEL                              CALIBER                                  
PG10AT1   API  PC9     DISCRIPTION                                              
PG19MF                   TEST ATF                                               
PG20                                                             US             
PG261000000010000                                                               
PG29                              IN 000000180000IN 000000180000                
PG51                                                                            
PG14 AT21-23-456-78-9A-01234                                                    
PG14 AT31-23-456-78-9A-01234                                                    
PG14 AT4123456789                                                               
PG14 AT5A-12-345-6789                                                           
PG30A                1101                                                       
PG32198CA                                                                       ", message.EM_FormattedMessageText);
		}

		public void TestAllATFLinesDeletedForPGACorrection()
		{
			SetUpData();
			invoiceLine.JI_Description = "DELETE ATF";
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;

			var atfLine1 = invoiceLine.ATFLines.AddNew();
			atfLine1.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			var atfLine2 = invoiceLine.ATFLines.AddNew();
			atfLine2.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var content = new PGACorrectionMessageBuilder(seEntry, ACEEntrySummaryMessageSendingOption.New(), false).GetSerialiseMessageContents();
			AssertContains(
@"---------------AABIInputB---------------
 Application Identifier Code (11-12)    :CA
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------APDCCA10----------------
 Action Code (5-5)      :R
 Entry Filer Code (6-8) :XJ5

----------------APDCCA40----------------
 Line Item Identifier (5-9) :1

----------------APDCCA60----------------

-----------------AENSOI-----------------
 Commercial Description Text (11-80) :DELETE ATF

----------------AEPAPG01----------------
 P G A Line Number (5-7)                :0
 Government Agency Code (8-10)          :ATF
 Government Agency Program Code (11-13) :COR
 Correction Indicator (79-79)           :D

---------------AABIInputY---------------
 Application Identifier Code (11-12) :CA", content);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "TOYOTA (JAPAN)";
			manufacturer.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "1234 PEACHTREE STREET";
			manufacturerAddress.OA_City = "ATLANTA";
			manufacturerAddress.OA_State = "GA";
			manufacturerAddress.OA_RL_NKRelatedPortCode = "USLAX";
			manufacturerAddress.OA_PostCode = "30301";
			manufacturerAddress.OA_CompanyNameOverride = "TEST ATF";
			DeclarationTestHelper.AddPGAContact(manufacturerAddress, "JANE", "SMITH", "7062345678", "J.SMITH@TOYOTAAMERICA.COM", null);
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
		}
		OrgHeader manufacturer;
	}
}
