package com.cargowise.eservices.adapter;

import com.cargowise.eservices.adapter.interfaces.IMessageOutbox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;

/**
 * A mailbox for sending messages.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class MessageOutbox extends MessageBox implements IMessageOutbox {

	/** Default constructor. */
	public MessageOutbox() { }

	/**
	 * Add {@link IeHubMessage} into Outbox.
	 * @param message this is the message inside of the outbox
	 */
	public void addMessage(final IeHubMessage message) {
		getMessageList().add(message);
	}
}
