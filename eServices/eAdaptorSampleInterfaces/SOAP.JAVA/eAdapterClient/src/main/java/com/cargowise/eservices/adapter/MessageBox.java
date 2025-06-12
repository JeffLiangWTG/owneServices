package com.cargowise.eservices.adapter;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import com.cargowise.eservices.adapter.interfaces.IMessageBox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;



/**
 * A mailbox contains and manages {@link IeHubMessage} messages.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public abstract class MessageBox implements IMessageBox {
	private final List<IeHubMessage> messageList = new ArrayList<IeHubMessage>();

	/** Default constructor. */
	public MessageBox() { }
	/**
	 * The total size in bytes of all the messages in the mailbox.
	 * @throws IOException if the IO requires it
	 * @return totalSizeInBytes
	 * 
	 */
	public long getSizeInKiloBytes() throws IOException {
		long totalSizeInBytes = 0;
		final int maxBytes = 1024;
		for (IeHubMessage message : this) {
			totalSizeInBytes += message.getMessageStream().count();
		}
		return totalSizeInBytes / maxBytes;
	}

	/**
	 * The number of messages.
	 * @return the list of messages.
	 */
	public int count() {
		return messageList.size();
	}

	/**
	 * Defined for calling for-each with mailbox.
	 * @return messageList which is a list of messages
	 */
	public Iterator<IeHubMessage> iterator() {
		return messageList.iterator();
	}

	/**
	 * Clear all the message in the mailbox.
	 */
	public void clear() {
		messageList.clear();
	}
	/**
	 * @return the messageList
	 */
	public List<IeHubMessage> getMessageList() {
		return messageList;
	}
}
