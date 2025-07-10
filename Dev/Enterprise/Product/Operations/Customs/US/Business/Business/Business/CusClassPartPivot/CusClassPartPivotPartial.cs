using System.ComponentModel;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	partial class AutoCusClassPartPivot
	{
		#region CD_OA_Manufacturer_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress CD_OA_Manufacturer_ZAddress
		{
			get
			{
				if (fCD_OA_Manufacturer_ZAddress == null)
				{
					fCD_OA_Manufacturer_ZAddress = GetNewCD_OA_ManufacturerAddress_ZAddress();
					fCD_OA_Manufacturer_ZAddress.IsOrgVisible = true;
					fCD_OA_Manufacturer_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fCD_OA_Manufacturer_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fCD_OA_Manufacturer_ZAddress;
			}
		}
		ZAddress fCD_OA_Manufacturer_ZAddress;

		protected virtual ZAddress GetNewCD_OA_ManufacturerAddress_ZAddress()
		{
			return new ZAddress(CD_OA_ManufacturerInfo);
		}

		#endregion

		#region CD_OA_Exporter_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress CD_OA_Exporter_ZAddress
		{
			get
			{
				if (fCD_OA_Exporter_ZAddress == null)
				{
					fCD_OA_Exporter_ZAddress = GetNewCD_OA_Exporter_ZAddress();
					fCD_OA_Exporter_ZAddress.IsOrgVisible = true;
					fCD_OA_Exporter_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fCD_OA_Exporter_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fCD_OA_Exporter_ZAddress;
			}
		}
		ZAddress fCD_OA_Exporter_ZAddress;

		protected virtual ZAddress GetNewCD_OA_Exporter_ZAddress()
		{
			return new ZAddress(CD_OA_ExporterInfo);
		}

		#endregion
	}
}
