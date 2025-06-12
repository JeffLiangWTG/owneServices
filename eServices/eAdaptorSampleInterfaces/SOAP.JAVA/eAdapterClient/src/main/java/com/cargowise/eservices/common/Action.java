package com.cargowise.eservices.common;

/**
 * An interface for delegating tasks.
 * <br><br>
 * Example:
 * <br><br>
 * The following code shows how to set server certificate validation call back with the anonymous class.
 * <pre>
 * {@code
 * ServicePointManager.setServerCertificateValidationCallback(new Action() {
 *   public void run() throws KeyManagementException, NoSuchAlgorithmException {
 *      ServicePointManager.acceptDifferencesBetweenGivenHostNameAndCertificate();
 *      ServicePointManager.acceptAllTrustingTrustManager();
 *   }
 * });
 * }
 * </pre>
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public interface Action {
	/**
	 * A method for delegating tasks.
	 * <br><br>
	 * Example:
	 * <br><br>
	 * The following code shows how to set server certificate validation call back with the anonymous class.
	 * <pre>
	 * {@code
	 * ServicePointManager.setServerCertificateValidationCallback(new Action() {
	 *   public void run() throws KeyManagementException, NoSuchAlgorithmException {
	 *      ServicePointManager.acceptDifferencesBetweenGivenHostNameAndCertificate();
	 *      ServicePointManager.acceptAllTrustingTrustManager();
	 *   }
	 * });
	 * }
	 * </pre>
	 * @throws Exception errors while executing the action.
	 */
	void run() throws Exception;
}
