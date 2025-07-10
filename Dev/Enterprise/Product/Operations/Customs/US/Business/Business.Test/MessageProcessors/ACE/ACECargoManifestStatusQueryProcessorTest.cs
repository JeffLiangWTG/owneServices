using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACECargoManifestStatusQueryProcessorTest : ABIProcessorTest<ACECargoManifestStatusQueryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestUpdateDeclarationFromQueryResultOfMaster()
		{
			SetUpData();

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0160 20160205", "0160", new ZDateTime(2016, 02, 05),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051  123456789012             0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WR4            013115051                           0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0160 20160206", "0161", new ZDateTime(2015, 02, 05),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051  123456789012             0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160206    0161 020515   ",
"WR4            013115051                           0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WR4            013115051  123456789012             0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0160 20160207", "0162", new ZDateTime(2016, 02, 15),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR1                                       NCA NCA0160 20160207    0162 021516   ",
"WR4            013115051                           0000154800KG              1  ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051  123456789012             0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0160 20160208", "0163", new ZDateTime(2016, 02, 06),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051  123456789012             0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160208    0163 020616   ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0160 20160209", "0164", new ZDateTime(2016, 02, 07),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051  123456789012             0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160209    0164 020716   ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WR4            013115051  123456789012             0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0160 20160200", "0165", new ZDateTime(2016, 02, 08),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR1                                       NCA NCA0160 20160200    0165 020816   ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051  123456789012             0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore(ZString.Empty, ZString.Empty, ZDateTime.Empty,
"WO101101SV9  71007882 0158-123456789                                            ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051  123456789012             0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160210    0166 020916   ",
"WR4            013115051  123456789012             0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("EW VESSEL          0", "05T02", new ZDateTime(2015, 02, 04),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WR4            013115051                           0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0160 20160205", "0160", new ZDateTime(2016, 02, 05),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WR4            013115051                           0000154800KG              1  ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0161 20160205", "0169", new ZDateTime(2016, 02, 15),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR1                                       NCA NCA0161 20160205    0169 021516   ",
"WR4            013115051                           0000154800KG              1  ",
"WR4            013115051  123456789012             0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0162 20160205", "0168", new ZDateTime(2016, 02, 25),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR1                                       NCA NCA0162 20160205    0168 022516   ",
"WR4            013115051                           0000154800KG              1  ",
"WR4            013115051  123456789012             0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WR4            013115051  123456789012             0000154800KG              1  ");

			UpdateDeclarationFromQueryResultOfMasterCore("NCA0161 20160205", "0169", new ZDateTime(2016, 02, 15),
"WO101101SV9  71007882 0158-123456789                                            ",
"WR1                                       NCA NCA0161 20160205    0169 021516   ",
"WR4            013115051  123456789012             0000154800KG              1  ",
"WR4            013115051                           0000154800KG              1  ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WR4            013115051  123456789012             0000154800KG              1  ");
		}

		void UpdateDeclarationFromQueryResultOfMasterCore(string vesselName, string voyageFlightNo, ZDateTime dateOfArrival, params string[] messageBlocks)
		{
			declaration.JE_VesselName = ZString.Empty;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "013115051";
			entry.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_161053";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_161053     WR1    SV9 71007882                                                    Y        Y  1101SV9CQ";

			message.EM_MessageNum = "HYEDUSCMT_161053";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, messageBlocks);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("JE_VesselName updated from response message", vesselName, declaration.JE_VesselName);
			AssertEquals("JE_VoyageFlightNo updated from response message", voyageFlightNo, declaration.JE_VoyageFlightNo);
			AssertEquals("JE_DateOfArrival updated from response message", dateOfArrival, declaration.JE_DateOfArrival);
		}

		public void TestReleaseDateInDeclarationUpdatedCorrectlyFromReleaseStatusQuery()
		{
			SetUpData();
			entry.Messages.Add(outgoing);

			declaration.ImportEntryNumber = "03435705";
			declaration.JE_MasterBill = "MAEU208474977";
			declaration.JE_HouseBill = "SV9FVN20016297";

			var cargoReleaseEntry = declaration.CustomsEntryHeaders.AddNew();
			cargoReleaseEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			cargoReleaseEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			var firstSOMessage = Factory.New<MQEDIMessage>();
			firstSOMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			firstSOMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			firstSOMessage.EM_MessageText = "B004601SV9SO                                                                    SO104601SV9  03435705 0137-181685900MAEUMSC ATHENS          111W 041221         SO20CR SV0152722                                                                SO40MMAEU208474977                                                              SO40HSV9FVN20016297                                        00000005PKG  00000005SO50041421082894BILL DEPARTED                                                   SO40MMAEU208474977                                                              SO40HSV9FSV90152722                                        00000051PKG  00000051SO50041421082894BILL DEPARTED                                                   SO60041421082898RELEASED                                04142101                Y  4601SV9SO00058";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var declarationBO = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(new ZDateTime(2021, 04, 14), declarationBO.JE_EntryAuthorisationDate);
			AssertEquals(CRLReleaseStatusList.Codes.REL, declarationBO.ReleaseStatus);
			AssertEquals(1, declarationBO.DispositionCodes.Count);

			var secondSOMessage = Factory.New<MQEDIMessage>();
			secondSOMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			secondSOMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			secondSOMessage.EM_MessageText = "B004601SV9SO                                                                    SO104601SV9  03435705 0137-181685900MAEUMSC ATHENS          111W 041221         SO20CR SV0152722                                                                SO40MMAEU208474977                                                              SO40HSV9FVN20016297                                        00000005PKG  00000005SO50042321092295BILL ARRIVED                                                    SO40MMAEU208474977                                                              SO40HSV9FSV90152722                                        00000051PKG  00000051SO50042321092295BILL ARRIVED                                                    SO60042321092298RELEASED                                04212101                SO60042321092201ONE USG                                                         Y  4601SV9SO00010";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			newFactory = new BusinessObjectFactory();
			declarationBO = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(new ZDateTime(2021, 04, 21), declarationBO.JE_EntryAuthorisationDate);
			AssertEquals(CRLReleaseStatusList.Codes.REL, declarationBO.ReleaseStatus);
			AssertEquals(3, declarationBO.DispositionCodes.Count);

			var loadedOutgoing = newFactory.Load<MQEDIMessage>(outgoing.PK);
			loadedOutgoing.EM_MessageNum = "DFDEWRPRD_7606538";
			loadedOutgoing.EM_MessageText = "B  4601SV9CQ                                               DFDEWRPRD_7606538    WR1                            MAEU208474977                          Y 2       Y  4601SV9CQ";

			var loadedIncoming = newFactory.Load<MQEDIMessage>(message.PK);
			loadedIncoming.EM_MessageNum = "DFDEWRPRD_7606538";
			loadedIncoming.EM_Status = EDIMessage.Status.Queued;
			loadedIncoming.EM_MessageText = "B004601SV9C1                                               DFDEWRPRD_7684504    WO104601SV9  03435705 0137-181685900MAEUMSC ATHENS          111W 041221         WO20CR SV0152722                                                                WO40MMAEU208474977                                                              WO40HSV9FVN20016297                                        00000005PKG  00000005WO50042321092295BILL ARRIVED                                                    WO40MMAEU208474977                                                              WO40HSV9FSV90152722                                        00000051PKG  00000051WO50042321092295BILL ARRIVED                                                    WO60042321092298RELEASED                                04212101                WO60042321092201ONE USG                                                         WO104601SV9  03435705 0137-181685900MAEUMSC ATHENS          111W 041221         WO20CR SV0152722                                                                WO40MMAEU208474977                                                              WO40HSV9FVN20016297                                        00000005PKG  00000005WO50041421082894BILL DEPARTED                                                   WO40MMAEU208474977                                                              WO40HSV9FSV90152722                                        00000051PKG  00000051WO50041421082894BILL DEPARTED                                                   WO60041421082898RELEASED                                04142101                Y  4601SV9C100174";

			newFactory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			newFactory = new BusinessObjectFactory();
			declarationBO = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(new ZDateTime(2021, 04, 21), declarationBO.JE_EntryAuthorisationDate);
			AssertEquals(CRLReleaseStatusList.Codes.REL, declarationBO.ReleaseStatus);
			AssertEquals(3, declarationBO.DispositionCodes.Count);
		}

		public void TestUpdatingEntryDispositions()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159674";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159674     WR1    SV9 70038383                                                    Y2       Y  3901SV9CQ00001";

			message.EM_MessageNum = "HYEDUSCMT_159674";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "HYEDUSCMT_159674";
			message.EM_MessageText =
				"B001101SV9C1                                               HYEDUSCMT_159674     " +
				"WO101101SV97003838301             APLUAA TEST1001T      Y                       " +
				"WO20RSNUnable to verify CBP disposition for full bill quantity on original entry" +
				"WO50010815104491NO BILL MATCH                                                   " +
				"WO60121514095717 ELECTRONIC INVOICE REQUIRED                     20             " +
				"WO101101SV97003838301             APLUAA TEST1001T      Y                       " +
				"WO20RSNUnable to verify CBP disposition for full bill quantity on original entry" +
				"WO50010815104491NO BILL MATCH                                                   " +
				"WO60052716101098RELEASED                                05271601                " +
				"WO60121514075622 RELEASE DATE SET                       12161403                " +
				"Y  3901SV9SO00000";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			entry.Reload();
			var declaration = entry.Declaration;
			AssertEquals(3, declaration.DispositionCodes.Count);

			Assert(declaration.DispositionCodes.Cast<DispositionData>().Any(x => x.US_Code == "17"));
			Assert(declaration.DispositionCodes.Cast<DispositionData>().Any(x => x.US_Code == "98"));
			Assert(declaration.DispositionCodes.Cast<DispositionData>().Any(x => x.US_Code == "22"));
		}

		public void TestShouldUpdateDispositionData()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159674";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159674     WR1    SV9 70038383                                                    Y2       Y  3901SV9CQ00001";

			message.EM_MessageNum = "HYEDUSCMT_159674";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "HYEDUSCMT_159674";
			message.EM_MessageText =
				"B001101SV9C1                                               HYEDUSCMT_159674     " +
				"WO101101SV97003838301             APLUAA TEST1001T      Y                       " +
				"WO20RSNUnable to verify CBP disposition for full bill quantity on original entry" +
				"WO50010815104491NO BILL MATCH                                                   " +
				"WO60121514095717 ELECTRONIC INVOICE REQUIRED                     20             " +
				"WO60121514075622 RELEASE DATE SET                       12161403                " +
				"WO70ODS000121514095901PGA DETAILS ACCEPTED        070401123                     " +
				"Y  3901SV9SO00000";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			entry.Reload();
			var declaration = entry.Declaration;
			AssertEquals(2, declaration.DispositionCodes.Count);
			AssertEquals(1, declaration.OGADispositionCodes.Count);
		}

		public void TestShouldNotUpdateDispositionData()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 70038383                                                    Y        Y  3901SV9CQ00001";

			message.EM_MessageNum = "HYEDUSCMT_159674";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "HYEDUSCMT_159674";
			message.EM_MessageText =
				"B001101SV9C1                                               HYEDUSCMT_159674     " +
				"WO101101SV97003838301             APLUAA TEST1001T      Y                       " +
				"WO20RSNUnable to verify CBP disposition for full bill quantity on original entry" +
				"WO50010815104491NO BILL MATCH                                                   " +
				"WO60121514095717 ELECTRONIC INVOICE REQUIRED                     20             " +
				"WO60121514075622 RELEASE DATE SET                       12161403                " +
				"WO70ODS000121514095901PGA DETAILS ACCEPTED        070401123                     " +
				"Y  3901SV9SO00000";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			entry.Reload();
			var declaration = entry.Declaration;
			AssertEquals("should not update entry dispositions.", 0, declaration.DispositionCodes.Count);
			AssertEquals("should not update PGA dispositions", 0, declaration.OGADispositionCodes.Count);
		}

		public void TestEntryNotFound()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			var image2 = new System.Drawing.Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			SetUpData();
			declaration.JE_GB = newBranch.PK;
			AssertEquals(entry.Declaration.Branch.PK, newBranch.PK);

			entry.EntryNumber = "70038383";
			entry.Messages.Add(outgoing);
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 70038383                                                    Y        Y  3901SV9CQ00001";
			outgoing.EM_GB = newBranch.PK;
			var processor = new ACECargoManifestStatusQueryProcessor();

			message.EM_MessageNum = "HYEDUSCMT_159673";
			message.EM_MessageText = "B001101SV9C1                                               HYEDUSCMT_159673     WR0SV9 70038383              950ENTRY RECORD NOT FOUND                          Y  1101SV9C100001                                                               ";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			DeclarationTestHelper.SetupForSendMessage();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			entry.Reload();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("<th>Error Description for SV970038383</th></tr></thead><tr><td>ENTRY RECORD NOT FOUND</td>"));

			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals(message.EM_LinkUniqueID, entry.PK);

			var dec = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(outgoing.Branch.PK, dec.Branch.PK);

			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		public void TestFailedProcessing()
		{
			SetUpData();
			entry.EntryNumber = "70038383";
			entry.Messages.Add(outgoing);
			outgoing.EM_MessageNum = "MAKORDORD_497676";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 70038383                                                    Y        Y  3901SV9CQ00001";
			var processor = new ACECargoManifestStatusQueryProcessor();

			message.EM_MessageNum = "MAKORDORD_497676";
			message.EM_MessageText = "B001001267C1                                               MAKORDORD_497676     WR1                                       CKK CKK0239 20150606    0239 060615   WSCCKK0239 06061511223687532     9                            11223687532ER     WSCCKK0239 06061511223687532     9      SHE00118378     9                       Y  1001267C100003";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
		}

		public void TestInBondQuery()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var itNumber = houseBill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "V12345678";

			var itNumber2 = houseBill.ITAndSplitDetails.AddNew();
			itNumber2.US_ITNumber = "00120000094";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159854";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159854     WR1                00120000094                                                  Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159854";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4V12345678   13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WS503121414121314                                                               ",
