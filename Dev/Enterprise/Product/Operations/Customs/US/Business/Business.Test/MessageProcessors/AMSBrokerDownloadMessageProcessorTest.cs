using System;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class AMSBrokerDownloadMessageProcessorTest : ABIProcessorTest<AMSBrokerDownloadMessageProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BrokerManifestDownload;

			message.EM_MessageText = "B018888XJ5BD                                                                    1MCNRU20CATRAIN                  0530100001000001Y       P                      2MCNRU00000005350902                                                            1P380210280500001    0000                                                       1JCNRU                                                                          1B000000053509801070000000020PCS  0000001450LB                                  2B0000000000  TORONTO MACMIL YA                                                 0NSH ACCESS TECHNOLOGIES                                                        2N2                                                                             3NTORONTO MACMIL YARDON                                                         0NCN ACME PACKAGING CORP                                                        2N5                                                                             3NCHICAGO INTER TERM IL                                                         0NCB CARGOWISE                          173802144                               1CCN  639956                                  RR000000000000000000000           0D           000000000000001450LB                                               1D0000000020LUMBER OR TIMBER, ROUGH OR DRESSED, DRIED                     XO    2DNO MARKS OR NUMBERS                                                           1JCNRU                                                                          1B000000053519801070000000020PCS  0000001450LB                                  2B0000000000  TORONTO MACMIL YA                                                 0NSH ACCESS TECHNOLOGIES                                                        2N2                                                                             3NTORONTO MACMIL YARDON                                                         0NCN ACME PACKAGING CORP                                                        2N5                                                                             3NCHICAGO INTER TERM IL                                                         0NCB CARGOWISE                          173802144                               1CCN  639956                                  RR000000000000000000000           0D           000000000000001450LB                                               1D0000000020LUMBER OR TIMBER, ROUGH OR DRESSED, DRIED                     XO    2DNO MARKS OR NUMBERS                                                           Y018888XJ5BD00017";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("AMS Broker Download"); }));
			AssertNotNull("An email should have been sent with subject containing 'Cargo Manifest Query'", email);

			IssuerAndBillNumber[] bills = new IssuerAndBillNumber.Loader(Factory).Load(message);
			AssertEquals(2, bills.Length);
			AssertEquals("BillsOfLading", "CNRU000000053509", bills[0].CY_Data);
			AssertEquals("BillsOfLading", "CNRU000000053519", bills[1].CY_Data);

			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestProcessWithMoreBlocks()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BrokerManifestDownload;

			message.EM_MessageText = "B018888XJ5BD                                                                    1MCNRU20CATRAIN                  0530100001000001Y       P                      2MCNRU00000005350902                                                            1P380210280500001    0000                                                       1JCNRU                                                                          1ACNRU3901A0000000535090000000535                                               1B000000053509801070000000020PCS  0000001450LB                                  2B0000000000  TORONTO MACMIL YA                                                 4BBN 123456789012345678901234567890                                             4BMA 123456789012345678901234567891                                             0NSH ACCESS TECHNOLOGIES                                                        2N2                                                                             3NTORONTO MACMIL YARDON                                                         4NEMJOO.YOUM@CARGOWISE.COM                                                      0NCN ACME PACKAGING CORP                                                        2N5                                                                             3NCHICAGO INTER TERM IL                                                         0NCB CARGOWISE                          173802144                               1I61N 123456789CNRU3901     1234567822-1234567AB12345678901                     1CCN  639956                                  RR000000000000000000000           2IS QUEEN VICTORIA                                                              2C123456789012345678901234567890     587411234567890                            2C123456789012345678901234567891     587411234567891                            0D           000000000000001450LB                                               1D0000000020LUMBER OR TIMBER, ROUGH OR DRESSED, DRIED                     XO    2DNO MARKS OR NUMBERS                                                           1V123456789012341DANGEROUS/EXPLOSIVE MATERIAL  55555555555                      2V 30CEN                                                                        3VDANGEROUS/EXPLOSIVE MATERIAL  FREE FORM DESCRIPTION OF HAZARDOUS MA           Y018888XJ5BD00017";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			IssuerAndBillNumber[] bills = new IssuerAndBillNumber.Loader(Factory).Load(message);
			AssertEquals(1, bills.Length);
		}

		public void TestProcess_CS00229138()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.BrokerManifestDownload;

			message.EM_MessageText = "B018888XJ5BD                                                                    1MPIFW11MYOOCL BEIJING           20E26     000001        W                      2M573732                                                                        1P2709071513                                                                    1JPIFW                                                                          1BTPE13061244 583090000000005CTN  0000000077KGN                                 2B0000000000  KEELUNG, CHINA (T            NYKS                                 4BOB NYKS2061071986                                                             0NSH JIN TAY INDUSTRI                                                           2N1F NO486, SEC 3, MIN CHIH RD, TAI SHAN HSIANG, NEW TAIPEI TW 243              0NCN ILLINOIS LOCK COMPANY                                                      2N301 W HINTZ RD                     WHEELING IL 60090                          0NCB C.H. ROBINSON INTERNATIONAL IN     173501791                               1CTRLU9623652   AFM5291                         000000000000000000000    L      1D0000000005CAM LOCK,KEY,CAM                                           CTN      2DILLIONIS LOCK C                                                               2DO.,(IN TRI.),CT                                                               2DN NO.:,MADE IN                                                                2DTAIWAN,R.O.C.                                                                 Y  3501791BD00018";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("AMS Broker Download"); })));
		}
	}
}
