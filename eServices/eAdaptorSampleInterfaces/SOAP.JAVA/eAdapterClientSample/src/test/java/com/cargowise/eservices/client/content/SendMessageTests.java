package com.cargowise.eservices.client.content;

import java.util.Map.Entry;
import java.util.concurrent.CancellationException;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.ScheduledExecutorService;

import org.junit.Test;
import org.mockito.Mockito;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.common.Action;

public class SendMessageTests extends TestCaseBase {
	static final String USERNAME = "Test Username";
	static final String PASSWORD = "testpassword";
	static final String WEB_SERVICE_ADDRESS = "testWebServiceAddress.com/service";

	@Test
	public void testSendMessage_Success() throws Exception {
		DataStore dataStore = new DataStore();
		final SendMessage sendMessageTask = Mockito.spy(new SendMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(sendMessageTask.getNewEHubAdapter()).toReturn(adapter);
		final eHubMessage message = eHubMessageUltility.generateMessage(USERNAME, "RecipientID");

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) {
				adapter.getOutbox().clear();
				return null;
			}
		}).when(adapter).sendMessages();

		dataStore.add(message, MessageStatus.Queued);

		assertException(new Action() {
			public void run() throws Exception {
				sendMessageTask.run();
			}
		}, CancellationException.class, null);

		assertEquals(1, dataStore.size());
		Entry<eHubMessage, MessageStatus> receivedMessage = dataStore.entrySet().iterator().next();
		assertEquals(message.getTrackingID(), receivedMessage.getKey().getTrackingID());
		assertEquals(MessageStatus.Pending, receivedMessage.getValue());
		assertEquals(0, adapter.getOutbox().count());
	}

	@Test
	public void testSendMessage_ExceedMaxRunningCount() throws Exception {
		DataStore dataStore = new DataStore();
		final SendMessage sendMessageTask = Mockito.spy(new SendMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(sendMessageTask.getNewEHubAdapter()).toReturn(adapter);
		final eHubMessage message = eHubMessageUltility.generateMessage(USERNAME, "RecipientID");

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) {
				// do nothing
				return null;
			}
		}).when(adapter).sendMessages();

		dataStore.add(message, MessageStatus.Queued);

		assertException(new Action() {
			public void run() throws Exception {
				sendMessageTask.run();
			}
		}, CancellationException.class, "[Warning]SendMessage Task could not send a message as expected after 1 runs.");

		assertEquals(1, dataStore.size());
		Entry<eHubMessage, MessageStatus> receivedMessage = dataStore.entrySet().iterator().next();
		assertEquals(message.getTrackingID(), receivedMessage.getKey().getTrackingID());
		assertEquals(MessageStatus.Queued, receivedMessage.getValue());
		assertEquals(1, adapter.getOutbox().count());
	}

	@Test
	public void testThrowExceptionInSubThread() throws Exception {
		SendMessage sendMessageTask = Mockito.spy(new SendMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, null, 1));
		Mockito.stub(sendMessageTask.getNewEHubAdapter()).toThrow(new Exception("test exception"));

		ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
		final Future<?> future = executor.submit(sendMessageTask);

		assertException(new Action() {
			public void run() throws Exception {
				future.get();
			}
		}, ExecutionException.class, "java.lang.RuntimeException: java.lang.Exception: test exception");
	}

	@Test
	eHubAdapter createMockEHubAdapter() throws Exception {
		eHubAdapter mockAdapter = Mockito.mock(eHubAdapter.class);
		MessageOutboxMock outbox = new MessageOutboxMock();
		Mockito.stub(mockAdapter.getOutbox()).toReturn(outbox);
		return mockAdapter;
	}
}
