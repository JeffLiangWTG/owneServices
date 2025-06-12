package com.cargowise.eservices.client.content;
/**
 * Exception for Invalid Data store key's.
 * @author karl.Silis
 *
 */
public class InvalidDataStoreKeyException extends Exception {
	private static final long serialVersionUID = -2062808753354255572L;

	/**
	 * Exception for Invalid Data store key's.
	 * @param message this is the message
	 */
	public InvalidDataStoreKeyException(String message) {
		super(message);
	}
}
