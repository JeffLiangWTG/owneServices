package com.cargowise.eservices.adapter;

import java.io.IOException;
import java.security.KeyManagementException;
import java.security.NoSuchAlgorithmException;
import java.util.ArrayList;
import java.util.List;

import javax.xml.soap.SOAPException;

import com.cargowise.eservices.adapter.interfaces.IAdapter;
import com.cargowise.eservices.adapter.interfaces.IMessageOutbox;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.Action;
import com.cargowise.eservices.common.SendStreamRequest;
import com.cargowise.eservices.common.ServicePointManager;
import com.google.common.annotations.VisibleForTesting;
import com.cargowise.eservices.common.eHubGatewayMessage;

/**
 * An adapter connecting to an eService web service. This adapter has the default server certificate validation
 * set to accept all trusting trust manager and accept all differences between given host name and certificate.
 * <br><br>
 * Example:
 * <br><br>
 * The following code shows how to set server certificate validation call back when using with eServer Adapter
 * <pre>
 * {@code
 * ServicePointManager.setServerCertificateValidationCallback(new Action() {
 *   public void run() throws KeyManagementException, NoSuchAlgorithmException {
 *      ServicePointManager.acceptDifferencesBetweenGivenHostNameAndCertificate();
 *      ServicePointManager.acceptAllTrustingTrustManager();
 *   }
 * });
 * Adapter adapter = new eXXXAdapter("webserviceurl.com", "UserID", "Password");
 * if(adapter.ping()) {
 *   adapter.getOutbox().addMessage(new eHubMessage(guid, sender, recipient,
 *          MessageSchemaType.Xml, "UDM", "http://www.edi.com.au/EnterpriseService/#XmlInterchange",
 *          buildMessage(), "Email Subject Test", "File Name Test"));
 *   adapter.sendMessages();
 * }
 * }
 * </pre>
 * 
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 * @see ServicePointManager
 */
public abstract class Adapter implements IAdapter {
	private boolean isClosed = false;
	Adapter() throws Exception {
		if (ServicePointManager.getServerCertificateValidationCallback() == null) {
			ServicePointManager.setServerCertificateValidationCallback(trustAllCertificatesCallback());
		}
		ServicePointManager.getServerCertificateValidationCallback().run();
		//here-------------------
	}

	static final Action trustAllCertificatesCallback() {
		return new Action() {
			public void run() throws KeyManagementException, NoSuchAlgorithmException {
				ServicePointManager.acceptDifferencesBetweenGivenHostNameAndCertificate();
				ServicePointManager.acceptAllTrustingTrustManager();
			}
		};
	}

	/**
	 * Get outgoing message mailbox.
	 * @return messageOutBox which is an outbox of messages, antonymous to an inbox.
	 */
	public IMessageOutbox getOutbox() {
		if (messageOutbox == null) {
			messageOutbox = createOutbox();
		}
		return messageOutbox;
	}

	private IMessageOutbox messageOutbox;

	/**
	 * Creates a new outbox.
	 * @return the new outbox
	 */
	@VisibleForTesting
	IMessageOutbox createOutbox() {
		return new MessageOutbox();
	}

	/**
	 * Send messages from the outgoing message mailbox to the web service.
	 * @throws Exception error when sending a message
	 * @see com.cargowise.eservices.common.request.SendMessageStreamRequest
	 */
	public void sendMessages() throws Exception {
		ServiceExceptionHandler.runEHubAction(new Action() {
			public void run() throws Exception {
				List<eHubGatewayMessage> messages = new ArrayList<eHubGatewayMessage>();
				for (IeHubMessage outboxMessage : getOutbox()) {
					eHubGatewayMessage gatewayMessage = new eHubGatewayMessage();
					gatewayMessage.setMessageTrackingID(outboxMessage.getTrackingID());
					gatewayMessage.setClientID(outboxMessage.getRecipientID());
					gatewayMessage.setSchemaName(outboxMessage.getSchemaName());
					gatewayMessage.setSchemaType(outboxMessage.getSchemaType());
					gatewayMessage.setApplicationCode(outboxMessage.getApplicationCode());
					gatewayMessage.setEmailSubject(outboxMessage.getEmailSubject());
					gatewayMessage.setFileName(outboxMessage.getFileName());
					gatewayMessage.setMessageStream(outboxMessage.getMessageStream().compressAndEncode());

					messages.add(gatewayMessage);
				}
				SendStreamRequest sendRequest = new SendStreamRequest(
						messages.toArray(new eHubGatewayMessage[messages.size()]));
				sendMessagesCore(sendRequest);

				getOutbox().clear();
			}
		});
	}
	/**
	 * Core message sending function.
	 * @param sendRequest is a SendStreamRequest variable used for sending messages.
	 * @throws Exception Error when sending a message
	 */
	protected abstract void sendMessagesCore(SendStreamRequest sendRequest) throws Exception;
	/**
	 * Ping the web service.
	 * @return the status of ping request(true/false)
	 * @throws Exception error during trying to ping the server
	 * @see com.cargowise.eservices.common.request.PingRequest
	 */
	public abstract boolean ping() throws Exception;

	/**
	 * Close the current connection session between this Adapter and the web service.
	 * @throws SOAPException if receiving error when calling a SOAP web service.
	 * @throws IOException if receiving error from {@link com.cargowise.eservices.common.Stream}
	 */
	public void close() throws SOAPException, IOException {
		if (messageOutbox != null) {
			messageOutbox.clear();
		}
		isClosed = true;
	}

	/**
	 * Close the current connection session between this Adapter and the web service.
	 * @see #close()
	 */
	@Override
	protected void finalize() throws SOAPException, IOException {
		if (!isClosed) {
			close();
		}
	}
}