"WN1            27042704                27046200012121462000 TEST CON   121214   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WS401121514121114                                                               ",
"WR4987456145   13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WN1            27042704                27046200012131462000 TEST CON   121314   ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);
			AssertEquals("Declaration dispositions", 0, declaration.DispositionCodes.Count);
			AssertEquals("Bill dispositions", 0, houseBill.DispositionCodes.Count);

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("<td>Actual Port Of Unlading Ocean Vessel Diversion</td><td>&nbsp;</td></tr><tr><td>In-Bond Originating Port</td><td>&nbsp;</td></tr><tr><td>Manifested In-Bond Destination Port</td><td>&nbsp;</td></tr><tr><td>Actual In-Bond Destination Manual Diversion</td><td>&nbsp;</td></tr><tr><td>Actual In-Bond Destination Via Edi In-Bond Diversion</td><td>2704</td></tr><tr><td>Vessel Departure Port</td><td>62000</td>"));
		}

		public void TestInBondStatus()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var itNumber = houseBill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "V12345678";

			var itNumber2 = houseBill.ITAndSplitDetails.AddNew();
			itNumber2.US_ITNumber = "00120000094";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159854";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159854     WR1                00120000094                                                  Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159854";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4V12345678   13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WS503121414121314                                                               ",
"WN1            27042704                27046200012121462000 TEST CON   121214   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WS401121514121114                                                               ",
"WR4987456145   13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WN1            27042704                27046200012131462000 TEST CON   121314   ",
"WSCKK0QF13706061511223687532     9                             1122368753ER     ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("<th>In-Bond Status</th><th>In-Bond Arrival Date</th><th>In-Bond Export Date</th><th>In-Bond Entry Type</th></tr></thead><tr><td>On File</td><td>15-Dec-14</td><td>11-Dec-14</td><td>&nbsp;</td></tr>"));

			var r1BlockTitleCount = Regex.Matches(email.Body, Regex.Escape("<b>ACE Cargo/Manifest/Entry Status Query Results</b>")).Count;
			AssertEquals("R1 block title count", 2, r1BlockTitleCount);
		}

		public void TestACEInBondStatus()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var itNumber = houseBill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "V12345678";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159854";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159854     WR1                V12345678                                                    Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159854";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4V12345678   13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WS5AR121414121314                                                               ",
"WN1            27042704                27046200012121462000 TEST CON   121214   ",
"WSCKK0QF13706061511223687532     9                             1122368753ER     ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("<th>In-Bond Status</th><th>In-Bond Arrival Date</th><th>In-Bond Export Date</th><th>In-Bond Entry Type</th></tr></thead><tr><td>Arrived</td><td>14-Dec-14</td><td>13-Dec-14</td><td>&nbsp;</td></tr>"));

			var r1BlockTitleCount = Regex.Matches(email.Body, Regex.Escape("<b>ACE Cargo/Manifest/Entry Status Query Results</b>")).Count;
			AssertEquals("R1 block title count", 1, r1BlockTitleCount);
		}

		public void TestInBondEntryTypeAndN0()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var itNumber = houseBill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "V12345678";

			var itNumber2 = houseBill.ITAndSplitDetails.AddNew();
			itNumber2.US_ITNumber = "00120000094";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159854";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159854     WR1                00120000094                                                  Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159854";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4V12345678   13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WS503121414121314                                                          70   ",
"WN04800211198560001                                                             ",
"WN1            27042704                27046200012121462000 TEST CON   121214   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WS401121514121114                                                          63   ",
"WR4987456145   13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WN1            27042704                27046200012131462000 TEST CON   121314   ",
"WSCKK0QF13706061511223687532     9                             1122368753ER61   ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("<th>In-Bond Entry Type</th>"));
			Assert(email.Body.Contains("63 - Immediate Export"));
			Assert(!email.Body.Contains("61 - Immediate Transport"));

			Assert(email.Body.Contains("<b>Bill Of Lading Quantities</b>"));
			Assert(email.Body.Contains("Master Bill Amended Quantity"));
			Assert(email.Body.Contains("48002111"));
			Assert(email.Body.Contains("House Bill Amended Quantity"));
			Assert(email.Body.Contains("98560001"));
		}

		public void TestACECargoManifestQueryStatusListWithAir()
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill2.CU_BillNum = "HOUSEBILL02";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159817";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159817     WR1                            MOLU13700107330                        YY2       Y  1101SV9CQ";
			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoManifestAirQuery;
			message.EM_MessageNum = "HYEDUSCMT_159817";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR1                                       MOLUHYUNDAI INTEGRAL    050E 042316   ",
"WR4            13700107330                         00000666CTN  MOLU    MY1     ",
"WR5121214075155 CARRIER BILL AMENDMENT - ADD                     666      001   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR3001HK8438909090                                                              ",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WR5121214075673 AMS FLIGHT NOT DEPARTED/ARRIVED                  00000049       ",
"WR5121214075969 BILL ON FILE                                     1920     001   ",
"WR512121407593Z BOL MATCHED TO ISF                               1920     002   ",
"WR5121214075872 CBPA INSPECTION/DOC REVIEW HOLD                  1920     003   ",
"WR5121214075375 CBPA DOC REVIEW HOLD REMOVED                     1920     004   ",
"WR5121214075219 CONVEYANCE ARRIVAL                                        005   ",
"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      006   ",
"WR51212140756A1 FDA PN ADVISORY                                  1920     007   ",
"WR512121407581F ENTER AND RELEASED GENERAL EXAM                  1920     008   ",
"WR512121407564E ENTRY CANCEL / DELETE                            1920     009   ",
"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      010   ",
"WN1            27042704                27046200012101462000 TEST CON   121014   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WR5121214075611 DOC REVIEW REQUIRED                              00000009       ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUHN1     ",
"WR5121214075603 PENDING EXAM                                     00000009       ",
"WSCNCA0188 07051693370841050  91                                                ",
"WSD12121407512055GENERATED IN RESPONSE TO AN AMENDMENT                          ",
"WSCNCA0188 07051693370841050  91          HOUSEBILL    91                       ",
"WSD1212140751201FGENERATED IN RESPONSE TO AN AMENDMEN2                          ",
"WSD1212140751201KGENERATED IN RESPONSE TO AN AMENDMEN3                          ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);

			AssertEquals("Declaration dispositions", 0, declaration.DispositionCodes.Count);
			AssertEquals("Bill dispositions Count", 11, houseBill.DispositionCodes.Count);
			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];

			Assert(email.Body.Contains("<tr><td>93370841050</td><td>1</td><td>55</td><td>GENERATED IN RESPONSE TO AN AMENDMENT</td><td>12-Dec-14 07:51</td></tr>"));
			Assert(email.Body.Contains("<tr><td>HOUSEBILL</td><td>2</td><td>1F</td><td>GENERATED IN RESPONSE TO AN AMENDMEN2</td><td>12-Dec-14 07:51</td></tr>"));
			Assert(email.Body.Contains("<tr><td>HOUSEBILL</td><td>3</td><td>1K</td><td>GENERATED IN RESPONSE TO AN AMENDMEN3</td><td>12-Dec-14 07:51</td></tr>"));
			AssertContains("ACE Cargo/Manifest/Entry Status Query Response on Air", email.Body);
		}

		public void TestWillNotMissR4AndR5BillDetailsWhenIsMessageOverDisplayLimitIsFalse()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "FLXT";
			masterBill.CU_BillNum = "TS2TI1899800";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "FLXT";
			houseBill.CU_BillNum = "00001921760B";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "MO4MGLSYR_2651770";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               MO4MGLSYR_2651770    WR1                            TS2TI1899800                           YY2       Y  1101SV9CQ";
			message.EM_MessageNum = "MO4MGLSYR_2651770";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"B000906DE6C1                                               MO4MGLSYR_2651770    ",
