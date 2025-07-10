using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	partial class AutoAPHISHeader
	{
		#region US_OA_ApplicantAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ApplicantAddress_ZAddress
		{
			get
			{
				if (fUS_OA_ApplicantAddress_ZAddress == null)
				{
					fUS_OA_ApplicantAddress_ZAddress = GetNewUS_OA_ApplicantAddress_ZAddress();
					fUS_OA_ApplicantAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_ApplicantAddress_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return fUS_OA_ApplicantAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_ApplicantAddress_ZAddress;

		protected void SetUS_OA_ApplicantAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingApplicantDefaults = true;
				US_OA_ApplicantAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingApplicantDefaults = false;
			}
		}
		protected bool disableSettingApplicantDefaults;

		protected virtual ZAddress GetNewUS_OA_ApplicantAddress_ZAddress()
		{
			return new ZAddress(US_OA_ApplicantAddressInfo);
		}

		#endregion

		#region US_OA_CropGrowerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_CropGrowerAddress_ZAddress
		{
			get
			{
				if (fUS_OA_CropGrowerAddress_ZAddress == null)
				{
					fUS_OA_CropGrowerAddress_ZAddress = GetNewUS_OA_CropGrowerAddress_ZAddress();
					fUS_OA_CropGrowerAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_CropGrowerAddress_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return fUS_OA_CropGrowerAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_CropGrowerAddress_ZAddress;

		protected void SetUS_OA_CropGrowerAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingCropGrowerDefaults = true;
				US_OA_CropGrowerAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingCropGrowerDefaults = false;
			}
		}
		protected bool disableSettingCropGrowerDefaults;

		protected virtual ZAddress GetNewUS_OA_CropGrowerAddress_ZAddress()
		{
			return new ZAddress(US_OA_CropGrowerAddressInfo);
		}

		#endregion

		#region US_OA_ShipperAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ShipperAddress_ZAddress
		{
			get
			{
				if (fUS_OA_ShipperAddress_ZAddress == null)
				{
					fUS_OA_ShipperAddress_ZAddress = GetNewUS_OA_ShipperAddress_ZAddress();
					fUS_OA_ShipperAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_ShipperAddress_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return fUS_OA_ShipperAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_ShipperAddress_ZAddress;

		protected void SetUS_OA_ShipperAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingShipperDefaults = true;
				US_OA_ShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingShipperDefaults = false;
			}
		}
		protected bool disableSettingShipperDefaults;

		protected virtual ZAddress GetNewUS_OA_ShipperAddress_ZAddress()
		{
			return new ZAddress(US_OA_ShipperAddressInfo);
		}

		#endregion

		#region US_OA_PermittedAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_PermittedAddress_ZAddress
		{
			get
			{
				if (fUS_OA_PermittedAddress_ZAddress == null)
				{
					fUS_OA_PermittedAddress_ZAddress = GetNewUS_OA_PermittedAddress_ZAddress();
					fUS_OA_PermittedAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_PermittedAddress_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return fUS_OA_PermittedAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_PermittedAddress_ZAddress;

		protected void SetUS_OA_PermittedAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingPermittedDefaults = true;
				US_OA_PermittedAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingPermittedDefaults = false;
			}
		}
		protected bool disableSettingPermittedDefaults;

		protected virtual ZAddress GetNewUS_OA_PermittedAddress_ZAddress()
		{
			return new ZAddress(US_OA_PermittedAddressInfo);
		}

		#endregion

		#region US_OA_USDAAPHISGrowerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_USDAAPHISGrowerAddress_ZAddress
		{
			get
			{
				if (fUS_OA_USDAAPHISGrowerAddress_ZAddress == null)
				{
					fUS_OA_USDAAPHISGrowerAddress_ZAddress = GetNewUS_OA_USDAAPHISGrowerAddress_ZAddress();
					fUS_OA_USDAAPHISGrowerAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_USDAAPHISGrowerAddress_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return fUS_OA_USDAAPHISGrowerAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_USDAAPHISGrowerAddress_ZAddress;

		protected virtual ZAddress GetNewUS_OA_USDAAPHISGrowerAddress_ZAddress()
		{
			return new ZAddress(US_OA_USDAAPHISGrowerAddressInfo);
		}

		#endregion
	}
}
