package com.cargowise.eservices.adapter;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;

import junit.framework.TestCase;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class MessageOutboxTests extends TestCase {

	@Test
	public void testAddMessage() {
		IeHubMessage messageMock = Mockito.mock(IeHubMessage.class);

		MessageOutbox outbox = new MessageOutbox();
		assertEquals(0, outbox.count());
		outbox.addMessage(messageMock);
		assertEquals(1, outbox.count());
	}
}
