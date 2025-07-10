using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	partial class AutoFWSHeader
	{
		#region US_OA_FWSImporterAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_FWSImporterAddress_ZAddress
		{
			get
			{
				if (fUS_OA_FWSImporterAddress_ZAddress == null)
				{
					fUS_OA_FWSImporterAddress_ZAddress = GetNewUS_OA_FWSImporterAddress_ZAddress();
					fUS_OA_FWSImporterAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_FWSImporterAddress_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return fUS_OA_FWSImporterAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_FWSImporterAddress_ZAddress;

		protected void SetUS_OA_FWSImporterAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingFWSImporterDefaults = true;
				US_OA_FWSImporterAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingFWSImporterDefaults = false;
			}
		}
		protected bool disableSettingFWSImporterDefaults;

		protected virtual ZAddress GetNewUS_OA_FWSImporterAddress_ZAddress()
		{
			return new ZAddress(US_OA_FWSImporterAddressInfo);
		}

		#endregion

		#region US_OA_FWSExporterAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_FWSExporterAddress_ZAddress
		{
			get
			{
				if (fUS_OA_FWSExporterAddress_ZAddress == null)
				{
					fUS_OA_FWSExporterAddress_ZAddress = GetNewUS_OA_FWSExporterAddress_ZAddress();
					fUS_OA_FWSExporterAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_FWSExporterAddress_ZAddress.DefaultAddressType = AddressType.OFC;
				}
				return fUS_OA_FWSExporterAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_FWSExporterAddress_ZAddress;

		protected void SetUS_OA_FWSExporterAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingFWSExporterDefaults = true;
				US_OA_FWSExporterAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingFWSExporterDefaults = false;
			}
		}
		protected bool disableSettingFWSExporterDefaults;

		protected virtual ZAddress GetNewUS_OA_FWSExporterAddress_ZAddress()
		{
			return new ZAddress(US_OA_FWSExporterAddressInfo);
		}

		#endregion
	}
}
