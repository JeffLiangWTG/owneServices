package com.cargowise.eservices.common;

/**
 * Message status of an eHub Gateway Message.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public enum MessageStatus {
	RECEIVED(0), PROCESSING(1), PROCESSED(2), DISTRIBUTED(3), FAILED(255);

	private final int value;

	MessageStatus(final int value) {
		this.value = value;
	}
	/**
	 * A toString that returns the message status.
	 * @return the message status
	 */
	public String toString() {
		return String.valueOf(value);
	}
}