"WR1                                       ONEYHYUNDAI SPEED       034E 032123   ",
"WR4            TS2TI1899800                        00001719CTN  ONEY    MY1     ",
"WR502102311153Z BOL MATCHED TO ISF                               0        001   ",
"WR502102311323Z BOL MATCHED TO ISF                               0        002   ",
"WR5021623053469 BILL ON FILE                                     1719     003   ",
"WR503162312071C ENTER AND RELEASED GENERAL EXAM                  1719     004   ",
"WN1            4601                        58023021923                          ",
"WR1                                       FLXTHYUNDAI SPEED       034E 031823   ",
"WR4            TS2TI189980000001921760A            00000915CTN  ONEYFLXTHY1     ",
"WR5020823051169 BILL ON FILE                                     915      001   ",
"WR502102311153Z BOL MATCHED TO ISF                               915      002   ",
"WR502162305341Y MVOC-NVOCC BILL OF LADING MATCH                  915      003   ",
"Y  0906DE6C100017     ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("<th>In-Bond Number</th><th>Master Bill Number</th><th>House Bill Number</th><th>Sub-House Bill Number</th><th>Manifest Qty</th><th>UQ</th><th>Master Bill Issuer Code</th><th>House Bill Issuer Code</th><th>Bill of Lading Type</th><th>ISF Indicator</th><th>MOT</th>"));
			Assert(email.Body.Contains("<tr><td>&nbsp;</td><td>TS2TI1899800</td><td>00001921760A</td><td>&nbsp;</td><td>915</td><td>CTN</td><td>ONEY</td><td>FLXT</td><td>House Bill of Lading</td><td>ISF On File</td><td>Ocean</td></tr>"));
			Assert(email.Body.Contains("<th>Sequence</th><th>Disposition Code</th><th>Disposition Desc</th><th>Disposition Date/Time</th><th>Quantity</th></tr></thead><tr><td>1</td><td>69</td><td>BILL ON FILE</td><td>08-Feb-23 05:11</td><td>915</td></tr><tr><td>2</td><td>3Z</td><td>BOL MATCHED TO ISF</td><td>10-Feb-23 11:15</td><td>915</td></tr><tr><td>3</td><td>1Y</td><td>MVOC-NVOCC BILL OF LADING MATCH</td><td>16-Feb-23 05:34</td><td>915</td>"));
		}

		public void TestCreateAttachmentWhenMessageOverDisplayLimit()
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill2.CU_BillNum = "HOUSEBILL02";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159817";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159817     WR1                            MOLU13700107330                        YY2       Y  1101SV9CQ";
			outgoing.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoManifestAirQuery;
			message.EM_MessageNum = "HYEDUSCMT_159817";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR1                                       MOLUHYUNDAI INTEGRAL    050E 042316   ",
"WR4            13700107330                         00000666CTN  MOLU    MY1     ",
"WR5121214075155 CARRIER BILL AMENDMENT - ADD                     666      001   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR3001HK8438909090                                                              ",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WR5121214075673 AMS FLIGHT NOT DEPARTED/ARRIVED                  00000049       ",
"WR5121214075969 BILL ON FILE                                     1920     001   ",
"WR512121407593Z BOL MATCHED TO ISF                               1920     002   ",
"WR5121214075872 CBPA INSPECTION/DOC REVIEW HOLD                  1920     003   ",
"WR5121214075375 CBPA DOC REVIEW HOLD REMOVED                     1920     004   ",
"WR5121214075219 CONVEYANCE ARRIVAL                                        005   ",
"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      006   ",
"WR51212140756A1 FDA PN ADVISORY                                  1920     007   ",
"WR512121407581F ENTER AND RELEASED GENERAL EXAM                  1920     008   ",
"WR512121407564E ENTRY CANCEL / DELETE                            1920     009   ",
"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      010   ",
"WN1            27042704                27046200012101462000 TEST CON   121014   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WR5121214075611 DOC REVIEW REQUIRED                              00000009       ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUHN1     ",
"WR5121214075603 PENDING EXAM                                     00000009       ",
"WR1                                       NYKSHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510                          00000666CTN  NYKS    MY1     ",
"WR5032916045155 CARRIER BILL AMENDMENT - ADD                     666      001   ",
"WN1            1001                        57035032816                          ",
"WR1                                       CHQFHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  CSHUA6030942            00000048CTN  NYKSCHQFHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  48       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067CA            00000072CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  72       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067CB            00000138CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  138      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067CD            00000074CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  74       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067D             00000149CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  149      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067E             00000003PKG  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  3        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067G             00000002PKG  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  2        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067J             00000001CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  1        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067L             00000022CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  22       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067M             00000037CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  37       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067N             00000003PKG  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  3        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067R             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  104      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067C             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  105      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067E             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  106      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067F             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  107      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067H             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  108      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067I             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  109      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067K             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  110      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067P             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  110      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067Q             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  112      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067S             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  113      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067T             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  114      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067U             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  115      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067V             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  116      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067W             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  117      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067X             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  118      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067Y             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  119      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067Z             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  120      001   ",
"WSCNCA0188 07051693370841050  91                                                ",
"WSD12121407512055GENERATED IN RESPONSE TO AN AMENDMENT                          ",
"WSCNCA0188 07051693370841050  91          HOUSEBILL    91                       ",
"WSD1212140751201FGENERATED IN RESPONSE TO AN AMENDMEN2                          ",
"WSD1212140751201KGENERATED IN RESPONSE TO AN AMENDMEN3                          ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);

			AssertEquals("Declaration dispositions", 0, declaration.DispositionCodes.Count);
			AssertEquals("Bill dispositions Count", 11, houseBill.DispositionCodes.Count);
			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];

			Assert(email.Body.Contains(ACECargoManifestStatusQueryProcessor.EmailBodyWhenUseAttachment));
			AssertContains("ACE Cargo/Manifest/Entry Status Query Response on Air", email.Body);
			AssertEquals(3, email.Attachments.Count);
			var attachment = email.Attachments.ToList<AttachmentDef>().First(x => x.DisplayName == ACECargoManifestStatusQueryProcessor.MessageDetailsAttachmentFileName);
			var attachmentBody = Encoding.ASCII.GetString(attachment.Data);
			Assert(attachmentBody.Contains("<tr><td>HOUSEBILL</td><td>2</td><td>1F</td><td>GENERATED IN RESPONSE TO AN AMENDMEN2</td><td>12-Dec-14 07:51</td></tr>"));
			Assert(attachmentBody.Contains("<tr><td>HOUSEBILL</td><td>3</td><td>1K</td><td>GENERATED IN RESPONSE TO AN AMENDMEN3</td><td>12-Dec-14 07:51</td></tr>"));
		}

		public void TestACECargoManifestQueryMessages()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";
			declaration.JE_DeclarationReference = "53638383";
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
			@"B  3901WFBCQ                                               HYEDUSCMT_159673     " +
			 "WR1                                            29759922004TVL040150             " +
			 "Y  3901WFBCQ";

			var processor = new ACECargoManifestStatusQueryProcessor();

			message.EM_MessageNum = "HYEDUSCMT_159673";
			message.EM_MessageText =
			@"B003901WFBC1                                               HYEDUSCMT_159673     " +
			 "WR1                                       CAL CAL5148 20160619    5148 061916   " +
			 "WSCCAL5148 06191629759922004    90        TVL040150    90                       " +
			 "WSD0620161014201CENTER AND RELEASED GENERAL EXAM                                " +
			 "Y  3901WFBC100003";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
				"WR1                                       CAL CAL5148 20160619    5148 061916   ",
				"WSCCAL5148 06191629759922004    90        TVL040150    90                       ",
				"WSD0620161014201CENTER AND RELEASED GENERAL EXAM                                ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("Response on Entry 70038383 in job 53638383"));
		}
		public void TestACECargoManifestQueryStatusList()
		{
			SetUpData();

			var fumigationHoldForCBPAPlacedAtPortOfDischarge = "73";
			var importerSecurityFilingonFile = "3Z";
			var inspectionOrDocumentReviewHoldforCBPAPlacedAtPortOfDischarge = "72";
			var billOnFile = "69";
			var enteredAndReleasedGeneralExamination = "1C";

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "73", "73 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "3Z DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "72", "72 DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "69", "69 DESC", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1C", "1C DESC", startDate, endDate);

			var attribute11 = helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName5.ZXE_Name, "Y");
			var attribute31 = helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, attributeName5.ZXE_Name, "Y");
			var attribute51 = helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, attributeName3.ZXE_Name, "Y");
			Factory.Save();

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill2.CU_BillNum = "HOUSEBILL02";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159817";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159817     WR1                            MOLU13700107330                        YY2       Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159817";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR1                                       MOLUHYUNDAI INTEGRAL    050E 042316   ",
"WR4            13700107330                         00000666CTN  MOLU    MY1     ",
"WR5121214075155 CARRIER BILL AMENDMENT - ADD                     666      001   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR3001HK8438909090                                                              ",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WR5121214075673 AMS FLIGHT NOT DEPARTED/ARRIVED                  00000049       ",
"WR5121214075969 BILL ON FILE                                     1920     001   ",
"WR512121407593Z BOL MATCHED TO ISF                               1920     002   ",
"WR5121214075872 CBPA INSPECTION/DOC REVIEW HOLD                  1920     003   ",
"WR5121214075375 CBPA DOC REVIEW HOLD REMOVED                     1920     004   ",
"WR5121214075219 CONVEYANCE ARRIVAL                                        005   ",
"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      006   ",
"WR51212140756A1 FDA PN ADVISORY                                  1920     007   ",
"WR512121407581F ENTER AND RELEASED GENERAL EXAM                  1920     008   ",
"WR512121407564E ENTRY CANCEL / DELETE                            1920     009   ",
"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      010   ",
"WN1            27042704                27046200012101462000 TEST CON   121014   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WR5121214075611 DOC REVIEW REQUIRED                              00000009       ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUHN1     ",
"WR5121214075603 PENDING EXAM                                     00000009       ",
"WSD1213150754201CENTER AND RELEASED GENERAL EXAM                                ");

			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);

			var list = DispositionCodeListLoader.GetDispositionCodes(houseBill.Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode);

			AssertEquals("Declaration dispositions", 0, declaration.DispositionCodes.Count);
			AssertEquals("Bill dispositions Count", 12, houseBill.DispositionCodes.Count);
			AssertEquals("Bill Code0", houseBill.DispositionCodes[0].US_Code, fumigationHoldForCBPAPlacedAtPortOfDischarge);
			AssertStartsWith("Bill Desc0", houseBill.DispositionCodes[0].DispositionCodeDesc, list.GetDescriptionFromCode(fumigationHoldForCBPAPlacedAtPortOfDischarge));

			AssertEquals("Bill Code2", houseBill.DispositionCodes[2].US_Code, importerSecurityFilingonFile);
			AssertStartsWith("Bill Desc2", houseBill.DispositionCodes[2].DispositionCodeDesc, list.GetDescriptionFromCode(importerSecurityFilingonFile));

			AssertEquals("Bill Code3", houseBill.DispositionCodes[3].US_Code, inspectionOrDocumentReviewHoldforCBPAPlacedAtPortOfDischarge);
			AssertStartsWith("Bill Desc3", houseBill.DispositionCodes[3].DispositionCodeDesc, list.GetDescriptionFromCode(inspectionOrDocumentReviewHoldforCBPAPlacedAtPortOfDischarge));

			AssertEquals("Bill Code1", houseBill.DispositionCodes[1].US_Code, billOnFile);
			AssertStartsWith("Bill Desc1", houseBill.DispositionCodes[1].DispositionCodeDesc, list.GetDescriptionFromCode(billOnFile));

			AssertEquals("Bill Code11", houseBill.DispositionCodes[11].US_Code, enteredAndReleasedGeneralExamination);
			AssertStartsWith("Bill Desc11", houseBill.DispositionCodes[11].DispositionCodeDesc, list.GetDescriptionFromCode(enteredAndReleasedGeneralExamination));

			masterBill.DispositionCodes.Reload(true);
			AssertEquals("Bill dispositions", 1, masterBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "55", masterBill.DispositionCodes[0].US_Code);
		}

		public void TestInBondStatusMessageOverDispLimit()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var itNumber = houseBill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "V12345678";

			var itNumber2 = houseBill.ITAndSplitDetails.AddNew();
			itNumber2.US_ITNumber = "00120000094";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159854";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159854     WR1                00120000094                                                  Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159854";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4V12345678   13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WS503121414121314                                                               ",
