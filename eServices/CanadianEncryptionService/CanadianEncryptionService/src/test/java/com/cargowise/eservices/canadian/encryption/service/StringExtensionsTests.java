package com.cargowise.eservices.canadian.encryption.service;

import java.util.Collections;

import junit.framework.TestCase;

public class StringExtensionsTests extends TestCase {
	public final void testTruncateOver300Characters() {
		String context = String.join("", Collections.nCopies(500, "a"));
		assertEquals(String.join("", Collections.nCopies(300, "a")), StringExtensions.truncateForLogging(context));
	}

	public final void testNormalStringLessThan300() {
		String context = String.join(" ", Collections.nCopies(10, "Hello"));
		assertEquals(context, StringExtensions.truncateForLogging(context));
	}

	public final void testNewLine() {
		String context = "Hello\r\nWorld\r This \n is\r\nnew application.";
		assertEquals("Hello World This is new application.", StringExtensions.truncateForLogging(context));
	}
}
