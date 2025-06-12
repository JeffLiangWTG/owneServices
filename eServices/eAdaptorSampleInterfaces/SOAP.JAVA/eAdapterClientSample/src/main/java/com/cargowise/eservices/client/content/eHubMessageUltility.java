package com.cargowise.eservices.client.content;

import java.nio.charset.StandardCharsets;

import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.adapter.interfaces.IMessageInbox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.Stream;

/**
 * eHubMessageUtility prints out the eHub Message.
 */
public final class eHubMessageUltility {
	private eHubMessageUltility() { }
	/**
	 * displays the Inbox of MEssages.
	 * @param senderOrReciepient string to indicate Sender or Recipient of the message.
	 * @param inbox inbox of messages
	 */
	public static void displayInboxMessages(String senderOrReciepient, IMessageInbox inbox) {
		for (IeHubMessage eHubMessage : inbox) {
			System.out.println("=================================Message=============================================");
			System.out.println("Message: " + eHubMessage);
			System.out.println("Content: " + eHubMessage.getMessageStream());
			System.out.println("=====================================================================================");
		}
	}

	/**
	 * Generates a Message for the eHub MEssage.
	 * @param sender message Sender
	 * @param recipient of the generated message
	 * @return a generated message
	 * @throws eHubAdapterException Exception of the eHub Adapter
	 */
	public static eHubMessage generateMessage(String sender, String recipient) throws eHubAdapterException {
		return generateMessage(Guid.newGuid(), sender, recipient);
	}

	/**
	 * generates a message for the Ehub Message.
	 * @param guid the guid of the generated message
	 * @param sender person who sent the message
	 * @param recipient of the generated message
	 * @return eHub Message
	 * @throws eHubAdapterException exception of the class
	 */
	public static eHubMessage generateMessage(Guid guid, String sender, String recipient) throws eHubAdapterException {
		return new eHubMessage(guid, sender, recipient,
				MessageSchemaType.Xml, "UDM", "http://www.edi.com.au/EnterpriseService/#XmlInterchange",
				buildMessage(), "Email Subject Test", "File Name Test");
	}

	/**
	 * Builds the Message within a Stream.
	 * @return a Stream with the message information within it.
	 */
	public static Stream buildMessage() {
		StringBuilder sb = new StringBuilder();
		sb.append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
		sb.append("<XmlInterchange xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Version=\"1\" xmlns=\"http://www.edi.com.au/EnterpriseService/\">\r\n");
		sb.append("  <InterchangeInfo>\r\n");
		sb.append("    <Date>2010-11-17T07:53:29.2230000+11:00</Date>\r\n");
		sb.append("    <XmlType>Verbose</XmlType>\r\n");
		sb.append("  </InterchangeInfo>\r\n");
		sb.append("</XmlInterchange>");
		return new Stream(sb.toString().getBytes(StandardCharsets.UTF_8));
	}
}
