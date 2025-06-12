package com.cargowise.eservices.canadian.encryption;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.Charset;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.security.PrivateKey;
import java.security.cert.CertificateException;
import java.util.InputMismatchException;

import javax.activation.CommandMap;
import javax.activation.MailcapCommandMap;
import javax.mail.Header;
import javax.mail.MessagingException;

import org.apache.log4j.Logger;

import com.entrust.toolkit.User;
import com.entrust.toolkit.credentials.StreamProfileReader;
import com.entrust.toolkit.exceptions.CertificationException;
import com.entrust.toolkit.exceptions.UserBadPasswordException;
import com.entrust.toolkit.exceptions.UserFatalException;
import com.entrust.toolkit.exceptions.UserNotLoggedInException;
import com.entrust.toolkit.util.SecureStringBuffer;
import com.google.common.annotations.VisibleForTesting;

import iaik.asn1.structures.AlgorithmID;
import iaik.smime.EncryptedContent;
import iaik.smime.SMimeBodyPart;
import iaik.smime.SMimeMultipart;
import iaik.smime.SignedContent;
import iaik.utils.Util;
import iaik.x509.X509Certificate;

/**
 * Encoding Library.
 * 
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.8
 */
public class CIGEncode {
	@VisibleForTesting
	final transient X509Certificate[] signerCertificates;
	@VisibleForTesting
	final transient X509Certificate recipientCertificate;
	private final transient X509Certificate signerCertificate;
	private final transient PrivateKey signerPrivateKey;
	private final transient Logger logger;
	private final transient User sender;
	private static final int BUFFER_SIZE = 4096;
	private static final int BOUNDRY_PART3_POSITION = 13;

	/**
	 * Initialize recipient private key, sender certificates and logger for
	 * CIGEncode.
	 * 
	 * @param recipientPublicKey
	 *            public key of the recipient
	 * @param senderPrivateKey
	 *            private key of the sender
	 * @param password
	 *            password of the sender's private key
	 * @param logger
	 *            web service log4j
	 * @throws UserBadPasswordException
	 *             occurs when password is invalid
	 * @throws UserFatalException
	 *             occurs when Invalid EPF certificate
	 * @throws CertificateException
	 *             invalid Certificate
	 * @throws IOException
	 *             error occurs during read/write input/output
	 */
	public CIGEncode(final InputStream recipientPublicKey, final InputStream senderPrivateKey, final String password,
			final Logger logger)
			throws UserBadPasswordException, UserFatalException, CertificateException, IOException {
		this(recipientPublicKey, senderPrivateKey, password, new User(), logger);
	}

	@SuppressWarnings("deprecation")
	CIGEncode(final InputStream recipientPublicKey, final InputStream senderPrivateKey, final String password,
			final User sender, final Logger logger)
			throws UserBadPasswordException, UserFatalException, CertificateException, IOException {
		setCommandMap();
		this.logger = logger;
		final StreamProfileReader entrustProfileReader = new StreamProfileReader(senderPrivateKey);
		this.sender = sender;

		try {
			sender.login(entrustProfileReader, new SecureStringBuffer(password));
		} catch (UserBadPasswordException | UserFatalException | CertificateException e) {
			logger.error("Login failed: " + e.getMessage(), e);
			throw e;
		}

		try {
			recipientCertificate = new X509Certificate(recipientPublicKey);
		} catch (CertificateException | IOException e) {
			logger.error("Recipient Public Certificate is invalid: " + e.getMessage());
			throw e;
		}

		try {
			try {
				sender.validate(recipientCertificate);
			} catch (CertificationException e) {
				logger.warn("Recipient's Certificate not trusted! " + e.getMessage());
			}
			signerCertificates = new X509Certificate[2];
			signerCertificate = sender.getVerificationCertificate();
			signerCertificates[0] = signerCertificate;
			signerCertificates[1] = sender.getCaCertificate();
			signerPrivateKey = sender.getSigningKey();
		} catch (UserNotLoggedInException e) {
			throw newIllegalStateException(e);
		}
	}

