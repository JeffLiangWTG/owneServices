using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class NHTSADetailsValidation : Customs.Business.MultiLineAddInfos.CusAddInfoValidation
	{
		public NHTSADetailsValidation(NHTSADetails details)
			: base(details)
		{
		}

		new NHTSADetails Parent
		{
			get { return (NHTSADetails)base.Parent; }
		}

		NHTSAHeader Header
		{
			get { return Parent.Header; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateHasAtLeastOneVINNumber();
			ValidateUS_NHTIdentityNumQualifier();
			ValidateUS_NHTIdentityNumber();
			ValidateUS_NHTLPCOType();
			ValidateUS_NHTLPCONumber();
			ValidateUS_NHTLPCODateType();
			ValidateUS_NHTLPCODate();
		}

		protected void ValidateHasAtLeastOneVINNumber()
		{
			var header = this.Header;
			if (header.IsPGAValidationOn && header.IsMotorVehicles)
			{
				if (!Parent.AdditionalNumbers.HasNumberType(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN))
				{
					Parent.AddRowMessageError(ValidationConstants.NHTSA.VINIsMandatoryForAllVehicles);
				}
			}
		}

		public void ValidateUS_NHTIdentityNumQualifier()
		{
			ValidateCalculatedProperty(Parent.US_NHTIdentityNumQualifierInfo);
		}

		protected void CheckUS_NHTIdentityNumQualifier()
		{
			IdentityNumberAndLPCODetailsValidator.ValidateAdditionalIdentityNumQualifier(Parent.US_NHTIdentityNumQualifierInfo, Header, Parent.AddInfoLookups.NumberTypes);
			ValidateUS_NHTIdentityNumber();
		}

		public void ValidateUS_NHTIdentityNumber()
		{
			ValidateCalculatedProperty(Parent.US_NHTIdentityNumberInfo);
		}

		protected void CheckUS_NHTIdentityNumber()
		{
			if (Header.IsPGAValidationOn)
			{
				IdentityNumberAndLPCODetailsValidator.ValidateAdditionalIdentityNumber(Parent.US_NHTIdentityNumberInfo, Parent.US_NHTIdentityNumber, Parent.US_NHTIdentityNumQualifier, Parent.IsVehicleIdentificationNumber, Header.US_NHTBoxNumber);
			}
		}

		public void ValidateUS_NHTLPCOType()
		{
			ValidateCalculatedProperty(Parent.US_NHTLPCOTypeInfo);
		}

		protected void CheckUS_NHTLPCOType()
		{
			IdentityNumberAndLPCODetailsValidator.ValidateLPCOType(Parent.US_NHTLPCOTypeInfo, Parent.AddInfoLookups.LPCOTypes, Parent.US_NHTLPCONumber, Parent.US_NHTLPCODateType, Parent.US_NHTLPCODate, Parent.US_NHTLPCOQuantity);
			ValidateUS_NHTLPCONumber();
		}

		public void ValidateUS_NHTLPCONumber()
		{
			ValidateCalculatedProperty(Parent.US_NHTLPCONumberInfo);
		}

		protected void CheckUS_NHTLPCONumber()
		{
			var permitAndLicense = Parent.FirstNHTSAPermitAndLicenses;
			if (permitAndLicense != null)
			{
				IdentityNumberAndLPCODetailsValidator.ValidateLPCONumber(Parent.US_NHTLPCONumberInfo, Header.IsPGAValidationOn, Parent.US_NHTLPCOType, permitAndLicense.IsRegisteredImporterNumber, permitAndLicense.IsNHTSAImportPermissionLetter, permitAndLicense.IsVehicleEligibilityNumber);
			}
		}

		public void ValidateUS_NHTLPCODateType()
		{
			ValidateCalculatedProperty(Parent.US_NHTLPCODateTypeInfo);
		}

		protected void CheckUS_NHTLPCODateType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTLPCODateTypeInfo, Parent.AddInfoLookups.LPCODateTypes);
			IdentityNumberAndLPCODetailsValidator.ValidateLPCODateType(Parent.US_NHTLPCODateTypeInfo, Parent.US_NHTLPCODate);

			ValidateUS_NHTLPCODate();
		}

		public void ValidateUS_NHTLPCODate()
		{
			ValidateCalculatedProperty(Parent.US_NHTLPCODateInfo);
		}

		protected void CheckUS_NHTLPCODate()
		{
			IdentityNumberAndLPCODetailsValidator.ValidateLPCODate(Parent.US_NHTLPCODateInfo, Parent.US_NHTLPCODateType);

			ValidateUS_NHTLPCODateType();
		}
	}
}
