using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PackageValidationTest : Customs.Business.Testing.CusDecHouseContainerPackValidationTest
	{
		protected override void MakeAssertionsOnCW_PackQtyValidationNotificationType(BaseJobDeclaration declaration)
		{
			var invoiceLine1 = declaration.InvoiceLines[0];
			var package = declaration.Packages[0];
			var info = package.CW_PackQtyInfo;
			Assert("No message errors", !info.HasMessageError(TotalNumberOfPacksExceedsCW_PackQtyMessage(20)));
			invoiceLine1.PackagesPivot[0].CHC_NumberOfPacks = 20;
			Assert("No message errors", !info.HasMessageError(TotalNumberOfPacksExceedsCW_PackQtyMessage(20)));
		}

		public void TestCheckCW_PackQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var package = declaration.Packages.AddNew();
			var info = package.CW_PackQtyInfo;
			(invoiceLine1.PackagesPivot.AddPivotFor(package) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 50;
			(invoiceLine2.PackagesPivot.AddPivotFor(package) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 40;
			(invoiceLine3.PackagesPivot.AddPivotFor(package) as InvoiceLinePackagePivot).CHC_NumberOfPacks = 20;
			package.CW_PackQty = 100;
			Assert("No message errors", !info.HasMessageError(TotalNumberOfPacksExceedsCW_PackQtyMessage(110)));
			package.CW_PackQty = 110;
			Assert("No message errors", !info.HasMessageError(TotalNumberOfPacksExceedsCW_PackQtyMessage(110)));
			AssertNoErrorContaining(info, MandatoryValidation.ValueCannotBeZero);
			package.CW_PackQty = 0;
			AssertHasErrorContaining(info, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckCW_PackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			var info = package.CW_PackTypeInfo;
			package.CW_PackType = "CTN";
			AssertNoErrorContaining(info, MandatoryValidation.MustBeEntered);
			package.CW_PackType = ZString.Empty;
			AssertHasErrorContaining(info, MandatoryValidation.MustBeEntered);
		}

		ZString TotalNumberOfPacksExceedsCW_PackQtyMessage(ZInt totalNumbers) => $"The total number of packs included in invoice(s) and invoice line(s) is {totalNumbers}, it exceeds this package quantity.";
		public void TestCheckCW_NetWeight()
		{
			package.CW_GrossWeight = 5.0m;
			package.CW_NetWeight = 5.0m;
			AssertHasMessageError(package.CW_NetWeightInfo, ValidationConstants.Package.NetWeightLessThanGrossWeight);
			package.CW_NetWeight = 4.99m;
			AssertNoMessageError(package.CW_NetWeightInfo, ValidationConstants.Package.NetWeightLessThanGrossWeight);
			package.CW_NetWeight = -1.23m;
			AssertHasMessageError(package.CW_NetWeightInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
			package.CW_NetWeight = 1.23m;
			AssertNoMessageError(package.CW_NetWeightInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
		}

		public void TestCheckCW_NetWeightUQ()
		{
			package.CW_GrossWeightUQ = "KG";
			package.CW_NetWeightUQ = "LB";
			AssertHasMessageError(package.CW_NetWeightUQInfo, ValidationConstants.Package.SameWeightUnit);
			package.CW_NetWeightUQ = "KG";
			AssertNoMessageError(package.CW_NetWeightUQInfo, ValidationConstants.Package.SameWeightUnit);
			package.CW_NetWeightUQ = "XX";
			AssertHasMessageError(package.CW_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
			package.CW_NetWeight = 1.23m;
			package.CW_NetWeightUQ = string.Empty;
			AssertHasMessageError(package.CW_NetWeightUQInfo, ValidationConstants.Package.NetWeightUQIsRequiredWhenNetWeightIsGreaterThanZero);
			package.CW_NetWeight = 0m;
			package.CW_NetWeightUQ = string.Empty;
			AssertNoMessageError(package.CW_NetWeightUQInfo, ValidationConstants.Package.NetWeightUQIsRequiredWhenNetWeightIsGreaterThanZero);
		}

		public void TestCheckCW_GrossWeight()
		{
			package.CW_NetWeight = 5.0m;
			package.CW_GrossWeight = 5.0m;
			AssertHasMessageError(package.CW_GrossWeightInfo, ValidationConstants.Package.NetWeightLessThanGrossWeight);
			package.CW_GrossWeight = 5.01m;
			AssertNoMessageError(package.CW_GrossWeightInfo, ValidationConstants.Package.NetWeightLessThanGrossWeight);
			package.CW_GrossWeight = -1.23m;
			AssertHasMessageError(package.CW_GrossWeightInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
			package.CW_GrossWeight = 1.23m;
			AssertNoMessageError(package.CW_GrossWeightInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
		}

		public void TestCheckCW_GrossWeightUQ()
		{
			package.CW_NetWeightUQ = "LB";
			package.CW_GrossWeightUQ = "KG";
			AssertHasMessageError(package.CW_GrossWeightUQInfo, ValidationConstants.Package.SameWeightUnit);
			package.CW_GrossWeightUQ = "LB";
			AssertNoMessageError(package.CW_GrossWeightUQInfo, ValidationConstants.Package.SameWeightUnit);
			package.CW_GrossWeightUQ = "XX";
			AssertHasMessageError(package.CW_GrossWeightUQInfo, ListValidation.InvalidCodeMessageError);
			package.CW_GrossWeight = 1.23m;
			package.CW_GrossWeightUQ = string.Empty;
			AssertHasMessageError(package.CW_GrossWeightUQInfo, ValidationConstants.Package.GrossWeightUQIsRequiredWhenGrossWeightIsGreaterThanZero);
			package.CW_GrossWeight = 0m;
			package.CW_GrossWeightUQ = string.Empty;
			AssertNoMessageError(package.CW_GrossWeightUQInfo, ValidationConstants.Package.GrossWeightUQIsRequiredWhenGrossWeightIsGreaterThanZero);
		}

		public void TestCheckCW_DimensionUQ()
		{
			package.CW_DimensionUQ = "XX";
			AssertHasMessageError(package.CW_DimensionUQInfo, ListValidation.InvalidCodeMessageError);
			package.CW_DimensionUQ = DimensionUQList.Codes.CM;
			AssertNoMessageErrorContaining(package.CW_DimensionUQInfo, ListValidation.InvalidCodeMessageError);
			package.CW_Length = 1.23m;
			package.CW_Height = 0m;
			package.CW_Width = 0m;
			package.CW_DimensionUQ = string.Empty;
			AssertHasMessageError(package.CW_DimensionUQInfo, ValidationConstants.Package.DimensionUQIsRequiredWhenLengthOrHeightOrWidthIsGreaterThanZero);
			package.CW_Length = 0m;
			package.CW_Height = 1.23m;
			package.CW_Width = 0m;
			package.CW_DimensionUQ = string.Empty;
			AssertHasMessageError(package.CW_DimensionUQInfo, ValidationConstants.Package.DimensionUQIsRequiredWhenLengthOrHeightOrWidthIsGreaterThanZero);
			package.CW_Length = 0m;
			package.CW_Height = 0m;
			package.CW_Width = 1.23m;
			package.CW_DimensionUQ = string.Empty;
			AssertHasMessageError(package.CW_DimensionUQInfo, ValidationConstants.Package.DimensionUQIsRequiredWhenLengthOrHeightOrWidthIsGreaterThanZero);
			package.CW_Length = 0m;
			package.CW_Height = 0m;
			package.CW_Width = 0m;
			package.CW_DimensionUQ = string.Empty;
			AssertNoMessageError(package.CW_DimensionUQInfo, ValidationConstants.Package.DimensionUQIsRequiredWhenLengthOrHeightOrWidthIsGreaterThanZero);
		}

		public void TestCheckCW_VolumeUQ()
		{
			package.CW_VolumeUQ = "XX";
			AssertHasMessageError(package.CW_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			package.CW_VolumeUQ = "L";
			AssertNoMessageErrorContaining(package.CW_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			package.CW_Volume = 1.23m;
			package.CW_VolumeUQ = string.Empty;
			AssertHasMessageError(package.CW_VolumeUQInfo, ValidationConstants.Package.VolumeUQIsRequiredWhenVolumeIsGreaterThanZero);
			package.CW_Volume = 0m;
			package.CW_VolumeUQ = string.Empty;
			AssertNoMessageError(package.CW_VolumeUQInfo, ValidationConstants.Package.VolumeUQIsRequiredWhenVolumeIsGreaterThanZero);
		}

		public void TestCheckCW_Volume()
		{
			package.CW_Volume = -1.23m;
			AssertHasMessageError(package.CW_VolumeInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
			package.CW_Volume = 1.23m;
			AssertNoMessageError(package.CW_VolumeInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
		}

		public void TestCheckCW_Length()
		{
			package.CW_Length = -1.23m;
			AssertHasMessageError(package.CW_LengthInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
			package.CW_Length = 1.23m;
			AssertNoMessageError(package.CW_LengthInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
		}

		public void TestCheckCW_Height()
		{
			package.CW_Height = -1.23m;
			AssertHasMessageError(package.CW_HeightInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
			package.CW_Height = 1.23m;
			AssertNoMessageError(package.CW_HeightInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
		}

		public void TestCheckCW_Width()
		{
			package.CW_Width = -1.23m;
			AssertHasMessageError(package.CW_WidthInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
			package.CW_Width = 1.23m;
			AssertNoMessageError(package.CW_WidthInfo, ValidationConstants.Package.ValueShouldNotBeNegative);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			package = declaration.Packages.AddNew();
		}

		JobDeclaration declaration;
		Package package;
	}
}
