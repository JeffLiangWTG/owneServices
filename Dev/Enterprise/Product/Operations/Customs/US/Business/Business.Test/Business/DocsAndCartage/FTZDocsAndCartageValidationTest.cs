using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZDocsAndCartageValidationTest : TestCaseWithFactory
	{
		public void TestCheckJP_OA_DeliveryCartageCoAddr()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.US_F_IncludePTT = true;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.ValidateOrgPK();
			var errorText = string.Format(ValidationConstants.FTZ.DataRequiredWhenPTTIncluded, "Carrier");
			AssertHasMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			declaration.DeliveryOrPickupCartageCoPK = organization.PK;
			AssertNoMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
			declaration.US_F_IncludePTT = false;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.ValidateOrgPK();
			AssertHasWarning(declaration.DeliveryOrPickupCartageCoPKInfo, ValidationConstants.FTZ.CarrierWillNotBeSend);
			declaration.DeliveryOrPickupCartageCoPK = ZGuid.Empty;
			AssertNoWarning(declaration.DeliveryOrPickupCartageCoPKInfo, ValidationConstants.FTZ.CarrierWillNotBeSend);
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			errorText = string.Format(ValidationConstants.FTZ.DataRequired, "Carrier");
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr_ZAddress.ValidateOrgPK();
			AssertHasMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
			declaration.DeliveryOrPickupCartageCoPK = organization.PK;
			AssertNoMessageError(declaration.DeliveryOrPickupCartageCoPKInfo, errorText);
		}
	}
}
