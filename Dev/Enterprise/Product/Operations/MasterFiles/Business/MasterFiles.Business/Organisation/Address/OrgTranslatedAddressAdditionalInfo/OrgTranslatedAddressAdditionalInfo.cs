using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTranslatedAddressAdditionalInfo : AutoOrgTranslatedAddressAdditionalInfo
	{
		public OrgTranslatedAddressAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString OTI_AdditionalInfo
		{
			get => base.OTI_AdditionalInfo;
			set
			{
				var oldValue = base.OTI_AdditionalInfo;
				base.OTI_AdditionalInfo = value;
				if (oldValue != base.OTI_AdditionalInfo && AddressAdditionalInfo != null && AddressAdditionalInfo.OAI_IsPrimary)
				{
					var targetTranslatedAddresses = AddressAdditionalInfo.Address.TranslatedAddresses
						.Where(t => t.OTA_Language == OTI_Language && t.OTA_AdditionalAddressInformation == oldValue);

					targetTranslatedAddresses.ForEach(t => t.OTA_AdditionalAddressInformation = base.OTI_AdditionalInfo);
				}
			}
		}

		public override ZString OTI_Language
		{
			get => base.OTI_Language;
			set
			{
				var oldValue = base.OTI_Language;
				base.OTI_Language = value;
				if (oldValue != base.OTI_Language && AddressAdditionalInfo != null && AddressAdditionalInfo.OAI_IsPrimary)
				{
					var targetTranslatedAddress = AddressAdditionalInfo.Address.TranslatedAddresses
						.Where(t => t.OTA_Language == oldValue && t.OTA_AdditionalAddressInformation == OTI_AdditionalInfo);

					targetTranslatedAddress.ForEach(t => t.OTA_Language = base.OTI_Language);
				}
			}
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
