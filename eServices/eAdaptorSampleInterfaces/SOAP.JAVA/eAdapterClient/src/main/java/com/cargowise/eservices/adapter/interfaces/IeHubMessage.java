package com.cargowise.eservices.adapter.interfaces;

import com.cargowise.eservices.common.Guid;
import com.cargowise.eservices.common.MessageSchemaType;
import com.cargowise.eservices.common.Stream;

/**
 * An interface of a message for sending to/receiving from eService web services.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IeHubMessage {
	/** get a tracking ID of a message.
	 * @return tracking id as GUID */
	Guid getTrackingID();
	/** get a client ID of sender.
	 * @return sender ID */
	String getSenderID();
	/** get a client ID of recipient.
	 * @return recipient ID */
	String getRecipientID();
	/** get a message schema type of a message.
	 * @return message schema type */
	MessageSchemaType getSchemaType();
	/** get an application code of a message.
	 * @return application code */
	String getApplicationCode();
	/** get a schema name of a message.
	 * @return schema name */
	String getSchemaName();
	/** get a stream of a message's content.
	 * @return message stream */
	Stream getMessageStream();
	/** get an email subject of a message.
	 * @return email subject */
	String getEmailSubject();
	/** get a file name of a message.
	 * @return file name */
	String getFileName();
}
