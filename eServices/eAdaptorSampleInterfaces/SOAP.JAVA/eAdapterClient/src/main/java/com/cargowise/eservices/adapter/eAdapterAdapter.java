package com.cargowise.eservices.adapter;

import java.io.IOException;

import javax.xml.soap.SOAPException;

import com.cargowise.eservices.common.SendStreamRequest;
import com.cargowise.eservices.common.ServiceClient;
import com.google.common.annotations.VisibleForTesting;

import edu.umd.cs.findbugs.annotations.SuppressFBWarnings;

import com.cargowise.eservices.common.eAdapterStreamedService;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;

/**
 * An adapter connecting to the eAdapter Inbound web service, it contains a
 * mailbox of outcoming messages ({@link com.cargowise.eservices.adapter.MessageOutbox}).
 * <br><br>
 * {@inheritDoc}
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
@SuppressWarnings("checkstyle:typename")
public class eAdapterAdapter extends Adapter { //NOPMD: TypeName
	private eAdapterStreamedService client;

	@SuppressFBWarnings()
	eAdapterAdapter() throws Exception {
		super();
	}

	/**
	 * A constructor specifying the web service's authentications for the connection.
	 * @param serverAddress the url of the web service
	 * @param clientID the client ID to login
	 * @param password the password to login
	 * @throws Exception exceptions could occur when create a new service client.
	 */
	@SuppressWarnings("PMD.ConstructorCallsOverridableMethod")
	public eAdapterAdapter(final String serverAddress, final String clientID, final String password) throws Exception {
		this();
		setup(serverAddress, clientID, password);
	}

	/**
	 * setup.
	 * @param serverAddress Address for the Server.
	 * @param clientID Clients ID
	 * @param password Clients Password.
	 */
	public final void setup(final String serverAddress, final String clientID, final String password) {
		client = createService(serverAddress, clientID, password);
	}

	/**
	 * {@inheritDoc}
	 */
	@Override
	public boolean ping() throws Exception {
		return client.ping();
	}

	@Override
	protected final void sendMessagesCore(final SendStreamRequest sendRequest) throws Exception {
		client.sendStream(sendRequest);
	}

	/**
	 * {@inheritDoc}
	 */
	@Override
	public void close() throws SOAPException, IOException {
		((ServiceClient) client).close();
		super.close();
	}

	/**
	 * Creating a new client a.k.a. Service..
	 * @param serverAddress Address for the Server
	 * @param clientID Clients Identification Number
	 * @param password Clients Password
	 * @return a new client
	 */
	@VisibleForTesting
	eAdapterStreamedService createService(final String serverAddress, final String clientID, final String password) {
		return createClient(serverAddress, clientID, password);
	}

	/**
	 * E-Adapter Streamer Service.
	 * @param serverAddress The address of the Server
	 * @param clientID The Clients Identification number
	 * @param password Clients Password
	 * @return serviceClient
	 */
	public final eAdapterStreamedService createClient(final String serverAddress, final String clientID, final String password) {
		eAdapterStreamedServiceClient serviceClient = new eAdapterStreamedServiceClient(serverAddress);
		serviceClient.getClientCredentials().setUsername(clientID);
		serviceClient.getClientCredentials().setPassword(password);
		return serviceClient;
	}
}
