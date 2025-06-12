package com.cargowise.eservices.client.content;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map.Entry;

import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.common.Guid;

/**
 * DataStore class presents the database of messages and their status
 * Using LinkedHashMap to display message list by their inserted order.
 */
public class DataStore extends LinkedHashMap<eHubMessage, MessageStatus> {
	private static final long serialVersionUID = 1577043941407081931L;

	/**
	 * This shows the message status.
	 * @param key the key
	 * @param value the value
	 * @return  the key and value together as a put
	 * @throws InvalidDataStoreKeyException throws Invalid Data Store Keys
	 */
	public MessageStatus add(eHubMessage key, MessageStatus value) throws InvalidDataStoreKeyException {
		if (containsKey(key)) {
			throw new InvalidDataStoreKeyException(String.format("A message with trackingID {%s} is already in the pending list. Please assign each message all having different trackingid.",
					key.getTrackingID()));
		} else {
			return put(key, value);
		}
	}

	/**
	 * update functionality.
	 * @param key this is the key
	 * @param value this is the value
	 * @throws InvalidDataStoreKeyException throws Invalid Data Store Keys
	 */
	public void update(eHubMessage key, MessageStatus value) throws InvalidDataStoreKeyException {
		if (containsKey(key)) {
			put(key, value);
		} else {
			throw new InvalidDataStoreKeyException(String.format("A message with trackingID {%s} is not existing in DataStore.",
					key.getTrackingID()));
		}
	}

	/**
	 * the get Messages functionality.
	 * @param value identifier for Message Status
	 * @return the key of an entry
	 */
	public List<eHubMessage> getMessages(MessageStatus value) {
		List<eHubMessage> messages = new ArrayList<eHubMessage>();
		for (Entry<eHubMessage, MessageStatus> entry : this.entrySet()) {
			if (entry.getValue().equals(value)) {
				messages.add(entry.getKey());
			}
		}
		return messages;
	}

	/**
	 * the get Messages functionality.
	 * @param trackingID the Tracking ID attached to the Message
	 * @param status the status of the message
	 * @return the key of an entry
	 */
	public eHubMessage getMessage(Guid trackingID, MessageStatus status) {
		for (Entry<eHubMessage, MessageStatus> entry : this.entrySet()) {
			if (entry.getKey().getTrackingID().equals(trackingID) && entry.getValue().equals(status)) {
				return entry.getKey();
			}
		}
		return null;
	}

	/**
	 * containsKey checks to see if the message contains a key.
	 * @param key - the messages key
	 * @return a Boolean s to whether the key exists within the message.
	 */
	final boolean containsKey(final eHubMessage key) {
		for (Entry<eHubMessage, MessageStatus> entry : this.entrySet()) {
			if (entry.getKey().getTrackingID().equals(key.getTrackingID())) {
				return true;
			}
		}
		return false;
	}
}
