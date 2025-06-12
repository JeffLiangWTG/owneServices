package com.cargowise.eservices.canadian.encryption.service;

import java.io.IOException;
import java.security.cert.CertificateException;
import java.util.InputMismatchException;

import javax.jws.WebMethod;
import javax.jws.WebParam;
import javax.jws.WebService;

import com.entrust.toolkit.cms.NotARecipientException;
import com.entrust.toolkit.exceptions.UserBadPasswordException;
import com.entrust.toolkit.exceptions.UserFatalException;

import iaik.smime.SMimeException;

/**
 * Candian Encrypt & Decrypt Web Service interface.
 * 
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.8
 */
@WebService(targetNamespace = "http://cargowise.com/eservices/CanadianEncryptionService")
interface IEncryptDecryptService {
	/**
	 * Encrypt message from a text. Deprecated, please use
	 * {@link #encryptContent(String)} instead.
	 * 
	 * @deprecated
	 * @param text
	 *            message text
	 * @param isProduction
	 *            will be removed in next iteration(not used)
	 * @return byte array of encrypted message
	 */
	@WebMethod(action = "EncryptMessage")
	byte[] encryptMessage(@WebParam(name = "text") String text, @WebParam(name = "isProduction") boolean isProduction)
			throws UserBadPasswordException, InputMismatchException, UserFatalException, IllegalStateException,
			CertificateException, IOException;

	/**
	 * Encrypt message from a text.
	 * 
	 * @param text
	 *            message text
	 * @return byte array of encrypted message
	 */
	@WebMethod(action = "EncryptContent")
	byte[] encryptContent(@WebParam(name = "text") String text) throws UserBadPasswordException, InputMismatchException,
			UserFatalException, IllegalStateException, CertificateException, IOException;

	/**
	 * Encrypt MDN message, which contains '--DBOUNDARY--' text string, from a
	 * text. Deprecated, please use {@link #encryptMdnContent(String)} instead.
	 * 
	 * @deprecated
	 * @param text
	 *            message text
	 * @param isProduction
	 *            will be removed in next iteration(not used)
	 * @return byte array of encrypted message
	 */
	@WebMethod(action = "EncryptMdn")
	byte[] encryptMdn(@WebParam(name = "text") String text, @WebParam(name = "isProduction") boolean isProduction)
			throws UserBadPasswordException, InputMismatchException, UserFatalException, IllegalStateException,
			CertificateException, IOException;

	/**
	 * Encrypt MDN message, which contains '--DBOUNDARY--' text string, from a
	 * text.
	 * 
	 * @param text
	 *            message text
	 * @return byte array of encrypted message
	 */
	@WebMethod(action = "EncryptMdnContent")
	byte[] encryptMdnContent(@WebParam(name = "text") String text) throws UserBadPasswordException,
			InputMismatchException, UserFatalException, IllegalStateException, CertificateException, IOException;

	/**
	 * Decrypt encoded message from byte array. Deprecated, please use
	 * {@link #decryptContent(byte[])} instead.
	 * 
	 * @deprecated
	 * @param encodedData
	 *            encrypted message's byte array
	 * @param isProduction
	 *            will be removed in next iteration(not used)
	 * @return decrypted message text
	 */
	@WebMethod(action = "Decrypt")
	String decrypt(@WebParam(name = "encodedData") byte[] encodedData,
			@WebParam(name = "isProduction") boolean isProduction) throws UserBadPasswordException, UserFatalException,
			SMimeException, CertificateException, NotARecipientException, IllegalStateException, IOException;

	/**
	 * Decrypt encoded message from byte array.
	 * 
	 * @param encodedData
	 *            encrypted message's byte array
	 * @return decrypted message text
	 */
	@WebMethod(action = "DecryptContent")
	String decryptContent(@WebParam(name = "encodedData") byte[] encodedData)
			throws UserBadPasswordException, UserFatalException, SMimeException, CertificateException,
			NotARecipientException, IllegalStateException, IOException;
}
