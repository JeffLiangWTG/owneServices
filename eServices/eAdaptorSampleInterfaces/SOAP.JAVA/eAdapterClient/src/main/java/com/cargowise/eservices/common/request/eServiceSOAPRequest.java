package com.cargowise.eservices.common.request;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.Calendar;
import javax.xml.soap.MessageFactory;
import javax.xml.soap.MimeHeaders;
import javax.xml.soap.Name;
import javax.xml.soap.SOAPBody;
import javax.xml.soap.SOAPConnection;
import javax.xml.soap.SOAPElement;
import javax.xml.soap.SOAPEnvelope;
import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPHeader;
import javax.xml.soap.SOAPMessage;

import org.w3c.dom.DOMException;
import org.w3c.dom.Node;

import com.cargowise.eservices.common.ClientCredentials;
import com.cargowise.eservices.common.FaultException;

/**
 * An abstract class for eService SOAP action.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public abstract class EServiceSOAPRequest {
	private ClientCredentials clientCredentials;

	/**
	 * A constructor specifying the client credential configurations.
	 * @param clientCredentials service credential authentication settings
	 * @throws SOAPException if receiving error when calling a SOAP web service.
	 */
	public EServiceSOAPRequest(final ClientCredentials clientCredentials) throws SOAPException {
		this.clientCredentials = clientCredentials;
	}

	/**
	 * Send a request to the web service and return the response from that web service.
	 * @param endpoint the url of the web service.
	 * @param connection the current SOAP connection.
	 * @return the response from the web service.
	 * @throws Exception the issues could occur during the transaction.
	 */
	public Object sendRequestAndGetRespone(final String endpoint, final SOAPConnection connection) throws Exception {
		SOAPMessage responedSOAPMessage = sendRequestCore(endpoint, connection);
		return getRespone(responedSOAPMessage);
	}

	/**
	 * Class for getting a response that takes in a SOAP message, and a Responed SOAP message, then hands out a response.
	 * @param responedSOAPMessage is the Responed message.
	 * @return getResponseCore is the message formed using both the SOAP message and the responed SOAP message
	 * @throws DOMException throws the DOMException
	 * @throws SOAPException throws the SOAPException
	 * @throws FaultException throws FaultException
	 * @throws IOException throws IO Exception
	 */
	protected Object getRespone(final SOAPMessage responedSOAPMessage) throws DOMException, SOAPException, FaultException, IOException {
		SOAPBody body = responedSOAPMessage.getSOAPBody();
		Node firstChild = body.getFirstChild();
		if (firstChild == null) {
			return getResponeCore(responedSOAPMessage);
		} else if (firstChild.getLocalName().equals("Fault")) {
			throw new FaultException(responedSOAPMessage);
		}
		return getResponeCore(responedSOAPMessage);
	}

	/**
	 * 
	 * @param responedSOAPMessage is the Responed message.
	 * @return a call from the connection method
	 * @throws DOMException throws the DOMException
	 * @throws SOAPException throws the SOAPException
	 * @throws IOException throws IO Exception
	 */
	protected abstract Object getResponeCore(SOAPMessage responedSOAPMessage) throws DOMException, SOAPException, IOException;

	final SOAPMessage sendRequestCore(final String endpoint, final SOAPConnection connection)
			throws UnsupportedOperationException, SOAPException, IOException {
		return connection.call(buildMessage(), endpoint);
	}

	/**
	 * SOAPMessage that builds message Core.
	 * @return soapMessage
	 * @throws SOAPException throws the SOAPException
	 * @throws IOException throws IO Exception
	 */
	protected abstract SOAPMessage buildMessageCore() throws SOAPException, IOException;

	/**
	 * Gets the SOAP Action.
	 * @return soap Message
	 */
	protected abstract String getSOAPAction();

	/**
	 * SOAP Message that builds the message (not core).
	 * @return soap Message
	 * @throws SOAPException throws the SOAPException
	 * @throws IOException throws IO Exception
	 */
	protected SOAPMessage buildMessage() throws SOAPException, IOException {
		SOAPMessage soapMessage = buildMessageCore();
		MimeHeaders mimeHeaders = soapMessage.getMimeHeaders();
		mimeHeaders.setHeader("SOAPAction", getSOAPAction());
		mimeHeaders.setHeader("Expect", "100-continue");
		soapMessage.saveChanges();
		return soapMessage;
	}

	/**
	 * gets the Message Envelope.
	 * @param soapMessage is the SOAP message used
	 * @return the soap section of the soap message
	 * @throws SOAPException throws the SOAP Exception
	 */
	protected static SOAPEnvelope getMessageEnvelope(final SOAPMessage soapMessage) throws SOAPException {
		return soapMessage.getSOAPPart().getEnvelope();
	}

	/**
	 * gets the soap message template.
	 * @return soapMessage
	 * @throws SOAPException throws the SOAP Exception
	 */
	protected SOAPMessage getSOAPMessageTemplate() throws SOAPException {
		MessageFactory messageFactory = MessageFactory.newInstance();
		SOAPMessage soapMessage = messageFactory.createMessage();
		SOAPEnvelope envelope = getMessageEnvelope(soapMessage);

		SOAPHeader header = envelope.getHeader();
		envelope.addNamespaceDeclaration(EHubNamespace.WSSECURITY_UTILITY, EHubNamespace.WSSECURITY_UTILITY_URL);

		SOAPElement security = header.addChildElement("Security", EHubNamespace.WSSECURITY_SECEXT,
				EHubNamespace.WSSECURITY_SECEXT_URL);
		Name qname = envelope.createName("mustUnderstand", EHubNamespace.SOAP_ENV, EHubNamespace.SOAP_ENV_URL);
		security.addAttribute(qname, "1");

		SOAPElement timestamp = security.addChildElement("Timestamp", EHubNamespace.WSSECURITY_UTILITY);
		qname = envelope.createName("Id", EHubNamespace.WSSECURITY_UTILITY, EHubNamespace.WSSECURITY_UTILITY_URL);
		timestamp.addAttribute(qname, "_0");
		SOAPElement created = timestamp.addChildElement("Created", EHubNamespace.WSSECURITY_UTILITY);
		created.addTextNode(clientCredentials.getCurrentDate());
		SOAPElement expires = timestamp.addChildElement("Expires", EHubNamespace.WSSECURITY_UTILITY);
		expires.addTextNode(clientCredentials.getExpiryDate());

		SOAPElement usernameToken = security.addChildElement("UsernameToken", EHubNamespace.WSSECURITY_SECEXT);
		qname = envelope.createName("Id", EHubNamespace.WSSECURITY_UTILITY, EHubNamespace.WSSECURITY_UTILITY_URL);
		usernameToken.addAttribute(qname, getUsernameTokenUUID());
		SOAPElement username = usernameToken.addChildElement("Username", EHubNamespace.WSSECURITY_SECEXT);
		username.addTextNode(clientCredentials.getUsername());
		SOAPElement password = usernameToken.addChildElement("Password", EHubNamespace.WSSECURITY_SECEXT);
		password.addTextNode(clientCredentials.getPassword());
		qname = envelope.createName("Type", EHubNamespace.WSSECURITY_SECEXT, EHubNamespace.WSSECURITY_SECEXT_URL);
		password.addAttribute(qname, EHubNamespace.USERNAME_TOKEN_URL);

		return soapMessage;
	}

	final String getUsernameTokenUUID() {
		return "uuid-"
				+ java.util.UUID.nameUUIDFromBytes(clientCredentials.getUsername().getBytes(StandardCharsets.UTF_8))
				+ "-" + getSeconds();
	}

	/**
	 * getSeconds gets the time of running from the calendar in seconds.
	 * @return Calendar Seconds
	 */
	public int getSeconds() {
		return Calendar.getInstance().get(Calendar.SECOND);
	}

	/**
	 * The E-hub name space.
	 * @author Karl.Silis
	 *
	 */
	protected static class EHubNamespace { //NOPMD
		public static final String WSSECURITY_UTILITY = "u";
		public static final String WSSECURITY_UTILITY_URL =
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
		public static final String WSSECURITY_SECEXT = "o";
		public static final String WSSECURITY_SECEXT_URL =
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
		public static final String SOAP_ENV = "SOAP-ENV";
		public static final String SOAP_ENV_URL = "http://schemas.xmlsoap.org/soap/envelope/";
		public static final String USERNAME_TOKEN_URL =
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
		public static final String CARGOWISE = "h";
		public static final String CARGOWISE_URL = "http://CargoWise.com/eHub/2010/06";
		public static final String CARGOWISE_PING_URL = "http://CargoWise.com/eHub/2010/06/eAdapterStreamedService/Ping";
		public static final String CARGOWISE_SENDSTREAM_URL = "http://CargoWise.com/eHub/2010/06/eAdapterStreamedService/SendStream";
		public static final String CARGOWISE_RETRIEVESTREAM_URL = "http://CargoWise.com/eHub/2010/06/eHubStreamedService/RetrieveStream";
		public static final String CARGOWISE_FINALISEBATCH_URL = "http://CargoWise.com/eHub/2010/06/eHubStreamedService/FinaliseBatch";
	}
}
