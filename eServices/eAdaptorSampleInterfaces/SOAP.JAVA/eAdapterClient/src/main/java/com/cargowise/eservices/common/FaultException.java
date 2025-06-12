package com.cargowise.eservices.common;

import java.io.StringWriter;

import javax.xml.soap.SOAPException;
import javax.xml.soap.SOAPMessage;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.w3c.dom.DOMException;
import org.w3c.dom.Node;

/**
 * Represents a SOAP fault.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class FaultException extends Exception {
	private static final long serialVersionUID = -1989645323721240584L;
	private String faultCode = "";
	private String xmlNode = "";

	/**
	 * Initializes a new instance of the FaultException class using the specified fault SOAP message.
	 * @param responedSOAPMessage a SOAP message which is in faulty state.
	 * @throws DOMException DOMSTRING_SIZE_ERR: Raised when it would return more characters than
	 * fit in a DOMString variable on the implementation platform.
	 * @throws SOAPException error while proccessing SOAP message.
	 */
	public FaultException(final SOAPMessage responedSOAPMessage) throws DOMException, SOAPException {
		this(responedSOAPMessage.getSOAPBody().getFirstChild().getChildNodes().item(0).getFirstChild().getNodeValue(),
				responedSOAPMessage.getSOAPBody().getFirstChild().getChildNodes().item(1).getFirstChild().getNodeValue());
		this.xmlNode = nodeToString(responedSOAPMessage.getSOAPBody().getFirstChild());
	}

	/**
	 * Initializes a new instance of the FaultException class using the specified reason.
	 * @param message error message.
	 */
	public FaultException(final String message) {
		super(message);
	}

	/**
	 * Initializes a new instance of the FaultException class using the specified reason and SOAP fault code.
	 * @param faultCode the SOAP fault code for the fault.
	 * @param message the reason for the SOAP fault.
	 */
	public FaultException(final String faultCode, final String message) {
		super(message);
		this.faultCode = faultCode;
	}

	/**
	 * @return the SOAP fault code for the fault.
	 */
	public String getFaultCode() {
		return faultCode;
	}

	/**
	 * @return the xml content of fault node inside SOAP message.
	 */
	public String getXMLNode() {
		return xmlNode;
	}

	static String nodeToString(final Node node) {
		StringWriter sw = new StringWriter();

		try {
			Transformer t = TransformerFactory.newInstance().newTransformer();
			t.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
			t.setOutputProperty(OutputKeys.INDENT, "yes");
			t.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", "2");
			t.transform(new DOMSource(node), new StreamResult(sw));
		} catch (TransformerException e) {
			e.printStackTrace();
			return "";
		}

		return sw.toString();
	}

	/**
	 * Creates and returns a string representation of the fault exception.
	 */
	@Override
	public String toString() {
		StringBuilder builder = new StringBuilder();
		builder.append(String.format("Exception: %1$s.%n", getClass().getName()));
		builder.append(String.format("FaultCode: %1$s%n", faultCode));
		builder.append(String.format("Message: %1$s", getLocalizedMessage()));
		if (!xmlNode.isEmpty()) {
			builder.append(String.format("%n%1$s", xmlNode));
		}
		return builder.toString();
	}
}
