package com.cargowise.eservices.common;

import javax.xml.soap.SOAPConnection;
import javax.xml.soap.SOAPConnectionFactory;
import javax.xml.soap.SOAPException;

import com.cargowise.eservices.common.request.PingRequest;
import com.cargowise.eservices.common.request.EServiceSOAPRequest;

/**
 * A abstract class for eService web service client.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public abstract class ServiceClient {
	private ClientCredentials clientCredentials;
	private String endpoint;

	/**
	 * A constructor specifying the endpoint of web service address.
	 * @param endpoint endpoint url of the web service
	 */
	public ServiceClient(final String endpoint) {
		this.endpoint = endpoint;
	}

	/**
	 * Get existing or create new {@link ClientCredentials}.
	 * @return the client credentials within the SOAP message.
	 */
	public ClientCredentials getClientCredentials() {
		if (clientCredentials == null) {
			clientCredentials = new ClientCredentials();
		}
		return clientCredentials;
	}

	/**
	 * @param clientCredentials the clientCredentials to set
	 */
	protected void setClientCredentials(final ClientCredentials clientCredentials) {
		this.clientCredentials = clientCredentials;
	}

	/**
	 * Ping the web service.
	 * @return the status of ping request(true/false)
	 * @throws Exception error during trying to ping the server
	 * @see com.cargowise.eservices.common.request.PingRequest
	 */
	public boolean ping() throws Exception {
		return Boolean.parseBoolean((String) sendRequest(new PingRequest(getClientCredentials())));
	}

	/**
	 * Sends a request for the Service Client using the SOAP request as a hand in.
	 * @param request is the response from the pre sent request.
	 * @return request response
	 * @throws Exception sendRequest
	 */
	protected Object sendRequest(final EServiceSOAPRequest request) throws Exception {
		return request.sendRequestAndGetRespone(endpoint, getConnection());
	}

	final SOAPConnection getConnection() throws UnsupportedOperationException, SOAPException {
		if (soapConnection == null) {
			soapConnection = SOAPConnectionFactory.newInstance().createConnection();
		}
		return soapConnection;
	}
	private SOAPConnection soapConnection;

	@Override
	protected final void finalize() throws SOAPException {
		close();
	}

	/**
	 * 
	 * @throws SOAPException as
	 */
	public final void close() throws SOAPException {
		if (soapConnection != null) {
			soapConnection.close();
		}
	}
}
