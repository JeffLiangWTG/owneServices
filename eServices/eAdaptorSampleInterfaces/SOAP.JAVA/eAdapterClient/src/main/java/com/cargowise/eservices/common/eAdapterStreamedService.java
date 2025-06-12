package com.cargowise.eservices.common;

/**
 * An interface for EAdapter Inbound Web Service client.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface eAdapterStreamedService {
	/** Ping the web service.
	 * @throws Exception error while ping the web service
	 * @return the status of ping message successfulness */
	boolean ping() throws Exception;
	/** Send messages from the outgoing message mailbox to the web service.
	 * @param request the request of sending stream's SOAP action
	 * @throws Exception error while sending messages */
	void sendStream(SendStreamRequest request) throws Exception;
}
