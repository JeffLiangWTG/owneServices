package com.cargowise.eservices.common;

import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.TimeZone;

/**
 * Enables the user to configure client and service credentials as well as service
 * credential authentication settings for use on the client side of communication.
* <br><br>
 * Example:
 * <br><br>
 * <pre>
 * {@code
 * eHubStreamedServiceClient client = new eHubStreamedServiceClient("webserviceaddress.com");
 * client.getClientCredentials().setUsername("ClientID");
 * client.getClientCredentials().setPassword("Password");
 * client.ping(); // try to ping the web service with configured credentials.
 * }
 * </pre>
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 * @see <a href="https://msdn.microsoft.com/en-us/library/system.servicemodel.description.clientcredentials%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396">ClientCredentials C#</a>
 */
public class ClientCredentials {
	private String username;
	private String password;
	private final SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");

	/**
	 * A constructor specifying username and password of the authentication.
	 * @param username user name
	 * @param password password
	 */
	public ClientCredentials(final String username, final String password) {
		sdf.setTimeZone(TimeZone.getTimeZone("UTC"));
		this.username = username;
		this.password = password;
	}

	/**
	 * 
	 */
	public ClientCredentials() {
		this("", "");
	}

	/**
	 * @return the username
	 */
	public String getUsername() {
		return username;
	}

	/**
	 * @param username the username to set
	 */
	public void setUsername(final String username) {
		this.username = username;
	}

	/**
	 * @return the password
	 */
	public String getPassword() {
		return password;
	}

	/**
	 * @param password the password to set
	 */
	public void setPassword(final String password) {
		this.password = password;
	}

	/**
	 * Get current date to determine the created date and expired date inside a SOAP message.
	 * @return the characters of current local time in UTC.
	 */
	public String getCurrentDate() {
		currentDate = Calendar.getInstance();
		currentDateExpired = false;
		return sdf.format(currentDate.getTime());
	}

	private Calendar currentDate = Calendar.getInstance();
	private boolean currentDateExpired = false;

	/**
	 * Get Expiry Date after calling CurrentDate which is 30 minute after
	 * CurrentDate.
	 * 
	 * @return Expiry Date
	 */
	public String getExpiryDate() {
		final int halfMinute = 30;
		if (!currentDateExpired) {
			currentDate.add(Calendar.MINUTE, halfMinute);
			currentDateExpired = true;
		}
		return sdf.format(currentDate.getTime());
	}
}
