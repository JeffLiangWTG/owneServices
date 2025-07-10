using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCarrierCombinedCollectionProvider))]
	public class USCarrierCombinedCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(USCarrierCombinedCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.US.Carrier;

		protected override int ExpectedMaxLength => USCarrierCombinedSchema.UI_Code.MaxLength;
	}
}
