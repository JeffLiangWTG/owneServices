package com.cargowise.eservices.mornitoring.healthcheck.api;

import java.util.concurrent.CompletableFuture;

/**
 * Health check item provider interface.
 */
public interface IHealthCheckItemProvider {

	/**
	 * @return name of health check provider
	 */
	String getName();

	/**
	 * 
	 * @return HealthCheckItem
	 */
	CompletableFuture<HealthCheckItem> checkHealthAsync();
}
