using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class GlobalBusinessIdentifierDataValidation : AutoGlobalBusinessIdentifierDataValidation
	{
		public GlobalBusinessIdentifierDataValidation(AutoGlobalBusinessIdentifierData parent)
			: base(parent)
		{
		}

		public new GlobalBusinessIdentifierData Parent => (GlobalBusinessIdentifierData)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSubmissionStatus();
			ValidateGBIStatus();
		}

		protected override void CheckUS_OA_AddressDetails()
		{
			base.CheckUS_OA_AddressDetails();
			MandatoryValidation.CheckEntered(Parent.US_OA_AddressDetailsInfo);
		}

		protected override void CheckUS_IsManufacturer()
		{
			base.CheckUS_IsManufacturer();
			CheckAtLeastOneRoleIsTicked(Parent.US_IsManufacturerInfo);
		}

		protected override void CheckUS_IsShipper()
		{
			base.CheckUS_IsShipper();
			CheckAtLeastOneRoleIsTicked(Parent.US_IsShipperInfo);
		}

		protected override void CheckUS_IsSeller()
		{
			base.CheckUS_IsSeller();
			CheckAtLeastOneRoleIsTicked(Parent.US_IsSellerInfo);
		}

		protected override void CheckUS_IsExporter()
		{
			base.CheckUS_IsExporter();
			CheckAtLeastOneRoleIsTicked(Parent.US_IsExporterInfo);
		}

		protected override void CheckUS_IsPackager()
		{
			base.CheckUS_IsPackager();
			CheckAtLeastOneRoleIsTicked(Parent.US_IsPackagerInfo);
		}

		protected override void CheckUS_IsDistributor()
		{
			base.CheckUS_IsDistributor();
			CheckAtLeastOneRoleIsTicked(Parent.US_IsDistributorInfo);
		}

		void CheckAtLeastOneRoleIsTicked(ZPropertyInfo propertyInfo)
		{
			if (!Parent.US_IsManufacturer && !Parent.US_IsShipper && !Parent.US_IsSeller && !Parent.US_IsExporter && !Parent.US_IsPackager && !Parent.US_IsDistributor)
			{
				propertyInfo.AddMessageError(AtLeastOneRoleShouldBeTicked);
			}
		}
		internal const string AtLeastOneRoleShouldBeTicked = "Please select at least one potential role for this entity.";

		protected override void CheckUS_DUNS()
		{
			base.CheckUS_DUNS();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DUNSInfo);

			var errorText = DataUniversalNumberingSystemValidator.GetDUNSNumberError(Parent.US_DUNS);
			if (!string.IsNullOrEmpty(errorText))
			{
				Parent.US_DUNSInfo.AddWarning(errorText);
			}
		}

		protected override void CheckUS_GLN()
		{
			base.CheckUS_GLN();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_GLNInfo);

			if (!Parent.US_GLN.IsNumbersOnlyOrEmpty || Parent.US_GLN.Length != 13)
			{
				Parent.US_GLNInfo.AddWarning(GlobalLocationNumberFormat);
			}
		}
		internal const string GlobalLocationNumberFormat = "Global Location Number should be 13 digits.";

		protected override void CheckUS_LEI()
		{
			base.CheckUS_LEI();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_LEIInfo);

			if (!Parent.US_LEI.IsLettersAndNumbersOnlyOrEmpty || Parent.US_LEI.Length != 20)
			{
				Parent.US_LEIInfo.AddWarning(LegalEntityIdentifierFormat);
			}
		}
		internal const string LegalEntityIdentifierFormat = "Legal Entity Identifier should be a 20 alpha numeric number.";

		public void ValidateSubmissionStatus()
		{
			ValidateCalculatedProperty(Parent.SubmissionStatusInfo);
		}

		protected void CheckSubmissionStatus()
		{
			if (GBISubmissionStatusList.IsWaitingForResponse(Parent.SubmissionStatus))
			{
				Parent.SubmissionStatusInfo.AddMessageError(AwaitingStatus);
			}
		}
		internal const string AwaitingStatus = "A GBI Status Notification message has not been received from CBP yet.";

		public void ValidateGBIStatus()
		{
			ValidateCalculatedProperty(Parent.GBIStatusInfo);
		}

		protected void CheckGBIStatus()
		{
		}
	}
}
