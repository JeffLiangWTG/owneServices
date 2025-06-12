package com.cargowise.eservices.common;

import java.io.BufferedInputStream;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.FilterInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.Base64;
import java.util.zip.GZIPInputStream;
import java.util.zip.GZIPOutputStream;
import java.util.zip.InflaterInputStream;
import java.util.zip.ZipInputStream;

import javax.xml.soap.SOAPElement;
import javax.xml.soap.SOAPException;

/**
 * Provides a generic view representing an input stream of bytes. It contains the encode, decode, compress and decompress functions.
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public final class Stream extends FilterInputStream {
	private long pos = 0;

	private long mark = 0;
	public static final int BUFFERSIZE = 8192;
	static final int ZIPHEADERBYTES = 1347093252;

	/**
	 * A constructor specifying the InputStream.
	 * @param stream input stream of bytes.
	 * @throws IOException read bytes issues.
	 */
	public Stream(final InputStream stream) throws IOException {
		super(stream);
		if (!markSupported()) {
			in = new BufferedInputStream(stream);
		}
		mark(available() + 1);
	}

	/**
	 * A constructor specifying bytes.
	 * @param bytes the bytes of input.
	 */
	public Stream(final byte[] bytes) {
		super(new ByteArrayInputStream(bytes));
	}

	/**
	 * Decode and decompress a current stream to another stream.
	 * @return a decoded and decompressed stream.
	 * @throws IOException stream and memory problems.
	 */
	public Stream decodeAndDecompress() throws IOException {
		return getDecompressed(decodeStream(this));
	}

	/**
	 * Compress and encode a current stream to another stream.
	 * @return a compressed and encoded stream.
	 * @throws IOException stream and memory problems.
	 */
	public Stream compressAndEncode() throws IOException {
		return encodeStream(getCompressed(this));
	}

	/**
	 * Get the position within the current stream.
	 * @return the current position while reading the stream.
	 */
	public synchronized long getPosition() {
		return pos;
	}

	@Override
	public synchronized int read() throws IOException {
		int b = super.read();
		if (b >= 0) {
			pos += 1;
		}
		return b;
	}

	@Override
	public synchronized int read(final byte[] b, final int off, final int len) throws IOException {
		int n = super.read(b, off, len);
		if (n > 0) {
			pos += n;
		}
		return n;
	}

	@Override
	public synchronized long skip(final long skip) throws IOException {
		long n = super.skip(skip);
		if (n > 0) {
			pos += n;
		}
		return n;
	}

	@Override
	public synchronized void mark(final int readlimit) {
		super.mark(readlimit);
		mark = pos;
	}

	@Override
	public synchronized void reset() throws IOException {
		super.reset();
		pos = mark;
	}

	/**
	 * Get the total size in bytes of the current stream.
	 * @return the total size in bytes of the current stream.
	 * @throws IOException stream and memory problems.
	 */
	public int count() throws IOException {
		return in.available();
	}

	static Stream encodeStream(final ByteArrayOutputStream stream) throws IOException {
		ByteArrayInputStream bytesInput = new ByteArrayInputStream(Base64.getEncoder().encode(stream.toByteArray()));
		return new Stream(bytesInput);
	}

	Stream encodeStream() throws IOException {
		ByteArrayOutputStream buffer = new ByteArrayOutputStream();
		reset();
		int nRead;
		final int maxByte = 16384;
		byte[] data = new byte[maxByte];

		while ((nRead = this.read(data, 0, data.length)) != -1) {
			buffer.write(data, 0, nRead);
		}

		buffer.flush();

		reset();
		return encodeStream(buffer);
	}

	static Stream getDecompressed(final byte[] data) throws IOException {
		final int fourthData = 4;
		return data.length == 0 ? new Stream(data) : isZipCompressedData(readBytes(data, fourthData))
				? decompressZipStream(data) : decompressGZipStream(data);
	}

	static byte[] decodeStream(final Stream stream) throws IOException {
		ByteArrayOutputStream buffer = new ByteArrayOutputStream();
		byte[] data = new byte[BUFFERSIZE];
		int bytesRead;
		stream.reset();
		while ((bytesRead = stream.read(data, 0, data.length)) > 0) {
			buffer.write(data, 0, bytesRead);
		}
		buffer.flush();
		stream.reset();
		return Base64.getDecoder().decode(buffer.toByteArray());
	}

	static byte[] readBytes(final byte[] data, final int count) throws IOException {
		byte[] bytes = new byte[count];
		int offset = 0;

		while (offset < count) {
			bytes[offset] = data[offset];
			offset++;
		}
		return bytes;
	}

	static boolean isZipCompressedData(final byte[] data) {
		final int lengthFour = 4;
		return data != null && data.length >= lengthFour && ByteBuffer.wrap(data).getInt() == ZIPHEADERBYTES;
	}

	static ByteArrayOutputStream getCompressed(final Stream sourceStream) throws IOException {
		ByteArrayOutputStream result = new ByteArrayOutputStream();
		GZIPOutputStream compressedStream = new GZIPOutputStream(result);
		byte[] buffer = new byte[BUFFERSIZE];
		sourceStream.reset();
		long retval = sourceStream.read(buffer, 0, BUFFERSIZE);
		while (retval > 0) {
			compressedStream.write(buffer, 0, (int) retval);
			retval = sourceStream.read(buffer, 0, BUFFERSIZE);
		}
		compressedStream.close();
		return result;
	}

	static Stream decompressGZipStream(final byte[] data) throws IOException {
		return decompress(new GZIPInputStream(new ByteArrayInputStream(data), data.length));
	}

	static Stream decompressZipStream(final byte[] data) throws IOException {
		ByteArrayOutputStream result = new ByteArrayOutputStream();
		ZipInputStream zin = new ZipInputStream(new ByteArrayInputStream(data));
		while ((zin.getNextEntry()) != null) {
			for (int c = zin.read(); c != -1; c = zin.read()) {
				result.write(c);
			}
			zin.closeEntry();
		}

		zin.close();
		return new Stream(new ByteArrayInputStream(result.toByteArray()));
	}

	static Stream decompress(final InflaterInputStream decompressedStream) throws IOException {
		ByteArrayOutputStream result = new ByteArrayOutputStream();

		byte[] buffer = new byte[BUFFERSIZE];
		long retval = decompressedStream.read(buffer, 0, BUFFERSIZE);
		while (retval == BUFFERSIZE) {
			result.write(buffer, 0, (int) retval);
			result.flush();
			retval = decompressedStream.read(buffer, 0, BUFFERSIZE);
		}

		if ((int) retval > 0) {
			result.write(buffer, 0, (int) retval);
		}
		result.flush();
		decompressedStream.close();

		return new Stream(new ByteArrayInputStream(result.toByteArray()));
	}

	/**
	 * Copy the stream content to a SOAP element of a SOAP message.
	 * @param messageElement a SOAP element of a SOAP message.
	 * @throws SOAPException SOAP issues.
	 * @throws IOException input and output issues.
	 */
	public void copyTo(final SOAPElement messageElement) throws SOAPException, IOException {
		byte[] buffer = new byte[BUFFERSIZE];
		int length;
		while ((length = this.read(buffer)) != -1) {
			messageElement.addTextNode(new String(buffer, 0, length, StandardCharsets.UTF_8));
		}
		this.reset();
	}

	@Override
	public String toString() {
		final int thirdSubString = 3;
		try {
			String result = readToEndCore().toString();
			return result.startsWith("﻿") ? result.substring(thirdSubString) : result;
		} catch (IOException e) {
			e.printStackTrace();
		}
		return "Empty";
	}

	/**
	 * Reads all characters from the current position to the end of the stream.
	 * @return The rest of the stream as a string, from the current position to the end.
	 * If the current position is at the end of the stream, returns an empty string ("").
	 * @throws IOException stream and memory problems.
	 */
	public String readToEnd() throws IOException {
		StringBuilder sb = readToEndCore();
		sb.append("\r\n");
		return sb.toString();
	}

	StringBuilder readToEndCore() throws IOException {
		StringBuilder sb = new StringBuilder();
		byte[] buffer = new byte[BUFFERSIZE];
		int length;
		while ((length = this.read(buffer)) != -1) {
			sb.append(new String(buffer, 0, length, StandardCharsets.UTF_8));
		}
		this.reset();
		return sb;
	}
}
