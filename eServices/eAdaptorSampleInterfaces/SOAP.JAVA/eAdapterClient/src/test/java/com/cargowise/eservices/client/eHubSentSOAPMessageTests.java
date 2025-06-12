package com.cargowise.eservices.client;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.StringReader;
import java.lang.reflect.Method;
import java.nio.charset.StandardCharsets;

import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPMessage;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactoryConfigurationError;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.eAdapterAdapter;
import com.cargowise.eservices.common.ClientCredentials;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.SendStreamRequest;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.common.eHubGatewayMessage;
import com.cargowise.eservices.common.request.PingRequest;
import com.cargowise.eservices.common.request.RetrieveStreamResponseRequest;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class EHubSentSOAPMessageTests extends TestCase {
	@Test
	public void testSOAPMessage_PingRequest()
			throws TransformerFactoryConfigurationError, Exception {
		ClientCredentials crendentails = Mockito.spy(new ClientCredentials("Sender ID", "Password"));
		Mockito.stub(crendentails.getCurrentDate()).toReturn("2015-12-02T02:12:17.186Z");
		Mockito.stub(crendentails.getExpiryDate()).toReturn("2015-12-02T02:17:17.186Z");

		PingRequest pingRequest = Mockito.spy(new PingRequest(crendentails));
		Mockito.when(pingRequest.getSeconds()).thenReturn(47); // mock private method
		Method method = pingRequest.getClass().getDeclaredMethod("buildMessage");
		method.setAccessible(true);
		SOAPMessage soapMessage = (SOAPMessage) method.invoke(pingRequest);

		StringBuilder sb = new StringBuilder();
		sb.append("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" "
				+ "xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">\n");
		sb.append("  <SOAP-ENV:Header>\n");
		sb.append("    <o:Security xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-"
				+ "200401-wss-wssecurity-secext-1.0.xsd\" SOAP-ENV:mustUnderstand=\"1\">\n");
		sb.append("      <u:Timestamp u:Id=\"_0\">\n");
		sb.append("        <u:Created>2015-12-02T02:12:17.186Z</u:Created>\n");
		sb.append("        <u:Expires>2015-12-02T02:17:17.186Z</u:Expires>\n");
		sb.append("      </u:Timestamp>\n");
		sb.append("      <o:UsernameToken u:Id=\"uuid-4d6760bc-c1bb-3076-bf47-4030f9dedcfd-47\">\n");
		sb.append("        <o:Username>Sender ID</o:Username>\n");
		sb.append("        <o:Password o:Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-"
				+ "200401-wss-username-token-profile-1.0#PasswordText\">Password</o:Password>\n");
		sb.append("      </o:UsernameToken>\n");
		sb.append("    </o:Security>\n");
		sb.append("  </SOAP-ENV:Header>\n");
		sb.append("  <SOAP-ENV:Body>\n");
		sb.append("    <Ping xmlns=\"http://CargoWise.com/eHub/2010/06\"/>\n");
		sb.append("  </SOAP-ENV:Body>\n");
		sb.append("</SOAP-ENV:Envelope>");

		assertEquals(xmlStrim(sb.toString()), getXMLMessage(soapMessage));
	}

	@Test
	public void testSOAPMessage_SendMessageStreamRequest()
			throws TransformerFactoryConfigurationError, Exception {
		ClientCredentials crendentails = Mockito.spy(new ClientCredentials("Sender ID", "Password"));
		Mockito.stub(crendentails.getCurrentDate()).toReturn("2015-12-02T02:12:17.186Z");
		Mockito.stub(crendentails.getExpiryDate()).toReturn("2015-12-02T02:17:17.186Z");

		eHubGatewayMessage message1 = new eHubGatewayMessage();
		message1.setApplicationCode("XMS");
		message1.setEmailSubject("email@email.com");
		message1.setFileName("BLLIST.xml");
		message1.setMessageTrackingID(new Guid("dcff3331-23bf-41df-ab99-3d1b427debde"));
		message1.setClientID("RECIPIENT1");
		message1.setSchemaName("BLLIST#Schema");
		message1.setSchemaType(MessageSchemaType.Xml);
		message1.setMessageStream(new Stream(new BufferedInputStream(new ByteArrayInputStream("Test".getBytes(StandardCharsets.UTF_8)))));

		eHubGatewayMessage message2 = new eHubGatewayMessage();
		message2.setApplicationCode("CIM");
		message2.setEmailSubject("email2@email.com");
		message2.setFileName("FlatFileSample.txt");
		message2.setMessageTrackingID(new Guid("b2d61d56-f000-4d1f-8e19-58ad441ceb20"));
		message2.setClientID("RECIPIENT2");
		message2.setSchemaName("FlatFile#Schema");
		message2.setSchemaType(MessageSchemaType.FlatFile);
		message2.setMessageStream(new Stream(new BufferedInputStream(new ByteArrayInputStream("Test2".getBytes(StandardCharsets.UTF_8)))));
		Guid trackingID = new Guid("ffe25258-b9b6-4896-840f-3e2adb35b923");
		SendStreamRequest request = new SendStreamRequest(trackingID, new eHubGatewayMessage[] { message1, message2 });
		SendMessageStreamRequest pingRequest = PowerMockito.spy(new SendMessageStreamRequest(crendentails, request));
		PowerMockito.doReturn(47).when(pingRequest, "getSeconds");
		Method method = pingRequest.getClass().getDeclaredMethod("buildMessage");
		method.setAccessible(true);
		SOAPMessage soapMessage = (SOAPMessage) method.invoke(pingRequest);

		StringBuilder sb = new StringBuilder();
		sb.append("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">\n");
		sb.append("  <SOAP-ENV:Header>\n");
		sb.append("    <o:Security xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" SOAP-ENV:mustUnderstand=\"1\">\n");
		sb.append("      <u:Timestamp u:Id=\"_0\">\n");
		sb.append("        <u:Created>2015-12-02T02:12:17.186Z</u:Created>\n");
		sb.append("        <u:Expires>2015-12-02T02:17:17.186Z</u:Expires>\n");
		sb.append("      </u:Timestamp>\n");
		sb.append("      <o:UsernameToken u:Id=\"uuid-4d6760bc-c1bb-3076-bf47-4030f9dedcfd-47\">\n");
		sb.append("        <o:Username>Sender ID</o:Username>\n");
		sb.append("        <o:Password o:Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-"
				+ "username-token-profile-1.0#PasswordText\">Password</o:Password>\n");
		sb.append("      </o:UsernameToken>\n");
		sb.append("    </o:Security>\n");
		sb.append("    <h:SendStreamRequestTrackingID xmlns:h=\"http://CargoWise.com/eHub/2010/06\">"
				+ "ffe25258-b9b6-4896-840f-3e2adb35b923</h:SendStreamRequestTrackingID>\n");
		sb.append("  </SOAP-ENV:Header>\n");
		sb.append("  <SOAP-ENV:Body>\n");
		sb.append("    <SendStreamRequest xmlns=\"http://CargoWise.com/eHub/2010/06\">\n");
		sb.append("      <Payload>\n");
		sb.append("        <Message ApplicationCode=\"XMS\" ClientID=\"RECIPIENT1\" EmailSubject=\"email@email.com\" "
				+ "FileName=\"BLLIST.xml\" SchemaName=\"BLLIST#Schema\" SchemaType=\"Xml\" TrackingID="
				+ "\"dcff3331-23bf-41df-ab99-3d1b427debde\">Test</Message>\n");
		sb.append("        <Message ApplicationCode=\"CIM\" ClientID=\"RECIPIENT2\" EmailSubject=\"email2@email.com\" "
				+ "FileName=\"FlatFileSample.txt\" SchemaName=\"FlatFile#Schema\" SchemaType=\"FlatFile\" TrackingID="
				+ "\"b2d61d56-f000-4d1f-8e19-58ad441ceb20\">Test2</Message>\n");
		sb.append("      </Payload>\n");
		sb.append("    </SendStreamRequest>\n");
		sb.append("  </SOAP-ENV:Body>\n");
		sb.append("</SOAP-ENV:Envelope>");
		assertEquals(xmlStrim(sb.toString()), getXMLMessage(soapMessage));
	}

	@Test
	public void testSOAPMessage_RetrieveStreamResponseRequest()
			throws TransformerFactoryConfigurationError, Exception {
		ClientCredentials crendentails = Mockito.spy(new ClientCredentials("Sender ID", "Password"));
		Mockito.stub(crendentails.getCurrentDate()).toReturn("2015-12-02T02:12:17.186Z");
		Mockito.stub(crendentails.getExpiryDate()).toReturn("2015-12-02T02:17:17.186Z");

		RetrieveStreamResponseRequest request = PowerMockito.spy(new RetrieveStreamResponseRequest(crendentails));
		PowerMockito.doReturn(47).when(request, "getSeconds");
		Method method = request.getClass().getDeclaredMethod("buildMessage");
		method.setAccessible(true);
		SOAPMessage soapMessage = (SOAPMessage) method.invoke(request);

		StringBuilder sb = new StringBuilder();
		sb.append("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">\n");
		sb.append("  <SOAP-ENV:Header>\n");
		sb.append("    <o:Security xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" SOAP-ENV:mustUnderstand=\"1\">\n");
		sb.append("      <u:Timestamp u:Id=\"_0\">\n");
		sb.append("        <u:Created>2015-12-02T02:12:17.186Z</u:Created>\n");
		sb.append("        <u:Expires>2015-12-02T02:17:17.186Z</u:Expires>\n");
		sb.append("      </u:Timestamp>\n");
		sb.append("      <o:UsernameToken u:Id=\"uuid-4d6760bc-c1bb-3076-bf47-4030f9dedcfd-47\">\n");
		sb.append("        <o:Username>Sender ID</o:Username>\n");
		sb.append("        <o:Password o:Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-"
				+ "wss-username-token-profile-1.0#PasswordText\">Password</o:Password>\n");
		sb.append("      </o:UsernameToken>\n");
		sb.append("    </o:Security>\n");
		sb.append("  </SOAP-ENV:Header>\n");
		sb.append("  <SOAP-ENV:Body/>\n");
		sb.append("</SOAP-ENV:Envelope>");

		assertEquals(xmlStrim(sb.toString()), getXMLMessage(soapMessage));
	}

	final String getXMLMessage(final SOAPMessage message)
			throws SOAPException, IOException, TransformerFactoryConfigurationError, TransformerException {
		ByteArrayOutputStream out = new ByteArrayOutputStream();
		message.writeTo(out);
		return out.toString(StandardCharsets.UTF_8.name());
	}

	public static String xmlStrim(final String input) {
		BufferedReader reader = new BufferedReader(new StringReader(input));
		StringBuffer result = new StringBuffer();
		try {
			String line;
			while ((line = reader.readLine()) != null) {
				result.append(line.trim());
			}
			return result.toString();
		} catch (IOException e) {
				throw new RuntimeException(e);
		}
	}
}
