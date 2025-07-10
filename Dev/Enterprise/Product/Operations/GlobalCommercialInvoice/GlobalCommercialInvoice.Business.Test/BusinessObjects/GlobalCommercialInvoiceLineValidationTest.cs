using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.GlobalCommercialInvoice.Business.Test.BusinessObjects
{
	public class GlobalCommercialInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestGIL_Description_WithEmptyValue_IsValid()
		{
			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_Description = string.Empty;
			AssertNoErrors(invoiceLine.GIL_DescriptionInfo);

			invoiceLine.GIL_Description = new string('X', AutoGlobalCommercialInvoiceHeader.Schema.GIH_DescriptionMaxLength);
			AssertNoErrors(invoiceLine.GIL_DescriptionInfo);
		}

		public void TestValidateGIL_GrossWeightUQ()
		{
			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_GrossWeightUQ = string.Empty;
			invoiceLine.GIL_GrossWeight = 0;
			AssertNoErrors(invoiceLine.GIL_GrossWeightUQInfo);

			invoiceLine.GIL_GrossWeightUQ = "XX";
			AssertHasError(invoiceLine.GIL_GrossWeightUQInfo, "Enter a valid " + invoiceLine.GIL_GrossWeightUQInfo.Description + ".");

			invoiceLine.GIL_GrossWeight = 1;
			invoiceLine.GIL_GrossWeightUQ = string.Empty;
			AssertHasError(invoiceLine.GIL_GrossWeightUQInfo, "Please enter a Unit of Gross Weight.");

			invoiceLine.GIL_GrossWeightUQ = Constants.Weight.Kilograms;
			AssertNoErrors(invoiceLine.GIL_GrossWeightUQInfo);
		}

		public void TestValidateGIL_GrossWeight()
		{
			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_GrossWeight = -1;
			AssertHasError(invoiceLine.GIL_GrossWeightInfo, "Please enter a 'Unit of Gross Weight' greater than or equal to 0.");

			invoiceLine.GIL_GrossWeight = 0;
			AssertNoWarnings(invoiceLine.GIL_GrossWeightInfo);

			invoiceLine.GIL_GrossWeight = 1;
			AssertNoWarnings(invoiceLine.GIL_GrossWeightInfo);
		}

		public void TestValidateGIL_NetWeightUQ()
		{
			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_NetWeightUQ = string.Empty;
			invoiceLine.GIL_NetWeight = 0;
			AssertNoErrors(invoiceLine.GIL_NetWeightUQInfo);

			invoiceLine.GIL_NetWeightUQ = "XX";
			AssertHasError(invoiceLine.GIL_NetWeightUQInfo, "Enter a valid " + invoiceLine.GIL_NetWeightUQInfo.Description + ".");

			invoiceLine.GIL_NetWeight = 1;
			invoiceLine.GIL_NetWeightUQ = string.Empty;
			AssertHasError(invoiceLine.GIL_NetWeightUQInfo, "Please enter a Unit of Net Weight.");

			invoiceLine.GIL_NetWeightUQ = Constants.Weight.Kilograms;
			AssertNoErrors(invoiceLine.GIL_NetWeightUQInfo);
		}

		public void TestValidateGIL_NetWeight()
		{
			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_NetWeight = -1;
			AssertHasError(invoiceLine.GIL_NetWeightInfo, "Please enter a 'Unit of Net Weight' greater than or equal to 0.");

			invoiceLine.GIL_NetWeight = 0;
			AssertNoWarnings(invoiceLine.GIL_NetWeightInfo);

			invoiceLine.GIL_NetWeight = 1;
			AssertNoWarnings(invoiceLine.GIL_NetWeightInfo);
		}

		public void TestValidateGIL_VolumeUQ()
		{
			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_VolumeUQ = string.Empty;
			invoiceLine.GIL_Volume = 0;
			AssertNoErrors(invoiceLine.GIL_VolumeUQInfo);

			invoiceLine.GIL_VolumeUQ = "XX";
			AssertHasError(invoiceLine.GIL_VolumeUQInfo, "Enter a valid " + invoiceLine.GIL_VolumeUQInfo.Description + ".");

			invoiceLine.GIL_Volume = 1;
			invoiceLine.GIL_VolumeUQ = string.Empty;
			AssertHasError(invoiceLine.GIL_VolumeUQInfo, "Please enter a Unit of Volume.");

			invoiceLine.GIL_VolumeUQ = Constants.Volume.CubicMetres;
			AssertNoErrors(invoiceLine.GIL_VolumeUQInfo);
		}

		public void TestValidateGIL_Volume()
		{
			var invoiceLine = Factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_Volume = -1;
			AssertHasError(invoiceLine.GIL_VolumeInfo, "Please enter a 'Unit of Volume' greater than or equal to 0.");

			invoiceLine.GIL_Volume = 0;
			AssertNoWarnings(invoiceLine.GIL_VolumeInfo);

			invoiceLine.GIL_Volume = 1;
			AssertNoWarnings(invoiceLine.GIL_VolumeInfo);
		}
	}
}
