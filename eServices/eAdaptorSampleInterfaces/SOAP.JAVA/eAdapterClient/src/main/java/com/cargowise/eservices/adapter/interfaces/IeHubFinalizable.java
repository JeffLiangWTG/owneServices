package com.cargowise.eservices.adapter.interfaces;

import com.cargowise.eservices.common.Guid;

/**
 * An Adapter interface for eHub gateway web service, which includes incoming and outgoing mailbox.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface IeHubFinalizable {
	/**
	 * allow the adapter to finalise a message after processing.
	 * @param batchID the batch ID matching batch ID of the SOAP message from eService web service.
	 * @throws Exception errors when finalising messages
	 * */
	void finalise(Guid batchID) throws Exception;
}
