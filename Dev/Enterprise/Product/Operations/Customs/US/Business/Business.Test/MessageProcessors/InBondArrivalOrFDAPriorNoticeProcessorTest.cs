using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class InBondArrivalOrFDAPriorNoticeProcessorTest : ABIProcessorTest<InBondArrivalOrFDAPriorNoticeProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestInBondArrivalOrFDAPriorNoticeProcessorWithMixWT95AndEBBlcok()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			Factory.Save();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse,
				new string[]
			{
				"101568741294                                                                    ",
				"200701070835003901                                                              ",
				"9501124 INBOND NBR CONCLUDED                                                    ",
				"9501270 TRANSACTION DATA  REJECTED                                              ",
				"EYDETAIL REC COUNT NOT EQUAL 'Y' COUNT                                          ",
				"EBTRANSACTION DATA REJECTED                                                     "
			}
			);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			entry.CH_Status = "";
			entry.EntryNumber = "568741294";

			processor.Process();

			AssertEquals("Status for entry", ImportMessageStatusList.Codes.ErrorArrival, entry.CH_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("In-Bond Arrival"); }));

			AssertNotNull("An email with subject containing 'In-Bond Arrival' should have been created.", email);
			Assert(!email.Body.Contains("Error Narrative Message"));
		}

		public void TestInBondArrivalOrFDAPriorNoticeProcessorWithEBBlcok()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			Factory.Save();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse,
				new string[]
			{
				"EYDETAIL REC COUNT NOT EQUAL 'Y' COUNT                                          ",
				"EBTRANSACTION DATA REJECTED                                                     "
			}
			);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			entry.CH_Status = "";
			entry.EntryNumber = "568741294";

			processor.Process();

			AssertEquals("Status for entry", ImportMessageStatusList.Codes.ErrorArrival, entry.CH_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("In-Bond Arrival"); }));

			AssertNotNull("An email with subject containing 'In-Bond Arrival' should have been created.", email);
			Assert(email.Body.Contains("Error Narrative Message"));
			Assert(email.Body.Contains("DETAIL REC COUNT NOT EQUAL"));
			Assert(email.Body.Contains("TRANSACTION DATA REJECTED"));
		}

		public void TestProcessForBillForPriorNotice()
		{
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "870345978";

			outgoingMessage.EM_LinkedObject = bill;
			incomingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondFDATransmission;
			bill.CU_Status = ImportMessageStatusList.Codes.AwaitingFDATransmission;

			Factory.Save();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse,
				new string[]
			{
				"109            APLU870345978                                                    ",
				"OI        CUPS & FOOD CONTAINRS,OF P                                            ",
				"FD0100116WYY04   AUSLNDEVELOPER                ECPEFCIA113MAN ECPEFCIA113MAN    ",
				"FD020000000000                                                                  ",
				"FD030000002000            CUPS                                                  ",
				"FD04              AARON CROS61280102222                                         ",
				"9502271 DATA ADDED AS REQUESTED                                                 "
			}
			);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			processor.Process();

			AssertEquals("Message is linked up", bill, incomingMessage.EM_LinkedObject);
			AssertEquals("Status is updated", ImportMessageStatusList.Codes.ClearFDATransmission, bill.CU_Status);
			AssertEquals("Email with subject, 'FDA Prior Notice' sent", true, Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject.Contains("FDA Prior Notice"));
		}

		public void TestProcessingStandAlonePriorNotice()
		{
			outgoingMessage.EM_LinkedObject = declaration;
			incomingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.FDAPriorNotice;
			declaration.FDAMsgStatus = FDAStatusList.Codes.AWA;

			Factory.Save();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse,
				new string[]
			{
				"109                                                                             ",
				"OI        GOODS                                                                 ",
				"FD0100112AAB01   AUSLNTHE BUILDER              AUAUECOM2AUE   AUAUECOM2AUE      ",
				"FD020000100000KG                                                                ",
				"FD030000012500                                                                  ",
				"FD04              BOB SMITH 8015324561                                          ",
				"9502271 DATA ADDED AS REQUESTED                                                 ",
			}
			);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			processor.Process();

			AssertEquals("Message is linked up", declaration, incomingMessage.EM_LinkedObject);
			AssertEquals("Status is updated", FDAStatusList.Codes.ACR, declaration.FDAMsgStatus);
			AssertEquals("Email with subject, 'FDA Prior Notice' sent", "FDA Prior Notice Response for " + declaration.JE_DeclarationReference, Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		public void TestStandalonePNResponseCanDetermineJobNo()
		{
			declaration.FDAMsgStatus = FDAStatusList.Codes.AWA;
			declaration.JE_GS_NKCusAgent = staffZ2.GS_Code;
			declaration.ImportEntryNumber = "40022648";
			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "2";
			outgoingMessage.EM_MessageText = "B  8888XJ5WP                                               " + EDIMessage.MessageNumberPlaceHolder + "                109                                                                             OI        MANGOS                                                                FD0100121RFB15   AUSLNKNIGHT                   AUSHACUS184ALE AUSHACUS184ALE    FD020000010000KG                                                                FD030000001111                                                                  FD04              BRENDON PA8473645600                                          FD05ADA02082011                                                                 FD05APA1101                                                                     FD05ATA0930                                                                     FD05CO116 MAIN STREET                                                           FD05CO2UNIT 1                                                                   FD05COAUS                                                                       FD05COCABC EXPORTS USA                                                          FD05COEJOHN@TESTCOMPANY.COM.UK                                                  FD05COFKYRA                                                                     FD05COMKNIGHT                                                                   FD05CON91-013199000                                                             FD05COP8258425845                                                               FD05COUCHICAGO                                                                  FD05COVIL                                                                       FD05COX0000000000                                                               FD05COZ60611                                                                    FD05CSHAU                                                                       FD05EFCXJ5                                                                      FD05ENT70036930                                                                 FD05ETP01                                                                       FD05FIRC001                                                                     FD05FMEB                                                                        FD05HTS0804504040                                                               FD05IM116 MAIN STREET                                                           FD05IM2UNIT 1                                                                   FD05IMAUS                                                                       FD05IMCABC EXPORTS USA                                                          FD05IMEJOHN@TESTCOMPANY.COM.UK                                                  FD05IMFKYRA                                                                     FD05IMMKNIGHT                                                                   FD05IMN91-013199000                                                             FD05IMP8258425845                                                               FD05IMSIL                                                                       FD05IMUCHICAGO                                                                  FD05IMX0000000000                                                               FD05IMZ60611                                                                    FD05MOT11                                                                       FD05OFTI                                                                        FD05SA116 MAIN STREET                                                           FD05SA2UNIT 1                                                                   FD05SACCHICAGO                                                                  FD05SASIL                                                                       FD05SCAAPLU                                                                     FD05SCCUS                                                                       FD05SCNABC EXPORTS USA                                                          FD05SCZ60611                                                                    FD05SEMJOHN@TESTCOMPANY.COM.UK                                                  FD05SFNKYRA                                                                     FD05SFTF                                                                        FD05SFX0000000000                                                               FD05SPN8258425845                                                               FD05VFT18E                                                                      FD05BOLAPLUOB384857                                                             FD05NHBAPLUHB03858                                                              FD05TEMJOHN@SOMECOMPANY.ORG                                                     OI        MALE HORSES, PUREBRED BREE                                            FD0100283J--TL   AUSLNKNIGHT                   AUSHACUS184ALE AUSHACUS184ALE    FD020000001200KG                                                                FD030000001111                                                                  FD04              BRENDON PA8473645600                                          FD05ADA02082011                                                                 FD05APA1101                                                                     FD05ATA0930                                                                     FD05CO116 MAIN STREET                                                           FD05CO2UNIT 1                                                                   FD05COAUS                                                                       FD05COCABC EXPORTS USA                                                          FD05COEJOHN@TESTCOMPANY.COM.UK                                                  FD05COFKYRA                                                                     FD05COMKNIGHT                                                                   FD05CON91-013199000                                                             FD05COP8258425845                                                               FD05COUCHICAGO                                                                  FD05COVIL                                                                       FD05COX0000000000                                                               FD05COZ60611                                                                    FD05CSHAU                                                                       FD05EFCXJ5                                                                      FD05ENT70036930                                                                 FD05ETP01                                                                       FD05FIRC001                                                                     FD05FMEB                                                                        FD05HTS0101100010                                                               FD05IM116 MAIN STREET                                                           FD05IM2UNIT 1                                                                   FD05IMAUS                                                                       FD05IMCABC EXPORTS USA                                                          FD05IMEJOHN@TESTCOMPANY.COM.UK                                                  FD05IMFKYRA                                                                     FD05IMMKNIGHT                                                                   FD05IMN91-013199000                                                             FD05IMP8258425845                                                               FD05IMSIL                                                                       FD05IMUCHICAGO                                                                  FD05IMX0000000000                                                               FD05IMZ60611                                                                    FD05MOT11                                                                       FD05OFTI                                                                        FD05SA116 MAIN STREET                                                           FD05SA2UNIT 1                                                                   FD05SACCHICAGO                                                                  FD05SASIL                                                                       FD05SCAAPLU                                                                     FD05SCCUS                                                                       FD05SCNABC EXPORTS USA                                                          FD05SCZ60611                                                                    FD05SEMJOHN@TESTCOMPANY.COM.UK                                                  FD05SFNKYRA                                                                     FD05SFTF                                                                        FD05SFX0000000000                                                               FD05SPN8258425845                                                               FD05VFT18E                                                                      FD05BOLAPLUOB384857                                                             FD05NHBAPLUHB03858                                                              FD05TEMJOHN@SOMECOMPANY.ORG                                                     OI        FRESH MANGOES,ENT 9/1-5/31                                            FD0100321RFB15   AUSLNKNIGHT                   AUSHACUS184ALE AUSHACUS184ALE    FD020000010000KG                                                                FD030000001111                                                                  FD04              BRENDON PA8473645600                                          FD05ADA02082011                                                                 FD05APA1101                                                                     FD05ATA0930                                                                     FD05CO116 MAIN STREET                                                           FD05CO2UNIT 1                                                                   FD05COAUS                                                                       FD05COCABC EXPORTS USA                                                          FD05COEJOHN@TESTCOMPANY.COM.UK                                                  FD05COFKYRA                                                                     FD05COMKNIGHT                                                                   FD05CON91-013199000                                                             FD05COP8258425845                                                               FD05COUCHICAGO                                                                  FD05COVIL                                                                       FD05COX0000000000                                                               FD05COZ60611                                                                    FD05CSHAU                                                                       FD05EFCXJ5                                                                      FD05ENT70036930                                                                 FD05ETP01                                                                       FD05FIRC001                                                                     FD05FMEB                                                                        FD05HTS0804504040                                                               FD05IM116 MAIN STREET                                                           FD05IM2UNIT 1                                                                   FD05IMAUS                                                                       FD05IMCABC EXPORTS USA                                                          FD05IMEJOHN@TESTCOMPANY.COM.UK                                                  FD05IMFKYRA                                                                     FD05IMMKNIGHT                                                                   FD05IMN91-013199000                                                             FD05IMP8258425845                                                               FD05IMSIL                                                                       FD05IMUCHICAGO                                                                  FD05IMX0000000000                                                               FD05IMZ60611                                                                    FD05MOT11                                                                       FD05OFTI                                                                        FD05SA116 MAIN STREET                                                           FD05SA2UNIT 1                                                                   FD05SACCHICAGO                                                                  FD05SASIL                                                                       FD05SCAAPLU                                                                     FD05SCCUS                                                                       FD05SCNABC EXPORTS USA                                                          FD05SCZ60611                                                                    FD05SEMJOHN@TESTCOMPANY.COM.UK                                                  FD05SFNKYRA                                                                     FD05SFTF                                                                        FD05SFX0000000000                                                               FD05SPN8258425845                                                               FD05VFT18E                                                                      FD05BOLAPLUOB384857                                                             FD05NHBAPLUHB03858                                                              FD05TEMJOHN@SOMECOMPANY.ORG                                                     Y  8888XJ5WP";

			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse;
			incomingMessage.EM_MessageText = "B  8888XJ5WT                                               2                    109                                                                             OI        MANGOS                                                                FD0100121RFB15   AUSLNKNIGHT                   AUSHACUS184ALE AUSHACUS184ALE    FD020000010000KG                                                                FD030000001111                                                                  FD04              BRENDON PA8473645600                                          OI        MALE HORSES, PUREBRED BREE                                            FD0100283J--TL   AUSLNKNIGHT                   AUSHACUS184ALE AUSHACUS184ALE    FD020000001200KG                                                                FD030000001111                                                                  FD04              BRENDON PA8473645600                                          OI        FRESH MANGOES,ENT 9/1-5/31                                            FD0100321RFB15   AUSLNKNIGHT                   AUSHACUS184ALE AUSHACUS184ALE    FD020000010000KG                                                                FD030000001111                                                                  FD04              BRENDON PA8473645600                                          9502271 DATA ADDED AS REQUESTED                                                 Y  8888XJ5WT";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("1 email generated", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FDA Prior Notice Response for " + declaration.JE_DeclarationReference, email.Subject);
		}

		protected override void EndToEndCore()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			Factory.Save();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse,
				new string[]
			{
				"101568741294                                                                    ",
				"200701070835003901                                                              ",
				"9501124 INBOND NBR CONCLUDED                                                    ",
				"9501270 TRANSACTION DATA  REJECTED                                              "
			}
			);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			entry.CH_Status = "";
			entry.EntryNumber = "568741294";

			processor.Process();

			AssertEquals("Status for entry", ImportMessageStatusList.Codes.ErrorArrival, entry.CH_Status);
			AssertEquals("message is attached to entry", entry, incomingMessage.EM_LinkedObject);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("In-Bond Arrival"); }));
			AssertNotNull("An email with subject containing 'In-Bond Arrival' should have been created.", email);

			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestProcessForSuccess()
		{
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse,
				new string[]
			{
				"101568741294                                                                    ",
				"200701070635003901                                                              ",
				"9502271 DATA ADDED AS REQUESTED                                                 "
			}
			);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			entry.CH_Status = "";
			entry.EntryNumber = "568741294";

			processor.Process();

			AssertEquals("Status for entry", ImportMessageStatusList.Codes.ClearArrival, entry.CH_Status);
			AssertEquals("message is attached to entry", entry, incomingMessage.EM_LinkedObject);
			AssertNotNull("An email with subject containing 'In-Bond Arrival' should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("In-Bond Arrival"); })));
		}

		public void TestProcessAndSendToEmailGroup()
		{
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse,
				new string[]
			{
				"101568741294                                                                    ",
				"200701070635003901                                                              ",
				"9502271 DATA ADDED AS REQUESTED                                                 "
			}
			);

			incomingMessage.EM_MessageNum = outgoingMessage.EM_MessageNum + "BH";
			AssertEquals("PreCondition:Message does not have original message", null, incomingMessage.OriginalMessage);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			processor.Process();

			AssertNotNull("An email with subject containing 'In-Bond Arrival' should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("In-Bond Arrival"); })));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;

			entry = declaration.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entry;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondArrival;
			outgoingMessage.EM_SystemCreateUser = "~1";
			outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + " B";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "~1";
			staff.GS_LoginName = "~1";
			staff.GS_EmailAddress = "fakey@cargowise.com";

			Factory.Save();

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_MessageNum = outgoingMessage.EM_MessageNum;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse;
			processor = new InBondArrivalOrFDAPriorNoticeProcessor();
			processor.Message = incomingMessage;
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		InBondArrivalOrFDAPriorNoticeProcessor processor;
		MQEDIMessage incomingMessage;
		MQEDIMessage outgoingMessage;
	}
}
