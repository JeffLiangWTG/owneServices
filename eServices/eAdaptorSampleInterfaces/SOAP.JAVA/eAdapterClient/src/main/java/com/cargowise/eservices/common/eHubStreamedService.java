package com.cargowise.eservices.common;

/**
 * An interface for eHub gateway streamed service client.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface eHubStreamedService {
	/** Ping the web service.
	 * @return the status of ping request(true/false)
	 * @throws Exception error during trying to ping the server
	 * @see com.cargowise.eservices.common.request.PingRequest */
	boolean ping() throws Exception;
	/** Send messages from the outgoing message mailbox to the web service.
	 * @param request the request of sending stream's SOAP action
	 * @throws Exception error while sending messages */
	void sendStream(SendStreamRequest request) throws Exception;
	/** Retrieve messages from the web service to the incoming message mailbox.
	 * @return the response from web service.
	 * @throws Exception error while retrieving messages */
	RetrieveStreamResponse retrieveStream() throws Exception;
	/** Finalise a message after retrieving and processing.
	 * @param trackingID matching batch ID of the SOAP message from eService web service.
	 * @throws Exception error while finalising messages */
	void finaliseBatch(Guid trackingID) throws Exception;
	/** Unsupported GetMessageStatuses Web Service Action.
	 * @param trackingIDs unsupported
	 * @return unsupported
	 * @throws Exception unsupported */
	java.util.HashMap<String, MessageStatus> getMessageStatuses(String[] trackingIDs) throws Exception;
}
