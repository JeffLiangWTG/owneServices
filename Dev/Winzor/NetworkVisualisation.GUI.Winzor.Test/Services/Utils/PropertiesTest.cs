using CargoWise.NetworkVisualisation.GUI.Services.Utils;

namespace NetworkVisualisation.GUI.Winzor.Test.Services.Utils;

public class PropertiesTest
{
	[TestCase(0.000000001, 0.00000001, true)]
	[TestCase(0.00000001, 0.0000001, false)]
	public void TestEquivalent(object a, object b, bool expected)
	{
		Assert.That(Properties.Equivalent(a, b), Is.EqualTo(expected));
	}

	[TestCase(0.000000001, 0.00000001)]
	public void TestTryUpdate_WhenEquivalents(object newValue, object currentValue)
	{
		object? valueAfterUpdate = null;

		Assert.That(Properties.TryUpdate(newValue, currentValue, (value) => valueAfterUpdate = value), Is.False);
		Assert.That(valueAfterUpdate, Is.Null);
	}

	[TestCase(0.00000001, 0.0000001)]
	public void TestTryUpdate_WhenNotEquivalents(object newValue, object currentValue)
	{
		object? valueAfterUpdate = null;

		Assert.That(Properties.TryUpdate(newValue, currentValue, (value) => valueAfterUpdate = value), Is.True);
		Assert.That(valueAfterUpdate, Is.EqualTo(newValue));
		Assert.That(valueAfterUpdate, Is.Not.EqualTo(currentValue));
	}
}
