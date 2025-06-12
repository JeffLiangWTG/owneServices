package com.cargowise.eservices.common;

import java.lang.reflect.Method;

import javax.xml.soap.SOAPMessage;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.eAdapterAdapter;
import com.cargowise.eservices.common.request.RetrieveStreamResponseRequest;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;
import com.cargowise.eservices.common.request.EServiceSOAPRequest;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class EHubMessageSerializerTests extends TestCase {

	@Test
	public void testSerializeDeserialize() throws Exception {
		try (Stream sourceStream1 = new Stream(EHubMessageSerializerTests.class.getResourceAsStream("BLLIST.xml"));
		Stream sourceStream2 = new Stream(EHubMessageSerializerTests.class.getResourceAsStream("FlatFileSample.txt"))) {
			eHubGatewayMessage message1 = new eHubGatewayMessage();
			message1.setApplicationCode("XMS");
			message1.setEmailSubject("email@email.com");
			message1.setFileName("BLLIST.xml");
			message1.setMessageTrackingID(Guid.newGuid());
			message1.setClientID("RECIPIENT1");
			message1.setSchemaName("BLLIST#Schema");
			message1.setSchemaType(MessageSchemaType.Xml);
			message1.setMessageStream(sourceStream1.compressAndEncode());
			eHubGatewayMessage message2 = new eHubGatewayMessage();
			message2.setApplicationCode("CIM");
			message2.setEmailSubject("email2@email.com");
			message2.setFileName("FlatFileSample.txt");
			message2.setMessageTrackingID(Guid.newGuid());
			message2.setClientID("RECIPIENT2");
			message2.setSchemaName("FlatFile#Schema");
			message2.setSchemaType(MessageSchemaType.FlatFile);
			message2.setMessageStream(sourceStream2.compressAndEncode());

			SendStreamRequest request = new SendStreamRequest(new eHubGatewayMessage[] { message1, message2 });
			ClientCredentials clientCredentials = new ClientCredentials("Sender ID", "password");
			SendMessageStreamRequest webActionSendRequest = new SendMessageStreamRequest(clientCredentials, request);
			Method buildMessageMethod = EServiceSOAPRequest.class.getDeclaredMethod("buildMessage");
			buildMessageMethod.setAccessible(true);
			SOAPMessage soapMessage = (SOAPMessage) buildMessageMethod.invoke(webActionSendRequest);
			RetrieveStreamResponseRequest webActionRetrieveRequest = PowerMockito.spy(new RetrieveStreamResponseRequest(clientCredentials));
			Method getResponeMethod = webActionRetrieveRequest.getClass().getDeclaredMethod("getRespone", SOAPMessage.class);
			getResponeMethod.setAccessible(true);

			RetrieveStreamResponse response = (RetrieveStreamResponse) getResponeMethod.invoke(webActionRetrieveRequest, soapMessage);
			assertEquals(2, response.getMessages().length);
			eHubGatewayMessage message = response.getMessages()[0];
			assertEquals("XMS", message.getApplicationCode());
			assertEquals("email@email.com", message.getEmailSubject());
			assertEquals("BLLIST.xml", message.getFileName());
			assertEquals(message1.getMessageTrackingID(), message.getMessageTrackingID());
			assertEquals("RECIPIENT1", message.getClientID());
			assertEquals("BLLIST#Schema", message.getSchemaName());
			assertEquals(MessageSchemaType.Xml, message.getSchemaType());
			sourceStream1.reset();
			assertEquals(sourceStream1.compressAndEncode().readToEnd(), message.getMessageStream().readToEnd());
			message = response.getMessages()[1];
			assertEquals("CIM", message.getApplicationCode());
			assertEquals("email2@email.com", message.getEmailSubject());
			assertEquals("FlatFileSample.txt", message.getFileName());
			assertEquals(message2.getMessageTrackingID(), message.getMessageTrackingID());
			assertEquals("RECIPIENT2", message.getClientID());
			assertEquals("FlatFile#Schema", message.getSchemaName());
			assertEquals(MessageSchemaType.FlatFile, message.getSchemaType());
			sourceStream2.reset();
			assertEquals(sourceStream2.compressAndEncode().readToEnd(), message.getMessageStream().readToEnd());
		}
	}
}