"WN1            27042704                27046200012121462000 TEST CON   121214   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WS401121514121114                                                               ",
"WR4987456145   13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WN1            27042704                27046200012131462000 TEST CON   121314   ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ",
"WSCKK0QF13706061511223687532     9                              1122368753ER    ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(!email.Body.Contains("<td>In-Bond Status</td>"));
			AssertEquals(3, email.Attachments.Count);
		}

		public void TestOceanOrTruckMasterBillQuery()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill2.CU_BillNum = "HOUSEBILL02";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159817";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159817     WR1                            MOLU13700107330                        YY2       Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159817";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR3001HK8438909090                                                              ",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WR5121214075673 AMS FLIGHT NOT DEPARTED/ARRIVED                  00000049       ",
"WN1            27042704                27046200012101462000 TEST CON   121014   ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL02             00000034KG   MOLUAPLUHN1     ",
"WR5121214075611 DOC REVIEW REQUIRED                              00000009       ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUHN1     ",
"WR5121214075603 PENDING EXAM                                     00000009       ");

			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);

			AssertEquals("Declaration dispositions", 0, declaration.DispositionCodes.Count);
			AssertEquals("Bill dispositions", 2, houseBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", 1, houseBill2.DispositionCodes.Count);
		}

		public void TestOceanOrTruckHouseBillQuery()
		{
			OceanOrTruckHouseBillQueryCore(
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUHN1     ",
"WR5121214075603 PENDING EXAM                                     00000009       ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR3001HK8438909090                                                              ",
"WR4            13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WR4            13700107330 HOUSEBILL02             00000128BO   MOLUMOLUMN1     ",
"WR4            MASTERBL2                           00000780PL   XXXD            ",
"WN1            27042704                27046200012101462000 TEST CON   121014   ");
		}

		public void TestOceanOrTruckHouseBillQueryVersion1()
		{
			OceanOrTruckHouseBillQueryCore(
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR4            13700107330 HOUSEBILL01             0000000034KG   MOLUMOLUHN11  ",
"WR5121214075603 PENDING EXAM                                     00000009       ",
"WR12704            01                     APLUAA TEST1            001T 121414   ",
"WR2                                                                         A001",
"WR3001HK8438909090                                                              ",
"WR4            13700107330 HOUSEBILL01             0000000034KG   MOLUMOLUMN11  ",
"WR4            13700107330 HOUSEBILL02             0000000128BO   MOLUMOLUMN11  ",
"WR4            MASTERBL2                           0000000780PL   XXXD       1  ",
"WN1            27042704                27046200012101462000 TEST CON   121014   ");
		}

		void OceanOrTruckHouseBillQueryCore(params string[] messageBlocks)
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill2.CU_BillNum = "HOUSEBILL02";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.US_UI_NKBillIssuerSCAC = "XXXD";
			masterBill2.CU_BillNum = "MASTERBL2";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159817";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159817     WR1                            MOLU13700107330                        YY2       Y  1101SV9CQ";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			message.EM_MessageNum = "HYEDUSCMT_159817";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WSA            APLUHOUSEBILL02 122BILL NBR NOT ON FILE                          ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);
			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("<th>Error Description for Master Bill Number: APLUHOUSEBILL02</th></tr></thead><tr><td>BILL NBR NOT ON FILE</td>"));

			processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, messageBlocks);
			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);

			AssertEquals("Declaration dispositions", 0, declaration.DispositionCodes.Count);
			AssertEquals("Bill dispositions", 1, houseBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", 0, houseBill2.DispositionCodes.Count);

			AssertEquals("Vessel updated from response message", "001T", declaration.JE_VoyageFlightNo);
			AssertEquals("JE_DateOfArrival updated from response message", new ZDateTime(2014, 12, 14), declaration.JE_DateOfArrival);
			AssertEquals("Total number of packages updated from response message", 908, declaration.JE_TotalNoOfPacks);
			AssertEquals("Carrier SCAC should not be updated from response message", "MOLU", declaration.US_UI_NKCarrierSCAC);

			AssertEquals("Bill should not be updated from message because we have a duplicate blocks for house bill", 0m, houseBill.CU_NoOfPacks);
			AssertEquals("Bill should not be updated from message", "", houseBill.CU_PackType);

			AssertEquals("Bill should be updated from message", 128m, houseBill2.CU_NoOfPacks);
			AssertEquals("Bill should be updated from message", "BO", houseBill2.CU_PackType);

			AssertEquals("Bill should be updated from message", 780m, masterBill2.CU_NoOfPacks);
			AssertEquals("Bill should be updated from message", "PL", masterBill2.CU_PackType);

			declaration.JE_VoyageFlightNo = ZString.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			declaration.JE_TotalNoOfPacks = 0;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			houseBill2.CU_NoOfPacks = ZDecimal.Zero;
			houseBill2.CU_PackType = ZString.Empty;
			masterBill2.CU_NoOfPacks = ZDecimal.Zero;
			masterBill2.CU_PackType = ZString.Empty;

			processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;
			var messageBlocksList = messageBlocks.ToList();
			messageBlocksList.RemoveAt(7);
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, messageBlocksList.ToArray());
			processor.Process();

			AssertEquals("Vessel updated from response message", "001T", declaration.JE_VoyageFlightNo);
			AssertEquals("JE_DateOfArrival updated from response message", new ZDateTime(2014, 12, 14), declaration.JE_DateOfArrival);
			AssertEquals("Total number of packages updated from response message", 942, declaration.JE_TotalNoOfPacks);
			AssertEquals("Carrier SCAC should not be updated from response message", "", declaration.US_UI_NKCarrierSCAC);

			AssertEquals("Bill should be updated from message", 34m, houseBill.CU_NoOfPacks);
			AssertEquals("Bill should be updated from message", "KG", houseBill.CU_PackType);

			AssertEquals("Bill should be updated from message", 128m, houseBill2.CU_NoOfPacks);
			AssertEquals("Bill should be updated from message", "BO", houseBill2.CU_PackType);

			AssertEquals("Bill should be updated from message", 780m, masterBill2.CU_NoOfPacks);
			AssertEquals("Bill should be updated from message", "PL", masterBill2.CU_PackType);
		}

		public void TestUpdateEntryWithResultsForEntryQuery()
		{
			UpdateEntryWithResultsForEntryQueryCore(
"WO101101SV9  71007882 0158-123456789                                            ",
"WO20CR B00162786                                                                ",
"WO40 APLUMST010815                                         00000100CS   00000000",
"WO50013115051093   BILL MATCH                                                   ",
"WO60013115051025ENTRY WILL BE CANCELLED IN 7 DAYS                               ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051                           00154800KG                   ");
		}

		public void TestUpdateEntryWithResultsForEntryQueryVersion1()
		{
			UpdateEntryWithResultsForEntryQueryCore(
"WO101101SV9  71007882 0158-123456789                                            ",
"WO20CR B00162786                                                                ",
"WO40 APLUMST010815                                         00000100CS   00000000",
"WO50013115051093   BILL MATCH                                                   ",
"WO60013115051025ENTRY WILL BE CANCELLED IN 7 DAYS                               ",
"WR13901SJ50000001501                     CCCSNEW VESSEL          005T02020415   ",
"WR4            013115051                           0000154800KG              1  ");
		}

		void UpdateEntryWithResultsForEntryQueryCore(params string[] messageBlocks)
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "013115051";
			entry.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_161053";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_161053     WR1    SV9 71007882                                                    Y        Y  1101SV9CQ";

			message.EM_MessageNum = "HYEDUSCMT_161053";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, messageBlocks);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			AssertEquals("message has a linked object", entry, message.EM_LinkedObject);
			AssertEquals("Vessel updated from response message", "05T02", declaration.JE_VoyageFlightNo);
			AssertEquals("JE_DateOfArrival updated from response message", new ZDateTime(2015, 02, 04), declaration.JE_DateOfArrival);
			AssertEquals("Total number of packages updated from response message", 154800, declaration.JE_TotalNoOfPacks);
			AssertEquals("Carrier SCAC should not be updated from response message", "MOLU", declaration.US_UI_NKCarrierSCAC);

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("ENTRY WILL BE CANCELLED IN 7 DAYS"));
		}

		public void TestWR4WSCWithManifestQuantity()
		{
			WR4WSCWithManifestQuantityCore(
"WR1                                       NCA NCA0188 20160204    0188 020416   ",
"WSCNCA0188 02041693381645874   342     HKCLT2138135A  342  256                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WSCNCA0188 02041693381645874A  342  256HKCLT2138135A  342  256                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WSCNCA0160 02051693381645874B  342   86HKCLT2138135B  342   86                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WSCNCA0160 02051693381645874   342     HKCLT2138135B  342   86                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       NCA NCA0188 20160204    0188 020416   ",
"WSCNCA0188 02041693381645874   342     HKCLT2138135A  342  256                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WSCNCA0188 02041693381645874A  342  256HKCLT2138135A  342  256                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WSCNCA0160 02051693381645874B  342   86HKCLT2138135B  342   86                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WSCNCA0160 02051693381645874   342     HKCLT2138135B  342   86                  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ");
		}

		public void TestWR4WSCWithManifestQuantityVersion1()
		{
			WR4WSCWithManifestQuantityCore(
"WR1                                       NCA NCA0188 20160204    0188 020416   ",
"WSCNCA0188 02041693381645874   342     HKCLT2138135A  342  256               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                             1  ",
"WSCNCA0188 02041693381645874A  342  256HKCLT2138135A  342  256               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                             1  ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WSCNCA0160 02051693381645874B  342   86HKCLT2138135B  342   86               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WSCNCA0160 02051693381645874   342     HKCLT2138135B  342   86               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                             1  ",
"WR1                                       NCA NCA0188 20160204    0188 020416   ",
"WSCNCA0188 02041693381645874   342     HKCLT2138135A  342  256               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                             1  ",
"WSCNCA0188 02041693381645874A  342  256HKCLT2138135A  342  256               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       NCA NCA0160 20160205    0160 020516   ",
"WSCNCA0160 02051693381645874B  342   86HKCLT2138135B  342   86               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
"WSCNCA0160 02051693381645874   342     HKCLT2138135B  342   86               1  ",
"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ");
		}

		void WR4WSCWithManifestQuantityCore(params string[] messageBlocks)
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "YNCA";
			masterBill.CU_BillNum = "2600652510";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "NCA";
			houseBill.CU_BillNum = "HKCLT2138135";
			houseBill.CU_PackType = "CT";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, messageBlocks);
			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("We only need to sum up the first instance of each House or Master.", 342, declaration.JE_TotalNoOfPacks);
				AssertEquals("Housebill has manifest quantity", (ZDecimal)342, houseBill.CU_NoOfPacks);
			});
		}

		public void TestWR4WSCWithManifestQuantity2()
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "YNCA";
			masterBill.CU_BillNum = "2600652510";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.US_UI_NKBillIssuerSCAC = "AZA";
			houseBill1.CU_BillNum = "BGY15054414";
			houseBill1.CU_PackType = "CT";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "AZA";
			houseBill2.CU_BillNum = "MIL15055134";
			houseBill2.CU_PackType = "CT";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
				"WR1                                       AZA AZA0614 20151230    0614 123015   ",
				"WSD0209161659221CENTER AND RELEASED GENERAL EXAM                                ",
				"WR1 AZA AZA0614 20151230    0614 123015                                         ",
				"WSCAZA0614 12301505517632786A  101   88                                         ",
				"WSCAZA0614 12301505517632786A  101   88 BGY15054414A    6    3                  ",
				"WSCAZA0614 12301505517632786A  101   88 BIE15054980A   83   73                  ",
				"WSCAZA0614 12301505517632786A  101   88 MIL15055134    12                       ",
				"WR1 AZA AZA0614 20160101    0614 010116                                         ",
				"WSCAZA0614 01011605517632786B  101    7                                         ",
				"WSCAZA0614 01011605517632786B  101    7 BGY15054414B    6    1                  ",
				"WSCAZA0614 01011605517632786B  101    7 BIE15054980B   83    6                  ",
				"WR1 AZA AZA0614 20160102    0614 010216                                         ",
				"WSCAZA0614 01021605517632786C  101    6                                         ",
				"WSCAZA0614 01021605517632786C  101    6 BGY15054414C    6    2                  ",
				"WSCAZA0614 01021605517632786C  101    6 BIE15054980C   83    4                  ");
			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("We only need to sum up the first instance of each House or Master.", 18, declaration.JE_TotalNoOfPacks);
				AssertEquals("Housebill has manifest quantity", (ZDecimal)6, houseBill1.CU_NoOfPacks);
				AssertEquals("Housebill has manifest quantity", (ZDecimal)12, houseBill2.CU_NoOfPacks);
			});
		}

		public void TestWR4WSCWithManifestQuantity3()
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "YNCA";
			masterBill.CU_BillNum = "2600652510";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.US_UI_NKBillIssuerSCAC = "AZA";
			houseBill1.CU_BillNum = "BGY15054414";
			houseBill1.CU_PackType = "CT";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "AZA";
			houseBill2.CU_BillNum = "MIL15055134";
			houseBill2.CU_PackType = "CT";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,

