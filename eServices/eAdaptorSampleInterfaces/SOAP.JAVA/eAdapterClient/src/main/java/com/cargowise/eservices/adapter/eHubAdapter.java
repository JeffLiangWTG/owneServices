package com.cargowise.eservices.adapter;

import java.io.IOException;

import javax.xml.soap.SOAPException;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.adapter.interfaces.IMessageInbox;
import com.cargowise.eservices.adapter.interfaces.IeHubAdapter;
import com.cargowise.eservices.adapter.interfaces.IeHubFinalizable;
import com.cargowise.eservices.common.Action;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.RetrieveStreamResponse;
import com.cargowise.eservices.common.SendStreamRequest;
import com.cargowise.eservices.common.ServiceClient;
import com.google.common.annotations.VisibleForTesting;

import edu.umd.cs.findbugs.annotations.SuppressFBWarnings;

import com.cargowise.eservices.common.eHubStreamedService;
import com.cargowise.eservices.common.EHubStreamedServiceClient;

/**
 * An adapter connecting to the eHub Gateway web service, it contains mailboxes
 * of incoming and outcoming messages ({@link com.cargowise.eservices.adapter.MessageInbox},
 * {@link com.cargowise.eservices.adapter.MessageOutbox}).
 * <br><br>
 * {@inheritDoc}
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
@SuppressFBWarnings(value = "NM_CLASS_NAMING_CONVENTION", justification = "keep the same naming convention as C#")
public class eHubAdapter extends Adapter implements IeHubAdapter, IeHubFinalizable {
	private eHubStreamedService client;
	private String clientID;

	eHubAdapter() throws Exception {
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
	public eHubAdapter(final String serverAddress, final String clientID, final String password) throws Exception {
		this();
		setup(serverAddress, clientID, password);
	}

	final void setup(final String serverAddress, final String theClientID, final String password) {
		this.clientID = theClientID;
		setClient(createService(serverAddress, theClientID, password));
	}

	/**
	 * Get incoming message mailbox.
	 * @return the Message Inbox of a user
	 */
	public IMessageInbox getInbox() {
		if (messageInbox == null) {
			messageInbox = createInbox();
		}
		return messageInbox;
	}

	private IMessageInbox messageInbox;

	/**
	 * Create a new inbox.
	 * @return the new inbox.
	 */
	@VisibleForTesting
	IMessageInbox createInbox() {
		return new MessageInbox(this, clientID);
	}

	@Override
	/**
	 */
	public boolean ping() throws Exception {
		return client.ping();
	}

	@Override
	protected final void sendMessagesCore(final SendStreamRequest sendRequest) throws Exception {
		client.sendStream(sendRequest);
	}

	/**
	 * Retrieve messages from the web service to the incoming message mailbox.
	 * @throws Exception if Retrieve Message is null
	 * @see com.cargowise.eservices.common.request.RetrieveStreamResponseRequest
	 */
	public void retrieveMessages() throws Exception {
		if (!getInbox().isRetrievableMessages()) {
			throw new eHubAdapterException("Execute getInbox().markAsRead() method before retrieve new Messages");
		}

		ServiceExceptionHandler.runEHubAction(new Action() {
			public void run() throws Exception {
				RetrieveStreamResponse response = client.retrieveStream();

				if (response.getMessages().length > 0) {
					getInbox().readMessageBatch(response.getTrackingID(), response.getMessages());
				}
			}
		});
	}

	/**
	 * Finalise a message after retrieving and processing. Usually called by calling {@link MessageInbox#markAsRead()}.
	 * @param batchID the variable to identify a finalised batch
	 * @throws Exception if finalising the batch fails to run an eHub action.
	 * @see com.cargowise.eservices.common.request.FinaliseBatchRequest
	 */
	public void finalise(final Guid batchID) throws Exception {
		ServiceExceptionHandler.runEHubAction(new Action() {
			public void run() throws Exception {
				client.finaliseBatch(batchID);
			}
		});
	}

	@Override
	/**
	 */
	public void close() throws SOAPException, IOException {
		super.close();

		if (messageInbox != null) {
			messageInbox.clear();
		}

		((ServiceClient) client).close();
	}

	/**
	 * Create a new service / Client.
	 * @param endpointAddress
	 * @param theClientID
	 * @param password
	 * @return the new Client
	 */
	@VisibleForTesting
	eHubStreamedService createService(final String endpointAddress, final String theClientID, final String password) {
		return createClient(endpointAddress, theClientID, password);
	}

	final eHubStreamedService createClient(final String endpointAddress, final String theClientID, final String password) {
		EHubStreamedServiceClient theClient = new EHubStreamedServiceClient(endpointAddress);
		theClient.getClientCredentials().setUsername(theClientID);
		theClient.getClientCredentials().setPassword(password);
		return theClient;
	}

	/**
	 * @return the client
	 */
	public eHubStreamedService getClient() {
		return client;
	}

	/**
	 * @param client the client to set
	 */
	public void setClient(final eHubStreamedService client) {
		this.client = client;
	}
}
