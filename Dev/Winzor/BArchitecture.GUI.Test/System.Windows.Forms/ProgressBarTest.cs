using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class ProgressBarTest
{
	[Test]
	public async Task CheckProgressBarBeingRendered()
	{
		using var ctx = new WinzorTestContext();
		ProgressBar progressBar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
			};
			return progressBar;
		});
		Assert.That(rendered.Find(".progressbar"), Is.Not.Null);
	}

	[Test]
	public async Task CheckValueIsBeingIncrementedByStepSize()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
				Value = 50,
				Step = 10
			};
			progressBar.PerformStep();
			Assert.That(progressBar.Value, Is.EqualTo(60));

			progressBar.Step = 20;
			progressBar.PerformStep();
			Assert.That(progressBar.Value, Is.EqualTo(80));
		});
	}

	[Test]
	public async Task CheckValueSetToMaximumIfGreaterThanMaximum()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
				Value = 90,
			};
			progressBar.Maximum = 80;
			Assert.That(progressBar.Value, Is.EqualTo(80));
		});
	}

	[Test]
	public async Task CheckValueSetToMinimumIfLesserThanMinimum()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
				Value = 20,
			};
			progressBar.Minimum = 30;
			Assert.That(progressBar.Value, Is.EqualTo(30));
		});
	}

	[Test]
	public async Task CheckMinimumNeverGreaterThanMaximum()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
			};

			progressBar.Minimum = 30;
			progressBar.Maximum = 20;
			Assert.That(progressBar.Minimum, Is.EqualTo(progressBar.Maximum));

			progressBar.Maximum = 20;
			progressBar.Minimum = 30;
			Assert.That(progressBar.Maximum, Is.EqualTo(progressBar.Minimum));
		});
	}

	[Test]
	public async Task CheckValueOutsideOfMinAndMaxThrowsException()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
				Maximum = 100,
				Minimum = 50
			};
			Assert.Throws<ArgumentOutOfRangeException>(() => progressBar.Value = 40);
			Assert.Throws<ArgumentOutOfRangeException>(() => progressBar.Value = 110);
		});
	}

	[TestCase(40, 100, 40)]
	[TestCase(40, 120, 33)]
	[TestCase(40, 60, 66)]
	[TestCase(40, 30, 100)]
	public async Task CheckProgressBarWidthStyleSetCorrectly(int value, int maximum, int percentage)
	{
		using var ctx = new WinzorTestContext();
		ProgressBar progressBar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
				Value = value,
				Maximum = maximum
			};
			return progressBar;
		});
		Assert.That(rendered.Find(".progressbar-filled").GetAttribute("style"), Does.Contain($"width:{percentage}%;"));
	}

	[Test]
	public async Task CheckPerformStepTriggersRender()
	{
		using var ctx = new WinzorTestContext();
		ProgressBar progressBar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
				Value = 40,
				Step = 10,
			};
			return progressBar;
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));

		await progressBar.InvokeWinzorDispatcherAsync(() => progressBar.PerformStep());
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task CheckMinimumAndMaximumHavingNegativeValueThrowsException()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
			};
			Assert.Throws<ArgumentOutOfRangeException>(() => progressBar.Minimum = -10);
			Assert.Throws<ArgumentOutOfRangeException>(() => progressBar.Maximum = -10);
		});
	}

	[Test]
	public async Task CheckPerformStepDoesNotSetValueOutsideRangeOfMinimumAndMaximum()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var progressBar = new ProgressBar()
			{
				Width = 50,
				Height = 50,
				Minimum = 30,
				Maximum = 50,
				Value = 40,
				Step = 30
			};
			progressBar.PerformStep();
			Assert.That(progressBar.Value, Is.EqualTo(progressBar.Maximum));
			progressBar.Step = -30;
			progressBar.PerformStep();
			Assert.That(progressBar.Value, Is.EqualTo(progressBar.Minimum));
		});
	}
}
