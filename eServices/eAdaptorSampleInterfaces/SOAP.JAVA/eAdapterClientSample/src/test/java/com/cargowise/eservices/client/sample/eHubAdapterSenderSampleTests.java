package com.cargowise.eservices.client.sample;

import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.client.content.DataStore;
import com.cargowise.eservices.client.content.MessageInboxMock;
import com.cargowise.eservices.client.content.MessageOutboxMock;
import com.cargowise.eservices.client.content.MessageStatus;
import com.cargowise.eservices.client.content.ReceiveStatusMessage;
import com.cargowise.eservices.client.content.SendMessage;
import com.cargowise.eservices.client.content.TestCaseBase;
import com.cargowise.eservices.client.content.eHubMessageUltility;
import com.cargowise.eservices.common.Action;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.ServicePointManager;
import com.cargowise.eservices.common.Stream;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eHubAdapterSenderSample.class, Thread.class })
public class eHubAdapterSenderSampleTests extends TestCaseBase {
	static final String WEB_SERVICE_ADDRESS = "https://localhost/eHubGateway/eHubStreamedService.svc";
	static final String SENDER = "SenderID";
	static final String RECIPIENT = "RecipientID";
	static final String PASSWORD = "password";
	boolean messageIsReadyToBePickup = false;
	Guid guid1 = new Guid("340364b0-0e0f-4506-88d9-fa0a5c789d96");

	@Test
	public void testPingFail_DoNothing() throws Exception {
		eHubAdapter mockAdapter = Mockito.mock(eHubAdapter.class);
		Mockito.when(mockAdapter.ping()).thenReturn(false);
		Mockito.when(eHubAdapterSenderSample.getNewEHubAdapter()).thenReturn(mockAdapter);

		eHubAdapterSenderSample.main(new String[] {});

		assertEquals(String.format("Ping %s: false\r\n", eHubAdapterSenderSample.WEB_SERVICE_ADDRESS), output.toString());
	}

