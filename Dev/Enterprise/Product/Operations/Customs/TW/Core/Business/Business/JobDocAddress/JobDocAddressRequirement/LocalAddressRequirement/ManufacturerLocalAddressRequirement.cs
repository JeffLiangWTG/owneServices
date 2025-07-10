using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ManufacturerLocalAddressRequirement : TWJobDocAddressRequirement
	{
		public ManufacturerLocalAddressRequirement(DocAddressType defaultDocAddressType)
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
				if (parent.E2_AddressOverride &&
					parent.Parent is JobComInvoiceLine line &&
					line.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101)?.ControllingMessageHeader is CusTWControllingMessageHeader cMHeader &&
					parent.E2_CompanyName.IsEmpty &&
					!line.ManufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader) &&
					(line.ManufacturerDocAddress.CompanyName.IsEmpty || cMHeader.IsCertificate15))
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerAddressRequirement.ManufacturerLocalCompanyNameResString));
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
			if (validation is TWJobDocAddressValidation twValidation)
			{
				var parent = validation.Parent as TWJobDocAddress;
				if (parent.E2_AddressOverride &&
					parent.Parent is JobComInvoiceLine line
					&& line.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101)?.ControllingMessageHeader is CusTWControllingMessageHeader cMHeader &&
					!line.ManufacturerDocAddress.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader) &&
					parent.E2_Address1.IsEmpty &&
					(line.ManufacturerDocAddress.E2_Address1.IsEmpty || cMHeader.IsCertificate15))
				{
					parent.E2_Address1Info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TWJobDocAddressValidation.ManufacturerLocalAddressResString));
				}

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
