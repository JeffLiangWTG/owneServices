package com.cargowise.eservices.canadian.encryption.service;

/**
 * String extensions for truncate log message.
 * 
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.8
 */
public final class StringExtensions {
	private static final int TRUNCATE_MAX_LENGTH = 300;

	private StringExtensions() { }

	/**
	 * Truncate message for logging.
	 * @param message logging message
	 * @return truncated message
	 */
	public static String truncateForLogging(final String message) {
		if (message == null || message.trim().isEmpty()) {
			return "";
		}
		final String loggingMessage = message.replaceAll("\\s+", " ").trim();
		return loggingMessage.length() > TRUNCATE_MAX_LENGTH ? loggingMessage.substring(0, TRUNCATE_MAX_LENGTH) : loggingMessage;
	}
}
