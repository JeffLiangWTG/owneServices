package com.cargowise.eservices.adapter.interfaces;

import java.io.IOException;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.eHubGatewayMessage;

/**
 * An interface for incoming message mailbox.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IMessageInbox extends IMessageBox, IInboxInternal {
	/** mark all message as processed so the inbox could be reuseable.
	 * @throws Exception error while finalise messages */
	void markAsRead() throws Exception;
	/** read message batch from web service and process those messages.
	 * @param guid batch id of SOAP message
	 * @param messages list of {@link eHubGatewayMessage}
	 * @throws IOException  for EHubAdapterException
	 * @throws eHubAdapterException throws the e-Hub
	 * */
	void readMessageBatch(Guid guid, eHubGatewayMessage[] messages) throws eHubAdapterException, IOException;
}
