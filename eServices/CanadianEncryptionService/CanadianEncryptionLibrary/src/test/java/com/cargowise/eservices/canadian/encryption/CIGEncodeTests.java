package com.cargowise.eservices.canadian.encryption;

import static org.junit.Assert.*;

import java.io.BufferedInputStream;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.Charset;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.security.cert.CertificateException;
import java.util.InputMismatchException;

import javax.mail.MessagingException;

import org.apache.commons.io.IOUtils;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Matchers;
import org.mockito.Mockito;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.cargowise.eservices.canadian.encryption.helper.TestCaseWithEncodeDecode;
import com.entrust.toolkit.User;
import com.entrust.toolkit.credentials.StreamProfileReader;
import com.entrust.toolkit.exceptions.CertificationException;
import com.entrust.toolkit.exceptions.UserBadPasswordException;
import com.entrust.toolkit.exceptions.UserFatalException;
import com.entrust.toolkit.exceptions.UserNotLoggedInException;
import com.entrust.toolkit.exceptions.UserRecoverException;
import com.entrust.toolkit.util.SecureStringBuffer;

import iaik.smime.SignedContent;
import iaik.x509.X509Certificate;

@RunWith(PowerMockRunner.class)
@PrepareForTest(User.class)
public class CIGEncodeTests extends TestCaseWithEncodeDecode {
	@Test
	public void testCreateSignedAndEncryptedMessage_Success() throws Exception {
		String input = "UNA:+.? 'UNB+UNOA:3+CLIENT+INETCECPT+090121:1641+20'UNG+GSMCAR+U41091N1+SRT+090121:1641+20+UN+D:00A:SUPRPT'"
				+ "UNE+0+20'UNZ+1+20'";
		CIGEncode cigencode = new CIGEncode(getSenderPublicKey(), getSenderPrivateKey(), PASSWORD, LOGGER);
		ByteArrayOutputStream outputStream = new ByteArrayOutputStream();

		// Test createSignedAndEncryptedMessage method taking Stream
		cigencode.createSignedAndEncryptedMessage(new ByteArrayInputStream(input.getBytes(Charset.forName("UTF-8"))),
				outputStream);
		assertEquals(input, getCIGDecodeEncodeHelper().getDecryptedContentAndRemoveHeader(outputStream.toByteArray()));

		// Test createSignedAndEncryptedMessage method taking String
		byte[] result = cigencode.createSignedAndEncryptedMessage(input);
		assertEquals(input, getCIGDecodeEncodeHelper().getDecryptedContentAndRemoveHeader(result));
	}

	@Test
	public void testCreateSignedAndEncryptedMDN_Success() throws Exception {
		CIGEncode cigencode = new CIGEncode(getSenderPublicKey(), getSenderPrivateKey(), PASSWORD, LOGGER);
		ByteArrayOutputStream outputStream = new ByteArrayOutputStream();

		// Test createdSignedAndEncryptedMDN method taking Stream
		InputStream inputStream = CIGEncodeTests.class.getResourceAsStream("MdnRequestContent.txt");
		try {
			cigencode.createdSignedAndEncryptedMDN(inputStream, outputStream);
		} finally {
			inputStream.close();
		}

		String output = getCIGDecodeEncodeHelper().getDecryptedContent(outputStream.toByteArray());
		try {
			assertEquals(IOUtils.toString(CIGEncodeTests.class.getResourceAsStream("MdnRequestContent_Matching.txt"),
					"UTF-8"), output);
		} finally {
			inputStream.close();
		}

		// Test createdSignedAndEncryptedMDN method taking String
		byte[] outputArray = cigencode.createdSignedAndEncryptedMDN(
				IOUtils.toString(CIGEncodeTests.class.getResourceAsStream("MdnRequestContent.txt"), "UTF-8"));
		assertEquals(
				IOUtils.toString(CIGEncodeTests.class.getResourceAsStream("MdnRequestContent_Matching.txt"), "UTF-8"),
				getCIGDecodeEncodeHelper().getDecryptedContent(outputArray));
	}

	// region Exception

