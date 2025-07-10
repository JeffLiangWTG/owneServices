using System;
using System.Collections.Generic;
using Enterprise.Edifact;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class EIDOResponseDecoderTest : BaseAgencyTest
	{
		public void TestGarbage()
		{
			const string messageText =
				"Want to make your finglonger?" +
				"Buy Agrofass!" +
				"";

			try
			{
				EIDOResponseDecoder.Parse(messageText);
				Fail("Should have thrown an InvalidFormatException");
			}
			catch (InvalidFormatException ex)
			{
				AssertEquals("Corrupted or Malformed D99A APERAK Response Message. Cannot Process.", ex.Message);
			}
		}

		public void TestAcceptanceMessage()
		{
			string messageText =
				"UNH+12345+APERAK:D:99A:UN:ANZ23'" +
				"BGM+7+001+9+AP'" +
				"DTM+137:20060425093000:204'" +
				"DOC+640+EIDO123'" +
				"DTM+137:200404250915:203'" +
				"NAD+MS+1-STOP'" +
				"ERC+COM000'" +
				"FTX+AAO+++MESSAGE RECEIVED WITHOUT ERROR'" +
				"RFF+EQD:KKFU1635133'" +
				"UNT+10+12345'" +
				"";

			IEIDOResponseMessage message = EIDOResponseDecoder.Parse(messageText);
			AssertEquals("Response Date Time", new DateTime(2006, 04, 25, 09, 30, 00), message.ResponseDateTime);
			AssertEquals("Document Issued Date", new DateTime(2004, 04, 25, 09, 15, 00), message.DocumentIssuedDate);
			AssertEquals("Document Reference", "EIDO123", message.DocumentReference);
			AssertEquals("Message Recipient", null, message.MessageRecipient);
			AssertEquals("Message Sender", "1-STOP", message.MessageSender);
			AssertEquals("Response Type", EIDOResponseType.Accepted, message.ResponseType);

			AssertContainsExactElementsInAnyOrder("Errors",
				new string[]
				{
					"COM000: MESSAGE RECEIVED WITHOUT ERROR\r\nEquipment: KKFU1635133"
				},
				ResponseErrorAsString(message.Errors));
		}

		public void TestReceivedMessage()
		{
			string messageText =
				"UNH+12345+APERAK:D:99A:UN:ANZ23'" +
				"BGM+7+001+9+CA'" +
				"DTM+137:20060425093000:204'" +
				"DOC+640+EIDO123'" +
				"DTM+137:20040425091500:204'" +
				"NAD+MS+1-STOP'" +
				"ERC+COM019'" +
				"FTX+AAO+++E-IDO RECEIPT ACKNOWLEDGEMENT - NO VALIDATION OF E-IDO DETAILS'" +
				"RFF+EQD:KKFU1635133'" +
				"UNT+10+12345'" +
				"";

			IEIDOResponseMessage message = EIDOResponseDecoder.Parse(messageText);
			AssertEquals("Response Date Time", new DateTime(2006, 04, 25, 09, 30, 00), message.ResponseDateTime);
			AssertEquals("Document Issued Date", new DateTime(2004, 04, 25, 09, 15, 00), message.DocumentIssuedDate);
			AssertEquals("Document Reference", "EIDO123", message.DocumentReference);
			AssertEquals("Message Recipient", null, message.MessageRecipient);
			AssertEquals("Message Sender", "1-STOP", message.MessageSender);
			AssertEquals("Response Type", EIDOResponseType.Received, message.ResponseType);

			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					"COM019: E-IDO RECEIPT ACKNOWLEDGEMENT - NO VALIDATION OF E-IDO DETAILS\r\nEquipment: KKFU1635133"
				},
				ResponseErrorAsString(message.Errors));
		}

		public void TestRejectionMessage()
		{
			string messageText =
				"UNH+12345+APERAK:D:99A:UN:ANZ23'" +
				"BGM+7+001+9+RE'" +
				"DTM+137:20060425093000:204'" +
				"DOC+640+EIDO123'" +
				"DTM+137:20040425091500:204'" +
				"NAD+MS+1-STOP'" +
				"NAD+MR+EAGLE'" +
				"ERC+COM016'" +
				"FTX+AAO+++INVALID MESSAGE FUNCTION'" +
				"ERC+COM017'" +
				"FTX+AAO+++PASSWORD NOT RECOGNISED FOR LINE OPERATOR'" +
				"RFF+EQD:KKFU1635133'" +
				"UNT+12+12345'" +
				"";

			IEIDOResponseMessage message = EIDOResponseDecoder.Parse(messageText);
			AssertEquals("Response Date Time", new DateTime(2006, 04, 25, 09, 30, 00), message.ResponseDateTime);
			AssertEquals("Document Issued Date", new DateTime(2004, 04, 25, 09, 15, 00), message.DocumentIssuedDate);
			AssertEquals("Document Reference", "EIDO123", message.DocumentReference);
			AssertEquals("Message Recipient", "EAGLE", message.MessageRecipient);
			AssertEquals("Message Sender", "1-STOP", message.MessageSender);
			AssertEquals("Response Type", EIDOResponseType.Rejected, message.ResponseType);

			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					"COM016: INVALID MESSAGE FUNCTION\r\nNone: ",
					"COM017: PASSWORD NOT RECOGNISED FOR LINE OPERATOR\r\nEquipment: KKFU1635133"
				},
				ResponseErrorAsString(message.Errors));
		}

		#region Implementation

		IEnumerable<string> ResponseErrorAsString(IEnumerable<IEIDOResponseError> errors)
		{
			foreach (IEIDOResponseError error in errors)
			{
				yield return ResponseErrorAsString(error);
			}
		}

		string ResponseErrorAsString(IEIDOResponseError error)
		{
			if (error == null)
			{
				return null;
			}
			else
			{
				return string.Format("{0}: {1}\r\n{2}: {3}", error.Code, error.Description, error.ErrorRefType, error.Reference);
			}
		}

		#endregion
	}
}
