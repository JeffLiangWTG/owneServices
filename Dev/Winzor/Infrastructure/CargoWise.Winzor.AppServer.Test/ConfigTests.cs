using CargoWise.Blazor.Common;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using static CargoWise.BrandManager.BrandingFactory;

namespace CargoWise.Blazor.AppServer.Test
{
	[TestFixture]
	public class ConfigTests
	{
		[Test]
		public void TestConfigIsSet()
		{
			var cargwiseOptions = new CargoWiseOptions();
			ConfigurationBinder.Bind(new TestConfiguration(), "CargoWiseOptions", cargwiseOptions);

			Assert.That(cargwiseOptions.DatabaseName, Is.Not.Empty);
			Assert.That(cargwiseOptions.DbServerName, Is.Not.Empty);
		}
	}
}
