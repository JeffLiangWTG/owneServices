package com.cargowise.eservices.adapter;

import static org.junit.Assert.*;

import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.eAdapterStreamedServiceClient;
import com.cargowise.eservices.common.Stream;
import com.cargowise.eservices.common.request.SendMessageStreamRequest;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ eAdapterAdapter.class, SendMessageStreamRequest.class, eAdapterStreamedServiceClient.class})

public class MessageBoxTests {
	@Test
	public void testSizeInKiloBytes() throws IOException, IllegalArgumentException, IllegalAccessException {
		try (InputStream messageStream1 = MessageBoxTests.class.getResourceAsStream("PSAXmlInputSmall.xml")) {
		InputStream messageStream2 = MessageBoxTests.class.getResourceAsStream("PSAXmlInputSmall.xml");
		IeHubMessage messageMock1 = Mockito.mock(IeHubMessage.class);
		Mockito.stub(messageMock1.getMessageStream()).toReturn(new Stream(messageStream1));
		IeHubMessage messageMock2 = Mockito.mock(IeHubMessage.class);
		Mockito.stub(messageMock2.getMessageStream()).toReturn(new Stream(messageStream2));

		MessageBox messageBoxMock = PowerMockito.mock(MessageBox.class, Mockito.CALLS_REAL_METHODS);
		ArrayList<IeHubMessage> messageList = new ArrayList<IeHubMessage>();
		PowerMockito.field(MessageBox.class, "messageList").set(messageBoxMock, messageList);
		assertNotNull(messageBoxMock.getMessageList());
		messageBoxMock.getMessageList().add(messageMock1);
		messageBoxMock.getMessageList().add(messageMock2);

		assertEquals(11, messageBoxMock.getSizeInKiloBytes());
		}
	}

	@Test
	public void testCount() throws IllegalArgumentException, IllegalAccessException {
		MessageBox messageBoxMock = Mockito.mock(MessageBox.class, Mockito.CALLS_REAL_METHODS);
		ArrayList<IeHubMessage> messageList = new ArrayList<IeHubMessage>();
		PowerMockito.field(MessageBox.class, "messageList").set(messageBoxMock, messageList);
		messageBoxMock.getMessageList().add(Mockito.mock(IeHubMessage.class));
		messageBoxMock.getMessageList().add(Mockito.mock(IeHubMessage.class));

		assertEquals(2, messageBoxMock.count());
	}

	@Test
	public void testClear() throws IllegalArgumentException, IllegalAccessException {
		MessageBox messageBoxMock = Mockito.mock(MessageBox.class, Mockito.CALLS_REAL_METHODS);
		ArrayList<IeHubMessage> messageList = new ArrayList<IeHubMessage>();
		PowerMockito.field(MessageBox.class, "messageList").set(messageBoxMock, messageList);
		messageBoxMock.getMessageList().add(Mockito.mock(IeHubMessage.class));
		messageBoxMock.getMessageList().add(Mockito.mock(IeHubMessage.class));

		messageBoxMock.clear();
		assertEquals(0, messageBoxMock.count());
	}
}
