using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EDICommunicationsModeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMessagePurposeWarning()
		{
			var purpose = Factory.New<IEDIMessagePurpose>();
			purpose.EMP_Code = "GOB";
			purpose.EMP_Description = "Ping Pong";
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var mode1 = org.EDICommunicationsModes.AddNew();

			mode1.EK_MessagePurpose = "PIK";
			AssertHasWarning(mode1.EK_MessagePurposeInfo, EDICommunicationsModeValidation.GetInvalidPurposeCodeMessage().ToString
			());

			mode1.EK_MessagePurpose = "GOB";
			AssertNoWarning(mode1.EK_MessagePurposeInfo, EDICommunicationsModeValidation.GetInvalidPurposeCodeMessage().ToString());
		}

		public void TestRCVIsNotAllowedForEAdaptorOrEHub()
		{
			var commsMode = Factory.New<EDICommunicationsMode>();
			commsMode.EK_Module = "SHP";
			commsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			commsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			commsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

			CombineAssertions(delegate
			{
				AssertHasError(commsMode.EK_CommsDirectionInfo, "Only TRX (Transmit) can be used with Comm. Transport HUB");
				AssertHasError(commsMode.EK_CommunicationsTransportInfo, "HUB can only be used with Comm. Direction TRX (Transmit)");
			});

			commsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;

			CombineAssertions(delegate
			{
				AssertNoErrors(commsMode.EK_CommsDirectionInfo);
				AssertNoErrors(commsMode.EK_CommunicationsTransportInfo);
			});

			commsMode.EK_CommsDirection = "";

			CombineAssertions(delegate
			{
				AssertNoErrors(commsMode.EK_CommsDirectionInfo);
				AssertNoErrors(commsMode.EK_CommunicationsTransportInfo);
			});

			commsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			commsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;

			CombineAssertions(delegate
			{
				AssertHasError(commsMode.EK_CommsDirectionInfo, "Only TRX (Transmit) can be used with Comm. Transport EDP");
				AssertHasError(commsMode.EK_CommunicationsTransportInfo, "EDP can only be used with Comm. Direction TRX (Transmit)");
			});

			commsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;

			CombineAssertions(delegate
			{
				AssertNoErrors(commsMode.EK_CommsDirectionInfo);
				AssertNoErrors("If eAdaptorNext is disabled should not show error on EDI Client field", commsMode.EK_ECC_CommunicationPartyConfigInfo);
				AssertHasError(commsMode.EK_CommunicationsTransportInfo, "Registry: eServices>eAdaptor>Outbound>Service URL must be specified when using EDP Transport");
			});

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				commsMode.Validation.ValidateAll();
				CombineAssertions(delegate
				{
					AssertNoErrors(commsMode.EK_CommsDirectionInfo);
					AssertNoErrors(commsMode.EK_CommunicationsTransportInfo);
				});

				commsMode.EK_CommsDirection = "";

				CombineAssertions(delegate
				{
					AssertNoErrors(commsMode.EK_CommsDirectionInfo);
					AssertNoErrors(commsMode.EK_CommunicationsTransportInfo);
				});

				commsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				commsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				commsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Receive;

				CombineAssertions(delegate
				{
					AssertNoErrors(commsMode.EK_CommsDirectionInfo);
					AssertNoErrors(commsMode.EK_CommunicationsTransportInfo);
				});
			}
		}

		public void TestEHubIsAllowedByDefault()
		{
			var commsMode = Factory.New<EDICommunicationsMode>();
			commsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			commsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			AssertNoErrors(commsMode.EK_CommunicationsTransportInfo);
		}

		public void TestEHubOrEAdaptorMustBeUsedWhenSendingUniversalXml()
		{
			var errorMessage = "Only [HUB], [EDP] or [XTT] can be selected when using Universal XML.";

			CombineAssertions(delegate
			{
				Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoErrors("XUE and HUB", Mode.EK_CommunicationsTransportInfo);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				AssertHasError("XUE and EMA", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateAll();
					AssertNoErrors("XUE and EDP", Mode.EK_CommunicationsTransportInfo);
				}
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertHasError("XUE and NXC", Mode.EK_CommunicationsTransportInfo, errorMessage);

				Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoErrors("XUS and HUB", Mode.EK_CommunicationsTransportInfo);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				AssertHasError("XUS and EMT", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateAll();
					AssertNoErrors("XUS and EDP", Mode.EK_CommunicationsTransportInfo);
				}
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				AssertHasError("XUS and FTP", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertHasError("XUS and NXC", Mode.EK_CommunicationsTransportInfo, errorMessage);

				Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoError("XML and HUB", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				AssertNoError("XML and EMT", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateAll();
					AssertNoErrors("XML and EDP", Mode.EK_CommunicationsTransportInfo);
				}
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				AssertNoError("XML and FTP", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertNoError("XML and NXC", Mode.EK_CommunicationsTransportInfo, errorMessage);

				Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoErrors("XUI and HUB", Mode.EK_CommunicationsTransportInfo);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				AssertHasError("XUI and EMT", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateAll();
					AssertNoErrors("XUI and EDP", Mode.EK_CommunicationsTransportInfo);
				}
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				AssertHasError("XUI and FTP", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertHasError("XUI and NXC", Mode.EK_CommunicationsTransportInfo, errorMessage);

				Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule;
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoErrors("XUL and HUB", Mode.EK_CommunicationsTransportInfo);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				AssertHasError("XUL and EMT", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateAll();
					AssertNoErrors("XUL and EDP", Mode.EK_CommunicationsTransportInfo);
				}
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				AssertHasError("XUL and FTP", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertHasError("XUL and NXC", Mode.EK_CommunicationsTransportInfo, errorMessage);

				Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransactionBatch;
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoErrors("XUX and HUB", Mode.EK_CommunicationsTransportInfo);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				AssertHasError("XUX and EMT", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateAll();
					AssertNoErrors("XUX and EDP", Mode.EK_CommunicationsTransportInfo);
				}
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				AssertHasError("XUX and FTP", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertHasError("XUX and NXC", Mode.EK_CommunicationsTransportInfo, errorMessage);

				Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalActivity;
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoErrors("XUA and HUB", Mode.EK_CommunicationsTransportInfo);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				AssertHasError("XUA and EMT", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateAll();
					AssertNoErrors("XUA and EDP", Mode.EK_CommunicationsTransportInfo);
				}
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
				AssertHasError("XUA and FTP", Mode.EK_CommunicationsTransportInfo, errorMessage);
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				AssertHasError("XUA and NXC", Mode.EK_CommunicationsTransportInfo, errorMessage);
			});
		}

		public void TestEK_CommunicationsTransport_UniversalXml_FileFormat_ALL()
		{
			var warning = "is not supported for Universal XML file formats.";

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			AssertNoWarningContaining("ALL and HUB", Mode.EK_CommunicationsTransportInfo, warning);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			AssertHasWarningContaining("ALL and EMT", Mode.EK_CommunicationsTransportInfo, warning);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			AssertNoWarningContaining("ALL and EDP", Mode.EK_CommunicationsTransportInfo, warning);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			AssertHasWarningContaining("ALL and EMA", Mode.EK_CommunicationsTransportInfo, warning);
		}

		public void TestEK_CommunicationsTransportWontAllowNativeXMLConnectorUnlessLicenced()
		{
			Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector = true;
			AssertEquals("Precondition: NativeXMLConnectorLicence.IsRegisteredAndNotExpired", true, Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
			AssertNoErrors(Mode.EK_CommunicationsTransportInfo);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			AssertNoErrors(Mode.EK_CommunicationsTransportInfo);

			Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector = false;
			AssertEquals("Precondition: NativeXMLConnectorLicence.IsRegisteredAndNotExpired", false, Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
			AssertHasErrorContaining(Mode.EK_CommunicationsTransportInfo, DeliveryFactory.NativeXMLConnectorPermissionFailureMessage);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			AssertNoErrors(Mode.EK_CommunicationsTransportInfo);
		}

		public void TestCheckEK_DestinationForNativeXMLConnector()
		{
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;

			Mode.EK_Destination = "Invalid URI String";
			AssertHasErrors(Mode.EK_DestinationInfo);

			Mode.EK_Destination = "999.999.999.999";
			AssertHasErrors(Mode.EK_DestinationInfo);

			Mode.EK_Destination = "10.0.0.0";
			AssertHasErrors(Mode.EK_DestinationInfo);

			Mode.EK_Destination = "ftp://10.0.0.0";
			AssertHasErrors(Mode.EK_DestinationInfo);

			Mode.EK_Destination = "http://10.0.0.0";
			AssertNoErrors(Mode.EK_DestinationInfo);
		}

		public void TestCommunicationsTransportFTP()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			Mode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
			AssertNoErrors("TransportInfo should have errors", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_Module = EDICommunicationsMode.Modules.ClientSpecific;
			AssertNoErrors("TransportInfo should no longer have errors", Mode.EK_CommunicationsTransportInfo);
		}

		public void TestCommunicationsTransportWhenFileFormatNTF()
		{
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			AssertHasErrors("TransportInfo should have errors", Mode.EK_CommunicationsTransportInfo);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			AssertNoErrors("TransportInfo should have no errors", Mode.EK_CommunicationsTransportInfo);
		}

		public void TestCommunicationsTransportWhenFileFormatOCI()
		{
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.OrderImportReport;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			AssertHasErrors("TransportInfo should have errors", Mode.EK_CommunicationsTransportInfo);
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			AssertNoErrors("TransportInfo should have no errors", Mode.EK_CommunicationsTransportInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTmpOrTempPath", Justification = "Path isn't actually used... just simulating validation")]
		public void TestCheckEK_Destination()
		{
			Mode.EK_CommunicationsTransport = "EMT";
			Mode.EK_Destination = "art_couldnt_think_of_a_funny_email_address";
			AssertHasErrors("EK_Destination should have errors", Mode.EK_DestinationInfo);

			Mode.EK_Destination = "art.minareci@cargowise.com";
			AssertNoErrors("EK_DestinationInfo should not have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "FIL";
			Mode.EK_Destination = new string(System.IO.Path.GetInvalidPathChars());
			AssertHasErrors("EK_DestinationInfo should have errors", Mode.EK_DestinationInfo);

			Mode.EK_Destination = @"X:\temp";
			AssertNoErrors("EK_DestinationInfo should not have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;
			Mode.EK_Destination = @"Whatever";
			AssertHasError(Mode.EK_DestinationInfo, "URI must start with ftp://");
			Mode.EK_Destination = @"ftp://www.HungarianReference.com";
			AssertNoError(Mode.EK_DestinationInfo, "URI must start with ftp://");
		}

		public void TestCheckEK_Destination_BlankEmail()
		{
			Mode.EK_CommunicationsTransport = "EMT";
			Mode.EK_Destination = "";
			AssertHasErrors("EK_DestinationInfo should have errors", Mode.EK_DestinationInfo);
		}

		public void TestCheckEK_Destination_BlankDirectory()
		{
			Mode.EK_CommunicationsTransport = "FIL";
			Mode.EK_Destination = "";
			AssertHasErrors("EK_DestinationInfo should have errors", Mode.EK_DestinationInfo);
		}

		public void TestCheckEK_Destination_Code()
		{
			Mode.EK_CommunicationsTransport = "HUB";
			Mode.EK_Destination = "";
			AssertHasErrors("EK_DestinationInfo should have errors", Mode.EK_DestinationInfo);
		}

		public void TestCheckEK_DestinationForFTPServerURL()
		{
			AssertExceptionThrown("This validation is for ZString Property only", typeof(DeveloperNotificationException),
				() => Mode.Validation.CheckEK_DestinationForFTPServerURL(Mode.EK_GGInfo));
		}

		public void TestCheckEK_Destination_DifferentTransport()
		{
			Mode.EK_CommunicationsTransport = "EMT";
			Mode.EK_Destination = "a@a.com";
			AssertNoErrors("EK_DestinationInfo should not have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "EMT";
			AssertNoErrors("EK_DestinationInfo should not have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "EMA";
			AssertNoErrors("EK_DestinationInfo should not have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "EML";
			AssertNoErrors("EK_DestinationInfo should not have errors", Mode.EK_DestinationInfo);

			Mode.EK_Destination = "a@a.com@com@com";
			Mode.EK_CommunicationsTransport = "EMT";
			AssertHasErrors("EK_DestinationInfo should have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "EML";
			AssertHasErrors("EK_DestinationInfo should have errors", Mode.EK_DestinationInfo);

			Mode.EK_CommunicationsTransport = "EMA";
			AssertHasErrors("EK_DestinationInfo should have errors", Mode.EK_DestinationInfo);
		}

		public void TestCheckEK_Module()
		{
			Mode.EK_ParentTableCode = "OH";
			Mode.EK_Module = "ere";
			AssertHasErrors("EK_ModuleInfo should have errors", Mode.EK_ModuleInfo);

			Mode.EK_Module = "SHP";
			AssertNoErrors("EK_ModuleInfo should not have errors", Mode.EK_ModuleInfo);
		}

		public void TestCheckEK_ModuleUniqueForCLI()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsMode mode = header.EDICommunicationsModes.AddNew();
			mode.EK_Module = "CLI";
			AssertNoErrors("EK_Module should have no errors when set to CLI first time", mode.EK_ModuleInfo);

			EDICommunicationsMode mode2 = header.EDICommunicationsModes.AddNew();
			mode2.EK_Module = "CLI";
			AssertHasErrors("EK_Module should have errors when set to 2nd CLI", mode2.EK_ModuleInfo);
		}

		public void TestCheckEK_ModuleForNet_OrgAsPayableAndReceivable()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader header = Factory.New<OrgHeader>();
			var orgCompanyData = Factory.New<OrgCompanyData>();
			orgCompanyData.OB_OH = header.PK;
			orgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			orgCompanyData.OB_IsCreditor = false;
			orgCompanyData.OB_IsDebtor = false;

			var mode = header.EDICommunicationsModes.AddNew();
			mode.EK_Module = EDICommunicationsMode.Modules.Netting;
			AssertHasError(mode.EK_ModuleInfo, "An organization participating in netting must be marked as both Payable and Receivable.");

			orgCompanyData.OB_IsCreditor = true;
			orgCompanyData.OB_IsDebtor = true;

			mode.EK_Module = EDICommunicationsMode.Modules.Netting;
			AssertNoError(mode.EK_ModuleInfo, "An organization participating in netting must be marked as both Payable and Receivable.");

			orgCompanyData.OB_IsCreditor = true;
			orgCompanyData.OB_IsDebtor = false;

			mode.EK_Module = EDICommunicationsMode.Modules.Netting;
			AssertHasError(mode.EK_ModuleInfo, "An organization participating in netting must be marked as both Payable and Receivable.");

			orgCompanyData.OB_IsCreditor = true;
			orgCompanyData.OB_IsDebtor = true;

			mode.EK_Module = EDICommunicationsMode.Modules.Netting;
			AssertNoError(mode.EK_ModuleInfo, "An organization participating in netting must be marked as both Payable and Receivable.");

			orgCompanyData.OB_IsCreditor = false;
			orgCompanyData.OB_IsDebtor = true;

			mode.EK_Module = EDICommunicationsMode.Modules.Netting;
			AssertHasError(mode.EK_ModuleInfo, "An organization participating in netting must be marked as both Payable and Receivable.");
		}

		public void TestCheckEK_ModuleNotUniqueForNonCLI()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			EDICommunicationsMode mode = header.EDICommunicationsModes.AddNew();
			mode.EK_Module = "SHP";
			AssertNoErrors("EK_Module should have no errors when set to non CLI first time", mode.EK_ModuleInfo);

			EDICommunicationsMode mode2 = header.EDICommunicationsModes.AddNew();
			mode2.EK_Module = "SHP";
			AssertNoErrors("EK_Module should have no errors when set to 2nd non CLI", mode.EK_ModuleInfo);
		}

		public void TestCheckEK_FileFormat()
		{
			Mode.EK_FileFormat = "art";
			AssertHasErrors("EK_FileFormatInfo should have errors", Mode.EK_FileFormatInfo);

			Mode.EK_FileFormat = "XML";
			AssertNoErrors("EK_FileFormatInfo should not have errors", Mode.EK_FileFormatInfo);

			Mode.EK_FileFormat = "ALL";
			AssertNoErrors("EK_FileFormatInfo should not have errors", Mode.EK_FileFormatInfo);

			Mode.EK_FileFormat = "NTF";
			AssertNoErrors("EK_FileFormatInfo should not have errors", Mode.EK_FileFormatInfo);

			Mode.EK_FileFormat = "art";
			AssertHasErrors("EK_FileFormatInfo should have errors", Mode.EK_FileFormatInfo);
		}

		public void TestCheckEK_CommunicationsTransport()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (Mode.GetValidationSuspender())
			{
				Mode.FillWithValidTestData();
				Mode.Organisation.FillWithValidTestData();
				Factory.Save();
			}

			Mode.EK_CommunicationsTransport = "xyz";
			AssertHasErrors("EK_CommunicationsTransportInfo should have errors", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_CommunicationsTransport = "FIL";
			AssertNoErrors("EK_CommunicationsTransportInfo should not have errors", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_CommunicationsTransport = "EMT";
			AssertNoErrors("EK_CommunicationsTransportInfo should not have errors", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_CommunicationsTransport = "EMA";
			AssertNoErrors("EK_CommunicationsTransportInfo should not have errors", Mode.EK_CommunicationsTransportInfo);

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;

			AssertEquals(true, Mode.EK_CommunicationsTransportInfo.HasChanges);

			Mode.EK_ECC_CommunicationPartyConfig = ZGuid.Empty;

			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			using (Mode.GetValidationSuspender())
			{
				Factory.Save();
			}

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			AssertNoErrors("EK_ECC_CommunicationPartyConfig should not have errors", Mode.EK_ECC_CommunicationPartyConfigInfo);
			AssertHasErrorContaining(Mode.EK_CommunicationsTransportInfo, "Registry: eServices>eAdaptor>Outbound>Service URL must be specified when using EDP Transport");
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				Mode.Validation.ValidateEK_CommunicationsTransport();
				AssertNoErrors("EK_CommunicationsTransport should not have errors", Mode.EK_CommunicationsTransportInfo);
			}
		}

		public void TestSHNModuleIsNotOK()
		{
			Mode.EK_ParentTableCode = "OH";
			Mode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
			AssertHasErrors("Shipnet is not valid for module", Mode.EK_ModuleInfo);
		}

		public void TestCommunicationsTransportClientIDIsValidated()
		{
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			AssertEquals("Precondition: Mode.EK_Destination", "", Mode.EK_Destination);
			AssertHasError(Mode.EK_DestinationInfo, "Please enter an eHub Client ID.");

			Mode.EK_Destination = "CodeShorterThan25Chars";
			AssertNoErrors(Mode.EK_DestinationInfo);

			Mode.EK_Destination = new string('z', 37);
			AssertHasError(Mode.EK_DestinationInfo, "eHub Client ID cannot be longer than 36 characters.");
		}

		public void TestCommunicationsTransportRecipientIDIsValidated()
		{
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			AssertEquals("Precondition: Mode.EK_Destination", "", Mode.EK_Destination);
			AssertHasErrorContaining(Mode.EK_DestinationInfo, "Please enter an eAdaptor Recipient ID.");

			Mode.EK_Destination = "CodeShorterThan25Chars";
			AssertNoErrors(Mode.EK_DestinationInfo);

			Mode.EK_Destination = "Thisistoolongforthefieldandshouldreturnanerror";
			AssertHasErrorContaining(Mode.EK_DestinationInfo, "eAdaptor Recipient ID cannot be longer than 25 characters.");
		}

		public void TestEK_TransportMode()
		{
			Mode.EK_TransportMode = "POO";
			AssertHasError(Mode.EK_TransportModeInfo, "Enter a valid Transport Mode.");
			Mode.EK_TransportMode = "AIR";
			AssertNoErrors(Mode.EK_TransportModeInfo);
		}

		public void TestEK_RecipientRole()
		{
			Mode.EK_RecipientRole = "POO";
			AssertHasError(Mode.EK_RecipientRoleInfo, "Enter a valid Recipient Role.");
			Mode.EK_RecipientRole = "ORP";
			AssertNoErrors(Mode.EK_RecipientRoleInfo);
		}

		public void TestEK_EventCode()
		{
			Mode.EK_EventCode = "POO";
			AssertHasError(Mode.EK_EventCodeInfo, "Enter a valid Event Code.");
			Mode.EK_EventCode = Events.CustomisableEvent01Code;
			AssertNoErrors(Mode.EK_EventCodeInfo);
		}

		public void TestEK_EventReferenceCondition_Empty()
		{
			Mode.EK_EventReferenceConditionType = "POO";
			AssertHasError(Mode.EK_EventReferenceConditionTypeInfo, "Enter a valid Event Reference Condition Type.");
			Mode.EK_EventReferenceConditionType = "";
			AssertNoErrors(Mode.EK_EventReferenceConditionTypeInfo);
			Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;
			AssertNoErrors(Mode.EK_EventReferenceConditionTypeInfo);
		}

		public void TestEK_EventReferenceCondition_MCR()
		{
			Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.ConditionWithMacros;
			AssertHasError(Mode.EK_EventReferenceConditionTypeInfo, "Enter a valid Event Reference Condition Type.");
		}

		public void TestEK_EventReferenceCondition_UDF()
		{
			Mode.EK_EventReferenceConditionType = EventReferenceConditionList.Codes.UserDefined;
			AssertHasError(Mode.EK_EventReferenceConditionTypeInfo, "Enter a valid Event Reference Condition Type.");
		}

		public void TestEK_EventReferenceCondition_WeThoughtOfEverything()
		{
			foreach (var code in new EventReferenceConditionList().GetAllCodes())
			{
				if (code != EventReferenceConditionList.Codes.ConditionWithMacros && code != EventReferenceConditionList.Codes.UserDefined)
				{
					AssertNoExceptionThrown(() => EDICommunicationsModeValidation.CheckReferenceConditionValue(Factory, code, Mode.EK_EventReferenceConditionValueInfo));
				}
			}
		}

		public void TestEK_EventReferenceConditionValue_CoupledToStmALogMaxLength()
		{
			AssertEquals(StmALogSchema.SL_Reference.MaxLength, EDICommunicationsModeSchema.EK_EventReferenceConditionValue.MaxLength);
		}

		EDICommunicationsMode Mode
		{
			get
			{
				if (mode == null)
				{
					OrgHeader org = Factory.New<OrgHeader>();
					mode = Factory.New<EDICommunicationsMode>();
					mode.EK_ParentID = org.PK;
					mode.EK_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
				}

				return mode;
			}
		}
		EDICommunicationsMode mode;

		public void TestCheckEK_DestinationShipnet()
		{
			ShipnetMode.EK_CommunicationsTransport = ZString.Empty;
			ShipnetMode.EK_Destination = ZString.Empty;
			AssertNoErrors("Mode.EK_DestinationInfo", ShipnetMode.EK_DestinationInfo);

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			ShipnetMode.EK_Destination = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_DestinationInfo, "Please enter an Email Address.");
			ShipnetMode.EK_Destination = "Blah";
			AssertNoErrorContaining(ShipnetMode.EK_DestinationInfo, "Please enter an Email Address.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			ShipnetMode.EK_Destination = ZString.Empty;
			AssertNoErrorContaining(ShipnetMode.EK_DestinationInfo, "Please enter a Destination Directory.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
			ShipnetMode.EK_Destination = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_DestinationInfo, "Please enter a Destination Directory.");
			ShipnetMode.EK_Destination = "Blah";
			AssertNoErrorContaining(ShipnetMode.EK_DestinationInfo, "Please enter a Destination Directory.");

			ShipnetMode.EK_CommunicationsTransport = ZString.Empty;
			ShipnetMode.EK_Destination = ZString.Empty;
			AssertNoErrors("Mode.EK_DestinationInfo", ShipnetMode.EK_DestinationInfo);
		}

		public void TestCheckEK_LoginName()
		{
			ShipnetMode.EK_CommunicationsTransport = ZString.Empty;
			ShipnetMode.EK_LoginName = ZString.Empty;
			AssertNoErrors("Mode.EK_LoginNameInfo", ShipnetMode.EK_LoginNameInfo);

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			ShipnetMode.EK_LoginName = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_LoginNameInfo, "Please enter an Username.");
			ShipnetMode.EK_LoginName = "Blah";
			AssertNoErrorContaining(ShipnetMode.EK_LoginNameInfo, "Please enter an Username.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			ShipnetMode.EK_LoginName = ZString.Empty;
			AssertNoErrors("Mode.EK_LoginNameInfo", ShipnetMode.EK_LoginNameInfo);
		}

		public void TestCheckEK_PortNumber()
		{
			ShipnetMode.EK_CommunicationsTransport = ZString.Empty;
			ShipnetMode.EK_PortNumber = 0;
			AssertNoErrors("Mode.EK_PortNumberInfo", ShipnetMode.EK_PortNumberInfo);

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			ShipnetMode.EK_PortNumber = 0;
			AssertHasErrorContaining(ShipnetMode.EK_PortNumberInfo, "Please enter a Port Number.");
			ShipnetMode.EK_PortNumber = 12;
			AssertNoErrorContaining(ShipnetMode.EK_PortNumberInfo, "Please enter a Port Number.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			ShipnetMode.EK_PortNumber = 0;
			AssertNoErrors("Mode.EK_PortNumberInfo", ShipnetMode.EK_PortNumberInfo);
		}

		public void TestCheckEK_Password()
		{
			ShipnetMode.EK_CommunicationsTransport = ZString.Empty;
			ShipnetMode.EK_Password = ZString.Empty;
			AssertNoErrors("Mode.EK_PasswordInfo", ShipnetMode.EK_PasswordInfo);

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			ShipnetMode.EK_Password = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_PasswordInfo, "Please enter a Password.");
			ShipnetMode.EK_Password = "Blah";
			AssertNoErrorContaining(ShipnetMode.EK_PasswordInfo, "Please enter a Password.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			ShipnetMode.EK_Password = ZString.Empty;
			AssertNoErrors("Mode.EK_PasswordInfo", ShipnetMode.EK_PasswordInfo);
		}

		public void TestCheckEK_Filename()
		{
			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			ShipnetMode.EK_Filename = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_FilenameInfo, "Please enter an Export File Name.");
			ShipnetMode.EK_Filename = "TESTFILE";
			AssertNoErrorContaining(ShipnetMode.EK_FilenameInfo, "Please enter an Export File Name.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			ShipnetMode.EK_Filename = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_FilenameInfo, "Please enter an Export File Name.");
			ShipnetMode.EK_Filename = "TESTFILE";
			AssertNoErrorContaining(ShipnetMode.EK_FilenameInfo, "Please enter an Export File Name.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
			ShipnetMode.EK_Filename = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_FilenameInfo, "Please enter an Export File Name.");
			ShipnetMode.EK_Filename = "TESTFILE";
			AssertNoErrorContaining(ShipnetMode.EK_FilenameInfo, "Please enter an Export File Name.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
			ShipnetMode.EK_Filename = "abc(*datetime*)def";
			AssertHasWarning(ShipnetMode.EK_FilenameInfo, "When specifying (*DateTime*), you must also specify other details about the job otherwise files may be overwritten if several are created at the same time.");
			ShipnetMode.EK_Filename = "tst(*DATETIME*)tst(*dateTime*)tst";
			AssertHasWarning(ShipnetMode.EK_FilenameInfo, "When specifying (*DateTime*), you must also specify other details about the job otherwise files may be overwritten if several are created at the same time.");
			ShipnetMode.EK_Filename = "Shipment(*DateTime*)Shipment(*abc*)tst";
			AssertNoWarning(ShipnetMode.EK_FilenameInfo, "When specifying (*DateTime*), you must also specify other details about the job otherwise files may be overwritten if several are created at the same time.");
		}

		public void TestCheckEK_ServerAddressSubject_ForShipnet()
		{
			ShipnetMode.EK_CommunicationsTransport = ZString.Empty;
			ShipnetMode.EK_ServerAddressSubject = ZString.Empty;
			AssertNoErrors("Mode.EK_ServerAddressSubjectInfo", ShipnetMode.EK_ServerAddressSubjectInfo);

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
			ShipnetMode.EK_ServerAddressSubject = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_ServerAddressSubjectInfo, "Please enter an Email Subject.");
			ShipnetMode.EK_ServerAddressSubject = "Blah";
			AssertNoErrorContaining(ShipnetMode.EK_ServerAddressSubjectInfo, "Please enter an Email Subject.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
			ShipnetMode.EK_ServerAddressSubject = ZString.Empty;
			AssertHasErrorContaining(ShipnetMode.EK_ServerAddressSubjectInfo, "Please enter a Server Address.");
			ShipnetMode.EK_ServerAddressSubject = "Blah";
			AssertNoErrorContaining(ShipnetMode.EK_ServerAddressSubjectInfo, "Please enter a Server Address.");

			ShipnetMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
			ShipnetMode.EK_ServerAddressSubject = ZString.Empty;
			AssertNoErrors("Mode.EK_ServerAddressSubjectInfo", ShipnetMode.EK_ServerAddressSubjectInfo);
		}

		public void TestCheckEK_ServerAddressSubject_ForOrderImportReport()
		{
			Mode.EK_Module = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.OrderImportReport;

			Mode.EK_ServerAddressSubject = "Subject";
			AssertHasErrors("EK_ServerAddressSubjectInfo should have errors when set, using OrderImportReport file format", Mode.EK_ServerAddressSubjectInfo);

			Mode.EK_ServerAddressSubject = "";
			AssertNoErrors("EK_ServerAddressSubjectInfo should not have errors when no subject is set", Mode.EK_ServerAddressSubjectInfo);

			Mode.EK_Module = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			Mode.EK_ServerAddressSubject = "Subject";
			AssertNoErrors("EK_ServerAddressSubjectInfo should not have errors when not using OrderImportReport", Mode.EK_ServerAddressSubjectInfo);
		}

		EDICommunicationsMode ShipnetMode
		{
			get
			{
				if (shipnetMode == null)
				{
					shipnetMode = Factory.New<EDICommunicationsMode>();
					shipnetMode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
				}
				return shipnetMode;
			}
		}
		EDICommunicationsMode shipnetMode;

		public void TestHUB_Incompatible_CommsDirection()
		{
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML; // make sure we're using compatible FileFormat
			Mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			AssertHasErrors("HUB+RCV is incompatible and requires error in EK_CommunicationsTransportInfo", Mode.EK_CommunicationsTransportInfo);
			AssertHasErrors("HUB+RCV is incompatible and requires error in EK_CommsDirectionInfo", Mode.EK_CommsDirectionInfo);
		}

		public void TestHUB_Compatible_CommsDirection()
		{
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML; // make sure we're using compatible FileFormat
			Mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			AssertNoErrors("HUB+TRX is compatible, EK_CommunicationsTransportInfo should be error free", Mode.EK_CommunicationsTransportInfo);
			AssertNoErrors("HUB+TRX is compatible, EK_CommsDirectionInfo should be error free", Mode.EK_CommsDirectionInfo);
		}

		public void TestHUB_FileFormat()
		{
			Mode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			var organisation = Mode.Organisation;
			organisation.OH_IsAirLine = true;
			organisation.OH_IsShippingProvider = true;

			Mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit; // make sure we're using compatible CommsDirection
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
			AssertHasErrorContaining(Mode.EK_CommunicationsTransportInfo, "HUB can only be used with the following File Formats:");
			AssertHasErrorContaining(Mode.EK_FileFormatInfo, "HUB can only be used with the following File Formats:");

			Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			Mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit; // make sure we're using compatible CommsDirection
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			AssertNoErrors("HUB+XML are compatible, EK_CommunicationsTransportInfo should be error free", Mode.EK_CommunicationsTransportInfo);
			AssertNoErrors("HUB+XML are compatible, EK_FileFormatInfo should be error free", Mode.EK_FileFormatInfo);

			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.FXL;
			AssertNoErrors("HUB+FXL are compatible, EK_CommunicationsTransportInfo should be error free", Mode.EK_CommunicationsTransportInfo);
			AssertNoErrors("HUB+FXL are compatible, EK_FileFormatInfo should be error free", Mode.EK_FileFormatInfo);

			Mode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			Mode.EK_ParentID = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Mode.EK_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			Mode.Organisation.OH_IsShippingProvider = true;
			Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.FHL;

			AssertNoErrors("HUB+FHL are compatible, EK_CommunicationsTransportInfo should be error free", Mode.EK_CommunicationsTransportInfo);
			AssertNoErrors("HUB+FHL are compatible, EK_FileFormatInfo should be error free", Mode.EK_FileFormatInfo);
		}

		public void TestCheckEK_ECC_CommunicationPartyConfig()
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();

			var mockFeatureData = new Mock<IFeatureData>();
			var mockFeatureControl = new Mock<IFeatureControlManager>();
			mockFeatureControl.Setup(f => f.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));

			using (ObjectFactory.Substitute(mockFeatureControl.Object))
			{
				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				Mode.EK_ECC_CommunicationPartyConfig = ZGuid.Empty;
				AssertHasError(Mode.EK_ECC_CommunicationPartyConfigInfo, "An EDI Client or Registry: eServices>eAdaptor>Outbound>Service URL must be specified when using EDP Transport");

				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				AssertNoErrors("Should not show validation error when a non eAdaptor interface transport mode is selected", Mode.EK_ECC_CommunicationPartyConfigInfo);

				Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				Mode.EK_ECC_CommunicationPartyConfig = party.OutboundConfig.PK;
				AssertNoErrors("No error should be shown when an active OutboundConfig is used", Mode.EK_ECC_CommunicationPartyConfigInfo);

				party.OutboundConfig.ECC_IsActive = false;
				Mode.Validation.ValidateEK_ECC_CommunicationPartyConfig();
				AssertEquals("There should not be duplicate notifications", 1, Mode.EK_ECC_CommunicationPartyConfigInfo.Notifications.Count());
				AssertHasError(Mode.EK_ECC_CommunicationPartyConfigInfo, "EDI Client outbound config must be active");

				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					Mode.Validation.ValidateEK_ECC_CommunicationPartyConfig();
					AssertHasError("Selected EDI Client should not have a disabled outbound config even if OutboundAdapterServiceUrl is specified", Mode.EK_ECC_CommunicationPartyConfigInfo, "EDI Client outbound config must be active");

					party.OutboundConfig.ECC_IsActive = true;
					Mode.Validation.ValidateEK_ECC_CommunicationPartyConfig();
					AssertNoErrors("No errors should be shown when OutboundAdapterServiceUrl is set to a non-blank value and an EDI Client with an active outbound config has been specified", Mode.EK_ECC_CommunicationPartyConfigInfo);

					Mode.EK_ECC_CommunicationPartyConfig = ZGuid.Empty;
					AssertNoErrors("No errors should be shown when OutboundAdapterServiceUrl is set a non-blank value with no specified EDI Client", Mode.EK_ECC_CommunicationPartyConfigInfo);
				}
			}
		}
	}
}