"WR1                                       AZA AZA0614 20151230    0614 123015   ",
"WSCAZA0614 12301505517632786A  101   88                                         ",
"WSCAZA0614 12301505517632786A  101   88 BGY15054414A    6    3                  ",
"WSD0104160810441CENTER AND RELEASED GENERAL EXAM                                ",
"WSCAZA0614 12301505517632786A  101   88 BIE15054980A   83   73                  ",
"WSD0104161144421CENTER AND RELEASED GENERAL EXAM                                ",
"WSCAZA0614 12301505517632786A  101   88 MIL15055134    12                       ",
"WSD0104160920191CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       AZA AZA0614 20160101    0614 010116   ",
"WSCAZA0614 01011605517632786B  101    7                                         ",
"WSCAZA0614 01011605517632786B  101    7 BGY15054414B    6    1                  ",
"WSD0104160810441CENTER AND RELEASED GENERAL EXAM                                ",
"WSCAZA0614 01011605517632786B  101    7 BIE15054980B   83    6                  ",
"WSD0104161144421CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       AZA AZA0614 20160102    0614 010216   ",
"WSCAZA0614 01021605517632786C  101    6                                         ",
"WSCAZA0614 01021605517632786C  101    6 BGY15054414C    6    2                  ",
"WSD0104160810441CENTER AND RELEASED GENERAL EXAM                                ",
"WSCAZA0614 01021605517632786C  101    6 BIE15054980C   83    4                  ",
"WSD0104161144421CENTER AND RELEASED GENERAL EXAM                                ");

			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("We only need to sum up the first instance of each House or Master.", 18, declaration.JE_TotalNoOfPacks);
				AssertEquals("Housebill has manifest quantity", (ZDecimal)6, houseBill1.CU_NoOfPacks);
				AssertEquals("Housebill has manifest quantity", (ZDecimal)12, houseBill2.CU_NoOfPacks);
			});
		}

		public void TestWR4WSCWithManifestQuantity4()
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "BAW";
			masterBill.CU_BillNum = "12526283132";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR1                                       BAW BAW0195 20160128    0195 012816   ",
"WSCBAW0195 01281612526283132     2                                              ",
"WSD0201161246391CENTER AND RELEASED GENERAL EXAM                                ",
"WSD0201161301341CENTER AND RELEASED GENERAL EXAM                                ");
			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("Simple AWB has manifest quantity", (ZDecimal)2, masterBill.CU_NoOfPacks);
			});
		}

		public void TestWR4WSCWithManifestQuantity5()
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "YNCA";
			masterBill.CU_BillNum = "2600652510";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.US_UI_NKBillIssuerSCAC = "BAW";
			houseBill1.CU_BillNum = "MIL16004090";
			houseBill1.CU_PackType = "CT";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR1                                       BAW BAW0173 20160204    0173 020416   ",
