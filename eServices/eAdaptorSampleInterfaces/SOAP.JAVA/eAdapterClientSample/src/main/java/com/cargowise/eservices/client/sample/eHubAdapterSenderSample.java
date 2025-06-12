package com.cargowise.eservices.client.sample;

import java.util.Map.Entry;
import java.util.concurrent.CancellationException;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.client.content.DataStore;
import com.cargowise.eservices.client.content.MessageStatus;
import com.cargowise.eservices.client.content.ReceiveStatusMessage;
import com.cargowise.eservices.client.content.SendMessage;
import com.cargowise.eservices.client.content.eHubMessageUltility;

/**
 * eHubAdapterSenderSample: present how a message is sent and updated its status.
 */
public final class eHubAdapterSenderSample {
	private eHubAdapterSenderSample() { }
	static final String WEB_SERVICE_ADDRESS = "https://{hostAddress}/eHubGateway/eHubStreamedService.svc";
	static final String USERNAME = "SenderID";
	static final String PASSWORD = "Password";
	static final String RECIPIENT = "RecipientID";
	static final int NUMBER_OF_RUNNING_THREAD_FOR_TASKS = 2;
	static final int TASK_FIXED_RATE_DELAY = 0;
	static final int TASK_FIXED_RATE_PERIOD = 1;
	static final TimeUnit TASK_TIME_UNIT = TimeUnit.SECONDS;
	static final int MAXIMUM_INTEVAL_FOR_EACH_TASK = 20;

	/**
	 * main of eHubAdapterSenderSample.
	 * @param args string array args
	 * @throws Exception throws this exception
	 */
	public static void main(String[] args) throws Exception {
		// Create new adapter to connect to the web services
		eHubAdapter adapter = getNewEHubAdapter();
		// You could test the connection by trying to ping the server
		boolean pingResult = adapter.ping();
		System.out.println(String.format("Ping %1$s: %2$s", WEB_SERVICE_ADDRESS, pingResult));
		// Close connection
		adapter.close();

		// If success to ping the server:
		//	- Create a periodic task to send queued messages from DataStore to server and change these messages to Pending status.
		// 	- Create a periodic task to receive status messages matching pending messages in DataStore and update those messages' status.
		if (pingResult) {
			// Create ScheduledExecutorService to run tasks periodically and parallel in multiple threads.
			int availableProcessors = Runtime.getRuntime().availableProcessors();
			ScheduledExecutorService scheduledExecutorService = Executors.newScheduledThreadPool(availableProcessors < NUMBER_OF_RUNNING_THREAD_FOR_TASKS
							? availableProcessors : NUMBER_OF_RUNNING_THREAD_FOR_TASKS);

			// Create new DataStore which presents a database table of the messages and their status.
			DataStore dataStore = getNewDataStore();

			// Create a periodic task to send queued messages from DataStore to server and change these messages to Pending status.
			SendMessage sendMessageTask = getNewSendMessageTask(dataStore);
			// Create a periodic task to receive status messages matching pending messages in DataStore and update those messages' status.
			ReceiveStatusMessage receiveStatusMessageTask = getNewReceiveStatusMessageTask(dataStore);

			// Schedule those tasks periodically
			Future<?> sendMessageFuture = scheduledExecutorService.scheduleAtFixedRate(sendMessageTask, TASK_FIXED_RATE_DELAY, TASK_FIXED_RATE_PERIOD, getTaskTimeUnit());
			Future<?> receiveStatusMessageFuture = scheduledExecutorService.scheduleAtFixedRate(receiveStatusMessageTask, TASK_FIXED_RATE_DELAY, TASK_FIXED_RATE_PERIOD, getTaskTimeUnit());

			// Add a queued Message to DataStore => expect SendMessage task will process and send this message
			// ReceiveStatusMessage task will try to receive a status message matching the message processed by SendMessage Task
			// (HINT: you could run eHubAdapterRecipientSample while eHubAdapterSenderSample is running, in order to see expected behaviors.)
			dataStore.add(generateMessage(), MessageStatus.Queued);

			// Let the main thread waits those tasks to complete. Throw any exception in those tasks if it occurs.
			waitFutureTaskCompleteAndCaptureExceptionIfOccur(sendMessageFuture);
			waitFutureTaskCompleteAndCaptureExceptionIfOccur(receiveStatusMessageFuture);

			// Print the messages and their status in DataStore after processed.
			System.out.println(String.format("There are %1$d message(s) in DataStore.", dataStore.size()));
			System.out.println("============================Status Outgoing Messages=================================");
			for (Entry<eHubMessage, MessageStatus> entry : dataStore.entrySet()) {
				System.out.println(String.format("%1$s - %2$s", entry.getKey().getTrackingID(), entry.getValue().name()));
			}
			System.out.println("=====================================================================================");
		}
	}

	/*
	 * Generate a message
	 */
	static eHubMessage generateMessage() throws eHubAdapterException {
		return eHubMessageUltility.generateMessage(USERNAME, RECIPIENT);
	}

	/*
	 * Let the main thread waits the future task to complete. Throw any exception in the task if it occurs.
	 */
	static void waitFutureTaskCompleteAndCaptureExceptionIfOccur(Future<?> futureTask) throws InterruptedException, ExecutionException {
		try {
			futureTask.get();
		} catch (ExecutionException e) {
			if (e.getCause() instanceof CancellationException) {
				String message = e.getCause().getMessage();
				if (message != null) {
					System.out.println(message);
				}
			} else {
				throw e;
			}
		}
	}

	static TimeUnit getTaskTimeUnit() {
		return TASK_TIME_UNIT;
	}

	static DataStore getNewDataStore() {
		return new DataStore();
	}

	static SendMessage getNewSendMessageTask(DataStore dataStore) {
		return new SendMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, MAXIMUM_INTEVAL_FOR_EACH_TASK);
	}

	static ReceiveStatusMessage getNewReceiveStatusMessageTask(DataStore dataStore) {
		return new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, MAXIMUM_INTEVAL_FOR_EACH_TASK);
	}

	static eHubAdapter getNewEHubAdapter() throws Exception {
		return new eHubAdapter(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD);
	}
}
