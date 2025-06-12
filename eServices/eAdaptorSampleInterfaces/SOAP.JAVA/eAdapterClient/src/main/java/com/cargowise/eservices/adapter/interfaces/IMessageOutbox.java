package com.cargowise.eservices.adapter.interfaces;

/**
 * An interface for outcoming message mailbox.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IMessageOutbox extends IMessageBox {
	/** add a message into the mailbox.
	 * @param message an {@link IeHubMessage} */
	void addMessage(IeHubMessage message);
}
