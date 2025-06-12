package com.cargowise.eservices.adapter;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.nio.charset.StandardCharsets;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;
import com.cargowise.eservices.common.eHubGatewayMessage;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;
import com.google.common.io.ByteStreams;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class EHubMessageTests extends TestCase {

	@Test
	public void testeHubMessage() throws IOException, eHubAdapterException {
		ByteArrayOutputStream streamOutput = new ByteArrayOutputStream();
		OutputStreamWriter writer = new OutputStreamWriter(streamOutput, StandardCharsets.UTF_8);
		writer.write("aaaaaaaaaaaabbbbbbbbbbbbbbbcccccccccccccccccddddddddddddddddd");
		writer.flush();
		Stream stream = new Stream(new ByteArrayInputStream(streamOutput.toByteArray()));
		ByteStreams.skipFully(stream, 5);
		Guid trackingID = Guid.newGuid();
		eHubMessage message = new eHubMessage(trackingID, "SenderID", "RecipientID", MessageSchemaType.Xml, "XMS",
				"BLAH", stream, "", "");
		assertEquals(trackingID, message.getTrackingID());
		assertEquals("SenderID", message.getSenderID());
		assertEquals("RecipientID", message.getRecipientID());
		assertEquals(MessageSchemaType.Xml, message.getSchemaType());
		assertEquals("XMS", message.getApplicationCode());
		assertEquals("BLAH", message.getSchemaName());
		assertEquals("", message.getEmailSubject());
		assertEquals("", message.getFileName());
		assertEquals(0, message.getMessageStream().getPosition());

		ByteStreams.skipFully(stream, 5);
		message = new eHubMessage(trackingID, "SenderID", "RecipientID", MessageSchemaType.Xml, "XMS", "BLAH", stream,
				"blah@blah.com", "xml.txt");
		assertEquals(trackingID, message.getTrackingID());
		assertEquals("SenderID", message.getSenderID());
		assertEquals("RecipientID", message.getRecipientID());
		assertEquals(MessageSchemaType.Xml, message.getSchemaType());
		assertEquals("XMS", message.getApplicationCode());
		assertEquals("BLAH", message.getSchemaName());
		assertEquals("blah@blah.com", message.getEmailSubject());
		assertEquals("xml.txt", message.getFileName());
		assertEquals(0, message.getMessageStream().getPosition());
	}

	@Test
	public void testToString() {
		eHubGatewayMessage message = new eHubGatewayMessage();
		message.setApplicationCode("XMS");
		message.setEmailSubject("email@email.com");
		message.setFileName("BLLIST.xml");
		message.setClientID("RECIPIENT1");
		message.setSchemaName("BLLIST#Schema");

		assertEquals("null|RECIPIENT1|BLLIST#Schema|null|XMS|email@email.com|BLLIST.xml", message.toString());

		message.setMessageTrackingID(new Guid("dcff3331-23bf-41df-ab99-3d1b427debde"));

		message.setSchemaType(MessageSchemaType.Xml);
		assertEquals("dcff3331-23bf-41df-ab99-3d1b427debde|RECIPIENT1|BLLIST#Schema|Xml|XMS|email@email.com|BLLIST.xml", message.toString());
	}
}
