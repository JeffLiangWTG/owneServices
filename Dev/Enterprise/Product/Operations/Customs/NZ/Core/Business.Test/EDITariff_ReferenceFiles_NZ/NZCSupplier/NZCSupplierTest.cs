using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NZCSupplier))]
	public class NZCSupplierTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var supplier = Factory.New<NZCSupplier>();
			supplier.U8_CustomsCode = "anelle El";

			AssertEquals("Supplier anelle El", supplier.HumanReadableName);
		}

		public void TestDescription()
		{
			NZCSupplier supplier = Factory.New<NZCSupplier>();
			supplier.U8_CountryCode = "FG";
			supplier.U8_CompanyName = "FIOCCHI HANDGUNS";
			AssertEquals("supplier.U8_Description", "FIOCCHI HANDGUNS (FG)", supplier.U8_Description);
			AssertEquals("supplier.U8_Description", "FIOCCHI HANDGUNS (FG)", DescriptionPropertyAttribute.DescriptionFromBusinessObject(supplier));
		}
	}
}