	@Test
	public void testSuccessSendAMessageAndReceiveMatchingStatusMessage() throws Exception {
		setupEnviromentMock();
		PowerMockito.when(eHubAdapterSenderSample.getTaskTimeUnit()).thenReturn(TimeUnit.MILLISECONDS);
		PowerMockito.when(eHubAdapterSenderSample.generateMessage()).thenReturn(eHubMessageUltility.generateMessage(guid1, SENDER, RECIPIENT));

		DataStore dataStore = new DataStore();
		PowerMockito.when(eHubAdapterSenderSample.getNewDataStore()).thenReturn(dataStore);
		SendMessage sendMessageTask = Mockito.spy(new SendMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 10));
		PowerMockito.when(eHubAdapterSenderSample.getNewSendMessageTask(dataStore)).thenReturn(sendMessageTask);
		ReceiveStatusMessage receiveStatusMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 10));
		PowerMockito.when(eHubAdapterSenderSample.getNewReceiveStatusMessageTask(dataStore)).thenReturn(receiveStatusMessageTask);

		eHubAdapter mockAdapterSendMessage = Mockito.mock(eHubAdapter.class);
		final MessageOutboxMock outbox = new MessageOutboxMock();
		Mockito.stub(mockAdapterSendMessage.getOutbox()).toReturn(outbox);
		PowerMockito.when(sendMessageTask, PowerMockito.method(SendMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterSendMessage);

		final eHubAdapter mockAdapterReceiveStatusMessage = Mockito.mock(eHubAdapter.class);
		MessageInboxMock inbox = new MessageInboxMock();
		Mockito.stub(mockAdapterReceiveStatusMessage.getInbox()).toReturn(inbox);
		PowerMockito.when(receiveStatusMessageTask, PowerMockito.method(ReceiveStatusMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterReceiveStatusMessage);

		final eHubMessage statusMessage = new eHubMessage(guid1, "eHub", SENDER, MessageSchemaType.Xml,
				"",  MessageStatus.MSS, new Stream("test".getBytes()), "", "");

		Mockito.doAnswer(invocation -> {
			outbox.clear();
			return null;
		}).when(mockAdapterSendMessage).sendMessages();
		Mockito.doAnswer(invocation -> {
			messageIsReadyToBePickup = true;
			return null;
		}).when(mockAdapterSendMessage).close();

		messageIsReadyToBePickup = false;
		Mockito.doAnswer(invocation -> {
			if (messageIsReadyToBePickup) {
				((MessageInboxMock) mockAdapterReceiveStatusMessage.getInbox()).addMessage(statusMessage);
			}
			messageIsReadyToBePickup = false;
			return null;
		}).when(mockAdapterReceiveStatusMessage).retrieveMessages();

		eHubAdapterSenderSample.main(new String[] {});

		StringBuilder expect = new StringBuilder();
		expect.append("Ping " + eHubAdapterSenderSample.WEB_SERVICE_ADDRESS + ": true\r\n");
		expect.append("[SenderID]Success - Sent 1 message(s)\r\n");
		expect.append("[SenderID]Success - Received 1 message(s)\r\n");
		expect.append("[SenderID]Matching outgoing Interchange with tracking id 340364b0-0e0f-4506-88d9-fa0a5c789d96 is updated with Sent status.\r\n");
		expect.append("=================================Message=============================================\r\n");
		expect.append(String.format("Message: %1$s|eHub|%2$s|MessageStatusSuccess|Xml|||\r\n", guid1, SENDER));
		expect.append("Content: test\r\n");
		expect.append("=====================================================================================\r\n");
		expect.append("There are 1 message(s) in DataStore.\r\n");
		expect.append("============================Status Outgoing Messages=================================\r\n");
		expect.append(String.format("%1$s - Sent\r\n", guid1));
		expect.append("=====================================================================================\r\n");

		assertEquals(expect.toString(), output.toString());
	}

	@Test
	public void testFailSendMessage_FailReceiveMatchingStatusMessage() throws Exception {
		setupEnviromentMock();
		PowerMockito.when(eHubAdapterSenderSample.getTaskTimeUnit()).thenReturn(TimeUnit.MILLISECONDS);
		PowerMockito.when(eHubAdapterSenderSample.generateMessage()).thenReturn(eHubMessageUltility.generateMessage(guid1, SENDER, RECIPIENT));

		DataStore dataStore = new DataStore();
		PowerMockito.when(eHubAdapterSenderSample.getNewDataStore()).thenReturn(dataStore);
		SendMessage sendMessageTask = Mockito.spy(new SendMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 2));
		PowerMockito.when(eHubAdapterSenderSample.getNewSendMessageTask(dataStore)).thenReturn(sendMessageTask);
		ReceiveStatusMessage receiveStatusMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 2));
		PowerMockito.when(eHubAdapterSenderSample.getNewReceiveStatusMessageTask(dataStore)).thenReturn(receiveStatusMessageTask);

		eHubAdapter mockAdapterSendMessage = Mockito.mock(eHubAdapter.class);
		final MessageOutboxMock outbox = new MessageOutboxMock();
		Mockito.stub(mockAdapterSendMessage.getOutbox()).toReturn(outbox);
		Mockito.doNothing().when(mockAdapterSendMessage).sendMessages();
		PowerMockito.when(sendMessageTask, PowerMockito.method(SendMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterSendMessage);

		final eHubAdapter mockAdapterReceiveStatusMessage = Mockito.mock(eHubAdapter.class);
		MessageInboxMock inbox = new MessageInboxMock();
		Mockito.stub(mockAdapterReceiveStatusMessage.getInbox()).toReturn(inbox);
		PowerMockito.when(receiveStatusMessageTask, PowerMockito.method(ReceiveStatusMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterReceiveStatusMessage);

		eHubAdapterSenderSample.main(new String[] {});

		StringBuilder expect = new StringBuilder();
		expect.append("Ping " + eHubAdapterSenderSample.WEB_SERVICE_ADDRESS + ": true\r\n");
		expect.append("[Warning]SendMessage Task could not send a message as expected after 2 runs.\r\n");
		expect.append("[Warning]ReceiveStatusMessage Task could not receive a status message as expected after 2 runs.\r\n");
		expect.append("There are 1 message(s) in DataStore.\r\n");
		expect.append("============================Status Outgoing Messages=================================\r\n");
		expect.append(String.format("%1$s - Queued\r\n", guid1));
		expect.append("=====================================================================================\r\n");

		assertEquals(expect.toString(), output.toString());
	}

	@Test
	public void testSuccessSendAMessage_FailReceiveMatchingStatusMessage() throws Exception {
		setupEnviromentMock();
		PowerMockito.when(eHubAdapterSenderSample.getTaskTimeUnit()).thenReturn(TimeUnit.MILLISECONDS);
		PowerMockito.when(eHubAdapterSenderSample.generateMessage()).thenReturn(eHubMessageUltility.generateMessage(guid1, SENDER, RECIPIENT));

		DataStore dataStore = new DataStore();
		PowerMockito.when(eHubAdapterSenderSample.getNewDataStore()).thenReturn(dataStore);
		SendMessage sendMessageTask = Mockito.spy(new SendMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 10));
		PowerMockito.when(eHubAdapterSenderSample.getNewSendMessageTask(dataStore)).thenReturn(sendMessageTask);
		ReceiveStatusMessage receiveStatusMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 10));
		PowerMockito.when(eHubAdapterSenderSample.getNewReceiveStatusMessageTask(dataStore)).thenReturn(receiveStatusMessageTask);

		eHubAdapter mockAdapterSendMessage = Mockito.mock(eHubAdapter.class);
		final MessageOutboxMock outbox = new MessageOutboxMock();
		Mockito.stub(mockAdapterSendMessage.getOutbox()).toReturn(outbox);
		Mockito.doAnswer(invocation -> {
			outbox.clear();
			return null;
		}).when(mockAdapterSendMessage).sendMessages();
		PowerMockito.when(sendMessageTask, PowerMockito.method(SendMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterSendMessage);

		final eHubAdapter mockAdapterReceiveStatusMessage = Mockito.mock(eHubAdapter.class);
		MessageInboxMock inbox = new MessageInboxMock();
		Mockito.stub(mockAdapterReceiveStatusMessage.getInbox()).toReturn(inbox);
		PowerMockito.when(receiveStatusMessageTask, PowerMockito.method(ReceiveStatusMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterReceiveStatusMessage);

		eHubAdapterSenderSample.main(new String[] {});

		StringBuilder expect = new StringBuilder();
		expect.append("Ping " + eHubAdapterSenderSample.WEB_SERVICE_ADDRESS + ": true\r\n");
		expect.append("[SenderID]Success - Sent 1 message(s)\r\n");
		expect.append("[Warning]ReceiveStatusMessage Task could not receive a status message as expected after 10 runs.\r\n");
		expect.append("There are 1 message(s) in DataStore.\r\n");
		expect.append("============================Status Outgoing Messages=================================\r\n");
		expect.append(String.format("%1$s - Pending\r\n", guid1));
		expect.append("=====================================================================================\r\n");

		assertEquals(expect.toString(), output.toString());
	}

	@Test
	public void testSubThreadThrowException() throws Exception {
		setupEnviromentMock();
		PowerMockito.when(eHubAdapterSenderSample.getTaskTimeUnit()).thenReturn(TimeUnit.MILLISECONDS);

		DataStore dataStore = new DataStore();
		PowerMockito.when(eHubAdapterSenderSample.getNewDataStore()).thenReturn(dataStore);
		SendMessage sendMessageTask = Mockito.spy(new SendMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 2));
		PowerMockito.when(eHubAdapterSenderSample.getNewSendMessageTask(dataStore)).thenReturn(sendMessageTask);
		ReceiveStatusMessage receiveStatusMessageTask = Mockito.spy(new ReceiveStatusMessage(WEB_SERVICE_ADDRESS, SENDER, PASSWORD, dataStore, 2));
		PowerMockito.when(eHubAdapterSenderSample.getNewReceiveStatusMessageTask(dataStore)).thenReturn(receiveStatusMessageTask);

		eHubAdapter mockAdapter = Mockito.mock(eHubAdapter.class);
		final MessageOutboxMock outbox = new MessageOutboxMock();
		Mockito.stub(mockAdapter.getOutbox()).toReturn(outbox);
		MessageInboxMock inbox = new MessageInboxMock();
		Mockito.stub(mockAdapter.getInbox()).toReturn(inbox);

		PowerMockito.when(sendMessageTask, PowerMockito.method(SendMessage.class, "getNewEHubAdapter")).withNoArguments().thenThrow(new Exception("test exception")).thenReturn(mockAdapter);
		PowerMockito.when(receiveStatusMessageTask, PowerMockito.method(ReceiveStatusMessage.class, "getNewEHubAdapter")).withNoArguments().thenThrow(new Exception("test exception 2"));

		assertException(new Action() {
			public void run() throws Exception {
				eHubAdapterSenderSample.main(new String[] {});
			}
		}, ExecutionException.class, "java.lang.RuntimeException: java.lang.Exception: test exception");

		assertException(new Action() {
			public void run() throws Exception {
				eHubAdapterSenderSample.main(new String[] {});
			}
		}, ExecutionException.class, "java.lang.RuntimeException: java.lang.Exception: test exception 2");
	}

	private void setupEnviromentMock() throws Exception {
		eHubAdapter mockAdapter = Mockito.mock(eHubAdapter.class);
		Mockito.stub(mockAdapter.ping()).toReturn(true);
		PowerMockito.when(eHubAdapterSenderSample.getNewEHubAdapter()).thenReturn(mockAdapter);
	}

	@Override
	@Test
	protected void setUp() throws Exception {
		super.setUp();
		PowerMockito.spy(eHubAdapterSenderSample.class);

		ServicePointManager.setServerCertificateValidationCallback(new Action() {
			public void run() throws Exception {
			}
		});
	}
}
