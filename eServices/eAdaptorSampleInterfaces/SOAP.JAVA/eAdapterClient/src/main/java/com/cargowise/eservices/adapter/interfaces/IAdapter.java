package com.cargowise.eservices.adapter.interfaces;

/**
 * An adapter interface which includes outgoing mailbox and sending messages function.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IAdapter {
	/** allow the adapter to get outgoing message mailbox object.
	 * @return an Out-bound Mailbox */
	IMessageOutbox getOutbox();
	/** allow the adapter to send message(s) to a web service.
	 * @throws Exception errors when sending messages */
	void sendMessages() throws Exception;
}
