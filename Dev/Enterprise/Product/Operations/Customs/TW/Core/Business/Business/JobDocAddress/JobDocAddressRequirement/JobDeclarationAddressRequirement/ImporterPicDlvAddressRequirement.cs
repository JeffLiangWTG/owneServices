using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ImporterPicDlvAddressRequirement : TWJobDocAddressRequirement
	{
		public ImporterPicDlvAddressRequirement(DocAddressType defaultDocAddressType, AddressType defaultAddressType)
			: base(defaultDocAddressType, defaultAddressType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			this.ValidateCompanyName += ValidateE2_CompanyName;
			this.ValidateState += ValidateCheckE2_State;
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.E2_CompanyNameInfo);
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
