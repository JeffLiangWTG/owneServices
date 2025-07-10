using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class SterlingCommerceMessageDeliveryTest : TestCaseWithFactory
	{
		MessageProcessorCommunicationModesResult GetModes(IList<IEDICommunicationsMode> modes)
		{
			return new MessageProcessorCommunicationModesResult(modes, null);
		}
		public void TestProcessDeliverMessages_SaveToFileWithoutConsol()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			ForwardingShipment testBizObj = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipmentValueObjectDataAdapter dataAdapter = new ForwardingShipmentValueObjectDataAdapter();

			SterlingCommerceMessageDelivery testSterlingCommerceMessageDelivery = new SterlingCommerceMessageDelivery(GetModes(new EDICommunicationsMode[] { mode }), testBizObj, dataAdapter, null);

			using (TempDirectory tempDir = new TempDirectory())
			{
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
				mode.EK_Destination = tempDir.DirectoryName;
				mode.EK_Filename = "TestProcessDeliverMessages.txt";
				mode.EK_LocalPartyVanID = "LocalVanId";
				mode.EK_RelatedPartyVanID = "RelatedVanId";
				Factory.Save();
				MemoryStream xmlStreamFromBizObj = GetExpectedBizObjXmlStream(testBizObj, dataAdapter, mode);
				byte[] flatFileBytesFromBiZO = GetExpectedFlatFileBytes(xmlStreamFromBizObj, dataAdapter);

				testSterlingCommerceMessageDelivery.Process(Notifications);
				Factory.Save();

				string fileName = Path.Combine(mode.EK_Destination, mode.EK_Filename);
				AssertEquals("Should create the target file", true, File.Exists(fileName));

				string flatFileStringFromBizO = System.Text.Encoding.UTF8.GetString(flatFileBytesFromBiZO).Trim();
				string[] stringsFromBizO = flatFileStringFromBizO.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				string[] stringsFromFromFile = File.ReadAllLines(fileName);
				Assert(stringsFromFromFile.Length > 1);
				CompareFileContent(stringsFromFromFile, stringsFromBizO);
				File.Delete(fileName);
			}
		}

		public void TestProcessDeliverMessages_SaveToFile()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			ForwardingConsol testBizObj = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testBizObj.Shipments.Add(shipment);
			ForwardingConsolWithShipmentValueObjectDataAdapter dataAdapter = new ForwardingConsolWithShipmentValueObjectDataAdapter(shipment);

			SterlingCommerceMessageDelivery testSterlingCommerceMessageDelivery = new SterlingCommerceMessageDelivery(GetModes(new EDICommunicationsMode[] { mode }), testBizObj, dataAdapter, null);

			using (TempDirectory tempDir = new TempDirectory())
			{
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
				mode.EK_Destination = tempDir.DirectoryName;
				mode.EK_Filename = "TestProcessDeliverMessages.txt";
				mode.EK_LocalPartyVanID = "LocalVanId";
				mode.EK_RelatedPartyVanID = "RelatedVanId";
				Factory.Save();
				MemoryStream xmlStreamFromBizObj = GetExpectedBizObjXmlStream(testBizObj, dataAdapter, mode);
				byte[] flatFileBytesFromBiZO = GetExpectedFlatFileBytes(xmlStreamFromBizObj, dataAdapter);

				testSterlingCommerceMessageDelivery.Process(Notifications);
				Factory.Save();

				string fileName = Path.Combine(mode.EK_Destination, mode.EK_Filename);
				AssertEquals("Should create the target file", true, File.Exists(fileName));

				string flatFileStringFromBizO = System.Text.Encoding.UTF8.GetString(flatFileBytesFromBiZO).Trim();
				string[] stringsFromBizO = flatFileStringFromBizO.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				string[] stringsFromFromFile = File.ReadAllLines(fileName);

				CompareFileContent(stringsFromFromFile, stringsFromBizO);
				File.Delete(fileName);
			}
		}

		public void TestProcessDeliverMessages_EmailAsAttachment()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			ForwardingConsol testBizObj = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testBizObj.Shipments.Add(shipment);
			ForwardingConsolWithShipmentValueObjectDataAdapter dataAdapter = new ForwardingConsolWithShipmentValueObjectDataAdapter(shipment);

			SterlingCommerceMessageDelivery testSterlingCommerceMessageDelivery = new SterlingCommerceMessageDelivery(GetModes(new EDICommunicationsMode[] { mode }), testBizObj, dataAdapter, null);

			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@test.com";
			mode.EK_LocalPartyVanID = "LocalVanId";
			mode.EK_RelatedPartyVanID = "RelatedVanId";

			Factory.Save();
			MemoryStream xmlStreamFromBizObj = GetExpectedBizObjXmlStream(testBizObj, dataAdapter, mode);
			byte[] flatFileBytesFromBiZO = GetExpectedFlatFileBytes(xmlStreamFromBizObj, dataAdapter);

			testSterlingCommerceMessageDelivery.Process(Notifications);
			Factory.Save();

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should send it to correct recepient", true, createdEmail.Recipients.Contains(mode.EK_Destination));
			AssertEquals("Should send it to correct recepients", 1, createdEmail.Recipients.Count);
			AssertEquals("Attachment name", mode.EK_Filename, createdEmail.Attachments[0].DisplayName);

			string flatFileStringFromBizO = System.Text.Encoding.UTF8.GetString(flatFileBytesFromBiZO).Trim();
			string[] stringsFromBizO = flatFileStringFromBizO.Split(new string[] { "\r\n" }, StringSplitOptions.None);

			string flatFileStringFromFile = System.Text.Encoding.UTF8.GetString(createdEmail.Attachments[0].Data).Trim();
			string[] stringsFromFromFile = flatFileStringFromFile.Split(new string[] { "\r\n" }, StringSplitOptions.None);

			CompareFileContent(stringsFromFromFile, stringsFromBizO);
		}

		void CompareFileContent(string[] stringsFromFromFile, string[] stringsFromBizO)
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertEquals("Generated file should have event record", stringsFromFromFile.Length, stringsFromBizO.Length + 1);

			int differences = 0;
			int sameStrings = 0;
			foreach (string stringFromFile in stringsFromFromFile)
			{
				bool stringWasFound = false;
				foreach (string stringFromBizO in stringsFromBizO)
				{
					if (stringFromBizO == stringFromFile)
					{
						stringWasFound = true;
						sameStrings++;
						break;
					}
				}
				if (!stringWasFound)
				{
					differences++;
				}
			}

			AssertEquals("Should be only one difference between BizOString and FileString - event", 1, differences);
			AssertEquals("All strings from bizO should be in file", stringsFromBizO.Length, sameStrings);
		}

		#region Implementation

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		MemoryStream GetExpectedBizObjXmlStream(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, EDICommunicationsMode mode)
		{
			MemoryStream result = new MemoryStream();

			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
			serializer.ExportXmlData(result, dataAdapter, new BusinessObject[] { bizObj }, new ValueObjectExportContext(Notifications), mode.EK_LocalPartyVanID, mode.EK_RelatedPartyVanID, "");
			result.Position = 0;

			return result;
		}

		byte[] GetExpectedFlatFileBytes(MemoryStream xmlStream, IValueObjectDataAdapter dataAdapter)
		{
			XmlTextReader reader = new XmlTextReader(xmlStream);

			Xsd.XmlInterchange interchange;
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.Consol consol = new Xsd.Consol();

			if (dataAdapter is ForwardingConsolWithShipmentValueObjectDataAdapter)
			{
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(Xsd.Consols));
				Xsd.ConsolCollection consols = ((Xsd.Consols)Xsd.XmlInterchange.DeserializeInterchangeAndPayload(reader, serializer, out interchange)).Consol;
				consol = consols[0];
				shipment = consol.Shipments[0];
			}
			else if (dataAdapter is ForwardingShipmentValueObjectDataAdapter)
			{
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(Xsd.Shipments));
				Xsd.ShipmentCollection shipments = ((Xsd.Shipments)Xsd.XmlInterchange.DeserializeInterchangeAndPayload(reader, serializer, out interchange)).Shipment;
				shipment = shipments[0];
			}
			else
			{
				interchange = Xsd.XmlInterchange.ReadInterchangeOnly(reader, null);
			}

			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(new BusinessObjectFactory(), consol, shipment, interchange.InterchangeInfo);
			MemoryStream result = new MemoryStream();
			sterling.Export(result);
			return result.ToArray();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#endregion
	}
}
