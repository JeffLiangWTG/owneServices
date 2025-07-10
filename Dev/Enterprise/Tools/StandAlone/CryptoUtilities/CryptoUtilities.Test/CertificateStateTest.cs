using System;
using CargoWise.IO;
using Enterprise.CryptoUtilities;
using NUnit.Framework;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CertificateStateTest : TestCase
	{
		public void TestConstructor()
		{
			CertificateState state = new CertificateState(Certificate, Certificate.ValidToDate.AddDays(-20));
			AssertNotNull(state);
		}

		[TestTimeZone]
		public void TestErrors()
		{
			var state = new CertificateState(Certificate, Certificate.ValidToDate.AddDays(20));
			AssertEquals("ErrorCount", 1, state.Errors.Length);
			AssertEquals("Error#1", "The certificate has expired as of " + new DateTime(2004, 4, 2, 2, 34, 58) + ".", state.Errors[0]);
		}

		[TestTimeZone]
		public void TestWarnings()
		{
			var state = new CertificateState(Certificate, Certificate.ValidToDate.AddDays(-3));
			AssertEquals("WarningCount", 1, state.Warnings.Length);
			AssertEquals("Warning#1", "The certificate will shortly expire.  Please replace this certificate by " + new DateTime(2004, 4, 2, 2, 34, 58) + ".", state.Warnings[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string certificateFilename = resourceRetriever.SaveResourceToFile("Conf.cer");
				Certificate = new Certificate(certificateFilename);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			Certificate.Dispose();
		}

		Certificate Certificate { get; set; }
	}
}
