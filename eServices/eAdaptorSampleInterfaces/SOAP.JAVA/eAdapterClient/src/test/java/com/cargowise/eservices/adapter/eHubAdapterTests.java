package com.cargowise.eservices.adapter;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.OutputStreamWriter;
import java.nio.charset.StandardCharsets;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Matchers;
import org.mockito.Mockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.adapter.interfaces.IMessageInbox;
import com.cargowise.eservices.adapter.interfaces.IMessageOutbox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.RetrieveStreamResponse;
import com.cargowise.eservices.common.SendStreamRequest;
import com.cargowise.eservices.common.ServicePointManager;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;
import com.cargowise.eservices.common.eHubGatewayMessage;
import com.cargowise.eservices.common.eHubStreamedService;
import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})
public class EHubAdapterTests extends TestCase {

	@Test
	public void testSendMessages() throws Exception {
		final Guid trackingID = Guid.newGuid();
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		ByteArrayOutputStream streamOutput = new ByteArrayOutputStream();
		OutputStreamWriter writer = new OutputStreamWriter(streamOutput, StandardCharsets.UTF_8);
		writer.write("aaaaaaaaaavvvvvvbbbbbbbbbbbbb");
		writer.flush();
		Stream stream = new Stream(new ByteArrayInputStream(streamOutput.toByteArray()));

		IeHubMessage message = new eHubMessage(trackingID, "SenderID", "RecipientID", MessageSchemaType.Xml, "XMS",
				"SchemaName", stream, "blah@blah.com", "blah.xml");

		eHubStreamedService serviceMock = Mockito.mock(eHubStreamedService.class);

		Mockito.doAnswer(invocation -> {
				SendStreamRequest request = invocation.getArgumentAt(0, SendStreamRequest.class);
				assertEquals(1, request.getMessages().length);
				assertEquals(trackingID, request.getMessages()[0].getMessageTrackingID());
				assertEquals("RecipientID", request.getMessages()[0].getClientID());
				assertEquals(MessageSchemaType.Xml, request.getMessages()[0].getSchemaType());
				assertEquals("XMS", request.getMessages()[0].getApplicationCode());
				assertEquals("SchemaName", request.getMessages()[0].getSchemaName());
				assertEquals("blah@blah.com", request.getMessages()[0].getEmailSubject());
				assertEquals("blah.xml", request.getMessages()[0].getFileName());
				assertEquals("aaaaaaaaaavvvvvvbbbbbbbbbbbbb",
						request.getMessages()[0].getMessageStream().decodeAndDecompress().toString());
				return null;

		}).when(serviceMock).sendStream(Matchers.<SendStreamRequest>any());

		IMessageOutbox outoboxMock = new MessageOutbox();
		outoboxMock.addMessage(message);

		eHubAdapter adapterMock = Mockito.spy(new eHubAdapter());
		Mockito.stub(adapterMock.createOutbox()).toReturn(outoboxMock);
		Mockito.stub(adapterMock.createService(Matchers.anyString(), Matchers.anyString(), Matchers.anyString()))
				.toReturn(serviceMock);
		adapterMock.setup("https://test.svc", "Sender ID", "password");

		adapterMock.sendMessages();
		Mockito.verify(serviceMock, Mockito.atLeastOnce()).sendStream(Matchers.<SendStreamRequest>any());
	}

	@Test
	public void testSendMessages_EmptyOutbox() throws Exception {
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		eHubStreamedService serviceMock = Mockito.mock(eHubStreamedService.class);
		Mockito.doAnswer(invocation -> {
				SendStreamRequest request = invocation.getArgumentAt(0, SendStreamRequest.class);
				assertEquals(0, request.getMessages().length);
				return null;
		}).when(serviceMock).sendStream(Matchers.<SendStreamRequest>any());

		IMessageOutbox outoboxMock = new MessageOutbox();
		eHubAdapter adapterMock = Mockito.spy(new eHubAdapter());
		Mockito.stub(adapterMock.createOutbox()).toReturn(outoboxMock);
		Mockito.stub(adapterMock.createService(Matchers.anyString(), Matchers.anyString(), Matchers.anyString()))
				.toReturn(serviceMock);
		adapterMock.setup("https://test.svc", "Sender ID", "password");

		adapterMock.sendMessages();
		Mockito.verify(serviceMock, Mockito.atLeastOnce()).sendStream(Matchers.<SendStreamRequest>any());
	}

