using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class GlobalBusinessIdentifierDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_OA_AddressDetails()
		{
			messageData.US_OA_AddressDetails = ZGuid.Empty;
			AssertHasErrorContaining(messageData.US_OA_AddressDetailsInfo, MandatoryValidation.MustBeEntered);

			messageData.US_OA_AddressDetails = messageData.Organization.MainAddress.PK;
			AssertNoErrors(messageData.US_OA_AddressDetailsInfo);
		}

		public void TestCheckUS_IsManufacturer()
		{
			CheckAtLeastOneRoleShouldBeTicked(messageData.US_IsManufacturerInfo);
		}

		public void TestCheckUS_IsShipper()
		{
			CheckAtLeastOneRoleShouldBeTicked(messageData.US_IsShipperInfo);
		}

		public void TestCheckUS_IsSeller()
		{
			CheckAtLeastOneRoleShouldBeTicked(messageData.US_IsSellerInfo);
		}

		public void TestCheckUS_IsExporter()
		{
			CheckAtLeastOneRoleShouldBeTicked(messageData.US_IsExporterInfo);
		}

		public void TestCheckUS_IsPackager()
		{
			CheckAtLeastOneRoleShouldBeTicked(messageData.US_IsPackagerInfo);
		}

		public void TestCheckUS_IsDistributor()
		{
			CheckAtLeastOneRoleShouldBeTicked(messageData.US_IsDistributorInfo);
		}

		void CheckAtLeastOneRoleShouldBeTicked(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = ZBool.False;
			AssertHasMessageError(propertyInfo, GlobalBusinessIdentifierDataValidation.AtLeastOneRoleShouldBeTicked);

			propertyInfo.Value = ZBool.True;
			AssertNoMessageError(propertyInfo, GlobalBusinessIdentifierDataValidation.AtLeastOneRoleShouldBeTicked);
		}

		public void TestCheckUS_DUNS()
		{
			messageData.US_DUNS = ZString.Empty;
			AssertHasMessageErrorContaining(messageData.US_DUNSInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(messageData.US_DUNSInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);

			messageData.US_DUNS = "~";
			AssertNoMessageErrorContaining(messageData.US_DUNSInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(messageData.US_DUNSInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);

			messageData.US_DUNS = "123456789";
			AssertNoMessageErrorContaining(messageData.US_DUNSInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarning(messageData.US_DUNSInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);
		}

		public void TestCheckUS_GLN()
		{
			messageData.US_GLN = ZString.Empty;
			AssertHasMessageErrorContaining(messageData.US_GLNInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(messageData.US_GLNInfo, GlobalBusinessIdentifierDataValidation.GlobalLocationNumberFormat);

			messageData.US_GLN = "~";
			AssertNoMessageErrorContaining(messageData.US_GLNInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(messageData.US_GLNInfo, GlobalBusinessIdentifierDataValidation.GlobalLocationNumberFormat);

			messageData.US_GLN = "1234567890123";
			AssertNoMessageErrorContaining(messageData.US_GLNInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarning(messageData.US_GLNInfo, GlobalBusinessIdentifierDataValidation.GlobalLocationNumberFormat);
		}

		public void TestCheckUS_LEI()
		{
			messageData.US_LEI = ZString.Empty;
			AssertHasMessageErrorContaining(messageData.US_LEIInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(messageData.US_LEIInfo, GlobalBusinessIdentifierDataValidation.LegalEntityIdentifierFormat);

			messageData.US_LEI = "~";
			AssertNoMessageErrorContaining(messageData.US_LEIInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(messageData.US_LEIInfo, GlobalBusinessIdentifierDataValidation.LegalEntityIdentifierFormat);

			messageData.US_LEI = "1234567890ABCDEFGHIJ";
			AssertNoMessageErrorContaining(messageData.US_LEIInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarning(messageData.US_LEIInfo, GlobalBusinessIdentifierDataValidation.LegalEntityIdentifierFormat);
		}

		public void TestCheckSubmissionStatus()
		{
			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.AwaitingGBIAdd;
			AssertHasMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.ClearGBIAdd;
			AssertNoMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.ErrorGBIAdd;
			AssertNoMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.AwaitingGBIUpdate;
			AssertHasMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.ClearGBIUpdate;
			AssertNoMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.ErrorGBIUpdate;
			AssertNoMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.AwaitingGBIDelete;
			AssertHasMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.ClearGBIDelete;
			AssertNoMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);

			messageData.SubmissionStatus = GBISubmissionStatusList.Codes.ErrorGBIDelete;
			AssertNoMessageError(messageData.SubmissionStatusInfo, GlobalBusinessIdentifierDataValidation.AwaitingStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "DRUMARKON INTERNATIONAL BV";
			organisation.MainAddress.OA_Address1 = "SPORTLAAN 1A";
			organisation.MainAddress.OA_Address2 = "4209 AX";
			organisation.MainAddress.OA_City = "SCHELLUINEN";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "NLSLN";
			organisation.MainAddress.OA_PostCode = "12222";

			var wrapper = OrgHeaderWrapper.New(organisation);
			messageData = new GlobalBusinessIdentifierData(wrapper);
		}

		GlobalBusinessIdentifierData messageData;
	}
}
