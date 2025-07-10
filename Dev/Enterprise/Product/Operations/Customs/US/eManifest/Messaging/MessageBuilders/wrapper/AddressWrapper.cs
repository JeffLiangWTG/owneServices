using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class AddressWrapper : IAddress
	{
		public AddressWrapper(JobDocAddress address, bool dummy = false)
		{
			this.address = address;
			this.dummy = dummy;
		}

		#region Implementation of IAddress

		public ZString Address
		{
			get { return FallbackToDummy(address.E2_Address1 + " " + address.E2_Address2); }
		}

		public ZString City
		{
			get { return FallbackToDummy(address.E2_City); }
		}

		public ZString StateOrProvince
		{
			get { return FallbackToDummy(address.E2_State); }
		}

		public ZString Country
		{
			get { return FallbackToDummy(address.E2_RN_NKCountryCode); }
		}

		public ZString Postcode
		{
			get { return FallbackToDummy(address.E2_Postcode); }
		}

		public ZBool IsEmpty
		{
			get { return !dummy && address.IsEmpty; }
		}

		#endregion

		protected ZString FallbackToDummy(ZString value)
		{
			return address.IsEmpty && dummy ? (ZString)"DUMMY" : value;
		}

		readonly JobDocAddress address;
		protected readonly bool dummy;
	}
}
