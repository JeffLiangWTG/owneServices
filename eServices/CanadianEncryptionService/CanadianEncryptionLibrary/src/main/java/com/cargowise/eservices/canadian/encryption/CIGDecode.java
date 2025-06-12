package com.cargowise.eservices.canadian.encryption;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.Charset;
import java.security.SignatureException;
import java.security.cert.CertificateException;
import java.util.Enumeration;
import java.util.Properties;

import javax.activation.CommandMap;
import javax.activation.MailcapCommandMap;
import javax.mail.Header;
import javax.mail.Message;
import javax.mail.MessagingException;
import javax.mail.Multipart;
import javax.mail.Part;
import javax.mail.Session;
import javax.mail.internet.MimeMessage;

import org.apache.log4j.Level;
import org.apache.log4j.Logger;

import com.entrust.toolkit.User;
import com.entrust.toolkit.cms.NotARecipientException;
import com.entrust.toolkit.credentials.StreamProfileReader;
import com.entrust.toolkit.exceptions.CertificationException;
import com.entrust.toolkit.exceptions.UserBadPasswordException;
import com.entrust.toolkit.exceptions.UserFatalException;
import com.entrust.toolkit.exceptions.UserNotLoggedInException;
import com.entrust.toolkit.util.SecureStringBuffer;
import com.google.common.annotations.VisibleForTesting;

import iaik.asn1.structures.AlgorithmID;
import iaik.cms.CMSException;
import iaik.cms.SignerInfo;
import iaik.smime.EncryptedContent;
import iaik.smime.SMimeException;
import iaik.smime.SignedContent;
import iaik.utils.Util;
import iaik.x509.X509Certificate;

/**
 * Decoding Library.
 * 
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.8
 */
public class CIGDecode {
	private static final String UTF_8 = "UTF-8";
	private final transient User recipient;
	private final transient Logger logger;
	private static final int BUFFER_SIZE = 1024;

	/**
	 * Initialize recipient private key and logger for CIGDecode.
	 * 
	 * @param recipientPrivateKey
	 *            private key of the recipient
	 * @param password
	 *            password of the private key
	 * @param logger
	 *            web service log4j
	 * @throws UserBadPasswordException
	 *             occurs when password is invalid
	 * @throws UserFatalException
	 *             occurs while reading the file-based Entrust Digital Identity
	 *             (EPF)
	 * @throws CertificateException
	 *             occurs when Invalid EPF certificate
	 */
	public CIGDecode(final InputStream recipientPrivateKey, final String password, final Logger logger)
			throws UserBadPasswordException, UserFatalException, CertificateException {
		this(recipientPrivateKey, password, new User(), logger);
	}

	@SuppressWarnings("deprecation")
	CIGDecode(final InputStream recipientPrivateKey, final String password, final User recipient, final Logger logger)
			throws UserBadPasswordException, UserFatalException, CertificateException {
		setCommandMap();
		this.logger = logger;
		final StreamProfileReader entrustProfileReader = new StreamProfileReader(recipientPrivateKey);
		this.recipient = recipient;
		try {
			recipient.login(entrustProfileReader, new SecureStringBuffer(password));
		} catch (UserBadPasswordException | UserFatalException | CertificateException e) {
			logger.error("Login failed: " + e.getMessage(), e);
			throw e;
		}
	}

	/**
	 * Dispose com.entrust.toolkit.User. {@inheritDoc}
	 */
	@Override
	protected final void finalize() throws Throwable {
		try {
			if (recipient != null) {
				recipient.logout();
			}
		} catch (UserNotLoggedInException e) {
			logger.warn("User was unexpected logout before. User must only be logout in finalize.", e);
		}
		super.finalize();
	}

	/**
	 * Decrypt message content from byte array.
	 * 
	 * @param encryptedMessage
	 *            byte array of encrypted message
	 * @return message content text
	 * @throws SMimeException
	 *             Error during Canadian's encryption side about SMime header
	 * @throws CertificateException
	 *             Invalid certificate
	 * @throws NotARecipientException
	 *             Invalid recipient
	 * @throws IOException
	 *             error occurs during read/write input/output
	 * @throws IllegalStateException
	 *             runtime error occurs which is in illegal state
	 */
	public String decryptMessage(final byte[] encryptedMessage)
			throws SMimeException, CertificateException, NotARecipientException, IOException, IllegalStateException {
		final ByteArrayInputStream inputStream = new ByteArrayInputStream(encryptedMessage);
		final ByteArrayOutputStream outputStream = decryptMessage(inputStream);
		return outputStream.toString(UTF_8);
	}

