using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ModuleEntryHeaderCollectionProvider))]
	sealed class ModuleEntryHeaderCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ModuleEntryHeaderCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.EntryHeader;
	}
}
