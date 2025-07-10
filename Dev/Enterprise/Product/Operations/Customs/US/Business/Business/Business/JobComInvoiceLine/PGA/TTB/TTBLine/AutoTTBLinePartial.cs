using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	partial class AutoTTBLine
	{
		#region New Properties

		protected bool IsNotReleaseUnderBond
		{
			get { return !US_IsReleaseUnderBond; }
		}

		#region US_OA_ConsigneeAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ConsigneeAddress_ZAddress
		{
			get
			{
				if (fUS_OA_ConsigneeAddress_ZAddress == null)
				{
					fUS_OA_ConsigneeAddress_ZAddress = GetNewUS_OA_ConsigneeAddress_ZAddress();
					fUS_OA_ConsigneeAddress_ZAddress.IsOrgVisible = true;
					fUS_OA_ConsigneeAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetCBPAddress);
				}
				return fUS_OA_ConsigneeAddress_ZAddress;
			}
		}
		ZAddress fUS_OA_ConsigneeAddress_ZAddress;

		protected void SetUS_OA_ConsigneeAddress_ZAddressOrgPKWithoutSettingDefaults(ZGuid orgPK)
		{
			try
			{
				disableSettingConsigneeDefaults = true;
				US_OA_ConsigneeAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(orgPK);
			}
			finally
			{
				disableSettingConsigneeDefaults = false;
			}
		}
		protected bool disableSettingConsigneeDefaults;

		protected virtual ZAddress GetNewUS_OA_ConsigneeAddress_ZAddress()
		{
			return new ZAddress(US_OA_ConsigneeAddressInfo);
		}

		#endregion

		#endregion
	}
}
