using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFDocAddressRequirement : JobDocAddressRequirement
	{
		#region Constructor

		public ISFDocAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType, bool saveEvenIfBlank, int defaultMax, AddressType defaultAddressType = AddressType.NoDefault)
			: this(defaultDocAddressType, defaultContactType, saveEvenIfBlank, defaultAddressType)
		{
			this.DefaultMax = defaultMax;
		}

		public ISFDocAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType, bool saveEvenIfBlank, AddressType defaultAddressType = AddressType.NoDefault)
			: this(defaultDocAddressType, defaultContactType, defaultAddressType)
		{
			this.SaveEvenIfBlank = saveEvenIfBlank;
		}

		public ISFDocAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType, AddressType defaultAddressType = AddressType.NoDefault)
			: this(defaultDocAddressType, defaultAddressType)
		{
			this.DefaultContactType = defaultContactType;
		}

		public ISFDocAddressRequirement(DocAddressType defaultDocAddressType, AddressType defaultAddressType = AddressType.NoDefault)
		{
			this.DefaultDocAddressType = defaultDocAddressType;
			this.DefaultAddressType = defaultAddressType;
		}

		public ISFDocAddressRequirement()
		{
		}

		#endregion

		#region Validation Delegates

		public ValidationDelegate ValidateSocialSecurityNumber;
		public ValidationDelegate ValidateSocialSecurityNumberDateOfBirth;

		#endregion
	}
}
