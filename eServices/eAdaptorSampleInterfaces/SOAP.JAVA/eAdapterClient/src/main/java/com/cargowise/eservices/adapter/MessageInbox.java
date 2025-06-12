package com.cargowise.eservices.adapter;

import java.io.IOException;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.adapter.interfaces.IMessageInbox;
import com.cargowise.eservices.adapter.interfaces.IeHubFinalizable;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.eHubGatewayMessage;

/**
 * A mailbox for receiving messages.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class MessageInbox extends MessageBox implements IMessageInbox {
	private IeHubFinalizable parentAdapter;
	private String recipientID;
	private Guid theBatchID = Guid.EMPTY;

	/**
	 * Constructor specifying client ID defined in the {@link Adapter} for receiving matching message purpose.
	 * @param parentAdapter the adapter creates new MessageInbox object.
	 * @param recipientID current client ID defined in the adapter as this message's recipient ID.
	 */
	public MessageInbox(final IeHubFinalizable parentAdapter, final String recipientID) {
		this.recipientID = recipientID;
		this.parentAdapter = parentAdapter;
	}

	/**
	 * @return the parentAdapter
	 */
	public IeHubFinalizable getParentAdapter() {
		return parentAdapter;
	}

	/**
	 * @param parentAdapter the parentAdapter to set
	 */
	public void setParentAdapter(final IeHubFinalizable parentAdapter) {
		this.parentAdapter = parentAdapter;
	}

	/**
	 * @return the recipientID
	 */
	public String getRecipientID() {
		return recipientID;
	}

	/**
	 * @param recipientID the recipientID to set
	 */
	public void setRecipientID(final String recipientID) {
		this.recipientID = recipientID;
	}

	/**
	 * @return the batchID
	 */
	public Guid getBatchID() {
		return theBatchID;
	}

	/**
	 * @param batchID the batchID to set
	 */
	public void setBatchID(final Guid batchID) {
		this.theBatchID = batchID;
	}

	/**
	 * Notice the web service that it is finished processing the messages. Send back
	 * {@link com.cargowise.eservices.common.request.FinaliseBatchRequest}.
	 * @throws Exception error while finalise messages
	 */
	public void markAsRead() throws Exception {
		if (!Guid.isEmpty(theBatchID)) {
			parentAdapter.finalise(this.theBatchID);
			this.clear();
			this.theBatchID = Guid.EMPTY;
		}
	}

	/**
	 * Read messages from the web service via adapter as calling
	 * {@link eHubAdapter#retrieveMessages()} and then retrieving those messages into this inbox.
	 * @throws eHubAdapterException if {@link eHubMessage} throws eHubAdapterException
	 * @throws IOException for if the IO returns an issue
	 * @param batchID the batchID
	 * @param messages is the message being used
	 */
	public void readMessageBatch(final Guid batchID, final eHubGatewayMessage[] messages) throws eHubAdapterException, IOException {
		this.theBatchID = batchID;

		for (eHubGatewayMessage gatewayMessage : messages) {
			getMessageList().add(new eHubMessage(gatewayMessage.getMessageTrackingID(), gatewayMessage.getClientID(),
					this.recipientID, gatewayMessage.getSchemaType(), gatewayMessage.getApplicationCode(),
					gatewayMessage.getSchemaName(), gatewayMessage.getMessageStream().decodeAndDecompress(),
					gatewayMessage.getEmailSubject(), gatewayMessage.getFileName()));
		}

	}

	/**
	 * Check if {@link #markAsRead()} is called before executing {@link #readMessageBatch(Guid, eHubGatewayMessage[])}.
	 * It is called inside {@link eHubAdapter#retrieveMessages()}.
	 * @return bacthID as a boolean to see if the batchID is retrievable.
	 */
	public boolean isRetrievableMessages() {
		return Guid.isEmpty(theBatchID);
	}
}
