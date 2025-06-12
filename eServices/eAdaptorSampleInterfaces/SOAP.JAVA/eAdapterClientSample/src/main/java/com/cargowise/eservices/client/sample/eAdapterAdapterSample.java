package com.cargowise.eservices.client.sample;

import com.cargowise.eservices.adapter.eAdapterAdapter;
import com.cargowise.eservices.client.content.eHubMessageUltility;
import com.google.common.annotations.VisibleForTesting;

/**
 * How to use eAdapterAdapter Sample.
 * @author eServices
 *
 */
public final class eAdapterAdapterSample {
	private eAdapterAdapterSample() { }
	public static final String WEB_SERVICE_ADDRESS = "https://{hostAddress}/services/eAdapterStreamedService.svc";
	public static final String USERNAME = "SenderID";
	public static final String PASSWORD = "Password";
	public static final String RECIPIENT = "RecipientID";

	/**
	 * main of eAdaptorAdapterSample.
	 * @param args string array for args
	 * @throws Exception any exception that needs to be thrown
	 */
	public static void main(String[] args) throws Exception {
//		The following code shows how we could forward proxy in Java.
//		System.setProperty("http.proxyHost", "127.0.0.1");
//		System.setProperty("https.proxyHost", "127.0.0.1");
//		System.setProperty("http.proxyPort", "8888");
//		System.setProperty("https.proxyPort", "8888");

		// Create new adapter to connect to the web services
		eAdapterAdapter adapter = getNewEAdapterAdapter();

		// You could test the connection by trying to ping the server
		boolean result = adapter.ping();
		System.out.println("Ping " + WEB_SERVICE_ADDRESS + ": " + result);

		// Add a message to Outbox in order to send. You could add many as you want. They will send all together at the same time.
		adapter.getOutbox().addMessage(eHubMessageUltility.generateMessage(USERNAME, RECIPIENT));

		// Send that message
		int messageCount = adapter.getOutbox().count();
		adapter.sendMessages();
		System.out.println(String.format("Success - Sent %d message(s)", messageCount - adapter.getOutbox().count()));

		// Close connection
		adapter.close();
	}

	@VisibleForTesting
	static eAdapterAdapter getNewEAdapterAdapter() throws Exception {
		return new eAdapterAdapter(WEB_SERVICE_ADDRESS, USERNAME, PASSWORD);
	}
}
