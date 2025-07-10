using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WTG.RtfConverter.Html;

namespace Enterprise.Winzor.Architecture.Test;

public class EntryPointPoolTest
{
	[Test]
	public async Task TestOnEnteringPoolReaderIsReleasedWhenFormWritten()
	{
		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices();
		var options = Options.Create(new EntryPointPoolOptions());
		var formQueue = new OpeningFormQueue();
		Form form = null;

		using var winzorDispatcher = serviceProvider.GetRequiredService<WinzorDispatcher>();

		await winzorDispatcher.InvokeAsync(() =>
		{
			form = new Form();
		});

		using (form)
		{
			var pool = new EntryPointPool(options, formQueue, NullLogger<EntryPointPool>.Instance);
			var uri = new Uri("https://localhost");
			var windowService = new Mock<IWindowService>();

			var task = pool.EnterAsync(uri, windowService.Object);

			Assert.That(() => task.IsCompleted, Is.False.After(100));

			formQueue.Write(form);

			var formFromChannel = await task;

			Assert.That(formFromChannel, Is.EqualTo(form));
		}
	}

	[TestCase(1u)]
	[TestCase(2u)]
	[TestCase(3u)]
	public void TestCheckPoolFillsPoolWhenPoolIsNotFull(uint poolSize)
	{
		var options = Options.Create(new EntryPointPoolOptions
		{
			MinimumPoolSize = poolSize
		});
		var formQueue = new OpeningFormQueue();
		var windowService = new Mock<IWindowService>();
		var pool = new EntryPointPool(options, formQueue, NullLogger<EntryPointPool>.Instance);
		var baseDomain = "https://localhost";
		var poolUrl = $"{baseDomain}/pool";
		pool.CheckPool(new Uri(baseDomain), windowService.Object);

		windowService.Verify(w => w.RequestCreateHiddenWindowAsync(
			It.Is<CreateWindowOptions>(o => o.Uri.ToString().StartsWith(poolUrl) && Guid.Parse(o.Uri.AbsolutePath.Substring(6)) != Guid.Empty)),
			Times.Exactly((int)poolSize));
	}

	[Test]
	public void TestCheckPoolUsesAUniqueAddressPerWindow()
	{
		var options = Options.Create(new EntryPointPoolOptions
		{
			MinimumPoolSize = 3
		});
		var formQueue = new OpeningFormQueue();
		var windowService = new Mock<IWindowService>();
		var pool = new EntryPointPool(options, formQueue, NullLogger<EntryPointPool>.Instance);
		var baseDomain = "https://localhost";
		var poolUrl = $"{baseDomain}/pool";
		pool.CheckPool(new Uri(baseDomain), windowService.Object);

		Assert.That(() => windowService.Invocations.Count, Is.EqualTo(3).After(100, 1));

		var uniqueIds = windowService.Invocations
			.Select(i => (CreateWindowOptions)i.Arguments[0])
			.Select(o => Guid.Parse(o.Uri.AbsolutePath.Substring(6)))
			.ToList();

		Assert.That(uniqueIds.Count, Is.EqualTo(3));
		Assert.That(uniqueIds, Is.Unique);
		Assert.That(uniqueIds, Has.All.Matches<Guid>(id => id != Guid.Empty));
	}

	[Test]
	public void TestCheckPoolDoesNotReattemptLaunchUntilRegistrationTimeoutExpires()
	{
		var options = Options.Create(new EntryPointPoolOptions());
		var queue = new OpeningFormQueue();
		var windowService = new Mock<IWindowService>();
		var pool = new EntryPointPool(options, queue, NullLogger<EntryPointPool>.Instance);
		var baseDomain = "https://localhost";
		pool.CheckPool(new Uri(baseDomain), windowService.Object);

		Assert.That(() => windowService.Invocations.Count, Is.EqualTo(options.Value.MinimumPoolSize).After(1000, 10));

		windowService.Verify(w => w.RequestCreateHiddenWindowAsync(It.IsAny<CreateWindowOptions>()), Times.Exactly((int)options.Value.MinimumPoolSize));
		windowService.Reset();

		pool.CheckPool(new Uri(baseDomain), windowService.Object);

		Assert.That(() => windowService.Invocations.Any(), Is.False.After((int)options.Value.PoolFillWaitPeriod.TotalMilliseconds));

		pool.CheckPool(new Uri(baseDomain), windowService.Object);

		Assert.That(() => windowService.Invocations.Count, Is.EqualTo(options.Value.MinimumPoolSize).After(1000, 10));

		windowService.Verify(w => w.RequestCreateHiddenWindowAsync(It.IsAny<CreateWindowOptions>()), Times.Exactly((int)options.Value.MinimumPoolSize));
	}

