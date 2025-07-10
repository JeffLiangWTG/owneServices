using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OnHoldTermsRegistryItem))]
	sealed class OnHoldTermsRegistryItemTest : StronglyTypedRegistryItemTestCase<OnHoldTerms>
	{
		protected override StronglyTypedRegistryItem<OnHoldTerms, OnHoldTerms> GetNewRegistryItem()
		{
			return new OnHoldTermsRegistryItem("", null, null, null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
		}
	}
}
