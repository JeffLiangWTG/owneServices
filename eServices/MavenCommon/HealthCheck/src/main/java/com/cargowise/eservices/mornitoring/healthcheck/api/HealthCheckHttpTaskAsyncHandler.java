package com.cargowise.eservices.mornitoring.healthcheck.api;

import java.lang.reflect.Array;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.stream.Collectors;

import javax.ws.rs.GET;
import javax.ws.rs.container.AsyncResponse;
import javax.ws.rs.container.Suspended;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.ResponseBuilder;

import org.glassfish.jersey.server.ManagedAsync;

/**
 * Health check handler.
 */
public class HealthCheckHttpTaskAsyncHandler {

	private final transient IHealthCheckItemProvider[] providers;

	/**
	 * Health check handler constructor.
	 * 
	 * @param providers health check providers
	 */
	public HealthCheckHttpTaskAsyncHandler(final IHealthCheckItemProvider[] providers) { // NOPMD use Array.
		if (providers == null) {
			this.providers = new IHealthCheckItemProvider[0];
		} else {
			this.providers = Arrays.copyOf(providers, providers.length);
		}
	}

	/**
	 * Process health check HTTP request.
	 * @param asyncResponse Asynchronous resposne
	 * @throws Exception Exception
	 */
	@GET
	@ManagedAsync
	public void processRequestAsync(@Suspended final AsyncResponse asyncResponse) throws Exception { //NOPMD throw raw Exception
		ResponseBuilder responseBuilder;

		responseBuilder = Response.ok();
		responseBuilder.header("Content-Type", "text/plain;charset=utf-8");
		responseBuilder.header("Cache-Control", "no-cache");
		responseBuilder.entity(getDocumentText());

		asyncResponse.resume(responseBuilder.build());
	}

	@SuppressWarnings("unchecked")
	private String getDocumentText() throws Exception { //NOPMD throw raw Exception
		final List<CompletableFuture<String>> providerFutures = Arrays.stream(this.providers)
				.map(provider -> executeHealthCheckAndBuildLineAsync(provider)).collect(Collectors.toList());

		final CompletableFuture<Void> allFutures = CompletableFuture.allOf(
			providerFutures.toArray((CompletableFuture<String>[]) Array.newInstance(CompletableFuture.class, providerFutures.size())));

		final CompletableFuture<List<String>> linesFuture = allFutures.thenApply(v -> {
			return providerFutures.stream().map(providerFuture -> providerFuture.join()).collect(Collectors.toList());
		});

		final List<String> lines = linesFuture.get();
		return String.join(System.lineSeparator(), lines);
	}

	private CompletableFuture<String> executeHealthCheckAndBuildLineAsync(final IHealthCheckItemProvider provider) {
		return CompletableFuture.supplyAsync(() -> {
			try {
				final CompletableFuture<HealthCheckItem> itemFuture = provider.checkHealthAsync();
				final HealthCheckItem item = itemFuture.get();
				return String.format("%s(%s): %s", getCategory(item.getStatus()), provider.getName(),
						item.getDescrption());
			} catch (Exception e) { //NOPMD catch raw Exception
				return e.getMessage();
			}
		});
	}

	private static String getCategory(final HealthCheckStatus status) throws IllegalArgumentException {
		switch (status) {
			case ERROR:
				return "ERROR";

			case OK:
				return "INFO";

			case WARNING:
				return "WARNING";

			default:
				throw new IllegalArgumentException(String.format("Unknown %s with value '%s' was provided.",
						HealthCheckStatus.class.getName(), status.name()));
		}
	}
}
