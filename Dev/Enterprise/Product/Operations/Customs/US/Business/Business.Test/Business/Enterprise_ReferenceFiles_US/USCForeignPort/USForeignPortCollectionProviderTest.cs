using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USForeignPortCollectionProvider))]
	public class USForeignPortCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(USCForeignPortCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.US.ForeignPort;

		protected override int ExpectedMaxLength => 5;
	}
}
