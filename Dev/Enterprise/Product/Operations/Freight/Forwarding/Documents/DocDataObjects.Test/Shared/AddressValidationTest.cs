using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class AddressValidationTest : TestCaseWithFactory
	{
		public void TestCompanyName()
		{
			var address = new Address(Factory);
			address.CompanyNameInfo.AddErrorIfEmpty();
			address.ValidateAll();

			var dynamicAddress = address.MakeDynamic(validationProvider: new DocDataObjectValidationProvider(), dynamicDataFactory: new DocDataObjectDynamicDataFactory());
			var companyName = dynamicAddress.GetDynamicProperty(nameof(address.CompanyName));

			AssertMultilineASCIIEquals("",
				"Value is required.",
				companyName.ToFormatString());

			address.CompanyName = "Fudger Ltd.";

			dynamicAddress = address.MakeDynamic(validationProvider: new DocDataObjectValidationProvider());
			dynamicAddress.Validate();

			companyName = dynamicAddress.GetDynamicProperty(nameof(address.CompanyName));

			AssertMultilineASCIIEquals("",
				"",
				companyName.ToFormatString());
		}
	}
}