"WR2                                                                         E858",
"WSCBAW0173 02041612526283014A   85   74                                         ",
"WSD0204161159291FCBP LOCAL TRANSFER AUTHORIZED                                  ",
"WSCBAW0173 02041612526283014A   85   74 MIL16004075    38                       ",
"WSD0205161224511CENTER AND RELEASED GENERAL EXAM                                ",
"WSCBAW0173 02041612526283014A   85   74 MIL16004079    36                       ",
"WSD0205161222501CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       BAW BAW0181 20160204    0181 020416   ",
"WR2                                                                         E858",
"WSCBAW0181 02041612526283014B   85   11                                         ",
"WSD0204161650561FCBP LOCAL TRANSFER AUTHORIZED                                  ",
"WSCBAW0181 02041612526283014B   85   11 BGY16003823     9                       ",
"WSD0205161038371CENTER AND RELEASED GENERAL EXAM                                ",
"WSCBAW0181 02041612526283014B   85   11 MIL16004090     1                       ",
"WSD0205161334171CENTER AND RELEASED GENERAL EXAM                                ",
"WSCBAW0181 02041612526283014B   85   11 MIL16004113     1                       ",
"WSD0205161121381CENTER AND RELEASED GENERAL EXAM                                ");
			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("We only need to sum up the first instance of each House or Master.", 1, declaration.JE_TotalNoOfPacks);
				AssertEquals("HAWB has manifest quantity", (ZDecimal)1, houseBill1.CU_NoOfPacks);
			});
		}

		public void TestWR4WSCWithManifestQuantity6()
		{
			SetUpData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "YNCA";
			masterBill.CU_BillNum = "2600652510";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.US_UI_NKBillIssuerSCAC = "BAW";
			houseBill1.CU_BillNum = "MIL16004075";
			houseBill1.CU_PackType = "CT";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR1                                       BAW BAW0173 20160204    0173 020416   ",
"WR2                                                                         E858",
"WSCBAW0173 02041612526283014A   85   74                                         ",
"WSD0204161159291FCBP LOCAL TRANSFER AUTHORIZED                                  ",
"WSCBAW0173 02041612526283014A   85   74 MIL16004075    38                       ",
"WSD0205161224511CENTER AND RELEASED GENERAL EXAM                                ",
"WSCBAW0173 02041612526283014A   85   74 MIL16004079    36                       ",
"WSD0205161222501CENTER AND RELEASED GENERAL EXAM                                ",
"WR1                                       BAW BAW0181 20160204    0181 020416   ",
"WR2                                                                         E858",
"WSCBAW0181 02041612526283014B   85   11                                         ",
"WSD0204161650561FCBP LOCAL TRANSFER AUTHORIZED                                  ",
"WSCBAW0181 02041612526283014B   85   11 BGY16003823     9                       ",
"WSD0205161038371CENTER AND RELEASED GENERAL EXAM                                ",
"WSCBAW0181 02041612526283014B   85   11 MIL16004090     1                       ",
"WSD0205161334171CENTER AND RELEASED GENERAL EXAM                                ",
"WSCBAW0181 02041612526283014B   85   11 MIL16004113     1                       ",
"WSD0205161121381CENTER AND RELEASED GENERAL EXAM                                ");
			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("We only need to sum up the first instance of each House or Master.", 38, declaration.JE_TotalNoOfPacks);
				AssertEquals("HAWB has manifest quantity", (ZDecimal)38, houseBill1.CU_NoOfPacks);
			});
		}

		public void TestWR4WSCWithManifestQuantity7()
		{
			WR4WSCWithManifestQuantity7Core(
"WR1                                       BAW BAW0173 20160204    0173 020416   ",
"WR1                                       UASUCMA CGM CENTAURUS   069E 021416   ",
"WR4            CNTXM130695                         364     CTN  UASU    MY1     ",
"WR502121614001C ENTER AND RELEASED GENERAL EXAM                  364      001   ",
"WN1            2811                        57078013016                          ",
"WR1                                       BWLECMA CGM CENTAURUS   069E 021516   ",
"WR4            CNTXM130695 XMN601022420            364     CTN  UASUBWLEHY1     ",
"WR501291610421Y MVOC - NVOCC BILL OF LADING MATCH                  364      001 ");
		}

		public void TestWR4WSCWithManifestQuantity7Version1()
		{
			WR4WSCWithManifestQuantity7Core(
"WR1                                       BAW BAW0173 20160204    0173 020416   ",
"WR1                                       UASUCMA CGM CENTAURUS   069E 021416   ",
"WR4            CNTXM130695                         364       CTN  UASU    MY11  ",
"WR502121614001C ENTER AND RELEASED GENERAL EXAM                  364      001   ",
"WN1            2811                        57078013016                          ",
"WR1                                       BWLECMA CGM CENTAURUS   069E 021516   ",
"WR4            CNTXM130695 XMN601022420            364       CTN  UASUBWLEHY11  ",
"WR501291610421Y MVOC - NVOCC BILL OF LADING MATCH                  364      001 ");
		}

		void WR4WSCWithManifestQuantity7Core(params string[] messageBlocks)
		{
			SetUpData();

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "YNCA";
			masterBill.CU_BillNum = "2600652510";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.US_UI_NKBillIssuerSCAC = "BAW";
			houseBill1.CU_BillNum = "XMN601022420";
			houseBill1.CU_PackType = "CT";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, messageBlocks);
			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("We only need to sum up the first instance of each House or Master.", 364, declaration.JE_TotalNoOfPacks);
				AssertEquals("Bill has manifest quantity", (ZDecimal)364, houseBill1.CU_NoOfPacks);
			});
		}

		public void TestWR4WSCWithManifestQuantity8()
		{
			WR4WSCWithManifestQuantity8Core(
"WR1                                       UASUCSCL AUTUMN         0025E020716   ",
"WR4VB962339868 CNSHA537490                         923     CTN  UASU    MY1     ",
"WR50211161738NP NOTIFICATION PENDING UPDATE                      923      001   ",
"WN1VB962339868 27042704    27042006        57020012816                          ",
"WR1                                       BWLECSCL AUTUMN         0025E020716   ",
"WR4            CNSHA537490 SHA601014652            97      CTN  UASUBWLEHY1     ",
"WR5020916170019 CONVEYANCE ARRIVAL                                        001   ",
"WR1                                       BWLECSCL AUTUMN         0025E020716   ",
"WR4            CNSHA537490 SHA601014657            350     CTN  UASUBWLEHY1     ",
"WR5020916170019 CONVEYANCE ARRIVAL                                        001   ",
"WR1                                       BWLECSCL AUTUMN         0025E020716   ",
"WR4            CNSHA537490 SHA601019998            476     CTN  UASUBWLEHY1     ",
"WR5020916170019 CONVEYANCE ARRIVAL                                        001   ");
		}

		public void TestWR4WSCWithManifestQuantity8Version1()
		{
			WR4WSCWithManifestQuantity8Core(
"WR1                                       UASUCSCL AUTUMN         0025E020716   ",
"WR4VB962339868 CNSHA537490                         923       CTN  UASU    MY11  ",
"WR50211161738NP NOTIFICATION PENDING UPDATE                      923      001   ",
"WN1VB962339868 27042704    27042006        57020012816                          ",
"WR1                                       BWLECSCL AUTUMN         0025E020716   ",
"WR4            CNSHA537490 SHA601014652            97        CTN  UASUBWLEHY11  ",
"WR5020916170019 CONVEYANCE ARRIVAL                                        001   ",
"WR1                                       BWLECSCL AUTUMN         0025E020716   ",
"WR4            CNSHA537490 SHA601014657            350       CTN  UASUBWLEHY11  ",
"WR5020916170019 CONVEYANCE ARRIVAL                                        001   ",
"WR1                                       BWLECSCL AUTUMN         0025E020716   ",
"WR4            CNSHA537490 SHA601019998            476       CTN  UASUBWLEHY11  ",
"WR5020916170019 CONVEYANCE ARRIVAL                                        001   ");
		}

		void WR4WSCWithManifestQuantity8Core(params string[] messageBlocks)
		{
			SetUpData();

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "YNCA";
			masterBill.CU_BillNum = "2600652510";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.US_UI_NKBillIssuerSCAC = "CTN";
			houseBill1.CU_BillNum = "SHA601014657";
			houseBill1.CU_PackType = "CT";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "CTN";
			houseBill2.CU_BillNum = "SHA601019998";
			houseBill2.CU_PackType = "CT";

			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, messageBlocks);
			processor.Process();

			CombineAssertions(() =>
			{
				AssertEquals("We only need to sum up the first instance of each House or Master.", 826, declaration.JE_TotalNoOfPacks);
				AssertEquals("Bill1 has manifest quantity", (ZDecimal)350, houseBill1.CU_NoOfPacks);
				AssertEquals("Bill2 has manifest quantity", (ZDecimal)476, houseBill2.CU_NoOfPacks);
			});
		}

		public void TestWO70()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y2       " +
				"Y  3901SV9CQ00001";

			var processor = new ACECargoManifestStatusQueryProcessor();

			message.EM_MessageNum = "HYEDUSCMT_159673";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
				"WO101101SV97003838301             APLUAA TEST1001T      Y                       ",
				"WO20RSNUnable to verify CBP disposition for full bill quantity on original entry",
				"WO50010815104491NO BILL MATCH                                                   ",
				"WO60121514095717 ELECTRONIC INVOICE REQUIRED                     20             ",
				"WO60121514075622 RELEASE DATE SET                       12161403                ",
				"WO70ODS000121514095901PGA DETAILS ACCEPTED        070401123                     ",
				"WO711 1234567890  0616150910     100144                                         ",
				"WO712 2222222222  0316150910     122126                                         ",
				"WR12704SV9 7003838301                     APLUAA TEST1            001T 120414   ",
				"WR2                                                                         A001",
				"WR3001HK8438909090                                                              ",
				"WR4            MASTER11    HOUSE34                 00000034KG   APLUWERTMN1     ",
				"WR512021407561Y MVOC-NVOCC BILL OF LADING MATCH                  00000049       ",
				"WR5120214075619 CONVEYANCE ARRIVAL                                              ",
				"WN1            27042704                27046200011301462000 TEST CON   113014   ");

			processor.Process();
			AssertEquals("message has a linked object", entry, message.EM_LinkedObject);

			AssertEquals("PGA dispositions", 1, entry.Declaration.OGADispositionCodes.Count);
			AssertEquals("PGA dispositions", "04", entry.Declaration.OGADispositionCodes[0].US_Code);
			AssertEquals("PGA dispositions", "01", entry.Declaration.OGADispositionCodes[0].US_OGADispositionStatusCode);
			AssertEquals("PGA dispositions", "07", entry.Declaration.OGADispositionCodes[0].US_OGADispositionStatusCodeEntryLine);
			AssertEquals("PGA dispositions", "01", entry.Declaration.OGADispositionCodes[0].US_ReviewReasonCode);
			AssertEquals("PGA dispositions", "123", entry.Declaration.OGADispositionCodes[0].US_OGADispositionBeginningCBPLine);

			AssertEquals("Disposition codes for entry", 2, entry.Declaration.DispositionCodes.Count);
			AssertEquals("Disposition codes for entry", "17", entry.Declaration.DispositionCodes[0].US_Code);
			AssertEquals("Disposition codes for entry", "22", entry.Declaration.DispositionCodes[1].US_Code);
			AssertEquals("Release Date updated", new ZDate(2014, 12, 16), entry.Declaration.JE_EntryAuthorisationDate);

			AssertEquals(2, entry.Declaration.OGADispositionCodes[0].OGADispositionDetails.Count);
			AssertEquals("100", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[0].US_LineSubReasonCode1);
			AssertEquals("144", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[0].US_LineSubReasonCode2);
			AssertEquals("1", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[0].US_ReferenceIDQualifier);
			AssertEquals("1234567890", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[0].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[0].US_ReceiptDateTime);

			AssertEquals("122", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[1].US_LineSubReasonCode1);
			AssertEquals("126", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[1].US_LineSubReasonCode2);
			AssertEquals("2", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[1].US_ReferenceIDQualifier);
			AssertEquals("2222222222", entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[1].US_ReferenceID);
			AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), entry.Declaration.OGADispositionCodes[0].OGADispositionDetails[1].US_ReceiptDateTime);
		}

		public void TestWR5ShouldNotSetEntryDispositions()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "AZ2";
			declaration.JE_TotalNoOfPacksPackType = "";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "10367983";

			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "NYKS";
			masterBill.CU_BillNum = "2600652510";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "JHJT";
			houseBill.CU_BillNum = "B602160067M";
			houseBill.CU_NoOfPacks = 37m;
			houseBill.CU_PackType = "CT";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.US_UI_NKBillIssuerSCAC = "JHJT";
			houseBill2.CU_BillNum = "B602160067L";
			houseBill2.CU_NoOfPacks = 22m;
			houseBill2.CU_PackType = "CT";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "OIAOUSGLO_532719";
			outgoing.EM_MessageText = "B  2904AZ2CQ                                               OIAOUSGLO_532719     WR1                            NYKS2600652510                         Y         Y  2904AZ2CQ";
			declaration.Messages.Add(outgoing);

			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "OIAOUSGLO_532719";

			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WR1                                       NYKSHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510                          00000666CTN  NYKS    MY1     ",
