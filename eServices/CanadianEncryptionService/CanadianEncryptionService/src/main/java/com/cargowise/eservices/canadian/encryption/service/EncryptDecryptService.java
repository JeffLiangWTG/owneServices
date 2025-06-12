package com.cargowise.eservices.canadian.encryption.service;

import java.io.IOException;
import java.io.InputStream;
import java.nio.file.InvalidPathException;
import java.security.InvalidParameterException;
import java.security.cert.CertificateException;
import java.util.InputMismatchException;

import javax.annotation.Resource;
import javax.jws.WebMethod;
import javax.jws.WebService;
import javax.servlet.ServletContext;
import javax.xml.ws.WebServiceContext;
import javax.xml.ws.handler.MessageContext;

import org.apache.log4j.Logger;

import com.cargowise.eservices.canadian.encryption.CIGDecode;
import com.cargowise.eservices.canadian.encryption.CIGEncode;
import com.entrust.toolkit.cms.NotARecipientException;
import com.entrust.toolkit.exceptions.UserBadPasswordException;
import com.entrust.toolkit.exceptions.UserFatalException;
import com.google.common.annotations.VisibleForTesting;

import iaik.smime.SMimeException;

/**
 * Candian Encrypt & Decrypt Web Service.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.8
 */
@WebService(serviceName = "EncryptDecryptService",
targetNamespace = "http://cargowise.com/eservices/CanadianEncryptionService",
endpointInterface = "com.cargowise.eservices.canadian.encryption.service.IEncryptDecryptService", portName = "EncryptDecryptPort")
public class EncryptDecryptService implements IEncryptDecryptService {

	private CIGEncode cigencode;
	private CIGDecode cigdecode;
	@Resource
	private static WebServiceContext context;
	private ServletContext servletContext;
	private static final Logger LOG = Logger.getLogger(EncryptDecryptService.class);
	private static final String WEB_INF_DIR = "/WEB-INF/";

	/**
	 * {@inheritDoc}
	 */
	@Deprecated //TODO need to removed after moved to encryptContent(String).
	@Override
	public byte[] encryptMessage(final String text, final boolean isProduction) throws UserBadPasswordException, InputMismatchException,
			UserFatalException, IllegalStateException, CertificateException, IOException {
		return encryptContent(text);
	}

	/**
	 * {@inheritDoc}
	 */
	@Override
	public byte[] encryptContent(final String text) throws UserBadPasswordException, InputMismatchException,
			UserFatalException, IllegalStateException, CertificateException, IOException {
		LOG.info(String.format("EncryptMessage: %1$s", StringExtensions.truncateForLogging(text)));
		return getCigencode().createSignedAndEncryptedMessage(text);
	}

	/**
	 * Encrypt MDN message, which contains '--DBOUNDARY--' text string, from a text.
	 * 
	 * @deprecated use {@link #encryptMdnContent(String)} instead.
	 * @param text message text
	 * @param isProduction will be removed in next iteration(not used)
	 * @return byte array of encrypted message
	 */
	@Deprecated //TODO need to removed after moved to encryptMdnContent(String).
	@Override
	public byte[] encryptMdn(final String text, final boolean isProduction) throws UserBadPasswordException, InputMismatchException,
			UserFatalException, IllegalStateException, CertificateException, IOException {
		return encryptMdnContent(text);
	}

	/**
	 * Encrypt MDN message, which contains '--DBOUNDARY--' text string, from a text.
	 * 
	 * @param text message text
	 * @return byte array of encrypted message
	 */
	@Override
	public byte[] encryptMdnContent(final String text) throws UserBadPasswordException, InputMismatchException,
			UserFatalException, IllegalStateException, CertificateException, IOException {
		LOG.info(String.format("EncryptMdn: %1$s", StringExtensions.truncateForLogging(text)));
		return getCigencode().createdSignedAndEncryptedMDN(text);
	}