	/**
	 * Decrypt message content from a stream.
	 * 
	 * @param inputStream
	 *            Stream of encrypted message
	 * @return message content text
	 * @throws SMimeException
	 *             Error during Canadian's encryption side about SMime header
	 * @throws CertificateException
	 *             Invalid certificate
	 * @throws NotARecipientException
	 *             Invalid recipient
	 * @throws IOException
	 *             error occurs during read/write input/output
	 * @throws IllegalStateException
	 *             runtime error occurs which is in illegal state
	 */
	public ByteArrayOutputStream decryptMessage(final InputStream inputStream)
			throws SMimeException, CertificateException, NotARecipientException, IOException, IllegalStateException {
		final ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
		decryptMessage(inputStream, outputStream);
		return outputStream;
	}

	/**
	 * Decrypt message content from a stream and output into a stream.
	 * 
	 * @param inputStream
	 *            Stream of encrypted message
	 * @param outputStream
	 *            the decrypted message will be written into this stream
	 * @throws SMimeException
	 *             Error during Canadian's encryption side about SMime header
	 * @throws CertificateException
	 *             Invalid certificate
	 * @throws NotARecipientException
	 *             Invalid recipient
	 * @throws IOException
	 *             error occurs during read/write input/output
	 * @throws IllegalStateException
	 *             runtime error occurs which is in illegal state
	 */
	public void decryptMessage(final InputStream inputStream, final OutputStream outputStream)
			throws SMimeException, CertificateException, NotARecipientException, IOException, IllegalStateException {
		try {
			processMimeObject(getMimeMessage(inputStream), outputStream);
		} catch (UserNotLoggedInException ex) {
			throw newIllegalStateException(ex);
		} catch (MessagingException ex) {
			final Throwable innerException = ex.getCause();
			if (innerException instanceof IOException) {
				throw (IOException) innerException;
			}
			throw new UnsupportedOperationException(ex);
		}
	}

	/**
	 * Get mime message to wrap the input encrypted string. This mime message is
	 * used to decrypt the content.
	 */
	@VisibleForTesting
	MimeMessage getMimeMessage(final InputStream inputStream) throws MessagingException {
		final Properties props = System.getProperties();
		final Session session = Session.getDefaultInstance(props, null);
		return new MimeMessage(session, inputStream);
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
		CommandMap.setDefaultCommandMap(mc);
	}

	private void processMimeObject(final Object mimeObjectParent, final OutputStream outputStream) throws IOException,
			CertificateException, NotARecipientException, MessagingException, SMimeException, UserNotLoggedInException {
		Object mimeObject;
		if (mimeObjectParent instanceof Part) {
			writeHeaders(((Part) mimeObjectParent).getAllHeaders(), outputStream);
			mimeObject = ((Part) mimeObjectParent).getContent();
		} else {
			mimeObject = mimeObjectParent;
		}

		if (mimeObject instanceof EncryptedContent) {
			final EncryptedContent encryptedContent = (EncryptedContent) mimeObject;
			encryptedContent.setEncryptionAlgorithm(AlgorithmID.cast5_CBC, 0);
			try {
				encryptedContent.decryptSymmetricKey(recipient);
			} catch (NotARecipientException ex) {
				throw new CertificateException("Certificates mismatch.", ex);
			}
			processMimeObject(encryptedContent.getContent(), outputStream);
		} else if (mimeObject instanceof SignedContent) {
			final SignedContent signedContent = (SignedContent) mimeObject;
			processSignedContent(signedContent, outputStream);
			processMimeObject(signedContent.getContent(), outputStream);
		} else if (mimeObject instanceof String) {
			outputStream.write(((String) mimeObject).getBytes(Charset.forName(UTF_8)));
		} else if (mimeObject instanceof Multipart) {
			final Multipart multiPart = (Multipart) mimeObject;
			final int count = multiPart.getCount();
			for (int i = 0; i < count; i++) {
				processMimeObject(multiPart.getBodyPart(i), outputStream);
				outputStream.write("\r\n<CIGDecode Part Boundry>".getBytes(Charset.forName(UTF_8)));
			}
		} else if (mimeObject instanceof Message) {
			((Message) mimeObject).writeTo(outputStream);
		} else if (mimeObject instanceof InputStream) {
			final InputStream isPart = (InputStream) mimeObject;
			final byte[] inFileByteArray = new byte[BUFFER_SIZE];
			final StringBuffer stringBuff = new StringBuffer();
			int size = isPart.read(inFileByteArray);
			while (size > 0) {
				stringBuff.append(Util.toASCIIString(inFileByteArray, 0, size));
				size = isPart.read(inFileByteArray);
			}
			outputStream.write(stringBuff.toString().getBytes(Charset.forName(UTF_8)));
			isPart.close();
		} else {
			logger.warn("Unknown mime content: " + mimeObject);
		}
	}

