using System;
using CargoWise.Application;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.Testing
{
	class GlobalBusinessIdentifierMessageProcessorTest : ABIProcessorTest<GlobalBusinessIdentifierMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "TST";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "TS1";
			staff1.GS_LoginName = "TS1";
			staff1.GS_EmailAddress = "staff1@test.com";
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "TS2";
			staff2.GS_LoginName = "TS2";
			staff2.GS_EmailAddress = "staff2@test.com";
			Factory.Save();

			using (USCustomsDataRegistry.Instance.ABIMessagesGroup.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, false)))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TEST001";
				org.OH_FullName = "TEST COMP";
				org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUN111", Core.Constants.CountryCodes.UnitedStates);
				var secondAddress = org.Addresses.AddNew();
				secondAddress.OA_Address1 = "Address 1";
				secondAddress.OA_Address2 = "Address 2";
				secondAddress.OA_City = "Houston";
				secondAddress.OA_State = USStateList.Codes.Texas;
				secondAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				secondAddress.OA_PostCode = "7042";
				secondAddress.OA_Phone = "01011111111";
				secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "GLN222", Core.Constants.CountryCodes.UnitedStates);
				secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "LEI333", Core.Constants.CountryCodes.UnitedStates);

				var orgWrapper = OrgHeaderWrapper.New(org);
				var gbiData = new GlobalBusinessIdentifierData(orgWrapper);
				gbiData.US_OA_AddressDetails = secondAddress.PK;
				gbiData.US_WebsiteURL = "https://WWW.TEST.COM";
				gbiData.US_IsManufacturer = true;
				gbiData.US_IsShipper = true;
				gbiData.US_IsSeller = true;
				gbiData.US_IsExporter = true;
				gbiData.US_IsPackager = true;
				gbiData.US_IsDistributor = true;
				gbiData.SaveGlobalBusinessIdentifiers();

				var builder = new GlobalBusinessIdentifierMessageBuilder(gbiData, GlobalBusinessIdentifierMessageType.Original);
				var sentMessage = builder.Generate();
				sentMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent;
				sentMessage.EM_MessageNum = "EDIEDIDAT_1";
				Factory.Save();

				AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIAdd, gbiData.SubmissionStatus);

				var receivedMessage = Factory.New<MQEDIMessage>();
				receivedMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport;
				receivedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDeleteResponse;
				receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
				receivedMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Receive;
				receivedMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
				receivedMessage.EM_MessageText = "B  8888XJ5GX                                               EDIEDIDAT_1          " +
					"GE10ATEST COMP                                                                  " +
					"GE9011X11APPLICATION ID CODE MISSING                                            " +
					"GE9011X32Y-REC DOES NOT MATCH B-REC                                             " +
					"GE20DUNSDUN111                                                                  " +
					"GE20GLN GLN222                                                                  " +
					"GE20LEI LEI333                                                                  " +
					"GE21MFSHSEEXPKDR                                                                " +
					"GE30ADDRESS 1                                                                   " +
					"GE31ADDRESS 2                                                                   " +
					"GE32HOUSTON                            TX                      US7042           " +
					"GE4001011111111                                                                 " +
					"GE41HTTPS://WWW.TEST.COM                                                        " +
					"GE9001   GE DATA REJECTED                                                       " +
					"Y  8888XJ5GX";

				var logger = new LoggingInformation();
				var processor = new ABIMessageProcessorFactory(logger);
				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
				});
				AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, receivedMessage.EM_Status);
				AssertEquals(GBISubmissionStatusList.Codes.ErrorGBIAdd, gbiData.SubmissionStatus);
				AssertEquals(2, orgWrapper.Messages.Count);

				var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
				AssertEquals(1, emails.Count);

				var email = emails[0];
				var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(orgWrapper);
				AssertEquals("Global Business Identifier Enrollment Response (Failure) for TEST001", email.Subject);
				AssertContains("DUNS : DUN111 <br />GLN : GLN222 <br />LEI : LEI333 <br /><br />Firm Name : TEST COMP <br />Street Address : ADDRESS 1,ADDRESS 2 <br />City : HOUSTON <br />State : TX <br /><br />Zip/Postal Code : 7042 <br />ISO Country Code : US <br /><br />Phone : 01011111111 <br />Website URL : HTTPS://WWW.TEST.COM <br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Roles</th><th>&nbsp;</th></tr></thead><tr><td>Manufacturer</td><td>Y</td></tr><tr><td>Shipper</td><td>Y</td></tr><tr><td>Seller</td><td>Y</td></tr><tr><td>Exporter</td><td>Y</td></tr><tr><td>Packager</td><td>Y</td></tr><tr><td>Distributor</td><td>Y</td></tr></table><br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Data Reference</th><th>Condition Code</th><th>Message</th></tr></thead><tr><td>GE10</td><td>&nbsp;</td><td>ATEST COMP                                                                  </td></tr><tr><td>&nbsp;</td><td>X11</td><td>APPLICATION ID CODE MISSING</td></tr><tr><td>&nbsp;</td><td>X32</td><td>Y-REC DOES NOT MATCH B-REC</td></tr><tr><td>GE20</td><td>&nbsp;</td><td>DUNSDUN111                                                                  </td></tr><tr><td>GE20</td><td>&nbsp;</td><td>GLN GLN222                                                                  </td></tr><tr><td>GE20</td><td>&nbsp;</td><td>LEI LEI333                                                                  </td></tr><tr><td>GE21</td><td>&nbsp;</td><td>MFSHSEEXPKDR                                                                </td></tr><tr><td>GE30</td><td>&nbsp;</td><td>ADDRESS 1                                                                   </td></tr><tr><td>GE31</td><td>&nbsp;</td><td>ADDRESS 2                                                                   </td></tr><tr><td>GE32</td><td>&nbsp;</td><td>HOUSTON                            TX                      US7042           </td></tr><tr><td>GE40</td><td>&nbsp;</td><td>01011111111                                                                 </td></tr><tr><td>GE41</td><td>&nbsp;</td><td>HTTPS://WWW.TEST.COM                                                        </td></tr></table><br />", email.Body);

				AssertEquals(2, email.Recipients.Count);
				AssertEquals(true, email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals(true, email.Recipients.Contains(staff2.GS_EmailAddress));

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				receivedMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
				receivedMessage.EM_MessageText = "B  8888XJ5GX                                               EDIEDIDAT_1          " +
					"GE10ATEST COMP                                                                  " +
					"GE20DUNSDUN111                                                                  " +
					"GE20GLN GLN222                                                                  " +
					"GE20LEI LEI333                                                                  " +
					"GE21MFSHSEEXPKDR                                                                " +
					"GE30ADDRESS 1                                                                   " +
					"GE31ADDRESS 2                                                                   " +
					"GE32HOUSTON                            TX                      US7042           " +
					"GE4001011111111                                                                 " +
					"GE41HTTPS://WWW.TEST.COM                                                        " +
					"GE9002   GE DATA ACCEPTED                                                       " +
					"Y  8888XJ5GX";

				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
				});
				AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, receivedMessage.EM_Status);
				AssertEquals(GBISubmissionStatusList.Codes.ClearGBIAdd, gbiData.SubmissionStatus);
				AssertEquals(2, orgWrapper.Messages.Count);

				emails = Env.OutgoingCustomsMailManager.EmailsCreated;
				AssertEquals(1, emails.Count);

				email = emails[0];
				AssertEquals("Global Business Identifier Enrollment Response for TEST001", email.Subject);
				AssertContains("DUNS : DUN111 <br />GLN : GLN222 <br />LEI : LEI333 <br /><br />Firm Name : TEST COMP <br />Street Address : ADDRESS 1,ADDRESS 2 <br />City : HOUSTON <br />State : TX <br /><br />Zip/Postal Code : 7042 <br />ISO Country Code : US <br /><br />Phone : 01011111111 <br />Website URL : HTTPS://WWW.TEST.COM <br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Roles</th><th>&nbsp;</th></tr></thead><tr><td>Manufacturer</td><td>Y</td></tr><tr><td>Shipper</td><td>Y</td></tr><tr><td>Seller</td><td>Y</td></tr><tr><td>Exporter</td><td>Y</td></tr><tr><td>Packager</td><td>Y</td></tr><tr><td>Distributor</td><td>Y</td></tr></table><br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Data Reference</th><th>Condition Code</th><th>Message</th></tr></thead><tr><td>GE10</td><td>&nbsp;</td><td>ATEST COMP                                                                  </td></tr><tr><td>GE20</td><td>&nbsp;</td><td>DUNSDUN111                                                                  </td></tr><tr><td>GE20</td><td>&nbsp;</td><td>GLN GLN222                                                                  </td></tr><tr><td>GE20</td><td>&nbsp;</td><td>LEI LEI333                                                                  </td></tr><tr><td>GE21</td><td>&nbsp;</td><td>MFSHSEEXPKDR                                                                </td></tr><tr><td>GE30</td><td>&nbsp;</td><td>ADDRESS 1                                                                   </td></tr><tr><td>GE31</td><td>&nbsp;</td><td>ADDRESS 2                                                                   </td></tr><tr><td>GE32</td><td>&nbsp;</td><td>HOUSTON                            TX                      US7042           </td></tr><tr><td>GE40</td><td>&nbsp;</td><td>01011111111                                                                 </td></tr><tr><td>GE41</td><td>&nbsp;</td><td>HTTPS://WWW.TEST.COM                                                        </td></tr></table><br />", email.Body);

				AssertEquals(2, email.Recipients.Count);
				AssertEquals(true, email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals(true, email.Recipients.Contains(staff2.GS_EmailAddress));

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			}
		}

		public void TestGetGlobalIdentifiersAndEmailBodyNoExceptionThrown()
		{
			var receivedMessage = Factory.New<MQEDIMessage>();
			receivedMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport;
			receivedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDeleteResponse;
			receivedMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Receive;
			receivedMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
			receivedMessage.EM_MessageText = "B003901SV9GX                                               HYEDUSCMT_383755     " +
				"GE10DMY COMPANY TEST                                                            " +
				"GE20DUNS85684033TEST1                                                           " +
				"GE20GLN 07005401TEST1                                                           " +
				"GE20LEI 529900ZUTW57BUITEST1                                                    " +
				"GE9002   GE DATA ACCEPTED                                                       " +
				"Y  3901SV9GX00000";

			var logger = new LoggingInformation();
			var processor = new ABIMessageProcessorFactory(logger);
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(receivedMessage);
			});
		}

		public void TestNoOriginalMessage()
		{
			var receivedMessage = Factory.New<MQEDIMessage>();
			receivedMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport;
			receivedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDeleteResponse;
			receivedMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Receive;
			receivedMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
			receivedMessage.EM_MessageText = "B  8888XJ5GX                                               EDIEDIDAT_1          " +
				"GE10ATEST COMP                                                                  " +
				"GE9011X11APPLICATION ID CODE MISSING                                            " +
				"GE9011X32Y-REC DOES NOT MATCH B-REC                                             " +
				"GE20DUNSDUN111                                                                  " +
				"GE20GLN GLN222                                                                  " +
				"GE20LEI LEI333                                                                  " +
				"GE21MFSHSEEXPKDR                                                                " +
				"GE30ADDRESS 1                                                                   " +
				"GE31ADDRESS 2                                                                   " +
				"GE32HOUSTON                            TX                      US7042           " +
				"GE4001011111111                                                                 " +
				"GE41HTTPS://WWW.TEST.COM                                                        " +
				"GE9001   GE DATA REJECTED                                                       " +
				"Y  8888XJ5GX";

			var logger = new LoggingInformation();
			var processor = new ABIMessageProcessorFactory(logger);
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(receivedMessage);
			});
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, receivedMessage.EM_Status);
			AssertEquals(null, receivedMessage.EM_LinkedObject);

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			var email = emails[0];
			AssertEquals("Global Business Identifier Enrollment Response (Failure) for Unknown", email.Subject);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public void TestInvalidLinkedObject()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST001";

			var orgWrapper = OrgHeaderWrapper.New(org);
			var gbiData = new GlobalBusinessIdentifierData(orgWrapper);
			gbiData.US_DUNS = "DUN111";
			gbiData.US_GLN = "GLN222";
			gbiData.US_LEI = "LEI333";
			gbiData.US_FirmName = "TEST COMP";
			gbiData.US_Address1 = "Address 1";
			gbiData.US_Address2 = "Address 2";
			gbiData.US_City = "Houston";
			gbiData.US_State = USStateList.Codes.Texas;
			gbiData.US_PostCode = "7042";
			gbiData.US_Country = Core.Constants.CountryCodes.UnitedStates;
			gbiData.US_Phone = "01011111111";
			gbiData.US_WebsiteURL = "https://WWW.TEST.COM";
			gbiData.US_IsManufacturer = true;
			gbiData.US_IsShipper = true;
			gbiData.US_IsSeller = true;
			gbiData.US_IsExporter = true;
			gbiData.US_IsPackager = true;
			gbiData.US_IsDistributor = true;

			var builder = new GlobalBusinessIdentifierMessageBuilder(gbiData, GlobalBusinessIdentifierMessageType.Original);
			var sentMessage = builder.Generate();
			sentMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent;
			sentMessage.EM_MessageNum = "EDIEDIDAT_1";
			sentMessage.EM_LinkedObject = null;
			Factory.Save();

			var receivedMessage = Factory.New<MQEDIMessage>();
			receivedMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport;
			receivedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDeleteResponse;
			receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
			receivedMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Receive;
			receivedMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
			receivedMessage.EM_MessageText = "B  8888XJ5GX                                               EDIEDIDAT_1          " +
				"GE10ATEST COMP                                                                  " +
				"GE20DUNSDUN111                                                                  " +
				"GE20GLN GLN222                                                                  " +
				"GE20LEI LEI333                                                                  " +
				"GE21MFSHSEEXPKDR                                                                " +
				"GE30ADDRESS 1                                                                   " +
				"GE31ADDRESS 2                                                                   " +
				"GE32HOUSTON                            TX                      US7042           " +
				"GE4001011111111                                                                 " +
				"GE41HTTPS://WWW.TEST.COM                                                        " +
				"GE9002   GE DATA ACCEPTED                                                       " +
				"Y  8888XJ5GX";

			var logger = new LoggingInformation();
			var processor = new ABIMessageProcessorFactory(logger);
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(receivedMessage);
			});
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, receivedMessage.EM_Status);

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			var email = emails[0];
			AssertEquals("Global Business Identifier Enrollment Response for Unknown", email.Subject);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}
	}
}