	/**
	 * Decrypt encoded message from byte array.
	 * 
	 * @deprecated use {@link #decryptContent(byte[])} instead.
	 * @param encodedData encrypted message's byte array
	 * @param isProduction will be removed in next iteration(not used)
	 * @return decrypted message text
	 */
	@Deprecated //TODO need to removed after moved to decryptContent(byte[]).
	@Override
	public String decrypt(final byte[] encodedData, final boolean isProduction) throws UserBadPasswordException, UserFatalException,
			SMimeException, CertificateException, NotARecipientException, IllegalStateException, IOException {
		return decryptContent(encodedData);
	}

	/**
	 * Decrypt encoded message from byte array.
	 * 
	 * @param encodedData encrypted message's byte array
	 * @return decrypted message text
	 */
	@Override
	public String decryptContent(final byte[] encodedData) throws UserBadPasswordException, UserFatalException,
			SMimeException, CertificateException, NotARecipientException, IllegalStateException, IOException {
		LOG.info(String.format("Decrypting encodedData's length: %1$d", encodedData.length));
		return getCigdecode().decryptMessage(encodedData);
	}

	/**
	 * get CIGEncode library.
	 */
	@WebMethod(exclude = true)
	private CIGEncode getCigencode() throws UserBadPasswordException, UserFatalException, CertificateException, IOException {
		if (cigencode == null) {
			setCigencode(new CIGEncode(getRecipientPublicKey(), getSenderPrivateKey(), getPassword(), LOG));
		}
		return cigencode;
	}

	/**
	 * get CIGDecode library.
	 */
	@WebMethod(exclude = true)
	private CIGDecode getCigdecode() throws UserBadPasswordException, UserFatalException, CertificateException {
		System.setProperty("mail.mime.base64.ignoreerrors", "true");
		if (cigdecode == null) {
			setCigdecode(new CIGDecode(getSenderPrivateKey(), getPassword(), LOG));
		}
		return cigdecode;
	}

	/**
	 * set CIGEncode library.
	 */
	@WebMethod(exclude = true)
	private void setCigencode(final CIGEncode cigencode) {
		this.cigencode = cigencode;
	}

	/**
	 * set CIGDecode library.
	 */
	@WebMethod(exclude = true)
	private void setCigdecode(final CIGDecode cigdecode) {
		this.cigdecode = cigdecode;
	}

	/**
	 * Get password of sender recipient key from config file.
	 */
	@VisibleForTesting
	@WebMethod(exclude = true)
	String getPassword() {
		return getServletContext().getInitParameter("Password");
	}

	/**
	 * Get sender recipient key from config file.
	 */
	@VisibleForTesting
	@WebMethod(exclude = true)
	InputStream getSenderPrivateKey() {
		return getResourceStreamFromContextParam("SenderPrivateKey");
	}

	/**
	 * Get recipient public key from config file.
	 * @return public key stream
	 */
	@VisibleForTesting
	@WebMethod(exclude = true)
	InputStream getRecipientPublicKey() {
		return getResourceStreamFromContextParam("RecipientPublicKey");
	}

	@WebMethod(exclude = true)
	private InputStream getResourceStreamFromContextParam(final String contextParam) throws InvalidParameterException, InvalidPathException {
		final String contextValue = getServletContext().getInitParameter(contextParam);
		if (contextValue == null || contextValue.trim().isEmpty()) {
			throw new InvalidParameterException(String.format("web.xml does not contain a valid '%1$2s' context-param.", contextParam));
		} else {
			final String resourcePath = WEB_INF_DIR + contextValue;
			final InputStream resourceStream = getServletContext().getResourceAsStream(resourcePath);
			if (resourceStream == null) {
				throw new InvalidPathException(getServletContext().getRealPath("/").replace("\\", "/") + resourcePath.substring(1),
						"Invalid file path");
			}
			return resourceStream;
		}
	}

	@WebMethod(exclude = true)
	private ServletContext getServletContext() {
		if (servletContext == null) {
			setServletContext((ServletContext) context.getMessageContext().get(MessageContext.SERVLET_CONTEXT));
		}
		return servletContext;
	}

	@WebMethod(exclude = true)
	private void setServletContext(final ServletContext servletContext) {
		this.servletContext = servletContext;
	}
}
