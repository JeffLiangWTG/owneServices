using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class FIRMSProcessorTest : ABIProcessorTest<ACEFIRMSProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			ABIOutputBlockControlGenerator generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			ERFF211 f211 = new ERFF211();
			ERFF311 f311 = new ERFF311();
			ERFF411 f411 = new ERFF411();
			generator.AddMessageBlock(f211);
			generator.AddMessageBlock(f311);
			generator.AddMessageBlock(f411);

			ProcessMessage(generator);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 0; }
		}

		protected override bool ClearEmails
		{
			get { return false; }
		}

		public void TestErrorResponse1()
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			messageResponse = mock.Object;
			messageResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			messageResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageResponse.EM_MessageNum = "8862";
			messageResponse.EM_MessageText =
				"B  8888XJ5FO                                               11333                " +
				"F111    DSAFASDFASDFD                      99                                   " +
				"F411                            0D6FACILITY NAME NOT ON FILE                    " +
				"Y  8888XJ5FR00003                                                               ";

			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			AssertEquals("FIRMS Request Response for FIRMS Code: '' Name of Facility: 'DSAFASDFASDFD' District Code: '99'", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}

		public void TestUpdateIsActiveOnErrorResponse()
		{
			var firmsCode = Factory.LoadTop1<USCFIRMS>(new ZQuery(USCFIRMSSchema.US_Code, "T487"));
			if (firmsCode == null)
			{
				firmsCode = Factory.New<USCFIRMS>();
				firmsCode.US_Code = "T487";
			}
			firmsCode.US_IsActive = true;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			messageResponse = mock.Object;
			messageResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			messageResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageResponse.EM_MessageNum = "8862";
			messageResponse.EM_MessageText =
				"B  8888XJ5FO                                               11333                " +
				"F111T487                                                                        " +
				"F411                            012FIRMS CODE NOT ON FILE                       " +
				"Y  8888XJ5FR00003                                                               ";

			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var firmsCodeLoaded = factory2.Load<USCFIRMS>(firmsCode.PK);

			AssertEquals("Should be deactivated", false, firmsCodeLoaded.US_IsActive);
		}

		public void TestIsOriginalMessageRequestForAllFirms()
		{
			//ensure that an 'all' request is a request with begin date of 1/1/1970 only
			new ReferenceFileRequester().RequestFIRMSCode();

			ZQuery query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, MQEDIMessage.ApplicationCodes.USCustomsImport);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, MQEDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_Status, MQEDIMessage.Status.Queued);
			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			MQEDIMessage[] messages = Factory.Load<MQEDIMessage>(query);
			MQEDIMessage message = messages[0];
			var blocks = message.GetMessageBlocks<ERFF111>();
			AssertNotEquals(0, blocks.Count);
			foreach (ERFF111 erff111 in blocks)
			{
				AssertEquals(new ZDate(2000, 1, 1), erff111.BeginDate);
				Assert(erff111.DistrictCode.IsEmpty);
				Assert(erff111.FIRMSCode.IsEmpty);
				Assert(erff111.NameOfFacility.IsEmpty);
				Assert(erff111.DistrictCode.IsEmpty);
			}
		}

		public void TestMaximumTopLevelMessageBlocksPerProcessor()
		{
			//ensure that FIRMS processor is an oll or nothing process. ie, all blocks are processed at the same time so the 'Active status' will end up being correct
			var fIRMSProcessor = new ACEFIRMSProcessor();
			AssertEquals(0, fIRMSProcessor.MaximumTopLevelMessageBlocksPerProcessor);
		}

		[StressTest]
		public void TestProcessResponse_AllFirmsRequested()
		{
			if (MessageTransmitted != MessageResponse)
			{
				Factory.Save();
			}
			new USRIncomingMessageProcessor().ExecuteBatch();

			int activeUSCFirms = Factory.GetDatabaseCount(typeof(USCFIRMS), new ZQuery(USCFIRMSSchema.US_IsActive, true));
			AssertEquals(13736, activeUSCFirms);
		}

		[StressTest]
		public void TestProcessResponse_SpecificRequests()
		{
			MessageTransmitted.EM_MessageText =
						"B018888XJ5FI                                               8862                 " +
						"F111                                         010189                             " +
						"Y  8888XJ5FI00001                                                               ";

			if (MessageTransmitted != MessageResponse)
			{
				Factory.Save();
			}
			new USRIncomingMessageProcessor().ExecuteBatch();

			int activeUSCFirms = Factory.GetDatabaseCount(typeof(USCFIRMS), new ZQuery(USCFIRMSSchema.US_IsActive, true));
			AssertEquals(13736, activeUSCFirms);

			int inactiveUSCFirms = Factory.GetDatabaseCount(typeof(USCFIRMS), new ZQuery(USCFIRMSSchema.US_IsActive, false));
			AssertEquals(3838, inactiveUSCFirms);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("FIRMS Request Response", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains("Your query for multiple FIRMS codes was successful", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		[StressTest]
		public void TestProcessSplitResponse()
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			MQEDIMessage messageResponse2 = mock.Object;
			messageResponse2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageResponse2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			messageResponse2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageResponse2.EM_MessageNum = "8862";
			messageResponse2.EM_MessageText =
				"B018888XJ5FO                                               8862";

			messageResponse2.EM_MessageText +=
				"                 F2111501L177A04YELLOW FREIGHT LINES               042689                        F311PO BOX 1801                        WILMINGTON                         NC    F41128402    US";

			for (int i = 0; i < 1099; i++)
			{
				messageResponse2.EM_MessageText +=
				"                                                                 F2111501L177A04YELLOW FREIGHT LINES               042689                        F311PO BOX 1801                        WILMINGTON                         NC    F41128402    US";
			}

			messageResponse2.EM_MessageText += "                                                                 Y  8888XJ5FR06720";

			var mock2 = Factory.NewMoq<MQEDIMessage>();
			mock2.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			MQEDIMessage messageResponse3 = mock2.Object;
			messageResponse3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageResponse3.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			messageResponse3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageResponse3.EM_MessageNum = "8862";
			messageResponse3.EM_MessageText =
				"B  8888XJ5FO                                               8862                 " +
				"F2111501L177A04YELLOW FREIGHT LINES               042689                        " +
				"F311PO BOX 1801                        WILMINGTON                         NC    " +
				"F41128402    US                                                                 " +
				"F2112402S369A08U S CUSTOMS DISTRICT OFFICE        050189                        " +
				"F311BRIDGE OF THE AMERICAS             EL PASO                            TX    " +
				"F41179905    US                                                                 " +
				"Y  8888XJ5FR06720                                                               ";
			Factory.Save();
			System.Threading.Thread.Sleep(1000);
			var messageCount = Factory.Load<EDIMessage>(new ZQuery()).Length;
			AssertEquals(2, messageCount);
			new USRIncomingMessageProcessor().ExecuteBatch();

			var body = Env.OutgoingCustomsMailManager.EmailsCreated[0].Body;
			AssertEquals("Should only be one email for a FIRMS code response split over multiple messages", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("FIRMS Request Response", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertContains("Your query for multiple FIRMS codes was successful", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestProcessResponse_()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			MessageResponse.EM_MessageText =
						"B  8888XJ5FO                                               8862                 " +
						"F2112801Y874A01SUMMIT CFS, INC.                   022208                        " +
						"F311501 S AIRPORT BLVD                 SOUTH SAN FRANCISCO                CA    " +
						"F411940806920US                                                                 " +
						"Y  8888XJ5FR06720                                                               ";
			Factory.Save();
			new USRIncomingMessageProcessor().ExecuteBatch();

			USCFIRMS firm = Factory.LoadTop1<USCFIRMS>(new ZQuery(USCFIRMSSchema.US_Code, "Y874"));
			AssertEquals("SOUTH SAN FRANCISCO", firm.US_City);
			AssertEquals("501 S AIRPORT BLVD", firm.US_Address);
			AssertEquals("CA", firm.US_State);
			AssertEquals("US", firm.US_Country);
			AssertEquals("940806920", firm.US_ZipCode);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("FIRMS Request Response for FIRMS Code: 'Y874' Name of Facility: 'SUMMIT CFS, INC.' District Code: '2801'", email.Subject);

			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestProcessResponseWithEmptyCode()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			MessageResponse.EM_MessageText =
						"B  8888XJ5FO                                               8862                 " +
						"F2112801    A01SUMZIT CFS, INC.                   022208                        " +
						"F311501 S AIRPORT BLVD                 SOUTH SAN FRANCISCO                CA    " +
						"F411940806920US                                                                 " +
						"Y  8888XJ5FR06720                                                               ";
			Factory.Save();
			new USRIncomingMessageProcessor().ExecuteBatch();

			var firm = Factory.LoadTop1<USCFIRMS>(new ZQuery(USCFIRMSSchema.US_Name, SQLComparisonOperator.StartsWith, "SUMZIT"));
			AssertNull(firm);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("FIRMS Request Response for FIRMS Code: '' Name of Facility: 'SUMZIT CFS, INC.' District Code: '2801'", email.Subject);
		}

		#region Implementation

		protected override MessageProcessorFactory CreateNewMessageProcessorFactory(BatchProcessor.LoggingInformation logger)
		{
			return new USRMessageProcessorFactory(logger);
		}

		#region Message Transmitted

		MQEDIMessage MessageTransmitted
		{
			get
			{
				if (messageTransmitted == null)
				{
					var mock = Factory.NewMoq<MQEDIMessage>();
					mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
					messageTransmitted = mock.Object;
					messageTransmitted.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReference;
					messageTransmitted.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
					messageTransmitted.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					messageTransmitted.EM_MessageNum = "8862";
					messageTransmitted.EM_MessageText =
						"B018888XJ5FQ                                               8862                 " +
						"F111                                         010170                             " +
						"Y  8888XJ5FI00001                                                               ";
				}
				return messageTransmitted;
			}
		}
		MQEDIMessage messageTransmitted;

		#endregion

		#region Message Response

		MQEDIMessage MessageResponse
		{
			get
			{
				if (messageResponse == null)
				{
					var mock = Factory.NewMoq<MQEDIMessage>();
					mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
					messageResponse = mock.Object;
					messageResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
					messageResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
					messageResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					messageResponse.EM_MessageNum = "8862";
					messageResponse.EM_MessageText =
						"B  8888XJ5FO                                               8862                 " +
						"F2111501L177A04YELLOW FREIGHT LINES               042689                        " +
						"F311PO BOX 1801                        WILMINGTON                         NC    " +
						"F41128402    US                                                                 " +
						"F2112402S369A08U S CUSTOMS DISTRICT OFFICE        050189                        " +
						"F311BRIDGE OF THE AMERICAS             EL PASO                            TX    " +
						"F41179905    US                                                                 " +
						"Y  8888XJ5FR06720                                                               ";
				}
				return messageResponse;
			}
		}
		MQEDIMessage messageResponse;

		#endregion

		#endregion
	}
}
