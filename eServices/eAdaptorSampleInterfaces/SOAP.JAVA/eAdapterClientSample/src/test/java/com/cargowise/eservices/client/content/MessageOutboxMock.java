package com.cargowise.eservices.client.content;

import com.cargowise.eservices.adapter.MessageBox;
import com.cargowise.eservices.adapter.interfaces.IMessageOutbox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;

public class MessageOutboxMock extends MessageBox implements IMessageOutbox {
	public final void addMessage(IeHubMessage message) {
		getMessageList().add(message);
	}
}
