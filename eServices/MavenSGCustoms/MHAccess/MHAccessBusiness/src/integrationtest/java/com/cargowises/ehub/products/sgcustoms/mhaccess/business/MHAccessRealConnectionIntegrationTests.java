package com.cargowises.ehub.products.sgcustoms.mhaccess.business;

import static org.junit.Assert.*;

import java.io.IOException;
import java.io.InputStream;
import java.net.URI;
import java.nio.charset.StandardCharsets;
import java.nio.file.Paths;
import java.util.StringJoiner;

import javax.xml.bind.JAXBException;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.TransformerException;

import org.apache.commons.io.IOUtils;
import org.apache.logging.log4j.Level;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.core.LoggerContext;
import org.apache.logging.log4j.core.config.Configuration;
import org.apache.logging.log4j.core.config.Configurator;
import org.junit.AfterClass;
import org.junit.BeforeClass;
import org.xml.sax.SAXException;

import com.cargowises.ehub.products.sgcustoms.mhaccess.business.helper.MHAccessBuilder;
import com.cargowises.ehub.products.sgcustoms.mhaccess.business.helper.MHAccessException;
import com.cargowises.ehub.products.sgcustoms.mhaccess.business.helper.XMLHelper;
import com.cargowises.ehub.products.sgcustoms.mhaccess.business.schema.MHAccessResponse;

/**
 * This file will use ${basedir}/src/test/resources/com/sns/mhx/props/mx.properties configuration.
 */
public class MHAccessRealConnectionIntegrationTests {
	private String trackingID = "cef535c8-525e-4ca6-8640-729f1b0c3fbf";
	private static Level lastLevel;
	private static LoggerContext ctx = (LoggerContext) LogManager.getContext(false);
	// Get correct Test Username and Password before run this.
	//http://au2sp-shub-401.sand.wtg.zone/eHubPortal/ClientRegistrations > SGCustomsAccount
	private static final String USERNAME = "USERNAME";
	private static final String PASSWORD = "PASSWORD";

	//@org.junit.Test
	@org.junit.Ignore
	public final void testSubmitMessage_Success()
			throws IOException, ParserConfigurationException, TransformerException, SAXException, JAXBException {
		MHAccessBuilder messageBuilder = getMHAccessBuilder();

		String expected = new StringJoiner("\r\n")
				.add("<response xmlns=\"http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09\">")
				.add("  <status>success</status>")
				.add("  <state>submit</state>")
				.add("</response>").add("").toString();
		try (InputStream stream = AbstractMHAccessSessionTests.class.getResourceAsStream("EDIFACT.TXT");
				SubmitMessage mhaccessSession = new SubmitMessage(stream, USERNAME, PASSWORD, trackingID,
						messageBuilder, new MHAccess())) {
			MHAccessResponse result = mhaccessSession.submitRequest();
			assertEquals(expected, XMLHelper.toPrettyXML(result));
		}
	}

	//@org.junit.Test
	@org.junit.Ignore
	public final void testRetrieveMessages_Success()
			throws IOException, ParserConfigurationException, TransformerException, SAXException, JAXBException {
		MHAccessBuilder messageBuilder = getMHAccessBuilder();

		String expected = new StringJoiner("\r\n")
				.add("<response xmlns=\"http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09\">")
				.add("  <status>success</status>")
				.add("  <state>retrieve</state>")
				//<messages>
				//	<message msgID="M201709141636053672556" trackingID="1fbdb946-17ad-4313-807c-92babd38465d">UNA:+.? '
				//		UNB+UNOA:2+PRET1.PRET001:ZZ+VWGT.VWGT001:ZZ+20170906:0812+90020992++AIRERR'...</message>
				//</messages>")
				.add("</response>").add("").toString();

		try (RetrieveMessages mhaccessSession = new RetrieveMessages(USERNAME, PASSWORD, messageBuilder,
				new MHAccess())) {
			MHAccessResponse result = mhaccessSession.submitRequest();
			assertEquals(expected, XMLHelper.toPrettyXML(result));
		}
	}