	/**
	 * Dispose com.entrust.toolkit.User. {@inheritDoc}
	 */
	@Override
	protected final void finalize() throws Throwable {
		try {
			if (sender != null) {
				sender.logout();
			}
		} catch (UserNotLoggedInException e) {
			logger.warn("User was unexpected logout before. User must only be logout in finalize.", e);
		}
		super.finalize();
	}

	/**
	 * Encrypt message from a text.
	 * 
	 * @param inputContent
	 *            message text
	 * @return byte array of encrypted message
	 * @throws IllegalStateException
	 *             runtime error occurs which is in illegal state
	 * @throws IOException
	 *             error occurs during read/write input/output
	 */
	public byte[] createSignedAndEncryptedMessage(final String inputContent) throws IllegalStateException, IOException {
		final ByteArrayInputStream inputStream = new ByteArrayInputStream(
				inputContent.getBytes(Charset.forName("UTF-8")));
		final ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
		createSignedAndEncryptedMessage(inputStream, outputStream);
		return outputStream.toByteArray();
	}

	/**
	 * Encrypt message from a stream and output to a stream.
	 * 
	 * @param inputStream
	 *            message test
	 * @param outputStream
	 *            this stream will be used to load the encrypted message
	 * @throws IllegalStateException
	 *             runtime error occurs which is in illegal state
	 * @throws IOException
	 *             error occurs during read/write input/output
	 */
	public void createSignedAndEncryptedMessage(final InputStream inputStream, final OutputStream outputStream)
			throws IllegalStateException, IOException {
		try {
			final byte[] inFileByteArray = new byte[BUFFER_SIZE];
			final StringBuffer stringBuff = new StringBuffer();
			int size = inputStream.read(inFileByteArray);
			while (size > 0) {
				stringBuff.append(Util.toASCIIString(inFileByteArray, 0, size));
				size = inputStream.read(inFileByteArray);
			}

			SignedContent sc;
			try {
				sc = getNewSignedContent();
			} catch (InvalidKeyException ex) {
				logger.error("Invalid key specified for signing: " + ex.getMessage(), ex);
				throw ex;
			}
			sc.setContentContentHeaders(new Header[] {new Header("Content-Type", "Application/edi-edifact")});
			sc.setText(stringBuff.toString());

			encryptAndWriteTo(sc, outputStream);
		} catch (NoSuchAlgorithmException | InvalidKeyException | MessagingException ex) {
			throw newIllegalStateException(ex);
		}
	}

	/**
	 * Encrypt MDN message, which contains '--DBOUNDARY--' text string, from a
	 * stream and output to a stream.
	 * 
	 * @param inputContent
	 *            message test
	 * @return byte array of encrypted message
	 * @throws IllegalStateException
	 *             runtime error occurs which is in illegal state
	 * @throws InputMismatchException
	 *             missing '--DBOUNDARY--' text string
	 * @throws IOException
	 *             error occurs during read/write input/output
	 */
	public byte[] createdSignedAndEncryptedMDN(final String inputContent)
			throws InputMismatchException, IllegalStateException, IOException {
		final ByteArrayInputStream inputStream = new ByteArrayInputStream(inputContent.getBytes(Charset.forName("UTF-8")));
		final ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
		createdSignedAndEncryptedMDN(inputStream, outputStream);
		return outputStream.toByteArray();
	}

