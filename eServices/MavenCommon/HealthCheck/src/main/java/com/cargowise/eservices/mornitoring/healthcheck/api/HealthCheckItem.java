package com.cargowise.eservices.mornitoring.healthcheck.api;

/**
 * Health Check Item.
 */
public class HealthCheckItem {

	private final transient String description;
	private final transient HealthCheckStatus status;

	/**
	 * Initialize Health Check Item.
	 * @param status health check' status
	 * @param description description
	 */
	public HealthCheckItem(final HealthCheckStatus status, final String description) {
		this.status = status;
		this.description = description;
	}

	/**
	 * {@inheritDoc}
	 */
	public String getDescrption() {
		return this.description;
	}

	/**
	 * {@inheritDoc}
	 */
	public HealthCheckStatus getStatus() {
		return this.status;
	}

	/**
	 * Return OK result.
	 * @param description health check's description
	 * @return HealthCheckItem
	 */
	public static HealthCheckItem info(final String description) {
		return new HealthCheckItem(HealthCheckStatus.OK, description);
	}

	/**
	 * Return warning result.
	 * @param description health check description
	 * @return HealthCheckItem
	 */
	public static HealthCheckItem warning(final String description) {
		return new HealthCheckItem(HealthCheckStatus.WARNING, description);
	}

	/**
	 * Return error result.
	 * @param description health check description
	 * @return HealthCheckItem
	 */
	public static HealthCheckItem error(final String description) {
		return new HealthCheckItem(HealthCheckStatus.ERROR, description);
	}
}
