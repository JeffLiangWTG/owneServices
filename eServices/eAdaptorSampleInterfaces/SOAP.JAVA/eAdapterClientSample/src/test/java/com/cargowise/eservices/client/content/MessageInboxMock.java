package com.cargowise.eservices.client.content;

import java.util.Collection;

import com.cargowise.eservices.adapter.MessageBox;
import com.cargowise.eservices.adapter.interfaces.IMessageInbox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.eHubGatewayMessage;

public class MessageInboxMock extends MessageBox implements IMessageInbox {
	boolean canRetrieveMessages;

	public void readMessageBatch(Guid batchID, eHubGatewayMessage[] messages) {
	}

	public final boolean isRetrievableMessages() {
		return canRetrieveMessages;
	}

	public final void setCanRetrieveMessages(boolean canRetrieveMessages) {
		this.canRetrieveMessages = canRetrieveMessages;
	}

	public final void markAsRead() {
		getMessageList().clear();
	}

	public final void addMessage(IeHubMessage message) {
		getMessageList().add(message);
	}

	public final void addMessage(Collection<IeHubMessage> retrieveMessages) {
		getMessageList().addAll(retrieveMessages);
	}
}