	@Test
	public void testRetrieveMessages_CannotRetrieveMessages() throws Exception {
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		eHubStreamedService serviceMock = Mockito.mock(eHubStreamedService.class);

		IMessageInbox inboxMock = Mockito.mock(IMessageInbox.class);
		Mockito.stub(inboxMock.isRetrievableMessages()).toReturn(false);

		final eHubAdapter adapterMock = Mockito.spy(new eHubAdapter());
		Mockito.stub(adapterMock.createInbox()).toReturn(inboxMock);
		Mockito.stub(adapterMock.createService(Matchers.anyString(), Matchers.anyString(), Matchers.anyString()))
				.toReturn(serviceMock);
		adapterMock.setup("https://test.svc", "Sender ID", "password");

		assertException(new com.cargowise.eservices.common.Action() {
			public void run() throws Exception {
				adapterMock.retrieveMessages();
			}
		}, eHubAdapterException.class, "Execute getInbox().markAsRead() method before retrieve new Messages");
	}

	@Test
	public void testRetrieveMessages_EmptyResponse() throws Exception {
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		eHubStreamedService serviceMock = Mockito.mock(eHubStreamedService.class);
		Mockito.stub(serviceMock.retrieveStream())
				.toReturn(new RetrieveStreamResponse(Guid.newGuid(), new eHubGatewayMessage[] {}));

		IMessageInbox inboxMock = Mockito.mock(IMessageInbox.class);
		Mockito.stub(inboxMock.isRetrievableMessages()).toReturn(true);

		eHubAdapter adapterMock = Mockito.spy(new eHubAdapter());
		Mockito.stub(adapterMock.createInbox()).toReturn(inboxMock);
		Mockito.stub(adapterMock.createService(Matchers.anyString(), Matchers.anyString(), Matchers.anyString()))
				.toReturn(serviceMock);
		adapterMock.setup("https://test.svc", "Sender ID", "password");

		adapterMock.retrieveMessages();
		Mockito.verify(inboxMock, Mockito.never()).readMessageBatch(Matchers.<Guid>any(),
				Matchers.<eHubGatewayMessage[]>any());
	}

	@Test
	public void testRetrieveMessages() throws Exception {
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		final eHubGatewayMessage[] messages = new eHubGatewayMessage[] {};
		final Guid trackingID = Guid.newGuid();

		eHubStreamedService serviceMock = Mockito.mock(eHubStreamedService.class);
		Mockito.stub(serviceMock.retrieveStream()).toReturn(new RetrieveStreamResponse(Guid.newGuid(), messages));

		IMessageInbox inboxMock = Mockito.mock(IMessageInbox.class);
		Mockito.stub(inboxMock.isRetrievableMessages()).toReturn(true);
		Mockito.doAnswer(invocation -> {
				assertEquals(trackingID, invocation.getArgumentAt(0, Guid.class));
				assertEquals(messages, invocation.getArgumentAt(1, eHubGatewayMessage[].class));
				return null;
		}).when(inboxMock).readMessageBatch(trackingID, messages);

		eHubAdapter adapterMock = Mockito.spy(new eHubAdapter());
		Mockito.stub(adapterMock.createInbox()).toReturn(inboxMock);
		Mockito.stub(adapterMock.createService(Matchers.anyString(), Matchers.anyString(), Matchers.anyString()))
				.toReturn(serviceMock);
		adapterMock.setup("https://test.svc", "Sender ID", "password");

		adapterMock.retrieveMessages();
		Mockito.verify(inboxMock, Mockito.atLeastOnce()).isRetrievableMessages();
	}
	protected final void assertException(final com.cargowise.eservices.common.Action action, final Class<?> exceptionType, final String exceptionMessage) {
		try {
			action.run();
		} catch (Exception ex) {
			assertEquals(exceptionType, ex.getClass());
			assertEquals(exceptionMessage, ex.getMessage());
			return;
		}

		fail(String.format("Expected exception with type %1$s, but no exception occured", exceptionType.toString()));
	}
}
