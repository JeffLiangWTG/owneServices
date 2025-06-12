package com.cargowise.eservices.adapter.interfaces;

import java.io.IOException;

/**
 * An interface for message mailbox.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IMessageBox extends Iterable<IeHubMessage> {
	/** get size of messages in kilobytes.
	 * @return size in kilobytes
	 * @throws IOException stream's problems */
	long getSizeInKiloBytes() throws IOException;
	/** get the total number of messages in the mailbox.
	 * @return number of messages in mailbox */
	int count();
	/** clear all messages in the mailbox. */
	void clear();
}