	/**
	 * Encrypt MDN message, which contains '--DBOUNDARY--' text string, from a
	 * stream and output to a stream.
	 * 
	 * @param inputStream
	 *            message test
	 * @param outputStream
	 *            this stream will be used to load the encrypted message
	 * @throws IllegalStateException
	 *             runtime error occurs which is in illegal state
	 * @throws InputMismatchException
	 *             missing '--DBOUNDARY--' text string
	 * @throws IOException
	 *             error occurs during read/write input/output
	 */
	public void createdSignedAndEncryptedMDN(final InputStream inputStream, final OutputStream outputStream)
			throws InputMismatchException, IllegalStateException, IOException {
		try {
			final byte[] inFileByteArray = new byte[BUFFER_SIZE];
			final StringBuffer stringBuff = new StringBuffer();
			int size = inputStream.read(inFileByteArray);
			while (size > 0) {
				stringBuff.append(Util.toASCIIString(inFileByteArray, 0, size));
				size = inputStream.read(inFileByteArray);
			}

			final int startOfBoundry = stringBuff.indexOf("--DBOUNDARY--");
			if (startOfBoundry < 0) {
				throw new InputMismatchException(
						"The input to createdSignedAndEncryptedMDN did not contain a '--DBOUNDARY--' text string");
			}

			final SMimeMultipart mpReport = new SMimeMultipart("report; report-type=\"disposition notification\"");
			final SMimeBodyPart part1 = new SMimeBodyPart();
			part1.setText("EDI Message Received/Decrypted/Validated and Forwarded for Processing");
			final SMimeBodyPart part2 = new SMimeBodyPart();
			part2.setText(stringBuff.substring(0, startOfBoundry));
			final SMimeBodyPart part3 = new SMimeBodyPart();
			part3.setText(stringBuff.substring(startOfBoundry + BOUNDRY_PART3_POSITION));
			mpReport.addBodyPart(part1);
			mpReport.addBodyPart(part2);
			mpReport.addBodyPart(part3);

			SignedContent sc;
			try {
				sc = getNewSignedContent();
			} catch (InvalidKeyException ex) {
				logger.error("Invalid key specified for signing: " + ex.getMessage(), ex);
				throw ex;
			}
			sc.setContent(mpReport, mpReport.getContentType());
			sc.setCertificates(signerCertificates);

			part2.setHeader("Content-Type", "Message/disposition-notification");
			part3.setHeader("Content-Type", "Message/rfc822");

			encryptAndWriteTo(sc, outputStream);
		} catch (NoSuchAlgorithmException | InvalidKeyException | MessagingException ex) {
			throw newIllegalStateException(ex);
		}
	}

	/**
	 * Encrypt the signed content and write it into a stream.
	 */
	@VisibleForTesting
	void encryptAndWriteTo(final SignedContent sc, final OutputStream outputStream)
			throws MessagingException, IOException {
		final EncryptedContent ec = new EncryptedContent(sc);
		ec.addRecipient(recipientCertificate, AlgorithmID.rsaEncryption);
		ec.setEncryptionAlgorithm(AlgorithmID.cast5_CBC, 0);
		ec.writeTo(outputStream);
	}

	/**
	 * Get new signed content in order to assign the message into.
	 */
	@VisibleForTesting
	SignedContent getNewSignedContent() throws InvalidKeyException, NoSuchAlgorithmException {
		final SignedContent sc = new SignedContent(false);
		sc.setCertificates(signerCertificates);
		sc.addSigner(signerPrivateKey, signerCertificate);
		return sc;
	}

	/**
	 * Get new user to login sender's private key.
	 * 
	 * @return New user
	 */
	@VisibleForTesting
	User getNewUser() {
		return new User();
	}

	private IllegalStateException newIllegalStateException(final Exception ex) {
		final IllegalStateException exception = new IllegalStateException("Unexpected error during encryption.", ex);
		logger.error(exception);
		return exception;
	}

	private static void setCommandMap() {
		final MailcapCommandMap mc = (MailcapCommandMap) CommandMap.getDefaultCommandMap();
		mc.addMailcap("multipart/signed;; x-java-content-handler=iaik.smime.signed_content");
		mc.addMailcap("application/x-pkcs7-signature;; x-java-content-handler=iaik.smime.signed_content");
		mc.addMailcap("application/x-pkcs7-mime;; x-java-content-handler=iaik.smime.encrypted_content");
		mc.addMailcap("application/pkcs7-signature;; x-java-content-handler=iaik.smime.signed_content");
		mc.addMailcap("application/pkcs7-mime;; x-java-content-handler=iaik.smime.encrypted_content");
		mc.addMailcap("application/x-pkcs10;; x-java-content-handler=iaik.smime.pkcs10_content");
		mc.addMailcap("application/pkcs10;; x-java-content-handler=iaik.smime.pkcs10_content");
		mc.addMailcap("text/plain;; x-java-content-handler=com.sun.mail.handlers.text_plain");
		mc.addMailcap("multipart/*;; x-java-content-handler=com.sun.mail.handlers.multipart_mixed");
		mc.addMailcap("message/*;; x-java-content-handler=com.sun.mail.handlers.message_rfc822");
		CommandMap.setDefaultCommandMap(mc);
	}

}
