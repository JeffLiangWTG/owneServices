using System.Collections.Generic;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(LocalTransportFormCustomisationSettingsProvider))]
	public class LocalTransportFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<LocalTransportFormCustomisationSettingsProvider>
	{
		public override LocalTransportFormCustomisationSettingsProvider GetNewProvider()
		{
			return new LocalTransportFormCustomisationSettingsProvider();
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var provider = GetNewProvider();
			AssertEquals(true, ((IList<string>)provider.PropertiesThatAffectWorkflow).Contains(JobCartageSchema.JJ_GB.Name));
			AssertEquals(true, ((IList<string>)provider.PropertiesThatAffectWorkflow).Contains(JobCartageSchema.JJ_E3_NKJobType.Name));
			AssertEquals(true, ((IList<string>)provider.PropertiesThatAffectWorkflow).Contains(JobCartageSchema.JJ_OH_ClientID.Name));
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}
	}
}
