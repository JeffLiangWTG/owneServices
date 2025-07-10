using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialData))]
	sealed class GlbCompanyCredentialDataTest : TestCaseWithFactory
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			const string configurationName = "CFG";
			const string interchangeType = "ICT";
			var credentialApplicableInfos = new[] { DummyBusinessObject.New(Factory).Z0_CalculatedInfo };
			Func<GlbCompany, object> createCredential = (GlbCompany company) => { return null; };

			AssertExceptionThrown<ArgumentException>(() => new GlbCompanyCredentialData(null, interchangeType, credentialApplicableInfos, createCredential));
			AssertExceptionThrown<ArgumentException>(() => new GlbCompanyCredentialData(configurationName, null, credentialApplicableInfos, createCredential));
			AssertExceptionThrown<ArgumentException>(() => new GlbCompanyCredentialData(configurationName, interchangeType, null, createCredential));
			AssertExceptionThrown<ArgumentException>(() => new GlbCompanyCredentialData(configurationName, interchangeType, credentialApplicableInfos, null));
			AssertExceptionThrown<ArgumentException>(() => new GlbCompanyCredentialData(configurationName, interchangeType, Array.Empty<ZPropertyInfo>(), createCredential));

			var companyCredentialData = new GlbCompanyCredentialData(configurationName, interchangeType, credentialApplicableInfos, createCredential);

			AssertEquals("ConfigurationName", configurationName, companyCredentialData.ConfigurationName);
			AssertEquals("InterchangeTypeForSending", interchangeType, companyCredentialData.InterchangeTypeForSending);
			AssertEquals("CredentialApplicableInfos", credentialApplicableInfos, companyCredentialData.CredentialApplicableInfos);
			AssertEquals("CreateCredential", createCredential, companyCredentialData.CreateCredential);
		});
	}
}
