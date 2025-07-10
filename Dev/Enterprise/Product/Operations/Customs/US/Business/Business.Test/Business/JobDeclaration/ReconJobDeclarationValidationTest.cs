using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ReconJobDeclarationValidationTest : TestCaseWithFactory
	{
		[TestDate(2018, 03, 01)]
		public void TestCheckJE_ApplicationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_OtherReconIndicator = "VC";
			var reconDec = new ReconDeclaration(declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertHasMessageError(reconDec.JE_ApplicationCodeInfo, ReconJobDeclarationValidation.ApplicationCodeMessageText);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertNoMessageError(reconDec.JE_ApplicationCodeInfo, ReconJobDeclarationValidation.ApplicationCodeMessageText);
		}

		public void TestNoTotalWeightMessageErrorOnRegonJob()
		{
			using (CustomsDataRegistry.Instance.SeverityLevelOfTotalWeightValidation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation))
			{
				var declaration = Factory.New<JobDeclaration>();
				var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line.JI_CustomsUnitQty = "KG";
				line.JI_CustomsQuantity = 5000;
				var line2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				line2.JI_CustomsUnitQty = "BOX";
				line2.JI_CustomsQuantity = 51;

				declaration.JE_TotalWeightUnit = "T";
				declaration.JE_TotalWeight = 6m;
				AssertNoWarnings(declaration.JE_TotalWeightInfo);

				declaration.JE_TotalWeight = 4m;
				AssertHasMessageError("Declaration has message error on TotalWeight.", declaration.JE_TotalWeightInfo, declaration.Validation.MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);

				var reconDec = new ReconDeclaration(declaration);
				reconDec.US_SchDEntry = "1001";
				reconDec.ReconWrappedJobDeclaration.Validation.ValidateAll();
				AssertNoMessageErrors("Recon should not have error message on TotalWeights", reconDec.ReconWrappedJobDeclaration.JE_TotalWeightInfo);
			}
		}

		[TestDate(2012, 01, 01)]//recon interest rate
		public void TestRunReconRelatedValidationsOnly()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1001", "Test Name", startDate, endDate);
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();

			var reconDec = new ReconDeclaration(declaration);
			reconDec.US_SchDEntry = "1001";
			reconDec.ReconWrappedJobDeclaration.JE_TransportMode = "";
			reconDec.ReconWrappedJobDeclaration.JE_ContainerMode = "";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "98-1234567AA");

			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = importer.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;

			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.Yes;
			Factory.Save();

			var company = Factory.New<GlbCompany>();
			company.GC_Name = "MY COMPANY";
			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MY BRANCH";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_WorkPhone = "MY PHONE";
			staff.GS_FullName = "MY NAME";
			staff.GS_GB_HomeBranch = branch.PK;

			reconDec.JE_GS_NKCusAgent = staff.GS_Code;
			reconDec.IOROrgPK = importer.PK;
			reconDec.US_SuretyCode = "891";
			reconDec.US_EstimatedEntryDate = ZDateTime.Today.AddDays(2);
			reconDec.US_TeamNo = "1RS";
			reconDec.US_IssueCode = "VC";
			reconDec.US_IsAggregate = true;
			reconDec.US_R_IsNoChangeAgg = true;
			reconDec.US_Comment = "Comment";
			reconDec.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			reconDec.BrokerToPayIndicator = YesNoDefaultList.Codes.No;
			reconDec.US_ImportEntrySource = "1";
			reconDec.US_SchDEntry = "1001";

			var entryHeader = reconDec.OriginalEntries.AddNew();

			entryHeader.US_PendingActionID = "1111";
			entryHeader.US_PendingActionIDType = "A";
			entryHeader.US_R_ReleaseDate = ZDateTime.Today.AddDays(-1);
			entryHeader.US_PaymentDate = ZDateTime.Today.AddDays(-1);
			entryHeader.US_SchDEntry = "1001";
			entryHeader.CH_OrigEntryReference = "XJ560011281";
			entryHeader.US_R_DateForMPFCalc = ZDateTime.Today;
			entryHeader.US_PendingActionIDType = PendingActionIDTypeList.Codes.A;
			entryHeader.US_PendingActionID = "1";

			reconDec.RunPreSaveValidation();
			entryHeader.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;

			AssertEquals("No message errors are expected:If this fails, it means you have added a validation that is not relevant to Recon Business", "", reconDec.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString());
		}

		public void TestCheckJE_OH_Importer_POAWithAttribute()
		{
			var validator = new AuthorityToActValidator();

			var ior = Factory.New<OrgHeader>();
			var poaDocument = ior.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);

			var attribute1 = poaDocument.Attributes.AddNew();
			attribute1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			attribute1.D0_AttribValue = ImportExportCodeList.Codes.Export;

			var notMatchErrorMessage = AuthorityToActValidator.GetPOANotValidForConditions(validator.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)", "US and/or this direction");
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_OtherReconIndicator = "VC";
			var reconDec = new ReconDeclaration(declaration);
			reconDec.IOROrgPK = ior.PK;
			AssertHasMessageError(reconDec.IOROrgPKInfo, notMatchErrorMessage);

			attribute1.D0_AttribValue = ImportExportCodeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(reconDec.IOROrgPKInfo, notMatchErrorMessage);

			var attribute2 = poaDocument.Attributes.AddNew();
			attribute2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(reconDec.IOROrgPKInfo, notMatchErrorMessage);

			attribute2.D0_AttribValue = "2705";
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(reconDec.IOROrgPKInfo, notMatchErrorMessage);

			attribute2.Delete();
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(reconDec.IOROrgPKInfo, notMatchErrorMessage);
		}
	}
}
