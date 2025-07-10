using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAPHISInspectionAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Date()
		{
			var inspection = Header.Inspections.AddNew();
			inspection.US_Date = new ZDateTime(2015, 7, 1);
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Inspection Date");
			AssertNoMessageError(inspection.US_DateInfo, messageError);
			inspection.US_Date = ZDateTime.Empty;
			AssertHasMessageError(inspection.US_DateInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			inspection.AddInfoValidation.ValidateUS_Date();
			AssertNoMessageError(inspection.US_DateInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			inspection.AddInfoValidation.ValidateUS_Date();
			AssertHasMessageError(inspection.US_DateInfo, messageError);
		}

		public void TestCheckUS_PortCode()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "43@3", "Test Name", startDate, endDate);
			newFactory.Save();

			var uNLOCO1 = Factory.NewWithValidTestData<MasterFiles.Business.RefUNLOCO>();
			uNLOCO1.Code = "AUSYD";
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Inspection Port Code");
			var inspection = Header.Inspections.AddNew();

			inspection.US_TestingStatus = InspectionStatusList.Codes.LabTestingPreviouslyPerformed;
			inspection.AddInfoValidation.ValidateUS_Location();
			AssertHasMessageErrorContaining(inspection.US_LocationInfo, messageError);

			inspection.US_TestingStatus = InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection;
			inspection.AddInfoValidation.ValidateUS_Location();
			AssertHasMessageErrorContaining(inspection.US_LocationInfo, messageError);

			inspection.US_TestingStatus = InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection;
			inspection.US_Location = "43@3";
			AssertNoMessageErrorContaining(inspection.US_LocationInfo, ListValidation.InvalidCodeMessageError);

			inspection.US_TestingStatus = InspectionStatusList.Codes.PreviouslyPerformed;
			inspection.AddInfoValidation.ValidateUS_Location();
			AssertHasMessageErrorContaining(inspection.US_LocationInfo, ListValidation.InvalidCodeMessageError);

			inspection.US_Location = "AUSYD";
			AssertNoMessageErrorContaining(inspection.US_LocationInfo, ListValidation.InvalidCodeMessageError);

			inspection.US_Location = "~";
			AssertHasMessageErrorContaining(inspection.US_LocationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_TestingStatus()
		{
			var inspection = Header.Inspections.AddNew();
			inspection.US_TestingStatus = "#";
			AssertNoMessageErrorContaining(inspection.US_TestingStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(inspection.US_TestingStatusInfo, ListValidation.InvalidCodeMessageError);
			inspection.US_TestingStatus = ZString.Empty;
			AssertHasMessageErrorContaining(inspection.US_TestingStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inspection.US_TestingStatusInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			inspection.AddInfoValidation.ValidateUS_TestingStatus();
			AssertNoMessageErrorContaining(inspection.US_TestingStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inspection.US_TestingStatusInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			inspection.AddInfoValidation.ValidateUS_TestingStatus();
			AssertHasMessageErrorContaining(inspection.US_TestingStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inspection.US_TestingStatusInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in InspectionStatusList.GetListForAPHIS(Factory))
			{
				inspection.US_TestingStatus = pair.Code;
				AssertNoMessageErrorContaining(inspection.US_TestingStatusInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(inspection.US_TestingStatusInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		APHISHeader Header
		{
			get
			{
				if (aphisHeader == null)
				{
					aphisHeader = InvoiceLine.APHISHeaders.AddNew();
					aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return aphisHeader;
			}
		}
		APHISHeader aphisHeader;

		#endregion
	}
}
