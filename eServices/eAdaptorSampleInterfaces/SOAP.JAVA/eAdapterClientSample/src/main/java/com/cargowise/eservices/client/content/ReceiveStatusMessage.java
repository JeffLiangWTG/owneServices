package com.cargowise.eservices.client.content;

import java.util.concurrent.CancellationException;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.adapter.interfaces.IMessageInbox;
import com.cargowise.eservices.adapter.interfaces.IeHubAdapter;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;

/**
 * ReceiveStatusMessage: a task for receive a status message using eHubAdapter and how to process it.
 */
public class ReceiveStatusMessage implements Runnable {
	private final String username;
	private final String password;
	private final String eHubStreamServiceAddress;
	private final DataStore dataStore;
	private final int maximumRunningCount;
	private int runningCount;
	private boolean isReceived;

	/**
	 * Status of the received message.
	 * @param eHubStreamServiceAddress Stream Service Address.
	 * @param username Users username
	 * @param password Users password
	 * @param dataStore Data Store of the message
	 * @param maximumRunningCount Maximum count of the running count
	 */
	public ReceiveStatusMessage(String eHubStreamServiceAddress, String username, String password, DataStore dataStore, int maximumRunningCount) {
		this.username = username;
		this.password = password;
		this.eHubStreamServiceAddress = eHubStreamServiceAddress;
		this.dataStore = dataStore;
		this.maximumRunningCount = maximumRunningCount;
		runningCount = 0;
		isReceived = false;
	}

	/**
	 * Create eHubAdapter to create connection with the web service
	 * Retrieved messages from the server to adapter.Inbox
	 * Download those messages from adapter.Inbox, identify each message if it is a status message and process it.
	 * Mark as read these message to notify the server that you successfully received those messages.
	 * Stop this task when:
	 * 	-	Successful retrieve any message from server
	 * 	-	Running count exceeds its limit, show warning message
	 */
	public void run() {
		try {
			IeHubAdapter adapter = getNewEHubAdapter();
			adapter.retrieveMessages();
			downloadMessages(adapter);
			adapter.getInbox().markAsRead();
			adapter.close();
		} catch (CancellationException e) {
			throw e;
		} catch (Exception e) {
			throw new RuntimeException(e);
		}
		if (isReceived) {
			throw new CancellationException();
		}
		if (++runningCount >= maximumRunningCount) {
			throw new CancellationException(String.format("[Warning]ReceiveStatusMessage Task could not receive a status message as expected after %d runs.", runningCount));
		}
	}

	/*
	 * Create eHubAdapter to create connection with the web service
	 */
	IeHubAdapter getNewEHubAdapter() throws Exception {
		return new eHubAdapter(eHubStreamServiceAddress, username, password);
	}

	/*
	 * Download those messages from adapter.Inbox, identify each message if it is a status message and process it.
	 */
	void downloadMessages(IeHubAdapter adapter) throws InvalidDataStoreKeyException {
		IMessageInbox inbox = adapter.getInbox();
		if (inbox.count() > 0) {
			System.out.println(String.format("[%s]Success - Received %d message(s)", username, adapter.getInbox().count()));
			for (IeHubMessage message : adapter.getInbox()) {
				processMessage(message);
			}
			eHubMessageUltility.displayInboxMessages(username, inbox);
		}
	}

	/*
	 * Process each message as identify if it is a status message and update the matching Pending message if found.
	 */
	void processMessage(IeHubMessage message) throws InvalidDataStoreKeyException {
		MessageStatus status = MessageStatus.getMatchingMessageStatus(message);
		handleStatusMessage(status, (eHubMessage) message);
	}

	/*
	 * Find matching pending message and try to update its status matching the status message.
	 */
	void handleStatusMessage(MessageStatus status, IeHubMessage statusMessage) throws InvalidDataStoreKeyException {
		MessageStatus bestMatchOutgoingInterchangeStatus = MessageStatus.getBestMatchOutgoingInterchangeStatus(status);
		eHubMessage matchingMessage = dataStore.getMessage(statusMessage.getTrackingID(), bestMatchOutgoingInterchangeStatus);
		if (matchingMessage != null) {
			System.out.println(String.format(
					"[%1$s]Matching outgoing Interchange with tracking id %2$s is updated with %3$s status.", username,
					matchingMessage.getTrackingID(), status.name()));
			dataStore.update(matchingMessage, status);
		} else {
			System.out.println(String.format(
					"[%1$s]Found one status message with trackingID %2$s not matching any outgoing messages.", username,
					statusMessage.getTrackingID()));
		}
		isReceived = true;
	}
}
