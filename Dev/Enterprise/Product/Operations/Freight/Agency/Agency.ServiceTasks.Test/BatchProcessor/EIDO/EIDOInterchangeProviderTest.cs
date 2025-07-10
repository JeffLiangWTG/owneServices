using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[EIDOMessagingConfiguration(Testing = false, Principals = new string[] { "Principal1", "Principal2" })]
	internal class EIDOInterchangeProviderTest : BaseAgencyTest
	{
		public void TestDeletedContainer()
		{
			EIDOMessage message1 = EIDOMessage.New(Container1, EIDOMessageFunction.Original, "Message1+<<MSGNO PLACEHOLDER>>");
			EIDOMessage message2 = EIDOMessage.New(Container2, EIDOMessageFunction.Original, "Message2+<<MSGNO PLACEHOLDER>>");
			Factory.Save();
			Container1.Delete();
			Factory.Save();
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message1);
			messages.Add(message2);
			EDIInterchange[] interchanges = new EIDOInterchangeProvider(messages, new FailedMessageList()).Interchanges;
			AssertContainsExactElementsInAnyOrder("Should have 1 interchange", new string[] { "SENDERID1|RECIPIENTID1" }, Array.ConvertAll(interchanges, ExtractSenderAndRecipient));
			AssertContainsExactElementsInAnyOrder("The interchange should not contain the message from the deleted container", (m) => m.EM_MessageText, new EDIMessage[] { message2 }, interchanges[0].ContainedMessages.ToArray<EDIMessage>());
			AssertEquals("message1.EM_Status", EIDOMessage.Status.Failed, message1.EM_Status);
			AssertEquals("message2.EM_Status", EIDOMessage.Status.Sent, message2.EM_Status);
		}

		public void TestInstructionsStayInSync()
		{
			EIDOInterchangeProvider provider = new EIDOInterchangeProvider(new NonDependentEDIMessageCollection(Factory), new FailedMessageList());
			string instructions = (string)typeof(InterchangeProviderBase).InvokeMember("InstructionHowToSetInterchangeSenderID", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, provider, Array.Empty<object>());
			Enterprise.Integration.IRegistryItemInternals registryItem = AgencyRegistry.Instance.EIDOMessagingDetails;
			string expectedInstructions = "You need to enable E-IDO messaging from the registry item: {0}";
			AssertEquals(string.Format(expectedInstructions, registryItem.Location), instructions);
		}

		public void TestMessagesAreGroupedByPrincipal()
		{
			EIDOMessage message1 = EIDOMessage.New(Container1, EIDOMessageFunction.Original, "Message1");
			EIDOMessage message2 = EIDOMessage.New(Container1, EIDOMessageFunction.Original, "Message2");
			EIDOMessage message3 = EIDOMessage.New(Container2, EIDOMessageFunction.Original, "Message2");
			EIDOMessage message4 = EIDOMessage.New(Container2, EIDOMessageFunction.Original, "Message3");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message1);
			messages.Add(message2);
			messages.Add(message3);
			messages.Add(message4);
			EDIInterchange[] interchanges = new EIDOInterchangeProvider(messages, new FailedMessageList()).Interchanges;
			Array.Sort(interchanges, (x, y) => x.EI_From.CompareTo(y.EI_From));
			AssertEquals("should have 2 interchanges", 2, interchanges.Length);
			AssertContainsExactElementsInAnyOrder("Should have 2 interchanges", new string[] { "SENDERID|RECIPIENTID", "SENDERID1|RECIPIENTID1", }, Array.ConvertAll(interchanges,ExtractSenderAndRecipient));
			EDIInterchange interchange1 = SelectInterchange(interchanges, "SENDERID|RECIPIENTID");
			EDIInterchange interchange2 = SelectInterchange(interchanges, "SENDERID1|RECIPIENTID1");
			AssertContainsExactElementsInAnyOrder((m) => m.EM_MessageText, new EDIMessage[] { message1, message2 }, interchange1.ContainedMessages.ToArray<EDIMessage>());
			AssertContainsExactElementsInAnyOrder((m) => m.EM_MessageText, new EDIMessage[] { message3, message4 }, interchange2.ContainedMessages.ToArray<EDIMessage>());
		}

		public void TestInterchangeText()
		{
			EIDOMessage message = EIDOMessage.New(Container1, EIDOMessageFunction.Original, "MESSAGE+BODY+END'");
			message.EM_IsTestMessage = false;
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message);
			EDIInterchange[] interchanges = new EIDOInterchangeProvider(messages, new FailedMessageList()).Interchanges;
			AssertEquals("assert should be 1 interchange", 1, interchanges.Length);
			const string interchangeText = "UNB+UNOA:4+SENDERID+RECIPIENTID+{0:yyMMdd:HHmm}+{1}'" + "MESSAGE+BODY+END'" + "UNZ+1+{1}'" + "";
			AssertMessageEquals("Should have the correct text", string.Format(interchangeText, interchanges[0].PreparationDateTime, EDIInterchange.InterchangeNumberPlaceHolder), interchanges[0].EI_InterchangeText);
		}

		#region Implementation
		EDIInterchange SelectInterchange(EDIInterchange[] interchanges, string senderAndRecipient)
		{
			foreach (EDIInterchange interchange in interchanges)
			{
				if (ExtractSenderAndRecipient(interchange) == senderAndRecipient)
				{
					return interchange;
				}
			}

			return null;
		}

		string ExtractSenderAndRecipient(EDIInterchange interchange)
		{
			string[] elements = interchange.EI_HeaderText.ToString().Split('+');
			if (elements.Length < 3)
			{
				return "|";
			}
			else if (elements.Length == 3)
			{
				return elements[2] + "|";
			}
			else
			{
				return elements[2] + "|" + elements[3];
			}
		}

		OrgHeader Principal1
		{
			get
			{
				if (principal1 == null)
				{
					principal1 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal1");
					principal1.OH_Code = "Principal1";
				}

				return principal1;
			}
		}

		OrgHeader principal1;
		OrgHeader Principal2
		{
			get
			{
				if (principal2 == null)
				{
					principal2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal2");
					principal2.OH_Code = "Principal2";
				}

				return principal2;
			}
		}

		OrgHeader principal2;
		AgencyShipment Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					shipment1 = Factory.New<AgencyShipment>();
					shipment1.JS_OH_DeliveryAgent = Principal1.PK;
				}

				return shipment1;
			}
		}

		AgencyShipment shipment1;
		AgencyShipment Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					shipment2 = Factory.New<AgencyShipment>();
					shipment2.JS_OH_DeliveryAgent = Principal2.PK;
				}

				return shipment2;
			}
		}

		AgencyShipment shipment2;
		AgencyShipmentContainer Container1
		{
			get
			{
				return container1 ?? (container1 = Shipment1.RealContainers.AddNew());
			}
		}

		AgencyShipmentContainer container1;
		AgencyShipmentContainer Container2
		{
			get
			{
				return container2 ?? (container2 = Shipment2.RealContainers.AddNew());
			}
		}

		AgencyShipmentContainer container2;
		#endregion
	}
}
