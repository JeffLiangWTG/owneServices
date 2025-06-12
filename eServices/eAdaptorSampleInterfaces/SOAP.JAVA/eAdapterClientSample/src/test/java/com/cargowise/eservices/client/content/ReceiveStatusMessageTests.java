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
import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.common.Action;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.Stream;

//@RunWith(PowerMockRunner.class)
public class ReceiveStatusMessageTests extends TestCaseBase {
	static final String USERNAME = "SenderID";
	static final String PASSWORD = "testpassword";
	static final String WEB_SERVICE_ADDRESS = "testWebServiceAddress.com/service";
	static final String RECIPIENT = "RecipientID";

	@Test
	public void testReceiveSuccessStatusMessage_Success() throws Exception {
		DataStore dataStore = new DataStore();
		final ReceiveStatusMessage receiveStatusMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(receiveStatusMessageTask.getNewEHubAdapter()).toReturn(adapter);
		eHubMessage message = eHubMessageUltility.generateMessage(USERNAME, PASSWORD);
		final eHubMessage statusMessage = new eHubMessage(message.getTrackingID(), USERNAME, RECIPIENT, MessageSchemaType.Xml,
				"",  MessageStatus.MSS, new Stream("test".getBytes()), "", "");

		dataStore.add(message, MessageStatus.Pending);

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) throws eHubAdapterException {
				((MessageInboxMock) adapter.getInbox()).addMessage(statusMessage);
				return null;
			}
		}).when(adapter).retrieveMessages();

		assertException(new Action() {
			public void run() throws Exception {
				receiveStatusMessageTask.run();
			}
		}, CancellationException.class, null);

		assertEquals(1, dataStore.size());
		Entry<eHubMessage, MessageStatus> receivedMessage = dataStore.entrySet().iterator().next();
		assertEquals(message.getTrackingID(), receivedMessage.getKey().getTrackingID());
		assertEquals(MessageStatus.Sent, receivedMessage.getValue());
	}

	@Test
	public void testReceiveFailStatusMessage_Success() throws Exception {
		DataStore dataStore = new DataStore();
		final ReceiveStatusMessage receiveStatusMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(receiveStatusMessageTask.getNewEHubAdapter()).toReturn(adapter);
		eHubMessage message = eHubMessageUltility.generateMessage(USERNAME, PASSWORD);
		final eHubMessage statusMessage = new eHubMessage(message.getTrackingID(), USERNAME, RECIPIENT, MessageSchemaType.Xml,
				"",  MessageStatus.MSF, new Stream("test".getBytes()), "", "");

		dataStore.add(message, MessageStatus.Pending);

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) throws eHubAdapterException {
				((MessageInboxMock) adapter.getInbox()).addMessage(statusMessage);
				return null;
			}
		}).when(adapter).retrieveMessages();

		assertException(new Action() {
			public void run() throws Exception {
				receiveStatusMessageTask.run();
			}
		}, CancellationException.class, null);

		assertEquals(1, dataStore.size());
		Entry<eHubMessage, MessageStatus> receivedMessage = dataStore.entrySet().iterator().next();
		assertEquals(message.getTrackingID(), receivedMessage.getKey().getTrackingID());
		assertEquals(MessageStatus.Failed, receivedMessage.getValue());
	}

	@Test
	public void testReceiveAcknowledgementStatusMessage_Success() throws Exception {
		DataStore dataStore = new DataStore();
		final ReceiveStatusMessage receiveStatusMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(receiveStatusMessageTask.getNewEHubAdapter()).toReturn(adapter);
		eHubMessage message = eHubMessageUltility.generateMessage(USERNAME, RECIPIENT);
		final eHubMessage statusMessage = new eHubMessage(message.getTrackingID(), USERNAME, RECIPIENT, MessageSchemaType.Xml,
				"",  MessageStatus.MSA, new Stream("test".getBytes()), "", "");

		dataStore.add(message, MessageStatus.Sent);

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) throws eHubAdapterException {
				((MessageInboxMock) adapter.getInbox()).addMessage(statusMessage);
				return null;
			}
		}).when(adapter).retrieveMessages();

		assertException(new Action() {
			public void run() throws Exception {
				receiveStatusMessageTask.run();
			}
		}, CancellationException.class, null);

		assertEquals(1, dataStore.size());
		Entry<eHubMessage, MessageStatus> receivedMessage = dataStore.entrySet().iterator().next();
		assertEquals(message.getTrackingID(), receivedMessage.getKey().getTrackingID());
		assertEquals(MessageStatus.Acknowledged, receivedMessage.getValue());
	}

	@Test
	public void testReceiveStatusMessage_NotMatchingAnyExistingOutgoingMessage_Fail() throws Exception {
		DataStore dataStore = new DataStore();
		final ReceiveStatusMessage receiveMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(receiveMessageTask.getNewEHubAdapter()).toReturn(adapter);
		final eHubMessage message = eHubMessageUltility.generateMessage(USERNAME, RECIPIENT);

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) throws eHubAdapterException {
				((MessageInboxMock) adapter.getInbox()).addMessage(message);
				return null;
			}
		}).when(adapter).retrieveMessages();

		assertException(new Action() {
			public void run() throws Exception {
				receiveMessageTask.run();
			}
		}, RuntimeException.class, "java.security.InvalidParameterException: MessageStatus.Received does not have any matching outgoing interchange status.");
	}

	@Test
	public void testReceiveStatusMessage_ExceedMaxRunningCount() throws Exception {
		DataStore dataStore = new DataStore();
		final ReceiveStatusMessage receiveMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, dataStore, 1));
		final eHubAdapter adapter = createMockEHubAdapter();
		Mockito.stub(receiveMessageTask.getNewEHubAdapter()).toReturn(adapter);

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) throws eHubAdapterException {
				// do nothing
				return null;
			}
		}).when(adapter).retrieveMessages();

		assertException(new Action() {
			public void run() throws Exception {
				receiveMessageTask.run();
			}
		}, CancellationException.class, "[Warning]ReceiveStatusMessage Task could not receive a status message as expected after 1 runs.");

		assertEquals(0, dataStore.size());
	}

	@Test
	public void testThrowExceptionInSubThread() throws Exception {
		final ReceiveStatusMessage receiveMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD, null, 1));
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
