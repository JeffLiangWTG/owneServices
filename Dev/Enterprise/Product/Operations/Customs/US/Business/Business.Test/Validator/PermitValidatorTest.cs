using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class PermitValidatorTest<T> : TestCaseWithFactory where T : PermitValidator
	{
		public void TestPermitValidatorTypeMatches()
		{
			AssertType(typeof(T), Validator);
		}

		public void TestPermits()
		{
			var originalEntryType = InvoiceLine.Declaration.US_EntryType;

			foreach (string permitRequiredEntryTypes in PermitRequiredEntryTypes)
			{
				InvoiceLine.Declaration.US_EntryType = permitRequiredEntryTypes;
				Assert(Validator.GetErrorTextForRequirement(ZString.Empty).Contains(" is required."));
			}

			foreach (string permitNotRequiredEntryTypes in PermitNotRequiredEntryTypes)
			{
				InvoiceLine.Declaration.US_EntryType = permitNotRequiredEntryTypes;
				AssertEquals("", Validator.GetErrorTextForRequirement(ZString.Empty));
			}

			InvoiceLine.Declaration.US_EntryType = originalEntryType;

			foreach (string validNumber in ValidNumbers)
			{
				AssertEquals("", Validator.GetErrorTextIfInvalidFormatOrNotRequired(validNumber));
			}

			foreach (string invalidNumber in InvalidNumbers)
			{
				Assert(Validator.GetErrorTextIfInvalidFormatOrNotRequired(invalidNumber).Contains(" number is invalid. The format should be "));
			}
		}

		protected abstract string LicenseTypeCode { get; }
		protected abstract string LicenseTypeDescription { get; }
		protected virtual string FormatMask => @"\w{1,9}";

		protected virtual string FormatErrorText => "1 to 9 alpha-numeric characters";

		protected abstract string[] ValidNumbers { get; }

		protected abstract string[] InvalidNumbers { get; }

		protected virtual string[] PermitRequiredEntryTypes => new string[] { "", EntryTypeList.Codes.PermanentExhibition };

		protected virtual string[] PermitNotRequiredEntryTypes => new string[] { EntryTypeList.Codes.Warehouse, EntryTypeList.Codes.ReWarehouse, EntryTypeList.Codes.TemporaryImportationBond };

		PermitValidator validator;
		protected PermitValidator Validator => validator ?? (validator = PermitValidatorHelper.GetPermitValidator(LicenseTypeCode, InvoiceLine));

		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
					declaration.Invoices.AddNew();
					invoiceLine = declaration.InvoiceLines.AddNew();
					SetupExtraTestDataForInvoiceLine(invoiceLine);
				}

				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		protected virtual void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicenseTypeCode, LicenseTypeDescription, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, FormatMask);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatErrorText, FormatErrorText);
			Factory.Save();
		}
	}
}
