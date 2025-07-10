using Enterprise.Freight.Agency.DataTransfer.MessageProcessing.TestFiles;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	sealed class CMMEmailGeneratorTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerate()
		{
			var generator = new CMMEmailGenerator();
			generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			generator.WriteVoyageHeader(null, null, null);
			generator.WriteInfo("Vessel Info Text Line 1.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", null, null);
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, "lloyds", null);
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, null, "voyage");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", "lloyds", "voyage");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteContainerHeader("Container 1", "container 1 url");
			generator.WriteWarning("Warning Text Line 1.");
			generator.WriteInfo("Info Text Line 1.");
			generator.WriteContainerFooter();
			generator.WriteContainerHeader("Container 2", null);
			generator.WriteInfo("Info Text Line 2.");
			generator.WriteContainerFooter();
			generator.WriteSubscriptionComment(true, "Warning Subscription Comment");
			generator.WriteSubscriptionComment(false, "Info Subscription Comment");
			var ackEmail = generator.ToAckEmail();
			var warningEmail = generator.ToWarningEmail();
			AssertMultilineASCIIEquals("ackEmail.Body", CMMEmailGeneratorForTest.GetInfoSample(), ackEmail.Body);
			AssertEquals("ackEmail.ContentType", EmailContentTypes.HTML, ackEmail.ContentType);
			AssertEquals("ackEmail.Subject", "Processed TypeDescription CMM Message From SenderName - Containers Without Warnings", ackEmail.Subject);
			AssertMultilineASCIIEquals("warningEmail.Body", CMMEmailGeneratorForTest.GetWarningSample(), warningEmail.Body);
			AssertEquals("warningEmail.ContentType", EmailContentTypes.HTML, warningEmail.ContentType);
			AssertEquals("warningEmail.Subject", "Processed TypeDescription CMM Message From SenderName - Containers With Warnings", warningEmail.Subject);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerate_NoWarning()
		{
			var generator = new CMMEmailGenerator();
			generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			generator.WriteVoyageHeader(null, null, null);
			generator.WriteInfo("Vessel Info Text Line 1.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", null, null);
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, "lloyds", null);
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, null, "voyage");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", "lloyds", "voyage");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteContainerHeader("Container 2", null);
			generator.WriteInfo("Info Text Line 2.");
			generator.WriteContainerFooter();
			generator.WriteSubscriptionComment(true, "Warning Subscription Comment");
			generator.WriteSubscriptionComment(false, "Info Subscription Comment");
			var ackEmail = generator.ToAckEmail();
			var warningEmail = generator.ToWarningEmail();
			AssertMultilineASCIIEquals("ackEmail.Body", CMMEmailGeneratorForTest.GetInfoSample(), ackEmail.Body);
			AssertEquals("ackEmail.ContentType", EmailContentTypes.HTML, ackEmail.ContentType);
			AssertEquals("ackEmail.Subject", "Processed TypeDescription CMM Message From SenderName - Containers Without Warnings", ackEmail.Subject);
			AssertEquals("warningEmail", null, warningEmail);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerate_AllWarning()
		{
			var generator = new CMMEmailGenerator();
			generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			generator.WriteVoyageHeader(null, null, null);
			generator.WriteInfo("Vessel Info Text Line 1.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", null, null);
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, "lloyds", null);
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, null, "voyage");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", "lloyds", "voyage");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteContainerHeader("Container 1", "container 1 url");
			generator.WriteWarning("Warning Text Line 1.");
			generator.WriteInfo("Info Text Line 1.");
			generator.WriteContainerFooter();
			generator.WriteSubscriptionComment(true, "Warning Subscription Comment");
			generator.WriteSubscriptionComment(false, "Info Subscription Comment");
			var ackEmail = generator.ToAckEmail();
			var warningEmail = generator.ToWarningEmail();
			AssertEquals("ackEmail", null, ackEmail);
			AssertMultilineASCIIEquals("warningEmail.Body", CMMEmailGeneratorForTest.GetWarningSample(), warningEmail.Body);
			AssertEquals("warningEmail.ContentType", EmailContentTypes.HTML, warningEmail.ContentType);
			AssertEquals("warningEmail.Subject", "Processed TypeDescription CMM Message From SenderName - Containers With Warnings", warningEmail.Subject);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerate_VoyageWarning()
		{
			var generator = new CMMEmailGenerator();
			generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			generator.WriteVoyageHeader(null, null, null);
			generator.WriteWarning("Vessel Warning Text Line 1.");
			generator.WriteInfo("Vessel Info Text Line 1.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", null, null);
			generator.WriteWarning("Vessel Warning Text Line 2.");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, "lloyds", null);
			generator.WriteWarning("Vessel Warning Text Line 2.");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader(null, "voyage", null);
			generator.WriteWarning("Vessel Warning Text Line 2.");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteVoyageHeader("vessel", "lloyds", "voyage");
			generator.WriteWarning("Vessel Warning Text Line 2.");
			generator.WriteInfo("Vessel Info Text Line 2.");
			generator.WriteVoyageFooter();
			generator.WriteContainerHeader("Container 2", null);
			generator.WriteInfo("Info Text Line 2.");
			generator.WriteContainerFooter();
			generator.WriteContainerHeader("Container 1", "container 1 url");
			generator.WriteWarning("Warning Text Line 1.");
			generator.WriteInfo("Info Text Line 1.");
			generator.WriteContainerFooter();
			generator.WriteSubscriptionComment(true, "Warning Subscription Comment");
			generator.WriteSubscriptionComment(false, "Info Subscription Comment");
			var ackEmail = generator.ToAckEmail();
			var warningEmail = generator.ToWarningEmail();
			AssertEquals("ackEmail", null, ackEmail);
			AssertMultilineASCIIEquals("warningEmail.Body", CMMEmailGeneratorForTest.GetSample(), warningEmail.Body);
			AssertEquals("warningEmail.ContentType", EmailContentTypes.HTML, warningEmail.ContentType);
			AssertEquals("warningEmail.Subject", "Processed TypeDescription CMM Message From SenderName", warningEmail.Subject);
		}

		static class CMMEmailGeneratorForTest
		{
			const string prefix = "CMMEmailGenerator\\";
			public static string GetInfoSample()
			{
				return TestFileHelper.GetText(prefix + "InfoSample.htm");
			}

			public static string GetSample()
			{
				return TestFileHelper.GetText(prefix + "Sample.htm");
			}

			public static string GetWarningSample()
			{
				return TestFileHelper.GetText(prefix + "WarningSample.htm");
			}
		}
	}
}
