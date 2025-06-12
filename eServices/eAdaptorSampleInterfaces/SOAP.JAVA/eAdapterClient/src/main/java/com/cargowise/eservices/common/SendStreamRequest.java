package com.cargowise.eservices.common;

/**
 * Send stream request to the web service, it contains the trackingID of the request message and the list of ehub gateway messages.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class SendStreamRequest {
	private Guid sendStreamRequestTrackingID;
	private eHubGatewayMessage[] messages;

	/**
	 * A constructor specifying the list of ehub gateway messages.
	 * @param messages the list of ehub gateway messages
	 */
	public SendStreamRequest(final eHubGatewayMessage[] messages) {
		this(Guid.newGuid(), messages);
	}

	/**
	 * A constructor specifying the trackingID of response message and the list of ehub gateway messages.
	 * @param sendStreamRequestTrackingID the id of the request message.
	 * @param messages the list of ehub gateway messages.
	 */
	public SendStreamRequest(final Guid sendStreamRequestTrackingID, final eHubGatewayMessage[] messages) {
		this.sendStreamRequestTrackingID = sendStreamRequestTrackingID;
		this.messages = messages;
	}

	/**
	 * @return the sendStreamRequestTrackingID
	 */
	public Guid getSendStreamRequestTrackingID() {
		return sendStreamRequestTrackingID;
	}

	/**
	 * @param sendStreamRequestTrackingID the sendStreamRequestTrackingID to set
	 */
	public void setSendStreamRequestTrackingID(final Guid sendStreamRequestTrackingID) {
		this.sendStreamRequestTrackingID = sendStreamRequestTrackingID;
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
