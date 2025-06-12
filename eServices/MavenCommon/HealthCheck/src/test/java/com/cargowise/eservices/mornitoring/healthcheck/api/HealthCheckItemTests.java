package com.cargowise.eservices.mornitoring.healthcheck.api;

import static org.junit.Assert.assertEquals;

import org.junit.Test;

/**
 * HealthCheckItem tests.
 */
public class HealthCheckItemTests {
	@Test
	public void healthCheckItemTest() {
		HealthCheckItem checkItem = HealthCheckItem.info("description");
		assertEquals(checkItem.getStatus(), HealthCheckStatus.OK);
		assertEquals(checkItem.getDescrption(), "description");

		checkItem = HealthCheckItem.error("description");
		assertEquals(checkItem.getStatus(), HealthCheckStatus.ERROR);
		assertEquals(checkItem.getDescrption(), "description");

		checkItem = HealthCheckItem.warning("description");
		assertEquals(checkItem.getStatus(), HealthCheckStatus.WARNING);
		assertEquals(checkItem.getDescrption(), "description");
	}
}