	/*If this test fails. Please run
	 * testSubmitMessage_Success first
	 */
	//@org.junit.Test
	@org.junit.Ignore
	public final void testCheckMessageSent_Success()
			throws IOException, ParserConfigurationException, TransformerException, SAXException, JAXBException {
		MHAccessBuilder messageBuilder = getMHAccessBuilder();

		String expected = new StringJoiner("\r\n")
				.add("<response xmlns=\"http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09\">")
				.add("  <status>success</status>")
				.add("  <state>check message sent</state>")
				.add("</response>").add("").toString();

		try (CheckMessageSent mhaccessSession = new CheckMessageSent(USERNAME, PASSWORD, trackingID,
				messageBuilder,	new MHAccess())) {
			MHAccessResponse result = mhaccessSession.submitRequest();
			assertEquals(expected, XMLHelper.toPrettyXML(result));
		}
	}

	//@org.junit.Test
	@org.junit.Ignore
	public final void testDeleteMessages_Success() throws IOException, MHAccessException, JAXBException,
			ParserConfigurationException, TransformerException, SAXException {
		final String[] msgIDs = new String[] { "M201709141636053672556T", "M201709141636053672555T",
				"M201709141636053672554T", "M201709141636053672553T", "M201709141636053672552T" };
		MHAccessBuilder messageBuilder = getMHAccessBuilder();

		String expected = new StringJoiner("\r\n")
				.add("<response xmlns=\"http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09\">")
				.add("  <status>success</status>")
				.add("  <state>delete</state>")
				.add("</response>").add("").toString();

		try (DeleteMessages mhaccessSession = new DeleteMessages(USERNAME, PASSWORD, messageBuilder,
				new MHAccess(), msgIDs)) {
			MHAccessResponse result = mhaccessSession.submitRequest();
			assertEquals(expected, XMLHelper.toPrettyXML(result));
		}
	}

	//@org.junit.Test
	@org.junit.Ignore
	public final void testChangePassword_Success() throws IOException, MHAccessException, JAXBException,
			ParserConfigurationException, TransformerException, SAXException {
		MHAccessBuilder messageBuilder = getMHAccessBuilder();

		String expected = new StringJoiner("\r\n")
				.add("<response xmlns=\"http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09\">")
				.add("  <status>fail</status>")
				.add("  <state>change password</state>")
				.add("  <errorCode>1803</errorCode>")
				.add("  <errorMessage>Password Change Failed, reason is "
						+ "[Your new password must not be the same as the old password.]</errorMessage>")
				.add("</response>").add("").toString();

		try (ChangePassword mhaccessSession = new ChangePassword(USERNAME, PASSWORD, PASSWORD, messageBuilder,
				new MHAccess())) {
			MHAccessResponse result = mhaccessSession.submitRequest();
			assertEquals(expected, XMLHelper.toPrettyXML(result));
		}
	}

	private MHAccessBuilder getMHAccessBuilder() throws IOException {
		URI mhxKey = Paths.get(System.getProperty("user.dir"), "..",
				"MHAccessGatewayWebService/src/main/webapp/WEB-INF/MHAccess/key.txt").toUri();
		String key = IOUtils.toString(mhxKey, StandardCharsets.UTF_8);
		System.out.println("EncryptionKey: " + key);

		MHAccessConfiguration config = new MHAccessConfiguration();
		config.setServerIP("uat-oci.accessced.com");
		config.setSecuredURL("https");
		config.setEncryptionKey(key);
		MHAccessBuilder builder = new MHAccessBuilder(config);
		return builder;
	}

	@BeforeClass
	public static void runOnceBeforeClass() throws IOException {
		lastLevel = LogManager.getRootLogger().getLevel();
		System.out.println("Last log level: " + lastLevel);
		final Configuration config = ctx.getConfiguration();
		ctx.getConfiguration().getRootLogger().addAppender(config.getAppender("Console"), Level.TRACE, null);
		Configurator.setRootLevel(Level.TRACE);
		ctx.updateLoggers();
		System.out.println("Current log level: " + LogManager.getRootLogger().getLevel());
	}

	@AfterClass
	public static void runOnceAfterClass() {
		ctx.reconfigure();
		System.out.println("Reset log level: " + LogManager.getRootLogger().getLevel());
		System.out.println("Remaining appender(s): " + String.join(", ", ctx.getConfiguration().getRootLogger().getAppenders().keySet()));
	}
}
