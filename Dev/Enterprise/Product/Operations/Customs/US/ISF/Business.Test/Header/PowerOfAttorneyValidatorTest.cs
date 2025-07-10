using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class PowerOfAttorneyValidatorTest : TestCaseWithFactory
	{
		public void TestValidatePowerOfAttorneyDocumentDates()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var company1PK = company1.PK.ToGuid();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(company1PK, Guid.Empty, Guid.Empty, Customs.Business.PowerOfAttorneyNotificationTypeList.Codes.Warning);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Customs.Business.PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			CusISFHeader header = Factory.New<CusISFHeader>();
			using (header.SuspendValidationTesting())
			{
				PowerOfAttorneyValidator poav = new PowerOfAttorneyValidator();
				OrgHeader importer = Factory.New<OrgHeader>();
				JobRequiredDocument poaDocument = importer.RequiredDocuments.AddNew("POA");
				poaDocument.EQ_DocType = "POA";
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Empty;
				header.BF_OH_Importer = importer.PK;
				string documentOwner = "organization";
				string receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poav.CountrySpecificNameForPOA, documentOwner);
				string expiryDateRequiredForPeriodicDocument = PowerOfAttorneyValidator.GetExpiryDateRequiredForPeriodicDocumentString(poav.CountrySpecificNameForPOA, documentOwner);
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, receivedDateRequired);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
				poaDocument.EQ_ValidToDate = ZDateTime.Today;
				header.ClearAllNotifications();
				string pOAWillExpireSoon = PowerOfAttorneyValidator.GetPOAWillExpireSoonString(ZDate.Today.ToString(), documentOwner, poav.CountrySpecificNameForPOA);
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, expiryDateRequiredForPeriodicDocument);
				AssertHasWarningContaining(header.BF_OH_ImporterInfo, pOAWillExpireSoon);
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(60);
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertNoWarningContaining(header.BF_OH_ImporterInfo, pOAWillExpireSoon);
				poaDocument.EQ_ValidToDate = ZDateTime.Today;
				string pOAExpired = PowerOfAttorneyValidator.GetPOAExpiredString(ZDate.Today.AddDays(-1).ToString(), documentOwner, poav.CountrySpecificNameForPOA);
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, pOAExpired);
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, pOAExpired);
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
				JobRequiredDocAttrib attrib = poaDocument.Attributes.AddNew();
				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Export;
				string poaString = PowerOfAttorneyValidator.GetNoPOADocumentForImporterString(poav.CountrySpecificNameForPOA);
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				AssertNoWarningContaining(header.BF_OH_ImporterInfo, poaString);
				header.BF_GB = branch1.PK;
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasWarningContaining(header.BF_OH_ImporterInfo, poaString);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				header.BF_GB = GlbBranch.CurrentBranch.PK;
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				AssertNoWarningContaining(header.BF_OH_ImporterInfo, poaString);
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Import;
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				importer.RequiredDocuments.RemoveAndDeleteAll();
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				poaDocument = header.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Empty;
				receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poav.CountrySpecificNameForPOA, "ISF");
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageError(header.BF_OH_ImporterInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				header.ClearAllNotifications();
				poav.ValidatePowerOfAttorneyDocumentDates(header.BF_OH_ImporterInfo, poaDocument, "ISF");
				AssertHasMessageError(header.BF_OH_ImporterInfo, receivedDateRequired);
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertNoMessageError(header.BF_OH_ImporterInfo, receivedDateRequired);
				header.ClearAllNotifications();
				poav.ValidatePowerOfAttorneyDocumentDates(header.BF_OH_ImporterInfo, poaDocument, "ISF");
				AssertNoMessageError(header.BF_OH_ImporterInfo, receivedDateRequired);
				attrib = poaDocument.Attributes.AddNew();
				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Export;
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertHasMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Import;
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
				attrib.D0_AttribValue = ImportExportCodeList.Codes.ImportISF;
				header.ClearAllNotifications();
				poav.Validate(header, header.Importer, header.BF_OH_ImporterInfo);
				AssertNoMessageErrorContaining(header.BF_OH_ImporterInfo, poaString);
			}
		}
	}
}
