package com.cargowise.eservices.common;

import java.util.Optional;

import edu.umd.cs.findbugs.annotations.SuppressFBWarnings;

/**
 * A message container matches the details of eHub gateway SOAP message.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
@SuppressFBWarnings(value = "NM_CLASS_NAMING_CONVENTION", justification = "keep the same naming convention as C#")
public class eHubGatewayMessage {
	private Guid messageTrackingID;
	private String clientID;
	private String schemaName;
	private MessageSchemaType schemaType;
	private String applicationCode;
	private String emailSubject;
	private String fileName;
	private Stream messageStream;


	/**
	 * @return the messageTrackingID
	 */
	public Guid getMessageTrackingID() {
		return messageTrackingID;
	}

	/**
	 * @param messageTrackingID the messageTrackingID to set
	 */
	public void setMessageTrackingID(final Guid messageTrackingID) {
		this.messageTrackingID = messageTrackingID;
	}

	/**
	 * @return the clientID
	 */
	public String getClientID() {
		return clientID;
	}

	/**
	 * @param clientID the clientID to set
	 */
	public void setClientID(final String clientID) {
		this.clientID = clientID;
	}

	/**
	 * @return the schemaName
	 */
	public String getSchemaName() {
		return schemaName;
	}

	/**
	 * @param schemaName the schemaName to set
	 */
	public void setSchemaName(final String schemaName) {
		this.schemaName = schemaName;
	}

	/**
	 * @return the schemaType
	 */
	public MessageSchemaType getSchemaType() {
		return schemaType;
	}

	/**
	 * @param schemaType the schemaType to set
	 */
	public void setSchemaType(final MessageSchemaType schemaType) {
		this.schemaType = schemaType;
	}

	/**
	 * @return the applicationCode
	 */
	public String getApplicationCode() {
		return applicationCode;
	}

	/**
	 * @param applicationCode the applicationCode to set
	 */
	public void setApplicationCode(final String applicationCode) {
		this.applicationCode = applicationCode;
	}

	/**
	 * @return the emailSubject
	 */
	public String getEmailSubject() {
		return emailSubject;
	}

	/**
	 * @param emailSubject the emailSubject to set
	 */
	public void setEmailSubject(final String emailSubject) {
		this.emailSubject = emailSubject;
	}

	/**
	 * @return the fileName
	 */
	public String getFileName() {
		return fileName;
	}

	/**
	 * @param fileName the fileName to set
	 */
	public void setFileName(final String fileName) {
		this.fileName = fileName;
	}

	/**
	 * @return the messageStream
	 */
	public Stream getMessageStream() {
		return messageStream;
	}

	/**
	 * @param messageStream the messageStream to set
	 */
	public void setMessageStream(final Stream messageStream) {
		this.messageStream = messageStream;
	}

	/**
	 * eHubGatewayMessage toString.
	 * @return the String for use that combines all of the variables in this class that are relevant to use.
	 */
	public final String toString() {
		return String.join("|", Optional.ofNullable(messageTrackingID).map(x -> x.toString()).orElse(null), clientID, schemaName,
				Optional.ofNullable(schemaType).map(x -> x.name()).orElse(null),
				applicationCode,
				emailSubject, fileName);
	}
}
