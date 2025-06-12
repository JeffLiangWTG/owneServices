package com.cargowise.eservices.mornitoring.healthcheck.api;

import static org.junit.Assert.assertEquals;

import java.util.concurrent.CompletableFuture;

import javax.ws.rs.container.AsyncResponse;
import javax.ws.rs.core.MultivaluedMap;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.Response.Status;

import org.junit.Ignore;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Captor;
import org.mockito.Mock;
import org.mockito.Mockito;
import org.mockito.runners.MockitoJUnitRunner;

/**
 * Unit test for HealthCheckHttpTaskAsyncHandler.
 */
@RunWith(MockitoJUnitRunner.class)
public class HealthCheckHttpTaskAsyncHandlerTests {

	@Mock
	private AsyncResponse asyncResponse;

	@Captor
	private ArgumentCaptor<Response> captorReponse;

	@Test
	public void buildHealthCheckDocumentMultipleProviders() throws Exception {
		IHealthCheckItemProvider[] providers =
			new IHealthCheckItemProvider[]{
				new HealthCheckItemProviderOne(), new HealthCheckItemProviderTwo(), new HealthCheckItemProviderThree()
			};
		HealthCheckHttpTaskAsyncHandler handler = new HealthCheckHttpTaskAsyncHandler(providers);

		handler.processRequestAsync(this.asyncResponse);
		Mockito.verify(asyncResponse).resume(this.captorReponse.capture());
		Response response = captorReponse.getValue();

		MultivaluedMap<String, Object> map = response.getMetadata();
		String[] expectedLines = new String[]{
			"INFO(Provider One): Service One is alive.",
			"ERROR(Provider Two): Service Two is down.",
			"WARNING(Provider Three): Service Three is running slow."
		};
		assertEquals(String.join(System.lineSeparator(), expectedLines), response.getEntity());
		assertEquals("no-cache", map.get("Cache-Control").get(0));
		assertEquals("text/plain;charset=utf-8", map.get("Content-Type").get(0));
		assertEquals(Status.OK.getStatusCode(), response.getStatus());
	}

	/**
	 * Sample health check provider.
	 */
	@Ignore
	public static class HealthCheckItemProviderOne implements IHealthCheckItemProvider {

		/**
		 * {@inheritDoc}
		 */
		@Override
		public String getName() {
			return "Provider One";
		}

		/**
		 * {@inheritDoc}
		 */
		@Override
		public CompletableFuture<HealthCheckItem> checkHealthAsync() {
			return CompletableFuture.supplyAsync(() -> {
				return HealthCheckItem.info("Service One is alive.");
			});
		}
	}

	/**
	 * Sample health check provider.
	 */
	@Ignore
	public static class HealthCheckItemProviderTwo implements IHealthCheckItemProvider {

		/**
		 * {@inheritDoc}
		 */
		@Override
		public String getName() {
			return "Provider Two";
		}

		/**
		 * {@inheritDoc}
		 */
		@Override
		public CompletableFuture<HealthCheckItem> checkHealthAsync() {
			return CompletableFuture.supplyAsync(() -> {
				return HealthCheckItem.error("Service Two is down.");
			});
		}
	}

	/**
	 * Sample health check provider.
	 */
	@Ignore
	public static class HealthCheckItemProviderThree implements IHealthCheckItemProvider {

		/**
		 * {@inheritDoc}
		 */
		@Override
		public String getName() {
			return "Provider Three";
		}

		/**
		 * {@inheritDoc}
		 */
		@Override
		public CompletableFuture<HealthCheckItem> checkHealthAsync() {
			return CompletableFuture.supplyAsync(() -> {
				return HealthCheckItem.warning("Service Three is running slow.");
			});
		}
	}
}
