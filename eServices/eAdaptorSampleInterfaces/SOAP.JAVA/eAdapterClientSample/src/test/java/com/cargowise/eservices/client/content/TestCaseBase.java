package com.cargowise.eservices.client.content;

import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;

import org.junit.Test;
import org.mockito.internal.configuration.GlobalConfiguration;
import org.mockito.internal.progress.ThreadSafeMockingProgress;
import org.powermock.api.support.ClassLoaderUtil;
import org.powermock.core.MockRepository;
import org.powermock.reflect.Whitebox;

import com.cargowise.eservices.common.ServicePointManager;

import junit.framework.TestCase;

public abstract class TestCaseBase extends TestCase {
	protected OutputStream output;

	public TestCaseBase() {
		super();
		MockRepository.addAfterMethodRunner(new MockitoStateCleaner());
	}

	@Test
	protected void setUp() throws Exception {
		output = new OutputStream() {
			private StringBuilder string = new StringBuilder();
			@Override
			public void write(final int b) throws IOException {
				this.string.append((char) b);
			}

			public String toString() {
				return this.string.toString();
			}
		};

		System.setOut(new PrintStream(output));
		ServicePointManager.setServerCertificateValidationCallback(() -> { });
	}

	@Test
	protected void assertException(final com.cargowise.eservices.common.Action action, final Class<?> exceptionType, final String exceptionMessage) {
		try {
			action.run();
		} catch (Exception ex) {
			assertEquals(exceptionType, ex.getClass());
			assertEquals(exceptionMessage, ex.getMessage());
			return;
		}

		fail(String.format("Expected exception with type %1$s, but no exception occured", exceptionType.toString()));
	}

	private static class MockitoStateCleaner implements Runnable {
		public void run() {
			clearMockProgress();
			clearConfiguration();
		}

		private void clearMockProgress() {
			clearThreadLocalIn(ThreadSafeMockingProgress.class);
		}

		private void clearConfiguration() {
			clearThreadLocalIn(GlobalConfiguration.class);
		}

		@SuppressWarnings("unchecked")
		private void clearThreadLocalIn(final Class<?> cls) {
			Whitebox.getInternalState(cls, ThreadLocal.class).set(null);
			final Class<?> clazz = ClassLoaderUtil.loadClass(cls, ClassLoader.getSystemClassLoader());
			Whitebox.getInternalState(clazz, ThreadLocal.class).set(null);
		}
	}
}
