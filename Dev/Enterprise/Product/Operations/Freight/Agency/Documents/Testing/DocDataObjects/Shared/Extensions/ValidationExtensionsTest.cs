using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class ValidationExtensionsTest : TestCaseWithFactory
	{
		public void TestIsPartyNameAndAddressEmpty()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			Assert(address.IsPartyNameAndAddressEmpty());

			address.CompanyName = "CompanyName";
			Assert(address.IsPartyNameAndAddressEmpty());

			address.AddressLine1 = "AddressLine1";
			Assert(address.IsPartyNameAndAddressEmpty());

			address.Country.Code = "CN";
			Assert(!address.IsPartyNameAndAddressEmpty());
		}

		public void TestAddValidationDependencies()
		{
			var context = new CommonContext(Factory);
			var address1 = AddressBuilder.Create(context, (OrgAddress)null);
			var address2 = AddressBuilder.Create(context, (OrgAddress)null);

			address2.CompanyNameInfo.AddMessageError(() => address1.CompanyName.IsEmpty, "Empty");

			address1.CompanyName = "CompanyName";
			AssertNoMessageError(address2.CompanyNameInfo, "Empty");

			address1.CompanyName = ZString.Empty;
			AssertNoMessageError(address2.CompanyNameInfo, "Empty");

			address2.AddValidationDependencies(address2.CompanyNameInfo, address1.CompanyNameInfo);

			address1.CompanyName = "CompanyName";
			AssertNoMessageError(address2.CompanyNameInfo, "Empty");

			address1.CompanyName = ZString.Empty;
			AssertHasMessageError(address2.CompanyNameInfo, "Empty");
		}
	}
}
