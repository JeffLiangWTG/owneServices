package com.cargowise.eservices.common.request;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Iterator;

import javax.xml.soap.SOAPBody;
import javax.xml.soap.SOAPElement;
import javax.xml.soap.SOAPEnvelope;
import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPHeader;
import javax.xml.soap.SOAPMessage;

import org.w3c.dom.DOMException;
import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import com.cargowise.eservices.common.ClientCredentials;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.RetrieveStreamResponse;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.common.eHubGatewayMessage;

/**
 * A SOAP request class presents SOAP action of CargoWise retrieve stream response request.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class RetrieveStreamResponseRequest extends EServiceSOAPRequest {
	/**
	 * A constructor specifying client credentials.
	 * @param clientCredentials the authentication information.
	 * @throws SOAPException if receiving error when calling a SOAP web service.
	 */
	public RetrieveStreamResponseRequest(final ClientCredentials clientCredentials) throws SOAPException {
		super(clientCredentials);
	}

	@SuppressWarnings("rawtypes")
	@Override
	protected final Object getResponeCore(final SOAPMessage responedSOAPMessage) throws DOMException, SOAPException, IOException {
		ArrayList<eHubGatewayMessage> messages = new ArrayList<eHubGatewayMessage>();

		SOAPEnvelope envelope = getMessageEnvelope(responedSOAPMessage);

		SOAPHeader header = envelope.getHeader();
		Guid trackingID = Guid.EMPTY;
		for (Iterator i = header.getChildElements(); i.hasNext();) {
			SOAPElement current = (SOAPElement) i.next();
			if (current.getLocalName().equals("TrackingID")) {
				trackingID = new Guid(current.getFirstChild().getNodeValue());
			}
		}

		SOAPBody body = envelope.getBody();
		NodeList messageList = body.getFirstChild().getFirstChild().getChildNodes();
		for (int i = 0; i < messageList.getLength(); i++) {
			Node currentMessage = messageList.item(i);
			eHubGatewayMessage gatewayMessage = new eHubGatewayMessage();
			NamedNodeMap attributes = currentMessage.getAttributes();
			gatewayMessage.setApplicationCode(attributes.getNamedItem("ApplicationCode").getNodeValue());
			gatewayMessage.setClientID(attributes.getNamedItem("ClientID").getNodeValue());
			gatewayMessage.setEmailSubject(attributes.getNamedItem("EmailSubject").getNodeValue());
			gatewayMessage.setFileName(attributes.getNamedItem("FileName").getNodeValue());
			gatewayMessage.setMessageTrackingID(new Guid(attributes.getNamedItem("TrackingID").getNodeValue()));
			gatewayMessage.setSchemaName(attributes.getNamedItem("SchemaName").getNodeValue());
			gatewayMessage
					.setSchemaType(MessageSchemaType.valueOf(attributes.getNamedItem("SchemaType").getNodeValue()));
			gatewayMessage.setMessageStream(getContentStream(currentMessage.getChildNodes()));
			messages.add(gatewayMessage);
		}

		return new RetrieveStreamResponse(trackingID, messages.toArray(new eHubGatewayMessage[messages.size()]));
	}

	static Stream getContentStream(final NodeList childNodes) throws IOException {
		StringBuilder bd = new StringBuilder();
		for (int i = 0; i < childNodes.getLength(); i++) {
			bd.append(childNodes.item(i).getNodeValue());
		}
		return new Stream(new ByteArrayInputStream(bd.toString().getBytes(StandardCharsets.UTF_8)));
	}

	@Override
	protected final SOAPMessage buildMessageCore() throws SOAPException, IOException {
		return getSOAPMessageTemplate();
	}

	@Override
	protected final String getSOAPAction() {
		return EHubNamespace.CARGOWISE_RETRIEVESTREAM_URL;
	}
}
