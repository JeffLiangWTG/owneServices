package com.cargowise.eservices.client.sample;

import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.eHubAdapter;
import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.client.content.DataStore;
import com.cargowise.eservices.client.content.MessageInboxMock;
import com.cargowise.eservices.client.content.ReceiveMessage;
import com.cargowise.eservices.client.content.TestCaseBase;
import com.cargowise.eservices.client.content.eHubMessageUltility;
import com.cargowise.eservices.common.Action;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.ServicePointManager;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ Guid.class, eHubAdapterRecipientSample.class, Thread.class })
public class eHubAdapterRecipientSampleTests extends TestCaseBase {
	static final String WEB_SERVICE_ADDRESS = "https://localhost/eHubGateway/eHubStreamedService.svc";
	static final String SENDER = "SenderID";
	static final String RECIPIENT = "RecipientID";
	static final String PASSWORD = "password";
	Guid guid1 = new Guid("340364b0-0e0f-4506-88d9-fa0a5c789d96");

	@Test
	public void testPingFail_DoNothing() throws Exception {
		eHubAdapter mockAdapter = Mockito.mock(eHubAdapter.class);
		Mockito.stub(mockAdapter.ping()).toReturn(false);
		PowerMockito.when(eHubAdapterRecipientSample.getNewEHubAdapter()).thenReturn(mockAdapter);

		eHubAdapterRecipientSample.main(new String[] {});

		assertEquals(String.format("Ping %s: false\r\n", eHubAdapterRecipientSample.WEB_SERVICE_ADDRESS), output.toString());
	}

