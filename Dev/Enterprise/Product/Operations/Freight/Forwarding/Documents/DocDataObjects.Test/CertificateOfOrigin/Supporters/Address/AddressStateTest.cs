using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Supporters.Address
{
	[TestedType(typeof(AddressState))]
	class AddressStateTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new AddressState();

		public void Test_IsSameAsExporter_And_IsUnknown_Are_MutuallyExclusive()
		{
			var addressState = new AddressState()
			{
				IsSameAsExporter = true,
				IsUnknown = true,
				ExcludeFromPDF = false
			};

			AssertEquals(false, addressState.IsSameAsExporter);
			AssertEquals(true, addressState.IsUnknown);

			addressState.IsUnknown = true;
			addressState.IsSameAsExporter = true;

			AssertEquals(false, addressState.IsUnknown);
			AssertEquals(true, addressState.IsSameAsExporter);
		}
	}
}
