using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class PowerOfAttorneyValidatorTest : TestCaseWithFactory
	{
		public void TestValidatePowerOfAttorneyDocumentDates()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Customs.Business.PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			using (moveHeader.SuspendValidationTesting())
			{
				PowerOfAttorneyValidator poav = new PowerOfAttorneyValidator();
				OrgHeader carrier = Factory.New<OrgHeader>();
				JobRequiredDocument poaDocument = carrier.RequiredDocuments.AddNew("POA");
				poaDocument.EQ_DocType = "POA";
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Empty;
				moveHeader.BM_OA_InBondCarrier = carrier.MainAddress.PK;
				string documentOwner = "organization";
				string receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poav.CountrySpecificNameForPOA, documentOwner);
				string expiryDateRequiredForPeriodicDocument = PowerOfAttorneyValidator.GetExpiryDateRequiredForPeriodicDocumentString(poav.CountrySpecificNameForPOA, documentOwner);
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertHasMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, expiryDateRequiredForPeriodicDocument);
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertHasMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, receivedDateRequired);
				AssertHasMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, expiryDateRequiredForPeriodicDocument);
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
				poaDocument.EQ_ValidToDate = ZDateTime.Today;
				moveHeader.ClearAllNotifications();
				string pOAWillExpireSoon = PowerOfAttorneyValidator.GetPOAWillExpireSoonString(ZDate.Today.ToString(), documentOwner, poav.CountrySpecificNameForPOA);
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertNoMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, expiryDateRequiredForPeriodicDocument);
				AssertHasWarningContaining(moveHeader.BM_SystemLastEditUserInfo, pOAWillExpireSoon);
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(60);
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertNoWarningContaining(moveHeader.BM_SystemLastEditUserInfo, pOAWillExpireSoon);
				poaDocument.EQ_ValidToDate = ZDateTime.Today;
				string pOAExpired = PowerOfAttorneyValidator.GetPOAExpiredString(ZDate.Today.AddDays(-1).ToString(), documentOwner, poav.CountrySpecificNameForPOA);
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertNoMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, pOAExpired);
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertHasMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, pOAExpired);
				poaDocument.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
				JobRequiredDocAttrib attrib = poaDocument.Attributes.AddNew();
				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Export;
				string poaString = PowerOfAttorneyValidator.GetNoPOADocumentForImporterString(poav.CountrySpecificNameForPOA);
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertHasMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, poaString);
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Import;
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertNoMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, poaString);
				carrier.RequiredDocuments.RemoveAndDeleteAll();
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertHasMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, poaString);
				poaDocument = header.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Empty;
				receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poav.CountrySpecificNameForPOA, "In-Bond");
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertHasMessageError(moveHeader.BM_SystemLastEditUserInfo, receivedDateRequired);
				AssertNoMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, poaString);
				moveHeader.ClearAllNotifications();
				poav.ValidatePowerOfAttorneyDocumentDates(moveHeader.BM_SystemLastEditUserInfo, poaDocument, "In-Bond");
				AssertHasMessageError(moveHeader.BM_SystemLastEditUserInfo, receivedDateRequired);
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertNoMessageError(moveHeader.BM_SystemLastEditUserInfo, receivedDateRequired);
				moveHeader.ClearAllNotifications();
				poav.ValidatePowerOfAttorneyDocumentDates(moveHeader.BM_SystemLastEditUserInfo, poaDocument, "In-Bond");
				AssertNoMessageError(moveHeader.BM_SystemLastEditUserInfo, receivedDateRequired);
				attrib = poaDocument.Attributes.AddNew();
				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Export;
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertHasMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, poaString);
				attrib.D0_AttribValue = ImportExportCodeList.Codes.Import;
				moveHeader.ClearAllNotifications();
				poav.Validate(moveHeader, moveHeader.InBondCarrierOrg, moveHeader.BM_SystemLastEditUserInfo);
				AssertNoMessageErrorContaining(moveHeader.BM_SystemLastEditUserInfo, poaString);
			}
		}
	}
}
