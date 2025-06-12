package com.cargowise.eservices.common;

import java.security.KeyManagementException;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSession;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

/**
 * Provides connection management for HTTP connections.
 * <br><br>
 * Example:
 * <br><br>
 * The following code shows how to set server certificate validation call back when using with eServer Adapter
 * <pre>
 * {@code
 * ServicePointManager.setServerCertificateValidationCallback(new Action() {
 *   public void run() throws KeyManagementException, NoSuchAlgorithmException {
 *      ServicePointManager.acceptDifferencesBetweenGivenHostNameAndCertificate();
 *      ServicePointManager.acceptAllTrustingTrustManager();
 *   }
 * });
 * Adapter adapter = new eXXXAdapter("webserviceurl.com", "UserID", "Password");
 * }
 * </pre>
 * 
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 * @see <a href="https://msdn.microsoft.com/en-us/library/system.net.servicepointmanager(v=vs.110).aspx">ServicePointManager C#</a>
 */
public final class ServicePointManager { //NOPMD
	private ServicePointManager() { }
	private static Action serverCertificateValidationCallback;

	/** Get the callback to validate a server certificate.
	 * @return the instance of the callback */
	public static Action getServerCertificateValidationCallback() {
		return serverCertificateValidationCallback;
	}

	/** Set the callback to validate a server certificate.
	 * @param serverCertificateValidationCallback the callback to validate a server certificate */
	public static void setServerCertificateValidationCallback(final Action serverCertificateValidationCallback) {
		ServicePointManager.serverCertificateValidationCallback = serverCertificateValidationCallback;
	}

	/**
	 * Configuration for accepting all the differences between the given host name and the certificates.
	 */
	public static void acceptDifferencesBetweenGivenHostNameAndCertificate() {
		HostnameVerifier hv = new HostnameVerifier() {
			public boolean verify(final String hostname, final SSLSession session) {
				return true;
			}
		};
		HttpsURLConnection.setDefaultHostnameVerifier(hv);
	}

	/**
	 * Configuration for accepting all trusting trust manager.
	 * @throws KeyManagementException Certificate Trusting issues.
	 * @throws NoSuchAlgorithmException thrown when a particular cryptographic algorithm is requested but is not available in the environment.
	 */
	public static void acceptAllTrustingTrustManager() throws NoSuchAlgorithmException, KeyManagementException {
		// Create a trust manager that does not validate certificate chains
		TrustManager[] trustAllCerts = new TrustManager[] { new X509TrustManager() {
			public java.security.cert.X509Certificate[] getAcceptedIssuers() {
				java.security.cert.X509Certificate[] array = null;
				return array;
			}

			public void checkClientTrusted(final java.security.cert.X509Certificate[] certs, final String authType) {
			}

			public void checkServerTrusted(final java.security.cert.X509Certificate[] certs, final String authType) {
			}
		} };

		SSLContext sc = SSLContext.getInstance("TLS"); //"SSL";
		sc.init(null, trustAllCerts, new SecureRandom());
		SSLContext.setDefault(sc);
	}
}
