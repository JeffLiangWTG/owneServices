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

import com.cargowise.eservices.adapter.interfaces.IMessageOutbox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.SendStreamRequest;
import com.cargowise.eservices.common.ServicePointManager;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;
import com.google.common.annotations.VisibleForTesting;
import com.cargowise.eservices.common.eAdapterStreamedService;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class EAdapterAdapterTests extends TestCase {
	/**
	 * Tests Send Message Functionality.
	 * @throws Exception
	 */
	@VisibleForTesting
	public void testSendMessages() throws Exception {
		final Guid trackingID = Guid.newGuid();
		ByteArrayOutputStream streamOutput = new ByteArrayOutputStream();
		OutputStreamWriter writer = new OutputStreamWriter(streamOutput, StandardCharsets.UTF_8);
		writer.write("aaaaaaaaaavvvvvvbbbbbbbbbbbbb");
		writer.flush();
		Stream stream = new Stream(new ByteArrayInputStream(streamOutput.toByteArray()));

		IeHubMessage message = new eHubMessage(trackingID, "SenderID", "RecipientID", MessageSchemaType.Xml, "XMS",
				"SchemaName", stream, "blah@blah.com", "blah.xml");

		eAdapterStreamedService serviceMock = Mockito.mock(eAdapterStreamedService.class);

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

		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		eAdapterAdapter adapterMock = Mockito.spy(new eAdapterAdapter());
		Mockito.stub(adapterMock.createOutbox()).toReturn(outoboxMock);
		Mockito.stub(adapterMock.createService(Matchers.anyString(), Matchers.anyString(), Matchers.anyString()))
				.toReturn(serviceMock);
		adapterMock.setup("https://test.svc", "Sender ID", "password");

		adapterMock.sendMessages();
		Mockito.verify(serviceMock, Mockito.atLeastOnce()).sendStream(Matchers.<SendStreamRequest>any());
	}

	@Test
	public void testSendMessages_EmptyOutbox() throws Exception {
		eAdapterStreamedService serviceMock = Mockito.mock(eAdapterStreamedService.class);
		Mockito.doAnswer(invocation -> {
			SendStreamRequest request = invocation.getArgumentAt(0, SendStreamRequest.class);
			assertEquals(0, request.getMessages().length);
			return null;
		}).when(serviceMock).sendStream(Matchers.<SendStreamRequest>any());

		IMessageOutbox outoboxMock = new MessageOutbox();
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		eAdapterAdapter adapterMock = Mockito.spy(new eAdapterAdapter());
		Mockito.stub(adapterMock.createOutbox()).toReturn(outoboxMock);
		Mockito.stub(adapterMock.createService(Matchers.anyString(), Matchers.anyString(), Matchers.anyString()))
				.toReturn(serviceMock);
		adapterMock.setup("https://test.svc", "Sender ID", "password");

		adapterMock.sendMessages();
		Mockito.verify(serviceMock, Mockito.atLeastOnce()).sendStream(Matchers.<SendStreamRequest>any());
	}
}
