using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusOutturnHeader))]
	sealed class CusOutturnHeaderWorkflowProviderTest : WorkflowProviderTest<CusOutturnHeader, ProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.SeaCargoOutturnWorkflowDescriptorCode;

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => nameof(CusOutturnHeader.C6_ResponsiblePartyID);

		protected override void SetUp()
		{
			base.SetUp();
			// For now only AU use CusOutturnHeader
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
		}
		IDisposable countrySetter;

		protected override void TearDown()
		{
			countrySetter?.Dispose();
			countrySetter = null;
			base.TearDown();
		}
	}
}
