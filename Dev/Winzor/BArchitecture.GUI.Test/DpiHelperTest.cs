using System.Windows.Forms;
using NUnit.Framework;

namespace WinzorFramework.Test;

public class DpiHelperTest
{
	[Test]
	public void DpiHelperIsScalingRequirementMetIsFalse()
	{
		Assert.That(DpiHelper.IsScalingRequirementMet, Is.False);
	}
}
