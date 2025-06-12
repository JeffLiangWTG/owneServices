package com.cargowise.eservices.client.content;

import java.util.concurrent.CancellationException;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.adapter.interfaces.IMessageInbox;
import com.cargowise.eservices.adapter.interfaces.IeHubAdapter;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;

/**
 * ReceiveMessage: a task for receive a message using eHubAdapter.
 * 
 */
public class ReceiveMessage implements Runnable {
	private final String username;
	private final String password;
	private final String eHubStreamServiceAddress;
	private final DataStore dataStore;
	private final int maximumRunningCount;
	private int runningCount;
	private boolean isReceived;

	/**
	 * Receive Message Function.
	 * @param eHubStreamServiceAddress Stream Service Address.
	 * @param username users username
	 * @param password users password
	 * @param dataStore data store of the user.
	 * @param maximumRunningCount maximum running count of the message
	 */
	public ReceiveMessage(String eHubStreamServiceAddress, String username, String password, DataStore dataStore, int maximumRunningCount) {
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
	 * Download those messages from adapter.Inbox and add them to DataStore with Received status.
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
			throw new CancellationException(String.format("[Warning]ReceiveMessage Task could not receive a message as expected after %d runs.", runningCount));
		}
	}

	/*
	 * Create eHubAdapter to create connection with the web service
	 */
	IeHubAdapter getNewEHubAdapter() throws Exception {
		return new eHubAdapter(eHubStreamServiceAddress, username, password);
	}

	/*
	 * Download those messages from adapter.Inbox and add them to DataStore with Received status.
	 */
	void downloadMessages(IeHubAdapter adapter) throws InvalidDataStoreKeyException {
		IMessageInbox inbox = adapter.getInbox();
		if (inbox.count() > 0) {
			System.out.println(String.format("[%s]Success - Received %d message(s)", username, adapter.getInbox().count()));
			for (IeHubMessage message : adapter.getInbox()) {
				processMessage(message);
			}
			eHubMessageUltility.displayInboxMessages(username, inbox);
			isReceived = true;
		}
	}

	/*
	 * Process each Message as adding them to DataStore with Received status.
	 */
	void processMessage(IeHubMessage message) throws InvalidDataStoreKeyException {
		dataStore.add((eHubMessage) message, MessageStatus.Received);
	}
}
