using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public abstract class BunitTestContext : TestContextWrapper
{
	[SetUp]
	public virtual void Setup() => TestContext = new Bunit.TestContext();

	[TearDown]
	public void TearDown() => TestContext?.Dispose();
}
