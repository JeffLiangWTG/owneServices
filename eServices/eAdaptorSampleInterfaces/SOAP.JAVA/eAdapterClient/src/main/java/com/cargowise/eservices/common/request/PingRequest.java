package com.cargowise.eservices.common.request;

import javax.xml.soap.SOAPBody;
import javax.xml.soap.SOAPEnvelope;
import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPMessage;

import org.w3c.dom.DOMException;

import com.cargowise.eservices.common.ClientCredentials;

/**
 * A SOAP request class presents SOAP action of CargoWise ping request.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class PingRequest extends EServiceSOAPRequest {
	/**
	 * A constructor specifying client credentials.
	 * @param clientCredentials the authentication information.
	 * @throws SOAPException if receiving error when calling a SOAP web service.
	 */
	public PingRequest(final ClientCredentials clientCredentials) throws SOAPException {
		super(clientCredentials);
	}

	@Override
	protected final Object getResponeCore(final SOAPMessage responedSOAPMessage) throws DOMException, SOAPException {
		return responedSOAPMessage.getSOAPBody().getFirstChild().getFirstChild().getFirstChild().getNodeValue();
	}

	@Override
	protected final SOAPMessage buildMessageCore() throws SOAPException {
		SOAPMessage message = getSOAPMessageTemplate();
		SOAPEnvelope envelope = getMessageEnvelope(message);
		SOAPBody body = envelope.getBody();
		body.addChildElement("Ping", "", EHubNamespace.CARGOWISE_URL);

		return message;
	}

	@Override
	protected final String getSOAPAction() {
		return EHubNamespace.CARGOWISE_PING_URL;
	}
}