"WR5032916045155 CARRIER BILL AMENDMENT - ADD                     666      001   ",
"WN1            1001                        57035032816                          ",
"WR1                                       CHQFHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  CSHUA6030942            00000048CTN  NYKSCHQFHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  48       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067CA            00000072CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  72       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067CB            00000138CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  138      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067CD            00000074CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  74       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067D             00000149CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  149      001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067E             00000003PKG  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  3        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067G             00000002PKG  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  2        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067J             00000001CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  1        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067L             00000022CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  22       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067M             00000037CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  37       001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067N             00000003PKG  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  3        001   ",
"WR1                                       JHJTHYUNDAI INTEGRAL    050E 042316   ",
"WR4            2600652510  B602160067R             00000104CTN  NYKSJHJTHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  104      001   ",
"WR1                                       MIQOHYUNDAI INTEGRAL    050E 042916   ",
"WR4            2600652510  SHS16001441             00000013CTN  NYKSMIQOHY1     ",
"WR503291604511Y MVOC-NVOCC BILL OF LADING MATCH                  13       001   ");

			processor.Process();
			AssertEquals("message has a linked object", declaration, message.EM_LinkedObject);

			AssertEquals("Declaration dispositions", 0, declaration.DispositionCodes.Count);
			AssertEquals("Bill dispositions", 1, masterBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "55", masterBill.DispositionCodes[0].US_Code);

			var statusMessage = Factory.New<MQEDIMessage>();
			statusMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			statusMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			statusMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			statusMessage.EM_Status = EDIMessage.Status.Queued;
			statusMessage.EM_MessageText = "B004601AZ2SO                                                                    SO104601AZ2  10367983 0134-204611100NYKSHYUNDAI INTEGRAL    50E  042316         SO20CR S00441418                                                                SO40MNYKS2600652510                                                             SO40HJHJTB602160067L                                       00000022     00000022SO50042516170395BILL ARRIVED                                                    SO40MNYKS2600652510                                                             SO40HJHJTB602160067M                                       00000037     00000037SO50042516170395BILL ARRIVED                                                    SO60042516170390UNDER CBP REVIEW                                                Y  4601AZ2SO00000";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			declaration.DispositionCodes.Reload(true);
			AssertEquals("Declaration dispositions - only Disposition Code '90' from the block SO60 should be added for declaration", 1, declaration.DispositionCodes.Count);
			AssertEquals("90", declaration.DispositionCodes[0].US_Code);

			masterBill.DispositionCodes.Reload(true);
			AssertEquals("Bill dispositions", 1, masterBill.DispositionCodes.Count);
			AssertEquals("Bill dispositions", "55", masterBill.DispositionCodes[0].US_Code);
		}

		[ExpectNoExceptions]
		[TestDate(2021, 12, 17)]
		public void TestDispositionOrderGreaterThanShortMaximumValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBill = "2750955000";
			declaration.JE_MasterBillIssuerSCAC = "MATS";
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var bill = declaration.PrimaryMasterBill;
			var disposition = bill.DispositionCodes.AddNewIfNotExist("1C", new ZDateTime(2021, 12, 06));
			disposition.US_Order = short.MaxValue;
			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_343257";
			outgoing.EM_MessageText = "B  53018SYCQ                                               HYEDUSCMT_343257     WR1                            MATS2750955000                         Y 2       Y  53018SYCQ";
			declaration.Messages.Add(outgoing);

			var message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "HYEDUSCMT_343257";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = @"B0053018SYC1                                               HYEDUSCMT_343257     " +
"WR1                                       MATSMANULANI            176  122121   " +
"WR2                                                                         Z773" +
"WR4            2750955000                          00009143PCS  MATS    MY1     " +
"WR5120621043169 BILL ON FILE                                     900      001   " +
"WR5120621043254 CARRIER BILL AMENDMENT - DELETE                  900      002   " +
"WR5120621043255 CARRIER BILL AMENDMENT - ADD                     900      003   " +
"WR512062121413Z BOL MATCHED TO ISF                               0        004   " +
"WR5120721195454 CARRIER BILL AMENDMENT - DELETE                  900      005   " +
"WR5120721195455 CARRIER BILL AMENDMENT - ADD                     9143     006   " +
"WR512142101113Z BOL MATCHED TO ISF                               0        999   " +
"WR512142101113Z BOL MATCHED TO ISF                               0        999   " +
"WR512142101113Z BOL MATCHED TO ISF                               0        999   " +
"WN1            2709                        57035120821                          " +
"Y  53018SYC100120";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedMessage = newFactory.Load<EDIMessage>(message.PK);
			AssertEquals("loadedMessage.EM_Status", EDIMessage.Status.Received, loadedMessage.EM_Status);
			var loadedBill = newFactory.Load<Bill>(bill.PK);
			AssertEquals("loadedBill.DispositionCodes.Count", 8, loadedBill.DispositionCodes.Count);
			AssertEquals("Latest disposition code", "3Z", loadedBill.DispositionCodes.GetLatestDisposition().US_Code);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals(4, System.Text.RegularExpressions.Regex.Matches(email.Body, "<td>BOL MATCHED TO ISF</td>").Count);
		}

		public void TestEmailHasDispositionDetails()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText =
			@"B  3901WFBCQ                                               HYEDUSCMT_159673     " +
			 "WR1                                            29759922004TVL040150             " +
			 "Y  3901WFBCQ";

			var processor = new ACECargoManifestStatusQueryProcessor();

			message.EM_MessageNum = "HYEDUSCMT_159673";
			message.EM_MessageText =
			@"B003901WFBC1                                               HYEDUSCMT_159673     " +
			 "WR1                                       CAL CAL5148 20160619    5148 061916   " +
			 "WSCCAL5148 06191629759922004    90        TVL040150    90                       " +
			 "WSD0620161014201CENTER AND RELEASED GENERAL EXAM                                " +
			 "Y  3901WFBC100003";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
				"WR1                                       CAL CAL5148 20160619    5148 061916   ",
				"WSCCAL5148 06191629759922004    90        TVL040150    90                       ",
				"WSD0620161014201CENTER AND RELEASED GENERAL EXAM                                ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];

			Assert(email.Body.Contains("<td>1C</td><td>ENTER AND RELEASED GENERAL EXAM</td>"));
		}

		public void TestDispositionFollowsBillDetail()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			// dummy send message
			outgoing.EM_MessageNum = "AWUJECJEC_285132";
			outgoing.EM_MessageText =
				"B013901SV9CQ                                               HYEDUSCMT_159673     " +
				"WR1    SV9 70038383                                                    Y        " +
				"Y  3901SV9CQ00001";
			outgoing.EM_ApplicationReference = "UpdateEntryWithResults";

			var processor = new ACECargoManifestStatusQueryProcessor();

			message.EM_MessageNum = "AWUJECJEC_285132";
			message.EM_MessageText =
			@"B004701820C1                                               AWUJECJEC_285132     " +
			 "WR1                                       UAL UAL0056 20160906    0056 090616   " +
			 "WSCUAL0056 09061601604084916     3                            01604084916AR61   " +
			 "WSD0906160535381DIN-BOND MOVEMENT AUTHORIZED BY CBP                             " +
			 "WSD0906161619051DIN-BOND MOVEMENT AUTHORIZED BY CBP                             " +
			 "WSCUAL0056 09061601604084916     3         IM116080     1                       " +
			 "WSD0908161724361CENTER AND RELEASED GENERAL EXAM                                " +
			 "WSD0908161725514EENTRY CANCEL / DELETE                                          " +
			 "WSD0908161725511CENTER AND RELEASED GENERAL EXAM                                " +
			 "WSCUAL0056 09061601604084916     3        IM1160801     1                       " +
			 "WSD0908160947451CENTER AND RELEASED GENERAL EXAM                                " +
			 "WSD0908160948274EENTRY CANCEL / DELETE                                          " +
			 "WSD0908160948271CENTER AND RELEASED GENERAL EXAM                                " +
			 "WSCUAL0056 09061601604084916     3        IM1160802     1                       " +
			 "WSD0908160957211CENTER AND RELEASED GENERAL EXAM                                " +
			 "WSD0908160958084EENTRY CANCEL / DELETE                                          " +
			 "WSD0908160958081CENTER AND RELEASED GENERAL EXAM                                " +
			 "Y  4701820C100016                                                               ";

			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
				"WR1                                       UAL UAL0056 20160906    0056 090616   ",
				"WSCUAL0056 09061601604084916     3                            01604084916AR61   ",
				"WSD0906160535381DIN-BOND MOVEMENT AUTHORIZED BY CBP                             ",
				"WSD0906161619051DIN-BOND MOVEMENT AUTHORIZED BY CBP                             ",
				"WSCUAL0056 09061601604084916     3         IM116080     1                       ",
				"WSD0908161724361CENTER AND RELEASED GENERAL EXAM                                ",
				"WSD0908161725514EENTRY CANCEL / DELETE                                          ",
				"WSD0908161725511CENTER AND RELEASED GENERAL EXAM                                ",
				"WSCUAL0056 09061601604084916     3        IM1160801     1                       ",

				// disposition blocks (WR5), not from original message.
				"WR5121214075673 AMS FLIGHT NOT DEPARTED/ARRIVED                  00000049       ",
				"WR5121214075969 BILL ON FILE                                     1920     001   ",
				"WR512121407593Z BOL MATCHED TO ISF                               1920     002   ",
				"WR5121214075872 CBPA INSPECTION/DOC REVIEW HOLD                  1920     003   ",
				"WR5121214075375 CBPA DOC REVIEW HOLD REMOVED                     1920     004   ",
				"WR5121214075219 CONVEYANCE ARRIVAL                                        005   ",

				// disposition error blocks (WSD)
				"WSD0908160947451CENTER AND RELEASED ABCDEFG                                     ",
				"WSD0908160948274EENTRY CANCEL / DELETE                                          ",
				"WSD0908160948271CENTER AND RELEASED GENERAL EXAM                                ",

				"WSCUAL0056 09061601604084916     3        IM1160802     1                       ",

				"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      006   ",
				"WR51212140756A1 FDA PN ADVISORY                                  1920     007   ",
				"WR512121407581F ENTER AND RELEASED GENERAL EXAM                  1920     008   ",
				"WR512121407564E ENTRY CANCEL / DELETE                            1920     009   ",
				"WR512121407561F ENTER AND RELEASED GENERAL EXAM                  960      010   ",

				"WSD0908160957211CENTER AND RELEASED HIJKLMN                                     ",
				"WSD0908160958084EENTRY CANCEL / DELETE                                          ",
				"WSD0908160958081CENTER AND RELEASED GENERAL EXAM                                ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var emailBody = email.Body;

			var bill1TableHeaderIndex = emailBody.IndexOf("<b>01604084916/IM1160801 Bills Details</b>");
			var bill1DispositionTableIndex = emailBody.IndexOf("<td>AMS FLIGHT NOT DEPARTED/ARRIVED</td>");
			var bill1DispositionErrorTableIndex = emailBody.IndexOf("<td>ENTER AND RELEASED ABCDEFG</td>");
			var bill2TableHeaderIndex = emailBody.IndexOf("<b>01604084916/IM1160802 Bills Details</b>");
			var bill2DispositionErrorTableIndex = emailBody.IndexOf("<td>ENTER AND RELEASED HIJKLMN</td>");

			Assert("'IM1160801 Bills Details' should exist", bill1TableHeaderIndex > 0);
			Assert("Disposition table should exist", bill1DispositionTableIndex > 0);
			Assert("Bill 1 Disposition errors table should exist", bill1DispositionErrorTableIndex > 0);
			Assert("'IM1160802 Bills Details' should exist", bill2TableHeaderIndex > 0);
			Assert("Bill 2 Disposition errors table should exist", bill2DispositionErrorTableIndex > 0);

			Assert("Disposition table should follow 'IM1160801 Bills Details'", bill1TableHeaderIndex < bill1DispositionTableIndex);
			Assert("Bill 1 Disposition errors table should follow Disposition table", bill1DispositionTableIndex < bill1DispositionErrorTableIndex);
			Assert("'IM1160802 Bills Details' should follow Bill 1 Disposition errors table", bill1DispositionErrorTableIndex < bill2TableHeaderIndex);
			Assert("Bill 2 Disposition errors table should follow 'IM1160802 Bills Details'", bill2TableHeaderIndex < bill2DispositionErrorTableIndex);
		}

		public void TestProcessDispositionsWithSameDate()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "73000414";

			outgoing.EM_MessageNum = "HYEDUSCMT_199876";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_199876     WR1    SV9 73000414                                                             Y  1101SV9CQ";

			message.EM_MessageNum = "HYEDUSCMT_199876";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "HYEDUSCMT_199876";
			message.EM_MessageText =
				"B001101SV9C1                                               HYEDUSCMT_199876     " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919131795BILL ARRIVED                                                    " +
