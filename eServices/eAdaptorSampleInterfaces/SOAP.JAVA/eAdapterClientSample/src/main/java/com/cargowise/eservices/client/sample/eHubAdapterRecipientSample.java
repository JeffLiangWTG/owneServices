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
import com.cargowise.eservices.client.content.DataStore;
import com.cargowise.eservices.client.content.MessageStatus;
import com.cargowise.eservices.client.content.ReceiveMessage;
import com.google.common.annotations.VisibleForTesting;

/**
 * eHubAdapterRecipientSample: present how recipient receives the message after eHubAdapterSenderSample sent that message.
 */
public final class eHubAdapterRecipientSample {
	private eHubAdapterRecipientSample() { }
	static final String WEB_SERVICE_ADDRESS = "https://localhost/eHubGateway/eHubStreamedService.svc";
	static final String USERNAME = "RecipientID";
	static final String PASSWORD = "Password";
	static final int NUMBER_OF_RUNNING_THREAD_FOR_TASKS = 2;
	static final int TASK_FIXED_RATE_DELAY = 0;
	static final int TASK_FIXED_RATE_PERIOD = 1;
	static final TimeUnit TASK_TIME_UNIT = TimeUnit.SECONDS;
	static final int MAXIMUM_INTEVAL_FOR_EACH_TASK = 20;

	/**
	 * main of eHubAdapterRecipientSample.
	 * @param args string array for args
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

		// If success to ping the server, create a periodic task to receive messages from server
		if (pingResult) {
			// Create ScheduledExecutorService to run tasks periodically and parallel in multiple threads.
			int availableProcessors = Runtime.getRuntime().availableProcessors();
			ScheduledExecutorService scheduledExecutorService = Executors.newScheduledThreadPool(availableProcessors < NUMBER_OF_RUNNING_THREAD_FOR_TASKS
					? availableProcessors : NUMBER_OF_RUNNING_THREAD_FOR_TASKS);

			// Create new DataStore which presents a database table of the messages and their status.
			DataStore dataStore = getNewDataStore();

			// Create a periodic task to receive messages from server to this UserNameID and put them to DataStore.
			ReceiveMessage receiveMessageTask = getNewReceiveMessageTask(dataStore);

			// Schedule that task periodically
			Future<?> receiveMessageFuture = scheduledExecutorService.scheduleAtFixedRate(receiveMessageTask, TASK_FIXED_RATE_DELAY, TASK_FIXED_RATE_PERIOD, getTaskTimeUnit());

			// Let the main thread waits that task to complete. Throw any exception in the task if it occurs.
			// (HINT: you could run eHubAdapterSenderSample while eHubAdapterRecipientSample is running, in order to see expected behaviors.)
			waitFutureTaskCompleteAndCaptureExceptionIfOccur(receiveMessageFuture);

			// Print the messages and their status in DataStore after processed.
			System.out.println(String.format("There are %1$d message(s) in DataStore.", dataStore.size()));
			System.out.println("============================Status Incoming Messages=================================");
			for (Entry<eHubMessage, MessageStatus> entry : dataStore.entrySet()) {
				System.out.println(String.format("%1$s - %2$s", entry.getKey().getTrackingID(), entry.getValue().name()));
			}
			System.out.println("=====================================================================================");
		}
	}

	/*
	 * Let the main thread waits the future task to complete. Throw any exception in the task if it occurs.
	 */
	static void waitFutureTaskCompleteAndCaptureExceptionIfOccur(Future<?> receiveMessageFuture) throws InterruptedException, ExecutionException {
		try {
			receiveMessageFuture.get();
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

	static ReceiveMessage getNewReceiveMessageTask(DataStore dataStore) {
		return new ReceiveMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, MAXIMUM_INTEVAL_FOR_EACH_TASK);
	}

	static TimeUnit getTaskTimeUnit() {
		return TASK_TIME_UNIT;
	}

	static DataStore getNewDataStore() {
		return new DataStore();
	}

	@VisibleForTesting
	static eHubAdapter getNewEHubAdapter() throws Exception {
		return new eHubAdapter(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD);
	}
}
