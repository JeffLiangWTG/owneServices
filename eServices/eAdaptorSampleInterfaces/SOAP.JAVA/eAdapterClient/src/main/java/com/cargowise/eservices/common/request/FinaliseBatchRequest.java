package com.cargowise.eservices.common.request;

import java.io.IOException;

import javax.xml.soap.SOAPElement;
import javax.xml.soap.SOAPEnvelope;
import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPMessage;

import org.w3c.dom.DOMException;

import com.cargowise.eservices.common.ClientCredentials;
import com.cargowise.eservices.common.Guid;

/**
 * A SOAP request class presents SOAP action of CargoWise finalise batch request.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class FinaliseBatchRequest extends EServiceSOAPRequest {
	private final Guid trackingID;

	/**
	 * A constructor specifying client credentials and tracking id of a SOAP message.
	 * @param clientCredentials the authentication information.
	 * @param trackingID tracking id of a SOAP message.
	 * @throws SOAPException if receiving error when calling a SOAP web service.
	 */
	public FinaliseBatchRequest(final ClientCredentials clientCredentials, final Guid trackingID) throws SOAPException {
		super(clientCredentials);
		this.trackingID = trackingID;
	}

	@Override
	protected final Object getResponeCore(final SOAPMessage responedSOAPMessage) throws DOMException, SOAPException {
		return true;
	}

	@Override
	protected final SOAPMessage buildMessageCore() throws SOAPException, IOException {
		SOAPMessage message = getSOAPMessageTemplate();
		SOAPEnvelope envelope = getMessageEnvelope(message);
		SOAPElement sendStreamRequest = envelope.getBody().addChildElement("FinaliseBatch", "", EHubNamespace.CARGOWISE_URL);
		SOAPElement payload = sendStreamRequest.addChildElement("trackingID");
		payload.addTextNode(trackingID.toString());

		return message;
	}

	@Override
	protected final String getSOAPAction() {
		return EHubNamespace.CARGOWISE_FINALISEBATCH_URL;
	}

}
