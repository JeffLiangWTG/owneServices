using System;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GoodsLocationCollectionProvider))]
	sealed class GoodsLocationCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ZZRefCusCodeListCombinedCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		protected override int ExpectedMaxLength => ZZRefCusCodeListSchema.ZZD_Code.MaxLength;
	}
}
