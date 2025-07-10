using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class SupplierLocalAddressRequirement : TWJobDocAddressRequirement
	{
		public SupplierLocalAddressRequirement(DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			this.ValidateCompanyName += ValidateE2_CompanyName;
			this.ValidateAddress1 += ValidateE2_Address1;
			this.ValidateState += ValidateCheckE2_State;
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_CompanyNameInfo;
				if (parent.E2_AddressOverride && parent.Parent is CusTWControllingMessageHeader cMHeader && cMHeader.IsNX101 && (cMHeader.SupplierDocumentaryAddress.CompanyName.IsEmpty || cMHeader.IsCertificate15))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalCompanyNameResString);
				}
				var maxLangth = 70;
				if (parent.IsImport && parent.E2_CompanyName.Length > maxLangth)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxLangth));
				}
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent &&
				parent.E2_AddressOverride &&
				parent.Parent is CusTWControllingMessageHeader cMHeader &&
				cMHeader.IsNX101 &&
				parent.E2_Address1.IsEmpty &&
				(cMHeader.SupplierDocumentaryAddress.E2_Address1.IsEmpty || cMHeader.IsCertificate15))
			{
				parent.E2_Address1Info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalAddressResString));
			}

			if (validation is TWJobDocAddressValidation twValidation)
			{
				twValidation.ValidateLocalAddressE2_Address1Maxlength();
			}
		}

		void ValidateCheckE2_State(JobDocAddressValidation validation)
		{
			if (validation is TWJobDocAddressValidation twValidation)
			{
				twValidation.ValidateState();
			}
		}
	}
}
