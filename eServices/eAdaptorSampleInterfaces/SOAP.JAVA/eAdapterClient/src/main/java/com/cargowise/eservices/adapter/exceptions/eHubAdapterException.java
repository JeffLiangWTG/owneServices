package com.cargowise.eservices.adapter.exceptions;

import edu.umd.cs.findbugs.annotations.SuppressFBWarnings;

/**
 * Used to indicate an error condition which occurs inside eService adapter or is collected from web service by an eService adapter.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class eHubAdapterException extends Exception {
	private static final long serialVersionUID = 2769585032665730040L;

	/** Default constructor. */
	@SuppressFBWarnings(value = "NM_CLASS_NAMING_CONVENTION", justification = "keep the same naming convention as C#")
	public eHubAdapterException() {
		super();
	}

	/** Constructor specifying the error message.
	 * @param message error message */
	public eHubAdapterException(final String message) {
		super(message);
	}

	/** Constructor specifying the error message and inner Exception.
	 * @param message error message
	 * @param innerException inner Exception */
	public eHubAdapterException(final String message, final Exception innerException) {
		super(message, innerException);
	}
}
