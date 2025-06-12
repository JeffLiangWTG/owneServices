package com.cargowise.eservices.adapter.interfaces;

import java.io.IOException;

import javax.xml.soap.SOAPException;

/**
 * An Adapter interface for eHub gateway web service.
 * It includes incoming/outgoing mailbox and methods matching
 * {@link com.cargowise.eservices.common.request.SendMessageStreamRequest} and
 * {@link com.cargowise.eservices.common.request.RetrieveStreamResponseRequest} SOAPAction.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IeHubAdapter extends IAdapter {
	/** allow the adapter to get incoming message mailbox object.
	 * @return an In bound Mailbox */
	IMessageInbox getInbox();
	/** allow the adapter to retrieve message(s) from a web service.
	 * @throws Exception errors when retrieving messages */
	void retrieveMessages() throws Exception;
	/** close connection.
	 * @throws SOAPException errors when closing SOAPConnectionFactory
	 * @throws IOException erros when closing messages' stream in Inbox */
	void close() throws SOAPException, IOException;
}
