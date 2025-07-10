using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.NL.ServiceTasks.Testing;

[TestedType(typeof(MessagingServiceTask))]
sealed class MessagingServiceTaskTest : TestCaseWithFactory
{
	public void TestHostedServiceRequirement()
	{
		var hostedServiceRequirementMethod = typeof(MessagingServiceTask).GetMethods().FirstOrDefault(p => Attribute.IsDefined(p, typeof(HostedServiceRequirementAttribute)));
		AssertNotNull("Method with Attribute HostedServiceRequirement should exist on MessagingServiceTask", hostedServiceRequirementMethod);

		var nlCompany = Factory.NewWithValidTestData<GlbCompany>();
		nlCompany.GC_RN_NKCountryCode = "NL";
		nlCompany.GC_IsActive = true;
		var nlBranch = Factory.NewWithValidTestData<GlbBranch>();
		nlBranch.GB_GC = nlCompany.PK;
		nlBranch.GB_RN_NKCountryCode = "NL";
		nlBranch.GB_IsActive = true;
		Factory.Save();

		CombineAssertions(() =>
		{
			var combinations = new List<(string submissionType, bool requiredExpected)> { ("BLT", true), ("BTH", true), ("BIT", true), ("ITF", false) };
			foreach (var combination in combinations)
			{
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(nlCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { SubmissionType = combination.submissionType }))
				{
					AssertEquals($"Registry Setting Submission Type: {combination.submissionType}", combination.requiredExpected ? string.Empty : "There is no BLT configured in the Registry.", hostedServiceRequirementMethod.Invoke(null, null));
				}
			}
		});
	}
}
