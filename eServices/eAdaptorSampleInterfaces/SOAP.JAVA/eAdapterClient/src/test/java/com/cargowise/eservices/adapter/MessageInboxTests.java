package com.cargowise.eservices.adapter;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.nio.charset.StandardCharsets;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Matchers;
import org.mockito.Mockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.adapter.interfaces.IeHubFinalizable;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;
import com.cargowise.eservices.common.eHubGatewayMessage;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class MessageInboxTests extends TestCase {

	@Test
	public void testMarkAsRead_InboxEmpty() throws Exception {
		IeHubFinalizable adapterMock = Mockito.mock(IeHubFinalizable.class);

		MessageInbox inbox = new MessageInbox(adapterMock, "RecipintID");
		inbox.markAsRead();
		Mockito.verify(adapterMock, Mockito.never()).finalise(Matchers.<Guid>any());
	}

	@Test
	public void testMarkAsRead() throws Exception {
		IeHubFinalizable adapterMock = Mockito.mock(IeHubFinalizable.class);

		MessageInbox inbox = new MessageInbox(adapterMock, "RecipintID");
		inbox.setBatchID(Guid.newGuid());
		inbox.markAsRead();
		Mockito.verify(adapterMock, Mockito.atLeastOnce()).finalise(Matchers.<Guid>any());
		assertEquals(Guid.EMPTY, inbox.getBatchID());
	}

	@Test
	public void testReadMessageBatch() throws eHubAdapterException, IOException {
		IeHubFinalizable adapterMock = Mockito.mock(IeHubFinalizable.class);
		Guid batchID = Guid.newGuid();
		MessageInbox inboxMock = new MessageInbox(adapterMock, "RecipintID");

		ByteArrayOutputStream streamOutput = new ByteArrayOutputStream();
		OutputStreamWriter writer = new OutputStreamWriter(streamOutput, StandardCharsets.UTF_8);
		writer.write("aaaaaaaaaaaabbbbbbbbbbbbbbbcccccccccccccccccddddddddddddddddd");
		writer.flush();
		try (Stream stream = new Stream(new ByteArrayInputStream(streamOutput.toByteArray()))) {
			eHubGatewayMessage message = new eHubGatewayMessage();
			message.setApplicationCode("");
			message.setEmailSubject("subj");
			message.setFileName("blah.txt");
			message.setMessageTrackingID(batchID);
			message.setClientID("SenderID");
			message.setSchemaName("http://www.edi.com.au/EnterpriseService/#XmlInterchange");
			message.setSchemaType(MessageSchemaType.Xml);
			message.setMessageStream(stream.compressAndEncode());
			inboxMock.readMessageBatch(batchID, new eHubGatewayMessage[] { message });
			assertEquals(1, inboxMock.count());
			for (IeHubMessage inboxMessage : inboxMock) {
				assertEquals("", inboxMessage.getApplicationCode());
				assertEquals("subj", inboxMessage.getEmailSubject());
				assertEquals("blah.txt", inboxMessage.getFileName());
				assertEquals(batchID, inboxMessage.getTrackingID());
				assertEquals("http://www.edi.com.au/EnterpriseService/#XmlInterchange", inboxMessage.getSchemaName());
				assertEquals(MessageSchemaType.Xml, inboxMessage.getSchemaType());
				assertEquals("aaaaaaaaaaaabbbbbbbbbbbbbbbcccccccccccccccccddddddddddddddddd\r\n",
						inboxMessage.getMessageStream().readToEnd());
			}
		}
	}

	@Test
	public void testCanRetrieveMessages() {
		IeHubFinalizable adapterMock = Mockito.mock(IeHubFinalizable.class);

		MessageInbox inbox = new MessageInbox(adapterMock, "RecipintID");
		assertTrue(inbox.isRetrievableMessages());
		inbox.setBatchID(Guid.newGuid());
		assertFalse(inbox.isRetrievableMessages());
	}
}
