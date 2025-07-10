using System;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Reports.Testing
{
	[TestedType(typeof(USCFDAProductNumberCollectionProvider))]
	sealed class USCFDAProductNumberCollectionProviderTest : DocumentEngine.RuntimeOptions.Testing.CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ZZRefCusCodeListCombinedCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;
	}
}
