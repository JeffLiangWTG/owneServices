using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	sealed class FileFetcherRetryFixture
	{
		[Test]
		public void TestExecute_WithNoExceptions_ShouldReturnResult()
		{
			var expectedResult = 1;
			var testAction = new TestAction<int>(new Func<int>[] { () => expectedResult });

			var actualResult = FileFetcherRetry.Execute(testAction.Execute);

			Assert.Multiple(() =>
			{
				Assert.That(testAction.InvocationCount, Is.EqualTo(1));
				Assert.That(actualResult, Is.EqualTo(expectedResult));
			});
		}

		[Test]
		public void TestExecute_WithTransientExceptions_ShouldRetryAndReturnResult()
		{
			var expectedResult = 1;
			var testAction = new TestAction<int>(new Func<int>[]
			{
				() => throw GetHandledTransientException(),
				() => throw GetHandledTransientException(),
				() => expectedResult
			});

			var actualResult = FileFetcherRetry.Execute(testAction.Execute, retryCount: 2, retryDelayInSeconds: 0);

			Assert.Multiple(() =>
			{
				Assert.That(testAction.InvocationCount, Is.EqualTo(3));
				Assert.That(actualResult, Is.EqualTo(expectedResult));
			});
		}

		[Test]
		public void TestExecute_WithPermanentExceptions_ShouldRethrowException()
		{
			var testAction = new TestAction<int>(new Func<int>[]
			{
				() => throw GetHandledTransientException(),
				() => throw GetHandledTransientException(),
				() => throw GetHandledTransientException(),
			});

			Assert.Throws<ArgumentException>(() => FileFetcherRetry.Execute(testAction.Execute, retryCount: 2, retryDelayInSeconds: 0));
			Assert.That(testAction.InvocationCount, Is.EqualTo(3));
		}

		[Test]
		public void TestExecute_WithUnhandledExceptions_ShouldRethrowException()
		{
			var testAction = new TestAction<int>(new Func<int>[]
			{
				() => throw GetHandledTransientException(),
				() => throw new InvalidOperationException()
			});

			Assert.Throws<InvalidOperationException>(() => FileFetcherRetry.Execute(testAction.Execute, retryCount: 3, retryDelayInSeconds: 0));
			Assert.That(testAction.InvocationCount, Is.EqualTo(2));
		}

		Exception GetHandledTransientException() => new ArgumentException("Cannot find last modification datetime");

		class TestAction<T>
		{
			public int InvocationCount { get; private set; }
			private readonly Func<T>[] actions;

			public TestAction(Func<T>[] actions)
			{
				this.actions = actions;
				InvocationCount = 0;
			}

			public T Execute()
			{
				InvocationCount++;
				return actions[InvocationCount - 1]();
			}
		}
	}
}