	private void writeHeaders(final Enumeration<?> headers, final OutputStream outputStream) throws IOException {
		for (final Enumeration<?> enumeration = headers; enumeration.hasMoreElements();) {
			final Header header = (Header) enumeration.nextElement();
			outputStream.write(header.getName().getBytes(Charset.forName(UTF_8)));
			outputStream.write(": ".getBytes(Charset.forName(UTF_8)));
			outputStream.write(header.getValue().getBytes(Charset.forName(UTF_8)));
			outputStream.write("\r\n".getBytes(Charset.forName(UTF_8)));
		}
		outputStream.write("<CIGDecode Part Boundry>\r\n".getBytes(Charset.forName(UTF_8)));
	}

	private void processSignedContent(final SignedContent signedContent, final OutputStream outputStream)
			throws CertificateException, IOException, UserNotLoggedInException {
		if (!isSMimeTypeAsCertsOnly(signedContent)) {
			final SignerInfo signerInfo = getSignerInfo(signedContent);
			if (signerInfo != null) {
				final byte[] signedDigest = getSignedDigest(signerInfo);
				if (signedDigest != null) {
					outputStream.write("Received-content-MIC: ".getBytes(Charset.forName(UTF_8)));
					outputStream.write(Util.Base64Encode(signedDigest));
					outputStream.write("\r\n".getBytes(Charset.forName(UTF_8)));
					outputStream.write("<CIGDecode Part Boundry>\r\n".getBytes(Charset.forName(UTF_8)));
				}

				X509Certificate[] certs;
				try {
					certs = Util.convertCertificateChain(signedContent.getCertificates());
				} catch (CertificateException ex) {
					throw newIllegalStateException(ex);
				}

				if (logger.isEnabledFor(Level.WARN)) {
					validateCertificates(certs);
				}
			}
		}
	}

	/**
	 * Get signed digest from a signer information.
	 */
	@VisibleForTesting
	byte[] getSignedDigest(final SignerInfo signerInfo) {
		try {
			return signerInfo.getSignedDigest();
		} catch (CMSException e) {
			logger.warn("Verification with verify(cert) failed: " + e.getMessage(), e);
			return new byte[0];
		}
	}

	/**
	 * Get signer information from a SignedContent.
	 */
	@VisibleForTesting
	SignerInfo getSignerInfo(final SignedContent signedContent) {
		try {
			final X509Certificate signerCertificate = signedContent.verify();
			return signedContent.verify(signerCertificate);
		} catch (SignatureException ex) {
			logger.warn("Signature verification error: " + ex.getMessage(), ex);
			return null;
		}
	}

	/**
	 * Check SignedContent's SMIME type is "certs-only".
	 */
	@VisibleForTesting
	boolean isSMimeTypeAsCertsOnly(final SignedContent signedContent) {
		return signedContent.getSMimeType().equals("certs-only");
	}

	/**
	 * Validate whether certificate is valid. If the certificate is invalid,
	 * logging the warns.
	 */
	@VisibleForTesting
	boolean validateCertificates(final X509Certificate... certs) throws UserNotLoggedInException {
		final X509Certificate[] certificates = Util.arrangeCertificateChain(certs, false);
		boolean isValid = true;
		if (certificates != null) {
			for (int i = 0; i < certificates.length; i++) {
				try {
					recipient.validate(certificates[i]);
				} catch (CertificationException e) {
					logger.warn("Certificate not trusted: " + e.getMessage(), e);
					isValid = false;
				}
			}
		}
		return isValid;
	}

	private IllegalStateException newIllegalStateException(final Exception ex) {
		final IllegalStateException exception = new IllegalStateException("Unexpected error during decryption.", ex);
		logger.error(exception);
		return exception;
	}
}
