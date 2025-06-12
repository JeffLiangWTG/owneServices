package com.cargowise.eservices.client.content;

import java.security.InvalidParameterException;

import com.cargowise.eservices.adapter.interfaces.IeHubMessage;

/**
 * MessageStatus:  present 6 status states of a message
 * 	Queued: First initial state of a message which is waiting for be processed.
 * 	Pending: The status is changed to Pending after the message is sent to the server.
 * 	Failed: There is a failure while sending a message to recipient.
 * 	Sent: Successful send a message to recipient.
 * 	Acknowledged: After the message is successfully sent to recipient, recipient will send back an acknowledge status message.
 * 	Received: Successful receive a message.
 */
public enum MessageStatus {
	Queued, Pending, Failed, Acknowledged, Sent, Received;

	/**
	 * Get a matching previous status(before updating it) with the candidate status.
	 * @param messageStatus this is the message status
	 * @return messageStatus
	 */
	public static MessageStatus getBestMatchOutgoingInterchangeStatus(MessageStatus messageStatus) {
		switch (messageStatus) {
		case Pending:
			return Queued;
		case Failed:
		case Sent:
			return Pending;
		case Acknowledged:
			return Sent;
		default:
			throw new InvalidParameterException("MessageStatus." + messageStatus.name() + " does not have any matching outgoing interchange status.");
		}
	}

	/**
	 * Get a matching MessageStatus from a incoming status message.
	 * @param statusMessage is the status of the message (Sent/Failed etc).
	 * @return messageStatus (Status of the message)
	 */
	public static MessageStatus getMatchingMessageStatus(IeHubMessage statusMessage) {
		String schemaName = statusMessage.getSchemaName();
		if (schemaName.equals(MessageStatus.MSA)) {
			return MessageStatus.Acknowledged;
		} else if (schemaName.equals(MessageStatus.MSF)) {
			return MessageStatus.Failed;
		} else if (schemaName.equals(MessageStatus.MSS)) {
			return MessageStatus.Sent;
		}
		return MessageStatus.Received;
	}

	public static final String MSF = "MessageStatusFailed";
	public static final String MSS = "MessageStatusSuccess";
	public static final String MSA = "http://cargowise.com/ehub/core/2014/09#AcknowledgementReceived";
}
