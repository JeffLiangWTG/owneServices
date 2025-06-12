package com.cargowise.eservices.common;

import java.util.UUID;

/**
 * Represents a globally unique identifier (GUID).
 * @author WiseTech Global
 * @version 1.0.0
 * @since 1.5
 */
public class Guid {
	/** An instance of the Guid structure whose value is all zeros. */
	public static final Guid EMPTY = new Guid("00000000-0000-0000-0000-000000000000");

	private String uuid;

	/**
	 * Initializes a new instance of the Guid structure by using the value represented by the specified string.
	 * @param uuid A string that contains a GUID in one of the following formats ("d" represents a hexadecimal digit whose case is ignored):<br>
	 * Groups of 8, 4, 4, 4, and 12 digits with hyphens between the groups.
	 * The entire GUID can optionally be enclosed in matching braces or parentheses:<br>
	 * dddddddd-dddd-dddd-dddd-dddddddddddd
	 */
	public Guid(final String uuid) {
		this.uuid = uuid;
	}

	/**
	 * Check whether the guid is empty.("00000000-0000-0000-0000-000000000000").
	 * @param guid the checking GUID.
	 * @return whether it is empty.
	 */
	public static boolean isEmpty(final Guid guid) {
		return guid == null || guid.toString().equals(EMPTY.toString());
	}

	/**
	 * Initializes a new instance of the Guid structure.
	 * @return A new GUID object with random number.
	 */
	public static Guid newGuid() {
		return new Guid(UUID.randomUUID().toString());
	}

	/** Returns a string representation of the value of this instance in dddddddd-dddd-dddd-dddd-dddddddddddd format.
	 * @return the uuid string*/
	public String toString() {
		return uuid;
	}
/**
 * 
 */
	@Override
	public boolean equals(final Object obj) {
		if (obj != null) {
			return this.uuid.equals(obj.toString());
		}
		return false;
	}

	@Override
	public final int hashCode() {
		return this.uuid.hashCode();
	}
}
