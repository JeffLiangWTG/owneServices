using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	sealed class CMDGeneratorTest : TestCaseWithFactory
	{
		public void TestInitialise()
		{
			ForwardingShipment shipment = CreateShipment("USCOL");
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			shipmentWrapper.ShowAll = true;
			AssertEquals("Precondition, ShowAll is set to true", true, shipmentWrapper.ShowAll);
			CMDGenerator generator = new CMDGenerator(shipmentWrapper);
			AssertEquals("Now the filter should be set to active only", true, shipmentWrapper.ShowActiveMessagesOnly);
			FieldInfo recipientField = typeof(CMDGenerator).GetField("recipient", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertEquals("Should be initialised to an empty string if not specified", "", recipientField.GetValue(generator));
			generator = new CMDGenerator(shipmentWrapper, "ASDF");
			AssertEquals("ASDF", recipientField.GetValue(generator));
			generator = new CMDGenerator(shipmentWrapper, "LONGONE");
			AssertEquals("Anything longer than 4 character should be trimmed", "LONG", recipientField.GetValue(generator));
		}

		public void TestGetCMDString()
		{
			ForwardingShipment shipment = CreateShipment("AUSYD");
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";
			Transport transport = consol.Transports[0];
			transport.JW_ATD = ZDateTime.Now;
			transport.JW_ATA = ZDateTime.Now;
			shipment.Consols.Add(consol);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			ZString expected = new CMD(consol, new CMDData(shipmentWrapper)).ToString();
			ZString generated = generator.BaseGetCMDString(consol);
			AssertEquals(false, expected.IsEmpty);
			AssertEquals(expected, generated);
		}

		public void TestInsertMessage()
		{
			using (SGCustomsDataRegistry.Instance.SendViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				GlbCompany.CurrentCompany.GC_Code = "GC";
				registrationKey.EnterpriseCodeForTest = "EC";
				registrationKey.ServerCodeForTest = "001";

				ForwardingShipment shipment = CreateShipment("AUSYD");
				var shipmentWrapper = new CMDShipmentWrapper(shipment);
				var generator = new DummyGenerator(shipmentWrapper);
				AssertEquals(0, shipmentWrapper.Messages.Count);
				generator.ExposedInsertMessage("CONSOL1", "001", "blablabla", "512-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(1, generator.NoOfMessagesSent);

				var message = shipmentWrapper.Messages[0];
				AssertEquals("blablabla", message.EM_MessageText);
				AssertEquals("001", message.EM_MessageSubType);
				AssertEquals("CONSOL1", message.EM_ApplicationReference);
				AssertEquals(EDIMessage.Status.Queued, message.EM_Status);

				Assert("There should be an interchange for this message", !message.EM_EI.IsEmpty);
				var interchange = Factory.Load<EDIInterchange>(message.EM_EI);
				AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EC001", interchange.EI_From);
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("CMD\r\nRJSYD\r\n", interchange.EI_HeaderText);
				AssertEquals("CCN CMD Processor", interchange.EI_To);
				AssertEquals(message.EM_ApplicationCode, interchange.EI_ApplicationCode);
				AssertEquals(message.EM_ApplicationCode, interchange.EI_InterchangeType);
				AssertEquals(message.EM_MessageText, interchange.EI_BodyText);
				AssertEquals(((char)4).ToString(), interchange.EI_FooterText);

				registrationKey.EnterpriseCodeForTest = "";
				shipmentWrapper.Messages.RemoveAll();
				AssertEquals(0, shipmentWrapper.Messages.Count);
				generator.ExposedInsertMessage("CONSOL1", "TDB", "blablabla", "512-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(2, generator.NoOfMessagesSent);

				message = shipmentWrapper.Messages[0];
				Assert("There should be an interchange for this message", !message.EM_EI.IsEmpty);
				interchange = Factory.Load<EDIInterchange>(message.EM_EI);
				AssertEquals("GC001", interchange.EI_From);
				AssertEquals("CMD\r\nTDB\r\n", interchange.EI_HeaderText);
				shipmentWrapper.Messages.RemoveAll();
				AssertEquals(0, shipmentWrapper.Messages.Count);
				generator.ExposedInsertMessage("CONSOL1", "GHA", "blablabla", "081-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(3, generator.NoOfMessagesSent);

				message = shipmentWrapper.Messages[0];
				Assert("There should be an interchange for this message", !message.EM_EI.IsEmpty);
				interchange = Factory.Load<EDIInterchange>(message.EM_EI);
				AssertEquals("GC001", interchange.EI_From);
				AssertEquals("CMD\r\nQFSYD\r\n", interchange.EI_HeaderText);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestInsertMessageWhenSGCMDIsEnabled()
		{
			using (SGCustomsDataRegistry.Instance.SendViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				GlbCompany.CurrentCompany.GC_Code = "GC";
				registrationKey.EnterpriseCodeForTest = "EC";
				registrationKey.ServerCodeForTest = "001";

				ForwardingShipment shipment = CreateShipment("AUSYD");
				var shipmentWrapper = new CMDShipmentWrapper(shipment);
				var generator = new DummyGenerator(shipmentWrapper);
				AssertEquals(0, shipmentWrapper.Messages.Count);
				generator.ExposedInsertMessage("CONSOL1", "001", "blablabla", "512-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(1, generator.NoOfMessagesSent);

				var message = shipmentWrapper.Messages[0];
				AssertEquals("blablabla", message.EM_MessageText);
				AssertEquals("001", message.EM_MessageSubType);
				AssertEquals("CONSOL1", message.EM_ApplicationReference);
				AssertEquals(EDIMessage.Status.Sent, message.EM_Status);
				Assert("There should be an interchange for this message", !message.EM_EI.IsEmpty);

				var interchange = Factory.Load<EDIInterchange>(message.EM_EI);
				AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EC001", interchange.EI_From);
				AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);

				AssertXMLEquals(@"<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">
<MessageType>CMD</MessageType>
<Priority>QK</Priority>
<Carrier></Carrier>
<CreationDateTime>2020-01-01T00:00:00.0000000Z</CreationDateTime>
<HAWB></HAWB>
<MAWB></MAWB>
<IssuingCarrierAgentIATACode></IssuingCarrierAgentIATACode>
<Body>
<![CDATA[", interchange.EI_HeaderText);

				AssertEquals("eHubAirService", interchange.EI_To);
				AssertEquals(message.EM_ApplicationCode, interchange.EI_ApplicationCode);
				AssertEquals(message.EM_ApplicationCode, interchange.EI_InterchangeType);
				AssertEquals(message.EM_MessageText, interchange.EI_BodyText);
				AssertXMLEquals(@"]]>
</Body>
</CargoIMP>", interchange.EI_FooterText);

				registrationKey.EnterpriseCodeForTest = "";
				shipmentWrapper.Messages.RemoveAll();
				AssertEquals(0, shipmentWrapper.Messages.Count);
				generator.ExposedInsertMessage("CONSOL1", "TDB", "blablabla", "512-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(2, generator.NoOfMessagesSent);

				message = shipmentWrapper.Messages[0];
				Assert("There should be an interchange for this message", !message.EM_EI.IsEmpty);
				interchange = Factory.Load<EDIInterchange>(message.EM_EI);
				AssertEquals("GC001", interchange.EI_From);
				AssertXMLEquals(@"<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">
<MessageType>CMD</MessageType>
<Priority>QK</Priority>
<Carrier></Carrier>
<CreationDateTime>2020-01-01T00:00:00.0000000Z</CreationDateTime>
<HAWB></HAWB>
<MAWB></MAWB>
<IssuingCarrierAgentIATACode></IssuingCarrierAgentIATACode>
<Body>
<![CDATA[", interchange.EI_HeaderText);
				shipmentWrapper.Messages.RemoveAll();
				AssertEquals(0, shipmentWrapper.Messages.Count);
				generator.ExposedInsertMessage("CONSOL1", "GHA", "blablabla", "081-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(3, generator.NoOfMessagesSent);
				message = shipmentWrapper.Messages[0];
				Assert("There should be an interchange for this message", !message.EM_EI.IsEmpty);
				interchange = Factory.Load<EDIInterchange>(message.EM_EI);
				AssertEquals("GC001", interchange.EI_From);
				AssertEquals(@"<CargoIMP xmlns=""http://cargowise.com/cargoimp/201108"">
<MessageType>CMD</MessageType>
<Priority>QK</Priority>
<Carrier></Carrier>
<CreationDateTime>2020-01-01T00:00:00.0000000Z</CreationDateTime>
<HAWB></HAWB>
<MAWB></MAWB>
<IssuingCarrierAgentIATACode></IssuingCarrierAgentIATACode>
<Body>
<![CDATA[", interchange.EI_HeaderText);
			}
		}

		public void TestTestInsertMessage_InterchangeStatus_ShouldBeHUQ_IfSGCMDIsEnabled()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			GlbCompany.CurrentCompany.GC_Code = "GC";
			registrationKey.EnterpriseCodeForTest = "EC";
			registrationKey.ServerCodeForTest = "001";
			using (SGCustomsDataRegistry.Instance.SendViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ForwardingShipment shipment = CreateShipment("AUSYD");
				CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
				DummyGenerator dummy = new DummyGenerator(shipmentWrapper);
				AssertEquals(0, shipmentWrapper.Messages.Count);
				dummy.ExposedInsertMessage("CONSOL1", "001", "blablabla", "512-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(1, dummy.NoOfMessagesSent);
				AssertEquals("blablabla", shipmentWrapper.Messages[0].EM_MessageText);
				AssertEquals("001", shipmentWrapper.Messages[0].EM_MessageSubType);
				AssertEquals("CONSOL1", shipmentWrapper.Messages[0].EM_ApplicationReference);
				Assert("There should be an interchange for this message", !shipmentWrapper.Messages[0].EM_EI.IsEmpty);
				var interchange = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[0].EM_EI);
				AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EC001", interchange.EI_From);
				AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("eHubAirService", interchange.EI_To);
			}
		}

		public void TestInsertMessageWhenRecipientIsSpecified()
		{
			using (SGCustomsDataRegistry.Instance.SendViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				GlbCompany.CurrentCompany.GC_Code = "GC";
				registrationKey.EnterpriseCodeForTest = "EC";
				registrationKey.ServerCodeForTest = "001";
				ForwardingShipment shipment = CreateShipment("AUSYD");
				CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
				DummyGenerator dummy = new DummyGenerator(shipmentWrapper, "Testing123");
				AssertEquals(0, shipmentWrapper.Messages.Count);
				dummy.ExposedInsertMessage("CONSOL1", "001", "blablabla", "512-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(1, dummy.NoOfMessagesSent);
				Assert("There should be an interchange for this message", !shipmentWrapper.Messages[0].EM_EI.IsEmpty);
				var interchange = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[0].EM_EI);
				AssertEquals("CMD\r\nTest\r\n", interchange.EI_HeaderText);
				registrationKey.EnterpriseCodeForTest = "";
				shipmentWrapper.Messages.RemoveAll();
				AssertEquals(0, shipmentWrapper.Messages.Count);
				dummy.ExposedInsertMessage("CONSOL1", "TDB", "blablabla", "512-12345678");
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(2, dummy.NoOfMessagesSent);
				Assert("There should be an interchange for this message", !shipmentWrapper.Messages[0].EM_EI.IsEmpty);
				interchange = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[0].EM_EI);
				AssertEquals("CMD\r\nTDB\r\n", interchange.EI_HeaderText);
			}
		}

		public void TestGenerateSendMessagesWhenCollectionIsEmpty()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			GenerateConsolsForTest(shipment);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_ApplicationReference, ListSortDirection.Ascending);
			AssertEquals(4, shipmentWrapper.Messages.Count);
			AssertEquals(4, generator.NoOfMessagesSent);
			AssertEquals("", generator.ErrorMessage);
			AssertEquals("CMD/2\r\nA/N/N\r\nTesting123", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nConsol2", shipmentWrapper.Messages[1].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nConsol4", shipmentWrapper.Messages[2].EM_MessageText);
			AssertEquals("CMD/2\r\nA/N/N\r\nConsol5", shipmentWrapper.Messages[3].EM_MessageText);
		}

		public void TestGenerateSendMessagesWhenCollectionIsNotEmpty()
		{
			ForwardingShipment shipment = CreateShipment("HKHKG");
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			GenerateMessagesForTest(shipmentWrapper);
			shipmentWrapper.RefreshMessageList();
			GenerateConsolsForTest(shipment);
			Factory.Save();
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_ApplicationReference, ListSortDirection.Ascending);
			AssertEquals(7, shipmentWrapper.Messages.Count);
			AssertEquals(5, generator.NoOfMessagesSent);
			AssertEquals("", generator.ErrorMessage);
			AssertEquals("CMD/2\r\nA/N/N\r\nTesting123", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nConsol2", shipmentWrapper.Messages[1].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting345", shipmentWrapper.Messages[2].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nConsol4", shipmentWrapper.Messages[3].EM_MessageText);
			AssertEquals("CMD/2\r\nM/N/N\r\nConsol5", shipmentWrapper.Messages[4].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting789", shipmentWrapper.Messages[5].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting890", shipmentWrapper.Messages[6].EM_MessageText);
		}

		public void TestGenerateLateSendMessages()
		{
			using (SGCustomsDataRegistry.Instance.SendViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ForwardingShipment shipment = CreateShipment("USLAX");
				GenerateConsolsWithLateMessagesForTest(shipment);
				CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
				DummyGenerator generator = new DummyGenerator(shipmentWrapper);
				generator.GenerateSendMessages();
				shipmentWrapper.RefreshMessageList();
				shipmentWrapper.Messages.Sort(new CMDMessageComparer());
				AssertEquals(4, shipmentWrapper.Messages.Count);
				AssertEquals(4, generator.NoOfMessagesSent);
				AssertEquals("", generator.ErrorMessage);
				AssertEquals("CMD/2\r\nA/N/Y\r\nTesting123", shipmentWrapper.Messages[0].EM_MessageText);
				AssertEquals("CMD/2\r\nA/N/Y\r\nTesting123", shipmentWrapper.Messages[1].EM_MessageText);
				AssertEquals("CMD/2\r\nD/N/Y\r\nConsol2", shipmentWrapper.Messages[2].EM_MessageText);
				AssertEquals("CMD/2\r\nD/N/Y\r\nConsol2", shipmentWrapper.Messages[3].EM_MessageText);
				// Assert who the message is intended to be sent to
				AssertEquals("GHA", shipmentWrapper.Messages[0].EM_MessageSubType);
				AssertEquals("TDB", shipmentWrapper.Messages[1].EM_MessageSubType);
				AssertEquals("GHA", shipmentWrapper.Messages[2].EM_MessageSubType);
				AssertEquals("TDB", shipmentWrapper.Messages[3].EM_MessageSubType);
				// Assert the interchanges
				var interchange1 = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[0].EM_EI);
				var interchange2 = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[1].EM_EI);
				var interchange3 = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[2].EM_EI);
				var interchange4 = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[3].EM_EI);
				AssertEquals("CMD\r\nLAX\r\n", interchange1.EI_HeaderText);
				AssertEquals("CMD\r\nTDB\r\n", interchange2.EI_HeaderText);
				AssertEquals("CMD\r\nLAX\r\n", interchange3.EI_HeaderText);
				AssertEquals("CMD\r\nTDB\r\n", interchange4.EI_HeaderText);
			}
		}

		class CMDMessageComparer : IComparer<CMDEDIMessage>
		{
			#region IComparer<CMDEDIMessage> Members
			int IComparer<CMDEDIMessage>.Compare(CMDEDIMessage x, CMDEDIMessage y)
			{
				int result = x.EM_ApplicationReference.CompareTo(y.EM_ApplicationReference);
				if (result == 0)
				{
					result = x.EM_MessageType.CompareTo(y.EM_MessageType);
				}

				if (result == 0)
				{
					result = x.EM_MessageSubType.CompareTo(y.EM_MessageSubType);
				}

				return result;
			}
			#endregion
		}

		public void TestGenerateLateSendMessageWhenPreviousMessageIsNotLate()
		{
			ForwardingShipment shipment = CreateShipment("USLAX");
			DummyConsol consol = GenerateConsol(shipment, "Consol1", "CMD/2\r\nA/N/N\r\nTesting123", true, true);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			AssertEquals(1, shipmentWrapper.Messages.Count);
			AssertEquals(1, generator.NoOfMessagesSent);
			shipmentWrapper.Messages[0].Reply = new CMDInbound("CMA/2\r\nA/N/N\r\nTesting123");
			consol.CMDString = "CMD/2\r\nM/N/Y\r\nTesting123";
			generator.GenerateSendMessages();
			AssertEquals("1 for GHA and 1 for TDB", 2, shipmentWrapper.Messages.Count);
			AssertEquals("1 for GHA and 1 for TDB", 2, generator.NoOfMessagesSent);
		}

		public void TestGenerateWithdrawMessages()
		{
			ForwardingShipment shipment = CreateShipment("IDJKT");
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			GenerateMessagesForTest(shipmentWrapper);
			shipmentWrapper.RefreshMessageList();
			CMDGenerator generator = new CMDGenerator(shipmentWrapper);
			generator.GenerateWithdrawMessages();
			shipmentWrapper.RefreshMessageList();
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_ApplicationReference, ListSortDirection.Ascending);
			AssertEquals(6, shipmentWrapper.Messages.Count);
			AssertEquals(5, generator.NoOfMessagesSent);
			AssertEquals("", generator.ErrorMessage);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting123", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting234", shipmentWrapper.Messages[1].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting345", shipmentWrapper.Messages[2].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting567", shipmentWrapper.Messages[3].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting789", shipmentWrapper.Messages[4].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nTesting890", shipmentWrapper.Messages[5].EM_MessageText);
			shipmentWrapper.ShowAll = true;
			AssertEquals(13, shipmentWrapper.Messages.Count);
		}

		public void TestGenerateWithdrawMessageWhenInterchangeExists()
		{
			using (SGCustomsDataRegistry.Instance.SendViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ForwardingShipment shipment = CreateShipment("IDJKT");
				DummyConsol consol = GenerateConsol(shipment, "Consol1", "CMD/2\r\nA/N/N\r\nTesting123", true, true);
				EDIInterchange interchange = Factory.New<EDIInterchange>();
				interchange.EI_HeaderText = "blabla";
				CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
				CMDEDIMessage message = shipmentWrapper.Messages.AddNew();
				message.EM_MessageText = "CMD/2\r\nA/N/N\r\nTesting123";
				message.EM_ApplicationReference = "Consol1";
				message.EM_EI = interchange.PK;
				CMDGenerator generator = new CMDGenerator(shipmentWrapper);
				generator.GenerateWithdrawMessages();
				shipmentWrapper.RefreshMessageList();
				AssertEquals(1, shipmentWrapper.Messages.Count);
				AssertEquals(1, generator.NoOfMessagesSent);
				Assert("New message should be different than the previous one", message.PK != shipmentWrapper.Messages[0].PK);
				Assert("New interchange should be different than the previous one", interchange.PK != shipmentWrapper.Messages[0].EM_EI);
				var newInterchange = Factory.Load<EDIInterchange>(shipmentWrapper.Messages[0].EM_EI);
				AssertEquals("blabla", newInterchange.EI_HeaderText);
			}
		}

		public void TestErrorValidation()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(true, shipmentWrapper);
			generator.GenerateSendMessages();
			AssertEquals("There is no Exemption Code or Permit Numbers entered to send.", generator.ErrorMessage);
			var cmdData = Factory.New<CMDPermitNumber>();
			cmdData.CY_ParentID = shipment.PK;
			cmdData.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cmdData.CY_Code = CustomsEntryTypeList.Singapore.Permit;
			cmdData.CY_Data = "0001";
			generator = new DummyGenerator(true, shipmentWrapper);
			generator.GenerateSendMessages();
			AssertEquals("There is no Consol to send the CMD Messages for.", generator.ErrorMessage);
			GenerateConsolsForTest(shipment);
			generator = new DummyGenerator(true, shipmentWrapper);
			generator.GenerateSendMessages();
			AssertEquals("", generator.ErrorMessage);
			shipmentWrapper.Messages.RemoveAll();
			generator = new DummyGenerator(true, shipmentWrapper);
			generator.GenerateWithdrawMessages();
			AssertEquals("There is no previously sent CMD Messages to be deleted.", generator.ErrorMessage);
			GenerateMessagesForTest(shipmentWrapper);
			generator = new DummyGenerator(true, shipmentWrapper);
			generator.GenerateWithdrawMessages();
			AssertEquals("", generator.ErrorMessage);
		}

		public void TestOtherNonCMDMessagesDontGetSent()
		{
			int beforeMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			ForwardingShipment shipment = CreateShipment("IDDPS");
			GenerateConsolsForTest(shipment);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			GenerateFNAMessagesForTest(shipmentWrapper);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_MessageText, ListSortDirection.Ascending);
			AssertEquals(4, shipmentWrapper.Messages.Count);
			AssertEquals("The FNA Messages should not be sent", 4, generator.NoOfMessagesSent);
			AssertEquals("", generator.ErrorMessage);
			AssertEquals("CMD/2\r\nA/N/N\r\nConsol5", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals("CMD/2\r\nA/N/N\r\nTesting123", shipmentWrapper.Messages[1].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nConsol2", shipmentWrapper.Messages[2].EM_MessageText);
			AssertEquals("CMD/2\r\nD/N/N\r\nConsol4", shipmentWrapper.Messages[3].EM_MessageText);
			CMDEDIMessage[] allMessages = (CMDEDIMessage[])Factory.Load(typeof(CMDEDIMessage), new ZQuery());
			AssertEquals("CMDs and FNAs", 7, allMessages.Length - beforeMessageCount);
			ZQuery fNAFilter = new ZQuery(EDIMessageSchema.EM_MessageType, "FNA");
			CMDEDIMessage[] fNAMessages = (CMDEDIMessage[])Factory.Load(typeof(CMDEDIMessage), fNAFilter);
			AssertEquals(3, fNAMessages.Length);
			ZString[] fNAMessagesText = new ZString[] { fNAMessages[0].EM_MessageText, fNAMessages[1].EM_MessageText, fNAMessages[2].EM_MessageText };
			Array.Sort(fNAMessagesText);
			AssertEquals("FNA\r\n\r\nFNA\r\nACK/balkasdf\r\nCMD/2\r\nA/N/N\r\nTesting123", fNAMessagesText[0]);
			AssertEquals("FNA\r\n\r\nFNA\r\nACK/balkasdf\r\nCMD/2\r\nA/N/N\r\nTesting234", fNAMessagesText[1]);
			AssertEquals("FNA\r\n\r\nFNA\r\nACK/balkasdf\r\nCMD/2\r\nM/N/N\r\nTesting345", fNAMessagesText[2]);
		}

		public void TestDuplicateCMDMessagesDontGetSentAgain()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			GenerateConsolsForDuplicateCMDMessagesTest(shipment);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			GenerateDuplicateCMDMessages(shipmentWrapper);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.ShowActiveMessagesOnly = true;
			shipmentWrapper.RefreshMessageList();
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_ApplicationReference, ListSortDirection.Ascending);
			AssertEquals(4, shipmentWrapper.Messages.Count);
			AssertEquals("Only CONSOLA CMD Messages should be sent", 2, generator.NoOfMessagesSent);
			AssertEquals("", generator.ErrorMessage);
			AssertEquals("CONSOLA", shipmentWrapper.Messages[0].EM_ApplicationReference);
			AssertEquals("CMD/2\r\nM/N/Y\r\nTesting123a", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals(shipmentWrapper.Messages[0].EM_ApplicationReference, shipmentWrapper.Messages[1].EM_ApplicationReference);
			AssertEquals(shipmentWrapper.Messages[0].EM_MessageText, shipmentWrapper.Messages[1].EM_MessageText);
			shipmentWrapper.Messages[0].Reply = new CMDInbound("CMA/2\r\nM/N/Y\r\n");
			shipmentWrapper.Messages[1].Reply = new CMDInbound("CMA/2\r\nM/N/Y\r\n");
			AssertEquals("CONSOLB", shipmentWrapper.Messages[2].EM_ApplicationReference);
			AssertEquals("CMD/2\r\nD/N/Y\r\nTesting234", shipmentWrapper.Messages[2].EM_MessageText);
			AssertEquals(shipmentWrapper.Messages[2].EM_ApplicationReference, shipmentWrapper.Messages[3].EM_ApplicationReference);
			AssertEquals(shipmentWrapper.Messages[2].EM_MessageText, shipmentWrapper.Messages[3].EM_MessageText);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(4, shipmentWrapper.Messages.Count);
			AssertEquals("No new messages should be sent", 0, generator.NoOfMessagesSent);
		}

		public void TestNewCMDIsCreatedIfNoReplyIsReceivedForPreviousNewCMD()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			DummyConsol consol1 = GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/N\r\nTesting123", true, true);
			DummyConsol consol2 = GenerateConsol(shipment, "CONSOL2", "CMD/2\r\nA/N/N\r\nConsol2", true, true);
			DummyConsol consol3 = GenerateConsol(shipment, "CONSOL99", "CMD/2\r\nM/N/N\r\nConsol99", true, true);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(3, shipmentWrapper.Messages.Count);
			AssertEquals(3, generator.NoOfMessagesSent);
			consol1.CMDString = "CMD/2\r\nA/N/N\r\nTesting234";
			consol2.CMDString = "CMD/2\r\nA/N/N\r\nConsol2m";
			consol3.CMDString = "CMD/2\r\nA/N/N\r\nConsol99b";
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(3, shipmentWrapper.Messages.Count);
			AssertEquals(3, generator.NoOfMessagesSent);
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_MessageText, ListSortDirection.Ascending);
			AssertEquals("CMD/2\r\nA/N/N\r\nConsol2m", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals("CMD/2\r\nA/N/N\r\nTesting234", shipmentWrapper.Messages[1].EM_MessageText);
			AssertEquals("CMD/2\r\nM/N/N\r\nConsol99b", shipmentWrapper.Messages[2].EM_MessageText);
		}

		public void TestNewCMDIsCreatedIfFNAReplyIsReceivedForPreviousNewCMD()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			DummyConsol consol1 = GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/N\r\nTesting123", true, true);
			DummyConsol consol2 = GenerateConsol(shipment, "CONSOL2", "CMD/2\r\nA/N/Y\r\nConsol2", true, true);
			DummyConsol consol3 = GenerateConsol(shipment, "CONSOL99", "CMD/2\r\nM/N/N\r\nConsol99", true, true);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(4, shipmentWrapper.Messages.Count);
			AssertEquals(4, generator.NoOfMessagesSent);
			CMDEDIMessage[] replies = GenerateFNAMessagesForTest(shipmentWrapper);
			shipmentWrapper.Messages[0].Reply = new CMDInbound(replies[0].EM_MessageText);
			shipmentWrapper.Messages[1].Reply = new CMDInbound(replies[1].EM_MessageText);
			shipmentWrapper.Messages[2].Reply = new CMDInbound(replies[2].EM_MessageText);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(4, shipmentWrapper.Messages.Count);
			AssertEquals(0, generator.NoOfMessagesSent);
			consol1.CMDString = "CMD/2\r\nA/N/N\r\nTesting234";
			consol2.CMDString = "CMD/2\r\nA/N/Y\r\nConsol2m";
			consol3.CMDString = "CMD/2\r\nM/N/N\r\nConsol99b";
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(4, shipmentWrapper.Messages.Count);
			AssertEquals(4, generator.NoOfMessagesSent);
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_MessageText, ListSortDirection.Ascending);
			AssertEquals("CMD/2\r\nA/N/N\r\nTesting234", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals("CMD/2\r\nA/N/Y\r\nConsol2m", shipmentWrapper.Messages[1].EM_MessageText);
			AssertEquals("CMD/2\r\nA/N/Y\r\nConsol2m", shipmentWrapper.Messages[2].EM_MessageText);
			AssertEquals("CMD/2\r\nM/N/N\r\nConsol99b", shipmentWrapper.Messages[3].EM_MessageText);
		}

		public void TestModifyCMDIsCreatedIfCMAReplyIsReceivedForPreviousNewCMD()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			DummyConsol consol1 = GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/N\r\nTesting123", true, true);
			DummyConsol consol2 = GenerateConsol(shipment, "CONSOL2", "CMD/2\r\nA/N/N\r\nConsol2", true, true);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(2, shipmentWrapper.Messages.Count);
			AssertEquals(2, generator.NoOfMessagesSent);
			CMDEDIMessage[] replies = GenerateCMAMessagesForTest(shipmentWrapper);
			shipmentWrapper.Messages[0].Reply = new CMDInbound(replies[0].EM_MessageText);
			shipmentWrapper.Messages[1].Reply = new CMDInbound(replies[1].EM_MessageText);
			consol1.CMDString = "CMD/2\r\nA/N/N\r\nTesting234";
			consol2.CMDString = "CMD/2\r\nA/N/N\r\nConsol2m";
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(2, shipmentWrapper.Messages.Count);
			AssertEquals(2, generator.NoOfMessagesSent);
			shipmentWrapper.Messages.Sort(EDIMessageSchema.Constants.EM_MessageText, ListSortDirection.Ascending);
			AssertEquals("CMD/2\r\nM/N/N\r\nConsol2m", shipmentWrapper.Messages[0].EM_MessageText);
			AssertEquals("CMD/2\r\nM/N/N\r\nTesting234", shipmentWrapper.Messages[1].EM_MessageText);
		}

		public void TestBaseIsImportOrExport()
		{
			ZString originalCode = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			try
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "IDJKT";
				ForwardingShipment shipment = CreateShipment("IDDPS");
				CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
				DummyGenerator generator = new DummyGenerator(shipmentWrapper);
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "HKHKG";
				AssertEquals(false, generator.BaseIsImportOrExport(consol));
				consol.JK_RL_NKLoadPort = "IDJKT";
				AssertEquals(true, generator.BaseIsImportOrExport(consol));
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "IDJKT";
				AssertEquals(true, generator.BaseIsImportOrExport(consol));
				consol.JK_RL_NKLoadPort = "IDDPS";
				AssertEquals(false, generator.BaseIsImportOrExport(consol));
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = originalCode;
			}
		}

		public void TestTDBMessageDoesntGetDeletedWhenCorrespondingGHAMessageIsNotDeleted()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			DummyConsol consol = GenerateConsol(shipment, "Consol1", "CMD/2\r\nA/N/Y\r\nTesting123", true, true);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(2, shipmentWrapper.Messages.Count);
			ZGuid message1Guid = shipmentWrapper.Messages[0].PK;
			ZGuid message2Guid = shipmentWrapper.Messages[1].PK;
			AssertEquals(2, generator.NoOfMessagesSent);
			Factory.Save();
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(2, shipmentWrapper.Messages.Count);
			AssertNotNull(shipmentWrapper.Messages.FindByPK(message1Guid));
			AssertNotNull(shipmentWrapper.Messages.FindByPK(message2Guid));
			AssertEquals(0, generator.NoOfMessagesSent);
		}

		public void TestGenerateSendMessagesWhenContentIsTheSameButRecipientIsDifferent()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			DummyConsol consol1 = GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/N\r\nTesting123", true, true);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(1, shipmentWrapper.Messages.Count);
			AssertEquals(1, generator.NoOfMessagesSent);
			generator = new DummyGenerator(shipmentWrapper);
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(1, shipmentWrapper.Messages.Count);
			AssertEquals("Nothing should be sent as the data is unchanged and reply is null", 0, generator.NoOfMessagesSent);
			shipmentWrapper.Messages[0].Reply = new CMDInbound("FNA\r\nOLDR\r\nFNA\r\nACK/alsjkfasdf\r\nCMD/2\r\nA/N/N\r\nTesting234");
			generator = new DummyGenerator(shipmentWrapper, "OLDR");
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(1, shipmentWrapper.Messages.Count);
			AssertEquals("Nothing should be sent as the data is unchanged and recipient is the same as the previous recipient", 0, generator.NoOfMessagesSent);
			shipmentWrapper.Messages[0].Reply = new CMDInbound("FNA\r\nOLDR\r\nFNA\r\nACK/alsjkfasdf\r\nCMD/2\r\nA/N/N\r\nTesting234");
			generator = new DummyGenerator(shipmentWrapper, "");
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(1, shipmentWrapper.Messages.Count);
			AssertEquals("Nothing should be sent as the data is unchanged and recipient is not specified", 0, generator.NoOfMessagesSent);
			generator = new DummyGenerator(shipmentWrapper, "NEWR");
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(1, shipmentWrapper.Messages.Count);
			AssertEquals("Data is unchanged but message is sent to a new recipient", 1, generator.NoOfMessagesSent);
		}

		public void TestOnlyOldTDBMessageGetsDeletedWhenSameMessageIsSentToDifferentRecipient()
		{
			ForwardingShipment shipment = CreateShipment("IDDPS");
			DummyConsol consol1 = GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/Y\r\nTesting123", true, true);
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			DummyGenerator generator = new DummyGenerator(shipmentWrapper, "SATS");
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals(2, shipmentWrapper.Messages.Count);
			AssertEquals(2, generator.NoOfMessagesSent);
			shipmentWrapper.Messages[0].Reply = new CMDInbound("FNA\r\nSATS\r\nFNA\r\nACK/alsjkfasdf\r\nCMD/2\r\nA/N/Y\r\nTesting234");
			generator = new DummyGenerator(shipmentWrapper, "CIAS");
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertEquals("New TDB should not be set as inactive", 2, shipmentWrapper.Messages.Count);
			AssertEquals(2, generator.NoOfMessagesSent);
		}

		public void TestCMDInterchangeGetsAirlineCodeFromDepartureFlight()
		{
			var shipment = CreateShipment("IDDPS");
			var consol = GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/Y\r\nTesting123", true, true);

			Transport tran = consol.MostInterestingTransportForBinding[0];
			tran.JW_ETD = new DateTime(2009, 03, 24);
			tran.JW_TransportMode = Core.Constants.TransportModes.Air;
			tran.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			tran.JW_VoyageFlight = "BA123";

			var shipmentWrapper = new CMDShipmentWrapper(shipment);
			var generator = new DummyGenerator(shipmentWrapper, "SATS");
			generator.GenerateSendMessages();
			shipmentWrapper.RefreshMessageList();
			AssertContains("<Carrier>BA</Carrier>", shipmentWrapper.Messages[0].Interchange.EI_HeaderText);
		}

		#region Implementation
		void GenerateConsolsForTest(ForwardingShipment shipment)
		{
			GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/N\r\nTesting123", true, true);
			GenerateConsol(shipment, "CONSOL2", "CMD/2\r\nD/N/N\r\nConsol2", true, true);
			GenerateConsol(shipment, "CONSOL3", "CMD/2\r\nM/N/N\r\nConsol3", false, false);
			GenerateConsol(shipment, "CONSOL4", "CMD/2\r\nD/N/N\r\nConsol4", true, true);
			GenerateConsol(shipment, "CONSOL5", "CMD/2\r\nA/N/N\r\nConsol5", true, true);
		}

		void GenerateConsolsWithLateMessagesForTest(ForwardingShipment shipment)
		{
			GenerateConsol(shipment, "CONSOL1", "CMD/2\r\nA/N/Y\r\nTesting123", true, true);
			GenerateConsol(shipment, "CONSOL2", "CMD/2\r\nD/N/Y\r\nConsol2", true, true);
		}

		CMDEDIMessage[] GenerateFNAMessagesForTest(CMDShipmentWrapper shipmentWrapper)
		{
			CMDEDIMessage message1 = shipmentWrapper.Messages.AddNew();
			message1.EM_MessageText = "FNA\r\n\r\nFNA\r\nACK/balkasdf\r\nCMD/2\r\nA/N/N\r\nTesting123";
			message1.EM_ApplicationReference = "CONSOL1";
			message1.EM_MessageType = "FNA";
			CMDEDIMessage message2 = shipmentWrapper.Messages.AddNew();
			message2.EM_MessageText = "FNA\r\n\r\nFNA\r\nACK/balkasdf\r\nCMD/2\r\nA/N/N\r\nTesting234";
			message2.EM_ApplicationReference = "CONSOL2";
			message2.EM_MessageType = "FNA";
			CMDEDIMessage message3 = shipmentWrapper.Messages.AddNew();
			message3.EM_MessageText = "FNA\r\n\r\nFNA\r\nACK/balkasdf\r\nCMD/2\r\nM/N/N\r\nTesting345";
			message3.EM_ApplicationReference = "CONSOL99";
			message3.EM_MessageType = "FNA";
			return new CMDEDIMessage[] { message1, message2, message3 };
		}

		CMDEDIMessage[] GenerateCMAMessagesForTest(CMDShipmentWrapper shipmentWrapper)
		{
			CMDEDIMessage message1 = shipmentWrapper.Messages.AddNew();
			message1.EM_MessageText = "CMA\r\n\r\nCMA/2\r\nA/N/N\r\nTesting123";
			message1.EM_ApplicationReference = "CONSOL1";
			message1.EM_MessageType = "CMA";
			CMDEDIMessage message2 = shipmentWrapper.Messages.AddNew();
			message2.EM_MessageText = "CMA\r\n\r\nCMA/2\r\nA/N/N\r\nTesting234";
			message2.EM_ApplicationReference = "CONSOL2";
			message2.EM_MessageType = "CMA";
			return new CMDEDIMessage[] { message1, message2 };
		}

		void GenerateMessagesForTest(CMDShipmentWrapper shipmentWrapper)
		{
			CMDEDIMessage message1 = shipmentWrapper.Messages.AddNew();
			message1.EM_MessageText = "CMD/2\r\nA/N/N\r\nTesting123";
			message1.EM_ApplicationReference = "CONSOL1";
			message1.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message1.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/N\r\nTesting123");
			CMDEDIMessage message2 = shipmentWrapper.Messages.AddNew();
			message2.EM_MessageText = "CMD/2\r\nM/N/N\r\nTesting234";
			message2.EM_ApplicationReference = "CONSOL2";
			message2.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message2.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nM/N/N\r\nTesting234");
			CMDEDIMessage message3 = shipmentWrapper.Messages.AddNew();
			message3.EM_MessageText = "CMD/2\r\nD/N/N\r\nTesting345";
			message3.EM_ApplicationReference = "CONSOL3";
			message3.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message3.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nD/N/N\r\nTesting345");
			CMDEDIMessage message4 = shipmentWrapper.Messages.AddNew();
			message4.EM_MessageText = "CMD/2\r\nM/N/N\r\nTesting456";
			message4.EM_ApplicationReference = "CONSOL4";
			message4.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message4.EM_IsActive = false;
			message4.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nM/N/N\r\nTesting456");
			CMDEDIMessage message5 = shipmentWrapper.Messages.AddNew();
			message5.EM_MessageText = "CMD/2\r\nM/N/N\r\nTesting567";
			message5.EM_ApplicationReference = "CONSOL5";
			message5.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message5.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nM/N/N\r\nTesting567");
			CMDEDIMessage message6 = shipmentWrapper.Messages.AddNew();
			message6.EM_MessageText = "CMD/2\r\nA/N/N\r\nTesting678";
			message6.EM_ApplicationReference = "CONSOL6";
			message6.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message6.EM_IsActive = false;
			message6.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/N\r\nTesting678");
			CMDEDIMessage message7 = shipmentWrapper.Messages.AddNew();
			message7.EM_MessageText = "CMD/2\r\nM/N/N\r\nTesting789";
			message7.EM_ApplicationReference = "CONSOL7";
			message7.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message7.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nM/N/N\r\nTesting789");
			CMDEDIMessage message8 = shipmentWrapper.Messages.AddNew();
			message8.EM_MessageText = "CMD/2\r\nA/N/N\r\nTesting890";
			message8.EM_ApplicationReference = "CONSOL8";
			message8.EM_MessageSubType = CMDGenerator.Constants.GHA;
			message8.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/N\r\nTesting890");
		}

		void GenerateDuplicateCMDMessages(CMDShipmentWrapper shipmentWrapper)
		{
			CMDEDIMessage message1 = shipmentWrapper.Messages.AddNew();
			message1.EM_MessageText = "CMD/2\r\nA/N/Y\r\nTesting123";
			message1.EM_ApplicationReference = "CONSOLA";
			message1.EM_MessageSubType = "TDB";
			message1.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			CMDEDIMessage message2 = shipmentWrapper.Messages.AddNew();
			message2.EM_MessageText = "CMD/2\r\nA/N/Y\r\nTesting123";
			message2.EM_ApplicationReference = "CONSOLA";
			message2.EM_MessageSubType = "GHA";
			message2.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			CMDEDIMessage message3 = shipmentWrapper.Messages.AddNew();
			message3.EM_MessageText = "CMD/2\r\nA/N/Y\r\nTesting123";
			message3.EM_ApplicationReference = "CONSOLA";
			message3.EM_MessageSubType = "TDB";
			message3.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			CMDEDIMessage message4 = shipmentWrapper.Messages.AddNew();
			message4.EM_MessageText = "CMD/2\r\nA/N/Y\r\nTesting123";
			message4.EM_ApplicationReference = "CONSOLA";
			message4.EM_MessageSubType = "GHA";
			message4.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			CMDEDIMessage message5 = shipmentWrapper.Messages.AddNew();
			message5.EM_MessageText = "CMD/2\r\nD/N/Y\r\nTesting234";
			message5.EM_ApplicationReference = "CONSOLB";
			message5.EM_MessageSubType = "TDB";
			message5.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			CMDEDIMessage message6 = shipmentWrapper.Messages.AddNew();
			message6.EM_MessageText = "CMD/2\r\nD/N/Y\r\nTesting234";
			message6.EM_ApplicationReference = "CONSOLB";
			message6.EM_MessageSubType = "GHA";
			message6.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			CMDEDIMessage message7 = shipmentWrapper.Messages.AddNew();
			message7.EM_MessageText = "CMD/2\r\nD/N/Y\r\nTesting234";
			message7.EM_ApplicationReference = "CONSOLB";
			message7.EM_MessageSubType = "TDB";
			message7.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			CMDEDIMessage message8 = shipmentWrapper.Messages.AddNew();
			message8.EM_MessageText = "CMD/2\r\nD/N/Y\r\nTesting234";
			message8.EM_ApplicationReference = "CONSOLB";
			message8.EM_MessageSubType = "GHA";
			message8.Reply = new CMDInbound("CMA/2\r\n\r\nCMA/2\r\nA/N/Y\r\n");
			Factory.Save();
		}

		void GenerateConsolsForDuplicateCMDMessagesTest(ForwardingShipment shipment)
		{
			GenerateConsol(shipment, "CONSOLA", "CMD/2\r\nA/N/Y\r\nTesting123a", true, true);
			GenerateConsol(shipment, "CONSOLB", "CMD/2\r\nD/N/Y\r\nTesting234", true, true);
		}

		DummyConsol GenerateConsol(ForwardingShipment shipment, string iD, string cMDString, bool isAir, bool isImportOrExport)
		{
			DummyConsol consol = (DummyConsol)shipment.Consols.AddNew(typeof(DummyConsol));
			consol.CMDString = cMDString;
			consol.IsImportOrExport = isImportOrExport;
			consol.JK_UniqueConsignRef = iD;
			consol.JK_TransportMode = (isAir) ? Core.Constants.TransportModes.Air : Core.Constants.TransportModes.Sea;
			return consol;
		}

		ForwardingShipment CreateShipment(string originCode)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = originCode;
			return shipment;
		}

		#region DummyGenerator
		class DummyGenerator : CMDGenerator
		{
			public DummyGenerator(CMDShipmentWrapper shipmentWrapper) : this(false, shipmentWrapper)
			{
			}

			public DummyGenerator(CMDShipmentWrapper shipmentWrapper, ZString recipient) : this(false, shipmentWrapper, recipient)
			{
			}

			public DummyGenerator(bool validationRequired, CMDShipmentWrapper shipmentWrapper) : base(shipmentWrapper)
			{
				this.validationRequired = validationRequired;
			}

			public DummyGenerator(bool validationRequired, CMDShipmentWrapper shipmentWrapper, ZString recipient) : base(shipmentWrapper, recipient)
			{
				this.validationRequired = validationRequired;
			}

			protected override string GetCMDString(ForwardingConsol consol)
			{
				return ((DummyConsol)consol).CMDString;
			}

			protected override bool IsImportOrExport(ForwardingConsol consol)
			{
				return ((DummyConsol)consol).IsImportOrExport;
			}

			public bool BaseIsImportOrExport(ForwardingConsol consol)
			{
				return base.IsImportOrExport(consol);
			}

			public string BaseGetCMDString(ForwardingConsol consol)
			{
				return base.GetCMDString(consol);
			}

			public void ExposedInsertMessage(string consolID, string messageSubType, string messageText, string masterBillNumber)
			{
				base.InsertMessage(consolID, messageSubType, messageText, masterBillNumber);
			}

			protected override bool IsValidSendMessage
			{
				get
				{
					return !validationRequired || base.IsValidSendMessage;
				}
			}

			protected override bool IsValidWithdrawMessage
			{
				get
				{
					return !validationRequired || base.IsValidWithdrawMessage;
				}
			}

			public override ZString ErrorMessage
			{
				get
				{
					return validationRequired ? base.ErrorMessage : ZString.Empty;
				}
			}

			readonly bool validationRequired;
		}

		class DummyConsol : ForwardingConsol
		{
			public DummyConsol(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			string fCMDString;
			public string CMDString
			{
				get
				{
					return fCMDString;
				}

				set
				{
					fCMDString = value;
				}
			}

			bool fIsImportOrExport;
			public bool IsImportOrExport
			{
				get
				{
					return fIsImportOrExport;
				}

				set
				{
					fIsImportOrExport = value;
				}
			}
		}
		#endregion
		#endregion
	}
}
