using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.Types;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Business.Test
{
	public class TelematicsNotifierTest : TestCaseWithFactory
	{
		public void TestTelematicsNotifierProvidesDeviceDetailsToTelematicsMiddlewareServices()
		{
			// Arrange
			// Act
			telematicsNotifier.NotifyTelematicsServicesOfDeviceDetails(Factory, new[] { "TEL1", "TEL2" }, "WTG-123", "IVU", "01020304", string.Empty, "JJLSYD123");

			// Assert
			var queuedMessages = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.Empty)).ToList();
			AssertEquals(2, queuedMessages.Count);
			var recipients = queuedMessages.Select(interchange => interchange.EI_To.ToString());
			AssertContainsExactElementsInAnyOrder(new[] { "TEL1", "TEL2" }, recipients);
		}

		public void TestTelematicsServicesMessage()
		{
			Test(
				"DeviceHardwareIdentifier=\"01020304\" DeviceHumanReadableIdentifier=\"WTG-123\" DeviceModel=\"IVU\" CargoWiseOneLicense=\"JJLSYD123\"",
				string.Empty,
				"JJLSYD123",
				string.Empty);
		}

		public void TestTelematicsDeviceReAssigned()
		{
			Test(
				"DeviceHardwareIdentifier=\"01020304\" DeviceHumanReadableIdentifier=\"WTG-123\" DeviceModel=\"IVU\" CargoWiseOneLicense=\"JJLSYD123\"",
				"<RevokeDevicesFromClients>\r\n\t\t\t<RevokeDeviceFromClient DeviceHardwareIdentifier=\"01020304\" DeviceHumanReadableIdentifier=\"WTG-123\" CargoWiseOneLicense=\"WISGLOSYD\" />\r\n\t\t</RevokeDevicesFromClients>",
				"JJLSYD123",
				"WISGLOSYD");
		}

		public void TestTelematicsNotifyWiseTechGlobalOfDeviceDetails()
		{
			telematicsNotifier.NotifyMobileServicesOfDeviceDetails(Factory, "MSCI0001", "WTG-123", false, "WTG", "M01", DeviceKind.Android, "01020304", "JJLSYD123");

			var queuedMessages = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.Empty)).ToList();
			AssertEquals(1, queuedMessages.Count);
			var recipients = queuedMessages.Select(interchange => interchange.EI_To.ToString());
			AssertContainsExactElementsInAnyOrder(new[] { "MSCI0001" }, recipients);
		}

		public void TestNotifyMobileServicesOfBYODRegistration()
		{
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EDIPRODID");

			telematicsNotifier.NotifyMobileServicesOfBYODRegistration(Factory, "SOMEDEVICE", "M01", DeviceKind.AppleMobile, "VAS003");

			var queuedInterchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.Empty)).ToList();
			AssertEquals(1, queuedInterchanges.Count);
			AssertContainsExactElementsInAnyOrder("EDIPRODID", queuedInterchanges[0].EI_To.ToString());

			AssertEquals(1, queuedInterchanges[0].ContainedMessages.Count);
			var ediMessage = queuedInterchanges[0].ContainedMessages[0];
			AssertEquals(TelematicsMessageList.Codes.ProtobufData, ediMessage.EM_MessageSubType);
			AssertEquals("<ProtobufData>EAEaJiIAAAAIoQESHQoKU09NRURFVklDRRIDTTAxGgoIARIGVkFTMDAz</ProtobufData>", ediMessage.EM_MessageText);
		}

		public void TestNotifyMobileServicesOfBYODDeregistration()
		{
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EDIPRODID");
			telematicsNotifier.NotifyMobileServicesOfBYODDeregistration(Factory, "SOMEDEVICE");

			var queuedInterchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.Empty)).ToList();
			AssertEquals(1, queuedInterchanges.Count);
			AssertContainsExactElementsInAnyOrder("EDIPRODID", queuedInterchanges[0].EI_To.ToString());

			AssertEquals(1, queuedInterchanges[0].ContainedMessages.Count);
			var ediMessage = queuedInterchanges[0].ContainedMessages[0];
			AssertEquals(TelematicsMessageList.Codes.ProtobufData, ediMessage.EM_MessageSubType);
			AssertEquals("<ProtobufData>EAEaFREAAAAIogESDAoKU09NRURFVklDRQ==</ProtobufData>", ediMessage.EM_MessageText);
		}

		[UseSnapshotProtection]
		public void TestTelematicsShouldNotifySynchronously()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var cdcTable = new CdcTable(GlbDeviceAssignmentDivotSchema.Constants.SqlSchemaName, GlbDeviceAssignmentDivotSchema.Constants.TableName);
				if (CdcDatabase.IsEnabled(adminConnection, Db.DatabaseName) && cdcTable.IsCdcEnabled(adminConnection))
				{
					cdcTable.DisableCdc(adminConnection, $"{GlbDeviceAssignmentDivotSchema.Constants.SqlSchemaName}_{GlbDeviceAssignmentDivotSchema.Constants.TableName}");
				}
				Assert(!cdcTable.IsCdcEnabled(adminConnection));
				Assert(telematicsNotifier.ShouldNotifySynchronously);
			}
		}

		[UseSnapshotProtection]
		public void TestTelematicsShouldNotifySynchronouslyIsFalse()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var cdcTable = new CdcTableForTesting(GlbDeviceAssignmentDivotSchema.Constants.SqlSchemaName, GlbDeviceAssignmentDivotSchema.Constants.TableName);
				if (!CdcDatabase.IsEnabled(adminConnection, Db.DatabaseName))
				{
					CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				}
				if (!cdcTable.IsCdcEnabled(adminConnection))
				{
					cdcTable.EnableCdc(adminConnection);
				}
				Assert(cdcTable.IsCdcEnabled(adminConnection));
				Assert(!telematicsNotifier.ShouldNotifySynchronously);
			}
		}

		void Test(string expectedAssignMessage, string expectedRevokeMessage, string deviceAssignedTo, string deviceWasAssignedTo)
		{
			// Arrange
			var licenceCode = Env.CurrentCompany.GetLicenceCode();

			// Act
			telematicsNotifier.NotifyTelematicsServicesOfDeviceDetails(Factory, new[] { "TEL1" }, "WTG-123", "IVU", "01020304", deviceWasAssignedTo, deviceAssignedTo);
			Factory.Save();

			// Assert
			var queuedInterchanges = Factory.Load<EDIInterchange>(new ZQuery()).ToList();
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, queuedInterchanges.Count);
			var ediInterchanges = queuedInterchanges
				.Where(interchange => interchange.EI_To != "MS123")
				.ToList();

			AssertEquals(1, ediInterchanges.Count);
			var ediInterchange = ediInterchanges.Single();
			AssertEquals(string.Empty, ediInterchange.EI_BodyText);
			AssertEquals("TEL1", ediInterchange.EI_To);
			AssertEquals(licenceCode, ediInterchange.EI_From);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, ediInterchange.EI_ReceiveTransmit);
			AssertEquals(ApplicationCodeList.Codes.Telematics, ediInterchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.Telematics, ediInterchange.EI_InterchangeType);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, ediInterchange.EI_Status);
			AssertEquals(EDIInterchangeTransportTypeList.Codes.eHub, ediInterchange.EI_TransportType);

			AssertEquals(1, ediMessages.Length);
			var ediMessage = ediMessages.Single(message => message.EM_MessageSubType == TelematicsMessageList.Codes.TelematicsXmlData);
			AssertXMLContains(expectedAssignMessage, ediMessage.EM_MessageText);
			AssertXMLContains(expectedRevokeMessage, ediMessage.EM_MessageText);
			AssertEquals(ApplicationCodeList.Codes.Telematics, ediMessage.EM_ApplicationCode);
			AssertEquals(TelematicsMessageList.Codes.TelematicsXmlData, ediMessage.EM_MessageSubType);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, ediMessage.EM_Status);
		}

		protected override void SetUp()
		{
			telematicsNotifier = new TelematicsNotifier();
			base.SetUp();
		}

		TelematicsNotifier telematicsNotifier;
	}
}
