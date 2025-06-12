package com.cargowise.eservices.adapter;

import com.cargowise.eservices.adapter.exceptions.eHubAdapterException;
import com.cargowise.eservices.common.Action;
import com.cargowise.eservices.common.FaultException;
/**
 * 
 * @author Karl.Silis
 *
 */
final class ServiceExceptionHandler { //NOPMD
	private ServiceExceptionHandler() { }
	public static void runEHubAction(final Action eHubMethod) throws Exception {
		try {
			eHubMethod.run();
		} catch (FaultException ex) {
			throw new eHubAdapterException(ex.getMessage(), ex);
		} catch (Exception e) {
			throw new eHubAdapterException(e.getMessage(), e);
		}
	}
}
