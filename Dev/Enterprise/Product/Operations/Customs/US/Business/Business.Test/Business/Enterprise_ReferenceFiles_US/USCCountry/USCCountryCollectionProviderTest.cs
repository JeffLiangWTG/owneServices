using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCCountryCollectionProvider))]
	public class USCCountryCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(USCCountryCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.US.Country;

		protected override int ExpectedMaxLength => USCCountrySchema.UC_Code.MaxLength;
	}
}
