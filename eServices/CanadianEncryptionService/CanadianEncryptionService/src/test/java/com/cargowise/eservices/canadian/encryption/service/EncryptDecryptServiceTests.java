package com.cargowise.eservices.canadian.encryption.service;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.nio.charset.Charset;

import org.apache.commons.io.IOUtils;
import org.mockito.Mockito;

import junit.framework.TestCase;

@SuppressWarnings("deprecation")
public class EncryptDecryptServiceTests extends TestCase {

	public final void testEncrypt_Success() throws Exception {
		// Test Encrypt
		String input = "UNA:+.? 'UNB+UNOA:3+CLIENT+INETCECPT+090121:1641+20'UNG+GSMCAR+U41091N1+SRT+090121:1641+20+UN+D:00A:SUPRPT'"
				+ "UNE+0+20'UNZ+1+20'";
		IEncryptDecryptService service = getEncryptDecryptService();
		byte[] encryptData = service.encryptMessage(input, true);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decrypt(encryptedMimeMessage, true);
		String[] lines = result.split("\r\n");
		assertEquals(input, lines[lines.length - 1]);
	}

	public final void testEncryptContent_Success() throws Exception {
		// Test Encrypt
		String input = "UNA:+.? 'UNB+UNOA:3+CLIENT+INETCECPT+090121:1641+20'UNG+GSMCAR+U41091N1+SRT+090121:1641+20+UN+D:00A:SUPRPT'"
				+ "UNE+0+20'UNZ+1+20'";
		IEncryptDecryptService service = getEncryptDecryptService();
		byte[] encryptData = service.encryptContent(input);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decryptContent(encryptedMimeMessage);
		String[] lines = result.split("\r\n");
		assertEquals(input, lines[lines.length - 1]);
	}

	public final void testEncryptMDN_Success() throws Exception {
		// Test Encrypt
		String input = IOUtils.toString(EncryptDecryptServiceTests.class.getResource("MdnRequestContent.txt"), "UTF-8");
		IEncryptDecryptService service = getEncryptDecryptService();
		byte[] encryptData = service.encryptMdn(input, true);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decrypt(encryptedMimeMessage, true);
		assertEquals(IOUtils.toString(EncryptDecryptServiceTests.class.getResource("MdnRequestContent_Matching.txt"), "UTF-8"),
				result.replaceFirst("Received-content-MIC: .*", "Received-content-MIC: pr0qhRoqtrxPxF4rCVlBeGgbWV4=")
						.replaceFirst("Content-Length: .*", "Content-Length: 6343"));
	}

	public final void testEncryptMDNContent_Success() throws Exception {
		// Test Encrypt
		String input = IOUtils.toString(EncryptDecryptServiceTests.class.getResource("MdnRequestContent.txt"), "UTF-8");
		IEncryptDecryptService service = getEncryptDecryptService();
		byte[] encryptData = service.encryptMdnContent(input);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decryptContent(encryptedMimeMessage);
		assertEquals(IOUtils.toString(EncryptDecryptServiceTests.class.getResource("MdnRequestContent_Matching.txt"), "UTF-8"),
				result.replaceFirst("Received-content-MIC: .*", "Received-content-MIC: pr0qhRoqtrxPxF4rCVlBeGgbWV4=")
						.replaceFirst("Content-Length: .*", "Content-Length: 6343"));
	}

	public final void testDecrypt_Success() throws Exception {
		byte[] encodedData = IOUtils.toByteArray(EncryptDecryptServiceTests.class.getResourceAsStream("Base64EncryptedMessage.txt"));
		IEncryptDecryptService service = getEncryptDecryptService();
		String result = service.decrypt(encodedData, true);
		assertEquals(IOUtils.toString(EncryptDecryptServiceTests.class.getResourceAsStream("Base64DecryptedMessage.txt"), "UTF-8"), result);
	}

	public final void testDecryptContent_Success() throws Exception {
		byte[] encodedData = IOUtils.toByteArray(EncryptDecryptServiceTests.class.getResourceAsStream("Base64EncryptedMessage.txt"));
		IEncryptDecryptService service = getEncryptDecryptService();
		String result = service.decryptContent(encodedData);
		assertEquals(IOUtils.toString(EncryptDecryptServiceTests.class.getResourceAsStream("Base64DecryptedMessage.txt"), "UTF-8"), result);
	}

	private String ignoreerrors = null;

	@Override
	protected final void setUp() throws Exception {
		ignoreerrors = System.getProperty("mail.mime.base64.ignoreerrors");
		if (ignoreerrors == null || !ignoreerrors.equals("true")) {
			System.setProperty("mail.mime.base64.ignoreerrors", "true");
		}
	}

	@Override
	protected final void tearDown() {
		if (ignoreerrors == null) {
			System.clearProperty("mail.mime.base64.ignoreerrors");
		} else if (!ignoreerrors.equals("true")) {
			System.setProperty("mail.mime.base64.ignoreerrors", ignoreerrors);
		}
	}

	private static ClassLoader classLoader = ClassLoader.getSystemClassLoader();

	private IEncryptDecryptService getEncryptDecryptService() {
		EncryptDecryptService service = Mockito.spy(new EncryptDecryptService());
		Mockito.doReturn("CargoWise2010").when(service).getPassword();
		Mockito.doAnswer((inputStream) -> classLoader.getResourceAsStream("CACertTest.cer")).when(service)
				.getRecipientPublicKey(); // return new instance everytime
		Mockito.doAnswer((inputStream) -> classLoader.getResourceAsStream("CACertTest.epf")).when(service)
				.getSenderPrivateKey(); // return new instance everytime

		return service;
	}

	private byte[] combineByteArray(final byte[] bytes1, final byte[] bytes2) throws IOException {
		ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
		outputStream.write(bytes1);
		outputStream.write(bytes2);
		return outputStream.toByteArray();
	}

	private byte[] getMimeHeaders(final int messageLength) {
		StringBuilder mimeHeaders = new StringBuilder();
		mimeHeaders.append("MIME-Version: 1.0\r\n");
		mimeHeaders.append("From: ver7enterprise@cargowise.com\r\n");
		mimeHeaders.append("Message-ID: <15830.3046945.433.1487113607876.JavaMail.ctladm3@EC07ZT5615>\r\n");
		mimeHeaders.append("Date: Fri, 17 Feb 2017 23:06:47 GMT\r\n");
		mimeHeaders.append("Accept: application/pkcs7-mime; text/plain\r\n");
		mimeHeaders.append("Connection: keep-alive\r\n");
		mimeHeaders.append("Host: www.test.com\r\n");
		mimeHeaders.append("Content-Transfer-Encoding: binary\r\n");
		mimeHeaders.append("User-Agent: Canadian Customs InboundServiceTask 1.0\r\n");
		mimeHeaders.append("Content-Type: Application/pkcs7-mime\r\n");
		mimeHeaders.append(String.format("Content-Length: %1$d\r%n\r%n", messageLength));
		return mimeHeaders.toString().getBytes(Charset.forName("UTF-8"));
	}
}
