package com.cargowise.eservices.client.content;

import java.util.Map.Entry;

import org.junit.Test;

import com.cargowise.eservices.adapter.eHubMessage;
import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.common.Action;

public class DataStoreTests extends TestCaseBase {
	@Test
	public void testAddEHubMessage() throws eHubAdapterException, InvalidDataStoreKeyException {
		final DataStore dataStore = new DataStore();
		final eHubMessage message = eHubMessageUltility.generateMessage("Sender", "Recipient");
		dataStore.add(message, MessageStatus.Queued);
		assertEquals(1, dataStore.size());

		assertException(new Action() {
			public void run() throws Exception {
				dataStore.add(message, MessageStatus.Sent);
			}
		}, InvalidDataStoreKeyException.class, String.format("A message with trackingID {%s} is already in the pending list. Please assign each message all having different trackingid.",
				message.getTrackingID()));
	}

	@Test
	public void testUpdateEHubMessage() throws eHubAdapterException, InvalidDataStoreKeyException {
		final DataStore dataStore = new DataStore();
		final eHubMessage message = eHubMessageUltility.generateMessage("Sender", "Recipient");

		assertException(new Action() {
			public void run() throws Exception {
				dataStore.update(message, MessageStatus.Sent);
			}
		}, InvalidDataStoreKeyException.class, String.format("A message with trackingID {%s} is not existing in DataStore.",
				message.getTrackingID()));

		dataStore.add(message, MessageStatus.Queued);
		dataStore.update(message, MessageStatus.Sent);

		assertEquals(1, dataStore.size());

		Entry<eHubMessage, MessageStatus> dataStoreEHubMessage = dataStore.entrySet().iterator().next();
		assertEquals(message.getTrackingID(), dataStoreEHubMessage.getKey().getTrackingID());
		assertEquals(MessageStatus.Sent, dataStoreEHubMessage.getValue());
	}

	@Test
	public void testGetMessage() throws eHubAdapterException, InvalidDataStoreKeyException {
		final DataStore dataStore = new DataStore();
		final eHubMessage message = eHubMessageUltility.generateMessage("Sender", "Recipient");

		dataStore.add(message, MessageStatus.Queued);
		eHubMessage dataStoreMessage = dataStore.getMessage(message.getTrackingID(), MessageStatus.Pending);
		assertNull(dataStoreMessage);

		dataStore.update(message, MessageStatus.Pending);
		dataStoreMessage = dataStore.getMessage(message.getTrackingID(), MessageStatus.Pending);
		assertNotNull(dataStoreMessage);
	}
}