	[Test]
	public void TestCheckPoolDoesNotCallRequestNewWindowAsyncWhenPoolIsFull()
	{
		var options = Options.Create(new EntryPointPoolOptions { MinimumPoolSize = 3 });
		var formQueue = new OpeningFormQueue();
		var windowService = new Mock<IWindowService>();
		var pool = new EntryPointPool(options, formQueue, NullLogger<EntryPointPool>.Instance);
		var baseDomain = new Uri("https://localhost");

		// These three calls with run synchronously until we hit the await
		// So we know we'll increment the counter internally by the time we hit CheckPoolAsync
		_ = pool.EnterAsync(baseDomain, windowService.Object);
		_ = pool.EnterAsync(baseDomain, windowService.Object);
		_ = pool.EnterAsync(baseDomain, windowService.Object);

		Assert.That(() => windowService.Invocations.Count, Is.EqualTo(2).After(100, 1));

		windowService.Reset();

		pool.CheckPool(baseDomain, windowService.Object);
		Assert.That(() => windowService.Invocations.Count, Is.EqualTo(0).After(100));
		windowService.Verify(w => w.RequestCreateHiddenWindowAsync(It.IsAny<CreateWindowOptions>()), Times.Never);
	}

	[Test]
	public async Task TestEnterDecrementsCountEvenIfExceptionThrownAsync()
	{
		var options = Options.Create(new EntryPointPoolOptions() { MinimumPoolSize = 3, PoolFillWaitPeriod = TimeSpan.FromMilliseconds(0) });

		var formQueue = new Mock<IOpeningFormQueue>();
		formQueue.Setup(f => f.ReadAsync(default)).ThrowsAsync(new Exception());
		var windowService = new Mock<IWindowService>();
		var pool = new EntryPointPool(options, formQueue.Object, NullLogger<EntryPointPool>.Instance);

		Assert.That(pool.PoolSize, Is.Zero);

		var baseDomain = new Uri("https://localhost");

		await Assert.ThatAsync(async () => await pool.EnterAsync(baseDomain, windowService.Object), Throws.Exception);

		Assert.That(() => pool.PoolSize, Is.EqualTo(0).After(1000, 10));

		Assert.That(() => windowService.Invocations.Count, Is.EqualTo(5).After(1000, 10));

		windowService.Verify(w => w.RequestCreateHiddenWindowAsync(It.IsAny<CreateWindowOptions>()), Times.Exactly(5));
	}

	[Test]
	public void ExceptionsThrownByCheckPoolAreLogged()
	{
		var options = Options.Create(new EntryPointPoolOptions() { MinimumPoolSize = 3, PoolFillWaitPeriod = TimeSpan.FromMilliseconds(0) });
		var logger = new Mock<ILogger<EntryPointPool>>();
		var windowService = new Mock<IWindowService>();
		windowService
			.Setup(w => w.RequestCreateHiddenWindowAsync(It.IsAny<CreateWindowOptions>()))
			.ThrowsAsync(new Exception());

		var pool = new EntryPointPool(options, new OpeningFormQueue(), logger.Object);

		pool.CheckPool(new Uri("https://example.com"), windowService.Object);

		Assert.That(() => logger.Invocations.Count, Is.EqualTo(3).After(100, 1));

		logger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((_, _) => true), It.IsAny<AggregateException>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(3));
	}
}
