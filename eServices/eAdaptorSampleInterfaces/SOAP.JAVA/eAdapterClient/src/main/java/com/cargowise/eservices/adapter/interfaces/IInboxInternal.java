package com.cargowise.eservices.adapter.interfaces;

import java.io.IOException;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.eHubGatewayMessage;

/**
 * The interface of incoming message mailbox.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IInboxInternal {
	/** incoming message mailbox is able to tell whether a adapter could retrieve messages.
	 * @return permission to retrieve messages */
	boolean isRetrievableMessages();
	/** read message batch from a webservices and decide how to handle these messages.
	 * @param batchID batch ID of SOAP message from eService Web Service.
	 * @param messages list of eHubGatewayMessage
	 * @throws eHubAdapterException {@link com.cargowise.eservices.adapter.eHubMessage} throws eHubAdapterException
	 * @throws IOException {@link com.cargowise.eservices.common.Stream} problems */
	void readMessageBatch(Guid batchID, eHubGatewayMessage[] messages) throws eHubAdapterException, IOException;
}
