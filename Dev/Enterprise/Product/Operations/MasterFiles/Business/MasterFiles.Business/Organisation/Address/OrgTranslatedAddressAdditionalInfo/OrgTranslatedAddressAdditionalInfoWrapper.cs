using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTranslatedAddressAdditionalInfoWrapper : NonPersistentBusinessObject
	{
		public OrgTranslatedAddressAdditionalInfoWrapper(OrgAddressAdditionalInfo orgAddressAdditionalInfo, OrgTranslatedAddress translatedAddress) : base(translatedAddress.Factory)
		{
			AddressAdditionalInfo = orgAddressAdditionalInfo;
			TranslatedAddress = translatedAddress;
		}
		public OrgTranslatedAddressAdditionalInfoWrapper(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string IsPrimary = "IsPrimary";
			public const string AdditionalInfo = "AdditionalInfo";
			public const string TranslatedAdditionalInfo = "TranslatedAdditionalInfo";
		}

		#endregion

		public OrgTranslatedAddress TranslatedAddress { get; }

		public OrgAddressAdditionalInfo AddressAdditionalInfo { get; set; }

		public ZBool IsPrimary => AddressAdditionalInfo?.OAI_IsPrimary ?? false;

		public ZPropertyInfo IsPrimaryInfo
		{
			get { return AddressAdditionalInfo?.OAI_IsPrimaryInfo ?? GetZPropertyInfo(Schema.IsPrimary); }
		}

		public ZString AdditionalInfo
		{
			get { return AddressAdditionalInfo?.OAI_AdditionalInfo ?? ZString.Empty; }
		}

		public ZPropertyInfo AdditionalInfoInfo
		{
			get { return AddressAdditionalInfo?.OAI_AdditionalInfoInfo ?? GetZPropertyInfo(Schema.AdditionalInfo); }
		}

		[MaxLength(OrgTranslatedAddressAdditionalInfo.Schema.OTI_AdditionalInfoMaxLength)]
		public ZString TranslatedAdditionalInfo
		{
			get { return TranslatedAddressAdditionalInfo?.OTI_AdditionalInfo ?? ZString.Empty; }
			set
			{
				if (IsPrimary)
				{
					TranslatedAddress.OTA_AdditionalAddressInformation = value;
				}

				var translatedInfo = TranslatedAddressAdditionalInfo;
				if (value.IsEmpty)
				{
					translatedInfo?.Delete();
				}
				else
				{
					if (translatedInfo == null)
					{
						translatedInfo = AddressAdditionalInfo.TranslatedInfos.AddNew();
					}
					translatedInfo.OTI_AdditionalInfo = value;
					translatedInfo.OTI_Language = TranslatedAddress.Language;
				}
				TranslatedAdditionalInfoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TranslatedAdditionalInfoInfo
		{
			get { return GetZPropertyInfo(Schema.TranslatedAdditionalInfo); }
		}

		public OrgTranslatedAddressAdditionalInfo TranslatedAddressAdditionalInfo => AddressAdditionalInfo?.TranslatedInfos.FirstOrDefault(u => u.OTI_Language == TranslatedAddress.Language);

		public override void Delete()
		{
			base.Delete();
			TranslatedAddressAdditionalInfo?.Delete();
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}

