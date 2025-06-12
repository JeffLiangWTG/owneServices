package com.cargowise.eservices.client.sample;

import java.util.ArrayList;
import java.util.List;

import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.adapter.eAdapterAdapter;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.client.content.MessageOutboxMock;
import com.cargowise.eservices.client.content.TestCaseBase;
import com.cargowise.eservices.common.Action;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.ServicePointManager;

@RunWith(PowerMockRunner.class)
@PrepareForTest({ Guid.class, eAdapterAdapterSample.class})
public class eAdapterAdapterSampleTests extends TestCaseBase {
	public final void testSendMessageWithStatusSent() throws Exception {
		Guid guid1 = new Guid("340364b0-0e0f-4506-88d9-fa0a5c789d96");
		PowerMockito.when(Guid.newGuid()).thenReturn(guid1);

		final List<IeHubMessage> receivedMessageInServer = new ArrayList<IeHubMessage>();
		eAdapterAdapter mockAdapter = Mockito.mock(eAdapterAdapter.class);
		final MessageOutboxMock outbox = new MessageOutboxMock();
		Mockito.stub(mockAdapter.getOutbox()).toReturn(outbox);

		Mockito.stub(mockAdapter.ping()).toReturn(true);

		Mockito.doAnswer((invocation) -> {
			for (IeHubMessage mMessage : outbox) {
				receivedMessageInServer.add(mMessage);
			}
			outbox.clear();
			return null;
		}).when(mockAdapter).sendMessages();

		PowerMockito.when(eAdapterAdapterSample.getNewEAdapterAdapter()).thenReturn(mockAdapter);

		eAdapterAdapterSample.main(new String[] {});

		StringBuilder expect = new StringBuilder();
		expect.append("Ping " + eAdapterAdapterSample.WEB_SERVICE_ADDRESS + ": true\r\n");
		expect.append("Success - Sent 1 message(s)\r\n");

		assertEquals(expect.toString(), output.toString());
		assertEquals(1, receivedMessageInServer.size());
		assertEquals(guid1, receivedMessageInServer.get(0).getTrackingID());
	}

	@Override
	protected final void setUp() throws Exception {
		super.setUp();
		PowerMockito.spy(eAdapterAdapterSample.class);
		PowerMockito.spy(Guid.class);
		ServicePointManager.setServerCertificateValidationCallback(new Action() {
			public void run() throws Exception {
			}
		});
	}
}
