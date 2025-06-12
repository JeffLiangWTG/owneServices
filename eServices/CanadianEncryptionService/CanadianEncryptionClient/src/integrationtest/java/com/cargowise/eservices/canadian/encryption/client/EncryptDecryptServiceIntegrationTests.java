package com.cargowise.eservices.canadian.encryption.client;

import static org.junit.Assert.assertEquals;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.net.URL;
import java.nio.charset.Charset;
import java.util.Properties;

import org.apache.commons.io.IOUtils;
import org.junit.AfterClass;
import org.junit.BeforeClass;
import org.junit.Test;

public class EncryptDecryptServiceIntegrationTests {
	@Test
	public void testEncrypt_Success() throws Throwable {
		// Test Encrypt
		String input = "UNA:+.? 'UNB+UNOA:3+CLIENT+INETCECPT+090121:1641+20'UNG+GSMCAR+U41091N1+SRT+090121:1641+20+UN"
				+ "+D:00A:SUPRPT'UNE+0+20'UNZ+1+20'";
		byte[] encryptData = service.encryptMessage(input, true);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decrypt(encryptedMimeMessage, true);
		String[] lines = result.split("\r\n");
		assertEquals(input, lines[lines.length - 1]);
	}

	@Test
	public void testEncryptContent_Success() throws Throwable {
		// Test Encrypt
		String input = "UNA:+.? 'UNB+UNOA:3+CLIENT+INETCECPT+090121:1641+20'UNG+GSMCAR+U41091N1+SRT+090121:1641+20+UN+D:00A:SUPRPT'"
				+ "UNE+0+20'UNZ+1+20'";
		byte[] encryptData = service.encryptContent(input);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decryptContent(encryptedMimeMessage);
		String[] lines = result.split("\r\n");
		assertEquals(input, lines[lines.length - 1]);
	}

	@Test
	public void testEncryptMDN_Success() throws Throwable {
		// Test Encrypt
		String input = IOUtils
				.toString(EncryptDecryptServiceIntegrationTests.class.getResource("MdnRequestContent.txt"), "UTF-8");
		byte[] encryptData = service.encryptMdn(input, true);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decrypt(encryptedMimeMessage, true);
		assertEquals(IOUtils.toString(
				EncryptDecryptServiceIntegrationTests.class.getResource("MdnRequestContent_Matching.txt"), "UTF-8"),
				result.replaceFirst("Received-content-MIC: .*", "Received-content-MIC: pr0qhRoqtrxPxF4rCVlBeGgbWV4=")
						.replaceFirst("Content-Length: .*", "Content-Length: 6343"));
	}

	@Test
	public void testEncryptMDNContent_Success() throws Throwable {
		// Test Encrypt
		String input = IOUtils
				.toString(EncryptDecryptServiceIntegrationTests.class.getResource("MdnRequestContent.txt"), "UTF-8");
		byte[] encryptData = service.encryptMdnContent(input);

		// Test Decrypt
		byte[] encryptedMimeMessage = combineByteArray(getMimeHeaders(encryptData.length), encryptData);
		String result = service.decryptContent(encryptedMimeMessage);
		assertEquals(IOUtils.toString(
				EncryptDecryptServiceIntegrationTests.class.getResource("MdnRequestContent_Matching.txt"), "UTF-8"),
				result.replaceFirst("Received-content-MIC: .*", "Received-content-MIC: pr0qhRoqtrxPxF4rCVlBeGgbWV4=")
						.replaceFirst("Content-Length: .*", "Content-Length: 6343"));
	}

	@Test
	public void testDecrypt_Success() throws Throwable {
		byte[] encodedData = IOUtils.toByteArray(
				EncryptDecryptServiceIntegrationTests.class.getResourceAsStream("Base64EncryptedMessage.txt"));
		String result = service.decrypt(encodedData, true);
		assertEquals(IOUtils.toString(
				EncryptDecryptServiceIntegrationTests.class.getResourceAsStream("Base64DecryptedMessage.txt"), "UTF-8"),
				result);
	}

	@Test
	public void testDecryptContent_Success() throws Throwable {
		byte[] encodedData = IOUtils.toByteArray(
				EncryptDecryptServiceIntegrationTests.class.getResourceAsStream("Base64EncryptedMessage.txt"));

		String result = service.decryptContent(encodedData);
		assertEquals(IOUtils.toString(
				EncryptDecryptServiceIntegrationTests.class.getResourceAsStream("Base64DecryptedMessage.txt"), "UTF-8"),
				result);
	}

	private static IEncryptDecryptService service;

	@BeforeClass
	public static void runOnceBeforeClass() throws IOException {
		URL url = new URL(getURL());
		service = new EncryptDecryptService(url).getEncryptDecryptPort();
	}

	static String getURL() throws IOException {
		java.io.InputStream is = ClassLoader.getSystemClassLoader()
				.getResourceAsStream("maven(auto-generated).properties");
		java.util.Properties p = new Properties();
		p.load(is);
		String hostname = p.getProperty("tomcat_hostname");
		String port = p.getProperty("tomcat_port");
		String protocol = p.getProperty("tomcat_protocol");
		String path = p.getProperty("tomcat_deployment_path");
		return String.format("%1$s://%2$s:%3$s/%4$s/CanadianEncryptionService?wsdl", protocol, hostname, port, path);
	}

	// Run once, e.g close connection, cleanup
	@AfterClass
	public static void runOnceAfterClass() {
		((com.sun.xml.ws.Closeable) service).close();
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
