using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
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
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class GlobalBusinessIdentifierStatusNotificationProcessorTest : ABIProcessorTest<GlobalBusinessIdentifierStatusNotificationProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		[TestDate(2023, 01, 23)]
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
				var dun = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUN111", Core.Constants.CountryCodes.UnitedStates);
				var secondAddress = org.Addresses.AddNew();
				secondAddress.OA_Address1 = "Address 1";
				secondAddress.OA_Address2 = "Address 2";
				secondAddress.OA_City = "Houston";
				secondAddress.OA_State = USStateList.Codes.Texas;
				secondAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				secondAddress.OA_PostCode = "7042";
				secondAddress.OA_Phone = "01011111111";
				var gln = secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "GLN222", Core.Constants.CountryCodes.UnitedStates);
				var lei = secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "LEI333", Core.Constants.CountryCodes.UnitedStates);

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
				sentMessage.EM_Status = EDIMessage.Status.Sent;
				sentMessage.EM_MessageNum = "EDIEDIDAT_1";
				sentMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIAdd, gbiData.SubmissionStatus);

				var receivedMessage = Factory.New<MQEDIMessage>();
				receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				receivedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.GBIReferenceStatusUpdate;
				receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				receivedMessage.EM_Status = EDIMessage.Status.Queued;
				receivedMessage.EM_MessageText =
					"B003901XJ5GO                                                                    " +
					"GO20DUNSDUN111                                                                  " +
					"GO300124231111010GBI IS VALID                                                   " +
					"GO20GLN GLN222                                                                  " +
					"GO300124231111020GBI NOT FOUND                                                  " +
					"GO20LEI LEI333                                                                  " +
					"GO300124231111030GBI IS INACTIVE                                                " +
					"GO900124231111001GBI ACCEPTED                                                   " +
					"Y  3901XJ5GO                                                                    ";

				var logger = new LoggingInformation();
				var processor = new ABIMessageProcessorFactory(logger);
				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
				});
				AssertEquals(EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierNotification, receivedMessage.EM_MessageSubType);
				AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);
				AssertEquals(2, orgWrapper.Messages.Count);
				AssertEquals("GBI ACCEPTED", "001", gbiData.GBIStatus);
				AssertEquals(true, dun.OrgCusCodeValidity.IsVerified);
				AssertEquals(false, gln.OrgCusCodeValidity.IsVerified);
				AssertEquals(false, lei.OrgCusCodeValidity.IsVerified);
				AssertEquals(receivedMessage.EM_MessageText, dun.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified);
				AssertEquals(ZString.Empty, gln.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified);
				AssertEquals(ZString.Empty, lei.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified);

				var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
				AssertEquals(1, emails.Count);

				var email = emails[0];
				var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(orgWrapper);
				AssertEquals("Global Business Identifier Status Notification Response for TEST001", email.Subject);
				AssertContains("DUNS : DUN111 <br />GLN : GLN222 <br />LEI : LEI333 <br /><br />Firm Name : TEST COMP <br />Street Address : ADDRESS 1,ADDRESS 2 <br />City : HOUSTON <br />State : TX <br /><br />Zip/Postal Code : 7042 <br />ISO Country Code : US <br /><br />Phone : 01011111111 <br />Website URL : HTTPS://WWW.TEST.COM <br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Roles</th><th>&nbsp;</th></tr></thead><tr><td>Manufacturer</td><td>Y</td></tr><tr><td>Shipper</td><td>Y</td></tr><tr><td>Seller</td><td>Y</td></tr><tr><td>Exporter</td><td>Y</td></tr><tr><td>Packager</td><td>Y</td></tr><tr><td>Distributor</td><td>Y</td></tr></table><br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>GBI Overall Status</th><th>Description</th></tr></thead><tr><td>001</td><td>GBI ACCEPTED</td></tr></table><br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>GBI Ref. ID Qualifier</th><th>GBI Ref. ID</th><th>Disposition Code</th><th>Description</th></tr></thead><tr><td>DUNS</td><td>DUN111</td><td>010</td><td>GBI IS VALID</td></tr><tr><td>GLN</td><td>GLN222</td><td>020</td><td>GBI NOT FOUND</td></tr><tr><td>LEI</td><td>LEI333</td><td>030</td><td>GBI IS INACTIVE</td></tr></table><br />\r\n", email.Body);

				AssertEquals(2, email.Recipients.Count);
				AssertEquals(true, email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals(true, email.Recipients.Contains(staff2.GS_EmailAddress));

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				receivedMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
				receivedMessage.EM_MessageText =
					"B003901XJ5GO                                                                    " +
					"GO20DUNSDUN111                                                                  " +
					"GO300124231111020GBI NOT FOUND                                                  " +
					"GO20GLN GLN222                                                                  " +
					"GO300124231111010GBI IS VALID                                                   " +
					"GO20LEI LEI333                                                                  " +
					"GO300124231111030GBI IS INACTIVE                                                " +
					"GO900124231111002GBI REJECTED                                                   " +
					"Y  3901XJ5GO                                                                    ";

				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
				});
				AssertEquals(EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierNotification, receivedMessage.EM_MessageSubType);
				AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);
				AssertEquals(2, orgWrapper.Messages.Count);
				AssertEquals("GBI REJECTED", "002", gbiData.GBIStatus);
				AssertEquals(false, dun.OrgCusCodeValidity.IsVerified);
				AssertEquals(true, gln.OrgCusCodeValidity.IsVerified);
				AssertEquals(false, lei.OrgCusCodeValidity.IsVerified);
				AssertEquals(ZString.Empty, dun.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified);
				AssertEquals(receivedMessage.EM_MessageText, gln.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified);
				AssertEquals(ZString.Empty, lei.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified);

				emails = Env.OutgoingCustomsMailManager.EmailsCreated;
				AssertEquals(1, emails.Count);

				email = emails[0];
				AssertEquals("Global Business Identifier Status Notification Response (Failure) for TEST001", email.Subject);
				AssertContains("DUNS : DUN111 <br />GLN : GLN222 <br />LEI : LEI333 <br /><br />Firm Name : TEST COMP <br />Street Address : ADDRESS 1,ADDRESS 2 <br />City : HOUSTON <br />State : TX <br /><br />Zip/Postal Code : 7042 <br />ISO Country Code : US <br /><br />Phone : 01011111111 <br />Website URL : HTTPS://WWW.TEST.COM <br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Roles</th><th>&nbsp;</th></tr></thead><tr><td>Manufacturer</td><td>Y</td></tr><tr><td>Shipper</td><td>Y</td></tr><tr><td>Seller</td><td>Y</td></tr><tr><td>Exporter</td><td>Y</td></tr><tr><td>Packager</td><td>Y</td></tr><tr><td>Distributor</td><td>Y</td></tr></table><br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>GBI Overall Status</th><th>Description</th></tr></thead><tr><td>002</td><td>GBI REJECTED</td></tr></table><br />", email.Body);
				AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>GBI Ref. ID Qualifier</th><th>GBI Ref. ID</th><th>Disposition Code</th><th>Description</th></tr></thead><tr><td>DUNS</td><td>DUN111</td><td>020</td><td>GBI NOT FOUND</td></tr><tr><td>GLN</td><td>GLN222</td><td>010</td><td>GBI IS VALID</td></tr><tr><td>LEI</td><td>LEI333</td><td>030</td><td>GBI IS INACTIVE</td></tr></table><br />", email.Body);

				AssertEquals(2, email.Recipients.Count);
				AssertEquals(true, email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals(true, email.Recipients.Contains(staff2.GS_EmailAddress));

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			}
		}

		public void TestCannotFindGlobalBusinessIdentifierData()
		{
			var receivedMessage = Factory.New<MQEDIMessage>();
			receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			receivedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.GBIReferenceStatusUpdate;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
			receivedMessage.EM_MessageText =
				"B003901SV9GO                                               HYEDUSCMT_383755     " +
				"GO20DUNS85684033TEST1                                                           " +
				"GO20GLN 07005401TEST1                                                           " +
				"GO20LEI 529900ZUTW57BUITEST1                                                    " +
				"GO900213231125003GBI DELETED                                                    " +
				"Y  3901SV9GO00000";

			var logger = new LoggingInformation();
			var processor = new ABIMessageProcessorFactory(logger);
			processor.ProcessMessage(receivedMessage);

			AssertEquals("ErrorReporter.LastMessageReported", "DUNS: 85684033TEST1, GLN: 07005401TEST1, LEI: 529900ZUTW57BUITEST1", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestVerificationStatusAndGBIStatusWhenNoGO30()
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
				var dun = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUN111", Core.Constants.CountryCodes.UnitedStates);
				var secondAddress = org.Addresses.AddNew();
				secondAddress.OA_Address1 = "Address 1";
				secondAddress.OA_Address2 = "Address 2";
				secondAddress.OA_City = "Houston";
				secondAddress.OA_State = USStateList.Codes.Texas;
				secondAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				secondAddress.OA_PostCode = "7042";
				secondAddress.OA_Phone = "01011111111";
				var gln = secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.GlobalLocationNumber, "GLN222", Core.Constants.CountryCodes.UnitedStates);
				var lei = secondAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "LEI333", Core.Constants.CountryCodes.UnitedStates);

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
				sentMessage.EM_Status = EDIMessage.Status.Sent;
				sentMessage.EM_MessageNum = "EDIEDIDAT_1";
				sentMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				var receivedMessage = Factory.New<MQEDIMessage>();
				receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				receivedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.GBIReferenceStatusUpdate;
				receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				receivedMessage.EM_Status = EDIMessage.Status.Queued;
				receivedMessage.EM_MessageText =
					"B003901XJ5GO                                                                    " +
					"GO20DUNSDUN111                                                                  " +
					"GO300124231111010GBI IS VALID                                                   " +
					"GO20GLN GLN222                                                                  " +
					"GO300124231111010GBI IS VALID                                                   " +
					"GO20LEI LEI333                                                                  " +
					"GO300124231111030GBI IS INACTIVE                                                " +
					"GO900124231111001GBI ACCEPTED                                                   " +
					"Y  3901XJ5GO                                                                    ";

				var logger = new LoggingInformation();
				var processor = new ABIMessageProcessorFactory(logger);
				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
				});

				AssertEquals("GBI ACCEPTED", "001", gbiData.GBIStatus);
				AssertEquals(true, dun.OrgCusCodeValidity.IsVerified);
				AssertEquals(true, gln.OrgCusCodeValidity.IsVerified);
				AssertEquals(false, lei.OrgCusCodeValidity.IsVerified);
				AssertEquals("VERIFIED", dun.OrgCusCodeValidity.VerificationStatus);
				AssertEquals("VERIFIED", gln.OrgCusCodeValidity.VerificationStatus);
				AssertEquals("NOT VERIFIED", lei.OrgCusCodeValidity.VerificationStatus);

				receivedMessage.EM_Status = EDIMessage.Status.Queued;
				receivedMessage.EM_MessageText =
					"B003901XJ5GO                                                                    " +
					"GO20DUNSDUN111                                                                  " +
					"GO20GLN GLN222                                                                  " +
					"GO20LEI LEI333                                                                  " +
					"GO900124231111003GBI DELETED                                                    " +
					"Y  3901XJ5GO                                                                    ";

				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
				});

				AssertEquals("GBI DELETED", "003", gbiData.GBIStatus);
				AssertEquals(false, dun.OrgCusCodeValidity.IsVerified);
				AssertEquals(false, gln.OrgCusCodeValidity.IsVerified);
				AssertEquals(false, lei.OrgCusCodeValidity.IsVerified);
				AssertEquals("NOT VERIFIED", dun.OrgCusCodeValidity.VerificationStatus);
				AssertEquals("NOT VERIFIED", gln.OrgCusCodeValidity.VerificationStatus);
				AssertEquals("NOT VERIFIED", lei.OrgCusCodeValidity.VerificationStatus);
			}
		}
	}
}
