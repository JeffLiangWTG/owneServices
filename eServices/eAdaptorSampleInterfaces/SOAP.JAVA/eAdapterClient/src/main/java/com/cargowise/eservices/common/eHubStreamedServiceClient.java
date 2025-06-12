package com.cargowise.eservices.common;

import java.util.HashMap;

import javax.xml.soap.SOAPException;

import com.cargowise.eservices.common.request.FinaliseBatchRequest;
import com.cargowise.eservices.common.request.RetrieveStreamResponseRequest;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;


/**
 * eHub Gateway streamed service client.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class EHubStreamedServiceClient extends ServiceClient implements eHubStreamedService {
	/**
	 * A constructor specifying the endpoint of web service address.
	 * @param endpoint endpoint of web service address
	 */
	public EHubStreamedServiceClient(final String endpoint) {
		super(endpoint);
	}

	/**
	 * {@inheritDoc}
	 * @see SendStreamRequest
	 */
	public void sendStream(final SendStreamRequest request) throws Exception {
		sendRequest(new SendMessageStreamRequest(getClientCredentials(), request));
	}

	/**
	 * {@inheritDoc}
	 */
	public HashMap<String, MessageStatus> getMessageStatuses(final String[] trackingIDs) {
		throw new UnsupportedOperationException("Unsupported GetMessageStatuses Web Service Action.");
	}

	/**
	 * {@inheritDoc}
	 * @see FinaliseBatchRequest
	 */
	public void finaliseBatch(final Guid trackingID) throws SOAPException, Exception {
		sendRequest(new FinaliseBatchRequest(getClientCredentials(), trackingID));
	}

	/**
	 * {@inheritDoc}
	 * @see RetrieveStreamResponseRequest
	 */
	public RetrieveStreamResponse retrieveStream() throws Exception {
		return (RetrieveStreamResponse) sendRequest(new RetrieveStreamResponseRequest(getClientCredentials()));
	}
}