"WO60030919131798RELEASED                                03091901                " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919131795BILL ARRIVED                                                    " +
"WO60030919131790UNDER CBP REVIEW                                                " +
"WO60030919131790UNDER CBP REVIEW                                                " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919131795BILL ARRIVED                                                    " +
"WO60030919131781IN-BOND PORT DISCREPANCY                                        " +
"Y  1101SV9C100036";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(entry.Declaration.PK);
			AssertEquals(CRLReleaseStatusList.Codes.REL, decLoaded.ReleaseStatus);
			AssertEquals(new ZDateTime(2019, 03, 09), decLoaded.JE_EntryAuthorisationDate.Date);
		}

		public void TestReleaseStatusWithOneUSGInC1()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "73000414";

			outgoing.EM_MessageNum = "HYEDUSCMT_199876";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_199876     WR1    SV9 73000414                                                     2       Y  1101SV9CQ";

			message.EM_MessageNum = "HYEDUSCMT_199876";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "HYEDUSCMT_199876";
			message.EM_MessageText =
				"B001101SV9C1                                               HYEDUSCMT_199876     " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO60030919131801ONE USG                                                         " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919131795BILL ARRIVED                                                    " +
"WO60030919131790UNDER CBP REVIEW                                                " +
"WO60030919131798RELEASED                                03091901                " +
"WO60030919131701ONE USG                                                         " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919130995BILL ARRIVED                                                    " +
"WO60030919130990UNDER CBP REVIEW                                                " +
"WO60030919130990UNDER CBP REVIEW                                                " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919130895BILL ARRIVED                                                    " +
"WO60030919130890UNDER CBP REVIEW                                                " +
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR BCHM00012663                                                             " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919130795BILL ARRIVED                                                    " +
"WO60030919130797ADMISSIBLE                                                      " +
"WO101101SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         " +
"WO20CR M00012663                                                                " +
"WO40RNISD00147479                                          00000001     00000001" +
"WO50030919004494BILL DEPARTED                                                   " +
"WO60030919004497ADMISSIBLE                                                      " +
"WO101101SV9  73000414 01031101-00085                             030819         " +
"WO20CR M00012663                                                                " +
"WO40RNISD00147479                                          00000001     00000000" +
"WO50030819120991NO BILL MATCH                                                   " +
"Y  1101SV9C100036";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(entry.Declaration.PK);
			AssertEquals(CRLReleaseStatusList.Codes.REL, decLoaded.ReleaseStatus);
			AssertEquals(new ZDateTime(2019, 03, 09), decLoaded.JE_EntryAuthorisationDate.Date);
		}

		public void TestEmailDoesNotContainSSNNumber()
		{
			SetUpData();
			var masterBill = declaration.Bills.AddNew();
			masterBill.US_UI_NKBillIssuerSCAC = "MOLU";
			masterBill.CU_BillNum = "13700107330";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "MOLU";
			houseBill.CU_BillNum = "HOUSEBILL01";

			var itNumber = houseBill.ITAndSplitDetails.AddNew();
			itNumber.US_ITNumber = "V12345678";

			declaration.Messages.Add(outgoing);

			outgoing.EM_MessageNum = "HYEDUSCMT_159854";
			outgoing.EM_MessageText = "B  1101SV9CQ                                               HYEDUSCMT_159854     WR1                V12345678                                                    Y  1101SV9CQ";
			message.EM_MessageNum = "HYEDUSCMT_159854";
			var processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WO100708SV9  73000414 01123-45-6789 NISD864X2TR864X2        19030030919         ",
"WR2                                                                         A001",
"WR4V12345678   13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WS5AR121414121314                                                               ",
"WN1            27042704                27046200012121462000 TEST CON   121214   ",
"WSCKK0QF13706061511223687532     9                             1122368753ER     ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			Assert("Email does not contain SSN Number", !emails[0].Body.Contains("<td>Importer of Record Number</td><td>123-45-6789</td>"));

			processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
"WO100708SV9  73000414 01031101-00085NISD864X2TR864X2        19030030919         ",
"WR2                                                                         A001",
"WR4V12345678   13700107330 HOUSEBILL01             00000034KG   MOLUMOLUMN1     ",
"WS5AR121414121314                                                               ",
"WN1            27042704                27046200012121462000 TEST CON   121214   ",
"WSCKK0QF13706061511223687532     9                             1122368753ER     ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			AssertEquals("1 email sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			Assert("EIN Number or CBP Number included in the mail", emails[0].Body.Contains("<td>Importer of Record Number</td><td>031101-00085</td>"));
		}

		protected override void EndToEndCore()
		{
			SetUpData();
			entry.Messages.Add(outgoing);
			entry.EntryNumber = "70038383";

			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 70038383                                                    Y2       Y  3901SV9CQ00001";

			var processor = new ACECargoManifestStatusQueryProcessor();

			message.EM_MessageNum = "HYEDUSCMT_159673";
			message.EM_MessageText =
				"B001101SV9C1                                               HYEDUSCMT_159673     " +
				"WO101101SV97003838301             APLUAA TEST1001T      Y                       " +
				"WO20RSNUnable to verify CBP disposition for full bill quantity on original entry" +
				"WO50010815104491NO BILL MATCH                                                   " +
				"WO60121514095717 ELECTRONIC INVOICE REQUIRED                     20             " +
				"WO60121514075622 RELEASE DATE SET                       12161403                " +
				"WO70ODS000121514095901PGA DETAILS ACCEPTED        070401123                     " +
				"WR12704SV9 7003838301                     APLUAA TEST1            001T 120414   " +
				"WR2                                                                         A001" +
				"WR3001HK8438909090                                                              " +
				"WR4            MASTER11    HOUSE34                 00000034KG   APLUWERTMN1     " +
				"WR512021407561Y MVOC-NVOCC BILL OF LADING MATCH                  00000049       " +
				"WN1            27042704                27046200011301462000 TEST CON   113014   " +
				"Y  3901SV9SO00000";
			processor.Message = message;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
				"WO101101SV97003838301             APLUAA TEST1001T      Y                       ",
				"WO20RSNUnable to verify CBP disposition for full bill quantity on original entry",
				"WO50010815104491NO BILL MATCH                                                   ",
				"WO60121514095717 ELECTRONIC INVOICE REQUIRED                     20             ",
				"WO60121514075622 RELEASE DATE SET                       12161403                ",
				"WO70ODS000121514095901PGA DETAILS ACCEPTED        070401123                     ",
				"WR12704SV9 7003838301                     APLUAA TEST1            001T 120414   ",
				"WR2                                                                         A001",
				"WR3001HK8438909090                                                              ",
				"WR4            MASTER11    HOUSE34                 00000034KG   APLUWERTMN1     ",
				"WR512021407561Y MVOC-NVOCC BILL OF LADING MATCH                  00000049       ",
				"WN1            27042704                27046200011301462000 TEST CON   113014   ");

			processor.Process();
			AssertEquals("message has a linked object", entry, message.EM_LinkedObject);

			AssertEquals("PGA dispositions", 1, entry.Declaration.OGADispositionCodes.Count);
			AssertEquals("PGA dispositions", "04", entry.Declaration.OGADispositionCodes[0].US_Code);
			AssertEquals("PGA dispositions", "01", entry.Declaration.OGADispositionCodes[0].US_OGADispositionStatusCode);
			AssertEquals("PGA dispositions", "07", entry.Declaration.OGADispositionCodes[0].US_OGADispositionStatusCodeEntryLine);
			AssertEquals("PGA dispositions", "01", entry.Declaration.OGADispositionCodes[0].US_ReviewReasonCode);
			AssertEquals("PGA dispositions", "123", entry.Declaration.OGADispositionCodes[0].US_OGADispositionBeginningCBPLine);

			AssertEquals("Disposition codes for entry", 2, entry.Declaration.DispositionCodes.Count);
			AssertEquals("Disposition codes for entry", "17", entry.Declaration.DispositionCodes[0].US_Code);
			AssertEquals("Disposition codes for entry", "22", entry.Declaration.DispositionCodes[1].US_Code);
			AssertEquals("Release Date updated", new ZDate(2014, 12, 16), entry.Declaration.JE_EntryAuthorisationDate);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "EDIDUSDAT_4010997";
			outgoing.EM_MessageText = "B013901SV9CQ                                               EDIDUSDAT_4010997    WR1    SV9 70038383                                                    Y        Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message.EM_MessageNum = "EDIDUSDAT_4010997";
			message.EM_MessageText = "B001101SV9C1                                               EDIDUSDAT_4010997    WO103901SV97003838397 0123-456789012ALP                     2538 0909131        WO20CR B00160701                                                                WO40M    ALP61325218                                                            WO40H    HAWB001                                           00001350AT   00000000WO50093013005495BILL ARRIVED                             Y                      WO60093013005498RELEASED                                09301301                Y  3901SV9SO00000";

			processor = new ACECargoManifestStatusQueryProcessor();
			processor.Message = message;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse,
	"WO103901SV97003838397 0123-456789012ALP                     2538 0909131        ",
	"WO20CR B00160701                                                                ",
	"WO40M    ALP61325218                                                            ",
	"WO40H    HAWB001                                           00001350AT           ",
	"WO50093013005495BILL ARRIVED                             Y                      ",
	"WO60093013005498RELEASED                                09301301                ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals(1, ((ZString)email.Body).OccurrencesIgnoringCase("The system has already processed a Release Notification message with more current details. Release details on the Declaration have not been updated as a result."));
		}

		void SetUpData()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "SV9";
			declaration.JE_TotalNoOfPacksPackType = "";

			entry = declaration.CustomsEntryHeaders.AddNew();

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
		}
		JobDeclaration declaration;
		CusEntryHeader entry;
		MQEDIMessage outgoing;
		MQEDIMessage message;
	}
}
