using System;
using System.Globalization;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests.Services;

[TestFixture]
public class CertificateConfigTests
{
	[Test]
	public void TestCertificateHasExpired()
	{
		var cert = CertificateConfig.GetCertificate();
		var date = Convert.ToDateTime(cert.GetExpirationDateString(), new CultureInfo("es-ES", false));
		var timeLeft = date - DateTime.UtcNow;
		Assert.That(30, Is.LessThan(timeLeft.TotalDays));
	}
}
