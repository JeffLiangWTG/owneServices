using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentReasonRegistryItem))]
	sealed class EPaymentReasonRegistryItemTest : StronglyTypedRegistryItemTestCase<EPaymentReasonCollection>
	{
		protected override StronglyTypedRegistryItem<EPaymentReasonCollection, EPaymentReasonCollection> GetNewRegistryItem()
		{
			return new EPaymentReasonRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
