using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationCollectionProvider))]
	class JobDeclarationCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(BaseJobDeclarationCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.JobDeclaration;

		protected override int ExpectedMaxLength => JobDeclarationSchema.JE_DeclarationReference.MaxLength;
	}
}
