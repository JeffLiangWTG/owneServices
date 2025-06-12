package com.cargowise.eservices.canadian.encryption.helper;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;

import org.apache.log4j.EnhancedPatternLayout;
import org.apache.log4j.Layout;
import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.apache.log4j.WriterAppender;
import org.junit.AfterClass;
import org.junit.BeforeClass;

import com.cargowise.eservices.canadian.encryption.TestCIGDecodeEncodeHelper;

public abstract class TestCaseWithEncodeDecode {
	@BeforeClass
	public static void runOnceBeforeClass() throws Exception {
		ignoreerrors = System.getProperty("mail.mime.base64.ignoreerrors");
		if (ignoreerrors == null || !ignoreerrors.equals("true")) {
			System.setProperty("mail.mime.base64.ignoreerrors", "true");
		}
		out = new ByteArrayOutputStream();
		Layout layout = new EnhancedPatternLayout("[%-5p] %c{1}:%L - %m%n %throwable{6}");
		rootLogger.setLevel(Level.OFF);
		rootLogger.addAppender(new WriterAppender(layout, out));
		LOGGER.setLevel(Level.DEBUG);
		LOGGER.addAppender(new WriterAppender(layout, out));
	}

	@AfterClass
	public static void runOnceAfterClass() {
		if (ignoreerrors == null) {
			System.clearProperty("mail.mime.base64.ignoreerrors");
		} else if (!ignoreerrors.equals("true")) {
			System.setProperty("mail.mime.base64.ignoreerrors", ignoreerrors);
		}
		LOGGER.removeAllAppenders();
	}

	protected final void assertExpectedException(final ExceptionThrower exceptionThrower,
			final Class<? extends Exception> cls, final String exceptionMessage) {
		assertExpectedException(exceptionThrower, cls, exceptionMessage, null);
	}

	protected final void assertExpectedException(final String assertMessage, final ExceptionThrower exceptionThrower,
			final Class<? extends Exception> cls, final String exceptionMessage) {
		assertExpectedException(assertMessage, exceptionThrower, cls, exceptionMessage, null);
	}

	protected final void assertExpectedException(final ExceptionThrower exceptionThrower,
			final Class<? extends Exception> cls, final String exceptionMessage, final String innerExceptionMessage) {
		assertExpectedException(null, exceptionThrower, cls, exceptionMessage, innerExceptionMessage);
	}

	protected final void assertExpectedException(final String assertMessage, final ExceptionThrower exceptionThrower,
			final Class<? extends Exception> cls, final String exceptionMessage, final String innerExceptionMessage) {
		try {
			exceptionThrower.throwException();
			fail("Should fail exception");
		} catch (Exception ex) {
			if (cls.isInstance(ex)) {
				assertEquals(exceptionMessage, ex.getMessage());
				if (innerExceptionMessage != null) {
					Throwable innerException = ex.getCause();
					if (innerException == null) {
						fail("Expected [" + innerExceptionMessage + "] but no inner exception occurs");
					}
					assertEquals(innerExceptionMessage,
							ex.getCause().getClass().getName() + ": " + ex.getCause().getMessage());
				}
			} else {
				StringWriter writer = new StringWriter();
				PrintWriter printWriter = new PrintWriter(writer);
				if (assertMessage != null) {
					printWriter.write(assertMessage + "\r\n");
				}
				printWriter.write(
						String.format("Unexpected Exception occurs: expected [%1$s] but actual [%2$s].\r%n%3$s\r%n",
								cls.getSimpleName(), ex.getClass().getSimpleName(), ex.getMessage()));
				ex.printStackTrace(printWriter);
				printWriter.flush();
				fail(writer.toString());
			}
		}
	}

	private static ClassLoader classLoader = ClassLoader.getSystemClassLoader();

	public static InputStream getRecipientPublicKey() {
		return classLoader.getResourceAsStream("cert/CBSAPublic.cer");
	}

	public static InputStream getSenderPrivateKey() {
		return classLoader.getResourceAsStream("cert/CACertTest.epf");
	}

	public static InputStream getSenderPublicKey() {
		return classLoader.getResourceAsStream("cert/CACertTest.cer");
	}

	public static final String PASSWORD = "CargoWise2010";

	private TestCIGDecodeEncodeHelper cigDecodeEncodeHelper;

	protected final TestCIGDecodeEncodeHelper getCIGDecodeEncodeHelper() throws Exception {
		if (cigDecodeEncodeHelper == null) {
			cigDecodeEncodeHelper = new TestCIGDecodeEncodeHelper(getSenderPublicKey(), getSenderPrivateKey(), PASSWORD,
					LOGGER);
		}
		return cigDecodeEncodeHelper;
	}

	protected final String getLogContent() throws UnsupportedEncodingException {
		return out.toString("UTF-8").trim();
	}

	private static ByteArrayOutputStream out = null;
	protected static final Logger LOGGER = Logger.getLogger(TestCaseWithEncodeDecode.class);
	private static Logger rootLogger = Logger.getRootLogger();
	private static String ignoreerrors = null;
}
