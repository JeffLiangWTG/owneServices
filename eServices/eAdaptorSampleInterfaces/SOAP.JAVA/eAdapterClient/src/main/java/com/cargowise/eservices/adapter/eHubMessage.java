package com.cargowise.eservices.adapter;

import java.io.IOException;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.adapter.interfaces.IeHubMessage;
import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.Stream;

import edu.umd.cs.findbugs.annotations.SuppressFBWarnings;

/**
 * A message for sending to/receiving from eService web services.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
@SuppressFBWarnings(value = "NM_CLASS_NAMING_CONVENTION", justification = "keep the same naming convention as C#")
public class eHubMessage implements IeHubMessage {

	private Guid trackingID;
	private String senderID;
	private String recipientID;
	private MessageSchemaType schemaType;
	private String applicationCode;
	private String schemaName;
	private Stream messageStream;
	private String emailSubject;
	private String filename;

	/**
	 * A constructor specifying message data.
	 * @param trackingID the tracking ID of a message(example '340364b0-0e0f-4506-88d9-fa0a5c789d96').
	 * @param senderID client ID of the sender.
	 * @param recipientID client ID of the recipient.
	 * @param schemaType message schemaType such as {@link MessageSchemaType#Xml} and {@link MessageSchemaType#FlatFile}.
	 * @param applicationCode 3 characters of application code, leave blank if unknown.
	 * @param schemaName schema name of the file.
	 * @param messageStream the stream of message content.
	 * @throws eHubAdapterException if the error from eService Web Service.
	 */
	public eHubMessage(final String trackingID, final String senderID, final String recipientID, final MessageSchemaType schemaType,
			final String applicationCode, final String schemaName, final Stream messageStream) throws eHubAdapterException {
		this(new Guid(trackingID), senderID, recipientID, schemaType, applicationCode, schemaName, messageStream, "",
				"");
	}

	//try https://stackoverflow.com/questions/4023185/how-to-disable-a-particular-checkstyle-rule-for-a-particular-line-of-code

	/**
	 * A constructor specifying message data.
	 * @param trackingID the tracking ID of a message(example '340364b0-0e0f-4506-88d9-fa0a5c789d96').
	 * @param senderID client ID of the sender.
	 * @param recipientID client ID of the recipient.
	 * @param schemaType message schemaType such as {@link MessageSchemaType#Xml} and {@link MessageSchemaType#FlatFile}.
	 * @param applicationCode 3 characters of application code, leave blank if unknown.
	 * @param schemaName schema name of the file.
	 * @param messageStream the stream of message content.
	 * @param emailSubject the email subject of the message.
	 * @param filename the file name of the message.
	 * @throws eHubAdapterException if the error from eService Web Service.
	 */
	public eHubMessage(final Guid trackingID, final String senderID, final String recipientID, final MessageSchemaType schemaType,
			final String applicationCode, final String schemaName, final Stream messageStream,
			final String emailSubject, final String filename)
					throws eHubAdapterException {
		validateStream(messageStream);

		this.trackingID = trackingID;
		this.senderID = senderID;
		this.recipientID = recipientID;
		this.schemaType = schemaType;
		this.applicationCode = applicationCode;
		this.schemaName = schemaName;
		this.messageStream = messageStream;
		this.emailSubject = emailSubject;
		this.filename = filename;
	}

	/**
	 * @return the trackingID
	 */
	public Guid getTrackingID() {
		return trackingID;
	}

	/**
	 * @param trackingID the trackingID to set
	 */
	public void setTrackingID(final Guid trackingID) {
		this.trackingID = trackingID;
	}

	/**
	 * @return the senderID
	 */
	public String getSenderID() {
		return senderID;
	}

	/**
	 * @param senderID the senderID to set
	 */
	public void setSenderID(final String senderID) {
		this.senderID = senderID;
	}

	/**
	 * @return the recipientID
	 */
	public String getRecipientID() {
		return recipientID;
	}

	/**
	 * @param recipientID the recipientID to set
	 */
	public void setRecipientID(final String recipientID) {
		this.recipientID = recipientID;
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
	 * @return the filename
	 */
	public String getFileName() {
		return filename;
	}

	/**
	 * @param fileName the filename to set
	 */
	public void setFileName(final String fileName) {
		this.filename = fileName;
	}

	/**
	 * Close Stream of the message content and finalize it.
	 */
	@Override
	protected void finalize() throws IOException {
		if (this.messageStream != null) {
			this.messageStream.close();
		}
	}

	static void validateStream(final Stream messageStream) throws eHubAdapterException {
		try {
			if (messageStream != null && messageStream.getPosition() != 0) {
				messageStream.reset();
			}
		} catch (Exception ex) {
			throw new eHubAdapterException("Unable to set message stream position to 0.", ex);
		}
	}

	/**
	 * @return the content in following format trackingID|senderID|recipientID|schemaName|schemaType|applicationCode|emailSubject|fileName
	 */
	@Override
	public String toString() {
		return String.join("|", trackingID.toString(), senderID, recipientID, schemaName, schemaType.name(),
				applicationCode, emailSubject, filename);
	}
}
