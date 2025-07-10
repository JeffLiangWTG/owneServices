using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class BatchSGCInterchangeSender40ScriptTest : TestCaseWithFactory
	{
		public void TestPackageAttachmentInterchange()
		{
			using (var dir = new TempDirectory())
			{
				var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				var ediMessage = Factory.New<SGEDIMessage>();
				ediInterchange.ContainedMessages.Add(ediMessage);
				var sender = new BatchSGCInterchangeSender40ScriptTestClass2(Factory);
				var fileName = sender.PackageAttachmentInterchangeExtend(ediInterchange, dir.DirectoryName, "TempInterchangeTextForTest");
				AssertEquals("Should return '.edi' as the file extension.", Path.Combine(dir.DirectoryName, "TempInterchangeTextForTest.edi"), fileName);
				ediInterchange.ContainedMessages.RemoveAndDeleteAll();
				var xmlMessage = Factory.New<SGXmlEDIMessage>();
				ediInterchange.ContainedMessages.Add(xmlMessage);
				fileName = sender.PackageAttachmentInterchangeExtend(ediInterchange, dir.DirectoryName, "TempInterchangeTextForTest");
				AssertEquals("Should return '.xml' as the file extension.", Path.Combine(dir.DirectoryName, "TempInterchangeTextForTest.xml"), fileName);
			}
		}

		public void TestEnvironmentInvalid()
		{
			Interchange.Factory.Save();
			var sender = new BatchSGCInterchangeSender40ScriptTestClass(false, true, true, Interchange);
			sender.ExecuteBatch();
			AssertEquals("Status", EDIInterchange.Status.Queued, Interchange.EI_Status);
			Assert("Not Exists", !File.Exists(Env.TempPath + "SG4Outbox\\v13t000\\2.edi"));
		}

		[TestDate(2012, 02, 07)]
		public void TestSendEDIFACTMessageInInterchange()
		{
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var message = CreateMessage<CUSDECEDIMessage>();
				var interchangeSender = new BatchSGCInterchangeSender40ScriptTestClass2(Factory);
				interchangeSender.PrepareInterchangesPublicTestMethod();
				var sg4Interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertNotNull("The interchange should have been created for this entry", sg4Interchange);
				AssertEquals("Application code is SG4", EDIInterchange.ApplicationCodes.SingaporeTradenet4, sg4Interchange.EI_ApplicationCode);
				AssertMultilineASCIIEquals("Body matches message", message.EM_MessageText, sg4Interchange.EI_BodyText);
				AssertEquals("Footer is UNZ", "UNZ+1+1'", sg4Interchange.EI_FooterText);
				AssertEquals("Senders Mailbox", "ASDF.ASDF23L", sg4Interchange.EI_From);
				AssertEquals("Header is UNA + UNB", "UNA:+.? 'UNB+UNOA:4+ASDF.ASDF23L:ZZ+DCS4.DCS4001:ZZ+20120207:0000+1++CUSDEC'", sg4Interchange.EI_HeaderText);
				AssertEquals("Interchange type", MessageTypeCodeList.Codes.INP, sg4Interchange.EI_InterchangeType);
				AssertEquals("Is transmit", EDIMessage.Direction.Transmit, sg4Interchange.EI_ReceiveTransmit);
				AssertEquals("Status is queued", CUSDECEDIMessage.Status.Queued, sg4Interchange.EI_Status);
				AssertEquals("Recieviers Mailbox", "DCS4.DCS4001", sg4Interchange.EI_To);
			}
		}

		[TestDate(2012, 02, 07)]
		public void TestSendXmlMessageInInterchange()
		{
			var message = CreateMessage<SGXmlEDIMessage>();
			var interchangeSender = new BatchSGCInterchangeSender40ScriptTestClass2(Factory);
			interchangeSender.PrepareInterchangesPublicTestMethod();
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertNotNull("The interchange should have been created for this entry", interchange);
			AssertEquals("Application code is SGX", EDIInterchange.ApplicationCodes.SGCustomsTradenetXML, interchange.EI_ApplicationCode);
			AssertEquals("Header is empty", string.Empty, interchange.EI_HeaderText);
			AssertEquals("Footer is empty", string.Empty, interchange.EI_FooterText);
			AssertEquals("Senders Mailbox", "ASDF.ASDF23L", interchange.EI_From);
			AssertEquals("Recieviers Mailbox", "DCS4.DCS4001", interchange.EI_To);
			AssertContains("Should populate sender ID in .", "<cbc:SenderID>ASDF.ASDF23L</cbc:SenderID>", interchange.EI_BodyText);
			AssertContains("Should populate recievier ID.", "<cbc:RecipientID>DCS4.DCS4001</cbc:RecipientID>", interchange.EI_BodyText);
			message.ReloadSafe();
			AssertMultilineASCIIEquals("Body matches message", message.EM_MessageText, interchange.EI_BodyText);
			AssertEquals("Interchange type", MessageTypeCodeList.Codes.INP, interchange.EI_InterchangeType);
			AssertEquals("Is transmit", EDIMessage.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("Status is queued", CUSDECEDIMessage.Status.Queued, interchange.EI_Status);
		}

		T CreateMessage<T>()
			where T : EDIMessage
		{
			var testSGBroker = Factory.New<GlbStaff>();
			testSGBroker.GS_Code = "TST";
			testSGBroker.GS_FullName = "Test SG4 Broker";
			var wrapper = SGGlbStaffWrapper.Get(testSGBroker);
			wrapper.Tradenetv4Password.GP_UserID = "ASDF23L";
			wrapper.Tradenetv4Password.GP_CurrentPassword = new TwoWayEncoder(testSGBroker.PK.ToGuid()).Encrypt("TRADENET");
			wrapper.Tradenetv4Password.GP_PasswordStatus = "OK";
			GlbStaff.CurrentUser.GS_Code = testSGBroker.GS_Code;
			var additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			var declaration = Factory.New<JobDeclaration>();
			declaration.AdditionalMessageInformation = additionalMessageInformation;
			additionalMessageInformation.SupportingDocuments.AddNew();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_GS_NKCusAgent = testSGBroker.GS_Code;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "87033120";
			invoiceLine.JI_LinePrice = 1000m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var messageManager = new MessageManagerTestClass(declaration);
			messageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
			CombineAssertions(() =>
			{
				AssertEquals(1, entryHeader.Messages.Count);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, declaration.JE_GS_NKCusAgent);
				AssertEquals(new ZDateTime(2012, 02, 07), declaration.JE_EntrySubmittedDate);
			}

			);
			var message = entryHeader.Messages[0];
			CombineAssertions(() =>
			{
				AssertType<T>("Message Type", message);
				AssertEquals("EM_Status", CUSDECEDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.INP, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", CUSDECEDIMessage.Declaration, message.EM_MessageSubType);
				AssertEquals("CH_Status", Core.SGConstants.DeclarationStatus.DeclarationPending, entryHeader.CH_Status);
			}

			);
			return message as T;
		}

		[TestDate(2020, 01, 17)]
		public void TestSendXMLGeneratedMessage()
		{
			var testSGBroker = Factory.New<GlbStaff>();
			testSGBroker.GS_Code = "TST";
			testSGBroker.GS_FullName = "Test SG4 Broker";
			var wrapper = SGGlbStaffWrapper.Get(testSGBroker);
			wrapper.Tradenetv4Password.GP_UserID = "ASDF23L";
			wrapper.Tradenetv4Password.GP_CurrentPassword = new TwoWayEncoder(testSGBroker.PK.ToGuid()).Encrypt("TRADENET");
			wrapper.Tradenetv4Password.GP_PasswordStatus = "OK";
			GlbStaff.CurrentUser.GS_Code = testSGBroker.GS_Code;
			var additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "TST";
			var declaration = Factory.New<JobDeclaration>();
			declaration.AdditionalMessageInformation = additionalMessageInformation;
			var supportingDocument = additionalMessageInformation.SupportingDocuments.AddNew();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_GS_NKCusAgent = testSGBroker.GS_Code;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "87033120";
			invoiceLine.JI_LinePrice = 1000m;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var messageManager = new MessageManagerTestClass(declaration);
			messageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entryHeader.Messages.Count);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, declaration.JE_GS_NKCusAgent);
			AssertEquals(new ZDateTime(2020, 01, 17), declaration.JE_EntrySubmittedDate);
			var interchangeSender = new BatchSGCInterchangeSender40ScriptTestClass2(Factory);
			interchangeSender.PrepareInterchangesPublicTestMethod();
			var xmlInterchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertNotNull("The interchange should have been created for this xml message entry", xmlInterchange);
			AssertEquals("Application code is SGX", EDIInterchange.ApplicationCodes.SGCustomsTradenetXML, xmlInterchange.EI_ApplicationCode);
			AssertEquals("Footer should not contain any Edifact string", "", xmlInterchange.EI_FooterText);
			AssertEquals("Senders Mailbox", "ASDF.ASDF23L", xmlInterchange.EI_From);
			AssertEquals("Header should not contain any Edifact string", "", xmlInterchange.EI_HeaderText);
			AssertEquals("Interchange type", MessageTypeCodeList.Codes.INP, xmlInterchange.EI_InterchangeType);
			AssertEquals("Is transmit", EDIMessage.Direction.Transmit, xmlInterchange.EI_ReceiveTransmit);
			AssertEquals("Status is queued", CUSDECEDIMessage.Status.Queued, xmlInterchange.EI_Status);
			AssertEquals("Receiviers Mailbox", "DCS4.DCS4001", xmlInterchange.EI_To);
		}

		[TestDate(2007, 1, 1, 10, 11, 12)]
		public void TestMultipleMessagesPopulateNewInterchanges()
		{
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var testSGBroker = Factory.New<GlbStaff>();
				testSGBroker.GS_Code = "TST";
				testSGBroker.GS_FullName = "Test SG4 Broker";
				var wrapper = SGGlbStaffWrapper.Get(testSGBroker);
				wrapper.Tradenetv4Password.GP_UserID = "E01T001";
				wrapper.Tradenetv4Password.GP_CurrentPassword = new TwoWayEncoder(testSGBroker.PK.ToGuid()).Encrypt("TRADENET");
				wrapper.Tradenetv4Password.GP_PasswordStatus = "OK";
				GlbStaff.CurrentUser.GS_Code = testSGBroker.GS_Code;
				var additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
				additionalMessageInformation.AM_Broker = "TST";
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.AdditionalMessageInformation = additionalMessageInformation;
				declaration1.JE_MessageType = MessageTypeCodeList.Codes.INP;
				declaration1.JE_GS_NKCusAgent = testSGBroker.GS_Code;
				var invoiceHeader = declaration1.Invoices.AddNew();
				invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "87033120";
				invoiceLine.JI_LinePrice = 1000m;
				declaration1.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration1.DoMerge();
				var messageManager = new MessageManagerTestClass(declaration1);
				messageManager.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				var entryHeader = (CusEntryHeader)declaration1.ActiveEntryHeaders[0];
				var message1 = (CUSDECEDIMessage)entryHeader.Messages[0];
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.AdditionalMessageInformation = additionalMessageInformation;
				declaration2.JE_MessageType = MessageTypeCodeList.Codes.COO;
				declaration2.JE_GS_NKCusAgent = testSGBroker.GS_Code;
				var invoiceHeader2 = declaration2.Invoices.AddNew();
				invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoiceHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "87033120";
				declaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration2.DoMerge();
				var messageManager2 = new MessageManagerTestClass(declaration2);
				messageManager2.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				var entryHeader2 = (CusEntryHeader)declaration2.ActiveEntryHeaders[0];
				var message2 = (CUSDECEDIMessage)entryHeader2.Messages[0];
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.AdditionalMessageInformation = additionalMessageInformation;
				declaration3.JE_MessageType = MessageTypeCodeList.Codes.OUT;
				declaration3.JE_GS_NKCusAgent = testSGBroker.GS_Code;
				var invoiceHeader3 = declaration3.Invoices.AddNew();
				invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoiceHeader3.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				var invoiceLine3 = invoiceHeader3.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "87033120";
				declaration3.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration3.DoMerge();
				var messageManager3 = new MessageManagerTestClass(declaration3);
				messageManager3.SendOriginalMessages(new SendsMessagesToCustomsShutterUpperer());
				var entryHeader3 = (CusEntryHeader)declaration3.ActiveEntryHeaders[0];
				var message3 = (CUSDECEDIMessage)entryHeader3.Messages[0];
				var interchangeSender = new BatchSGCInterchangeSender40ScriptTestClass2(Factory);
				interchangeSender.PrepareInterchangesPublicTestMethod();
				var interchanges = interchangeSender.Interchanges;
				AssertEquals("3 separate Interchanges should have been created", 3, interchanges.Count);
				var interchange = interchanges[0];
				AssertEquals("Interchange 1: Header", "UNA:+.? 'UNB+UNOA:4+E01T.E01T001:ZZ+DCST.DCST401:ZZ+20070101:1011+1++CUSDEC'", interchange.EI_HeaderText.ToString());
				AssertEquals("Interchange 1: Footer", "UNZ+1+1'", interchange.EI_FooterText.ToString());
				AssertEquals("DCST.DCST401", interchange.EI_To);
				AssertEquals("E01T.E01T001", interchange.EI_From);
				AssertEquals("Interchange body text must match message1", message1.EM_MessageText, interchange.EI_BodyText);
				interchange = interchanges[1];
				AssertEquals("Interchange 2: Header", "UNA:+.? 'UNB+UNOA:4+E01T.E01T001:ZZ+DCST.DCST401:ZZ+20070101:1011+2++TCODEC'", interchange.EI_HeaderText.ToString());
				AssertEquals("Interchange 2: Footer", "UNZ+1+2'", interchange.EI_FooterText.ToString());
				AssertEquals("DCST.DCST401", interchange.EI_To);
				AssertEquals("E01T.E01T001", interchange.EI_From);
				AssertEquals("Interchange body text must match message2", message2.EM_MessageText, interchange.EI_BodyText);
				interchange = interchanges[2];
				AssertEquals("Interchange 3: Header", "UNA:+.? 'UNB+UNOA:4+E01T.E01T001:ZZ+DCST.DCST401:ZZ+20070101:1011+3++CUSDEC'", interchange.EI_HeaderText.ToString());
				AssertEquals("Interchange 3: Footer", "UNZ+1+3'", interchange.EI_FooterText.ToString());
				AssertEquals("DCST.DCST401", interchange.EI_To);
				AssertEquals("E01T.E01T001", interchange.EI_From);
				AssertEquals("Interchange body text must match message3", message3.EM_MessageText, interchange.EI_BodyText);
			}
		}

		#region BatchSGCInterchangeSender40ScriptTestClass
		class BatchSGCInterchangeSender40ScriptTestClass : BatchSGCInterchangeSender40Script
		{
			public BatchSGCInterchangeSender40ScriptTestClass(bool isEnvironmentDataValidResult, bool testConnectionResult, bool uploadAndSubmitFileResult, EDIInterchange interchange)
			{
				this.isEnvironmentDataValidResult = isEnvironmentDataValidResult;
				this.testConnectionResult = testConnectionResult;
				this.interchange = interchange;
				this.uploadAndSubmitFileResult = uploadAndSubmitFileResult;
				this.hasAttachment = false;
				this.UploadCount = 0;
			}

			public BatchSGCInterchangeSender40ScriptTestClass(bool isEnvironmentDataValidResult, bool testConnectionResult, bool uploadAndSubmitFileResult, EDIInterchange interchange, bool hasAttachment) : this(isEnvironmentDataValidResult, testConnectionResult, uploadAndSubmitFileResult, interchange)
			{
				this.hasAttachment = hasAttachment;
			}

			public int UploadCount
			{
				get;
				set;
			}

			protected override bool UploadAndSubmitFile(bool isProduction, string fileName, string mailBox, EDIInterchange interchange)
			{
				UploadCount++;
				var contentID = interchange.EI_InterchangeNum;
				if (uploadAndSubmitFileResult)
				{
					if (hasAttachment && fileName.EndsWith("\\SG4Outbox\\v13t000\\3.zip"))
					{
						Assert("Exists", File.Exists(Env.TempPath + "SG4Outbox\\v13t000\\3.zip"));
						AssertZipFileContents2(Env.TempPath + "SG4Outbox\\v13t000\\3.zip");
						AssertEquals("3", contentID);
					}

					if (hasAttachment && fileName.EndsWith("\\SG4Outbox\\v13t000\\2.zip"))
					{
						Assert("Exists", File.Exists(Env.TempPath + "SG4Outbox\\v13t000\\2.zip"));
						AssertZipFileContents(Env.TempPath + "SG4Outbox\\v13t000\\2.zip");
						AssertEquals("2", contentID);
					}

					if (!hasAttachment && fileName.EndsWith("\\SG4Outbox\\v13t000\\2.zip"))
					{
						Assert("FileName", fileName.EndsWith("\\SG4Outbox\\v13t000\\2.edi"));
						Assert("Exists", File.Exists(Env.TempPath + "SG4Outbox\\v13t000\\2.edi"));
						AssertEquals("File Contents", this.interchange.EI_HeaderText + this.interchange.EI_BodyText + this.interchange.EI_FooterText, File.ReadAllText(Env.TempPath + "SG4Outbox\\v13t000\\2.edi"));
						AssertEquals("2", contentID);
					}

					AssertEquals("DCS4001", mailBox);
				}

				return uploadAndSubmitFileResult;
			}

			readonly bool uploadAndSubmitFileResult;
			readonly bool hasAttachment;
			void AssertZipFileContents(string fileName)
			{
				bool reportFound = false;
				bool report2Found = false;
				bool interchangeFound = false;
				using (FileStream zipStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				{
					using (ZipInputStream inputStream = new ZipInputStream(zipStream))
					{
						inputStream.IsStreamOwner = false;
						ZipEntry theEntry;
						while ((theEntry = inputStream.GetNextEntry()) != null)
						{
							if (theEntry.Name.Equals("report.txt", StringComparison.OrdinalIgnoreCase))
							{
								reportFound = true;
							}

							if (theEntry.Name.Equals("report2.txt", StringComparison.OrdinalIgnoreCase))
							{
								report2Found = true;
							}

							if (theEntry.Name.Equals("2.edi", StringComparison.OrdinalIgnoreCase))
							{
								interchangeFound = true;
							}
						}
					}
				}

				Assert("Report", reportFound);
				Assert("Report2", report2Found);
				Assert("Interchange", interchangeFound);
				Assert("Comment is in the file as ZipEntry has a bug where the comment is read out but is really there", File.ReadAllText(fileName).Contains("isPayLoad"));
			}

			void AssertZipFileContents2(string fileName)
			{
				bool testFound = false;
				bool interchangeFound = false;
				using (FileStream zipStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				{
					using (ZipInputStream inputStream = new ZipInputStream(zipStream))
					{
						inputStream.IsStreamOwner = false;
						ZipEntry theEntry;
						while ((theEntry = inputStream.GetNextEntry()) != null)
						{
							if (theEntry.Name.Equals("test.pdf", StringComparison.OrdinalIgnoreCase))
							{
								testFound = true;
							}

							if (theEntry.Name.Equals("3.edi", StringComparison.OrdinalIgnoreCase))
							{
								interchangeFound = true;
							}
						}
					}
				}

				Assert("Report", testFound);
				Assert("Interchange", interchangeFound);
				Assert("Comment is in the file as ZipEntry has a bug where the comment is read out but is really there", File.ReadAllText(fileName).Contains("isPayLoad"));
			}

			protected override bool TestConnection()
			{
				return testConnectionResult;
			}

			readonly bool testConnectionResult;
			protected override LoginCommand CheckBrokerMailbox(GlbStaff broker)
			{
				var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
				loginCommand.LoginState = LoginCommand.LoginStateType.LoggedIn;
				return loginCommand;
			}

			protected override bool IsEnvironmentDataValid()
			{
				return isEnvironmentDataValidResult;
			}

			readonly bool isEnvironmentDataValidResult;
			readonly EDIInterchange interchange;
		}

		#endregion
		#region Implementation
		protected override void TearDown()
		{
			base.TearDown();
			TempDirectory.DeleteDirectory(Path.Combine(Env.TempPath, SGBatchProcessorConstants.Directories.OuputDirectory));
		}

		#region Interchange
		EDIInterchange Interchange
		{
			get
			{
				if (interchange == null)
				{
					interchange = Factory.New<EDIInterchange>();
					interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.SingaporeTradenet4;
					interchange.EI_Status = EDIInterchange.Status.Queued;
					interchange.EI_From = Wrapper.Tradenetv4Password.GP_UserID.Left(4) + "." + Wrapper.Tradenetv4Password.GP_UserID;
					interchange.EI_To = EDIInterchange.InterchangePartyIDs.TradeNetV4LiveSystem;
					interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
					interchange.EI_HeaderText = "UNB+UNOA:4+PROD.DCST201OLDONE:ZZ+TEST1:ZZ+070621:1323+1++TCODEC'";
					interchange.EI_BodyText = "UNH+1+TCODEC:0:1:RT:040+COODEC'BGM+860:::COO+123456789012        200706210004+9'CST++NH'RFF+MS:TEST'TDT+20++1'DTM+136::102'GEI+5+:Y'LOC+11'LOC+36'LOC+166+ZA'NAD+AE+123456789012++EAGLE DATAMATION INTERNATIONAL PTE :LTD'NAD+DT++TEST'CTA+IC+:TEST'COM+234234234:TE'NAD+MF+++A.A.L. SHIPPING AGENCIES P/L'UNS+D'DMS++860'DOC+860:::1+1++1'CUX+2:ZAR'LIN++4'CST+1+01011000'MEA+AAF++NMB:12.0000'MEA+AAX++:0.0000'MOA+63:12.00'LOC+27'DTM+94::102'RFF+IV'DTM+3:20070619:102'CST++010110'UNS+S'CNT+5:1'UNT+32+1'";
					interchange.EI_FooterText = "UNZ+1+1'";
					interchange.EI_InterchangeNum = "2";
					interchange.EI_GB = MasterFiles.Business.GlbBranch.CurrentBranch.PK;
				}

				return interchange;
			}
		}

		EDIInterchange interchange;
		#endregion
		#region Broker
		SGGlbStaffWrapper Wrapper => wrapper ?? (wrapper = SGGlbStaffWrapper.Get(Broker));
		SGGlbStaffWrapper wrapper;
		GlbStaff Broker
		{
			get
			{
				if (broker == null)
				{
					broker = Factory.New<GlbStaff>();
					broker.GS_IsActive = true;
					Wrapper.Tradenetv4Password.GP_UserID = "v13t000";
					Wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Current";
					Wrapper.Tradenetv4Password.GP_PasswordStatus = Core.Constants.PasswordOK;
					broker.GS_Code = "ZAC";
					broker.GS_EmailAddress = "test1@hotmail.com";
				}

				return broker;
			}
		}

		GlbStaff broker;
		#endregion
		#region Test Class
		class MessageManagerTestClass : MessageManager
		{
			public MessageManagerTestClass(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override ZString MessageApplicationCode => ApplicationCodeList.Codes.SGCustomsTradenet4;
		}

		#endregion
		#endregion
		#region BatchSGCInterchangeSender40ScriptTestClass2
		class BatchSGCInterchangeSender40ScriptTestClass2 : BatchSGCInterchangeSender40Script
		{
			public BatchSGCInterchangeSender40ScriptTestClass2(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;
			public EDIInterchangeCollection Interchanges
			{
				get
				{
					var sgInterchangesQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SGConstants.TradeNetVersion.Four);
					sgInterchangesQuery.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.Queued);
					sgInterchangesQuery.OrderBy = EDIInterchangeSchema.Constants.EI_InterchangeNum;
					return new EDIInterchangeCollection(factory, sgInterchangesQuery);
				}
			}

			public string PackageAttachmentInterchangeExtend(EDIInterchange interchange, string outputDirectory, string fileNameNaked)
			{
				return base.PackageAttachmentInterchange(interchange, outputDirectory, fileNameNaked);
			}

			public void PrepareInterchangesPublicTestMethod()
			{
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var statusType = CUSDECEDIMessage.Status.Queued;
				foreach (var applicationCode in Helper.ApplicationCodes)
				{
					var filter = new ZQuery();
					filter.AddToFilter(EDIMessageSchema.EM_Status, statusType);
					filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
					filter.AddToFilter(EDIMessageSchema.EM_EI, null);
					filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, applicationCode);
					filter.AddToFilter(ValidBranchesForMessageFilter(new string[] { applicationCode }));
					filter.AddToFilter(ValidTransmitDateMessageFilter);
					filter.AddToFilter(AdditionalFilter);
					filter.IncludeBlob(EDIMessageSchema.EM_MessageText);
					filter.IncludeBlob(EDIMessageSchema.EM_MessageNText);
					filter.MaximumRows = NumberToBatch;
					filter.OrderBy = EDIMessage.Schema.EM_MessageNum;
					filter.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
					var readyMessages = new NonDependentEDIMessageCollection(factory);
					readyMessages.Load(filter);
					PackageMessagesIntoInterchanges(readyMessages);
				}

				factory.Save();
			}
		}
		#endregion
	}
}
