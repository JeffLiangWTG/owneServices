using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEFDAValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDimensions()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertEquals(FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals, FDA.US_DimUQ);

			FDA.US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals;
			AssertHasMessageErrorContaining(FDA.US_DimUQInfo, ListValidation.InvalidCodeMessageError);

			FDA.US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals;
			AssertNoMessageErrorContaining(FDA.US_DimUQInfo, ListValidation.InvalidCodeMessageError);

			FDA.US_ContainerDimType = CylindricalRectangularList.Codes.Cylindrical;
			FDA.US_CanDim1Inch = 10;
			FDA.US_CanDim1_16th = 18;
			AssertHasMessageError(FDA.US_CanDim1_16thInfo, ACEFDAValidation.Dimensions16th);
			FDA.US_CanDim1_16th = 11;
			AssertNoMessageError(FDA.US_CanDim1_16thInfo, ACEFDAValidation.Dimensions16th);

			FDA.US_CanDim2Inch = 12;
			FDA.US_CanDim2_16th = 19;
			AssertHasMessageError(FDA.US_CanDim2_16thInfo, ACEFDAValidation.Dimensions16th);
			FDA.US_CanDim2_16th = 9;
			AssertNoMessageError(FDA.US_CanDim2_16thInfo, ACEFDAValidation.Dimensions16th);

			FDA.US_CanDim3Inch = 11;
			FDA.US_CanDim3_16th = 50;
			AssertHasMessageError(FDA.US_CanDim3_16thInfo, ACEFDAValidation.Dimensions16th);
			FDA.US_CanDim3_16th = 11;
			AssertNoMessageError(FDA.US_CanDim3_16thInfo, ACEFDAValidation.Dimensions16th);

			FDA.US_CanDim1Inch = 121;
			FDA.US_CanDim1_16th = 92;
			AssertHasMessageError(FDA.US_CanDim1_16thInfo, ACEFDAValidation.Dimensions16th);
			AssertHasMessageError(FDA.US_CanDim1InchInfo, ACEFDAValidation.DimensionsInch);
			FDA.US_CanDim1Inch = 12;
			FDA.US_CanDim1_16th = 12;
			AssertNoMessageError(FDA.US_CanDim1_16thInfo, ACEFDAValidation.Dimensions16th);
			AssertNoMessageError(FDA.US_CanDim1InchInfo, ACEFDAValidation.DimensionsInch);

			FDA.US_CanDim2Inch = 133;
			FDA.US_CanDim2_16th = 93;
			AssertHasMessageError(FDA.US_CanDim2_16thInfo, ACEFDAValidation.Dimensions16th);
			AssertHasMessageError(FDA.US_CanDim2InchInfo, ACEFDAValidation.DimensionsInch);
			FDA.US_CanDim2Inch = 13;
			FDA.US_CanDim2_16th = 13;
			AssertNoMessageError(FDA.US_CanDim2_16thInfo, ACEFDAValidation.Dimensions16th);
			AssertNoMessageError(FDA.US_CanDim2InchInfo, ACEFDAValidation.DimensionsInch);

			FDA.US_CanDim3Inch = 144;
			FDA.US_CanDim3_16th = 94;
			AssertHasMessageError(FDA.US_CanDim3_16thInfo, ACEFDAValidation.Dimensions16th);
			AssertHasMessageError(FDA.US_CanDim3InchInfo, ACEFDAValidation.DimensionsInch);
			FDA.US_CanDim3_16th = 14;
			FDA.US_CanDim3Inch = 14;
			AssertNoMessageError(FDA.US_CanDim3_16thInfo, ACEFDAValidation.Dimensions16th);
			AssertNoMessageError(FDA.US_CanDim3InchInfo, ACEFDAValidation.DimensionsInch);
		}

		protected ACEFDA FDA
		{
			get
			{
				if (fda == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					fda = invoiceLine.ACE_FDALines.AddNew();
				}
				return fda;
			}
		}
		ACEFDA fda;
	}
}
