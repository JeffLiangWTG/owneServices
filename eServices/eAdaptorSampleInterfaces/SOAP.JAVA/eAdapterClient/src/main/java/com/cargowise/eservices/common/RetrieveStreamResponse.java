package com.cargowise.eservices.common;

/**
 * Retrieve stream response from the web service, it contains the trackingID of response message and the list of ehub gateway messages.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class RetrieveStreamResponse {
	private Guid trackingID;
	private eHubGatewayMessage[] messages;

	/**
	 * Default constructor.
	 */
	public RetrieveStreamResponse() {
	}

	/**
	 * A constructor specifying the trackingID of response message and the list of ehub gateway messages.
	 * @param trackingID batch ID of SOAP message
	 * @param messages list of {@link eHubGatewayMessage}
	 */
	public RetrieveStreamResponse(final Guid trackingID, final eHubGatewayMessage[] messages) {
		this.trackingID = trackingID;
		this.messages = messages;
	}

	/**
	 * @return the trackingID
	 */
	public Guid getTrackingID() {
		return trackingID;
	}

	/**
	 * @param trackingID the trackingID to set
	 */
	public void setTrackingID(final Guid trackingID) {
		this.trackingID = trackingID;
	}

	/**
	 * @return the messages
	 */
	public eHubGatewayMessage[] getMessages() {
		return messages;
	}

	/**
	 * @param messages the messages to set
	 */
	public void setMessages(final eHubGatewayMessage[] messages) {
		this.messages = messages;
	}
}
