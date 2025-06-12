package com.cargowise.eservices.common;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.nio.charset.Charset;

import javax.xml.soap.MessageFactory;
import javax.xml.soap.MimeHeaders;
import javax.xml.soap.SOAPConnection;
import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPMessage;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Matchers;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.eAdapterAdapter;
import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class ServiceExceptionHandlerTests extends TestCase {

	@Test
	public void testEmptyEAdapterInboundRegistration() throws Exception {
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
		final eAdapterAdapter eadapter = PowerMockito.spy(new eAdapterAdapter("url", "username", "password"));
		eAdapterStreamedServiceClient eAdapterStreamedServiceClient = PowerMockito.spy(new eAdapterStreamedServiceClient("url"));


		SendMessageStreamRequest sendStreamRequest = PowerMockito.spy(new SendMessageStreamRequest(
				new ClientCredentials("username", "password"), new SendStreamRequest(
						new eHubGatewayMessage[] {})));

		PowerMockito.doNothing().when(eadapter).close();
		PowerMockito.doReturn(eAdapterStreamedServiceClient)
				.when(eadapter, PowerMockito.method(eAdapterAdapter.class, "createService", String.class, String.class, String.class))
				.withArguments(Matchers.anyString(), Matchers.anyString(), Matchers.anyString());
		PowerMockito.doReturn(sendStreamRequest).when(eAdapterStreamedServiceClient, PowerMockito
				.method(eAdapterStreamedServiceClient.class, "getSendMessageStreamRequest", SendStreamRequest.class))
				.withArguments(Matchers.anyObject());
		PowerMockito.doReturn(getSoapMessageFromString("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/"
				+ "\"><s:Body><s:Fault><faultcode>s:Client</faultcode><faultstring xml:lang=\"en-AU\">Registry eService/"
				+ "eAdapter Inbound Authentications is empty.</faultstring></s:Fault></s:Body></s:Envelope>"))
				.when(sendStreamRequest,	PowerMockito.method(SendMessageStreamRequest.class,
						"sendRequestCore", String.class, SOAPConnection.class))
				.withArguments(Matchers.anyString(), Matchers.any(SOAPConnection.class));

		eadapter.setup("url", "username", "password");

		assertExceptionThrown(eHubAdapterException.class, "Registry eService/eAdapter Inbound Authentications is empty.", new Action() {
			public void run() throws Exception {
				eadapter.sendMessages();
			}
		});
	}

	private SOAPMessage getSoapMessageFromString(final String xml) throws SOAPException, IOException {
		MessageFactory factory = MessageFactory.newInstance();
		SOAPMessage message = factory.createMessage(new MimeHeaders(),
				new ByteArrayInputStream(xml.getBytes(Charset.forName("UTF-8"))));
		return message;
	}

	final void assertExceptionThrown(final Class<?> exception, final String exceptionMessage, final Action action) throws Exception {
		try {
			action.run();
		} catch (Exception e) {
			if (exception.isAssignableFrom(e.getClass())) {
				assertEquals(exceptionMessage, e.getMessage());
				return;
			} else {
				throw e;
			}
		}
		fail("Expected exception " + exception);
	}
}