	@Test
	public void testSuccessReceiveMessage() throws Exception {
		setupEnviromentMock();
		PowerMockito.when(eHubAdapterRecipientSample.getTaskTimeUnit()).thenReturn(TimeUnit.MILLISECONDS);

		DataStore dataStore = new DataStore();
		PowerMockito.when(eHubAdapterRecipientSample.getNewDataStore()).thenReturn(dataStore);
		ReceiveMessage receiveMessageTask = Mockito.spy(new ReceiveMessage(WEB_SERVICE_ADDRESS, RECIPIENT, PASSWORD, dataStore, 1));
		PowerMockito.when(eHubAdapterRecipientSample.getNewReceiveMessageTask(dataStore)).thenReturn(receiveMessageTask);

		final eHubAdapter mockAdapterReceiveMessage = Mockito.mock(eHubAdapter.class);
		MessageInboxMock inbox = new MessageInboxMock();
		Mockito.stub(mockAdapterReceiveMessage.getInbox()).toReturn(inbox);
		PowerMockito.when(receiveMessageTask, PowerMockito.method(ReceiveMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterReceiveMessage);

		Mockito.doAnswer(new Answer<Void>() {
			public Void answer(final InvocationOnMock invocation) throws eHubAdapterException {
				((MessageInboxMock) mockAdapterReceiveMessage.getInbox()).addMessage(eHubMessageUltility.generateMessage(guid1, SENDER, RECIPIENT));
				return null;
			}
		}).doNothing().when(mockAdapterReceiveMessage).retrieveMessages();

		eHubAdapterRecipientSample.main(new String[] {});

		StringBuilder expect = new StringBuilder();
		expect.append("Ping " + eHubAdapterRecipientSample.WEB_SERVICE_ADDRESS + ": true\r\n");
		expect.append("[RecipientID]Success - Received 1 message(s)\r\n");
		expect.append("=================================Message=============================================\r\n");
		expect.append(String.format("Message: %1$s|%2$s|%3$s|http://www.edi.com.au/EnterpriseService/#XmlInterchange|Xml|UDM|Email Subject Test|File Name Test\r\n", guid1, SENDER, RECIPIENT));
		expect.append("Content: <?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
		expect.append("<XmlInterchange xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Version=\"1\" xmlns=\"http://www.edi.com.au/EnterpriseService/\">\r\n");
		expect.append("  <InterchangeInfo>\r\n");
		expect.append("    <Date>2010-11-17T07:53:29.2230000+11:00</Date>\r\n");
		expect.append("    <XmlType>Verbose</XmlType>\r\n");
		expect.append("  </InterchangeInfo>\r\n");
		expect.append("</XmlInterchange>\r\n");
		expect.append("=====================================================================================\r\n");
		expect.append("There are 1 message(s) in DataStore.\r\n");
		expect.append("============================Status Incoming Messages=================================\r\n");
		expect.append(String.format("%1$s - Received\r\n", guid1));
		expect.append("=====================================================================================\r\n");

		assertEquals(expect.toString(), output.toString());
	}

	@Test
	public void testFailReceiveMessage() throws Exception {
		setupEnviromentMock();
		PowerMockito.when(eHubAdapterRecipientSample.getTaskTimeUnit()).thenReturn(TimeUnit.MILLISECONDS);

		DataStore dataStore = new DataStore();
		PowerMockito.when(eHubAdapterRecipientSample.getNewDataStore()).thenReturn(dataStore);
		ReceiveMessage receiveMessageTask = Mockito.spy(new ReceiveMessage(WEB_SERVICE_ADDRESS, RECIPIENT, PASSWORD, dataStore, 1));
		PowerMockito.when(eHubAdapterRecipientSample.getNewReceiveMessageTask(dataStore)).thenReturn(receiveMessageTask);

		final eHubAdapter mockAdapterReceiveMessage = Mockito.mock(eHubAdapter.class);
		MessageInboxMock inbox = new MessageInboxMock();
		Mockito.stub(mockAdapterReceiveMessage.getInbox()).toReturn(inbox);
		PowerMockito.when(receiveMessageTask, PowerMockito.method(ReceiveMessage.class, "getNewEHubAdapter")).withNoArguments().thenReturn(mockAdapterReceiveMessage);

		eHubAdapterRecipientSample.main(new String[] {});

		StringBuilder expect = new StringBuilder();
		expect.append("Ping " + eHubAdapterRecipientSample.WEB_SERVICE_ADDRESS + ": true\r\n");
		expect.append("[Warning]ReceiveMessage Task could not receive a message as expected after 1 runs.\r\n");
		expect.append("There are 0 message(s) in DataStore.\r\n");
		expect.append("============================Status Incoming Messages=================================\r\n");
		expect.append("=====================================================================================\r\n");

		assertEquals(expect.toString(), output.toString());
	}

	@Test
	public void testSubThreadThrowException() throws Exception {
		setupEnviromentMock();
		PowerMockito.when(eHubAdapterRecipientSample.getTaskTimeUnit()).thenReturn(TimeUnit.MILLISECONDS);

		DataStore dataStore = new DataStore();
		PowerMockito.when(eHubAdapterRecipientSample.getNewDataStore()).thenReturn(dataStore);
		ReceiveMessage receiveMessageTask = Mockito.spy(new ReceiveMessage(WEB_SERVICE_ADDRESS, RECIPIENT, PASSWORD, dataStore, 1));
		PowerMockito.when(eHubAdapterRecipientSample.getNewReceiveMessageTask(dataStore)).thenReturn(receiveMessageTask);

		PowerMockito.when(receiveMessageTask, PowerMockito.method(ReceiveMessage.class, "getNewEHubAdapter")).withNoArguments().thenThrow(new Exception("test exception"));

		assertException(new Action() {
			public void run() throws Exception {
				eHubAdapterRecipientSample.main(new String[] {});
			}
		}, ExecutionException.class, "java.lang.RuntimeException: java.lang.Exception: test exception");
	}

	@Test
	void setupEnviromentMock() throws Exception {
		eHubAdapter mockAdapter = Mockito.mock(eHubAdapter.class);
		Mockito.stub(mockAdapter.ping()).toReturn(true);
		PowerMockito.when(eHubAdapterRecipientSample.getNewEHubAdapter()).thenReturn(mockAdapter);
	}

	@Override
	@Test
	protected void setUp() throws Exception {
		super.setUp();
		PowerMockito.spy(eHubAdapterRecipientSample.class);
		ServicePointManager.setServerCertificateValidationCallback(new Action() {
			public void run() throws Exception {
			}
		});
	}
}
