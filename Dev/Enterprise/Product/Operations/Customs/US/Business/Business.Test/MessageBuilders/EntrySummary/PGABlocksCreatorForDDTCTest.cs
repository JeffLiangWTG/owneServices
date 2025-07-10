using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForDDTCTest : PGABlocksCreatorTest
	{
		[TestDate(2014, 11, 01)]
		public void TestDDTCData()
		{
			SetUpData();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001DTCDTC                                                                   
PG02P                                                                           
PG14 S73SG12323                                                        123.18A2 
PG14 DD1RE3234                                                                  
PG30A112320151730                                                               ", message.EM_FormattedMessageText);
		}

		public void TestDDTCDeletedForPGACorrection()
		{
			SetUpData();
			invoiceLine.JI_Description = "DELETE DDTC";
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCTrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;

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
 Commercial Description Text (11-80) :DELETE DDTC

----------------AEPAPG01----------------
 P G A Line Number (5-7)                :0
 Government Agency Code (8-10)          :DTC
 Government Agency Program Code (11-13) :COR
 Correction Indicator (79-79)           :D

---------------AABIInputY---------------
 Application Identifier Code (11-12) :CA", content);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";

			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");
			invoiceLine.JI_OA_ExporterAddress = ior.MainAddress.PK;
			invoiceLine.JI_Description = "ABCDEFG DESC";
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCLicenseType = DDTCLicenseTypeCodes.Codes.S73;
			invoiceLine.US_DDTCLicenseNo = "SG12323";
			invoiceLine.US_DDTCRegistrationNo = "RE3234";
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			invoiceLine.US_DDTCArrivalDate = new ZDateTime(2015, 11, 23, 17, 30, 32);
		}
	}
}