	@Test
	public void testConstructor_UserBadPasswordException_Fail() {
		assertExpectedException(
				() -> new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), "InvalidPassword", LOGGER),
				UserBadPasswordException.class, "Incorrect Password!");
	}

	@Test
	public void testConstructor_UserFatalException_Fail() {
		assertExpectedException(
				() -> new CIGEncode(getRecipientPublicKey(),
						new ByteArrayInputStream("Invalid Certificate".getBytes(Charset.forName("UTF-8"))), PASSWORD,
						LOGGER),
				UserFatalException.class,
				"An error occured while reading the file-based Entrust Digital Identity (EPF)");
	}

	@Test
	public void testConstructor_CertificateException_InvalidRecipientPublicCertificate_Fail() {
		assertExpectedException(
				() -> new CIGEncode(new ByteArrayInputStream("Invalid Certificate".getBytes(Charset.forName("UTF-8"))),
						getSenderPrivateKey(), PASSWORD, LOGGER),
				CertificateException.class, "iaik.asn1.CodingException: ASN.1 creation error:Invalid character '32'!");
	}

	@Test
	public void testConstructor_IOException_Fail() throws IOException {
		InputStream stream = getRecipientPublicKey();
		stream.close();
		assertExpectedException(() -> new CIGEncode(stream, getSenderPrivateKey(), PASSWORD, LOGGER), IOException.class,
				"Stream closed");
	}

	@Test
	public void testConstructor_CertificateException_InvalidSenderPrivateKey_Failed()
			throws UserRecoverException, UserBadPasswordException, UserFatalException, CertificateException {
		CertificateException ex = new CertificateException("Invalid EPF certifcate");
		User user = PowerMockito.mock(User.class);
		PowerMockito.doThrow(ex).when(user).login(Matchers.any(StreamProfileReader.class),
				Matchers.any(SecureStringBuffer.class));
		assertExpectedException(() -> new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), PASSWORD, user, LOGGER),
				ex.getClass(), ex.getMessage());
	}

	@Test
	public void testConstructor_IllegalStateException_UserNotLoggedInException_Fail()
			throws UserNotLoggedInException, CertificationException {
		UserNotLoggedInException ex = new UserNotLoggedInException("Somehow user is logged out");
		User user = PowerMockito.mock(User.class);
		PowerMockito.doThrow(ex).when(user).validate(Matchers.any(X509Certificate.class));

		assertExpectedException(() -> new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), PASSWORD, user, LOGGER),
				IllegalStateException.class, "Unexpected error during encryption.",
				ex.getClass().getName() + ": " + ex.getMessage());

		User user1 = PowerMockito.mock(User.class);
		PowerMockito.doThrow(ex).when(user1).getVerificationCertificate();
		assertExpectedException(() -> new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), PASSWORD, user, LOGGER),
				IllegalStateException.class, "Unexpected error during encryption.",
				ex.getClass().getName() + ": " + ex.getMessage());
	}

	@Test
	public void testCreateSignedAndEncrypted_IllegalStateException_NoSuchAlgorithmException_InvalidKeyException_Fail()
			throws UserBadPasswordException, UserFatalException, CertificateException, IOException, InvalidKeyException,
			NoSuchAlgorithmException {
		String message = "Invalid state which could never be reached";
		CIGEncode encode = Mockito.spy(new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), PASSWORD, LOGGER));
		Mockito.doThrow(new NoSuchAlgorithmException(message)).doThrow(new InvalidKeyException(message)).when(encode)
				.getNewSignedContent();
		// createSignedAndEncryptedMessage
		assertExpectedException(
				() -> encode.createSignedAndEncryptedMessage(
						new ByteArrayInputStream(message.getBytes(Charset.forName("UTF-8"))),
						new ByteArrayOutputStream()),
				IllegalStateException.class, "Unexpected error during encryption.",
				"java.security.NoSuchAlgorithmException: Invalid state which could never be reached");
		assertExpectedException(
				() -> encode.createSignedAndEncryptedMessage(
						new ByteArrayInputStream(message.getBytes(Charset.forName("UTF-8"))),
						new ByteArrayOutputStream()),
				IllegalStateException.class, "Unexpected error during encryption.",
				"java.security.InvalidKeyException: Invalid state which could never be reached");
		// createdSignedAndEncryptedMDN
		Mockito.doThrow(new NoSuchAlgorithmException(message)).doThrow(new InvalidKeyException(message)).when(encode)
				.getNewSignedContent();
		InputStream input = CIGEncodeTests.class.getResourceAsStream("MdnRequestContent.txt");
		try {
			input.mark(input.available() + 1);
			assertExpectedException(() -> encode.createdSignedAndEncryptedMDN(input, new ByteArrayOutputStream()),
					IllegalStateException.class, "Unexpected error during encryption.",
					"java.security.NoSuchAlgorithmException: Invalid state which could never be reached");
			input.reset();
			assertExpectedException(() -> encode.createdSignedAndEncryptedMDN(input, new ByteArrayOutputStream()),
					IllegalStateException.class, "Unexpected error during encryption.",
					"java.security.InvalidKeyException: Invalid state which could never be reached");
		} finally {
			input.close();
		}
	}

	@Test
	public void testCreateSignedAndEncrypted_IllegalStateException_MessagingException_Fail()
			throws IOException, MessagingException, UserBadPasswordException, UserFatalException, CertificateException {
		String message = "Invalid state which could never be reached";
		CIGEncode encode = Mockito.spy(new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), PASSWORD, LOGGER));
		Mockito.doThrow(new MessagingException(message)).when(encode)
				.encryptAndWriteTo(Matchers.any(SignedContent.class), Matchers.any(OutputStream.class));
		// createSignedAndEncryptedMessage
		assertExpectedException(
				() -> encode.createSignedAndEncryptedMessage(
						new ByteArrayInputStream(message.getBytes(Charset.forName("UTF-8"))),
						new ByteArrayOutputStream()),
				IllegalStateException.class, "Unexpected error during encryption.",
				"javax.mail.MessagingException: Invalid state which could never be reached");
		// createdSignedAndEncryptedMDN
		InputStream input = CIGEncodeTests.class.getResourceAsStream("MdnRequestContent.txt");
		try {
			assertExpectedException(() -> encode.createdSignedAndEncryptedMDN(input, new ByteArrayOutputStream()),
					IllegalStateException.class, "Unexpected error during encryption.",
					"javax.mail.MessagingException: Invalid state which could never be reached");
		} finally {
			input.close();
		}
	}

	@Test
	public void testCreateSignedAndEncrypted_IOException_Fail()
			throws UserBadPasswordException, UserFatalException, CertificateException, IOException, MessagingException {
		CIGEncode encode = Mockito.spy(new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), PASSWORD, LOGGER));
		BufferedInputStream inputStream = new BufferedInputStream(
				new ByteArrayInputStream("Hello World".getBytes(Charset.forName("UTF-8"))));
		inputStream.close();
		Mockito.doThrow(new IOException("Readonly Stream!")).when(encode)
				.encryptAndWriteTo(Matchers.any(SignedContent.class), Matchers.any(OutputStream.class));
		// createSignedAndEncryptedMessage
		assertExpectedException("Try read Input Stream",
				() -> encode.createSignedAndEncryptedMessage(inputStream, new ByteArrayOutputStream()),
				IOException.class, "Stream closed");
		assertExpectedException("Try write Output Stream",
				() -> encode.createSignedAndEncryptedMessage(
						new ByteArrayInputStream("Hello World".getBytes(Charset.forName("UTF-8"))),
						new ByteArrayOutputStream()),
				IOException.class, "Readonly Stream!");
		// createdSignedAndEncryptedMDN
		assertExpectedException("Try read Input Stream",
				() -> encode.createdSignedAndEncryptedMDN(inputStream, new ByteArrayOutputStream()), IOException.class,
				"Stream closed");
		InputStream input = CIGEncodeTests.class.getResourceAsStream("MdnRequestContent.txt");
		try {
			assertExpectedException("Try write Output Stream",
				() -> encode.createdSignedAndEncryptedMDN(input, new ByteArrayOutputStream()),
				IOException.class, "Readonly Stream!");
		} finally {
			input.close();
		}
	}

	@Test
	public void testCreateSignedAndEncryptedMDN_InputMismatchException_Fail()
			throws UserBadPasswordException, UserFatalException, CertificateException, IOException {
		CIGEncode encode = new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), PASSWORD, LOGGER);
		assertExpectedException(
				() -> encode.createdSignedAndEncryptedMDN(
						new ByteArrayInputStream("Hello World".getBytes(Charset.forName("UTF-8"))),
						new ByteArrayOutputStream()),
				InputMismatchException.class,
				"The input to createdSignedAndEncryptedMDN did not contain a '--DBOUNDARY--' text string");
	}

	// endregion
}
