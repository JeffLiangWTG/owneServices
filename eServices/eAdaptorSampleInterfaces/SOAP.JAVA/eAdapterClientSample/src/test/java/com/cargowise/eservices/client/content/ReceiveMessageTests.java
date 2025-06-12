package com.cargowise.eservices.client.content;

import static org.junit.jupiter.api.Assertions.assertThrows;

import java.util.Map.Entry;
import java.util.concurrent.CancellationException;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.ScheduledExecutorService;

import org.junit.Test;
import org.mockito.Mockito;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.common.Action;

public class ReceiveMessageTests extends TestCaseBase {
	static final String USERNAME = "Test Username";
	static final String PASSWORD = "testpassword";
	static final String WEB_SERVICE_ADDRESS = "testWebServiceAddress.com/service";

	@Test
	public void testReceiveMessage() throws Exception {
		DataStore dataStore = new DataStore();
		final ReceiveMessage receiveMessageTask = Mockito.spy(new ReceiveMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(receiveMessageTask.getNewEHubAdapter()).toReturn(adapter);
		final eHubMessage message = eHubMessageUltility.generateMessage(USERNAME, "RecipientID");

		Mockito.doAnswer(invocation -> {
			((MessageInboxMock) adapter.getInbox()).addMessage(message);
			return null;
		}).when(adapter).retrieveMessages();

		assertThrows(CancellationException.class, () -> receiveMessageTask.run());

		assertEquals(1, dataStore.size());
		Entry<eHubMessage, MessageStatus> receivedMessage = dataStore.entrySet().iterator().next();
		assertEquals(message.getTrackingID(), receivedMessage.getKey().getTrackingID());
		assertEquals(MessageStatus.Received, receivedMessage.getValue());
	}

	@Test
	public void testReceiveMessage_ExceedMaxRunningCount() throws Exception {
		DataStore dataStore = new DataStore();
		final ReceiveMessage receiveMessageTask = Mockito.spy(new ReceiveMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(receiveMessageTask.getNewEHubAdapter()).toReturn(adapter);

		Mockito.doAnswer(invocation -> {
				// do nothing
				return null;
		}).when(adapter).retrieveMessages();

		CancellationException exception = assertThrows(CancellationException.class, () -> receiveMessageTask.run());
		assertEquals("[Warning]ReceiveMessage Task could not receive a message as expected after 1 runs.", exception.getMessage());
		assertEquals(0, dataStore.size());
	}

	@Test
	public void testThrowExceptionInSubThread() throws Exception {
		final ReceiveMessage receiveMessageTask = Mockito.spy(new ReceiveMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, null, 1));
		Mockito.stub(receiveMessageTask.getNewEHubAdapter()).toThrow(new Exception("test exception"));

		ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
		final Future<?> future = executor.submit(receiveMessageTask);

		assertException(new Action() {
			public void run() throws Exception {
				future.get();
			}
		}, ExecutionException.class, "java.lang.RuntimeException: java.lang.Exception: test exception");
	}

	@Test
	eHubAdapter createMockEHubAdapter() throws Exception {
		eHubAdapter mockAdapter = Mockito.mock(eHubAdapter.class);
		MessageInboxMock inbox = new MessageInboxMock();
		Mockito.stub(mockAdapter.getInbox()).toReturn(inbox);

		return mockAdapter;
	}
}
