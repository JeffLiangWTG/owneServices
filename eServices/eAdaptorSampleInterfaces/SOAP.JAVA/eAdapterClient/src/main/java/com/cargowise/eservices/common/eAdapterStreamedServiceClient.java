package com.cargowise.eservices.common;

import javax.xml.soap.SOAPException;

import com.cargowise.eservices.common.request.SendMessageStreamRequest;

import edu.umd.cs.findbugs.annotations.SuppressFBWarnings;

/**
 * EAdapter Inbound Web Service client.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class eAdapterStreamedServiceClient extends ServiceClient implements eAdapterStreamedService {
	/**
	 * A constructor specifying the endpoint of web service address.
	 * @param endpoint of web service address
	 */
	@SuppressFBWarnings(value = "NM_CLASS_NAMING_CONVENTION", justification = "keep the same naming convention as C#")
	public eAdapterStreamedServiceClient(final String endpoint) {
		super(endpoint);
	}

	/**
	 * {@inheritDoc}
	 * @see SendStreamRequest
	 */
	public void sendStream(final SendStreamRequest request) throws Exception {
		sendRequest(getSendMessageStreamRequest(request));
	}

	final SendMessageStreamRequest getSendMessageStreamRequest(final SendStreamRequest request) throws SOAPException {
		return new SendMessageStreamRequest(getClientCredentials(), request);
	}
}
