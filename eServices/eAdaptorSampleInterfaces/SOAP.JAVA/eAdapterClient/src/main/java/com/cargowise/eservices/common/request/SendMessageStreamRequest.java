package com.cargowise.eservices.common.request;

import java.io.IOException;

import javax.xml.soap.SOAPElement;
import javax.xml.soap.SOAPEnvelope;
import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPMessage;

import org.w3c.dom.DOMException;

import com.cargowise.eservices.common.ClientCredentials;
import com.cargowise.eservices.common.SendStreamRequest;
import com.cargowise.eservices.common.eHubGatewayMessage;

/**
 * A SOAP request class presents SOAP action of CargoWise send stream request.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */

public class SendMessageStreamRequest extends EServiceSOAPRequest {
	private SendStreamRequest request;

	/**
	 * A constructor specifying client credentials and {@link SendStreamRequest} of a SOAP message.
	 * @param clientCredentials the authentication information.
	 * @param request send stream request.
	 * @throws SOAPException if receiving error when calling a SOAP web service.
	 */
	public SendMessageStreamRequest(final ClientCredentials clientCredentials, final SendStreamRequest request) throws SOAPException {
		super(clientCredentials);
		this.request = request;
	}

	@Override
	protected final Object getResponeCore(final SOAPMessage responedSOAPMessage) throws DOMException, SOAPException {
		return true;
	}

	@Override
	protected final SOAPMessage buildMessageCore() throws SOAPException, IOException {
		SOAPMessage message = getSOAPMessageTemplate();
		SOAPEnvelope envelope = getMessageEnvelope(message);

		SOAPElement sendStreamRequestTrackingID = envelope.getHeader().addChildElement("SendStreamRequestTrackingID",
				EHubNamespace.CARGOWISE, EHubNamespace.CARGOWISE_URL);
		sendStreamRequestTrackingID.addTextNode(request.getSendStreamRequestTrackingID().toString());

		SOAPElement sendStreamRequest = envelope.getBody().addChildElement("SendStreamRequest", "",
				EHubNamespace.CARGOWISE_URL);
		SOAPElement payload = sendStreamRequest.addChildElement("Payload");
		for (eHubGatewayMessage gatewayMessage : request.getMessages()) {
			SOAPElement messageElement = payload.addChildElement("Message");
			messageElement.addAttribute(envelope.createName("ApplicationCode"), gatewayMessage.getApplicationCode());
			messageElement.addAttribute(envelope.createName("ClientID"), gatewayMessage.getClientID());
			messageElement.addAttribute(envelope.createName("EmailSubject"), gatewayMessage.getEmailSubject());
			messageElement.addAttribute(envelope.createName("FileName"), gatewayMessage.getFileName());
			messageElement.addAttribute(envelope.createName("SchemaName"), gatewayMessage.getSchemaName());
			messageElement.addAttribute(envelope.createName("SchemaType"), gatewayMessage.getSchemaType().toString());
			messageElement.addAttribute(envelope.createName("TrackingID"),
					gatewayMessage.getMessageTrackingID().toString());
			gatewayMessage.getMessageStream().copyTo(messageElement);
		}

		return message;
	}

	@Override
	protected final String getSOAPAction() {
		return EHubNamespace.CARGOWISE_SENDSTREAM_URL;
	}
}
