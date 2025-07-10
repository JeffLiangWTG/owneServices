using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class CommonImportAddInfoJobComHeaderValidationTest : BusinessObjectValidationTestCase
	{
		protected void AssertUS_DeductADDCVDDuty(JobComInvoiceHeader invoice)
		{
			invoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JobDeclaration.US_EntryType = "03";
			invoice.JZ_IncoTerm = "DDP";
			AssertHasMessageErrorContaining(invoice.US_DeductADDCVDDutyInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.US_DeductADDCVDDuty = "G";
			AssertHasMessageErrorContaining(invoice.US_DeductADDCVDDutyInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_DeductADDCVDDuty = "Y";
			AssertNoMessageErrorContaining(invoice.US_DeductADDCVDDutyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoice.US_DeductADDCVDDutyInfo, ListValidation.InvalidCodeMessageError);
		}

		protected void AssertUS_UC_NKCountryOfExport(JobComInvoiceHeader invoice)
		{
			invoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JobDeclaration.US_EntryType = "01";
			invoice.US_UC_NKCountryOfExport = "US";
			AssertHasMessageError(invoice.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoice.JobDeclaration.US_EntryType = "21";
			invoice.AddInfoValidation.ValidateUS_UC_NKCountryOfExport();
			AssertNoMessageError(invoice.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoice.JobDeclaration.US_EntryType = "22";
			invoice.AddInfoValidation.ValidateUS_UC_NKCountryOfExport();
			AssertNoMessageError(invoice.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoice.JobDeclaration.US_EntryType = "01";
			invoice.US_UC_NKCountryOfExport = "PR";
			AssertHasMessageError(invoice.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoice.JobDeclaration.US_EntryType = "31";
			invoice.AddInfoValidation.ValidateUS_UC_NKCountryOfExport();
			AssertNoMessageError(invoice.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoice.JobDeclaration.US_EntryType = "32";
			invoice.AddInfoValidation.ValidateUS_UC_NKCountryOfExport();
			AssertNoMessageError(invoice.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoice.US_UC_NKCountryOfExport = "ZZ";
			AssertHasMessageError(invoice.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_UC_NKCountryOfExport = "AU";
			AssertNoMessageError(invoice.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			invoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			invoice.US_UC_NKCountryOfExport = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertHasMessageError(invoice.US_UC_NKCountryOfExportInfo, ExternalValidation.CanadianProvinceCodeNotAllowedForCountryOfExport);
			invoice.US_UC_NKCountryOfExport = "CA";
			AssertNoMessageError(invoice.US_UC_NKCountryOfExportInfo, ExternalValidation.CanadianProvinceCodeNotAllowedForCountryOfExport);
		}

		protected void AssertUS_UC_NKCountryOfOrigin(JobComInvoiceHeader invoice)
		{
			invoice.US_UC_NKCountryOfOrigin = "!!";
			AssertHasMessageErrors(invoice.US_UC_NKCountryOfOriginInfo);
			invoice.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageErrors(invoice.US_UC_NKCountryOfOriginInfo);
			invoice.US_UC_NKCountryOfOrigin = USCCountry.Unknown;
			AssertNoMessageErrors(invoice.US_UC_NKCountryOfOriginInfo);
			invoice.US_UC_NKCountryOfExport = "CA";
			invoice.US_UC_NKCountryOfOrigin = "CA";
			AssertHasMessageError(invoice.US_UC_NKCountryOfOriginInfo, ExternalValidation.CAIsNotAValidCountryForCustomsMessagingPurpose);
			invoice.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertNoMessageError(invoice.US_UC_NKCountryOfOriginInfo, ExternalValidation.CAIsNotAValidCountryForCustomsMessagingPurpose);
			invoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.US_UC_NKCountryOfOrigin = "US";
			AssertNoMessageErrors(invoice.US_UC_NKCountryOfOriginInfo);
			invoice.US_UC_NKCountryOfOrigin = "**";
			AssertNoMessageErrors(invoice.US_UC_NKCountryOfOriginInfo);
		}

		protected void AssertUS_ZoneStatus(JobComInvoiceHeader invoice)
		{
			invoice.US_ZoneStatus = "~";
			AssertHasMessageError(invoice.US_ZoneStatusInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			AssertNoMessageError(invoice.US_ZoneStatusInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_PrivilegedStatusDate = ZDateTime.Invalid;
			AssertHasNotifications("Preconditions: US_PrivilegedStatusDate validation should add at least 1 notification when invalid date was set in US_PrivilegedStatusDate", invoice.US_PrivilegedStatusDateInfo);
			for (int i = 0; i < invoice.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				invoice.US_PrivilegedStatusDate = ZDateTime.Invalid;
				invoice.US_PrivilegedStatusDateInfo.ClearAllNotifications();
				invoice.US_ZoneStatus = invoice.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (invoice.AddInfoLookups.US_ZoneStatusList[i].Code == ZoneStatusList.Codes.PrivilegedForeign)
				{
					AssertHasNotifications("ValidateUS_PrivilegedStatusDate shoud be called here", invoice.US_PrivilegedStatusDateInfo);
				}
				else
				{
					AssertNoNotifications("ValidateUS_PrivilegedStatusDate shoud be called here", invoice.US_PrivilegedStatusDateInfo);
				}
			}
		}
	}
}
