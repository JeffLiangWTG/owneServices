using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWJobDocAddressRequirement : JobDocAddressRequirement
	{
		public TWJobDocAddressRequirement(DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
			Initialize();
		}

		public TWJobDocAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType)
			: base(defaultDocAddressType, defaultContactType)
		{
			Initialize();
		}

		public TWJobDocAddressRequirement(DocAddressType defaultDocAddressType, AddressType defaultAddressType)
			: base(defaultDocAddressType, defaultAddressType)
		{
			Initialize();
		}

		public delegate void TWValidationDelegate(TWJobDocAddressValidation validation);

		public TWValidationDelegate ValidateIDCodeType;
		public TWValidationDelegate ValidateCBPCodeType;
		public TWValidationDelegate ValidateIDCode;
		public TWValidationDelegate ValidateCBPCode;
		public TWValidationDelegate ValidateFRICode;
		public TWValidationDelegate ValidatePhone;
		public TWValidationDelegate ValidateFax;

		protected virtual void Initialize()
		{
			this.ValidateCompanyName += EmptyValidation;
			this.ValidateAddress1 += EmptyValidation;
			this.ValidateCity += EmptyValidation;
			this.ValidatePostCode += EmptyValidation;
		}

		protected void EmptyValidation(JobDocAddressValidation validation) { }
	}
}
