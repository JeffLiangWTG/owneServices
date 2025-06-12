package com.cargowise.eservices.client.content;

import java.util.List;
import java.util.concurrent.CancellationException;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.adapter.interfaces.IMessageOutbox;
import com.cargowise.eservices.adapter.interfaces.IeHubAdapter;

/**
 * SendMessage: a task to send a message using eHubAdapter.
 */
public class SendMessage implements Runnable {
	private final String username;
	private final String password;
	private String eHubStreamServiceAddress;
	private final DataStore dataStore;
	private final int maximumRunningCount;
	private int runningCount;
	private boolean isSent;

	/**
	 * 
	 * @param eHubStreamServiceAddress Stream Service Address
	 * @param username Users username
	 * @param password Users password
	 * @param dataStore datastore of the message
	 * @param maximumRunningCount maximum count of the running count
	 */
	public SendMessage(String eHubStreamServiceAddress, String username, String password, DataStore dataStore, int maximumRunningCount) {
		this.username = username;
		this.password = password;
		this.eHubStreamServiceAddress = eHubStreamServiceAddress;
		this.dataStore = dataStore;
		this.maximumRunningCount = maximumRunningCount;
		runningCount = 0;
		isSent = false;
	}

	/**
	 * Create eHubAdapter to create connection with the web service
	 * Load all eHubMessage in DataStore with Queued status.
	 * Add all Queued message to adapter.Outbox.
	 * Send those message to the server.
	 * Finalize those messages after sending.
	 * Stop this task when:
	 * 	-	Successful retrieve any queued message from DataStore and send it to server
	 * 	-	Running count exceeds its limit, show warning message
	 */
	public void run() {
		try {
			IeHubAdapter adapter = getNewEHubAdapter();
			List<eHubMessage> outgoingQueuedMessages = dataStore.getMessages(MessageStatus.Queued);
			fillOutbox(adapter.getOutbox(), outgoingQueuedMessages);
			adapter.sendMessages();
			finalizeMessages(outgoingQueuedMessages, adapter.getOutbox());
			adapter.close();
		} catch (CancellationException e) {
			throw e;
		} catch (Exception e) {
			throw new RuntimeException(e);
		}
		if (isSent) {
			throw new CancellationException();
		}
		if (++runningCount >= maximumRunningCount) {
			throw new CancellationException(String.format("[Warning]SendMessage Task could not send a message as expected after %d runs.", runningCount));
		}
	}

	/*
	 * Create eHubAdapter to create connection with the web service
	 */
	IeHubAdapter getNewEHubAdapter() throws Exception {
		return new eHubAdapter(eHubStreamServiceAddress, username, password);
	}

	/*
	 * Add all Queued message to adapter.Outbox.
	 */
	void fillOutbox(IMessageOutbox outbox, List<eHubMessage> outgoingQueuedMessages) {
		for (eHubMessage message : outgoingQueuedMessages) {
			outbox.addMessage(message);
		}
	}

	/*
	 * Finalize those messages after sending as updating their status to pending.
	 */
	void finalizeMessages(List<eHubMessage> outgoingQueuedMessages, IMessageOutbox outbox) throws InvalidDataStoreKeyException {
		int sentMessageCount = outgoingQueuedMessages.size() - outbox.count();
		if (sentMessageCount > 0) {
			for (eHubMessage message : outgoingQueuedMessages) {
				dataStore.update(message, MessageStatus.Pending);
			}
			System.out.println(String.format("[%1$s]Success - Sent %2$d message(s)", username,
					sentMessageCount));
			isSent = true;
		}
	}

	/**
	 * @return the eHubStreamServiceAddress
	 */
	public String geteHubStreamServiceAddress() {
		return eHubStreamServiceAddress;
	}

	/**
	 * @param eHubStreamServiceAddress the eHubStreamServiceAddress to set
	 */
	public void seteHubStreamServiceAddress(String eHubStreamServiceAddress) {
		this.eHubStreamServiceAddress = eHubStreamServiceAddress;
	}

	/**
	 * is sent.
	 * @return that it's sent
	 */
	public boolean isSent() {
		return isSent;
	}

	/**
	 * set sent.
	 * @param isSent1 set sent
	 */
	public void setSent(boolean isSent1) {
		this.isSent = isSent1;
	}
}
