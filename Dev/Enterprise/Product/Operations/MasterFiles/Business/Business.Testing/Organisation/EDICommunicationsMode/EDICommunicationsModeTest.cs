using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EDICommunicationsMode))]
	sealed class EDICommunicationsModeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEAdaptorAllowsYouToSelectAFileFormat()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = JobInvoicingConsumerTypes.Shipment.Code;
			mode.EK_CommsDirection = "TRX";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;

			CombineAssertions(delegate
			{
				AssertEquals("mode.EK_FileFormatInfo.ReadOnly", false, mode.EK_FileFormatInfo.ReadOnly);
				var fileFormatList = mode.Lookups.FileFormatList;
				string actualModeString = fileFormatList.GetHumanReadableListOfElements("\r\n");
				AssertNotEquals(-1, actualModeString.IndexOf("XML - XML"));
				AssertNotEquals(-1, actualModeString.IndexOf("SFF - Sterling Commerce Flat File"));
				AssertNotEquals(-1, actualModeString.IndexOf("NTF - Notification Email"));
				AssertNotEquals(-1, actualModeString.IndexOf("EXL - XML With eDoc"));
				AssertNotEquals(-1, actualModeString.IndexOf("DXL - XML With Descartes envelope"));
				AssertNotEquals(-1, actualModeString.IndexOf("XMB - Debtor Balance"));
				AssertNotEquals(-1, actualModeString.IndexOf("XUE - XML Universal Event"));
				AssertNotEquals(-1, actualModeString.IndexOf("XUS - XML Universal Shipment"));
				AssertNotEquals(-1, actualModeString.IndexOf("ALL - All"));
				AssertEquals("mode.EK_FileFormat - should not have changed", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, mode.EK_FileFormat);
			});
		}

		public void TestOrganizationNameAndCodeAreCorrect()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OHC";
			orgHeader.OH_FullName = $"{Guid.NewGuid()}";

			Factory.Save();

			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_ParentID = orgHeader.PK;

			AssertEquals("mode.EK_OH_Code", orgHeader.OH_Code, mode.EK_OH_Code);
			AssertEquals("mode.EK_OH_FullName", orgHeader.OH_FullName, mode.EK_OH_FullName);
		}

		public void TestNativeXMLConnectorAllowsYouToSelectAFileFormat()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = JobInvoicingConsumerTypes.Shipment.Code;
			mode.EK_CommsDirection = "TRX";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;

			CombineAssertions(delegate
			{
				AssertEquals("mode.EK_FileFormatInfo.ReadOnly", false, mode.EK_FileFormatInfo.ReadOnly);
				var fileFormatList = mode.Lookups.FileFormatList;
				AssertMultilineASCIIEquals("mode.Lookups.FileFormatList", @"
XML - XML
XUE - XML Universal Event
XUS - XML Universal Shipment
".Trim(), fileFormatList.GetHumanReadableListOfElements("\r\n"));

				AssertEquals("mode.EK_FileFormat - should not have changed", EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, mode.EK_FileFormat);
			});
		}

		public void TestNativeXMLConnectorTransportModeBlanksOutUnusedProperties()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Filename = "abc";
			mode.EK_ServerAddressSubject = "xxx";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.IFS;
			mode.EK_MessagePurpose = "DDD";

			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
			AssertEquals("EK_Filename", ZString.Empty, mode.EK_Filename);
			AssertEquals("EK_ServerAddressSubject", ZString.Empty, mode.EK_ServerAddressSubject);
			AssertEquals("EK_FileFormat", EDICommunicationsModeFileFormatList.Codes.XML, mode.EK_FileFormat);
			AssertEquals("EK_MessagePurpose", "DDD", mode.EK_MessagePurpose);

			AssertEquals("EK_Filename should be readonly on NativeXMLConnector", true, mode.EK_FilenameInfo.ReadOnly);
			AssertEquals("EK_ServerAddressSubject should be readonly on NativeXMLConnector", true, mode.EK_ServerAddressSubjectInfo.ReadOnly);
			AssertEquals("EK_FileFormat should be readonly on NativeXMLConnector", false, mode.EK_FileFormatInfo.ReadOnly);
			AssertEquals("EK_MessagePurpose should be readonly on NativeXMLConnector", false, mode.EK_MessagePurposeInfo.ReadOnly);
			AssertEquals("EK_LocalPartyVanID should be readonly on NativeXMLConnector", true, mode.EK_LocalPartyVanIDInfo.ReadOnly);
			AssertEquals("EK_RelatedPartyVanID should be readonly on NativeXMLConnector", true, mode.EK_RelatedPartyVanIDInfo.ReadOnly);
		}

		public void TestWhenUS_BIRD_DataTransfer()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_FileFormat = "AAA";
			mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Receive;

			mode.EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			AssertEquals(EDIMessageDelivery.ReplacementConstants.JobNumber + "_" + EDIMessageDelivery.ReplacementConstants.DateTime + "_" + EDIMessageDelivery.ReplacementConstants.Type + ".txt", mode.EK_Filename);
		}

		public void TestOrganisation()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			AssertNull(mode.Organisation);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var companyData = Factory.New<OrgCompanyData>();
			companyData.OB_OH = orgHeader.PK;
			mode.EK_ParentID = companyData.PK;
			AssertEquals(orgHeader, mode.Organisation);

			mode.EK_ParentID = orgHeader.PK;
			AssertEquals(orgHeader, mode.Organisation);
		}

		public void TestFTPCommModeDisablesFields()
		{
			var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;

			AssertEquals("field should be readonly on non FTP", true, mode.EK_FtpLockingMethodInfo.ReadOnly);
			AssertEquals("field should be readonly on non FTP", true, mode.EK_PortNumberInfo.ReadOnly);
			AssertEquals("field should be readonly on non FTP", true, mode.EK_LoginNameInfo.ReadOnly);
			AssertEquals("field should be readonly on non FTP", true, mode.EK_PasswordInfo.ReadOnly);
			AssertEquals("field should be readonly on non FTP", true, mode.EK_CertificateInfo.ReadOnly);

			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
			AssertEquals("field should not be readonly on non FTP", false, mode.EK_FtpLockingMethodInfo.ReadOnly);
			AssertEquals("field should not be readonly on non FTP", false, mode.EK_PortNumberInfo.ReadOnly);
			AssertEquals("field should not be readonly on non FTP", false, mode.EK_LoginNameInfo.ReadOnly);
			AssertEquals("field should not be readonly on non FTP", false, mode.EK_PasswordInfo.ReadOnly);
			AssertEquals("field should not be readonly on non FTP", false, mode.EK_CertificateInfo.ReadOnly);
		}

		public void TestHUBCommModeDisablesFields()
		{
			// By Leo Liang
			// When XXXInfo.Readonly is called, EDICommunicationsMode will call GetReadOnlySecurity,
			// And will throw a NullObjectException if EDICommunicationsMode do not have OrgHeader
			var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			mode.EK_ParentID = orgHeader.PK;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

			AssertEquals("field should be readonly on HUB", false, mode.EK_MessagePurposeInfo.ReadOnly);
			AssertEquals("field should be readonly on HUB", true, mode.EK_LocalPartyVanIDInfo.ReadOnly);
			AssertEquals("field should be readonly on HUB", true, mode.EK_RelatedPartyVanIDInfo.ReadOnly);
		}

		public void TestSaveResetsErrorTime()
		{
			var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			var stamp = DateTime.Now.AddHours(24);
			mode.EK_LastFailed = stamp;
			Factory.Save();
			Assert("should change stamp on save", mode.EK_LastFailed != stamp);
			mode.Delete();
		}

		public void TestTransportWithNTF()
		{
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			AssertNoErrors("No errors for EMA", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			AssertHasErrors("errors for EMA", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			AssertNoErrors("No errors for EMA", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			AssertNoErrors("No errors for EMA", Mode.EK_CommunicationsTransportInfo);
		}

		public void TestModuleChangesCommTransportLookups()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.CommunicationsTransportList.ContainsCode(ShipnetExportCommunicationsTransportMappingList.Codes.Email));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(ShipnetExportCommunicationsTransportMappingList.Codes.FTP));

			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface));

			Mode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(ShipnetExportCommunicationsTransportMappingList.Codes.Email));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(ShipnetExportCommunicationsTransportMappingList.Codes.FTP));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.CommunicationsTransportList.ContainsCode(ShipnetExportCommunicationsTransportMappingList.Codes.File));

			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment));
			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText));
			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector));
			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.CommunicationsTransportList.ContainsCode(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface));
		}

		public void TestModuleChangesChangeFileFormat()
		{
			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.FileFormatList.ContainsCode(""));

			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.All));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XML));
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.SterlingCommerceFlatFile));

			Mode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
			Assert("CommunicationsTransportList Lookups should contain item", Mode.Lookups.FileFormatList.ContainsCode(""));

			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.All));
			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.XML));
			Assert("CommunicationsTransportList Lookups should not contain item", !Mode.Lookups.FileFormatList.ContainsCode(EDICommunicationsModeFileFormatList.Codes.SterlingCommerceFlatFile));
		}

		public void TestSettingTransportTypeShouldNotEmptyMessagePurpose()
		{
			Mode.EK_MessagePurpose = "DDD";
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			AssertEquals("EK_MessagePurpose", "DDD", Mode.EK_MessagePurpose);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			AssertEquals("EK_MessagePurpose", "DDD", Mode.EK_MessagePurpose);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
			AssertEquals("EK_MessagePurpose", "DDD", Mode.EK_MessagePurpose);
		}

		public void TestSettingTransportTypeClearsErrorsForDestination()
		{
			Mode.EK_CommunicationsTransport = "FIL";
			Mode.EK_Destination = @"x:\NonExistentFolder";
			AssertNoErrors("EK_Destination should have no error", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "EMT";
			AssertHasErrors("EK_Destination should have an error", Mode.EK_DestinationInfo);
		}

		public void TestSettingTransportTypeSetsErrorForDestination()
		{
			Mode.EK_CommunicationsTransport = "EMT";
			Mode.EK_Destination = "a@a.com@a.com";
			AssertHasErrors("EK_Destination should have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "FIL";
			AssertNoErrors("EK_Destination should have errors cleared", Mode.EK_DestinationInfo);
		}

		public void TestSecurity()
		{
			bool defaultValue = Env.Security.OrgConfigModifyGeneral.IsAllowed;

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var collection = new EDICommunicationsModeDependentCollection(header);
			collection.AddNew();
			collection.AddNew();
			Factory.Save();

			try
			{
				Assert("Access Allowed EK_CommunicationsTransportInfo for - should Not be Readonly", !collection[0].EK_CommunicationsTransportInfo.ReadOnly);
				Assert("Access Allowed EK_CommunicationsTransportInfo- should Not be Readonly", !collection[1].EK_CommunicationsTransportInfo.ReadOnly);
				Assert("Access Allowed EK_DestinationInfo- should Not be Readonly", !collection[0].EK_DestinationInfo.ReadOnly);
				Assert("Access Allowed EK_DestinationInfo- should Not be Readonly", !collection[1].EK_DestinationInfo.ReadOnly);

				Env.Security.OrgConfigModifyGeneral.IsAllowed = false;

				Assert("Access NOT Allowed - EK_CommunicationsTransportInfo should be Readonly", collection[0].EK_CommunicationsTransportInfo.ReadOnly);
				Assert("Access NOT Allowed - EK_CommunicationsTransportInfo should be Readonly", collection[1].EK_CommunicationsTransportInfo.ReadOnly);
				Assert("Access NOT Allowed - EK_DestinationInfo should be Readonly", collection[0].EK_DestinationInfo.ReadOnly);
				Assert("Access NOT Allowed - EK_DestinationInfo should be Readonly", collection[1].EK_DestinationInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConfigModifyGeneral.IsAllowed = defaultValue;
				header.Delete();
				Factory.Save();
			}
		}

		public void TestAddressSubjectInfoReadOnlyWhenFile()
		{
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			Assert("EK_ServerAddressSubjectInfo should not be read-only", !Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			Assert("EK_ServerAddressSubjectInfo should not be read-only", !Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
			Assert("EK_ServerAddressSubjectInfo should be read-only", Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
			Assert("EK_ServerAddressSubjectInfo should be read-only", Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Assert("EK_ServerAddressSubjectInfo should not be read-only", !Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.FHL;
			Assert("EK_FileNameInfo should be read-only", Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.FWB;
			Assert("EK_FileNameInfo should be read-only", Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
			Mode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			Assert("EK_ServerAddressSubjectInfo should not be read-only", !Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
			Assert("EK_ServerAddressSubjectInfo should be read-only", Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			Assert("EK_ServerAddressSubjectInfo should not be read-only", !Mode.EK_ServerAddressSubjectInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = "";
			Assert("EK_ServerAddressSubjectInfo should be read-only", Mode.EK_ServerAddressSubjectInfo.ReadOnly);
		}

		public void TestCommTransportChangesFileNameAccess()
		{
			Mode.EK_Module = "";  //precondition
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			Assert("EK_FileNameInfo should be read-only", Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.FHL;
			Assert("EK_FileNameInfo should be read-only", Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.FWB;
			Assert("EK_FileNameInfo should be read-only", Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			Assert("EK_FileNameInfo should not be read-only", !Mode.EK_FilenameInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = "";
			Assert("EK_FileNameInfo should be read-only", Mode.EK_FilenameInfo.ReadOnly);
		}

		public void TestCommPartyIDsAccess()
		{
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
			Assert("EK_LocalPartyVanIDInfo should not be read-only", !Mode.EK_LocalPartyVanIDInfo.ReadOnly);
			Assert("EK_RelatedPartyVanIDInfo should not be read-only", !Mode.EK_RelatedPartyVanIDInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Assert("EK_LocalPartyVanIDInfo should not read-only", Mode.EK_LocalPartyVanIDInfo.ReadOnly);
			Assert("EK_RelatedPartyVanIDInfo should not read-only", Mode.EK_RelatedPartyVanIDInfo.ReadOnly);

			Mode.EK_CommunicationsTransport = "";
			Assert("EK_LocalPartyVanIDInfo should not be read-only", !Mode.EK_LocalPartyVanIDInfo.ReadOnly);
			Assert("EK_RelatedPartyVanIDInfo should not be read-only", !Mode.EK_RelatedPartyVanIDInfo.ReadOnly);
		}

		public void TestCommTransportClearsErrorForFileName()
		{
			Mode.EK_Filename = "";
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;

			AssertHasErrors("Empty file name should give error for SaveToFile", Mode.EK_FilenameInfo);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			AssertNoErrors("Empty file should not give error for EmailAsText", Mode.EK_FilenameInfo);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			AssertNoErrors("Empty file name should not give error for EmailAsAttachment", Mode.EK_FilenameInfo);
		}

		public void TestEK_EventReferenceConditionValue_ReadOnly()
		{
			Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReference;
			AssertEquals(false, Mode.EK_EventReferenceConditionValueInfo.ReadOnly);
			Mode.EK_EventReferenceConditionType = "POO";
			AssertEquals(true, Mode.EK_EventReferenceConditionValueInfo.ReadOnly);
		}

		EDICommunicationsMode Mode
		{
			get { return mode ?? (mode = Factory.New<EDICommunicationsMode>()); }
		}
		EDICommunicationsMode mode;
	}
}
